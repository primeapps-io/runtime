using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common;
using PrimeApps.Model.Common.App;
using PrimeApps.Model.Common.Profile;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/app_draft_user")]
    public class AppDraftUserController : DraftBaseController
    {
        private IRelationRepository _relationRepository;
        private IUserRepository _userRepository;
        private IProfileRepository _profileRepository;
        private ISettingRepository _settingRepository;
        private IModuleRepository _moduleRepository;
        private IConfiguration _configuration;
        private Warehouse _warehouse;
        private IModuleHelper _moduleHelper;
        private IApplicationRepository _applicationRepository;
        private IPlatformRepository _platformRepository;


        public AppDraftUserController(IRelationRepository relationRepository, IProfileRepository profileRepository, ISettingRepository settingRepository, IModuleRepository moduleRepository, Warehouse warehouse, IModuleHelper moduleHelper, IConfiguration configuration, IHelpRepository helpRepository, IUserRepository userRepository, IApplicationRepository applicationRepository, IPlatformRepository platformRepository)
        {
            _relationRepository = relationRepository;
            _profileRepository = profileRepository;
            _settingRepository = settingRepository;
            _warehouse = warehouse;
            _configuration = configuration;
            _moduleHelper = moduleHelper;
            _userRepository = userRepository;
            _applicationRepository = applicationRepository;
            _platformRepository = platformRepository;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_userRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_relationRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_profileRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_settingRepository, PreviewMode, AppId, TenantId);

            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="Create"></param>
        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]AppDraftUserModel userModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userRepository.GetByEmail(userModel.Email);

            if (user != null)
                return BadRequest(new { message = "User already exist" });


            var clientId = _configuration.GetValue("AppSettings:ClientId", string.Empty);
            var result = 0;
            string password = "";

            if (!string.IsNullOrEmpty(clientId))
            {
                var appInfo = await _applicationRepository.GetByNameAsync(clientId);

                using (var httpClient = new HttpClient())
                {
                    var url = Request.Scheme + "://" + appInfo.Setting.AuthDomain + "/user/add_app_draft_user";
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Headers["Authorization"].ToString().Substring("Basic ".Length).Trim());

                    var json = JsonConvert.SerializeObject(userModel);
                    var response = await httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

                    if (!response.IsSuccessStatusCode)
                        return BadRequest(response);

                    using (var content = response.Content)
                    {
                        var stringResult = content.ReadAsStringAsync().Result;

                        var jsonResult = JObject.Parse(stringResult);
                        password = jsonResult["password"].ToString();
                        var platformUserId = int.Parse(jsonResult["id"].ToString());
                        var tenantUser = new TenantUser
                        {
                            Id = platformUserId,
                            FirstName = userModel.FirstName,
                            IsActive = userModel.IsActive,
                            LastName = userModel.LastName,
                            Email = userModel.Email,
                            FullName = userModel.FirstName + " " + userModel.LastName,
                            ProfileId = userModel.ProfileId,
                            RoleId = userModel.RoleId,
                            Culture = AppUser.Culture,
                            Currency = AppUser.Currency
                        };

                        await _userRepository.CreateAsync(tenantUser);
                    }
                }
            }

            return StatusCode(201, new { password = password });
        }

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        /// <param name="UpdatedUser"></param>
        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]AppDraftUserModel userModel)
        {
            var user = await _userRepository.GetById(id);
            if (user == null)
                return BadRequest();

            user.FirstName = userModel.FirstName;
            user.LastName = userModel.LastName;
            user.IsActive = userModel.IsActive;
            user.FullName = userModel.FirstName + " " + userModel.LastName;
            user.ProfileId = userModel.ProfileId;
            user.RoleId = userModel.RoleId;
            await _userRepository.UpdateAsync(user);

            return Ok();
        }

        /// <summary>
        /// Removes a profile and replaces its relations with another profile.
        /// </summary>
        /// <param name="RemovalRequest"></param>
        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userRepository.GetById(id);

            if (user == null)
                return BadRequest();

            user.Deleted = true;
            await _userRepository.UpdateAsync(user);

            return Ok();
        }

        [Route("count"), HttpGet]
        public async Task<IActionResult> Count()
        {
            var count = await _userRepository.Count();

            return Ok(count);
        }

        [Route("find"), HttpPost]
        public async Task<IActionResult> Find([FromBody]PaginationModel paginationModel)
        {
            var users = await _userRepository.Find(paginationModel);

            return Ok(users);
        }

        [Route("send_email_password"), HttpPost]
        public async Task<IActionResult> SendEmailPassword([FromBody]JObject data)
        {
            var applicationInfo = await _applicationRepository.Get(int.Parse(data["app_id"].ToString()));

            var templates = await _platformRepository.GetAppTemplate(int.Parse(data["app_id"].ToString()),
                AppTemplateType.Email, data["culture"].ToString().Substring(0, 2), "app_draft_user");

            foreach (var template in templates)
            {
                var content = template.Content;

                content = content.Replace("{:DisplayName}", data["display_name"].ToString());
                content = content.Replace("{:Email}", data["email"].ToString());
                content = content.Replace("{:Password}", data["password"].ToString());


                Email notification = new Email(_configuration, null, template.Subject, content);

                var req = JsonConvert.DeserializeObject<JObject>(template.Settings);

                if (req != null)
                {
                    var senderEmail = (string)req["MailSenderEmail"] ?? applicationInfo.Setting.MailSenderEmail;
                    var senderName = (string)req["MailSenderName"] ?? applicationInfo.Setting.MailSenderName;
                    notification.AddRecipient(data["email"].ToString());
                    notification.AddToQueue(senderEmail, senderName, null, null, content, template.Subject);
                }
            }

            return Ok();
        }
    }
}