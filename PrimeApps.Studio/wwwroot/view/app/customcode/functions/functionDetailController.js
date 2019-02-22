'use strict';

angular.module('primeapps')

    .controller('FunctionDetailController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'FunctionsService', '$localStorage', '$sce', '$window', 'FunctionsDeploymentService',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, FunctionsService, $localStorage, $sce, $window, FunctionsDeploymentService) {

            $scope.loadingDeployments = true;
            $scope.loading = true;
            
            $scope.name = $state.params.name;
            $scope.orgId = $state.params.orgId;

            $scope.$parent.menuTopTitle = $scope.currentApp.label;
            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'functions';

            $scope.app = $rootScope.currentApp;
            $scope.tabManage = {
                activeTab: "overview"
            };

            $scope.deployments = [];

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }
            };

            $scope.generator(10);

            $scope.requestModel = {
                limit: "10",
                offset: 0
            };

            $scope.reload = function () {
                $scope.loadingDeployments = true;
                FunctionsDeploymentService.count($scope.function.id)
                    .then(function (response) {
                        $scope.pageTotal = response.data;

                        if ($scope.requestModel.offset != 0 && ($scope.requestModel.offset * $scope.requestModel.limit) >= $scope.pageTotal) {
                            $scope.requestModel.offset = $scope.requestModel.offset - 1;
                        }

                        FunctionsDeploymentService.find($scope.function.id, $scope.requestModel)
                            .then(function (response) {
                                $scope.deployments = response.data;
                                $scope.loadingDeployments = false;
                            });
                    });
            };

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;
                FunctionsDeploymentService.find(requestModel)
                    .then(function (response) {
                        $scope.deployments = response.data;
                        $scope.loading = false;
                    });

            };

            $scope.getTime = function (time) {
                return moment(time).format("DD-MM-YYYY HH:ss");
            };

            $scope.getIcon = function (status) {
                switch (status) {
                    case 'running':
                        return $sce.trustAsHtml('<i style="color:#0d6faa;" class="fas fa-clock"></i>');
                    case 'failed':
                        return $sce.trustAsHtml('<i style="color:rgba(218,10,0,1);" class="fas fa-times"></i>');
                    case 'succeed':
                        return $sce.trustAsHtml('<i style="color:rgba(16,124,16,1);" class="fas fa-check"></i>');
                }
            };

            $scope.changeOffset = function () {
                $scope.changePage(1)
            };

            $scope.runtimes = [
                {id: 1, name: "dotnetcore (2.0)", value: "dotnetcore2.0", type: "cs", editor: "csharp", editorDependencySample: "<Project Sdk=\"Microsoft.NET.Sdk\">\n\n  <PropertyGroup>\n    <TargetFramework>netstandard2.0</TargetFramework>\n  </PropertyGroup>\n\n  <ItemGroup>\n    <PackageReference Include=\"Kubeless.Functions\" Version=\"0.1.1\" />\n  </ItemGroup>\n\n</Project>", editorCodeSample: "using System;\r\nusing Kubeless.Functions;\r\nusing Newtonsoft.Json.Linq;\r\n\r\npublic class {{handler.class}}\r\n{\r\n    public object {{handler.method}}(Event k8Event, Context k8Context)\r\n    {\r\n        var obj = new JObject();\r\n        obj[\"data\"] = k8Event.Data.ToString();\r\n        \r\n        return obj;\r\n    }\r\n}"},
                {id: 2, name: "python (2.7)", value: "python2.7", type: "py", editor: "python", editorDependencySample: "from hellowithdepshelper import foo", editorCodeSample: "def {{handler.method}}(event, context):\n  print event['data']\n  return event['data']\n  "},
                {id: 3, name: "python (3.4)", value: "python3.4", type: "py", editor: "python", editorDependencySample: "from hellowithdepshelper import foo", editorCodeSample: "def {{handler.method}}(event, context):\n  print event['data']\n  return event['data']\n  "},
                {id: 4, name: "python (3.6)", value: "python3.6", type: "py", editor: "python", editorDependencySample: "from hellowithdepshelper import foo", editorCodeSample: "def {{handler.method}}(event, context):\n  print event['data']\n  return event['data']\n  "},
                {id: 5, name: "nodejs (6)", value: "nodejs6", type: "js", editor: "javascript", editorDependencySample: "{\n    \"name\": \"hellonodejs\",\n    \"version\": \"0.0.1\",\n    \"dependencies\": {\n        \"end-of-stream\": \"^1.4.1\",\n        \"from2\": \"^2.3.0\",\n        \"lodash\": \"^4.17.5\"\n    }\n}", editorCodeSample: "'use strict';\r\n\r\nconst _ = require('lodash');\r\n\r\nmodule.exports = {\r\n    {{handler.method}}: (event, context) => {\r\n        _.assign(event.data, {date: new Date().toTimeString()})\r\n        return JSON.stringify(event.data);\r\n    },\r\n};"},
                {id: 6, name: "nodejs (8)", value: "nodejs8", type: "js", editor: "javascript", editorDependencySample: "{\n    \"name\": \"hellonodejs\",\n    \"version\": \"0.0.1\",\n    \"dependencies\": {\n        \"end-of-stream\": \"^1.4.1\",\n        \"from2\": \"^2.3.0\",\n        \"lodash\": \"^4.17.5\"\n    }\n}", editorCodeSample: "'use strict';\r\n\r\nconst _ = require('lodash');\r\n\r\nmodule.exports = {\r\n    {{handler.method}}: (event, context) => {\r\n        _.assign(event.data, {date: new Date().toTimeString()})\r\n        return JSON.stringify(event.data);\r\n    },\r\n};"},
                {id: 7, name: "ruby (2.4)", value: "ruby2.4", type: "rb", editor: "ruby", editorDependencySample: "source 'https://rubygems.org'\n\ngem 'logging'", editorCodeSample: "require 'logging'\r\n\r\ndef {{handler.method}}(event, context)\r\n  logging = Logging.logger(STDOUT)\r\n  logging.info \"it works!\"\r\n  \"hello world\"\r\nend"},
                {id: 8, name: "php (7.2)", value: "php7.2", type: "php", editor: "php", editorDependencySample: "from hellowithdepshelper import foo", editorCodeSample: "\n<?php\n\nfunction {{handler.method}}($event, $context) {\n  return \"Hello World\";\n}\n"},
                {id: 9, name: "go (1.10)", value: "go1.10", type: "go", editor: "golang", editorDependencySample: "\n[[constraint]]\n  name = \"github.com/sirupsen/logrus\"\n  branch = \"master\"", editorCodeSample: "package kubeless\r\n\r\nimport (\r\n\t\"github.com/kubeless/kubeless/pkg/functions\"\r\n\t\"github.com/sirupsen/logrus\"\r\n)\r\n\r\n// Hello sample function with dependencies\r\nfunc {{handler.method}}(event functions.Event, context functions.Context) (string, error) {\r\n\tlogrus.Info(event.Data)\r\n\treturn \"Hello world!\", nil\r\n}"},
                {
                    id: 10,
                    name: "java (1.8)",
                    value: "java1.8",
                    type: "java",
                    editor: "java",
                    editorDependencySample: "<project xmlns=\"http://maven.apache.org/POM/4.0.0\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://maven.apache.org/POM/4.0.0 http://maven.apache.org/xsd/maven-4.0.0.xsd\">\n  <modelVersion>4.0.0</modelVersion>\n  <artifactId>function</artifactId>\n  <name>function</name>\n  <version>1.0-SNAPSHOT</version>\n  <dependencies>\n     <dependency>\n       <groupId>joda-time</groupId>\n       <artifactId>joda-time</artifactId>\n       <version>2.9.2</version>\n     </dependency>\n      <dependency>\n          <groupId>io.kubeless</groupId>\n          <artifactId>params</artifactId>\n          <version>1.0-SNAPSHOT</version>\n      </dependency>\n  </dependencies>\n  <parent>\n    <groupId>io.kubeless</groupId>\n    <artifactId>kubeless</artifactId>\n    <version>1.0-SNAPSHOT</version>\n  </parent>\n</project>",
                    editorCodeSample: "package io.kubeless;\r\n\r\nimport io.kubeless.Event;\r\nimport io.kubeless.Context;\r\n\r\npublic class {{handler.class}} {\r\n    public String {{handler.method}}(io.kubeless.Event event, io.kubeless.Context context) {\r\n        return \"Hello world!\";\r\n    }\r\n}"
                }
            ];

            /*if (!$scope.orgId || !$scope.appId) {
             $state.go('studio.apps', { organizationId: $scope.orgId });
             }*/

            $scope.functionForm = {};
            $scope.loading = true;
            //var currentOrganization = $localStorage.get("currentApp");
            $scope.organization = $filter('filter')($rootScope.organizations, {id: $scope.orgId})[0];
            $scope.giteaUrl = giteaUrl;

            /*$scope.aceOption = {
             mode: 'javascript',
             theme: 'tomorrow_night',
             onLoad: function (_ace) {
             // HACK to have the ace instance in the scope...
             $scope.modeChanged = function () {
             _ace.getSession().setMode("ace/mode/javascript");
             };
             }
             };*/

            if (!$scope.name) {
                $state.go('studio.app.functions');
            }

            FunctionsService.getByName($scope.name)
                .then(function (response) {
                    if (!response.data) {
                        toastr.error('Function Not Found !');
                        $state.go('studio.app.functions');
                    }
                    $scope.functionCopy = angular.copy(response.data);
                    $scope.function = response.data;
                    $scope.reload();
                    $scope.loading = false;
                });

            angular.element($window).bind('resize', function () {

                var layout = document.getElementsByClassName("setup-layout");
                var logger = document.getElementById("log-screen");
                logger.style.width = (layout[0].offsetWidth - 15) + "px";

                // manuall $digest required as resize event
                // is outside of angular
                $scope.$digest();

            });

            $scope.getFileType = function () {
                return $filter('filter')($scope.runtimes, {value: $scope.function.runtime})[0].type;
            };

            $scope.closeModal = function () {
                $scope.function = angular.copy($scope.functionCopy);
                $scope.createFormModal.hide();
            };

            $scope.runFunction = function (run) {
                $scope.running = true;
                $scope.response = null;
                FunctionsService.run($scope.name, run.type, run.body)
                    .then(function (response) {
                        $scope.response = response.data;
                        $scope.running = false;
                    })
                    .catch(function (response) {
                        if (response.status === 503) {
                            toastr.error('No endpoints available for service ' + $scope.function.name);
                        }
                        else {
                            toastr.error('An error occurred while running the function !');
                        }
                        $scope.running = false;
                    });
            };

            $scope.asHtml = function () {
                return $sce.trustAsHtml($scope.logs);
            };

            $scope.openTerminal = function () {
                var layout = document.getElementsByClassName("setup-layout");
                var logger = document.getElementById("log-screen");
                logger.style.width = (layout[0].offsetWidth - 15) + "px";

                $scope.showConsole = !$scope.showConsole;
                if (!$scope.logs) {
                    $scope.getLogs();
                }
            };

            $scope.getLogs = function () {
                $scope.refreshLogs = true;
                if (!$scope.logs) {
                    $scope.logs = "Loading...";
                }
                FunctionsService.getPods($scope.name)
                    .then(function (response) {
                        if (response.data.length > 0) {
                            var pods = response.data;
                            var activePod = $filter('filter')(pods, function (pod) {
                                return pod.status.phase === 'Running';
                            }, true)[0];
                            if (activePod) {
                                FunctionsService.getLogs(activePod.metadata.name)
                                    .then(function (response) {
                                        if (response.data) {
                                            $scope.logs = response.data;
                                            $scope.logsLoading = false;
                                            $timeout(function () {
                                                var logArea = document.getElementById('logArea');
                                                logArea.scrollTo(0, logArea.scrollHeight);
                                            }, 100);
                                            $scope.logs = response.data;
                                            $scope.refreshLogs = false;
                                        }
                                        else {
                                            $scope.logs = "No Logs Found...";
                                            $scope.logsLoading = false;
                                        }
                                    }).catch(function () {
                                    toastr.error('An error occurred while getting the logs!');
                                    $scope.refreshLogs = false;
                                })
                            }
                        }
                        else {
                            $scope.logs = "No Logs Found...";
                            $scope.logsLoading = false;
                            $scope.refreshLogs = false;
                        }
                    })

            };

            $scope.checkFunctionHandler = function (func) {
                if (func.handler) {
                    func.handler = func.handler.replace(/\s/g, '');
                    func.handler = func.handler.replace(/[^a-zA-Z\.]/g, '');
                    var dotIndex = func.handler.indexOf('.');
                    if (dotIndex > -1) {
                        if (dotIndex == 0) {
                            func.handler = func.handler.split('.').join('');
                        }
                        else {
                            func.handler = func.handler.split('.').join('');
                            func.handler = func.handler.slice(0, dotIndex) + "." + func.handler.slice(dotIndex);
                        }
                    }
                }
            };

            $scope.save = function (functionFormValidation) {
                if (!functionFormValidation.$valid)
                    return;

                $scope.saving = true;

                FunctionsService.update($scope.name, $scope.function)
                    .then(function (response) {
                        $scope.functionCopy = angular.copy($scope.function);
                        $scope.saving = false;
                        toastr.success("Function saved successfully.");
                    });
            };

            $scope.runDeployment = function () {
                toastr.success("Deployment Started");
                FunctionsService.deploy($scope.function.name)
                    .then(function (response) {
                        //setAceOption($scope.record.runtime);

                    })
                    .catch(function (response) {
                    });
            };
        }
    ]);