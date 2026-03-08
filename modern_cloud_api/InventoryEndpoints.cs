using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace modern_cloud_api
{
    public class InventoryEndpoints
    {
        private readonly ILogger _logger;

        public InventoryEndpoints(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<InventoryEndpoints>();
        }

        // The O(1) Single SKU Lookup
        [Function("GetSingleModern")]
        public async Task<HttpResponseData> GetSingleProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "modern/product/{sku}")] HttpRequestData req,
            string sku)
        {
            var watch = Stopwatch.StartNew();
            
            var connectionString = Environment.GetEnvironmentVariable("AzureTableStorage");
            var tableClient = new TableClient(connectionString, "FastInventory");

            var response = req.CreateResponse();

            try
            {
                var tableEntity = await tableClient.GetEntityAsync<TableEntity>("Hardware", sku);
                watch.Stop();

                // SUCCESS: Set status FIRST, then write JSON
                response.StatusCode = HttpStatusCode.OK;
                await response.WriteAsJsonAsync(new {
                    Source = "Azure Function (Serverless)",
                    Name = tableEntity.Value.GetString("Name"),
                    Sku = sku,
                    CurrentStock = tableEntity.Value.GetInt32("TotalQuantity"),
                    TimeTakenMs = watch.ElapsedMilliseconds
                });
                
                return response;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                // FAILURE: Set status FIRST, then write string
                response.StatusCode = HttpStatusCode.NotFound;
                await response.WriteStringAsync("Product not found in Azure.");
                return response;
            }
        }

        // The Full Catalog Fetch
        [Function("GetAllModern")]
        public async Task<HttpResponseData> GetAllProducts(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "modern/all")] HttpRequestData req)
        {
            var watch = Stopwatch.StartNew();
            var connectionString = Environment.GetEnvironmentVariable("AzureTableStorage");
            var tableClient = new TableClient(connectionString, "FastInventory");

            var products = new List<object>();
            
            await foreach (var page in tableClient.QueryAsync<TableEntity>().AsPages())
            {
                foreach(var entity in page.Values)
                {
                    products.Add(new {
                        Name = entity.GetString("Name"),
                        Sku = entity.RowKey,
                        CurrentStock = entity.GetInt32("TotalQuantity")
                    });
                }
            }
            watch.Stop();

            var response = req.CreateResponse();
            
            // Set status FIRST, then write JSON
            response.StatusCode = HttpStatusCode.OK;
            await response.WriteAsJsonAsync(new {
                Source = "Azure Function (Serverless)",
                TotalCount = products.Count,
                TimeTakenMs = watch.ElapsedMilliseconds,
                Products = products
            });

            return response;
        }
    }
}