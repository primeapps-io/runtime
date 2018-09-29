using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common.Bpm;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Model.Repositories
{
    public class BpmRepository : RepositoryBaseTenant, IBpmRepository
    {
        public BpmRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        #region BpmWorkflow
        public async Task<BpmWorkflow> Get(int id)
        {
            var bpmWorkFlow = await DbContext.BpmWorkflows.Where(q => q.Id == id && !q.Deleted).FirstOrDefaultAsync();

            return bpmWorkFlow;
        }

        public async Task<List<BpmWorkflow>> GetAll(string code = null, int? version = null, bool active = true, bool deleted = false)
        {
            var bpmWorkFlows = DbContext.BpmWorkflows.Where(q => q.Deleted == deleted && q.Active == active);

            if (code != null)
                bpmWorkFlows = bpmWorkFlows.Where(q => q.Code == code);
            if (version.HasValue)
                bpmWorkFlows = bpmWorkFlows.Where(q => q.Version == version);

            return await bpmWorkFlows.ToListAsync();
        }

        public async Task<List<BpmWorkflow>> GetByModuleId(int moduleId, bool active = true, bool deleted = false)
        {
            var bpmWorkFlows = await DbContext.BpmWorkflows.Where(q => q.ModuleId == moduleId && q.Active == active && q.Deleted == deleted).ToListAsync();

            return bpmWorkFlows;
        }

        public async Task<List<BpmWorkflow>> GetAllBasic()
        {
            var bpmWorkFlows = await DbContext.BpmWorkflows.Where(q => !q.Deleted).ToListAsync();

            return bpmWorkFlows;
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
        #endregion BpmWorkflow

        #region BpmWorkflowLog

        public async Task<bool> HasLog(int workflowId, int moduleId, int recordId)
        {
            var hasLog = await DbContext.BpmWorkflowLogs
                .AnyAsync(x => !x.Deleted &&
                x.WorkflowId == workflowId &&
                x.ModuleId == moduleId &&
                x.RecordId == recordId);

            return hasLog;
        }

        public async Task<int> CreateLog(BpmWorkflowLog workflowLog)
        {
            DbContext.BpmWorkflowLogs.Add(workflowLog);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteLogs(int workflowId)
        {
            var workflowLogs = await DbContext.BpmWorkflowLogs
                .Where(x => !x.Deleted && x.WorkflowId == workflowId)
                .ToListAsync();

            if (workflowLogs.Count < 1)
                return -1;

            foreach (var workflowLog in workflowLogs)
            {
                workflowLog.Deleted = true;
            }

            return await DbContext.SaveChangesAsync();
        }
        #endregion BpmWorkflowLog
    }
}