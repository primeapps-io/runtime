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
using Newtonsoft.Json.Serialization;
using PrimeApps.App.ActionFilters;
using System.Configuration;
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
            var hangfireStorage = new PostgreSqlStorage(ConfigurationManager.ConnectionStrings["HangfireConnection"].ConnectionString);
            Hangfire.GlobalConfiguration.Configuration.UseStorage(hangfireStorage);
            services.AddHangfire(x => x.UseStorage(hangfireStorage));

            //Register DI
            DIRegister(services, Configuration);

            /*services.Configure<MvcOptions>(options =>
			{
				options.Filters.Add(new RequireHttpsAttribute());
			});*/

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
                    #endregion
                })
                .AddViewLocalization(
                        LanguageViewLocationExpanderFormat.Suffix,
                        opts =>
                        {
                            opts.ResourcesPath = "Localization";
                        })

                .AddDataAnnotationsLocalization();

			services.AddWebOptimizer(pipeline =>
			{
				pipeline.AddJavaScriptBundle("/scripts/bundles-js/auth.js",
					"/scripts/vendor/jquery.js",
					"/scripts/vendor/jquery-maskedinput.js",
					"/scripts/vendor/sweetalert.js",
					"/scripts/vendor/spin.js",
					"/scripts/vendor/ladda.js");

				pipeline.AddJavaScriptBundle("/scripts/bundles-js/vovoc.js",
					"/scripts/app.js",
					"/scripts/interceptors.js",
					"/scripts/routes.js",
					"/scripts/config.js",
					"/scripts/constants.js",
					"/scripts/utils.js",
					"/scripts/directives.js",
					"/scripts/filters.js",
					"/views/authService.js",
					"/views/appController.js",
					"/views/appService.js",
					"/views/app/crmController.js",
					"/views/app/note/noteService.js",
					"/views/app/note/noteDirective.js",
					"/views/app/documents/documentService.js",
					"/views/app/documents/documentDirective.js",
					"/views/app/module/moduleService.js",
					"/views/setup/help/helpService.js",
					"/views/setup/setupController.js",
					"/views/setup/payment/paymentService.js",
					"/views/setup/payment/paymentDirective.js",
					"/views/setup/workgroups/workgroupService.js",
					"/views/setup/messaging/messagingService.js",
					"/views/app/payment/paymentFormController.js",
					"/views/app/join/joinController.js",
					"/views/app/phone/sipPhoneController.js");

				pipeline.AddJavaScriptBundle("/scripts/bundles-js/vendor.js",
					"/scripts/vendor/angular.js",
					"/scripts/vendor/angular-ui-router.js",
					"/scripts/vendor/ocLazyLoad.js",
					"/scripts/vendor/angular-cookies.js",
					"/scripts/vendor/angular-translate.js",
					"/scripts/vendor/angular-animate.js",
					"/scripts/vendor/angular-sanitize.js",
					"/scripts/vendor/angular-strap.js",
					"/scripts/vendor/angular-strap.tpl.js",
					"/scripts/vendor/angular-ui-bootstrap-custom.js",
					"/scripts/vendor/angular-ui-bootstrap-custom-tpls.js",
					"/scripts/vendor/angular-xeditable.js",
					"/scripts/vendor/angular-ladda.js",
					"/scripts/vendor/angular-ui-utils.js",
					"/scripts/vendor/angular-ui-tinymce.js",
					"/scripts/vendor/mentio/mentio.js",
					"/scripts/vendor/ng-table.js",
					"/scripts/vendor/spin.js",
					"/scripts/vendor/ladda.js",
					"/scripts/vendor/moment.js",
					"/scripts/vendor/es5-shim.js",
					"/scripts/vendor/es5-sham.js",
					"/scripts/vendor/angular-file-upload.js",
					"/scripts/vendor/angular-bootstrap-show-errors.js",
					"/scripts/vendor/ngToast.js",
					"/scripts/vendor/angular-block-ui.js",
					"/scripts/vendor/angular-touch.js",
					"/scripts/vendor/ng-sortable.js",
					"/scripts/vendor/file-saver.js",
					"/scripts/vendor/ng-img-crop.js",
					"/scripts/vendor/angular-images-resizer.js",
					"/scripts/vendor/angular-ui-tree.js",
					"/scripts/vendor/plupload.full.js",
					"/scripts/vendor/angular-plupload.js",
					"/scripts/vendor/ng-tags-input.js",
					"/scripts/vendor/angular-ui-mask.js",
					"/scripts/vendor/powerbi.js",
					"/scripts/vendor/ace/ace.js",
					"/scripts/vendor/angular-ui-ace.js",
					"/scripts/vendor/ace/ext-language_tools.js",
					"/scripts/vendor/angular-ui-select.js",
					"/scripts/vendor/angular-resizable.js",
					"/scripts/vendor/clipboard.js",
					"/scripts/vendor/angular-translate-extentions.js",
					"/scripts/vendor/angular-dynamic-locale.js",
					"/scripts/vendor/locales/moment-locales.js",
					"/scripts/vendor/angular-slider.js",
					"/scripts/vendor/angular-bootstrap-calendar-tpls.js",
					"/scripts/vendor/dragular.js",
					"/scripts/vendor/angucomplete-alt-custom.js",
					"/scripts/vendor/ngclipboard.js",
					"/scripts/vendor/moment-business-days.js",
					"/scripts/vendor/moment-weekdaysin.js");

				pipeline.AddCssBundle("/styles/bundles-css/auth.css",
					"/styles/vendor/bootstrap.css",
					"/styles/vendor/flaticon.css",
					"/styles/vendor/ladda-themeless.css",
					"/styles/vendor/font-awesome.css");

				pipeline.AddCssBundle("/styles/bundles-css/vendor.css",
					"/styles/vendor/angular-block-ui.css",
					"/styles/vendor/angular-bootstrap-calendar.css",
					"/styles/vendor/angular-motion.css",
					"/styles/vendor/angular-resizable.css",
					"/styles/vendor/angular-ui-tree.css",
					"/styles/vendor/bootstrap-additions.css",
					"/styles/vendor/dragular.css",
					"/styles/vendor/flaticon.css",
					"/styles/vendor/font-awesome.css",
					"/styles/vendor/ladda-themeless.css",
					"/styles/vendor/ng-table.css",
					"/styles/vendor/ng-tags-input.bootstrap.css",
					"/styles/vendor/ng-tags-input.css",
					"/styles/vendor/ngToast.css",
					"/styles/vendor/select.css",
					"/styles/vendor/xeditable.css",
					"/styles/ui.css");

				pipeline.AddCssBundle("/styles/bundles-css/app.css",
					"/styles/app.css");
			});

			var awsOptions = Configuration.GetAWSOptions();
            awsOptions.DefaultClientConfig.ServiceURL = ConfigurationManager.ConnectionStrings["AzureStorageConnection"].ConnectionString;
            //awsOptions.Credentials = new EnvironmentVariablesAWSCredentials(); // For futures usage!!! Getting credentials from docker environmental variables.
            awsOptions.Credentials = new BasicAWSCredentials(
                ConfigurationManager.AppSettings.Get("AzureStorageAccessKey"),
                ConfigurationManager.AppSettings.Get("AzureStorageSecretKey"));
            services.AddDefaultAWSOptions(awsOptions);
            services.AddAWSService<IAmazonS3>();

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
            }



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

			app.UseCors("AllowAll");

            JobConfiguration(app);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Auth}/{action=Test}/{id?}"
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
