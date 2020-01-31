using System;
using System.Collections.Generic;
using PrimeApps.Model.Entities.Tenant;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IRecordRepository : IRepositoryBaseTenant
    {
        Task<JObject> GetById(Module module, int recordId, bool roleBasedEnabled = true, ICollection<Module> lookupModules = null, bool deleted = false, bool profileBasedEnabled = true, OperationType operation = OperationType.read);
        Task<JArray> GetAllById(string moduleName, List<int> recordIds, bool roleBasedEnabled = true);
        Task<JArray> Find(string moduleName, FindRequest findRequest, bool roleBasedEnabled = true, bool profileBasedEnabled = true, int timezoneOffset = 180);
        Task<int> Create(JObject record, Module module);
        Task<int> Update(JObject record, Module module, bool delete = false, bool isUtc = true);
        Task<int> Delete(JObject record, Module module);
        Task<int> AddRelations(JArray records, string moduleName, string relatedModuleName, int relationId = 0, bool twoway = false);
        Task<int> DeleteRelation(JObject record, string moduleName, string relatedModuleName, int relationId = 0, bool twoway = false);
        void SetPicklists(Module module, JObject record, string picklistLanguage);
        void MultiselectsToString(Module module, JObject record);
        void ArrayToString(Module module, JObject record);
        Task<int> UpdateSystemData(int createdBy, DateTime createdAt, string tenantLanguage, int appId);
        Task<int> UpdateSampleData(PlatformUser user);
        Task<int> DeleteSampleData(List<Module> modules);
        JObject GetLookupIds(JArray lookupRequest);
        Task<int> CreateBulk(JArray records, Module module);
        JArray LookupUser(LookupUserRequest request);
        Task<JObject> RecordPermissionControl(string moduleName, int userId, JObject record, OperationType operation, List<string> removedFields = null);
    }
}
