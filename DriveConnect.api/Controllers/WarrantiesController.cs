using DriveConnect.domain.Entities;
using DriveConnect.infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveConnect.api.Controllers;

[Route("tenant/{companyId:int}/warranties")]
public sealed class WarrantiesController : TenantControllerBase
{
    private readonly ISyncService _syncService;
    public WarrantiesController(ITenantDbContextFactory tenantDbFactory, ISyncService syncService)
        : base(tenantDbFactory)
    {
        _syncService = syncService;
    }

    [HttpGet]
    public async Task<ActionResult<List<VehicleWarranty>>> GetAll(int companyId)
    {
        if (!CanAccessCompany(companyId))
            return Forbid();

        using var tenantDb = await GetTenantDbAsync(companyId);
        var query = tenantDb.VehicleWarranties.AsNoTracking();

        if (IsStaffUser)
        {
            if (!TryGetCurrentBranchId(out var branchId))
                return Forbid();

            query = query.Where(x => x.BranchId == branchId);
        }

        return Ok(await query.ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult<VehicleWarranty>> Create(int companyId, VehicleWarranty item)
    {
        if (!CanAccessCompany(companyId) || !IsStaffUser)
            return Forbid();

        if (!TryGetCurrentBranchId(out var branchId))
            return Forbid();

        using var tenantDb = await GetTenantDbAsync(companyId);
        item.BranchId = branchId;
        tenantDb.VehicleWarranties.Add(item);
        await tenantDb.SaveChangesAsync();
        await _syncService.EnqueueAsync(
            tenantDb, "VehicleWarranty", "Create", item.WarrantyId, item);
        return Created($"/tenant/{companyId}/warranties/{item.WarrantyId}", item);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<VehicleWarranty>> Update(int companyId, int id, VehicleWarranty updated)
    {
        if (!CanAccessCompany(companyId))
            return Forbid();

        using var tenantDb = await GetTenantDbAsync(companyId);
        var existing = await tenantDb.VehicleWarranties.FindAsync(id);
        if (existing == null) return NotFound();

        if (!CanAccessBranch(existing.BranchId))
            return Forbid();

        existing.CustomerName = updated.CustomerName;
        existing.PhoneNumber = updated.PhoneNumber;
        existing.VehicleModel = updated.VehicleModel;
        existing.PurchaseDate = updated.PurchaseDate;
        existing.WarrantyStart = updated.WarrantyStart;
        existing.WarrantyEnd = updated.WarrantyEnd;
        existing.Coverage = updated.Coverage;
        existing.Status = updated.Status;

        await tenantDb.SaveChangesAsync();
        await _syncService.EnqueueAsync(
            tenantDb, "VehicleWarranty", "Update", id, existing);
        return Ok(existing);
    }
}
