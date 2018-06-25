using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeApps.App.ActionFilters;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Repositories.Interfaces;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
namespace PrimeApps.App.Controllers
{
    [Route("api/view"), Authorize/*, SnakeCase*/]
	public class ViewController : BaseController
    {
        private IViewRepository _viewRepository;
        private IUserRepository _userRepository;

	    private IRecordHelper _recordHelper;

        public ViewController(IViewRepository viewRepository, IUserRepository userRepository, IRecordHelper recordHelper)
        {
            _viewRepository = viewRepository;
            _userRepository = userRepository;
	        _recordHelper = recordHelper;
        }

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			SetContext(context);
			SetCurrentUser(_userRepository);
			SetCurrentUser(_viewRepository);

			base.OnActionExecuting(context);
		}

		[Route("get/{id:int}"), HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var viewEntity = await _viewRepository.GetById(id);

            if (viewEntity == null)
                return NotFound();

            var viewViewModel = ViewHelper.MapToViewModel(viewEntity);

            return Ok(viewViewModel);
        }

        [Route("get_all/{moduleId:int}"), HttpGet]
        public async Task<IActionResult> GetAll(int moduleId)
        {
            if (moduleId < 1)
                return BadRequest("Module id is required!");

            var viewEntities = await _viewRepository.GetAll(moduleId);
            var viewsViewModel = ViewHelper.MapToViewModel(viewEntities);

            return Ok(viewsViewModel);
        }

        [Route("get_all"), HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var viewEntities = await _viewRepository.GetAll();
            var viewsViewModel = ViewHelper.MapToViewModel(viewEntities);

            return Ok(viewsViewModel);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]ViewBindingModel view)
        {
            if (!_recordHelper.ValidateFilterLogic(view.FilterLogic, view.Filters))
                ModelState.AddModelError("request._filter_logic", "The field FilterLogic is invalid or has no filters.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var viewEntity = await ViewHelper.CreateEntity(view, _userRepository);
            var result = await _viewRepository.Create(viewEntity);

            if (result < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            var uri = new Uri(Request.GetDisplayUrl());
			return Created(uri.Scheme + "://" + uri.Authority + "/api/view/get/" + viewEntity.Id, viewEntity);
            //return Created(Request.Scheme + "://" + Request.Host + "/api/view/get/" + viewEntity.Id, viewEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]ViewBindingModel view)
        {
            var viewEntity = await _viewRepository.GetById(id);

            if (viewEntity == null)
                return NotFound();

            var currentFieldIds = viewEntity.Fields.Select(x => x.Id).ToList();
            var currentFilterIds = new List<int>();

            if (viewEntity.SystemType == SystemType.Custom)
                currentFilterIds = viewEntity.Filters.Select(x => x.Id).ToList();

            if (viewEntity.Shares != null && viewEntity.Shares.Count > 0)
            {
                var currentShares = viewEntity.Shares.ToList();

                foreach (ViewShares share in currentShares)
                {
                    await _viewRepository.DeleteViewShare(share, share.User);
                }
            }

            await ViewHelper.UpdateEntity(view, viewEntity, _userRepository);
            await _viewRepository.Update(viewEntity, currentFieldIds, currentFilterIds);

            return Ok(viewEntity);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var viewEntity = await _viewRepository.GetById(id);

            if (viewEntity == null)
                return NotFound();

            await _viewRepository.DeleteSoft(viewEntity);

            return Ok();
        }

        [Route("get_view_state/{moduleId:int}"), HttpGet]
        public async Task<IActionResult> GetViewState(int moduleId)
        {
            if (moduleId < 1)
                return BadRequest("Module id is required!");

            var viewState = await _viewRepository.GetViewState(moduleId, AppUser.Id);

            return Ok(viewState);
        }

        [Route("set_view_state"), HttpPut]
        public async Task<IActionResult> SetViewState([FromBody] ViewStateBindingModel viewState)
        {
            var viewStateEntity = await _viewRepository.GetViewState(viewState.ModuleId, AppUser.Id);

            if (!viewState.Id.HasValue && viewStateEntity == null)
            {
                var viewStateCreateEntity = ViewHelper.CreateEntityViewState(viewState, AppUser.Id);
                var resultCreate = await _viewRepository.CreateViewState(viewStateCreateEntity);

                if (resultCreate < 1)
                    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
                //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

                var uri = new Uri(Request.GetDisplayUrl());
				return Created(uri.Scheme + "://" + uri.Authority + "/api/view/get_view_state/" + viewStateCreateEntity.ModuleId, viewStateCreateEntity);
                //return Created(Request.Scheme + "://" + Request.Host + "/api/view/get_view_state/" + viewStateCreateEntity.ModuleId, viewStateCreateEntity);
            }

            ViewHelper.UpdateEntityViewState(viewState, viewStateEntity);
            await _viewRepository.UpdateViewState(viewStateEntity);

            return Ok(viewStateEntity);
        }
    }
}
