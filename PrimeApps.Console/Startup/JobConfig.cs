using Hangfire;
using Hangfire.Common;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json;
using PrimeApps.Console.ActionFilters;
using System;
using Humanizer;
using Microsoft.Extensions.Configuration;

namespace PrimeApps.Console
{
    public partial class Startup
    {
        private static readonly string QueueName = "queue_" + Environment.MachineName.Underscore();

        public static void JobConfiguration(IApplicationBuilder app, IConfiguration configuration)
        {
            var enableJobs = bool.Parse(configuration.GetSection("AppSettings")["EnableJobs"]);

            if (!enableJobs)
                return;

            app.UseHangfireServer(new BackgroundJobServerOptions { Queues = new[] { QueueName, "default" } });
            app.UseHangfireDashboard("/jobs", new DashboardOptions { Authorization = new[] { new HangfireAuthorizationFilter() } });
            JobHelper.SetSerializerSettings(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0 });
        }
    }
}