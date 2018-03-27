using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.App.ActionFilters;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Controllers
{
    [Route("api/template"), Authorize, SnakeCase]
    public class TemplateController : BaseController
    {
        private readonly ITemplateRepostory _templateRepostory;
        private readonly IUserRepository _userRepository;

        public TemplateController(ITemplateRepostory templateRepostory, IUserRepository userRepository)
        {
            _templateRepostory = templateRepostory;
            _userRepository = userRepository;
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
        public async Task<IActionResult> GetAll(TemplateType type = TemplateType.NotSet, string moduleName = "")
        {
            var templates = await _templateRepostory.GetAll(type, moduleName);

            return Ok(templates);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create(TemplateBindingModel template)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var templateEntity = await TemplateHelper.CreateEntity(template, _userRepository);
            var result = await _templateRepostory.Create(templateEntity);

            if (result < 1)
                throw new HttpResponseException(HttpStatusCode.InternalServerError);

            if (template.Chunks > 0)
                Storage.CommitFile(template.Content, $"templates/{template.Content}", template.ContentType, string.Format("inst-{0}", AppUser.TenantGuid), template.Chunks);

            var uri = Request.RequestUri;
            return Created(uri.Scheme + "://" + uri.Authority + "/api/template/get/" + templateEntity.Id, templateEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update([FromRoute]int id, [FromBody]TemplateBindingModel template)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var templateEntity = await _templateRepostory.GetById(id);

            if (templateEntity == null)
                return NotFound();

            await TemplateHelper.UpdateEntity(template, templateEntity, _userRepository);
            await _templateRepostory.Update(templateEntity);

            if (template.Chunks > 0)
                Storage.CommitFile(template.Content, $"templates/{template.Content}", template.ContentType, string.Format("inst-{0}", AppUser.TenantGuid), template.Chunks);

            return Ok(templateEntity);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete([FromRoute]int id)
        {
            var templateEntity = await _templateRepostory.GetById(id);

            if (templateEntity == null)
                return NotFound();

            await _templateRepostory.DeleteSoft(templateEntity);

            return Ok();
        }
    }
}