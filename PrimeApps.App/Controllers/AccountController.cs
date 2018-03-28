using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Threading;
using System.Globalization;
using System.Net;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.App.Providers;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Entities.Platform.Identity;
using Npgsql;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Helpers.QueryTranslation;
using ChallengeResult = PrimeApps.App.Results.ChallengeResult;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;

namespace PrimeApps.App.Controllers
{
    [Route("api/account")]
    public class AccountController : BaseController
    {
        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;
        private IUserRepository _userRepository;
        private IProfileRepository _profileRepository;
        private IRoleRepository _roleRepository;
        private IRecordRepository _recordRepository;
        private IPlatformUserRepository _platformUserRepository;
        private ITenantRepository _tenantRepository;
        private Warehouse _warehouse;

        public AccountController(ApplicationUserManager userManager, ISecureDataFormat<AuthenticationTicket> accessTokenFormat, IProfileRepository profileRepository, IUserRepository userRepository, IRoleRepository roleRepository, IRecordRepository recordRepository, IPlatformUserRepository platformUserRepository, ITenantRepository tenantRepository, Warehouse warehouse) : this(profileRepository, userRepository, roleRepository, recordRepository, platformUserRepository, tenantRepository, warehouse)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public AccountController(IProfileRepository profileRepository, IUserRepository userRepository, IRoleRepository roleRepository, IRecordRepository recordRepository, IPlatformUserRepository platformUserRepository, ITenantRepository tenantRepository, Warehouse warehouse) : this()
        {
            _userRepository = userRepository;
            _profileRepository = profileRepository;
            _roleRepository = roleRepository;
            _recordRepository = recordRepository;
            _warehouse = warehouse;
            _platformUserRepository = platformUserRepository;
            _tenantRepository = tenantRepository;

            //Set warehouse database name Ofisim to integration
            //_warehouse.DatabaseName = "Ofisim";
        }
        public AccountController() { }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        // GET account/user_info
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("user_info")]
        public UserInfoViewModel GetUserInfo()
        {
            var externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            return new UserInfoViewModel
            {
                Email = User.Identity.GetUserName(),
                HasRegistered = externalLogin == null,
                LoginProvider = externalLogin != null ? externalLogin.LoginProvider : null
            };
        }

        // POST account/logout
        [Route("logout")]
        public IActionResult Logout()
        {
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }

