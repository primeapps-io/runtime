using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.App.Services;
using PrimeApps.App.Storage;
using PrimeApps.Model.Common.User;
using PrimeApps.Model.Common.UserApps;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using User = PrimeApps.Model.Entities.Application.TenantUser;
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
            SetCurrentUser(_userRepository);
            SetCurrentUser(_settingRepository);
            SetCurrentUser(_profileRepository);
            SetCurrentUser(_roleRepository);
            SetCurrentUser(_recordRepository);
            SetCurrentUser(_platformUserRepository);
            SetCurrentUser(_platformRepository);
            SetCurrentUser(_tenantRepository);
            SetCurrentUser(_platformWarehouseRepository);

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
                Response.Headers.Add("Content-Disposition", "attachment; filename=" + fileName); // force download
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
            User tenantUserToEdit = await _userRepository.GetById(AppUser.Id);

            if (user.picture != tenantUserToEdit.Picture && user.picture != null)
            {
                //if users avatar changed check update it.
                if (user.picture.Trim() != string.Empty)
                {
                    //if users avatar changed check update it.

                    //if (userToEdit.avatar != null)
                    //{
                    //    //if the user had an avatar already, remove it from AzureStorage.
                    //    // AzureStorage.RemoveFile("user-images", userToEdit.avatar);
                    //}

                    //update the new filename.
                    tenantUserToEdit.Picture = user.picture;
                }
            }

            /// update other properties
            userToEdit.FirstName = user.firstName;
            userToEdit.LastName = user.lastName;
            userToEdit.Setting.Phone = user.phone;
            /// update tenant database properties
            tenantUserToEdit.FirstName = user.firstName;
            tenantUserToEdit.LastName = user.lastName;
            tenantUserToEdit.FullName = user.firstName + " " + user.lastName;

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;
            await _platformUserRepository.UpdateAsync(userToEdit);
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
            if (!Helpers.Constants.CULTURES.Contains(culture)) { return NotFound(); }

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
            if (!Helpers.Constants.CURRENCIES.Contains(currency)) { return NotFound(); }

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
            var user = await _userRepository.GetUserInfoAsync(AppUser.Id);

            if (user != null)
            {
                var tenant = await _tenantRepository.GetTenantInfo(AppUser.TenantId);

                if (tenant != null)
                    return Ok(tenant[0].users);
                else
                    return BadRequest();
            }
            return Unauthorized();
        }

        /// <summary>
        /// Gets users account, we use this after login procedure as a first job to initialize client ui.
        /// </summary>
        /// <returns>Account.</returns>
        [Route("MyAccount"), HttpPost]
        public async Task<IActionResult> MyAccount()
        {
            AccountInfo acc = new AccountInfo();
            List<UserAppInfo> apps = null;
            acc.user = await _userRepository.GetUserInfoAsync(AppUser.Id);


            if (acc.user != null)
            {

                var tenant = await _tenantRepository.GetTenantInfo(AppUser.TenantId);

                //TODO Removed
                /*using (var dbContext = new PlatformDBContext())
                {
                    apps = dbContext.UserApps.Where(x => x.UserId == AppUser.Id)
                        .Select(i => new UserAppInfo()
                        {
                            Id = i.Id,
                            Email = i.Email,
                            TenantId = i.TenantId,
                            UserId = i.UserId,
                            Active = i.Active,
                            AppId = i.AppId,
                            MainTenantId = i.MainTenantId
                        }).ToList();
                }
				
                if (AppUser.AppId == 1 || AppUser.AppId == 4)
                {
                    var activeApp = apps.SingleOrDefault(x => x.Active);
                    var mainTenant = await _platformUserRepository.Get(acc.user.ID);
                    var mainApp = new UserAppInfo();
                    mainApp.MainTenantId = mainTenant.Id;
                    mainApp.Email = mainTenant.Email;
                    mainApp.AppId = mainTenant.TenantsAsUser.Where(x => x.Id == AppUser.TenantId).FirstOrDefault().AppId;
                    mainApp.TenantId = mainTenant.Id;
                    mainApp.UserId = mainTenant.Id;
                    mainApp.Active = activeApp == null;
                    apps.Add(mainApp);
                }*/

                acc.user.tenantLanguage = AppUser.TenantLanguage;
                acc.instances = tenant;
                acc.user.picture = AzureStorage.GetAvatarUrl(acc.user.picture, _configuration);
                //acc.user.hasAnalytics = AppUser.HasAnalyticsLicense;
                acc.imageUrl = _configuration.GetSection("AppSettings")["BlobUrl"] + "/record-detail-" + tenant[0].tenantId + "/";
                acc.user.userLicenseCount = tenant[0].licenses.UserLicenseCount;
                acc.user.moduleLicenseCount = tenant[0].licenses.ModuleLicenseCount;
                //acc.user.isPaidCustomer = AppUser.IsPaidCustomer;
                //acc.user.deactivated = AppUser.IsDeactivated;
                acc.user.tenantId = AppUser.TenantId;
                acc.user.appId = AppUser.AppId;
                acc.apps = apps;
                if (tenant[0].licenses.AnalyticsLicenseCount > 0)
                    acc.instances[0].hasAnalytics = true;

                foreach (var inst in acc.instances)
                {
                    inst.logoUrl = AzureStorage.GetLogoUrl(inst.logoUrl, _configuration);
                }

                if (acc.user.deactivated)
                    throw new ApplicationException(HttpStatusCode.Status409Conflict.ToString());
                //throw new HttpResponseException(HttpStatusCode.Status409Conflict);

                return Ok(acc);
            }

            acc = null;
            return Ok(acc); //Success service request - but no account data - disabled user(inactive)
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

            var tenantId = AppUser.TenantId;
            var adminUserLocalId = AppUser.Id;
            var adminUserGlobalId = AppUser.Id;
            var adminUserEmail = AppUser.Email;
            var culture = AppUser.Culture;
            var currency = AppUser.Currency;
            var picklistLanguage = AppUser.TenantLanguage;
            var appId = AppUser.AppId;
            var createdBy = AppUser.Email;

            if (addUserBindingModel.TenantId.HasValue)
            {
                if (!AppUser.Email.EndsWith("@ofisim.com"))
                    return StatusCode(HttpStatusCode.Status403Forbidden);

                var tenantWithOwner = await _platformUserRepository.GetTenantWithOwner(addUserBindingModel.TenantId.Value);

                tenantId = addUserBindingModel.TenantId.Value;
                adminUserLocalId = tenantWithOwner.OwnerId;
                adminUserGlobalId = tenantWithOwner.OwnerId;
                adminUserEmail = tenantWithOwner.Owner.Email;
                culture = tenantWithOwner.Owner.Setting.Culture;
                currency = tenantWithOwner.Owner.Setting.Currency;
                appId = tenantWithOwner.AppId;
                createdBy = tenantWithOwner.Owner.Email;

                _userRepository.TenantId = tenantId;
                _profileRepository.TenantId = tenantId;
                _roleRepository.TenantId = tenantId;
                _recordRepository.TenantId = tenantId;
            }


            //Register
            var tenant = _platformRepository.GetTenant(tenantId);

            if (tenant.TenantUsers.Count >= tenant.License.UserLicenseCount)
                return StatusCode(HttpStatusCode.Status402PaymentRequired);

            var randomPassword = Utils.GenerateRandomUnique(8);

            PlatformUser applicationUser = null;
            if (checkEmail != EmailAvailableType.AvailableForApp)
            {
                applicationUser = new PlatformUser
                {
                    Email = addUserBindingModel.Email,
                    FirstName = addUserBindingModel.FirstName,
                    LastName = addUserBindingModel.LastName,
                    Setting = new PlatformUserSetting()
                };

                applicationUser.Setting.Culture = tenant.Setting.Culture;
                applicationUser.Setting.Language = tenant.Setting.Language;
                //tenant.Setting.TimeZone = 
                applicationUser.Setting.Currency = tenant.Setting.Currency;


                var result = _platformUserRepository.CreateUser(applicationUser).Result;
                if (result == 0)
                {
                    ModelState.AddModelError("", "user not created");
                    return BadRequest(ModelState);
                }
            }
            else
            {
                applicationUser = await _platformUserRepository.Get(addUserBindingModel.Email);
            }

			var appInfo = await _applicationRepository.Get(appId);

            using (var httpClient = new HttpClient())
            {

                var url = Request.Scheme + "://" + appInfo.Setting.AuthDomain + "/user/register";
                httpClient.BaseAddress = new Uri(url);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var json = JsonConvert.SerializeObject(new RegisterBindingModel { Email = addUserBindingModel.Email, FirstName = addUserBindingModel.FirstName, LastName = addUserBindingModel.LastName, Password = randomPassword });
                var response = await httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

                /*if (!response.IsSuccessStatusCode)
					return BadRequest(response);*/

                using (var content = response.Content)
                {
                    var stringResult = content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrEmpty(stringResult))
                    {
                        var jsonResult = JObject.Parse(stringResult);
                        if (!string.IsNullOrEmpty(jsonResult["token"].ToString()))
                        {
                            var template = _platformRepository.GetAppTemplate(appId, AppTemplateType.Email, "email_confirm", culture.Substring(0, 2));
                            if (template != null)
                            {
                                template.Content.Replace("{{:FIRSTNAME}}", addUserBindingModel.FirstName);
                                template.Content.Replace("{{:LASTNAME}}", addUserBindingModel.LastName);
                                template.Content.Replace("{{:EMAIL}}", addUserBindingModel.Email);
                                template.Content.Replace("{:Url}", Request.Scheme + "://" + appInfo.Setting.AuthDomain + "/user/activate?token=" + jsonResult["token"]);

                                Email notification = new Email(template.Subject, template.Content, _configuration);

                                notification.AddRecipient(template.MailSenderEmail);
                                notification.AddToQueue(template.MailSenderEmail, template.MailSenderName);
                            }
                        }
                    }
                }
            }

            var registerModel = new RegisterBindingModel();
            registerModel.Email = addUserBindingModel.Email;
            registerModel.FirstName = addUserBindingModel.FirstName;
            registerModel.LastName = addUserBindingModel.LastName;
            registerModel.Password = randomPassword;
            registerModel.License = "F89E4FBF-A50F-40BA-BBEC-FE027F3F1524";//Free license

            //TODO Integration
            //Queue.QueueBackgroundWorkItem(async token => _integration.InsertUser(registerModel, _warehouse));

            var platformUser = await _platformUserRepository.Get(applicationUser.Id);
            var adminUser = await _platformUserRepository.GetTenantWithOwner(tenantId);


            //Set warehouse database name
            var warehouseInfo = await _platformWarehouseRepository.GetByTenantId(tenantId);

            _warehouse.DatabaseName = warehouseInfo != null ? warehouseInfo.DatabaseName : "0";

            var tenantUser = await _userRepository.GetById(platformUser.Id);

            if (tenantUser == null)
            {
                tenantUser = new User
                {
                    Id = platformUser.Id,
                    Email = addUserBindingModel.Email,
                    FirstName = addUserBindingModel.FirstName,
                    LastName = addUserBindingModel.LastName,
                    FullName = $"{addUserBindingModel.FirstName} {addUserBindingModel.LastName}",
                    Picture = "",
                    IsActive = true,
                    IsSubscriber = false,
                    Culture = culture,
                    Currency = currency,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByEmail = adminUserEmail
                };

                await _userRepository.CreateAsync(tenantUser);
            }
            else
            {
                randomPassword = "*******";
                tenantUser.IsActive = true;
                await _userRepository.UpdateAsync(tenantUser);
            }

            await _profileRepository.AddUserAsync(platformUser.Id, addUserBindingModel.ProfileId);
            await _roleRepository.AddUserAsync(platformUser.Id, addUserBindingModel.RoleId);

            var currentTenant = _platformRepository.GetTenant(tenantId);

            platformUser.TenantsAsUser.Add(new UserTenant { Tenant = currentTenant, PlatformUser = platformUser });

            await _platformUserRepository.UpdateAsync(platformUser);

            await _platformUserRepository.UpdateAsync(platformUser);

            return Ok(randomPassword);
        }

        //TODO TenantId
        [Route("get_user"), HttpGet]
        public async Task<IActionResult> GetUser([FromQuery(Name = "email")]string email, [FromQuery(Name = "tenantId")] int tenantId)
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
            var user = await _userRepository.GetById(userEntity.Id);

            var userModel = new
            {
                Id = user.Id,
                TenantId = tenantId,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                IsSubscriber = user.IsSubscriber,
                IsActive = user.IsActive,
                Deleted = user.Deleted,
                ProfileId = user.ProfileId,
                RoleId = user.RoleId
            };

            return Ok(userModel);
        }

        [Route("get_users_by_profile_ids"), HttpPost]
        public async Task<IActionResult> GetUserByProfileIds([FromBody]List<int> ids)
        {
            var users = await _userRepository.GetByProfileIds(ids);

            return Ok(users);
        }

		//TODO Removed
		[Route("get_office_users"), HttpGet]
		public async Task<IActionResult> GetOfficeUsers()
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
