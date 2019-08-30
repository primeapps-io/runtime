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
        private IPosgresHelper _posgresHelper;

        public Dump(IConfiguration configuration, IPosgresHelper posgresHelper)
        {
            _configuration = configuration;
            _posgresHelper = posgresHelper;
        }

        [QueueCustom]
        public void Create(JObject request)
        {
            var dumpDirectory = _configuration.GetValue("AppSettings:DumpDirectory", string.Empty);
            var appId = (int)request["app_id"];

            if (!Directory.Exists(dumpDirectory))
                Directory.CreateDirectory(dumpDirectory);

            try
            {
                _posgresHelper.Dump("StudioDBConnection", $"app{appId}", dumpDirectory, dumpDirectory);
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
            var dumpDirectory = _configuration.GetValue("AppSettings:DumpDirectory", string.Empty);
            var connectionStringName = (string)request["environment"] == "test" ? "PlatformDBConnectionTest" : "PlatformDBConnection";
            var appId = (int)request["app_id"];

            try
            {
                _posgresHelper.Create(connectionStringName, $"app{appId}_new");
                _posgresHelper.Restore(connectionStringName, $"app{appId}", dumpDirectory, $"app{appId}_new", dumpDirectory);
                _posgresHelper.Template(connectionStringName, $"app{appId}");
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