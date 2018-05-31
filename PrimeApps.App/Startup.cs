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
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using PrimeApps.App.ActionFilters;
using PrimeApps.App.Storage;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Web;

namespace PrimeApps.App
{
    public partial class Startup
    {
        public IHostingEnvironment HostingEnvironment { get; }
        public IConfiguration Configuration { get; }

        public static string PublicClientId { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940

        public Startup(IHostingEnvironment env)
        {
            PublicClientId = "self";

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
            /*GlobalConfiguration.Configuration.UsePostgreSqlStorage(ConfigurationManager.ConnectionStrings["HangfireConnection"].ConnectionString);
			services.AddHangfire(config => config.UsePostgreSqlStorage(ConfigurationManager.ConnectionStrings["HangfireConnection"].ConnectionString));*/


            var hangfireStorage = new PostgreSqlStorage(ConfigurationManager.ConnectionStrings["HangfireConnection"].ConnectionString);
            Hangfire.GlobalConfiguration.Configuration.UseStorage(hangfireStorage);
            services.AddHangfire(x => x.UseStorage(hangfireStorage));

            //Register DI
            DIRegister(services, Configuration);

            /*services.Configure<MvcOptions>(options =>
			{
				options.Filters.Add(new RequireHttpsAttribute());
			});*/
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


            services.AddMvc(options =>
            {

                options.CacheProfiles.Add("Nocache",
                    new CacheProfile()
                    {
                        Location = ResponseCacheLocation.None,
                        NoStore = true,
                    });
            })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(opt =>
                {
                    #region SnakeCaseAttribute
                    opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    opt.SerializerSettings.ContractResolver = new DefaultContractResolver()
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy(),
                    };
                    opt.SerializerSettings.Converters.Add(new StringEnumConverter());
                    #endregion
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
            awsOptions.DefaultClientConfig.ServiceURL = ConfigurationManager.ConnectionStrings["AzureStorageConnection"].ConnectionString;
            //awsOptions.Credentials = new EnvironmentVariablesAWSCredentials(); // For futures usage!!! Getting credentials from docker environmental variables.
            awsOptions.Credentials = new BasicAWSCredentials(
                ConfigurationManager.AppSettings.Get("AzureStorageAccessKey"),
                ConfigurationManager.AppSettings.Get("AzureStorageSecretKey"));
            services.AddDefaultAWSOptions(awsOptions);
            services.AddAWSService<IAmazonS3>();
            services.AddTransient<IUnifiedStorage, UnifiedStorage>();
            AuthConfiguration(services, Configuration);


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            /*app.Use(async (context, next) =>
            {
                if (context.Request.QueryString.HasValue)
                {
                    if (!string.IsNullOrWhiteSpace(context.Request.Headers["Authorization"]))
                    {
                        var queryString = HttpUtility.ParseQueryString(context.Request.QueryString.Value);
                        string token = queryString.Get("access_token");

                        if (!string.IsNullOrWhiteSpace(token))
                        {
                            context.Request.Headers.Add("Authorization", new[] { string.Format("Bearer {0}", token) });
                        }
                    }
                }
                // Call the next delegate/middleware in the pipeline
                await next.Invoke();
            });*/

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
            //app.UseHttpsRedirection();
            app.UseWebOptimizer();



            /// Must always stay before static files middleware.

            /*
			 * In ASP.NET, static files are stored in various directories and referenced in the views.
			 * In ASP.NET Core, static files are stored in the "web root" (<content root>/wwwroot), unless configured otherwise.
			 * The files are loaded into the request pipeline by invoking the UseStaticFiles
			 */
            app.UseStaticFiles();

            app.UseAuthentication();

            //app.UseMyMiddleware();
            //app.UseIdentity();

            app.UseCors(cors =>
              cors
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin()
            );

            JobConfiguration(app);

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

            /*app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });*/
        }
    }
}
