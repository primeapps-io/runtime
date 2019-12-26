using System.Threading.Tasks;
using PrimeApps.Model.Repositories.Interfaces;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Common.Profile;
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeApps.Model.Helpers;

namespace PrimeApps.App.Controllers
{
    [Route("api/Profile")]
    public class ProfileController : ApiBaseController
    {
        private IProfileRepository _profileRepository;
        private Warehouse _warehouse;

        public ProfileController(IProfileRepository profileRepository, Warehouse warehouse)
        {
            _profileRepository = profileRepository;
            _warehouse = warehouse;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_profileRepository, PreviewMode, TenantId, AppId);

            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Creates a new profile.
        /// </summary>
        /// <param name="NewProfile"></param>
        [Route("Create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]ProfileDTO NewProfile)
        {
            //Set Warehouse
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            await _profileRepository.CreateAsync(NewProfile, AppUser.TenantLanguage);

            return Ok();
        }

        /// <summary>
        /// Updates an existing profile.
        /// </summary>
        /// <param name="UpdatedProfile"></param>
        [Route("Update"), HttpPost]
        public async Task<IActionResult> Update([FromBody]ProfileDTO UpdatedProfile)
        {
            //Set Warehouse
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            await _profileRepository.UpdateAsync(UpdatedProfile, AppUser.TenantLanguage);
            return Ok();
        }

        /// <summary>
        /// Removes a profile and replaces its relations with another profile.
        /// </summary>
        /// <param name="RemovalRequest"></param>
        [Route("Remove"), HttpPost]
        public async Task<IActionResult> Remove([FromBody]ProfileRemovalDTO RemovalRequest)
        {
            await _profileRepository.RemoveAsync(RemovalRequest.RemovedProfile.Id, RemovalRequest.TransferProfile.Id);


            return Ok();
        }

        /// <summary>
        /// Gets all profiles and permissions belong to users workgroups with a lightweight user id list.
        /// </summary>
        /// <returns></returns>
        [Route("GetAll"), HttpPost]
        public async Task<IActionResult> GetAll()
        {
            IEnumerable<ProfileWithUsersDTO> profileList = await _profileRepository.GetAllProfiles(AppUser.Language);

            return Ok(profileList);
        }

        /// <summary>
        /// Changes users profile with another one.
        /// </summary>
        /// <param name="transfer"></param>
        [Route("ChangeUserProfile"), HttpPost]
        public async Task<IActionResult> ChangeUserProfile([FromBody]ProfileTransferDTO transfer)
        {
            await _profileRepository.AddUserAsync(transfer.UserID, transfer.TransferedProfileID);
            /// update session cache

            return Ok();
        }

        [Route("GetAllBasic"), HttpGet]
        public async Task<IActionResult> GetAllBasic()
        {
            var profiles = await _profileRepository.GetAll();

            return Ok(profiles);
        }
    }
}