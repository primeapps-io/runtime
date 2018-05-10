using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.App.Jobs;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
	[Authorize(AuthenticationSchemes = "Bearer")]
	public class TestController : BaseController
    {
		private IUserRepository _userRepository;
		private ISettingRepository _settingRepository;
		private IProfileRepository _profileRepository;
		private IRoleRepository _roleRepository;
		private IRecordRepository _recordRepository;
		private IPlatformUserRepository _platformUserRepository;
		private ITenantRepository _tenantRepository;
		private IPlatformWarehouseRepository _platformWarehouseRepository;
		private Warehouse _warehouse;

		public TestController(IUserRepository userRepository, ISettingRepository settingRepository, IProfileRepository profileRepository, IRoleRepository roleRepository, IRecordRepository recordRepository, IPlatformUserRepository platformUserRepository, ITenantRepository tenantRepository, IPlatformWarehouseRepository platformWarehouseRepository, Warehouse warehouse)
		{
			_userRepository = userRepository;
			_settingRepository = settingRepository;
			_profileRepository = profileRepository;
			_roleRepository = roleRepository;
			_warehouse = warehouse;
			_recordRepository = recordRepository;
			_platformUserRepository = platformUserRepository;
			_tenantRepository = tenantRepository;
			_platformWarehouseRepository = platformWarehouseRepository;
			//Set warehouse database name Ofisim to integration
			//_warehouse.DatabaseName = "Ofisim";
		}

		[HttpGet]
		public IEnumerable<string> Get()
		{
			return new string[] { "value1", "value2" };
		}
	}
}