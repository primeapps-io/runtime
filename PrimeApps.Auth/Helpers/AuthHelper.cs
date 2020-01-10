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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using PrimeApps.Model.Common.App;

namespace PrimeApps.Auth.UI
{
	public class AuthHelper
	{
		public static async Task<ApplicationInfoViewModel> GetApplicationInfo(IConfiguration configuration, HttpRequest request, string returnUrl, IApplicationRepository applicationRepository)
		{
			var language = !string.IsNullOrEmpty(request.Cookies[".AspNetCore.Culture"]) ? request.Cookies[".AspNetCore.Culture"].Split("uic=")[1] : null;

			//If user language did not detect in current screens, set browser default language.
			if (string.IsNullOrEmpty(language))
			{
				var defaultCulture = request.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture?.Culture?.Name;
				var browserLang = request.GetTypedHeaders().AcceptLanguage.First()?.Value.Value;
				language = (browserLang == "tr-TR" || browserLang == "tr" || defaultCulture == "tr-TR" || defaultCulture == "tr") ? "tr" : "en";
			}

			var cdnUrlStatic = "";
			var cdnUrl = configuration.GetValue("webOptimizer:cdnUrl", string.Empty);

			if (!string.IsNullOrEmpty(cdnUrl))
			{
				var versionStatic = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
				cdnUrlStatic = cdnUrl + "/" + versionStatic;
			}

			var redirectUri = GetQueryValue(returnUrl, "redirect_uri");
			var clientId = GetQueryValue(returnUrl, "client_id");
			App app;

			if (!string.IsNullOrWhiteSpace(redirectUri) && (redirectUri.StartsWith("https") || redirectUri.StartsWith("http")))
			{
				redirectUri = HttpUtility.UrlDecode(redirectUri);
				var domain = redirectUri.Replace("/signin-oidc", "").Replace("https://", "").Replace("http://", "");
				app = await applicationRepository.GetAppWithDomain(domain);
			}
			else
			{
				app = await applicationRepository.GetByNameAsync(clientId);
			}

			var previewMode = configuration.GetValue("AppSettings:PreviewMode", string.Empty);
			var preview = !string.IsNullOrEmpty(previewMode) && previewMode == "app";

			var defaultTheme = new JObject
			{
				["logo"] = "/images/logo.png",
				["favicon"] = "/images/primeapps.ico",
				["color"] = "#555198",
				["title"] = "PrimeApps",
				["banner"] = new JArray()
			};

			var banner = new JObject
			{
				["image"] = "/images/banner.svg",
				["descriptions"] = new JObject
				{
					["en"] = "Welcome to PrimeApps",
					["tr"] = "PrimeApps’e Hoşgeldiniz"
				}
			};

			((JArray)defaultTheme["banner"]).Add(banner);

			/*try
            {
                defaultTheme = JObject.Parse(app.Setting.AuthTheme);
            }
            catch (Exception e)
            {
                defaultTheme = JObject.Parse(JsonConvert.DeserializeObject(app.Setting.AuthTheme).ToString());
            }*/

			if (preview)
			{
				var previewAppId = GetQueryValue(returnUrl, "preview_app_id");

				var studioUrl = configuration.GetValue("AppSettings:StudioUrl", string.Empty);
				if (!string.IsNullOrEmpty(studioUrl) && !string.IsNullOrEmpty(previewAppId))
				{
					using (var httpClient = new HttpClient())
					{
						var url = studioUrl + "/api/app_draft/get_app_settings/" + previewAppId;

						httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

						var response = await httpClient.GetAsync(url);
						var content = await response.Content.ReadAsStringAsync();
						if (!response.IsSuccessStatusCode)
						{
							ErrorHandler.LogError(new Exception(content), "Status Code: " + response.StatusCode + " app_id: " + previewAppId);
						}

						var appSettings = JObject.Parse(content);

						if (appSettings != null && appSettings["auth_theme"] != null)
						{
							app.Setting.AuthTheme = (string)appSettings["auth_theme"];
						}
					}
				}
			}

			var multiLanguage = string.IsNullOrEmpty(app.Setting.Language);

			if (!multiLanguage)
				language = app.Setting.Language;

			JObject theme = null;

			try
			{
				theme = JObject.Parse(app.Setting.AuthTheme);
			}
			catch (Exception)
			{
				theme = JObject.Parse(JsonConvert.DeserializeObject(app.Setting.AuthTheme).ToString());
			}

			if (theme["banner"] == null)
			{
				theme["banner"] = new JArray();
				var themeBanner = new JObject
				{
					["image"] = null,
					["descriptions"] = null
				};
				((JArray)theme["banner"]).Add(themeBanner);
			}

			var storageUrl = configuration.GetValue("AppSettings:StorageUrl", string.Empty);

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

			if (theme["banner"][0]["descriptions"].IsNullOrEmpty())
			{
				theme["banner"][0]["descriptions"] = defaultTheme["banner"][0]["descriptions"];
			}

			if (theme["banner"][0]["image"].IsNullOrEmpty())
			{
				theme["banner"][0]["image"] = defaultTheme["banner"][0]["image"];
			}
			else
			{
				theme["banner"][0]["image"] = storageUrl + "/" + theme["banner"][0]["image"].ToString();
			}

			if (!theme["headLine"].IsNullOrEmpty())
			{
				if (!theme["headLine"]["en"].IsNullOrEmpty())
				{
					theme["headLine"]["en"] = theme["headLine"]["en"].ToString()
						.Replace("{auth_domain}", app.Setting.AuthDomain)
						.Replace("{client_id}", clientId);
				}

				if (!theme["headLine"]["tr"].IsNullOrEmpty())
				{
					theme["headLine"]["tr"] = theme["headLine"]["tr"].ToString()
						.Replace("{auth_domain}", app.Setting.AuthDomain)
						.Replace("{client_id}", clientId);
				}
			}

            var application = new ApplicationInfoViewModel
            {
                Id = app.Id,
                Name = app.Name,
                Title = theme["title"].ToString(),
                MultiLanguage = multiLanguage,
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
                    Options = new JObject { ["enable_registration"] = app.Setting.EnableRegistration, ["enable_api_registration"] = app.Setting.EnableAPIRegistration }
                },
                Preview = preview,
                Secret = app.Secret
            };

