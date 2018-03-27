using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using PrimeApps.App.ActionFilters;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Controllers
{
    [RoutePrefix("api/user_group"), Authorize, SnakeCase]
    public class UserGroupController : BaseController
    {
        private IUserGroupRepository _userGroupRepository;
        private IUserRepository _userRepository;

        public UserGroupController(IUserGroupRepository userGroupRespository, IUserRepository userRespository)
        {
            _userGroupRepository = userGroupRespository;
            _userRepository = userRespository;
        }

        [Route("get/{id:int}"), HttpGet]
        public async Task<IHttpActionResult> Get(int id)
        {
            var userGroupEntity = await _userGroupRepository.GetById(id);

            if (userGroupEntity == null)
                return NotFound();

            return Ok(userGroupEntity);
        }

        [Route("get_all"), HttpGet]
        public async Task<ICollection<UserGroup>> GetAll()
        {
            return await _userGroupRepository.GetAll();
        }

        [Route("create"), HttpPost]
        public async Task<IHttpActionResult> Create(UserGroupBindingModel userGroup)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var users = await _userRepository.GetByIds(userGroup.UserIds);

            if (users.Count < userGroup.UserIds.Count)
                return BadRequest("User not found.");

            var userGroupEntity = UserGroupHelper.CreateEntity(userGroup, users);
            var result = await _userGroupRepository.Create(userGroupEntity);

            if (result < 1)
                throw new HttpResponseException(HttpStatusCode.InternalServerError);

            userGroupEntity = await _userGroupRepository.GetById(userGroupEntity.Id);

            var uri = Request.RequestUri;
            return Created(uri.Scheme + "://" + uri.Authority + "/api/user_group/get/" + userGroupEntity.Id, userGroupEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IHttpActionResult> Update([FromUri]int id, [FromBody]UserGroupBindingModel userGroup)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userGroupEntity = await _userGroupRepository.GetById(id);

            if (userGroupEntity == null)
                return NotFound();

            var users = await _userRepository.GetByIds(userGroup.UserIds);

            if (users.Count < userGroup.UserIds.Count)
                return BadRequest("User not found.");

            UserGroupHelper.UpdateEntity(userGroup, userGroupEntity, users);
            await _userGroupRepository.Update(userGroupEntity);

            return Ok(userGroupEntity);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IHttpActionResult> Delete([FromUri]int id)
        {
            var userGroupEntity = await _userGroupRepository.GetById(id);

            if (userGroupEntity == null)
                return NotFound();

            await _userGroupRepository.DeleteSoft(userGroupEntity);

            return Ok();
        }
    }
}
