using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeApps.Model.Repositories.Interfaces;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;

namespace PrimeApps.App.Controllers
{
    [Route("api/tenant")]
    public class PlatformController : BaseController
    {
        private ITenantRepository _tenantRepository;

        public PlatformController(ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        [Route("get_ids_by_app"), HttpGet]
        public async Task<IActionResult> GetIdsByApp([FromQuery(Name = "app_id")]int appId)
        {
            if (appId <= 0)
                return BadRequest("app_id cannot be zero or negative.");

            var tenantIds = await _tenantRepository.GetByAppId(appId);

            return Ok(tenantIds);
        }
    }
}