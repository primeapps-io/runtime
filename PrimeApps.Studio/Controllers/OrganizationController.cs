using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common;
using PrimeApps.Model.Common.Organization;
using PrimeApps.Model.Common.Team;
using PrimeApps.Model.Entities.Console;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Constants;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Services;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/organization"), Authorize(AuthenticationSchemes = "Bearer"), ActionFilters.CheckHttpsRequire, ResponseCache(CacheProfileName = "Nocache")]
    public class OrganizationController : BaseController
    {
        private IBackgroundTaskQueue Queue;
        private IConfiguration _configuration;
        private IOrganizationRepository _organizationRepository;
        private IOrganizationUserRepository _organizationUserRepository;
        private IAppDraftRepository _appDraftRepository;
        private IPlatformUserRepository _platformUserRepository;
        private IPlatformRepository _platformRepository;
        private IServiceScopeFactory _serviceScopeFactory;
        private IConsoleUserRepository _consoleUserRepository;
        private IApplicationRepository _applicationRepository;
        private ITeamRepository _teamRepository;
        private IGiteaHelper _giteaHelper;
        private IPermissionHelper _permissionHelper;
        private IOrganizationHelper _organizationHelper;

        public OrganizationController(IBackgroundTaskQueue queue,
            IConfiguration configuration,
            IOrganizationRepository organizationRepository,
            IOrganizationUserRepository organizationUserRepository,
            IPlatformUserRepository platformUserRepository,
            IAppDraftRepository applicationDraftRepository,
            ITeamRepository teamRepository,
            IApplicationRepository applicationRepository,
            IPlatformRepository platformRepository,
            IConsoleUserRepository consoleUserRepository,
            IServiceScopeFactory serviceScopeFactory,
            IPermissionHelper permissionHelper,
            IOrganizationHelper organizationHelper,
            IGiteaHelper giteaHelper)
        {
            Queue = queue;
            _organizationRepository = organizationRepository;
            _appDraftRepository = applicationDraftRepository;
            _platformUserRepository = platformUserRepository;
            _organizationUserRepository = organizationUserRepository;
            _serviceScopeFactory = serviceScopeFactory;
            _consoleUserRepository = consoleUserRepository;
            _applicationRepository = applicationRepository;
            _platformRepository = platformRepository;
            _teamRepository = teamRepository;
            _configuration = configuration;

            _giteaHelper = giteaHelper;
            _permissionHelper = permissionHelper;
            _organizationHelper = organizationHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //SetContext(context);
            SetContextUser();
            SetCurrentUser(_organizationRepository);
            SetCurrentUser(_appDraftRepository);
            SetCurrentUser(_platformUserRepository);
            SetCurrentUser(_organizationUserRepository);
            SetCurrentUser(_consoleUserRepository);
            SetCurrentUser(_platformRepository);
            SetCurrentUser(_teamRepository);

        }

        [Route("get/{id:int}"), HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var organizations = await _organizationRepository.Get(AppUser.Id, id);

            return Ok(organizations);
        }

        [Route("collaborators"), HttpPost]
        public async Task<IActionResult> CollaboratorsAsync([FromBody]OrganizationCollaboratorModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isOrganizationAvailable = _organizationRepository.IsOrganizationAvaliable(AppUser.Id, model.OrganizationId);

            if (!isOrganizationAvailable)
                return BadRequest(ApiResponseMessages.ORGANIZATION_NOT_FOUND);

            var users = await _organizationUserRepository.GetByOrganizationId(model.OrganizationId);

            var collaborators = await _organizationHelper.CreateCollaborators(users, model.OrganizationId);

            //if (model.OrderBy != null && model.OrderBy.ToLower() == "desc")
            //{
            //    if (model.OrderField != null && model.OrderField.ToLower() == "role")
            //        collaborators = collaborators.OrderByDescending(x => x.Role).ToList();
            //    else
            //        collaborators = collaborators.OrderByDescending(x => x.FullName).ToList();
            //}
            //else
            //{
            //    if (model.OrderField != null && model.OrderField.ToLower() == "role")
            //        collaborators = collaborators.OrderBy(x => x.Role).ToList();
            //    else
            //        collaborators = collaborators.OrderBy(x => x.FullName).ToList();
            //}

            return Ok(collaborators);
        }

        [Route("teams"), HttpPost]
        public async Task<IActionResult> Teams([FromBody] OrganizationTeamModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isOrganizationAvailable = _organizationRepository.IsOrganizationAvaliable(AppUser.Id, model.OrganizationId);

            if (!isOrganizationAvailable)
                return BadRequest(ApiResponseMessages.ORGANIZATION_NOT_FOUND);

            var teams = await _teamRepository.GetByOrganizationId(model.OrganizationId);

            var teamModel = new List<TeamModel>();

            foreach (var team in teams)
            {
                var appIds = await _appDraftRepository.GetByTeamId(team.Id);

                teamModel.Add(new TeamModel
                {
                    Id = team.Id,
                    Name = team.Name,
                    Icon = team.Icon,
                    TeamUsers = team.TeamUsers.Select(x => new TeamUserModel
                    {
                        UserId = x.UserId
                    }).ToList(),
                    AppIds = appIds,
                    Deleted = team.Deleted
                });
            }

            return Ok(teamModel);
        }

        [Route("apps"), HttpPost]
        public async Task<IActionResult> Apps([FromBody] JObject request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request["organization_id"].IsNullOrEmpty())
                return BadRequest("Organization id required.");

            var isOrganizationAvailable = _organizationRepository.IsOrganizationAvaliable(AppUser.Id, (int)request["organization_id"]);

            if (!isOrganizationAvailable)
                return BadRequest(ApiResponseMessages.ORGANIZATION_NOT_FOUND);

            var search = "";
            var page = 0;
            var status = PublishStatus.NotSet;

            if (request != null)
            {
                if (!request["search"].IsNullOrEmpty())
                    search = request["search"].ToString();

                if (!request["page"].IsNullOrEmpty())
                    page = (int)request["page"];

                if (!request["status"].IsNullOrEmpty())
                    status = (PublishStatus)int.Parse(request["status"].ToString());
            }

            var apps = await _appDraftRepository.GetByOrganizationId(AppUser.Id, (int)request["organization_id"], search, page, status);


            return Ok(apps);
        }

        [Route("get_all/{id:int}"), HttpGet]
        public async Task<IActionResult> GetAll(int id)
        {
            var organization = await _organizationRepository.GetAll(AppUser.Id, id);

            if (organization == null)
                return BadRequest(ApiResponseMessages.ORGANIZATION_NOT_FOUND);

            var organizationApps = await _appDraftRepository.GetByOrganizationId(id, AppUser.Id);

            var organizationDTO = new OrganizationModel
            {
                Id = organization.Id,
                Name = organization.Name,
                Label = organization.Label,
                Icon = organization.Icon,
                OwnerId = organization.OwnerId,
                Default = organization.Default,
                Teams = organization.Teams.Where(x => !x.Deleted).Select(x =>
                    new TeamModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Icon = x.Icon,
                        TeamUsers = x.TeamUsers.Select(y =>
                            new TeamUserModel
                            {
                                UserId = y.UserId
                            })
                        .ToList(),
                        Deleted = x.Deleted
                    })
                .ToList(),
                Apps = organizationApps,
                Users = new List<OrganizationUserModel>()
            };

            foreach (var user in organization.OrganizationUsers)
            {
                var platformUser = await _platformUserRepository.GetSettings(user.UserId);

                organizationDTO.Users.Add(new OrganizationUserModel
                {
                    Id = user.Id,
                    OrganizationId = user.OrganizationId,
                    Role = user.Role,
                    Email = platformUser.Email,
                    FirstName = platformUser.FirstName,
                    LastName = platformUser.LastName,
                    FullName = platformUser.FirstName + " " + platformUser.LastName,
                    CreatedAt = platformUser.CreatedAt
                });
            }

            foreach (var team in organizationDTO.Teams)
            {
                var appIds = await _appDraftRepository.GetByTeamId(team.Id);

                team.AppIds = appIds;
            }

            return Ok(organizationDTO);
        }

        [Route("get_users/{id:int}"), HttpGet]
        public async Task<IActionResult> GetUsers(int id)
        {
            var organization = await _organizationRepository.GetUsersByOrganizationId(id);

            var ids = new List<int>();

            foreach (var item in organization)
                ids.Add(item.UserId);

            var orgJOject = new JObject();
            var array = new JArray();
            var platformUsers = await _platformUserRepository.GetByIds(ids);

            orgJOject["organization_id"] = id;

            foreach (var user in platformUsers)
            {
                var data = new JObject();

                data["user_id"] = user.Id;

                data["first_name"] = user.FirstName;
                data["last_name"] = user.LastName;
                data["full_name"] = user.GetFullName();
                data["email"] = user.Email;

                array.Add(data);
            }

            orgJOject["users"] = array;

            return Ok(orgJOject);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody] OrganizationModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var organization = new Organization
            {
                Name = model.Name,
                Label = model.Label,
                Icon = model.Icon,
                Color = model.Color,
                OwnerId = AppUser.Id,
                OrganizationUsers = new List<OrganizationUser>()
            };

            organization.OrganizationUsers.Add(new OrganizationUser
            {
                UserId = AppUser.Id,
                Role = OrganizationRole.Administrator
            });

            var result = await _organizationRepository.Create(organization);

            if (result < 0)
                return BadRequest("An error occurred while creating an organization");

            Queue.QueueBackgroundWorkItem(token => _giteaHelper.CreateOrganization(model.Name, model.Label, AppUser.Email, Request.Cookies["gitea_token"]));

            return Ok(organization.Id);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]OrganizationModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var organization = await _organizationRepository.Get(AppUser.Id, id);

            organization.Label = model.Label;
            organization.Icon = model.Icon;
            organization.Color = model.Color;

            if (organization == null)
                return NotFound(ApiResponseMessages.ORGANIZATION_NOT_FOUND);

            await _organizationRepository.Update(organization);

            return Ok();
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var organization = await _organizationRepository.Get(AppUser.Id, id);

            if (organization == null)
                return NotFound(ApiResponseMessages.ORGANIZATION_NOT_FOUND);

            if (organization.Default)
                return BadRequest("Default organization cannot be deleted.");

            if (!await _permissionHelper.CheckUserRole(AppUser.Id, organization.Id, OrganizationRole.Administrator))
                return Forbid(ApiResponseMessages.PERMISSION);

            await _organizationRepository.Delete(organization);

            return Ok();
        }

        [Route("add_user"), HttpPost]
        public async Task<IActionResult> AddUser([FromBody] OrganizationUserModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var organization = await _organizationRepository.Get(AppUser.Id, model.OrganizationId);

            if (organization == null)
                return BadRequest(ApiResponseMessages.ORGANIZATION_NOT_FOUND);

            if (!await _permissionHelper.CheckUserRole(AppUser.Id, model.OrganizationId, OrganizationRole.Administrator))
                return Forbid(ApiResponseMessages.PERMISSION);

            var appInfo = await _applicationRepository.GetByNameAsync(_configuration.GetSection("AppSettings")["ClientId"]);

            var result = 0;
            string password = "";
            using (var httpClient = new HttpClient())
            {
                var token = await HttpContext.GetTokenAsync("access_token");
                var url = Request.Scheme + "://" + appInfo.Setting.AuthDomain + "/user/add_organization_user";
                httpClient.BaseAddress = new Uri(url);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Headers["Authorization"].ToString().Substring("Basic ".Length).Trim());

                var json = JsonConvert.SerializeObject(model);
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

                    var platformUser = await _platformUserRepository.Get(model.Email);

                    var consoleUser = new ConsoleUser
                    {
                        Id = platformUser.Id,
                        UserOrganizations = new List<OrganizationUser>()
                    };

                    consoleUser.UserOrganizations.Add(new OrganizationUser
                    {
                        UserId = platformUser.Id,
                        Role = model.Role,
                        OrganizationId = model.OrganizationId,
                        CreatedById = AppUser.Id,
                        CreatedAt = DateTime.Now
                    });

                    result = await _consoleUserRepository.Create(consoleUser);
                }
            }

            return StatusCode(201, new { password = password });
        }

        [Route("delete_user"), HttpPost]
        public async Task<IActionResult> DeleteUser([FromBody]OrganizationUser model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var organization = await _organizationRepository.Get(AppUser.Id, model.OrganizationId);

            if (organization == null)
                return NotFound(ApiResponseMessages.ORGANIZATION_NOT_FOUND);

            if (!await _permissionHelper.CheckUserRole(AppUser.Id, model.OrganizationId, OrganizationRole.Administrator))
                return Forbid(ApiResponseMessages.PERMISSION);

            if (model.Id == organization.OwnerId)
                return BadRequest(ApiResponseMessages.OWN_ORGANIZATION);

            var result = await _organizationUserRepository.Delete(model);

            return Ok(result);
        }

        [Route("update_user"), HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody]OrganizationUserModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var organization = _organizationRepository.IsOrganizationAvaliable(AppUser.Id, model.OrganizationId);

            if (!organization)
                return NotFound(ApiResponseMessages.ORGANIZATION_NOT_FOUND);

            if (!await _permissionHelper.CheckUserRole(AppUser.Id, model.OrganizationId, OrganizationRole.Administrator))
                return Forbid(ApiResponseMessages.PERMISSION);

            var user = await _organizationUserRepository.Get(model.Id, model.OrganizationId);
            user.Role = model.Role;

            var result = await _organizationUserRepository.Update(user);

            return Ok(result);
        }

        [Route("find/{organizationId:int}"), HttpPost]
        public async Task<IActionResult> Find(int organizationId, [FromBody]PaginationModel paginationModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isOrganizationAvailable = _organizationRepository.IsOrganizationAvaliable(AppUser.Id, organizationId);

            if (!isOrganizationAvailable)
                return BadRequest(ApiResponseMessages.ORGANIZATION_NOT_FOUND);

            var users = await _organizationUserRepository.GetByOrganizationId(organizationId);

            var collaborators = await _organizationHelper.CreateCollaborators(users, organizationId);
            collaborators = collaborators.Skip(paginationModel.Offset * paginationModel.Limit)
                .Take(paginationModel.Limit).ToList();

            if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
            {
                var propertyInfo = typeof(Team).GetProperty(paginationModel.OrderColumn);

                if (paginationModel.OrderType == "asc")
                {
                    collaborators = collaborators.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
                }
                else
                {
                    collaborators = collaborators.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList();
                }

            }

            if (collaborators == null)
                return NotFound();

            return Ok(collaborators);
        }

        [Route("count/{organizationId:int}"), HttpGet]
        public async Task<IActionResult> Count(int organizationId)
        {
            var users = await _organizationUserRepository.GetByOrganizationId(organizationId);

            var collaborators = await _organizationHelper.CreateCollaborators(users, organizationId);

            var count = collaborators != null ? collaborators.Count() : 0;

            if (count < 1)
                return NotFound();

            return Ok(count);
        }

        [Route("is_unique_name"), HttpGet]
        public async Task<IActionResult> IsUniqueName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest(ModelState);

            var result = await _organizationRepository.IsOrganizationNameAvailable(name);

            return Ok(result);
        }
    }
}
