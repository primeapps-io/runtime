using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
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
		public static void AuthConfiguration(IServiceCollection services, IConfiguration configuration)
		{
			JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

			services.AddMvcCore()
				.AddAuthorization()
				.AddJsonFormatters();

			services.AddAuthentication("Bearer")
				.AddIdentityServerAuthentication(options =>
				{
					#region CRM için kullanılacak bilgiler
					options.Authority = "http://localhost:5002"; 
					#endregion

					#region İK için kullanılacak bilgiler
					//options.Authority = "http://localhost:5004"; 
					#endregion
					options.RequireHttpsMetadata = false;
					options.ApiName = "api1";
					options.Events = new OpenIdConnectEvents
					{
						
						OnRedirectToIdentityProvider = n =>
						{
							/*var applicationRepository = (IApplicationRepository)n.HttpContext.RequestServices.GetService(typeof(IApplicationRepository));
							var appInfo = applicationRepository.Get(n.Request.Host.Value);
							n.Options.Authority = n.Request.Scheme + "://" + appInfo.Setting.AuthDomain;*/
							return Task.FromResult(0);
						}
					};
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

					#region CRM için kullanılacak bilgiler
					options.Authority = "http://localhost:5002";
					options.ClientId = "ofisim.crm"; 
					#endregion

					#region İK için kullanılacak bilgiler
					//options.Authority = "http://localhost:5004";
					//options.ClientId = "ofisim.ik";
					#endregion
					options.RequireHttpsMetadata = false;
					options.ClientSecret = "secret";
					options.ResponseType = "code id_token";

					options.SaveTokens = true;
					options.GetClaimsFromUserInfoEndpoint = true;

					options.Scope.Add("api1");
					options.Scope.Add("email");

					options.Events = new OpenIdConnectEvents
					{
						OnRedirectToIdentityProvider = n =>
						{
							/*var applicationRepository = (IApplicationRepository)n.HttpContext.RequestServices.GetService(typeof(IApplicationRepository));
							var appInfo = applicationRepository.Get(n.Request.Host.Value);
							var newMeta = new Uri(n.Options.MetadataAddress);

							n.Options.ClientId = "ofisim." + appInfo.Name;
							n.Options.Authority = n.Request.Scheme + "://" + appInfo.Setting.AuthDomain;
							n.Options.MetadataAddress = newMeta.Scheme + "://" + appInfo.Setting.AuthDomain + newMeta.AbsolutePath;*/
							return Task.FromResult(0);
						},
						/*OnAuthorizationCodeReceived = async ctx =>
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
						},*/
						OnUserInformationReceived = context =>
						{
							var a = context.ProtocolMessage.AccessToken;
							return Task.CompletedTask;
						},
					};
					//options.Scope.Add("offline_access");
				});
		}
	}
}
