using Microsoft.EntityFrameworkCore;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace PrimeApps.Model.Repositories
{
    public class PlatformWorkflowRepository : RepositoryBasePlatform, IPlatformWorkflowRepository
    {
        public PlatformWorkflowRepository(PlatformDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        public async Task<AppWorkflow> GetById(int id)
        {
            var workflow = await GetWorkflowQuery()
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);

            return workflow;
        }

        public async Task<ICollection<AppWorkflow>> GetAll(int appId, bool? active = null)
        {
            var workflows = GetWorkflowQuery()
                .Where(x => !x.Deleted);

            workflows = workflows.Where(x => x.AppId == appId);

            if (active.HasValue)
                workflows = workflows.Where(x => x.Active == active.Value);

            return await workflows.ToListAsync();
        }

        public async Task<int> Create(AppWorkflow workflow)
        {
            DbContext.AppWorkflows.Add(workflow);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(AppWorkflow workflow)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteSoft(AppWorkflow workflow)
        {
            workflow.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> CreateLog(AppWorkflowLog workflowLog)
        {
            DbContext.AppWorkflowLogs.Add(workflowLog);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteLogs(int workflowId)
        {
            var workflowLogs = await DbContext.AppWorkflowLogs
                .Where(x => !x.Deleted && x.AppWorkflowId == workflowId)
                .ToListAsync();

            if (workflowLogs.Count < 1)
                return -1;

            foreach (var workflowLog in workflowLogs)
            {
                workflowLog.Deleted = true;
            }

            return await DbContext.SaveChangesAsync();
        }

        private IQueryable<AppWorkflow> GetWorkflowQuery()
        {
            return DbContext.AppWorkflows
                .Include(x => x.WebHook).Where(z => !z.Deleted);
        }
    }
}
