using System;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Studio.Services;

namespace PrimeApps.Studio.Helpers
{
    public interface ICommandHistoryHelper
    {
        void Add(string sql, DateTime executedAt, string createdByEmail, int recordId = 0);
    }

    public class CommandHistoryHelper : ICommandHistoryHelper
    {
        private IServiceScopeFactory _serviceScopeFactory;
        private IHttpContextAccessor _context;
        private IConfiguration _configuration;

        public CommandHistoryHelper(IHttpContextAccessor context, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public void Add(string sql, DateTime executedAt, string createdByEmail, int recordId = 0)
        {
            string tableName = GetTableName(sql);
            string statement = GetStatement(sql);

            var previewMode = _configuration.GetValue("AppSettings:PreviewMode", string.Empty);
            previewMode = !string.IsNullOrEmpty(previewMode) ? previewMode : "tenant";

            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetService<TenantDBContext>();
                using (var commandHistory = new CommandHistoryRepository(databaseContext, _configuration))
                {
                    var history = new CommandHistory
                    {
                        CommandText = sql,
                        RecordId = recordId,
                        TableName = tableName,
                        ExecutedAt = executedAt,
                        CreatedByEmail = createdByEmail
                    };

                    commandHistory.Create(history);
                }
            }
        }

        public string GetTableName(string query)
        {
            Regex nameExtractor = new Regex("((?<=INSERT\\sINTO\\s)|(?<=UPDATE\\s)|(?<=DELETE\\sFROM\\s))([^\\s]+)");

            Match match = nameExtractor.Match(query);

            return match.Value;
        }

        public string GetStatement(string query)
        {
            Regex nameExtractor = new Regex("((?<=INSERT\\sINTO\\s)|(?<=UPDATE\\s)|(?<=DELETE\\sFROM\\s))([^\\s]+)");

            Match match = nameExtractor.Match(query);

            return match.Value;
        }
    }
}