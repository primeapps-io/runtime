using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.App.Services;
using PrimeApps.Model.Context;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Controllers
{
    [Route("api/account")]
    public class AccountController : Controller
    {
        public IBackgroundTaskQueue Queue { get; }
        private IApplicationRepository _applicationRepository;
        private IPlatformRepository _platformRepository;
        private IPlatformUserRepository _platformUserRepository;
        private IDocumentHelper _documentHelper;
        private IConfiguration _configuration;
        private IServiceScopeFactory _serviceScopeFactory;
        private PlatformDBContext _dBContext;


        public AccountController(IApplicationRepository applicationRepository, IConfiguration configuration,
            IPlatformUserRepository platformUserRepository, IPlatformRepository platformRepository,
            IBackgroundTaskQueue queue, IDocumentHelper documentHelper, IServiceScopeFactory serviceScopeFactory, PlatformDBContext dBContext)
        {
            _platformUserRepository = platformUserRepository;
            _platformRepository = platformRepository;
            _documentHelper = documentHelper;
            _configuration = configuration;
            _applicationRepository = applicationRepository;
            _serviceScopeFactory = serviceScopeFactory;
            _dBContext = dBContext;
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("user_created")]
        public async Task<IActionResult> UserCreated([FromBody] JObject request)
        {
            if (request["email"].IsNullOrEmpty() || request["app_id"].IsNullOrEmpty())
                return BadRequest();

            var applicationInfo = await _applicationRepository.Get(int.Parse(request["app_id"].ToString()));
            var guidId = (Guid)request["guid_id"];
            var appId = (int)request["app_id"];
            var language = request["tenant_language"].ToString();

            await _documentHelper.UploadSampleDocuments(guidId, appId, language);

            //Queue.QueueBackgroundWorkItem(token =>
            //    _documentHelper.UploadSampleDocuments(guidId, appId, language));

            if (!string.IsNullOrEmpty(request["code"].ToString()) &&
                (!bool.Parse(request["user_exist"].ToString()) || !bool.Parse(request["email_confirmed"].ToString())))
            {
                var url = Request.Scheme + "://" + applicationInfo.Setting.AuthDomain +
                          "/account/confirmemail?email={0}&code={1}&returnUrl={2}";

                var templates = await _platformRepository.GetAppTemplate(int.Parse(request["app_id"].ToString()),
                    AppTemplateType.Email, request["culture"].ToString().Substring(0, 2), "email_confirm");

                var appSettings = _dBContext.AppSettings.Where(x => x.AppId == appId).SingleOrDefault();

                foreach (var template in templates)
                {
                    var content = template.Content;

                    content = content.Replace("{:AppUrl}", appSettings.AppDomain);
                    content = content.Replace("{:FirstName}", request["first_name"].ToString());
                    content = content.Replace("{:LastName}", request["last_name"].ToString());
                    content = content.Replace("{:Email}", request["email"].ToString());
                    content = content.Replace("{:Url}",
                        string.Format(url, request["email"].ToString(),
                            WebUtility.UrlEncode(request["code"].ToString()),
                            HttpUtility.UrlEncode(request["return_url"].ToString())));

                    Email notification = new Email(template.Subject, content, _configuration, _serviceScopeFactory);

                    var req = JsonConvert.DeserializeObject<JObject>(template.Settings);

                    if (req != null)
                    {
                        var senderEmail = (string)req["MailSenderEmail"] ?? applicationInfo.Setting.MailSenderEmail;
                        var senderName = (string)req["MailSenderName"] ?? applicationInfo.Setting.MailSenderName;
                        notification.AddRecipient(request["email"].ToString());
                        notification.AddToQueue(senderEmail, senderName);
                    }
                }
            }

            return Ok();
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("send_password_reset")]
        public async Task<IActionResult> SendPasswordReset([FromBody] JObject request)
        {
            if (request["email"].IsNullOrEmpty() || request["code"].IsNullOrEmpty())
                return BadRequest();

            var applicationInfo = await _applicationRepository.Get(int.Parse(request["app_id"].ToString()));

            var url = Request.Scheme + "://" + applicationInfo.Setting.AuthDomain +
                      "/Account/ResetPassword?code={0}&guid={1}&returnUrl={2}";
            var user = await _platformUserRepository.Get(request["email"].ToString());

            var templates = await _platformRepository.GetAppTemplate(int.Parse(request["app_id"].ToString()),
                AppTemplateType.Email, request["culture"].ToString().Substring(0, 2), "password_reset");

            var appId = int.Parse(request["app_id"].ToString());

            var appSettings = _dBContext.AppSettings.Where(x => x.AppId == appId).SingleOrDefault();

            foreach (var template in templates)
            {
                var content = template.Content;

                content = content.Replace("{:AppUrl}", appSettings.AppDomain);
                content = content.Replace("{:PasswordResetUrl}",
                    string.Format(url, HttpUtility.UrlEncode(request["code"].ToString()),
                        new Guid(request["guid_id"].ToString()),
                        HttpUtility.UrlEncode(request["return_url"].ToString())));
                content = content.Replace("{:FullName}", user.FirstName + " " + user.LastName);
                Email notification = new Email(template.Subject, content, _configuration, _serviceScopeFactory);

                var req = JsonConvert.DeserializeObject<JObject>(template.Settings);
                if (req != null)
                {
                    var senderEmail = (string)req["MailSenderEmail"] ?? applicationInfo.Setting.MailSenderEmail;
                    var senderName = (string)req["MailSenderName"] ?? applicationInfo.Setting.MailSenderName;

                    notification.AddRecipient(request["email"].ToString());
                    notification.AddToQueue(senderEmail, senderName);
                }
            }

            return Ok();
        }

        [Route("change_password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordBindingModel changePasswordBindingModel)
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
                var response =
                    await httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                    return BadRequest(response.StatusCode);
            }

            return Ok();
        }
    }
}