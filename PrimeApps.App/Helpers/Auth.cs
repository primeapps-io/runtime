using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Web;
using System.Threading.Tasks;
using PrimeApps.App.Models;
using System.Security.Claims;
using System.Threading;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform.Identity;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Helpers;
using System.Configuration;

namespace PrimeApps.App.Helpers
{
    public class Auth : IDisposable
    {
        private PlatformDBContext _ctx;

        public Auth()
        {
            _ctx = new PlatformDBContext();
        }

        public async Task<bool> AddClient(Client client)
        {

            var existingToken = _ctx.Clients.SingleOrDefault(r => r.Name == client.Name && r.ApplicationType == client.ApplicationType);

            if (existingToken != null)
                return false;

            _ctx.Clients.Add(client);

            return await _ctx.SaveChangesAsync() > 0;
        }

        public Client FindClient(string clientId)
        {
            var client = _ctx.Clients.Find(clientId);

            return client;
        }

        public async Task<bool> AddRefreshToken(RefreshToken token)
        {

            var existingToken = _ctx.RefreshTokens.SingleOrDefault(r => r.Subject == token.Subject && r.ClientId == token.ClientId);

            if (existingToken != null)
            {
                var result = await RemoveRefreshToken(existingToken);
            }

            _ctx.RefreshTokens.Add(token);

            return await _ctx.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveRefreshToken(string refreshTokenId)
        {
            var refreshToken = await _ctx.RefreshTokens.FindAsync(refreshTokenId);

            if (refreshToken != null)
            {
                _ctx.RefreshTokens.Remove(refreshToken);
                return await _ctx.SaveChangesAsync() > 0;
            }

            return false;
        }

        public async Task<RefreshToken> FindRefreshToken(string refreshTokenId)
        {
            var refreshToken = await _ctx.RefreshTokens.FindAsync(refreshTokenId);

            return refreshToken;
        }

        public async Task<bool> RemoveRefreshToken(RefreshToken refreshToken)
        {
            _ctx.RefreshTokens.Remove(refreshToken);
            return await _ctx.SaveChangesAsync() > 0;
        }

        public List<RefreshToken> GetAllRefreshTokens()
        {
            return _ctx.RefreshTokens.ToList();
        }

        public void Dispose()
        {
            _ctx.Dispose();

        }
    }

    public static class AuthHelper
    {
        public static string GetHash(string input)
        {
            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();
            var byteValue = System.Text.Encoding.UTF8.GetBytes(input);
            var byteHash = hashAlgorithm.ComputeHash(byteValue);

            return Convert.ToBase64String(byteHash);
        }

        /// <summary>
        /// Creates a new CurrentPrinciple object for the active thread.
        /// It is helpful for the services like Account and Public, which doesn't require activation to make tenant specific operations.
        /// Important: This method is currently experimental. It must be tested carefully for the usage scenario.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="autoId"></param>
        public static void CreateCurrentPrincipal(string tenantId, string autoId)
        {

            var claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new Claim("tenant_id", tenantId));
            claimsIdentity.AddClaim(new Claim("user_id", autoId));
            Thread.CurrentPrincipal = new ClaimsPrincipal(claimsIdentity);
        }

        public static string AppInfo = "";
        public static async Task<JObject> GetApplicationInfo(HttpRequestBase request, string language)
        {
            var url = request.Url.Host;
            var json = "";
            Thread.CurrentThread.CurrentUICulture = language == "en" ? new CultureInfo("en-GB") : new CultureInfo("tr-TR");

            var useCdn = bool.Parse(ConfigurationManager.AppSettings["UseCdn"]);
            var cdnUrlStatic = "";

            if (useCdn)
            {
                var versionStatic = ((AssemblyVersionStaticAttribute)System.Reflection.Assembly.GetAssembly(typeof(Auth)).GetCustomAttributes(typeof(AssemblyVersionStaticAttribute), false)[0]).Version;
                cdnUrlStatic = ConfigurationManager.AppSettings["CdnUrl"] + "/" + versionStatic;
            }

            var index = request.Url.OriginalString.IndexOf(request.Url.PathAndQuery);
            var apiUrl = request.Url.PathAndQuery != "/" ? request.Url.OriginalString.Remove(index) + "/api/Public/GetCustomInfo?customDomain=" + request.Url.Authority : request.Url.OriginalString.Remove(request.Url.OriginalString.Length - 1) + "/api/Public/GetCustomInfo?customDomain=" + request.Url.Authority;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.GetAsync(apiUrl);
                var res = "";

                if (response.IsSuccessStatusCode)
                {
                    using (var content = response.Content)
                    {
                        res = content.ReadAsStringAsync().Result;
                        if (res != "")
                        {
                            var result = JObject.Parse(res);
                            var title = !result["Title"].IsNullOrEmpty() ? result["Title"] : "PrimeApps";
                            var description = !result["Description"].IsNullOrEmpty() ? result["Description"] : "";
                            var color = !result["Color"].IsNullOrEmpty() ? result["Color"] : "#555198";
                            var lang = !result["Language"].IsNullOrEmpty() ? (string)result["Language"] : string.Empty;
                            var favicon = !result["Favicon"].IsNullOrEmpty() ? result["Favicon"] : cdnUrlStatic + "/images/favicon/primeapps.ico";
                            json = @"{app: 'primeapps', title: '" + title + "', logo: '" + result["Logo"] + "', desc_tr:'" + description + "', desc_en:'" + description + "', color: '" + color + "', customDomain: true, language: '" + lang + "', favicon: '" + favicon + "' }";
                            AppInfo = json;
                            return JObject.Parse(json);
                        }
                    }
                }
            }

