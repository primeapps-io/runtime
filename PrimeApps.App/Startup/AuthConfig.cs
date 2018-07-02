using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PrimeApps.App
{
	public partial class Startup
	{
		public static void AuthConfiguration(IServiceCollection services, IConfiguration configuration, IHostingEnvironment env)
		{
			JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

			services.AddMvcCore()
				.AddAuthorization()
				.AddJsonFormatters();

			services.AddAuthentication("Bearer")
				.AddIdentityServerAuthentication(options =>
				{
					/*if (env.IsDevelopment())
					{
						if (int.Parse(configuration.GetSection("AppSettings")["DevelopmentApp"]) == 1)
						{
							#region CRM için kullanılacak bilgiler
							options.Authority = "http://localhost:5002";
							#endregion
						}
						else if (int.Parse(configuration.GetSection("AppSettings")["DevelopmentApp"]) == 4)
						{
							#region İK için kullanılacak bilgiler
							options.Authority = "http://localhost:5004";
							#endregion
						}
					}
					else
					{
						options.Authority = configuration.GetSection("AppSettings")["DevelopmentApp"];
					}*/
					options.Authority = "http://localhost:5002";
					options.RequireHttpsMetadata = false;
					options.ApiName = "api1";
					/*options.Events = new OpenIdConnectEvents
					{

						OnRedirectToIdentityProvider = n =>
						{
							var applicationRepository = (IApplicationRepository)n.HttpContext.RequestServices.GetService(typeof(IApplicationRepository));
							var appInfo = applicationRepository.Get(n.Request.Host.Value);
							n.Options.Authority = n.Request.Scheme + "://" + appInfo.Setting.AuthDomain;
							n.ProtocolMessage.RequestUri = n.Request.Scheme + "://" + appInfo.Setting.AuthDomain;
							return Task.FromResult(0);
						}
					};*/
				});

			services.AddAuthentication(options =>
			{
				options.DefaultScheme = "Cookies";
				options.DefaultChallengeScheme = "oidc";
			})
				.AddCookie("Cookies")
				.AddOpenIdConnect("oidc", options =>
				{
					options.TokenValidationParameters.NameClaimType = "email";
					options.SignInScheme = "Cookies";

					/*if (env.IsDevelopment())
					{
						if (int.Parse(configuration.GetSection("AppSettings")["DevelopmentApp"]) == 1)
						{
							#region CRM için kullanılacak bilgiler
							options.Authority = "http://localhost:5002";
							options.ClientId = "ofisim.crm";
							#endregion
						}
						else if (int.Parse(configuration.GetSection("AppSettings")["DevelopmentApp"]) == 4)
						{
							#region İK için kullanılacak bilgiler
							options.Authority = "http://localhost:5004";
							options.ClientId = "ofisim.ik";
							#endregion
						}
					}
					else
					{
						options.Authority = configuration.GetSection("AppSettings")["DevelopmentApp"];
					}*/

					options.Authority = "http://localhost:5002";
					options.ClientId = "primeapps.mvc";

					options.RequireHttpsMetadata = false;
					options.ClientSecret = "secret";
					options.ResponseType = "code id_token";

					options.SaveTokens = true;
					options.GetClaimsFromUserInfoEndpoint = true;

					options.Scope.Add("api1");
					options.Scope.Add("email");

					options.Events = new OpenIdConnectEvents
					{
						/*OnRedirectToIdentityProvider = n =>
						{
							var applicationRepository = (IApplicationRepository)n.HttpContext.RequestServices.GetService(typeof(IApplicationRepository));
							var appInfo = applicationRepository.Get(n.Request.Host.Value);
							var newMeta = new Uri(n.Options.MetadataAddress);

							n.Options.ClientId = "ofisim." + appInfo.Name;
							n.Options.Authority = n.Request.Scheme + "://" + appInfo.Setting.AuthDomain;
							n.Options.MetadataAddress = newMeta.Scheme + "://" + appInfo.Setting.AuthDomain + newMeta.AbsolutePath;
							n.ProtocolMessage.RequestUri = n.Request.Scheme + "://" + appInfo.Setting.Domain;
							n.ProtocolMessage.IssuerAddress = n.Request.Scheme + "://" + appInfo.Setting.AuthDomain + "/connect/authorize";
							n.ProtocolMessage.ClientId = "ofisim." + appInfo.Name;
							n.ProtocolMessage.RedirectUri = n.Request.Scheme + "://" + appInfo.Setting.Domain + "/signin-oidc";

							var host = n.HttpContext.Request.Host.Host;

							var disco = await DiscoveryClient.GetAsync(n.Request.Scheme + "://" + appInfo.Setting.AuthDomain);
							var authorizeUrl = new RequestUrl(n.ProtocolMessage.IssuerAddress).CreateAuthorizeUrl(
								clientId: "ofisim." + appInfo.Name,
								responseType: "code id_token",
								scope: "openid profile api1 email",
								redirectUri: n.Request.Scheme + "://" + appInfo.Setting.Domain + "/signin-oidc",
								state: CryptoRandom.CreateUniqueId(), //"random_state",
								nonce: CryptoRandom.CreateUniqueId(), //"random_nonce",
								responseMode: "form_post",
								acrValues: appInfo.Setting.Domain);


							return Task.FromResult(0);
						},
						OnAuthorizationCodeReceived = async ctx =>
						{
							var request = ctx.HttpContext.Request;
							var currentUri = UriHelper.BuildAbsolute(request.Scheme, request.Host, request.PathBase, request.Path);
							var credential = new ClientCredential(ctx.Options.ClientId, ctx.Options.ClientSecret);

							var distributedCache = ctx.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();
							string userId = ctx.Principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

							var cache = new AdalDistributedTokenCache(distributedCache, userId);

							var authContext = new AuthenticationContext(ctx.Options.Authority, cache);

							var result = await authContext.AcquireTokenByAuthorizationCodeAsync(
								ctx.ProtocolMessage.Code, new Uri(currentUri), credential, ctx.Options.Resource);

							ctx.HandleCodeRedemption(result.AccessToken, result.IdToken);
						}*/
						/*OnTokenValidated = async ctx =>
						{
							//Get user's immutable object id from claims that came from Azure AD
							string oid = ctx.Principal.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");

							//Get EF context
							/*var db = ctx.HttpContext.RequestServices.GetRequiredService<AuthorizationDbContext>();

							//Check is user a super admin
							bool isSuperAdmin = await db.SuperAdmins.AnyAsync(a => a.ObjectId == oid);
							if (isSuperAdmin)
							{
								//Add claim if they are
								var claims = new List<Claim>
											{
												new Claim(ClaimTypes.Role, "superadmin")
											};
								var appIdentity = new ClaimsIdentity(claims);

								ctx.Principal.AddIdentity(appIdentity);
						},
						OnUserInformationReceived = context =>
						{
							var a = context.ProtocolMessage.AccessToken;
							return Task.CompletedTask;
						},*/
					};
					//options.Scope.Add("offline_access");
				});
		}
	}
}
