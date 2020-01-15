using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Storage;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;

namespace PrimeApps.App.Controllers
{
    [Route("api/template"), Authorize]
    public class TemplateController : ApiBaseController
    {
        private readonly ITemplateRepository _templateRepostory;
        private readonly IUserRepository _userRepository;
        private readonly IRecordRepository _recordRepository;
        private readonly IModuleRepository _moduleRepository;
        private IConfiguration _configuration;

        public TemplateController(ITemplateRepository templateRepostory, IUserRepository userRepository, IRecordRepository recordRepository, IModuleRepository moduleRepository, IConfiguration configuration)
        {
            _templateRepostory = templateRepostory;
            _userRepository = userRepository;
            _recordRepository = recordRepository;
            _moduleRepository = moduleRepository;
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_userRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_templateRepostory, PreviewMode, TenantId, AppId);

            base.OnActionExecuting(context);
        }

        [Route("get/{id:int}"), HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var template = await _templateRepostory.GetById(id);

            if (template == null)
                return NotFound();

            return Ok(template);
        }

        [Route("get_all"), HttpGet]
        public async Task<IActionResult> GetAll([FromQuery(Name = "type")]TemplateType type = TemplateType.NotSet, [FromQuery(Name = "moduleName")]string moduleName = "")
        {
            var language = AppUser.Language.ToEnum<LanguageType>();
            var templates = await _templateRepostory.GetAll(type, language, true, moduleName);

            return Ok(templates);
        }

        [Route("get_all_list"), HttpGet]
        public async Task<IActionResult> GetAllList(TemplateType type = TemplateType.NotSet, TemplateType excelType = TemplateType.NotSet, string moduleName = "")
        {
            var language = AppUser.Language.ToEnum<LanguageType>();
            var templates = await _templateRepostory.GetAllList(language, type, excelType, moduleName);

            return Ok(templates);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]TemplateBindingModel template)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (template.Language == LanguageType.NotSet)
                template.Language = AppUser.Language.ToEnum<LanguageType>();

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
        public async Task<IActionResult> CreateExcel(TemplateBindingModel template)
        {
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var templateEntity = await _templateRepostory.GetById(id);

            if (templateEntity == null)
                return NotFound();

            if (templateEntity.SystemType == SystemType.System && templateEntity.TemplateType == TemplateType.Email)
                return Forbid();

            if (template.Language == LanguageType.NotSet)
                template.Language = AppUser.Language.ToEnum<LanguageType>();

            TemplateHelper.UpdateEntity(template, templateEntity, _userRepository);
            await _templateRepostory.Update(templateEntity);

            if (template.Chunks > 0)
                await AzureStorage.CommitFile(template.Content, $"templates/{template.Content}", template.ContentType, string.Format("inst-{0}", AppUser.TenantGuid), template.Chunks, _configuration);

            return Ok(templateEntity);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var templateEntity = await _templateRepostory.GetById(id);

            if (templateEntity == null)
                return NotFound();

            if (templateEntity.SystemType == SystemType.System && templateEntity.TemplateType == TemplateType.Email)
                return Forbid();

            await _templateRepostory.DeleteSoft(templateEntity);

            return Ok();
        }
    }
}