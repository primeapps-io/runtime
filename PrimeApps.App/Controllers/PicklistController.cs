using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.App.ActionFilters;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Controllers
{
    [Route("api/picklist"), Authorize, SnakeCase]
    public class PicklistController : BaseController
    {
        private IPicklistRepository _picklistRepository;

        public PicklistController(IPicklistRepository picklistRepository)
        {
            _picklistRepository = picklistRepository;
        }

        [Route("get/{id:int}"), HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var picklistEntity = await _picklistRepository.GetById(id);

            if (picklistEntity == null)
                return NotFound();

            var picklistViewModel = PicklistHelper.MapToViewModel(picklistEntity);

            return Ok(picklistViewModel);
        }

        [Route("get_all"), HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var picklistEntities = await _picklistRepository.GetAll();

            return Ok(picklistEntities);
        }

        [Route("find"), HttpPost]
        public async Task<IActionResult> Find(List<int> ids)
        {
            var picklistEntities = await _picklistRepository.Find(ids);
            var picklistsViewModel = PicklistHelper.MapToViewModel(picklistEntities);

            return Ok(picklistsViewModel);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create(PicklistBindingModel picklist)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var picklistEntity = PicklistHelper.CreateEntity(picklist);
            var result = await _picklistRepository.Create(picklistEntity);

            if (result < 1)
                throw new HttpResponseException(HttpStatusCode.InternalServerError);

            var uri = Request.RequestUri;
            return Created(uri.Scheme + "://" + uri.Authority + "/api/picklist/get/" + picklistEntity.Id, picklistEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update([FromRoute]int id, [FromBody]PicklistBindingModel picklist)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var picklistEntity = await _picklistRepository.GetById(id);

            if (picklistEntity == null)
                return NotFound();

            PicklistHelper.UpdateEntity(picklist, picklistEntity);
            await _picklistRepository.Update(picklistEntity);

            return Ok(picklistEntity);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete([FromRoute]int id)
        {
            var picklistEntity = await _picklistRepository.GetById(id);

            if (picklistEntity == null)
                return NotFound();

            await _picklistRepository.DeleteSoft(picklistEntity);

            return Ok();
        }
    }
}
