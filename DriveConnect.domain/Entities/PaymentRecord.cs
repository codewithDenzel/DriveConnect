namespace DriveConnect.domain.Entities;

public class PaymentRecord
{
    public int PaymentRecordId { get; set; }
    public int CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    // Audit Trailing Properties
    public string WhoRequested { get; set; } = string.Empty;
    public DateTime WhenRequested { get; set; } = DateTime.UtcNow;
    public string WhoValidated { get; set; } = string.Empty;
    public DateTime? WhenValidated { get; set; }
    public string ValidationStatus { get; set; } = "Pending";
    public string ValidationNotes { get; set; } = string.Empty;

    public Customer? Customer { get; set; }
}