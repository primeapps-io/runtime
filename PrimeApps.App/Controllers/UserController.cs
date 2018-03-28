using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.App.Providers;
using PrimeApps.App.Results;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.User;
using PrimeApps.Model.Common.UserApps;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories.Interfaces;
 
using PrimeApps.Model.Helpers;
using User = PrimeApps.Model.Entities.Application.TenantUser;
using Utils = PrimeApps.App.Helpers.Utils;
using PrimeApps.Model.Entities.Platform.Identity;
using HttpStatusCode =Microsoft.AspNetCore.Http.StatusCodes;
namespace PrimeApps.App.Controllers
{
    [Route("api/User"), Authorize]
    public class UserController : BaseController
    {
        private IUserRepository _userRepository;
        private ISettingRepository _settingRepository;
        private IProfileRepository _profileRepository;
        private IRoleRepository _roleRepository;
        private IRecordRepository _recordRepository;
        private IPlatformUserRepository _platformUserRepository;
        private ITenantRepository _tenantRepository;
        private IPlatformWarehouseRepository _platformWarehouseRepository;
        private Warehouse _warehouse;

        public UserController(IUserRepository userRepository, ISettingRepository settingRepository, IProfileRepository profileRepository, IRoleRepository roleRepository, IRecordRepository recordRepository, IPlatformUserRepository platformUserRepository, ITenantRepository tenantRepository, IPlatformWarehouseRepository platformWarehouseRepository, Warehouse warehouse)
        {
            _userRepository = userRepository;
            _settingRepository = settingRepository;
            _profileRepository = profileRepository;
            _roleRepository = roleRepository;
            _warehouse = warehouse;
            _recordRepository = recordRepository;
            _platformUserRepository = platformUserRepository;
            _tenantRepository = tenantRepository;
            _platformWarehouseRepository = platformWarehouseRepository;
            //Set warehouse database name Ofisim to integration
            //_warehouse.DatabaseName = "Ofisim";
        }

