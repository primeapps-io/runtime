using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Auth.Data;
using PrimeApps.Auth.Models;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using IdentityServer4.Services;
using System.Reflection;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Repositories;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace PrimeApps.Auth
{
	public class Startup
	{
		public IConfiguration Configuration { get; }
		public IHostingEnvironment Environment { get; }


		public Startup(IConfiguration configuration, IHostingEnvironment environment)
		{
			Configuration = configuration;
			Environment = environment;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));
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
						opt.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"),
							sql => sql.MigrationsAssembly(migrationsAssembly));
				})
				// this adds the operational data from DB (codes, tokens, consents)
				.AddOperationalStore(options =>
				{
					options.ConfigureDbContext = opt =>
						opt.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"),
							sql => sql.MigrationsAssembly(migrationsAssembly));

					// this enables automatic token cleanup. this is optional.
					options.EnableTokenCleanup = true;
					options.TokenCleanupInterval = 3600; //3600 (1 hour)
				})
				.AddAspNetIdentity<ApplicationUser>()
				.AddProfileService<CustomProfileService>();
				//.AddRedirectUriValidator<CustomRedirectUriValidator>();

			if (Environment.IsDevelopment())
			{
				builder.AddDeveloperSigningCredential();
			}
			else
			{
				throw new Exception("need to configure key material");
			}

			services.AddAuthentication()
				.AddOpenIdConnect("aad", "Azure AD", options =>
				{
					options.Authority = "https://login.microsoftonline.com/common/";
					options.ClientId = "7697cae4-0291-4449-8046-7b1cae642982";
					options.GetClaimsFromUserInfoEndpoint = true;
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuer = false,

					};

					options.Events = new OpenIdConnectEvents
					{
						OnAuthorizationCodeReceived = async ctx =>
						{
							var request = ctx.HttpContext.Request;
							var currentUri = UriHelper.BuildAbsolute(request.Scheme, request.Host, request.PathBase, request.Path);
							var credential = new ClientCredential(ctx.Options.ClientId, ctx.Options.ClientSecret);

							var distributedCache = ctx.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();
							string userId = ctx.Principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

							/*var cache = new AdalDistributedTokenCache(distributedCache, userId);

							var authContext = new AuthenticationContext(ctx.Options.Authority, cache);

							var result = await authContext.AcquireTokenByAuthorizationCodeAsync(
								ctx.ProtocolMessage.Code, new Uri(currentUri), credential, ctx.Options.Resource);

							ctx.HandleCodeRedemption(result.AccessToken, result.IdToken);*/
						}
					};
				})
				.AddGoogle(options =>
				{
					options.ClientId = "708996912208-9m4dkjb5hscn7cjrn5u0r4tbgkbj1fko.apps.googleusercontent.com";
					options.ClientSecret = "wdfPY6t8H8cecgjlxud__4Gh";
				});

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
				app.UseExceptionHandler("/Home/Error");
				app.UseHttpsRedirection();
				app.UseHsts();
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
	}
}
