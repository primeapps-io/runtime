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
        Task CreateUser(string email, string password, string firstName, string lastName);
        Task<string> GetToken(string email, string password);
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


        public async Task CreateUser(string email, string password, string firstName, string lastName)
        {
            var enableGiteaIntegration = _configuration.GetValue("AppSettings:GiteaEnabled", string.Empty);
            if (!string.IsNullOrEmpty(enableGiteaIntegration))
            {
                if (!bool.Parse(enableGiteaIntegration))
                    return;
            }
            else
                return;

            var userName = GetUserName(email);

            using (var httpClient = new HttpClient())
            {
                var request = new JObject
                {
                    ["email"] = email,
                    ["full_name"] = firstName + " " + lastName,
                    ["login_name"] = email,
                    ["password"] = password,
                    ["send_notify"] = false,
                    ["must_change_password"] = false,
                    ["source_id"] = 0,
                    ["username"] = userName,
                    ["must_change_password"] = false
                };

                SetHeaders(client: httpClient, type: "basic", email: email);
                var giteaUrl = _configuration.GetValue("AppSettings:GiteaUrl", string.Empty);
                var response = new HttpResponseMessage();

                if (!string.IsNullOrEmpty(giteaUrl))
                {
                    response = await httpClient.PostAsync(giteaUrl + "/api/v1/admin/users", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
                }

                if (!response.IsSuccessStatusCode)
                {
                    var resp = await response.Content.ReadAsStringAsync();
                    ErrorHandler.LogError(new Exception(resp), "Status Code: " + response.StatusCode + ", user: " + email + ", password: " + password);
                }

                await CreateAccessToken(email, password);
            }
        }

        public async Task<string> GetToken(string email, string password)
        {
            var enableGiteaIntegration = _configuration.GetValue("AppSettings:GiteaEnabled", string.Empty);

            if (!string.IsNullOrEmpty(enableGiteaIntegration) && bool.Parse(enableGiteaIntegration))
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        SetHeaders(httpClient, "basic", email, password);
                        var userName = GetUserName(email);
                        var giteaUrl = _configuration.GetValue("AppSettings:GiteaUrl", string.Empty);
                        if (!string.IsNullOrEmpty(giteaUrl))
                        {
                            var response = await httpClient.GetAsync(giteaUrl + "/api/v1/users/" + userName + "/tokens");
                            var resp = await response.Content.ReadAsStringAsync();
                            var giteaResponse = JArray.Parse(resp);

                            return giteaResponse.Count > 0 ? giteaResponse[0]["sha1"].ToString() : string.Empty;
                        }
                        else
                            return string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogError(ex, "GiteaHelper Get Gitea Token. User:" + email);
                    return string.Empty;
                }
            }
            else
                return string.Empty;
        }

        private HttpClient SetHeaders(HttpClient client, string type, string email = null, string password = null, string token = null)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (type == "token")
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", token);
            else
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", GetUserBasicAuthToken(email, password));

            return client;
        }

        private string GetUserBasicAuthToken(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                var giteaEmail = _configuration.GetValue("AppSettings:GiteaEmail", string.Empty);
                var giteaPassword = _configuration.GetValue("AppSettings:GiteaPassword", string.Empty);

                if (!string.IsNullOrEmpty(giteaEmail) && !string.IsNullOrEmpty(giteaPassword))
                {
                    email = giteaEmail;
                    password = giteaPassword;
                }
                else
                {
                    throw new Exception("AppSettings:GiteaEmail and AppSettings:GiteaPassword cannot be null!");
                }
            }

            var basicAuthToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{email}:{password}"));

            return basicAuthToken;
        }

        public static string GetUserName(string email)
        {
            return string.Join("", (email.Replace("@", string.Empty)).Split("."));
        }

        public async Task CreateAccessToken(string email, string password)
        {
            var enableGiteaIntegration = _configuration.GetValue("AppSettings:GiteaEnabled", string.Empty);
            if (!string.IsNullOrEmpty(enableGiteaIntegration))
            {
                if (!bool.Parse(enableGiteaIntegration))
                    return;
            }
            else
                return;

            var username = GetUserName(email);
            using (var httpClient = new HttpClient())
            {
                var request = new JObject
                {
                    ["name"] = "primeapps"
                };

                SetHeaders(httpClient, "basic", email, password);

                var giteaUrl = _configuration.GetValue("AppSettings:GiteaUrl", string.Empty);
                var response = new HttpResponseMessage();

                if (!string.IsNullOrEmpty(giteaUrl))
                {
                    response = await httpClient.PostAsync(giteaUrl + "/api/v1/users/" + username + "/tokens", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
                }

                if (!response.IsSuccessStatusCode)
                {
                    var resp = await response.Content.ReadAsStringAsync();
                    ErrorHandler.LogError(new Exception(resp), "Status Code: " + response.StatusCode + ", user: " + email + ", password: " + password);
                }
            }
        }
    }
}