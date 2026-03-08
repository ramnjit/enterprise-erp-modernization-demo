using Microsoft.AspNetCore.Mvc;
using Azure.Data.Tables;
using LegacyErpMonolith.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace LegacyErpMonolith.Controllers
{
    [ApiController]
    [Route("api/migration")]
    public class DataMigrationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public DataMigrationController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // We use GET here strictly so you can easily trigger it from your web browser
        [HttpGet("start")]
        public async Task<IActionResult> StartMigration()
        {
            var sw = Stopwatch.StartNew();

            // CONNECT: Hook into Azure Storage Account
            var connectionString = _config.GetValue<string>("AzureTableStorage");
            var serviceClient = new TableServiceClient(connectionString);
            
            // Automatically create the "FastInventory" table if it doesn't exist yet
            var tableClient = serviceClient.GetTableClient("FastInventory");
            await tableClient.CreateIfNotExistsAsync();

            // EXTRACT & TRANSFORM
            // Calculate Total = (Inventory - Orders) right here in memory
            var products = await _context.Products
                .Select(p => new
                {
                    Sku = p.Sku,
                    Name = p.Name,
                    Price = p.Price,
                    TotalQuantity = _context.Inventories.Where(i => i.ProductId == p.Id).Sum(i => i.Quantity)
                                  - _context.Orders.Where(o => o.ProductId == p.Id).Sum(o => o.Quantity)
                })
                .ToListAsync();

            // LOAD Box in batches of 100 to respect Azure's speed limits
            var batch = new List<TableTransactionAction>();
            int totalUploaded = 0;

            foreach (var product in products)
            {
                // Create the flat NoSQL document
                var entity = new TableEntity("Hardware", product.Sku) // PartitionKey = "Hardware", RowKey = Sku
                {
                    { "Name", product.Name },
                    { "Price", product.Price },
                    { "TotalQuantity", product.TotalQuantity },
                    { "Status", product.TotalQuantity > 20 ? "In Stock" : "Low Stock" },
                    { "Architecture", "Cloud Read Model" }
                };

                batch.Add(new TableTransactionAction(TableTransactionActionType.UpsertReplace, entity));

                // When the box has 100 items, ship it to Azure and start a new box
                if (batch.Count == 100)
                {
                    await tableClient.SubmitTransactionAsync(batch);
                    totalUploaded += batch.Count;
                    batch.Clear();
                }
            }

            // Ship whatever is left over in the final box
            if (batch.Count > 0)
            {
                await tableClient.SubmitTransactionAsync(batch);
                totalUploaded += batch.Count;
            }

            sw.Stop();

            return Ok(new {
                Message = "ETL Migration Complete! Data is now in the cloud.",
                RowsMigrated = totalUploaded,
                TimeTakenSeconds = sw.Elapsed.TotalSeconds
            });
        }
    }
}