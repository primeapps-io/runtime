using IdentityServer4;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace PrimeApps.Auth
{
    public partial class Startup
    {
        public static void AuthConfiguration(IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication()
                .AddJwtBearer(jwt =>
                {
                    jwt.Authority = configuration.GetValue("AppSettings:AuthUrl", string.Empty);
                    jwt.Audience = "api1";
                    jwt.RequireHttpsMetadata = configuration.GetValue("AppSettings:AuthUrl", string.Empty).Contains("https");

                })
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
                });
        }
    }
}
