using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DriveConnect.infrastructure.Data;

namespace DriveConnect.infrastructure.Services
{
    public interface ISyncService
    {
        Task EnsureSchemaAsync(TenantDriveConnectDbContext db);

        Task EnqueueAsync(
            TenantDriveConnectDbContext db,
            string entityType,
            string operation,
            int localId,
            object? payload);

        Task<bool> EnqueueIfMissingAsync(
            TenantDriveConnectDbContext db,
            string entityType,
            string operation,
            int localId,
            object? payload);

        Task<IReadOnlyList<SyncQueueItem>> GetPendingAsync(
            TenantDriveConnectDbContext db,
            int take = 25);

        Task MarkSyncedAsync(
            TenantDriveConnectDbContext db,
            long queueId);

        Task MarkFailedAsync(
            TenantDriveConnectDbContext db,
            long queueId,
            string error);

        Task<SyncEntityMapping?> GetMappingBySyncIdAsync(
            TenantDriveConnectDbContext db,
            string entityType,
            Guid syncId);

        Task<SyncEntityMapping?> GetMappingByLocalIdAsync(
            TenantDriveConnectDbContext db,
            string entityType,
            int localId);

        Task UpsertMappingAsync(
            TenantDriveConnectDbContext db,
            string entityType,
            int localId,
            Guid syncId,
            int remoteId);

        Task DeleteMappingAsync(
            TenantDriveConnectDbContext db,
            string entityType,
            Guid syncId);
    }
}
