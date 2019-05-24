﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.App.Helpers;
using System.IdentityModel.Tokens.Jwt;
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
                    var authenticationServerURL = configuration.GetValue("AppSettings:AuthenticationServerURL", string.Empty);
                    if (!string.IsNullOrEmpty(authenticationServerURL))
                    {
                        options.Authority = authenticationServerURL;
                    }

                    var httpsRedirection = configuration.GetValue("AppSettings:HttpsRedirection", string.Empty);
                    if (!string.IsNullOrEmpty(httpsRedirection))
                    {
                        options.RequireHttpsMetadata = bool.Parse(httpsRedirection);
                    }

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
                    var authenticationServerURL = configuration.GetValue("AppSettings:AuthenticationServerURL", string.Empty);
                    if (!string.IsNullOrEmpty(authenticationServerURL))
                    {
                        options.Authority = authenticationServerURL;
                    }

                    var clientId = configuration.GetValue("AppSettings:ClientId", string.Empty);
                    if (!string.IsNullOrEmpty(clientId))
                    {
                        options.ClientId = clientId;
                    }

                    var clientSecret = configuration.GetValue("AppSettings:ClientSecret", string.Empty);
                    if (!string.IsNullOrEmpty(clientSecret))
                    {
                        options.ClientSecret = clientSecret;
                    }

                    var httpsRedirection = configuration.GetValue("AppSettings:HttpsRedirection", string.Empty);
                    if (!string.IsNullOrEmpty(httpsRedirection))
                    {
                        options.RequireHttpsMetadata = bool.Parse(httpsRedirection);
                    }

                    options.ResponseType = "code id_token";
                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.Scope.Add("api1");
                    options.Scope.Add("email");
                    options.Events.OnRedirectToIdentityProvider = ctx =>
                    {
                        ctx.HttpContext.Request.Query.TryGetValue("preview", out var preview);
                        if (!string.IsNullOrEmpty(preview))
                        {
                            var previewApp = AppHelper.GetPreviewApp(preview);
                            if (!string.IsNullOrEmpty(previewApp))
                            {
                                if (previewApp.Contains("app"))
                                {
                                    var appId = previewApp.Split("app_id=")[1];
                                    ctx.ProtocolMessage.SetParameter("preview_app_id", appId);
                                }
                            }
                        }
                        return Task.CompletedTask;
                    };
                    options.Events.OnRemoteFailure = context =>
                    {
                        if (context.Failure.Message.Contains("Correlation failed"))
                            context.Response.Redirect("/");
                        else
                            context.Response.Redirect("/Error");

                        context.HandleResponse();

                        return Task.CompletedTask;
                    };
                });
        }
    }
}