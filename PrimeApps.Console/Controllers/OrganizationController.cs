using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Console.Constants;
using PrimeApps.Console.Helpers;
using PrimeApps.Model.Common.Organization;
using PrimeApps.Model.Common.Team;
using PrimeApps.Model.Entities.Console;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Console.Controllers
{
    [Route("api/organization"), Authorize(AuthenticationSchemes = "Bearer"), ActionFilters.CheckHttpsRequire, ResponseCache(CacheProfileName = "Nocache")]
    public class OrganizationController : BaseController
    {
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

        private IPermissionHelper _permissionHelper;

        public OrganizationController(IConfiguration configuration, IOrganizationRepository organizationRepository, IOrganizationUserRepository organizationUserRepository, IPlatformUserRepository platformUserRepository, IAppDraftRepository applicationDraftRepository, ITeamRepository teamRepository, IApplicationRepository applicationRepository, IPlatformRepository platformRepository, IConsoleUserRepository consoleUserRepository, IServiceScopeFactory serviceScopeFactory, IPermissionHelper permissionHelper)
        {
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

            _permissionHelper = permissionHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated || string.IsNullOrWhiteSpace(context.HttpContext.User.FindFirst("email").Value))
                context.Result = new UnauthorizedResult();

            SetContextUser();
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
            var collaborators = new List<OrganizationUserModel>();

            foreach (var user in users)
            {
                var platformUser = await _platformUserRepository.GetSettings(user.UserId);

                if (platformUser != null)
                {
                    collaborators.Add(new OrganizationUserModel
                    {
                        Id = user.UserId,
                        OrganizationId = model.OrganizationId,
                        Role = user.Role,
                        Email = platformUser.Email,
                        FirstName = platformUser.FirstName,
                        LastName = platformUser.LastName,
                        FullName = platformUser.FirstName + " " + platformUser.LastName,
                        CreatedAt = platformUser.CreatedAt
                    });
                }
            }

            if (model.OrderBy != null && model.OrderBy.ToLower() == "desc")
            {
                if (model.OrderField != null && model.OrderField.ToLower() == "role")
                    collaborators = collaborators.OrderByDescending(x => x.Role).ToList();
                else
                    collaborators = collaborators.OrderByDescending(x => x.FullName).ToList();
            }
            else
            {
                if (model.OrderField != null && model.OrderField.ToLower() == "role")
                    collaborators = collaborators.OrderBy(x => x.Role).ToList();
                else
                    collaborators = collaborators.OrderBy(x => x.FullName).ToList();
            }

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
        public async Task<IActionResult> Apps([FromBody] OrganizationAppModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isOrganizationAvailable = _organizationRepository.IsOrganizationAvaliable(AppUser.Id, model.OrganizationId);

            if (!isOrganizationAvailable)
                return BadRequest(ApiResponseMessages.ORGANIZATION_NOT_FOUND);

            var apps = await _appDraftRepository.GetByOrganizationId(AppUser.Id, model.OrganizationId);


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
                Icon = organization.Icon,
                OwnerId = organization.OwnerId,
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

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody] OrganizationModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var organization = new Organization
            {
                Name = model.Name,
                Icon = model.Icon,
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

            return Ok(result);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]OrganizationModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var organization = await _organizationRepository.Get(AppUser.Id, id);

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

                    var templates = await _platformRepository.GetAppTemplate(AppUser.AppId, AppTemplateType.Email, AppUser.Culture.Substring(0, 2), "organization_invitation");

                    foreach (var template in templates)
                    {
                        template.Content = template.Content.Replace("{:FirstName}", model.FirstName);
                        template.Content = template.Content.Replace("{:LastName}", model.LastName);
                        template.Content = template.Content.Replace("{:Organization}", organization.Name);
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

                    await _consoleUserRepository.Create(consoleUser);
                }
            }

            return Ok();
        }

        [Route("delete_user"), HttpDelete]
        public async Task<IActionResult> DeleteUser([FromBody]OrganizationUser model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var organization = await _organizationRepository.Get(AppUser.Id, model.OrganizationId);

            if (organization == null)
                return NotFound(ApiResponseMessages.ORGANIZATION_NOT_FOUND);

            if (!await _permissionHelper.CheckUserRole(AppUser.Id, model.OrganizationId, OrganizationRole.Administrator))
                return Forbid(ApiResponseMessages.PERMISSION);

            if (AppUser.Id == organization.OwnerId)
                return BadRequest(ApiResponseMessages.OWN_ORGANIZATION);

            await _organizationUserRepository.Delete(model);

            return Ok();
        }

        [Route("update_user"), HttpDelete]
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

            await _organizationUserRepository.Update(user);

            return Ok();
        }
    }
}
