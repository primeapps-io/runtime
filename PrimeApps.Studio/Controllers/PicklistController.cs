using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeApps.Model.Common;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Models;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
namespace PrimeApps.Studio.Controllers
{
    [Route("api/picklist"), Authorize]
    public class PicklistController : DraftBaseController
    {
        private IPicklistRepository _picklistRepository;

        public PicklistController(IPicklistRepository picklistRepository)
        {
            _picklistRepository = picklistRepository;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_picklistRepository, PreviewMode, AppId, TenantId);

            base.OnActionExecuting(context);
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

        //[Route("find"), HttpPost]
        //public async Task<IActionResult> Find([FromBody]List<int> ids)
        //{
        //	var picklistEntities = await _picklistRepository.Find(ids);
        //	var picklistsViewModel = PicklistHelper.MapToViewModel(picklistEntities);

        //	return Ok(picklistsViewModel);
        //}

        [Route("find"), HttpPost]
        public async Task<IActionResult> Find([FromBody]PaginationModel paginationModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var picklists = await _picklistRepository.Find(paginationModel);

            return Ok(picklists);
        }

        [Route("count"), HttpGet]
        public async Task<int> Count()
        {
            var count = await _picklistRepository.Count();

            return count;
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]PicklistBindingModel picklist)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var picklistEntity = PicklistHelper.CreateEntity(picklist);
            var result = await _picklistRepository.Create(picklistEntity);

            if (result < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            var uri = new Uri(Request.GetDisplayUrl());
            return Created(uri.Scheme + "://" + uri.Authority + "/api/picklist/get/" + picklistEntity.Id, picklistEntity);
            //return Created(Request.Scheme + "://" + Request.Host + "/api/picklist/get/" + picklistEntity.Id, picklistEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]PicklistBindingModel picklist)
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
        public async Task<IActionResult> Delete(int id)
        {
            var picklistEntity = await _picklistRepository.GetById(id);

            if (picklistEntity == null)
                return NotFound();

            await _picklistRepository.DeleteSoft(picklistEntity);

            return Ok();
        }
    }
}
