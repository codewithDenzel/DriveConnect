using DriveConnect.domain.Entities;
using DriveConnect.infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveConnect.api.Controllers;

[Route("tenant/{companyId:int}/feedback")]
public sealed class FeedbackController : TenantControllerBase
{
    private readonly ISyncService _syncService;
    public FeedbackController(ITenantDbContextFactory tenantDbFactory, ISyncService syncService)
        : base(tenantDbFactory)
    {
        _syncService = syncService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Feedback>>> GetAll(int companyId)
    {
        if (!CanAccessCompany(companyId))
            return StatusCode(StatusCodes.Status403Forbidden);

        using var tenantDb = await GetTenantDbAsync(companyId);
        var query = tenantDb.Feedback.AsNoTracking();

        if (IsStaffUser)
        {
            if (!TryGetCurrentBranchId(out var branchId))
                return StatusCode(StatusCodes.Status403Forbidden);

            query = query.Where(x => x.BranchId == branchId);
        }

        return Ok(await query.ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult<Feedback>> Create(int companyId, Feedback item)
    {
        if (!CanAccessCompany(companyId) || !IsStaffUser)
            return StatusCode(StatusCodes.Status403Forbidden);

        if (!TryGetCurrentBranchId(out var branchId))
            return StatusCode(StatusCodes.Status403Forbidden);

        using var tenantDb = await GetTenantDbAsync(companyId);
        item.BranchId = branchId;
        if (item.CreatedAt == default) item.CreatedAt = DateTime.UtcNow;
        tenantDb.Feedback.Add(item);
        await tenantDb.SaveChangesAsync();
        await _syncService.EnqueueAsync(
            tenantDb, "Feedback", "Create", item.FeedbackId, item);
        return Created($"/tenant/{companyId}/feedback/{item.FeedbackId}", item);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Feedback>> Update(int companyId, int id, Feedback updated)
    {
        if (!CanAccessCompany(companyId))
            return Forbid();

        using var tenantDb = await GetTenantDbAsync(companyId);
        var existing = await tenantDb.Feedback.FindAsync(id);
        if (existing == null) return NotFound();
        if (!CanAccessBranch(existing.BranchId))
            return Forbid();

        existing.CustomerName = updated.CustomerName;
        existing.PhoneNumber = updated.PhoneNumber;
        existing.Type = updated.Type;
        existing.Rating = updated.Rating;
        existing.Comment = updated.Comment;
        existing.HandledBy = updated.HandledBy;
        existing.Status = updated.Status;
        existing.ReviewedAt = updated.Status == "Reviewed"
            ? updated.ReviewedAt ?? DateTime.UtcNow
            : null;

        await tenantDb.SaveChangesAsync();
        await _syncService.EnqueueAsync(
            tenantDb, "Feedback", "Update", id, existing);
        return Ok(existing);
    }
}
