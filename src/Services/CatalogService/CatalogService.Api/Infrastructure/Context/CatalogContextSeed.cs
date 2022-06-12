using CatalogService.Api.Core.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CatalogService.Api.Infrastructure.Context
{
    public class CatalogContextSeed
    {
        public async Task SeedAsync(CatalogContext context, IWebHostEnvironment env, ILogger<CatalogContextSeed> logger)
        {
            var policy = Policy.Handle<SqlException>()
                .WaitAndRetryAsync(retryCount: 3, sleepDurationProvider: retry => TimeSpan.FromSeconds(5), onRetry: (exc, timeSpan, retry, ctx) =>
                {
                    logger.LogWarning(exc, "Exception  detected on retry");
                });

            await policy.ExecuteAsync(() => ProcessSeeding(context, logger));
        }

        private async Task ProcessSeeding(CatalogContext context, ILogger logger)
        {
            if (!context.CatalogBrands.Any())
            {
                await context.CatalogBrands.AddRangeAsync(new CatalogBrand[] {
                    new CatalogBrand{ Brand = "Apple"},
                    new CatalogBrand{ Brand = "Sony" },
                    new CatalogBrand{ Brand = "Asus" }
                });

                await context.SaveChangesAsync();
            }

            if (!context.CatalogTypes.Any())
            {
                await context.CatalogTypes.AddRangeAsync(new CatalogType[]
                    {
                        new CatalogType { Type = "Mobile Phone" },
                        new CatalogType { Type = "Laptop" },
                        new CatalogType { Type = "Camera" },
                        new CatalogType { Type = "Smart Watch" }
                    });

                await context.SaveChangesAsync();
            }

            if (!context.CatalogItems.Any())
            {
                await context.CatalogItems.AddRangeAsync(new CatalogItem[]
                    {
                        new CatalogItem{ CatalogTypeId = 1, CatalogBrandId = 1, AvailableStock = 100, Description = "iPhone 13 Pro Max 256 GB", Name = "iPhone 13 Pro Max", OnReOrder = true, PictureFileName = "1.jpg", Price = 30000  },
                        new CatalogItem{ CatalogTypeId = 1, CatalogBrandId = 2, AvailableStock = 100, Description = "Sony SmartPhone", Name = "Sony SmartPhone", OnReOrder = false, PictureFileName = "1.jpg", Price = 45000  },
                        new CatalogItem{ CatalogTypeId = 2, CatalogBrandId = 3, AvailableStock = 100, Description = "Asus Ultrabook X123-SH", Name = "Asus Ultrabook", OnReOrder = false, PictureFileName = "2.jpg", Price = 20000  },
                        new CatalogItem{ CatalogTypeId = 3, CatalogBrandId = 2, AvailableStock = 20, Description = "Sony A7RII Mirrorless", Name = "Sony A7RII", OnReOrder = true, PictureFileName = "3.jpeg", Price = 25000  }
                    });

                await context.SaveChangesAsync();
            }
        }
    }
}
