using Bogus;
using LegacyErpMonolith.Models;

namespace LegacyErpMonolith.Data;

public static class DataSeeder
{
    public static void SeedData(AppDbContext context)
    {
        // If we already have products, don't seed again.
        if (context.Products.Any()) return;

        Console.WriteLine("Seeding database... This might take a few seconds.");

        // Generate 1,000 realistic Retail Products
        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Sku, f => f.Commerce.Ean13())
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Price, f => Math.Round(f.Random.Decimal(5.0m, 500.0m), 2));

        var products = productFaker.Generate(1000);
        context.Products.AddRange(products);
        context.SaveChanges(); // Save so the database generates the Product IDs

        // Generate 5,000 Inventory Records
        var locations = new[] { "JVL-WH1", "JVL-WH2", "MAD-STR", "CHI-STR" };
        var inventoryFaker = new Faker<Inventory>()
            .RuleFor(i => i.ProductId, f => f.PickRandom(products).Id)
            .RuleFor(i => i.LocationCode, f => f.PickRandom(locations))
            .RuleFor(i => i.Quantity, f => f.Random.Int(0, 500));

        var inventory = inventoryFaker.Generate(5000);
        context.Inventories.AddRange(inventory);

        // Generate 50,000 Orders
        var orderFaker = new Faker<Order>()
            .RuleFor(o => o.ProductId, f => f.PickRandom(products).Id)
            .RuleFor(o => o.OrderDate, f => f.Date.Past(2))
            .RuleFor(o => o.Status, f => f.PickRandom("Pending", "Shipped", "Delivered"))
            .RuleFor(o => o.Quantity, f => f.Random.Int(1, 10))
            .RuleFor(o => o.TotalAmount, (f, o) => {
                var price = products.First(p => p.Id == o.ProductId).Price;
                return price * o.Quantity; // Calculate a realistic total
            });

        var orders = orderFaker.Generate(50000); 
        context.Orders.AddRange(orders);

        // Commit everything to the SQLite database
        context.SaveChanges();
        
        Console.WriteLine("Database successfully seeded with 56,000 records!");
    }
}