using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.App.ActionFilters;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.App.Jobs;
using PrimeApps.Model.Common.Document;
using PrimeApps.Model.Common.Warehouse;

namespace PrimeApps.App.Controllers
{
    [Route("api/analytics"), Authorize, SnakeCase]
    public class AnalyticsController : BaseController
    {
        private Warehouse _warehouseHelper;
        private IAnalyticRepository _analyticRepository;
        private IUserRepository _userRepository;
        private IPlatformWarehouseRepository _warehousePlatformRepository;
        public AnalyticsController(Warehouse warehouseHelper, IAnalyticRepository analyticRepository, IUserRepository userRepository, IPlatformWarehouseRepository warehousePlatformRepository)
        {
            _warehouseHelper = warehouseHelper;
            _analyticRepository = analyticRepository;
            _userRepository = userRepository;
            _warehousePlatformRepository = warehousePlatformRepository;
        }

        [Route("create_warehouse"), HttpPost]
        public async Task<IActionResult> CreateWarehouse(WarehouseCreateRequest request)
        {
            if (!AppUser.Email.EndsWith("@ofisim.com"))
                return StatusCode(HttpStatusCode.Forbidden);

            var isPasswordComplex = await Utils.IsComplexPassword(request.DatabasePassword);

            if (!isPasswordComplex)
                ModelState.AddModelError("password", "Password validation failed. The password does not meet policy requirements because it is not complex enough.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Model.Entities.Platform.PlatformWarehouse warehouse = await _warehousePlatformRepository.GetByTenantId(request.TenantId);

            if (warehouse != null)
                return BadRequest("Already exists");

            var powerBiWorkspace = await PowerBiHelper.CreateWorkspace();
            request.PowerBiWorkspaceId = powerBiWorkspace.WorkspaceId;

            var jobId = BackgroundJob.Enqueue(() => _warehouseHelper.Create(request, AppUser.Email));

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
            var warehouseInfo = await AnalyticsHelper.GetWarehouse(AppUser.TenantId);

            if (warehouseInfo == null)
                return NotFound();

            return Ok(warehouseInfo);
        }

        [Route("change_warehouse_password"), HttpPut]
        public async Task<IActionResult> ChangeWarehousePassword(WarehousePasswordRequest request)
        {
            var isPasswordComplex = await Utils.IsComplexPassword(request.DatabasePassword);

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
        public async Task<IHttpActionResult> Get(int id)
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
        public async Task<IHttpActionResult> GetReports()
        {
            var analytics = await _analyticRepository.GetReports();
            var reports = await PowerBiHelper.GetReports(AppUser.TenantId, analytics);

            return Ok(reports);
        }

        [Route("save_pbix"), HttpPost]
        public async Task<IActionResult> SavePbix()
        {
            var stream = await Request.Content.ReadAsStreamAsync();
            DocumentUploadResult result;
            var isUploaded = DocumentHelper.Upload(stream, out result);

            if (!isUploaded && result == null)
                return NotFound();

            if (!isUploaded)
                return BadRequest();

            var pibxUrl = DocumentHelper.Save(result, "analytics-" + AppUser.TenantId);

            return Ok(pibxUrl);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create(AnalyticBindingModel analytic)
        {
            var analyticEntity = await AnalyticsHelper.CreateEntity(analytic, _userRepository);
            var result = await _analyticRepository.Create(analyticEntity);

            if (result < 1)
                throw new HttpResponseException(HttpStatusCode.InternalServerError);

            var powerBiReportName = analyticEntity.Id.ToString();
            var import = await PowerBiHelper.ImportPbix(analytic.PbixUrl, powerBiReportName, AppUser.TenantId);

            if (string.IsNullOrWhiteSpace(import.Id))
            {
                await _analyticRepository.DeleteSoft(analyticEntity);
                throw new Exception("Pbix import not succeeded");
            }

            // Wait 5 second for PowerBI Embedded import pbix completed
            await Task.Delay(5000);

            var report = await PowerBiHelper.GetReportByName(AppUser.TenantId, powerBiReportName);

            if (report == null)
            {
                await _analyticRepository.DeleteSoft(analyticEntity);
                throw new Exception("Pbix import not succeeded");
            }

            analyticEntity.PowerBiReportId = report.Id;

            await _analyticRepository.Update(analyticEntity);
            BackgroundJob.Enqueue(() => PowerBiHelper.UpdateConnectionString(analyticEntity.Id, AppUser.TenantId));

            var uri = Request.RequestUri;
            return Created(uri.Scheme + "://" + uri.Authority + "/analytics/get_reports", analyticEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update([FromRoute]int id, [FromBody]AnalyticBindingModel analytic)
        {
            var analyticEntity = await _analyticRepository.GetById(id);

            if (analyticEntity == null)
                return NotFound();

            if (analyticEntity.PbixUrl != analytic.PbixUrl)
            {
                await PowerBiHelper.DeleteReport(AppUser.TenantId, analyticEntity.Id);

                var powerBiReportName = analyticEntity.Id.ToString();
                var import = await PowerBiHelper.ImportPbix(analytic.PbixUrl, powerBiReportName, AppUser.TenantId);

                if (string.IsNullOrWhiteSpace(import.Id))
                    throw new Exception("Pbix import not succeeded");

                // Wait 5 second for PowerBI Embedded import pbix completed
                await Task.Delay(5000);

                var report = await PowerBiHelper.GetReportByName(AppUser.TenantId, powerBiReportName);

                if (report == null)
                    throw new Exception("Pbix import not succeeded");

                analyticEntity.PowerBiReportId = report.Id;

                BackgroundJob.Enqueue(() => PowerBiHelper.UpdateConnectionString(analyticEntity.Id, AppUser.TenantId));
            }

            if (analyticEntity.Shares != null && analyticEntity.Shares.Count > 0)
            {
                var currentShares = analyticEntity.Shares.ToList();

                foreach (var sharedUser in currentShares)
                {
                    await _analyticRepository.DeleteAnalyticShare(analyticEntity, sharedUser);
                }
            }

            await AnalyticsHelper.UpdateEntity(analytic, analyticEntity, _userRepository);
            await _analyticRepository.Update(analyticEntity);

            return Ok(analyticEntity);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete([FromRoute]int id)
        {
            var analyticEntity = await _analyticRepository.GetById(id);

            if (analyticEntity == null)
                return NotFound();

            await _analyticRepository.DeleteSoft(analyticEntity);

            return Ok();
        }
    }
}