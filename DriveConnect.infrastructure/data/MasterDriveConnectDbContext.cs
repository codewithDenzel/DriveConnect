using Microsoft.EntityFrameworkCore;
using DriveConnect.domain.Entities;

namespace DriveConnect.infrastructure.Data;

public class MasterDriveConnectDbContext : DbContext
{
    public MasterDriveConnectDbContext(DbContextOptions<MasterDriveConnectDbContext> options) : base(options) { }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CompanyDatabase> CompanyDatabases => Set<CompanyDatabase>();
    public DbSet<Device> Devices => Set<Device>();

    // System Administration Entities
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Company>(entity =>
        {
            entity.HasKey(x => x.CompanyId);
            entity.Property(x => x.CompanyCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.CompanyName).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => x.CompanyCode).IsUnique();
        });

        builder.Entity<CompanyDatabase>(entity =>
        {
            entity.HasKey(x => x.CompanyDatabaseId);
            entity.Property(x => x.ServerName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.DatabaseName).HasMaxLength(200).IsRequired();
            entity.HasOne(x => x.Company)
                .WithMany()
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Device>(entity =>
        {
            entity.HasKey(x => x.DeviceId);
            entity.Property(x => x.DeviceCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.DeviceName).HasMaxLength(200).IsRequired();
            entity.HasOne(x => x.Company)
                .WithMany(x => x.Devices)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.CompanyId, x.DeviceCode }).IsUnique();
        });

        builder.Entity<AppUser>(entity =>
        {
            entity.HasKey(x => x.UserId);
            entity.Property(x => x.Username).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Role).HasMaxLength(50).IsRequired();
            entity.HasOne(x => x.Company)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.Email).IsUnique();
        });

        builder.Entity<Subscription>(entity =>
        {
            entity.HasKey(x => x.SubscriptionId);
            entity.Property(x => x.PlanName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.MonthlyFee).HasPrecision(18, 2);
            entity.HasOne(x => x.Company)
                .WithMany(x => x.Subscriptions)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}