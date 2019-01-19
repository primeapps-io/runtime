using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Auth.Helpers
{
    public interface IGiteaHelper
    {
        Task CreateUser(string email, string password, string fullName);
    }

    public class GiteaHelper : IGiteaHelper
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IHttpContextAccessor _context;
        private IConfiguration _configuration;

        public GiteaHelper(IHttpContextAccessor context,
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task CreateUser(string email, string password, string fullName)
        {
            if (!bool.Parse(_configuration.GetSection("AppSettings")["EnableGiteaIntegration"]))
                return;

            using (var httpClient = new HttpClient())
            {
                var request = new JObject
                {
                    ["email"] = email,
                    ["password"] = password,
                    ["full_name"] = fullName,
                    ["username"] = GetUserName(email)
                };

                SetHeaders(httpClient, _configuration.GetSection("AppSettings")["GiteaEmail"], _configuration.GetSection("AppSettings")["GiteaPassword"]);

                var response = await httpClient.PostAsync(_configuration.GetSection("AppSettings")["GiteaUrl"] + "/api/v1/admin/users", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    var resp = await response.Content.ReadAsStringAsync();
                    ErrorHandler.LogError(new Exception(resp), "Status Code: " + response.StatusCode + ", user: " + email + ", password: " + password);
                }

                await CreateAccessToken(email, password);
            }
        }

        public async Task CreateAccessToken(string email, string password)
        {
            if (!bool.Parse(_configuration.GetSection("AppSettings")["EnableGiteaIntegration"]))
                return;

            var username = GetUserName(email);
            using (var httpClient = new HttpClient())
            {
                var request = new JObject
                {
                    ["name"] = "primeapps"
                };

                SetHeaders(httpClient, email, password);

                var response = await httpClient.PostAsync(_configuration.GetSection("AppSettings")["GiteaUrl"] + "/api/v1/users/" + username + "/tokens", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    var resp = await response.Content.ReadAsStringAsync();
                    ErrorHandler.LogError(new Exception(resp), "Status Code: " + response.StatusCode + ", user: " + email + ", password: " + password);
                }
            }
        }

        private void SetHeaders(HttpClient client, string email, string password)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", GetBasicAuthToken(email, password));
        }

        private string GetBasicAuthToken(string email, string password)
        {
            byte[] bytes = Encoding.GetEncoding(28591).GetBytes(email + ":" + password);
            return Convert.ToBase64String(bytes);
        }

        private string GetUserName(string email)
        {
            var query = email.Replace("@", string.Empty).Split(".");
            Array.Resize(ref query, query.Length - 1);
            return string.Join("", query);
        }
    }
}
