using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Models;

namespace PrimeApps.Studio.Helpers
{
    public interface IFunctionHelper
    {
        void CreateSample(string giteaToken, string email, int appId, FunctionBindingModel function);
        string GetTypeWithRuntime(FunctionRuntime runtime);
        JObject CreateFunctionRequest(FunctionBindingModel model, JObject functionCurrent = null);
        Task<JObject> Get(string functionName);
        Task<string> GetFunctionUrl(string functionName);
        Task<HttpResponseMessage> Run(string functionUrl, string functionHttpMethod, string functionRequestBody);
        Task<JArray> GetPods(string functionName);
        Task<string> GetLogs(string podName);
        string GetSampleFunction(FunctionRuntime runtime, string moduleHandler);
    }

    public class FunctionHelper : IFunctionHelper
    {
        private IConfiguration _configuration;
        private IGiteaHelper _giteaHelper;
        private IAppDraftRepository _appDraftRepository;
        private string _kubernetesClusterRootUrl;

        public FunctionHelper(IConfiguration configuration, IGiteaHelper giteaHelper, IAppDraftRepository appDraftRepository)
        {
            _configuration = configuration;
            _giteaHelper = giteaHelper;
            _appDraftRepository = appDraftRepository;
            _kubernetesClusterRootUrl = _configuration["AppSettings:KubernetesClusterRootUrl"];
        }

        public async void CreateSample(string giteaToken, string email, int appId, FunctionBindingModel function)
        {
            var enableGiteaIntegration = _configuration.GetValue("AppSettings:GiteaEnabled", string.Empty);

            if (!string.IsNullOrEmpty(enableGiteaIntegration) && bool.Parse(enableGiteaIntegration))
            {
                var app = await _appDraftRepository.Get(appId);
                var repository = await _giteaHelper.GetRepositoryInfo(giteaToken, email, app.Name);

                if (repository != null)
                {
                    var giteaDirectory = _configuration.GetValue("AppSettings:GiteaDirectory", string.Empty);

                    if (!string.IsNullOrEmpty(giteaDirectory))
                    {
                        var localPath = giteaDirectory + repository["name"].ToString();

                        _giteaHelper.CloneRepository(giteaToken, repository["clone_url"].ToString(), localPath);

                        var fileName = $"functions/{function.Name}.cs";

                        if (!System.IO.File.Exists(fileName))
                        {
                            using (var repo = new Repository(localPath))
                            {
                                var sample = GetSampleFunction(function.Runtime, function.Handler);
                                using (var fs = System.IO.File.Create(localPath + "/" + fileName))
                                {
                                    var info = new UTF8Encoding(true).GetBytes(sample);
                                    // Add some information to the file.
                                    fs.Write(info, 0, info.Length);
                                }

                                //System.IO.File.WriteAllText(localPath, sample);
                                Commands.Stage(repo, "*");

                                var signature = new Signature(
                                    new Identity("system", "system@primeapps.io"), DateTimeOffset.Now);

                                var status = repo.RetrieveStatus();

                                if (!status.IsDirty)
                                    return;

                                // Commit to the repository
                                var commit = repo.Commit("Created function " + function.Name, signature, signature);
                                _giteaHelper.Push(repo, giteaToken);

                                repo.Dispose();
                                _giteaHelper.DeleteDirectory(localPath);
                            }
                        }
                    }
                }
            }
        }

