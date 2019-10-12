using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Helpers;
using PrimeApps.Studio.ActionFilters;
using PrimeApps.Studio.Helpers;

namespace PrimeApps.Studio.Jobs
{
    public class Dump
    {
        private IConfiguration _configuration;

        public Dump(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [QueueCustom]
        public void Create(JObject request)
        {
            var dumpDirectory = _configuration.GetValue("AppSettings:DataDirectory", string.Empty);
            var postgresPath = _configuration.GetValue("AppSettings:PostgresPath", string.Empty);
            var dbConnection = _configuration.GetConnectionString("StudioDBConnection");

            var appId = (int)request["app_id"];

            if (!Directory.Exists(dumpDirectory))
                Directory.CreateDirectory(dumpDirectory);

            try
            {
                PostgresHelper.Dump(dbConnection, $"app{appId}", postgresPath, dumpDirectory, dumpDirectory);
            }
            catch (Exception ex)
            {
                var notification = new Helpers.Email(_configuration, null, "app" + appId + " Error", "app" + appId + " has error");
                notification.AddRecipient(request["notification_email"].ToString());
                notification.AddToQueue("notifications@primeapps.io", "PrimeApps", null, null, ex.Message + ". app" + appId + " Error", "app" + appId + " has error");

                ErrorHandler.LogError(ex);
                return;
            }

            if (!request["notification_email"].IsNullOrEmpty())
            {
                var notification = new Helpers.Email(_configuration, null, "Database Dump Ready", "Database dump is ready.");
                notification.AddRecipient((string)request["notification_email"]);
                notification.AddToQueue("notifications@primeapps.io", "PrimeApps", null, null, "Database dump is ready.", "Database Dump Ready");
            }
        }

        [QueueCustom]
        public void Restore(JObject request)
        {
            var dumpDirectory = _configuration.GetValue("AppSettings:DataDirectory", string.Empty);
            var postgresPath = _configuration.GetValue("AppSettings:PostgresPath", string.Empty);
            var connectionStringName = (string)request["environment"] == "test" ? "PlatformDBConnectionTest" : "PlatformDBConnection";

            var dbConnection = _configuration.GetConnectionString(connectionStringName);

            var appId = (int)request["app_id"];

            try
            {
                PostgresHelper.Create(dbConnection, $"app{appId}_new", postgresPath);
                PostgresHelper.Restore(dbConnection, $"app{appId}", dumpDirectory, $"app{appId}_new", dumpDirectory);
                PostgresHelper.Template(dbConnection, $"app{appId}");
            }
            catch (Exception ex)
            {
                var notification = new Helpers.Email(_configuration, null, "app" + appId + " Error", "app" + appId + " has error");
                notification.AddRecipient((string)request["notification_email"]);
                notification.AddToQueue("notifications@primeapps.io", "PrimeApps", null, null, ex.Message + ". app" + appId + " Error", "app" + appId + " has error");

                ErrorHandler.LogError(ex);
                return;
            }

            if (!request["notification_email"].IsNullOrEmpty())
            {
                var notification = new Helpers.Email(_configuration, null, "Database Dump Restored", "Database dump is restored.");
                notification.AddRecipient(request["notification_email"].ToString());
                notification.AddToQueue("notifications@primeapps.io", "PrimeApps", null, null, "Database dump is restored.", "Database Dump Restored");
            }
        }
    }
}