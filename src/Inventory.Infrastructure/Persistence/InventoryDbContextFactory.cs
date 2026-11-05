using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace Inventory.Infrastructure.Persistence;
public sealed class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Environment variable 'ConnectionStrings__DefaultConnection' is required to run EF Core commands.");
        }
        var options = new DbContextOptionsBuilder<InventoryDbContext>().UseSqlServer(connectionString).Options;
        return new InventoryDbContext(options);
    }
}
