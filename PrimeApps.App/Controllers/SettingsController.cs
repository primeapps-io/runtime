using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.App.ActionFilters;
using PrimeApps.App.Models;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Controllers
{
    [Route("api/settings"), Authorize, SnakeCase]
    public class SettingController : BaseController
    {
        private ISettingRepository _settingRepository;
        private IUserRepository _userRepository;

        public SettingController(ISettingRepository settingRespository, IUserRepository userRespository)
        {
            _settingRepository = settingRespository;
            _userRepository = userRespository;
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
        public async Task<IActionResult> GetByKey(SettingType settingType, string key, [FromUri]int? userId = 0)
        {
            if (settingType == SettingType.Email || settingType == SettingType.SMS || settingType == SettingType.Phone)
                userId = AppUser.Id;

            var settingEntity = await _settingRepository.GetByKeyAsync(key, userId);

            if (settingEntity == null)
                return Ok();

            return Ok(settingEntity);
        }

        [Route("get_all/{settingType}"), HttpGet]
        public async Task<ICollection<Setting>> GetAll(SettingType settingType, [FromUri]int? userId = 0)
        {
            if (settingType == SettingType.Email || settingType == SettingType.SMS || settingType == SettingType.Phone)
                userId = AppUser.Id;

            if (userId.HasValue && userId.Value > 0)
                return await _settingRepository.GetAsync(settingType, userId.Value);

            return await _settingRepository.GetAsync(settingType);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create(SettingBindingModel setting)
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
                throw new HttpResponseException(HttpStatusCode.InternalServerError);

            settingEntity.Id = result;

            var uri = Request.RequestUri;
            return Created(uri.Scheme + "://" + uri.Authority + "/api/setting/get/" + settingEntity.Id, settingEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update([FromUri]int id, [FromBody]SettingBindingModel setting)
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
        public async Task<IActionResult> Delete([FromUri]int id)
        {
            var settingEntity = await _settingRepository.GetById(id);

            if (settingEntity == null)
                return NotFound();

            await _settingRepository.DeleteSoft(settingEntity);

            return Ok();
        }
    }
}
