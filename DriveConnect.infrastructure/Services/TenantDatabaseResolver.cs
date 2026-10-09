using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DriveConnect.infrastructure.Data;

namespace DriveConnect.infrastructure.Services
{
    public class TenantDatabaseResolver : ITenantDatabaseResolver
    {
        private readonly MasterDriveConnectDbContext _masterDb;

        public TenantDatabaseResolver(MasterDriveConnectDbContext masterDb)
        {
            _masterDb = masterDb;
        }

        public async Task<TenantDatabaseInfo> GetDatabaseInfoAsync(int companyId)
        {
            var matches = await _masterDb.CompanyDatabases
                .AsNoTracking()
                .Where(x => x.CompanyId == companyId && x.IsActive)
                .ToListAsync();

            if (matches.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No active tenant database found for CompanyId {companyId}.");
            }

            if (matches.Count > 1)
            {
                throw new InvalidOperationException(
                    $"More than one active tenant database is configured for CompanyId {companyId}. " +
                    "Each company must have exactly one active tenant database mapping.");
            }

            var tenantDatabase = matches[0];

            var databaseIsShared = await _masterDb.CompanyDatabases
                .AsNoTracking()
                .AnyAsync(x =>
                    x.IsActive &&
                    x.CompanyId != companyId &&
                    x.ServerName == tenantDatabase.ServerName &&
                    x.DatabaseName == tenantDatabase.DatabaseName);

            if (databaseIsShared)
            {
                throw new InvalidOperationException(
                    $"The tenant database '{tenantDatabase.DatabaseName}' is assigned to more than one company. " +
                    "Each tenant must use a separate database.");
            }

            return new TenantDatabaseInfo
            {
                ServerName = tenantDatabase.ServerName,
                DatabaseName = tenantDatabase.DatabaseName,
                CredentialKey = $"Tenant{companyId}"
            };
        }
    }
}