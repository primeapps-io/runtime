using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories;
using PrimeApps.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PrimeApps.App
{
    public partial class Startup
    {
		public static void RegisterAuth(IServiceCollection services)
		{
			var clientId = ConfigurationManager.AppSettings["ida:ClientID"];
			var authority = "https://login.microsoftonline.com/common/";

			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddCookie("Cookieauth", options =>
				{
					options.LoginPath = new PathString("/auth/login");
					options.ExpireTimeSpan = TimeSpan.FromDays(14);
				})
				.AddOpenIdConnect(options =>
				{
					options.Authority = authority;
					options.ClientId = clientId;
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuer = false
					};
					options.Events = new OpenIdConnectEvents
					{
						OnRedirectToIdentityProvider = ctx =>
						{
							var appBaseUrl = ctx.Request.Scheme + "://" + ctx.Request.Host + ctx.Request.PathBase;
							ctx.ProtocolMessage.RedirectUri = appBaseUrl + "/";
							ctx.ProtocolMessage.PostLogoutRedirectUri = appBaseUrl;
							return Task.FromResult(0);
						},
						OnAuthorizationCodeReceived = async ctx =>
						{
							var code = ctx.ProtocolMessage.Code;
							// Redeem auth code for access token and cache it for later use
							ctx.HttpContext.User = ctx.Principal;

							IAzureAdTokenService tokenService = (IAzureAdTokenService)ctx.HttpContext.RequestServices.GetService(typeof(IAzureAdTokenService));
							await tokenService.RedeemAuthCodeForAadGraph(ctx.ProtocolMessage.Code, ctx.Properties.Items[OpenIdConnectDefaults.RedirectUriForCodePropertiesKey]);

							// Notify the OIDC middleware that we already took care of code redemption.
							ctx.HandleCodeRedemption();
						},
						OnTokenValidated = async ctx =>
						{
							var issuer = ctx.SecurityToken.Issuer;
							var email = ctx.Principal.FindFirst(ClaimTypes.Name).Value;
							var name = ctx.Principal.FindFirst(ClaimTypes.GivenName).Value;
							var lastname = ctx.Principal.FindFirst(ClaimTypes.Surname).Value;

							PlatformUser user;
							using (var dbContext = new PlatformDBContext())
							{
								using (var platformUserRepository = new PlatformUserRepository(dbContext))
								{
									user = await platformUserRepository.GetUserByActiveDirectoryTenantEmail(email);


									if (user == null)
									{
										var resultControl = await platformUserRepository.IsEmailAvailable(email);

										if (resultControl == false)
											ctx.Response.Redirect("/Auth/Login?Error=notActive");
										else
											ctx.Response.Redirect("/Auth/Register?Email=" + email + "&Name=" + HttpUtility.UrlEncode(name) + "&Lastname=" + HttpUtility.UrlEncode(lastname) + "&OfficeSignIn=" + true);

										ctx.HandleResponse();
										return /*Task.FromResult(0)*/;
										//throw new SecurityTokenValidationException();
									}
								}
							}

							var currentNameClaim = ctx.Principal.FindFirst(ClaimTypes.Name);

							var claimsIdentity = (ClaimsIdentity)ctx.Principal.Identity;
							//add your custom claims here
							claimsIdentity.RemoveClaim(currentNameClaim);
							claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, user.Email));
							claimsIdentity.AddClaim(new Claim("user_id", user.Id.ToString()));
							claimsIdentity.AddClaim(new Claim("tenant_id", user.TenantId.ToString()));

							return /*Task.FromResult(0)*/;
						},
						OnAuthenticationFailed = ctx =>
						{
							ctx.Response.Redirect("/Auth/Login?ReturnUrl=/&Error=notFound");
							ctx.HandleResponse();

							return Task.FromResult(0);
						}

					};
				})
				.AddJwtBearer(options =>
				{
					var issuer = "0f!s!mJWT";
					var audienceId = ConfigurationManager.AppSettings["as:AudienceId"];

					//byte[] encodedBytes = System.Text.Encoding.Unicode.GetBytes(ConfigurationManager.AppSettings["as:AudienceSecret"]);
					//string audienceSecret = Convert.ToBase64String(encodedBytes);

					//var audienceSecret = TextEncodings.Base64Url.Decode(ConfigurationManager.AppSettings["as:AudienceSecret"]);

					options.RequireHttpsMetadata = false;
					options.SaveToken = true;

					options.TokenValidationParameters = new TokenValidationParameters()
					{
						ValidateIssuer = true,
						ValidateAudience = true,
						ValidateLifetime = true,
						ValidateIssuerSigningKey = true,

						ValidIssuer = issuer,
						ValidAudience = audienceId,
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["as:AudienceSecret"])) //Secret
					};

				});

		}
	}
}
