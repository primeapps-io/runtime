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
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddWebOptimizer(pipeline =>
            {
                pipeline.AddJavaScriptBundle("/scripts/bundles-js/auth.js",
                    "scripts/vendor/jquery.js",
                    "scripts/vendor/jquery-maskedinput.js",
                    "scripts/vendor/sweetalert.js",
                    "scripts/vendor/spin.js",
                    "scripts/vendor/ladda.js");

                pipeline.AddCssBundle("/styles/bundles-css/auth.css",
                    "styles/vendor/bootstrap.css",
                    "styles/vendor/flaticon.css",
                    "styles/vendor/ladda-themeless.css",
                    "styles/vendor/font-awesome.css");
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
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

            var supportedCultures = new[]
            {
                new CultureInfo("tr"),
                new CultureInfo("tr-TR"),
                new CultureInfo("en"),
                new CultureInfo("en-US")
            };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("tr"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });


            app.Use(async (ctx, next) =>
            {
                ctx.Response.Headers.Add("Content-Security-Policy", "default-src 'self' * 'unsafe-inline' 'unsafe-eval' data:");
                await next();
            });

            app.UseWebOptimizer();
            app.UseStaticFiles();
            app.UseIdentityServer();
            app.UseMvcWithDefaultRoute();
        }
    }
}
