using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace PrimeApps.Console
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
                    options.Authority = configuration.GetSection("AppSettings")["AuthenticationServerURL"];
                    options.RequireHttpsMetadata = bool.Parse(configuration.GetSection("AppSettings")["HttpsRedirection"]);
                    options.ApiName = "api1";
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
                    options.Authority = configuration.GetSection("AppSettings")["AuthenticationServerURL"];
                    options.ClientId = configuration.GetSection("AppSettings")["ClientId"];
                    options.ClientSecret = configuration.GetSection("AppSettings")["ClientSecret"];
                    options.RequireHttpsMetadata = bool.Parse(configuration.GetSection("AppSettings")["HttpsRedirection"]);
                    options.ResponseType = "code id_token";
                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.Scope.Add("api1");
                    options.Scope.Add("email");
                    options.RemoteAuthenticationTimeout = TimeSpan.FromSeconds(20);
                });
        }
    }
}
