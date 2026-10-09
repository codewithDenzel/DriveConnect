using System.Security.Claims;
using DriveConnect.domain.Entities;
using DriveConnect.infrastructure.Services;
using DriveConnect.infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveConnect.api.Controllers;

[Route("tenant/{companyId:int}/promotions")]
public sealed class PromotionsController : TenantControllerBase
{
    private readonly ISyncService _syncService;
    private readonly ITenantPlanService _tenantPlanService;

    public PromotionsController(
        ITenantDbContextFactory tenantDbFactory,
        ISyncService syncService,
        ITenantPlanService tenantPlanService)
        : base(tenantDbFactory)
    {
        _syncService = syncService;
        _tenantPlanService = tenantPlanService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Promotion>>> GetAll(int companyId)
    {
        if (!(await _tenantPlanService.GetFeaturesAsync(companyId)).CanUsePromotions)
            return StatusCode(StatusCodes.Status403Forbidden, "Promotions require the Pro or Pro Max plan.");


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
        if (!(await _tenantPlanService.GetFeaturesAsync(companyId)).CanUsePromotions)
            return StatusCode(StatusCodes.Status403Forbidden, "Promotions require the Pro or Pro Max plan.");


        if (!CanAccessCompany(companyId) || !IsStaffUser)
            return Forbid();

        if (!TryGetCurrentBranchId(out var branchId))
            return Forbid();

        if (string.IsNullOrWhiteSpace(item.Title) ||
            string.IsNullOrWhiteSpace(item.Description) ||
            string.IsNullOrWhiteSpace(item.Reason))
        {
            return BadRequest("Promotion Title, Description, and Reason are required.");
        }

        if (item.StartDate.Date > item.EndDate.Date)
            return BadRequest("End Date cannot be before Start Date.");

        if (item.DiscountType == "Percentage" &&
            (item.DiscountValue < 0 || item.DiscountValue > 100))
        {
            return BadRequest("Percentage discount must be between 0 and 100.");
        }

        if (item.DiscountValue < 0)
            return BadRequest("Discount Value cannot be negative.");

        using var tenantDb = await GetTenantDbAsync(companyId);

        item.BranchId = branchId;
        item.CreatedBy = User.FindFirst("displayName")?.Value ?? User.FindFirstValue(ClaimTypes.Name) ?? item.CreatedBy;
        item.ApprovedBy = null;
        item.Status = "Draft";

        if (item.CreatedAt == default)
            item.CreatedAt = DateTime.UtcNow;

        tenantDb.Promotions.Add(item);
        await tenantDb.SaveChangesAsync();

        await _syncService.EnqueueAsync(
            tenantDb,
            "Promotion",
            "Create",
            item.PromotionId,
            item);

        return Created(
            $"/tenant/{companyId}/promotions/{item.PromotionId}",
            item);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Promotion>> Update(
        int companyId,
        int id,
        Promotion updated)
    {
        if (!(await _tenantPlanService.GetFeaturesAsync(companyId)).CanUsePromotions)
            return StatusCode(StatusCodes.Status403Forbidden, "Promotions require the Pro or Pro Max plan.");


        if (!CanAccessCompany(companyId))
            return Forbid();

        using var tenantDb = await GetTenantDbAsync(companyId);
        var existing = await tenantDb.Promotions.FindAsync(id);

        if (existing == null)
            return NotFound();

        if (!CanAccessBranch(existing.BranchId))
            return Forbid();

        if (string.IsNullOrWhiteSpace(updated.Title) ||
            string.IsNullOrWhiteSpace(updated.Description) ||
            string.IsNullOrWhiteSpace(updated.Reason))
        {
            return BadRequest("Promotion Title, Description, and Reason are required.");
        }

        if (updated.StartDate.Date > updated.EndDate.Date)
            return BadRequest("End Date cannot be before Start Date.");

        if (updated.DiscountType == "Percentage" &&
            (updated.DiscountValue < 0 || updated.DiscountValue > 100))
        {
            return BadRequest("Percentage discount must be between 0 and 100.");
        }

        if (updated.DiscountValue < 0)
            return BadRequest("Discount Value cannot be negative.");

        existing.Title = updated.Title;
        existing.Description = updated.Description;
        existing.Reason = updated.Reason;
        existing.DiscountType = updated.DiscountType;
        existing.DiscountValue = updated.DiscountValue;
        existing.StartDate = updated.StartDate;
        existing.EndDate = updated.EndDate;

        if (IsStaffUser)
        {
            if (existing.Status != "Draft")
                return Forbid();

            existing.Status = "Draft";
            existing.ApprovedBy = null;
        }
        else if (IsAdminUser)
        {
            existing.Status = updated.Status;
            existing.ApprovedBy = updated.Status == "Active"
                ? User.FindFirst("displayName")?.Value ?? User.FindFirstValue(ClaimTypes.Name)
                : null;
        }

        await tenantDb.SaveChangesAsync();

        await _syncService.EnqueueAsync(
            tenantDb,
            "Promotion",
            "Update",
            id,
            existing);

        return Ok(existing);
    }
}
