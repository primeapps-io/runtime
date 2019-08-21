using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace PrimeApps.Model.Helpers
{
    public static class PosgresHelper
    {
        public static void Create(string connectionString, string databaseName, string postgresPath, string logDirectory = "", string locale = "")
        {
            var npgsqlConnection = new NpgsqlConnectionStringBuilder(connectionString);
            Environment.SetEnvironmentVariable("PGPASSWORD", npgsqlConnection.Password);

            if (string.IsNullOrEmpty(databaseName))
                databaseName = npgsqlConnection.Database;

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

        public static void Drop(string connectionString, string databaseName, string postgresPath, string logDirectory = "", bool ifExist = true)
        {
            var npgsqlConnection = new NpgsqlConnectionStringBuilder(connectionString);
            Environment.SetEnvironmentVariable("PGPASSWORD", npgsqlConnection.Password);

            if (string.IsNullOrEmpty(databaseName))
                databaseName = npgsqlConnection.Database;

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

        public static bool Dump(string connectionString, string databaseName, string postgresPath, string dumpDirectory, string logDirectory = "")
        {
            var npgsqlConnection = new NpgsqlConnectionStringBuilder(connectionString);
            Environment.SetEnvironmentVariable("PGPASSWORD", npgsqlConnection.Password);

            if (string.IsNullOrEmpty(databaseName))
                databaseName = npgsqlConnection.Database;

            var dumpFile = Path.Combine(dumpDirectory, $"{databaseName}.dmp");

            if (File.Exists(dumpFile))
                File.Delete(dumpFile);

            var fileName = !string.IsNullOrEmpty(postgresPath) ? postgresPath + "pg_dump" : "pg_dump";

            var arguments = $"-h {npgsqlConnection.Host} -U {npgsqlConnection.Username} -p {npgsqlConnection.Port} -Fc {databaseName} -f {dumpFile} --clean --no-acl --no-owner";

            var psi = new ProcessStartInfo();
            psi.FileName = fileName;
            psi.Arguments = arguments;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            try
            {
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

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static bool Restore(string connectionString, string databaseName, string postgresPath, string dumpDirectory, string targetDatabaseName = "", string logDirectory = "")
        {
            var npgsqlConnection = new NpgsqlConnectionStringBuilder(connectionString);
            Environment.SetEnvironmentVariable("PGPASSWORD", npgsqlConnection.Password);

            if (string.IsNullOrEmpty(databaseName))
                databaseName = npgsqlConnection.Database;

            var dumpFile = Path.Combine(dumpDirectory, $"{databaseName}.dmp");
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

            try
            {
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
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static bool Run(string connectionString, string databaseName, string script, string logDirectory = "")
        {
            var npgsqlConnection = new NpgsqlConnectionStringBuilder(connectionString);

            if (!string.IsNullOrEmpty(databaseName))
                npgsqlConnection.Database = databaseName;

            using (var conn = new NpgsqlConnection(npgsqlConnection.ToString()))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = script;

                    try
                    {
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        return true;
                    }
                    catch (Exception e)
                    {
                        return false;
                    }
                }
            }
        }

        public static NpgsqlDataReader Read(string connectionString, string databaseName, string script, string logDirectory = "")
        {
            var npgsqlConnection = new NpgsqlConnectionStringBuilder(connectionString);

            if (!string.IsNullOrEmpty(databaseName))
                npgsqlConnection.Database = databaseName;

            using (var conn = new NpgsqlConnection(npgsqlConnection.ToString()))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand(script, conn))
                {
                    try
                    {
                        return cmd.ExecuteReader();
                    }
                    catch (Exception e)
                    {
                        return null;
                    }
                }
            }
        }
    }
}