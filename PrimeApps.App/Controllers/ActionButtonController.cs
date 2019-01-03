using System;
using PrimeApps.App.ActionFilters;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Repositories.Interfaces;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PrimeApps.App.Controllers
{
    [Route("api/action_button"), Authorize]

	public class ActionButtonController : ApiBaseController
    {
        private IActionButtonRepository _actionButtonRepository;

        public ActionButtonController(IActionButtonRepository actionButtonRepository)
        {
            _actionButtonRepository = actionButtonRepository;
        }

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			SetContext(context);
			SetCurrentUser(_actionButtonRepository, DBMode, TenantId, AppId);

			base.OnActionExecuting(context);
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
        public async Task<IActionResult> Create([FromBody]ActionButtonBindingModel actionbutton)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var actionButtonEntity = ActionButtonHelper.CreateEntity(actionbutton);
            var result = await _actionButtonRepository.Create(actionButtonEntity);

            if (result < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            var uri = new Uri(Request.GetDisplayUrl());
			//return Created(uri.Scheme + "://" + uri.Authority + "/api/action_button/get/" + actionButtonEntity.Id, actionButtonEntity);
			return Created(uri.Scheme + "://" + uri.Authority + "/api/action_button/get/" + actionButtonEntity.Id, actionButtonEntity);
        }


        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]ActionButtonBindingModel actionbutton)
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
        public async Task<IActionResult> Delete(int id)
        {
            var actionButtonEntity = await _actionButtonRepository.GetByIdBasic(id);

            if (actionButtonEntity == null)
                return NotFound();

            await _actionButtonRepository.DeleteSoft(actionButtonEntity);

            return Ok();
        }
    }
}