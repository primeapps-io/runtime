using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;
using WarehouseHelper = PrimeApps.App.Jobs.Warehouse;
using Hangfire;
using PrimeApps.App.Services;
using Microsoft.EntityFrameworkCore;

namespace PrimeApps.App
{
	public partial class Startup
	{
		public static void DIRegister(IServiceCollection services)
		{
			services.AddDbContext<TenantDBContext>(options =>
				options.UseNpgsql("Data Source=blog.db"));

			services.AddDbContext<PlatformDBContext>(options =>
				options.UseNpgsql("Data Source=blog.db"));

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

			GlobalConfiguration.Configuration.UseActivator(new HangfireActivator(services.BuildServiceProvider()));
		}
	}
}
