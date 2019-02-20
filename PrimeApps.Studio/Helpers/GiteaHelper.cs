using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
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

namespace PrimeApps.Studio.Helpers
{
    public interface IGiteaHelper
    {
        Task<string> GetFile(string fileName, string organizationName, string appName, CustomCodeType type);
        void Push(Repository repo, string token);
        Task<JObject> GetRepositoryInfo(string token, string email, string repositoryName);
        void CloneRepository(string token, string cloneUrl, string localFolder);
        Task CreateUser(string email, string password, string firstName, string lastName, string orgName);
        Task CreateOrganization(string uniqueName, string fullName, string email, string token, string type = "token");
        Task CreateRepository(int organizationId, string appName, UserItem appUser, string token);
        Task<string> GetSHAToken(string email, string password);
        void DeleteDirectory(string targetDir);
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

        public async Task<string> GetFile(string fileName, string organizationName, string appName, CustomCodeType type)
        {
            using (var httpClient = new HttpClient())
            {
                SetHeaders(client: httpClient, type: "basic", email: _configuration.GetSection("AppSettings")["GiteaEmail"], password: _configuration.GetSection("AppSettings")["GiteaPassword"]);
                var url = string.Format(_configuration.GetSection("AppSettings")["GiteaUrl"] + "/{0}/{1}/raw/branch/master/{2}/{3}", organizationName, appName, type.ToString().ToLower(), fileName);
                var response = await httpClient.GetAsync(url);

                var resp = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    ErrorHandler.LogError(new Exception(resp), "Status Code: " + response.StatusCode + ",GetRepositoryInfo" );
                    return null;
                }

                return resp;
            };
        }

        public void Push(Repository repo, string token)
        {
            PushOptions options = GetOptions("push", token);
            repo.Network.Push(repo.Branches["master"], options);
        }

