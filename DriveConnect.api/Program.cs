using DriveConnect.domain.Entities;
using DriveConnect.infrastructure.Data;
using DriveConnect.infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MasterDriveConnectDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

builder.Services.AddScoped<ITenantDatabaseResolver, TenantDatabaseResolver>();

var app = builder.Build();

app.UseHttpsRedirection();

// --- SALES LEADS ENDPOINTS ---

app.MapPost("/tenant/{companyId:int}/sales", async (int companyId, SalesLead lead, ITenantDatabaseResolver resolver, IConfiguration config) =>
{
    using var tenantDb = await GetTenantDb(companyId, resolver, config);

    // Ensure CreatedAt is set
    if (lead.CreatedAt == default) lead.CreatedAt = DateTime.UtcNow;

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

    existing.FirstName = updated.FirstName;
    existing.MiddleName = updated.MiddleName;
    existing.LastName = updated.LastName;
    existing.PhoneNumber = updated.PhoneNumber;
    existing.EmailAddress = updated.EmailAddress;
    existing.CarModel = updated.CarModel;
    existing.Status = updated.Status;
    existing.EstimatedCost = updated.EstimatedCost;
    existing.HandledBy = updated.HandledBy;

    // Timestamp rules
    if (updated.Status == "Closed Won" || updated.Status == "Closed Lost" || updated.Status == "Archived")
    {
        existing.CompletedAt = updated.CompletedAt ?? DateTime.UtcNow;
    }
    else
    {
        existing.CompletedAt = null;
    }

    await tenantDb.SaveChangesAsync();
    return Results.Ok(existing);
});

app.MapDelete("/tenant/{companyId:int}/sales/{id:int}", async (int companyId, int id, ITenantDatabaseResolver resolver, IConfiguration config) =>
{
    using var tenantDb = await GetTenantDb(companyId, resolver, config);
    var existing = await tenantDb.SalesLeads.FindAsync(id);
    if (existing == null) return Results.NotFound();

    tenantDb.SalesLeads.Remove(existing);
    await tenantDb.SaveChangesAsync();
    return Results.NoContent();
});

// --- REPAIR TICKETS ENDPOINTS ---

app.MapPost("/tenant/{companyId:int}/repairs", async (int companyId, RepairTicket ticket, ITenantDatabaseResolver resolver, IConfiguration config) =>
{
    using var tenantDb = await GetTenantDb(companyId, resolver, config);

    if (ticket.CreatedAt == default) ticket.CreatedAt = DateTime.UtcNow;

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

    existing.FirstName = updated.FirstName;
    existing.MiddleName = updated.MiddleName;
    existing.LastName = updated.LastName;
    existing.PhoneNumber = updated.PhoneNumber;
    existing.EmailAddress = updated.EmailAddress;
    existing.CarModel = updated.CarModel;
    existing.Concern = updated.Concern;
    existing.Status = updated.Status;
    existing.EstimatedCost = updated.EstimatedCost;
    existing.HandledBy = updated.HandledBy;
    existing.PickupStatus = updated.PickupStatus;

    // Timestamp rules
    if (updated.Status == "Repaired" || updated.Status == "Archived")
    {
        existing.CompletedAt = updated.CompletedAt ?? DateTime.UtcNow;
    }
    else
    {
        existing.CompletedAt = null;
    }

    if (updated.PickupStatus == "Picked Up")
    {
        existing.PickedUpAt = updated.PickedUpAt ?? DateTime.UtcNow;
    }

    await tenantDb.SaveChangesAsync();
    return Results.Ok(existing);
});

app.MapDelete("/tenant/{companyId:int}/repairs/{id:int}", async (int companyId, int id, ITenantDatabaseResolver resolver, IConfiguration config) =>
{
    using var tenantDb = await GetTenantDb(companyId, resolver, config);
    var existing = await tenantDb.RepairTickets.FindAsync(id);
    if (existing == null) return Results.NotFound();

    tenantDb.RepairTickets.Remove(existing);
    await tenantDb.SaveChangesAsync();
    return Results.NoContent();
});

