using DriveConnect.domain.DTO;
using DriveConnect.infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DriveConnect.infrastructure.Services;

public static class TenantPlanCatalog
{
    public const string Basic = "Basic";
    public const string Pro = "Pro";
    public const string ProMax = "Pro Max";

    public static bool IsSupportedSelection(string? planName)
    {
        var value = (planName ?? string.Empty).Trim();
        return value.Equals(Basic, StringComparison.OrdinalIgnoreCase)
            || value.Equals(Pro, StringComparison.OrdinalIgnoreCase)
            || value.Equals(ProMax, StringComparison.OrdinalIgnoreCase);
    }

    public static string Normalize(string? planName)
    {
        var value = (planName ?? string.Empty).Trim();

        if (value.Equals(Basic, StringComparison.OrdinalIgnoreCase)
            || value.Equals("Starter", StringComparison.OrdinalIgnoreCase))
            return Basic;

        if (value.Equals(Pro, StringComparison.OrdinalIgnoreCase)
            || value.Equals("Professional", StringComparison.OrdinalIgnoreCase))
            return Pro;

        if (value.Equals(ProMax, StringComparison.OrdinalIgnoreCase)
            || value.Equals("Enterprise", StringComparison.OrdinalIgnoreCase))
            return ProMax;

        return Basic;
    }
}

public interface ITenantPlanService
{
    Task<TenantPlanFeatures> GetFeaturesAsync(int companyId);
}

public sealed class TenantPlanService : ITenantPlanService
{
    private readonly MasterDriveConnectDbContext _masterDb;

    public TenantPlanService(MasterDriveConnectDbContext masterDb)
    {
        _masterDb = masterDb;
    }

    public async Task<TenantPlanFeatures> GetFeaturesAsync(int companyId)
    {
        var subscription = await _masterDb.Subscriptions
            .AsNoTracking()
            .Where(x => x.CompanyId == companyId && x.IsActive)
            .OrderByDescending(x => x.EndDate)
            .ThenByDescending(x => x.StartDate)
            .FirstOrDefaultAsync();

        // If a company has no current subscription, give it only the Basic feature set.
        // Existing sales/service/customer data remains available.
        var subscriptionIsActive = subscription != null
            && subscription.EndDate.Date >= DateTime.UtcNow.Date;

        var plan = subscriptionIsActive
            ? TenantPlanCatalog.Normalize(subscription!.PlanName)
            : TenantPlanCatalog.Basic;

        var isPro = plan == TenantPlanCatalog.Pro;
        var isProMax = plan == TenantPlanCatalog.ProMax;

        return new TenantPlanFeatures
        {
            PlanName = plan,
            SubscriptionActive = subscriptionIsActive,
            CanUseStaticDashboard = true,
            CanGenerateReports = true,
            CanUsePromotions = subscriptionIsActive && (isPro || isProMax),
            CanUseBranching = subscriptionIsActive && isProMax,
            CanUseBusinessIntelligence = subscriptionIsActive && isProMax
        };
    }
}
