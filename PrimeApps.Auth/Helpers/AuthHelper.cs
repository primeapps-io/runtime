using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace PrimeApps.Auth.Helpers
{
    public class AuthHelper
    {
		public static async Task<JObject> GetApplicationInfo(IConfiguration configuration, HttpRequest request, string returnUrl, string language)
		{

			//Thread.CurrentThread.CurrentUICulture = language == "en" ? new CultureInfo("en-GB") : new CultureInfo("tr-TR");

			var cdnUrlStatic = "";
			var cdnUrl = configuration.GetSection("webOptimizer")["cdnUrl"];
			if (!string.IsNullOrEmpty(cdnUrl))
			{
				var versionStatic = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
				cdnUrlStatic = cdnUrl + "/" + versionStatic;
			}

			var json = @"{
							app: 'primeapps', 
							title: 'PrimeApps', 
							logo: '" + cdnUrlStatic + "/images/login/logos/login_primeapps.png', " +
							"description: { banner1: { image: '', desc_tr: '', desc_en: '' }}," +
							"desc_tr:'BUILD POWERFUL BUSINESS APPS 10X FASTER', " +
							"desc_en:'BUILD POWERFUL BUSINESS APPS 10X FASTER', " +
							"color: '#555198', " +
							"customDomain: false, " +
							"language: '', " +
							"favicon: '" + cdnUrlStatic + "/images/favicon/primeapps.ico'," +
							"cdnUrl: '"+ cdnUrlStatic + "'" +
						"}";


			Uri url = null;

			if ((returnUrl.Split("&")).Length > 1)
				url = new Uri(HttpUtility.UrlDecode((returnUrl.Split("&")).Where(x => x.Contains("redirect_uri")).FirstOrDefault().Split("redirect_uri=")[1]));

			if (url != null && !url.Authority.Contains("localhost"))
			{
				var apiUrl = url.Scheme + "://" + url.Authority + "/api/platform/get_domain_info?domain=" + url.Authority;

				using (var httpClient = new HttpClient())
				{
					httpClient.BaseAddress = new Uri(apiUrl);
					httpClient.DefaultRequestHeaders.Accept.Clear();
					httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

					var response = await httpClient.GetAsync(apiUrl);
					var customerJsonString = await response.Content.ReadAsStringAsync();
					var deserialized = JsonConvert.DeserializeObject(custome‌​rJsonString);

					if (response.IsSuccessStatusCode)
					{
						//ViewBag.AppInfo = deserialized;

						using (var content = response.Content)
						{
							var result = content.ReadAsStringAsync().Result;
							if (result != "")
							{
								var jsonResult = JObject.Parse(result);
								var title = !string.IsNullOrEmpty(jsonResult["title"].ToString()) ? jsonResult["Title"] : "PrimeApps";
								var description = !string.IsNullOrEmpty(jsonResult["Description"].ToString())? jsonResult["Description"] : "";
								var color = !string.IsNullOrEmpty(jsonResult["Color"].ToString()) ? jsonResult["Color"] : "#555198";
								var lang = !string.IsNullOrEmpty(jsonResult["Language"].ToString())? (string)jsonResult["Language"] : string.Empty;
								var favicon = !string.IsNullOrEmpty(jsonResult["Favicon"].ToString()) ? jsonResult["Favicon"] : cdnUrlStatic + "/images/favicon/primeapps.ico";
								var image = !string.IsNullOrEmpty(jsonResult["Image"].ToString()) ? jsonResult["Image"] : null;
								//Thread.CurrentThread.CurrentUICulture = lang == "en" ? new CultureInfo("en-GB") : new CultureInfo("tr-TR");
								json = @"{app: 'primeapps', title: '" + title + "', logo: '" + jsonResult["Logo"] + "', desc_tr:'" + description + "', desc_en:'" + description + "', color: '" + color + "', customDomain: true, language: '" + lang + "', favicon: '" + favicon + "', customImage: '" + image + "' }";
								return JObject.Parse(json);
							}
						}
					}
				}
			}
			return JObject.Parse(json);
		}
		
		public static void SetLanguae(HttpRequest response, string lang)
		{
			if (string.IsNullOrWhiteSpace(lang))
				lang = "tr";

			/*var cookieVisitor = new HttpCookie("_lang", lang) { Expires = DateTime.Now.AddYears(20) };
			response.Cookies.Add(cookieVisitor);*/
		}

		public static string GetLanguage(HttpRequest response)
		{
			var lang = response.Cookies["_lang"];
			if (lang != null)
			{
				return lang.ToString();
			}

			SetLanguae(response, "tr");
			return "tr";
		}
	}
}
