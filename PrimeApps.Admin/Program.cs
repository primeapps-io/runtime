using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace PrimeApps.Admin
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var hostBuilder = WebHost.CreateDefaultBuilder(args)
                .UseSetting("https_port", "443")
                .UseStartup<Startup>();

            var useProxy = Environment.GetEnvironmentVariable("Proxy__UseProxy");

            if (!string.IsNullOrWhiteSpace(useProxy) && bool.Parse(useProxy))
            {
                var proxyUrl = Environment.GetEnvironmentVariable("Proxy__ProxyUrl");

                if (!string.IsNullOrWhiteSpace(proxyUrl))
                {
                    var webProxy = new WebProxy(proxyUrl);
                    hostBuilder.UseSentry(o => o.HttpProxy = webProxy);
                }
            }
            else
            {
                hostBuilder.UseSentry();
            }

            if (args.Contains("--run-as-service"))
                hostBuilder.UseContentRoot(AppContext.BaseDirectory);

            return hostBuilder.Build();
        }
    }
}