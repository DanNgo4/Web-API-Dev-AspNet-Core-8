using Microsoft.EntityFrameworkCore;
using BasicEfCoreDemo.Models;

namespace BasicEfCoreDemo.Data
{
    // This class represents the database
    public class InvoiceDbContext(DbContextOptions<InvoiceDbContext> options) : DbContext(options)
    {
        // This DbSet represents the "Invoices" table in the database
        public DbSet<Invoice> Invoices => Set<Invoice>();
    }
}
