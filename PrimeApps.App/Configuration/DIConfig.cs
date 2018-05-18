using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;
using WarehouseHelper = PrimeApps.App.Jobs.Warehouse;
using Hangfire;
using PrimeApps.App.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Repositories;
using System.Reflection;
using System.Linq;
using System;
using System.Configuration;
using Microsoft.AspNetCore.Http;
using PrimeApps.App.ActionFilters;
using Minio;

namespace PrimeApps.App
{
	public partial class Startup
	{
		public static void DIRegister(IServiceCollection services, IConfiguration configuration)
		{
			
			services.AddDbContext<TenantDBContext>(options =>
				options.UseNpgsql(ConfigurationManager.ConnectionStrings["PostgreSqlConnection"].ConnectionString));
			
			services.AddDbContext<PlatformDBContext>(options =>
				options.UseNpgsql(ConfigurationManager.ConnectionStrings["PostgreSqlConnection"].ConnectionString));

			/*services.AddScoped<IActionButtonRepository, ActionButtonRepository>();
			services.AddScoped<IAnalyticRepository, AnalyticRepository>();
			services.AddScoped<IAuditLogRepository, AuditLogRepository>();
			services.AddScoped<IChangeLogRepository, ChangeLogRepository>();
			services.AddScoped<IConversionMappingRepository, ConversionMappingRepository>();
			services.AddScoped<IDashboardRepository, DashboardRepository>();
			services.AddScoped<IDashletRepository, DashletRepository>();
			services.AddScoped<IDocumentRepository, DocumentRepository>();
			services.AddScoped<IHelpRepository, HelpRepository>();
			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<IUserRepository, UserRepository>();*/

			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			// Register Repositories
			foreach (var a in new string[] { "PrimeApps.Model" })
			{
				Assembly loadedAss = Assembly.Load(a);

				var allServices = loadedAss.GetTypes().Where(t =>
									t.GetTypeInfo().IsClass &&
									!t.GetTypeInfo().IsAbstract && t.GetTypeInfo().Name.EndsWith("Repository"));

				foreach (var type in allServices)
				{
					var allInterfaces = type.GetInterfaces().Where(x => x.Name.EndsWith("Repository"));
					var mainInterfaces = allInterfaces.Except
							(allInterfaces.SelectMany(t => t.GetInterfaces()));
					foreach (var itype in mainInterfaces)
					{
						if (allServices.Any(x => !x.Equals(type) && itype.IsAssignableFrom(x)))
						{
							throw new Exception("The " + itype.Name + " type has more than one implementations, please change your filter");
						}
						services.AddTransient(itype, type);
					}
				}

			}

			services.AddScoped<WarehouseHelper, WarehouseHelper>();
			services.AddScoped<Warehouse, Warehouse>();
			services.AddScoped<Jobs.Email.Email, Jobs.Email.Email>();
			services.AddScoped<Jobs.Messaging.EMail.EMailClient, Jobs.Messaging.EMail.EMailClient>();
			services.AddScoped<Jobs.Messaging.SMS.SMSClient, Jobs.Messaging.SMS.SMSClient>();
			services.AddScoped<Jobs.Reminder.Activity, Jobs.Reminder.Activity>();
			services.AddScoped<Jobs.ExchangeRate, Jobs.ExchangeRate>();
			services.AddScoped<Jobs.TrialNotification, Jobs.TrialNotification>();
			services.AddScoped<Jobs.AccountDeactivate, Jobs.AccountDeactivate>();
			services.AddScoped<Jobs.UpdateLeave, Jobs.UpdateLeave>();
			services.AddScoped<Jobs.EmployeeCalculation, Jobs.EmployeeCalculation>();
			services.AddScoped<Jobs.AccountCleanup, Jobs.AccountCleanup>();
            services.AddTransient(x => new MinioClient(ConfigurationManager.ConnectionStrings["AzureStorageConnection"].ConnectionString,
                ConfigurationManager.AppSettings.Get("AzureStorageAccessKey"),
                ConfigurationManager.AppSettings.Get("AzureStorageSecretKey")
                ));
            services.AddSingleton(configuration);

			GlobalConfiguration.Configuration.UseActivator(new HangfireActivator(services.BuildServiceProvider()));
		}
	}
}
