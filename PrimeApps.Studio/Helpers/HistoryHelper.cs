using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;

namespace PrimeApps.Studio.Helpers
{
    public interface IHistoryHelper
    {
        Task DeleteDbRecord(CurrentUser currentUser, Guid commandId);
        Task Database(string sql, DateTime executedAt, string createdByEmail, CurrentUser currentUser, Guid commandId);
        Task Storage(string fileName, string uniqueName, string operation, string path, string createdByEmail, CurrentUser currentUser);
    }

    public class HistoryHelper : IHistoryHelper
    {
        private IServiceScopeFactory _serviceScopeFactory;
        private IHttpContextAccessor _context;
        private IConfiguration _configuration;

        public HistoryHelper(IHttpContextAccessor context, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task DeleteDbRecord(CurrentUser currentUser, Guid commandId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = scope.ServiceProvider.GetService<TenantDBContext>();
                using (var historyDatase = new HistoryDatabaseRepository(databaseContext, _configuration))
                {
                    historyDatase.CurrentUser = currentUser;

                    var history = await historyDatase.Get(commandId);

                    if (history != null)
                    {
                        history.Deleted = true;
                        await historyDatase.Update(history);
                    }
                }
            }
        }

        public async Task Database(string sql, DateTime executedAt, string createdByEmail, CurrentUser currentUser, Guid commandId)
        {
            var tableName = Model.Helpers.PackageHelper.GetTableName(sql);

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = scope.ServiceProvider.GetService<TenantDBContext>();
                using (var historyDatase = new HistoryDatabaseRepository(databaseContext, _configuration))
                {
                    historyDatase.CurrentUser = currentUser;

                    var history = new HistoryDatabase
                    {
                        CommandText = sql.Replace(Environment.NewLine, " "),
                        TableName = tableName,
                        ExecutedAt = executedAt,
                        CommandId = commandId,
                        CreatedByEmail = createdByEmail ?? ""
                    };

                    await historyDatase.Create(history);
                }
            }
        }

        public async Task Storage(string fileName, string uniqueName, string operation, string path, string createdByEmail, CurrentUser currentUser)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = scope.ServiceProvider.GetService<TenantDBContext>();
                using (var historyStorage = new HistoryStorageRepository(databaseContext, _configuration))
                {
                    historyStorage.CurrentUser = currentUser;

                    var mimeType = DocumentHelper.GetMimeType(fileName);
                    var history = new HistoryStorage
                    {
                        MimeType = mimeType,
                        Path = path,
                        Operation = operation,
                        FileName = fileName,
                        UniqueName = uniqueName,
                        ExecutedAt = DateTime.Now,
                        CreatedByEmail = createdByEmail ?? ""
                    };

                    await historyStorage.Create(history);
                }
            }
        }
    }
}