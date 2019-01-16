using PrimeApps.Console.Models;
using PrimeApps.Console.Helpers;
using PrimeApps.Model.Constants;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Http.Extensions;
using System.Linq;
using PrimeApps.Model.Common;
using PrimeApps.Model.Common.Profile;

namespace PrimeApps.Console.Controllers
{
	[Route("api/Profile")]
	public class ProfileController : DraftBaseController
	{
		private IRelationRepository _relationRepository;
        private IUserRepository _userRepository;
        private IProfileRepository _profileRepository;
		private ISettingRepository _settingRepository;
		private IModuleRepository _moduleRepository;
		private IConfiguration _configuration;
		private Warehouse _warehouse;
		private IModuleHelper _moduleHelper;

		public ProfileController(IRelationRepository relationRepository, IProfileRepository profileRepository, ISettingRepository settingRepository, IModuleRepository moduleRepository, Warehouse warehouse, IModuleHelper moduleHelper, IConfiguration configuration,IHelpRepository helpRepository,IUserRepository userRepository)
		{
			_relationRepository = relationRepository;
			_profileRepository = profileRepository;
			_settingRepository = settingRepository;			
			_warehouse = warehouse;
			_configuration = configuration;
			_moduleHelper = moduleHelper;
            _userRepository = userRepository;
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
            await _profileRepository.RemoveAsync(RemovalRequest.RemovedProfile.ID, RemovalRequest.TransferProfile.ID);


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
