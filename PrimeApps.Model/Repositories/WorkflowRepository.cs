using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Common;

namespace PrimeApps.Model.Repositories
{
    public class WorkflowRepository : RepositoryBaseTenant, IWorkflowRepository
    {
        public WorkflowRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        public async Task<Workflow> GetById(int id)
        {
            var workflow = await GetWorkflowQuery()
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);

            return workflow;
        }

        public async Task<ICollection<Workflow>> GetAll(int? moduleId = null, bool? active = null)
        {
            var workflows = GetWorkflowQuery()
                .Where(x => !x.Deleted);

            if (moduleId.HasValue)
                workflows = workflows.Where(x => x.ModuleId == moduleId);

            if (active.HasValue)
                workflows = workflows.Where(x => x.Active == active.Value);

            return await workflows.ToListAsync();
        }

        public async Task<ICollection<Workflow>> GetAllBasic()
        {
            var workflows = await DbContext.Workflows
                .Where(x => !x.Deleted)
                .ToListAsync();

            return workflows;
        }

        public async Task<ICollection<Workflow>> Find(PaginationModel paginationModel)
        {
            var rules = await DbContext.Workflows
                .Skip(paginationModel.Offset * paginationModel.Limit)
                .Take(paginationModel.Limit).ToListAsync();

            if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
            {
                var propertyInfo = typeof(Workflow).GetProperty(paginationModel.OrderColumn);

                if (paginationModel.OrderType == "asc")
                {
                    rules = rules.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
                }
                else
                {
                    rules = rules.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList();
                }
            }

            return rules;
        }

        public async Task<int> Count()
        {
            var count = await DbContext.Workflows.Where(x => !x.Deleted).CountAsync();

            return count;
        }

        public async Task<int> Create(Workflow workflow)
        {
            DbContext.Workflows.Add(workflow);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(Workflow workflow, List<int> currentFilterIds)
        {
            foreach (var filterId in currentFilterIds)
            {
                var currentFilter = workflow.Filters.First(x => x.Id == filterId);
                workflow.Filters.Remove(currentFilter);
                DbContext.WorkflowFilters.Remove(currentFilter);
            }

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteSoft(Workflow workflow)
        {
            workflow.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteHard(Workflow workflow)
        {
            DbContext.Workflows.Remove(workflow);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<bool> HasLog(int workflowId, int moduleId, int recordId)
        {
            var hasLog = await DbContext.WorkflowLogs
                .AnyAsync(x => !x.Deleted &&
                x.WorkflowId == workflowId &&
                x.ModuleId == moduleId &&
                x.RecordId == recordId);

            return hasLog;
        }

        public async Task<int> CreateLog(WorkflowLog workflowLog)
        {
            DbContext.WorkflowLogs.Add(workflowLog);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteLogs(int workflowId)
        {
            var workflowLogs = await DbContext.WorkflowLogs
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

        public async Task<ICollection<UserBasic>> GetRecipients(Workflow workflow)
        {
            if (workflow.SendNotification == null || workflow.SendNotification.RecipientsArray.Length < 1)
                return null;

            var recipients = new List<UserBasic>();
            var recipientEmails = workflow.SendNotification.RecipientsArray;

            var recipientUsers = await DbContext.Users
                .Where(x => recipientEmails.Contains(x.Email))
                .ToListAsync();

            if (recipientUsers.Count > 0)
            {
                foreach (var user in recipientUsers)
                {
                    var recipient = new UserBasic
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName
                    };

                    recipients.Add(recipient);
                }
            }

            return recipients;
        }

        public async Task<ICollection<UserBasic>> GetCC(Workflow workflow)
        {
            if (workflow.SendNotification == null || workflow.SendNotification.CCArray.Length < 1)
                return null;

            var cc = new List<UserBasic>();
            var ccEmails = workflow.SendNotification.CCArray;

            var ccUsers = await DbContext.Users
                .Where(x => ccEmails.Contains(x.Email))
                .ToListAsync();

            if (ccUsers.Count > 0)
            {
                foreach (var user in ccUsers)
                {
                    var ccUser = new UserBasic
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName
                    };

                    cc.Add(ccUser);
                }
            }

            return cc;
        }

        public async Task<ICollection<UserBasic>> GetBcc(Workflow workflow)
        {
            if (workflow.SendNotification == null || workflow.SendNotification.BccArray.Length < 1)
                return null;

            var bcc = new List<UserBasic>();
            var bccEmails = workflow.SendNotification.BccArray;

            var bccUsers = await DbContext.Users
                .Where(x => bccEmails.Contains(x.Email))
                .ToListAsync();

            if (bccUsers.Count > 0)
            {
                foreach (var user in bccUsers)
                {
                    var bccUser = new UserBasic
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName
                    };

                    bcc.Add(bccUser);
                }
            }

            return bcc;
        }

        public async Task<UserBasic> GetTaskOwner(Workflow workflow)
        {
            if (workflow.CreateTask == null || workflow.CreateTask.Owner < 0)
                return null;

            var ownerUser = await DbContext.Users
                .FirstOrDefaultAsync(x => x.Id == workflow.CreateTask.Owner);

            UserBasic owner = null;

            if (ownerUser != null)
            {
                owner = new UserBasic
                {
                    Id = ownerUser.Id,
                    Email = ownerUser.Email,
                    FullName = ownerUser.FullName
                };
            }

            return owner;
        }

        private IQueryable<Workflow> GetWorkflowQuery()
        {
            return DbContext.Workflows
                .Include(x => x.Filters).Where(z => !z.Deleted)
                .Include(x => x.SendNotification)
                .Include(x => x.CreateTask)
                .Include(x => x.FieldUpdate)
                .Include(x => x.WebHook)
                .Include(x => x.Module)
                .Include(x => x.Module.Fields);
        }
    }
}
