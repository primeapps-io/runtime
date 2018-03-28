using PrimeApps.App.ActionFilters;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Repositories.Interfaces;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
namespace PrimeApps.App.Controllers
{
    [Route("api/module_profile_settings"), Authorize, SnakeCase]
    public class ModuleProfileSettingController : BaseController
    {
        private IModuleProfileSettingRepository _moduleProfileSettingRepository;

        public ModuleProfileSettingController(IModuleProfileSettingRepository moduleProfileSettingRepository)
        {
            _moduleProfileSettingRepository = moduleProfileSettingRepository;
        }

        [Route("get_all"), HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var moduleProfileSettingEntities = await _moduleProfileSettingRepository.GetAllBasic();

            return Ok(moduleProfileSettingEntities);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create(ModuleProfileSettingBindingModels moduleProfileSetting)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var moduleProfileSettingEntity = ModuleProfileSettingHelper.CreateEntity(moduleProfileSetting, _moduleProfileSettingRepository);
            var result = await _moduleProfileSettingRepository.Create(moduleProfileSettingEntity);

            if (result < 1)
                throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            var uri = Request.RequestUri;
            return Created(uri.Scheme + "://" + uri.Authority + "/api/user_custom_shares/get/" + moduleProfileSettingEntity.Id, moduleProfileSettingEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update([FromRoute]int id, [FromBody]ModuleProfileSettingBindingModels moduleProfileSetting)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var moduleProfileSettingEntity = await _moduleProfileSettingRepository.GetByIdBasic(id);

            if (moduleProfileSettingEntity == null)
                return NotFound();

            ModuleProfileSettingHelper.UpdateEntity(moduleProfileSetting, moduleProfileSettingEntity, _moduleProfileSettingRepository);
            await _moduleProfileSettingRepository.Update(moduleProfileSettingEntity);

            return Ok(moduleProfileSettingEntity);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete([FromRoute]int id)
        {
            var moduleProfileSettingEntity = await _moduleProfileSettingRepository.GetByIdBasic(id);

            if (moduleProfileSettingEntity == null)
                return NotFound();

            await _moduleProfileSettingRepository.DeleteSoft(moduleProfileSettingEntity);

            return Ok();
        }
    }
}