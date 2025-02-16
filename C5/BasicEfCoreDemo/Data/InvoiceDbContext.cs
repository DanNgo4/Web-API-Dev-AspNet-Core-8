using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using BasicEfCoreDemo.Models;
using BasicEfCoreDemo.Enums;

namespace BasicEfCoreDemo.Data
{
    // This class represents the database
    public class InvoiceDbContext(DbContextOptions<InvoiceDbContext> options) : DbContext(options)
    {
        // This DbSet represents the "Invoices" table in the database
        public DbSet<Invoice> Invoices => Set<Invoice>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Invoice>().HasData(
                new Invoice
                {
                    Id            = Guid.NewGuid(),
                    InvoiceNumber = "INV-001",
                    ContactName   = "Iron Man",
                    Description   = "Invoice for the first month",
                    Amount        = 100,
                    InvoiceDate   = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    DueDate       = new DateTimeOffset(2023, 1, 15, 0, 0, 0, TimeSpan.Zero),
                    Status        = InvoiceStatus.AwaitPayment
                },
                new Invoice
                {
                    Id            = Guid.NewGuid(),
                    InvoiceNumber = "INV-002",
                    ContactName   = "Captain America",
                    Description   = "Invoice for the first month",
                    Amount        = 200,
                    InvoiceDate   = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    DueDate       = new DateTimeOffset(2021, 1, 15, 0, 0, 0, TimeSpan.Zero),
                    Status        = InvoiceStatus.AwaitPayment
                },
                new Invoice
                {
                    Id            = Guid.NewGuid(),
                    InvoiceNumber = "INV-003",
                    ContactName   = "Thor",
                    Description   = "Invoice for the first month",
                    Amount        = 300,
                    InvoiceDate   = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    DueDate       = new DateTimeOffset(2021, 1, 15, 0, 0, 0, 0, TimeSpan.Zero),
                    Status        = InvoiceStatus.Draft
                }
            );

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(InvoiceDbContext).Assembly);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
            //optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
    }
}
