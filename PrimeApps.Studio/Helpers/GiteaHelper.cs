using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IdentityModel.Client;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.Component;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Models;

namespace PrimeApps.Studio.Helpers
{
    public interface IGiteaHelper
    {
        Task SyncCollaborators(int organizationId, string operation, int appId, int? teamId, int? userId);
        JArray GetFileNames(string location, string path, bool getControllerName = false);
        Task<string> GetFile(string fileName, string organizationName, string appName, CustomCodeType type);
        void Push(Repository repo);
        Task<JObject> GetRepositoryInfo(string repositoryName, int organizationId);
        string CloneRepository(string cloneUrl, string folderName, bool deleteIfExist = true);
        Task CreateOrganization(string uniqueName, string fullName, string email, string type = "token");
        Task CreateRepository(int organizationId, string appName, UserItem appUser);
        Task<string> GetSHAToken(string email, string password);
        void DeleteDirectory(string targetDir);
        string GetToken();
        string SetToken(string giteaToken = null);
    }

    public class GiteaHelper : IGiteaHelper
    {
        private string Token
        {
            get => SetToken();
            set => AccessToken = value;
        }

        private string AccessToken { get; set; }
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
            Token = SetToken();
        }

        public async Task SyncCollaborators(int organizationId, string operation, int appId, int? teamId, int? userId)
        {
            var giteaUrl = _configuration.GetValue("AppSettings:GiteaUrl", string.Empty);
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var cacheHelper = scope.ServiceProvider.GetRequiredService<ICacheHelper>();

                var databaseContext = scope.ServiceProvider.GetRequiredService<StudioDBContext>();
                var platformDbContext = scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
                using (var appDraftRepository = new AppDraftRepository(databaseContext, _configuration))
                using (var collaboratorsRepository = new CollaboratorsRepository(databaseContext, _configuration))
                {
                    var userIds = new List<int>();

                    var studioDatabaseContext = scope.ServiceProvider.GetRequiredService<StudioDBContext>();

                    if (teamId != null)
                    {
                        using (var teamRepository = new TeamRepository(studioDatabaseContext, _configuration))
                        {
                            var team = await teamRepository.GetByTeamId((int)teamId);
                            foreach (var user in team.TeamUsers)
                            {
                                if (!userIds.Contains(user.UserId))
                                {
                                    if (operation == "create")
                                    {
                                        userIds.Add(user.UserId);
                                    }
                                    else
                                    {
                                        /*
                                         * If user added without team and also with team.
                                         * We can not delete from gitea.
                                         */
                                        var userAppTeam = await collaboratorsRepository.CheckUserAddedMultipleTimes(user.UserId, organizationId, appId);

                                        if (!userAppTeam)
                                            userIds.Add(user.UserId);
                                    }
                                }
                            }
                        }
                    }
                    else if (userId != null)
                    {
                        if (operation == "create")
                        {
                            userIds.Add((int)userId);
                        }
                        else
                        {
                            /*
                             * If user added without team and also with team.
                             * We can not delete from gitea.
                             */
                            var userAppTeam = await collaboratorsRepository.CheckUserAddedMultipleTimes((int)userId, organizationId, appId, true);

                            if (!userAppTeam)
                                userIds.Add((int)userId);
                        }
                    }


                    var app = await appDraftRepository.Get(appId);
                    var repo = await GetRepositoryInfo(app.Name, organizationId);
                    var ownerName = repo["owner"]["username"].ToString();
                    var currentGiteaUsers = await GetRepositoryCollaborators(ownerName, app.Name);

                    using (var platformUserRepository = new PlatformUserRepository(platformDbContext, _configuration)) //, cacheHelper))
                    {
                        foreach (var id in userIds)
                        {
                            var user = await platformUserRepository.Get(id);
                            var userName = GetUserName(user.Email);

                            var giteaUser = currentGiteaUsers?.FirstOrDefault(x => x["email"].ToString() == user.Email);

                            if (giteaUser != null && operation != "delete") continue;
                            using (var httpClient = new HttpClient())
                            {
                                SetHeaders(client: httpClient, type: "token");
                                if (string.IsNullOrEmpty(giteaUrl)) continue;
                                switch (operation)
                                {
                                    case "create":
                                    {
                                        var requestUrl = $"{giteaUrl}/api/v1/repos/{repo["owner"]["username"]}/{app.Name}/collaborators/{userName}";

                                        await httpClient.PutAsync(requestUrl, new StringContent(JsonConvert.SerializeObject(new JObject()), Encoding.UTF8, "application/json"));
                                        break;
                                    }

                                    case "delete":
                                    {
                                        var requestUrl = $"{giteaUrl}/api/v1/repos/{repo["owner"]["username"]}/{app.Name}/collaborators/{userName}";

                                        await httpClient.DeleteAsync(requestUrl);
                                        break;
                                    }

                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public async Task<JArray> GetRepositoryCollaborators(string ownerName, string appName)
        {
            var giteaUrl = _configuration.GetValue("AppSettings:GiteaUrl", string.Empty);
            using (var httpClient = new HttpClient())
            {
                SetHeaders(client: httpClient, type: "token");
                if (string.IsNullOrEmpty(giteaUrl)) return null;
                var requestUrl = $"{giteaUrl}/api/v1/repos/{ownerName}/{appName}/collaborators";

                var responseRepo = await httpClient.GetAsync(requestUrl);
                var responseString = await responseRepo.Content.ReadAsStringAsync();
                return responseRepo.IsSuccessStatusCode ? JArray.Parse(responseString) : null;
            }
        }

        public JArray GetFileNames(string location, string path, bool getControllerName = false)
        {
            var names = new JArray();
            using (var repo = new Repository(location))
            {
                foreach (var e in repo.Index)
                {
                    if (e.Path.StartsWith(path))
                    {
                        /*if (getControllerName && e.Path.Contains(".js"))
                        {
                            var code = File.ReadAllText(location + "//" + e.Path);
                            if (code.Contains("angular.module('primeapps'"))
                            {
                                var match = Regex.Matches(code, @"'(.*?)'");
                                if (match.Success)
                                {
                                    var key = match.Value;
                                    names.Add(new JObject() {["path"] = e.Path, ["controller_name"] = key});
                                }
                            }
                        }*/

                        names.Add(new JObject() {["path"] = e.Path});
                    }
                }

                repo.Dispose();
            }

            return names;
        }

        public async Task<string> GetFile(string fileName, string organizationName, string appName, CustomCodeType type)
        {
            using (var httpClient = new HttpClient())
            {
                var giteaEmail = _configuration.GetValue("AppSettings:GiteaEmail", string.Empty);
                var giteaPassword = _configuration.GetValue("AppSettings:GiteaPassword", string.Empty);
                if (!string.IsNullOrEmpty(giteaEmail) && !string.IsNullOrEmpty(giteaPassword))
                {
                    SetHeaders(client: httpClient, type: "basic", email: giteaEmail, password: giteaPassword);
                }

                var giteaUrl = _configuration.GetValue("AppSettings:GiteaUrl", string.Empty);
                var url = "";
                if (!string.IsNullOrEmpty(giteaUrl))
                {
                    url = string.Format(giteaUrl + "/{0}/{1}/raw/branch/master/{2}/{3}", organizationName, appName, type.ToString().ToLower(), fileName);
                }

                var response = await httpClient.GetAsync(url);

                var resp = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    ErrorHandler.LogError(new Exception(resp), "Status Code: " + response.StatusCode + ",GetRepositoryInfo");
                    return null;
                }

                return resp;
            }
        }

        public void Push(Repository repo)
        {
            PushOptions options = GetOptions("push");
            repo.Network.Push(repo.Branches["master"], options);
        }

        public async Task<JObject> GetRepositoryInfo(string repositoryName, int organizationId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = scope.ServiceProvider.GetRequiredService<StudioDBContext>();
                using (var organizationRepository = new OrganizationRepository(databaseContext, _configuration))
                {
                    var organization = await organizationRepository.Get(organizationId);

                    if (organization != null)
                    {
                        using (var httpClient = new HttpClient())
                        {
                            SetHeaders(client: httpClient, type: "token");
                            var giteaUrl = _configuration.GetValue("AppSettings:GiteaUrl", string.Empty);
                            var url = "";
                            if (!string.IsNullOrEmpty(giteaUrl))
                            {
                                url = string.Format(giteaUrl + "/api/v1/repos/{0}/{1}", organization.Name, repositoryName);
                            }

                            var response = await httpClient.GetAsync(url);

                            var resp = await response.Content.ReadAsStringAsync();

                            if (!response.IsSuccessStatusCode)
                            {
                                ErrorHandler.LogError(new Exception(resp), "Status Code: " + response.StatusCode + ",GetRepositoryInfo");
                                return null;
                            }

                            return JObject.Parse(resp);
                        }
                    }

                    return null;
                }
            }
        }

        public string CloneRepository(string cloneUrl, string folderName, bool deleteIfExist = true)
        {
            var giteaDirectory = _configuration.GetValue("AppSettings:GiteaDirectory", string.Empty);
            if (string.IsNullOrEmpty(giteaDirectory))
                return null;

            var localFolder = giteaDirectory + folderName;

            if (Directory.Exists(localFolder))
            {
                if (deleteIfExist)
                    DeleteDirectory(localFolder);
                else
                    return null;
            }

            if (!Directory.Exists(giteaDirectory))
                Directory.CreateDirectory(giteaDirectory);

            var cloneOptions = GetOptions("clone");

            Repository.Clone(cloneUrl, localFolder, cloneOptions);
            return localFolder;
        }

        public async Task CreateOrganization(string uniqueName, string fullName, string email, string type = "token")
        {
            var enableGiteaIntegration = _configuration.GetValue("AppSettings:GiteaEnabled", string.Empty);
            if (!string.IsNullOrEmpty(enableGiteaIntegration))
            {
                if (!bool.Parse(enableGiteaIntegration))
                    return;
            }
            else
                return;
            //var userName = GetUserName(appUser.Email);

            using (var httpClient = new HttpClient())
            {
                var request = new JObject
                {
                    ["username"] = uniqueName,
                    ["full_name"] = fullName
                };

                SetHeaders(client: httpClient, type: type);

                var giteaUrl = _configuration.GetValue("AppSettings:GiteaUrl", string.Empty);
                var response = new HttpResponseMessage();

                if (!string.IsNullOrEmpty(giteaUrl))
                {
                    response = await httpClient.PostAsync(giteaUrl + "/api/v1/orgs", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
                }

                if (!response.IsSuccessStatusCode)
                {
                    var resp = await response.Content.ReadAsStringAsync();
                    ErrorHandler.LogError(new Exception(resp), "Status Code: " + response.StatusCode + ", user: " + email + ", new organization name: " + uniqueName);
                }
            }
        }

        public async Task CreateRepository(int organizationId, string appName, UserItem appUser)
        {
            var enableGiteaIntegration = _configuration.GetValue("AppSettings:GiteaEnabled", string.Empty);
            if (!string.IsNullOrEmpty(enableGiteaIntegration))
            {
                if (!bool.Parse(enableGiteaIntegration))
                    return;
            }
            else
                return;

            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<StudioDBContext>();
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

                        SetHeaders(client: httpClient, type: "token");

                        var url = "/api/v1/org/" + organization.Name + "/repos";

                        if (GetUserName(appUser.Email) == organization.Name)
                            url = "/api/v1/user/repos";

                        var giteaUrl = _configuration.GetValue("AppSettings:GiteaUrl", string.Empty);
                        var response = new HttpResponseMessage();

                        if (!string.IsNullOrEmpty(giteaUrl))
                        {
                            response = await httpClient.PostAsync(giteaUrl + url, new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
                        }

                        var resp = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            ErrorHandler.LogError(new Exception(resp), "Status Code: " + response.StatusCode + ", user: " + appUser.Email + ", new organization name: " + organization.Name + ", app name: " + appName);
                        }

                        var cloneUrl = JObject.Parse(resp)["clone_url"].ToString();
                        var templateUrl = "";

                        if (!string.IsNullOrEmpty(giteaUrl))
                        {
                            templateUrl = giteaUrl + "/primeapps/template.git";
                        }

                        //Clone auto generated repository in local folder.
                        var localFolder = CloneRepository(cloneUrl, appName);

                        using (var repo = new Repository(localFolder))
                        {
                            repo.Network.Remotes.Add("template", templateUrl);

                            var fetchOptions = GetOptions("fetch");

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
                            var pushOptions = GetOptions("push");
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

                SetHeaders(client: httpClient, type: "basic", email: email, password: password);

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

        public async Task<string> GetSHAToken(string email, string password)
        {
            var enableGiteaIntegration = _configuration.GetValue("AppSettings:GiteaEnabled", string.Empty);
            if (!string.IsNullOrEmpty(enableGiteaIntegration))
            {
                if (!bool.Parse(enableGiteaIntegration))
                    return null;
            }
            else
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


                    var giteaUrl = _configuration.GetValue("AppSettings:GiteaUrl", string.Empty);
                    var response = new HttpResponseMessage();

                    if (!string.IsNullOrEmpty(giteaUrl))
                    {
                        response = await httpClient.PostAsync(giteaUrl + "/api/v1/users/" + userName + "/tokens", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
                    }

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

        private HttpClient SetHeaders(HttpClient client, string type, string email = null, string password = null)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (type == "token")
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", Token);
            else
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", GetUserBasicAuthToken(email, password));

            return client;
        }

        protected dynamic GetOptions(string type)
        {
            var credential = new UsernamePasswordCredentials() {Username = Token, Password = String.Empty};
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
                case "pull":
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

                var response = await httpClient.PostAsync(_configuration.GetValue("AppSettings")["GiteaUrl"] + "/api/v1/users/" + userName + "/tokens", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
                var resp = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    ErrorHandler.LogError(new Exception(resp), "Status Code: " + response.StatusCode + ", GiteaHelper get access token method; username : " + userName);
                    return null;
                }

                return resp.ToString();
            }
        }*/

        public static string GetUserName(string email)
        {
            return string.Join("", (email.Replace("@", string.Empty)).Split("."));
        }

        public string GetToken()
        {
            if (_context.HttpContext == null)
                return null;

            if (!string.IsNullOrEmpty(_context.HttpContext.Request.Cookies["gitea_token"]))
            {
                var accessToken = _context.HttpContext.Request.Cookies["gitea_token"];
                AccessToken = accessToken;
                return accessToken;
            }

            var authorization = _context.HttpContext.Request.Headers["Authorization"];

            if (!string.IsNullOrEmpty(authorization))
            {
                var token = authorization[0].Remove(0, "Bearer ".Length);
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

                if (!string.IsNullOrEmpty(_configuration.GetValue("AppSettings:GiteaEnabled", string.Empty)) && bool.Parse(_configuration.GetValue("AppSettings:GiteaEnabled", string.Empty)))
                {
                    var accessToken = jwtToken?.Claims.FirstOrDefault(claim => claim.Type == "gitea_token")?.Value;
                    AccessToken = accessToken;
                    return accessToken;
                }
            }

            return null;
        }

        public string SetToken(string giteaToken = null)
        {
            if (!string.IsNullOrEmpty(giteaToken))
            {
                AccessToken = giteaToken;
                return giteaToken;
            }

            if (!string.IsNullOrEmpty(AccessToken))
                return AccessToken;

            return GetToken();
        }
    }
}