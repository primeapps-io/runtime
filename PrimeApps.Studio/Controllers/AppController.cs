using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common.App;
using PrimeApps.Model.Entities.Studio;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Constants;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Services;
using PrimeApps.Model.Storage;
using PrimeApps.Model.Entities.Platform;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/app")]
    public class AppController : ApiBaseController
    {
        private IBackgroundTaskQueue Queue;
        private IConfiguration _configuration;
        private IPlatformUserRepository _platformUserRepository;
        private IAppDraftRepository _appDraftRepository;
        private ICollaboratorsRepository _collaboratorRepository;
        private IOrganizationRepository _organizationRepository;
        private IPermissionHelper _permissionHelper;
        private IGiteaHelper _giteaHelper;
        private IUnifiedStorage _storage;
        private IUserRepository _userRepository;
        private IAppDraftTemplateHelper _draftTemplateHelper;
        private IAppDraftTemplateRepository _appDraftTemplateRepository;

        public AppController(IConfiguration configuration,
            IBackgroundTaskQueue queue,
            IPlatformUserRepository platformUserRepository,
            IAppDraftRepository appDraftRepository,
            ICollaboratorsRepository collaboratorRepository,
            IOrganizationRepository organizationRepository,
            IPermissionHelper permissionHelper,
            IGiteaHelper giteaHelper,
            IUnifiedStorage storage,
            IUserRepository userRepository,
            IAppDraftTemplateHelper draftTemplateHelper,
            IAppDraftTemplateRepository appDraftTemplateRepository)
        {
            Queue = queue;
            _configuration = configuration;
            _platformUserRepository = platformUserRepository;
            _appDraftRepository = appDraftRepository;
            _collaboratorRepository = collaboratorRepository;
            _organizationRepository = organizationRepository;
            _permissionHelper = permissionHelper;
            _giteaHelper = giteaHelper;
            _storage = storage;
            _userRepository = userRepository;
            _draftTemplateHelper = draftTemplateHelper;
            _appDraftTemplateRepository = appDraftTemplateRepository;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_platformUserRepository);
            SetCurrentUser(_appDraftRepository);
            SetCurrentUser(_organizationRepository);
            SetCurrentUser(_collaboratorRepository);
            SetCurrentUser(_appDraftTemplateRepository);
            base.OnActionExecuting(context);
        }

        [Route("get/{id:int}"), HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            //
            //            if (!await _permissionHelper.CheckUserRole(AppUser.Id, OrganizationId, OrganizationRole.Administrator))
            //                return Forbid(ApiResponseMessages.PERMISSION);

            var app = await _appDraftRepository.Get(id);
            app.Secret = CryptoHelper.Decrypt(app.Secret);
            return Ok(app);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]AppDraftModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _permissionHelper.CheckUserRole(AppUser.Id, OrganizationId, OrganizationRole.Administrator))
                return Forbid(ApiResponseMessages.PERMISSION);

            var secret = Guid.NewGuid().ToString().Replace("-", string.Empty);
            var secretEncrypt = CryptoHelper.Encrypt(secret);

            var app = new AppDraft
            {
                Name = model.Name,
                Label = model.Label,
                Description = model.Description,
                Logo = model.Logo,
                OrganizationId = OrganizationId,
                TempletId = model.TempletId,
                Color = model.Color,
                Icon = model.Icon,
                Secret = secretEncrypt,
                Setting = new AppDraftSetting()
                {
                    AuthDomain = _configuration.GetValue("AppSettings:AuthenticationServerURL", string.Empty).Replace("https://", string.Empty).Replace("http://", string.Empty),
                    AppDomain = string.Format(_configuration.GetValue("AppSettings:AppUrl", string.Empty), model.Name),
                    Currency = "USD",
                    Culture = "en-US",
                    TimeZone = "America/New_York",
                    Language = "en",
                    AuthTheme = new JObject()
                    {
                        ["color"] = "#555198",
                        ["title"] = "PrimeApps",
                        ["banner"] = new JArray { new JObject { ["image"] = "", ["descriptions"] = "" } },
                    }.ToJsonString(),
                    AppTheme = new JObject()
                    {
                        ["color"] = "#555198",
                        ["title"] = "PrimeApps",
                    }.ToJsonString(),
                    MailSenderName = "PrimeApps",
                    MailSenderEmail = "app@primeapps.io",
                    Options = new JObject
                    {
                        ["enable_registration"] = true,
                        ["enable_api_registration"] = true,
                        ["enable_ldap"] = false
                    }.ToJsonString()
                }
            };

            var result = await _appDraftRepository.Create(app);

            if (result < 0)
                return BadRequest("An error occurred while creating an app");

            app.Collaborators = new List<AppCollaborator>
            {
                new AppCollaborator
                {
                    UserId = AppUser.Id, Profile = ProfileEnum.Manager
                }
            };

            var resultUpdate = await _appDraftRepository.Update(app);

            if (resultUpdate < 0)
                return BadRequest("An error occurred while creating an app");

            await Postgres.CreateDatabaseWithTemplet(_configuration.GetConnectionString("TenantDBConnection"), app.Id, model.TempletId);

            #region #3793
            //Add app owner to users table for preview application.
            var tenantUser = new TenantUser
            {
                Id = AppUser.Id,
                FirstName = AppUser.FirstName,
                IsActive = true,
                LastName = AppUser.LastName,
                Email = AppUser.Email,
                FullName = AppUser.FullName,
                ProfileId = 1,
                RoleId = 1,
                Culture = AppUser.Culture,
                Currency = AppUser.Currency
            };

            _userRepository.CurrentUser = new CurrentUser { UserId = 1, TenantId = app.Id, PreviewMode = "app" };
            await _userRepository.CreateAsync(tenantUser);
            var platformUser = await _platformUserRepository.GetWithTenants(AppUser.Email);

            //Platform user_tenant table include this user with tenant_id = 1 for preview application.
            if (platformUser.TenantsAsUser.FirstOrDefault(x => x.TenantId == 1) == null)
            {
                platformUser.TenantsAsUser.Add(new UserTenant { TenantId = 1, PlatformUser = platformUser });
                await _platformUserRepository.UpdateAsync(platformUser);
            }
            #endregion
            //Default templates
            var appDraftTemplates = await _appDraftTemplateRepository.GetAllById(1);

            Queue.QueueBackgroundWorkItem(token => _draftTemplateHelper.CreateAppDraftTemplates(appDraftTemplates, app));
            Queue.QueueBackgroundWorkItem(token => _giteaHelper.CreateRepository(OrganizationId, model.Name, AppUser));

            if (Request.Host.Value.Contains("localhost"))
                await _storage.CreateBucketPolicy($"app{app.Id}", $"{Request.Scheme}://localhost:*", UnifiedStorage.PolicyType.StudioPolicy);
            else
            {
                await _storage.CreateBucketPolicy($"app{app.Id}", $"{Request.Scheme}://*.primeapps.io", UnifiedStorage.PolicyType.StudioPolicy);
                await _storage.AddHttpReferrerUrlToBucket($"app{app.Id}", $"{Request.Scheme}://*.primeapps.app", UnifiedStorage.PolicyType.StudioPolicy);
            }

            return Ok(app);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]AppDraftModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _permissionHelper.CheckUserRole(AppUser.Id, OrganizationId, OrganizationRole.Administrator))
                return Forbid(ApiResponseMessages.PERMISSION);

            var app = await _appDraftRepository.Get(id);
            app.Label = model.Label;
            app.Description = model.Description;
            app.Logo = model.Logo;
            app.Icon = model.Icon;
            app.Color = model.Color;

            app.Setting.AppDomain = model.AppDomain;
            app.Setting.AuthDomain = model.AuthDomain;

            var options = JObject.Parse(app.Setting.Options);
            options["enable_registration"] = model.EnableRegistration;
            options["enable_api_registration"] = model.EnableAPIRegistration;
            options["enable_ldap"] = model.EnableLDAP;
            options["protect_modules"] = model.ProtectModules.ToString();
            options["selected_modules"] = (JArray)model.SelectedModules;
            // options["clear_all_records"] = model.ClearAllRecords;

            app.Setting.Options = options.ToJsonString();

            await _appDraftRepository.Update(app);

            return Ok(app);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _permissionHelper.CheckUserRole(AppUser.Id, OrganizationId, OrganizationRole.Administrator))
                return Forbid(ApiResponseMessages.PERMISSION);

            var app = await _appDraftRepository.Get(id);
            var result = await _appDraftRepository.Delete(app);

            return Ok(result);
        }

        /*[Route("get_all"), HttpPost]
		public async Task<IActionResult> Organizations([FromBody]JObject request)
		{
			var search = "";
			var page = 0;

			if (request != null)
			{
				if (!request["search"].IsNullOrEmpty())
					search = request["search"].ToString();

				if (request["page"].IsNullOrEmpty())
					page = (int)request["page"];
			}

			var organizations = await _appDraftRepository.GetAllByUserId(AppUser.Id, search, page);

			return Ok(organizations);
		}*/

        [Route("is_unique_name"), HttpGet]
        public async Task<IActionResult> IsUniqueName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest(ModelState);

            var app = await _appDraftRepository.Get(name);

            return app == null ? Ok(true) : Ok(false);
        }

        [Route("get_collaborators/{id:int}"), HttpGet]
        public async Task<IActionResult> GetAppCollaborators(int id)
        {
            if (!await _permissionHelper.CheckUserRole(AppUser.Id, OrganizationId, OrganizationRole.Administrator))
                return Forbid(ApiResponseMessages.PERMISSION);

            var app = await _appDraftRepository.GetAppCollaborators(id);

            return Ok(app);
        }


        [Route("update_auth_theme/{id:int}"), HttpPut]
        public async Task<IActionResult> UpdateAuthTheme(int id, [FromBody]JObject model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _permissionHelper.CheckUserRole(AppUser.Id, OrganizationId, OrganizationRole.Administrator))
                return Forbid(ApiResponseMessages.PERMISSION);


            var result = await _appDraftRepository.UpdateAuthTheme(id, model);

            return Ok(result);
        }

        [Route("get_auth_theme/{id:int}"), HttpGet]
        public async Task<IActionResult> GetAuthTheme(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _permissionHelper.CheckUserRole(AppUser.Id, OrganizationId, OrganizationRole.Administrator))
                return Forbid(ApiResponseMessages.PERMISSION);

            var app = await _appDraftRepository.GetSettings(id);

            if (app != null)
                return Ok(app.AuthTheme);
            else
                return Ok(app);
        }

        [Route("update_app_theme/{id:int}"), HttpPut]
        public async Task<IActionResult> UpdateAppTheme(int id, [FromBody]JObject model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _permissionHelper.CheckUserRole(AppUser.Id, OrganizationId, OrganizationRole.Administrator))
                return Forbid(ApiResponseMessages.PERMISSION);


            var result = await _appDraftRepository.UpdateAppTheme(id, model);

            return Ok(result);
        }

        [Route("get_app_theme/{id:int}"), HttpGet]
        public async Task<IActionResult> GetAppTheme(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _permissionHelper.CheckUserRole(AppUser.Id, OrganizationId, OrganizationRole.Administrator))
                return Forbid(ApiResponseMessages.PERMISSION);

            var app = await _appDraftRepository.GetSettings(id);

            return app != null ? Ok(app.AppTheme) : Ok(app);
        }

        [Route("migration/{id:int}"), HttpGet]
        public async Task<IActionResult> Migration(int id)
        {
            var app = await _appDraftRepository.Get(id);

            if (!string.IsNullOrEmpty(app.Logo))
            {
                var regex = new Regex(@"[\w-]+.(jpg|png|jpeg)");
                var match = regex.Match(app.Logo);
                if (match.Success)
                {
                    Console.WriteLine("MATCH VALUE: " + match.Value);
                }
            }

            return app != null ? Ok(app.Setting.AppTheme) : Ok(app);
        }
    }
}