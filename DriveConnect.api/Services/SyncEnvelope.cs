using System;

namespace DriveConnect.api.Services;

public sealed class SyncEnvelope
{
    public string EntityType { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public int LocalId { get; set; }
    public Guid SyncId { get; set; }
    public string? PayloadJson { get; set; }
}
