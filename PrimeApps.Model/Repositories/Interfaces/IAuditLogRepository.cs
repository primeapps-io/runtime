using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Common.AuditLog;
using PrimeApps.Model.Entities.Application;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IAuditLogRepository : IRepositoryBaseTenant
    {
        Task<ICollection<AuditLog>> Find(AuditLogRequest request);
        Task<int> Create(AuditLog auditLog);
    }
}
