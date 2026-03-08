namespace LegacyErpMonolith.Models;

public class Order
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }

    // Navigation back to the Product
    public Product? Product { get; set; }
}