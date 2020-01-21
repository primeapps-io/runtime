using System;
using System.Diagnostics;
using System.Globalization;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Hangfire;
using Hangfire.Redis;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using PrimeApps.Model.Context;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Services;

namespace PrimeApps.Studio
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
            
            //Redis connection
            var redisConnection = Configuration.GetConnectionString("RedisConnection");

            //Hangfire configuration
            var redisStorageOptions = new RedisStorageOptions
            {
                Prefix = "{studio}:",
                Db = 1
            };
            var hangfireStorage = new RedisStorage(redisConnection, redisStorageOptions);

            GlobalConfiguration.Configuration.UseStorage(hangfireStorage);
            services.AddHangfire(x => x.UseStorage(hangfireStorage));

            //Other configurations
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

            services.AddSingleton<ODataQueryStringFixer>();//For OData Filter Middlewares
            services.AddOData();
            services.AddODataQueryFilter();

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

            if (!string.IsNullOrEmpty(redisConnection))
                services.AddDistributedRedisCache(option => { option.Configuration = Configuration.GetConnectionString("RedisConnection"); });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var databaseContext = scope.ServiceProvider.GetRequiredService<StudioDBContext>();
                var queue = app.ApplicationServices.GetService<IBackgroundTaskQueue>();
                var context = app.ApplicationServices.GetService<IHttpContextAccessor>();
                var tracerHelper = app.ApplicationServices.GetService<IHistoryHelper>();

                var listener = databaseContext.GetService<DiagnosticSource>();
                (listener as DiagnosticListener).SubscribeWithAdapter(new CommandListener(queue, tracerHelper, context, Configuration));
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

            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            };

            app.UseODataQueryStringFixer(); //For OData Filter Middleware
            app.UseWebSockets(new WebSocketOptions() { KeepAliveInterval = TimeSpan.FromSeconds(10) });
            app.Use(async (ctx, next) =>
            {
                if (ctx.Request.Path == "/log_stream")
                {
                    if (ctx.WebSockets.IsWebSocketRequest)
                    {
                        var webSocketHelper = (IWebSocketHelper)ctx.RequestServices.GetService(typeof(IWebSocketHelper));

                        var wSocket = await ctx.WebSockets.AcceptWebSocketAsync();
                        await webSocketHelper.LogStream(ctx, wSocket);
                    }
                    else
                    {
                        ctx.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }
            });

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
                /*
                * These two option for odata controller.
                */
                routes.Select().Expand().Filter().OrderBy().MaxTop(null).Count();
                routes.EnableDependencyInjection();
            });
        }
    }
}