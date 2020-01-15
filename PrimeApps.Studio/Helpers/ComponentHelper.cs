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
using LibGit2Sharp.Handlers;
using Microsoft.AspNetCore.Hosting;
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
        Task<JArray> GetAllFileNames(int appId, string path, int organizationId);
        Task<bool> CreateSample(int appId, ComponentModel component, int organizationId);
        Task<bool> CreateSampleScript(int appId, ComponentModel script, int organizationId);
    }

    public class ComponentHelper : IComponentHelper
    {
        private IServiceScopeFactory _serviceScopeFactory;
        private IHttpContextAccessor _context;
        private IConfiguration _configuration;
        private IGiteaHelper _giteaHelper;
        private CurrentUser _currentUser;
        private IHostingEnvironment _hostingEnvironment;

        public ComponentHelper(IHttpContextAccessor context, IConfiguration configuration, IGiteaHelper giteaHelper, IServiceScopeFactory serviceScopeFactory, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
            _giteaHelper = giteaHelper;
            _currentUser = UserHelper.GetCurrentUser(_context);
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<JArray> GetAllFileNames(int appId, string componentName, int organizationId)
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
                        var repository = await _giteaHelper.GetRepositoryInfo(app.Name, organizationId);
                        if (repository != null)
                        {
                            var status = _giteaHelper.CloneRepository(repository["clone_url"].ToString(), repository["name"].ToString(), false);

                            var giteaDirectory = _configuration.GetValue("AppSettings:DataDirectory", string.Empty);
                            var localFolder = giteaDirectory + repository["name"];

                            var nameList = _giteaHelper.GetFileNames(localFolder, "components/" + componentName);

                            if (!string.IsNullOrEmpty(status))
                                _giteaHelper.DeleteDirectory(localFolder);

                            return nameList;
                        }
                    }
                }
            }

            return null;
        }

        public async Task<bool> CreateSample(int appId, ComponentModel component, int organizationId)
        {
            var enableGiteaIntegration = DataHelper.GetDataDirectoryPath(_configuration, _hostingEnvironment);
            var giteaEnabled = _configuration.GetValue("AppSettings:GiteaEnabled", string.Empty);
            if (!string.IsNullOrEmpty(enableGiteaIntegration) && bool.Parse(giteaEnabled))
            {
                try
                {
                    using (var _scope = _serviceScopeFactory.CreateScope())
                    {
                        var databaseContext = _scope.ServiceProvider.GetRequiredService<StudioDBContext>();
                        using (var _appDraftRepository = new AppDraftRepository(databaseContext, _configuration))
                        {
                            var app = await _appDraftRepository.Get(appId);
                            var repository = await _giteaHelper.GetRepositoryInfo(app.Name, organizationId);
                            if (repository != null)
                            {
                                var localPath = _giteaHelper.CloneRepository(repository["clone_url"].ToString(), repository["name"].ToString());
                                var path = Path.Combine(localPath, "components", component.Name);

                                if (!Directory.Exists(path))
                                {
                                    Directory.CreateDirectory(path);

                                    var files = new JArray()
                                    {
                                        new JObject
                                        {
                                            ["filePath"] = Path.Combine(path, "sample.html"),
                                            ["type"] = "html"
                                        },
                                        new JObject
                                        {
                                            ["filePath"] = Path.Combine(path, "sampleController.js"),
                                            ["type"] = "controller"
                                        },
                                        new JObject
                                        {
                                            ["filePath"] = Path.Combine(path, "sampleService.js"),
                                            ["type"] = "service"
                                        }
                                    };

                                    using (var repo = new Repository(localPath))
                                    {
                                        foreach (var file in files)
                                        {
                                            var sample = GetSampleComponent(file["type"].ToString());

                                            using (var fs = System.IO.File.Create((string)file["filePath"]))
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
                                            return false;
                                        }

                                        //System.IO.File.WriteAllText(localPath, sample);
                                        Commands.Stage(repo, "*");

                                        var signature = new Signature(
                                            new Identity("system", "system@primeapps.io"), DateTimeOffset.Now);

                                        // Commit to the repository
                                        var commit = repo.Commit("Created component " + component.Name, signature, signature);
                                        _giteaHelper.Push(repo);

                                        repo.Dispose();
                                    }
                                }

                                _giteaHelper.DeleteDirectory(localPath);
                                return true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogError(ex, "Sample component not created." + "Component Name: " + component.Name + ", Organization Id: " + organizationId + "App Id: " + appId);
                    return false;
                }
            }

            return false;
        }

        public async Task<bool> CreateSampleScript(int appId, ComponentModel script, int organizationId)
        {
            var enableGiteaIntegration = _configuration.GetValue("AppSettings:GiteaEnabled", string.Empty);

            if (string.IsNullOrEmpty(enableGiteaIntegration) || !bool.Parse(enableGiteaIntegration))
                return false;

            try
            {
                using (var _scope = _serviceScopeFactory.CreateScope())
                {
                    var databaseContext = _scope.ServiceProvider.GetRequiredService<StudioDBContext>();
                    using (var _appDraftRepository = new AppDraftRepository(databaseContext, _configuration))
                    {
                        var app = await _appDraftRepository.Get(appId);
                        var repository = await _giteaHelper.GetRepositoryInfo(app.Name, organizationId);
                        if (repository != null)
                        {
                            var localPath = _giteaHelper.CloneRepository(repository["clone_url"].ToString(), repository["name"].ToString());
                            var scriptsPath = Path.Combine(localPath, "scripts");
                            Directory.CreateDirectory(scriptsPath);
                            var fileName = Path.Combine(scriptsPath, $"{script.Name}.js");

                            using (var repo = new Repository(localPath))
                            {
                                var sample = "console.log('Hello World!');";

                                using (var fs = System.IO.File.Create(fileName))
                                {
                                    var info = new UTF8Encoding(true).GetBytes(sample);
                                    // Add some information to the file.
                                    fs.Write(info, 0, info.Length);
                                }

                                var status = repo.RetrieveStatus();

                                if (!status.IsDirty)
                                {
                                    _giteaHelper.DeleteDirectory(localPath);
                                    return false;
                                }

                                //System.IO.File.WriteAllText(localPath, sample);
                                Commands.Stage(repo, "*");

                                var signature = new Signature(
                                    new Identity("system", "system@primeapps.io"), DateTimeOffset.Now);

                                // Commit to the repository
                                var commit = repo.Commit("Created script " + script.Name, signature, signature);
                                _giteaHelper.Push(repo);

                                repo.Dispose();
                                _giteaHelper.DeleteDirectory(localPath);
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, "Sample script not created." + "Script Name: " + script.Name + ", Organization Id: " + organizationId + "App Id: " + appId);
                return false;
            }

            return false;
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
                           "app.controller('SampleController', ['$rootScope', '$controller', '$scope', 'ngToast', '$location', '$state', 'ModuleService', '$filter', '$window', '$localStorage', 'config', '$timeout', '$modal', '$http', '$cookies', '$interval','SampleService'," + Environment.NewLine +
                           "\tfunction ($rootScope, $controller, $scope, ngToast, $location, $state, ModuleService, $filter, $window, $localStorage, config, $timeout, $modal, $http, $cookies, $interval, SampleService) {" + Environment.NewLine +
                           "\t\t$scope.title = 'Hello from Sample Component';" + Environment.NewLine +
                           "\t\tSampleService.get(2)" + Environment.NewLine +
                           "\t\t\t.then(function(response){" + Environment.NewLine +
                           "\t\t\t\t$scope.body = response.data;" + Environment.NewLine +
                           "\t\t\t})" + Environment.NewLine +
                           "\t\t\t.catch(function(response){" + Environment.NewLine + Environment.NewLine +
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