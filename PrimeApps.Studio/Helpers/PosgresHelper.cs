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
                        var sw = new StreamWriter($"{logDirectory + Path.DirectorySeparatorChar + databaseName}_create.log");
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
                        var sw = new StreamWriter($"{logDirectory + Path.DirectorySeparatorChar + databaseName}_drop.log");
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

            var dumpFile = $"{dumpDirectory + Path.DirectorySeparatorChar + databaseName}.dmp";

            if (File.Exists(dumpFile))
                File.Delete(dumpFile);

            var postgresPath = _configuration.GetValue("AppSettings:PostgresPath", string.Empty);
            var fileName = !string.IsNullOrEmpty(postgresPath) ? postgresPath + "pg_dump" : "pg_dump";

            var arguments = $"-h {npgsqlConnection.Host} -U {npgsqlConnection.Username} -p {npgsqlConnection.Port} -Fc {databaseName} -f {dumpFile}";

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
                        var sw = new StreamWriter($"{logDirectory + Path.DirectorySeparatorChar + databaseName}_dump.log");
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

            var dumpFile = $"{dumpDirectory + Path.DirectorySeparatorChar + databaseName}.dmp";
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
                        var sw = new StreamWriter($"{logDirectory + Path.DirectorySeparatorChar + targetDatabaseName}_restore.log");
                        sw.WriteLine(reader.ReadToEnd());
                        sw.Close();
                    }
                }

                process.WaitForExit();
                process.Close();
            }
        }
    }
}