using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;

using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Enums;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using static PrimeApps.App.Helpers.ProcessHelper;
using Newtonsoft.Json.Linq;

namespace PrimeApps.App.Controllers
{
	[Route("api/process_request"), Authorize]
	public class ProcessRequestController : ApiBaseController
	{
		private IProcessRequestRepository _processRequestRepository;
		private IModuleRepository _moduleRepository;
		private IRecordRepository _recordRepository;
		private Warehouse _warehouse;
		private IConfiguration _configuration;

		private ICalculationHelper _calculationHelper;
		private IProcessHelper _processHelper;
		private IRecordHelper _recordHelper;

		public ProcessRequestController(IProcessRequestRepository processRequestRepository, IModuleRepository moduleRepository, IRecordRepository recordRepository, ICalculationHelper calculationHelper, IProcessHelper processHelper, IRecordHelper recordHelper, Warehouse warehouse, IConfiguration configuration)
		{
			_processRequestRepository = processRequestRepository;
			_moduleRepository = moduleRepository;
			_recordRepository = recordRepository;
			_warehouse = warehouse;

			_calculationHelper = calculationHelper;
			_processHelper = processHelper;
			_recordHelper = recordHelper;
			_configuration = configuration;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			SetContext(context);
			SetCurrentUser(_processRequestRepository, PreviewMode, TenantId, AppId);
			SetCurrentUser(_moduleRepository, PreviewMode, TenantId, AppId);
			SetCurrentUser(_recordRepository, PreviewMode, TenantId, AppId);

			base.OnActionExecuting(context);
		}

		[Route("get_requests/{id:int}"), HttpGet]
		public async Task<IActionResult> GetAll(int id)
		{
			var processRequestEntities = await _processRequestRepository.GetByProcessId(id);

			return Ok(processRequestEntities);
		}

		[Route("approve_multiple_request"), HttpPut]
		public async Task<IActionResult> ApproveMultipleRequest([FromBody]JObject request)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			if (!request["record_ids"].HasValues && request["module_name"].IsNullOrEmpty())
				return BadRequest();

			if (request["record_ids"].Type != JTokenType.Array)
				return BadRequest("Plesae send record_ids array.");

			for (int i = 0; i < ((JArray)request["record_ids"]).Count; i++)
			{
				var requestEntity = await _processRequestRepository.GetByRecordIdWithOutOperationType((int)request["record_ids"][i], request["module_name"].ToString());

				if (requestEntity == null || requestEntity.Active == false)
					continue;

				await _processHelper.ApproveRequest(requestEntity, AppUser, _warehouse, _recordHelper.BeforeCreateUpdate, _recordHelper.AfterUpdate, _recordHelper.GetAllFieldsForFindRequest);
				await _processRequestRepository.Update(requestEntity);

				await _processHelper.AfterCreateProcess(requestEntity, AppUser, _warehouse, _recordHelper.BeforeCreateUpdate, _recordHelper.UpdateStageHistory, _recordHelper.AfterUpdate, _recordHelper.AfterCreate, _recordHelper.GetAllFieldsForFindRequest);

				if (request["module_name"].ToString() == "izinler")
				{
					//süreç bitiminde kalan izin hakkı tekrar kontrol ettiriliyor. 
					var moduleEntity = await _moduleRepository.GetByName(request["module_name"].ToString());
					await _calculationHelper.Calculate((int)request["record_ids"][i], moduleEntity, AppUser, _warehouse, OperationType.update, _recordHelper.BeforeCreateUpdate, _recordHelper.AfterUpdate, _recordHelper.GetAllFieldsForFindRequest);
				}
			}

			return Ok();
		}

		[Route("approve"), HttpPut]
		public async Task<IActionResult> ApproveRequest([FromBody]ProcessRequestModel request)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var requestEntity = await _processRequestRepository.GetByRecordId(request.RecordId, request.ModuleName, request.OperationType);

			if (requestEntity == null || requestEntity.Active == false)
				return NotFound();

			await _processHelper.ApproveRequest(requestEntity, AppUser, _warehouse, _recordHelper.BeforeCreateUpdate, _recordHelper.AfterUpdate, _recordHelper.GetAllFieldsForFindRequest);
			await _processRequestRepository.Update(requestEntity);

			await _processHelper.AfterCreateProcess(requestEntity, AppUser, _warehouse, _recordHelper.BeforeCreateUpdate, _recordHelper.UpdateStageHistory, _recordHelper.AfterUpdate, _recordHelper.AfterCreate, _recordHelper.GetAllFieldsForFindRequest);

			if (request.ModuleName == "izinler")
			{
				//süreç bitiminde kalan izin hakkı tekrar kontrol ettiriliyor. 
				var moduleEntity = await _moduleRepository.GetByName(request.ModuleName);
				await _calculationHelper.Calculate(request.RecordId, moduleEntity, AppUser, _warehouse, OperationType.update, _recordHelper.BeforeCreateUpdate, _recordHelper.AfterUpdate, _recordHelper.GetAllFieldsForFindRequest);
			}

