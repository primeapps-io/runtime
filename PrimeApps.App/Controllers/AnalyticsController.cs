using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.App.ActionFilters;
using PrimeApps.App.Extensions;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.App.Jobs;
using PrimeApps.Model.Common.Document;
using PrimeApps.Model.Common.Warehouse;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace PrimeApps.App.Controllers
{
    [Route("api/analytics"), Authorize]
    public class AnalyticsController : ApiBaseController
    {
        private Warehouse _warehouseHelper;
        private IAnalyticRepository _analyticRepository;
        private IUserRepository _userRepository;
        private IPlatformWarehouseRepository _warehousePlatformRepository;
        private IConfiguration _configuration;
        private IDocumentHelper _documentHelper;
        private IPowerBiHelper _powerBiHelper;
        private IAnalyticsHelper _analyticsHelper;

        public AnalyticsController(Warehouse warehouseHelper, IAnalyticRepository analyticRepository, IUserRepository userRepository, IPlatformWarehouseRepository warehousePlatformRepository, IConfiguration configuration, IDocumentHelper documentHelper, IPowerBiHelper powerBiHelper,IAnalyticsHelper analyticsHelper)
        {
            _warehouseHelper = warehouseHelper;
            _analyticRepository = analyticRepository;
            _userRepository = userRepository;
            _warehousePlatformRepository = warehousePlatformRepository;
            _documentHelper = documentHelper;
            _powerBiHelper = powerBiHelper;
            _analyticsHelper = analyticsHelper;
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_analyticRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_userRepository, PreviewMode, TenantId, AppId);

            base.OnActionExecuting(context);
        }

        [Route("create_warehouse"), HttpPost]
        public async Task<IActionResult> CreateWarehouse([FromBody]WarehouseCreateRequest request)
        {
            if (!AppUser.Email.EndsWith("@ofisim.com"))
                return StatusCode(HttpStatusCode.Status403Forbidden);

            var isPasswordComplex = Utils.IsComplexPassword(request.DatabasePassword);

            if (!isPasswordComplex)
                ModelState.AddModelError("password", "Password validation failed. The password does not meet policy requirements because it is not complex enough.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Model.Entities.Platform.PlatformWarehouse warehouse = await _warehousePlatformRepository.GetByTenantId(request.TenantId);

            if (warehouse != null)
                return BadRequest("Already exists");

            var powerBiWorkspace = await _powerBiHelper.CreateWorkspace();
            request.PowerBiWorkspaceId = powerBiWorkspace.WorkspaceId;

            var jobId = BackgroundJob.Enqueue(() => _warehouseHelper.Create(request, AppUser));

            _analyticRepository.TenantId = request.TenantId;
            var currentAnalytics = await _analyticRepository.GetAll();

            foreach (var currentAnalytic in currentAnalytics)
            {
                await _analyticRepository.DeleteSoft(currentAnalytic);
            }

            return Ok("Started. JobId: " + jobId);
        }

        [Route("get_warehouse_info"), HttpGet]
        public async Task<IActionResult> GetWarehouseInfo()
        {
            var warehouseInfo = await _analyticsHelper.GetWarehouse(AppUser.TenantId, _configuration);

            if (warehouseInfo == null)
                return NotFound();

            return Ok(warehouseInfo);
        }

        [Route("change_warehouse_password"), HttpPut]
        public async Task<IActionResult> ChangeWarehousePassword([FromBody]WarehousePasswordRequest request)
        {
            var isPasswordComplex = Utils.IsComplexPassword(request.DatabasePassword);

            if (!isPasswordComplex)
                ModelState.AddModelError("password", "Password validation failed. The password does not meet policy requirements because it is not complex enough.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var warehouse = await _warehousePlatformRepository.GetByTenantId(AppUser.TenantId);

            if (warehouse == null)
                return NotFound();

            _warehouseHelper.ChangePassword(request, warehouse);

            return Ok();
        }

        [Route("get/{id:int}"), HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var analytic = await _analyticRepository.GetById(id);

            if (analytic == null)
                return NotFound();

            return Ok(analytic);
        }

        [Route("get_all"), HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var analytics = await _analyticRepository.GetAll();

            return Ok(analytics);
        }

        [Route("get_reports"), HttpGet]
        public async Task<IActionResult> GetReports()
        {
            var analytics = await _analyticRepository.GetReports(AppUser.HasAdminProfile);
            var reports = await _powerBiHelper.GetReports(AppUser, analytics);

            return Ok(reports);
        }

        [Route("save_pbix"), HttpPost]
        public async Task<IActionResult> SavePbix()
        {
            var stream = await Request.ReadAsStreamAsync();
            DocumentUploadResult result;
            var isUploaded = _documentHelper.Upload(stream, out result);

            if (!isUploaded && result == null)
                return NotFound();

            if (!isUploaded)
                return BadRequest();

            var pibxUrl = _documentHelper.Save(result, "analytics-" + AppUser.TenantId);

            return Ok(pibxUrl);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]AnalyticBindingModel analytic)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var analyticEntity = await _analyticsHelper.CreateEntity(analytic, _userRepository);
            var result = await _analyticRepository.Create(analyticEntity);

            if (result < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            var powerBiReportName = analyticEntity.Id.ToString();
            var import = await _powerBiHelper.ImportPbix(analytic.PbixUrl, powerBiReportName, AppUser);

            if (string.IsNullOrWhiteSpace(import.Id))
            {
                await _analyticRepository.DeleteSoft(analyticEntity);
                throw new Exception("Pbix import not succeeded");
            }

            // Wait 5 second for PowerBI Embedded import pbix completed
            await Task.Delay(5000);

            var report = await _powerBiHelper.GetReportByName(AppUser, powerBiReportName);

            if (report == null)
            {
                await _analyticRepository.DeleteSoft(analyticEntity);
                throw new Exception("Pbix import not succeeded");
            }

            analyticEntity.PowerBiReportId = report.Id;

            await _analyticRepository.Update(analyticEntity);
            BackgroundJob.Enqueue(() => _powerBiHelper.UpdateConnectionString(analyticEntity.Id, AppUser));

            var uri = new Uri(Request.GetDisplayUrl());
            return Created(uri.Scheme + "://" + uri.Authority + "/analytics/get_reports", analyticEntity);
            //return Created(Request.Scheme + "://" + Request.Host + "/analytics/get_reports", analyticEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]AnalyticBindingModel analytic)
        {
            var analyticEntity = await _analyticRepository.GetById(id);

            if (analyticEntity == null)
                return NotFound();

            if (analyticEntity.PbixUrl != analytic.PbixUrl)
            {
                await _powerBiHelper.DeleteReport(AppUser, analyticEntity.Id);

                var powerBiReportName = analyticEntity.Id.ToString();
                var import = await _powerBiHelper.ImportPbix(analytic.PbixUrl, powerBiReportName, AppUser);

                if (string.IsNullOrWhiteSpace(import.Id))
                    throw new Exception("Pbix import not succeeded");

                // Wait 5 second for PowerBI Embedded import pbix completed
                await Task.Delay(5000);

                var report = await _powerBiHelper.GetReportByName(AppUser, powerBiReportName);

                if (report == null)
                    throw new Exception("Pbix import not succeeded");

                analyticEntity.PowerBiReportId = report.Id;

                BackgroundJob.Enqueue(() => _powerBiHelper.UpdateConnectionString(analyticEntity.Id, AppUser));
            }

            if (analyticEntity.Shares != null && analyticEntity.Shares.Count > 0)
            {
                var currentShares = analyticEntity.Shares.ToList();

                foreach (var shared in currentShares)
                {
                    await _analyticRepository.DeleteAnalyticShare(shared, shared.TenantUser);
                }
            }

            await _analyticsHelper.UpdateEntity(analytic, analyticEntity, _userRepository);
            await _analyticRepository.Update(analyticEntity);

            return Ok(analyticEntity);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var analyticEntity = await _analyticRepository.GetById(id);

            if (analyticEntity == null)
                return NotFound();

            await _analyticRepository.DeleteSoft(analyticEntity);

            return Ok();
        }
    }
}