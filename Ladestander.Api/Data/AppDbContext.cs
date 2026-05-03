using Ladestander.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ladestander.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<BillingPeriod> BillingPeriods => Set<BillingPeriod>();
    public DbSet<ChargingSession> ChargingSessions => Set<ChargingSession>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customer");
            entity.HasKey(e => e.CustomerId);

            entity.Property(e => e.RfidNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.MiddleName).HasMaxLength(255);
            entity.Property(e => e.LastName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255);
        });

        modelBuilder.Entity<BillingPeriod>(entity =>
        {
            entity.ToTable("BillingPeriod");
            entity.HasKey(e => e.BillingPeriodId);

            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.MonthName).HasMaxLength(50).IsRequired();

            entity.Property(e => e.TransportEnergyNorth).HasPrecision(10, 5);
            entity.Property(e => e.TransportNetwork).HasPrecision(10, 5);
            entity.Property(e => e.TransportEnergySouth).HasPrecision(10, 5);
            entity.Property(e => e.ElectricityTax).HasPrecision(10, 5);
            entity.Property(e => e.VariablePrice).HasPrecision(10, 5);
            entity.Property(e => e.GreenElectricity).HasPrecision(10, 5);
            entity.Property(e => e.ClimateContribution).HasPrecision(10, 5);

            entity.Property(e => e.TsoSubscription).HasPrecision(10, 2);
            entity.Property(e => e.NetworkSubscription).HasPrecision(10, 2);
            entity.Property(e => e.SupplySubscription).HasPrecision(10, 2);
            entity.Property(e => e.NetsFee).HasPrecision(10, 2);

            entity.Property(e => e.MonthlyConsumption).HasPrecision(10, 3);
            entity.Property(e => e.AveragePriceKWh).HasPrecision(10, 4);
            entity.Property(e => e.AverageSolarPriceKWh).HasPrecision(10, 4);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("Invoice");
            entity.HasKey(e => e.InvoiceId);

            entity.Property(e => e.InvoiceNumber).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TotalEnergyKWh).HasPrecision(18, 3);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);

            entity.HasOne(e => e.Customer)
                .WithMany(e => e.Invoices)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.BillingPeriod)
                .WithMany(e => e.Invoices)
                .HasForeignKey(e => e.BillingPeriodId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChargingSession>(entity =>
        {
            entity.ToTable("ChargingSession");
            entity.HasKey(e => e.ChargingSessionId);

            entity.Property(e => e.ChargerAlias).HasMaxLength(255);
            entity.Property(e => e.SourceUserName).HasMaxLength(255);
            entity.Property(e => e.EnergyKWh).HasPrecision(10, 3);

            entity.HasOne(e => e.Customer)
                .WithMany(e => e.ChargingSessions)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.BillingPeriod)
                .WithMany(e => e.ChargingSessions)
                .HasForeignKey(e => e.BillingPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Invoice)
                .WithMany(e => e.ChargingSessions)
                .HasForeignKey(e => e.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.ToTable("AdminUser");
            entity.HasKey(e => e.AdminUserId);

            entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(50).IsRequired();
        });
    }
}