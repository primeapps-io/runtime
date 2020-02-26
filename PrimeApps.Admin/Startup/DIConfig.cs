using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PrimeApps.Admin.Helpers;
using PrimeApps.Admin.Jobs;
using PrimeApps.Admin.Services;
using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Storage;
using PublishHelper = PrimeApps.Admin.Helpers.PublishHelper;

namespace PrimeApps.Admin
{
    public partial class Startup
    {
        public static void DIRegister(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<TenantDBContext>(options => options.UseNpgsql(configuration.GetConnectionString("TenantDBConnection")));
            services.AddDbContext<PlatformDBContext>(options => options.UseNpgsql(configuration.GetConnectionString("PlatformDBConnection")));
            services.AddScoped(p => new PlatformDBContext(p.GetService<DbContextOptions<PlatformDBContext>>(), configuration));

            services.AddHttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton(configuration);

            services.AddTransient<IApplicationRepository, ApplicationRepository>();
            services.AddTransient<IPlatformUserRepository, PlatformUserRepository>();
            services.AddTransient<IPlatformRepository, PlatformRepository>();
            services.AddTransient<ITenantRepository, TenantRepository>();
            services.AddTransient<IReleaseRepository, ReleaseRepository>();
            services.AddTransient<ITemplateRepository, TemplateRepository>();
            services.AddTransient<IHistoryDatabaseRepository, HistoryDatabaseRepository>();
            services.AddTransient<IHistoryStorageRepository, HistoryStorageRepository>();
            services.AddTransient<IModuleRepository, ModuleRepository>();
            services.AddTransient<ISettingRepository, SettingRepository>();

            services.AddHostedService<QueuedHostedService>();
            services.AddScoped<IRedisHelper, RedisHelper>();
            services.AddScoped<ICacheHelper, CacheHelper>();
            services.AddScoped<IOrganizationHelper, OrganizationHelper>();
            services.AddScoped<IPublishHelper, PublishHelper>();
            services.AddScoped<IWebSocketHelper, WebSocketHelper>();
            services.TryAddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.TryAddSingleton<IPublish, Publish>();
            services.AddTransient<IUnifiedStorage, UnifiedStorage>();
            services.AddScoped<IMigrationHelper, MigrationHelper>();
        }
    }
}