using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text.RegularExpressions;
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
        private IHistoryHelper _historyHelper;
        private IHttpContextAccessor _context;
        private CurrentUser _currentUser { get; set; }
        private DbCommand _command;
        private bool _hastExecuting = false;
        private Guid? _lastCommandId = null;
        private CurrentUser CurrentUser => _currentUser ?? (_currentUser = UserHelper.GetCurrentUser(_context));

        public CommandListener(IBackgroundTaskQueue queue, IHistoryHelper historyHelper, IHttpContextAccessor context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
            _queue = queue;
            _historyHelper = historyHelper;
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandExecuting")]
        public void OnCommandExecuting(DbCommand command, DbCommandMethod executeMethod, Guid commandId, Guid connectionId, bool async, DateTimeOffset startTime)
        {
            if ((command.CommandText.StartsWith("INSERT", true, null) && !command.CommandText.Contains("public.history_database") && !command.CommandText.Contains("public.history_storage")) ||
                command.CommandText.StartsWith("UPDATE", true, null) ||
                command.CommandText.StartsWith("CREATE", true, null) ||
                command.CommandText.StartsWith("DELETE", true, null) ||
                command.CommandText.StartsWith("DROP", true, null) ||
                command.CommandText.StartsWith("ALTER", true, null))
            {
                _lastCommandId = commandId;
                _hastExecuting = true;
                _command = command;
            }
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandExecuted")]
        public void OnCommandExecuted(object result, bool async)
        {
            if (result == null) return;

            DbCommand command;
            if (!(result is RelationalDataReader) && _command != null)
                command = _command;
            else
            {
                command = ((RelationalDataReader)result).DbCommand;
            }

            if (command != null && command.Connection?.Database != "studio" && !command.CommandText.Contains("public.history_database") && !command.CommandText.Contains("public.history_storage") && (
                    command.CommandText.StartsWith("INSERT", true, null) ||
                    command.CommandText.StartsWith("UPDATE", true, null) ||
                    command.CommandText.StartsWith("CREATE", true, null) ||
                    command.CommandText.StartsWith("DELETE", true, null) ||
                    command.CommandText.StartsWith("DROP", true, null) ||
                    command.CommandText.StartsWith("ALTER", true, null)))
            {
                if (_hastExecuting && _lastCommandId.HasValue)
                {
                    var rawQuery = GetGeneratedQuery(_command);
                    var executedAt = DateTime.Now;
                    var email = _context?.HttpContext?.User?.FindFirst("email").Value;

                    if (command.Connection?.Database != CurrentUser.PreviewMode + CurrentUser.TenantId)
                        _currentUser = null;

                    var currentUser = CurrentUser;
                    _queue.QueueBackgroundWorkItem(token => _historyHelper.Database(rawQuery, executedAt, email, currentUser, (Guid)_lastCommandId));
                }

                _hastExecuting = false;
            }
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandError")]
        public void OnCommandError(Exception ex, bool async)
        {
            var currenUser = CurrentUser;

            if (_lastCommandId.HasValue && _hastExecuting)
                _queue.QueueBackgroundWorkItem(token => _historyHelper.DeleteDbRecord(currenUser, (Guid)_lastCommandId));
        }

        public string GetGeneratedQuery(DbCommand dbCommand)
        {
            var query = dbCommand.CommandText;
            foreach (DbParameter parameter in dbCommand.Parameters)
            {
                var value = "";

                if (string.IsNullOrEmpty(parameter.Value.ToString()) && parameter.IsNullable)
                    value = "NULL";
                else
                    value = parameter.Value.ToString();

                if (value != "NULL")
                {
                    if (parameter.DbType == DbType.DateTime)
                    {
                        var e = DateTime.Parse(value);

                        value = "'" + e.ToString("yyyy-MM-dd hh:mm:ss.fffff") + "'";
                    }
                    else if (parameter.DbType == DbType.String)
                    {
                        value = "'" + value.ToString() + "'";
                    }
                    else if (parameter.DbType == DbType.Boolean)
                    {
                        value = bool.Parse(value) ? "'true'" : "'false'";
                    }
                }

                if (query.Contains(parameter.ParameterName + ","))
                {
                    // in query parameters
                    query = query.Replace(parameter.ParameterName + ",", value + ",");
                }
                else if (query.Contains(parameter.ParameterName + ";"))
                {
                    // in update or delete statement parameter
                    query = query.Replace(parameter.ParameterName + ";", value + ";");
                }
                else if (query.Contains(parameter.ParameterName + ")"))
                {
                    // End of query parameters
                    query = query.Replace(parameter.ParameterName + ")", value + ")");
                }
                else if (query.Contains("= " + parameter.ParameterName))
                {
                    // End of query parameters
                    query = query.Replace("= " + parameter.ParameterName, "= " + value);
                }
            }

            return query;
        }
    }
}