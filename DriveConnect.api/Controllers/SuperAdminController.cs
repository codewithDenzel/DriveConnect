using System.Security.Claims;
using DriveConnect.domain.Entities;
using DriveConnect.infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DriveConnect.api.Controllers;

[ApiController]
[Authorize(Roles = "Super Admin")]
[Route("super-admin")]
public sealed class SuperAdminController : ControllerBase
{
    private readonly MasterDriveConnectDbContext _masterDb;

    public SuperAdminController(MasterDriveConnectDbContext masterDb)
    {
        _masterDb = masterDb;
    }

    [HttpGet("companies")]
    public async Task<ActionResult<List<CompanyOverview>>> GetCompanies()
    {
        var companies = await _masterDb.Companies
            .AsNoTracking()
            .OrderBy(x => x.CompanyName)
            .Select(x => new CompanyOverview(
                x.CompanyId,
                x.CompanyCode,
                x.CompanyName,
                x.IsActive,
                x.CreatedAt,
                x.Subscriptions
                    .Where(s => s.IsActive)
                    .OrderByDescending(s => s.EndDate)
                    .Select(s => s.PlanName)
                    .FirstOrDefault(),
                x.Subscriptions
                    .Where(s => s.IsActive)
                    .OrderByDescending(s => s.EndDate)
                    .Select(s => (decimal?)s.MonthlyFee)
                    .FirstOrDefault(),
                x.Subscriptions
                    .Where(s => s.IsActive)
                    .OrderByDescending(s => s.EndDate)
                    .Select(s => (DateTime?)s.EndDate)
                    .FirstOrDefault()))
            .ToListAsync();

        return Ok(companies);
    }

    [HttpGet("companies/{companyId:int}/subscription")]
    public async Task<ActionResult<SubscriptionOverview>> GetSubscription(int companyId)
    {
        var subscription = await _masterDb.Subscriptions
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.EndDate)
            .Select(x => new SubscriptionOverview(
                x.SubscriptionId,
                x.CompanyId,
                x.PlanName,
                x.MonthlyFee,
                x.StartDate,
                x.EndDate,
                x.IsActive))
            .FirstOrDefaultAsync();

        if (subscription == null)
            return NotFound("No subscription record was found for this company.");

        return Ok(subscription);
    }

    [HttpGet("system-status")]
    public ActionResult<SystemStatusOverview> GetSystemStatus()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        return Ok(new SystemStatusOverview(
            "Online",
            "DriveConnect API",
            role ?? string.Empty,
            userId ?? string.Empty));
    }
}

public sealed record CompanyOverview(
    int CompanyId,
    string CompanyCode,
    string CompanyName,
    bool IsActive,
    DateTime CreatedAt,
    string? ActivePlanName,
    decimal? ActiveMonthlyFee,
    DateTime? SubscriptionEndDate);

public sealed record SubscriptionOverview(
    int SubscriptionId,
    int CompanyId,
    string PlanName,
    decimal MonthlyFee,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive);

public sealed record SystemStatusOverview(
    string Status,
    string Service,
    string Role,
    string UserId);
