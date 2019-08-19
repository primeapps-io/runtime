using System;
using Hangfire;
using Hangfire.Common;
using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace PrimeApps.Admin
{
	public partial class Startup
	{
		private static readonly string QueueName = "queue_" + Environment.MachineName.Underscore();

		public static void JobConfiguration(IApplicationBuilder app, IConfiguration configuration)
		{
			var enableJobsSetting = configuration.GetValue("AppSettings:EnableJobs", string.Empty);

			if (!string.IsNullOrEmpty(enableJobsSetting))
			{
				var enableJobs = bool.Parse(enableJobsSetting);

				if (!enableJobs)
					return;
			}
			else
				return;
			
			app.UseHangfireServer(new BackgroundJobServerOptions { Queues = new[] { QueueName, "default" } });
			/*app.UseHangfireDashboard("/jobs", new DashboardOptions { Authorization = new[] { new HangfireAuthorizationFilter() } });*/
			JobHelper.SetSerializerSettings(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

			GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0 });
		}
	}
}