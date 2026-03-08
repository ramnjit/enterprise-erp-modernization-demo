namespace LegacyErpMonolith.Models;

public class Inventory
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string LocationCode { get; set; } = string.Empty;
    public int Quantity { get; set; }

    // Navigation back to the Product
    public Product? Product { get; set; }
}