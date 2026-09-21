using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DriveConnect.domain.Entities
{
    [Table("Feedback")]
    public class Feedback
    {
        [Key]
        public int FeedbackId { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Type { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string? HandledBy { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}