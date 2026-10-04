using System;
using System.Net.Http.Json;
using DriveConnect.infrastructure.Data;
using DriveConnect.infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DriveConnect.api.Services;

public sealed class SyncWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SyncWorker> _logger;

    public SyncWorker(
        IServiceScopeFactory scopeFactory,
        IHttpClientFactory httpFactory,
        IConfiguration configuration,
        ILogger<SyncWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _httpFactory = httpFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "DriveConnect sync cycle failed. Local data remains available.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ProcessPendingAsync(CancellationToken token)
    {
        if (!_configuration.GetValue<bool>("Sync:Enabled"))
            return;

        var remoteBaseUrl = _configuration["Sync:RemoteBaseUrl"];
        var syncKey = _configuration["Sync:SharedSecret"];

        if (string.IsNullOrWhiteSpace(remoteBaseUrl) ||
            string.IsNullOrWhiteSpace(syncKey))
            return;

        using var scope = _scopeFactory.CreateScope();

        var masterDb = scope.ServiceProvider
            .GetRequiredService<MasterDriveConnectDbContext>();
        var tenantFactory = scope.ServiceProvider
            .GetRequiredService<ITenantDbContextFactory>();
        var syncService = scope.ServiceProvider
            .GetRequiredService<ISyncService>();

        var companyIds = await masterDb.CompanyDatabases
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Select(x => x.CompanyId)
            .Distinct()
            .ToListAsync(token);

        using var http = _httpFactory.CreateClient("DriveConnectCloudSync");
        http.BaseAddress = new Uri(remoteBaseUrl.TrimEnd('/') + "/");

        http.DefaultRequestHeaders.Remove("X-DriveConnect-Sync-Key");
        http.DefaultRequestHeaders.Add("X-DriveConnect-Sync-Key", syncKey);

        foreach (var companyId in companyIds)
        {
            token.ThrowIfCancellationRequested();

            using var tenantDb = await tenantFactory.CreateAsync(companyId);
            var pending = await syncService.GetPendingAsync(tenantDb);

            foreach (var item in pending)
            {
                token.ThrowIfCancellationRequested();

                var envelope = new SyncEnvelope
                {
                    EntityType = item.EntityType,
                    Operation = item.Operation,
                    LocalId = item.LocalId,
                    SyncId = item.SyncId,
                    PayloadJson = item.PayloadJson
                };

                try
                {
                    var response = await http.PostAsJsonAsync(
                        $"tenant/{companyId}/sync/apply",
                        envelope,
                        token);

                    if (response.IsSuccessStatusCode)
                    {
                        await syncService.MarkSyncedAsync(
                            tenantDb,
                            item.SyncQueueId);
                    }
                    else
                    {
                        var body = await response.Content.ReadAsStringAsync(token);

                        await syncService.MarkFailedAsync(
                            tenantDb,
                            item.SyncQueueId,
                            $"HTTP {(int)response.StatusCode}: {body}");
                    }
                }
                catch (Exception ex) when (
                    ex is HttpRequestException or TaskCanceledException)
                {
                    await syncService.MarkFailedAsync(
                        tenantDb,
                        item.SyncQueueId,
                        ex.Message);

                    return;
                }
            }
        }
    }
}
