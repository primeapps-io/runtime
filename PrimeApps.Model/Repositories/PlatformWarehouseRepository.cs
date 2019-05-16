using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Model.Repositories
{
	public class PlatformWarehouseRepository : RepositoryBasePlatform, IPlatformWarehouseRepository
	{
		public PlatformWarehouseRepository(PlatformDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { } /*, ICacheHelper cacheHelper , cacheHelper*/

		public Task<PlatformWarehouse> GetByTenantId(int tenantId)
		{
			return DbContext.Warehouses.Where(x => x.TenantId == tenantId).SingleOrDefaultAsync();
		}

		public PlatformWarehouse GetByTenantIdSync(int tenantId)
		{
			return DbContext.Warehouses.SingleOrDefault(x => x.TenantId == tenantId);
		}

		public PlatformWarehouse Create(PlatformWarehouse warehouse)
		{
			warehouse.Completed = false;

			DbContext.Warehouses.Add(warehouse);
			DbContext.SaveChanges();

			return warehouse;
		}

		public void SetCompleted(PlatformWarehouse warehouse, string userEmail)
		{
			warehouse.Completed = true;
			DbContext.SaveChanges();

			SetAnalyticsLicense(warehouse.TenantId);
			//SendCompletedMail(warehouse.DatabaseName, userEmail);
		}

		private void SetAnalyticsLicense(int tenantId)
		{
			// var tenant = DbContext.Tenants.Single(x => x.Id == tenantId);
			var tenantLicenses = DbContext.TenantLicenses.Single(x => x.TenantId == tenantId);

			//App publish edildikten sonra 
			if (tenantLicenses != null) //&& tenantLicenses.AnalyticsLicenseCount > 0)
			{
				//AnalyticsLicenseCount'u 1'e çekiyoruz.
				tenantLicenses.AnalyticsLicenseCount = 1;
				DbContext.SaveChanges();
			}		
		}

		private void SendCompletedMail(string warehouseName, string email)
		{
			//var emailQueue = new crmEmailQueue()
			//{
			//    EmailTo = email,
			//    EmailFrom = "destek@ofisim.com",
			//    ReplyTo = "destek@ofisim.com",
			//    FromName = "Ofisim.com",
			//    Subject = "Warehouse veritabanı oluşturuldu",
			//    Body = warehouseName + " isimli warehouse veritabanı başarılı şekilde oluşturuldu.",
			//    UniqueID = null,
			//    QueueTime = DateTime.UtcNow
			//};

			//session.SaveOrUpdate(emailQueue);
		}
	}
}
