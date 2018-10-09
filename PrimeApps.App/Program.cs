using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using Sentry;

namespace PrimeApps.App
{
    public class Program
    {
        public static IConfiguration Configuration { get; set; }

        public static void Main(string[] args)
        {
            using (SentrySdk.Init(Configuration.GetValue("AppSettings:SentryClientKey", String.Empty)))
            {
                CreateWebHostBuilder(args).Build().Run();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {

            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSentry();
        }
    }
}
