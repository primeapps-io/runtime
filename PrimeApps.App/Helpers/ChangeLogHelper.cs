using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Repositories;
using System;
using System.Threading.Tasks;
using PrimeApps.Model.Common.Cache;

namespace PrimeApps.App.Helpers
{
	public interface IChangeLogHelper
	{
		Task<bool> CreateLog(UserItem appUser, JObject record, Module module);
	}
    public class ChangeLogHelper : IChangeLogHelper
    {
        public async Task<bool> CreateLog(UserItem appUser, JObject record, Module module)
        {
            int result = 0;
            using (var databaseContext = new TenantDBContext(appUser.TenantId))
            {
                using (var changeLogRepository = new ChangeLogRepository(databaseContext))
                {
                    var changeLog = new ChangeLog()
                    {
                        Record = record,
                        RecordId = (int)record["id"],
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedById = appUser.Id
                    };
                    result = await changeLogRepository.Create(changeLog);

                    if (result < 1)
                    {
                        //ErrorLog.GetDefault(null).Log(new Error(new Exception("ChangeLog cannot be created! Object: " + changeLog.ToJsonString())));
                    }
                }
            }

            return result > 0;
        }
    }
}