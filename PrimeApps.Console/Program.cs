using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace PrimeApps.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSentry()
                .Build();
        }
    }
}
