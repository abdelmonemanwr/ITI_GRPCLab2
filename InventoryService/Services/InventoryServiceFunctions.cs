using Grpc.Core;
using InventoryService.Proto;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Concurrent;
using static InventoryService.Proto.Inventory;

namespace InventoryService.Services
{
    public class InventoryServiceFunctions : InventoryBase
    {
        private static readonly List<Product> Products = new List<Product>();
        public override Task<ProductExistenceResponse> GetProductById(ProductIdRequest request, ServerCallContext context)
        {
            bool exists = Products.Any(p => p.Id == request.Id);
            return Task.FromResult(new ProductExistenceResponse { Exists = exists });
        }

        [Authorize(AuthenticationSchemes = Declartions.ApiKeySchemeName)]
        public override Task<AddProductResponse> AddProduct(Product request, ServerCallContext context)
        {
            Products.Add(request);
            return Task.FromResult(new AddProductResponse { Message = "Product added successfully" });
        }

        [Authorize(AuthenticationSchemes = Declartions.ApiKeySchemeName)]
        public override Task<UpdateProductResponse> UpdateProduct(Product request, ServerCallContext context)
        {
            var product = Products.FirstOrDefault(p => p.Id == request.Id);
            if (product != null)
            {
                product.Title = request.Title;
                product.Price = request.Price;
                product.Quantity = request.Quantity;
                return Task.FromResult(new UpdateProductResponse { Message = "Product updated successfully" });
            }
            else
            {
                return Task.FromResult(new UpdateProductResponse { Message = "Product not found" });
            }
        }

        [Authorize(AuthenticationSchemes = Declartions.ApiKeySchemeName)]
        public override async Task<BulkProductResponse> AddBulkProducts(IAsyncStreamReader<Product> requestStream, ServerCallContext context)
        {
            int Count = 0;
            await foreach (var product in requestStream.ReadAllAsync())
            {
                Products.Add(product);
                Count++;
            }
            return new BulkProductResponse { InsertedCount = Count };
        }

        [Authorize(AuthenticationSchemes = Declartions.ApiKeySchemeName)]
        public override async Task GetProductReport(ProductReportRequest request, IServerStreamWriter<Product> responseStream, ServerCallContext context)
        {
            List<Product> data = Products.ToList();
            if (request.PriceOrder)
            {
                foreach (var product in data.OrderBy(p => p.Price))
                {
                    await responseStream.WriteAsync(product);
                }
            }
            else if (request.CategoryFilter != Category.Not)
            {
                foreach (var product in data.Where(p => p.Category == request.CategoryFilter))
                {
                    await responseStream.WriteAsync(product);
                }
            }
            else
            {
                foreach (var product in data)
                {
                    await responseStream.WriteAsync(product);
                }
            }
        }
    }
}
