using DriveConnect.api.Services;
using DriveConnect.infrastructure.Data;
using DriveConnect.infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var masterConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(masterConnectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is not configured.");
}

builder.Services.AddDbContext<MasterDriveConnectDbContext>(options =>
    options.UseSqlServer(
        masterConnectionString,
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.AddScoped<ITenantDatabaseResolver, TenantDatabaseResolver>();
builder.Services.AddScoped<ITenantDbContextFactory, TenantDbContextFactory>();
builder.Services.AddScoped<ISyncService, SyncService>();
builder.Services.AddScoped<SyncApplier>();

builder.Services.AddHttpClient("DriveConnectCloudSync");

builder.Services.AddHostedService<SyncWorker>();
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "DriveConnect API"
}));

app.MapControllers();

app.Run();
