using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common.App;
using System.Web;
using Newtonsoft.Json;
using PrimeApps.Model.Helpers;

namespace PrimeApps.App.Helpers
{
    public static class AppHelper
    {
        public static string GetPreviewApp(string preview)
        {
            var previewToken = HttpUtility.UrlDecode(preview);
            var previewDB = CryptoHelper.Decrypt(previewToken.Replace(" ", "+"));
            if (!string.IsNullOrEmpty(previewDB))
                return previewDB;

            return null;
        }

        public static async Task<ApplicationInfoViewModel> GetApplicationInfo(IConfiguration configuration, HttpRequest request, Model.Entities.Platform.App app, int? appId, bool preview = false)
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

            var defaultTheme = new JObject
            {
                ["logo"] = "/images/logo.jpg",
                ["favicon"] = "/images/favicon.ico",
                ["color"] = "#555198",
                ["title"] = "PrimeApps",
            };
            
            /*try
            {
                defaultTheme = JObject.Parse(app.Setting.AppTheme);
            }
            catch (Exception e)
            {
                defaultTheme = JObject.Parse(JsonConvert.DeserializeObject(app.Setting.AppTheme).ToString());
            }*/

            if (preview)
            {
                var studioUrl = configuration.GetValue("AppSettings:StudioUrl", string.Empty);
                if (!string.IsNullOrEmpty(studioUrl) && appId != null && appId > 0)
                {
                    using (var httpClient = new HttpClient())
                    {
                        var url = studioUrl + "/api/app_draft/get_app_settings/" + appId;

                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = await httpClient.GetAsync(url);
                        var content = await response.Content.ReadAsStringAsync();
                        if (!response.IsSuccessStatusCode)
                        {
                            ErrorHandler.LogError(new Exception(content), "Status Code: " + response.StatusCode + " app_id: " + appId);
                        }

                        var appSettings = JObject.Parse(content);
                        if (appSettings != null && appSettings["app_theme"] != null)
                        {
                            app.Setting.AppTheme = (string)appSettings["app_theme"];
                        }
                    }
                }
            }

            var multiLanguage = string.IsNullOrEmpty(app.Setting.Language);

            if (!multiLanguage)
                language = app.Setting.Language;

            JObject theme = null;

            var storageUrl = configuration.GetValue("AppSettings:StorageUrl", string.Empty);

            try
            {
                theme = JObject.Parse(app.Setting.AppTheme);
            }
            catch (Exception)
            {
                theme = JObject.Parse(JsonConvert.DeserializeObject(app.Setting.AppTheme).ToString());
            }

            //Preview mode'ta eğer ilgili app'e ait branding ayarları yoksa defaultta Primeapps
            if (theme["title"].IsNullOrEmpty())
            {
                theme["title"] = defaultTheme["title"];
            }

            if (theme["logo"].IsNullOrEmpty())
            {
                theme["logo"] = defaultTheme["logo"];
            }
            else
            {
                theme["logo"] = storageUrl + "/" + theme["logo"];
            }

            if (theme["color"].IsNullOrEmpty())
            {
                theme["color"] = defaultTheme["color"];
            }

            if (theme["favicon"].IsNullOrEmpty())
            {
                theme["favicon"] = defaultTheme["favicon"];
            }
            else
            {
                theme["favicon"] = storageUrl + "/" + theme["favicon"];
            }

            var application = new ApplicationInfoViewModel
            {
                Id = app.Id,
                Name = app.Name,
                Description = app.Description,
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