using Microsoft.AspNetCore.Hosting;
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
        public void Run(JObject request)
        {
            var dumpDirectory = _configuration.GetValue("AppSettings:DumpDirectory", string.Empty);

            if (!Directory.Exists(dumpDirectory))
                Directory.CreateDirectory(dumpDirectory);

            foreach (var id in request["app_ids"])
            {
                try
                {
                    _posgresHelper.Dump("StudioDBConnection", $"app{id}", dumpDirectory, dumpDirectory);
                }
                catch (Exception ex)
                {
                    var notification = new Helpers.Email(_configuration, null, "app" + id + " Error", "app" + id + " has error");
                    notification.AddRecipient(request["notification_email"].ToString());
                    notification.AddToQueue("notifications@primeapps.io", "PrimeApps", null, null, ex.Message + id + " Error", "app" + id + " has error");

                    ErrorHandler.LogError(ex);
                }
            }

            if (!request["notification_email"].IsNullOrEmpty())
            {
                var notification = new Helpers.Email(_configuration, null, "Database Dump Ready", "Database dump is ready!");
                notification.AddRecipient(request["notification_email"].ToString());
                notification.AddToQueue("notifications@primeapps.io", "PrimeApps", null, null, "Database Dump Ready", "Database Dump Ready");
            }
        }
    }
}