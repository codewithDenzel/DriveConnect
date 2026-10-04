using DriveConnect.domain.Entities;
using DriveConnect.infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveConnect.api.Controllers;

[Route("tenant/{companyId:int}/warranty-claims")]
public sealed class WarrantyClaimsController : TenantControllerBase
{
    public WarrantyClaimsController(ITenantDbContextFactory tenantDbFactory) : base(tenantDbFactory) { }

    [HttpGet]
    public async Task<ActionResult<List<WarrantyClaim>>> GetAll(int companyId)
    {
        using var tenantDb = await GetTenantDbAsync(companyId);
        return Ok(await tenantDb.WarrantyClaims.AsNoTracking().ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult<WarrantyClaim>> Create(int companyId, WarrantyClaim item)
    {
        using var tenantDb = await GetTenantDbAsync(companyId);
        if (item.DateReported == default) item.DateReported = DateTime.UtcNow;
        tenantDb.WarrantyClaims.Add(item);
        await tenantDb.SaveChangesAsync();
        return Created($"/tenant/{companyId}/warranty-claims/{item.ClaimId}", item);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<WarrantyClaim>> Update(int companyId, int id, WarrantyClaim updated)
    {
        using var tenantDb = await GetTenantDbAsync(companyId);
        var existing = await tenantDb.WarrantyClaims.FindAsync(id);
        if (existing == null) return NotFound();

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
        return Ok(existing);
    }
}
