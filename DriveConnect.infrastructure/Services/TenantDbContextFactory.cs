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
            var userId = _configuration[$"TenantCredentials:{databaseInfo.CredentialKey}:UserId"];
            var password = _configuration[$"TenantCredentials:{databaseInfo.CredentialKey}:Password"];

            var connectionString =
                $"Server={databaseInfo.ServerName};" +
                $"Database={databaseInfo.DatabaseName};" +
                $"User Id={userId};" +
                $"Password={password};" +
                $"Encrypt=True;" +
                $"TrustServerCertificate=True;" +
                $"MultipleActiveResultSets=True;";

            var options = new DbContextOptionsBuilder<TenantDriveConnectDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new TenantDriveConnectDbContext(options);
        }
    }
}