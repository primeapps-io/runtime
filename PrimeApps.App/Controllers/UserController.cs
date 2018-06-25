using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Extensions;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Common.User;
using PrimeApps.Model.Common.UserApps;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Helpers;
using User = PrimeApps.Model.Entities.Application.TenantUser;
using Utils = PrimeApps.App.Helpers.Utils;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using Hangfire;
using PrimeApps.Model.Entities.Platform;
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeApps.Model.Common.Resources;
using PrimeApps.App.Storage;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Enums;
using Newtonsoft.Json;
using System.Text;

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
        private ITenantRepository _tenantRepository;
        private IPlatformWarehouseRepository _platformWarehouseRepository;
        private Warehouse _warehouse;
        private IConfiguration _configuration;

	    private IIntegration _integration;
	    public IBackgroundTaskQueue Queue { get; }
		public UserController(IUserRepository userRepository, ISettingRepository settingRepository, IProfileRepository profileRepository, IRoleRepository roleRepository, IRecordRepository recordRepository, IPlatformUserRepository platformUserRepository, IPlatformRepository platformRepository, ITenantRepository tenantRepository, IPlatformWarehouseRepository platformWarehouseRepository, IIntegration integration, IBackgroundTaskQueue queue, Warehouse warehouse, IConfiguration configuration)
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

	        _integration = integration;
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
            var file = AzureStorage.GetBlob("user-images", fileName, _configuration);
            try
            {
                //if the file exists, fetchattributes method will fetch the attributes, otherwise it'll throw an exception/
                await file.FetchAttributesAsync();

                return await AzureStorage.DownloadToFileStreamResultAsync(file, fileName);

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

            if (user.email != userToEdit.Email && user.email != "")
            {
                //if email has changed, we need a special procedure here.
                //we won't apply the changes on the email address of user,
                //before they confirm it from the email address which they have changed.

                //generate a code to apply changes on users table.
                string passcode = Crypto.GenerateRandomCode(16);

                //TODO: Pending User Edit
                ////if this user had any pending user edit requests before, clean time.
                //crmPendingUserEdits.ClearPendingRequests(userToEdit.ID, user.defaultInstanceID, session);

                ////create database entry
                //crmPendingUserEdits req = new crmPendingUserEdits()
                //{
                //    email = user.email,
                //    passcode = passcode,
                //    isApproved = true,
                //    isUsed = false,
                //    instanceId = user.defaultInstanceID,
                //    userID = AppUser.GlobalId,
                //    requestTime = DateTime.UtcNow
                //};
                //session.Save(req);

                var subdomain = _configuration.GetSection("AppSettings")["TestMode"] == "true" ? "api-test" : "api";

                //compose a new email to the new email address of the user.
                Dictionary<string, string> emailData = new Dictionary<string, string>();
                emailData.Add("EmailResetUrl", string.Format("https://{0}.ofisim.com/REST/Public/ConfirmEmail/{1}", subdomain, passcode));

                Email removalNotification = new Email(EmailResource.EmailReset, Thread.CurrentThread.CurrentCulture.Name, emailData, _configuration, AppUser.AppId);
                //removalNotification.AddRecipient(req.email);
                removalNotification.AddToQueue();
            }

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
                //acc.user.userLicenseCount = AppUser.UserLicenseCount;
                //acc.user.moduleLicenseCount = AppUser.ModuleLicenseCount;
                //acc.user.isPaidCustomer = AppUser.IsPaidCustomer;
                //acc.user.deactivated = AppUser.IsDeactivated;
                acc.user.tenantId = AppUser.TenantId;
                acc.user.appId = AppUser.AppId;
                acc.apps = apps;

                if (acc.user.deactivated)
                    throw new ApplicationException(HttpStatusCode.Status409Conflict.ToString());
                //throw new HttpResponseException(HttpStatusCode.Status409Conflict);

                return Ok(acc);
            }

            acc = null;
            return Ok(acc); //Success service request - but no account data - disabled user(inactive)
        }

        //TODO Removed
        /*[Route("ActiveDirectoryInfo"), HttpGet]
        public async Task<IActionResult> GetAdInfo()
        {
            PlatformUser accountOwner = await _platformUserRepository.GetUserByAutoId(AppUser.TenantId);

            if (accountOwner == null || accountOwner.ActiveDirectoryTenantId < 1) return Ok(false);
            using (var dbContext = new PlatformDBContext())
            {
                var tenantId = accountOwner.ActiveDirectoryTenantId;
                var adTenant = dbContext.ActiveDirectoryTenants.FirstOrDefault(a => a.Id == tenantId);

                var data = new
                {
                    info = adTenant,
                    email = accountOwner.ActiveDirectoryEmail

                };

                return Ok(data);
            }
        }*/


        /// <summary>
        /// Uploads a new avatar for the user.
        /// </summary>
        /// <param name="fileContents">The file contents.</param>
        /// <returns>System.String.</returns>
        [Route("UploadAvatar"), HttpPost]
        public async Task<IActionResult> UploadAvatar()
        {
            // try to parse stream.
            Stream requestStream = await Request.ReadAsStreamAsync();

            HttpMultipartParser parser = new HttpMultipartParser(requestStream, "file");

            if (parser.Success)
            {
                //if succesfully parsed, then continue to thread.
                if (parser.FileContents.Length <= 0)
                {
                    //if file is invalid, then stop thread and return bad request status code.
                    return BadRequest();
                }

                //initialize chunk parameters for the upload.
                int chunk = 0;
                int chunks = 1;

                var uniqueName = string.Empty;

                if (parser.Parameters.Count > 1)
                {
                    //this is a chunked upload process, calculate how many chunks we have.
                    chunk = int.Parse(parser.Parameters["chunk"]);
                    chunks = int.Parse(parser.Parameters["chunks"]);

                    //get the file name from parser
                    if (parser.Parameters.ContainsKey("name"))
                        uniqueName = parser.Parameters["name"];
                }

                if (string.IsNullOrEmpty(uniqueName))
                {
                    var ext = Path.GetExtension(parser.Filename);
                    uniqueName = Guid.NewGuid() + ext;
                }

                //upload file to the temporary AzureStorage.
                AzureStorage.UploadFile(chunk, new MemoryStream(parser.FileContents), "temp", uniqueName, parser.ContentType, _configuration);

                if (chunk == chunks - 1)
                {
                    //if this is last chunk, then move the file to the permanent storage by commiting it.
                    //as a standart all avatar files renamed to UserID_UniqueFileName format.
                    var user_image = string.Format("{0}_{1}", AppUser.Id, uniqueName);
                    AzureStorage.CommitFile(uniqueName, user_image, parser.ContentType, "user-images", chunks, _configuration);
                    return Ok(user_image);
                }

                //return content type.
                return Ok(parser.ContentType);
            }
            //this is not a valid request so return fail.
            return Ok("Fail");
        }

        [Route("get_all"), HttpGet]
        public async Task<ICollection<User>> GetAll()
        {
            return await _userRepository.GetAllAsync();
        }

        [Route("add_user"), HttpPost]
        public async Task<IActionResult> AddUser([FromBody]AddUserBindingModel addUserBindingModel)
        {
			var checkEmail = await _platformUserRepository.IsEmailAvailable(addUserBindingModel.Email, addUserBindingModel.AppId);

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

            //TODO Removed
            /*if (!request.notCheckIsAdmin)
			{
				//get the instance that invitation request will be created on.
				var isOperationAllowed = await Cache.Tenant.CheckProfilesAdministrativeRights(tenantId, adminUserLocalId);

				if (!isOperationAllowed)
				{
					//if current user is not the admin of the instance then reject that request and send a forbidden http request.
					return StatusCode(HttpStatusCode.Forbidden);
				}
				
			}*/

            //if (!crmLicenseUsage.HasUserCapacity(tenantId))
            //         {
            //             //if capacity is exceeded, return payment required status code and cancel process.
            //             return StatusCode(HttpStatusCode.PaymentRequired);
            //         }

            //var hasAccount = crmPendingShareRequests.Invite(request.Email, instanceId, adminUserGlobalId, request.ProfileId, request.RoleId, createdBy);

            //if (hasAccount)
            //    return StatusCode(HttpStatusCode.Conflict);

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

            var appInfo = _platformRepository.GetAppInfo(appId);

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

								Email notification = new Email(template.Subject, template.Content);

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

	        Queue.QueueBackgroundWorkItem(async token => _integration.InsertUser(registerModel, _warehouse));

            var user = await _platformUserRepository.Get(applicationUser.Id);
            var adminUser = await _platformUserRepository.GetTenantWithOwner(tenantId);


			var tenantUser = new User
			{
				Id = user.Id,
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

            var currentTenant = _platformRepository.GetTenant(tenantId);

            user.TenantsAsUser.Add(new UserTenant { Tenant = currentTenant, PlatformUser = user });

            await _platformUserRepository.UpdateAsync(user);

            //Set warehouse database name
            var warehouseInfo = await _platformWarehouseRepository.GetByTenantId(tenantId);

            _warehouse.DatabaseName = warehouseInfo != null ? warehouseInfo.DatabaseName : "0";

			await _userRepository.CreateAsync(tenantUser);
			await _profileRepository.AddUserAsync(user.Id, addUserBindingModel.ProfileId);
			await _roleRepository.AddUserAsync(user.Id, addUserBindingModel.RoleId);

            await _platformUserRepository.UpdateAsync(user);

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
        /*
        public async Task<IActionResult> GetOfficeUsers()
        {
            var clientId = ConfigurationManager.AppSettings["ida:ClientID"];
            var appKey = ConfigurationManager.AppSettings["ida:Password"];
            var graphResourceID = "https://graph.windows.net";
            var graphSettings = new GraphSettings
            {
                ApiVersion = "2013-11-08",
                GraphDomainName = "graph.windows.net"
            };
            try
            {
                var signedInUserID = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
                var tenantID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
                var userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

                // use the token for querying the graph
                var graphClient = new ActiveDirectoryClient(new Uri(graphResourceID + '/' + tenantID), () => GetTokenForGraph(tenantID, signedInUserID, userObjectID, clientId, appKey, graphResourceID));

                //var users = await graphClient.Users.Where(x => x.ObjectId.Equals(userObjectID)).ExecuteAsync();
                var users = graphClient.Users.ExecuteAsync().Result.CurrentPage.ToList();

                //Core merge
                var userResponse = await graphClient.Users.ExecuteAsync();
                var users = userResponse.CurrentPage.ToList();

                while (userResponse.MorePagesAvailable)
                {
                    userResponse = await userResponse.GetNextPageAsync();

                    var newUsers = userResponse.CurrentPage.ToList();
                    users.AddRange(newUsers);
                }

                users = users.OrderBy(x => x.Mail).ToList();

                List<PlatformUser> systemUsers = await _platformUserRepository.GetAllByTenant(AppUser.TenantId);
                var availableUsers = new JArray();
				
                for (var i = users.Length - 1; i >= 0; i--)
                {
                    var officeUser = users[i];
                    var user = systemUsers.FirstOrDefault(x => x.ActiveDirectoryEmail == officeUser.Mail);
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
            }

        }

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

        private async Task<string> GetTokenForGraph(string tenantID, string signedInUserID, string userObjectID, string clientId, string appKey, string graphResourceID)
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
