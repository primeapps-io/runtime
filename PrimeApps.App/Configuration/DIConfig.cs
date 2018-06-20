using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;
using WarehouseHelper = PrimeApps.App.Jobs.Warehouse;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;

namespace PrimeApps.App
{
    public partial class Startup
    {
        public static void DIRegister(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<TenantDBContext>(options => options.UseNpgsql(configuration.GetConnectionString("TenantDBConnection")));
            services.AddDbContext<PlatformDBContext>(options => options.UseNpgsql(configuration.GetConnectionString("PlatformDBConnection")));

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton(configuration);

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
        }
    }
}
