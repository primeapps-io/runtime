using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Services;
using PrimeApps.Model.Storage;

namespace PrimeApps.Studio
{
    public partial class Startup
    {
        public static void DIRegister(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<TenantDBContext>(options => options.UseNpgsql(configuration.GetConnectionString("TenantDBConnection")));
            services.AddDbContext<PlatformDBContext>(options => options.UseNpgsql(configuration.GetConnectionString("PlatformDBConnection")));
            services.AddDbContext<StudioDBContext>(options => options.UseNpgsql(configuration.GetConnectionString("StudioDBConnection")));
            services.AddScoped(p => new PlatformDBContext(p.GetService<DbContextOptions<PlatformDBContext>>(), configuration));
            services.AddScoped(p => new StudioDBContext(p.GetService<DbContextOptions<StudioDBContext>>(), configuration));

            services.AddHttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton(configuration);

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

            services.AddScoped<ICacheHelper, CacheHelper>();
            services.AddScoped<IRecordHelper, Helpers.RecordHelper>();
            services.AddScoped<IAuditLogHelper, AuditLogHelper>();
            services.AddScoped<ICalculationHelper, CalculationHelper>();
            services.AddScoped<IChangeLogHelper, ChangeLogHelper>();
            //services.AddScoped<IFunctionHelper, FunctionHelper>();
            //services.AddScoped<IComponentHelper, ComponentHelper>();
            services.AddScoped<IModuleHelper, Helpers.ModuleHelper>();
            services.AddScoped<IWorkflowHelper, WorkflowHelper>();
            services.AddScoped<IProcessHelper, ProcessHelper>();
            services.AddScoped<IDocumentHelper, DocumentHelper>();
            //services.AddScoped<IBpmHelper, BpmHelper>();
            services.AddScoped<IRoleHelper, RoleHelper>();
            //services.AddScoped<IGiteaHelper, GiteaHelper>();
            services.AddScoped<INotificationHelper, NotificationHelper>();
            services.AddScoped<IActivityHelper, ActivityHelper>();
            services.AddScoped<IOrganizationHelper, OrganizationHelper>();
            services.AddScoped<IPermissionHelper, PermissionHelper>();
            //services.AddScoped<IDeploymentHelper, DeploymentHelper>();
            services.AddScoped<ActionButtonHelper, ActionButtonHelper>();
            services.AddScoped<Email, Email>();
            services.AddScoped<Warehouse, Warehouse>();
            services.AddTransient<IUnifiedStorage, UnifiedStorage>();
            services.AddScoped<IReportHelper, ReportHelper>();
            services.AddScoped<IPackageHelper, Helpers.PackageHelper>();
            services.AddScoped<IMigrationHelper, MigrationHelper>();
            services.AddScoped<IAppDraftTemplateHelper, AppDraftTemplateHelper>();
            
            services.TryAddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.TryAddSingleton<IHistoryHelper, HistoryHelper>();
            services.TryAddSingleton<IWebSocketHelper, WebSocketHelper>();
        }
    }
}