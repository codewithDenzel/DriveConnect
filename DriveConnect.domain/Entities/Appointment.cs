namespace DriveConnect.domain.Entities;

public class Appointment
{
    public int AppointmentId { get; set; }
    public int CustomerId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string ServiceType { get; set; } = string.Empty; // Test Drive, Consultation, Maintenance
    public string Notes { get; set; } = string.Empty;

    // Audit Trailing Properties
    public string WhoRequested { get; set; } = string.Empty;
    public DateTime WhenRequested { get; set; } = DateTime.UtcNow;
    public string WhoValidated { get; set; } = string.Empty;
    public DateTime? WhenValidated { get; set; }
    public string ValidationStatus { get; set; } = "Pending";
    public string ValidationNotes { get; set; } = string.Empty;

    public Customer? Customer { get; set; }
}