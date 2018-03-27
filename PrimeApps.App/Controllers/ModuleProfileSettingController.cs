using Newtonsoft.Json.Linq;
using PrimeApps.App.ActionFilters;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Entities;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace PrimeApps.App.Controllers
{
    [RoutePrefix("api/module_profile_settings"), Authorize, SnakeCase]
    public class ModuleProfileSettingController : BaseController
    {
        private IModuleProfileSettingRepository _moduleProfileSettingRepository;

        public ModuleProfileSettingController(IModuleProfileSettingRepository moduleProfileSettingRepository)
        {
            _moduleProfileSettingRepository = moduleProfileSettingRepository;
        }

        [Route("get_all"), HttpGet]
        public async Task<IHttpActionResult> GetAll()
        {
            var moduleProfileSettingEntities = await _moduleProfileSettingRepository.GetAllBasic();

            return Ok(moduleProfileSettingEntities);
        }

        [Route("create"), HttpPost]
        public async Task<IHttpActionResult> Create(ModuleProfileSettingBindingModels moduleProfileSetting)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var moduleProfileSettingEntity = ModuleProfileSettingHelper.CreateEntity(moduleProfileSetting, _moduleProfileSettingRepository);
            var result = await _moduleProfileSettingRepository.Create(moduleProfileSettingEntity);

            if (result < 1)
                throw new HttpResponseException(HttpStatusCode.InternalServerError);

            var uri = Request.RequestUri;
            return Created(uri.Scheme + "://" + uri.Authority + "/api/user_custom_shares/get/" + moduleProfileSettingEntity.Id, moduleProfileSettingEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IHttpActionResult> Update([FromUri]int id, [FromBody]ModuleProfileSettingBindingModels moduleProfileSetting)
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
        public async Task<IHttpActionResult> Delete([FromUri]int id)
        {
            var moduleProfileSettingEntity = await _moduleProfileSettingRepository.GetByIdBasic(id);

            if (moduleProfileSettingEntity == null)
                return NotFound();

            await _moduleProfileSettingRepository.DeleteSoft(moduleProfileSettingEntity);

            return Ok();
        }
    }
}