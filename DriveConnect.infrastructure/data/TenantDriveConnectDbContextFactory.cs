using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DriveConnect.infrastructure.Data
{
    public class TenantDriveConnectDbContextFactory : IDesignTimeDbContextFactory<TenantDriveConnectDbContext>
    {
        public TenantDriveConnectDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TenantDriveConnectDbContext>();

            // Set DRIVECONNECT_TENANT_MIGRATION_CONNECTION to target a provisioned tenant database.
            // Keep the LocalDB default for normal local development.
            var connectionString = Environment.GetEnvironmentVariable("DRIVECONNECT_TENANT_MIGRATION_CONNECTION");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = "Server=(localdb)\\mssqllocaldb;Database=DriveConnectTenant1;Trusted_Connection=True;TrustServerCertificate=True;";
            }

            optionsBuilder.UseSqlServer(connectionString);

            return new TenantDriveConnectDbContext(optionsBuilder.Options);
        }
    }
}