using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using PrimeApps.App.ActionFilters;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Entities;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Controllers
{
    [RoutePrefix("api/help"), Authorize, SnakeCase]
    public class HelpController : BaseController
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

        [Route("get/{id:int}"), HttpGet]
        public async Task<IHttpActionResult> GetById(int id)
        {
            var helpEntity = await _helpRepository.GetById(id);

            return Ok(helpEntity);
        }

        [Route("get_all"), HttpGet]
        public async Task<ICollection<Help>> GetAll([FromUri]ModalType modalType = ModalType.NotSet)
        {
            return await _helpRepository.GetAll(modalType);
        }

        [Route("get_by_type"), HttpGet]
        public async Task<Help> GetByType([FromUri]ModalType templateType, [FromUri]int? moduleId = null, [FromUri]string route = "")
        {
            var templates = await _helpRepository.GetByType(templateType, moduleId, route);

            return templates;
        }

        [Route("get_module_type"), HttpGet]
        public async Task<Help> GetModuleType([FromUri]ModalType templateType, [FromUri]ModuleType moduleType,[FromUri]int? moduleId = null)
        {
            var templates = await _helpRepository.GetModuleType(templateType, moduleType , moduleId);

            return templates;
        }

        [Route("get_first_screen"), HttpGet]
        public async Task<Help> GetFistScreen([FromUri]ModalType templateType, [FromUri]bool? firstscreen = false)
        {
            var templates = await _helpRepository.GetFistScreen(templateType, firstscreen);

            return templates;
        }

        [Route("get_custom_help"), HttpGet]
        public async Task<ICollection<Help>> GetCustomHelp([FromUri]ModalType templateType, [FromUri]bool? customhelp = false)
        {
            var templates = await _helpRepository.GetCustomHelp(templateType, customhelp);

            return templates;
        }

        [Route("create"), HttpPost]
        public async Task<IHttpActionResult> Create(HelpBindingModel help)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var helpEntity = await HelpHelper.CreateEntity(help, _userRepository);
            var result = await _helpRepository.Create(helpEntity);

            if (result < 1)
                throw new HttpResponseException(HttpStatusCode.InternalServerError);

            var uri = Request.RequestUri;
            return Created(uri.Scheme + "://" + uri.Authority + "/api/help/get/" + helpEntity.Id, helpEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IHttpActionResult> Update([FromUri]int id, [FromBody]HelpBindingModel help)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var helpEntity = await _helpRepository.GetByIdBasic(id);

            if (helpEntity == null)
                return NotFound();

            await HelpHelper.UpdateEntity(help, helpEntity, _userRepository);
            await _helpRepository.Update(helpEntity);

            return Ok(helpEntity);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IHttpActionResult> Delete([FromUri]int id)
        {
            var helpEntity = await _helpRepository.GetByIdBasic(id);

            if (helpEntity == null)
                return NotFound();

            await _helpRepository.DeleteSoft(helpEntity);

            return Ok();
        }

    }
}