// --- CRM HISTORY AND CUSTOMER ENGAGEMENT ENDPOINTS ---

 app.MapGet("/tenant/{companyId:int}/promotions", async (int companyId, ITenantDatabaseResolver resolver, IConfiguration config) =>
 {
     using var tenantDb = await GetTenantDb(companyId, resolver, config);
     return Results.Ok(await tenantDb.Promotions.AsNoTracking().ToListAsync());
 });

 app.MapPost("/tenant/{companyId:int}/promotions", async (int companyId, Promotion item, ITenantDatabaseResolver resolver, IConfiguration config) =>
 {
     using var tenantDb = await GetTenantDb(companyId, resolver, config);
     if (item.CreatedAt == default) item.CreatedAt = DateTime.UtcNow;
     tenantDb.Promotions.Add(item);
     await tenantDb.SaveChangesAsync();
     return Results.Created($"/tenant/{companyId}/promotions/{item.PromotionId}", item);
 });

 app.MapPut("/tenant/{companyId:int}/promotions/{id:int}", async (int companyId, int id, Promotion updated, ITenantDatabaseResolver resolver, IConfiguration config) =>
 {
     using var tenantDb = await GetTenantDb(companyId, resolver, config);
     var existing = await tenantDb.Promotions.FindAsync(id);
     if (existing == null) return Results.NotFound();
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
     return Results.Ok(existing);
 });

 app.MapGet("/tenant/{companyId:int}/feedback", async (int companyId, ITenantDatabaseResolver resolver, IConfiguration config) =>
 {
     using var tenantDb = await GetTenantDb(companyId, resolver, config);
     return Results.Ok(await tenantDb.Feedback.AsNoTracking().ToListAsync());
 });

 app.MapPost("/tenant/{companyId:int}/feedback", async (int companyId, Feedback item, ITenantDatabaseResolver resolver, IConfiguration config) =>
 {
     using var tenantDb = await GetTenantDb(companyId, resolver, config);
     if (item.CreatedAt == default) item.CreatedAt = DateTime.UtcNow;
     tenantDb.Feedback.Add(item);
     await tenantDb.SaveChangesAsync();
     return Results.Created($"/tenant/{companyId}/feedback/{item.FeedbackId}", item);
 });

 app.MapPut("/tenant/{companyId:int}/feedback/{id:int}", async (int companyId, int id, Feedback updated, ITenantDatabaseResolver resolver, IConfiguration config) =>
 {
     using var tenantDb = await GetTenantDb(companyId, resolver, config);
     var existing = await tenantDb.Feedback.FindAsync(id);
     if (existing == null) return Results.NotFound();
     existing.CustomerName = updated.CustomerName;
     existing.PhoneNumber = updated.PhoneNumber;
     existing.Type = updated.Type;
     existing.Rating = updated.Rating;
     existing.Comment = updated.Comment;
     existing.HandledBy = updated.HandledBy;
     existing.Status = updated.Status;
     existing.ReviewedAt = updated.Status == "Reviewed" ? (updated.ReviewedAt ?? DateTime.UtcNow) : null;
     await tenantDb.SaveChangesAsync();
     return Results.Ok(existing);
 });

 app.MapGet("/tenant/{companyId:int}/complaints", async (int companyId, ITenantDatabaseResolver resolver, IConfiguration config) =>
 {
     using var tenantDb = await GetTenantDb(companyId, resolver, config);
     return Results.Ok(await tenantDb.Complaints.AsNoTracking().ToListAsync());
 });

 app.MapPost("/tenant/{companyId:int}/complaints", async (int companyId, Complaint item, ITenantDatabaseResolver resolver, IConfiguration config) =>
 {
     using var tenantDb = await GetTenantDb(companyId, resolver, config);
     if (item.CreatedAt == default) item.CreatedAt = DateTime.UtcNow;
     tenantDb.Complaints.Add(item);
     await tenantDb.SaveChangesAsync();
     return Results.Created($"/tenant/{companyId}/complaints/{item.ComplaintId}", item);
 });

 app.MapPut("/tenant/{companyId:int}/complaints/{id:int}", async (int companyId, int id, Complaint updated, ITenantDatabaseResolver resolver, IConfiguration config) =>
 {
     using var tenantDb = await GetTenantDb(companyId, resolver, config);
     var existing = await tenantDb.Complaints.FindAsync(id);
     if (existing == null) return Results.NotFound();
     existing.CustomerName = updated.CustomerName;
     existing.PhoneNumber = updated.PhoneNumber;
     existing.Category = updated.Category;
     existing.Description = updated.Description;
     existing.Priority = updated.Priority;
     existing.HandledBy = updated.HandledBy;
     existing.Status = updated.Status;
     existing.Resolution = updated.Resolution;
     existing.ResolvedAt = updated.Status == "Resolved" || updated.Status == "Closed"
         ? (updated.ResolvedAt ?? DateTime.UtcNow)
         : null;
     await tenantDb.SaveChangesAsync();
     return Results.Ok(existing);
 });

 app.MapGet("/tenant/{companyId:int}/interactions", async (int companyId, ITenantDatabaseResolver resolver, IConfiguration config) =>
 {
     using var tenantDb = await GetTenantDb(companyId, resolver, config);
     return Results.Ok(await tenantDb.InteractionLogs.AsNoTracking().ToListAsync());
 });

 app.MapPost("/tenant/{companyId:int}/interactions", async (int companyId, InteractionLog item, ITenantDatabaseResolver resolver, IConfiguration config) =>
 {
     using var tenantDb = await GetTenantDb(companyId, resolver, config);
     if (item.CreatedAt == default) item.CreatedAt = DateTime.UtcNow;
     tenantDb.InteractionLogs.Add(item);
     await tenantDb.SaveChangesAsync();
     return Results.Created($"/tenant/{companyId}/interactions/{item.InteractionId}", item);
 });

 app.MapPut("/tenant/{companyId:int}/interactions/{id:int}", async (int companyId, int id, InteractionLog updated, ITenantDatabaseResolver resolver, IConfiguration config) =>
 {
     using var tenantDb = await GetTenantDb(companyId, resolver, config);
     var existing = await tenantDb.InteractionLogs.FindAsync(id);
     if (existing == null) return Results.NotFound();
     existing.CustomerName = updated.CustomerName;
     existing.PhoneNumber = updated.PhoneNumber;
     existing.InteractionType = updated.InteractionType;
     existing.Subject = updated.Subject;
     existing.Notes = updated.Notes;
     existing.HandledBy = updated.HandledBy;
     await tenantDb.SaveChangesAsync();
     return Results.Ok(existing);
 });

 app.MapGet("/tenant/{companyId:int}/warranties", async (int companyId, ITenantDatabaseResolver resolver, IConfiguration config) =>
 {
     using var tenantDb = await GetTenantDb(companyId, resolver, config);
     return Results.Ok(await tenantDb.VehicleWarranties.AsNoTracking().ToListAsync());
 });

 app.MapPost("/tenant/{companyId:int}/warranties", async (int companyId, VehicleWarranty item, ITenantDatabaseResolver resolver, IConfiguration config) =>
 {
     using var tenantDb = await GetTenantDb(companyId, resolver, config);
     tenantDb.VehicleWarranties.Add(item);
     await tenantDb.SaveChangesAsync();
     return Results.Created($"/tenant/{companyId}/warranties/{item.WarrantyId}", item);
 });

 app.MapPut("/tenant/{companyId:int}/warranties/{id:int}", async (int companyId, int id, VehicleWarranty updated, ITenantDatabaseResolver resolver, IConfiguration config) =>
 {
     using var tenantDb = await GetTenantDb(companyId, resolver, config);
     var existing = await tenantDb.VehicleWarranties.FindAsync(id);
     if (existing == null) return Results.NotFound();
     existing.CustomerName = updated.CustomerName;
     existing.PhoneNumber = updated.PhoneNumber;
     existing.VehicleModel = updated.VehicleModel;
     existing.PurchaseDate = updated.PurchaseDate;
     existing.WarrantyStart = updated.WarrantyStart;
     existing.WarrantyEnd = updated.WarrantyEnd;
     existing.Coverage = updated.Coverage;
     existing.Status = updated.Status;
     await tenantDb.SaveChangesAsync();
     return Results.Ok(existing);
 });

 app.MapGet("/tenant/{companyId:int}/warranty-claims", async (int companyId, ITenantDatabaseResolver resolver, IConfiguration config) =>
 {
     using var tenantDb = await GetTenantDb(companyId, resolver, config);
     return Results.Ok(await tenantDb.WarrantyClaims.AsNoTracking().ToListAsync());
 });

 app.MapPost("/tenant/{companyId:int}/warranty-claims", async (int companyId, WarrantyClaim item, ITenantDatabaseResolver resolver, IConfiguration config) =>
 {
     using var tenantDb = await GetTenantDb(companyId, resolver, config);
     if (item.DateReported == default) item.DateReported = DateTime.UtcNow;
     tenantDb.WarrantyClaims.Add(item);
     await tenantDb.SaveChangesAsync();
     return Results.Created($"/tenant/{companyId}/warranty-claims/{item.ClaimId}", item);
 });

 app.MapPut("/tenant/{companyId:int}/warranty-claims/{id:int}", async (int companyId, int id, WarrantyClaim updated, ITenantDatabaseResolver resolver, IConfiguration config) =>
 {
     using var tenantDb = await GetTenantDb(companyId, resolver, config);
     var existing = await tenantDb.WarrantyClaims.FindAsync(id);
     if (existing == null) return Results.NotFound();
     existing.WarrantyId = updated.WarrantyId;
     existing.CustomerName = updated.CustomerName;
     existing.PhoneNumber = updated.PhoneNumber;
     existing.VehicleModel = updated.VehicleModel;
     existing.Problem = updated.Problem;
     existing.DateReported = updated.DateReported;
     existing.HandledBy = updated.HandledBy;
     existing.Status = updated.Status;
     existing.Resolution = updated.Resolution;
     existing.DateResolved = updated.Status == "Resolved" ? (updated.DateResolved ?? DateTime.UtcNow) : null;
     await tenantDb.SaveChangesAsync();
     return Results.Ok(existing);
 });

 app.MapGet("/tenant/{companyId:int}/maintenance", async (int companyId, ITenantDatabaseResolver resolver, IConfiguration config) =>
 {
     using var tenantDb = await GetTenantDb(companyId, resolver, config);
     return Results.Ok(await tenantDb.MaintenanceRecords.AsNoTracking().ToListAsync());
 });

 app.MapPost("/tenant/{companyId:int}/maintenance", async (int companyId, MaintenanceRecord item, ITenantDatabaseResolver resolver, IConfiguration config) =>
 {
     using var tenantDb = await GetTenantDb(companyId, resolver, config);
     tenantDb.MaintenanceRecords.Add(item);
     await tenantDb.SaveChangesAsync();
     return Results.Created($"/tenant/{companyId}/maintenance/{item.MaintenanceId}", item);
 });

 app.MapPut("/tenant/{companyId:int}/maintenance/{id:int}", async (int companyId, int id, MaintenanceRecord updated, ITenantDatabaseResolver resolver, IConfiguration config) =>
 {
     using var tenantDb = await GetTenantDb(companyId, resolver, config);
     var existing = await tenantDb.MaintenanceRecords.FindAsync(id);
     if (existing == null) return Results.NotFound();
     existing.CustomerName = updated.CustomerName;
     existing.PhoneNumber = updated.PhoneNumber;
     existing.VehicleModel = updated.VehicleModel;
     existing.ServiceDate = updated.ServiceDate;
     existing.ServiceType = updated.ServiceType;
     existing.PlanCoverage = updated.PlanCoverage;
     existing.AssignedStaff = updated.AssignedStaff;
     existing.Status = updated.Status;
     existing.Notes = updated.Notes;
     await tenantDb.SaveChangesAsync();
     return Results.Ok(existing);
 });
 
 app.Run();

// HELPER: Generates the connection dynamically without duplicating code everywhere
async Task<TenantDriveConnectDbContext> GetTenantDb(int companyId, ITenantDatabaseResolver resolver, IConfiguration config)
{
    var dbInfo = await resolver.GetDatabaseInfoAsync(companyId);
    string connString = dbInfo.ServerName.Contains("(localdb)", StringComparison.OrdinalIgnoreCase)
        ? $"Server={dbInfo.ServerName};Database={dbInfo.DatabaseName};Trusted_Connection=True;TrustServerCertificate=True;"
        : $"Server={dbInfo.ServerName};Database={dbInfo.DatabaseName};User Id={config[$"TenantCredentials:{dbInfo.CredentialKey}:UserId"]};Password={config[$"TenantCredentials:{dbInfo.CredentialKey}:Password"]};TrustServerCertificate=True;";

    var options = new DbContextOptionsBuilder<TenantDriveConnectDbContext>()
        .UseSqlServer(connString, sqlOptions => sqlOptions.EnableRetryOnFailure())
        .Options;
    return new TenantDriveConnectDbContext(options);
}