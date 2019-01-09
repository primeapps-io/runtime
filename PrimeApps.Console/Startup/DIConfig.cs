using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Model.Context;
using System;
using System.Linq;
using System.Reflection;
using PrimeApps.Console.Services;
using PrimeApps.Console.Helpers;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Console
{
    public partial class Startup
    {
        public static void DIRegister(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ICacheHelper, CacheHelper>();
            services.AddDbContext<TenantDBContext>(options => options.UseNpgsql(configuration.GetConnectionString("TenantDBConnection")));
            services.AddDbContext<PlatformDBContext>(options => options.UseNpgsql(configuration.GetConnectionString("PlatformDBConnection")));
            services.AddDbContext<ConsoleDBContext>(options => options.UseNpgsql(configuration.GetConnectionString("ConsoleDBConnection")));
            services.AddScoped(p => new PlatformDBContext(p.GetService<DbContextOptions<PlatformDBContext>>()));
            services.AddScoped(p => new ConsoleDBContext(p.GetService<DbContextOptions<ConsoleDBContext>>()));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton(configuration);
            services.AddHttpContextAccessor();

            //Register all repositories
            foreach (var assembly in new[] { "PrimeApps.Model" })
            {
                var assemblies = Assembly.Load(assembly);
                var allServices = assemblies.GetTypes().Where(t => t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract && t.GetTypeInfo().Name.EndsWith("Repository")).ToList();

                foreach (var type in allServices)
                {
                    var allInterfaces = type.GetInterfaces().Where(x => x.Name.EndsWith("Repository")).ToList();
                    var mainInterfaces = allInterfaces.Except(allInterfaces.SelectMany(t => t.GetInterfaces()));

                    foreach (var itype in mainInterfaces)
                    {
                        if (allServices.Any(x => x != type && itype.IsAssignableFrom(x)))
                        {
                            throw new Exception("The " + itype.Name + " type has more than one implementations, please change your filter");
                        }

                        services.AddTransient(itype, type);
                    }
                }
            }
            services.AddScoped<Warehouse, Warehouse>();
            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

            services.AddScoped<IModuleHelper, Helpers.ModuleHelper>();
            services.AddScoped<IAuditLogHelper, AuditLogHelper>();
            services.AddScoped<IOrganizationHelper, OrganizationHelper>();

            services.AddScoped<Email, Email>();
            services.AddScoped<IPermissionHelper, PermissionHelper>();
        }
    }
}
