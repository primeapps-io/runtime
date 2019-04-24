using System;
using System.Data.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DiagnosticAdapter;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Helpers;
using PrimeApps.Studio.Helpers;

namespace PrimeApps.Studio.Services
{
    public class CommandListener
    {
        private IConfiguration _configuration;
        private IBackgroundTaskQueue _queue;
        private ICommandHistoryHelper _commandHistoryHelper;
        private IHttpContextAccessor _context;
        private static DbCommand _currentCommand;

        public CommandListener(IBackgroundTaskQueue queue, ICommandHistoryHelper commandHistoryHelper, IHttpContextAccessor context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
            _queue = queue;
            _commandHistoryHelper = commandHistoryHelper;
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandExecuting")]
        public void OnCommandExecuting(DbCommand command, DbCommandMethod executeMethod, Guid commandId, Guid connectionId, bool async, DateTimeOffset startTime)
        {
            Console.WriteLine("OnCommandExecuting");
            _currentCommand = command;
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandExecuted")]
        public void OnCommandExecuted(object result, bool async)
        {
            if (result == null) return;

            DbCommand dbCommand;
            RelationalDataReader command = null;
            if (!(result is RelationalDataReader)  && _currentCommand != null)
                dbCommand = _currentCommand;
            else
            {
                command = (RelationalDataReader)result;
                dbCommand = command.DbCommand;
            }

            if (dbCommand.CommandText.StartsWith("INSERT", true, null) ||
                dbCommand.CommandText.StartsWith("UPDATE", true, null) ||
                dbCommand.CommandText.StartsWith("CREATE", true, null) ||
                dbCommand.CommandText.StartsWith("DELETE", true, null) ||
                dbCommand.CommandText.StartsWith("DROP", true, null) ||
                dbCommand.CommandText.StartsWith("ALTER", true, null))
            {
                Console.WriteLine("OnCommandExecuted");
                var rawQuery = GetGeneratedQuery(dbCommand);

                JArray queryResult = command.DbDataReader.ResultToJArray();

                int recordId = 0;

                if (queryResult.HasValues)
                {
                    recordId = queryResult.First.Value<int>("id");
                }

                var executedAt = DateTime.Now;
                var email = _context.HttpContext.User.FindFirst("email").Value;
                //var tracerHelper = _app.ApplicationServices.GetService<ITracerHelper>();
                _queue.QueueBackgroundWorkItem(async token => _commandHistoryHelper.Add(rawQuery, executedAt, email, recordId));
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
    }
}