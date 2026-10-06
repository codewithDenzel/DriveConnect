using DriveConnect.domain.Entities;
using DriveConnect.infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveConnect.api.Controllers;

[Route("tenant/{companyId:int}/sales")]
public sealed class SalesController : TenantControllerBase
{
    private readonly ISyncService _syncService;
    public SalesController(ITenantDbContextFactory tenantDbFactory, ISyncService syncService)
        : base(tenantDbFactory)
    {
        _syncService = syncService;
    }

    [HttpGet]
    public async Task<ActionResult<List<SalesLead>>> GetAll(int companyId)
    {
        if (!CanAccessCompany(companyId))
            return Forbid();

        using var tenantDb = await GetTenantDbAsync(companyId);
        var query = tenantDb.SalesLeads.AsNoTracking();

        if (IsStaffUser)
        {
            if (!TryGetCurrentBranchId(out var branchId))
                return Forbid();

            query = query.Where(x => x.BranchId == branchId);
        }

        return Ok(await query.ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult<SalesLead>> Create(int companyId, SalesLead lead)
    {
        if (!CanAccessCompany(companyId) || !IsStaffUser)
            return Forbid();

        if (!TryGetCurrentBranchId(out var branchId))
            return Forbid();

        using var tenantDb = await GetTenantDbAsync(companyId);
        lead.BranchId = branchId;
        if (lead.CreatedAt == default) lead.CreatedAt = DateTime.UtcNow;
        tenantDb.SalesLeads.Add(lead);
        await tenantDb.SaveChangesAsync();
        await _syncService.EnqueueAsync(
            tenantDb, "SalesLead", "Create", lead.InquiryId, lead);
        return Created($"/tenant/{companyId}/sales/{lead.InquiryId}", lead);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<SalesLead>> Update(int companyId, int id, SalesLead updated)
    {
        if (!CanAccessCompany(companyId))
            return Forbid();

        using var tenantDb = await GetTenantDbAsync(companyId);
        var existing = await tenantDb.SalesLeads.FindAsync(id);
        if (existing == null) return NotFound();

        if (!CanAccessBranch(existing.BranchId))
            return Forbid();

        existing.FirstName = updated.FirstName;
        existing.MiddleName = updated.MiddleName;
        existing.LastName = updated.LastName;
        existing.PhoneNumber = updated.PhoneNumber;
        existing.EmailAddress = updated.EmailAddress;
        existing.CarModel = updated.CarModel;
        existing.Status = updated.Status;
        existing.EstimatedCost = updated.EstimatedCost;
        existing.HandledBy = updated.HandledBy;

        if (IsStaffUser &&
            !TryGetCurrentBranchId(out _))
        {
            return Forbid();
        }

        if (updated.Status == "Closed Won" || updated.Status == "Closed Lost" || updated.Status == "Archived")
            existing.CompletedAt = updated.CompletedAt ?? DateTime.UtcNow;
        else
            existing.CompletedAt = null;

        await tenantDb.SaveChangesAsync();
        await _syncService.EnqueueAsync(
            tenantDb, "SalesLead", "Update", id, existing);
        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int companyId, int id)
    {
        if (!CanAccessCompany(companyId))
            return Forbid();

        using var tenantDb = await GetTenantDbAsync(companyId);
        var existing = await tenantDb.SalesLeads.FindAsync(id);
        if (existing == null) return NotFound();

        if (!CanAccessBranch(existing.BranchId))
            return Forbid();

        tenantDb.SalesLeads.Remove(existing);
        await tenantDb.SaveChangesAsync();
        await _syncService.EnqueueAsync(
            tenantDb, "SalesLead", "Delete", id, null);
        return NoContent();
    }
}
