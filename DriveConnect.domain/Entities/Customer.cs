namespace DriveConnect.domain.Entities;

public class Customer
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    // Audit Trailing Properties
    public string WhoRequested { get; set; } = string.Empty;
    public DateTime WhenRequested { get; set; } = DateTime.UtcNow;
    public string WhoValidated { get; set; } = string.Empty;
    public DateTime? WhenValidated { get; set; }
    public string ValidationStatus { get; set; } = "Pending"; // Pending, Approved, Rejected
    public string ValidationNotes { get; set; } = string.Empty;

    public ICollection<SalesLead> SalesLeads { get; set; } = new List<SalesLead>();
    public ICollection<RepairTicket> RepairTickets { get; set; } = new List<RepairTicket>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<PaymentRecord> PaymentRecords { get; set; } = new List<PaymentRecord>();
}