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
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Entities.Platform;
using System.Text;
using IdentityServer4;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Helpers;
using Microsoft.EntityFrameworkCore;
using PrimeApps.Auth.Services;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.Net.Http.Headers;
using PrimeApps.Auth.Helpers;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Common.App;
using IdentityServer.LdapExtension.UserStore;

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
        private IGiteaHelper _giteaHelper;

        public IBackgroundTaskQueue Queue { get; }

        public IConfiguration _configuration { get; }

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
            IGiteaHelper giteaHelper,
            IConfiguration configuration)
        {
            _configuration = configuration;
            _giteaHelper = giteaHelper;
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
                new CookieOptions {Expires = DateTimeOffset.UtcNow.AddYears(1)}
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
                    return RedirectToAction(nameof(AccountController), nameof(AccountController.Index));
            }

            var cookieLang = AuthHelper.CurrentLanguage(Request);

            if (cookieLang != vm.Language)
                return RedirectToAction(nameof(AccountController.ChangeLanguage), "Account", new {language = vm.Language, returnUrl = vm.ReturnUrl});

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return await ExternalLogin(vm.ExternalLoginScheme, vm.ReturnUrl);
            }

            if (!vm.ApplicationInfo.Theme["custom"].IsNullOrEmpty())
                return View("Custom/Login" + vm.ApplicationInfo.Theme["custom"], vm);

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
                //ldap control
                var useLdap = _configuration.GetSection("Ldap").GetChildren().FirstOrDefault();
                if (useLdap != null)
                {
                    var _userStore = (ILdapUserStore)HttpContext.RequestServices.GetService(typeof(ILdapUserStore));
                    var ldapUser = _userStore.ValidateCredentials(model.Username, model.Password);

                    if (ldapUser == null)
                    {
                        await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials"));

                        ModelState.AddModelError("", AccountOptions.InvalidCredentialsErrorMessage);

                        vm.Error = "WrongInfo";

                        if (!vm.ApplicationInfo.Theme["custom"].IsNullOrEmpty())
                            return View("Custom/Login" + vm.ApplicationInfo.Theme["custom"], vm);

                        return View(vm);
                    }
                }

                var studioUrl = _configuration.GetValue("AppSettings:StudioUrl", string.Empty);
                var previewMode = _configuration.GetValue("AppSettings:PreviewMode", string.Empty);
                previewMode = !string.IsNullOrEmpty(previewMode) ? previewMode : "tenant";

                if (!string.IsNullOrEmpty(studioUrl) && studioUrl.Contains(vm.ApplicationInfo.Domain))
                {
                    var platformUser = await _platformUserRepository.Get(model.Username);

                    if (platformUser != null)
                    {
                        var url = studioUrl + "/api/account/user_available/" + platformUser.Id;
                        using (var httpClient = new HttpClient())
                        {
                            httpClient.DefaultRequestHeaders.Accept.Clear();
                            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            var isStudioUser = await httpClient.GetAsync(url);

                            if (!isStudioUser.IsSuccessStatusCode)
                            {
                                vm.Error = "WrongInfo";

                                if (!vm.ApplicationInfo.Theme["custom"].IsNullOrEmpty())
                                    return View("Custom/Login" + vm.ApplicationInfo.Theme["custom"], vm);

                                return View(vm);
                            }
                        }
                    }
                }

                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberLogin, lockoutOnFailure: false);
                var validationSkipDomains = _configuration.GetValue("AppSettings:ValidationSkipDomains", string.Empty);
                Array validationSkipDomainsArr = null;

                if (!string.IsNullOrEmpty(validationSkipDomains))
                    validationSkipDomainsArr = validationSkipDomains.Split(";");

                if (result.Succeeded)
                {
                    if (previewMode != "app" && vm.ApplicationInfo != null && (validationSkipDomainsArr == null || validationSkipDomainsArr.Length < 1 || Array.IndexOf(validationSkipDomainsArr, Request.Host.Host) < 0) && vm.ApplicationInfo.ApplicationSetting.RegistrationType == Model.Enums.RegistrationType.Tenant)
                    {
                        var platformUser = await _platformUserRepository.GetWithTenants(model.Username);

                        if (platformUser?.TenantsAsUser?.Count > 0)
                        {
                            var tenant = platformUser.TenantsAsUser.FirstOrDefault(x => x.Tenant.AppId == vm.ApplicationInfo.Id);

                            if (tenant == null)
                            {
                                await _signInManager.SignOutAsync();
                                vm.Error = "NotValidApp";

                                if (!vm.ApplicationInfo.Theme["custom"].IsNullOrEmpty())
                                    return View("Custom/Login" + vm.ApplicationInfo.Theme["custom"], vm);

                                return View(vm);
                            }
                        }
                        else
                        {
                            await _signInManager.SignOutAsync();
                            vm.Error = "NotValidApp";

                            if (!vm.ApplicationInfo.Theme["custom"].IsNullOrEmpty())
                                return View("Custom/Login" + vm.ApplicationInfo.Theme["custom"], vm);

                            return View(vm);
                        }
                    }

                    var user = await _userManager.FindByNameAsync(model.Username);

                    /*if (!string.IsNullOrEmpty(studioUrl) && studioUrl.Contains(vm.ApplicationInfo?.Domain))
                    {
                        var giteaToken = await _giteaHelper.GetToken(model.Username, model.Password);
                        if (!string.IsNullOrEmpty(giteaToken))
                        {
                            await _userManager.AddClaimAsync(user, new Claim("gitea_token", giteaToken));
                            Response.Cookies.Append("gitea_token", giteaToken);
                        }
                    }*/

                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName));

                    // make sure the returnUrl is still valid, and if so redirect back to authorize endpoint or a local page
                    // the IsLocalUrl check is only necessary if you want scriptRepositoryto support additional local pages, otherwise IsValidReturnUrl is more strict
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

            if (!vm.ApplicationInfo.Theme["custom"].IsNullOrEmpty())
                return View("Custom/Login" + vm.ApplicationInfo.Theme["custom"], vm);

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
                return RedirectToAction(nameof(AccountController.ChangeLanguage), "Account", new {language = vm.Language, returnUrl = vm.ReturnUrl});

            /*
            * If you want to make the user email field read only. Set true to ReadOnly variable.
            */
            vm.ReadOnly = false;

            if (!vm.ApplicationInfo.Theme["custom"].IsNullOrEmpty())
                return View("Custom/Register" + vm.ApplicationInfo.Theme["custom"], vm);

            return View(vm);
        }

        /// <summary>
        /// Handle postback from register page
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Register(RegisterInputModel model)
        {
            var vm = await BuildRegisterViewModelAsync(model);
            JArray actions = null;
            JToken action = null;
            JObject obj = null;

            var externalLoginChecker = false;
            if (!ModelState.IsValid)
            {
                vm.Error = "ModelStateNotValid";

                if (!vm.ApplicationInfo.Theme["custom"].IsNullOrEmpty())
                    return View("Custom/Register" + vm.ApplicationInfo.Theme["custom"], vm);

                return View(vm);
            }

            var externalLogin = vm.ApplicationInfo.ApplicationSetting.ExternalLogin != null ? JObject.Parse(vm.ApplicationInfo.ApplicationSetting.ExternalLogin) : null;

            if (externalLogin != null)
            {
                actions = (JArray)externalLogin["actions"];
                action = actions.Where(x => x["type"] != null && x["type"].ToString() == "login").FirstOrDefault();

                obj = new JObject
                {
                    ["email"] = model.Email,
                    ["password"] = model.Password
                };

                var externalLoginSignIn = await ExternalAuthHelper.Login(externalLogin, action, obj);

                if (externalLoginSignIn.Succeeded)
                    externalLoginChecker = true;
                else
                {
                    action = actions.Where(x => x["type"] != null && x["type"].ToString() == "check_user").FirstOrDefault();

                    var externalLoginCheckUser = await ExternalAuthHelper.CheckUser(externalLogin, action, obj);

                    if (externalLoginCheckUser.IsSuccessStatusCode)
                    {
                        vm.Error = "AlreadyRegisteredExternalLogin";
                        ViewBag.AuthFlowTitle = action["title"].ToString();
                        if (!vm.ApplicationInfo.Theme["custom"].IsNullOrEmpty())
                            return View("Custom/Register" + vm.ApplicationInfo.Theme["custom"], vm);

                        return View(vm);
                    }
                }
            }

            vm.ExternalLogin = externalLoginChecker;
            var createUserRespone = await CreateUser(model, vm.ApplicationInfo, vm.ReturnUrl);

            if (!string.IsNullOrEmpty(createUserRespone["Error"].ToString()))
            {
                vm.Error = createUserRespone["Error"].ToString();
                if (!vm.ApplicationInfo.Theme["custom"].IsNullOrEmpty())
                    return View("Custom/Register" + vm.ApplicationInfo.Theme["custom"], vm);

                return View(vm);
            }

            if (externalLogin != null && !vm.ExternalLogin)
            {
                obj = new JObject
                {
                    ["email"] = model.Email,
                    ["password"] = model.Password,
                    ["firstname"] = model.FirstName,
                    ["lastname"] = model.LastName,
                    ["fullname"] = model.FirstName + " " + model.LastName,
                    ["language"] = vm.ApplicationInfo.Language,
                    ["phone"] = model.PhoneNumber
                };

                action = actions.Where(x => x["type"] != null && x["type"].ToString() == "register").FirstOrDefault();

                await ExternalAuthHelper.Register(externalLogin, action, obj);
            }

            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await _signInManager.SignOutAsync();
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            var studioUrl = _configuration.GetValue("AppSettings:StudioUrl", string.Empty);
            if (!string.IsNullOrEmpty(studioUrl) && studioUrl.Contains(vm.ApplicationInfo?.Domain))
            {
                var giteaToken = await _giteaHelper.GetToken(model.Email, model.Password);
                if (!string.IsNullOrEmpty(giteaToken))
                {
                    var user = await _userManager.FindByNameAsync(model.Email);
                    await _userManager.AddClaimAsync(user, new Claim("gitea_token", giteaToken));
                    Response.Cookies.Append("gitea_token", giteaToken);
                }
            }

            var signInResult = await _signInManager.PasswordSignInAsync(model.Email, model.Password, true, lockoutOnFailure: false);

            if (signInResult.Succeeded)
                await _events.RaiseAsync(new UserLoginSuccessEvent(model.Email, createUserRespone["identity_user_id"].ToString(), model.Email));

            if (vm.ApplicationInfo != null)
                return Redirect(Request.Scheme + "://" + vm.ApplicationInfo.Domain);

            return RedirectToAction(nameof(HomeController), nameof(HomeController.Index));
        }

        [Route("api/account/create"), HttpPost]
        public async Task<IActionResult> Create(CreateAccountBindingModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var app = await _applicationRepository.GetByNameAsync(model.AppName);

            var application = new ApplicationInfoViewModel()
            {
                Id = app.Id,
                Name = app.Name,
                Language = model.Language ?? app.Setting.Language,
                Domain = app.Setting.AppDomain,
                ApplicationSetting = new ApplicationSettingViewModel
                {
                    Culture = model.Culture ?? app.Setting.Culture,
                    Currency = app.Setting.Currency,
                    TimeZone = app.Setting.TimeZone,
                    GoogleAnalytics = app.Setting.GoogleAnalyticsCode,
                    TenantOperationWebhook = app.Setting.TenantOperationWebhook,
                }
            };

            var createUserRespone = await CreateUser(new RegisterInputModel {Email = model.Email, Password = model.Password, Culture = model.Culture, FirstName = model.FirstName, LastName = model.LastName}, application, "");

            if (!string.IsNullOrEmpty(createUserRespone["Error"].ToString()))
                return BadRequest(new {ErrorMessage = createUserRespone["Error"].ToString()});

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail([FromQuery(Name = "email")]string email, [FromQuery(Name = "code")]string code)
        {
            //code = WebUtility.UrlDecode(code);

            var vm = BuildConfirmEmailViewModelAsync("");
            var user = await _userManager.FindByEmailAsync(email);

            if (email == null || code == null)
                vm.Error = "EmailConfirmMissingInfo";

            else if (user == null)
                vm.Error = "EmailConfirmUserNotFound";

            else if (user.EmailConfirmed)
                vm.Error = "EmailConfirmAlreadyConfirmed";
            else
            {
                var result = await _userManager.ConfirmEmailAsync(user, code);

                if (!result.Succeeded)
                    vm.Error = "EmailConfirmInvalidCode";
            }

            if (string.IsNullOrEmpty(vm.Error))
                vm.Success = "EmailConfirmed";

            return RedirectToAction(nameof(HomeController.Index), "Home", vm);
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
                return RedirectToAction(nameof(AccountController.ChangeLanguage), "Account", new {language = ViewBag.Language, returnUrl = Request.Path.Value + Request.QueryString.Value});

            //vm.ReadOnly = false;

            if (!vm.ApplicationInfo.Theme["custom"].IsNullOrEmpty())
                return View("Custom/ForgotPassword" + vm.ApplicationInfo.Theme["custom"], vm);

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
            {
                vm.Error = "ForgotNotFound";
                return View("Custom/ForgotPassword" + vm.ApplicationInfo.Theme["custom"], vm);
            }

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
                    if (!vm.ApplicationInfo.Theme["custom"].IsNullOrEmpty())
                        return View("Custom/ForgotPassword" + vm.ApplicationInfo.Theme["custom"], vm);

                    return View(vm);
                }
            }

            vm.Success = "Success";
            if (!vm.ApplicationInfo.Theme["custom"].IsNullOrEmpty())
                return View("Custom/ForgotPassword" + vm.ApplicationInfo.Theme["custom"], vm);

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword([FromQuery]string returnUrl, [FromQuery]string code, [FromQuery]Guid guid, string error)
        {
            var vm = await BuildResetPasswordViewModelAsync(returnUrl, HttpUtility.UrlDecode(code), guid, error: error);

            if (!vm.ApplicationInfo.Theme["custom"].IsNullOrEmpty())
                return View("Custom/ResetPassword" + vm.ApplicationInfo.Theme["custom"], vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, string client, string returnUrl = null)
        {
            var vm = await BuildResetPasswordViewModelAsync(model, returnUrl);

            if (string.IsNullOrEmpty(vm.Code))
            {
                vm.Error = "InvalidToken";
                if (!vm.ApplicationInfo.Theme["custom"].IsNullOrEmpty())
                    return View("Custom/ResetPassword" + vm.ApplicationInfo.Theme["custom"], vm);

                return View(vm);
            }

            var user = await _userManager.FindByIdAsync(vm.Guid.ToString());

            if (user == null)
            {
                vm.Error = "NotFound";
                if (!vm.ApplicationInfo.Theme["custom"].IsNullOrEmpty())
                    return View("Custom/ResetPassword" + vm.ApplicationInfo.Theme["custom"], vm);

                return View(vm);
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);

            if (result.Succeeded)
            {
                var application = await _applicationRepository.GetByNameAsync(client);
                var externalLogin = application.Setting.ExternalAuth != null ? JObject.Parse(application.Setting.ExternalAuth) : null;

                if (externalLogin != null)
                {
                    var actions = (JArray)externalLogin["actions"];
                    var action = actions.Where(x => x["type"] != null && x["type"].ToString() == "forgot_password").FirstOrDefault();

                    var obj = new JObject
                    {
                        ["email"] = user.Email,
                    };

                    var externalLoginForgotPasswordResult = await ExternalAuthHelper.ForgotPassword(externalLogin, action, obj);

                    if (externalLoginForgotPasswordResult.IsSuccessStatusCode)
                    {
                        action = actions.Where(x => x["type"] != null && x["type"].ToString() == "reset_password").FirstOrDefault();
                        var resetToken = await externalLoginForgotPasswordResult.Content.ReadAsStringAsync();

                        obj = new JObject
                        {
                            ["email"] = user.Email,
                            ["password_reset_token"] = resetToken,
                            ["new_password"] = model.Password
                        };

                        var externalLoginResetPasswordResult = await ExternalAuthHelper.ResetPassword(externalLogin, action, obj);

                        if (externalLoginResetPasswordResult.IsSuccessStatusCode)
                            return RedirectToAction("Login", "Account", new {vm.ReturnUrl, Success = "PasswordChanged"});
                        else
                        {
                            vm.Error = "InvalidToken";
                            if (!vm.ApplicationInfo.Theme["custom"].IsNullOrEmpty())
                                return View("Custom/ResetPassword" + vm.ApplicationInfo.Theme["custom"], vm);

                            return View(vm);
                        }
                    }
                }

                return RedirectToAction("Login", "Account", new {vm.ReturnUrl, Success = "PasswordChanged"});
            }

            vm.Error = "InvalidToken";
            if (!vm.ApplicationInfo.Theme["custom"].IsNullOrEmpty())
                return View("Custom/ResetPassword" + vm.ApplicationInfo.Theme["custom"], vm);

            return View(vm);
        }

        [HttpPost, AllowAnonymous]
        public async Task<bool> ExternalLoginForgotPassword([FromBody]ExternalLoginBindingModel model)
        {
            var application = await _applicationRepository.GetByNameAsync(model.client);
            var externalLogin = application.Setting.ExternalAuth != null ? JObject.Parse(application.Setting.ExternalAuth) : null;

            if (externalLogin != null)
            {
                var actions = (JArray)externalLogin["actions"];
                var action = actions.Where(x => x["type"] != null && x["type"].ToString() == "forgot_password").FirstOrDefault();

                var obj = new JObject
                {
                    ["email"] = model.email
                };

                var result = await ExternalAuthHelper.ForgotPassword(externalLogin, action, obj);

                return result.IsSuccessStatusCode;
            }

            return false;
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
                        {"returnUrl", returnUrl},
                        {"scheme", provider},
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
                var model = new RegisterInputModel();
                model.FirstName = result.Principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname");
                model.LastName = result.Principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname");
                model.Email = user.UserName;
                model.Culture = CultureInfo.CurrentCulture.Name;
                model.Password = Utils.GenerateRandomUnique(8);


                if (!string.IsNullOrEmpty(clientId))
                {
                    var platformUser = await _platformUserRepository.GetWithTenants(user.UserName);
                    var appInfo = await _applicationRepository.GetByNameAsync(clientId);
                    var userApp = platformUser?.TenantsAsUser.Where(x => x.Tenant.AppId == appInfo.Id);

                    var theme = JObject.Parse(appInfo.Setting.AuthTheme);

                    var _language = !string.IsNullOrEmpty(Request.Cookies[".AspNetCore.Culture"]) ? Request.Cookies[".AspNetCore.Culture"].Split("uic=")[1] : null;

                    var cdnUrlStatic = "";
                    var cdnUrl = _configuration.GetValue("webOptimizer:cdnUrl", string.Empty);

                    if (!string.IsNullOrEmpty(cdnUrl))
                    {
                        var versionStatic = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                        cdnUrlStatic = cdnUrl + "/" + versionStatic;
                    }

                    var application = new ApplicationInfoViewModel()
                    {
                        Id = appInfo.Id,
                        Name = appInfo.Name,
                        Title = theme["title"].ToString(),
                        MultiLanguage = string.IsNullOrEmpty(appInfo.Setting.Language),
                        Logo = appInfo.Logo,
                        Theme = theme,
                        Color = theme["color"].ToString(),
                        CustomDomain = false,
                        Language = _language,
                        Favicon = theme["favicon"].ToString(),
                        CdnUrl = cdnUrlStatic,
                        Domain = appInfo.Setting.AppDomain,
                        ApplicationSetting = new ApplicationSettingViewModel
                        {
                            Culture = appInfo.Setting.Culture,
                            Currency = appInfo.Setting.Currency,
                            TimeZone = appInfo.Setting.TimeZone,
                            GoogleAnalytics = appInfo.Setting.GoogleAnalyticsCode,
                            TenantOperationWebhook = appInfo.Setting.TenantOperationWebhook,
                        }
                    };

                    if (platformUser == null || userApp == null)
                    {
                        var createUserResponse = await CreateUser(model, application, "", true);

                        if (!string.IsNullOrEmpty(createUserResponse["Error"].ToString()))
                        {
                            /*vm.Error = createUserResponse["Error"].ToString();
                            return View(vm);*/
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

            /*if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout(vm);
            }*/

            Response.Cookies.Delete("gitea_token");
            return await Logout(vm);
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
                string url = Url.Action("Logout", new {logoutId = vm.LogoutId});

                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties {RedirectUri = url}, vm.ExternalAuthenticationScheme);
            }

            if (!string.IsNullOrEmpty(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            Response.Cookies.Delete("gitea_token");
            return Redirect(Request.Scheme + "://" + Request.Host.Value);
            //return View("LoggedOut", vm);
        }

        /// <summary>
        /// Create identity user. Only identity user, not platform or tenant user.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateIdentityUser([FromBody]CreateAccountBindingModel model)
        {
            var identityUser = await _userManager.FindByNameAsync(model.Email);

            if (identityUser != null)
                return BadRequest("User exist");

            var applicationUser = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                NormalizedEmail = model.Email,
                NormalizedUserName = !string.IsNullOrEmpty(model.FirstName) ? model.FirstName + " " + model.LastName : ""
            };

            var result = await _userManager.CreateAsync(applicationUser, model.Password);

            if (!result.Succeeded)
                throw new Exception(result.Errors.ToJsonString());

            result = await _userManager.AddClaimsAsync(applicationUser, new Claim[]
            {
                new Claim(JwtClaimTypes.Name, !string.IsNullOrEmpty(model.FirstName) ? model.FirstName + " " + model.LastName : ""),
                new Claim(JwtClaimTypes.GivenName, model.FirstName),
                new Claim(JwtClaimTypes.FamilyName, model.LastName),
                new Claim(JwtClaimTypes.Email, model.Email),
                new Claim(JwtClaimTypes.EmailVerified, "false", ClaimValueTypes.Boolean)
            });

            if (!result.Succeeded)
                throw new Exception(result.Errors.ToJsonString());

            return Ok();
        }

        /*****************************************/
        /* helper APIs for the AccountController */
        /*****************************************/

        private async Task<ResetPasswordViewModel> BuildResetPasswordViewModelAsync(string returnUrl, string code, Guid guid, string success = "", string error = "")
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            var applicationInfo = await AuthHelper.GetApplicationInfo(_configuration, Request, returnUrl, _applicationRepository);

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
            var applicationInfo = await AuthHelper.GetApplicationInfo(_configuration, Request, returnUrl, _applicationRepository);

            return new ForgotPasswordViewModel
            {
                ReturnUrl = returnUrl,
                ApplicationInfo = applicationInfo,
                Language = applicationInfo.Language,
                Success = success,
                Error = error
            };
        }

        private ConfirmEmailViewModel BuildConfirmEmailViewModelAsync(string returnUrl, string success = "", string error = "")
        {
            //var applicationInfo = await AuthHelper.GetApplicationInfoAsync(Configuration, Request, returnUrl, _applicationRepository);

            return new ConfirmEmailViewModel
            {
                ReturnUrl = returnUrl,
                //ApplicationInfo = applicationInfo,
                Language = CultureInfo.CurrentCulture.Name,
                Success = success,
                Error = error
            };
        }

        private async Task<ForgotPasswordViewModel> BuildForgotPasswordViewModelAsync(ForgotPasswordViewModel model, string returnUrl, string success = "", string error = "")
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            var applicationInfo = await AuthHelper.GetApplicationInfo(_configuration, Request, returnUrl, _applicationRepository);

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
            var applicationInfo = await AuthHelper.GetApplicationInfo(_configuration, Request, returnUrl, _applicationRepository);

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
                Language = applicationInfo?.Language,
                Success = success,
                Error = error
            };
        }

        private async Task<ApplicationViewModel> BuildIndexViewModelAsync(string returnUrl, string success = "", string error = "")
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            var applicationInfo = await AuthHelper.GetApplicationInfo(_configuration, Request, returnUrl, _applicationRepository);

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
            var applicationInfo = await AuthHelper.GetApplicationInfo(_configuration, Request, returnUrl, _applicationRepository);

            if (context?.IdP != null)
            {
                // this is meant to short circuit the UI and only trigger the one external IdP

                return new LoginViewModel
                {
                    EnableLocalLogin = false,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
                    ExternalProviders = new ExternalProvider[] {new ExternalProvider {AuthenticationScheme = context.IdP}},
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
            var vm = new LogoutViewModel {LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt};

            vm.ReturnUrl = returnUrl;

            if (User?.Identity.IsAuthenticated != true)
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
            return vm;
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
                        {"returnUrl", returnUrl},
                        {"scheme", AccountOptions.WindowsAuthenticationSchemeName},
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
                localSignInProps.StoreTokens(new[] {new AuthenticationToken {Name = "id_token", Value = id_token}});
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

        private async void DatabaseRollback(ApplicationUser user, bool newIdentityUser, bool newPlatformUser, IPlatformUserRepository platformUserRepository, ITenantRepository tenantRepository, Tenant tenant = null, PlatformUser platformUser = null)
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

        private async Task DeactivateUser(Tenant tenant)
        {
            await _tenantRepository.DeleteAsync(tenant);
        }

        private async Task<JObject> CreateUser(RegisterInputModel model, ApplicationInfoViewModel applicationInfo, string returnUrl, bool externalLogin = false)
        {
            var response = new JObject()
            {
                ["Error"] = null
            };

            if (applicationInfo.ApplicationSetting.RegistrationType == Model.Enums.RegistrationType.External)
                return response;

            var identityUser = await _userManager.FindByNameAsync(model.Email);
            var newIdentityUser = false;
            if (identityUser == null && !externalLogin)
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
                    response["Error"] = "UserNotCreated";
                    return response;
                }

                result = await _userManager.AddClaimsAsync(applicationUser, new Claim[]
                {
                    new Claim(JwtClaimTypes.Name, !string.IsNullOrEmpty(model.FirstName) ? model.FirstName + " " + model.LastName : ""),
                    new Claim(JwtClaimTypes.GivenName, model.FirstName),
                    new Claim(JwtClaimTypes.FamilyName, model.LastName),
                    new Claim(JwtClaimTypes.Email, model.Email),
                    new Claim(JwtClaimTypes.EmailVerified, "false", ClaimValueTypes.Boolean)
                });

                identityUser = await _userManager.FindByEmailAsync(model.Email);
            }
            else if (identityUser != null && applicationInfo.ApplicationSetting.RegistrationType == Model.Enums.RegistrationType.Studio)
            {
                response["Error"] = "UserExist";
                return response;
            }

            if (applicationInfo != null)
            {
                var token = "";
                var culture = !string.IsNullOrEmpty(model.Culture) ? model.Culture : applicationInfo.ApplicationSetting.Culture;

                if (!externalLogin && !identityUser.EmailConfirmed)
                    token = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);

                var newPlatformUser = false;
                PlatformUser platformUser = await _platformUserRepository.GetWithTenants(model.Email);

                if (platformUser != null)
                {
                    if (applicationInfo.ApplicationSetting.RegistrationType == Model.Enums.RegistrationType.Tenant)
                    {
                        var appTenant = platformUser.TenantsAsUser?.FirstOrDefault(x => x.Tenant.AppId == applicationInfo.Id);

                        if (appTenant != null)
                        {
                            response["Error"] = "AlreadyRegisterForApp";
                            return response;
                        }
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
                        platformUser.Setting.Currency = culture.Substring(0, 2);
                    }
                    else
                    {
                        platformUser.Setting.Culture = applicationInfo.ApplicationSetting.Culture;
                        platformUser.Setting.Currency = applicationInfo.Language;
                        platformUser.Setting.Language = applicationInfo.Language;
                        platformUser.Setting.TimeZone = applicationInfo.ApplicationSetting.TimeZone;
                    }

                    var result = await _platformUserRepository.CreateUser(platformUser);

                    if (result == 0)
                    {
                        DatabaseRollback(identityUser, newIdentityUser, newPlatformUser, null, null);
                        response["Error"] = "UnexpectedError";
                        return response;
                    }

                    platformUser = await _platformUserRepository.GetWithTenants(model.Email);
                }

                if (applicationInfo.ApplicationSetting.RegistrationType == Model.Enums.RegistrationType.Tenant)
                {
                    var tenantId = 0;
                    Tenant tenant = null;
                    //var tenantId = 2032;
                    try
                    {
                        tenant = new Tenant
                        {
                            //Id = tenantId,
                            AppId = applicationInfo.Id,
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
                                Culture = applicationInfo.ApplicationSetting.Culture,
                                Currency = applicationInfo.ApplicationSetting.Currency,
                                Language = applicationInfo.Language,
                                TimeZone = applicationInfo.ApplicationSetting.TimeZone
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
                            Currency = applicationInfo.ApplicationSetting.Currency,
                            CreatedAt = platformUser.CreatedAt,
                            CreatedByEmail = platformUser.Email
                        };

                        await Postgres.CreateDatabaseWithTemplate(_tenantRepository.DbContext.Database.GetDbConnection().ConnectionString, tenantId, applicationInfo.Id);

                        _userRepository.CurrentUser = new CurrentUser {TenantId = tenantId, UserId = platformUser.Id, PreviewMode = "tenant"};
                        _profileRepository.CurrentUser = new CurrentUser {TenantId = tenantId, UserId = platformUser.Id, PreviewMode = "tenant"};
                        _roleRepository.CurrentUser = new CurrentUser {TenantId = tenantId, UserId = platformUser.Id, PreviewMode = "tenant"};
                        _recordRepository.CurrentUser = new CurrentUser {TenantId = tenantId, UserId = platformUser.Id, PreviewMode = "tenant"};

                        _profileRepository.TenantId = _roleRepository.TenantId = _userRepository.TenantId = _recordRepository.TenantId = tenantId;

                        tenantUser.IsSubscriber = true;
                        await _userRepository.CreateAsync(tenantUser);

                        var userProfile = await _profileRepository.GetDefaultAdministratorProfileAsync();
                        var userRole = await _roleRepository.GetByIdAsync(1);

                        tenantUser.Profile = userProfile;
                        tenantUser.Role = userRole;


                        await _userRepository.UpdateAsync(tenantUser);
                        await _recordRepository.UpdateSystemData(platformUser.Id, DateTime.UtcNow, tenant.Setting.Language, applicationInfo.Id);

                        if (platformUser.TenantsAsUser == null)
                            platformUser.TenantsAsUser = new List<UserTenant>();

                        platformUser.TenantsAsUser.Add(new UserTenant {Tenant = tenant, PlatformUser = platformUser});

                        //user.TenantId = user.Id;
                        //tenant.License.HasAnalyticsLicense = true;
                        await _platformUserRepository.UpdateAsync(platformUser);
                        await _tenantRepository.UpdateAsync(tenant);

                        await _recordRepository.UpdateSampleData(platformUser);
                        //await Cache.ApplicationUser.Add(user.Email, user.Id);
                        //await Cache.User.Get(user.Id);

                        var url = Request.Scheme + "://" + applicationInfo.Domain + "/api/account/user_created";

                        var requestModel = new JObject
                        {
                            ["email"] = model.Email,
                            ["app_id"] = applicationInfo.Id,
                            ["guid_id"] = identityUser.Id,
                            ["tenant_language"] = tenant.Setting.Language,
                            ["code"] = token,
                            ["user_exist"] = newPlatformUser,
                            ["email_confirmed"] = identityUser.EmailConfirmed,
                            ["culture"] = culture,
                            ["first_name"] = model.FirstName,
                            ["last_name"] = model.LastName,
                            ["return_url"] = returnUrl
                        };

                        using (var httpClient = new HttpClient())
                        {
                            httpClient.BaseAddress = new Uri(url);
                            httpClient.DefaultRequestHeaders.Accept.Clear();
                            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                            var userCreatedResponse = await httpClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(requestModel), Encoding.UTF8, "application/json"));

                            /*if (!userCreatedResponse.IsSuccessStatusCode)
                            {
                                DatabaseRollback(identityUser, newIdentityUser, newPlatformUser, _platformUserRepository, _tenantRepository, tenant, platformUser);
                                response["Error"] = "TenantCreateError";
                                return response;
                                //TODO Loglara Eklenebilir.
                            }*/
                        }

                        //TODO Buraya webhook eklenecek. AppSetting üzerindeki TenantCreateWebhook alanı dolu kontrol edilecek doluysa bu url'e post edilecek
                        Queue.QueueBackgroundWorkItem(x => AuthHelper.TenantOperationWebhook(applicationInfo, tenant, tenantUser));

                        response["Success"] = true;
                    }
                    catch (Exception ex)
                    {
                        DatabaseRollback(identityUser, newIdentityUser, newPlatformUser, _platformUserRepository, _tenantRepository, tenant, platformUser);

                        throw ex;
                    }
                }

                var studioUrl = _configuration.GetValue("AppSettings:StudioUrl", string.Empty);
                if (identityUser != null && applicationInfo.ApplicationSetting.RegistrationType == RegistrationType.Studio && !string.IsNullOrEmpty(studioUrl))
                {
                    var cryptId = CryptoHelper.Encrypt(platformUser.Id.ToString());
                    var url = studioUrl + "/api/account/create";

                    var requestModel = new JObject
                    {
                        ["id"] = cryptId,
                        ["email"] = platformUser.Email,
                        ["password"] = model.Password,
                        ["first_name"] = platformUser.FirstName,
                        ["last_name"] = platformUser.LastName,
                        ["email"] = model.Email,
                        ["app_id"] = 2,
                        ["code"] = token,
                        ["culture"] = CultureInfo.CurrentCulture.Name,
                        ["user_exist"] = newPlatformUser,
                        ["email_confirmed"] = identityUser.EmailConfirmed
                    };

                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Accept.Clear();
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var userCreatedResponse = await httpClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(requestModel), Encoding.UTF8, "application/json"));

                        if (!userCreatedResponse.IsSuccessStatusCode)
                        {
                            var result = await userCreatedResponse.Content?.ReadAsStringAsync();

                            if (string.IsNullOrEmpty(result))
                                result = "";

                            ErrorHandler.LogError(new Exception(result), "Studio user create failed. StatusCode: " + userCreatedResponse.StatusCode + ", Url: " + url + ", Request: " + requestModel.ToJsonString());
                        }
                    }
                }
            }

            response["identity_user_id"] = identityUser.Id;

            return response;
        }
    }
}