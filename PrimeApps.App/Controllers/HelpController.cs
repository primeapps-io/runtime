using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeApps.App.ActionFilters;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;

namespace PrimeApps.App.Controllers
{
    [Route("api/help"), Authorize]
    public class HelpController : ApiBaseController
    {
        private IHelpRepository _helpRepository;
        private IUserRepository _userRepository;
        private IRecordRepository _recordRepository;
        private IProfileRepository _profileRepository;
        private IModuleRepository _moduleRepository;
        private IPicklistRepository _picklistRepository;

        public HelpController(IHelpRepository helpRepository, IUserRepository userRepository, IRecordRepository recordRepository, IModuleRepository moduleRepository, IPicklistRepository picklistRepository, IProfileRepository profileRepository)
        {
            _helpRepository = helpRepository;
            _userRepository = userRepository;
            _recordRepository = recordRepository;
            _moduleRepository = moduleRepository;
            _profileRepository = profileRepository;
            _picklistRepository = picklistRepository;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_helpRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_userRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_recordRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_moduleRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_profileRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_picklistRepository, PreviewMode, TenantId, AppId);

            base.OnActionExecuting(context);
        }

        [Route("get/{id:int}"), HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var helpEntity = await _helpRepository.GetById(id);

            return Ok(helpEntity);
        }

        [Route("get_all"), HttpGet]
        public async Task<ICollection<Help>> GetAll([FromQuery(Name = "modalType")]ModalType modalType = ModalType.NotSet)
        {
            var language = AppUser.Language.ToEnum<LanguageType>();

            return await _helpRepository.GetAll(modalType, language);
        }

        [Route("get_by_type"), HttpGet]
        public async Task<Help> GetByType([FromQuery(Name = "templateType")]ModalType templateType, [FromQuery(Name = "moduleId")]int? moduleId = null, [FromQuery(Name = "route")]string route = "")
        {
            var language = AppUser.Language.ToEnum<LanguageType>();
            var templates = await _helpRepository.GetByType(templateType, language, moduleId, route);

            return templates;
        }

        [Route("get_module_type"), HttpGet]
        public async Task<Help> GetModuleType([FromQuery(Name = "templateType")]ModalType templateType, [FromQuery(Name = "moduleType")]ModuleType moduleType, [FromQuery(Name = "moduleId")]int? moduleId = null)
        {
            var language = AppUser.Language.ToEnum<LanguageType>();
            var templates = await _helpRepository.GetModuleType(templateType, moduleType, language, moduleId);

            return templates;
        }

        [Route("get_first_screen"), HttpGet]
        public async Task<Help> GetFistScreen([FromQuery(Name = "templateType")]ModalType templateType, [FromQuery(Name = "firstscreen")]bool? firstscreen = false)
        {
            var language = AppUser.Language.ToEnum<LanguageType>();
            var templates = await _helpRepository.GetFistScreen(templateType, language, firstscreen);

            return templates;
        }

        [Route("get_custom_help"), HttpGet]
        public async Task<ICollection<Help>> GetCustomHelp([FromQuery(Name = "templateType")]ModalType templateType, [FromQuery(Name = "customhelp")]bool? customhelp = false)
        {
            var language = AppUser.Language.ToEnum<LanguageType>();
            var templates = await _helpRepository.GetCustomHelp(templateType, language, customhelp);

            return templates;
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]HelpBindingModel help)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var helpEntity = HelpHelper.CreateEntity(help, _userRepository);
            var result = await _helpRepository.Create(helpEntity);

            if (result < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            var uri = new Uri(Request.GetDisplayUrl());
            return Created(uri.Scheme + "://" + uri.Authority + "/api/help/get/" + helpEntity.Id, helpEntity);
            //return Created(Request.Scheme + "://" + Request.Host + "/api/help/get/" + helpEntity.Id, helpEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]HelpBindingModel help)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var helpEntity = await _helpRepository.GetByIdBasic(id);

            if (helpEntity == null)
                return NotFound();

            HelpHelper.UpdateEntity(help, helpEntity, _userRepository);
            await _helpRepository.Update(helpEntity);

            return Ok(helpEntity);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var helpEntity = await _helpRepository.GetByIdBasic(id);

            if (helpEntity == null)
                return NotFound();

            await _helpRepository.DeleteSoft(helpEntity);

            return Ok();
        }
    }
}