using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Console.Constants;
using PrimeApps.Console.Helpers;
using PrimeApps.Model.Common;
using PrimeApps.Model.Common.Team;
using PrimeApps.Model.Entities.Console;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Console.Controllers
{
    [Route("api/app_profile")]
    public class AppProfileController : ApiBaseController
    {
        private IConfiguration _configuration;
        private IAppProfileRepository _appProfileRepository;
        private IPermissionHelper _permissionHelper;

        public AppProfileController(IConfiguration configuration, IAppProfileRepository appProfileRepository, IPermissionHelper permissionHelper)
        {
            _appProfileRepository = appProfileRepository;
            _configuration = configuration;
            _permissionHelper = permissionHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_appProfileRepository);

            base.OnActionExecuting(context);
        }

        [Route("get_by_app_id/{id:int}"), HttpGet]
        public async Task<IActionResult> GetByAppId(int id)
        {
            var appProfiles = await _appProfileRepository.GetByAppId(id);

            return Ok(appProfiles);
        }
    }
}
