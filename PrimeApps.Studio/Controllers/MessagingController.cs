using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common;
using PrimeApps.Model.Common.Messaging;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Models;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/messaging"), Authorize]
    public class MessagingController : DraftBaseController
    {
        private IViewRepository _viewRepository;
        private IUserRepository _userRepository;
        private IDashboardRepository _dashboardRepository;
        private IRecordHelper _recordHelper;
        private IConfiguration _configuration;


        public MessagingController(IViewRepository viewRepository, IUserRepository userRepository, IDashboardRepository dashboardRepository, IRecordHelper recordHelper, IConfiguration configuration)
        {
            _viewRepository = viewRepository;
            _userRepository = userRepository;
            _dashboardRepository = dashboardRepository;
            _recordHelper = recordHelper;
            _configuration = configuration;

        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_userRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_viewRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_dashboardRepository, PreviewMode, TenantId, AppId);

            base.OnActionExecuting(context);
        }

        [Route("send_external_email")]
        public IActionResult SendExternalEmail([FromBody]ExternalEmail emailRequest)
        {
            if (emailRequest.Subject != null && emailRequest.TemplateWithBody != null && emailRequest.ToAddresses.Length > 0)
            {
                if (emailRequest.Cc == null)
                    emailRequest.Cc = "";

                if (emailRequest.Bcc == null)
                    emailRequest.Bcc = "";
                var externalEmail = new Email(_configuration, null, emailRequest.Subject, emailRequest.TemplateWithBody);
                foreach (var emailRecipient in emailRequest.ToAddresses)
                {
                    externalEmail.AddRecipient(emailRecipient);
                }

                externalEmail.AddToQueue(cc: emailRequest.Cc, bcc: emailRequest.Bcc, fromEmail: emailRequest.FromEmail, Name: emailRequest.FromName, Subject: emailRequest.Subject, Template: emailRequest.TemplateWithBody);

                return Ok(emailRequest.ToAddresses.Count());
            }

            return BadRequest();
        }
    }
}
