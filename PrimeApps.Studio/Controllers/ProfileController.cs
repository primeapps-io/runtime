using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PrimeApps.Model.Common;
using PrimeApps.Model.Common.App;
using PrimeApps.Model.Common.Profile;
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

        public AppDraftUserController(IRelationRepository relationRepository, IProfileRepository profileRepository, ISettingRepository settingRepository, IModuleRepository moduleRepository, Warehouse warehouse, IModuleHelper moduleHelper, IConfiguration configuration, IHelpRepository helpRepository, IUserRepository userRepository, IApplicationRepository applicationRepository)
        {
            _relationRepository = relationRepository;
            _profileRepository = profileRepository;
            _settingRepository = settingRepository;
            _warehouse = warehouse;
            _configuration = configuration;
            _moduleHelper = moduleHelper;
            _userRepository = userRepository;
            _applicationRepository = applicationRepository;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_relationRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_profileRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_settingRepository, PreviewMode, AppId, TenantId);

            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Creates a new profile.
        /// </summary>
        /// <param name="NewProfile"></param>
        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]AppDraftUserModel NewProfile)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            /*var organization = await _appDraftUserRepository.Get(AppUser.Id, model.OrganizationId);

            if (organization == null)
                return BadRequest(ApiResponseMessages.ORGANIZATION_NOT_FOUND);
                */

            var clientId = _configuration.GetValue("AppSettings:ClientId", string.Empty);
            var result = 0;
            string password = "";
            if (!string.IsNullOrEmpty(clientId))
            {
                var appInfo = await _applicationRepository.GetByNameAsync(clientId);

                using (var httpClient = new HttpClient())
                {
                    var token = await HttpContext.GetTokenAsync("access_token");
                    var url = Request.Scheme + "://" + appInfo.Setting.AuthDomain + "/user/add_app_draft_user";
                    httpClient.BaseAddress = new Uri(url);
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Headers["Authorization"].ToString().Substring("Basic ".Length).Trim());

                    var json = JsonConvert.SerializeObject(NewProfile);
                    var response = await httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

                    if (!response.IsSuccessStatusCode)
                        return BadRequest(response);

                    using (var content = response.Content)
                    {
                        var stringResult = content.ReadAsStringAsync().Result;

                        var jsonResult = JObject.Parse(stringResult);
                        password = jsonResult["password"].ToString();
                        var templates = await _platformRepository.GetAppTemplate(AppUser.AppId, AppTemplateType.Email, AppUser.Culture.Substring(0, 2), "organization_invitation");

                        foreach (var template in templates)
                        {
                            template.Content = template.Content.Replace("{:FirstName}", model.FirstName);
                            template.Content = template.Content.Replace("{:LastName}", model.LastName);
                            template.Content = template.Content.Replace("{:Organization}", organization.Label);
                            template.Content = template.Content.Replace("{:InvitationFrom}", AppUser.FullName);
                            template.Content = template.Content.Replace("{:Email}", model.Email);
                            template.Content = template.Content.Replace("{:Url}", Request.Scheme + "://" + appInfo.Setting.AuthDomain + "/account/confirmemail?email=" + model.Email + "&code=" + WebUtility.UrlEncode(jsonResult["token"].ToString()));

                            if (!string.IsNullOrEmpty(jsonResult["token"].ToString()))
                                template.Content = template.Content.Replace("{:ShowActivateEmail}", "initial");
                            else
                                template.Content = template.Content.Replace("{:ShowActivateEmail}", "none");

                            var req = JsonConvert.DeserializeObject<JObject>(template.Settings);

                            var myMessage = new MailMessage()
                            {
                                From = new MailAddress((string)req["MailSenderEmail"], (string)req["MailSenderName"]),
                                Subject = template.Subject,
                                Body = template.Content,
                                IsBodyHtml = true
                            };

                            myMessage.To.Add(model.Email);

                            var email = new Email(_configuration, _serviceScopeFactory);
                            email.TransmitMail(myMessage);
                        }
                    }
                    var platformUser = await _platformUserRepository.Get(model.Email);

                    var studioUser = new StudioUser
                    {
                        Id = platformUser.Id,
                        UserOrganizations = new List<OrganizationUser>()
                    };

                    studioUser.UserOrganizations.Add(new OrganizationUser
                    {
                        UserId = platformUser.Id,
                        Role = model.Role,
                        OrganizationId = model.OrganizationId,
                        CreatedById = AppUser.Id,
                        CreatedAt = DateTime.Now
                    });

                    result = await _studioUserRepository.Create(studioUser);
                }
            }

            return StatusCode(201, new { password = password });

            return Ok();
        }

        /// <summary>
        /// Updates an existing profile.
        /// </summary>
        /// <param name="UpdatedProfile"></param>
        [Route("update"), HttpPost]
        public async Task<IActionResult> Update([FromBody]ProfileDTO UpdatedProfile)
        {
            //Set Warehouse
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            await _profileRepository.UpdateAsync(UpdatedProfile, AppUser.TenantLanguage);
            return Ok();
        }

        /// <summary>
        /// Removes a profile and replaces its relations with another profile.
        /// </summary>
        /// <param name="RemovalRequest"></param>
        [Route("remove"), HttpPost]
        public async Task<IActionResult> Remove([FromBody]ProfileRemovalDTO RemovalRequest)
        {
            await _profileRepository.RemoveAsync(RemovalRequest.RemovedProfile.ID, RemovalRequest.TransferProfile.ID);


            return Ok();
        }

        /// <summary>
        /// Gets all profiles and permissions belong to users workgroups with a lightweight user id list.
        /// </summary>
        /// <returns></returns>
        [Route("get_all"), HttpPost]
        public async Task<IActionResult> GetAll()
        {
            IEnumerable<ProfileWithUsersDTO> profileList = await _profileRepository.GetAllProfiles();

            return Ok(profileList);
        }

        /// <summary>
        /// Changes users profile with another one.
        /// </summary>
        /// <param name="transfer"></param>
        [Route("change_user_profile"), HttpPost]
        public async Task<IActionResult> ChangeUserProfile([FromBody]ProfileTransferDTO transfer)
        {
            await _profileRepository.AddUserAsync(transfer.UserID, transfer.TransferedProfileID);
            /// update session cache

            return Ok();
        }

        [Route("get_all_basic"), HttpGet]
        public async Task<IActionResult> GetAllBasic()
        {
            var profiles = await _profileRepository.GetAll();

            return Ok(profiles);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var profileEntity = await _profileRepository.GetByIdBasic(id);

            if (profileEntity == null)
                return NotFound();

            await _profileRepository.DeleteSoft(profileEntity);

            return Ok();
        }

        [Route("count"), HttpGet]
        public async Task<IActionResult> Count([FromUri]TemplateType templateType)
        {
            var count = await _profileRepository.Count();

            //if (count < 1)
            //	return NotFound(count);

            return Ok(count);
        }

        [Route("find"), HttpPost]
        public async Task<IActionResult> Find([FromBody]PaginationModel paginationModel)
        {
            var templates = await _profileRepository.Find(paginationModel);

            //if (templates == null)
            //	return NotFound();

            return Ok(templates);
        }

    }
}
