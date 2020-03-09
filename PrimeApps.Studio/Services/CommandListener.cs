using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
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

        public CommandListener(IBackgroundTaskQueue queue, IHistoryHelper historyHelper, IHttpContextAccessor context,
            IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
            _queue = queue;
            _historyHelper = historyHelper;
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandExecuting")]
        public void OnCommandExecuting(DbCommand command, DbCommandMethod executeMethod, Guid commandId,
            Guid connectionId, bool async, DateTimeOffset startTime)
        {
            if ((command.CommandText.StartsWith("INSERT", true, null) &&
                 !command.CommandText.Contains("public.history_database") &&
                 !command.CommandText.Contains("public.history_storage")) ||
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

            if (command != null && command.Connection?.Database != "studio" &&
                !command.CommandText.Contains("public.history_database") &&
                !command.CommandText.Contains("public.history_storage") && (
                    command.CommandText.StartsWith("INSERT", true, null) ||
                    command.CommandText.StartsWith("UPDATE", true, null) ||
                    command.CommandText.StartsWith("CREATE", true, null) ||
                    command.CommandText.StartsWith("DELETE", true, null) ||
                    command.CommandText.StartsWith("DROP", true, null) ||
                    command.CommandText.StartsWith("ALTER", true, null)))
            {
                if (_hastExecuting && _lastCommandId.HasValue)
                {
                    /*while (((RelationalDataReader)result).DbDataReader.Read())
                    {
                        Console.WriteLine("{0}\t", ((RelationalDataReader)result).DbDataReader.GetInt32(0));
                    }*/

                    var rawQuery = GetGeneratedQuery(_command);
                    var executedAt = DateTime.Now;
                    var email = _context?.HttpContext?.User?.FindFirst("email").Value;

                    if (command.Connection?.Database != CurrentUser?.PreviewMode + CurrentUser?.TenantId)
                        _currentUser = null;

                    var currentUser = CurrentUser;

                    var sequences = new JObject();

                    var sqls = rawQuery.Split(";" + Environment.NewLine);
                    var newRawQuery = "";
                    foreach (var obj in sqls.OfType<string>().Select((sql, index) => new { sql, index }))
                    {
                        var sql = obj.sql;
                        var index = obj.index;

                        if (string.IsNullOrEmpty(sql))
                            continue;

                        if (sql.StartsWith("INSERT INTO"))
                        {
                            var tableName = Model.Helpers.PackageHelper.GetTableName(sql);
                            /*
                             * When app owners adding app user table for preview. We don't need to add history about it.
                             */
                            if (tableName == "public.users" || tableName == "public.user_tenants")
                                continue;

                            var check = PostgresHelper.Read(_configuration.GetConnectionString("StudioDBConnection"),
                                _command.Connection.Database,
                                $"SELECT column_name FROM information_schema.columns WHERE table_name='{tableName.Split("public.")[1]}' and column_name='id';",
                                "hasRows");

                            if (check)
                            {
                                if (sequences[tableName] == null)
                                {
                                    var arrayResult = PostgresHelper.Read(
                                        _configuration.GetConnectionString("StudioDBConnection"),
                                        _command.Connection.Database,
                                        $"SELECT last_value FROM {tableName.Split("public.")[1]}_id_seq;", "array");
                                    var value = 0;
                                    if (arrayResult != null)
                                    {
                                        foreach (var table in arrayResult)
                                        {
                                            value = (int)table["last_value"];
                                        }
                                    }

                                    var count = sqls.Count(x => x.Contains($"INSERT INTO {tableName}"));
                                    sequences[tableName] = value - (count - 1);
                                }
                                else
                                {
                                    sequences[tableName] = (int)sequences[tableName] + 1;
                                }

                                sqls[index] = sql.Replace(tableName + " (", tableName + " (id,")
                                    .Replace("VALUES (", $"VALUES ({sequences[tableName]},");
                            }

                            newRawQuery += sqls[index] + ";" + Environment.NewLine;
                        }
                        else
                        {
                            newRawQuery += sqls[index] + ";" + Environment.NewLine;
                        }
                    }

                    _queue.QueueBackgroundWorkItem(token =>
                        _historyHelper.Database(newRawQuery, executedAt, email, currentUser, (Guid)_lastCommandId));
                }

                _hastExecuting = false;
            }
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandError")]
        public void OnCommandError(Exception ex, bool async)
        {
            var currenUser = CurrentUser;

            if (_lastCommandId.HasValue && _hastExecuting)
                _queue.QueueBackgroundWorkItem(
                    token => _historyHelper.DeleteDbRecord(currenUser, (Guid)_lastCommandId));
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
                        value = "'" + value.Replace("'", "''").ToString() + "'";
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
                else if (query.Contains("= " + parameter.ParameterName + Environment.NewLine))
                {
                    // End of query parameters
                    query = query.Replace("= " + parameter.ParameterName + Environment.NewLine, "= " + value + Environment.NewLine);
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