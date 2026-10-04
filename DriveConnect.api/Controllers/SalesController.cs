using DriveConnect.api.Services;
using DriveConnect.domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DriveConnect.api.Controllers;

[Route("tenant/{companyId:int}/sales")]
public sealed class SalesController : TenantControllerBase
{
    public SalesController(TenantDbContextFactory tenantDbFactory) : base(tenantDbFactory) { }

    [HttpGet]
    public async Task<ActionResult<List<SalesLead>>> GetAll(int companyId)
    {
        using var tenantDb = await GetTenantDbAsync(companyId);
        return Ok(await tenantDb.SalesLeads.AsNoTracking().ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult<SalesLead>> Create(int companyId, SalesLead lead)
    {
        using var tenantDb = await GetTenantDbAsync(companyId);
        if (lead.CreatedAt == default) lead.CreatedAt = DateTime.UtcNow;
        tenantDb.SalesLeads.Add(lead);
        await tenantDb.SaveChangesAsync();
        return Created($"/tenant/{companyId}/sales/{lead.InquiryId}", lead);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<SalesLead>> Update(int companyId, int id, SalesLead updated)
    {
        using var tenantDb = await GetTenantDbAsync(companyId);
        var existing = await tenantDb.SalesLeads.FindAsync(id);
        if (existing == null) return NotFound();

        existing.FirstName = updated.FirstName;
        existing.MiddleName = updated.MiddleName;
        existing.LastName = updated.LastName;
        existing.PhoneNumber = updated.PhoneNumber;
        existing.EmailAddress = updated.EmailAddress;
        existing.CarModel = updated.CarModel;
        existing.Status = updated.Status;
        existing.EstimatedCost = updated.EstimatedCost;
        existing.HandledBy = updated.HandledBy;

        if (updated.Status == "Closed Won" || updated.Status == "Closed Lost" || updated.Status == "Archived")
            existing.CompletedAt = updated.CompletedAt ?? DateTime.UtcNow;
        else
            existing.CompletedAt = null;

        await tenantDb.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int companyId, int id)
    {
        using var tenantDb = await GetTenantDbAsync(companyId);
        var existing = await tenantDb.SalesLeads.FindAsync(id);
        if (existing == null) return NotFound();

        tenantDb.SalesLeads.Remove(existing);
        await tenantDb.SaveChangesAsync();
        return NoContent();
    }
}
