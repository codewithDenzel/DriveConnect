namespace DriveConnect.domain.DTO;

public sealed class TenantPlanFeatures
{
    public string PlanName { get; set; } = "Basic";
    public bool SubscriptionActive { get; set; }
    public bool CanUseStaticDashboard { get; set; } = true;
    public bool CanGenerateReports { get; set; } = true;
    public bool CanUsePromotions { get; set; }
    public bool CanUseBranching { get; set; }
    public bool CanUseBusinessIntelligence { get; set; }
}
