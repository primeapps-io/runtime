// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using IdentityServer4.Events;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using IdentityServer4.Extensions;
using System.Security.Principal;
using System.Security.Claims;
using IdentityModel;
using System.Linq;
using System;
using System.Collections.Generic;
using PrimeApps.Auth.Models;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using System.Net.Http;
using Newtonsoft.Json;
using System.Globalization;
using System.Threading;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Auth.Models.UserViewModels;
using System.Text;
using System.Net;
using IdentityServer4;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Helpers;
using Microsoft.EntityFrameworkCore;
using PrimeApps.Model.Enums;
using PrimeApps.Auth.Services;
using Newtonsoft.Json.Linq;

namespace PrimeApps.Auth.UI
{
    [SecurityHeaders]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;
        private IPlatformRepository _platformRepository;
        private IPlatformUserRepository _platformUserRepository;
        private IApplicationRepository _applicationRepository;
        private ITenantRepository _tenantRepository;
        private IUserRepository _userRepository;
        private IProfileRepository _profileRepository;
        private IRoleRepository _roleRepository;
        private IRecordRepository _recordRepository;

        public IBackgroundTaskQueue Queue { get; }

        public IConfiguration Configuration { get; }

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
            IBackgroundTaskQueue queue,
            IPlatformRepository platformRepository,
            IPlatformUserRepository platformUserRepository,
            IApplicationRepository applicationRepository,
            ITenantRepository tenantRepository,
            IUserRepository userRepository,
            IProfileRepository profileRepository,
            IRoleRepository roleRepository,
            IRecordRepository recordRepository,
            IConfiguration configuration)
        {
            Configuration = configuration;

            _userManager = userManager;
            _signInManager = signInManager;
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
            _platformRepository = platformRepository;
            _platformUserRepository = platformUserRepository;
            _applicationRepository = applicationRepository;
            _tenantRepository = tenantRepository;
            _userRepository = userRepository;
            _profileRepository = profileRepository;
            _roleRepository = roleRepository;
            _recordRepository = recordRepository;

            Queue = queue;

        }

        /// <summary>
        /// Account Index Page
        /// If there is a valid application redirect to application page.
        /// If not, show identity server index page.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string returnUrl)
        {
            var vm = await BuildIndexViewModelAsync(returnUrl);

            if (vm.ApplicationInfo != null)
                return Redirect(Request.Scheme + "://" + vm.ApplicationInfo.Domain);

            return View();
        }

        /// <summary>
        /// Change Language
        /// Using for change application current language in Authentication.
        /// </summary>
        [HttpGet]
        public IActionResult ChangeLanguage(string language, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(language)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return Redirect(returnUrl);
        }

        /// <summary>
        /// Login Page
        /// If user already logged in and url include client information. Redirect to application page.
        /// If no redirect to Identity Server Index page.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl, string success = "")
        {
            // build a model so we know what to show on the login page
            var vm = await BuildLoginViewModelAsync(returnUrl, success);

            if (User?.Identity.IsAuthenticated == true)
            {
                if (!string.IsNullOrEmpty(vm.ApplicationInfo.Domain))
                    return Redirect(Request.Scheme + "://" + vm.ApplicationInfo.Domain);
                else
                    return RedirectToAction(nameof(AccountController.Index), "Account");
            }

            var cookieLang = AuthHelper.CurrentLanguage(Request);

            if (cookieLang != vm.Language)
                return RedirectToAction(nameof(AccountController.ChangeLanguage), "Account", new { language = vm.Language, returnUrl = vm.ReturnUrl });

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return await ExternalLogin(vm.ExternalLoginScheme, vm.ReturnUrl);
            }

            return View(vm);
        }

        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {
            var vm = await BuildLoginViewModelAsync(model);

            if (button != "login")
            {
                // the user clicked the "cancel" button
                var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
                if (context != null)
                {
                    // if the user cancels, send a result back into IdentityServer as if they 
                    // denied the consent (even if this client does not require consent).
                    // this will send back an access denied OIDC error response to the client.
                    await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    // since we don't have a valid context, then we just go back to the home page
                    return Redirect("~/");
                }
            }

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberLogin, lockoutOnFailure: false);

                var validUrls = Configuration.GetValue("AppSettings:ValidUrls", String.Empty);
                Array validUrlsArr = null;
                if (!string.IsNullOrEmpty(validUrls))
                    validUrlsArr = validUrls.Split(";");

                if (result.Succeeded)
                {
                    if (vm.ApplicationInfo != null && Array.IndexOf(validUrlsArr, Request.Host.Host) == -1)
                    {
                        var platformUser = await _platformUserRepository.GetWithTenants(model.Username);

                        if (platformUser.TenantsAsUser.Count() > 0)
                        {
                            var tenant = platformUser.TenantsAsUser.Where(x => x.Tenant.AppId == vm.ApplicationInfo.Id).FirstOrDefault();
                            if (tenant == null)
                            {
                                await _signInManager.SignOutAsync();
                                vm.Error = "NotValidApp";
                                return View(vm);
                            }
                        }
                        else
                        {
                            await _signInManager.SignOutAsync();
                            vm.Error = "NotValidApp";
                            return View(vm);
                        }

                    }

                    var user = await _userManager.FindByNameAsync(model.Username);
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName));

                    // make sure the returnUrl is still valid, and if so redirect back to authorize endpoint or a local page
                    // the IsLocalUrl check is only necessary if you want to support additional local pages, otherwise IsValidReturnUrl is more strict
                    if (_interaction.IsValidReturnUrl(model.ReturnUrl) || Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    return Redirect("~/");
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials"));

                ModelState.AddModelError("", AccountOptions.InvalidCredentialsErrorMessage);
            }

            // something went wrong, show form with error
            vm.Error = "WrongInfo";

            return View(vm);
        }

        /// <summary>
        /// Register Page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Register(string returnUrl = null)
        {
            var vm = await BuildRegisterViewModelAsync(returnUrl);

            if (User?.Identity.IsAuthenticated == true)
            {
                if (!string.IsNullOrEmpty(vm.ApplicationInfo.Domain))
                    return Redirect(Request.Scheme + "://" + vm.ApplicationInfo.Domain);
                else
                    return RedirectToAction(nameof(AccountController.Index), "Account");
            }

            var cookieLang = AuthHelper.CurrentLanguage(Request);

            if (cookieLang != vm.Language)
                return RedirectToAction(nameof(AccountController.ChangeLanguage), "Account", new { language = vm.Language, returnUrl = vm.ReturnUrl });

            /*
            * If you want to make the user email field read only. Set true to ReadOnly variable.
            */
            vm.ReadOnly = false;

            return View(vm);
        }

        /// <summary>
        /// Handle postback from register page
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Register(RegisterInputModel model)
        {
            var vm = await BuildRegisterViewModelAsync(model);

            if (!ModelState.IsValid)
            {
                vm.Error = "ModelStateNotValid";
                return View(vm);
            }

            var identityUser = await _userManager.FindByNameAsync(model.Email);
            var newIdentityUser = false;
            if (identityUser == null)
            {
                newIdentityUser = true;
                var applicationUser = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    NormalizedEmail = model.Email,
                    NormalizedUserName = !string.IsNullOrEmpty(model.FirstName) ? model.FirstName + " " + model.LastName : ""
                };

                var result = await _userManager.CreateAsync(applicationUser, model.Password);

                if (!result.Succeeded)
                {
                    vm.Error = "UserNotCreated";
                    return View(vm);
                }

                result = _userManager.AddClaimsAsync(applicationUser, new Claim[]{
                    new Claim(JwtClaimTypes.Name, !string.IsNullOrEmpty(model.FirstName) ? model.FirstName + " " + model.LastName : ""),
                    new Claim(JwtClaimTypes.GivenName, model.FirstName),
                    new Claim(JwtClaimTypes.FamilyName, model.LastName),
                    new Claim(JwtClaimTypes.Email, model.Email),
                    new Claim(JwtClaimTypes.EmailVerified, "false", ClaimValueTypes.Boolean)
                }).Result;

                identityUser = await _userManager.FindByEmailAsync(model.Email);
            }

            if (vm.ApplicationInfo != null)
            {
                var token = "";
                var culture = !string.IsNullOrEmpty(model.Culture) ? model.Culture : vm.ApplicationInfo.Settings.Culture;

                /*var activateModel = new ActivateBindingModels
                {
                    email = model.Email,
                    app_id = applicationInfo.Id,
                    culture = culture,
                    first_name = model.FirstName,
                    last_name = model.LastName,
                    email_confirmed = identityUser.EmailConfirmed
                };*/

                if (!identityUser.EmailConfirmed)
                    token = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);

                var newPlatformUser = false;
                PlatformUser platformUser = await _platformUserRepository.GetWithTenants(model.Email);

                if (platformUser != null)
                {
                    var appTenant = platformUser.TenantsAsUser.FirstOrDefault(x => x.Tenant.AppId == vm.ApplicationInfo.Id);

                    if (appTenant != null)
                    {
                        vm.Error = "AlreadyRegisterForApp";
                        return View(vm);
                    }
                }
                else
                {
                    newPlatformUser = true;
                    platformUser = new PlatformUser
                    {
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Setting = new PlatformUserSetting()
                    };

                    if (!string.IsNullOrEmpty(culture))
                    {
                        platformUser.Setting.Culture = culture;
                        platformUser.Setting.Language = culture.Substring(0, 2);
                        //tenant.Setting.TimeZone =
                        platformUser.Setting.Currency = culture;
                    }
                    else
                    {
                        platformUser.Setting.Culture = vm.ApplicationInfo.Settings.Culture;
                        platformUser.Setting.Currency = vm.ApplicationInfo.Settings.Currency;
                        platformUser.Setting.Language = vm.ApplicationInfo.Language;
                        platformUser.Setting.TimeZone = vm.ApplicationInfo.Settings.TimeZone;

                    }

                    var result = await _platformUserRepository.CreateUser(platformUser);

                    if (result == 0)
                    {
                        DatabaseRollback(identityUser, newIdentityUser, newPlatformUser, null, null);
                        vm.Error = "UnexpectedError";
                        return View(vm);
                    }

                    platformUser = await _platformUserRepository.GetWithTenants(model.Email);
                }

                var tenantId = 0;
                Tenant tenant = null;
                //var tenantId = 2032;
                try
                {
                    tenant = new Tenant
                    {
                        //Id = tenantId,
                        AppId = vm.ApplicationInfo.Id,
                        Owner = platformUser,
                        UseUserSettings = true,
                        Title = model.FirstName + " " + model.LastName,
                        GuidId = Guid.NewGuid(),
                        License = new TenantLicense
                        {
                            UserLicenseCount = 5,
                            ModuleLicenseCount = 2
                        },
                        Setting = new TenantSetting
                        {
                            Culture = vm.ApplicationInfo.Settings.Culture,
                            Currency = vm.ApplicationInfo.Settings.Currency,
                            Language = vm.ApplicationInfo.Language,
                            TimeZone = vm.ApplicationInfo.Settings.TimeZone
                        },
                        CreatedBy = platformUser
                    };

                    await _tenantRepository.CreateAsync(tenant);
                    tenantId = tenant.Id;

                    platformUser.TenantsAsOwner.Add(tenant);
                    await _platformUserRepository.UpdateAsync(platformUser);

                    var tenantUser = new TenantUser
                    {
                        Id = platformUser.Id,
                        Email = platformUser.Email,
                        FirstName = platformUser.FirstName,
                        LastName = platformUser.LastName,
                        FullName = $"{platformUser.FirstName} {platformUser.LastName}",
                        IsActive = true,
                        IsSubscriber = false,
                        Culture = platformUser.Setting.Culture,
                        Currency = vm.ApplicationInfo.Settings.Currency,
                        CreatedAt = platformUser.CreatedAt,
                        CreatedByEmail = platformUser.Email
                    };

                    await Postgres.CreateDatabaseWithTemplate(_tenantRepository.DbContext.Database.GetDbConnection().ConnectionString, tenantId, vm.ApplicationInfo.Id);

                    _userRepository.CurrentUser = new CurrentUser { TenantId = tenant.Id, UserId = platformUser.Id };
                    _profileRepository.CurrentUser = new CurrentUser { TenantId = tenant.Id, UserId = platformUser.Id };
                    _roleRepository.CurrentUser = new CurrentUser { TenantId = tenant.Id, UserId = platformUser.Id };
                    _recordRepository.CurrentUser = new CurrentUser { TenantId = tenant.Id, UserId = platformUser.Id };

                    _profileRepository.TenantId = _roleRepository.TenantId = _userRepository.TenantId = _recordRepository.TenantId = tenantId;

                    tenantUser.IsSubscriber = true;
                    await _userRepository.CreateAsync(tenantUser);

                    var userProfile = await _profileRepository.GetDefaultAdministratorProfileAsync();
                    var userRole = await _roleRepository.GetByIdAsync(1);

                    tenantUser.Profile = userProfile;
                    tenantUser.Role = userRole;


                    await _userRepository.UpdateAsync(tenantUser);
                    await _recordRepository.UpdateSystemData(platformUser.Id, DateTime.UtcNow, tenant.Setting.Language, vm.ApplicationInfo.Id);


                    platformUser.TenantsAsUser.Add(new UserTenant { Tenant = tenant, PlatformUser = platformUser });

                    //user.TenantId = user.Id;
                    //tenant.License.HasAnalyticsLicense = true;
                    await _platformUserRepository.UpdateAsync(platformUser);
                    await _tenantRepository.UpdateAsync(tenant);

                    await _recordRepository.UpdateSampleData(platformUser);
                    //await Cache.ApplicationUser.Add(user.Email, user.Id);
                    //await Cache.User.Get(user.Id);

                    var url = Request.Scheme + "://" + vm.ApplicationInfo.Domain + "/api/account/user_created";

                    var requestModel = new JObject
                    {
                        ["email"] = model.Email,
                        ["app_id"] = vm.ApplicationInfo.Id,
                        ["guid_id"] = identityUser.Id,
                        ["tenant_language"] = tenant.Setting.Language,
                        ["code"] = token,
                        ["user_exist"] = newPlatformUser,
                        ["email_confirmed"] = identityUser.EmailConfirmed,
                        ["culture"] = culture,
                        ["first_name"] = model.FirstName,
                        ["last_name"] = model.LastName,
                        ["return_url"] = vm.ReturnUrl
                    };

                    using (var httpClient = new HttpClient())
                    {
                        httpClient.BaseAddress = new Uri(url);
                        httpClient.DefaultRequestHeaders.Accept.Clear();
                        httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                        var response = await httpClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(requestModel), Encoding.UTF8, "application/json"));

                        if (!response.IsSuccessStatusCode)
                        {
                            DatabaseRollback(identityUser, newIdentityUser, newPlatformUser, _platformUserRepository, _tenantRepository, tenant, platformUser);
                            vm.Error = "TenantCreateError";
                            return View(vm);
                            //TODO Loglara Eklenebilir.
                        }
                    }

                    //TODO Buraya webhook eklenecek. AppSetting üzerindeki TenantCreateWebhook alanı dolu kontrol edilecek doluysa bu url'e post edilecek
                    //Queue.QueueBackgroundWorkItem(async token => await _platformWorkflowHelper.Run(OperationType.insert, app));

                }
                catch (Exception ex)
                {
                    DatabaseRollback(identityUser, newIdentityUser, newPlatformUser, _platformUserRepository, _tenantRepository, tenant, platformUser);

                    throw ex;
                }
            }

            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await _signInManager.SignOutAsync();
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            var signInResult = await _signInManager.PasswordSignInAsync(model.Email, model.Password, true, lockoutOnFailure: false);

            if (signInResult.Succeeded)
                await _events.RaiseAsync(new UserLoginSuccessEvent(identityUser.UserName, identityUser.Id, identityUser.UserName));

            if (vm.ApplicationInfo != null)
                return Redirect(Request.Scheme + "://" + vm.ApplicationInfo.Domain);

            return RedirectToAction(nameof(HomeController), nameof(HomeController.Index));
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail([FromQuery(Name = "email")]string email, [FromQuery(Name = "code")]string code)
        {
            if (email == null || code == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                throw new ApplicationException($"Unable to load user with user '{email}'.");

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (!result.Succeeded)
                ViewBag.Error = result.Errors.FirstOrDefault().Code; //InvaliedToken

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ForgotPassword(string returnUrl, string email = "")
        {
            if (User?.Identity.IsAuthenticated == true)
                return RedirectToAction(nameof(AccountController.Index), "Account");

            var cookieLang = AuthHelper.CurrentLanguage(Request);

            var vm = await BuildForgotPasswordViewModelAsync(returnUrl);
            vm.Email = email;

            if (cookieLang != vm.Language)
                return RedirectToAction(nameof(AccountController.ChangeLanguage), "Account", new { language = ViewBag.Language, returnUrl = Request.Path.Value + Request.QueryString.Value });

            //vm.ReadOnly = false;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, string returnUrl = null)
        {
            var vm = await BuildForgotPasswordViewModelAsync(model, returnUrl);

            if (string.IsNullOrEmpty(model.Email))
                vm.Error = "InvalidEmail";

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                vm.Error = "ForgotNotFound";

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var url = Request.Scheme + "://" + vm.ApplicationInfo.Domain + "/api/account/send_password_reset";

            var requestModel = new JObject
            {
                ["return_url"] = vm.ReturnUrl,
                ["email"] = model.Email,
                ["app_id"] = vm.ApplicationInfo.Id,
                ["guid_id"] = user.Id,
                ["code"] = token,
                ["culture"] = CultureInfo.CurrentCulture.Name
            };

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(url);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var response = await httpClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(requestModel), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    vm.Error = "NotSendEmail";
                    return View(vm);
                }
            }

            vm.Success = "Success";
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword([FromQuery]string returnUrl, [FromQuery]string code, [FromQuery]Guid guid, string error)
        {
            var vm = await BuildResetPasswordViewModelAsync(returnUrl, HttpUtility.UrlDecode(code), guid, error: error);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, string returnUrl = null)
        {
            var vm = await BuildResetPasswordViewModelAsync(model, returnUrl);

            if (string.IsNullOrEmpty(vm.Code))
            {
                vm.Error = "InvalidToken";
                return View(vm);
            }

            var user = await _userManager.FindByIdAsync(vm.Guid.ToString());

            if (user == null)
            {
                vm.Error = "NotFound";
                return View(vm);
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);

            if (result.Succeeded)
                return RedirectToAction("Login", "Account", new { vm.ReturnUrl, Success = "PasswordChanged" });

            vm.Error = "InvalidToken";
            return View();
        }

        /// <summary>
        /// initiate roundtrip to external authentication provider
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExternalLogin(string provider, string returnUrl)
        {
            if (AccountOptions.WindowsAuthenticationSchemeName == provider)
            {
                // windows authentication needs special handling
                return await ProcessWindowsLoginAsync(returnUrl);
            }
            else
            {
                // start challenge and roundtrip the return URL and 
                var props = new AuthenticationProperties()
                {
                    RedirectUri = Url.Action("ExternalLoginCallback"),
                    Items =
                    {
                        { "returnUrl", returnUrl },
                        { "scheme", provider },
                    }
                };
                return Challenge(props, provider);
            }
        }

        /// <summary>
        /// Post processing of external authentication
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            // read external identity from the temporary cookie
            var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            if (result?.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }

            // lookup our user and external provider info
            var (user, provider, providerUserId, claims) = await FindUserFromExternalProviderAsync(result);

            if (user == null)
            {
                // this might be where you might initiate a custom workflow for user registration
                // in this sample we don't show how that would be done, as our sample implementation
                // simply auto-provisions new external user
                user = await AutoProvisionUserAsync(provider, providerUserId, claims);
            }

            if (user != null)
            {
                var clientId = AuthHelper.GetClientId(result.Properties.Items["returnUrl"]);

                if (!string.IsNullOrEmpty(clientId))
                {
                    var platformUser = await _platformUserRepository.GetWithTenants(user.UserName);
                    var appInfo = await _applicationRepository.GetByName(clientId);
                    var userApp = platformUser?.TenantsAsUser.Where(x => x.Tenant.AppId == appInfo.Id);

                    if (platformUser == null || userApp == null)
                    {
                        var createUrl = Request.Scheme + "://" + appInfo.Setting.AppDomain + "/api/account/create";

                        var activateModel = new ActivateBindingModels
                        {
                            email = user.UserName,
                            app_id = appInfo.Id,
                            culture = CultureInfo.CurrentCulture.Name,
                            first_name = result.Principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"),
                            last_name = result.Principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname"),
                            email_confirmed = user.EmailConfirmed
                        };

                        using (var httpClient = new HttpClient())
                        {
                            httpClient.BaseAddress = new Uri(createUrl);
                            httpClient.DefaultRequestHeaders.Accept.Clear();
                            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                            var response = await httpClient.PostAsync(createUrl, new StringContent(JsonConvert.SerializeObject(activateModel), Encoding.UTF8, "application/json"));

                            /*if (!response.IsSuccessStatusCode)
							{
								ViewBag.AppInfo = AuthHelper.GetApplicationInfo(Configuration, Request, Response, returnUrl, _applicationRepository);
								if (response.StatusCode == HttpStatusCode.Conflict)
								{
									ViewBag.Error = "alreadyRegisterForApp";
									return View(registerViewModel);
								}
								else
								{
									ViewBag.Error = "unexpectedError";
									return View(registerViewModel);
								}
							}*/
                        }
                    }
                }
            }

            /*await _userManager.AddClaimAsync(user, new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/tenantId", result.Principal.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid")));
			await _userManager.AddClaimAsync(user, new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", result.Principal.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")));
			await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.NameIdentifier, result.Principal.FindFirstValue(ClaimTypes.NameIdentifier)));
            */
            // this allows us to collect any additonal claims or properties
            // for the specific prtotocols used and store them in the local auth cookie.
            // this is typically used to store data needed for signout from those protocols.
            var additionalLocalClaims = new List<Claim>();
            var localSignInProps = new AuthenticationProperties();
            ProcessLoginCallbackForOidc(result, additionalLocalClaims, localSignInProps);
            ProcessLoginCallbackForWsFed(result, additionalLocalClaims, localSignInProps);
            ProcessLoginCallbackForSaml2p(result, additionalLocalClaims, localSignInProps);

            // issue authentication cookie for user
            // we must issue the cookie maually, and can't use the SignInManager because
            // it doesn't expose an API to issue additional claims from the login workflow
            var principal = await _signInManager.CreateUserPrincipalAsync(user);
            additionalLocalClaims.AddRange(principal.Claims);
            var name = principal.FindFirst(JwtClaimTypes.Name)?.Value ?? user.Id;
            await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.Id, name));
            await HttpContext.SignInAsync(user.Id, name, provider, localSignInProps, additionalLocalClaims.ToArray());

            // delete temporary cookie used during external authentication
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // validate return URL and redirect back to authorization endpoint or a local page
            var returnUrl = result.Properties.Items["returnUrl"];
            if (_interaction.IsValidReturnUrl(returnUrl) || Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect("~/");
        }

        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId, string returnUrl)
        {
            // build a model so the logout page knows what to display
            var vm = await BuildLogoutViewModelAsync(logoutId, returnUrl);

            if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout(vm);
            }

            return View(vm);
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await _signInManager.SignOutAsync();

                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }
            if (!string.IsNullOrEmpty(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return Redirect(Request.Scheme + "://" + Request.Host.Value);
            //return View("LoggedOut", vm);
        }

        /*****************************************/
        /* helper APIs for the AccountController */
        /*****************************************/

        private async Task<ResetPasswordViewModel> BuildResetPasswordViewModelAsync(string returnUrl, string code, Guid guid, string success = "", string error = "")
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            var applicationInfo = await AuthHelper.GetApplicationInfoAsync(Configuration, Request, returnUrl, _applicationRepository);

            return new ResetPasswordViewModel
            {
                Code = code,
                Guid = guid,
                ReturnUrl = returnUrl,
                ApplicationInfo = applicationInfo,
                Language = applicationInfo.Language,
                Success = success,
                Error = error
            };
        }
        private async Task<ResetPasswordViewModel> BuildResetPasswordViewModelAsync(ResetPasswordViewModel model, string returnUrl, string success = "", string error = "")
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            var vm = await BuildResetPasswordViewModelAsync(returnUrl, model.Code, model.Guid, success, error);
            return vm;
        }
        private async Task<ForgotPasswordViewModel> BuildForgotPasswordViewModelAsync(string returnUrl, string success = "", string error = "")
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            var applicationInfo = await AuthHelper.GetApplicationInfoAsync(Configuration, Request, returnUrl, _applicationRepository);

            return new ForgotPasswordViewModel
            {
                ReturnUrl = returnUrl,
                ApplicationInfo = applicationInfo,
                Language = applicationInfo.Language,
                Success = success,
                Error = error
            };
        }
        private async Task<ForgotPasswordViewModel> BuildForgotPasswordViewModelAsync(ForgotPasswordViewModel model, string returnUrl, string success = "", string error = "")
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            var applicationInfo = await AuthHelper.GetApplicationInfoAsync(Configuration, Request, returnUrl, _applicationRepository);

            return new ForgotPasswordViewModel
            {
                Email = model.Email,
                ReturnUrl = returnUrl,
                ApplicationInfo = applicationInfo,
                Language = applicationInfo.Language,
                Success = success,
                Error = error
            };
        }
        private async Task<RegisterViewModel> BuildRegisterViewModelAsync(string returnUrl, string success = "", string error = "")
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            var applicationInfo = await AuthHelper.GetApplicationInfoAsync(Configuration, Request, returnUrl, _applicationRepository);

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null ||
                            (x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase))
                )
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new RegisterViewModel
            {
                Culture = CultureInfo.CurrentCulture.Name,
                SendActivation = true,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                ExternalProviders = providers.ToArray(),
                ApplicationInfo = applicationInfo,
                Language = applicationInfo.Language,
                Success = success,
                Error = error
            };
        }
        private async Task<ApplicationViewModel> BuildIndexViewModelAsync(string returnUrl, string success = "", string error = "")
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            var applicationInfo = await AuthHelper.GetApplicationInfoAsync(Configuration, Request, returnUrl, _applicationRepository);

            return new ApplicationViewModel
            {
                ReturnUrl = returnUrl,
                ApplicationInfo = applicationInfo,
                Language = applicationInfo.Language,
                Success = success,
                Error = error
            };
        }
        private async Task<RegisterViewModel> BuildRegisterViewModelAsync(RegisterInputModel model, string error = "")
        {
            var vm = await BuildRegisterViewModelAsync(model.ReturnUrl, error: error);
            vm.FirstName = model.FirstName;
            vm.LastName = model.LastName;
            vm.Email = model.Email;
            vm.SendActivation = model.SendActivation;
            vm.Culture = model.Culture;
            vm.Password = null;
            return vm;

        }
        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl, string success = "", string error = "")
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            var applicationInfo = await AuthHelper.GetApplicationInfoAsync(Configuration, Request, returnUrl, _applicationRepository);

            if (context?.IdP != null)
            {
                // this is meant to short circuit the UI and only trigger the one external IdP

                return new LoginViewModel
                {
                    EnableLocalLogin = false,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
                    ExternalProviders = new ExternalProvider[] { new ExternalProvider { AuthenticationScheme = context.IdP } },
                    ApplicationInfo = applicationInfo,
                    Language = applicationInfo.Language,
                    Success = success,
                    Error = error
                };
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null ||
                            (x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase))
                )
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new LoginViewModel
            {
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                ExternalProviders = providers.ToArray(),
                ApplicationInfo = applicationInfo,
                Language = applicationInfo.Language,
                Success = success,
                Error = error
            };
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model, string error = "")
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl, error: error);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }

        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId, string returnUrl)
        {
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            vm.ShowLogoutPrompt = false;
            vm.ReturnUrl = returnUrl;
            return vm;

            //TODO Removed
            /*if (User?.Identity.IsAuthenticated != true)
			{
				// if the user is not authenticated, then just show logged out page
				vm.ShowLogoutPrompt = false;
				return vm;
			}

			var context = await _interaction.GetLogoutContextAsync(logoutId);
			if (context?.ShowSignoutPrompt == false)
			{
				// it's safe to automatically sign-out
				vm.ShowLogoutPrompt = false;
				return vm;
			}

			// show the logout prompt. this prevents attacks where the user
			// is automatically signed out by another malicious web page.
			return vm;*/
        }

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (User?.Identity.IsAuthenticated == true)
            {
                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }

        private async Task<IActionResult> ProcessWindowsLoginAsync(string returnUrl)
        {
            // see if windows auth has already been requested and succeeded
            var result = await HttpContext.AuthenticateAsync(AccountOptions.WindowsAuthenticationSchemeName);
            if (result?.Principal is WindowsPrincipal wp)
            {
                // we will issue the external cookie and then redirect the
                // user back to the external callback, in essence, tresting windows
                // auth the same as any other external authentication mechanism
                var props = new AuthenticationProperties()
                {
                    RedirectUri = Url.Action("ExternalLoginCallback"),
                    Items =
                    {
                        { "returnUrl", returnUrl },
                        { "scheme", AccountOptions.WindowsAuthenticationSchemeName },
                    }
                };

                var id = new ClaimsIdentity(AccountOptions.WindowsAuthenticationSchemeName);
                id.AddClaim(new Claim(JwtClaimTypes.Subject, wp.Identity.Name));
                id.AddClaim(new Claim(JwtClaimTypes.Name, wp.Identity.Name));

                // add the groups as claims -- be careful if the number of groups is too large
                if (AccountOptions.IncludeWindowsGroups)
                {
                    var wi = wp.Identity as WindowsIdentity;
                    var groups = wi.Groups.Translate(typeof(NTAccount));
                    var roles = groups.Select(x => new Claim(JwtClaimTypes.Role, x.Value));
                    id.AddClaims(roles);
                }

                await HttpContext.SignInAsync(
                    IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme,
                    new ClaimsPrincipal(id),
                    props);
                return Redirect(props.RedirectUri);
            }
            else
            {
                // trigger windows auth
                // since windows auth don't support the redirect uri,
                // this URL is re-triggered when we call challenge
                return Challenge(AccountOptions.WindowsAuthenticationSchemeName);
            }
        }

        private async Task<(ApplicationUser user, string provider, string providerUserId, IEnumerable<Claim> claims)> FindUserFromExternalProviderAsync(AuthenticateResult result)
        {
            var externalUser = result.Principal;

            // try to determine the unique id of the external user (issued by the provider)
            // the most common claim type for that are the sub claim and the NameIdentifier
            // depending on the external provider, some other claim type might be used
            var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                              externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                              throw new Exception("Unknown userid");

            // remove the user id claim so we don't include it as an extra claim if/when we provision the user
            var claims = externalUser.Claims.ToList();
            claims.Remove(userIdClaim);

            var provider = result.Properties.Items["scheme"];
            var providerUserId = userIdClaim.Value;

            // find external user
            var user = await _userManager.FindByLoginAsync(provider, providerUserId);

            return (user, provider, providerUserId, claims);
        }

        private async Task<ApplicationUser> AutoProvisionUserAsync(string provider, string providerUserId, IEnumerable<Claim> claims)
        {
            // create a list of claims that we want to transfer into our store
            var filtered = new List<Claim>();

            // user's display name
            var name = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name)?.Value ??
                claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;

            var first = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName)?.Value ??
                    claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;

            var last = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.FamilyName)?.Value ??
                claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;

            if (name != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Name, name));
            }
            else
            {
                if (first != null && last != null)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Name, first + " " + last));
                }
                else if (first != null)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Name, first));
                }
                else if (last != null)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Name, last));
                }
            }

            // email
            var email = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Email)?.Value ??
               claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (email != null)
            {
                filtered.Add(new Claim(JwtClaimTypes.Email, email));
            }
            else
            {
                email = claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value ??
                    claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;

                if (email != null)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Email, email));
                }
            }

            var platformUser = await _userManager.FindByEmailAsync(email);
            IdentityResult identityResult = null;
            if (platformUser == null)
            {
                platformUser = new ApplicationUser
                {
                    EmailConfirmed = true,
                    UserName = email,
                    NormalizedUserName = name ?? first + " " + last
                };

                identityResult = await _userManager.CreateAsync(platformUser);
                if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);

            }

            if (filtered.Any())
            {
                identityResult = await _userManager.AddClaimsAsync(platformUser, filtered);
                if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);
            }

            identityResult = await _userManager.AddLoginAsync(platformUser, new UserLoginInfo(provider, providerUserId, provider));
            if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);

            return platformUser;
        }

        private void ProcessLoginCallbackForOidc(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
            // if the external system sent a session id claim, copy it over
            // so we can use it for single sign-out
            var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != null)
            {
                localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            // if the external provider issued an id_token, we'll keep it for signout
            var id_token = externalResult.Properties.GetTokenValue("id_token");
            if (id_token != null)
            {
                localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = id_token } });
            }
        }

        private void ProcessLoginCallbackForWsFed(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
        }

        private void ProcessLoginCallbackForSaml2p(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        public async void DatabaseRollback(ApplicationUser user, bool newIdentityUser, bool newPlatformUser, IPlatformUserRepository platformUserRepository, ITenantRepository tenantRepository, Tenant tenant = null, PlatformUser platformUser = null)
        {
            if (tenant != null)
            {
                Postgres.DropDatabase(_tenantRepository.DbContext.Database.GetDbConnection().ConnectionString, tenant.Id, true);
                //await tenantRepository.DeleteAsync(tenant);
            }

            if (newPlatformUser && platformUser != null)
                await platformUserRepository.DeleteAsync(platformUser);

            if (newIdentityUser)
                await _userManager.DeleteAsync(user);
        }
    }
}