        public async Task<JObject> GetRepositoryInfo(string token, string email, string repositoryName)
        {
            using (var httpClient = new HttpClient())
            {
                SetHeaders(client: httpClient, type: "token", token: token);
                var url = string.Format(_configuration.GetSection("AppSettings")["GiteaUrl"] + "/api/v1/repos/{0}/{1}", GetUserName(email), repositoryName);
                var response = await httpClient.GetAsync(url);

                var resp = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    ErrorHandler.LogError(new Exception(resp), "Status Code: " + response.StatusCode + ",GetRepositoryInfo, token: " + token);
                    return null;
                }

                return JObject.Parse(resp);
            };
        }

        public void CloneRepository(string token, string cloneUrl, string localFolder)
        {
            var cloneOptions = GetOptions("clone", token);

            Repository.Clone(cloneUrl, localFolder, cloneOptions);
        }

        public async Task CreateUser(string email, string password, string firstName, string lastName, string orgName)
        {
            if (!bool.Parse(_configuration.GetSection("AppSettings")["EnableGiteaIntegration"]))
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
                    ["source_id"] = 0,
                    ["username"] = userName
                };

                SetHeaders(client: httpClient, type: "basic", email: email);

                var response = await httpClient.PostAsync(_configuration.GetSection("AppSettings")["GiteaUrl"] + "/api/v1/admin/users", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    var resp = await response.Content.ReadAsStringAsync();
                    ErrorHandler.LogError(new Exception(resp), "Status Code: " + response.StatusCode + ", user: " + email + ", password: " + password);
                }
                await CreateAccessToken(email, password);
            };
        }

        public async Task CreateOrganization(string uniqueName, string fullName, string email, string token, string type = "token")
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

                SetHeaders(client: httpClient, type: type, email: email, token: token);

                var response = await httpClient.PostAsync(_configuration.GetSection("AppSettings")["GiteaUrl"] + "/api/v1/orgs", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    var resp = await response.Content.ReadAsStringAsync();
                    ErrorHandler.LogError(new Exception(resp), "Status Code: " + response.StatusCode + ", user: " + email + ", new organization name: " + uniqueName);
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
                            ["auto_init"] = false,
                            ["private"] = true,
                            ["readme"] = "Default"
                        };

                        SetHeaders(client: httpClient, type: "token", email: appUser.Email, token: token);

                        var url = "/api/v1/org/" + organization.Name + "/repos";

                        if (GetUserName(appUser.Email) == organization.Name)
                            url = "/api/v1/user/repos";

                        var response = await httpClient.PostAsync(_configuration.GetSection("AppSettings")["GiteaUrl"] + url, new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

                        var resp = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            ErrorHandler.LogError(new Exception(resp), "Status Code: " + response.StatusCode + ", user: " + appUser.Email + ", new organization name: " + organization.Name + ", app name: " + appName);
                        }
                        var cloneUrl = JObject.Parse(resp)["clone_url"].ToString();
                        var localFolder = _configuration.GetSection("AppSettings")["GiteaDirectory"] + appName;
                        var templateUrl = _configuration.GetSection("AppSettings")["GiteaUrl"] + "/primeapps/template.git";

                        //Clone auto generated repository in local folder.
                        CloneRepository(token, cloneUrl, localFolder);

                        using (var repo = new Repository(localFolder))
                        {
                            repo.Network.Remotes.Add("template", templateUrl);

                            var fetchOptions = GetOptions("fetch", token);

                            foreach (Remote a in repo.Network.Remotes)
                            {
                                IEnumerable<string> refSpecs = a.FetchRefSpecs.Select(x => x.Specification);
                                Commands.Fetch(repo, a.Name, refSpecs, fetchOptions, "");
                            }

                            var signature = new Signature(
                                new Identity("system", "system@primeapps.io"), DateTimeOffset.Now);

                            Branch branch = repo.Branches["refs/remotes/template/master"];

                            repo.Merge(branch.Tip, signature);

                            Remote remote = repo.Network.Remotes["origin"];
                            var pushOptions = GetOptions("push", token);
                            repo.Network.Push(remote, @"refs/heads/master", pushOptions);

                            repo.Dispose();
                            //Delete created folder
                            DeleteDirectory(localFolder);
                        }
                    }
                }
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

                SetHeaders(client: httpClient, type: "basic", email: email, password: password);

                var response = await httpClient.PostAsync(_configuration.GetSection("AppSettings")["GiteaUrl"] + "/api/v1/users/" + username + "/tokens", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    var resp = await response.Content.ReadAsStringAsync();
                    ErrorHandler.LogError(new Exception(resp), "Status Code: " + response.StatusCode + ", user: " + email + ", password: " + password);
                }
            }
        }

        public async Task<string> GetSHAToken(string email, string password)
        {
            if (!bool.Parse(_configuration.GetSection("AppSettings")["EnableGiteaIntegration"]))
                return null;

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var request = new JObject
                    {
                        ["name"] = "primeapps"
                    };

                    SetHeaders(client: httpClient, type: "basic", email: email, password: password);
                    var userName = GetUserName(email);

                    var response = await httpClient.PostAsync(_configuration.GetSection("AppSettings")["GiteaUrl"] + "/api/v1/users/" + userName + "/tokens", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
                    var resp = await response.Content.ReadAsStringAsync();
                    var giteaResponse = JObject.Parse(resp);
                    return giteaResponse["sha1"].ToString();
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, "GiteaHelper Get Gitea Token. User:" + email);
                return null;
            }
        }

        public void DeleteDirectory(string targetDir)
        {
            File.SetAttributes(targetDir, FileAttributes.Normal);

            string[] files = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(targetDir, false);
        }

        private HttpClient SetHeaders(HttpClient client, string type, string email = null, string token = null, string password = null)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (type == "token")
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", token);
            else
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", GetUserBasicAuthToken(email, password));

            return client;
        }

        protected dynamic GetOptions(string type, string token)
        {
            var credential = new UsernamePasswordCredentials() { Username = token, Password = String.Empty };
            switch (type)
            {
                case "fetch":
                    return new FetchOptions
                    {
                        TagFetchMode = TagFetchMode.None,
                        CredentialsProvider = new CredentialsHandler((url, usernameFromUrl, types) => credential)
                    };
                case "clone":
                    return new CloneOptions
                    {
                        CredentialsProvider = (_url, _user, _cred) => credential
                    };
                case "push":
                    return new PushOptions
                    {
                        CredentialsProvider = (_url, _user, _cred) => credential
                    };
                default:
                    return null;
            }
        }

        private string GetUserBasicAuthToken(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                email = _configuration.GetSection("AppSettings")["GiteaEmail"];
                password = _configuration.GetSection("AppSettings")["GiteaPassword"];
            }

            byte[] bytes = Encoding.GetEncoding(28591).GetBytes(email + ":" + password);
            return Convert.ToBase64String(bytes);
        }

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
