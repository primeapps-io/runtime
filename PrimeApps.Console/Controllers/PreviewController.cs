using PrimeApps.Console.Models;
using PrimeApps.Console.Helpers;
using PrimeApps.Model.Constants;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Http.Extensions;
using System.Linq;
using PrimeApps.Model.Common;
using PrimeApps.Model.Common.Organization;
using PrimeApps.Console.Constants;
using PrimeApps.Model.Common.Component;

namespace PrimeApps.Console.Controllers
{
    [Route("api/preview")]
    public class PreviewController : DraftBaseController
    {
        private IConfiguration _configuration;
        private Warehouse _warehouse;

        public PreviewController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);

            base.OnActionExecuting(context);
        }

        [Route("key"), HttpGet]
        public IActionResult Key()
        {
            if (AppId == null)
                return BadRequest("AppId can not be null");

            var token = CryptoHelper.Encrypt("app_id=" + AppId, "222EF106646458CD59995D4378B55DF2");

            return Ok(token);
        }
    }
}
