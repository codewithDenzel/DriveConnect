using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DriveConnect.domain.Entities
{
    [Table("Promotions")]
    public class Promotion
    {
        [Key]
        public int PromotionId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? ApprovedBy { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}