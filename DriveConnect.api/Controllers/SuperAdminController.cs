using System.Security.Claims;
using DriveConnect.domain.Entities;
using DriveConnect.infrastructure.Data;
using DriveConnect.infrastructure.Services;
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
                    .Select(s => (decimal?)s.BillingAmount)
                    .FirstOrDefault(),
                x.Subscriptions
                    .Where(s => s.IsActive)
                    .OrderByDescending(s => s.EndDate)
                    .Select(s => s.BillingCycle)
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
                x.BillingCycle,
                x.BillingAmount,
                x.MonthlyFee,
                x.StartDate,
                x.EndDate,
                x.IsActive))
            .FirstOrDefaultAsync();

        if (subscription == null)
            return NotFound("No subscription record was found for this company.");

        return Ok(subscription);
    }

    [HttpPut("companies/{companyId:int}/subscription")]
    public async Task<ActionResult<SubscriptionOverview>> SaveSubscription(
        int companyId,
        [FromBody] SubscriptionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PlanName))
            return BadRequest("Plan name is required.");

        if (!TenantPlanCatalog.IsSupportedSelection(request.PlanName))
            return BadRequest("Plan name must be Basic, Pro, or Pro Max.");

        if (!string.Equals(request.BillingCycle, "Monthly", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(request.BillingCycle, "Annual", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Billing cycle must be Monthly or Annual.");
        }

        if (request.BillingAmount <= 0)
            return BadRequest("Billing amount must be greater than zero.");

        var companyExists = await _masterDb.Companies
            .AnyAsync(x => x.CompanyId == companyId);

        if (!companyExists)
            return NotFound("Company was not found.");

        var startDate = request.StartDate.Date;
        var billingCycle = string.Equals(request.BillingCycle, "Annual", StringComparison.OrdinalIgnoreCase)
            ? "Annual"
            : "Monthly";

        var endDate = billingCycle == "Annual"
            ? startDate.AddYears(1).AddDays(-1)
            : startDate.AddMonths(1).AddDays(-1);

        var monthlyEquivalent = billingCycle == "Annual"
            ? decimal.Round(request.BillingAmount / 12m, 2, MidpointRounding.AwayFromZero)
            : request.BillingAmount;

        var subscription = await _masterDb.Subscriptions
            .FirstOrDefaultAsync(x => x.CompanyId == companyId);

        if (subscription == null)
        {
            subscription = new Subscription
            {
                CompanyId = companyId
            };

            _masterDb.Subscriptions.Add(subscription);
        }

        subscription.PlanName = TenantPlanCatalog.Normalize(request.PlanName);
        subscription.BillingCycle = billingCycle;
        subscription.BillingAmount = decimal.Round(request.BillingAmount, 2, MidpointRounding.AwayFromZero);
        subscription.MonthlyFee = monthlyEquivalent;
        subscription.StartDate = startDate.ToUniversalTime();
        subscription.EndDate = endDate.ToUniversalTime();
        subscription.IsActive = request.IsActive;

        await _masterDb.SaveChangesAsync();

        return Ok(ToSubscriptionOverview(subscription));
    }

    [HttpPost("companies/{companyId:int}/subscription/deactivate")]
    public async Task<ActionResult> DeactivateSubscription(int companyId)
    {
        var subscription = await _masterDb.Subscriptions
            .FirstOrDefaultAsync(x => x.CompanyId == companyId);

        if (subscription == null)
            return NotFound("No subscription record was found for this company.");

        subscription.IsActive = false;
        await _masterDb.SaveChangesAsync();

        return NoContent();
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

    private static SubscriptionOverview ToSubscriptionOverview(Subscription subscription)
    {
        return new SubscriptionOverview(
            subscription.SubscriptionId,
            subscription.CompanyId,
            subscription.PlanName,
            subscription.BillingCycle,
            subscription.BillingAmount,
            subscription.MonthlyFee,
            subscription.StartDate,
            subscription.EndDate,
            subscription.IsActive);
    }
}

public sealed record CompanyOverview(
    int CompanyId,
    string CompanyCode,
    string CompanyName,
    bool IsActive,
    DateTime CreatedAt,
    string? ActivePlanName,
    decimal? ActiveBillingAmount,
    string? ActiveBillingCycle,
    DateTime? SubscriptionEndDate);

public sealed record SubscriptionOverview(
    int SubscriptionId,
    int CompanyId,
    string PlanName,
    string BillingCycle,
    decimal BillingAmount,
    decimal MonthlyFee,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive);

public sealed record SubscriptionRequest(
    string PlanName,
    string BillingCycle,
    decimal BillingAmount,
    DateTime StartDate,
    bool IsActive);

public sealed record SystemStatusOverview(
    string Status,
    string Service,
    string Role,
    string UserId);
