using System.Security.Claims;
using DriveConnect.domain.Entities;
using DriveConnect.infrastructure.Data;
using DriveConnect.infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

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

    [HttpPost("companies")]
    public async Task<IActionResult> CreateTenantCompany(
        [FromBody] TenantCompanyRegistrationRequest request)
    {
        var companyCode = request.CompanyCode?.Trim().ToUpperInvariant();
        var companyName = request.CompanyName?.Trim();
        var serverName = request.ServerName?.Trim();
        var databaseName = request.DatabaseName?.Trim();
        var adminUsername = request.AdminUsername?.Trim();
        var adminFirstName = request.AdminFirstName?.Trim();
        var adminMiddleName = string.IsNullOrWhiteSpace(request.AdminMiddleName)
            ? null
            : request.AdminMiddleName.Trim();
        var adminLastName = request.AdminLastName?.Trim();
        var adminEmail = request.AdminEmail?.Trim();

        if (string.IsNullOrWhiteSpace(companyCode) ||
            string.IsNullOrWhiteSpace(companyName) ||
            string.IsNullOrWhiteSpace(serverName) ||
            string.IsNullOrWhiteSpace(databaseName) ||
            string.IsNullOrWhiteSpace(adminUsername) ||
            string.IsNullOrWhiteSpace(adminFirstName) ||
            string.IsNullOrWhiteSpace(adminLastName) ||
            string.IsNullOrWhiteSpace(adminEmail) ||
            string.IsNullOrWhiteSpace(request.AdminPassword))
        {
            return BadRequest("Company, database, and initial Admin account fields are required.");
        }

        if (!TenantPlanCatalog.IsSupportedSelection(request.PlanName))
            return BadRequest("Plan name must be Basic, Pro, or Pro Max.");

        if (!string.Equals(request.BillingCycle, "Monthly", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(request.BillingCycle, "Annual", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Billing cycle must be Monthly or Annual.");
        }

        if (request.BillingAmount <= 0)
            return BadRequest("Billing amount must be greater than zero.");

        if (request.AdminPassword.Length < 8)
            return BadRequest("The initial Admin password must contain at least 8 characters.");

        if (!databaseName.All(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '-'))
            return BadRequest("Database name can contain letters, numbers, underscores, and hyphens only.");

        if (await _masterDb.Companies.AnyAsync(x => x.CompanyCode == companyCode))
            return Conflict("Company code is already in use.");

        if (await _masterDb.AppUsers.AnyAsync(x =>
            x.Username == adminUsername || x.Email == adminEmail))
        {
            return Conflict("The initial Admin username or email is already in use.");
        }

        var databaseAlreadyAssigned = await _masterDb.CompanyDatabases
            .AsNoTracking()
            .AnyAsync(x => x.IsActive &&
                           x.ServerName == serverName &&
                           x.DatabaseName == databaseName);

        if (databaseAlreadyAssigned)
            return Conflict("That database is already assigned to another company. Each tenant needs its own database.");

        var startDate = request.StartDate == default
            ? DateTime.UtcNow.Date
            : request.StartDate.Date;
        var billingCycle = string.Equals(
            request.BillingCycle,
            "Annual",
            StringComparison.OrdinalIgnoreCase)
            ? "Annual"
            : "Monthly";
        var endDate = billingCycle == "Annual"
            ? startDate.AddYears(1).AddDays(-1)
            : startDate.AddMonths(1).AddDays(-1);
        var amount = decimal.Round(request.BillingAmount, 2, MidpointRounding.AwayFromZero);
        var monthlyFee = billingCycle == "Annual"
            ? decimal.Round(amount / 12m, 2, MidpointRounding.AwayFromZero)
            : amount;

        // SQL Server retry-on-failure requires the whole user transaction
        // to run inside the configured EF Core execution strategy.
        var strategy = _masterDb.Database.CreateExecutionStrategy();

        var created = await strategy.ExecuteAsync(async () =>
        {
            // If the strategy retries after a transient failure, discard entities
            // tracked during the previous attempt before beginning a new transaction.
            _masterDb.ChangeTracker.Clear();

            await using var transaction =
                await _masterDb.Database.BeginTransactionAsync();

            var company = new Company
            {
                CompanyCode = companyCode!,
                CompanyName = companyName!,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _masterDb.Companies.Add(company);
            await _masterDb.SaveChangesAsync();

            var mainBranch = new Branch
            {
                CompanyId = company.CompanyId,
                BranchCode = "MAIN",
                BranchName = "Main Branch",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _masterDb.Branches.Add(mainBranch);
            await _masterDb.SaveChangesAsync();

            var admin = new AppUser
            {
                CompanyId = company.CompanyId,
                Username = adminUsername!,
                FirstName = adminFirstName!,
                MiddleName = adminMiddleName,
                LastName = adminLastName!,
                Email = adminEmail!,
                Role = "Admin",
                BranchId = mainBranch.BranchId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            admin.PasswordHash = new PasswordHasher<AppUser>()
                .HashPassword(admin, request.AdminPassword);

            _masterDb.AppUsers.Add(admin);

            _masterDb.CompanyDatabases.Add(new CompanyDatabase
            {
                CompanyId = company.CompanyId,
                ServerName = serverName!,
                DatabaseName = databaseName!,
                IsActive = true
            });

            _masterDb.Subscriptions.Add(new Subscription
            {
                CompanyId = company.CompanyId,
                PlanName = TenantPlanCatalog.Normalize(request.PlanName),
                BillingCycle = billingCycle,
                BillingAmount = amount,
                MonthlyFee = monthlyFee,
                StartDate = startDate.ToUniversalTime(),
                EndDate = endDate.ToUniversalTime(),
                IsActive = true
            });

            await _masterDb.SaveChangesAsync();
            await transaction.CommitAsync();

            return (Company: company, MainBranch: mainBranch, Admin: admin);
        });

        return Ok(new
        {
            created.Company.CompanyId,
            created.Company.CompanyCode,
            created.Company.CompanyName,
            DatabaseName = databaseName,
            PlanName = TenantPlanCatalog.Normalize(request.PlanName),
            InitialAdminUsername = created.Admin.Username,
            DefaultBranchName = created.MainBranch.BranchName
        });
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

public sealed record TenantCompanyRegistrationRequest(
    string CompanyCode,
    string CompanyName,
    string ServerName,
    string DatabaseName,
    string PlanName,
    string BillingCycle,
    decimal BillingAmount,
    DateTime StartDate,
    string AdminUsername,
    string AdminFirstName,
    string? AdminMiddleName,
    string AdminLastName,
    string AdminEmail,
    string AdminPassword);

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
