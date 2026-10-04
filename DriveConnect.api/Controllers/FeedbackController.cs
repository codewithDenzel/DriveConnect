using DriveConnect.domain.Entities;
using DriveConnect.infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveConnect.api.Controllers;

[Route("tenant/{companyId:int}/feedback")]
public sealed class FeedbackController : TenantControllerBase
{
    public FeedbackController(ITenantDbContextFactory tenantDbFactory) : base(tenantDbFactory) { }

    [HttpGet]
    public async Task<ActionResult<List<Feedback>>> GetAll(int companyId)
    {
        using var tenantDb = await GetTenantDbAsync(companyId);
        return Ok(await tenantDb.Feedback.AsNoTracking().ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult<Feedback>> Create(int companyId, Feedback item)
    {
        using var tenantDb = await GetTenantDbAsync(companyId);
        if (item.CreatedAt == default) item.CreatedAt = DateTime.UtcNow;
        tenantDb.Feedback.Add(item);
        await tenantDb.SaveChangesAsync();
        return Created($"/tenant/{companyId}/feedback/{item.FeedbackId}", item);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Feedback>> Update(int companyId, int id, Feedback updated)
    {
        using var tenantDb = await GetTenantDbAsync(companyId);
        var existing = await tenantDb.Feedback.FindAsync(id);
        if (existing == null) return NotFound();

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
        return Ok(existing);
    }
}
