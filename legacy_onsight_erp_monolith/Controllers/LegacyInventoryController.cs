using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LegacyErpMonolith.Data;

namespace LegacyErpMonolith.Controllers;

[ApiController]
[Route("api/legacy/")] 
public class LegacyInventoryController : ControllerBase
{
    private readonly AppDbContext _context;

    public LegacyInventoryController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("all")] 
    public async Task<IActionResult> GetAllLegacy()
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();

        var products = await _context.Products
            .Select(p => new 
            {
                Name = p.Name,
                Sku = p.Sku,
                CurrentStock = p.Inventories.Sum(i => i.Quantity) - p.Orders.Sum(o => o.Quantity)
            })
            .ToListAsync();

        watch.Stop();

        return Ok(new {
            Source = "SQLite (Legacy Monolith)",
            TotalCount = products.Count,
            TimeTakenMs = watch.ElapsedMilliseconds,
            Products = products // Returning the payload so it's a fair size comparison
        });
    }

    [HttpGet("product/{sku}")]
    public async Task<IActionResult> GetSingleLegacy(string sku)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();

        // Find the product (O(N) search in the product table)
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Sku == sku);
        if (product == null) return NotFound("Product not found.");

        // Scan 5,000 inventory rows
        var totalReceived = await _context.Inventories
            .Where(i => i.ProductId == product.Id)
            .SumAsync(i => i.Quantity);

        // Heavier Math: Scan 50,000 order rows
        var totalSold = await _context.Orders
            .Where(o => o.ProductId == product.Id)
            .SumAsync(o => o.Quantity);

        var currentStock = totalReceived - totalSold;

        watch.Stop();

        return Ok(new {
            Source = "SQLite (Legacy Monolith)",
            Name = product.Name,
            Sku = product.Sku,
            CurrentStock = currentStock,
            TimeTakenMs = watch.ElapsedMilliseconds
        });
    }
}