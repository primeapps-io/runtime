using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Auth.Models;
using PrimeApps.Auth.UI;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using Microsoft.Extensions.Configuration;

namespace PrimeApps.Auth.Helpers
{
    public interface IUserHelper
    {
        Task<ApplicationUser> CreateIdentityUser(AddUserBindingModel userModel, string domain);
        Task<PlatformUser> CreatePlatformUser(AddUserBindingModel userModel, string appName, bool isIntegration = false, PlatformUserSetting settings = null);
        Task<TenantUser> CreateTenantUser(int platformUserId, AddUserBindingModel userModel, int appId, int tenantId, string culture = "en-US", string currency = "en");
        Task<bool> CreateIntegrationUser(int appId, int tenantId, string appName, string secret, string domain);
    }

    public class UserHelper : IUserHelper
    {
        private string previewMode;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IHttpContextAccessor _context;
        private readonly IGiteaHelper _giteaHelper;

        public IConfiguration _configuration { get; }

        public UserHelper(IHttpContextAccessor context,
            IServiceScopeFactory serviceScopeFactory,
            IConfiguration configuration,
            IGiteaHelper giteaHelper)
        {
            _context = context;
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
            _giteaHelper = giteaHelper;

            previewMode = _configuration.GetValue("AppSettings:PreviewMode", string.Empty);
            previewMode = !string.IsNullOrEmpty(previewMode) ? previewMode : "tenant";
        }

        public async Task<ApplicationUser> CreateIdentityUser(AddUserBindingModel userModel, string domain)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                var user = new ApplicationUser
                {
                    UserName = userModel.Email,
                    Email = userModel.Email,
                    NormalizedEmail = userModel.Email,
                    NormalizedUserName = !string.IsNullOrEmpty(userModel.FirstName) ? userModel.FirstName + " " + userModel.LastName : ""
                };

                var result = await _userManager.CreateAsync(user, userModel.Password);

                if (!result.Succeeded)
                {
                    ErrorHandler.LogError(null, "Identity user not created successfully. Model: " + user.ToJsonString());
                    return null;
                }

                await _userManager.AddClaimsAsync(user, new[]
                {
                    new Claim(JwtClaimTypes.Name, !string.IsNullOrEmpty(userModel.FirstName) ? userModel.FirstName + " " + userModel.LastName : ""),
                    new Claim(JwtClaimTypes.GivenName, userModel.FirstName),
                    new Claim(JwtClaimTypes.FamilyName, userModel.LastName),
                    new Claim(JwtClaimTypes.Email, userModel.Email),
                    new Claim(JwtClaimTypes.EmailVerified, "false", ClaimValueTypes.Boolean)
                });

                var studioUrl = _configuration.GetValue("AppSettings:StudioUrl", string.Empty);

                user = await _userManager.FindByNameAsync(userModel.Email);

                if (!string.IsNullOrEmpty(studioUrl) && studioUrl.Contains(domain))
                {
                    await _giteaHelper.CreateUser(userModel.Email, userModel.Password, userModel.FirstName, userModel.LastName);

                    var giteaToken = await _giteaHelper.GetToken(userModel.Email, userModel.Password);
                    if (!string.IsNullOrEmpty(giteaToken))
                        await _userManager.AddClaimAsync(user, new Claim("gitea_token", giteaToken));
                }

