namespace DriveConnect.domain.Entities;

public class Lead
{
    public int LeadId { get; set; }
    public string LeadName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string Status { get; set; } = "New";
    public decimal EstimatedValue { get; set; }

    // Audit Trailing Properties
    public string WhoRequested { get; set; } = string.Empty;
    public DateTime WhenRequested { get; set; } = DateTime.UtcNow;
    public string WhoValidated { get; set; } = string.Empty;
    public DateTime? WhenValidated { get; set; }
    public string ValidationStatus { get; set; } = "Pending";
    public string ValidationNotes { get; set; } = string.Empty;
}