        public JObject CreateFunctionRequest(FunctionBindingModel model, JObject functionCurrent = null)
        {
            var function = new JObject
            {
                ["kind"] = "Function",
                ["apiVersion"] = "kubeless.io/v1beta1",
                ["metadata"] = new JObject()
            };
            function["metadata"]["name"] = model.Name;
            function["metadata"]["namespace"] = "default";
            function["spec"] = new JObject
            {
                ["function"] = "",
                ["deps"] = !string.IsNullOrWhiteSpace(model.Dependencies) ? model.Dependencies : "",
                ["handler"] = model.Handler,
                ["runtime"] = model.Runtime.GetAttributeOfType<EnumMemberAttribute>().Value,
                ["function-content-type"] = model.ContentType.GetAttributeOfType<EnumMemberAttribute>().Value
            };
            function["timeout"] = "180";

            if (functionCurrent != null && !functionCurrent.IsNullOrEmpty())
            {
                function["metadata"]["resourceVersion"] = (string)functionCurrent["metadata"]["resourceVersion"];
                function["metadata"]["uid"] = (string)functionCurrent["metadata"]["uid"];
                function["metadata"]["generation"] = (int)functionCurrent["metadata"]["generation"];
                function["spec"]["function"] = model.Function;

                if (model.ContentType == FunctionContentType.Text)
                    function["spec"]["checksum"] = "sha256:" + model.Function.ToSha256();
            }

            return function;
        }

        public async Task<JObject> Get(string functionName)
        {
            JObject function;

            using (var httpClient = new HttpClient())
            {
                var url = $"{_kubernetesClusterRootUrl}/apis/kubeless.io/v1beta1/namespaces/default/functions/{functionName}";

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(content))
                    throw new Exception("Kubernetes error. StatusCode: " + response.StatusCode + " Content: " + content);

                function = JObject.Parse(content);
            }

            return function;
        }

        public async Task<string> GetFunctionUrl(string functionName)
        {
            string functionUrl;

            using (var httpClient = new HttpClient())
            {
                var url = $"{_kubernetesClusterRootUrl}/api/v1/namespaces/default/services/" + functionName;

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(content))
                    throw new Exception("Kubernetes error. StatusCode: " + response.StatusCode + " Content: " + content);

                var function = JObject.Parse(content);
                var port = 8080;

                if (!function["spec"].IsNullOrEmpty() && !function["spec"]["ports"].IsNullOrEmpty())
                    port = (int)((JArray)function["spec"]["ports"])[0]["port"];

                functionUrl = $"{_kubernetesClusterRootUrl}/api/v1/namespaces/default/services/{functionName}:{port}/proxy/";
            }

