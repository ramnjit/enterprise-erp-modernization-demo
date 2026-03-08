namespace LegacyErpMonolith.Models;

public class Product
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    // Navigation properties to link our tables
    public List<Inventory> Inventories { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
}