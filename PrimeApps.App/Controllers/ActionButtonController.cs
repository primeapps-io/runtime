using PrimeApps.App.ActionFilters;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Repositories.Interfaces;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PrimeApps.App.Controllers
{
    [Route("api/action_button"), Authorize, SnakeCase]

    public class ActionButtonController : BaseController
    {
        private IActionButtonRepository _actionButtonRepository;

        public ActionButtonController(IActionButtonRepository actionButtonRepository)
        {
            _actionButtonRepository = actionButtonRepository;
        }
        [Route("get/{id:int}"), HttpGet]
        public async Task<IActionResult> GetActionButtons(int id)
        {
            var buttons = await _actionButtonRepository.GetByModuleId(id);

            if (buttons == null)
                return NotFound();

            return Ok(buttons);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create(ActionButtonBindingModel actionbutton)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var actionButtonEntity = ActionButtonHelper.CreateEntity(actionbutton);
            var result = await _actionButtonRepository.Create(actionButtonEntity);

            if (result < 1)
                throw new HttpResponseException(HttpStatusCode.InternalServerError);

            var uri = Request.RequestUri;
            return Created(uri.Scheme + "://" + uri.Authority + "/api/action_button/get/" + actionButtonEntity.Id, actionButtonEntity);
        }


        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update([FromRoute]int id, [FromBody]ActionButtonBindingModel actionbutton)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var actionButtonEntity = await _actionButtonRepository.GetById(id);

            if (actionButtonEntity == null)
                return NotFound();

            ActionButtonHelper.UpdateEntity(actionbutton, actionButtonEntity);
            await _actionButtonRepository.Update(actionButtonEntity);

            return Ok(actionButtonEntity);
        }


        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete([FromRoute]int id)
        {
            var actionButtonEntity = await _actionButtonRepository.GetByIdBasic(id);

            if (actionButtonEntity == null)
                return NotFound();

            await _actionButtonRepository.DeleteSoft(actionButtonEntity);

            return Ok();
        }
    }
}