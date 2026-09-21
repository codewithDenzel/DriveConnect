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



    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        

        // 1. Configuration for Car Sales (dbo.Inquiries)
        builder.Entity<SalesLead>(entity =>
        {
            entity.ToTable("Inquiries"); // Keeps your requested table name
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
    }
}