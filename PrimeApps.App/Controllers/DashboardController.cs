using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using PrimeApps.App.ActionFilters;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.App.Models;
using PrimeApps.App.Helpers;
using System;

namespace PrimeApps.App.Controllers
{
    [RoutePrefix("api/dashboard"), Authorize, SnakeCase]
    public class DashboardController : BaseController
    {
        private IDashletRepository _dashletRepository;
        private ISettingRepository _settingRepository;
        private IDashboardRepository _dashboardRepository;
        private IReportRepository _reportRepository;

        public DashboardController(ISettingRepository settingRepository, IDashletRepository dashletRepository, IDashboardRepository dashboardRepository, IReportRepository reportRepository)
        {
            _settingRepository = settingRepository;
            _dashletRepository = dashletRepository;
            _dashboardRepository = dashboardRepository;
            _reportRepository = reportRepository;
        }

        [Route("get_dashlets"), HttpGet]
        public async Task<IHttpActionResult> GetDashlets([FromUri]int dashboard, [FromUri]string locale = "", [FromUri]int? timezoneOffset = 180)
        {
            var dashlets = await _dashletRepository.GetDashboardDashlets(dashboard, AppUser, locale, timezoneOffset.Value);
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
        public async Task<IHttpActionResult> GetAll()
        {
            var dashboard = await _dashboardRepository.GetAllBasic(AppUser);
            return Ok(dashboard);
        }

        [Route("create"), HttpPost]
        public async Task<IHttpActionResult> Create(DashboardBindingModel dashboard)
        {
            var dashboardEntity = await DashboardHelper.CreateEntity(dashboard, AppUser);
            var create = await _dashboardRepository.Create(dashboardEntity);
            return Ok(create);
        }

        [Route("create_dashlet"), HttpPost]
        public async Task<IHttpActionResult> DashletCreate(DashletBindingModel dashlet)
        {

            var dashletEntity = await DashboardHelper.CreateEntityDashlet(dashlet, _dashboardRepository, _reportRepository);
            var create = await _dashletRepository.Create(dashletEntity);
            return Ok(create);
        }

        [Route("get_charts"), HttpGet]
        public async Task<IHttpActionResult> GetCharts()
        {
            var chartEntity = await _dashboardRepository.GetAllChart();

            if (chartEntity == null)
                return NotFound();

            var charViewModel = DashboardHelper.MapToViewModel(chartEntity);

            return Ok(charViewModel);

        }

        [Route("get_widgets"), HttpGet]
        public async Task<IHttpActionResult> GetWidgets()
        {
            var widgetEntity = await _dashboardRepository.GetAllWidget();

            if (widgetEntity == null)
                return NotFound();

            var charViewModel = DashboardHelper.MapToViewModel(widgetEntity);

            return Ok(charViewModel);

        }

        [Route("delete_dashlet/{id:int}"), HttpDelete]
        public async Task<IHttpActionResult> DeleteDashlet([FromUri]int id)
        {
            var dashletEntity = await _dashletRepository.GetDashletById(id);

            if (dashletEntity == null)
                return NotFound();

            await _dashletRepository.DeleteSoftDashlet(dashletEntity);

            return Ok();
        }

        [Route("change_order_dashlet"), HttpPut]
        public async Task<IHttpActionResult> UpdateDashlet([FromBody] JArray data)
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
        public async Task<IHttpActionResult> UpdateDashlet([FromUri]int id, [FromBody]DashletBindingModel dashlet)
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
