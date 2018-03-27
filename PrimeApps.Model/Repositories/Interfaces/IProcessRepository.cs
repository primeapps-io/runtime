using System.Collections.Generic;
using PrimeApps.Model.Entities.Application;
using System.Threading.Tasks;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IProcessRepository : IRepositoryBaseTenant
    {
        Task<Process> GetById(int id);
        Task<Process> GetAllById(int id);
        Task<ICollection<Process>> GetAll(int? moduleId = null, int? userId = null, bool? active = null);
        Task<int> DeleteSoft(Process process);
        Task<int> DeleteLogs(int processId);
        Task<bool> HasLog(int processId, int moduleId, int recordId);
        Task<int> CreateLog(ProcessLog processLog);
        Task<int> CreateRequest(ProcessRequest processRequest);
        Task<int> Create(Process process);
        Task<int> Update(Process process, List<int> currentFilterIds, List<int> currentApproverIds);
        Task<ICollection<ProcessApprover>> GetUsers(Process process);
        Task<ICollection<Process>> GetAllBasic();
    }
}
