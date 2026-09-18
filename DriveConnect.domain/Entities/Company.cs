namespace DriveConnect.domain.Entities;

public class Company
{
    public int CompanyId { get; set; }
    public string CompanyCode { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<Device> Devices { get; set; } = new List<Device>(); 
}