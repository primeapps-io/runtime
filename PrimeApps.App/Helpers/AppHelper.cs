using System.Reflection;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common.App;

namespace PrimeApps.App.Helpers
{
    public static class AppHelper
    {
        public static ApplicationInfoViewModel GetApplicationInfo(IConfiguration configuration, HttpRequest request, Model.Entities.Platform.App app)
        {
            var language = request.Cookies["_lang"];

            if (string.IsNullOrEmpty(language))
                language = "en";

            var cdnUrlStatic = "";
            var cdnUrl = configuration.GetValue("webOptimizer:cdnUrl", string.Empty);

            if (!string.IsNullOrEmpty(cdnUrl))
            {
                var versionStatic = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                cdnUrlStatic = cdnUrl + "/" + versionStatic;
            }

            var multiLanguage = string.IsNullOrEmpty(app.Setting.Language);

            if (!multiLanguage)
                language = app.Setting.Language;

            var theme = JObject.Parse(app.Setting.AppTheme);

            var application = new ApplicationInfoViewModel
            {
                Id = app.Id,
                Name = app.Name,
                Title = theme["title"].ToString(),
                MultiLanguage = string.IsNullOrEmpty(app.Setting.Language),
                Logo = theme["logo"].ToString(),
                Theme = theme,
                Color = theme["color"].ToString(),
                CustomDomain = false,
                Language = language,
                Favicon = theme["favicon"].ToString(),
                CdnUrl = cdnUrlStatic,
                Domain = app.Setting.AppDomain,
                ApplicationSetting = new ApplicationSettingViewModel
                {
                    Culture = app.Setting.Culture,
                    Currency = app.Setting.Currency,
                    TimeZone = app.Setting.TimeZone,
                    GoogleAnalytics = app.Setting.GoogleAnalyticsCode,
                    ExternalLogin = app.Setting.ExternalAuth,
                    RegistrationType = app.Setting.RegistrationType,
                    TenantOperationWebhook = app.Setting.TenantOperationWebhook,
                }
            };

            return application;
        }
    }
}