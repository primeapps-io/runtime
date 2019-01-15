using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Console.Helpers;
using PrimeApps.Console.Models;
using PrimeApps.Model.Common;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PrimeApps.Console.Controllers
{
    [Route("api/process")]
    public class ProcessController : DraftBaseController
    {
        private IConfiguration _configuration;
        private IProcessRepository _processRepository;
        private IModuleRepository _moduleRepository;
        private IViewRepository _viewRepository;
        private IPicklistRepository _picklistRepository;
        private IProcessHelper _processHelper;
        private Warehouse _warehouse;

        public ProcessController(IConfiguration configuration, IProcessRepository processRepository,
            IModuleRepository moduleRepository, IPicklistRepository picklistRepository,
            IViewRepository viewRepository, IProcessHelper processHelper,
            Warehouse warehouse)
        {
            _configuration = configuration;
            _processRepository = processRepository;
            _moduleRepository = moduleRepository;
            _picklistRepository = picklistRepository;
            _viewRepository = viewRepository;
            _warehouse = warehouse;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_processRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_moduleRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_picklistRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_viewRepository, PreviewMode, TenantId, AppId);

            base.OnActionExecuting(context);

        }

        [Route("find"), HttpPost]
        public async Task<IActionResult> Find([FromBody] PaginationModel paginationModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var processes = await _processRepository.Find(paginationModel);

            return Ok(processes);
        }

        [Route("count"), HttpGet]
        public async Task<IActionResult> Count()
        {
            var count = await _processRepository.Count();

            return Ok(count);
        }

        [Route("get/{id:int}"), HttpGet]
        public async Task<IActionResult> Get(int id)
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

            var processEntity = await _processHelper.CreateEntity(process, AppUser.TenantLanguage, _moduleRepository, _picklistRepository, _warehouse, AppUser);
            var result = await _processRepository.Create(processEntity);

            if (result < 1)
                throw new ApplicationException(HttpStatusCode.InternalServerError.ToString());
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
        public async Task<dynamic> Update(int id, [FromBody]ProcessBindingModel process)
        {
            var processEntity = await _processRepository.GetById(id);

            if (processEntity == null)
                return NotFound();

            var currentFilterIds = processEntity.Filters.Select(x => x.Id).ToList();
            var currentApproverIds = processEntity.Approvers.Select(x => x.Id).ToList();
            await _processHelper.UpdateEntity(process, processEntity, AppUser.TenantLanguage);
            await _processRepository.Update(processEntity, currentFilterIds, currentApproverIds);

            await _processRepository.DeleteLogs(id);

            return Ok(processEntity);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
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
