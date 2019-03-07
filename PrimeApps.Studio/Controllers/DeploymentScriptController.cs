using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Services;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/deployment_script")]
    public class DeploymentScriptController : DraftBaseController
    {
        private IBackgroundTaskQueue Queue;
        private IConfiguration _configuration;
        private IScriptRepository _scriptRepository;

        public DeploymentScriptController(IBackgroundTaskQueue queue, IConfiguration configuration, IScriptRepository scriptRepository)
        {
            Queue = queue;
            _configuration = configuration;
            _scriptRepository = scriptRepository;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_scriptRepository, PreviewMode, AppId, TenantId);
            base.OnActionExecuting(context);
        }

    }
}