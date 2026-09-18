using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DriveConnect.infrastructure.Data
{
    public class TenantDriveConnectDbContextFactory : IDesignTimeDbContextFactory<TenantDriveConnectDbContext>
    {
        public TenantDriveConnectDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TenantDriveConnectDbContext>();

            // Target LocalDB for zero-timeout local migrations
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=DriveConnectTenant1;Trusted_Connection=True;TrustServerCertificate=True;");

            return new TenantDriveConnectDbContext(optionsBuilder.Options);
        }
    }
}