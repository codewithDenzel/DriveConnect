using DriveConnect.domain.Entities;
using DriveConnect.infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveConnect.api.Controllers;

[Route("tenant/{companyId:int}/maintenance")]
public sealed class MaintenanceController : TenantControllerBase
{
    public MaintenanceController(ITenantDbContextFactory tenantDbFactory) : base(tenantDbFactory) { }

    [HttpGet]
    public async Task<ActionResult<List<MaintenanceRecord>>> GetAll(int companyId)
    {
        using var tenantDb = await GetTenantDbAsync(companyId);
        return Ok(await tenantDb.MaintenanceRecords.AsNoTracking().ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult<MaintenanceRecord>> Create(int companyId, MaintenanceRecord item)
    {
        using var tenantDb = await GetTenantDbAsync(companyId);
        tenantDb.MaintenanceRecords.Add(item);
        await tenantDb.SaveChangesAsync();
        return Created($"/tenant/{companyId}/maintenance/{item.MaintenanceId}", item);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MaintenanceRecord>> Update(int companyId, int id, MaintenanceRecord updated)
    {
        using var tenantDb = await GetTenantDbAsync(companyId);
        var existing = await tenantDb.MaintenanceRecords.FindAsync(id);
        if (existing == null) return NotFound();

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
        return Ok(existing);
    }
}
