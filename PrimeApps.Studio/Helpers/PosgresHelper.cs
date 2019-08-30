using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace PrimeApps.Studio.Helpers
{
    public interface IPosgresHelper
    {
        void Create(string connectionStringName, string databaseName, string logDirectory = "", string locale = "");
        void Drop(string connectionStringName, string databaseName, string logDirectory = "", bool ifExist = true);
        void Dump(string connectionStringName, string databaseName, string dumpDirectory, string logDirectory = "");
        void Restore(string connectionStringName, string databaseName, string dumpDirectory, string targetDatabaseName = "", string logDirectory = "");
        void Template(string connectionStringName, string databaseName);
    }

    public class PosgresHelper : IPosgresHelper
    {
        private IConfiguration _configuration;

        public PosgresHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Create(string connectionStringName, string databaseName, string logDirectory = "", string locale = "")
        {
            var connectionString = _configuration.GetConnectionString(connectionStringName);
            var npgsqlConnection = new NpgsqlConnectionStringBuilder(connectionString);
            Environment.SetEnvironmentVariable("PGPASSWORD", npgsqlConnection.Password);

            if (string.IsNullOrEmpty(databaseName))
                databaseName = npgsqlConnection.Database;

            var postgresPath = _configuration.GetValue("AppSettings:PostgresPath", string.Empty);
            var fileName = !string.IsNullOrEmpty(postgresPath) ? postgresPath + "createdb" : "createdb";
            var localeStr = !string.IsNullOrEmpty(locale) ? $"--lc-ctype={locale} --lc-collate={locale}" : "";
            var arguments = $"-h {npgsqlConnection.Host} -U {npgsqlConnection.Username} -p {npgsqlConnection.Port} --template=template0 --encoding=UTF8 {localeStr} {databaseName}";

            var psi = new ProcessStartInfo();
            psi.FileName = fileName;
            psi.Arguments = arguments;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            using (var process = Process.Start(psi))
            {
                using (var reader = process.StandardError)
                {
                    if (!string.IsNullOrEmpty(logDirectory))
                    {
                        var path = Path.Combine(logDirectory, $"{databaseName}_create.log");
                        var sw = new StreamWriter(path);
                        sw.WriteLine(reader.ReadToEnd());
                        sw.Close();
                    }
                }

                process.WaitForExit();
                process.Close();
            }
        }

        public void Drop(string connectionStringName, string databaseName, string logDirectory = "", bool ifExist = true)
        {
            var connectionString = _configuration.GetConnectionString(connectionStringName);
            var npgsqlConnection = new NpgsqlConnectionStringBuilder(connectionString);
            Environment.SetEnvironmentVariable("PGPASSWORD", npgsqlConnection.Password);

            if (string.IsNullOrEmpty(databaseName))
                databaseName = npgsqlConnection.Database;

            var postgresPath = _configuration.GetValue("AppSettings:PostgresPath", string.Empty);
            var fileName = !string.IsNullOrEmpty(postgresPath) ? postgresPath + "dropdb" : "dropdb";
            var exists = !ifExist ? "--if-exists" : "";
            var arguments = $"-h {npgsqlConnection.Host} -U {npgsqlConnection.Username} -p {npgsqlConnection.Port} {exists} {databaseName}";

            var psi = new ProcessStartInfo();
            psi.FileName = fileName;
            psi.Arguments = arguments;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            using (var process = Process.Start(psi))
            {
                using (var reader = process.StandardError)
                {
                    if (!string.IsNullOrEmpty(logDirectory))
                    {
                        var path = Path.Combine(logDirectory, $"{databaseName}_drop.log");
                        var sw = new StreamWriter(path);
                        sw.WriteLine(reader.ReadToEnd());
                        sw.Close();
                    }
                }

                process.WaitForExit();
                process.Close();
            }
        }

        public void Dump(string connectionStringName, string databaseName, string dumpDirectory, string logDirectory = "")
        {
            var connectionString = _configuration.GetConnectionString(connectionStringName);
            var npgsqlConnection = new NpgsqlConnectionStringBuilder(connectionString);
            Environment.SetEnvironmentVariable("PGPASSWORD", npgsqlConnection.Password);

            if (string.IsNullOrEmpty(databaseName))
                databaseName = npgsqlConnection.Database;

            var dumpFile = Path.Combine(dumpDirectory, $"{databaseName}.dmp");

            if (File.Exists(dumpFile))
                File.Delete(dumpFile);

            var postgresPath = _configuration.GetValue("AppSettings:PostgresPath", string.Empty);
            var fileName = !string.IsNullOrEmpty(postgresPath) ? postgresPath + "pg_dump" : "pg_dump";

            var arguments = $"-h {npgsqlConnection.Host} -U {npgsqlConnection.Username} -p {npgsqlConnection.Port} -Fc {databaseName} -f {dumpFile} --clean --no-acl --no-owner";

            var psi = new ProcessStartInfo();
            psi.FileName = fileName;
            psi.Arguments = arguments;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            using (var process = Process.Start(psi))
            {
                using (var reader = process.StandardError)
                {
                    if (!string.IsNullOrEmpty(logDirectory))
                    {
                        var path = Path.Combine(logDirectory, $"{databaseName}_dump.log");
                        var sw = new StreamWriter(path);
                        sw.WriteLine(reader.ReadToEnd());
                        sw.Close();
                    }
                }

                process.WaitForExit();
                process.Close();
            }
        }

        public void Restore(string connectionStringName, string databaseName, string dumpDirectory, string targetDatabaseName = "", string logDirectory = "")
        {
            var connectionString = _configuration.GetConnectionString(connectionStringName);
            var npgsqlConnection = new NpgsqlConnectionStringBuilder(connectionString);
            Environment.SetEnvironmentVariable("PGPASSWORD", npgsqlConnection.Password);

            if (string.IsNullOrEmpty(databaseName))
                databaseName = npgsqlConnection.Database;

            var dumpFile = Path.Combine(dumpDirectory, $"{databaseName}.dmp");
            var postgresPath = _configuration.GetValue("AppSettings:PostgresPath", string.Empty);
            var fileName = !string.IsNullOrEmpty(postgresPath) ? postgresPath + "pg_restore" : "pg_restore";

            if (string.IsNullOrEmpty(targetDatabaseName))
                targetDatabaseName = databaseName;

            var arguments = $"-h {npgsqlConnection.Host} -U {npgsqlConnection.Username} -p {npgsqlConnection.Port} -Fc -d {targetDatabaseName} {dumpFile}";

            var psi = new ProcessStartInfo();
            psi.FileName = fileName;
            psi.Arguments = arguments;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            using (var process = Process.Start(psi))
            {
                using (var reader = process.StandardError)
                {
                    if (!string.IsNullOrEmpty(logDirectory))
                    {
                        var path = Path.Combine(logDirectory, $"{databaseName}_restore.log");
                        var sw = new StreamWriter(path);
                        sw.WriteLine(reader.ReadToEnd());
                        sw.Close();
                    }
                }

                process.WaitForExit();
                process.Close();
            }
        }

        public void Template(string connectionStringName, string databaseName)
        {
            var npgsqlConnectionString = new NpgsqlConnectionStringBuilder(connectionStringName);
            npgsqlConnectionString.Database = databaseName;

            using (var connection = new NpgsqlConnection(npgsqlConnectionString.ToString()))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    // Check database is exists
                    command.CommandText = $"SELECT EXISTS(SELECT datname FROM pg_catalog.pg_database WHERE datname = '{databaseName}');";

                    var isExists = (bool)command.ExecuteScalar();

                    // Terminate connections on new database
                    command.CommandText = $"SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE datname = '{databaseName}_new' and pid <> pg_backend_pid();";

                    var result = command.ExecuteNonQuery();

                    if (result > -1)
                        throw new Exception($"Template database connections cannot be terminated. Database name: {databaseName}");

                    // Set new database as template
                    command.CommandText = $"UPDATE pg_database SET datistemplate=true, datallowconn=false WHERE datname='{databaseName}_new';";

                    result = command.ExecuteNonQuery();

                    if (result < 1)
                        throw new Exception($"Template database cannot be set as a template database. Database name: {databaseName}");

                    if (isExists)
                    {
                        // Rename existing database as old
                        command.CommandText = $"ALTER DATABASE \"{databaseName}\" RENAME TO \"{databaseName}_old\";";
                    }

                    result = command.ExecuteNonQuery();

                    if (result > -1)
                        throw new Exception($"Template database cannot be renamed as old. Database name: {databaseName}");

                    // Remove _new suffix from new database
                    command.CommandText = $"ALTER DATABASE \"{databaseName}_new\" RENAME TO \"{databaseName}\";";

                    result = command.ExecuteNonQuery();

                    if (result > -1)
                        throw new Exception($"New template database cannot be renamed as {databaseName}");

                    if (!isExists)
                    {
                        // Unset old database as template
                        command.CommandText = $"UPDATE pg_database SET datistemplate=false WHERE datname='{databaseName}_old';";

                        result = command.ExecuteNonQuery();

                        if (result < 1)
                            ErrorHandler.LogError(new Exception($"Template database (old) cannot be unset as template. Database name:{databaseName}"));

                        // Drop old database
                        command.CommandText = $"DROP DATABASE \"{databaseName}_old\";";

                        result = command.ExecuteNonQuery();

                        if (result > -1)
                            ErrorHandler.LogError(new Exception($"Template database (old) cannot be droped. Database name: {databaseName}"));
                    }
                }

                connection.Close();
            }
        }
    }
}