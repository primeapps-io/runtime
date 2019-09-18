using IdentityModel;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Auth.Models;
using PrimeApps.Auth.UI;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using User = PrimeApps.Model.Entities.Tenant.TenantUser;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Enums;
using Microsoft.AspNetCore.Http;
using PrimeApps.Model.Entities.Platform;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Common.App;
using System.Collections.Generic;
using Hangfire.Common;
using PrimeApps.Auth.Helpers;

namespace PrimeApps.Auth.Controllers
{
    [Route("[controller]")]
    [SecurityHeaders]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private IPlatformRepository _platformRepository;
        private IApplicationRepository _applicationRepository;
        private IUserRepository _userRepository;
        private ISettingRepository _settingRepository;
        private IPlatformUserRepository _platformUserRepository;
        private IProfileRepository _profileRepository;
        private IRoleRepository _roleRepository;
        private readonly IEventService _events;
        private IConfiguration _configuration;
        private IUserHelper _userHelper;

        public UserController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEventService events,
            IPlatformRepository platformRepository,
            IPlatformUserRepository platformUserRepository,
            IApplicationRepository applicationRepository,
            IUserRepository userRepository,
            ISettingRepository settingRepository,
            IProfileRepository profileRepository,
            IRoleRepository roleRepository,
            IPlatformWarehouseRepository platformWarehouseRepository,
            IConfiguration configuration,
            IUserHelper userHelper)
        {
            _applicationRepository = applicationRepository;
            _userManager = userManager;
            _signInManager = signInManager;
            _platformRepository = platformRepository;
            _userRepository = userRepository;
            _settingRepository = settingRepository;
            _profileRepository = profileRepository;
            _platformUserRepository = platformUserRepository;
            _roleRepository = roleRepository;
            _configuration = configuration;
            _events = events;

            _userHelper = userHelper;
        }

        [Route("add_app_draft_user"), HttpPost, Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> AddAppDraftUser([FromBody]AppDraftUserModel addUserBindingModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "ModelState is not valid.");
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(addUserBindingModel.AppName))
                return BadRequest("AppName is required!");

            if (User?.Identity.IsAuthenticated == false)
                return Unauthorized();

            var email = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
            //_platformUserRepository.CurrentUser = new Model.Helpers.CurrentUser { TenantId = addUserBindingModel.TenantId };
            var currentPlatformUser = await _platformUserRepository.GetWithSettings(email);

            var platformUser = await _platformUserRepository.GetAsync(addUserBindingModel.Email);

            var password = Utils.GenerateRandomUnique(8);

            if (!string.IsNullOrEmpty(addUserBindingModel.Password))
                password = addUserBindingModel.Password;

            var userModel = new AddUserBindingModel
            {
                Email = addUserBindingModel.Email,
                LastName = addUserBindingModel.LastName,
                FirstName = addUserBindingModel.FirstName,
                Password = password
            };

            if (platformUser == null)
            {
                var setting = new PlatformUserSetting
                {
                    Culture = currentPlatformUser.Setting.Culture,
                    Language = currentPlatformUser.Setting.Language,
                    Currency = currentPlatformUser.Setting.Currency,
                    TimeZone = currentPlatformUser.Setting.TimeZone
                };

                platformUser = await _userHelper.CreatePlatformUser(userModel, addUserBindingModel.AppName, false, setting);

                if (platformUser == null)
                {
                    ModelState.AddModelError("", "Platform user not created.");
                    return BadRequest(ModelState);
                }

                /*
                 * Need to get platformUser model again because not update tenantsAsUser information with returned platformUser model.
                 */
                platformUser = await _platformUserRepository.GetWithTenants(userModel.Email);

                platformUser.TenantsAsUser.Add(new UserTenant { TenantId = 1, UserId = platformUser.Id });
                await _platformUserRepository.UpdateAsync(platformUser);
            }

            var identityUser = await _userManager.FindByNameAsync(addUserBindingModel.Email);
            string token = null;

            //If user already registered check email is confirmed. If not return confirm token with status code.
            if (identityUser != null && !identityUser.EmailConfirmed)
                token = await GetConfirmToken(identityUser);
            else if (identityUser == null)
            {
                var app = _applicationRepository.GetByName(addUserBindingModel.AppName);
                identityUser = await _userHelper.CreateIdentityUser(userModel, app.Setting?.AppDomain);

                if (identityUser == null)
                    return BadRequest();

                token = await GetConfirmToken(identityUser);
            }

            return StatusCode(201, new { token = WebUtility.UrlEncode(token), password = password, id = platformUser.Id });
        }

        [Route("add_organization_user"), HttpPost, Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> AddOrganizationUser([FromBody]AddOrganizationUserBindingModel addUserBindingModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "ModelState is not valid.");
                return BadRequest(ModelState);
            }

            if (User?.Identity.IsAuthenticated == false)
                return Unauthorized();

            var email = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
            //_platformUserRepository.CurrentUser = new Model.Helpers.CurrentUser { TenantId = addUserBindingModel.TenantId };
            var currentPlatformUser = await _platformUserRepository.GetWithSettings(email);

            var platformUser = await _platformUserRepository.GetAsync(addUserBindingModel.Email);

            var password = Utils.GenerateRandomUnique(8);

            if (!string.IsNullOrEmpty(addUserBindingModel.Password))
                password = addUserBindingModel.Password;

            var userModel = new AddUserBindingModel
            {
                Email = addUserBindingModel.Email,
                LastName = addUserBindingModel.LastName,
                FirstName = addUserBindingModel.FirstName,
                Password = password
            };

            if (platformUser == null)
            {
                var setting = new PlatformUserSetting
                {
                    Culture = currentPlatformUser.Setting.Culture,
                    Language = currentPlatformUser.Setting.Language,
                    Currency = currentPlatformUser.Setting.Currency,
                    TimeZone = currentPlatformUser.Setting.TimeZone
                };

                platformUser = await _userHelper.CreatePlatformUser(userModel, "", false, setting);

                if (platformUser == null)
                {
                    ModelState.AddModelError("", "Platform user not created.");
                    return BadRequest(ModelState);
                }
            }

            var identityUser = await _userManager.FindByNameAsync(addUserBindingModel.Email);
            string token = null;

            //If user already registered check email is confirmed. If not return confirm token with status code.
            if (identityUser != null && !identityUser.EmailConfirmed)
                token = await GetConfirmToken(identityUser);
            else if (identityUser == null)
            {
                var app = _applicationRepository.GetByName(addUserBindingModel.AppName);

                identityUser = await _userHelper.CreateIdentityUser(userModel, app.Setting.AppDomain);

                if (identityUser == null)
                    return BadRequest();

                token = await GetConfirmToken(identityUser);
            }

            return StatusCode(201, new { token = WebUtility.UrlEncode(token), password });
        }

        [Route("add_user"), HttpPost, Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> AddUser([FromBody]AddUserBindingModel addUserBindingModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "ModelState is not valid.");
                return BadRequest(ModelState);
            }

            if (User?.Identity.IsAuthenticated == false)
                return Unauthorized();

            var email = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
            //_platformUserRepository.CurrentUser = new Model.Helpers.CurrentUser { TenantId = addUserBindingModel.TenantId };
            var currentPlatformUser = await _platformUserRepository.GetWithSettings(email);
            var tenantCheck = currentPlatformUser.TenantsAsUser.SingleOrDefault(x => x.TenantId == addUserBindingModel.TenantId);

            if (tenantCheck == null)
                return Unauthorized();

            _userRepository.CurrentUser = new CurrentUser { TenantId = addUserBindingModel.TenantId, UserId = currentPlatformUser.Id };

            var currentTenantUser = _userRepository.GetByIdSync(currentPlatformUser.Id);

            if (!currentTenantUser.Profile.HasAdminRights)
            {
                //checks custom profile settings
                _settingRepository.CurrentUser = new Model.Helpers.CurrentUser { TenantId = addUserBindingModel.TenantId, UserId = currentPlatformUser.Id };
                var customProfileSetting = await _settingRepository.GetByKeyAsync("custom_profile_permissions");
                if (customProfileSetting != null)
                {
                    JToken profileSetting = JObject.Parse(customProfileSetting.Value)["profilePermissions"].Where(x => (int)x["profileId"] == currentTenantUser.Profile.Id).FirstOrDefault();
                    if (profileSetting != null)
                    {
                        var hasUserCreatePermission = profileSetting["permissions"].Any(x => x.Value<string>() == "users");
                        if(!hasUserCreatePermission)
                            return Unauthorized();
                    }
                    else
                        return Unauthorized();
                }
                else
                    return Unauthorized();
            }

            var checkEmail = await _platformUserRepository.IsEmailAvailable(addUserBindingModel.Email, addUserBindingModel.AppId);

            if (checkEmail == EmailAvailableType.NotAvailable)
                return new StatusCodeResult(StatusCodes.Status409Conflict);

            var tenant = _platformRepository.GetTenant(addUserBindingModel.TenantId);

            if (tenant.TenantUsers.Count >= tenant.License.UserLicenseCount)
                return new StatusCodeResult(StatusCodes.Status402PaymentRequired);

            var password = Utils.GenerateRandomUnique(8);

            if (!string.IsNullOrWhiteSpace(addUserBindingModel.Password))
                password = addUserBindingModel.Password;

            var userModel = new AddUserBindingModel
            {
                Email = addUserBindingModel.Email,
                LastName = addUserBindingModel.LastName,
                FirstName = addUserBindingModel.FirstName,
                Password = password,
                Phone = addUserBindingModel.Phone
            };

            PlatformUser platformUser = null;
            if (checkEmail != EmailAvailableType.AvailableForApp)
            {
                var settings = new PlatformUserSetting
                {
                    Culture = tenant.Setting.Culture,
                    Language = tenant.Setting.Language,
                    Currency = tenant.Setting.Currency,
                    TimeZone = tenant.Setting.TimeZone
                };

                platformUser = await _userHelper.CreatePlatformUser(userModel, "", false, settings);

                if (platformUser == null)
                {
                    ModelState.AddModelError("", "Platform user not created.");
                    return BadRequest(ModelState);
                }
            }
            else
            {
                platformUser = await _platformUserRepository.GetAsync(addUserBindingModel.Email);
            }

            var appInfo = await _applicationRepository.Get(addUserBindingModel.AppId);

            var identityUser = await _userManager.FindByNameAsync(addUserBindingModel.Email);
            string token = null;

            //If user already registered check email is confirmed. If not return confirm token with status code.
            if (identityUser != null && !identityUser.EmailConfirmed)
                token = await GetConfirmToken(identityUser);
            else
            {
                identityUser = await _userHelper.CreateIdentityUser(userModel, appInfo.Setting?.AppDomain);

                if (identityUser == null)
                    return BadRequest();

                token = await GetConfirmToken(identityUser);
            }

            platformUser = await _platformUserRepository.Get(platformUser.Id);
            var tenantUser = _userRepository.GetById(platformUser.Id);

            if (tenantUser == null)
            {
                tenantUser = await _userHelper.CreateTenantUser(platformUser.Id, userModel, addUserBindingModel.AppId, addUserBindingModel.TenantId, currentPlatformUser.Setting.Culture, currentPlatformUser.Setting.Currency);

                if (tenantUser == null)
                {
                    ModelState.AddModelError("", "Tenant user not created.");
                    return BadRequest(ModelState);
                }
            }
            else
            {
                password = "*******";
                tenantUser.IsActive = true;
                await _userRepository.UpdateAsync(tenantUser);
            }

            _profileRepository.CurrentUser = _roleRepository.CurrentUser = new Model.Helpers.CurrentUser { TenantId = addUserBindingModel.TenantId, UserId = currentPlatformUser.Id };
            await _profileRepository.AddUserAsync(platformUser.Id, addUserBindingModel.ProfileId);
            await _roleRepository.AddUserAsync(platformUser.Id, addUserBindingModel.RoleId);

            var currentTenant = _platformRepository.GetTenant(addUserBindingModel.TenantId);

            platformUser.TenantsAsUser.Add(new UserTenant { Tenant = currentTenant, PlatformUser = platformUser });

            await _platformUserRepository.UpdateAsync(platformUser);

            var externalLogin = appInfo.Setting.ExternalAuth != null ? JObject.Parse(appInfo.Setting.ExternalAuth) : null;

            if (externalLogin != null)
            {
                var actions = (JArray)externalLogin["actions"];
                var action = actions.Where(x => x["type"] != null && x["type"].ToString() == "register").FirstOrDefault();

                var obj = new JObject
                {
                    ["email"] = addUserBindingModel.Email,
                    ["password"] = password,
                    ["first_name"] = addUserBindingModel.FirstName,
                    ["last_name"] = addUserBindingModel.LastName,
                    ["full_name"] = addUserBindingModel.FirstName + " " + addUserBindingModel.LastName,
                    ["language"] = addUserBindingModel.Culture
                };

                await ExternalAuthHelper.Register(externalLogin, action, obj);
            }

            return StatusCode(201, new { token = WebUtility.UrlEncode(token), password = password });
        }

        [Route("change_password"), HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordViewModel changePasswordViewModel, [FromQuery]string client)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByEmailAsync(changePasswordViewModel.Email);
            var result = await _userManager.ChangePasswordAsync(user, changePasswordViewModel.OldPassword, changePasswordViewModel.NewPassword);

            var application = await _applicationRepository.GetByNameAsync(client);
            var externalLogin = application.Setting.ExternalAuth != null ? JObject.Parse(application.Setting.ExternalAuth) : null;

            if (result.Succeeded && externalLogin != null)
            {
                var actions = (JArray)externalLogin["actions"];
                var action = actions.Where(x => x["type"] != null && x["type"].ToString() == "change_password").FirstOrDefault();

                var obj = new JObject
                {
                    ["email"] = changePasswordViewModel.Email,
                    ["old_password"] = changePasswordViewModel.OldPassword,
                    ["new_password"] = changePasswordViewModel.NewPassword
                };

                await ExternalAuthHelper.ChangePassword(externalLogin, action, obj);
            }

            var response = new JObject
            {
                ["message"] = result.ToString()
            };
            return result.Succeeded ? StatusCode((int)HttpStatusCode.OK, response) : StatusCode((int)HttpStatusCode.NotFound, response);
        }

        [HttpPost("verify_user", Name = "verify_user")]
        public async Task<bool> VerifyUser([FromBody]ExternalLoginBindingModel model)
        {
            var application = await _applicationRepository.GetByNameAsync(model.client);
            var externalLogin = application.Setting.ExternalAuth != null ? JObject.Parse(application.Setting.ExternalAuth) : null;

            if (externalLogin != null)
            {
                var actions = (JArray)externalLogin["actions"];
                var action = actions.Where(x => x["type"] != null && x["type"].ToString() == "login").FirstOrDefault();

                var obj = new JObject
                {
                    ["email"] = model.email,
                    ["password"] = model.password
                };

                var result = await ExternalAuthHelper.VerifyUser(externalLogin, action, obj);

                return result.IsSuccessStatusCode;
            }

            return false;
        }

        [Route("token"), HttpPost, Consumes("application/x-www-form-urlencoded")]
        public async Task<TokenResponse> GetToken([FromForm]PasswordTokenRequest tokenRequest)
        {
            var user = _platformUserRepository.GetByEmail(tokenRequest.UserName);

            if (user == null) //!user.IsIntegrationUser || user.IntegrationUserClientId != tokenRequest.ClientId
            //TODO: Make this method available to get token from PrimeApps CLI
                return new TokenResponse(HttpStatusCode.Unauthorized, "Unauthorized", "Unauthorized");

            var httpClient = new HttpClient();

            tokenRequest.Address = _configuration["AppSettings:Authority"] + "/connect/token";

            return await httpClient.RequestPasswordTokenAsync(tokenRequest);
        }

        [Route("reset_password"), HttpPost]
        public async Task<IActionResult> ResetPassword([FromBody]JObject request)
        {
            var userToken = request["token"].ToString();

            if (string.IsNullOrEmpty(userToken))
                return BadRequest("Token can not be null!");

            var userString = CryptoHelper.Decrypt(userToken);
            if (string.IsNullOrEmpty(userString))
                return BadRequest("Invalid token!");

            var userObj = JObject.Parse(userString);
            if (string.IsNullOrEmpty(userObj["email"].ToString()) || string.IsNullOrEmpty(userObj["password"].ToString()))
                return BadRequest("Invalid token !");

            if (userObj["password"].ToString().Length < 6)
                return BadRequest("Password minimum lengh must be 6.");

            var user = await _userManager.FindByEmailAsync(userObj["email"].ToString());

            if (user == null)
                return BadRequest("User not found !");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, token, userObj["password"].ToString());

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok();
        }

        //Helpers
        public async Task<string> GetConfirmToken(ApplicationUser user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }
    }
}