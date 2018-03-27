using Newtonsoft.Json.Linq;
using PrimeApps.CLI.Helpers;
using System;
using System.Linq;

namespace PrimeApps.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length < 2 || string.IsNullOrEmpty(args[0]) || string.IsNullOrEmpty(args[1]))
            {
                Console.WriteLine("No command area and/or command specified. CLI will exit.");
                Environment.Exit(0);
            }

            string commandArea = args[0];
            string command = args[1];
            string param1 = args.Length > 2 ? args[2] : null;
            string param2 = args.Length > 3 ? args[3] : null;
            string connStr = null;
            string version = null;

            var multitenancyDatabase = new Multitenancy.Database();
            var result = new JObject();
            Exception exception = null;
            var deploy = false;

            try
            {
                switch (commandArea)
                {
                    case "tenantUpdate":
                        switch (command)
                        {
                            case "-all":
                                result = multitenancyDatabase.MigrateTenantDatabases(param1);
                                break;
                            case "-all-deploy":
                                deploy = true;
                                if (!string.IsNullOrWhiteSpace(param1) && param1.Contains("server="))
                                {
                                    connStr = param1;
                                    version = param2;
                                }
                                else if (!string.IsNullOrWhiteSpace(param2) && param2.Contains("server="))
                                {
                                    connStr = param2;
                                    version = param1;
                                }

                                result = multitenancyDatabase.MigrateTenantDatabases(version, connStr);

                                break;
                            default:
                                var tenantDatabaseName = command;
                                result = multitenancyDatabase.MigrateDatabase(tenantDatabaseName, param1);
                                break;
                        }
                        break;
                    case "templateUpdate":
                        switch (command)
                        {
                            case "-all":
                                result = multitenancyDatabase.MigrateTemplateDatabases(param1);
                                break;
                            case "-all-deploy":
                                deploy = true;

                                if (!string.IsNullOrWhiteSpace(param1) && param1.Contains("server="))
                                {
                                    connStr = param1;
                                    version = param2;
                                }
                                else if (!string.IsNullOrWhiteSpace(param2) && param2.Contains("server="))
                                {
                                    connStr = param2;
                                    version = param1;
                                }

                                result = multitenancyDatabase.MigrateTemplateDatabases(version, connStr);
                                break;
                        }
                        break;
                    case "runSql":
                        switch (command)
                        {
                            case "-template":
                                result = multitenancyDatabase.RunSqlTemplateDatabases(param1, param2);
                                break;
                            default:
                                result = multitenancyDatabase.RunSqlTenantDatabases(param1, param2);
                                break;
                        }
                        break;
                    default:
                        Console.WriteLine("No command area specified. CLI will exit.");
                        break;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                PrintResult(result, deploy, version, exception);
            }
        }

        static void PrintResult(JObject result, bool deploy, string version = null, Exception ex = null)
        {
            if (version != null)
            {
                Console.WriteLine("DbStatus:" + result.ToString() + " for version " + version);
            }
            else
            {
                Console.WriteLine("DbStatus:" + result.ToString());
            }
            if (((JArray)result["failed"]).Count() > 0 && deploy) throw new Exception("Aborted deployment because of unsuccesful migration.");

            if (ex != null) throw ex;
        }
    }
}
