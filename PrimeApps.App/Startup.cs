using Amazon.Runtime;
using Amazon.S3;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using PrimeApps.App.Storage;
using System.Globalization;
using Microsoft.AspNetCore.HttpOverrides;
using Amazon;

namespace PrimeApps.App
{
    public partial class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //Register DI
            DIRegister(services, Configuration);

            //Configure Authentication
            AuthConfiguration(services, Configuration);

            var hangfireStorage = new PostgreSqlStorage(Configuration.GetConnectionString("PlatformDBConnection"));
            GlobalConfiguration.Configuration.UseStorage(hangfireStorage);
            services.AddHangfire(x => x.UseStorage(hangfireStorage));

            //Add Workflow service
            services.AddWorkflow(x => x.UsePostgreSQL(Configuration.GetConnectionString("PlatformDBConnection"), false, true));

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("tr-TR")
                };
                options.DefaultRequestCulture = new RequestCulture("tr-TR", "tr-TR");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    });
            });

            services.AddMvc(opt =>
                {
                    opt.CacheProfiles.Add("Nocache",
                        new CacheProfile()
                        {
                            Location = ResponseCacheLocation.None,
                            NoStore = true,
                        });
                })
                .AddWebApiConventions()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(opt =>
                {
                    opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    opt.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
                    opt.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    opt.SerializerSettings.DateParseHandling = DateParseHandling.None;
                    opt.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

                    opt.SerializerSettings.ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy(),
                    };
                    opt.SerializerSettings.Converters.Add(new StringEnumConverter());
                })
                .AddViewLocalization(
                    LanguageViewLocationExpanderFormat.Suffix,
                    opts => { opts.ResourcesPath = "Localization"; })
                .AddDataAnnotationsLocalization();

            var awsOptions = Configuration.GetAWSOptions();
            awsOptions.DefaultClientConfig.RegionEndpoint = RegionEndpoint.EUWest1;
            awsOptions.DefaultClientConfig.ServiceURL = Configuration.GetConnectionString("StorageConnection");
            awsOptions.Credentials = new BasicAWSCredentials(
                Configuration.GetSection("AppSettings")["StorageAccessKey"],
                Configuration.GetSection("AppSettings")["StorageSecretKey"]);
            services.AddDefaultAWSOptions(awsOptions);
            services.AddAWSService<IAmazonS3>();
            services.AddTransient<IUnifiedStorage, UnifiedStorage>();

            services.AddDistributedRedisCache(option => { option.Configuration = Configuration.GetConnectionString("RedisConnection"); });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            var enableHeaderForwarding = bool.Parse(Configuration.GetSection("AppSettings")["ForwardHeaders"]);
            var enableHttpsRedirection = bool.Parse(Configuration.GetSection("AppSettings")["HttpsRedirection"]);

            if (enableHeaderForwarding)
            {
                var fordwardedHeaderOptions = new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                };

                fordwardedHeaderOptions.KnownNetworks.Clear();
                fordwardedHeaderOptions.KnownProxies.Clear();

                app.UseForwardedHeaders(fordwardedHeaderOptions);
            }

            if (enableHttpsRedirection)
            {
                app.UseHsts().UseHttpsRedirection();
            }

            app.Use(async (ctx, next) =>
            {
                if (enableHttpsRedirection)
                    ctx.Request.Scheme = "https";
                else
                    ctx.Request.Scheme = "http";

                await next();
            });

            app.UseHangfireDashboard();
            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseCors(cors =>
                cors
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin()
            );

            JobConfiguration(app, Configuration);
            BpmConfiguration(app, Configuration);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}"
                );

                routes.MapRoute(
                    name: "DefaultApi",
                    template: "api/{controller}/{id}"
                );
            });
        }
    }
}