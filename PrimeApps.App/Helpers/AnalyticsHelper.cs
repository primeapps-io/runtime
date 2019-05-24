using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PrimeApps.App.Models;
using PrimeApps.App.Models.ViewModel.Analytics;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace PrimeApps.App.Helpers
{
	public interface IAnalyticsHelper
	{
		Analytic CreateEntity(AnalyticBindingModel analyticModel, IUserRepository userRepository);
		Analytic UpdateEntity(AnalyticBindingModel analyticModel, Analytic analytic, IUserRepository userRepository);
		Task<WarehouseInfo> GetWarehouse(int tenantId, IConfiguration configuration);
	}

	public class AnalyticsHelper : IAnalyticsHelper
	{
		private CurrentUser _currentUser;
		private IHttpContextAccessor _context;
		private IServiceScopeFactory _serviceScopeFactory;
		private IConfiguration _configuration;

		public AnalyticsHelper(IHttpContextAccessor context, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
		{
			_context = context;
			_currentUser = UserHelper.GetCurrentUser(_context, configuration);
			_configuration = configuration;
			_serviceScopeFactory = serviceScopeFactory;
		}

        public Analytic CreateEntity(AnalyticBindingModel analyticModel, IUserRepository userRepository)
        {
            var analytic = new Analytic
            {
                Label = analyticModel.Label,
                PbixUrl = analyticModel.PbixUrl,
                SharingType = analyticModel.SharingType != AnalyticSharingType.NotSet ? analyticModel.SharingType : AnalyticSharingType.Everybody,
                MenuIcon = analyticModel.MenuIcon ?? "fa fa-bar-chart"
            };

            CreateAnalyticShares(analyticModel, analytic, userRepository);

            return analytic;
        }

        public Analytic UpdateEntity(AnalyticBindingModel analyticModel, Analytic analytic, IUserRepository userRepository)
        {
            analytic.Label = analyticModel.Label;
            analytic.PbixUrl = analyticModel.PbixUrl;
            analytic.SharingType = analyticModel.SharingType != AnalyticSharingType.NotSet ? analyticModel.SharingType : AnalyticSharingType.Everybody;
            analytic.MenuIcon = analyticModel.MenuIcon ?? "fa fa-bar-chart";

            CreateAnalyticShares(analyticModel, analytic, userRepository);

            return analytic;
        }

        public async Task<WarehouseInfo> GetWarehouse(int tenantId, IConfiguration configuration)
		{
			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				var platformDatabaseContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
				var cacheHelper = _scope.ServiceProvider.GetRequiredService<ICacheHelper>();

				PlatformWarehouse warehouse = null;

				using (PlatformWarehouseRepository platformWarehouseRepository = new PlatformWarehouseRepository(platformDatabaseContext, _configuration))//, cacheHelper))
				{
					warehouse = await platformWarehouseRepository.GetByTenantId(tenantId);

					if (warehouse == null)
						return null;
				}


				if (!warehouse.Completed)
					return null;

				var warehouseServer = configuration.GetValue("AppSettings:WarehouseServer", string.Empty);

				var warehouseInfo = new WarehouseInfo
				{
					Server = !string.IsNullOrEmpty(warehouseServer) ? warehouseServer : null,
					Database = warehouse.DatabaseName,
					Username = warehouse.DatabaseUser
				};

				return warehouseInfo;
			}
		}

        private void CreateAnalyticShares(AnalyticBindingModel analyticModel, Analytic analytic, IUserRepository userRepository)
        {
            if (analyticModel.Shares != null && analyticModel.Shares.Count > 0)
            {
                analytic.Shares = new List<AnalyticShares>();

                foreach (var userId in analyticModel.Shares)
                {
                    var sharedUser = userRepository.GetById(userId);

                    if (sharedUser != null)
                        analytic.Shares.Add(sharedUser.SharedAnalytics.FirstOrDefault(x => x.UserId == userId && x.AnaltyicId == analytic.Id));
                }
            }
        }
    }
}