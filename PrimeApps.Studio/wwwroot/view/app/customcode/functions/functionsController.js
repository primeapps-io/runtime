'use strict';

angular.module('primeapps')

    .controller('FunctionsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'FunctionsService', '$localStorage', '$q',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, FunctionsService, $localStorage, $q) {
            $scope.appId = $state.params.appId;
            $scope.orgId = $state.params.orgId;

            $scope.$parent.menuTopTitle = $scope.currentApp.label;
            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'functions';
            $scope.environments = FunctionsService.getEnvironments();
            $scope.functionNameValid = null;
            $scope.isFunctionNameBlur = false;
            $scope.page = 1;
            $scope.runtimes = [
                {
                    id: 1,
                    name: "dotnetcore (2.0)",
                    value: "dotnetcore2.0",
                    editor: "csharp",
                    editorDependencySample: "<Project Sdk=\"Microsoft.NET.Sdk\">\n\n  <PropertyGroup>\n    <TargetFramework>netstandard2.0</TargetFramework>\n  </PropertyGroup>\n\n  <ItemGroup>\n    <PackageReference Include=\"Kubeless.Functions\" Version=\"0.1.1\" />\n  </ItemGroup>\n\n</Project>",
                    editorCodeSample: "using System;\r\nusing Kubeless.Functions;\r\nusing Newtonsoft.Json.Linq;\r\n\r\npublic class {{handler.class}}\r\n{\r\n    public object {{handler.method}}(Event k8Event, Context k8Context)\r\n    {\r\n        var obj = new JObject();\r\n        obj[\"data\"] = k8Event.Data.ToString();\r\n        \r\n        return obj;\r\n    }\r\n}"
                },
                {
                    id: 2,
                    name: "python (2.7)",
                    value: "python2.7",
                    editor: "python",
                    editorDependencySample: "bs4",
                    editorCodeSample: "def {{handler.method}}(event, context):\r\n return \"hello world\"\n"
                },
                {
                    id: 3,
                    name: "python (3.4)",
                    value: "python3.4",
                    editor: "python",
                    editorDependencySample: "bs4",
                    editorCodeSample: "def {{handler.method}}(event, context):\r\n return \"hello world\"\n"
                },
                {
                    id: 4,
                    name: "python (3.6)",
                    value: "python3.6",
                    editor: "python",
                    editorDependencySample: "bs4",
                    editorCodeSample: "def {{handler.method}}(event, context):\r\n return \"hello world\"\n"
                },
                {
                    id: 5,
                    name: "nodejs (6)",
                    value: "nodejs6",
                    editor: "javascript",
                    editorDependencySample: "{\n    \"name\": \"hellonodejs\",\n    \"version\": \"0.0.1\",\n    \"dependencies\": {\n        \"end-of-stream\": \"^1.4.1\",\n        \"from2\": \"^2.3.0\",\n        \"lodash\": \"^4.17.5\"\n    }\n}",
                    editorCodeSample: "'use strict';\r\n\r\nconst _ = require('lodash');\r\n\r\nmodule.exports = {\r\n    {{handler.method}}: (event, context) => {\r\n        _.assign(event.data, {date: new Date().toTimeString()})\r\n        return JSON.stringify(event.data);\r\n    },\r\n};"
                },
                {
                    id: 6,
                    name: "nodejs (8)",
                    value: "nodejs8",
                    editor: "javascript",
                    editorDependencySample: "{\n    \"name\": \"hellonodejs\",\n    \"version\": \"0.0.1\",\n    \"dependencies\": {\n        \"end-of-stream\": \"^1.4.1\",\n        \"from2\": \"^2.3.0\",\n        \"lodash\": \"^4.17.5\"\n    }\n}",
                    editorCodeSample: "'use strict';\r\n\r\nconst _ = require('lodash');\r\n\r\nmodule.exports = {\r\n    {{handler.method}}: (event, context) => {\r\n        _.assign(event.data, {date: new Date().toTimeString()})\r\n        return JSON.stringify(event.data);\r\n    },\r\n};"
                },
                {
                    id: 7,
                    name: "ruby (2.4)",
                    value: "ruby2.4",
                    editor: "ruby",
                    editorDependencySample: "source 'https://rubygems.org'\n\ngem 'logging'",
                    editorCodeSample: "require 'logging'\r\n\r\ndef {{handler.method}}(event, context)\r\n  logging = Logging.logger(STDOUT)\r\n  logging.info \"it works!\"\r\n  \"hello world\"\r\nend"
                },
                {
                    id: 8,
                    name: "php (7.2)",
                    value: "php7.2",
                    editor: "php",
                    editorDependencySample: "from hellowithdepshelper import foo",
                    editorCodeSample: "\n<?php\n\nfunction {{handler.method}}($event, $context) {\n  return \"Hello World\";\n}\n"
                },
                {
                    id: 9,
                    name: "go (1.10)",
                    value: "go1.10",
                    editor: "golang",
                    editorDependencySample: "\n[[constraint]]\n  name = \"github.com/sirupsen/logrus\"\n  branch = \"master\"",
                    editorCodeSample: "package kubeless\r\n\r\nimport (\r\n\t\"github.com/kubeless/kubeless/pkg/functions\"\r\n\t\"github.com/sirupsen/logrus\"\r\n)\r\n\r\n// Hello sample function with dependencies\r\nfunc {{handler.method}}(event functions.Event, context functions.Context) (string, error) {\r\n\tlogrus.Info(event.Data)\r\n\treturn \"Hello world!\", nil\r\n}"
                },
                {
                    id: 10,
                    name: "java (1.8)",
                    value: "java1.8",
                    editor: "java",
                    editorDependencySample: "<project xmlns=\"http://maven.apache.org/POM/4.0.0\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://maven.apache.org/POM/4.0.0 http://maven.apache.org/xsd/maven-4.0.0.xsd\">\n  <modelVersion>4.0.0</modelVersion>\n  <artifactId>function</artifactId>\n  <name>function</name>\n  <version>1.0-SNAPSHOT</version>\n  <dependencies>\n     <dependency>\n       <groupId>joda-time</groupId>\n       <artifactId>joda-time</artifactId>\n       <version>2.9.2</version>\n     </dependency>\n      <dependency>\n          <groupId>io.kubeless</groupId>\n          <artifactId>params</artifactId>\n          <version>1.0-SNAPSHOT</version>\n      </dependency>\n  </dependencies>\n  <parent>\n    <groupId>io.kubeless</groupId>\n    <artifactId>kubeless</artifactId>\n    <version>1.0-SNAPSHOT</version>\n  </parent>\n</project>",
                    editorCodeSample: "package io.kubeless;\r\n\r\nimport io.kubeless.Event;\r\nimport io.kubeless.Context;\r\n\r\npublic class {{handler.class}} {\r\n    public String {{handler.method}}(io.kubeless.Event event, io.kubeless.Context context) {\r\n        return \"Hello world!\";\r\n    }\r\n}"
                }
            ];

            $scope.closeModal = function () {
                $scope.createFormModal.hide();
                $scope.functionNameValid = null;
                $scope.isFunctionNameBlur = false;
            };

            $scope.changeRuntime = function () {
                if (!$scope.function.runtime) {
                    $scope.function.dependencies = "";
                }
                else {
                    var runtime = $filter('filter')($scope.runtimes, { value: $scope.function.runtime }, true)[0];
                    $scope.function.handler = "module.handler";
                    $scope.function.dependencies = runtime.editorDependencySample;
                }
            };

            $scope.currentApp = $localStorage.get("current_app");

            /*if (!$scope.orgId || !$scope.appId) {
             $state.go('studio.apps', { organizationId: $scope.orgId });
             }*/

            $scope.environmentChange = function (env, index, otherValue) {
                otherValue = otherValue || false;

                if (index === 2) {
                    $scope.environments[1].selected = true;
                    $scope.environments[1].disabled = !!env.selected;

                    if (otherValue) {
                        $scope.environments[2].selected = otherValue;
                    }
                }
            };

            $scope.function = {};
            $scope.functions = [];
            $scope.loading = true;
            $rootScope.breadcrumblist[2].title = 'Functions';

            $scope.createModal = function () {
                $scope.function = {};
                $scope.environments = FunctionsService.getEnvironments();
                $scope.environments[0].selected = true;

                $scope.createFormModal = $scope.createFormModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/customcode/functions/functionFormModal.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.createFormModal.$promise.then(function () {
                    $scope.createFormModal.show();
                });
            };

            $scope.getTime = function (time) {
                return moment(time).format("DD-MM-YYYY HH:ss");
            };

            $scope.save = function (functionFormValidation) {
                if (!functionFormValidation.$valid) {
                    toastr.error($filter('translate')('Module.RequiredError'));
                    return;
                }

                $scope.saving = true;
                $scope.function.content_type = 'text';
                $scope.function.environments = [];

                angular.forEach($scope.environments, function (env) {
                    if (env.selected)
                        $scope.function.environments.push(env.value);
                });

                FunctionsService.create($scope.function)
                    .then(function (response) {
                        $scope.saving = false;
                        $scope.createFormModal.hide();
                        toastr.success("Function is created successfully.");
                        $state.go('studio.app.functionDetail', { name: $scope.function.name });
                    })
                    .catch(function (response) {
                        $scope.saving = false;
                        toastr.error("Function isn't created successfully. Message: " + response.data.message);
                    })
            };

            $scope.delete = function (name, event) {
                var willDelete =
                    swal({
                        title: "Are you sure?",
                        text: " ",
                        icon: "warning",
                        buttons: ['Cancel', 'Yes'],
                        dangerMode: true
                    }).then(function (value) {
                        if (value) {
                            var elem = angular.element(event.srcElement);
                            angular.element(elem.closest('tr')).addClass('animated-background');
                            if (name) {
                                FunctionsService.delete(name)
                                    .then(function (response) {
                                        $scope.grid.dataSource.read();
                                        toastr.success("Function is deleted successfully.", "Deleted!");
                                    })
                                    .catch(function () {
                                        $scope.grid.dataSource.read();
                                    });
                            }
                        }
                    });
            };

            $scope.identifierCreate = function () {
                if (!$scope.function || !$scope.function.label) {
                    $scope.function.name = null;
                    $scope.functionNameValid = null;
                    $scope.isFunctionNameBlur = false;
                    return;
                }

                $scope.function.name = helper.getSlug($scope.function.label, '-');
                $scope.functionNameBlur($scope.function);
            };

            $scope.functionNameBlur = function (name) {
                //if ($scope.isFunctionNameBlur && $scope.functionNameValid)
                //    return;

                $scope.isFunctionNameBlur = true;
                $scope.checkFunctionName(name ? name : "");
            };

            var canceller = $q.defer();
            $scope.checkFunctionName = function (func) {
                if (!func || !func.name)
                    return;

                func.name = func.name.replace(/\s/g, '');
                func.name = func.name.replace(/[^a-zA-Z0-9\-]/g, '');

                //if (!$scope.isFunctionNameBlur)
                //    return;

                if ($scope.functionNameChecking) {
                    canceller.resolve("Cancel Request");
                    canceller = $q.defer();
                }

                $scope.functionNameChecking = true;
                $scope.functionNameValid = null;

                if (!func.name || func.name === '') {
                    $scope.functionNameChecking = false;
                    $scope.functionNameValid = false;
                    return;
                }

                FunctionsService.isFunctionNameUnique(func.name, canceller)
                    .then(function (response) {
                        console.log("response:" + response.data);
                        $scope.functionNameChecking = false;
                        if (response.data) {
                            $scope.functionNameValid = true;
                        }
                        else {
                            $scope.functionNameValid = false;
                        }
                    }).catch(angular.noop);
                /*.catch(function () {
                    $scope.functionNameValid = false;
                    $scope.functionNameChecking = false;
                })*/
            };

            $scope.checkFunctionHandler = function (func) {
                if (func.handler) {
                    func.handler = func.handler.replace(/\s/g, '');
                    func.handler = func.handler.replace(/[^a-zA-Z\.]/g, '');
                    var dotIndex = func.handler.indexOf('.');
                    if (dotIndex > -1) {
                        if (dotIndex === 0) {
                            func.handler = func.handler.split('.').join('');
                        }
                        else {
                            func.handler = func.handler.split('.').join('');
                            func.handler = func.handler.slice(0, dotIndex) + "." + func.handler.slice(dotIndex);
                        }
                    }
                }
            }

            //For Kendo UI
            $scope.goUrl = function (item) {
                var selection = window.getSelection();
                if (selection.toString().length === 0) {
                    $state.go('studio.app.functionDetail', { name: item.name }); //click event.
                }
            };

            var accessToken = $localStorage.read('access_token');

            $scope.mainGridOptions = {
                dataSource: {
                    type: "odata-v4",
                    page: 1,
                    pageSize: 10,
                    serverPaging: true,
                    serverFiltering: true,
                    serverSorting: true,
                    transport: {
                        read: {
                            url: "/api/functions/find",
                            type: 'GET',
                            dataType: "json",
                            beforeSend: function (req) {
                                req.setRequestHeader('Authorization', 'Bearer ' + accessToken);
                                req.setRequestHeader('X-App-Id', $rootScope.currentAppId);
                                req.setRequestHeader('X-Organization-Id', $rootScope.currentOrgId);
                            }
                        }
                    },
                    schema: {
                        data: "items",
                        total: "count",
                        model: {
                            id: "id",
                            fields: {
                                CreatedAt: { type: "date" }
                            }
                        },
                        parse: function (data) {
                            for (var i = 0; i < data.items.length; i++) {
                                var module = $filter('filter')($scope.modules, { id: data.items[i].module_id }, true);
                                if (module && module.length > 0)
                                    data.items[i].module = angular.copy(module[0]);
                            }

                            return data;
                        }
                    }

                },
                scrollable: false,
                persistSelection: true,
                sortable: true,
                filterable: {
                    extra: false
                },
                rowTemplate: function (e) {
                    var trTemp = '<tr ng-click="goUrl(dataItem)">';
                    trTemp += '<td> <span>' + e.name + '</span></td > ';
                    trTemp += '<td><span>' + e.label + '</span></td>';
                    trTemp += '<td><span>' + e.runtime + '</span></td>';
                    trTemp += '<td><span>' + e.handler + '</span></td>';
                    trTemp += '<td><span>' + $scope.getTime(e.created_at) + '</span></td>';
                    trTemp += '<td ng-click="$event.stopPropagation();"> <button ng-click="$event.stopPropagation(); delete(dataItem.name, $event);" type="button" class="action-button2-delete"><i class="fas fa-trash"></i></button></td></tr>';
                    return trTemp;
                },
                altRowTemplate: function (e) {
                    var trTemp = '<tr class="k-alt" ng-click="goUrl(dataItem)">';
                    trTemp += '<td> <span>' + e.name + '</span></td > ';
                    trTemp += '<td><span>' + e.label + '</span></td>';
                    trTemp += '<td><span>' + e.runtime + '</span></td>';
                    trTemp += '<td><span>' + e.handler + '</span></td>';
                    trTemp += '<td><span>' + $scope.getTime(e.created_at) + '</span></td>';
                    trTemp += '<td ng-click="$event.stopPropagation();"> <button ng-click="$event.stopPropagation(); delete(dataItem.name, $event);" type="button" class="action-button2-delete"><i class="fas fa-trash"></i></button></td></tr>';
                    return trTemp;
                },
                pageable: {
                    refresh: true,
                    pageSize: 10,
                    pageSizes: [10, 25, 50, 100],
                    buttonCount: 5,
                    info: true,
                },
                columns: [
                    {
                        field: 'Name',
                        title: 'Identifier',
                    },
                    {
                        field: 'Label',
                        title: 'Label',
                    },
                    {
                        field: 'Runtime',
                        title: 'Runtime',
                    },
                    {
                        field: 'Handler',
                        title: 'Handler',
                    },
                    {
                        field: 'CreatedAt',
                        title: 'Creation Time', 
                        filterable: {
                            ui: function (element) {
                                element.kendoDateTimePicker({
                                    format: '{0: dd-MM-yyyy  hh:mm}'
                                })
                            }
                        }
                    },
                    {
                        field: '',
                        title: '',
                        width: "90px"
                    }]
            };
            //For Kendo UI

        }
    ]);