        /// <summary>
        /// Gets avatar from blob storage by the file id.
        /// </summary>
        /// <param name="fileName">File name of the avatar</param>
        /// <returns>Stream.</returns>
        [Route("Avatar"), HttpPost]
        public IActionResult Avatar(string fileName)
        {
            //get uploaded file from storage
            var file = Storage.GetBlob("user-images", fileName);
            try
            {
                //if the file exists, fetchattributes method will fetch the attributes, otherwise it'll throw an exception/
                file.FetchAttributes();

                return new FileDownloadResult()
                {
                    Blob = file,
                    PublicName = fileName
                };
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
        public async Task<IActionResult> Edit(UserDTO user)
        {
            //get user to start modification.
            PlatformUser userToEdit = await _platformUserRepository.Get(AppUser.Id);
            User tenantUserToEdit = await _userRepository.GetById(AppUser.Id);

            if (user.picture != tenantUserToEdit.Picture && user.picture != null)
            {
                //if users avatar changed check update it.
                if (user.picture.Trim() != string.Empty)
                {
                    if (tenantUserToEdit.Picture != null)
                    {
                        //if the user had an avatar already, remove it from storage.
                        Storage.RemoveFile("user-images", tenantUserToEdit.Picture);
                    }

                    //update the new filename.
                    tenantUserToEdit.Picture = user.picture;
                }
            }



            if (user.firstName != userToEdit.FirstName || userToEdit.LastName != user.lastName)
            {
                /// user name has changed, update all session entries for user.
                UserItem se = await Cache.User.Get(userToEdit.Id);
                string newUserName = $"{user.firstName} {user.lastName}";

                se.UserName = newUserName;
                await Cache.User.Update(userToEdit.Id, se);
            }

            /// update other properties
            userToEdit.FirstName = user.firstName;
            userToEdit.LastName = user.lastName;
            userToEdit.PhoneNumber = user.phone;
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

                var subdomain = ConfigurationManager<>.AppSettings.Get("TestMode") == "true" ? "api-test" : "api";

                //compose a new email to the new email address of the user.
                Dictionary<string, string> emailData = new Dictionary<string, string>();
                emailData.Add("EmailResetUrl", string.Format("https://{0}.ofisim.com/REST/Public/ConfirmEmail/{1}", subdomain, passcode));

                Email removalNotification = new Email(typeof(Resources.Email.EmailReset), Thread.CurrentThread.CurrentCulture.Name, emailData, AppUser.AppId);
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
            PlatformUser user = await _platformUserRepository.Get(AppUser.Id);

            /// change culture and save it.
            user.Culture = culture;
            await _platformUserRepository.UpdateAsync(user);

            ///Modify the culture in current session and update it globally in all sessions of the user.
            AppUser.Culture = culture;
            await Cache.User.Update(AppUser.Id, AppUser);
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
            PlatformUser user = await _platformUserRepository.Get(AppUser.Id);
            //change culture and save it.
            user.Currency = currency;
            await _platformUserRepository.UpdateAsync(user);

            ///Modify the currency in current session and update it globally in all sessions of the user.
            AppUser.Currency = currency;
            await Cache.User.Update(AppUser.Id, AppUser);
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
                using (var dbContext = new PlatformDBContext())
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
                    mainApp.AppId = mainTenant.AppId;
                    mainApp.TenantId = mainTenant.Id;
                    mainApp.UserId = mainTenant.Id;
                    mainApp.Active = activeApp == null;
                    apps.Add(mainApp);
                }

                acc.user.tenantLanguage = AppUser.TenantLanguage;
                acc.instances = tenant;
                acc.user.picture = Helpers.Storage.GetAvatarUrl(acc.user.picture);
                //acc.user.hasAnalytics = AppUser.HasAnalyticsLicense;
                acc.imageUrl = ConfigurationManager.AppSettings.Get("BlobUrl") + "/record-detail-" + tenant[0].tenantId + "/";
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

        [Route("ActiveDirectoryInfo"), HttpGet]
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
        }


        /// <summary>
        /// Uploads a new avatar for the user.
        /// </summary>
        /// <param name="fileContents">The file contents.</param>
        /// <returns>System.String.</returns>
        [Route("UploadAvatar"), HttpPost]
        public async Task<IActionResult> UploadAvatar()
        {
            // try to parse stream.
            Stream requestStream = await Request.Content.ReadAsStreamAsync();
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

                //upload file to the temporary storage.
                Storage.UploadFile(chunk, new MemoryStream(parser.FileContents), "temp", uniqueName, parser.ContentType);

                if (chunk == chunks - 1)
                {
                    //if this is last chunk, then move the file to the permanent storage by commiting it.
                    //as a standart all avatar files renamed to UserID_UniqueFileName format.
                    var user_image = string.Format("{0}_{1}", AppUser.Id, uniqueName);
                    Storage.CommitFile(uniqueName, user_image, parser.ContentType, "user-images", chunks);
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
        public async Task<IActionResult> AddUser(AddUserBindingModel request)
        {
            var resultControl = await _platformUserRepository.IsEmailAvailable(request.Email);

            if (resultControl == false)
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

            if (request.TenantId.HasValue)
            {
                if (!AppUser.Email.EndsWith("@ofisim.com"))
                    return StatusCode(HttpStatusCode.Status403Forbidden);

                var subscriberUser = await _platformUserRepository.GetUserByAutoId(request.TenantId.Value);
                tenantId = subscriberUser.TenantId.Value;
                adminUserLocalId = subscriberUser.Id;
                adminUserGlobalId = subscriberUser.Id;
                adminUserEmail = subscriberUser.Email;
                culture = subscriberUser.Culture;
                currency = subscriberUser.Currency;
                appId = subscriberUser.AppId;
                createdBy = subscriberUser.Email;

                _userRepository.TenantId = tenantId;
                _profileRepository.TenantId = tenantId;
                _roleRepository.TenantId = tenantId;
                _recordRepository.TenantId = tenantId;
            }


	        if (!request.notCheckIsAdmin)
	        {
		        //get the instance that invitation request will be created on.
		        var isOperationAllowed = await Cache.Tenant.CheckProfilesAdministrativeRights(tenantId, adminUserLocalId);

		        if (!isOperationAllowed)
		        {
			        //if current user is not the admin of the instance then reject that request and send a forbidden http request.
			        return StatusCode(HttpStatusCode.Status403Forbidden);
		        }

			}

            //if (!crmLicenseUsage.HasUserCapacity(tenantId))
            //         {
            //             //if capacity is exceeded, return payment required status code and cancel process.
            //             return StatusCode(HttpStatusCode.PaymentRequired);
            //         }

            //var hasAccount = crmPendingShareRequests.Invite(request.Email, instanceId, adminUserGlobalId, request.ProfileId, request.RoleId, createdBy);

            //if (hasAccount)
            //    return StatusCode(HttpStatusCode.Status409Conflict);

            //Register
            var randomPassword = Utils.GenerateRandomUnique(8);

            var applicationUser = new PlatformUser
            {
                //Id = Guid.NewGuid(),
                UserName = request.Email,
                Email = request.Email,
                PhoneNumber = request.Phone,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Culture = culture,
                Currency = currency,
                CreatedAt = DateTime.Now,
                AppId = appId
            };

            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var result = await userManager.CreateAsync(applicationUser, randomPassword);

            if (!result.Succeeded)
                throw new Exception();

            var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(applicationUser.Id);
            var confirmResponse = await userManager.ConfirmEmailAsync(applicationUser.Id, confirmationToken);

            if (!confirmResponse.Succeeded)
                throw new Exception();

            var registerModel = new RegisterBindingModel();
            registerModel.Email = request.Email;
            registerModel.FirstName = request.FirstName;
            registerModel.LastName = request.LastName;
            registerModel.Password = randomPassword;
            registerModel.License = "F89E4FBF-A50F-40BA-BBEC-FE027F3F1524";//Free license

            HostingEnvironment.QueueBackgroundWorkItem(clt => Integration.InsertUser(registerModel, _warehouse));

            var user = await _platformUserRepository.Get(applicationUser.Id);
            var adminUser = await _platformUserRepository.GetUserByAutoId(tenantId);
            if (request.IsOfficeUser && adminUser.ActiveDirectoryTenantId > 0)
            {
                user.ActiveDirectoryEmail = request.Email;
                user.ActiveDirectoryTenantId = adminUser.ActiveDirectoryTenantId;
            }

            var tenantUser = new User
            {
                Id = user.Id,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                FullName = $"{request.FirstName} {request.LastName}",
                Picture = "",
                IsActive = true,
                IsSubscriber = false,
                Culture = culture,
                Currency = currency,
                CreatedAt = DateTime.UtcNow,
                CreatedByEmail = adminUserEmail
            };

            //Set warehouse database name
            var warehouseInfo = await _platformWarehouseRepository.GetByTenantId(tenantId);

            _warehouse.DatabaseName = warehouseInfo != null ? warehouseInfo.DatabaseName : "0";

            await _userRepository.CreateAsync(tenantUser);
            await _profileRepository.AddUserAsync(user.Id, request.ProfileId);
            await _roleRepository.AddUserAsync(user.Id, request.RoleId);

            user.TenantId = tenantId;

            await _platformUserRepository.UpdateAsync(user);
            await Cache.Tenant.UpdateRoles(user.TenantId.Value);
            await Cache.Tenant.UpdateProfiles(user.TenantId.Value);
            await Cache.ApplicationUser.Add(user.Email, user.Id);
            await Cache.User.Get(user.Id);

            return Ok(randomPassword);
        }

        [Route("get_user"), HttpGet]
        public async Task<IActionResult> GetUser(string email)
        {
            if (!AppUser.Email.EndsWith("@ofisim.com"))
                return StatusCode(HttpStatusCode.Status403Forbidden);

            var userEntity = await _platformUserRepository.Get(email);

            if (userEntity == null)
                return NotFound();

            _userRepository.TenantId = userEntity.TenantId;
            var user = await _userRepository.GetById(userEntity.Id);

            var userModel = new
            {
                Id = user.Id,
                TenantId = userEntity.TenantId,
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

                List<PlatformUser> systemUsers = await _platformUserRepository.GetAllByTenant(AppUser.TenantId);
                var availableUsers = new JArray();

                for (var i = users.Count - 1; i >= 0; i--)
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
        }
    }
}
