using PrimeApps.Model.Entities.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IProcessRequestRepository : IRepositoryBaseTenant
    {
        Task<ICollection<ProcessRequest>> GetByProcessId(int id);
        Task<ProcessRequest> GetByIdBasic(int id);
        Task<ProcessRequest> GetByRecordId(int id, OperationType operationType);
        Task<int> Update(ProcessRequest request);
    }
}
