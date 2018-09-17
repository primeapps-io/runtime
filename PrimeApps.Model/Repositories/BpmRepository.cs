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
            return null;
        }

        public async Task<ICollection<BpmWorkflow>> Find(BpmFindRequest request)
        {
            return null;
        }

        public async Task<int> Count(BpmFindRequest request)
        {
            return 0;
        }

        public async Task<int> Create(BpmWorkflow BpmWorkflow)
        {
            //DbContext.BpmWorkflows.Add(BpmWorkflow);

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
            //DbContext.BpmWorkflows.Remove(BpmWorkflow);

            return await DbContext.SaveChangesAsync();
        }
    }
}