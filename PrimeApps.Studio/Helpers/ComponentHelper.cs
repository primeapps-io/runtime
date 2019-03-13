using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common.Component;
using PrimeApps.Model.Context;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Models;

namespace PrimeApps.Studio.Helpers
{
    public interface IComponentHelper
    {
        Task<JArray> GetAllFileNames(string giteaToken, string email, int appId, string path);
        void CreateSample(string giteaToken, string email, int appId, ComponentModel component);
        void CreateSampleScript(string giteaToken, string email, int appId, ComponentModel script);
    }

    public class ComponentHelper : IComponentHelper
    {
        private IServiceScopeFactory _serviceScopeFactory;
        private IHttpContextAccessor _context;
        private IConfiguration _configuration;
        private IGiteaHelper _giteaHelper;
        private CurrentUser _currentUser;

        public ComponentHelper(IHttpContextAccessor context, IConfiguration configuration, IGiteaHelper giteaHelper, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
            _giteaHelper = giteaHelper;
            _currentUser = UserHelper.GetCurrentUser(_context);
        }

        public async Task<JArray> GetAllFileNames(string giteaToken, string email, int appId, string componentName)
        {
            var enableGiteaIntegration = _configuration.GetValue("AppSettings:GiteaEnabled", string.Empty);

            if (!string.IsNullOrEmpty(enableGiteaIntegration) && bool.Parse(enableGiteaIntegration))
            {
                using (var _scope = _serviceScopeFactory.CreateScope())
                {
                    var databaseContext = _scope.ServiceProvider.GetRequiredService<StudioDBContext>();
                    using (var _appDraftRepository = new AppDraftRepository(databaseContext, _configuration))
                    {
                        var app = await _appDraftRepository.Get(appId);
                        var repository = await _giteaHelper.GetRepositoryInfo(giteaToken, email, app.Name);
                        if (repository != null)
                        {
                            var giteaDirectory = _configuration.GetValue("AppSettings:GiteaDirectory", string.Empty);

                            if (!string.IsNullOrEmpty(giteaDirectory))
                            {
                                var localPath = giteaDirectory + repository["name"].ToString();

                                var result = _giteaHelper.CloneRepository(giteaToken, repository["clone_url"].ToString(), localPath, false);

                                /*using (var repo = new Repository(localPath))
                                {*/
                                var nameList = _giteaHelper.GetFileNames(localPath, "components/" + componentName);

                                if (result)
                                    _giteaHelper.DeleteDirectory(localPath);

                                return nameList;
                                /*}*/
                            }
                        }
                    }
                }
            }

            return null;
        }

