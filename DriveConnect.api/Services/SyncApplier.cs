using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using DriveConnect.domain.Entities;
using DriveConnect.infrastructure.Data;
using DriveConnect.infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace DriveConnect.api.Services;

public sealed class SyncApplier
{
    private readonly ISyncService _syncService;

    private static readonly Dictionary<string, (Type Type, string Key)> Types = new()
    {
        ["SalesLead"] = (typeof(SalesLead), nameof(SalesLead.InquiryId)),
        ["RepairTicket"] = (typeof(RepairTicket), nameof(RepairTicket.TicketId)),
        ["Promotion"] = (typeof(Promotion), nameof(Promotion.PromotionId)),
        ["Feedback"] = (typeof(Feedback), nameof(Feedback.FeedbackId)),
        ["Complaint"] = (typeof(Complaint), nameof(Complaint.ComplaintId)),
        ["InteractionLog"] = (typeof(InteractionLog), nameof(InteractionLog.InteractionId)),
        ["VehicleWarranty"] = (typeof(VehicleWarranty), nameof(VehicleWarranty.WarrantyId)),
        ["WarrantyClaim"] = (typeof(WarrantyClaim), nameof(WarrantyClaim.ClaimId)),
        ["MaintenanceRecord"] = (typeof(MaintenanceRecord), nameof(MaintenanceRecord.MaintenanceId))
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public SyncApplier(ISyncService syncService)
    {
        _syncService = syncService;
    }

    public async Task<object> ApplyAsync(
        TenantDriveConnectDbContext db,
        SyncEnvelope envelope)
    {
        if (!Types.TryGetValue(envelope.EntityType, out var info))
            throw new InvalidOperationException($"Unknown sync entity type: {envelope.EntityType}");

        if (string.Equals(envelope.Operation, "Delete", StringComparison.OrdinalIgnoreCase))
            return await ApplyDeleteAsync(db, envelope, info);

        if (string.IsNullOrWhiteSpace(envelope.PayloadJson))
            throw new InvalidOperationException("Sync payload is empty.");

        var incoming = JsonSerializer.Deserialize(
            envelope.PayloadJson,
            info.Type,
            JsonOptions);

        if (incoming == null)
            throw new InvalidOperationException("Sync payload could not be read.");

        var mapping = await _syncService.GetMappingBySyncIdAsync(
            db, envelope.EntityType, envelope.SyncId);

        if (info.Type == typeof(WarrantyClaim))
        {
            await ResolveWarrantyReferenceAsync(db, (WarrantyClaim)incoming);
        }

        object? existing = null;

        if (mapping?.RemoteId is int remoteId)
        {
            existing = await db.FindAsync(info.Type, remoteId);
        }

        if (existing != null)
        {
            SetKey(incoming, info.Key, mapping!.RemoteId!.Value);
            db.Entry(existing).CurrentValues.SetValues(incoming);

            await db.SaveChangesAsync();

            await _syncService.UpsertMappingAsync(
                db,
                envelope.EntityType,
                envelope.LocalId,
                envelope.SyncId,
                mapping.RemoteId.Value);

            return new { action = "updated", id = mapping.RemoteId.Value };
        }

        SetKey(incoming, info.Key, 0);
        db.Add(incoming);

        await db.SaveChangesAsync();

        var newRemoteId = GetKey(incoming, info.Key);

        await _syncService.UpsertMappingAsync(
            db,
            envelope.EntityType,
            envelope.LocalId,
            envelope.SyncId,
            newRemoteId);

        return new { action = "created", id = newRemoteId };
    }

    private async Task<object> ApplyDeleteAsync(
        TenantDriveConnectDbContext db,
        SyncEnvelope envelope,
        (Type Type, string Key) info)
    {
        var mapping = await _syncService.GetMappingBySyncIdAsync(
            db, envelope.EntityType, envelope.SyncId);

        if (mapping?.RemoteId is int remoteId)
        {
            var existing = await db.FindAsync(info.Type, remoteId);

            if (existing != null)
            {
                db.Remove(existing);
                await db.SaveChangesAsync();
            }
        }

        await _syncService.DeleteMappingAsync(
            db, envelope.EntityType, envelope.SyncId);

        return new { action = "deleted", id = mapping?.RemoteId };
    }

    private async Task ResolveWarrantyReferenceAsync(
        TenantDriveConnectDbContext db,
        WarrantyClaim claim)
    {
        var warrantyMapping = await _syncService.GetMappingByLocalIdAsync(
            db,
            "VehicleWarranty",
            claim.WarrantyId);

        if (warrantyMapping?.RemoteId is not int remoteWarrantyId)
        {
            throw new InvalidOperationException(
                $"Warranty {claim.WarrantyId} has not synced yet.");
        }

        claim.WarrantyId = remoteWarrantyId;
    }

    private static int GetKey(object entity, string propertyName)
    {
        var property = entity.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public);

        if (property == null)
            throw new InvalidOperationException(
                $"Primary key property {propertyName} was not found.");

        return Convert.ToInt32(property.GetValue(entity));
    }

    private static void SetKey(object entity, string propertyName, int value)
    {
        var property = entity.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public);

        if (property == null)
            throw new InvalidOperationException(
                $"Primary key property {propertyName} was not found.");

        property.SetValue(entity, value);
    }
}
