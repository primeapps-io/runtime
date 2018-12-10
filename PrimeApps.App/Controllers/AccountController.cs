using System;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Entities.Platform;
using Newtonsoft.Json;
using PrimeApps.App.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Enums;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using PrimeApps.Model.Common.Resources;
using System.Web;
using Sentry;

namespace PrimeApps.App.Controllers
{
    [Route("api/account")]
    public class AccountController : Controller
    {
        private IRecordRepository _recordRepository;
        private IApplicationRepository _applicationRepository;
        private IPlatformRepository _platformRepository;
        private IPlatformUserRepository _platformUserRepository;
        private ITenantRepository _tenantRepository;
        private IProfileRepository _profileRepository;
        private IUserRepository _userRepository;
        private IRoleRepository _roleRepository;
        private IDocumentHelper _documentHelper;
        private IConfiguration _configuration;

        public IBackgroundTaskQueue Queue { get; }
        public AccountController(IApplicationRepository applicationRepository, IRecordRepository recordRepository, IPlatformUserRepository platformUserRepository, IPlatformRepository platformRepository, IRoleRepository roleRepository, IProfileRepository profileRepository, IUserRepository userRepository, ITenantRepository tenantRepository, IBackgroundTaskQueue queue, IRecordHelper recordHelper, Warehouse warehouse, IConfiguration configuration, IDocumentHelper documentHelper)
        {
            _applicationRepository = applicationRepository;
            _recordRepository = recordRepository;
            _platformUserRepository = platformUserRepository;
            _tenantRepository = tenantRepository;
            _platformRepository = platformRepository;
            _profileRepository = profileRepository;
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _documentHelper = documentHelper;
            _configuration = configuration;
            Queue = queue;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody]CreateBindingModels createBindingModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Model.Entities.Platform.App app = null;

            if (!string.IsNullOrEmpty(createBindingModel.AppName))
                app = await _applicationRepository.GetByName(createBindingModel.AppName);
            else if (createBindingModel.AppId != null)
                app = await _applicationRepository.Get(createBindingModel.AppId);
            else
                return BadRequest();

            using (var client = new HttpClient())
            {
                var url = Request.Scheme + "://" + app.Setting.AuthDomain + "/api/account/create";
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var dataAsString = JsonConvert.SerializeObject(createBindingModel);
                var content = new StringContent(dataAsString);
                var response = await client.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                {
                    var data = response.Content.ReadAsStringAsync().Result;
                    return BadRequest(data);
                }

            }

            //TODO Buraya webhook eklenecek. AppSetting üzerindeki TenantCreateWebhook alanı dolu kontrol edilecek doluysa bu url'e post edilecek
            //Queue.QueueBackgroundWorkItem(async token => await _platformWorkflowHelper.Run(OperationType.insert, app));
            return Ok();
        }
        //return GetErrorResult(confirmResponse);
        //return BadRequestResult();

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

        [HttpPost]
        [AllowAnonymous]
        [Route("user_created")]
        public async Task<IActionResult> UserCreated([FromBody]JObject request)
        {
            if (request["email"].IsNullOrEmpty() || request["app_id"].IsNullOrEmpty())
                return BadRequest();

            var applicationInfo = await _applicationRepository.Get(int.Parse(request["app_id"].ToString()));

            Queue.QueueBackgroundWorkItem(token => _documentHelper.UploadSampleDocuments(new Guid(request["guid_id"].ToString()), int.Parse(request["app_id"].ToString()), request["tenant_language"].ToString()));

            if (!string.IsNullOrEmpty(request["code"].ToString()) && (!bool.Parse(request["user_exist"].ToString()) || !bool.Parse(request["email_confirmed"].ToString())))
            {
                var url = Request.Scheme + "://" + applicationInfo.Setting.AuthDomain + "/account/confirmemail?email={0}&code={1}&returnUrl={2}";

                var template = _platformRepository.GetAppTemplate(int.Parse(request["app_id"].ToString()), AppTemplateType.Email, "email_confirm", request["culture"].ToString().Substring(0, 2));
                var content = template.Content;

                content = content.Replace("{:FirstName}", request["first_name"].ToString());
                content = content.Replace("{:LastName}", request["last_name"].ToString());
                content = content.Replace("{:Email}", request["email"].ToString());
                content = content.Replace("{:Url}", string.Format(url, request["email"].ToString(), WebUtility.UrlEncode(request["code"].ToString()), HttpUtility.UrlEncode(request["return_url"].ToString())));

                Email notification = new Email(template.Subject, content, _configuration);

                var senderEmail = template.MailSenderEmail ?? applicationInfo.Setting.MailSenderEmail;
                var senderName = template.MailSenderName ?? applicationInfo.Setting.MailSenderName;

                notification.AddRecipient(request["email"].ToString());
                notification.AddToQueue(senderEmail, senderName);
            }
            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("send_password_reset")]
        public async Task<IActionResult> SendPasswordReset([FromBody]JObject request)
        {
            if (request["email"].IsNullOrEmpty() || request["code"].IsNullOrEmpty())
                return BadRequest();

            var applicationInfo = await _applicationRepository.Get(int.Parse(request["app_id"].ToString()));

            var url = Request.Scheme + "://" + applicationInfo.Setting.AuthDomain + "/Account/ResetPassword?code={0}&guid={1}&returnUrl={2}";
            var user = await _platformUserRepository.Get(request["email"].ToString());

            var template = _platformRepository.GetAppTemplate(int.Parse(request["app_id"].ToString()), AppTemplateType.Email, "password_reset", request["culture"].ToString().Substring(0, 2));
            var content = template.Content;

            content = content.Replace("{:PasswordResetUrl}", string.Format(url, HttpUtility.UrlEncode(request["code"].ToString()), new Guid(request["guid_id"].ToString()), HttpUtility.UrlEncode(request["return_url"].ToString())));
            content = content.Replace("{:FullName}", user.FirstName + " " + user.LastName);

            Email notification = new Email(template.Subject, content, _configuration);

            var senderEmail = template.MailSenderEmail ?? applicationInfo.Setting.MailSenderEmail;
            var senderName = template.MailSenderName ?? applicationInfo.Setting.MailSenderName;

            notification.AddRecipient(request["email"].ToString());
            notification.AddToQueue(senderEmail, senderName);

            return Ok();
        }

        // POST account/logout
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            var appInfo = await _applicationRepository.Get(Request.Host.Value);

            Response.Cookies.Delete("tenant_id");
            await HttpContext.SignOutAsync();

            return StatusCode(200, new { redirectUrl = Request.Scheme + "://" + appInfo.Setting.AuthDomain + "/Account/Logout?returnUrl=" + Request.Scheme + "://" + appInfo.Setting.AppDomain });
        }

    }
}


