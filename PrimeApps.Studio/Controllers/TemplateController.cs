using System;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Models;
using PrimeApps.Studio.Storage;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
namespace PrimeApps.Studio.Controllers
{
	[Route("api/template"), Authorize]
	public class TemplateController : DraftBaseController
	{
		private readonly ITemplateRepository _templateRepostory;
		private readonly IUserRepository _userRepository;
		private readonly IRecordRepository _recordRepository;
		private readonly IModuleRepository _moduleRepository;
		private IConfiguration _configuration;
		private IPlatformRepository _platformRepository;
        private IPermissionHelper _permissionHelper;

        public TemplateController(ITemplateRepository templateRepostory, IUserRepository userRepository, IRecordRepository recordRepository, IModuleRepository moduleRepository, IConfiguration configuration, IPlatformRepository platformRepository, IPermissionHelper permissionHelper)
		{
			_templateRepostory = templateRepostory;
			_userRepository = userRepository;
			_recordRepository = recordRepository;
			_moduleRepository = moduleRepository;
			_configuration = configuration;
			_platformRepository = platformRepository;
            _permissionHelper = permissionHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
		{
			SetContext(context);
			SetCurrentUser(_userRepository, PreviewMode, TenantId, AppId);
			SetCurrentUser(_templateRepostory, PreviewMode, TenantId, AppId);
			SetCurrentUser(_platformRepository);
			base.OnActionExecuting(context);
		}

		[Route("get/{id:int}"), HttpGet]
		public async Task<IActionResult> Get(int id)
		{
            if (!_permissionHelper.CheckUserProfile(UserProfile, "template", RequestTypeEnum.View))
                return StatusCode(403);

            var template = await _templateRepostory.GetById(id);

			if (template == null)
				return NotFound();

			return Ok(template);
		}

		[Route("get_all"), HttpGet]
		public async Task<IActionResult> GetAll([FromUri]TemplateType templateType)//JObject obj)// = TemplateType.NotSet, [FromQuery(Name = "moduleName")]string moduleName = "")	
		{
            if (!_permissionHelper.CheckUserProfile(UserProfile, "template", RequestTypeEnum.View))
                return StatusCode(403);

            var templates = await _templateRepostory.GetAll(templateType);//, moduleName);

			return Ok(templates);
		}

		[Route("get_all_list"), HttpGet]
		public async Task<IActionResult> GetAllList(TemplateType type = TemplateType.NotSet, TemplateType excelType = TemplateType.NotSet, string moduleName = "")
		{
            if (!_permissionHelper.CheckUserProfile(UserProfile, "template", RequestTypeEnum.View))
                return StatusCode(403);

            var templates = await _templateRepostory.GetAllList(type, excelType, moduleName);

			return Ok(templates);
		}

		[Route("create"), HttpPost]
		public async Task<IActionResult> Create([FromBody]TemplateBindingModel template)
		{
            if (!_permissionHelper.CheckUserProfile(UserProfile, "template", RequestTypeEnum.Create))
                return StatusCode(403);

            if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var templateEntity = TemplateHelper.CreateEntity(template, _userRepository);
			var result = await _templateRepostory.Create(templateEntity);

			if (result < 1)
				throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());

			if (template.Chunks > 0)
				await AzureStorage.CommitFile(template.Content, $"templates/{template.Content}", template.ContentType, string.Format("inst-{0}", AppUser.TenantGuid), template.Chunks, _configuration);

			var uri = new Uri(Request.GetDisplayUrl());
			return Created(uri.Scheme + "://" + uri.Authority + "/api/template/get/" + templateEntity.Id, templateEntity);
		}

		[Route("create_excel"), HttpPost]
		public async Task<IActionResult> CreateExcel([FromBody]TemplateBindingModel template)
		{
            if (!_permissionHelper.CheckUserProfile(UserProfile, "template", RequestTypeEnum.Create))
                return StatusCode(403);

            if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var templateEntity = TemplateHelper.CreateEntityExcel(template, _userRepository);
			var result = await _templateRepostory.Create(templateEntity);

			if (result < 1)
				throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());

			if (template.Chunks > 0)
				await AzureStorage.CommitFile(template.Content, $"templates/{template.Content}", template.ContentType, string.Format("inst-{0}", AppUser.TenantGuid), template.Chunks, _configuration);

			return Created(Request.Scheme + "://" + Request.Host + "/api/template/get/" + templateEntity.Id, templateEntity);
		}

