using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using PrimeApps.App.ActionFilters;
using PrimeApps.Model.Context;
using System.Globalization;
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
			//Register DI
			DIRegister(services, Configuration);

			services.AddLocalization(o => o.ResourcesPath = "Localization");

			services.Configure<MvcOptions>(options =>
			{
				options.Filters.Add(new RequireHttpsAttribute());
			});

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


			services.AddMvc()
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
				.AddJsonOptions(opt =>
				{
					#region SnakeCaseAttribute
					opt.SerializerSettings.ContractResolver = new DefaultContractResolver()
					{
						NamingStrategy = new SnakeCaseNamingStrategy()
					};
					#endregion
				}).AddViewLocalization(
				LanguageViewLocationExpanderFormat.Suffix,
				opts => { opts.ResourcesPath = "Localization"; })
				.AddDataAnnotationsLocalization();

			AuthConfiguration(services, Configuration);


			/*services.AddIdentity<PlatformUser, ApplicationRole>()
			   .AddUserStore<PlatformDBContext>()
			   .AddUserManager<ApplicationUserManager>()
			   .AddRoleManager<ApplicationUserRole>()
			   .AddEntityFrameworkStores<PlatformDBContext, int>()
			   .AddDefaultTokenProviders();*/

			/*services.AddIdentity<ApplicationUserManager, ApplicationUserRole>(options =>
			{
				options.Password.RequiredLength = 10;
				options.Password.RequireLowercase = false;
				options.Password.RequireUppercase = false;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireDigit = false;

				options.User.RequireUniqueEmail = true;
				options.User.AllowedUserNameCharacters = "";
			});*/

			

            // Add application services. DI
            //services.AddTransient<IProductRepository, ProductRepository>();

            //var clientId = ConfigurationManager.AppSettings["ida:ClientID"];
            //var appKey = ConfigurationManager.AppSettings["ida:Password"];
            //var authority = "https://login.microsoftonline.com/common/";
            //var graphResourceID = "https://graph.windows.net";
			
            /*services.AddMvc(options =>
            {

                options.CacheProfiles.Add("Nocache",
                    new CacheProfile()
                    {
                        Location = ResponseCacheLocation.None,
                        NoStore = true,
                    });
            });*/
            

            

            #region RequireHttpsAttribute
            services.Configure<MvcOptions>(options =>
                {
                    options.Filters.Add(new RequireHttps(Configuration));
                });
            #endregion

            RegisterBundle(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.Use(async (context, next) =>
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
            });

            app.UseAuthentication();

			BundleConfiguration(app, Configuration);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                //app.UseDatabaseErrorPage();
                //app.UseBrowserLink();
            }
            else
            {
                //app.UseExceptionHandler("/Home/Error");
            }
            /// Must always stay before static files middleware.
            app.UseHttpsRedirection();

            /*
			 * In ASP.NET, static files are stored in various directories and referenced in the views.
			 * In ASP.NET Core, static files are stored in the "web root" (<content root>/wwwroot), unless configured otherwise.
			 * The files are loaded into the request pipeline by invoking the UseStaticFiles
			 */
            app.UseStaticFiles();

            //app.UseIdentity();

            app.UseCors("AllowAll");

            JobConfiguration(app);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Auth}/{action=Login}/{id?}");
            });

            /*app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });*/
            app.UseHsts();
        }
    }
}
