using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Model.Helpers
{
    public static class Postgres
    {
        /// <summary>
        /// Creates connection string to the database of a tenant. If tenant id is lower then zero, connection string will be created without database, server level.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public static string GetConnectionString(string connectionString, int tenantId, string databasePrefix, string externalConnectionString = null)
        {
            var database = databasePrefix + tenantId;
            var builder = new DbConnectionStringBuilder(false);
            builder.ConnectionString = string.IsNullOrWhiteSpace(externalConnectionString) ? connectionString : externalConnectionString;

            if (tenantId < 0)
            {
                builder.Remove("database");
            }
            else
            {
                builder["database"] = database;
            }
            return builder.ConnectionString;
        }

        /// <summary>
        /// Creates connection string to a database.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public static string GetConnectionString(string connectionString, string databaseName, string externalConnectionString = null)
        {
            var builder = new DbConnectionStringBuilder(false);

            builder.ConnectionString = string.IsNullOrWhiteSpace(externalConnectionString) ? connectionString : externalConnectionString;

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                builder.Remove("database");
            }
            else
            {
                builder["database"] = databaseName;
            }
            return builder.ConnectionString;
        }

        /// <summary>
        /// Creates a new database with the given tenant id and app id template.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        public static async Task<bool> CreateDatabaseWithTemplate(string connectionString, int tenantId, int appId)
        {
            return await CreateDatabaseWithTemplate(connectionString, $"tenant{tenantId}", $"app{appId}");
        }

        public static async Task<bool> CreateDatabaseWithTemplate(string connectionString, string databaseName, string templateDbName)
        {
            bool result = false;
            int intResult = 0;

            try
            {
                using (var connection = new NpgsqlConnection(GetConnectionString(connectionString, -1, "tenant")))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"CREATE DATABASE \"{databaseName}\" ENCODING \"UTF8\" TEMPLATE \"{templateDbName}\"";

                        intResult = await command.ExecuteNonQueryAsync();
                        result = intResult < 0;
                    }

                    connection.Close();
                }

            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }
        /// <summary>
        /// Drops a tenant database if exists.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public static bool DropDatabase(string connectionString, int tenantId, bool terminateExistingConnections = false)
        {
            return DropDatabase(connectionString, $"tenant{tenantId}", terminateExistingConnections);
        }

        public static bool DropDatabase(string connectionString, string dbName, bool terminateExistingConnections = false)
        {
            bool result = false;
            try
            {
                using (var connection = new NpgsqlConnection(GetConnectionString(connectionString, -1, "tenant")))
                {
                    connection.Open();


                    using (var command = connection.CreateCommand())
                    {
                        if (terminateExistingConnections)
                        {
                            command.CommandText = $"SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '{dbName}'";
                            command.ExecuteScalar();
                        }

                        command.CommandText = $"DROP DATABASE IF EXISTS \"{dbName}\"";
                        result = command.ExecuteNonQuery() < 0;
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        public static IEnumerable<string> GetTenantDatabases(string connectionString, string externalConnectionString = null)
        {
            JArray dbs = new JArray();
            try
            {
                using (var connection = new NpgsqlConnection(GetConnectionString(connectionString, "postgres", externalConnectionString)))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT datname FROM pg_database WHERE datname LIKE 'tenant%' ORDER BY datname";

                        using (NpgsqlDataReader dataReader = command.ExecuteReader())
                        {
                            dbs = dataReader.MultiResultToJArray();
                        }

                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
            }

            return dbs.Select(x => x["datname"].ToString()).ToList();
        }

        public static IEnumerable<string> GetTemplateDatabases(string connectionString, string externalConnectionString = null)
        {
            JArray dbs = new JArray();
            try
            {
                using (var connection = new NpgsqlConnection(GetConnectionString(connectionString, "postgres", externalConnectionString)))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT datname FROM pg_database WHERE datname LIKE 'app%' OR datname LIKE 'templet%' ORDER BY datname";

                        using (NpgsqlDataReader dataReader = command.ExecuteReader())
                        {
                            dbs = dataReader.MultiResultToJArray();
                        }

                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
            }

            return dbs.Select(x => x["datname"].ToString()).ToList();
        }

        public static void PrepareTemplateDatabaseForUpgrade(string connectionString, string dbName, string externalConnectionString = null)
        {
            JArray dbs = new JArray();
            List<string> dbList = new List<string>();

            using (var connection = new NpgsqlConnection(GetConnectionString(connectionString, -1, externalConnectionString)))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    int intResult = 0;

                    command.CommandText = $"CREATE DATABASE \"{dbName}_new\" ENCODING \"UTF8\" TEMPLATE \"{dbName}\"";

                    intResult = command.ExecuteNonQuery();

                    if (intResult > -1) throw new Exception($"Template DB cannot be prepared for upgrade:{dbName}");
                }
                connection.Close();
            }
        }

        public static void FinalizeTemplateDatabaseUpgrade(string connectionString, string dbName, string externalConnectionString)
        {

            using (var connection = new NpgsqlConnection(GetConnectionString(connectionString, -1, externalConnectionString)))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {

                    int intResult = 0;


                    command.CommandText = $"update pg_database set datistemplate=true, datallowconn=false  where datname='{dbName}_new';";

                    intResult = command.ExecuteNonQuery();

                    if (intResult < 1) throw new Exception($"Template DB cannot set as a template database:{dbName}");


                    command.CommandText = $"alter database \"{dbName}\" rename to \"{dbName}_old\";";

                    intResult = command.ExecuteNonQuery();

                    if (intResult > -1) throw new Exception($"Template DB cannot be switched (RENAMEOLD):{dbName}");


                    command.CommandText = $"select pg_terminate_backend(pg_stat_activity.pid) from pg_stat_activity where datname = '{dbName}_new' and pid <> pg_backend_pid();";

                    intResult = command.ExecuteNonQuery();

                    if (intResult > -1) throw new Exception($"Template DB cannot be switched (TERMINATECONN):{dbName}");


                    command.CommandText = $"alter database \"{dbName}_new\" rename to \"{dbName}\";";

                    intResult = command.ExecuteNonQuery();

                    if (intResult > -1) throw new Exception($"Template DB cannot be switched (RENAMENEW):{dbName}");


                    command.CommandText = $"update pg_database set datistemplate=false where datname='{dbName}_old';";

                    intResult = command.ExecuteNonQuery();

                    if (intResult < 1) throw new Exception($"Template DB cannot be switched (NOTEMPLATE):{dbName}");


                    command.CommandText = $"drop database \"{dbName}_old\";";

                    intResult = command.ExecuteNonQuery();

                    if (intResult > -1) throw new Exception($"Template DB cannot be switched (DROP):{dbName}");

                }
                connection.Close();
            }
        }

        public static int ExecuteNonQuery(string connectionString, string databaseName, string sql, string externalConnectionString = null)
        {
            int result;

            using (var connection = new NpgsqlConnection(GetConnectionString(connectionString, databaseName, externalConnectionString)))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    result = command.ExecuteNonQuery();
                }
                connection.Close();
            }

            return result;
        }

        public static JArray ExecuteReader(string databaseName, string sql, string externalConnectionString = null)
        {
            JArray result;

            using (var connection = new NpgsqlConnection(GetConnectionString(databaseName, externalConnectionString)))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;

                    using (var reader = command.ExecuteReader())
                    {
                        result = reader.MultiResultToJArray();
                    }
                }
                connection.Close();
            }

            return result;
        }
    }
}
