using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Microsoft.AspNetCore.HttpOverrides;
using Amazon.Runtime;
using Amazon;
using Amazon.S3;
using Microsoft.AspNetCore.Http;

namespace PrimeApps.Auth
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

            //Configure Identity
            IdentityConfiguration(services, Configuration);

            //Configure Authentication
            AuthConfiguration(services, Configuration);

            services.AddMvc()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization()
                .AddJsonOptions(opt =>
                {
                    opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    opt.SerializerSettings.ContractResolver = new DefaultContractResolver()
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy(),
                    };
                    opt.SerializerSettings.Converters.Add(new StringEnumConverter());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddLocalization(options => options.ResourcesPath = "Resources");


            var storageUrl = Configuration.GetValue("AppSettings:StorageUrl", string.Empty);

            if (!string.IsNullOrEmpty(storageUrl))
            {
                Environment.SetEnvironmentVariable("AWS_ENABLE_ENDPOINT_DISCOVERY", "false");
                var awsOptions = Configuration.GetAWSOptions();
                awsOptions.DefaultClientConfig.RegionEndpoint = RegionEndpoint.EUWest1;
                awsOptions.DefaultClientConfig.ServiceURL = storageUrl;
                var storageAccessKey = Configuration.GetValue("AppSettings:StorageAccessKey", string.Empty);
                var storageSecretKey = Configuration.GetValue("AppSettings:StorageSecretKey", string.Empty);
                awsOptions.Credentials = new BasicAWSCredentials(storageAccessKey, storageSecretKey);
                services.AddDefaultAWSOptions(awsOptions);
                services.AddAWSService<IAmazonS3>();
            }

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //InitializeDatabase(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            var forwardHeaders = Configuration.GetValue("AppSettings:ForwardHeaders", string.Empty);

            if (!string.IsNullOrEmpty(forwardHeaders) && bool.Parse(forwardHeaders))
            {
                var fordwardedHeaderOptions = new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                };

                fordwardedHeaderOptions.KnownNetworks.Clear();
                fordwardedHeaderOptions.KnownProxies.Clear();

                app.UseForwardedHeaders(fordwardedHeaderOptions);
            }

            var httpsRedirection = Configuration.GetValue("AppSettings:HttpsRedirection", string.Empty);

            if (!string.IsNullOrEmpty(httpsRedirection) && bool.Parse(httpsRedirection))
            {
                app.UseHsts().UseHttpsRedirection();
            }

            var supportedCultures = new[]
            {
                new CultureInfo("tr"),
                new CultureInfo("tr-TR"),
                new CultureInfo("en"),
                new CultureInfo("en-US")
            };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });

            app.UseCors(cors =>
                cors
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin()
            );

            app.Use(async (ctx, next) =>
            {
                if (!string.IsNullOrEmpty(httpsRedirection) && bool.Parse(httpsRedirection))
                    ctx.Request.Scheme = "https";
                else
                    ctx.Request.Scheme = "http";

                ctx.Response.Headers.Add("Content-Security-Policy", "default-src 'self' * 'unsafe-inline' 'unsafe-eval' data:");
                await next();
            });

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Strict,
            });

            app.UseStaticFiles();
            app.UseIdentityServer();
            app.UseMvcWithDefaultRoute();

            /*app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}"
				);

				routes.MapRoute(
					name: "DefaultApi",
					template: "api/{controller}/{id}"
				);
			});*/
        }
    }
}