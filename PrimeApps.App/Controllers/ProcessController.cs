using System;
using PrimeApps.App.ActionFilters;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Repositories.Interfaces;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PrimeApps.App.Controllers
{
    [Route("api/process"), Authorize/*, SnakeCase*/]
	public class ProcessController : BaseController
    {
        private IProcessRepository _processRepository;
        private IModuleRepository _moduleRepository;
        private IViewRepository _viewRepository;
        private IPicklistRepository _picklistRepository;

        public ProcessController(IProcessRepository processRepository, IModuleRepository moduleRepository, IPicklistRepository picklistRepository, IViewRepository viewRepository)
        {
            _processRepository = processRepository;
            _viewRepository = viewRepository;
            _moduleRepository = moduleRepository;
            _picklistRepository = picklistRepository;
        }

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			SetContext(context);
			SetCurrentUser(_processRepository);
			SetCurrentUser(_viewRepository);
			SetCurrentUser(_moduleRepository);
			SetCurrentUser(_picklistRepository);

			base.OnActionExecuting(context);
		}

		[Route("get/{id:int}"), HttpGet]
        public async Task<IActionResult> Get([FromQuery(Name = "id")]int id)
        {
            var processEntity = await _processRepository.GetAllById(id);

            if (processEntity == null)
                return NotFound();

            processEntity.Approvers = await _processRepository.GetUsers(processEntity);

            return Ok(processEntity);
        }

        [Route("get_all"), HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var processEntities = await _processRepository.GetAllBasic();

            return Ok(processEntities);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]ProcessBindingModel process)
        {
            if (process.Approvers == null)
                ModelState.AddModelError("request._actions", "At least one action required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var processEntity = await ProcessHelper.CreateEntity(process, AppUser.TenantLanguage, _moduleRepository, _picklistRepository);
            var result = await _processRepository.Create(processEntity);

            if (result < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            //create approvel process views
            var module = await _moduleRepository.GetById(process.ModuleId);

            var hasProcessView = _viewRepository.DbContext.ViewFilters
                .Include(x => x.View)
                .Any(x => x.Field.Contains("process.process_requests.process_status") && !x.Deleted && x.View.ModuleId == module.Id);

            if (!hasProcessView)
            {
                var pendingViewMyRecordsEntity = ViewHelper.CreateViewPendingProcessRecords(module);
                var approvedViewMyRecordsEntity = ViewHelper.CreateViewApprovedProcessRecords(module);
                var rejectedViewMyRecordsEntity = ViewHelper.CreateViewRejectedProcessRecords(module);
                var pendingFromMeViewMyRecordsEntity = ViewHelper.CreateViewPendingFromMeProcessRecords(module, processEntity);

                await _viewRepository.Create(pendingViewMyRecordsEntity);
                await _viewRepository.Create(approvedViewMyRecordsEntity);
                await _viewRepository.Create(rejectedViewMyRecordsEntity);
                await _viewRepository.Create(pendingFromMeViewMyRecordsEntity);
            }

            var uri = new Uri(Request.GetDisplayUrl());
			return Created(uri.Scheme + "://" + uri.Authority + "/api/view/get/" + processEntity.Id, processEntity);
            //return Created(Request.Scheme + "://" + Request.Host + "/api/view/get/" + processEntity.Id, processEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<dynamic> Update([FromQuery(Name = "id")]int id, [FromBody]ProcessBindingModel process)
        {
            var processEntity = await _processRepository.GetById(id);

            if (processEntity == null)
                return NotFound();

            var currentFilterIds = processEntity.Filters.Select(x => x.Id).ToList();
            var currentApproverIds = processEntity.Approvers.Select(x => x.Id).ToList();
            await ProcessHelper.UpdateEntity(process, processEntity, AppUser.TenantLanguage, _moduleRepository, _picklistRepository);
            await _processRepository.Update(processEntity, currentFilterIds, currentApproverIds);

            await _processRepository.DeleteLogs(id);

            return Ok(processEntity);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete([FromQuery(Name = "id")]int id)
        {
            var processEntity = await _processRepository.GetById(id);

            if (processEntity == null)
                return NotFound();

            await _processRepository.DeleteSoft(processEntity);
            await _processRepository.DeleteLogs(id);

            return Ok();
        }
    }
}