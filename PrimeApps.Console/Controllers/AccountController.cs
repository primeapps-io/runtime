using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Console.Helpers;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace PrimeApps.Console.Controllers
{
    [Route("api/account")]
    public class AccountController : Controller
    {
        private IServiceScopeFactory _serviceScopeFactory;
        private IRecordRepository _recordRepository;
        private IApplicationRepository _applicationRepository;
        private IPlatformRepository _platformRepository;
        private IPlatformUserRepository _platformUserRepository;
        private ITenantRepository _tenantRepository;
        private IProfileRepository _profileRepository;
        private IUserRepository _userRepository;
        private IRoleRepository _roleRepository;
        private IConfiguration _configuration;
        
        public AccountController(IServiceScopeFactory serviceScopeFactory, IApplicationRepository applicationRepository, IRecordRepository recordRepository, IPlatformUserRepository platformUserRepository, IPlatformRepository platformRepository, IRoleRepository roleRepository, IProfileRepository profileRepository, IUserRepository userRepository, ITenantRepository tenantRepository, IConfiguration configuration)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _applicationRepository = applicationRepository;
            _recordRepository = recordRepository;
            _platformUserRepository = platformUserRepository;
            _tenantRepository = tenantRepository;
            _platformRepository = platformRepository;
            _profileRepository = profileRepository;
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _configuration = configuration;
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

            var templates = await _platformRepository.GetAppTemplate(int.Parse(request["app_id"].ToString()), AppTemplateType.Email, request["culture"].ToString().Substring(0, 2), "password_reset");
            foreach (var template in templates)
            {
                var content = template.Content;

                content = content.Replace("{:PasswordResetUrl}", string.Format(url, HttpUtility.UrlEncode(request["code"].ToString()), new Guid(request["guid_id"].ToString()), HttpUtility.UrlEncode(request["return_url"].ToString())));
                content = content.Replace("{:FullName}", user.FirstName + " " + user.LastName);

                var req = JsonConvert.DeserializeObject<JObject>(template.Settings);

                var myMessage = new MailMessage()
                {
                    From = new MailAddress((string)req["MailSenderEmail"] ?? applicationInfo.Setting.MailSenderEmail, (string)req["MailSenderName"] ?? applicationInfo.Setting.MailSenderName),
                    Subject = template.Subject,
                    Body = content,
                    IsBodyHtml = true
                };

                myMessage.To.Add(request["email"].ToString());

                Email email = new Email(_configuration, _serviceScopeFactory);
                email.TransmitMail(myMessage);
            }

            return Ok();
        }
    }
}
