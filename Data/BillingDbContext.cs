using Microsoft.EntityFrameworkCore;
using PatientBillingAPI.Models;

namespace PatientBillingAPI.Data;

public class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options){ }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>()
            .HasMany(p => p.Invoices)
            .WithOne(i => i.Patient)
            .HasForeignKey(i => i.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.Amount)
            .HasPrecision(10,2);
    }
}