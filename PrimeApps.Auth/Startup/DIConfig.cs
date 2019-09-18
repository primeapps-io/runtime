using System;
using System.Linq;
using System.Reflection;
using Amazon.S3;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Auth.Data;
using PrimeApps.Auth.Helpers;
using PrimeApps.Auth.Models;
using PrimeApps.Auth.Providers;
using PrimeApps.Auth.Repositories;
using PrimeApps.Auth.Repositories.IRepositories;
using PrimeApps.Auth.Services;
using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Util.Storage;

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

            services.AddSingleton(configuration);
            services.AddHttpContextAccessor();

            //Register all repositories
            foreach (var assembly in new[] {"PrimeApps.Model"})
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

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddHostedService<QueuedHostedService>();
            services.AddAWSService<IAmazonS3>();
            services.AddScoped<IGiteaHelper, GiteaHelper>();
            services.AddScoped<IUserHelper, UserHelper>();
            services.AddSingleton<IProfileService, CustomProfileService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IPlatformRepository, PlatformRepository>();
            services.AddTransient<IPlatformUserRepository, PlatformUserRepository>();
            services.AddTransient<IApplicationRepository, ApplicationRepository>();
            services.AddTransient<IUnifiedStorage, UnifiedStorage>();

            services.AddScoped<SignInManager<ApplicationUser>, ApplicationSignInManager>();
            services.AddTransient<IClientRepository, ClientRepository>();
        }
    }
}