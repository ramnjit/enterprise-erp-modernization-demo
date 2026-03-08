using Microsoft.EntityFrameworkCore;
using LegacyErpMonolith.Models;

namespace LegacyErpMonolith.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // These DbSets represent actual SQL tables
    public DbSet<Product> Products { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<Order> Orders { get; set; }
}