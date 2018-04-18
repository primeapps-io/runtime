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
namespace PrimeApps.App.Controllers
{
    [Route("api/user_custom_shares"), Authorize/*, SnakeCase*/]
	public class UserCustomShareController : BaseController
    {
        private IUserCustomShareRepository _userOwnerRepository;

        public UserCustomShareController(IUserCustomShareRepository userOwnerRepository)
        {
            _userOwnerRepository = userOwnerRepository;
        }

        [Route("get_all"), HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userOwnerEntities = await _userOwnerRepository.GetAllBasic();

            return Ok(userOwnerEntities);
        }

        [Route("get_by_userid/{id:int}"), HttpGet]
        public async Task<IActionResult> GetByUserId(int id)
        {
            var userOwnerEntity = await _userOwnerRepository.GetByUserId(id);

            if (userOwnerEntity == null)
                return NotFound();

            return Ok(userOwnerEntity);
        }

        [Route("get/{id:int}"), HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var userOwnerEntity = await _userOwnerRepository.GetById(id);

            if (userOwnerEntity == null)
                return NotFound();

            return Ok(userOwnerEntity);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create(UserCustomShareBindingModels userOwner)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userOwnerEntity = UserCustomShareHelper.CreateEntity(userOwner, _userOwnerRepository);
            var result = await _userOwnerRepository.Create(userOwnerEntity);

            if (result < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            var uri = new Uri(Request.GetDisplayUrl());
			return Created(uri.Scheme + "://" + uri.Authority + "/api/user_custom_shares/get/" + userOwnerEntity.Id, userOwnerEntity);
            //return Created(Request.Scheme + "://" + Request.Host + "/api/user_custom_shares/get/" + userOwnerEntity.Id, userOwnerEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update([FromRoute]int id, [FromBody]UserCustomShareBindingModels userOwner)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userOwnerEntity = await _userOwnerRepository.GetByIdBasic(id);

            if (userOwnerEntity == null)
                return NotFound();

            UserCustomShareHelper.UpdateEntity(userOwner, userOwnerEntity, _userOwnerRepository);
            await _userOwnerRepository.Update(userOwnerEntity);

            return Ok(userOwnerEntity);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete([FromRoute]int id)
        {
            var userOwnerEntity = await _userOwnerRepository.GetByIdBasic(id);

            if (userOwnerEntity == null)
                return NotFound();

            await _userOwnerRepository.DeleteSoft(userOwnerEntity);

            return Ok();
        }
    }
}