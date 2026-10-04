using DriveConnect.domain.Entities;
using DriveConnect.infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveConnect.api.Controllers;

[Route("tenant/{companyId:int}/promotions")]
public sealed class PromotionsController : TenantControllerBase
{
    public PromotionsController(ITenantDbContextFactory tenantDbFactory) : base(tenantDbFactory) { }

    [HttpGet]
    public async Task<ActionResult<List<Promotion>>> GetAll(int companyId)
    {
        using var tenantDb = await GetTenantDbAsync(companyId);
        return Ok(await tenantDb.Promotions.AsNoTracking().ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult<Promotion>> Create(int companyId, Promotion item)
    {
        using var tenantDb = await GetTenantDbAsync(companyId);
        if (item.CreatedAt == default) item.CreatedAt = DateTime.UtcNow;
        tenantDb.Promotions.Add(item);
        await tenantDb.SaveChangesAsync();
        return Created($"/tenant/{companyId}/promotions/{item.PromotionId}", item);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Promotion>> Update(int companyId, int id, Promotion updated)
    {
        using var tenantDb = await GetTenantDbAsync(companyId);
        var existing = await tenantDb.Promotions.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Title = updated.Title;
        existing.Description = updated.Description;
        existing.DiscountType = updated.DiscountType;
        existing.DiscountValue = updated.DiscountValue;
        existing.StartDate = updated.StartDate;
        existing.EndDate = updated.EndDate;
        existing.CreatedBy = updated.CreatedBy;
        existing.ApprovedBy = updated.ApprovedBy;
        existing.Status = updated.Status;

        await tenantDb.SaveChangesAsync();
        return Ok(existing);
    }
}
