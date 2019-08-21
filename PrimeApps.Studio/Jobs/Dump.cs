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
            var dumpDirectory = _configuration.GetValue("AppSettings:DumpDirectory", string.Empty);
            var postgresPath = _configuration.GetValue("AppSettings:PostgresPath", string.Empty);
            var dbConnection = _configuration.GetConnectionString("StudioDBConnection");

            var appId = (int)request["app_id"];

            if (!Directory.Exists(dumpDirectory))
                Directory.CreateDirectory(dumpDirectory);

            try
            {
                PosgresHelper.Dump(dbConnection, $"app{appId}", postgresPath, dumpDirectory, dumpDirectory);
            }
            catch (Exception ex)
            {
                var notification = new Helpers.Email(_configuration, null, "app" + appId + " Error", "app" + appId + " has error");
                notification.AddRecipient(request["notification_email"].ToString());
                notification.AddToQueue("notifications@primeapps.io", "PrimeApps", null, null, ex.Message + appId + " Error", "app" + appId + " has error");

                ErrorHandler.LogError(ex);
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
            var dumpDirectory = _configuration.GetValue("AppSettings:DumpDirectory", string.Empty);
            var postgresPath = _configuration.GetValue("AppSettings:PostgresPath", string.Empty);
            var dbConnection = _configuration.GetConnectionString("PlatformDBConnection");

            var appId = (int)request["app_id"];
            var appIdTarget = !request["app_id_target"].IsNullOrEmpty() ? (int)request["app_id_target"] : 0;

            try
            {
                PosgresHelper.Restore(dbConnection, $"app{appId}", postgresPath, dumpDirectory, appIdTarget > 0 ? $"app{appIdTarget}" : "", dumpDirectory);
            }
            catch (Exception ex)
            {
                var notification = new Helpers.Email(_configuration, null, "app" + appId + " Error", "app" + appId + " has error");
                notification.AddRecipient((string)request["notification_email"]);
                notification.AddToQueue("notifications@primeapps.io", "PrimeApps", null, null, ex.Message + appId + " Error", "app" + appId + " has error");

                ErrorHandler.LogError(ex);
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