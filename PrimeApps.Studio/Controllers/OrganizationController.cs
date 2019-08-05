using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
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
using PrimeApps.Model.Entities.Studio;
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
        private IStudioUserRepository _studioUserRepository;
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
            IStudioUserRepository studioUserRepository,
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
            _studioUserRepository = studioUserRepository;
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
            SetCurrentUser(_studioUserRepository);
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
        public async Task<IActionResult> Teams([FromBody]OrganizationTeamModel model)
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
        public async Task<IActionResult> Apps([FromBody]JObject request)
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
        public async Task<IActionResult> Create([FromBody]OrganizationModel model)
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

            Queue.QueueBackgroundWorkItem(token => _giteaHelper.CreateOrganization(model.Name, model.Label, AppUser.Email, "token"));

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
        public async Task<IActionResult> AddUser([FromBody]OrganizationUserModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var organization = await _organizationRepository.Get(AppUser.Id, model.OrganizationId);

            if (organization == null)
                return BadRequest(ApiResponseMessages.ORGANIZATION_NOT_FOUND);

            if (!await _permissionHelper.CheckUserRole(AppUser.Id, model.OrganizationId, OrganizationRole.Administrator))
                return Forbid(ApiResponseMessages.PERMISSION);

            var clientId = _configuration.GetValue("AppSettings:ClientId", string.Empty);
            string password = "";
            if (!string.IsNullOrEmpty(clientId))
            {
                var appInfo = await _applicationRepository.GetByNameAsync(clientId);

                using (var httpClient = new HttpClient())
                {
                    var platformUser = await _platformUserRepository.GetAsync(model.Email);

                    if (platformUser == null)
                    {
                        var token = await HttpContext.GetTokenAsync("access_token");
                        var url = Request.Scheme + "://" + appInfo.Setting.AuthDomain + "/user/add_organization_user";
                        httpClient.BaseAddress = new Uri(url);
                        httpClient.DefaultRequestHeaders.Accept.Clear();
                        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Request.Headers["Authorization"].ToString().Substring("Basic ".Length).Trim());

                        model.AppName = appInfo.Name;

                        var json = JsonConvert.SerializeObject(model);
                        var response = await httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

                        if (!response.IsSuccessStatusCode)
                            return BadRequest(response);

                        platformUser = await _platformUserRepository.GetAsync(model.Email);
                    }

                    var studioUser = await _studioUserRepository.GetWithOrganizations(platformUser.Id);

                    if (studioUser == null)
                    {
                        studioUser = new StudioUser
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

                        await _studioUserRepository.Create(studioUser);
                    }
                    else
                    {
                        studioUser.UserOrganizations.Add(new OrganizationUser
                        {
                            UserId = platformUser.Id,
                            Role = model.Role,
                            OrganizationId = model.OrganizationId,
                            CreatedById = AppUser.Id,
                            CreatedAt = DateTime.Now
                        });

                        await _studioUserRepository.Update(studioUser);
                    }
                }
            }

            return StatusCode(201, new {password = password});
        }

        [Route("delete_user"), HttpPost]
        public async Task<IActionResult> DeleteUser([FromBody]OrganizationUser model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.Role == PrimeApps.Model.Enums.OrganizationRole.Administrator && AppUser.Id == model.UserId)
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
                var propertyInfo = typeof(Team).GetProperty(char.ToUpper(paginationModel.OrderColumn[0]) + paginationModel.OrderColumn.Substring(1));

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

        [Route("is_user_exist"), HttpGet]
        public async Task<IActionResult> IsUserExist(string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email is required!");

            var result = await _platformUserRepository.GetAsync(email);

            return Ok(result);
        }

        [Route("send_email_password"), HttpPost]
        public async Task<IActionResult> SendEmailPassword([FromBody]JObject data)
        {
            var applicationInfo = await _applicationRepository.Get(int.Parse(data["app_id"].ToString()));

            var templates = await _platformRepository.GetAppTemplate(int.Parse(data["app_id"].ToString()),
                AppTemplateType.Email, data["culture"].ToString().Substring(0, 2), "add_collaborator");

            foreach (var template in templates)
            {
                var content = template.Content;

                content = content.Replace("{:FirstName}", data["first_name"].ToString());
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