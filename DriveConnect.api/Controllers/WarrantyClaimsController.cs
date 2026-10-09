using DriveConnect.domain.Entities;
using DriveConnect.infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveConnect.api.Controllers;

[Route("tenant/{companyId:int}/warranty-claims")]
public sealed class WarrantyClaimsController : TenantControllerBase
{
    private readonly ISyncService _syncService;
    public WarrantyClaimsController(ITenantDbContextFactory tenantDbFactory, ISyncService syncService)
        : base(tenantDbFactory)
    {
        _syncService = syncService;
    }

    [HttpGet]
    public async Task<ActionResult<List<WarrantyClaim>>> GetAll(int companyId)
    {
        if (!CanAccessCompany(companyId))
            return Forbid();

        using var tenantDb = await GetTenantDbAsync(companyId);
        var query = tenantDb.WarrantyClaims.AsNoTracking();

        if (IsStaffUser)
        {
            if (!TryGetCurrentBranchId(out var branchId))
                return Forbid();

            query = query.Where(x => x.BranchId == branchId);
        }

        return Ok(await query.ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult<WarrantyClaim>> Create(int companyId, WarrantyClaim item)
    {
        if (!CanAccessCompany(companyId) || !IsStaffUser)
            return Forbid();

        if (!TryGetCurrentBranchId(out var branchId))
            return Forbid();

        using var tenantDb = await GetTenantDbAsync(companyId);
        item.BranchId = branchId;
        if (item.DateReported == default) item.DateReported = DateTime.UtcNow;
        tenantDb.WarrantyClaims.Add(item);
        await tenantDb.SaveChangesAsync();
        await _syncService.EnqueueAsync(
            tenantDb, "WarrantyClaim", "Create", item.ClaimId, item);
        return Created($"/tenant/{companyId}/warranty-claims/{item.ClaimId}", item);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<WarrantyClaim>> Update(int companyId, int id, WarrantyClaim updated)
    {
        if (!CanAccessCompany(companyId))
            return Forbid();

        using var tenantDb = await GetTenantDbAsync(companyId);
        var existing = await tenantDb.WarrantyClaims.FindAsync(id);
        if (existing == null) return NotFound();

        if (!CanAccessBranch(existing.BranchId))
            return Forbid();

        existing.WarrantyId = updated.WarrantyId;
        existing.CustomerName = updated.CustomerName;
        existing.PhoneNumber = updated.PhoneNumber;
        existing.VehicleModel = updated.VehicleModel;
        existing.Problem = updated.Problem;
        existing.DateReported = updated.DateReported;
        existing.HandledBy = updated.HandledBy;
        existing.Status = updated.Status;
        existing.Resolution = updated.Resolution;
        existing.DateResolved = updated.Status == "Resolved"
            ? updated.DateResolved ?? DateTime.UtcNow
            : null;

        await tenantDb.SaveChangesAsync();
        await _syncService.EnqueueAsync(
            tenantDb, "WarrantyClaim", "Update", id, existing);
        return Ok(existing);
    }
}