            if (url.Contains("kobi.ofisim.com") || url.Contains("kobi-test.ofisim.com"))
            {
                json = @"{app: 'kobi', title: 'Ofisim Kobi', logo: '" + cdnUrlStatic + "/images/login/logos/login_kobi.png', desc_tr:'Müşterilerinizi, tekliflerinizi, cari hesaplarınızı ve stok bilgilerinizi kolayca yönetin.', desc_en:'Easily manage your customers, proposals, current accounts and stock information.', color: '#4eb739', language: '', favicon: '" + cdnUrlStatic + "/images/favicon/ofisim.ico' }";
            }
            else if (url.Contains("asistan.ofisim.com") || url.Contains("asistan-test.ofisim.com"))
            {
                json = @"{app: 'asistan', title: 'Ofisim Asistan', logo: '" + cdnUrlStatic + "/images/login/logos/login_asistan.png', desc_tr:'Asistanınız ve siz tüm işlerinizi tek bir uygulamada kolayca takip edin ve yönetin.', desc_en:'Your assistant and you can easily track and manage all your work in one application.', color: '#ed5f4e', language: 'tr', favicon: '" + cdnUrlStatic + "/images/favicon/ofisim.ico' }";
            }
            else if (url.Contains("ik.ofisim.com") || url.Contains("ik-test.ofisim.com") || url.Contains("ik-dev.ofisim.com"))
            {
                json = @"{app: 'ik', title: 'Ofisim İK', logo: '" + cdnUrlStatic + "/images/login/logos/login_ik.png', desc_tr:'Personelinizin izin, avans, harcama, zimmet, eğitim ve özlük bilgilerini kolayca yönetin.', desc_en:'Manage your employees leaves, advances, expenses, trainings and personal information easily.', color: '#46428f', language: 'tr', favicon: '" + cdnUrlStatic + "/images/favicon/ofisim.ico' }";
            }
            else if (url.Contains("cagri.ofisim.com") || url.Contains("cagri-test.ofisim.com"))
            {
                json = @"{app: 'cagri', title: 'Ofisim Çağrı', logo: '" + cdnUrlStatic + "/images/login/logos/login_cagri.png', desc_tr:'Çağrı merkezi performansınızı, telefonda satış gücünüzü ve destek kalitenizi arttırın.', desc_en:'Increase your call center performance, sales force and support quality.', color: '#77a6ff', language: 'tr', favicon: '" + cdnUrlStatic + "/images/favicon/ofisim.ico' }";
            }
            else if (url.Contains("crm.ofisim.com") || url.Contains("test.ofisim.com") || url.Contains("dev.ofisim.com"))
            {
                json = @"{app: 'crm', title: 'Ofisim CRM', logo: '" + cdnUrlStatic + "/images/login/logos/login_crm.png', desc_tr:'Satış, pazarlama ve müşteri takibi faaliyetlerinizi tek bir uygulama üzerinden kolayca yönetin.', desc_en:'Easily manage your sales, marketing and customer-driven activities from a single application.', color: '#135dea', language: '', favicon: '" + cdnUrlStatic + "/images/favicon/ofisim.ico' }";
            }
            else if (url.Contains("primeapps.io"))
            {
                json = @"{app: 'primeapps', title: 'PrimeApps', logo: '" + cdnUrlStatic + "/images/login/logos/login_primeapps.png', desc_tr:'', desc_en:'', color: '#555198', language: '', favicon: '" + cdnUrlStatic + "/images/favicon/primeapps.ico' }";
            }
            else
            {
                json = @"{app: 'primeapps', title: 'PrimeApps', logo: '" + cdnUrlStatic + "/images/login/logos/login_primeapps.png', desc_tr:'', desc_en:'', color: '#555198', language: '', favicon: '" + cdnUrlStatic + "/images/favicon/primeapps.ico' }";
            }

            AppInfo = json;
            return JObject.Parse(json);
        }
    }
}