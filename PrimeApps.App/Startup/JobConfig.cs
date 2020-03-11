using Hangfire;
using Hangfire.Common;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json;
using PrimeApps.App.ActionFilters;
using System;
using Hangfire.Dashboard.BasicAuthorization;
using Humanizer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace PrimeApps.App
{
	public partial class Startup
	{
		private static readonly string QueueName = "queue_" + Environment.MachineName.Underscore();

		public static void JobConfiguration(IApplicationBuilder app, IConfiguration configuration, IHostingEnvironment env)
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

			app.UseHangfireServer(new BackgroundJobServerOptions
			{
				Queues = new[] { QueueName, "default" }
			});

			DashboardOptions dashboardOptions;
			if (env.IsDevelopment())
			{
				dashboardOptions = new DashboardOptions { Authorization = new[] { new HangfireAuthorizationFilter() } };
			}
			else
			{
				var sslRedirect = configuration.GetValue("AppSettings:HttpsRedirection", string.Empty);
				dashboardOptions = new DashboardOptions
				{
					Authorization = new[]
					{
						new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
						{
							RequireSsl = true,
							SslRedirect = !string.IsNullOrEmpty(sslRedirect) && bool.Parse(sslRedirect),
							LoginCaseSensitive = true,
							Users = new[]
							{
								new BasicAuthAuthorizationUser
								{
									Login = "admin",
									PasswordClear = configuration.GetValue("AppSettings:JobsPassword", string.Empty)
								}
							}

						})
					}
				};
			}

			app.UseHangfireDashboard("/jobs", dashboardOptions);
			JobHelper.SetSerializerSettings(new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

			//ConfigureRecurringJobs();//Onpremise durumu ve bu job'larin app'e ozel olmasindan dolayi gecici olarak kapatildi. 

			GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0 });
		}

		public static void ConfigureRecurringJobs()
		{
			RecurringJob.AddOrUpdate<Jobs.TrialNotification>("trial-expire-notification", expire => expire.TrialExpire(), "0 * * * *", TimeZoneInfo.Utc, QueueName);
			RecurringJob.AddOrUpdate<Jobs.AccountDeactivate>("deactivate-account", deactivate => deactivate.Deactivate(), Cron.Daily(04, 00), TimeZoneInfo.Utc, QueueName);
			RecurringJob.AddOrUpdate<Jobs.UpdateLeave>("update-leave", update => update.Update(), Cron.Daily(04, 00), TimeZoneInfo.Utc, QueueName);
			RecurringJob.AddOrUpdate<Jobs.EmployeeCalculation>("employee-calculation", employee => employee.Calculate(), Cron.Daily(04, 00), TimeZoneInfo.Utc, QueueName);
			RecurringJob.AddOrUpdate<Jobs.AccountCleanup>("cleanup-account", cleanup => cleanup.Run(), Cron.Daily(03, 00), TimeZoneInfo.Utc, QueueName);
		}
	}
}