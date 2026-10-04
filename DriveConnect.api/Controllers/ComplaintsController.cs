using DriveConnect.domain.Entities;
using DriveConnect.infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveConnect.api.Controllers;

[Route("tenant/{companyId:int}/complaints")]
public sealed class ComplaintsController : TenantControllerBase
{
    private readonly ISyncService _syncService;
    public ComplaintsController(ITenantDbContextFactory tenantDbFactory, ISyncService syncService)
        : base(tenantDbFactory)
    {
        _syncService = syncService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Complaint>>> GetAll(int companyId)
    {
        using var tenantDb = await GetTenantDbAsync(companyId);
        return Ok(await tenantDb.Complaints.AsNoTracking().ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult<Complaint>> Create(int companyId, Complaint item)
    {
        using var tenantDb = await GetTenantDbAsync(companyId);
        if (item.CreatedAt == default) item.CreatedAt = DateTime.UtcNow;
        tenantDb.Complaints.Add(item);
        await tenantDb.SaveChangesAsync();
        await _syncService.EnqueueAsync(
            tenantDb, "Complaint", "Create", item.ComplaintId, item);
        return Created($"/tenant/{companyId}/complaints/{item.ComplaintId}", item);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Complaint>> Update(int companyId, int id, Complaint updated)
    {
        using var tenantDb = await GetTenantDbAsync(companyId);
        var existing = await tenantDb.Complaints.FindAsync(id);
        if (existing == null) return NotFound();

        existing.CustomerName = updated.CustomerName;
        existing.PhoneNumber = updated.PhoneNumber;
        existing.Category = updated.Category;
        existing.Description = updated.Description;
        existing.Priority = updated.Priority;
        existing.HandledBy = updated.HandledBy;
        existing.Status = updated.Status;
        existing.Resolution = updated.Resolution;
        existing.ResolvedAt = updated.Status == "Resolved" || updated.Status == "Closed"
            ? updated.ResolvedAt ?? DateTime.UtcNow
            : null;

        await tenantDb.SaveChangesAsync();
        await _syncService.EnqueueAsync(
            tenantDb, "Complaint", "Update", id, existing);
        return Ok(existing);
    }
}