		[Route("update/{id:int}"), HttpPut]
		public async Task<IActionResult> Update(int id, [FromBody]TemplateBindingModel template)
		{
            if (!_permissionHelper.CheckUserProfile(UserProfile, "template", RequestTypeEnum.Update))
                return StatusCode(403);

            if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var templateEntity = await _templateRepostory.GetById(id);

			if (templateEntity == null)
				return NotFound();

			TemplateHelper.UpdateEntity(template, templateEntity, _userRepository, null, null);
			await _templateRepostory.Update(templateEntity);

			if (template.Chunks > 0)
				await AzureStorage.CommitFile(template.Content, $"templates/{template.Content}", template.ContentType, string.Format("inst-{0}", AppUser.TenantGuid), template.Chunks, _configuration);

			return Ok(templateEntity);
		}

		[Route("delete/{id:int}"), HttpDelete]
		public async Task<IActionResult> Delete(int id)
		{
            if (!_permissionHelper.CheckUserProfile(UserProfile, "template", RequestTypeEnum.Delete))
                return StatusCode(403);

            var templateEntity = await _templateRepostory.GetById(id);

			if (templateEntity == null)
				return NotFound();

			await _templateRepostory.DeleteSoft(templateEntity);

			return Ok();
		}

        [Route("count"), HttpGet]
        public IActionResult Count([FromUri]TemplateType templateType)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "template", RequestTypeEnum.View))
                return StatusCode(403);

            var count = _templateRepostory.Count(templateType);
            return Ok(count);
        }

        [Route("find"), HttpPost]
		public async Task<IActionResult> Find([FromBody]PaginationModel paginationModel, [FromUri]TemplateType templateType = 0)
		{
            if (!_permissionHelper.CheckUserProfile(UserProfile, "template", RequestTypeEnum.View))
                return StatusCode(403);

            var templates = await _templateRepostory.Find(paginationModel, templateType);
			return Ok(templates);

		}

		[Route("create_app_email_template"), HttpPost]
		public async Task<IActionResult> CreateAppEmailTemplate([FromBody]AppTemplateBindingModel template, [FromUri]string currentAppName)
		{
            if (!_permissionHelper.CheckUserProfile(UserProfile, "template", RequestTypeEnum.Create))
                return StatusCode(403);

            if (!ModelState.IsValid)
				return BadRequest(ModelState);
			var app = await _platformRepository.AppGetByName(currentAppName.ToLower());
			var templateEntity = TemplateHelper.CreateEntityAppTemplate(template, app.Id);
			var result = await _platformRepository.CreateAppTemplate(templateEntity);

			if (result < 1)
				throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());

			var uri = new Uri(Request.GetDisplayUrl());
			return Created(uri.Scheme + "://" + uri.Authority + "/api/template/get/" + templateEntity.Id, templateEntity);
		}

		[Route("update_app_email_template/{id:int}"), HttpPut]
		public async Task<IActionResult> UpdateAppEmailTemplate(int id, [FromBody]AppTemplateBindingModel template)
		{
            if (!_permissionHelper.CheckUserProfile(UserProfile, "template", RequestTypeEnum.Update))
                return StatusCode(403);

            if (!ModelState.IsValid)
				return BadRequest(ModelState);

			if (!template.Deleted)
			{
				var templateEntity = await _platformRepository.GetAppTemplateById(id);

				if (templateEntity == null)
					return NotFound();

				TemplateHelper.UpdateEntity(null, null, null, template, templateEntity, true);
				await _platformRepository.UpdateAppTemplate(templateEntity);
				return Ok(templateEntity);
			}
			else
				return BadRequest("Deleted value can't be true");
		}

		[Route("count_app_email_template"), HttpGet]
		public async Task<IActionResult> CountAppTemplate([FromUri]string currentAppName)
		{
            if (!_permissionHelper.CheckUserProfile(UserProfile, "template", RequestTypeEnum.View))
                return StatusCode(403);

            var app = await _platformRepository.AppGetByName(currentAppName.ToLower());
			if (app == null)
			{
				return Ok(0);
			}
			var count = _platformRepository.Count(app.Id);
			return Ok(count);
		}

		[Route("find_app_email_template"), HttpPost]
		public async Task<IActionResult> FindAppTemplate([FromBody]PaginationModel paginationModel, [FromUri]string currentAppName)
		{
            if (!_permissionHelper.CheckUserProfile(UserProfile, "template", RequestTypeEnum.View))
                return StatusCode(403);

            var app = await _platformRepository.AppGetByName(currentAppName.ToLower());
			if (app == null)
			{
				return Ok();
			}
			var templates = await _platformRepository.Find(paginationModel, app.Id);
			return Ok(templates);
		}
	}
}