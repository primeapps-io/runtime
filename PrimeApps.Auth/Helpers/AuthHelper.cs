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
			var _language = !string.IsNullOrEmpty(request.Cookies[".AspNetCore.Culture"]) ? request.Cookies[".AspNetCore.Culture"].Split("uic=")[1] : null;

			var cdnUrlStatic = "";
			var cdnUrl = configuration.GetSection("webOptimizer")["cdnUrl"];
			if (!string.IsNullOrEmpty(cdnUrl))
			{
				var versionStatic = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
				cdnUrlStatic = cdnUrl + "/" + versionStatic;
			}

			var primeLang = _language ?? "tr";

			var application = new Application()
			{
				Name = "primeapps",
				MultiLanguage = true,
				Logo = cdnUrlStatic + "/images/login/logos/login_primeapps.png",
				Theme = new JObject
				{
					["title"] = "PrimeApps",
					["color"] = "#555198",
					["favicon"] = cdnUrlStatic + "/images/favicon/primeapps.ico",
					["banner"] = new JArray {
						new JObject {
							["image"] = "/images/login/banner/primeapps-background.jpg",
							["descriptions"] = new JObject {
								["en"] = "",
								["tr"] = "Personelinizin izin, avans, harcama, zimmet, eğitim ve özlük bilgilerini kolayca yönetin."
							}
						}
					}
				},
				CustomDomain = false,
				Language = primeLang,
				CdnUrl = cdnUrlStatic
			};

			var clientId = GetClientId(returnUrl);

			var validUrls = configuration.GetValue("AppSettings:ValidUrls", String.Empty);
			Array validUrlsArr = null;
			if (!string.IsNullOrEmpty(validUrls))
				validUrlsArr = validUrls.Split(";");

			if (!string.IsNullOrEmpty(clientId))
			{
				App result = null;

				result = await applicationRepository.GetByName(clientId);
				if (string.IsNullOrWhiteSpace(_language))
					_language = result.Setting.Language ?? "tr";

				if (result != null)
				{
					var appName = result.Name;
					string logo;
					JObject theme;

					if (Array.IndexOf(validUrlsArr, request.Host.Host) > -1)
					{
						theme = application.Theme;
						logo = cdnUrlStatic + "/images/login/logos/login_primeapps.png";
					}

					else
					{
						theme = !string.IsNullOrEmpty(result.Setting.AutTheme) ? JObject.Parse(result.Setting.AutTheme) : application.Theme;
						logo = result.Logo ?? cdnUrlStatic + "/images/login/logos/login_primeapps.png";
					}


					var multiLanguage = string.IsNullOrEmpty(result.Setting.Language);// result.UseTenantSettings;
					var domain = result.Setting.AppDomain;
					var appId = result.Id;
					//Thread.CurrentThread.CurrentUICulture = lang == "en" ? new CultureInfo("en-GB") : new CultureInfo("tr-TR");

					application = new Application()
					{
						Id = appId,
						Name = appName,
						Title = theme["title"].ToString(),
						MultiLanguage = multiLanguage,
						Logo = logo,
						Theme = theme,
						Color = theme["color"].ToString(),
						CustomDomain = false,
						Language = _language,
						Favicon = theme["favicon"].ToString(),
						CdnUrl = cdnUrlStatic,
						Domain = domain,
						Settings = new Settings
						{
							Culture = result.Setting.Culture,
							Currency = result.Setting.Currency,
							TimeZone = result.Setting.TimeZone,
							GoogleAnalytics = result.Setting.GoogleAnalyticsCode,
							TenantOperationWebhook = result.Setting.TenantOperationWebhook,
						}
					};
					//SetLanguage(response, request, _language);
					return application;

				}
			}

			//SetLanguage(response, request, _language);
			return application;
		}
		public static string CurrentLanguage(HttpRequest request)
		{
			return !string.IsNullOrEmpty(request.Cookies[".AspNetCore.Culture"]) ? request.Cookies[".AspNetCore.Culture"].Split("uic=")[1] : null;
		}

		public static string GetClientId(string url)
		{
			if (!string.IsNullOrEmpty(url))
			{
				var queryStrings = url.Split("&");
				if (queryStrings.Length > 0)
				{
					var clientIdIndex = Array.FindIndex(queryStrings, x => x.Contains("client_id"));
					if (clientIdIndex >= 0)
					{
						return queryStrings[clientIdIndex].Split("client_id=")[1];
					}
				}
			}
			return null;
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
						ErrorHandler.LogError(new Exception(resp),"Status Code: "+response.StatusCode +" tenant_id: " + tenant.Id + " first_name: " + tenantUser.FirstName + " last_name: " + tenantUser.LastName + " phone: " + tenantUser.Phone + " email: " + tenantUser.Email);
					}
				}
			}

			return true;
		}
	}
}
