using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeApps.App.ActionFilters;
using PrimeApps.App.Models;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
namespace PrimeApps.App.Controllers
{
    [Route("api/settings"), Authorize]
	public class SettingController : ApiBaseController
    {
        private ISettingRepository _settingRepository;
        private IUserRepository _userRepository;

        public SettingController(ISettingRepository settingRespository, IUserRepository userRespository)
        {
            _settingRepository = settingRespository;
            _userRepository = userRespository;
        }

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			SetContext(context);
			SetCurrentUser(_userRepository, PreviewMode, TenantId, AppId);
			SetCurrentUser(_settingRepository, PreviewMode, TenantId, AppId);

			base.OnActionExecuting(context);
		}

		[Route("get/{id:int}"), HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var settingEntity = await _settingRepository.GetById(id);

            if (settingEntity == null)
                return NotFound();

            return Ok(settingEntity);
        }

        [Route("get_by_key/{settingType}/{key}"), HttpGet]
        public async Task<IActionResult> GetByKey(SettingType settingType, string key, [FromQuery(Name = "userId")]int? userId = 0)
        {
            if (settingType == SettingType.Email || settingType == SettingType.SMS || settingType == SettingType.Phone)
                userId = AppUser.Id;

            var settingEntity = await _settingRepository.GetByKeyAsync(key, userId);

            if (settingEntity == null)
                return Ok();

            return Ok(settingEntity);
        }

        [Route("get_all/{settingType}"), HttpGet]
        public async Task<ICollection<Setting>> GetAll(SettingType settingType, [FromQuery(Name = "userId")]int? userId = 0)
        {
            if (settingType == SettingType.Email || settingType == SettingType.SMS || settingType == SettingType.Phone)
                userId = AppUser.Id;

            if (userId.HasValue && userId.Value > 0)
                return await _settingRepository.GetAsync(settingType, userId.Value);

            return await _settingRepository.GetAsync(settingType);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]SettingBindingModel setting)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (setting.UserId.HasValue)
            {
                var user = await _userRepository.GetById(setting.UserId.Value);

                if (user == null)
                    return BadRequest("User not found.");
            }

            var settingEntity = new Setting
            {
                Type = setting.Type,
                UserId = setting.UserId,
                Key = setting.Key,
                Value = setting.Value
            };

            var result = await _settingRepository.Create(settingEntity);

            if (result < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            settingEntity.Id = result;

            var uri = new Uri(Request.GetDisplayUrl());
			return Created(uri.Scheme + "://" + uri.Authority + "/api/setting/get/" + settingEntity.Id, settingEntity);
            //return Created(Request.Scheme + "://" + Request.Host + "/api/setting/get/" + settingEntity.Id, settingEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]SettingBindingModel setting)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var settingEntity = await _settingRepository.GetById(id);

            if (settingEntity == null)
                return NotFound();

            if (setting.UserId.HasValue)
            {
                var user = await _userRepository.GetById(setting.UserId.Value);

                if (user == null)
                    return BadRequest("User not found.");
            }

            settingEntity.Type = setting.Type;
            settingEntity.UserId = setting.UserId;
            settingEntity.Key = setting.Key;
            settingEntity.Value = setting.Value;

            await _settingRepository.Update(settingEntity);

            return Ok(settingEntity);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var settingEntity = await _settingRepository.GetById(id);

            if (settingEntity == null)
                return NotFound();

            await _settingRepository.DeleteSoft(settingEntity);

            return Ok();
        }
    }
}
