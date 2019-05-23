using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common.App;

namespace PrimeApps.App.Helpers
{
	public static class AppHelper
	{
		public static async Task<ApplicationInfoViewModel> GetApplicationInfo(IConfiguration configuration, HttpRequest request, Model.Entities.Platform.App app, bool preview = false)
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

			if (preview)
			{
				var studioUrl = configuration.GetValue("AppSettings:StudioUrl", string.Empty);
				if (!string.IsNullOrEmpty(studioUrl))
				{
					using (var httpClient = new HttpClient())
					{
						var url = studioUrl + "/api/app_draft/get_app_settings/" + request.Cookies["app_id"];

						httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

						var response = await httpClient.GetAsync(url);
						var content = await response.Content.ReadAsStringAsync();
						if (!response.IsSuccessStatusCode)
						{
							ErrorHandler.LogError(new Exception(content), "Status Code: " + response.StatusCode + " app_id: " + request.Cookies["app_id"]);
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