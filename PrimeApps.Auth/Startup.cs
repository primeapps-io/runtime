using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Auth.Data;
using PrimeApps.Auth.Models;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using IdentityServer4.Services;
using System.Reflection;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Repositories;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using System.Security.Claims;
using IdentityServer4;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.HttpOverrides;
using IdentityServer4.Configuration;
using PrimeApps.Auth.Services;

namespace PrimeApps.Auth
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            Configuration = builder;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(Configuration.GetConnectionString("AuthDBConnection")));
            services.AddDbContext<TenantDBContext>(options => options.UseNpgsql(Configuration.GetConnectionString("TenantDBConnection")));
            services.AddDbContext<PlatformDBContext>(options => options.UseNpgsql(Configuration.GetConnectionString("PlatformDBConnection")));
            services.AddScoped(p => new PlatformDBContext(p.GetService<DbContextOptions<PlatformDBContext>>()));

            services.AddSingleton(Configuration);

            services.AddIdentity<ApplicationUser, IdentityRole>(config =>
            {
                config.Password.RequiredLength = 6;
                config.Password.RequireLowercase = false;
                config.Password.RequireUppercase = false;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireDigit = false;
                config.User.RequireUniqueEmail = false;
                config.SignIn.RequireConfirmedEmail = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

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

            services.AddMvc()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization()
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
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);


            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddSingleton<IProfileService, CustomProfileService>();

            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            // Register Repositories
            foreach (var a in new string[] { "PrimeApps.Model" })
            {
                Assembly loadedAss = Assembly.Load(a);

                var allServices = loadedAss.GetTypes().Where(t =>
                                    t.GetTypeInfo().IsClass &&
                                    !t.GetTypeInfo().IsAbstract && t.GetTypeInfo().Name.EndsWith("Repository")).ToList();

                foreach (var type in allServices)
                {
                    var allInterfaces = type.GetInterfaces().Where(x => x.Name.EndsWith("Repository")).ToList();
                    var mainInterfaces = allInterfaces.Except
                            (allInterfaces.SelectMany(t => t.GetInterfaces()));
                    foreach (var itype in mainInterfaces)
                    {
                        if (allServices.Any(x => !x.Equals(type) && itype.IsAssignableFrom(x)))
                        {
                            throw new Exception("The " + itype.Name + " type has more than one implementations, please change your filter");
                        }
                        services.AddTransient(itype, type);
                    }
                }
            }

            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddTransient<IPlatformRepository, PlatformRepository>();
			services.AddTransient<IPlatformUserRepository, PlatformUserRepository>();
			services.AddTransient<IApplicationRepository, ApplicationRepository>();

            var builder = services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                })
                /*.AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryClients(Config.GetClients())
                .AddAspNetIdentity<ApplicationUser>()*/
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = opt =>
                        opt.UseNpgsql(Configuration.GetConnectionString("AuthDBConnection"),
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = opt =>
                        opt.UseNpgsql(Configuration.GetConnectionString("AuthDBConnection"),
                            sql => sql.MigrationsAssembly(migrationsAssembly));

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                    options.TokenCleanupInterval = 3600; //3600 (1 hour)
                })
                .AddAspNetIdentity<ApplicationUser>()
                .AddProfileService<CustomProfileService>()
                .AddRedirectUriValidator<CustomRedirectUriValidator>()
                .AddSigningCredential(LoadCertificate());

            services.AddAuthentication()
                .AddOpenIdConnect("aad", "Azure AD", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.SignOutScheme = IdentityServerConstants.SignoutScheme;

                    options.Authority = "https://login.microsoftonline.com/common/";
                    options.ClientId = "7697cae4-0291-4449-8046-7b1cae642982";
                    options.ClientSecret = "J2YHu8tqkM8YJh8zgSj8XP0eJpZlFKgshTehIe5ITvU=";
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.ResponseType = "code id_token";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };

                    options.Events = new OpenIdConnectEvents
                    {
                        OnAuthorizationCodeReceived = async ctx =>
                        {
                            /*HttpRequest request = ctx.HttpContext.Request;
							//We need to also specify the redirect URL used
							string currentUri = UriHelper.BuildAbsolute(request.Scheme, request.Host, request.PathBase, request.Path);
							//Credentials for app itself
							var credential = new ClientCredential(ctx.Options.ClientId, ctx.Options.ClientSecret);

							//Construct token cache
							ITokenCacheFactory cacheFactory = ctx.HttpContext.RequestServices.GetRequiredService<ITokenCacheFactory>();
							TokenCache cache = cacheFactory.CreateForUser(ctx.Principal);

							var authContext = new AuthenticationContext(ctx.Options.Authority, cache);

							//Get token for Microsoft Graph API using the authorization code
							string resource = "https://graph.microsoft.com";
							AuthenticationResult result = await authContext.AcquireTokenByAuthorizationCodeAsync(
								ctx.ProtocolMessage.Code, new Uri(currentUri), credential, resource);

							//Tell the OIDC middleware we got the tokens, it doesn't need to do anything
							ctx.HandleCodeRedemption(result.AccessToken, result.IdToken);*/
                            /*var claims = new List<Claim>
							{
								new Claim("validated_code", ctx.ProtocolMessage.Code)
							};

							var appIdentity = new ClaimsIdentity(claims);

							ctx.Principal.AddIdentity(appIdentity);
							ctx.HttpContext.User.AddIdentity(appIdentity);*/

                            /*HttpRequest request = ctx.HttpContext.Request;
							//We need to also specify the redirect URL used
							string currentUri = UriHelper.BuildAbsolute(request.Scheme, request.Host, request.PathBase, request.Path);
							//Credentials for app itself
							var credential = new ClientCredential(ctx.Options.ClientId, ctx.Options.ClientSecret);

							//Construct token cache
							ITokenCacheFactory cacheFactory = ctx.HttpContext.RequestServices.GetRequiredService<ITokenCacheFactory>();
							TokenCache cache = cacheFactory.CreateForUser(ctx.JwtSecurityToken.Claims.First(c => c.Type == "sub").Value);

							var authContext = new AuthenticationContext(ctx.Options.Authority, cache);

							//Get token for Microsoft Graph API using the authorization code
							string resource = "https://graph.microsoft.com";
							AuthenticationResult result = await authContext.AcquireTokenByAuthorizationCodeAsync(
								ctx.ProtocolMessage.Code, new Uri(currentUri), credential, resource);

							//Tell the OIDC middleware we got the tokens, it doesn't need to do anything
							//ctx.HandleCodeRedemption(result.AccessToken, result.IdToken);

							var claims = new List<Claim>
							{
								new Claim("validated_code", result.AccessToken)
							};

							var appIdentity = new ClaimsIdentity(claims);

							ctx.Principal.AddIdentity(appIdentity);
							ctx.HttpContext.User.AddIdentity(appIdentity);*/
                        }
                    };
                })
                /*.AddGoogle(options =>
				{
					options.ClientId = "708996912208-9m4dkjb5hscn7cjrn5u0r4tbgkbj1fko.apps.googleusercontent.com";
					options.ClientSecret = "wdfPY6t8H8cecgjlxud__4Gh";
				})
				.AddMicrosoftAccount(options => {
					options.ClientId = "9a44a984-84f8-417b-b39a-a312b87ddfea";
					options.SignInScheme = "Identity.External";
					options.ClientSecret = "aihswrSTL699=*^wXHDY42$";
				})*/;
            //dotnet ef migrations add InitialIdentityServerPersistedGrantDbMigration -c PersistedGrantDbContext -o Data/Migrations/IdentityServer/PersistedGrantDb
            //dotnet ef migrations add InitialIdentityServerConfigurationDbMigration -c ConfigurationDbContext -o Data/Migrations/IdentityServer/ConfigurationDb

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
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });

                app.UseExceptionHandler("/Home/Error");
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
                // Formatting numbers, dates, etc.
                SupportedCultures = supportedCultures,
                // UI strings that we have localized.
                SupportedUICultures = supportedCultures
            });


            app.Use(async (ctx, next) =>
            {
                ctx.Response.Headers.Add("Content-Security-Policy",
                                         "default-src 'self' * 'unsafe-inline' 'unsafe-eval' data:");
                await next();
            });

            app.UseWebOptimizer();
            //InitializeDatabase(app);

            app.UseStaticFiles();
            app.UseIdentityServer();
            app.UseMvcWithDefaultRoute();
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                if (!context.Clients.Any())
                {
                    foreach (var client in Config.GetClients())
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in Config.GetApiResources())
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }
        private X509Certificate2 LoadCertificate()
        {
            string location = Configuration.GetValue("AppSettings:AuthCertLocation", string.Empty);
            string exportKey = Configuration.GetValue("AppSettings:AuthCertExportKey", string.Empty);

            if (location == string.Empty) throw new ArgumentNullException("Authentication Certificate Location is not set!");

            return new X509Certificate2(location, exportKey, X509KeyStorageFlags.Exportable);
        }
    }
}
