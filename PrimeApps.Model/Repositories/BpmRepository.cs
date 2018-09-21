using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common.Bpm;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Model.Repositories
{
    public class BpmRepository : RepositoryBaseTenant, IBpmRepository
    {
        public BpmRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        public async Task<BpmWorkflow> Get(int id)
        {
            var bpmWorkFlow = await DbContext.BpmWorkflows.Where(q => q.Id == id && !q.Deleted).FirstOrDefaultAsync();

            return bpmWorkFlow;
        }

        public async Task<ICollection<BpmWorkflow>> Find(BpmFindRequest request)
        {
            var bpmWorkFlow = await DbContext.BpmWorkflows.Where(q => !q.Deleted).Take(request.Limit).ToListAsync();

            if (bpmWorkFlow.Count() < 1)
                return null;

            return bpmWorkFlow;
        }

        public async Task<int> Count(BpmFindRequest request)
        {
            var count = await DbContext.BpmWorkflows.Where(q => !q.Deleted).CountAsync();

            return count;
        }

        public async Task<int> Create(BpmWorkflow BpmWorkflow)
        {
            DbContext.BpmWorkflows.Add(BpmWorkflow);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(BpmWorkflow BpmWorkflow)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteSoft(BpmWorkflow BpmWorkflow)
        {
            BpmWorkflow.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteHard(BpmWorkflow BpmWorkflow)
        {
            DbContext.BpmWorkflows.Remove(BpmWorkflow);

            return await DbContext.SaveChangesAsync();
        }
    }
}