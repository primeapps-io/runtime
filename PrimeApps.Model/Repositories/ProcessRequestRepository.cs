using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Repositories
{
    public class ProcessRequestRepository : RepositoryBaseTenant, IProcessRequestRepository
    {
        public ProcessRequestRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        public async Task<ICollection<ProcessRequest>> GetByProcessId(int id)
        {
            var processRequests = await DbContext.ProcessRequests
                .Where(x => x.ProcessId == id)
                .ToListAsync();

            return processRequests;
        }

        public async Task<ProcessRequest> GetByIdBasic(int id)
        {
            var request = await DbContext.ProcessRequests
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);

            return request;
        }

        public async Task<ProcessRequest> GetByRecordId(int id, string moduleName, OperationType operationType)
        {
            var request = await DbContext.ProcessRequests
                .FirstOrDefaultAsync(x => x.RecordId == id && x.OperationType == operationType && x.Module == moduleName );

            return request;
        }

        public async Task<ProcessRequest> GetByRecordIdWithOutOperationType(int id, string moduleName)
        {
            var request = await DbContext.ProcessRequests
                .FirstOrDefaultAsync(x => x.RecordId == id && x.Module == moduleName);

            return request;
        }

        public async Task<int> Update(ProcessRequest request)
        {
            return await DbContext.SaveChangesAsync();
        }
    }
}
