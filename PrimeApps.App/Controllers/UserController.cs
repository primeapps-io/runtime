using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.App.Services;
using PrimeApps.App.Storage;
using PrimeApps.Model.Common.User;
using PrimeApps.Model.Common.UserApps;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Helpers.QueryTranslation;
using PrimeApps.Model.Repositories.Interfaces;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Management.Smo;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using User = PrimeApps.Model.Entities.Tenant.TenantUser;
using Utils = PrimeApps.App.Helpers.Utils;

namespace PrimeApps.App.Controllers
{
    [Route("api/User")]
    public class UserController : ApiBaseController
    {
        private IUserRepository _userRepository;
        private ISettingRepository _settingRepository;
        private IProfileRepository _profileRepository;
        private IRoleRepository _roleRepository;
        private IRecordRepository _recordRepository;
        private IPlatformUserRepository _platformUserRepository;
        private IPlatformRepository _platformRepository;
        private IApplicationRepository _applicationRepository;
        private ITenantRepository _tenantRepository;
        private IPlatformWarehouseRepository _platformWarehouseRepository;
        private Warehouse _warehouse;
        private IConfiguration _configuration;
        private IServiceScopeFactory _serviceScopeFactory;

        public IBackgroundTaskQueue Queue { get; }

        public UserController(IApplicationRepository applicationRepository, IUserRepository userRepository, ISettingRepository settingRepository, IProfileRepository profileRepository, IRoleRepository roleRepository, IRecordRepository recordRepository, IPlatformUserRepository platformUserRepository, IPlatformRepository platformRepository, ITenantRepository tenantRepository, IPlatformWarehouseRepository platformWarehouseRepository, IBackgroundTaskQueue queue, Warehouse warehouse, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            _userRepository = userRepository;
            _settingRepository = settingRepository;
            _profileRepository = profileRepository;
            _roleRepository = roleRepository;
            _warehouse = warehouse;
            _recordRepository = recordRepository;
            _platformUserRepository = platformUserRepository;
            _platformRepository = platformRepository;
            _tenantRepository = tenantRepository;
            _platformWarehouseRepository = platformWarehouseRepository;
            _applicationRepository = applicationRepository;
            _serviceScopeFactory = serviceScopeFactory;

            Queue = queue;

            //Set warehouse database name Ofisim to integration
            //_warehouse.DatabaseName = "Ofisim";
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_userRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_settingRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_profileRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_roleRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_recordRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_platformUserRepository);
            SetCurrentUser(_platformRepository);
            SetCurrentUser(_tenantRepository);
            SetCurrentUser(_platformWarehouseRepository);
            SetCurrentUser(_applicationRepository);

            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Gets avatar from blob storage by the file id.
        /// </summary>
        /// <param name="fileName">File name of the avatar</param>
        /// <returns>Stream.</returns>
        [Route("Avatar"), HttpPost]
        public async Task<IActionResult> Avatar([FromQuery(Name = "fileName")]string fileName)
        {
            //get uploaded file from storage
            var blob = AzureStorage.GetBlob("user-images", fileName, _configuration);
            try
            {
                //if the file exists, fetchattributes method will fetch the attributes, otherwise it'll throw an exception/
                await blob.FetchAttributesAsync();

                //return await AzureStorage.DownloadToFileStreamResultAsync(file, fileName);
                Response.Headers.Add("Content-Disposition", "attachment; filename=" + fileName);// force download
                await blob.DownloadToStreamAsync(Response.Body);
                return new EmptyResult();
            }
            catch (Exception)
            {
                //on any exception do nothing, continue and return no content status code, because that file does not exist.
            }

            return NotFound();
        }

