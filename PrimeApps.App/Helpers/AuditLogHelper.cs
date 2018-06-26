using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Helpers
{
	public interface IAuditLogHelper
	{
		Task CreateLog(UserItem appUser, int? recordId, string recordName, AuditType type,
			RecordActionType? recordActionType, SetupActionType? setupActionType, Module module = null);
	}

	public class AuditLogHelper : IAuditLogHelper
	{
		private IConfiguration _configuration;
		private IAuditLogRepository _auditLogRepository;
		private IHttpContextAccessor _context;
		public AuditLogHelper(IConfiguration configuration, IAuditLogRepository auditLogRepository, IHttpContextAccessor context)
		{
			_context = context;
			_configuration = configuration;
			_auditLogRepository = auditLogRepository;

			_auditLogRepository.CurrentUser = UserHelper.GetCurrentUser(_context);
		}
        private readonly List<string> ExcludedModules = new List<string> { "stage_history", "quote_products", "order_products" };

        public async Task CreateLog(UserItem appUser, int? recordId, string recordName, AuditType type, RecordActionType? recordActionType, SetupActionType? setupActionType, Module module = null)
        {
            var auditLog = new AuditLog
            {
                AuditType = type,
                RecordId = recordId,
                RecordName = recordName.Truncate(50),
                CreatedById = appUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            switch (type)
            {
                case AuditType.Record:
                    if (recordActionType == null)
                        throw new Exception("recordActionType cannot be null.");

                    if (module == null)
                        throw new Exception("module cannot be null.");

                    if (ExcludedModules.Contains(module.Name))
                        return;

                    auditLog.RecordActionType = recordActionType.Value;
                    auditLog.ModuleId = module.Id;
                    break;
                case AuditType.Setup:
                    if (setupActionType == null)
                        throw new Exception("setupActionType cannot be null.");

                    auditLog.SetupActionType = setupActionType.Value;
                    break;
            }

            try
            {
                var result = await _auditLogRepository.Create(auditLog);

                //if (result < 1)
                //ErrorLog(null).Log(new Error(new Exception("AuditLog cannot be created! Object: " + auditLog.ToJsonString())));
                  
            }
            catch (Exception ex)
            {
                //ErrorLog.GetDefault(null).Log(new Error(ex));
            }
        }
    }
}