                return user;
            }
        }

        public async Task<PlatformUser> CreatePlatformUser(AddUserBindingModel userModel, string appName, bool isIntegration = false, PlatformUserSetting settings = null)
        {
            if (settings == null)
            {
                settings = new PlatformUserSetting
                {
                    Culture = string.IsNullOrEmpty(userModel.Culture) ? "en-US" : userModel.Culture,
                    Language = string.IsNullOrEmpty(userModel.Language) ? "en" : userModel.Language,
                    Currency = string.IsNullOrEmpty(userModel.Currency) ? "USD" : userModel.Currency,
                    TimeZone = "America/Chicago"
                };
            }

            var user = new PlatformUser
            {
                Email = userModel.Email,
                FirstName = userModel.FirstName,
                LastName = userModel.LastName,
                Setting = settings,
                IsIntegrationUser = isIntegration,
                IntegrationUserClientId = isIntegration ? $"{appName}-integration" : null
            };

            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var platformDatabaseContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();

                using (var _platformUserRepository = new PlatformUserRepository(platformDatabaseContext, _configuration))//, cacheHelper))
                {
                    var result = await _platformUserRepository.CreateUser(user);
                    if (result != 0)
                    {
                        var platformUser = await _platformUserRepository.GetWithTenants(userModel.Email);
                        return platformUser;
                    }

                    ErrorHandler.LogError(null, "Platform user not created successfully. Model: " + user.ToJsonString());
                    return null;
                }
            }
        }

        public async Task<TenantUser> CreateTenantUser(int platformUserId, AddUserBindingModel userModel, int appId, int tenantId, string culture = "en-US", string currency = "USD")
        {
            var user = new TenantUser
            {
                Id = platformUserId,
                Email = userModel.Email,
                FirstName = userModel.FirstName,
                LastName = userModel.LastName,
                FullName = $"{userModel.FirstName} {userModel.LastName}",
                Phone = userModel.Phone,
                Picture = "",
                IsActive = true,
                IsSubscriber = false,
                Culture = culture,
                Currency = currency,
                CreatedAt = DateTime.UtcNow,
                CreatedByEmail = userModel.Email
            };

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = scope.ServiceProvider.GetRequiredService<TenantDBContext>();

                using (var _userRepository = new UserRepository(databaseContext, _configuration))
                {
                    var _currentUser = new CurrentUser { TenantId = previewMode == "app" ? appId : tenantId, UserId = platformUserId, PreviewMode = previewMode };
                    _userRepository.CurrentUser = _currentUser;

                    var result = await _userRepository.CreateAsync(user);
                    if (result != 0)
                        return user;

                    ErrorHandler.LogError(null, "Tenant user not created successfully. Model: " + user.ToJsonString());
                    return null;
                }
            }
        }

        public async Task<bool> CreateIntegrationUser(int appId, int tenantId, string appName, string secret, string domain)
        {
            var password = CryptoHelper.Decrypt(secret);

            var user = new AddUserBindingModel
            {
                Email = $"integration_{appId}_{tenantId}@primeapps.io",
                FirstName = "Integration",
                LastName = "User",
                Password = password
            };

            var resultIdentityUser = await CreateIdentityUser(user, domain);
            if (resultIdentityUser == null)
                return false;

            var resultPlatformUser = await CreatePlatformUser(user, appName, true);
            if (resultPlatformUser == null)
                return false;


            var resultTenantUser = await CreateTenantUser(resultPlatformUser.Id, user, appId, tenantId);
            if (resultTenantUser == null)
                return false;

            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var platformDatabaseContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();

                using (var _platformUserRepository = new PlatformUserRepository(platformDatabaseContext, _configuration))//, cacheHelper))
                using (var _tenantRepository = new TenantRepository(platformDatabaseContext, _configuration))//, cacheHelper))
                {
                    var tenant = await _tenantRepository.GetAsync(tenantId);
                    var platformUser = await _platformUserRepository.GetWithTenants(user.Email);

                    if (platformUser.TenantsAsUser == null)
                        platformUser.TenantsAsUser = new List<UserTenant>();

                    platformUser.TenantsAsUser.Add(new UserTenant { Tenant = tenant, PlatformUser = platformUser });

                    await _platformUserRepository.UpdateAsync(platformUser);
                }
            }

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = scope.ServiceProvider.GetRequiredService<TenantDBContext>();

                using (var _userRepository = new UserRepository(databaseContext, _configuration))
                {
                    var _currentUser = new CurrentUser { TenantId = previewMode == "app" ? appId : tenantId, UserId = resultTenantUser.Id, PreviewMode = previewMode };
                    _userRepository.CurrentUser = _currentUser;

                    var result = _userRepository.GetById(resultTenantUser.Id);

                    result.ProfileId = 1;
                    result.RoleId = 1;

                    await _userRepository.UpdateAsync(result);
                }
            }

            return true;
        }
    }
}