        /// <summary>
        /// Changes users personal informations and preferences.
        /// </summary>
        /// <param name="user">The user.</param>
        [Route("Edit"), HttpPost]
        public async Task<IActionResult> Edit([FromBody]UserDTO user)
        {
            //get user to start modification.
            PlatformUser userToEdit = await _platformUserRepository.GetSettings(AppUser.Id);
            User tenantUserToEdit = _userRepository.GetById(AppUser.Id);

            if (user.picture != tenantUserToEdit.Picture && user.picture != null)
            {
                //if users avatar changed check update it.
                //if (user.picture.Trim() == "")
                //{
                //if users avatar changed check update it.

                //if (userToEdit.avatar != null)
                //{
                //    //if the user had an avatar already, remove it from AzureStorage.
                //    // AzureStorage.RemoveFile("user-images", userToEdit.avatar);
                //}

                //update the new filename.
                tenantUserToEdit.Picture = user.picture;
                //}
            }

            /// update other properties                   
            userToEdit.FirstName = user.firstName;
            userToEdit.LastName = user.lastName;
            userToEdit.Setting.Phone = user.phone;
            /// update tenant database properties
            tenantUserToEdit.FirstName = user.firstName;
            tenantUserToEdit.LastName = user.lastName;
            tenantUserToEdit.FullName = user.firstName + " " + user.lastName;
            tenantUserToEdit.Phone = user.phone;

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;
            await _platformUserRepository.HardCodedUpdateUser(userToEdit);
            //update users record.
            await _userRepository.UpdateAsync(tenantUserToEdit);

            return Ok();
        }

        /// <summary>
        /// Changes culture setting for the user.
        /// </summary>
        /// <param name="culture">The culture.</param>
        [Route("ChangeCulture"), HttpPost]
        public async Task<IActionResult> ChangeCulture([FromBody]string culture)
        {
            /// if it's an unknown or an unsupported culture do nothing.
            if (!Helpers.Constants.CULTURES.Contains(culture))
            {
                return NotFound();
            }

            /// get user
            PlatformUser user = await _platformUserRepository.GetSettings(AppUser.Id);

            /// change culture and save it.
            user.Setting.Culture = culture;
            await _platformUserRepository.UpdateAsync(user);

            ///Modify the culture in current session and update it globally in all sessions of the user.
            AppUser.Culture = culture;
            return Ok();
        }

        /// <summary>
        /// Changes currency setting for the user.
        /// </summary>
        /// <param name="currency"></param>
        /// <returns></returns>
        [Route("ChangeCurrency"), HttpPost]
        public async Task<IActionResult> ChangeCurrency([FromBody]string currency)
        {
            /// if it's an unknown or an unsupported currency do nothing.
            if (!Helpers.Constants.CURRENCIES.Contains(currency))
            {
                return NotFound();
            }

            ///get user
            PlatformUser user = await _platformUserRepository.GetSettings(AppUser.Id);
            //change culture and save it.
            user.Setting.Currency = currency;
            await _platformUserRepository.UpdateAsync(user);

            ///Modify the currency in current session and update it globally in all sessions of the user.
            AppUser.Currency = currency;
            return Ok();
        }

        [Route("get_users"), HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var user = await _userRepository.GetUserInfoAsync(AppUser.Id);

                if (user != null)
                {
                    var tenantUser = _userRepository.GetSubscriber();

                    var tenant = await _tenantRepository.GetTenantInfo(AppUser.TenantId, tenantUser);

                    if (tenant != null)
                        return Ok(tenant[0].users);
                    else
                        return BadRequest();
                }

                return Unauthorized();
            }
            catch (Exception ex)
            {
                if (ex.InnerException is PostgresException)
                {
                    var innerEx = (PostgresException)ex.InnerException;

                    if (innerEx.SqlState == PostgreSqlStateCodes.DatabaseDoesNotExist)
                        return BadRequest("Account is deactivated and database is deleted.");
                }

                throw;
            }
        }

        /// <summary>
        /// Gets users account, we use this after login procedure as a first job to initialize client ui.
        /// </summary>
        /// <returns>Account.</returns>
        [Route("MyAccount"), HttpPost]
        public async Task<IActionResult> MyAccount()
        {
            var acc = new AccountInfo();
            var apps = new List<UserAppInfo>();
            var previewMode = _configuration.GetValue("AppSettings:PreviewMode", string.Empty);
            previewMode = !string.IsNullOrEmpty(previewMode) ? previewMode : "tenant";

            /*if (previewMode == "app")
                acc.user = await _userRepository.GetUserInfoAsync(1, false);
            else*/
            acc.user = await _userRepository.GetUserInfoAsync(AppUser.Id);

            if (acc.user != null)
            {
                var tenant = await _tenantRepository.GetTenantInfo(AppUser.TenantId, null);

                if (tenant == null || tenant.Count <= 0)
                    return Ok(null);

                acc.user.tenantLanguage = AppUser.TenantLanguage;
                acc.instances = tenant;
                var storageUrl = _configuration.GetValue("AppSettings:StorageUrl", string.Empty);
                if (!string.IsNullOrEmpty(storageUrl))
                {
                    acc.imageUrl = storageUrl + "/record-detail-" + tenant[0].tenantId + "/";
                }

                /*Eğer previewmode değilse lisansları setle*/
                if (String.Equals(previewMode, "tenant"))
                {
                    acc.user.userLicenseCount = tenant[0].licenses.UserLicenseCount;
                    acc.user.moduleLicenseCount = tenant[0].licenses.ModuleLicenseCount;
                    acc.user.hasAnalytics = tenant[0].licenses.AnalyticsLicenseCount > 0 ? true : false;
                }

                acc.user.tenantId = AppUser.TenantId;
                acc.user.appId = AppUser.AppId;
                acc.apps = apps;

                if (acc.user.deactivated && previewMode != "app")
                    throw new ApplicationException(HttpStatusCode.Status409Conflict.ToString());

                return Ok(acc);
            }

            return Ok(null);
        }

