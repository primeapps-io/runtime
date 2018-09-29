using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Common.Bpm;
using PrimeApps.Model.Entities.Tenant;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IBpmRepository : IRepositoryBaseTenant
    {
        Task<BpmWorkflow> Get(int id);
        Task<List<BpmWorkflow>> GetAll(string code = null, int? version = null, bool active = true, bool deleted = false);

        Task<List<BpmWorkflow>> GetByModuleId(int moduleId, bool active = true, bool deleted = false);

        Task<ICollection<BpmWorkflow>> Find(BpmFindRequest request);

        Task<int> Count(BpmFindRequest request);

        Task<int> Create(BpmWorkflow note);

        Task<int> Update(BpmWorkflow note);

        Task<int> DeleteSoft(BpmWorkflow note);

        Task<int> DeleteHard(BpmWorkflow note);

        #region BpmWorkflowLog
        Task<bool> HasLog(int workflowId, int moduleId, int recordId);

        Task<int> CreateLog(BpmWorkflowLog workflowLog);

        Task<int> DeleteLogs(int workflowId);
        #endregion
    }
}
