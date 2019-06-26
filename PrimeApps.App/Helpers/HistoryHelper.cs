using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;

namespace PrimeApps.Studio.Helpers
{
    public interface IHistoryHelper
    {
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

        public static string GetTableName(string query)
        {
            Regex nameExtractor = new Regex("((?<=INSERT\\sINTO\\s)|(?<=UPDATE\\s)|(?<=DELETE\\sFROM\\s))([^\\s]+)");

            Match match = nameExtractor.Match(query);

            return match.Value;
        }
    }
}