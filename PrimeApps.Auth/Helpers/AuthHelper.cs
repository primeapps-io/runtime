using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace PrimeApps.Auth.Helpers
{
	public class AuthHelper
	{
		public static JObject GetApplicationInfo(IConfiguration configuration, HttpRequest request, HttpResponse response, string returnUrl, IApplicationRepository applicationRepository)
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
			var json = @"{
							app: 'primeapps',
							title: 'PrimeApps',
							multi_language: true,
							logo: '" + cdnUrlStatic + "/images/login/logos/login_primeapps.png', " +
							"description: { banner1: { image: '', desc_tr: '', desc_en: '' }}," +
							"desc_tr:'BUILD POWERFUL BUSINESS APPS 10X FASTER', " +
							"desc_en:'BUILD POWERFUL BUSINESS APPS 10X FASTER', " +
							"color: '#555198', " +
							"banner: { images: ['/images/login/banner/primeapps-background.jpg'], descriptions: [{tr:'Personelinizin izin, avans, harcama, zimmet, eğitim ve özlük bilgilerini kolayca yönetin.', en: ''}]}," +
							"customDomain: false, " +
							"language: '" + primeLang + "', " +
							"favicon: '" + cdnUrlStatic + "/images/favicon/primeapps.ico'," +
							"cdnUrl: '" + cdnUrlStatic + "'" +
						"}";


			Uri url = null;

			if (!string.IsNullOrEmpty(returnUrl) && (returnUrl.Split("&")).Length > 1)
				url = new Uri(HttpUtility.UrlDecode((returnUrl.Split("&")).Where(x => x.Contains("redirect_uri")).FirstOrDefault().Split("redirect_uri=")[1]));

			if (url != null /*&& !url.Authority.Contains("localhost")*/)
			{
				App result = null;

				result = applicationRepository.Get(url.Authority);

				if (string.IsNullOrWhiteSpace(_language))
					_language = result.Setting.Language ?? "tr";

				if (result != null)
				{
					var app = result.Name;
					var logo = result.Logo ?? cdnUrlStatic + "/images/login/logos/login_primeapps.png";
					var title = result.Setting.Title ?? "";
					var description = result.Setting.Description ?? "";
					var color = result.Setting.Color ?? "#555198";
					var favicon = result.Setting.Favicon ?? cdnUrlStatic + "/images/favicon/primeapps.ico";
					var image = result.Setting.Image ?? null;
					var banner = result.Setting.Banner ?? cdnUrlStatic + "/images/login/banner/primeapps-background.jpg";
					var multiLanguage = result.UseTenantSettings;
					//Thread.CurrentThread.CurrentUICulture = lang == "en" ? new CultureInfo("en-GB") : new CultureInfo("tr-TR");
					json = @"{app: '" + app + "', " +
						"title: '" + title + "', " +
						"logo: '" + logo + "', " +
						"desc_tr:'" + description + "', " +
						"desc_en:'" + description + "', " +
						"color: '" + color + "', " +
						"customDomain: true, " +
						"language: '" + _language + "'," +
						"banner: '"+ banner +"'," +
						"favicon: '" + favicon + "'," +
						"customImage: '" + image + "'," +
						"multiLanguage: '" + multiLanguage + "'," +
						"cdnUrl: '" + cdnUrlStatic + "' }";

					//SetLanguage(response, request, _language);
					return JObject.Parse(json);

				}
			}
			
			//SetLanguage(response, request, _language);
			return JObject.Parse(json);
		}
		/*public static void SetLanguage(HttpResponse response, HttpRequest request, string lang)
		{
			response.Cookies.Append(
				CookieRequestCultureProvider.DefaultCookieName,
				CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(lang)),
				new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
			);
		}*/
	}
}
