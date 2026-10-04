using DriveConnect.infrastructure.Data;
using DriveConnect.infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace DriveConnect.api.Services;

public sealed class TenantDbContextFactory
{
    private readonly ITenantDatabaseResolver _resolver;
    private readonly IConfiguration _configuration;

    public TenantDbContextFactory(
        ITenantDatabaseResolver resolver,
        IConfiguration configuration)
    {
        _resolver = resolver;
        _configuration = configuration;
    }

    public async Task<TenantDriveConnectDbContext> CreateAsync(int companyId)
    {
        var dbInfo = await _resolver.GetDatabaseInfoAsync(companyId);

        string connectionString;

        if (dbInfo.ServerName.Contains("(localdb)", StringComparison.OrdinalIgnoreCase))
        {
            connectionString =
                $"Server={dbInfo.ServerName};" +
                $"Database={dbInfo.DatabaseName};" +
                "Trusted_Connection=True;" +
                "TrustServerCertificate=True;";
        }
        else
        {
            var userId = _configuration[$"TenantCredentials:{dbInfo.CredentialKey}:UserId"];
            var password = _configuration[$"TenantCredentials:{dbInfo.CredentialKey}:Password"];

            connectionString =
                $"Server={dbInfo.ServerName};" +
                $"Database={dbInfo.DatabaseName};" +
                $"User Id={userId};" +
                $"Password={password};" +
                "TrustServerCertificate=True;";
        }

        var options = new DbContextOptionsBuilder<TenantDriveConnectDbContext>()
            .UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure())
            .Options;

        return new TenantDriveConnectDbContext(options);
    }
}
