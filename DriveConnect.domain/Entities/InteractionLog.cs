using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DriveConnect.domain.Entities
{
    [Table("InteractionLogs")]
    public class InteractionLog
    {
        [Key]
        public int InteractionId { get; set; }
        public string? CustomerName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? InteractionType { get; set; }
        public string? Subject { get; set; }
        public string? Notes { get; set; }
        public string? HandledBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}