using Hangfire;
using Hangfire.Common;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json;
using PrimeApps.App.ActionFilters;
using System;
using Humanizer;
using Microsoft.Extensions.Configuration;

namespace PrimeApps.App
{
    public partial class Startup
    {
        private static readonly string QueueName = "queue_" + Environment.MachineName.Underscore();

        public static void JobConfiguration(IApplicationBuilder app, IConfiguration configuration)
        {
            var enableJobs = bool.Parse(configuration.GetSection("AppSettings")["EnableJobs"]);

            if (!enableJobs)
                return;

            app.UseHangfireServer(new BackgroundJobServerOptions {Queues = new[] {QueueName, "default"}});
            app.UseHangfireDashboard("/jobs", new DashboardOptions {Authorization = new[] {new HangfireAuthorizationFilter()}});
            JobHelper.SetSerializerSettings(new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});

            ConfigureRecurringJobs();

            GlobalJobFilters.Filters.Add(new QueueFilterAttribute());
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute {Attempts = 0});
        }

        public static void ConfigureRecurringJobs()
        {
            RecurringJob.AddOrUpdate<Jobs.ExchangeRate>("daily-exchange-rate", exchange => exchange.DailyRates(), Cron.Daily(13, 00), TimeZoneInfo.Utc, QueueName);
            RecurringJob.AddOrUpdate<Jobs.TrialNotification>("trial-expire-notification", expire => expire.TrialExpire(), "0 * * * *", TimeZoneInfo.Utc, QueueName);
            RecurringJob.AddOrUpdate<Jobs.AccountDeactivate>("deactivate-account", deactivate => deactivate.Deactivate(), Cron.Daily(04, 00), TimeZoneInfo.Utc, QueueName);
            RecurringJob.AddOrUpdate<Jobs.UpdateLeave>("update-leave", update => update.Update(), Cron.Daily(04, 00), TimeZoneInfo.Utc, QueueName);
            RecurringJob.AddOrUpdate<Jobs.EmployeeCalculation>("employee-calculation", employee => employee.Calculate(), Cron.Daily(04, 00), TimeZoneInfo.Utc, QueueName);
            RecurringJob.AddOrUpdate<Jobs.AccountCleanup>("cleanup-account", cleanup => cleanup.Run(), Cron.Daily(03, 00), TimeZoneInfo.Utc, QueueName);
        }
    }
}