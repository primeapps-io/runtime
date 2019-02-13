using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Context;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Console.Helpers
{
    public interface IGiteaHelper
    {
        Task CreateOrganization(string uniqueName, string fullName, UserItem appUser, string token);
        Task CreateRepository(int organizationId, string appName, UserItem appUser, string token);
    }

    public class GiteaHelper : IGiteaHelper
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IHttpContextAccessor _context;
        private IOrganizationRepository _organizationRepository;
        private IConfiguration _configuration;

        public GiteaHelper(IHttpContextAccessor context,
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            IOrganizationRepository organizationRepository)
        {
            _context = context;
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
            _organizationRepository = organizationRepository;
        }

        public async Task CreateOrganization(string uniqueName, string fullName, UserItem appUser, string token)
        {
            if (!bool.Parse(_configuration.GetSection("AppSettings")["EnableGiteaIntegration"]))
                return;
            //var userName = GetUserName(appUser.Email);

            using (var httpClient = new HttpClient())
            {
                var request = new JObject
                {
                    ["username"] = uniqueName,
                    ["full_name"] = fullName
                };

                SetHeaders(httpClient, appUser.Email, token);

                var response = await httpClient.PostAsync(_configuration.GetSection("AppSettings")["GiteaUrl"] + "/api/v1/orgs", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    var resp = await response.Content.ReadAsStringAsync();
                    ErrorHandler.LogError(new Exception(resp), "Status Code: " + response.StatusCode + ", user: " + appUser.Email + ", new organization name: " + uniqueName);
                }
            }
        }

        public async Task CreateRepository(int organizationId, string appName, UserItem appUser, string token)
        {
            if (!bool.Parse(_configuration.GetSection("AppSettings")["EnableGiteaIntegration"]))
                return;

            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<ConsoleDBContext>();
                using (var _organizationRepository = new OrganizationRepository(databaseContext, _configuration))
                {
                    var organization = await _organizationRepository.Get(appUser.Id, organizationId);

                    if (organization == null)
                    {
                        ErrorHandler.LogMessage("Organization not found for create repository in gitea helper." + "User: " + appUser.Email + ", organization id: " + organizationId + ", app name: " + appName);
                        return;
                    }

                    using (var httpClient = new HttpClient())
                    {
                        var request = new JObject
                        {
                            ["name"] = appName,
                            ["auto_init"] = true,
                            ["private"] = true,
                            ["readme"] = "Default"
                        };

                        SetHeaders(httpClient, appUser.Email, token);

                        var response = await httpClient.PostAsync(_configuration.GetSection("AppSettings")["GiteaUrl"] + "/api/v1/org/" + organization.Name + "/repos", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

                        var resp = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            ErrorHandler.LogError(new Exception(resp), "Status Code: " + response.StatusCode + ", user: " + appUser.Email + ", new organization name: " + organization.Name + ", app name: " + appName);
                        }
                        var cloneUrl = JObject.Parse(resp)["clone_url"].ToString();
                        var localFolder = _configuration.GetSection("AppSettings")["GiteaDirectory"] + appName;
                        var templateUrl = _configuration.GetSection("AppSettings")["GiteaUrl"] + "/primeapps/template.git";

                        //Clone auto generated repository in local folder.
                        var co = new CloneOptions
                        {
                            CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = token, Password = String.Empty }
                        };

                        Repository.Clone(cloneUrl, localFolder, co);

                        using (var repo = new Repository(localFolder))
                        {
                            repo.Network.Remotes.Add("template", templateUrl);
                            
                            repo.Network.Remotes.Update("template",
                                r => r.TagFetchMode = TagFetchMode.None);

                            FetchOptions options = new FetchOptions
                            {
                                CredentialsProvider = new CredentialsHandler(
                                (url, usernameFromUrl, types) =>
                                    new UsernamePasswordCredentials()
                                    {
                                        Username = token,
                                        Password = ""
                                    })
                            };

                            // Perform the actual fetch.
                            Commands.Fetch(repo, "template", new string[0], options, null);

                            
                        }
                    }
                }
            }
        }

        private void SetHeaders(HttpClient client, string email, string token)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", token);
        }

        /*c

        private string GetUserBasicAuthToken()
        {
            var email = _configuration.GetSection("AppSettings")["GiteaEmail"];
            var password = _configuration.GetSection("AppSettings")["GiteaPassword"];
            byte[] bytes = Encoding.GetEncoding(28591).GetBytes(email + ":" + password);
            return Convert.ToBase64String(bytes);
        }*/

        /*private async Task<string> GetAccessToken(string userName)
        {
            using (var httpClient = new HttpClient())
            {
                var request = new JObject
                {
                    ["name"] = "primeapps"
                };

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", GetUserBasicAuthToken());

                var response = await httpClient.PostAsync(_configuration.GetSection("AppSettings")["GiteaUrl"] + "/api/v1/users/" + userName + "/tokens", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
                var resp = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    ErrorHandler.LogError(new Exception(resp), "Status Code: " + response.StatusCode + ", GiteaHelper get access token method; username : " + userName);
                    return null;
                }

                return resp.ToString();
            }
        }*/

        private string GetUserName(string email)
        {
            var query = email.Replace("@", string.Empty).Split(".");
            Array.Resize(ref query, query.Length - 1);
            return string.Join("", query);
        }
    }
}
