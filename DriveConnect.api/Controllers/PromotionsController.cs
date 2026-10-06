using DriveConnect.domain.Entities;
using DriveConnect.infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveConnect.api.Controllers;

[Route("tenant/{companyId:int}/promotions")]
public sealed class PromotionsController : TenantControllerBase
{
    private readonly ISyncService _syncService;
    public PromotionsController(ITenantDbContextFactory tenantDbFactory, ISyncService syncService)
        : base(tenantDbFactory)
    {
        _syncService = syncService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Promotion>>> GetAll(int companyId)
    {
        if (!CanAccessCompany(companyId))
            return Forbid();

        using var tenantDb = await GetTenantDbAsync(companyId);
        var query = tenantDb.Promotions.AsNoTracking();

        if (IsStaffUser)
        {
            if (!TryGetCurrentBranchId(out var branchId))
                return Forbid();

            query = query.Where(x => x.BranchId == branchId);
        }

        return Ok(await query.ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult<Promotion>> Create(int companyId, Promotion item)
    {
        if (!CanAccessCompany(companyId) || !IsStaffUser)
            return Forbid();

        if (!TryGetCurrentBranchId(out var branchId))
            return Forbid();

        using var tenantDb = await GetTenantDbAsync(companyId);
        item.BranchId = branchId;
        if (item.CreatedAt == default) item.CreatedAt = DateTime.UtcNow;
        tenantDb.Promotions.Add(item);
        await tenantDb.SaveChangesAsync();
        await _syncService.EnqueueAsync(
            tenantDb, "Promotion", "Create", item.PromotionId, item);
        return Created($"/tenant/{companyId}/promotions/{item.PromotionId}", item);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Promotion>> Update(int companyId, int id, Promotion updated)
    {
        if (!CanAccessCompany(companyId))
            return Forbid();

        using var tenantDb = await GetTenantDbAsync(companyId);
        var existing = await tenantDb.Promotions.FindAsync(id);
        if (existing == null) return NotFound();

        if (!CanAccessBranch(existing.BranchId))
            return Forbid();

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
        await _syncService.EnqueueAsync(
            tenantDb, "Promotion", "Update", id, existing);
        return Ok(existing);
    }
}
