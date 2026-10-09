using DriveConnect.domain.Entities;
using DriveConnect.infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveConnect.api.Controllers;

[Route("tenant/{companyId:int}/maintenance")]
public sealed class MaintenanceController : TenantControllerBase
{
    private readonly ISyncService _syncService;
    public MaintenanceController(ITenantDbContextFactory tenantDbFactory, ISyncService syncService)
        : base(tenantDbFactory)
    {
        _syncService = syncService;
    }

    [HttpGet]
    public async Task<ActionResult<List<MaintenanceRecord>>> GetAll(int companyId)
    {
        if (!CanAccessCompany(companyId))
            return Forbid();

        using var tenantDb = await GetTenantDbAsync(companyId);
        var query = tenantDb.MaintenanceRecords.AsNoTracking();

        if (IsStaffUser)
        {
            if (!TryGetCurrentBranchId(out var branchId))
                return Forbid();

            query = query.Where(x => x.BranchId == branchId);
        }

        return Ok(await query.ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult<MaintenanceRecord>> Create(int companyId, MaintenanceRecord item)
    {
        if (!CanAccessCompany(companyId) || !IsStaffUser)
            return Forbid();

        if (!TryGetCurrentBranchId(out var branchId))
            return Forbid();

        using var tenantDb = await GetTenantDbAsync(companyId);
        item.BranchId = branchId;
        tenantDb.MaintenanceRecords.Add(item);
        await tenantDb.SaveChangesAsync();
        await _syncService.EnqueueAsync(
            tenantDb, "MaintenanceRecord", "Create", item.MaintenanceId, item);
        return Created($"/tenant/{companyId}/maintenance/{item.MaintenanceId}", item);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MaintenanceRecord>> Update(int companyId, int id, MaintenanceRecord updated)
    {
        if (!CanAccessCompany(companyId))
            return Forbid();

        using var tenantDb = await GetTenantDbAsync(companyId);
        var existing = await tenantDb.MaintenanceRecords.FindAsync(id);
        if (existing == null) return NotFound();

        if (!CanAccessBranch(existing.BranchId))
            return Forbid();

        existing.CustomerName = updated.CustomerName;
        existing.PhoneNumber = updated.PhoneNumber;
        existing.VehicleModel = updated.VehicleModel;
        existing.ServiceDate = updated.ServiceDate;
        existing.ServiceType = updated.ServiceType;
        existing.PlanCoverage = updated.PlanCoverage;
        existing.AssignedStaff = updated.AssignedStaff;
        existing.Status = updated.Status;
        existing.Notes = updated.Notes;

        await tenantDb.SaveChangesAsync();
        await _syncService.EnqueueAsync(
            tenantDb, "MaintenanceRecord", "Update", id, existing);
        return Ok(existing);
    }
}