			return Ok(requestEntity);
		}

		[Route("reject"), HttpPut]
		public async Task<IActionResult> RejectRequest([FromBody]ProcessRequestRejectModel request)
		{

			var requestEntity = await _processRequestRepository.GetByRecordId(request.RecordId, request.ModuleName, request.OperationType);

			if (requestEntity == null || requestEntity.Active == false)
				return NotFound();

			await _processHelper.RejectRequest(requestEntity, request.Message, AppUser, _warehouse);
			await _processRequestRepository.Update(requestEntity);

			await _processHelper.AfterCreateProcess(requestEntity, AppUser, _warehouse, _recordHelper.BeforeCreateUpdate, _recordHelper.UpdateStageHistory, _recordHelper.AfterUpdate, _recordHelper.AfterCreate, _recordHelper.GetAllFieldsForFindRequest);

			if (request.ModuleName == "izinler")
			{
				//süreç bitiminde kalan izin hakkı tekrar kontrol ettiriliyor. 
				var moduleEntity = await _moduleRepository.GetByName(request.ModuleName);
				await _calculationHelper.Calculate(request.RecordId, moduleEntity, AppUser, _warehouse, OperationType.update, _recordHelper.BeforeCreateUpdate, _recordHelper.AfterUpdate, _recordHelper.GetAllFieldsForFindRequest);
			}

			return Ok(requestEntity);
		}

		[Route("delete"), HttpPut]
		public async Task<IActionResult> DeleteRequest([FromBody]ProcessRequestDeleteModel request)
		{
			var moduleEntity = await _moduleRepository.GetById(request.ModuleId);
			var record = await _recordRepository.GetById(moduleEntity, request.RecordId, !AppUser.HasAdminProfile);
			await _processHelper.Run(OperationType.delete, record, moduleEntity, AppUser, _warehouse, Model.Enums.ProcessTriggerTime.Instant, _recordHelper.BeforeCreateUpdate, _recordHelper.GetAllFieldsForFindRequest, _recordHelper.UpdateStageHistory, _recordHelper.AfterUpdate, _recordHelper.AfterCreate);

			return Ok();
		}

		[Route("send_approval"), HttpPut]
		public async Task<IActionResult> ReApprovalRequest([FromBody]ProcessRequestModel request)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var requestEntity = await _processRequestRepository.GetByRecordId(request.RecordId, request.ModuleName, request.OperationType);

			if (requestEntity == null || requestEntity.Active == false)
				return NotFound();

			await _processHelper.SendToApprovalAgain(requestEntity, AppUser, _warehouse, _recordHelper.BeforeCreateUpdate, _recordHelper.AfterUpdate, _recordHelper.GetAllFieldsForFindRequest);
			await _processRequestRepository.Update(requestEntity);

			await _processHelper.AfterCreateProcess(requestEntity, AppUser, _warehouse, _recordHelper.BeforeCreateUpdate, _recordHelper.UpdateStageHistory, _recordHelper.AfterUpdate, _recordHelper.AfterCreate, _recordHelper.GetAllFieldsForFindRequest);

			return Ok(requestEntity);
		}

		[Route("send_approval_manuel"), HttpPost]
		public async Task<IActionResult> ManuelApprovalRequest([FromBody]ProcessRequestManuelModel request)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var moduleEntity = await _moduleRepository.GetById(request.ModuleId);
			var record = await _recordRepository.GetById(moduleEntity, request.RecordId, !AppUser.HasAdminProfile);
			try
			{
				await _processHelper.Run(OperationType.insert, record, moduleEntity, AppUser, _warehouse, ProcessTriggerTime.Manuel, _recordHelper.BeforeCreateUpdate, _recordHelper.GetAllFieldsForFindRequest, _recordHelper.UpdateStageHistory, _recordHelper.AfterUpdate, _recordHelper.AfterCreate);

				//süreç bitiminde kalan izin hakkı tekrar kontrol ettiriliyor. 
				if (moduleEntity.Name == "izinler")
					await _calculationHelper.Calculate((int)record["id"], moduleEntity, AppUser, _warehouse, OperationType.update, _recordHelper.BeforeCreateUpdate, _recordHelper.AfterUpdate, _recordHelper.GetAllFieldsForFindRequest);
			}
            catch (ProcessFilterNotMatchException ex)
            {
                if (ex.Message == "ProcessApproverNotFoundException")
                {
                    ModelState.AddModelError("ApproverNotFound", "Approver Not Found");
                    return BadRequest(ModelState);
                }
                else
                {
                    ModelState.AddModelError("FiltersNotMatch", "Filters don't matched");
                    return BadRequest(ModelState);
                }
            }

            return Ok();
		}
	}
}