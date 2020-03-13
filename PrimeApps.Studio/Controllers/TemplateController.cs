using System;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Models;
using PrimeApps.Model.Storage;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.AspNet.OData.Query;
using PrimeApps.Model.Entities.Studio;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using PrimeApps.Model.Entities.Tenant;
using System.Collections.Generic;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/template"), Authorize]
    public class TemplateController : DraftBaseController
    {
        private readonly ITemplateRepository _templateRepostory;
        private readonly IUserRepository _userRepository;
        private readonly IRecordRepository _recordRepository;
        private readonly IModuleRepository _moduleRepository;
        private readonly IAppDraftRepository _appDraftRepository;
        private readonly IAppDraftTemplateRepository _appDraftTemplateRepository;
        private IConfiguration _configuration;
        private IPlatformRepository _platformRepository;
        private IPermissionHelper _permissionHelper;

        public TemplateController(ITemplateRepository templateRepostory,
            IAppDraftTemplateRepository appDraftTemplateRepository, IUserRepository userRepository,
            IRecordRepository recordRepository, IModuleRepository moduleRepository, IConfiguration configuration,
            IPlatformRepository platformRepository, IPermissionHelper permissionHelper,
            IAppDraftRepository appDraftRepository)
        {
            _templateRepostory = templateRepostory;
            _userRepository = userRepository;
            _recordRepository = recordRepository;
            _moduleRepository = moduleRepository;
            _configuration = configuration;
            _platformRepository = platformRepository;
            _permissionHelper = permissionHelper;
            _appDraftRepository = appDraftRepository;
            _appDraftTemplateRepository = appDraftTemplateRepository;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_userRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_templateRepostory, PreviewMode, TenantId, AppId);
            SetCurrentUser(_appDraftTemplateRepository);
            SetCurrentUser(_platformRepository);
            SetCurrentUser(_appDraftRepository);
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
        public async Task<IActionResult>
            GetAll([FromUri] TemplateType templateType) //JObject obj)// = TemplateType.NotSet, [FromQuery(Name = "moduleName")]string moduleName = "")	
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "template", RequestTypeEnum.View))
                return StatusCode(403);

            var templates = await _templateRepostory.GetAll(templateType, LanguageType.NotSet, false); //, moduleName);

            return Ok(templates);
        }

        [Route("get_all_list"), HttpGet]
        public async Task<IActionResult> GetAllList(TemplateType type = TemplateType.NotSet,
            TemplateType excelType = TemplateType.NotSet, string moduleName = "")
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "template", RequestTypeEnum.View))
                return StatusCode(403);

            var templates = await _templateRepostory.GetAllList(LanguageType.NotSet, type, excelType, moduleName);

            return Ok(templates);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody] TemplateBindingModel template)
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
                await AzureStorage.CommitFile(template.Content, $"templates/{template.Content}", template.ContentType,
                    string.Format("inst-{0}", AppUser.TenantGuid), template.Chunks, _configuration);

            var uri = new Uri(Request.GetDisplayUrl());
            return Created(uri.Scheme + "://" + uri.Authority + "/api/template/get/" + templateEntity.Id,
                templateEntity);
        }

        [Route("create_excel"), HttpPost]
        public async Task<IActionResult> CreateExcel([FromBody] TemplateBindingModel template)
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
                await AzureStorage.CommitFile(template.Content, $"templates/{template.Content}", template.ContentType,
                    string.Format("inst-{0}", AppUser.TenantGuid), template.Chunks, _configuration);

            return Created(Request.Scheme + "://" + Request.Host + "/api/template/get/" + templateEntity.Id,
                templateEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody] TemplateBindingModel template)
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
                await AzureStorage.CommitFile(template.Content, $"templates/{template.Content}", template.ContentType,
                    string.Format("inst-{0}", AppUser.TenantGuid), template.Chunks, _configuration);

            return Ok(templateEntity);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id, [FromQuery]bool isAppTemplate = false)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "template", RequestTypeEnum.Delete))
                return StatusCode(403);

            if (isAppTemplate)
            {
                var templateEntity = await _appDraftTemplateRepository.Get(id);

                if (templateEntity == null)
                    return NotFound();

                if (templateEntity.SystemCode == "email_confirm" || templateEntity.SystemCode == "password_reset")
                    return Forbid();

                await _appDraftTemplateRepository.DeleteSoft(templateEntity);

                return Ok();
            }
            else
            {
                var templateEntity = await _templateRepostory.GetById(id);

                if (templateEntity == null)
                    return NotFound();

                await _templateRepostory.DeleteSoft(templateEntity);

                return Ok();
            }

        }

        [Route("count"), HttpGet]
        public IActionResult Count([FromUri] TemplateType templateType)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "template", RequestTypeEnum.View))
                return StatusCode(403);

            var count = _templateRepostory.Count(templateType);
            return Ok(count);
        }

        [Route("find")]
        public IActionResult Find(ODataQueryOptions<Template> queryOptions, [FromUri]TemplateType templateType = 0)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "template", RequestTypeEnum.View))
                return StatusCode(403);

            var temps = _templateRepostory.Find(templateType);

            var queryResults = (IQueryable<Template>)queryOptions.ApplyTo(temps, new ODataQuerySettings() { EnsureStableOrdering = false });
            return Ok(new PageResult<Template>(queryResults, Request.ODataFeature().NextLink, Request.ODataFeature().TotalCount));
        }

        [Route("create_app_email_template"), HttpPost]
        public async Task<IActionResult> CreateAppEmailTemplate([FromBody] AppTemplateBindingModel template)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "template", RequestTypeEnum.Create))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var templateEntity = TemplateHelper.CreateEntityAppTemplate(template, template.AppId);
            var result = await _appDraftTemplateRepository.Create(templateEntity);

            if (result < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());

            var settings = JsonConvert.DeserializeObject<JObject>(templateEntity.Settings);
            settings["id"] = templateEntity.Id;

            templateEntity.Settings = JsonConvert.SerializeObject(settings);
            await _appDraftTemplateRepository.Update(templateEntity);

            var uri = new Uri(Request.GetDisplayUrl());
            return Created(uri.Scheme + "://" + uri.Authority + "/api/template/get/" + templateEntity.Id,
                templateEntity);
        }

        [Route("update_app_email_template/{id:int}"), HttpPut]
        public async Task<IActionResult> UpdateAppEmailTemplate(int id, [FromBody] AppTemplateBindingModel template)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "template", RequestTypeEnum.Update))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var templateEntity = await _appDraftTemplateRepository.Get(id);

            if (templateEntity == null)
                return NotFound();

            TemplateHelper.UpdateEntity(null, null, null, template, templateEntity, true);
            await _appDraftTemplateRepository.Update(templateEntity);
            return Ok(templateEntity);
        }

        [Route("count_app_email_template"), HttpGet]
        public async Task<IActionResult> CountAppTemplate([FromUri] string currentAppName)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "template", RequestTypeEnum.View))
                return StatusCode(403);

            var app = await _appDraftRepository.GetByName(currentAppName.ToLower());
            var count = app != null ? _appDraftTemplateRepository.Count(app.Id) : 0;

            return Ok(count);
        }

        [Route("find_app_email_template")]
        public IActionResult FindAppEmailTemplate(ODataQueryOptions<AppDraftTemplate> queryOptions, [FromUri]int appId)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "template", RequestTypeEnum.View))
                return StatusCode(403);

            var views = _appDraftTemplateRepository.Find(appId);
            var queryResults = (IQueryable<AppDraftTemplate>)queryOptions.ApplyTo(views, new ODataQuerySettings() { EnsureStableOrdering = false });
            return Ok(new PageResult<AppDraftTemplate>(queryResults, Request.ODataFeature().NextLink, Request.ODataFeature().TotalCount));
        }

        [Route("get_all_by_app_id"), HttpGet]
        public async Task<IActionResult> GetAllByAppId()
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "template", RequestTypeEnum.View))
                return StatusCode(403);

            var templates = await _appDraftTemplateRepository.GetAll((int)AppId);

            return Ok(templates);
        }
    }
}