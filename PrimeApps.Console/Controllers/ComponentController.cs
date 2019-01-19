using PrimeApps.Console.Models;
using PrimeApps.Console.Helpers;
using PrimeApps.Model.Constants;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Http.Extensions;
using System.Linq;
using PrimeApps.Model.Common;
using PrimeApps.Model.Common.Organization;
using PrimeApps.Console.Constants;
using PrimeApps.Model.Common.Component;

namespace PrimeApps.Console.Controllers
{
    [Route("api/component")]
    public class ComponentController : DraftBaseController
    {
        private IComponentRepository _componentRepository;
        private IConfiguration _configuration;
        private Warehouse _warehouse;

        public ComponentController(IComponentRepository componentRepository, IConfiguration configuration)
        {
            _componentRepository = componentRepository;
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_componentRepository, PreviewMode, AppId, TenantId);

            base.OnActionExecuting(context);
        }

        [Route("count"), HttpGet]
        public async Task<IActionResult> Count()
        {
            var count = await _componentRepository.Count();

            return Ok(count);
        }

        [Route("find"), HttpPost]
        public async Task<IActionResult> Find([FromBody]PaginationModel paginationModel)
        {
            var components = await _componentRepository.Find(paginationModel); ;

            return Ok(components);
        }

        [Route("get/{id:int}"), HttpGet]
        public async Task<Component> Get(int id)
        {
            return await _componentRepository.Get(id);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody] ComponentModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var component = new Component
            {
                Name = model.Name,
                Content = model.Content,
                ModuleId = model.ModuleId,
                Type = model.Type,
                Place = model.Place,
                Order = model.Order
            };

            var result = await _componentRepository.Create(component);

            if (result < 0)
                return BadRequest("An error occurred while creating an component");

            return Ok(component.Id);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]ComponentModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var component = await _componentRepository.Get(id);

            if (component == null)
                return NotFound("Component not found!");

            component.Name = model.Name ?? component.Name;
            component.Content = model.Content ?? component.Content;
            component.ModuleId = model.ModuleId != 0 ? model.ModuleId : component.ModuleId;
            component.Type = model.Type != ComponentType.NotSet ? model.Type : component.Type;
            component.Place = model.Place != ComponentPlace.NotSet ? model.Place : component.Place;
            component.Order = model.Order != 0 ? model.Order : component.Order;

            await _componentRepository.Update(component);

            return Ok();
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var component = await _componentRepository.Get(id);

            if (component == null)
                return NotFound("Component not found!");

            await _componentRepository.Delete(component);

            return Ok();
        }
    }
}
