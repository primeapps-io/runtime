using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PrimeApps.App
{
    public partial class Startup
    {
	    public static void BundleConfiguration(IApplicationBuilder app, IConfiguration Configuration)
	    {
			var enableBundle = bool.Parse(ConfigurationManager.AppSettings["EnableBundle"]);


			/*if (enableBundle)*/
			    app.UseWebOptimizer();
		}

	    public static void RegisterBundle(IServiceCollection services)
	    {
			
		}
    }
}
