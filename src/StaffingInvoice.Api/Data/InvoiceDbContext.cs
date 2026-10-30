using Microsoft.EntityFrameworkCore;
using StaffingInvoice.Api.Models;

namespace StaffingInvoice.Api.Data;

public class InvoiceDbContext(DbContextOptions<InvoiceDbContext> options) : DbContext(options)
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<Timesheet> Timesheets => Set<Timesheet>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>().HasIndex(x => x.Name);
        modelBuilder.Entity<Client>().Property(x => x.Name).HasMaxLength(200).IsRequired();
        modelBuilder.Entity<Client>().Property(x => x.BillingEmail).HasMaxLength(320);
        modelBuilder.Entity<Resource>().Property(x => x.FullName).HasMaxLength(200).IsRequired();
        modelBuilder.Entity<Resource>().HasOne(x => x.Client).WithMany(x => x.Resources).HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Contract>().Property(x => x.HourlyRate).HasPrecision(18, 2);
        modelBuilder.Entity<Timesheet>().Property(x => x.RegularHours).HasPrecision(9, 2);
        modelBuilder.Entity<Timesheet>().Property(x => x.OvertimeHours).HasPrecision(9, 2);
        modelBuilder.Entity<Invoice>().HasIndex(x => x.InvoiceNumber).IsUnique();
        modelBuilder.Entity<Invoice>().Property(x => x.Subtotal).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>().Property(x => x.TaxRate).HasPrecision(6, 3);
        modelBuilder.Entity<Invoice>().Property(x => x.TaxAmount).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>().Property(x => x.TotalAmount).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>().Property(x => x.AmountPaid).HasPrecision(18, 2);
        modelBuilder.Entity<InvoiceLine>().Property(x => x.Hours).HasPrecision(9, 2);
        modelBuilder.Entity<InvoiceLine>().Property(x => x.HourlyRate).HasPrecision(18, 2);
        modelBuilder.Entity<InvoiceLine>().Property(x => x.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<InvoiceLine>().HasOne(x => x.Invoice).WithMany(x => x.Lines).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Payment>().Property(x => x.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<Timesheet>().HasOne(x => x.Invoice).WithMany().HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        base.OnModelCreating(modelBuilder);
    }
}
