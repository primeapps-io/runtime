using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using PrimeApps.Model.Common.AuditLog;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Model.Repositories
{
    public class AuditLogRepository : RepositoryBaseTenant, IAuditLogRepository
    {
        public AuditLogRepository(TenantDBContext dbContext) : base(dbContext) { }

        public async Task<ICollection<AuditLog>> Find(AuditLogRequest request)
        {
            var auditLogs = DbContext.AuditLogs
                .Include(x => x.CreatedBy)
                .Include(x => x.Module)
                .Where(x => !x.Deleted);

            if (request.RecordActionType != RecordActionType.NotSet)
                auditLogs = auditLogs.Where(x => x.RecordActionType == request.RecordActionType);

            if (request.SetupActionType != SetupActionType.NotSet)
                auditLogs = auditLogs.Where(x => x.SetupActionType == request.SetupActionType);

            if (request.UserId.HasValue)
                auditLogs = auditLogs.Where(x => x.CreatedById == request.UserId.Value);

            if (request.StartDate.HasValue)
                auditLogs = auditLogs.Where(x => x.CreatedAt >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                auditLogs = auditLogs.Where(x => x.CreatedAt <= request.EndDate.Value);

            auditLogs = auditLogs
                .OrderByDescending(x => x.CreatedAt)
                .Skip(request.Offset)
                .Take(request.Limit);

            return await auditLogs.ToListAsync();
        }

        public async Task<int> Create(AuditLog auditLog)
        {
            DbContext.AuditLogs.Add(auditLog);

            return await DbContext.SaveChangesAsync();
        }
    }
}
