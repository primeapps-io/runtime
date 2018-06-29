// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace PrimeApps.Auth
{
    public class Config
    {
        // scopes define the resources in your system
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
				new IdentityResources.Email(),
                new IdentityResources.Profile(),
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("api1", "PrimeApps Api Auth")
            };
        }

        // clients want to access resources (aka scopes)
        public static IEnumerable<Client> GetClients()
        {
            // client credentials client
            return new List<Client>
            {
                new Client
                {
                    ClientId = "primeapps.client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    ClientSecrets = 
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = { "api1" }
                },

                // resource owner password grant client
                new Client
                {
                    ClientId = "primeapps.resource",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    ClientSecrets = 
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = { "api1" }
				},

                // OpenID Connect hybrid flow and client credentials client (MVC)
                new Client
                {
                    ClientId = "primeapps.mvc",
                    ClientName = "PrimeApps MVC Client",
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
					AllowRememberConsent = false, // Permission ı hatırlıyor
					AlwaysSendClientClaims = true,
					RequireConsent = false,

                    ClientSecrets = 
                    {
                        new Secret("secret".Sha256())
                    },

                    RedirectUris = { "http://localhost:5002/signin-oidc" },
                    PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "api1"
                    }
                },
				// OpenID Connect hybrid flow and client credentials client (MVC)
                new Client
				{
					ClientId = "ofisim.crm",
					ClientName = "Ofisim CRM MVC Client",
					AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
					AllowRememberConsent = false, // Permission ı hatırlıyor
					AlwaysSendClientClaims = true,
					RequireConsent = false,

					ClientSecrets =
					{
						new Secret("secret".Sha256())
					},

					RedirectUris = { "http://localhost:5001/signin-oidc" },
					PostLogoutRedirectUris = { "http://localhost:5001/signout-callback-oidc" },

					AllowedScopes =
					{
						IdentityServerConstants.StandardScopes.OpenId,
						IdentityServerConstants.StandardScopes.Profile,
						IdentityServerConstants.StandardScopes.Email,
						"api1"
					}
				},
				// OpenID Connect hybrid flow and client credentials client (MVC)
                new Client
				{
					ClientId = "ofisim.ik",
					ClientName = "Ofisim IK Client",
					AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
					AllowRememberConsent = false, // Permission ı hatırlıyor
					AlwaysSendClientClaims = true,
					RequireConsent = false,

					ClientSecrets =
					{
						new Secret("secret".Sha256())
					},

					RedirectUris = { "http://localhost:5003/signin-oidc" },
					PostLogoutRedirectUris = { "http://localhost:5003/signout-callback-oidc" },

					AllowedScopes =
					{
						IdentityServerConstants.StandardScopes.OpenId,
						IdentityServerConstants.StandardScopes.Profile,
						IdentityServerConstants.StandardScopes.Email,
						"api1"
					}
				},
				 // JavaScript Client
                new Client
				{
					ClientId = "js",
					ClientName = "JavaScript Client",
					AllowedGrantTypes = GrantTypes.Implicit,
					AllowAccessTokensViaBrowser = true,

					RedirectUris = { "http://localhost:5002/callback.html" },
					PostLogoutRedirectUris = { "http://localhost:5002/index.html" },
					AllowedCorsOrigins = { "http://localhost:5002" },

					AllowedScopes =
					{
						IdentityServerConstants.StandardScopes.OpenId,
						IdentityServerConstants.StandardScopes.Profile,
						"api1"
					},
				}
			};
        }
    }
}