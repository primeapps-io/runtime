
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

			services.AddMvc(options =>
            {

                options.CacheProfiles.Add("Nocache",
                    new CacheProfile()
                    {
                        Location = ResponseCacheLocation.None,
                        NoStore = true,
                    });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
