using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Models;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;

namespace PrimeApps.Studio.Controllers
{
	[Route("api/help")]
	public class HelpController : DraftBaseController
	{
		private IRelationRepository _relationRepository;
        private IHelpRepository _helpRepository;
        private IUserRepository _userRepository;
        private IProfileRepository _profileRepository;
		private ISettingRepository _settingRepository; 
		private IConfiguration _configuration;
		private Warehouse _warehouse;
		private IModuleHelper _moduleHelper;
        private IPermissionHelper _permissionHelper;

        public HelpController(IRelationRepository relationRepository, IProfileRepository profileRepository, ISettingRepository settingRepository, IModuleRepository moduleRepository, Warehouse warehouse, IModuleHelper moduleHelper, IConfiguration configuration,IHelpRepository helpRepository,IUserRepository userRepository, IPermissionHelper permissionHelper)
		{
			_relationRepository = relationRepository;
			_profileRepository = profileRepository;
			_settingRepository = settingRepository;			
			_warehouse = warehouse;
			_configuration = configuration;
			_moduleHelper = moduleHelper;
            _helpRepository = helpRepository;
            _userRepository = userRepository;
            _permissionHelper = permissionHelper;
        }

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			SetContext(context);
			SetCurrentUser(_relationRepository, PreviewMode, AppId, TenantId);
			SetCurrentUser(_profileRepository, PreviewMode, AppId, TenantId);
			SetCurrentUser(_settingRepository, PreviewMode, AppId, TenantId);
			SetCurrentUser(_helpRepository, PreviewMode, AppId, TenantId);

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
            return await _helpRepository.GetAll(modalType);
        }

        [Route("count"), HttpGet]
        public async Task<IActionResult> Count()
        {
            var count = await _helpRepository.Count();
           
            return Ok(count);
        }

        [Route("find"), HttpPost]
        public async Task<IActionResult> Find([FromBody]PaginationModel paginationModel)
        {
            var helps = await _helpRepository.Find(paginationModel);

            return Ok(helps);
        }

        [Route("get_by_type"), HttpGet]
        public async Task<Help> GetByType([FromQuery(Name = "templateType")]ModalType templateType, [FromQuery(Name = "moduleId")]int? moduleId = null, [FromQuery(Name = "route")]string route = "")
        {
            var templates = await _helpRepository.GetByType(templateType, moduleId, route);

            return templates;
        }

        [Route("get_module_type"), HttpGet]
        public async Task<Help> GetModuleType([FromQuery(Name = "templateType")]ModalType templateType, [FromQuery(Name = "moduleType")]ModuleType moduleType, [FromQuery(Name = "moduleId")]int? moduleId = null)
        {
            var templates = await _helpRepository.GetModuleType(templateType, moduleType, moduleId);

            return templates;
        }

        [Route("get_first_screen"), HttpGet]
        public async Task<Help> GetFistScreen([FromQuery(Name = "templateType")]ModalType templateType, [FromQuery(Name = "firstscreen")]bool? firstscreen = false)
        {
            var templates = await _helpRepository.GetFistScreen(templateType, firstscreen);

            return templates;
        }

        [Route("get_custom_help"), HttpGet]
        public async Task<ICollection<Help>> GetCustomHelp([FromQuery(Name = "templateType")]ModalType templateType, [FromQuery(Name = "customhelp")]bool? customhelp = false)
        {
            var templates = await _helpRepository.GetCustomHelp(templateType, customhelp);

            return templates;
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]HelpBindingModel help)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "help", RequestTypeEnum.Create))
                return StatusCode(403);

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
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "help", RequestTypeEnum.Update))
                return StatusCode(403);

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
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "help", RequestTypeEnum.Delete))
                return StatusCode(403);

            var helpEntity = await _helpRepository.GetByIdBasic(id);

            if (helpEntity == null)
                return NotFound();

            await _helpRepository.DeleteSoft(helpEntity);

            return Ok();
        }

    }
}
