using System;

namespace DriveConnect.infrastructure.Services
{
    public sealed class SyncEntityMapping
    {
        public Guid SyncId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int LocalId { get; set; }
        public int? RemoteId { get; set; }
    }

    public sealed class SyncQueueItem
    {
        public long SyncQueueId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public int LocalId { get; set; }
        public Guid SyncId { get; set; }
        public string? PayloadJson { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AttemptCount { get; set; }
    }
}
