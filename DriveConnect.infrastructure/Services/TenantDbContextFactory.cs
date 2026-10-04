using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DriveConnect.infrastructure.Data;

namespace DriveConnect.infrastructure.Services
{
    public class TenantDbContextFactory : ITenantDbContextFactory
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
            var databaseInfo = await _resolver.GetDatabaseInfoAsync(companyId);

            string connectionString;

            if (databaseInfo.ServerName.Contains("(localdb)", StringComparison.OrdinalIgnoreCase))
            {
                connectionString =
                    $"Server={databaseInfo.ServerName};" +
                    $"Database={databaseInfo.DatabaseName};" +
                    "Trusted_Connection=True;" +
                    "TrustServerCertificate=True;" +
                    "MultipleActiveResultSets=True;";
            }
            else
            {
                var userId = _configuration[$"TenantCredentials:{databaseInfo.CredentialKey}:UserId"];
                var password = _configuration[$"TenantCredentials:{databaseInfo.CredentialKey}:Password"];

                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password))
                {
                    throw new InvalidOperationException(
                        $"Missing credentials for {databaseInfo.CredentialKey}. " +
                        "Configure the tenant database UserId and Password as environment variables.");
                }

                connectionString =
                    $"Server={databaseInfo.ServerName};" +
                    $"Database={databaseInfo.DatabaseName};" +
                    $"User Id={userId};" +
                    $"Password={password};" +
                    "Encrypt=True;" +
                    "TrustServerCertificate=True;" +
                    "MultipleActiveResultSets=True;";
            }

            var options = new DbContextOptionsBuilder<TenantDriveConnectDbContext>()
                .UseSqlServer(
                    connectionString,
                    sqlOptions => sqlOptions.EnableRetryOnFailure())
                .Options;

            return new TenantDriveConnectDbContext(options);
        }
    }
}
