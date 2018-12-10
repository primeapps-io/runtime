using System;
using System.Linq;
using System.Reflection;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Auth.Data;
using PrimeApps.Auth.Models;
using PrimeApps.Auth.Providers;
using PrimeApps.Auth.Services;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Auth
{
    public partial class Startup
    {
        public static void DIRegister(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("AuthDBConnection")));
            services.AddDbContext<TenantDBContext>(options => options.UseNpgsql(configuration.GetConnectionString("TenantDBConnection")));
            services.AddDbContext<PlatformDBContext>(options => options.UseNpgsql(configuration.GetConnectionString("PlatformDBConnection")));
            services.AddScoped(p => new PlatformDBContext(p.GetService<DbContextOptions<PlatformDBContext>>()));
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

            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IProfileService, CustomProfileService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IPlatformRepository, PlatformRepository>();
            services.AddTransient<IPlatformUserRepository, PlatformUserRepository>();
            services.AddTransient<IApplicationRepository, ApplicationRepository>();

            services.AddScoped<SignInManager<ApplicationUser>, ApplicationSignInManager>();
        }
    }
}
