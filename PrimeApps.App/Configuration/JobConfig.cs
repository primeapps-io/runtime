using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using PrimeApps.App.ActionFilters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.App
{
    public partial class Startup
    {
		public static void JobConfiguration(IApplicationBuilder app)
		{
			// Configure hangfire and initialize it.
			app.UseHangfireServer(new BackgroundJobServerOptions
			{
				Queues = ConfigurationManager.AppSettings["HangfireQueues"].Split(',')
			});
			app.UseHangfireDashboard("/jobs", new DashboardOptions { Authorization = new[] { new HangfireAuthorizationFilter() } });

			ConfigureRecurringJobs();
			ConfigureScheduledJobs();
			ConfigureBackgroundJobs();
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

		public static void ConfigureScheduledJobs()
		{
		}

		public static void ConfigureBackgroundJobs()
		{
		}
	}
}
