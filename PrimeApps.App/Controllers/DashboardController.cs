using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.App.Models;
using PrimeApps.App.Helpers;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace PrimeApps.App.Controllers
{
    [Route("api/dashboard"), Authorize]
    public class DashboardController : ApiBaseController
    {
        private IDashletRepository _dashletRepository;
        private ISettingRepository _settingRepository;
        private IDashboardRepository _dashboardRepository;
        private IReportRepository _reportRepository;
        private IRecordRepository _recordRepository;
        private IModuleRepository _moduleRepository;
        private IPicklistRepository _picklistRepository;
        private IViewRepository _viewRepository;
        private IConfiguration _configuration;

        public DashboardController(ISettingRepository settingRepository, IDashletRepository dashletRepository, IDashboardRepository dashboardRepository, IReportRepository reportRepository, IRecordRepository recordRepository, IModuleRepository moduleRepository, IPicklistRepository picklistRepository, IViewRepository viewRepository, IConfiguration configuration)
        {
            _settingRepository = settingRepository;
            _dashletRepository = dashletRepository;
            _dashboardRepository = dashboardRepository;
            _reportRepository = reportRepository;
            _recordRepository = recordRepository;
            _moduleRepository = moduleRepository;
            _picklistRepository = picklistRepository;
            _viewRepository = viewRepository;
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_settingRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_dashboardRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_dashletRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_reportRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_recordRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_moduleRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_picklistRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_viewRepository, PreviewMode, TenantId, AppId);
            base.OnActionExecuting(context);
        }

        [Route("get_dashlets"), HttpGet]
        public async Task<IActionResult> GetDashlets([FromQuery(Name = "dashboard")]int dashboard, [FromQuery(Name = "locale")]string locale = "", [FromQuery(Name = "timezoneOffset")]int? timezoneOffset = 180)
        {
            var dashlets = await _dashletRepository.GetDashboardDashlets(dashboard, AppUser, _reportRepository, _recordRepository, _moduleRepository, _picklistRepository, _viewRepository, _configuration, locale, timezoneOffset.Value);
            return Ok(dashlets);
        }

        //This method is temporary. TODO: Delete after new dashboard development finished
        [Route("get_custom"), HttpGet]
        public async Task<dynamic> GetCustom()
        {
            var userSetting = await _settingRepository.GetAsync(SettingType.Custom);
            var dashboardJson = userSetting?.FirstOrDefault(x => x.Key == "dashboardJson");

            if (dashboardJson == null)
                return null;

            var dashboard = JObject.Parse(dashboardJson.Value);

            return dashboard;
        }

        [Route("get_all"), HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var dashboard = await _dashboardRepository.GetAllBasic(AppUser);
            return Ok(dashboard);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]DashboardBindingModel dashboard)
        {
            var dashboardEntity = await DashboardHelper.CreateEntity(dashboard, AppUser);
            var create = await _dashboardRepository.Create(dashboardEntity);
            return Ok(create);
        }

        [Route("create_dashlet"), HttpPost]
        public async Task<IActionResult> DashletCreate([FromBody]DashletBindingModel dashlet)
        {

            var dashletEntity = await DashboardHelper.CreateEntityDashlet(dashlet, _dashboardRepository, _reportRepository);
            var create = await _dashletRepository.Create(dashletEntity);
            return Ok(create);
        }

        [Route("get_charts"), HttpGet]
        public async Task<IActionResult> GetCharts()
        {
            var chartEntity = await _dashboardRepository.GetAllChart();

            if (chartEntity == null)
                return NotFound();

            var charViewModel = DashboardHelper.MapToViewModel(chartEntity);

            return Ok(charViewModel);

        }

        [Route("get_widgets"), HttpGet]
        public async Task<IActionResult> GetWidgets()
        {
            var widgetEntity = await _dashboardRepository.GetAllWidget();

            if (widgetEntity == null)
                return NotFound();

            var charViewModel = DashboardHelper.MapToViewModel(widgetEntity);

            return Ok(charViewModel);

        }

        [Route("delete_dashlet/{id:int}"), HttpDelete]
        public async Task<IActionResult> DeleteDashlet(int id)
        {
            var dashletEntity = await _dashletRepository.GetDashletById(id);

            if (dashletEntity == null)
                return NotFound();

            await _dashletRepository.DeleteSoftDashlet(dashletEntity);

            return Ok();
        }

        [Route("change_order_dashlet"), HttpPut]
        public async Task<IActionResult> UpdateDashlet([FromBody] JArray data)
        {
            int counter = 0;
            foreach (JObject dashlet in data)
            {
                var id = dashlet["id"];
                var dashletEntity = await _dashletRepository.GetDashletById(Convert.ToInt32(id));
                if (dashletEntity != null)
                {
                    dashletEntity.Order = counter++;
                    await _dashletRepository.UpdateDashlet(dashletEntity);
                }

            }
            return Ok(true);
        }

        [Route("update_dashlet/{id:int}"), HttpPut]
        public async Task<IActionResult> UpdateDashlet(int id, [FromBody]DashletBindingModel dashlet)
        {
            var dashletEntity = await _dashletRepository.GetDashletById(id);

            if (dashletEntity == null)
                return NotFound();

            await DashboardHelper.UpdateEntityDashlet(dashlet, dashletEntity, _dashboardRepository, _reportRepository);

            await _dashletRepository.UpdateDashlet(dashletEntity);

            return Ok(dashletEntity);
        }
    }
}
