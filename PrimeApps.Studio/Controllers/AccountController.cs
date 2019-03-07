using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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

        public AccountController(IApplicationRepository applicationRepository,
            IOrganizationRepository organizationRepository,
            IStudioUserRepository studioUserRepository,
            IGiteaHelper giteaHelper)
        {
            _organizationRepository = organizationRepository;
            _studioUserRepository = studioUserRepository;
            _giteaHelper = giteaHelper;
            _applicationRepository = applicationRepository;
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
        public async Task<IActionResult> Create([FromBody] StudioUserBindingModel user)
        {
            if (string.IsNullOrEmpty(user.Id))
                return BadRequest("User id is required");

            var decryptId = CryptoHelper.Decrypt(user.Id);

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
                var query = user.Email.Replace("@", "").Split(".");
                Array.Resize(ref query, query.Length - 1);
                var orgName = string.Join("", query);

                organization = new Organization
                {
                    Name = orgName,
                    Label = user.FirstName + " " + user.LastName,
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

                await _giteaHelper.CreateUser(user.Email, user.Password, user.FirstName, user.LastName, orgName);
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