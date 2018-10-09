using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
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
        private IHttpContextAccessor _context;
        private IConfiguration _configuration;
        private IServiceScopeFactory _serviceScopeFactory;
        private CurrentUser _currentUser;

        public AuditLogHelper(IConfiguration configuration, IHttpContextAccessor context, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _serviceScopeFactory = serviceScopeFactory;

            _configuration = configuration;

            _currentUser = UserHelper.GetCurrentUser(_context);
        }

        public AuditLogHelper(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, CurrentUser currentUser)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;

            _currentUser = currentUser;
        }
        private readonly List<string> ExcludedModules = new List<string> { "stage_history", "quote_products", "order_products" };
			_currentUser = UserHelper.GetCurrentUser(_context);
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
				using (var scope = _serviceScopeFactory.CreateScope())
				{
					var context = scope.ServiceProvider.GetRequiredService<TenantDBContext>();
					using (var _auditLogRepository = new AuditLogRepository(context, _configuration))
					{
						_auditLogRepository.CurrentUser = _currentUser;
						var result1 = await _auditLogRepository.Create(auditLog);

						if (result1 < 1)
							ErrorHandler.LogError(new Exception("AuditLog cannot be created! Object: " + auditLog.ToJsonString()), "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name);
					}
				}


			}
			catch (Exception ex)
			{
				ErrorHandler.LogError(ex, "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + " " + "AuditType" + type + " " + "record_id:" + recordId);
			}
		}
	}
}