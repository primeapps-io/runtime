using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Models;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/setting")]
    public class SettingController : DraftBaseController
    {
        private IConfiguration _configuration;
        private ISettingRepository _settingRepository;
        private IUserRepository _userRepository;
        private IPermissionHelper _permissionHelper;

        public SettingController(IConfiguration configuration, ISettingRepository settingRepository, IPermissionHelper permissionHelper,
            IUserRepository userRepository)
        {
            _configuration = configuration;
            _settingRepository = settingRepository;
            _userRepository = userRepository;
            _permissionHelper = permissionHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_settingRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_userRepository, PreviewMode, AppId, TenantId);
            base.OnActionExecuting(context);
        }


        [Route("count"), HttpGet]
        public async Task<IActionResult> Count()
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "setting", RequestTypeEnum.View))
                return StatusCode(403);

            var count = await _settingRepository.Count();

            return Ok(count);
        }

        [Route("find")]
        public IActionResult Find(ODataQueryOptions<Setting> queryOptions)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "setting", RequestTypeEnum.View))
                return StatusCode(403);

            var components = _settingRepository.Find();

            var queryResults = (IQueryable<Setting>)queryOptions.ApplyTo(components, new ODataQuerySettings() { EnsureStableOrdering = false });
            return Ok(new PageResult<Setting>(queryResults, Request.ODataFeature().NextLink, Request.ODataFeature().TotalCount));
        }

        [Route("get/{id:int}"), HttpGet] //Onyl get for Custom type
        public async Task<IActionResult> Get(int id)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "setting", RequestTypeEnum.View))
                return StatusCode(403);

            var result = await _settingRepository.GetByIdWithType(id, SettingType.Custom);

            return Ok(result);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]SettingBindingModel model)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "setting", RequestTypeEnum.Create))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.Type == SettingType.NotSet)
                model.Type = SettingType.Custom;

            var settingEntity = new Setting
            {
                Type = model.Type,
                UserId = null,
                Key = model.Key,
                Value = model.Value
            };

            var result = await _settingRepository.Create(settingEntity);

            if (result < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());

            return Ok(settingEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]SettingBindingModel setting)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var settingEntity = await _settingRepository.GetByIdWithType(id, SettingType.Custom);

            if (settingEntity == null)
                return NotFound();

            settingEntity.Type = SettingType.Custom;
            settingEntity.UserId = null;
            settingEntity.Key = setting.Key;
            settingEntity.Value = setting.Value;

            await _settingRepository.Update(settingEntity);

            return Ok(settingEntity);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var settingEntity = await _settingRepository.GetByIdWithType(id, SettingType.Custom);

            if (settingEntity == null)
                return NotFound();

            await _settingRepository.DeleteSoft(settingEntity);

            return Ok();
        }

    }
}