using Newtonsoft.Json.Linq;
using PrimeApps.App.ActionFilters;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Entities;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace PrimeApps.App.Controllers
{
    [RoutePrefix("api/user_custom_shares"), Authorize, SnakeCase]
    public class UserCustomShareController : BaseController
    {
        private IUserCustomShareRepository _userOwnerRepository;

        public UserCustomShareController(IUserCustomShareRepository userOwnerRepository)
        {
            _userOwnerRepository = userOwnerRepository;
        }

        [Route("get_all"), HttpGet]
        public async Task<IHttpActionResult> GetAll()
        {
            var userOwnerEntities = await _userOwnerRepository.GetAllBasic();

            return Ok(userOwnerEntities);
        }

        [Route("get_by_userid/{id:int}"), HttpGet]
        public async Task<IHttpActionResult> GetByUserId(int id)
        {
            var userOwnerEntity = await _userOwnerRepository.GetByUserId(id);

            if (userOwnerEntity == null)
                return NotFound();

            return Ok(userOwnerEntity);
        }

        [Route("get/{id:int}"), HttpGet]
        public async Task<IHttpActionResult> GetById(int id)
        {
            var userOwnerEntity = await _userOwnerRepository.GetById(id);

            if (userOwnerEntity == null)
                return NotFound();

            return Ok(userOwnerEntity);
        }

        [Route("create"), HttpPost]
        public async Task<IHttpActionResult> Create(UserCustomShareBindingModels userOwner)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userOwnerEntity = UserCustomShareHelper.CreateEntity(userOwner, _userOwnerRepository);
            var result = await _userOwnerRepository.Create(userOwnerEntity);

            if (result < 1)
                throw new HttpResponseException(HttpStatusCode.InternalServerError);

            var uri = Request.RequestUri;
            return Created(uri.Scheme + "://" + uri.Authority + "/api/user_custom_shares/get/" + userOwnerEntity.Id, userOwnerEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IHttpActionResult> Update([FromUri]int id, [FromBody]UserCustomShareBindingModels userOwner)
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
        public async Task<IHttpActionResult> Delete([FromUri]int id)
        {
            var userOwnerEntity = await _userOwnerRepository.GetByIdBasic(id);

            if (userOwnerEntity == null)
                return NotFound();

            await _userOwnerRepository.DeleteSoft(userOwnerEntity);

            return Ok();
        }
    }
}