using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DriveConnect.domain.Entities
{
    [Table("MaintenanceRecords")]
    public class MaintenanceRecord
    {
        [Key]
        public int MaintenanceId { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? VehicleModel { get; set; }
        public DateTime ServiceDate { get; set; }
        public string? ServiceType { get; set; }
        public string? PlanCoverage { get; set; }
        public string? AssignedStaff { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }
}