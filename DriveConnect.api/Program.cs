using DriveConnect.domain.Entities;
using DriveConnect.infrastructure.Data;
using DriveConnect.infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MasterDriveConnectDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ITenantDatabaseResolver, TenantDatabaseResolver>();

var app = builder.Build();
app.UseHttpsRedirection();

// --- SALES LEADS ENDPOINTS ---
app.MapPost("/tenant/{companyId:int}/sales", async (int companyId, SalesLead lead, ITenantDatabaseResolver resolver, IConfiguration config) =>
{
    using var tenantDb = await GetTenantDb(companyId, resolver, config);
    tenantDb.SalesLeads.Add(lead);
    await tenantDb.SaveChangesAsync();
    return Results.Created($"/tenant/{companyId}/sales/{lead.InquiryId}", lead);
});

app.MapGet("/tenant/{companyId:int}/sales", async (int companyId, ITenantDatabaseResolver resolver, IConfiguration config) =>
{
    using var tenantDb = await GetTenantDb(companyId, resolver, config);
    return Results.Ok(await tenantDb.SalesLeads.AsNoTracking().ToListAsync());
});

app.MapPut("/tenant/{companyId:int}/sales/{id:int}", async (int companyId, int id, SalesLead updated, ITenantDatabaseResolver resolver, IConfiguration config) =>
{
    using var tenantDb = await GetTenantDb(companyId, resolver, config);
    var existing = await tenantDb.SalesLeads.FindAsync(id);
    if (existing == null) return Results.NotFound();

    existing.FirstName = updated.FirstName; existing.MiddleName = updated.MiddleName; existing.LastName = updated.LastName;
    existing.PhoneNumber = updated.PhoneNumber; existing.EmailAddress = updated.EmailAddress; existing.CarModel = updated.CarModel;
    existing.Status = updated.Status; existing.EstimatedCost = updated.EstimatedCost; existing.HandledBy = updated.HandledBy;
    existing.CompletedAt = updated.CompletedAt;

    await tenantDb.SaveChangesAsync(); return Results.Ok(existing);
});

app.MapDelete("/tenant/{companyId:int}/sales/{id:int}", async (int companyId, int id, ITenantDatabaseResolver resolver, IConfiguration config) =>
{
    using var tenantDb = await GetTenantDb(companyId, resolver, config);
    var existing = await tenantDb.SalesLeads.FindAsync(id);
    if (existing == null) return Results.NotFound();
    tenantDb.SalesLeads.Remove(existing); await tenantDb.SaveChangesAsync(); return Results.NoContent();
});

// --- REPAIR TICKETS ENDPOINTS ---
app.MapPost("/tenant/{companyId:int}/repairs", async (int companyId, RepairTicket ticket, ITenantDatabaseResolver resolver, IConfiguration config) =>
{
    using var tenantDb = await GetTenantDb(companyId, resolver, config);
    tenantDb.RepairTickets.Add(ticket);
    await tenantDb.SaveChangesAsync();
    return Results.Created($"/tenant/{companyId}/repairs/{ticket.TicketId}", ticket);
});

app.MapGet("/tenant/{companyId:int}/repairs", async (int companyId, ITenantDatabaseResolver resolver, IConfiguration config) =>
{
    using var tenantDb = await GetTenantDb(companyId, resolver, config);
    return Results.Ok(await tenantDb.RepairTickets.AsNoTracking().ToListAsync());
});

app.MapPut("/tenant/{companyId:int}/repairs/{id:int}", async (int companyId, int id, RepairTicket updated, ITenantDatabaseResolver resolver, IConfiguration config) =>
{
    using var tenantDb = await GetTenantDb(companyId, resolver, config);
    var existing = await tenantDb.RepairTickets.FindAsync(id);
    if (existing == null) return Results.NotFound();

    existing.FirstName = updated.FirstName; existing.MiddleName = updated.MiddleName; existing.LastName = updated.LastName;
    existing.PhoneNumber = updated.PhoneNumber; existing.EmailAddress = updated.EmailAddress; existing.CarModel = updated.CarModel;
    existing.Concern = updated.Concern; existing.Status = updated.Status; existing.EstimatedCost = updated.EstimatedCost;
    existing.HandledBy = updated.HandledBy; existing.CompletedAt = updated.CompletedAt;
    existing.PickupStatus = updated.PickupStatus; existing.PickedUpAt = updated.PickedUpAt;

    await tenantDb.SaveChangesAsync(); return Results.Ok(existing);
});

app.MapDelete("/tenant/{companyId:int}/repairs/{id:int}", async (int companyId, int id, ITenantDatabaseResolver resolver, IConfiguration config) =>
{
    using var tenantDb = await GetTenantDb(companyId, resolver, config);
    var existing = await tenantDb.RepairTickets.FindAsync(id);
    if (existing == null) return Results.NotFound();
    tenantDb.RepairTickets.Remove(existing); await tenantDb.SaveChangesAsync(); return Results.NoContent();
});

app.Run();

// HELPER: Generates the connection dynamically without duplicating code everywhere
async Task<TenantDriveConnectDbContext> GetTenantDb(int companyId, ITenantDatabaseResolver resolver, IConfiguration config)
{
    var dbInfo = await resolver.GetDatabaseInfoAsync(companyId);
    string connString = dbInfo.ServerName.Contains("(localdb)", StringComparison.OrdinalIgnoreCase)
        ? $"Server={dbInfo.ServerName};Database={dbInfo.DatabaseName};Trusted_Connection=True;TrustServerCertificate=True;"
        : $"Server={dbInfo.ServerName};Database={dbInfo.DatabaseName};User Id={config[$"TenantCredentials:{dbInfo.CredentialKey}:UserId"]};Password={config[$"TenantCredentials:{dbInfo.CredentialKey}:Password"]};TrustServerCertificate=True;";

    var options = new DbContextOptionsBuilder<TenantDriveConnectDbContext>().UseSqlServer(connString).Options;
    return new TenantDriveConnectDbContext(options);
}