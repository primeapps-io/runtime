using PrimeApps.App.ActionFilters;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;

using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Enums;
using Microsoft.AspNetCore.Mvc.Filters;
using static PrimeApps.App.Helpers.ProcessHelper;

namespace PrimeApps.App.Controllers
{
    [Route("api/process_request"), Authorize]
	public class ProcessRequestController : ApiBaseController
    {
        private IProcessRequestRepository _processRequestRepository;
        private IModuleRepository _moduleRepository;
        private IRecordRepository _recordRepository;
        private Warehouse _warehouse;

        public ProcessRequestController(IProcessRequestRepository processRequestRepository, IModuleRepository moduleRepository, IRecordRepository recordRepository, Warehouse warehouse)
        {
            _processRequestRepository = processRequestRepository;
            _moduleRepository = moduleRepository;
            _recordRepository = recordRepository;
            _warehouse = warehouse; 

        }

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			SetContext(context);
			SetCurrentUser(_processRequestRepository);
			SetCurrentUser(_moduleRepository);
			SetCurrentUser(_recordRepository);

			base.OnActionExecuting(context);
		}

        [Route("get_requests/{id:int}"), HttpGet]
        public async Task<IActionResult> GetAll(int id)
        {
            var processRequestEntities = await _processRequestRepository.GetByProcessId(id);

            return Ok(processRequestEntities);
        }

        [Route("approve_multiple_request"), HttpPut]
        public async Task<IActionResult> ApproveMultipleRequest(int[] RecordIds, string moduleName)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            for (int i = 0; i < RecordIds.Length; i++)
            {
                var requestEntity = await _processRequestRepository.GetByRecordId(RecordIds[i], moduleName, 0);
                if (requestEntity == null)
                    continue;
                await ProcessHelper.ApproveRequest(requestEntity, AppUser, _warehouse);
                await _processRequestRepository.Update(requestEntity);

                await ProcessHelper.AfterCreateProcess(requestEntity, AppUser, _warehouse);

            }

            return Ok();
        }

        [Route("approve"), HttpPut]
        public async Task<IActionResult> ApproveRequest(ProcessRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var requestEntity = await _processRequestRepository.GetByRecordId(request.RecordId, request.ModuleName, request.OperationType);

            if (requestEntity == null)
                return NotFound();

            await ProcessHelper.ApproveRequest(requestEntity, AppUser, _warehouse);
            await _processRequestRepository.Update(requestEntity);

            await ProcessHelper.AfterCreateProcess(requestEntity, AppUser, _warehouse);

            return Ok(requestEntity);
        }

        [Route("reject"), HttpPut]
        public async Task<IActionResult> RejectRequest(ProcessRequestRejectModel request)
        {

            var requestEntity = await _processRequestRepository.GetByRecordId(request.RecordId, request.ModuleName, request.OperationType);

            if (requestEntity == null)
                return NotFound();

            await ProcessHelper.RejectRequest(requestEntity, request.Message, AppUser, _warehouse);
            await _processRequestRepository.Update(requestEntity);

            await ProcessHelper.AfterCreateProcess(requestEntity, AppUser, _warehouse);

            return Ok(requestEntity);
        }

        [Route("delete"), HttpPut]
        public async Task<IActionResult> DeleteRequest(ProcessRequestDeleteModel request)
        {
            var moduleEntity = await _moduleRepository.GetById(request.ModuleId);
            var record = _recordRepository.GetById(moduleEntity, request.RecordId, !AppUser.HasAdminProfile);
            await ProcessHelper.Run(OperationType.delete, record, moduleEntity, AppUser, _warehouse, Model.Enums.ProcessTriggerTime.Instant);

            return Ok();
        }

        [Route("send_approval"), HttpPut]
        public async Task<IActionResult> ReApprovalRequest(ProcessRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var requestEntity = await _processRequestRepository.GetByRecordId(request.RecordId, request.ModuleName, request.OperationType);

            if (requestEntity == null)
                return NotFound();

            await ProcessHelper.SendToApprovalAgain(requestEntity, AppUser, _warehouse);
            await _processRequestRepository.Update(requestEntity);

            await ProcessHelper.AfterCreateProcess(requestEntity, AppUser, _warehouse);

            return Ok(requestEntity);
        }

        [Route("send_approval_manuel"), HttpPost]
        public async Task<IActionResult> ManuelApprovalRequest(ProcessRequestManuelModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var moduleEntity = await _moduleRepository.GetById(request.ModuleId);
            var record = _recordRepository.GetById(moduleEntity, request.RecordId, !AppUser.HasAdminProfile);
            try
            {
                await ProcessHelper.Run(OperationType.insert, record, moduleEntity, AppUser, _warehouse, ProcessTriggerTime.Manuel);
            }
            catch (ProcessFilterNotMatchException ex)
            {
                ModelState.AddModelError("FiltersNotMatch", "Filters don't matched");
                return BadRequest(ModelState);
            }

            return Ok();
        }
    }
}