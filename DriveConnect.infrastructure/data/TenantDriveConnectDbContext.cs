using DriveConnect.domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace DriveConnect.infrastructure.Data;

public class TenantDriveConnectDbContext : DbContext
{
    public TenantDriveConnectDbContext(DbContextOptions<TenantDriveConnectDbContext> options) : base(options) { }

    // Register both new tables
    public DbSet<SalesLead> SalesLeads => Set<SalesLead>();
    public DbSet<RepairTicket> RepairTickets => Set<RepairTicket>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<Feedback> Feedback => Set<Feedback>();
    public DbSet<Complaint> Complaints => Set<Complaint>();
    public DbSet<InteractionLog> InteractionLogs => Set<InteractionLog>();
    public DbSet<VehicleWarranty> VehicleWarranties => Set<VehicleWarranty>();
    public DbSet<WarrantyClaim> WarrantyClaims => Set<WarrantyClaim>();
    public DbSet<MaintenanceRecord> MaintenanceRecords => Set<MaintenanceRecord>();



    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        

        // 1. Configuration for Car Sales (dbo.Inquiries)
        builder.Entity<SalesLead>(entity =>
        {
            entity.ToTable("SalesLeads"); // Keeps your requested table name
            entity.HasKey(x => x.InquiryId);

            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.MiddleName).HasMaxLength(100);
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.EmailAddress).HasMaxLength(150);
            entity.Property(x => x.CarModel).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(50).HasDefaultValue("New Inquiry");
            entity.Property(x => x.EstimatedCost).HasPrecision(18, 2);
            entity.Property(x => x.HandledBy).HasMaxLength(100);

            // Notice: Concern, PickupStatus, and Audit properties are completely removed from here!
        });

        // 2. Configuration for Service & Repair (dbo.RepairTickets)
        builder.Entity<RepairTicket>(entity =>
        {
            entity.ToTable("RepairTickets"); // Creates the brand new table
            entity.HasKey(x => x.TicketId);

            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.MiddleName).HasMaxLength(100);
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.EmailAddress).HasMaxLength(150);
            entity.Property(x => x.CarModel).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Concern).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(50).HasDefaultValue("Diagnose");
            entity.Property(x => x.PickupStatus).HasMaxLength(50).HasDefaultValue("Pending");
            entity.Property(x => x.EstimatedCost).HasPrecision(18, 2);
            entity.Property(x => x.HandledBy).HasMaxLength(100);

            
        });

        builder.Entity<Promotion>(entity =>
        {
            entity.ToTable("Promotions");
            entity.HasKey(x => x.PromotionId);
            entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500).IsRequired();
            entity.Property(x => x.DiscountType).HasMaxLength(30).IsRequired();
            entity.Property(x => x.DiscountValue).HasPrecision(18, 2);
            entity.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ApprovedBy).HasMaxLength(100);
            entity.Property(x => x.Status).HasMaxLength(30).HasDefaultValue("Draft");
        });

        builder.Entity<Feedback>(entity =>
        {
            entity.ToTable("Feedback");
            entity.HasKey(x => x.FeedbackId);
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.MiddleName).HasMaxLength(100);
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Type).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Comment).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.HandledBy).HasMaxLength(100);
            entity.Property(x => x.Status).HasMaxLength(30).HasDefaultValue("New");
        });

        builder.Entity<Complaint>(entity =>
        {
            entity.ToTable("Complaints");
            entity.HasKey(x => x.ComplaintId);
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.MiddleName).HasMaxLength(100);
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.Priority).HasMaxLength(30).IsRequired();
            entity.Property(x => x.HandledBy).HasMaxLength(100);
            entity.Property(x => x.Status).HasMaxLength(30).HasDefaultValue("New");
            entity.Property(x => x.Resolution).HasMaxLength(1000);
        });

        builder.Entity<InteractionLog>(entity =>
        {
            entity.ToTable("InteractionLogs");
            entity.HasKey(x => x.InteractionId);
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.MiddleName).HasMaxLength(100);
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.InteractionType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Subject).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.HandledBy).HasMaxLength(100);
        });

        builder.Entity<VehicleWarranty>(entity =>
        {
            entity.ToTable("VehicleWarranties");
            entity.HasKey(x => x.WarrantyId);
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.MiddleName).HasMaxLength(100);
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.VehicleModel).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Coverage).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(30).HasDefaultValue("Active");
        });

        builder.Entity<WarrantyClaim>(entity =>
        {
            entity.ToTable("WarrantyClaims");
            entity.HasKey(x => x.ClaimId);
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.MiddleName).HasMaxLength(100);
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.VehicleModel).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Problem).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.HandledBy).HasMaxLength(100);
            entity.Property(x => x.Status).HasMaxLength(30).HasDefaultValue("Pending");
            entity.Property(x => x.Resolution).HasMaxLength(1000);
        });

        builder.Entity<MaintenanceRecord>(entity =>
        {
            entity.ToTable("MaintenanceRecords");
            entity.HasKey(x => x.MaintenanceId);
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.MiddleName).HasMaxLength(100);
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.VehicleModel).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ServiceType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PlanCoverage).HasMaxLength(150).IsRequired();
            entity.Property(x => x.AssignedStaff).HasMaxLength(100);
            entity.Property(x => x.Status).HasMaxLength(30).HasDefaultValue("Scheduled");
            entity.Property(x => x.Notes).HasMaxLength(1000);
        });
    }
}