        [Route("ActiveDirectoryInfo"), HttpGet]
        public async Task<IActionResult> GetAdInfo()
        {
            var accountOwner = await _platformUserRepository.GetUserByAutoId(AppUser.TenantId);

            if (accountOwner == null || accountOwner.Id < 1) return Ok(false);

            var tenantId = accountOwner.Id;
            var adTenant = await _tenantRepository.GetTenantInfo(tenantId, null);

            var data = new
            {
                info = adTenant,
                email = accountOwner.Email
            };

            return Ok(data);
        }

        [Route("get_all"), HttpGet]
        public async Task<ICollection<User>> GetAll()
        {
            return await _userRepository.GetAllAsync();
        }

        [Route("add_user"), HttpPost]
        public async Task<IActionResult> AddUser([FromBody]AddUserBindingModel addUserBindingModel)
        {
            var checkEmail = await _platformUserRepository.IsEmailAvailable(addUserBindingModel.Email, AppUser.AppId);

            if (checkEmail == EmailAvailableType.NotAvailable)
                return StatusCode(HttpStatusCode.Status409Conflict);

            //Set warehouse database name
            //_warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            var appInfo = await _applicationRepository.Get(AppUser.AppId);
            addUserBindingModel.TenantId = AppUser.TenantId;
            addUserBindingModel.AppId = AppUser.AppId;

            using (var httpClient = new HttpClient())
            {
                var token = Request.Headers["Authorization"];

                var url = Request.Scheme + "://" + appInfo.Setting.AuthDomain + "/user/add_user";
                httpClient.BaseAddress = new Uri(url);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.ToString().Replace("Bearer ", ""));

                var json = JsonConvert.SerializeObject(addUserBindingModel);
                var response = await httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                    return BadRequest(response);

                using (var content = response.Content)
                {
                    var stringResult = content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrEmpty(stringResult))
                    {
                        var jsonResult = JObject.Parse(stringResult);

                        if (!string.IsNullOrEmpty(jsonResult["token"].ToString()) && !addUserBindingModel.DontSendMail)
                        {
                            var templates = await _platformRepository.GetAppTemplate(AppUser.AppId, AppTemplateType.Email, AppUser.Language, "email_confirm");

                            foreach (var template in templates)
                            {
                                if (template != null)
                                {
                                    template.Content = template.Content.Replace("{:FirstName}", addUserBindingModel.FirstName);
                                    template.Content = template.Content.Replace("{:LastName}", addUserBindingModel.LastName);
                                    template.Content = template.Content.Replace("{:Email}", addUserBindingModel.Email);
                                    template.Content = template.Content.Replace("{:Url}", Request.Scheme + "://" + appInfo.Setting.AuthDomain + "/account/confirmemail?email=" + addUserBindingModel.Email + "&code=" + WebUtility.UrlDecode(jsonResult["token"].ToString()));

                                    Email notification = new Email(template.Subject, template.Content, _configuration, _serviceScopeFactory);
                                    var req = JsonConvert.DeserializeObject<JObject>(template.Settings);
                                    if (req != null)
                                    {
                                        notification.AddRecipient((string)req["MailSenderEmail"]);
                                        notification.AddToQueue((string)req["MailSenderEmail"], (string)req["MailSenderName"]);
                                    }
                                }
                            }
                        }


                        return Ok(new JObject { ["password"] = jsonResult["password"].ToString() });
                    }

                    if (response.IsSuccessStatusCode)
                        return Ok();

                    return BadRequest();
                }
            }
        }


        [Route("update_user_currency_culture"), HttpPost]
        public async Task<IActionResult> UpdateUserCurrencyCulture([FromBody]JObject User)
        {
            if (User["Id"].IsNullOrEmpty() || User["Culture"].IsNullOrEmpty() || User["Currency"].IsNullOrEmpty())
                return BadRequest();

            var userId = (int)User["Id"];
            PlatformUser userToEdit = await _platformUserRepository.GetSettings(userId);
            User tenantUserToEdit = _userRepository.GetById(userId);

            userToEdit.Setting.Culture = (string)User["Culture"];
            userToEdit.Setting.Currency = (string)User["Currency"];

            tenantUserToEdit.Culture = (string)User["Culture"];
            tenantUserToEdit.Currency = (string)User["Currency"];

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;
            await _platformUserRepository.HardCodedUpdateUser(userToEdit);
            //update users record.
            await _userRepository.UpdateAsync(tenantUserToEdit);

            return Ok(userId);

        }

        [Route("send_user_password"), HttpPost]
        public async Task<IActionResult> UserSendPassword([FromBody]JObject requestMail)
        {
            if (requestMail.IsNullOrEmpty())
                return BadRequest();

            var templates = await _platformRepository.GetAppTemplate(AppUser.AppId, AppTemplateType.Email, AppUser.Language, "send_password");

            foreach (var template in templates)
            {
                if (template != null)
                {
                    template.Content = template.Content.Replace("{:FullName}", requestMail["full_name"].ToString());
                    template.Content = template.Content.Replace("{:Email}", requestMail["email"].ToString());
                    template.Content = template.Content.Replace("{:Password}", requestMail["password"].ToString());

                    Email notification = new Email(template.Subject, template.Content, _configuration, _serviceScopeFactory);
                    var req = JsonConvert.DeserializeObject<JObject>(template.Settings);
                    if (req != null)
                    {
                        notification.AddRecipient(requestMail["email"].ToString());
                        notification.AddToQueue((string)req["MailSenderEmail"], (string)req["MailSenderName"]);
                    }
                }
            }

            return Ok();
        }

        //TODO TenantId
        [Route("get_user"), HttpGet]
        public async Task<IActionResult> GetUser([FromQuery(Name = "email")]string email, [FromQuery(Name = "tenantId")]int tenantId)
        {
            if (!AppUser.Email.EndsWith("@ofisim.com"))
                return StatusCode(HttpStatusCode.Status403Forbidden);

            var userEntity = await _platformUserRepository.Get(email);

            if (userEntity == null)
                return NotFound();

            var userTenant = userEntity.TenantsAsUser.Where(x => x.TenantId == tenantId);

            if (userTenant == null)
                return NotFound();

            _userRepository.TenantId = tenantId;
            var user = _userRepository.GetById(userEntity.Id);

            var userModel = new
            {
                Id = user.Id,
                TenantId = tenantId,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Phone = user.Phone,
                IsSubscriber = user.IsSubscriber,
                IsActive = user.IsActive,
                Deleted = user.Deleted,
                ProfileId = user.ProfileId,
                RoleId = user.RoleId
            };

            return Ok(userModel);
        }

        [Route("get_user_email_control"), HttpGet]
        public async Task<EmailAvailableType> GetUserEmailControl(string email)
        {
            return await _platformUserRepository.IsEmailAvailable(email, AppUser.AppId);
        }

        [Route("get_users_by_profile_ids"), HttpPost]
        public async Task<IActionResult> GetUserByProfileIds([FromBody]List<int> ids)
        {
            var users = await _userRepository.GetByProfileIds(ids);

            return Ok(users);
        }

        //TODO Removed
        [Route("get_office_users"), HttpGet]
        public IActionResult GetOfficeUsers()
        {
            /*var clientId = "7697cae4-0291-4449-8046-7b1cae642982";
            var appKey = "J2YHu8tqkM8YJh8zgSj8XP0eJpZlFKgshTehIe5ITvU=";
            var graphResourceID = "https://graph.windows.net";
            var graphSettings = new GraphSettings
            {
                ApiVersion = "2013-11-08",
                GraphDomainName = "graph.windows.net"
            };



            var signedInUserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var tenantID = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/tenantId");
            var userObjectID = User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");

            const String appClientID = "7697cae4-0291-4449-8046-7b1cae642982";
            //const String tenant = tenantID.ToString();
            const String authString = "https://login.microsoftonline.com/dd7a82b0-2195-409f-b09f-e0e8226753ad/";
            const String authClientSecret = "J2YHu8tqkM8YJh8zgSj8XP0eJpZlFKgshTehIe5ITvU=";

            try
            {
                // The AuthenticationContext is ADAL's primary class, in which you indicate the tenant to use.
                var authContext = new AuthenticationContext("https://login.microsoftonline.com/" + tenantID + "/");

                // The ClientCredential is where you pass in your client_id and client_secret, which are
                // provided to Azure AD in order to receive an access_token by using the app's identity.
                var credential = new ClientCredential(clientId, appKey);

                AuthenticationResult result = await authContext.AcquireTokenAsync("https://graph.windows.net", credential);

                HttpClient http = new HttpClient();
                string url = "https://graph.windows.net/" + tenantID + "/users" + "?" + "api-version=1.6";

                // Append the access token for the Graph API to the Authorization header of the request by using the Bearer scheme.
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", User.FindFirstValue("validated_code"));
                HttpResponseMessage response = await http.SendAsync(request);



                // use the token for querying the graph
                //var token = await HttpContext.GetTokenAsync("access_token");
                var graphClient = new ActiveDirectoryClient(new Uri(graphResourceID + '/' + tenantID), () => GetTokenForGraph(tenantID, signedInUserID, userObjectID, clientId, appKey, graphResourceID));

                //var users = await graphClient.Users.Where(x => x.ObjectId.Equals(userObjectID)).ExecuteAsync();
                var userResponse = await graphClient.Users.ExecuteAsync();
                var users = userResponse.CurrentPage.ToList();

                while (userResponse.MorePagesAvailable)
                {
                    userResponse = await userResponse.GetNextPageAsync();

                    var newUsers = userResponse.CurrentPage.ToList();
                    users.AddRange(newUsers);
                }

                users = users.OrderBy(x => x.Mail).ToList();
                var systemUsers = await _platformUserRepository.GetAllByTenant(AppUser.TenantId);
                var availableUsers = new JArray();

                for (var i = users.Count - 1; i >= 0; i--)
                {
                    var officeUser = users[i];
                    var user = systemUsers.FirstOrDefault(x => x.Email == officeUser.Mail);
                    if (user != null) continue;
                    var data = new JObject
                    {
                        {"id", officeUser.ObjectId},
                        {"email", officeUser.Mail},
                        {"name", officeUser.GivenName},
                        {"surname", officeUser.Surname},
                        {"fullName", officeUser.DisplayName},
                        {"phone", officeUser.TelephoneNumber}
                    };
                    availableUsers.Add(data);
                }
                return Ok(availableUsers);
            }
            catch (Exception e)
            {
                return Ok(false);
            }*/
            return Ok();
        }

        /*
        [Route("UpdateActiveDirectoryEmail"), HttpGet]
        public async Task<IActionResult> UpdateActiveDirectoryEmail(int userId, string email)
        {
            var resultControl = await _platformUserRepository.IsActiveDirectoryEmailAvailable(email);

            if (resultControl == false)
                return StatusCode(HttpStatusCode.Status409Conflict);

            var userEmailCheck = await _platformUserRepository.Get(email);

            if (userEmailCheck != null && userEmailCheck.Id != userId)
            {
                return StatusCode(HttpStatusCode.Status409Conflict);
            }

            var user = await _platformUserRepository.Get(userId);
            var adTenant = await _platformUserRepository.GetConfirmedActiveDirectoryTenant(user.TenantId.Value);

            user.ActiveDirectoryTenantId = adTenant.Id;
            user.ActiveDirectoryEmail = email;

            await _platformUserRepository.UpdateAsync(user);
            return Ok();
        }

        */
        /*private async Task<string> GetTokenForGraph(string tenantID, string signedInUserID, string userObjectID, string clientId, string appKey, string graphResourceID)
        {
             // get a token for the Graph without triggering any user interaction (from the cache, via multi-resource refresh token, etc)
            ClientCredential clientcred = new ClientCredential(clientId, appKey);
            // initialize AuthenticationContext with the token cache of the currently signed in user, as kept in the app's EF DB
            AuthenticationContext authContext = new AuthenticationContext(string.Format("https://login.microsoftonline.com/{0}", tenantID), new AdTokenCache(signedInUserID));
            AuthenticationResult result = await authContext.AcquireTokenSilentAsync(graphResourceID, clientcred, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));
            return result.AccessToken;
        }*/
    }
}