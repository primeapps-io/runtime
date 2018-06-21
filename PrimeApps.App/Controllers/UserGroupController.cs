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
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Repositories.Interfaces;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
namespace PrimeApps.App.Controllers
{
    [Route("api/user_group"), Authorize]
	public class UserGroupController : ApiBaseController
    {
        private IUserGroupRepository _userGroupRepository;
        private IUserRepository _userRepository;

        public UserGroupController(IUserGroupRepository userGroupRespository, IUserRepository userRespository)
        {
            _userGroupRepository = userGroupRespository;
            _userRepository = userRespository;
        }

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			SetContext(context);
			SetCurrentUser(_userRepository);
			SetCurrentUser(_userGroupRepository);

			base.OnActionExecuting(context);
		}

		[Route("get/{id:int}"), HttpGet]
        public async Task<IActionResult> Get(int id)
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
        public async Task<IActionResult> Create([FromBody]UserGroupBindingModel userGroup)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var users = await _userRepository.GetByIds(userGroup.UserIds);

            if (users.Count < userGroup.UserIds.Count)
                return BadRequest("User not found.");

            var userUserGroup = new List<TenantUserGroup>();
            foreach (var user in users)
            {
                if (user != null)
                    userUserGroup.Add(new TenantUserGroup{ UserId = user.Id });
            }

            var userGroupEntity = UserGroupHelper.CreateEntity(userGroup, userUserGroup);
            var result = await _userGroupRepository.Create(userGroupEntity);

            if (result < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            userGroupEntity = await _userGroupRepository.GetById(userGroupEntity.Id);

            var uri = new Uri(Request.GetDisplayUrl());
			return Created(uri.Scheme + "://" + uri.Authority + "/api/user_group/get/" + userGroupEntity.Id, userGroupEntity);
            //return Created(Request.Scheme + "://" + Request.Host + "/api/user_group/get/" + userGroupEntity.Id, userGroupEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]UserGroupBindingModel userGroup)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userGroupEntity = await _userGroupRepository.GetById(id);

            if (userGroupEntity == null)
                return NotFound();

            var users = await _userRepository.GetByIds(userGroup.UserIds);

            if (users.Count < userGroup.UserIds.Count)
                return BadRequest("User not found.");

            var userUserGroup = new List<TenantUserGroup>();
            foreach (var user in users)
            {
                if (user != null)
                    userUserGroup.Add(new TenantUserGroup { UserId = user.Id });
            }

            UserGroupHelper.UpdateEntity(userGroup, userGroupEntity, userUserGroup);
            await _userGroupRepository.Update(userGroupEntity);

            return Ok(userGroupEntity);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var userGroupEntity = await _userGroupRepository.GetById(id);

            if (userGroupEntity == null)
                return NotFound();

            await _userGroupRepository.DeleteSoft(userGroupEntity);

            return Ok();
        }
    }
}
