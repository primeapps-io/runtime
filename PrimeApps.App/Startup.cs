
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PrimeApps.App.Configuration;

namespace PrimeApps.App
{
    public class Startup
    {
		public IHostingEnvironment HostingEnvironment { get; }
	    public IConfiguration Configuration { get; }


		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940

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
			/*services.AddLocalization(o => o.ResourcesPath = "Resources");
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
	        });*/

			// Add application services. DI
			//services.AddTransient<IProductRepository, ProductRepository>();

			services.AddAuthentication()
				.AddJwtBearer(cfg =>
				{
					cfg.RequireHttpsMetadata = false;
					cfg.SaveToken = true;

					cfg.TokenValidationParameters = new TokenValidationParameters()
					{
						ValidIssuer = Configuration["Tokens:Issuer"],
						ValidAudience = Configuration["Tokens:Issuer"],
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Tokens:Key"]))
					};

				});

			/*services.AddMvc(options =>
            {

                options.CacheProfiles.Add("Nocache",
                    new CacheProfile()
                    {
                        Location = ResponseCacheLocation.None,
                        NoStore = true,
                    });
            });*/
	        services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
	        BundleConfig.RegisterBundle(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
	        app.UseAuthentication();

	        BundleConfig.Configuration(app);
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

			/// This must always come after exception handler middleware.
			app.UseHsts();

        }
    }
}
