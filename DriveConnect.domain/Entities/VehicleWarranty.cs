using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DriveConnect.domain.Entities
{
    [Table("VehicleWarranties")]
    public class VehicleWarranty
    {
        [Key]
        public int WarrantyId { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? VehicleModel { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime WarrantyStart { get; set; }
        public DateTime WarrantyEnd { get; set; }
        public string? Coverage { get; set; }
        public string? Status { get; set; }
    }
}