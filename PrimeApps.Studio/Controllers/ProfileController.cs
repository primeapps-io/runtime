using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common;
using PrimeApps.Model.Common.Profile;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/profile")]
    public class ProfileController : DraftBaseController
    {
        private IRelationRepository _relationRepository;
        private IUserRepository _userRepository;
        private IProfileRepository _profileRepository;
        private ISettingRepository _settingRepository;
        private IConfiguration _configuration;
        private Warehouse _warehouse;
        private IModuleHelper _moduleHelper;
        private IPermissionHelper _permissionHelper;

        public ProfileController(IRelationRepository relationRepository, IProfileRepository profileRepository, ISettingRepository settingRepository, Warehouse warehouse, IModuleHelper moduleHelper, IConfiguration configuration, IHelpRepository helpRepository, IUserRepository userRepository, IPermissionHelper permissionHelper)
        {
            _relationRepository = relationRepository;
            _profileRepository = profileRepository;
            _settingRepository = settingRepository;
            _warehouse = warehouse;
            _configuration = configuration;
            _moduleHelper = moduleHelper;
            _userRepository = userRepository;
            _permissionHelper = permissionHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_relationRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_profileRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_settingRepository, PreviewMode, AppId, TenantId);

            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Creates a new profile.
        /// </summary>
        /// <param name="NewProfile"></param>
        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]ProfileDTO NewProfile)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "profile", RequestTypeEnum.Create))
                return StatusCode(403);
            //Set Warehouse
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            var profile = await _profileRepository.CreateAsync(NewProfile, AppUser.TenantLanguage);

            return Ok(profile);
        }

        /// <summary>
        /// Updates an existing profile.
        /// </summary>
        /// <param name="UpdatedProfile"></param>
        [Route("update"), HttpPost]
        public async Task<IActionResult> Update([FromBody]ProfileDTO UpdatedProfile)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "profile", RequestTypeEnum.Update))
                return StatusCode(403);
            //Set Warehouse
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            await _profileRepository.UpdateAsync(UpdatedProfile, AppUser.TenantLanguage);
            return Ok();
        }

        /// <summary>
        /// Removes a profile and replaces its relations with another profile.
        /// </summary>
        /// <param name="RemovalRequest"></param>
        [Route("remove"), HttpPost]
        public async Task<IActionResult> Remove([FromBody]ProfileRemovalDTO RemovalRequest)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "profile", RequestTypeEnum.Delete))
                return StatusCode(403);

            await _profileRepository.RemoveAsync(RemovalRequest.RemovedProfile.Id, RemovalRequest.TransferProfile.Id);


            return Ok();
        }

        /// <summary>
        /// Gets all profiles and permissions belong to users workgroups with a lightweight user id list.
        /// </summary>
        /// <returns></returns>
        [Route("get_all"), HttpPost]
        public async Task<IActionResult> GetAll()
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "profile", RequestTypeEnum.View))
                return StatusCode(403);

            IEnumerable<ProfileWithUsersDTO> profileList = await _profileRepository.GetAllProfiles();

            return Ok(profileList);
        }

        /// <summary>
        /// Changes users profile with another one.
        /// </summary>
        /// <param name="transfer"></param>
        [Route("change_user_profile"), HttpPost]
        public async Task<IActionResult> ChangeUserProfile([FromBody]ProfileTransferDTO transfer)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "profile", RequestTypeEnum.Update))
                return StatusCode(403);

            await _profileRepository.AddUserAsync(transfer.UserID, transfer.TransferedProfileID);
            /// update session cache

            return Ok();
        }

        [Route("get_all_basic"), HttpGet]
        public async Task<IActionResult> GetAllBasic()
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "profile", RequestTypeEnum.View))
                return StatusCode(403);

            var profiles = await _profileRepository.GetAll();

            return Ok(profiles);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "profile", RequestTypeEnum.Delete))
                return StatusCode(403);

            var profileEntity = await _profileRepository.GetByIdBasic(id);

            if (profileEntity == null)
                return NotFound();

            await _profileRepository.DeleteSoft(profileEntity);

            return Ok();
        }

        [Route("count"), HttpGet]
        public IActionResult Count([FromUri]TemplateType templateType)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "profile", RequestTypeEnum.View))
                return StatusCode(403);

            var count = _profileRepository.Count();

            //if (count < 1)
            //	return NotFound(count);

            return Ok(count);
        }

        [Route("find")]
        public IActionResult Find(ODataQueryOptions<Profile> queryOptions)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "profile", RequestTypeEnum.View))
                return StatusCode(403);

            var profiles = _profileRepository.Find();
            var queryResults = (IQueryable<Profile>)queryOptions.ApplyTo(profiles, new ODataQuerySettings() { EnsureStableOrdering = false });
            return Ok(new PageResult<Profile>(queryResults, Request.ODataFeature().NextLink, Request.ODataFeature().TotalCount));
        }

    }
}
