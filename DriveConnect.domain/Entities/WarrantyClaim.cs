using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DriveConnect.domain.Entities
{
    [Table("WarrantyClaims")]
    public class WarrantyClaim
    {
        [Key]
        public int ClaimId { get; set; }
        public int WarrantyId { get; set; }
        public string? CustomerName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? VehicleModel { get; set; }
        public string? Problem { get; set; }
        public DateTime DateReported { get; set; }
        public string? HandledBy { get; set; }
        public string? Status { get; set; }
        public string? Resolution { get; set; }
        public DateTime? DateResolved { get; set; }
    }
}