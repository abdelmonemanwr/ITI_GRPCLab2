using Client.Models;
using Client.Proto;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
namespace Client.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult> AddOrUpdateProduct(ProductModel product)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:7275");
            var client = new Client.Proto.Inventory.InventoryClient(channel);
            var ExistRequest = new ProductIdRequest { Id = product.id };
            var ExistResponse = await client.GetProductByIdAsync(ExistRequest);
            if (ExistResponse != null) {
                var Request = new Product
                {
                    Id = product.id,
                    Title = product.title,
                    Price = product.price,
                    Quantity = product.quantity,
                    Category = (Category)product.category,
                    ExpireDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(product.expireDate.ToUniversalTime())
                };
                if (ExistResponse.Exists == true) {
                    var UpdateResponse = await client.UpdateProductAsync(Request);
                    return Ok(new { Status = 201, Product = Request , Msg=UpdateResponse.Message });
                }
                else { 
                    var InsertResponse = await client.AddProductAsync(Request);
                    return Created("",new { Status = 200, Product = Request , Msg = InsertResponse.Message });
                }
            }
            else {
                return NotFound(new { message = "Response For Product Equal Null" });
            }
        }

        [HttpPost("Stream")]
        public async Task<IActionResult> AddBulkProducts([FromBody] List<ProductModel> products)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:7275");
            var client = new Client.Proto.Inventory.InventoryClient(channel);
            var call = client.AddBulkProducts();

            foreach (var product in products)
            {
                await call.RequestStream.WriteAsync(new Product
                {
                    Id = product.id,
                    Title = product.title,
                    Price = product.price,
                    Quantity = product.quantity,
                    Category = (Category)product.category,
                    ExpireDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(product.expireDate.ToUniversalTime())
                });
            }

            await call.RequestStream.CompleteAsync();
            var response = await call;
            return Ok(new { message = "Products added successfully", count = response.InsertedCount });
        }
        [HttpGet("Report")]
        public async Task<ActionResult<IEnumerable<Product>>> GenerateProductReport(bool priceOrder = false, int categoryFilter = (int)Category.Not)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:7275");
            var client = new Client.Proto.Inventory.InventoryClient(channel);
            var request = new ProductReportRequest
            {
                PriceOrder = priceOrder,
                CategoryFilter = (Category)categoryFilter
            };

            var products = new List<Product>();
            var call = client.GetProductReport(request);
            while (await call.ResponseStream.MoveNext())
            {
                var product = call.ResponseStream.Current;
                products.Add(product);
            }
            return Ok(products);
        }
    }
}
