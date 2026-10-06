using DriveConnect.domain.Entities;
using DriveConnect.infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveConnect.api.Controllers;

[Route("tenant/{companyId:int}/interactions")]
public sealed class InteractionsController : TenantControllerBase
{
    private readonly ISyncService _syncService;
    public InteractionsController(ITenantDbContextFactory tenantDbFactory, ISyncService syncService)
        : base(tenantDbFactory)
    {
        _syncService = syncService;
    }

    [HttpGet]
    public async Task<ActionResult<List<InteractionLog>>> GetAll(int companyId)
    {
        if (!CanAccessCompany(companyId))
            return Forbid();

        using var tenantDb = await GetTenantDbAsync(companyId);
        var query = tenantDb.InteractionLogs.AsNoTracking();

        if (IsStaffUser)
        {
            if (!TryGetCurrentBranchId(out var branchId))
                return Forbid();

            query = query.Where(x => x.BranchId == branchId);
        }

        return Ok(await query.ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult<InteractionLog>> Create(int companyId, InteractionLog item)
    {
        if (!CanAccessCompany(companyId) || !IsStaffUser)
            return Forbid();

        if (!TryGetCurrentBranchId(out var branchId))
            return Forbid();

        using var tenantDb = await GetTenantDbAsync(companyId);
        item.BranchId = branchId;
        if (item.CreatedAt == default) item.CreatedAt = DateTime.UtcNow;
        tenantDb.InteractionLogs.Add(item);
        await tenantDb.SaveChangesAsync();
        await _syncService.EnqueueAsync(
            tenantDb, "InteractionLog", "Create", item.InteractionId, item);
        return Created($"/tenant/{companyId}/interactions/{item.InteractionId}", item);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<InteractionLog>> Update(int companyId, int id, InteractionLog updated)
    {
        using var tenantDb = await GetTenantDbAsync(companyId);
        var existing = await tenantDb.InteractionLogs.FindAsync(id);
        if (existing == null) return NotFound();

        if (!CanAccessBranch(existing.BranchId))
            return Forbid();

        existing.CustomerName = updated.CustomerName;
        existing.PhoneNumber = updated.PhoneNumber;
        existing.InteractionType = updated.InteractionType;
        existing.Subject = updated.Subject;
        existing.Notes = updated.Notes;
        existing.HandledBy = updated.HandledBy;

        await tenantDb.SaveChangesAsync();
        await _syncService.EnqueueAsync(
            tenantDb, "InteractionLog", "Update", id, existing);
        return Ok(existing);
    }
}
