using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common.Organization;
using PrimeApps.Model.Entities.Studio;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Threading.Tasks;

namespace PrimeApps.Admin.Helpers
{
    public class StudioClient : IDisposable
    {
        private HttpClient _client;
        private int _appId;
        private int _orgId;

        public StudioClient(IConfiguration configuration, string token, int appId = 0, int orgId = 0)
        {
            var apiBaseUrl = configuration.GetValue("AppSettings:StudioUrl", string.Empty) + "/api/";
            var useProxy = configuration.GetValue("Proxy:UseProxy", string.Empty);
            var proxyUrl = configuration.GetValue("Proxy:ProxyUrl", string.Empty);
            var certificateValidation = configuration.GetValue("Proxy:CertificateValidation", string.Empty);
            var httpClientHandler = new HttpClientHandler();

            if (!string.IsNullOrEmpty(useProxy) && bool.Parse(useProxy))
            {
                ICredentials credentials = null;

                if (proxyUrl.Contains('@'))
                {
                    var proxyUri = new Uri(proxyUrl);

                    if (proxyUri.UserInfo != null)
                    {
                        var userInfo = proxyUri.UserInfo.Split(':');
                        var userName = Uri.UnescapeDataString(userInfo[0]);
                        var password = Uri.UnescapeDataString(userInfo[1]);

                        credentials = new NetworkCredential(userName, password);
                    }
                }

                var proxy = new WebProxy(proxyUrl, false, null, credentials);

                httpClientHandler = new HttpClientHandler()
                {
                    Proxy = proxy,
                    ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) =>
                    {
                        if (!string.IsNullOrEmpty(certificateValidation) && !bool.Parse(certificateValidation))
                            return true;

                        return errors == SslPolicyErrors.None;
                    }
                };
            }

            var client = new HttpClient(httpClientHandler, disposeHandler: true);
            client.DefaultRequestHeaders.Accept.Clear();
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (orgId > 0)
            {
                client.DefaultRequestHeaders.Add("X-Organization-Id", orgId.ToString());
                _orgId = orgId;
            }

            if (appId > 0)
            {
                client.DefaultRequestHeaders.Add("X-App-Id", appId.ToString());
                _appId = appId;
            }

            _client = client;
        }

        public async Task<Package> PackageGetById(int id)
        {
            var response = await _client.GetAsync($"package/get/{id}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException();

                var errorData = await response.Content.ReadAsStringAsync();

                throw new Exception($"Method of Get Package result {response.StatusCode}. Application Id: {_appId}, Organization Id: {_orgId}, Response: {errorData}");
            }

            var data = await response.Content.ReadAsAsync<Package>();

            return data;
        }

        public async Task<Package> PackageLastDeployment()
        {
            var response = await _client.GetAsync($"package/get_last");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException();

                var errorData = await response.Content.ReadAsStringAsync();

                throw new Exception($"Method of Get Last Package result {response.StatusCode}. Application Id: {_appId}, Organization Id: {_orgId}, Response: {errorData}");
            }

            var data = await response.Content.ReadAsAsync<Package>();

            return data;
        }

        public async Task<List<Package>> PackageGetAll()
        {
            var response = await _client.GetAsync($"package/get_all/{_appId}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException();

                var errorData = await response.Content.ReadAsStringAsync();

                throw new Exception($"Method of Get Packages List result {response.StatusCode}. Application Id: {_appId}, Organization Id: {_orgId}, Response: {errorData}");
            }

            var data = await response.Content.ReadAsAsync<List<Package>>();

            return data;
        }

        public async Task<List<OrganizationModel>> OrganizationGetAllByUser()
        {
            var response = await _client.GetAsync($"user/organizations?includeApp=true");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException();

                var errorData = await response.Content.ReadAsStringAsync();

                throw new Exception($"Method of Get Organizations List result {response.StatusCode}. Application Id: {_appId}, Organization Id: {_orgId}, Response: {errorData}");
            }

            var data = await response.Content.ReadAsAsync<List<OrganizationModel>>();

            return data;
        }

        public async Task<Model.Common.User.StudioUser> GetUser()
        {
            var response = await _client.GetAsync($"user/me");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException();

                var errorData = await response.Content.ReadAsStringAsync();

                throw new Exception($"Method of Get User result {response.StatusCode}. Application Id: {_appId}, Organization Id: {_orgId}, Response: {errorData}");
            }

            var user = await response.Content.ReadAsAsync<Model.Common.User.StudioUser>();

            return user;
        }

        public async Task<AppDraft> AppDraftGetById(int id)
        {
            var response = await _client.GetAsync($"app/get/{id}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException();

                var errorData = await response.Content.ReadAsStringAsync();

                throw new Exception($"Method of Get App Draft result {response.StatusCode}. Application Id: {_appId}, Organization Id: {_orgId}, Response: {errorData}");
            }

            var data = await response.Content.ReadAsAsync<AppDraft>();

            return data;
        }

        public async Task<List<AppDraftTemplate>> GetAllAppTemplates()
        {
            var response = await _client.GetAsync($"template/get_all_by_app_id");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException();

                var errorData = await response.Content.ReadAsStringAsync();

                throw new Exception($"Method of Get App Draft Template result {response.StatusCode}. Application Id: {_appId}, Organization Id: {_orgId}, Response: {errorData}");
            }

            var data = await response.Content.ReadAsAsync<List<AppDraftTemplate>>();

            return data;
        }

        public void Dispose()
        {
        }
    }
}