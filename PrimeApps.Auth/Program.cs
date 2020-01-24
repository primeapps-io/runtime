using System;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace PrimeApps.Auth
{
    public class Program
    {
        public static IConfiguration Configuration { get; set; }

        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var hostBuilder = WebHost.CreateDefaultBuilder(args)
                .UseSetting("https_port", "443")
                .UseStartup<Startup>()
                .UseSentry();

            if (args.Contains("--run-as-service"))
                hostBuilder.UseContentRoot(AppContext.BaseDirectory);
            
            return hostBuilder.Build();
        }
    }
}