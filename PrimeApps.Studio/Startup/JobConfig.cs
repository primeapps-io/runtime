using System;
using Hangfire;
using Hangfire.Common;
using Hangfire.Dashboard.BasicAuthorization;
using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PrimeApps.Studio.ActionFilters;

namespace PrimeApps.Studio
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
				
			app.UseHangfireServer(new BackgroundJobServerOptions { Queues = new[] { QueueName, "default" } });

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

			GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0 });
		}
	}
}