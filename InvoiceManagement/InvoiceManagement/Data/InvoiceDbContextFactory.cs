using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InvoiceManagement.Data;

public class InvoiceDbContextFactory : IDesignTimeDbContextFactory<InvoiceDbContext>
{
    public InvoiceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InvoiceDbContext>();

        // Use connection string from environment or default
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
            ?? "Host=localhost;Database=invoice_db;Username=devuser;Password=devpass123";

        optionsBuilder.UseNpgsql(connectionString);

        return new InvoiceDbContext(optionsBuilder.Options);
    }
}
