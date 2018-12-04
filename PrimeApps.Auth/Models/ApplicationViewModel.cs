using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Auth.UI
{
    public class ApplicationViewModel
    {
        public Application ApplicationInfo { get; set; }
        public string Language { get; set; }
        public string Success { get; set; }
        public string Error { get; set; }

        public string ReturnUrl { get; set; }
    }

    public class Application
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Logo { get; set; }
        public string Color { get; set; }
        public bool CustomDomain { get; set; }
        public JObject Theme { get; set; }
        public string Language { get; set; }
        public string Domain { get; set; }
        public string Favicon { get; set; }
        public bool MultiLanguage { get; set; }
        public string CdnUrl { get; set; }
        public Settings Settings { get; set; }
    }

    public class Settings
    {
        public string Culture { get; set; }
        public string Currency { get; set; }
        public string TimeZone { get; set; }
        public string GoogleAnalytics { get; set; }
        public string ExternalLogin { get; set; }
		public string TenantOperationWebhook { get; set; }
	}
}
