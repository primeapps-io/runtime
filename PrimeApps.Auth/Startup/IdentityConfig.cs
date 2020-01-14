using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using IdentityServer.LdapExtension.Extensions;
using IdentityServer.LdapExtension.UserModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.KeyManagement.EntityFramework;
using IdentityServer4.Contrib.RedisStore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using PrimeApps.Auth.Data;
using PrimeApps.Auth.Models;
using IdentityServer4.EntityFramework.Stores;
using StackExchange.Redis;
using Microsoft.AspNetCore.DataProtection;

namespace PrimeApps.Auth
{
    public partial class Startup
    {
        public static void IdentityConfiguration(IServiceCollection services, IConfiguration configuration)
        {
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

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            var redis = ConnectionMultiplexer.Connect(configuration.GetConnectionString("RedisConnection"));
            var ser = services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                })
                .AddConfigurationStore(options => { options.ConfigureDbContext = opt => opt.UseNpgsql(configuration.GetConnectionString("AuthDBConnection"), sql => sql.MigrationsAssembly(migrationsAssembly)); })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = opt => opt.UseNpgsql(configuration.GetConnectionString("AuthDBConnection"), sql => sql.MigrationsAssembly(migrationsAssembly));

                    options.EnableTokenCleanup = false;
                    options.TokenCleanupInterval = 3600; //3600 (1 hour) 
                }) 
                //.AddRedisCaching(options =>
                //{
                //    options.RedisConnectionString = configuration.GetConnectionString("RedisConnection");
                //    options.KeyPrefix = "token";
                //})
                //////.AddPersistedGrantStore<PersistedGrantStore>()
                //.AddClientStoreCache<ClientStore>()
                //.AddResourceStoreCache<ResourceStore>()
                //.AddCorsPolicyCache<IdentityServer4.EntityFramework.Services.CorsPolicyService>()
                //.AddProfileServiceCache<CustomProfileService>()
                .AddSigningCredential(LoadCertificate())
                .AddSigningKeyManagement(options =>
                {
                    options.InitializationDuration = TimeSpan.FromSeconds(5);
                    options.InitializationSynchronizationDelay = TimeSpan.FromSeconds(1);

                    options.KeyActivationDelay = TimeSpan.FromSeconds(10);
                    options.KeyExpiration = TimeSpan.FromDays(1);
                    options.Licensee = "DEMO";
                    options.License = "eyJTb2xkRm9yIjowLjAsIktleVByZXNldCI6NiwiU2F2ZUtleSI6ZmFsc2UsIkxlZ2FjeUtleSI6ZmFsc2UsIlJlbmV3YWxTZW50VGltZSI6IjAwMDEtMDEtMDFUMDA6MDA6MDAiLCJhdXRoIjoiREVNTyIsImV4cCI6IjIwMjAtMDItMTJUMDA6MDA6MDQuOTU2ODEwOSswMDowMCIsImlhdCI6IjIwMjAtMDEtMTNUMDA6MDA6MDQiLCJvcmciOiJERU1PIiwiYXVkIjo1fQ==.ARaUh90xR8Ctp7/TSelbcXIjdRdwC5f/UHjVdrVX38jObv7zXWbyq5kdVjvAGe23Gqx9a2wUkORv2xuqvm61BhUHuNQ4EIu81QWRmZyG6DJwmhVctgAAfHNiP/he0wIN9aCUoaU9hafm3EuIYjt9G+5COus/m9POJIr42CWtpvrv+ScMaFqb00fOtFAcXxZbvkhB15Ef61JZkh7VW9c4bWOQIvqMnJDLQwsIXxVIBUdDhjC+Ss+qBk1FYSUJcudqmx/wLXHpmKmHCSCa4GWtkoBIEZvX+Aa/rAm3MSOuibuJK6VDKmik3jIbsOfeWsKe/5PXkE/Lx4j+guVV4k0EWQfqH30/T3bm1PJotsVENn5CkdpHeWBJuuEDidJxnWHLIkcizHHkEQwpMkWeN08Jq8qnYs0tuRZy4KIVJo/r5SwSgPx8L1uvC5vpHAeUwgd4OjzQtdmWDEOagiT6Ew5agNF1JpLgkPIkLFkhnOo2gTvlySMeDCHB8kKObdaeO5jWULgbo0lJWaK9ZH0Jruh4TabrXsRtPrnzUM5qb4Vrrz8L//2KCvkX0CeAxxxgRuIX/jvp34XoQjMixTLErDvZrkz54uBjIJvY9GcJvm8MgPgc70Evl/im7CljoKylV04sH6YWHzL1VeVmZ6BaHZyZlMdXR5VS0wxjGoZL6fFf0Gg=";
                })
                 .ProtectKeysWithDataProtection()
                 .PersistKeysToDatabase(new DatabaseKeyManagementOptions { ConfigureDbContext = options => options.UseNpgsql(configuration.GetConnectionString("test")) })
                .IdentityServer;

            //Fix for localhost identity cookie conflict
            var authority = configuration.GetValue("AppSettings:Authority", string.Empty);

            if (authority.Contains("localhost"))
                services.ConfigureApplicationCookie(options => { options.Cookie.Name = ".AspNetCore.Identity.Application." + authority.Substring(authority.Length - 4); });

            var useLdap = configuration.GetSection("Ldap").GetChildren().FirstOrDefault();
            if (useLdap != null)
                ser.AddLdapUsers<OpenLdapAppUser>(configuration.GetSection("Ldap"), UserStore.InMemory);

            ser.AddAspNetIdentity<ApplicationUser>();
            ser.AddProfileService<CustomProfileService>();

            //InitializeDatabase(app);
        }

        private static X509Certificate2 LoadCertificate()
        {
            var assembly = typeof(Startup).GetTypeInfo().Assembly;
            var embeddedFileProvider = new EmbeddedFileProvider(assembly, "PrimeApps.Auth");
            var certificateFileInfo = embeddedFileProvider.GetFileInfo("primeapps_id4.pfx");

            using (var certificateStream = certificateFileInfo.CreateReadStream())
            {
                byte[] certificatePayload;

                using (var memoryStream = new MemoryStream())
                {
                    certificateStream.CopyTo(memoryStream);
                    certificatePayload = memoryStream.ToArray();
                }

                return new X509Certificate2(certificatePayload, "1q2w3e4r5t", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            }
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