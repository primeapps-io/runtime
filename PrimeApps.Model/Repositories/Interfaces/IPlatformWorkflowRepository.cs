using PrimeApps.Model.Entities.Platform;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IPlatformWorkflowRepository : IRepositoryBasePlatform
    {
        Task<AppWorkflow> GetById(int id);
        Task<ICollection<AppWorkflow>> GetAll(int appId, bool? active = null);
        Task<int> Create(AppWorkflow workflow);
        Task<int> Update(AppWorkflow workflow);
        Task<int> DeleteSoft(AppWorkflow workflow);
        Task<int> CreateLog(AppWorkflowLog workflowLog);
        Task<int> DeleteLogs(int workflowId);
    }
}
