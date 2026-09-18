using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DriveConnect.domain.Entities
{
    [Table("Inquiries")]
    public class SalesLead
    {
        [Key]
        public int InquiryId { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? EmailAddress { get; set; }
        public string? CarModel { get; set; }
        public string? Status { get; set; }
        public decimal EstimatedCost { get; set; }
        public string? HandledBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}