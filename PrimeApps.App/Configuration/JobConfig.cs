using Hangfire;
using Hangfire.Common;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json;
using PrimeApps.App.ActionFilters;
using System;

namespace PrimeApps.App
{
    public partial class Startup
    {
        public static void JobConfiguration(IApplicationBuilder app)
        {
            app.UseHangfireServer(new BackgroundJobServerOptions { Queues = new[] { "Queue-" + Environment.MachineName } });
            app.UseHangfireDashboard("/jobs", new DashboardOptions { Authorization = new[] { new HangfireAuthorizationFilter() } });
            JobHelper.SetSerializerSettings(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            ConfigureRecurringJobs();

            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0 });
        }

        public static void ConfigureRecurringJobs()
        {
            RecurringJob.AddOrUpdate<Jobs.ExchangeRate>("daily-exchange-rate", exchange => exchange.DailyRates(), Cron.Daily(13, 00), TimeZoneInfo.Utc);
            RecurringJob.AddOrUpdate<Jobs.TrialNotification>("trial-expire-notification", expire => expire.TrialExpire(), "0 * * * *", TimeZoneInfo.Utc);
            RecurringJob.AddOrUpdate<Jobs.AccountDeactivate>("deactivate-account", deactivate => deactivate.Deactivate(), Cron.Daily(01, 00));
            RecurringJob.AddOrUpdate<Jobs.UpdateLeave>("update-leave", update => update.Update(), Cron.Daily(04, 00), TimeZoneInfo.Utc);
            RecurringJob.AddOrUpdate<Jobs.EmployeeCalculation>("employee-calculation", employee => employee.Calculate(), Cron.Daily(04, 00), TimeZoneInfo.Utc);
            RecurringJob.AddOrUpdate<Jobs.AccountCleanup>("cleanup-account", cleanup => cleanup.Run(), Cron.Daily(00, 00));
        }
    }
}
