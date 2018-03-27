using System.Threading.Tasks;
using PrimeApps.App.Results;
using PrimeApps.Model.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Common.Profile;

namespace PrimeApps.App.Controllers
{
    [Route("api/Profile")]
    public class ProfileController : BaseController
    {
        private IProfileRepository _profileRepository;

        public ProfileController(IProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        /// <summary>
        /// Creates a new profile.
        /// </summary>
        /// <param name="NewProfile"></param>
        [Route("Create"), HttpPost]
        public async Task<IActionResult> Create(ProfileDTO NewProfile)
        {
            //get instance admin to validate if entity belongs to this session's user.
            bool isOperationAllowed = await Cache.Tenant.CheckProfilesAdministrativeRights(AppUser.TenantId, AppUser.Id);

            if (!isOperationAllowed)
            {
                //if instance id does not belong to current session, stop the request and send the forbidden status code.
                return new ForbiddenResult(Request);
            }

            await _profileRepository.CreateAsync(NewProfile);

            await Cache.Tenant.UpdateProfiles(AppUser.TenantId);
            return Ok();
        }

        /// <summary>
        /// Updates an existing profile.
        /// </summary>
        /// <param name="UpdatedProfile"></param>
        [Route("Update"), HttpPost]
        public async Task<IActionResult> Update(ProfileDTO UpdatedProfile)
        {

            //get instance admin to validate if entity belongs to this session's user.
            bool isOperationAllowed = await Cache.Tenant.CheckProfilesAdministrativeRights(AppUser.TenantId, AppUser.Id);

            if (!isOperationAllowed)
            {
                //if instance id does not belong to current session, stop the request and send the forbidden status code.
                return new ForbiddenResult(Request);
            }
            await _profileRepository.UpdateAsync(UpdatedProfile);
            await Cache.Tenant.UpdateProfiles(AppUser.TenantId);
            return Ok();
        }

        /// <summary>
        /// Removes a profile and replaces its relations with another profile.
        /// </summary>
        /// <param name="RemovalRequest"></param>
        [Route("Remove"), HttpPost]
        public async Task<IHttpActionResult> Remove(ProfileRemovalDTO RemovalRequest)
        {
            //get instance admin to validate if entity belongs to this session's user.
            bool isOperationAllowed = await Cache.Tenant.CheckProfilesAdministrativeRights(AppUser.TenantId, AppUser.Id);

            if (!isOperationAllowed)
            {
                //if instance id does not belong to current session, stop the request and send the forbidden status code.
                return new ForbiddenResult(Request);
            }

            await _profileRepository.RemoveAsync(RemovalRequest.RemovedProfile.ID, RemovalRequest.TransferProfile.ID);

            await Cache.Tenant.UpdateProfiles(AppUser.TenantId);
            return Ok();
        }

        /// <summary>
        /// Gets all profiles and permissions belong to users workgroups with a lightweight user id list.
        /// </summary>
        /// <returns></returns>
        [Route("GetAll"), HttpPost]
        public async Task<IActionResult> GetAll()
        {
            IEnumerable<ProfileWithUsersDTO> profileList = await _profileRepository.GetAllProfiles();
            
            return Ok(profileList);
        }

        /// <summary>
        /// Changes users profile with another one.
        /// </summary>
        /// <param name="transfer"></param>
        [Route("ChangeUserProfile"), HttpPost]
        public async Task<IActionResult> ChangeUserProfile(ProfileTransferDTO transfer)
        {
            //get instance admin to validate if entity belongs to this session's user.
            bool isOperationAllowed = await Cache.Tenant.CheckProfilesAdministrativeRights(AppUser.TenantId, AppUser.Id);

            if (!isOperationAllowed)
            {
                //if instance id does not belong to current session, stop the request and send the forbidden status code.
                return new ForbiddenResult(Request);
            }


            await _profileRepository.AddUserAsync(transfer.UserID, transfer.TransferedProfileID);
            /// update session cache
            await Cache.Tenant.UpdateProfiles(AppUser.TenantId);
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
