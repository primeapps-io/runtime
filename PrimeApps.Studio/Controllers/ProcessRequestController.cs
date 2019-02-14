using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/process_request")]
    public class ProcessRequestController : DraftBaseController
    {
        private IProcessRequestRepository _processRequestRepository;
        private IModuleRepository _moduleRepository;
        private IRecordRepository _recordRepository;
        private Warehouse _warehouse;
        private IConfiguration _configuration;

        private ICalculationHelper _calculationHelper;
        private IProcessHelper _processHelper;
        private IRecordHelper _recordHelper;

        public ProcessRequestController(IConfiguration configuration, IProcessRequestRepository processRequestRepository,
            IModuleRepository moduleRepository, IRecordRepository recordRepository,
            IProcessHelper processHelper, IRecordHelper recordHelper,
            ICalculationHelper calculationHelper, Warehouse warehouse)
        {
            _configuration = configuration;
            _processRequestRepository = processRequestRepository;
            _moduleRepository = moduleRepository;
            _recordRepository = recordRepository;
            _warehouse = warehouse;

            _calculationHelper = calculationHelper;
            _processHelper = processHelper;
            _recordHelper = recordHelper;

        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_processRequestRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_recordRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_moduleRepository, PreviewMode, TenantId, AppId);

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
         

    }
}