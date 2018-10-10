using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;

namespace PrimeApps.Auth
{
    public class Program
    {
        public static IConfiguration Configuration { get; set; }

        public static void Main(string[] args)
        {
            // var builder = new ConfigurationBuilder()
            //     .SetBasePath(Directory.GetCurrentDirectory())
            //     .AddJsonFile("appsettings.json")
            //     .AddEnvironmentVariables();

            // Configuration = builder.Build();
            // Console.Title = "PrimeApps.Auth";

            // var seed = args.Any(x => x == "/seed");
            // if (seed) args = args.Except(new[] { "/seed" }).ToArray();

            // var host = CreateWebHostBuilder(args).Build();

            // if (seed)
            // {
            //     SeedData.EnsureSeedData(host.Services);
            //     return;
            // }
            CreateWebHostBuilder(args).Build().Run();

        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSentry();
        }
    }
}