        public async void CreateSample(string giteaToken, string email, int appId, ComponentModel component)
        {
            var enableGiteaIntegration = _configuration.GetValue("AppSettings:GiteaEnabled", string.Empty);

            if (!string.IsNullOrEmpty(enableGiteaIntegration) && bool.Parse(enableGiteaIntegration))
            {
                using (var _scope = _serviceScopeFactory.CreateScope())
                {
                    var databaseContext = _scope.ServiceProvider.GetRequiredService<StudioDBContext>();
                    using (var _appDraftRepository = new AppDraftRepository(databaseContext, _configuration))
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
                                if (!Directory.Exists(localPath + $"/components/{component.Name}"))
                                {
                                    Directory.CreateDirectory(localPath + $"/components/{component.Name}");

                                    var files = new JArray()
                                    {
                                        new JObject
                                        {
                                            ["filePath"] = $"components/{component.Name}/sample.html",
                                            ["type"] = "html"
                                        },
                                        new JObject
                                        {
                                            ["filePath"] = $"components/{component.Name}/sampleController.js",
                                            ["type"] = "controller"
                                        },
                                        new JObject
                                        {
                                            ["filePath"] = $"components/{component.Name}/sampleService.js",
                                            ["type"] = "service"
                                        }
                                    };

                                    using (var repo = new Repository(localPath))
                                    {
                                        foreach (var file in files)
                                        {
                                            var sample = GetSampleComponent(file["type"].ToString());

                                            using (var fs = System.IO.File.Create(localPath + "/" + file["filePath"].ToString()))
                                            {
                                                var info = new UTF8Encoding(true).GetBytes(sample);
                                                // Add some information to the file.
                                                fs.Write(info, 0, info.Length);
                                            }
                                        }

                                        var status = repo.RetrieveStatus();

                                        if (!status.IsDirty)
                                        {
                                            _giteaHelper.DeleteDirectory(localPath);
                                            return;
                                        }

                                        //System.IO.File.WriteAllText(localPath, sample);
                                        Commands.Stage(repo, "*");

                                        var signature = new Signature(
                                            new Identity("system", "system@primeapps.io"), DateTimeOffset.Now);

                                        // Commit to the repository
                                        var commit = repo.Commit("Created component " + component.Name, signature, signature);
                                        _giteaHelper.Push(repo, giteaToken);

                                        repo.Dispose();
                                    }
                                }
                                _giteaHelper.DeleteDirectory(localPath);
                            }
                        }
                    }
                }
            }
        }

        public async void CreateSampleScript(string giteaToken, string email, int appId, ComponentModel script)
        {
            var enableGiteaIntegration = _configuration.GetValue("AppSettings:GiteaEnabled", string.Empty);

            if (!string.IsNullOrEmpty(enableGiteaIntegration) && bool.Parse(enableGiteaIntegration))
            {
                using (var _scope = _serviceScopeFactory.CreateScope())
                {
                    var databaseContext = _scope.ServiceProvider.GetRequiredService<StudioDBContext>();
                    using (var _appDraftRepository = new AppDraftRepository(databaseContext, _configuration))
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
                                var fileName = $"/scripts/{script.Name}.js";

                                using (var repo = new Repository(localPath))
                                {
                                    var sample = "Console.log('Hello World!');";

                                    using (var fs = System.IO.File.Create(localPath + fileName))
                                    {
                                        var info = new UTF8Encoding(true).GetBytes(sample);
                                        // Add some information to the file.
                                        fs.Write(info, 0, info.Length);
                                    }

                                    var status = repo.RetrieveStatus();

                                    if (!status.IsDirty)
                                    {
                                        _giteaHelper.DeleteDirectory(localPath);
                                        return;
                                    }

                                    //System.IO.File.WriteAllText(localPath, sample);
                                    Commands.Stage(repo, "*");

                                    var signature = new Signature(
                                        new Identity("system", "system@primeapps.io"), DateTimeOffset.Now);

                                    // Commit to the repository
                                    var commit = repo.Commit("Created script " + script.Name, signature, signature);
                                    _giteaHelper.Push(repo, giteaToken);

                                    repo.Dispose();
                                    _giteaHelper.DeleteDirectory(localPath);
                                }
                            }
                        }
                    }
                }
            }
        }


        public string GetSampleComponent(string type)
        {
            switch (type)
            {
                case "html":
                    return "<div class=\"page\">" + Environment.NewLine +
                           "\t<div class=\"panel panel-default\">" + Environment.NewLine +
                           "\t\t<div class=\"panel-heading clearfix\">" + Environment.NewLine +
                           "\t\t\t<h4 class=\"pull-left\">{{ title }}</h4>" + Environment.NewLine +
                           "\t\t</div>" + Environment.NewLine +
                           "\t\t<div class=\"panel-body\" id=\"panelBody\">" + Environment.NewLine +
                           "\t\t\t<div class=\"box-page-loading\">" + Environment.NewLine +
                           "\t\t\t\t<i style=\"color: #eaa63c;\" class=\"fa fa-spinner fa-spin orange bigger-140\"></i> {{ body }}" + Environment.NewLine +
                           "\t\t\t</div>" + Environment.NewLine +
                           "\t\t</div>" + Environment.NewLine +
                           "\t</div>" + Environment.NewLine +
                           "</div>" + Environment.NewLine;
                case "controller":
                    return @"var app = angular.module('primeapps', [])" + Environment.NewLine + Environment.NewLine +
                           "app.controller('SampleController', ['$rootScope', '$controller', '$scope', 'ngToast', '$location', '$state', 'ModuleService', '$filter', '$window', '$localStorage', 'config', '$timeout', '$modal', '$http', '$cookies', '$interval','SampleService'" + Environment.NewLine +
                           "\tfunction ($rootScope, $controller, $scope, ngToast, $location, $state, ModuleService, $filter, $window, $localStorage, config, $timeout, $modal, $http, $cookies, $interval, SampleService) {" + Environment.NewLine +
                           "\t\t$scope.title = 'Hello from Sample Component';" + Environment.NewLine +
                           "\t\tSampleService.get(2)" + Environment.NewLine +
                           "\t\t\t.then(function(response){" + Environment.NewLine +
                           "\t\t\t\t$scope.body = response.data;" + Environment.NewLine +
                           "\t\t\t})" + Environment.NewLine +
                           "\t\t\t.catch(function(response(){" + Environment.NewLine + Environment.NewLine +
                           "\t\t\t});" + Environment.NewLine +
                           "\t}" + Environment.NewLine +
                           "]);" + Environment.NewLine;
                case "service":
                    return @"'use strict';" + Environment.NewLine + Environment.NewLine +
                           "angular.module('primeapps')" + Environment.NewLine +
                           "\t.factory('SampleService', ['$rootScope', '$http', '$localStorage', '$cache', '$q', '$filter', '$timeout', '$state', 'config', '$window', '$modal', '$sce'," + Environment.NewLine +
                           "\t\tfunction ($rootScope, $http, $localStorage, $cache, $q, $filter, $timeout, $state, config, $window, $modal, $sce) {" + Environment.NewLine +
                           "\t\t\treturn {" + Environment.NewLine +
                           "\t\t\t\tget: function (id) {" + Environment.NewLine +
                           "\t\t\t\t\treturn $http.get('http://localhost:8080/service/get/' + id);" + Environment.NewLine +
                           "\t\t\t\t}" + Environment.NewLine +
                           "\t\t\t};" + Environment.NewLine +
                           "\t\t}]);" + Environment.NewLine;
                default:
                    return null;
            }
        }
    }
}