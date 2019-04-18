using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DiagnosticAdapter;
using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;

namespace PrimeApps.Studio.Services
{
    public class CommandListener
    {
        private IBackgroundTaskQueue _queue;
        private IApplicationBuilder _app;
        public CommandListener(IBackgroundTaskQueue queue, IApplicationBuilder app)
        {
            _queue = queue;
            _app = app;
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandExecuting")]
        public void OnCommandExecuting(DbCommand command, DbCommandMethod executeMethod, Guid commandId, Guid connectionId, bool async, DateTimeOffset startTime)
        {
            Console.WriteLine("OnCommandExecuting");
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandExecuted")]
        public void OnCommandExecuted(RelationalDataReader readerResult, bool async)
        {
            if (readerResult.DbCommand.CommandText.StartsWith("INSERT", true, null) ||
             readerResult.DbCommand.CommandText.StartsWith("UPDATE", true, null) ||
              readerResult.DbCommand.CommandText.StartsWith("DELETE", true, null))
            {
                Console.WriteLine("OnCommandExecuted");
                string rawQuery = GetGeneratedQuery(readerResult.DbCommand);
                string result = GetTableName(rawQuery);

                // _queue.QueueBackgroundWorkItem(token =>
                // {
                //     using (var scope = _app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
                //     {
                //         var _configuration = _app.ApplicationServices.GetService<IConfiguration>();
                //         var databaseContext = scope.ServiceProvider.GetRequiredService<StudioDBContext>();
                //         using (var organizationRepository = new OrganizationRepository(databaseContext, _configuration))
                //         {
                //             var check = organizationRepository.IsOrganizationAvaliable(637, 276);
                //         }
                //     }
                //     return null;
                // });
            }
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandError")]
        public void OnCommandError(Exception exception, bool async)
        {
            Console.WriteLine("OnCommandError");
        }

        public string GetGeneratedQuery(DbCommand dbCommand)
        {
            var query = dbCommand.CommandText;
            foreach (DbParameter parameter in dbCommand.Parameters)
            {
                query = query.Replace(parameter.ParameterName, parameter.Value.ToString());
            }
            return query;
        }

        public string GetTableName(string query)
        {
            Regex nameExtractor = new Regex("((?<=INSERT\\sINTO\\s)|(?<=UPDATE\\s)|(?<=DELETE\\sFROM\\s))([^\\s]+)");

            Match match = nameExtractor.Match(query);

            return match.Value;
        }
    }
}