        // GET account/manage_info?returnUrl=&generateState=
        [Route("manage_info")]
        public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
        {
            var user = await UserManager.FindByIdAsync(int.Parse(User.Identity.GetUserId()));

            if (user == null)
            {
                return null;
            }

            var logins = new List<UserLoginInfoViewModel>();

            foreach (ApplicationUserLogin linkedAccount in user.Logins)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = linkedAccount.LoginProvider,
                    ProviderKey = linkedAccount.ProviderKey
                });
            }

            if (user.PasswordHash != null)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = LocalLoginProvider,
                    ProviderKey = user.UserName,
                });
            }

            return new ManageInfoViewModel
            {
                LocalLoginProvider = LocalLoginProvider,
                Email = user.UserName,
                Logins = logins,
                ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
            };
        }

        // POST account/change_password
        [Route("change_password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var prefixAvailable = User.Identity.Name.StartsWith("pre__");

            var result = await UserManager.ChangePasswordAsync(int.Parse(User.Identity.GetUserId()), model.OldPassword, model.NewPassword);

            var otherAppEmail = "";

            if (!prefixAvailable)
                otherAppEmail = "pre__" + User.Identity.Name;
            else
                otherAppEmail = Regex.Replace(User.Identity.Name, "^pre__", "");

            var otherAppUser = await _platformUserRepository.Get(otherAppEmail);

            if (otherAppUser != null)
            {
                var mainAppUser = await _platformUserRepository.Get(User.Identity.Name);
                otherAppUser.PasswordHash = mainAppUser.PasswordHash;
                await _platformUserRepository.UpdateAsync(otherAppUser);
            }

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST account/set_password
        [Route("set_password")]
        public async Task<IActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await UserManager.AddPasswordAsync(int.Parse(User.Identity.GetUserId()), model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST account/add_external
        [Route("add_external")]
        public async Task<IActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            var ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);

            if (ticket == null || ticket.Identity == null || (ticket.Properties != null
                && ticket.Properties.ExpiresUtc.HasValue
                && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
            {
                return BadRequest("External login failure.");
            }

            var externalData = ExternalLoginData.FromIdentity(ticket.Identity);

            if (externalData == null)
            {
                return BadRequest("The external login is already associated with an account.");
            }

            var result = await UserManager.AddLoginAsync(int.Parse(User.Identity.GetUserId()), new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST account/remove_login
        [Route("remove_login")]
        public async Task<IActionResult> RemoveLogin(RemoveLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result;

            if (model.LoginProvider == LocalLoginProvider)
            {
                result = await UserManager.RemovePasswordAsync(int.Parse(User.Identity.GetUserId()));
            }
            else
            {
                result = await UserManager.RemoveLoginAsync(int.Parse(User.Identity.GetUserId()), new UserLoginInfo(model.LoginProvider, model.ProviderKey));
            }

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // GET account/external
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        [AllowAnonymous]
        [Route("external", Name = "ExternalLogin")]
        public async Task<IActionResult> GetExternalLogin(string provider, string error = null)
        {
            if (error != null)
            {
                return Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));
            }

            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(provider, this);
            }

            var externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            if (externalLogin == null)
            {
                return InternalServerError();
            }

            if (externalLogin.LoginProvider != provider)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return new ChallengeResult(provider, this);
            }

            var user = await UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider, externalLogin.ProviderKey));

            var hasRegistered = user != null;

            if (hasRegistered)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

                var oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager, OAuthDefaults.AuthenticationType);
                var cookieIdentity = await user.GenerateUserIdentityAsync(UserManager, CookieAuthenticationDefaults.AuthenticationType);
                var properties = ApplicationOAuthProvider.CreateProperties(user.UserName);

                Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);
            }
            else
            {
                var claims = externalLogin.GetClaims();
                var identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);

                Authentication.SignIn(identity);
            }

            return Ok();
        }

        // GET account/externals?returnUrl=&generateState=
        [AllowAnonymous]
        [Route("externals")]
        public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        {
            var descriptions = Authentication.GetExternalAuthenticationTypes();
            var logins = new List<ExternalLoginViewModel>();

            string state;

            if (generateState)
            {
                const int strengthInBits = 256;
                state = RandomOAuthStateGenerator.Generate(strengthInBits);
            }
            else
            {
                state = null;
            }

            foreach (var description in descriptions)
            {
                var login = new ExternalLoginViewModel
                {
                    Name = description.Caption,
                    Url = Url.Route("ExternalLogin", new
                    {
                        provider = description.AuthenticationType,
                        response_type = "token",
                        client_id = Startup.PublicClientId,
                        redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
                        state = state
                    }),
                    State = state
                };
                logins.Add(login);
            }

            return logins;
        }

        // POST account/register
        [AllowAnonymous]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!string.IsNullOrWhiteSpace(model.Culture) && Helpers.Constants.CULTURES.Contains(model.Culture))
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(model.Culture);

            var currency = "";
            if (Helpers.Constants.CURRENCIES.Contains(model.Currency))
            {
                //value is valid and supported, assign it as currency.
            }

            if (currency == string.Empty)
            {
                //currency is still not assigned. assign it based on language.
                currency = Thread.CurrentThread.CurrentCulture.Name == "tr-TR" ? "TRY" : "EUR";
            }

            var user = new PlatformUser()
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.Phone,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Culture = model.Culture,
                Currency = currency,
                CreatedAt = DateTime.Now,
                AppId = model.AppID > 0 ? model.AppID : 1
            };

            var result = await UserManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            var token = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);

            //Insert user to Ofisim CRM account
            HostingEnvironment.QueueBackgroundWorkItem(clt => Integration.InsertSubscriber(model, _warehouse));

            //if (!isFreeLicense)
            //    HostingEnvironment.QueueBackgroundWorkItem(clt => Integration.InsertSubscriber(model, _warehouse));
            //else
            //    HostingEnvironment.QueueBackgroundWorkItem(clt => Integration.InsertUser(model, _warehouse));

            if (model.OfficeSignIn)
            {
                var automaticAccountActivationModel = new AutomaticAccountActivationModel
                {
                    Token = token,
                    Id = user.Id
                };
                return Ok(automaticAccountActivationModel);
            }

            SendActivationMail(token, false, user.Id, user.FirstName, user.LastName, user.Email, user.AppId);

            return Ok();
        }

        // POST account/register_external
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Route("register_external")]
        public async Task<IActionResult> RegisterExternal(RegisterExternalBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var info = await Authentication.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return InternalServerError();
            }

            var user = new PlatformUser() { UserName = model.Email, Email = model.Email };

            var result = await UserManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            result = await UserManager.AddLoginAsync(user.Id, info.Login);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // GET account/activate?userId=&token=&culture=
        [HttpGet]
        [AllowAnonymous]
        [Route("activate")]
        public async Task<IActionResult> Activate(string userId = "", string token = "", string culture = "", bool officeSignIn = false)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                ModelState.AddModelError("", "userId and token are required");
                return BadRequest(ModelState);
            }

            var userIdInt = int.Parse(userId);
            var confirmResponse = await UserManager.ConfirmEmailAsync(userIdInt, token);

            if (confirmResponse.Succeeded)
            {
                await UserManager.UpdateSecurityStampAsync(userIdInt);

                PlatformUser user = await _platformUserRepository.Get(userIdInt);

                if (user.TenantId.HasValue)
                {
                    ModelState.AddModelError("", "User is already activated");
                    return BadRequest(ModelState);
                }

                try
                {
                    var tenant = new Tenant
                    {
                        Id = user.Id,
                        Language = user.Culture.Substring(0, 2),
                        Owner = user,
                        GuidId = Guid.NewGuid(),
						UserLicenseCount = 5
                    };

                    await _tenantRepository.UpdateAsync(tenant);
                    user.Tenant = tenant;
                    await _platformUserRepository.UpdateAsync(user);

                    if (!string.IsNullOrWhiteSpace(culture) && Helpers.Constants.CULTURES.Contains(culture))
                        Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);

                    var tenantUser = new TenantUser
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        FullName = $"{user.FirstName} {user.LastName}",
                        IsActive = true,
                        IsSubscriber = false,
                        Culture = user.Culture,
                        Currency = user.Currency,
                        CreatedAt = user.CreatedAt,
                        CreatedByEmail = user.Email
                    };



                    await Postgres.CreateDatabaseWithTemplate(user.Id, user.AppId);
                    _profileRepository.TenantId = _roleRepository.TenantId = _userRepository.TenantId = _recordRepository.TenantId = user.Id;

                    tenantUser.IsSubscriber = true;
                    await _userRepository.CreateAsync(tenantUser);

                    var userProfile = await _profileRepository.GetDefaultAdministratorProfileAsync();
                    var userRole = await _roleRepository.GetByIdAsync(1);

                    tenantUser.Profile = userProfile;
                    tenantUser.Role = userRole;

                    await _userRepository.UpdateAsync(tenantUser);
                    await _recordRepository.UpdateSystemData(user.Id, DateTime.UtcNow, tenant.Language, user.AppId);

                    if (user.AppId == 1 || user.AppId == 2)
                        await _recordRepository.InsertSampleData(user.Id, tenant.Language, user.AppId);
                    else
                        await _recordRepository.UpdateSampleData(user);

                    if (officeSignIn)
                        user.ActiveDirectoryEmail = user.Email;

                    HostingEnvironment.QueueBackgroundWorkItem(clt => DocumentHelper.UploadSampleDocuments(user.Tenant.GuidId, user.AppId, tenant.Language));

                    user.TenantId = user.Id;
                    tenant.HasAnalyticsLicense = true;
                    await _platformUserRepository.UpdateAsync(user);
                    await _tenantRepository.UpdateAsync(tenant);

                    await Cache.ApplicationUser.Add(user.Email, user.Id);
                    await Cache.User.Get(user.Id);

                    HostingEnvironment.QueueBackgroundWorkItem(clt => Integration.UpdateSubscriber(user.Email, user.TenantId.Value, _warehouse));
                }
                catch (Exception ex)
                {
                    Postgres.DropDatabase(user.TenantId.Value, true);

                    await DeactivateUser(user);

                    throw ex;
                }

                return Ok();
            }

            return GetErrorResult(confirmResponse);
        }

        // GET account/resend_activation?email=&culture=
        [HttpGet]
        [AllowAnonymous]
        [Route("resend_activation")]
        public async Task<IActionResult> ResendActivationMail(string email = "", string culture = "")
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "Email are required");
                return BadRequest(ModelState);
            }

            var user = await UserManager.FindByNameAsync(email);

            if (user == null)
            {
                ModelState.AddModelError("not_found", "User not found");
                return BadRequest(ModelState);
            }

            if (!string.IsNullOrWhiteSpace(culture) && Helpers.Constants.CULTURES.Contains(culture))
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);

            var isFreeLicense = user.Id == user.TenantId;

            if (user.TenantId > 0)
            {
                ModelState.AddModelError("", "User has already activated");
                return BadRequest(ModelState);
            }

            var token = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
            SendActivationMail(token, isFreeLicense, user.Id, user.FirstName, user.LastName, user.Email, user.AppId);

            return Ok();
        }

        // GET account/forgot_password?email=&culture=
        [HttpGet]
        [AllowAnonymous]
        [Route("forgot_password")]
        public async Task<IActionResult> ForgotPassword(string email = "", string culture = "")
        {

            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "Email is required");
                return BadRequest(ModelState);
            }

            var user = await UserManager.FindByNameAsync(email);

            if (user == null)
            {
                ModelState.AddModelError("not_found", "User not found");
                return BadRequest(ModelState);
            }

            if (!user.EmailConfirmed)
            {
                ModelState.AddModelError("not_activated", "Email not confirmed");
                return BadRequest(ModelState);
            }

            if (!string.IsNullOrWhiteSpace(culture) && Helpers.Constants.CULTURES.Contains(culture))
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);

            var token = await UserManager.GeneratePasswordResetTokenAsync(user.Id);

            var fullName = user.FirstName + " " + user.LastName;

            SendPasswordReset(token, user.Id, email, fullName, user.AppId);

            return Ok();
        }

        // POST account/reset_password
        [HttpPost]
        [AllowAnonymous]
        [Route("reset_password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await UserManager.ResetPasswordAsync(model.UserId, model.Token, model.Password);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST account/register_client
        [HttpPost]
        [AllowAnonymous]
        [Route("register_client")]
        public async Task<IActionResult> RegisterClient(ClientBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var client = new Client();

            if (string.IsNullOrWhiteSpace(model.Id))
                client.Id = Guid.NewGuid().ToString("n");
            else
                client.Id = (new Guid(model.Id)).ToString("n");

            client.Secret = AuthHelper.GetHash(client.Id);
            client.Name = model.Name;
            client.ApplicationType = model.ApplicationType;
            client.Active = false;
            client.RefreshTokenLifeTime = model.RefreshTokenLifeTime;
            client.AllowedOrigin = model.AllowedOrigin;

            bool result;

            using (var auth = new Auth())
            {
                result = await auth.AddClient(client);
            }

            if (!result)
            {
                ModelState.AddModelError("client_exists", "Client already exists");
                return BadRequest(ModelState);
            }

            return Ok();
        }

        // POST account/change_email
        [Route("change_email")]
        public async Task<IActionResult> ChangeEmail(ChangeEmailBindingModel model)
        {
            if (!AppUser.Email.EndsWith("@ofisim.com"))
                return StatusCode(HttpStatusCode.Forbidden);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UserManager.FindByNameAsync(model.CurrentEmail);

            if (user == null)
                return BadRequest("User not found!");

            user.Email = model.NewEmail;
            user.UserName = model.NewEmail;

            _userRepository.TenantId = user.TenantId;

            if (_userRepository.TenantId > 0)
            {
                try
                {
                    var tenantUser = await _userRepository.GetByEmail(model.CurrentEmail);

                    if (tenantUser == null)
                        return BadRequest("User not found!");

                    tenantUser.Email = model.NewEmail;
                    await _userRepository.UpdateAsync(tenantUser);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException is PostgresException)
                    {
                        var innerEx = (PostgresException)ex.InnerException;

                        if (innerEx.SqlState != PostgreSqlStateCodes.DatabaseDoesNotExist)
                            throw;
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            var result = await UserManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await Cache.User.Remove(user.Id);
                //TODO: Update roles
            }

            return Ok();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("change_tenant")]
        public IActionResult ChangeTenant(int userId, int tenantId, int appId, string email)
        {
            UserApp userApp = null;

            using (var dbContext = new PlatformDBContext())
            {
                userApp = dbContext.UserApps.FirstOrDefault(x => x.UserId == userId && x.TenantId == tenantId && x.AppId == appId);

                if (userApp == null && userId != tenantId)
                {
                    ModelState.AddModelError("", "App is not active for this user.");
                    return BadRequest(ModelState);
                }
            }

            return Ok();
        }

        public static AuthenticationProperties CreateProperties(string userName, string clientId = null)
        {
            var data = new Dictionary<string, string>
            {
                {"userName", userName}
            };

            if (clientId != null)
                data.Add("as:client_id", clientId);

            return new AuthenticationProperties(data);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private IActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name)
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }

        private void SendActivationMail(string token, bool isFreeLicense, int userId, string firstName, string lastName, string email, int appId)
        {
            //create an email to send to the user for activation link.
            var emailData = new Dictionary<string, string>();
            string domain;

            if (!HttpContext.Current.Request.IsLocal)
            {
                domain = "https://{0}.ofisim.com/";
                var appDomain = "crm";

                switch (appId)
                {
                    case 2:
                        appDomain = "kobi";
                        break;
                    case 3:
                        appDomain = "asistan";
                        break;
                    case 4:
                        appDomain = "ik";
                        break;
                    case 5:
                        appDomain = "cagri";
                        break;
                }

                var subdomain = ConfigurationManager<>.AppSettings.Get("TestMode") == "true" ? "test" : appDomain;
                domain = string.Format(domain, subdomain);
            }
            else
            {
                domain = "http://localhost:5554/";
            }

            var url = !isFreeLicense ? domain + "auth/activation?token={0}&uid={1}&app={2}" : domain + "auth/activation?token={0}&uid={1}&app={2}&free=true";

            var tenantId = 0;

            // TODO: Pending Share Request
            //if (isFreeLicense)
            //{
            //    using (var session = Provider.GetSession())
            //    {
            //        var shareRequest = session
            //            .Query<crmPendingShareRequests>()
            //            .Fetch(x => x.Instance)
            //            .ThenFetch(x => x.Admin)
            //            .SingleOrDefault(x => x.Email == email);

            //        if (shareRequest != null)
            //        {
            //            var inviter = crmUser.GetByEmail(shareRequest.CreatedBy);
            //            tenantId = inviter.TenantID;
            //        }
            //    }
            //}

            emailData.Add("FirstName", firstName);
            emailData.Add("LastName", lastName);

            Email notification;
            var from = "";
            var fromName = "";

            if (tenantId != 0 && tenantId == int.Parse(ConfigurationManager.AppSettings["PrimeAppsTenantId"]))
            {
                url = !isFreeLicense ? "https://console.primeapps.io/#/auth/activation?token={0}&uid={1}&app={2}" : "https://console.primeapps.io/#/auth/activation?token={0}&uid={1}&app={2}&free=true";
                emailData.Add("Url", string.Format(url, HttpUtility.UrlEncode(token), userId, appId));
                notification = new Email(typeof(Microsoft.AspNetCore.Authentication.Resources.Email.PrimeAppsConfirm), Thread.CurrentThread.CurrentCulture.Name, emailData, appId);
                from = "notifications@primeapps.io";
                fromName = "PrimeApps";
            }
            else
            {
                emailData.Add("Url", string.Format(url, HttpUtility.UrlEncode(token), userId, appId));
                notification = new Email(typeof(Microsoft.AspNetCore.Authentication.Resources.Email.SubscriptionConfirm), Thread.CurrentThread.CurrentCulture.Name, emailData, appId);
            }

            notification.AddRecipient(email);
            notification.AddToQueue(0, 0, @from, fromName);
        }

        private async Task DeactivateUser(PlatformUser user)
        {

            user.EmailConfirmed = false;
            user.TenantId = 0;
            await _platformUserRepository.UpdateAsync(user);
        }

        private void SendPasswordReset(string token, int userId, string email, string fullName, int appId)
        {
            //create an email to send to the user for password reset link.
            string domain;
            var subdomain = "";

            if (!HttpContext.Current.Request.IsLocal)
            {
                domain = "https://{0}.ofisim.com/";
                var appDomain = "crm";

                switch (appId)
                {
                    case 2:
                        appDomain = "kobi";
                        break;
                    case 3:
                        appDomain = "asistan";
                        break;
                    case 4:
                        appDomain = "ik";
                        break;
                    case 5:
                        appDomain = "cagri";
                        break;
                    default:
                        appDomain = "crm";
                        break;
                }

                subdomain = ConfigurationManager.AppSettings.Get("TestMode") == "true" ? "test" : appDomain;
                domain = string.Format(domain, subdomain);
            }
            else
            {
                domain = "http://localhost:5554/";
            }

            var url = domain + "auth/ResetPassword?token={1}&uid={2}";

            var emailData = new Dictionary<string, string>();

            emailData.Add("PasswordResetUrl", string.Format(url, subdomain, HttpUtility.UrlEncode(token), userId));
            emailData.Add("FullName", fullName);

            var notification = new Email(typeof(Microsoft.AspNetCore.Authentication.Resources.Email.PasswordReset), Thread.CurrentThread.CurrentCulture.Name, emailData, appId);
            notification.AddRecipient(email);
            notification.AddToQueue();
        }

        #endregion
    }
}

