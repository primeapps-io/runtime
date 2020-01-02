using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Models;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/module_profile_settings"), Authorize]
    public class ModuleProfileSettingController : DraftBaseController
    {
        private IModuleProfileSettingRepository _moduleProfileSettingRepository;
        private IConfiguration _configuration;
        private IPermissionHelper _permissionHelper;

        public ModuleProfileSettingController(IModuleProfileSettingRepository moduleProfileSettingRepository, IConfiguration configuration, IPermissionHelper permissionHelper)
        {
            _moduleProfileSettingRepository = moduleProfileSettingRepository;
            _configuration = configuration;
            _permissionHelper = permissionHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_moduleProfileSettingRepository, PreviewMode, TenantId, AppId);

            base.OnActionExecuting(context);
        }

        [Route("get_all"), HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var moduleProfileSettingEntities = await _moduleProfileSettingRepository.GetAllBasic();

            return Ok(moduleProfileSettingEntities);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]ModuleProfileSettingBindingModels moduleProfileSetting)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "module_profile_settings", RequestTypeEnum.Create))
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var moduleProfileSettingEntity = ModuleProfileSettingHelper.CreateEntity(moduleProfileSetting, _moduleProfileSettingRepository);
            var result = await _moduleProfileSettingRepository.Create(moduleProfileSettingEntity);

            if (result < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            var uri = new Uri(Request.GetDisplayUrl());
            return Created(uri.Scheme + "://" + uri.Authority + "/api/user_custom_shares/get/" + moduleProfileSettingEntity.Id, moduleProfileSettingEntity);
            //return Created(Request.Scheme + "://" + Request.Host + "/api/user_custom_shares/get/" + moduleProfileSettingEntity.Id, moduleProfileSettingEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]ModuleProfileSettingBindingModels moduleProfileSetting)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "module_profile_settings", RequestTypeEnum.Update))
                return Forbid();

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
        public async Task<IActionResult> Delete(int id)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "module_profile_settings", RequestTypeEnum.Delete))
                return Forbid();

            var moduleProfileSettingEntity = await _moduleProfileSettingRepository.GetByIdBasic(id);

            if (moduleProfileSettingEntity == null)
                return NotFound();

            await _moduleProfileSettingRepository.DeleteSoft(moduleProfileSettingEntity);

            return Ok();
        }

        [Route("count/{id:int}"), HttpGet]
        public async Task<IActionResult> Count(int id)
        {
            var count = await _moduleProfileSettingRepository.Count(id);

            //if (count < 1)
            //	return NotFound(count);

            return Ok(count);
        }

        [Route("find")]
        public IActionResult Find(ODataQueryOptions<ModuleProfileSetting> queryOptions)
        {
            var templates = _moduleProfileSettingRepository.Find();
              
            var queryResults = (IQueryable<ModuleProfileSetting>)queryOptions.ApplyTo(templates, new ODataQuerySettings() { EnsureStableOrdering = false });
            return Ok(new PageResult<ModuleProfileSetting>(queryResults, Request.ODataFeature().NextLink, Request.ODataFeature().TotalCount));
        }
    }
}