            return functionUrl;
        }

        public async Task<HttpResponseMessage> Run(string functionUrl, string functionHttpMethod, string functionRequestBody)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response;
                var httpMethod = new HttpMethod(functionHttpMethod);

                if (httpMethod == HttpMethod.Get)
                {
                    response = await httpClient.GetAsync(functionUrl);
                }
                else if (httpMethod == HttpMethod.Post)
                {
                    var requestContent = new StringContent(functionRequestBody, Encoding.UTF8, "application/json");
                    response = await httpClient.PostAsync(functionUrl, requestContent);
                }
                else
                {
                    throw new Exception("Unsupported HttpMethod.");
                }

                return response;
            }
        }

        public async Task<JArray> GetPods(string functionName)
        {
            JArray pods;

            using (var httpClient = new HttpClient())
            {
                var url = $"{_kubernetesClusterRootUrl}/api/v1/pods?labelSelector=function%3D{functionName}";

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(content))
                    throw new Exception("Kubernetes error. StatusCode: " + response.StatusCode + " Content: " + content);

                var result = JObject.Parse(content);
                pods = (JArray)result["items"];
            }

            return pods;
        }

        public async Task<string> GetLogs(string podName)
        {
            string logs;

            using (var httpClient = new HttpClient())
            {
                var url = $"{_kubernetesClusterRootUrl}/api/v1/namespaces/default/pods/{podName}/log";

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                /*if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(content))
                    return content;*/

                logs = content;
            }

            return logs;
        }

        public string GetSampleFunction(FunctionRuntime runtime, string moduleHandler)
        {
            var handler = moduleHandler.Split(".");

            switch (runtime)
            {
                case FunctionRuntime.Dotnetcore20:
                    return string.Format(@"using System;" +
                                         "using Kubeless.Functions;" + Environment.NewLine +
                                         "using Newtonsoft.Json.Linq;" + Environment.NewLine +
                                         "public class {0}{{" + Environment.NewLine +
                                         "\tpublic object {1}(Event k8Event, Context k8Context)" + Environment.NewLine +
                                         "\t{{" + Environment.NewLine +
                                         "\t\treturn \"Hello World\";" + Environment.NewLine +
                                         "\t}}" + Environment.NewLine +
                                         "}}", handler[0], handler[1]);
                case FunctionRuntime.Python27:
                case FunctionRuntime.Python34:
                case FunctionRuntime.Python36:
                    return string.Format(@"def {0}(event, context):" + Environment.NewLine +
                                         "print event['data']" + Environment.NewLine +
                                         "return event['data']" + Environment.NewLine, handler[1]);

                case FunctionRuntime.Nodejs6:
                case FunctionRuntime.Nodejs8:
                    return string.Format(@"'use strict';" + Environment.NewLine +
                                         "const _ = require('lodash');" + Environment.NewLine +
                                         "module.exports = {{" + Environment.NewLine +
                                         "\t{0}: (event, context) => {{" + Environment.NewLine +
                                         "\t\t_.assign(event.data, {{date: new Date().toTimeString()}})" + Environment.NewLine +
                                         "\t\treturn JSON.stringify(event.data);" + Environment.NewLine +
                                         "\t}}," + Environment.NewLine +
                                         "}};", handler[1]);

                case FunctionRuntime.Go110:
                    return string.Format(@"package kubeless" + Environment.NewLine + Environment.NewLine +
                                         "import (\"github.com/kubeless/kubeless/pkg/functions\")" + Environment.NewLine + Environment.NewLine +
                                         "//Hello sample function with dependencies" + Environment.NewLine +
                                         "func {0}(event functions.Event, context functions.Context) (string, error) {{" + Environment.NewLine +
                                         "\treturn \"Hello world!\", nil" + Environment.NewLine +
                                         "}}", handler[1]);
                case FunctionRuntime.Java18:
                    return string.Format(@"package io.kubeless;" + Environment.NewLine + Environment.NewLine +
                                         "import io.kubeless.Event;" + Environment.NewLine +
                                         "import io.kubeless.Context;" + Environment.NewLine + Environment.NewLine +
                                         "public class {0} {{" + Environment.NewLine +
                                         "\tpublic String {1}(io.kubeless.Event event, io.kubeless.Context context) {{" + Environment.NewLine +
                                         "\t\treturn \"Hello world!\";" + Environment.NewLine +
                                         "\t}}" + Environment.NewLine +
                                         "}}", handler[0], handler[1]);

                case FunctionRuntime.Php72:
                    return string.Format(@"<?php" + Environment.NewLine + Environment.NewLine +
                                         "function {0}($event, $context) {{" + Environment.NewLine +
                                         "\treturn \"Hello World\";" + Environment.NewLine +
                                         "}}", handler[1]);
                case FunctionRuntime.Ruby24:
                    return string.Format(@"require 'logging'" + Environment.NewLine + Environment.NewLine +
                                         "def {0}(event, context)" + Environment.NewLine +
                                         "logging = Logging.logger(STDOUT)" + Environment.NewLine +
                                         "logging.info \"it works!\"" + Environment.NewLine +
                                         "\"hello world\"" + Environment.NewLine +
                                         "end", handler[1]);
                case FunctionRuntime.NotSet:
                    return "";
                default:
                    return "";
            }
        }

        public string GetTypeWithRuntime(FunctionRuntime runtime)
        {
            switch (runtime)
            {
                case FunctionRuntime.Dotnetcore20:
                    return "cs";
                case FunctionRuntime.Go110:
                    return "go";
                case FunctionRuntime.Java18:
                    return "java";
                case FunctionRuntime.Nodejs8:
                case FunctionRuntime.Nodejs6:
                    return "js";

                case FunctionRuntime.Php72:
                    return "php";
                case FunctionRuntime.Ruby24:
                    return "rb";
                case FunctionRuntime.Python27:
                case FunctionRuntime.Python34:
                case FunctionRuntime.Python36:
                    return "py";
                default:
                    return "txt";
            }
        }
    }
}