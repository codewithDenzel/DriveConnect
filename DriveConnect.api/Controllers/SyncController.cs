using DriveConnect.api.Services;
using DriveConnect.infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace DriveConnect.api.Controllers;

[Route("tenant/{companyId:int}/sync")]
public sealed class SyncController : TenantControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly SyncApplier _applier;

    public SyncController(
        ITenantDbContextFactory tenantDbFactory,
        IConfiguration configuration,
        SyncApplier applier)
        : base(tenantDbFactory)
    {
        _configuration = configuration;
        _applier = applier;
    }

    [AllowAnonymous]
    [HttpPost("apply")]
    public async Task<IActionResult> Apply(int companyId, SyncEnvelope envelope)
    {
        var expectedKey = _configuration["Sync:SharedSecret"];

        if (string.IsNullOrWhiteSpace(expectedKey) ||
            !Request.Headers.TryGetValue("X-DriveConnect-Sync-Key", out var suppliedKey) ||
            suppliedKey != expectedKey)
        {
            return Unauthorized();
        }

        if (envelope.SyncId == Guid.Empty ||
            string.IsNullOrWhiteSpace(envelope.EntityType) ||
            string.IsNullOrWhiteSpace(envelope.Operation))
        {
            return BadRequest("Invalid sync request.");
        }

        using var tenantDb = await GetTenantDbAsync(companyId);

        try
        {
            var result = await _applier.ApplyAsync(tenantDb, envelope);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}
