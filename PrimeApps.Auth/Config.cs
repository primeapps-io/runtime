using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

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
                // OpenID Connect hybrid flow and client credentials client (PrimeApps)
                new Client
                {
                    ClientId = "primeapps",
                    ClientName = "PrimeApps",
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    AllowRememberConsent = false,
                    AlwaysSendClientClaims = true,
                    RequireConsent = false,

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    RedirectUris = { "https://dev.primeapps.io/signin-oidc" },
                    PostLogoutRedirectUris = { "https://auth-dev.primeapps.io/signout-callback-oidc" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "api1"
                    },
                    AccessTokenLifetime = 864000
                },
                // OpenID Connect hybrid flow and client credentials client (PrimeApps)
                new Client
                {
                    ClientId = "primeapps_local",
                    ClientName = "PrimeApps",
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    AllowRememberConsent = false,
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
                    },
                    AccessTokenLifetime = 864000
                },
				// OpenID Connect hybrid flow and client credentials client (Ofisim CRM)
                new Client
                {
                    ClientId = "ofisim_crm",
                    ClientName = "Ofisim CRM",
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    AllowRememberConsent = false,
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
                    },
                    AccessTokenLifetime = 864000
                },
				// OpenID Connect hybrid flow and client credentials client (Ofisim IK)
                new Client
                {
                    ClientId = "ofisim_ik",
                    ClientName = "Ofisim IK",
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    AllowRememberConsent = false,
                    AlwaysSendClientClaims = true,
                    RequireConsent = false,

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    RedirectUris = { "http://localhost:5004/signin-oidc" },
                    PostLogoutRedirectUris = { "http://localhost:5004/signout-callback-oidc" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "api1"
                    },
                    AccessTokenLifetime = 864000
                },
                // OpenID Connect hybrid flow and client credentials client (PrimeApps Console)
                new Client
                {
                    ClientId = "primeapps_console",
                    ClientName = "PrimeApps Console",
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    AllowRememberConsent = false,
                    AlwaysSendClientClaims = true,
                    RequireConsent = false,

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    RedirectUris = { "https://console-dev.primeapps.io/signin-oidc" },
                    PostLogoutRedirectUris = { "https://console-dev.primeapps.io/signout-callback-oidc" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "api1"
                    },
                    AccessTokenLifetime = 864000
                },
                // OpenID Connect hybrid flow and client credentials client (PrimeApps Console Local)
                new Client
                {
                    ClientId = "primeapps_console_local",
                    ClientName = "PrimeApps Console",
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    AllowRememberConsent = false,
                    AlwaysSendClientClaims = true,
                    RequireConsent = false,

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    RedirectUris = { "http://localhost:5005/signin-oidc" },
                    PostLogoutRedirectUris = { "http://localhost:5005/signout-callback-oidc" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "api1"
                    },
                    AccessTokenLifetime = 864000
                }
            };
        }
    }
}