			return application;
		}

		public static string CurrentLanguage(HttpRequest request)
		{
			return !string.IsNullOrEmpty(request.Cookies[".AspNetCore.Culture"]) ? request.Cookies[".AspNetCore.Culture"].Split("uic=")[1] : null;
		}

		public static string GetQueryValue(string url, string parameter)
		{
			if (string.IsNullOrWhiteSpace(url))
				throw new Exception("Url cannot be null.");

			var returnUrl = HttpUtility.UrlDecode(url);

			if (!returnUrl.StartsWith("http://") || !returnUrl.StartsWith("https://"))
				returnUrl = "http://url.com" + returnUrl;

			var uri = new Uri(returnUrl);
			var value = HttpUtility.ParseQueryString(uri.Query).Get(parameter);

			if (!string.IsNullOrEmpty(value))
				return value;

			return null;
		}

		public static async Task<bool> TenantOperationWebhook(ApplicationInfoViewModel app, Tenant tenant, TenantUser tenantUser)
		{
			if (string.IsNullOrWhiteSpace(app.ApplicationSetting.TenantOperationWebhook))
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
					var response = await httpClient.PostAsync(app.ApplicationSetting.TenantOperationWebhook, new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

					if (!response.IsSuccessStatusCode)
					{
						var resp = await response.Content.ReadAsStringAsync();
						ErrorHandler.LogError(new Exception(resp), "Status Code: " + response.StatusCode + " tenant_id: " + tenant.Id + " first_name: " + tenantUser.FirstName + " last_name: " + tenantUser.LastName + " phone: " + tenantUser.Phone + " email: " + tenantUser.Email);
					}
				}
			}

			return true;
		}

		public static async Task<bool> StudioOperationWebhook(ApplicationInfoViewModel app, PlatformUser platformUser)
		{
			if (string.IsNullOrWhiteSpace(app.ApplicationSetting.TenantOperationWebhook))
				return true;

			using (var httpClient = new HttpClient())
			{
				if (platformUser.Id > 0)
				{
					var request = new JObject();
					request["user_id"] = platformUser.Id;
					request["first_name"] = platformUser.FirstName;
					request["last_name"] = platformUser.LastName;
					request["email"] = platformUser.Email;

					httpClient.DefaultRequestHeaders.Accept.Clear();
					httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
					var response = await httpClient.PostAsync(app.ApplicationSetting.TenantOperationWebhook, new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

					if (!response.IsSuccessStatusCode)
					{
						var resp = await response.Content.ReadAsStringAsync();
						ErrorHandler.LogError(new Exception(resp), "Status Code: " + response.StatusCode + " user_id: " + platformUser.Id + " first_name: " + platformUser.FirstName + " last_name: " + platformUser.LastName + " email: " + platformUser.Email);
					}
				}
			}

			return true;
		}
	}
}