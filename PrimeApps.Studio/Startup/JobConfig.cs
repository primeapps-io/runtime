using System;
using Hangfire;
using Hangfire.Common;
using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PrimeApps.Studio.ActionFilters;

namespace PrimeApps.Studio
{
	public partial class Startup
	{
		private static readonly string QueueName = "queue_" + Environment.MachineName.Underscore();

		public static void JobConfiguration(IApplicationBuilder app, IConfiguration configuration)
		{
			var enableJobs_ = configuration.GetValue("AppSettings:EnableJobs", string.Empty);

			if (!string.IsNullOrEmpty(enableJobs_))
			{
				var enableJobs = bool.Parse(enableJobs_);

				if (!enableJobs)
					return;
			}
			else
				return;
			
			app.UseHangfireServer(new BackgroundJobServerOptions { Queues = new[] { QueueName, "default" } });
			app.UseHangfireDashboard("/jobs", new DashboardOptions { Authorization = new[] { new HangfireAuthorizationFilter() } });
			JobHelper.SetSerializerSettings(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

			GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0 });
		}
	}
}