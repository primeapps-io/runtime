using System;
using System.Threading.Tasks;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Repositories.Interfaces;
using System.Net;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.App.ActionFilters;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PrimeApps.App.Controllers
{
    [Route("api/platform/workflow"), Authorize/*, SnakeCase*/]
    public class PlatformWorkflowController : BaseController
    {
        private IPlatformWorkflowRepository _workflowRepository;

        public PlatformWorkflowController(IPlatformWorkflowRepository workflowRepository)
        {
            _workflowRepository = workflowRepository;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            
            base.OnActionExecuting(context);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]PlatformWorkflowBindingModels workflow)
        {
            if (workflow.Actions == null || (workflow.Actions.WebHook == null))
                ModelState.AddModelError("request._actions", "At least one action required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var workflowEntity = PlatformWorkflowHelper.CreateEntity(workflow);
            _workflowRepository.DbContext.UserId = AppUser.Id;
            var result = await _workflowRepository.Create(workflowEntity);

            if (result < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            var uri = new Uri(Request.GetDisplayUrl());
            return Created(uri.Scheme + "://" + uri.Authority + "/api/view/get/" + workflowEntity.Id, workflowEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<dynamic> Update(int id, [FromBody]PlatformWorkflowBindingModels workflow)
        {
            var workflowEntity = await _workflowRepository.GetById(id);

            if (workflowEntity == null)
                return NotFound();
            
            PlatformWorkflowHelper.UpdateEntity(workflow, workflowEntity);
            await _workflowRepository.Update(workflowEntity);

            await _workflowRepository.DeleteLogs(id);

            return Ok(workflowEntity);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var workflowEntity = await _workflowRepository.GetById(id);

            if (workflowEntity == null)
                return NotFound();

            await _workflowRepository.DeleteSoft(workflowEntity);
            await _workflowRepository.DeleteLogs(id);

            return Ok();
        }
    }
}
