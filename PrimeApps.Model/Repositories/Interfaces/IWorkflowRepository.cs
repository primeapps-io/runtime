using System.Collections.Generic;
using PrimeApps.Model.Entities.Tenant;
using System.Threading.Tasks;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IWorkflowRepository : IRepositoryBaseTenant
    {
        Task<Workflow> GetById(int id);
        Task<ICollection<Workflow>> GetAll(int? moduleId = null, bool? active = null);
        Task<ICollection<Workflow>> GetAllBasic();
        Task<int> Create(Workflow workflow);
        Task<int> Update(Workflow workflow, List<int> currentFilterIds);
        Task<int> DeleteSoft(Workflow workflow);
        Task<int> DeleteHard(Workflow workflow);
        Task<bool> HasLog(int workflowId, int moduleId, int recordId);
        Task<int> CreateLog(WorkflowLog workflowLog);
        Task<int> DeleteLogs(int workflowId);
        Task<ICollection<UserBasic>> GetRecipients(Workflow workflow);
        Task<ICollection<UserBasic>> GetCC(Workflow workflow);
        Task<ICollection<UserBasic>> GetBcc(Workflow workflow);
        Task<UserBasic> GetTaskOwner(Workflow workflow);
    }
}
