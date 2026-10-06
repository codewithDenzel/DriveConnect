using DriveConnect.domain.Entities;
using DriveConnect.infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveConnect.api.Controllers;

[Route("tenant/{companyId:int}/repairs")]
public sealed class RepairsController : TenantControllerBase
{
    private readonly ISyncService _syncService;
    public RepairsController(ITenantDbContextFactory tenantDbFactory, ISyncService syncService)
        : base(tenantDbFactory)
    {
        _syncService = syncService;
    }

    [HttpGet]
    public async Task<ActionResult<List<RepairTicket>>> GetAll(int companyId)
    {
        if (!CanAccessCompany(companyId))
            return Forbid();

        using var tenantDb = await GetTenantDbAsync(companyId);
        var query = tenantDb.RepairTickets.AsNoTracking();

        if (IsStaffUser)
        {
            if (!TryGetCurrentBranchId(out var branchId))
                return Forbid();

            query = query.Where(x => x.BranchId == branchId);
        }

        return Ok(await query.ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult<RepairTicket>> Create(int companyId, RepairTicket ticket)
    {
        using var tenantDb = await GetTenantDbAsync(companyId);
        if (ticket.CreatedAt == default) ticket.CreatedAt = DateTime.UtcNow;
        tenantDb.RepairTickets.Add(ticket);
        await tenantDb.SaveChangesAsync();
        await _syncService.EnqueueAsync(
            tenantDb, "RepairTicket", "Create", ticket.TicketId, ticket);
        return Created($"/tenant/{companyId}/repairs/{ticket.TicketId}", ticket);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<RepairTicket>> Update(int companyId, int id, RepairTicket updated)
    {
        using var tenantDb = await GetTenantDbAsync(companyId);
        var existing = await tenantDb.RepairTickets.FindAsync(id);
        if (existing == null) return NotFound();

        existing.FirstName = updated.FirstName;
        existing.MiddleName = updated.MiddleName;
        existing.LastName = updated.LastName;
        existing.PhoneNumber = updated.PhoneNumber;
        existing.EmailAddress = updated.EmailAddress;
        existing.CarModel = updated.CarModel;
        existing.Concern = updated.Concern;
        existing.Status = updated.Status;
        existing.EstimatedCost = updated.EstimatedCost;
        existing.HandledBy = updated.HandledBy;
        existing.PickupStatus = updated.PickupStatus;

        if (updated.Status == "Repaired" || updated.Status == "Archived")
            existing.CompletedAt = updated.CompletedAt ?? DateTime.UtcNow;
        else
            existing.CompletedAt = null;

        if (updated.PickupStatus == "Picked Up")
            existing.PickedUpAt = updated.PickedUpAt ?? DateTime.UtcNow;

        await tenantDb.SaveChangesAsync();
        await _syncService.EnqueueAsync(
            tenantDb, "RepairTicket", "Update", id, existing);
        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int companyId, int id)
    {
        using var tenantDb = await GetTenantDbAsync(companyId);
        var existing = await tenantDb.RepairTickets.FindAsync(id);
        if (existing == null) return NotFound();

        tenantDb.RepairTickets.Remove(existing);
        await tenantDb.SaveChangesAsync();
        await _syncService.EnqueueAsync(
            tenantDb, "RepairTicket", "Delete", id, null);
        return NoContent();
    }
}
