using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using PrimeApps.App.Models;
using PrimeApps.App.Models.ViewModel.Analytics;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Entities.Platform;

namespace PrimeApps.App.Helpers
{
    public class AnalyticsHelper
    {
        public static async Task<Analytic> CreateEntity(AnalyticBindingModel analyticModel, IUserRepository userRepository)
        {
            var analytic = new Analytic
            {
                Label = analyticModel.Label,
                PbixUrl = analyticModel.PbixUrl,
                SharingType = analyticModel.SharingType != AnalyticSharingType.NotSet ? analyticModel.SharingType : AnalyticSharingType.Everybody,
                MenuIcon = analyticModel.MenuIcon ?? "fa fa-bar-chart"
            };

            await CreateAnalyticShares(analyticModel, analytic, userRepository);

            return analytic;
        }

        public static async Task<Analytic> UpdateEntity(AnalyticBindingModel analyticModel, Analytic analytic, IUserRepository userRepository)
        {
            analytic.Label = analyticModel.Label;
            analytic.PbixUrl = analyticModel.PbixUrl;
            analytic.SharingType = analyticModel.SharingType != AnalyticSharingType.NotSet ? analyticModel.SharingType : AnalyticSharingType.Everybody;
            analytic.MenuIcon = analyticModel.MenuIcon ?? "fa fa-bar-chart";

            await CreateAnalyticShares(analyticModel, analytic, userRepository);

            return analytic;
        }

        public static async Task<WarehouseInfo> GetWarehouse(int tenantId)
        {
            PlatformWarehouse warehouse = null;

            using (PlatformDBContext platformDBContext = new PlatformDBContext())
            {
                using (PlatformWarehouseRepository platformWarehouseRepository = new PlatformWarehouseRepository(platformDBContext))
                {
                    warehouse = await platformWarehouseRepository.GetByTenantId(tenantId);
                    
                    if (warehouse == null)
                        return null;
                }
            }

            if (!warehouse.Completed)
                return null;

            var warehouseInfo = new WarehouseInfo
            {
                Server = Configuration,
                Database = warehouse.DatabaseName,
                Username = warehouse.DatabaseUser
            };

            return warehouseInfo;
        }

        private static async Task CreateAnalyticShares(AnalyticBindingModel analyticModel, Analytic analytic, IUserRepository userRepository)
        {
            if (analyticModel.Shares != null && analyticModel.Shares.Count > 0)
            {
                analytic.Shares = new List<AnalyticShares>();

                foreach (var userId in analyticModel.Shares)
                {
                    var sharedUser = await userRepository.GetById(userId);

                    if (sharedUser != null)
                        analytic.Shares.Add(sharedUser.SharedAnalytics.FirstOrDefault(x => x.UserId == userId && x.AnaltyicId == analytic.Id));
                }
            }
        }
    }
}