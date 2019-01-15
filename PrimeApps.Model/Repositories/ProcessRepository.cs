using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common;

namespace PrimeApps.Model.Repositories
{
    public class ProcessRepository : RepositoryBaseTenant, IProcessRepository
    {
        public ProcessRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        public async Task<Process> GetById(int id)
        {
            var process = await GetProcessQuery()
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);

            return process;
        }

        public async Task<Process> GetAllById(int id)
        {
            var process = await GetProcessQueryForAll()
                .FirstOrDefaultAsync(x => x.Id == id);

            return process;
        }

        public async Task<ICollection<Process>> GetAll(int? moduleId = null, int? userId = null, bool? active = null)
        {
            var processes = GetProcessQuery()
                .Where(x => !x.Deleted);

            if (moduleId.HasValue)
                processes = processes.Where(x => x.ModuleId == moduleId);

            if (userId.HasValue && userId.Value > 0)
                processes = processes.Where(x => x.UserId == userId || x.UserId == 0);

            if (active.HasValue)
                processes = processes.Where(x => x.Active == active.Value);

            return await processes.ToListAsync();
        }

        public async Task<int> DeleteSoft(Process process)
        {
            process.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteLogs(int processId)
        {
            var processLogs = await DbContext.ProcessLogs
                .Where(x => !x.Deleted && x.ProcessId == processId)
                .ToListAsync();

            if (processLogs.Count < 1)
                return -1;

            foreach (var processLog in processLogs)
            {
                processLog.Deleted = true;
            }

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Count()
        {
            var count = await DbContext.Processes.CountAsync(x => !x.Deleted);

            return count;
        }

        public async Task<ICollection<Process>> Find(PaginationModel paginationModel)
        {
            var processes = await DbContext.Processes.Where(x => !x.Deleted).Skip(paginationModel.Offset * paginationModel
                .Limit).Take(paginationModel.Limit).ToListAsync();

            if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
            {
                var propertyInfo = typeof(Process).GetProperty(paginationModel.OrderColumn);

                if (paginationModel.OrderType == "asc")
                {
                    processes = processes.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
                }
                else
                {
                    processes = processes.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList();
                }
            }

            return processes;
        }

        public async Task<ICollection<Process>> GetAllBasic()
        {
            var processes = await DbContext.Processes
                .Where(x => !x.Deleted)
                .ToListAsync();

            return processes;
        }

        public async Task<int> Create(Process process)
        {
            DbContext.Processes.Add(process);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(Process process, List<int> currentFilterIds, List<int> currentApproverIds)
        {
            foreach (var filterId in currentFilterIds)
            {
                var currentFilter = process.Filters.First(x => x.Id == filterId);
                process.Filters.Remove(currentFilter);
                DbContext.ProcessFilters.Remove(currentFilter);
            }

            foreach (var approverId in currentApproverIds)
            {
                var currentApprover = process.Approvers.First(x => x.Id == approverId);
                process.Approvers.Remove(currentApprover);
                DbContext.ProcessApprovers.Remove(currentApprover);
            }

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> CreateRequest(ProcessRequest processRequest)
        {
            DbContext.ProcessRequests.Add(processRequest);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<bool> HasLog(int processId, int moduleId, int recordId)
        {
            var hasLog = await DbContext.ProcessLogs
                .AnyAsync(x => !x.Deleted &&
                x.ProcessId == processId &&
                x.ModuleId == moduleId &&
                x.RecordId == recordId);

            return hasLog;
        }

        public async Task<int> CreateLog(ProcessLog processLog)
        {
            DbContext.ProcessLogs.Add(processLog);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<ICollection<ProcessApprover>> GetUsers(Process process)
        {
            if (process.Approvers == null)
                return null;

            var approverList = new List<ProcessApprover>();
            var approvers = process.Approvers;

            var Users = await DbContext.Users
                .ToListAsync();

            if (Users.Count > 0)
            {
                foreach (var approver in approvers)
                {
                    var processApprover = new ProcessApprover
                    {
                        Id = approver.UserId,
                        Order = approver.Order,
                        CreatedById = approver.CreatedById,
                        CreatedAt = approver.CreatedAt

                    };

                    approverList.Add(processApprover);
                }
            }

            return approverList;
        }

        private IQueryable<Process> GetProcessQuery()
        {
            return DbContext.Processes
                .Include(x => x.Filters).Where(z => !z.Deleted)
                .Include(x => x.Approvers).Where(z => !z.Deleted)
                .Include(x => x.Module)
                .Include(x => x.Module.Fields)
                .Include(x => x.Module.Dependencies);
        }

        private IQueryable<Process> GetProcessQueryForAll()
        {
            return DbContext.Processes
                .Include(x => x.Filters)
                .Include(x => x.Approvers)
                .Include(x => x.Module)
                .Include(x => x.Module.Fields);
        }
    }
}
