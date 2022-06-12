using CatalogService.Api.Infrastructure.Context;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.IO;
using CatalogService.Api.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var hostBuilder = CreateHostBuilder(args);

            hostBuilder.MigrateDbContext<CatalogContext>((context, services) =>
            {
                new CatalogContextSeed()
                .SeedAsync(context, services.GetService<IWebHostEnvironment>(), services.GetService<ILogger<CatalogContextSeed>>()).Wait();
            });

            hostBuilder.Run();
        }

        static IWebHost CreateHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseWebRoot("Pics")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .Build();
        }
    }
}
