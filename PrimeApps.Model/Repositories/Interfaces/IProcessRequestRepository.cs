using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IProcessRequestRepository : IRepositoryBaseTenant
    {
        Task<ICollection<ProcessRequest>> GetByProcessId(int id);
        Task<ProcessRequest> GetByIdBasic(int id);
        Task<ProcessRequest> GetByRecordId(int id, string moduleName, OperationType operationType);
        Task<ProcessRequest> GetByRecordIdWithOutOperationType(int id, string moduleName);
        Task<int> Update(ProcessRequest request);
    }
}
