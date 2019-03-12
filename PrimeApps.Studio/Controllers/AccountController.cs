using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Entities.Studio;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Models;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/account")]
    public class AccountController : Controller
    {
        private IApplicationRepository _applicationRepository;
        private IOrganizationRepository _organizationRepository;
        private IStudioUserRepository _studioUserRepository;
        private IGiteaHelper _giteaHelper;
        private IPlatformRepository _platformRepository;
        private IConfiguration _configuration;


        public AccountController(IApplicationRepository applicationRepository,
            IOrganizationRepository organizationRepository,
            IStudioUserRepository studioUserRepository,
            IGiteaHelper giteaHelper, IPlatformRepository platformRepository, IConfiguration configuration)
        {
            _organizationRepository = organizationRepository;
            _studioUserRepository = studioUserRepository;
            _giteaHelper = giteaHelper;
            _applicationRepository = applicationRepository;
            _platformRepository = platformRepository;
            _configuration = configuration;

        }

        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            var appInfo = await _applicationRepository.Get(Request.Host.Value);

            Response.Cookies.Delete("tenant_id");
            Response.Cookies.Delete("gitea_token");
            await HttpContext.SignOutAsync();

            return StatusCode(200, new { redirectUrl = Request.Scheme + "://" + appInfo.Setting.AuthDomain + "/Account/Logout?returnUrl=" + Request.Scheme + "://" + appInfo.Setting.AppDomain });
        }

        [Route("change_password")]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordBindingModel changePasswordBindingModel)
        {
            if (HttpContext.User.FindFirst("email") == null || string.IsNullOrEmpty(HttpContext.User.FindFirst("email").Value))
                return Unauthorized();

            changePasswordBindingModel.Email = HttpContext.User.FindFirst("email").Value;

            var appInfo = await _applicationRepository.Get(Request.Host.Value);
            using (var httpClient = new HttpClient())
            {
                var url = Request.Scheme + "://" + appInfo.Setting.AuthDomain + "/user/change_password?client=" + appInfo.Name;
                httpClient.BaseAddress = new Uri(url);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var json = JsonConvert.SerializeObject(changePasswordBindingModel);
                var response = await httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                    return BadRequest(response);
            }

            return Ok();
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody] JObject user)
        {

            if (string.IsNullOrEmpty(user["id"].ToString()))
                return BadRequest("User id is required");

            var decryptId = CryptoHelper.Decrypt((user["id"].ToString()));

            var validId = int.TryParse(decryptId, out int id);

            if (!validId)
                return BadRequest("Id is not valid");

            var studioUser = new StudioUser
            {
                Id = id,
                UserOrganizations = new List<OrganizationUser>()
            };

            var result = await _studioUserRepository.Create(studioUser);
            Organization organization = null;

            if (result >= 1)
            {
                var userEmail = (string)user["email"];
                var query = userEmail.Replace("@", "").Split(".");
                Array.Resize(ref query, query.Length - 1);
                var orgName = string.Join("", query);

                organization = new Organization
                {
                    Name = orgName,
                    Label = user["first_name"] + " " + user["last_name"],
                    OwnerId = studioUser.Id,
                    CreatedById = studioUser.Id,
                    Default = true,
                    OrganizationUsers = new List<OrganizationUser>()
                };

                organization.OrganizationUsers.Add(new OrganizationUser
                {
                    UserId = studioUser.Id,
                    Role = OrganizationRole.Administrator,
                    CreatedById = studioUser.Id,
                    CreatedAt = DateTime.Now
                });

                await _organizationRepository.Create(organization);

                await _giteaHelper.CreateUser((string)user["email"], (string)user["password"], (string)user["first_name"], (string)user["last_name"], orgName);
            }

            var applicationInfo = await _applicationRepository.Get(int.Parse(user["app_id"].ToString()));

            if (!string.IsNullOrEmpty((string)user["code"]) &&
                (!bool.Parse((string)user["user_exist"]) || !bool.Parse((string)user["email_confirmed"])))
            {
                var url = Request.Scheme + "://" + applicationInfo.Setting.AuthDomain +
                          "/account/confirmemail?email={0}&code={1}&returnUrl={2}";

                var templates = await _platformRepository.GetAppTemplate(int.Parse(user["app_id"].ToString()),
                    AppTemplateType.Email, user["culture"].ToString().Substring(0, 2), "email_confirm");

                foreach (var template in templates)
                {
                    var content = template.Content;

                    content = content.Replace("{:FirstName}", user["first_name"].ToString());
                    content = content.Replace("{:LastName}", user["last_name"].ToString());
                    content = content.Replace("{:Email}", user["email"].ToString());
                    content = content.Replace("{:Url}",
                        string.Format(url, user["email"],
                            WebUtility.UrlEncode((string)user["code"]),
                            HttpUtility.UrlEncode((string)user["return_url"])));

                    Email notification = new Email(_configuration, null, template.Subject, content);

                    var req = JsonConvert.DeserializeObject<JObject>(template.Settings);

                    if (req != null)
                    {
                        var senderEmail = (string)req["MailSenderEmail"] ?? applicationInfo.Setting.MailSenderEmail;
                        var senderName = (string)req["MailSenderName"] ?? applicationInfo.Setting.MailSenderName;
                        notification.AddRecipient(user["email"].ToString());
                        notification.AddToQueue(senderEmail, senderName, null, null, content, template.Subject);
                    }
                }
            }

            return Ok(organization.Id);
        }

        [Route("user_available/{userId}"), HttpGet]
        public async Task<IActionResult> UserAvailable(int userId)
        {
            var user = await _studioUserRepository.Get(userId);

            if (user == null)
                return Conflict();

            return Ok();
        }
    }
}