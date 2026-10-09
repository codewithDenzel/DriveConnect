using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text.Json;
using System.Threading.Tasks;
using DriveConnect.infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DriveConnect.infrastructure.Services
{
    public sealed class SyncService : ISyncService
    {
        public async Task EnsureSchemaAsync(TenantDriveConnectDbContext db)
        {
            var cn = db.Database.GetDbConnection();
            if (cn.State != ConnectionState.Open) await cn.OpenAsync();

            await Exec(cn, @"IF OBJECT_ID('dbo.SyncEntityMap','U') IS NULL
BEGIN
CREATE TABLE dbo.SyncEntityMap(
SyncMapId INT IDENTITY PRIMARY KEY,
EntityType NVARCHAR(100) NOT NULL,
LocalId INT NOT NULL,
SyncId UNIQUEIDENTIFIER NOT NULL,
RemoteId INT NULL,
CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
CONSTRAINT UQ_SyncEntityMap_Entity_Local UNIQUE(EntityType,LocalId),
CONSTRAINT UQ_SyncEntityMap_SyncId UNIQUE(SyncId));
END");

            await Exec(cn, @"IF OBJECT_ID('dbo.SyncQueue','U') IS NULL
BEGIN
CREATE TABLE dbo.SyncQueue(
SyncQueueId BIGINT IDENTITY PRIMARY KEY,
EntityType NVARCHAR(100) NOT NULL,
Operation NVARCHAR(20) NOT NULL,
LocalId INT NOT NULL,
SyncId UNIQUEIDENTIFIER NOT NULL,
PayloadJson NVARCHAR(MAX) NULL,
CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
AttemptCount INT NOT NULL DEFAULT 0,
LastAttemptAt DATETIME2 NULL,
LastError NVARCHAR(2000) NULL,
SyncedAt DATETIME2 NULL);
END");

            await Exec(cn, @"IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_SyncQueue_Pending' AND object_id=OBJECT_ID('dbo.SyncQueue'))
CREATE INDEX IX_SyncQueue_Pending ON dbo.SyncQueue(SyncedAt,CreatedAt,SyncQueueId)");
        }

        public async Task EnqueueAsync(TenantDriveConnectDbContext db,string entityType,string operation,int localId,object? payload)
        {
            await EnsureSchemaAsync(db);
            var cn=db.Database.GetDbConnection();
            if(cn.State!=ConnectionState.Open) await cn.OpenAsync();
            var syncId=await GetOrCreateSyncId(cn,entityType,localId);
            var json=payload==null?null:JsonSerializer.Serialize(payload);
            await Exec(cn,@"INSERT INTO dbo.SyncQueue(EntityType,Operation,LocalId,SyncId,PayloadJson) VALUES(@e,@o,@l,@s,@p)",
                ("@e",entityType),("@o",operation),("@l",localId),("@s",syncId),("@p",(object?)json??DBNull.Value));
        }

        public async Task<bool> EnqueueIfMissingAsync(
            TenantDriveConnectDbContext db,
            string entityType,
            string operation,
            int localId,
            object? payload)
        {
            await EnsureSchemaAsync(db);
            var cn = db.Database.GetDbConnection();
            if (cn.State != ConnectionState.Open) await cn.OpenAsync();

            await using var cmd = cn.CreateCommand();
            cmd.CommandText = @"
SELECT CASE WHEN EXISTS (
    SELECT 1
    FROM dbo.SyncQueue
    WHERE EntityType = @e AND LocalId = @l
) THEN 1 ELSE 0 END";
            Add(cmd, "@e", entityType);
            Add(cmd, "@l", localId);

            var exists = Convert.ToInt32(await cmd.ExecuteScalarAsync()) == 1;
            if (exists)
                return false;

            await EnqueueAsync(db, entityType, operation, localId, payload);
            return true;
        }

        public async Task<IReadOnlyList<SyncQueueItem>> GetPendingAsync(TenantDriveConnectDbContext db,int take=25)
        {
            await EnsureSchemaAsync(db);
            var cn=db.Database.GetDbConnection();
            if(cn.State!=ConnectionState.Open) await cn.OpenAsync();
            await using var cmd=cn.CreateCommand();
            cmd.CommandText="SELECT TOP (@n) SyncQueueId,EntityType,Operation,LocalId,SyncId,PayloadJson,CreatedAt,AttemptCount FROM dbo.SyncQueue WHERE SyncedAt IS NULL ORDER BY CreatedAt,SyncQueueId";
            Add(cmd,"@n",take);
            var list=new List<SyncQueueItem>();
            await using var rd=await cmd.ExecuteReaderAsync();
            while(await rd.ReadAsync())
                list.Add(new SyncQueueItem{SyncQueueId=rd.GetInt64(0),EntityType=rd.GetString(1),Operation=rd.GetString(2),LocalId=rd.GetInt32(3),SyncId=rd.GetGuid(4),PayloadJson=rd.IsDBNull(5)?null:rd.GetString(5),CreatedAt=rd.GetDateTime(6),AttemptCount=rd.GetInt32(7)});
            return list;
        }

        public async Task MarkSyncedAsync(TenantDriveConnectDbContext db,long queueId)
        {
            await EnsureSchemaAsync(db);
            await Exec(db.Database.GetDbConnection(),"UPDATE dbo.SyncQueue SET SyncedAt=SYSUTCDATETIME(),LastAttemptAt=SYSUTCDATETIME(),LastError=NULL WHERE SyncQueueId=@id",("@id",queueId));
        }

        public async Task MarkFailedAsync(TenantDriveConnectDbContext db,long queueId,string error)
        {
            await EnsureSchemaAsync(db);
            if(error.Length>2000) error=error[..2000];
            await Exec(db.Database.GetDbConnection(),"UPDATE dbo.SyncQueue SET AttemptCount=AttemptCount+1,LastAttemptAt=SYSUTCDATETIME(),LastError=@e WHERE SyncQueueId=@id",("@id",queueId),("@e",error));
        }

        public Task<SyncEntityMapping?> GetMappingBySyncIdAsync(TenantDriveConnectDbContext db,string entityType,Guid syncId)
            => GetMap(db,"SELECT EntityType,LocalId,SyncId,RemoteId FROM dbo.SyncEntityMap WHERE EntityType=@e AND SyncId=@s",("@e",entityType),("@s",syncId));

        public Task<SyncEntityMapping?> GetMappingByLocalIdAsync(TenantDriveConnectDbContext db,string entityType,int localId)
            => GetMap(db,"SELECT EntityType,LocalId,SyncId,RemoteId FROM dbo.SyncEntityMap WHERE EntityType=@e AND LocalId=@l",("@e",entityType),("@l",localId));

        public async Task UpsertMappingAsync(TenantDriveConnectDbContext db,string entityType,int localId,Guid syncId,int remoteId)
        {
            await EnsureSchemaAsync(db);
            await Exec(db.Database.GetDbConnection(),@"IF EXISTS(SELECT 1 FROM dbo.SyncEntityMap WHERE EntityType=@e AND SyncId=@s)
UPDATE dbo.SyncEntityMap SET LocalId=@l,RemoteId=@r,UpdatedAt=SYSUTCDATETIME() WHERE EntityType=@e AND SyncId=@s
ELSE INSERT INTO dbo.SyncEntityMap(EntityType,LocalId,SyncId,RemoteId) VALUES(@e,@l,@s,@r)",
                ("@e",entityType),("@l",localId),("@s",syncId),("@r",remoteId));
        }

        public async Task DeleteMappingAsync(TenantDriveConnectDbContext db,string entityType,Guid syncId)
        {
            await EnsureSchemaAsync(db);
            await Exec(db.Database.GetDbConnection(),"DELETE FROM dbo.SyncEntityMap WHERE EntityType=@e AND SyncId=@s",("@e",entityType),("@s",syncId));
        }

        private static async Task<Guid> GetOrCreateSyncId(DbConnection cn,string entityType,int localId)
        {
            await using var find=cn.CreateCommand();
            find.CommandText="SELECT SyncId FROM dbo.SyncEntityMap WHERE EntityType=@e AND LocalId=@l";
            Add(find,"@e",entityType); Add(find,"@l",localId);
            var v=await find.ExecuteScalarAsync();
            if(v is Guid g) return g;
            var id=Guid.NewGuid();
            await Exec(cn,"INSERT INTO dbo.SyncEntityMap(EntityType,LocalId,SyncId) VALUES(@e,@l,@s)",("@e",entityType),("@l",localId),("@s",id));
            return id;
        }

        private static async Task<SyncEntityMapping?> GetMap(TenantDriveConnectDbContext db,string sql,params (string,object)[] ps)
        {
            await ((ISyncService)new SyncService()).EnsureSchemaAsync(db);
            var cn=db.Database.GetDbConnection();
            if(cn.State!=ConnectionState.Open) await cn.OpenAsync();
            await using var cmd=cn.CreateCommand(); cmd.CommandText=sql;
            foreach(var p in ps) Add(cmd,p.Item1,p.Item2);
            await using var rd=await cmd.ExecuteReaderAsync();
            if(!await rd.ReadAsync()) return null;
            return new SyncEntityMapping{EntityType=rd.GetString(0),LocalId=rd.GetInt32(1),SyncId=rd.GetGuid(2),RemoteId=rd.IsDBNull(3)?null:rd.GetInt32(3)};
        }

        private static async Task<int> Exec(DbConnection cn,string sql,params (string,object)[] ps)
        {
            if(cn.State!=ConnectionState.Open) await cn.OpenAsync();
            await using var cmd=cn.CreateCommand(); cmd.CommandText=sql;
            foreach(var p in ps) Add(cmd,p.Item1,p.Item2);
            return await cmd.ExecuteNonQueryAsync();
        }

        private static void Add(DbCommand cmd,string name,object value)
        { var p=cmd.CreateParameter(); p.ParameterName=name; p.Value=value; cmd.Parameters.Add(p); }
    }
}
