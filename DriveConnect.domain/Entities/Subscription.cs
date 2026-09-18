using DriveConnect.domain.Entities;

namespace DriveConnect.domain.Entities;

public class Subscription
{
    public int SubscriptionId { get; set; }
    public int CompanyId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public decimal MonthlyFee { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    public Company? Company { get; set; }
}