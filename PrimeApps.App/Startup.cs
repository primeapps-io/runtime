using Amazon.Runtime;
using Amazon.S3;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using PrimeApps.App.Storage;
using PrimeApps.Model.Repositories.Interfaces;
using System.Globalization;

namespace PrimeApps.App
{
    public partial class Startup
    {
        public IHostingEnvironment HostingEnvironment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            Configuration = builder;
            HostingEnvironment = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var hangfireStorage = new PostgreSqlStorage(Configuration.GetConnectionString("PlatformDBConnection"));
            GlobalConfiguration.Configuration.UseStorage(hangfireStorage);
            services.AddHangfire(x => x.UseStorage(hangfireStorage));

            //Register DI
            DIRegister(services, Configuration);

            services.AddDirectoryBrowser();

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("tr-TR")
                };
                options.DefaultRequestCulture = new RequestCulture("tr-TR", "tr-TR");

                // You must explicitly state which cultures your application supports.
                // These are the cultures the app supports for formatting 
                // numbers, dates, etc.

                options.SupportedCultures = supportedCultures;

                // These are the cultures the app supports for UI strings, 
                // i.e. we have localized resources for.

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
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
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
                    opts =>
                    {
                        opts.ResourcesPath = "Localization";
                    })
                .AddDataAnnotationsLocalization();

            RegisterBundle(services);

            var awsOptions = Configuration.GetAWSOptions();
            awsOptions.DefaultClientConfig.ServiceURL = Configuration.GetConnectionString("StorageConnection");
            awsOptions.Credentials = new BasicAWSCredentials(
                Configuration.GetSection("AppSettings")["StorageAccessKey"],
                Configuration.GetSection("AppSettings")["StorageSecretKey"]);
            services.AddDefaultAWSOptions(awsOptions);
            services.AddAWSService<IAmazonS3>();
            services.AddTransient<IUnifiedStorage, UnifiedStorage>();

            AuthConfiguration(services, Configuration, HostingEnvironment);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                //app.UseExceptionHandler("/Home/Error");
                app.UseHttpsRedirection();
                app.UseHsts();
            }

            app.UseHangfireDashboard();
            app.UseWebOptimizer();
            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseCors(cors =>
              cors
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin()
            );

            JobConfiguration(app, Configuration);

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
