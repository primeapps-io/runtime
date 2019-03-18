using System;
using System.Linq;
using PrimeApps.Model.Repositories.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Models;
using PrimeApps.Model.Enums;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common;
using PrimeApps.Model.Common.Chart;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/report")]
    public class ReportController : DraftBaseController
    {
        private IReportRepository _reportRepository;
        private IConfiguration _configuration;
        private IReportHelper _reportHelper;
        private IRecordHelper _recordHelper;
        private IPermissionHelper _permissionHelper;

        public ReportController(IReportRepository reportRepository, IConfiguration configuration,
            IReportHelper reportHelper, IRecordHelper recordHelper, IPermissionHelper permissionHelper)
        {
            _reportRepository = reportRepository;
            _configuration = configuration;
            _reportHelper = reportHelper;
            _recordHelper = recordHelper;
            _permissionHelper = permissionHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_reportRepository, PreviewMode, AppId, TenantId);

            base.OnActionExecuting(context);
        }

        [Route("count"), HttpGet]
        public async Task<IActionResult> Count()
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "report", RequestTypeEnum.View))
                return StatusCode(403);

            var count = await _reportRepository.Count();

            return Ok(count);
        }


        [Route("find"), HttpPost]
        public async Task<IActionResult> Find([FromBody] PaginationModel paginationModel)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "report", RequestTypeEnum.View))
                return StatusCode(403);

            var reports = await _reportRepository.Find(paginationModel);

            return Ok(reports);
        }

        [Route("get_by_id/{id:int}"), HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "report", RequestTypeEnum.View))
                return StatusCode(403);

            var report = await _reportRepository.GetById(id);

            return Ok(report);
        }

        [Route("get_categories"), HttpGet]
        public async Task<IActionResult> GetAllCategory()
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "report", RequestTypeEnum.View))
                return StatusCode(403);

            var categories = await _reportRepository.GetAllCategories();

            return Ok(categories);
        }

        [Route("create_category"), HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] ReportCategoryBindingModel reportCategory)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "report", RequestTypeEnum.Create))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reportCategoryEntity = _reportHelper.CreateCategoryEntity(reportCategory);
            var result = await _reportRepository.CreateCategory(reportCategoryEntity);

            if (result < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            var uri = new Uri(Request.GetDisplayUrl());
            return Created(uri.Scheme + "://" + uri.Authority + "/api/get_categories", reportCategoryEntity);
            //return Created(Request.Scheme + "://" + Request.Host + "/api/get_categories", reportCategoryEntity);
        }

        [Route("update_category/{id:int}"), HttpPut]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] ReportCategoryBindingModel reportCategory)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "report", RequestTypeEnum.Update))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reportCategoryEntity = await _reportRepository.GetCategoryById(id);

            if (reportCategoryEntity == null)
                return NotFound();

            _reportHelper.UpdateCategoryEntity(reportCategory, reportCategoryEntity);
            await _reportRepository.UpdateCategory(reportCategoryEntity);

            return Ok(reportCategoryEntity);
        }

        [Route("delete_category/{id:int}"), HttpDelete]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "report", RequestTypeEnum.Delete))
                return StatusCode(403);

            var reportCategoryEntity = await _reportRepository.GetCategoryById(id);

            if (reportCategoryEntity == null)
                return NotFound();

            await _reportRepository.DeleteSoftCategory(reportCategoryEntity);

            return Ok();
        }

        [Route("get_report/{id:int}"), HttpGet]
        public async Task<IActionResult> GetReport(int Id)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "report", RequestTypeEnum.View))
                return StatusCode(403);

            var report = await _reportRepository.GetById(Id);

            return Ok(report);
        }

        [Route("get_chart/{report:int}"), HttpGet]
        public async Task<IActionResult> GetChart(int report)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "report", RequestTypeEnum.View))
                return StatusCode(403);

            var chart = await _reportRepository.GetChartByReportId(report);
            var aggregation = chart.Report.Aggregations.FirstOrDefault();
            var response = new
            {
                chart = new ChartView
                {
                    Caption = chart.Caption,
                    ChartType = chart.ChartType,
                    Subcaption = chart.SubCaption,
                    Theme = chart.Theme,
                    Xaxisname = chart.XaxisName,
                    Yaxisname = chart.YaxisName,
                    ReportId = chart.ReportId.ToString(),
                    ReportModuleId = chart.Report.ModuleId,
                    ReportGroupField = chart.Report.GroupField,
                    ReportAggregationField = aggregation.Field
                }
            };

            return Ok(response);
        }

        [Route("get_widget/{report:int}"), HttpGet]
        public async Task<IActionResult> GetWidget(int report)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "report", RequestTypeEnum.View))
                return StatusCode(403);

            var widget = await _reportRepository.GetWidgetByReportId(report);
            return Ok(widget);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody] ReportBindingModel report)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "report", RequestTypeEnum.Create))
                return StatusCode(403);

            _reportHelper.Validate(report, ModelState, _recordHelper.ValidateFilterLogic);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reportEntity = await _reportHelper.CreateEntity(report);
            var resultReport = await _reportRepository.Create(reportEntity);

            if (resultReport < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());

            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            if (report.ReportType == ReportType.Summary)
            {
                var chartEntity = _reportHelper.CreateChartEntity(report.Chart, reportEntity.Id);
                var resultChart = await _reportRepository.CreateChart(chartEntity);

                if (resultChart < 1)
                {
                    await _reportRepository.DeleteHard(reportEntity);
                    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
                    //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);
                }
            }

            if (report.ReportType == ReportType.Single)
            {
                var widgetEntity = _reportHelper.CreateWidgetEntity(report.Widget, reportEntity.Id);
                var resultWidget = await _reportRepository.CreateWidget(widgetEntity);

                if (resultWidget < 1)
                {
                    await _reportRepository.DeleteHard(reportEntity);
                    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
                    //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);
                }
            }

            var uri = new Uri(Request.GetDisplayUrl());
            return Created(uri.Scheme + "://" + uri.Authority + "/api/report/get/" + reportEntity.Id, reportEntity);
            //return Created(Request.Scheme + "://" + Request.Host + "/api/report/get/" + reportEntity.Id, reportEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody] ReportBindingModel report)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "report", RequestTypeEnum.Update))
                return StatusCode(403);

            _reportHelper.Validate(report, ModelState, _recordHelper.ValidateFilterLogic);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reportEntity = await _reportRepository.GetById(id);

            if (reportEntity == null)
                return NotFound();

            if (reportEntity.ReportFeed == ReportFeed.Custom)
                return BadRequest("Custom reports cannot be updated");

            var currentFieldIds = reportEntity.Fields.Select(x => x.Id).ToList();
            var currentFilterIds = reportEntity.Filters.Select(x => x.Id).ToList();
            var currentAggregationIds = reportEntity.Aggregations.Select(x => x.Id).ToList();

            if (reportEntity.Shares != null && reportEntity.Shares.Count > 0)
            {
                var currentShares = reportEntity.Shares.ToList();

                foreach (var share in currentShares)
                {
                    await _reportRepository.DeleteReportShare(share, share.TenantUser);
                }
            }

            await _reportHelper.UpdateEntity(report, reportEntity);
            await _reportRepository.Update(reportEntity, currentFieldIds, currentFilterIds, currentAggregationIds);

            if (report.ReportType == ReportType.Summary)
            {
                var currentChartEntity = await _reportRepository.GetChartByReportId(reportEntity.Id);
                var chartEntity = _reportHelper.UpdateChartEntity(report.Chart, currentChartEntity);
                await _reportRepository.UpdateChart(chartEntity);
            }

            if (report.ReportType == ReportType.Single)
            {
                var currentWidgetEntity = await _reportRepository.GetWidgetByReportId(reportEntity.Id);
                var widgetEntity = _reportHelper.UpdateWidgetEntity(report.Widget, currentWidgetEntity);
                await _reportRepository.UpdateWidget(widgetEntity);
            }

            return Ok(reportEntity);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "report", RequestTypeEnum.Delete))
                return StatusCode(403);

            var reportEntity = await _reportRepository.GetById(id);

            if (reportEntity == null)
                return NotFound();

            await _reportRepository.DeleteSoft(reportEntity);

            return Ok();
        }
    }
}