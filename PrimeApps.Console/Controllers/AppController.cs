using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common.Organization;
using PrimeApps.Model.Common.User;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Console.Controllers
{
    [Route("api/app")]
    public class AppController : ApiBaseController
    {
        private IConfiguration _configuration;
        private IPlatformUserRepository _platformUserRepository;
        private IAppDraftRepository _appDraftRepository;
        private IOrganizationRepository _organizationRepository;

        public AppController(IConfiguration configuration, IPlatformUserRepository platformUserRepository, IAppDraftRepository appDraftRepository, IOrganizationRepository organizationRepository)
        {
            _platformUserRepository = platformUserRepository;
            _appDraftRepository = appDraftRepository;
            _organizationRepository = organizationRepository;
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            //SetCurrentUser(_platformUserRepository);
            base.OnActionExecuting(context);
        }

        [Route("get_all"), HttpPost]
        public async Task<IActionResult> Organizations([FromBody] JObject request)
        {
            var filter = "";
            var page = 0;
            if (request != null)
            {
                if (!request["search"].IsNullOrEmpty())
                    filter = request["search"].ToString();

                if (request["page"].IsNullOrEmpty())
                    page = (int)request["page"];
            }
            
            var organizations = await _appDraftRepository.GetAll(AppUser.Id, filter, page);

            return Ok(organizations);
        }
    }
}
