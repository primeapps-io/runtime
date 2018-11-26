using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Auth.Helpers;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using Sentry;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace PrimeApps.Auth.UI
{
    public class AuthHelper
    {
        public static async Task<Application> GetApplicationInfoAsync(IConfiguration configuration, HttpRequest request, string returnUrl, IApplicationRepository applicationRepository)
        {
            var language = !string.IsNullOrEmpty(request.Cookies[".AspNetCore.Culture"]) ? request.Cookies[".AspNetCore.Culture"].Split("uic=")[1] : null;

            var cdnUrlStatic = "";
            var cdnUrl = configuration.GetSection("webOptimizer")["cdnUrl"];

            if (!string.IsNullOrEmpty(cdnUrl))
            {
                var versionStatic = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                cdnUrlStatic = cdnUrl + "/" + versionStatic;
            }

            var clientId = GetClientId(returnUrl);
            var app = await applicationRepository.GetByName(clientId);

            if (string.IsNullOrWhiteSpace(language))
                language = app.Setting.Language ?? "tr";

            var theme = JObject.Parse(app.Setting.AuthTheme);

            var application = new Application
            {
                Id = app.Id,
                Name = app.Name,
                Title = theme["title"].ToString(),
                MultiLanguage = string.IsNullOrEmpty(app.Setting.Language),
                Logo = app.Logo,
                Theme = theme,
                Color = theme["color"].ToString(),
                CustomDomain = false,
                Language = language,
                Favicon = theme["favicon"].ToString(),
                CdnUrl = cdnUrlStatic,
                Domain = app.Setting.AppDomain,
                Settings = new Settings
                {
                    Culture = app.Setting.Culture,
                    Currency = app.Setting.Currency,
                    TimeZone = app.Setting.TimeZone,
                    GoogleAnalytics = app.Setting.GoogleAnalyticsCode,
                    TenantOperationWebhook = app.Setting.TenantOperationWebhook,
                }
            };
           
            //ErrorHandler.LogMessage("GetApplicationInfoAsync:: ReturnUrl: " + returnUrl + " ClientId: " + clientId + " AppId: " + app.Id + " App: " + app + " Application: " + application, Sentry.Protocol.SentryLevel.Info);
            return application;
        }

        public static string CurrentLanguage(HttpRequest request)
        {
            return !string.IsNullOrEmpty(request.Cookies[".AspNetCore.Culture"]) ? request.Cookies[".AspNetCore.Culture"].Split("uic=")[1] : null;
        }

        public static string GetClientId(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new Exception("Url cannot be null.");

            var returnUrl = HttpUtility.UrlDecode(url);

            if (!returnUrl.StartsWith("http://") || !returnUrl.StartsWith("https://"))
                returnUrl = "http://url.com" + returnUrl;

            var uri = new Uri(returnUrl);
            var clientId = HttpUtility.ParseQueryString(uri.Query).Get("client_id");

            return clientId;
        }

        public static async Task<bool> TenantOperationWebhook(Application app, Tenant tenant, TenantUser tenantUser)
        {
            if (string.IsNullOrWhiteSpace(app.Settings.TenantOperationWebhook))
                return true;

            using (var httpClient = new HttpClient())
            {
                if (tenant.Id > 0)
                {
                    var request = new JObject();
                    request["tenant_id"] = tenant.Id;
                    request["first_name"] = tenantUser.FirstName;
                    request["last_name"] = tenantUser.LastName;
                    request["phone"] = tenantUser.Phone;
                    request["email"] = tenantUser.Email;

                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    var response = await httpClient.PostAsync(app.Settings.TenantOperationWebhook, new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

                    if (!response.IsSuccessStatusCode)
                    {
                        var resp = await response.Content.ReadAsStringAsync();
                        ErrorHandler.LogError(new Exception(resp), "Status Code: " + response.StatusCode + " tenant_id: " + tenant.Id + " first_name: " + tenantUser.FirstName + " last_name: " + tenantUser.LastName + " phone: " + tenantUser.Phone + " email: " + tenantUser.Email);
                    }
                }
            }

            return true;
        }
    }
}