using System;
using System.Linq;
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
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common.Record;

namespace PrimeApps.App.Controllers
{
    [Route("api/report"), Authorize]
    public class ReportController : ApiBaseController
    {
        private IReportRepository _reportRepository;
        private IRecordRepository _recordRepository;
        private IModuleRepository _moduleRepository;
        private IPicklistRepository _picklistRepository;
        private IUserRepository _userRepository;
        private IConfiguration _configuration;

        private IRecordHelper _recordHelper;
        private IReportHelper _reportHelper;

        public ReportController(IReportRepository reportRepository, IRecordRepository recordRepository,
            IModuleRepository moduleRepository, IPicklistRepository picklistRepository, IUserRepository userRepository,
            IRecordHelper recordHelper, IReportHelper reportHelper, IConfiguration configuration)
        {
            _reportRepository = reportRepository;
            _recordRepository = recordRepository;
            _moduleRepository = moduleRepository;
            _picklistRepository = picklistRepository;
            _userRepository = userRepository;

            _recordHelper = recordHelper;
            _reportHelper = reportHelper;
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_reportRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_userRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_picklistRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_moduleRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_recordRepository, PreviewMode, TenantId, AppId);
            base.OnActionExecuting(context);
        }

        [Route("get_all"), HttpGet]
        public IActionResult GetAll()
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

        [Route("chart_filter"), HttpPost]
        public async Task<IActionResult> ChartFilter(JObject request)
        {
            if (string.IsNullOrEmpty(request["module_name"].ToString()))
                return BadRequest("module_name is required.");

            if (string.IsNullOrEmpty(request["aggregation_field"].ToString()))
                return BadRequest("aggregation_field is required.");
            
            var requestModel = JsonConvert.DeserializeObject<FindRequest>(JsonConvert.SerializeObject(request));
            
            var chartType = ChartType.Column3D;
            var aggregation = AggregationType.Count;

            if (!string.IsNullOrEmpty(request["chart_type"].ToString()))
                chartType = Enum.Parse<ChartType>(request["chart_type"].ToString());

            if (!string.IsNullOrEmpty(request["aggregation_type"].ToString()))
                aggregation = Enum.Parse<AggregationType>(request["aggregation_type"].ToString());

            var showDisplayValue = chartType != ChartType.Funnel && chartType != ChartType.Pyramid && aggregation != AggregationType.Count;
            var currentCulture = AppUser.Culture ?? (AppUser.Language == "en" ? "en-US" : "tr-TR");


            var result = await _reportRepository.ChartFilter(requestModel, request["aggregation_field"].ToString(), currentCulture, AppUser.TenantLanguage, request["model_name"].ToString(), _configuration, _picklistRepository, _recordRepository, _moduleRepository, showDisplayValue: showDisplayValue);

            return Ok(result);
        }

        [Route("get_chart/{report:int}"), HttpGet]
        public async Task<IActionResult> GetChart(int report)
        {
            var chart = await _reportRepository.GetChartByReportId(report);
            var aggregation = chart.Report.Aggregations.FirstOrDefault();
            var showDisplayValue = chart.ChartType != ChartType.Funnel && chart.ChartType != ChartType.Pyramid &&
                                   aggregation != null && aggregation.AggregationType != AggregationType.Count;
            var data = await _reportRepository.GetDashletReportData(report, _recordRepository, _moduleRepository,
                _picklistRepository, _configuration, AppUser, showDisplayValue: showDisplayValue);

            var response = new
            {
                data = data,
                chart = new ChartView
                {
                    CaptionEn = chart.CaptionEn,
                    CaptionTr = chart.CaptionTr,
                    ChartType = chart.ChartType,
                    SubcaptionEn = chart.SubCaptionEn,
                    SubcaptionTr = chart.SubCaptionTr,
                    Theme = chart.Theme,
                    XaxisnameEn = chart.XaxisNameEn,
                    XaxisnameTr = chart.XaxisNameTr,
                    YaxisnameEn = chart.YaxisNameEn,
                    YaxisnameTr = chart.YaxisNameTr,
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
            var response = await _reportRepository.GetDashletReportData(report, _recordRepository, _moduleRepository,
                _picklistRepository, _configuration, AppUser);
            var widget = await _reportRepository.GetWidgetByReportId(report);

            response.First()["color"] = widget.Color;
            response.First()["icon"] = widget.Icon;

            return Ok(response);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody] ReportBindingModel report)
        {
            _reportHelper.Validate(report, ModelState, _recordHelper.ValidateFilterLogic);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reportEntity = _reportHelper.CreateEntity(report);
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

            _reportHelper.UpdateEntity(report, reportEntity);
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
        public async Task<IActionResult> CreateCategory([FromBody] ReportCategoryBindingModel reportCategory)
        {
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
            var reportCategoryEntity = await _reportRepository.GetCategoryById(id);

            if (reportCategoryEntity == null)
                return NotFound();

            await _reportRepository.DeleteSoftCategory(reportCategoryEntity);

            return Ok();
        }
    }
}