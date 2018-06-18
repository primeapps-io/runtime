using System;
using System.Linq;
using System.Net;
using PrimeApps.App.ActionFilters;
using PrimeApps.Model.Repositories.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Common.Chart;
using PrimeApps.Model.Enums;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PrimeApps.App.Controllers
{
    [Route("api/report"), Authorize/*, SnakeCase*/]
	public class ReportController : BaseController
    {
        private IReportRepository _reportRepository;
        private IRecordRepository _recordRepository;
        private IModuleRepository _moduleRepository;
        private IPicklistRepository _picklistRepository;
        private IUserRepository _userRepository;

        public ReportController(IReportRepository reportRepository, IRecordRepository recordRepository, IModuleRepository moduleRepository, IPicklistRepository picklistRepository, IUserRepository userRepository)
        {
            _reportRepository = reportRepository;
            _recordRepository = recordRepository;
            _moduleRepository = moduleRepository;
            _picklistRepository = picklistRepository;
            _userRepository = userRepository;
        }

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			SetContext(context);
            SetCurrentUser(_reportRepository);
            SetCurrentUser(_userRepository);
			SetCurrentUser(_picklistRepository);
			SetCurrentUser(_moduleRepository);
			SetCurrentUser(_recordRepository);
            base.OnActionExecuting(context);
		}

		[Route("get_all"), HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var report = _reportRepository.GetAllBasic();

            return Ok(report);
        }

        [Route("get_report/{id:int}"), HttpGet]
        public async Task<IActionResult> GetReport(int Id)
        {
            var report = await _reportRepository.GetById(Id);

            return Ok(report);
        }
        [Route("get_chart/{report:int}"), HttpGet]
        public async Task<IActionResult> GetChart(int report)
        {
            var chart = await _reportRepository.GetChartByReportId(report);
            var aggregation = chart.Report.Aggregations.FirstOrDefault();
            var showDisplayValue = chart.ChartType != ChartType.Funnel && chart.ChartType != ChartType.Pyramid && aggregation != null && aggregation.AggregationType != AggregationType.Count;
            var data = await _reportRepository.GetDashletReportData(report, _recordRepository, _moduleRepository, _picklistRepository, AppUser, showDisplayValue: showDisplayValue);

            var response = new
            {
                data = data,
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
            var response = await _reportRepository.GetDashletReportData(report, _recordRepository, _moduleRepository, _picklistRepository, AppUser);
            var widget = await _reportRepository.GetWidgetByReportId(report);

            response.First()["color"] = widget.Color;
            response.First()["icon"] = widget.Icon;

            return Ok(response);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]ReportBindingModel report)
        {
            ReportHelper.Validate(report, ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reportEntity = await ReportHelper.CreateEntity(report, _userRepository);
            var resultReport = await _reportRepository.Create(reportEntity);

            if (resultReport < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());

            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            if (report.ReportType == ReportType.Summary)
            {
                var chartEntity = ReportHelper.CreateChartEntity(report.Chart, reportEntity.Id);
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
                var widgetEntity = ReportHelper.CreateWidgetEntity(report.Widget, reportEntity.Id);
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
        public async Task<IActionResult> Update(int id, [FromBody]ReportBindingModel report)
        {
            ReportHelper.Validate(report, ModelState);

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

            await ReportHelper.UpdateEntity(report, reportEntity, _userRepository);
            await _reportRepository.Update(reportEntity, currentFieldIds, currentFilterIds, currentAggregationIds);

            if (report.ReportType == ReportType.Summary)
            {
                var currentChartEntity = await _reportRepository.GetChartByReportId(reportEntity.Id);
                var chartEntity = ReportHelper.UpdateChartEntity(report.Chart, currentChartEntity);
                await _reportRepository.UpdateChart(chartEntity);
            }

            if (report.ReportType == ReportType.Single)
            {
                var currentWidgetEntity = await _reportRepository.GetWidgetByReportId(reportEntity.Id);
                var widgetEntity = ReportHelper.UpdateWidgetEntity(report.Widget, currentWidgetEntity);
                await _reportRepository.UpdateWidget(widgetEntity);
            }

            return Ok(reportEntity);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var reportEntity = await _reportRepository.GetById(id);

            if (reportEntity == null)
                return NotFound();

            await _reportRepository.DeleteSoft(reportEntity);

            return Ok();
        }

        [Route("get_categories"), HttpGet]
        public IActionResult GetCategories()
        {
            var reportCategories = _reportRepository.GetCategories(AppUser.Id);

            return Ok(reportCategories);
        }

        [Route("create_category"), HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody]ReportCategoryBindingModel reportCategory)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reportCategoryEntity = ReportHelper.CreateCategoryEntity(reportCategory);
            var result = await _reportRepository.CreateCategory(reportCategoryEntity);

            if (result < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            var uri = new Uri(Request.GetDisplayUrl());
			return Created(uri.Scheme + "://" + uri.Authority + "/api/get_categories", reportCategoryEntity);
            //return Created(Request.Scheme + "://" + Request.Host + "/api/get_categories", reportCategoryEntity);
        }

        [Route("update_category/{id:int}"), HttpPut]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody]ReportCategoryBindingModel reportCategory)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reportCategoryEntity = await _reportRepository.GetCategoryById(id);

            if (reportCategoryEntity == null)
                return NotFound();

            ReportHelper.UpdateCategoryEntity(reportCategory, reportCategoryEntity);
            await _reportRepository.UpdateCategory(reportCategoryEntity);

            return Ok(reportCategoryEntity);
        }

        [Route("delete_category/{id:int}"), HttpDelete]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var reportCategoryEntity = await _reportRepository.GetCategoryById(id);

            if (reportCategoryEntity == null)
                return NotFound();

            await _reportRepository.DeleteSoftCategory(reportCategoryEntity);

            return Ok();
        }
    }
}