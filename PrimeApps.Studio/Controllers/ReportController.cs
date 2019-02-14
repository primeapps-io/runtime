using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/report")]
    public class ReportController : DraftBaseController
    {
        private IReportRepository _reportRepository;
        private IConfiguration _configuration;
        private Warehouse _warehouse;

        public ReportController(IReportRepository reportRepository, IConfiguration configuration)
        {
            _reportRepository = reportRepository;
            _configuration = configuration;

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
            var count = await _reportRepository.Count();

            return Ok(count);
        }

        [Route("find"), HttpPost]
        public async Task<IActionResult> Find([FromBody]PaginationModel paginationModel)
        {
            var reports = await _reportRepository.Find(paginationModel);

            return Ok(reports);
        }

        [Route("get_by_id/{id:int}"), HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var report = await _reportRepository.GetById(id);

            return Ok(report);
        }
        [Route("get_categories"), HttpGet]
        public async Task<IActionResult> GetAllCategory()
        {
            var categories = await _reportRepository.GetAllCategories();

            return Ok(categories);
        }




    }
}
