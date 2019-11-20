'use strict';

angular.module('primeapps')

    .controller('ComponentEnvironmentSettingsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'ComponentsService', 'componentPlaces', 'componentPlaceEnums', 'componentTypeEnums', '$localStorage', 'ComponentsDeploymentService', '$sce',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, ComponentsService, componentPlaces, componentPlaceEnums, componentTypeEnums, $localStorage, ComponentsDeploymentService, $sce) {

            $scope.$parent.menuTopTitle = $scope.currentApp.label;
            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'components';
            $scope.app = $rootScope.currentApp;

            $scope.loading = true;

            $scope.$parent.$parent.tabManage = {
                activeTab: "development"
            };

            $scope.configParameters = [];
            var parameter = {};
            parameter.key = null;
            parameter.value = null;
            $scope.configParameters.push(parameter);

            var getRecord = function () {
                ComponentsService.getGlobalConfig()
                    .then(function (response) {

                        if (!response.data) {
                            toastr.error('Component Not Found !');
                            $state.go('studio.app.components');
                        }

                        $scope.id = response.data.id;
                        $scope.content = {};
                        /**development,test,production*/
                        $scope.currenContent = {};
                        $scope.component = response.data;
                        $scope.componentCopy = angular.copy(response.data);

                        if ($scope.component.content) {

                            $scope.component.content = JSON.parse($scope.component.content);
                            $scope.contentCopy = angular.copy($scope.component.content);

                            var activeTab = $scope.$parent.$parent.tabManage.activeTab;
                            $scope.changeTab(activeTab);
                        }

                        $scope.loading = false;
                    });
            };
            getRecord();
            $scope.save = function (contentFormValidation) {

                $scope.saving = true;
                $scope.loading = true;

                prepareContent($scope.content);
                switch ($scope.$parent.$parent.tabManage.activeTab) {
                    case "development":
                        $scope.contentCopy.development = $scope.currenContent;
                        $scope.contentCopy.test = $scope.testContentState ? $scope.testContentState : $scope.contentCopy.test;
                        $scope.contentCopy.production = $scope.productionContentState ? $scope.productionContentState : $scope.contentCopy.production;
                        break;
                    case "test":
                        $scope.contentCopy.test = $scope.currenContent;
                        $scope.contentCopy.development = $scope.developmentContentState ? $scope.developmentContentState : $scope.contentCopy.development;
                        $scope.contentCopy.production = $scope.productionContentState ? $scope.productionContentState : $scope.contentCopy.production;
                        break;
                    case "production":
                        $scope.contentCopy.development = $scope.developmentContentState ? $scope.developmentContentState : $scope.contentCopy.development;
                        $scope.contentCopy.test = $scope.testContentState ? $scope.testContentState : $scope.contentCopy.test;
                        $scope.contentCopy.production = $scope.currenContent;
                        break;
                }

                $scope.componentCopy.content = JSON.stringify($scope.contentCopy);

                ComponentsService.update($scope.id, $scope.componentCopy)
                    .then(function () {
                        $scope.saving = false;
                        toastr.success("Global Config updated successfully.");
                        $scope.loading = false;
                        getRecord();
                    })
                    .catch(function () {
                        $scope.saving = false;
                        $scope.loading = false;
                        toastr.error("Global Config didn't update.");
                    });
            };

            $scope.parameterRemove = function (itemName, parameterArray) {
                var index = parameterArray.indexOf(itemName);
                parameterArray.splice(index, 1);
            };

            $scope.parameterAdd = function (addItem, no) {

                var parameter = {};
                var parameterArray = [];
                switch (no) {
                    case 1:
                        parameter.urls = addItem.urls;
                        parameter.headers = {};
                        parameter.headers.x_user_id = addItem.headers["X-User-Id"];
                        parameter.headers.x_tenant_id = addItem.headers["X-Tenant-Id"];
                        parameterArray = $scope.trustedUrlsParameters;
                        break;
                    case 2:
                        parameter.route_template_urls = addItem.route_template_urls;
                        parameterArray = $scope.routeParameters;
                        break;
                    case 3:
                        parameter.key = addItem.key;
                        parameter.value = addItem.value;
                        parameterArray = $scope.configParameters;
                        break;
                }

                if (Object.keys(parameter).length > 0) {
                    if (parameterArray.length <= 20) {
                        parameterArray.push(parameter);
                    }
                }

                var lastConfigParameter = parameterArray [parameterArray.length - 1];
                for (var key in lastConfigParameter) {
                    if (lastConfigParameter.hasOwnProperty(key)) {
                        lastConfigParameter[key] = null;
                    }
                }
            };

            $scope.changeTab = function (tabName, previousContent) {

                $scope.previousTabName = $scope.$parent.$parent.tabManage.activeTab;
                $scope.$parent.$parent.tabManage.activeTab = tabName;

                setPreviousContent($scope.previousTabName, previousContent);

                switch (tabName) {
                    case "development":
                        $scope.currenContent = $scope.developmentContentState ? $scope.developmentContentState : $scope.contentCopy.development;
                        break;
                    case "test":
                        $scope.currenContent = $scope.testContentState ? $scope.testContentState : $scope.contentCopy.test;
                        break;
                    case "production":
                        $scope.currenContent = $scope.productionContentStat ? $scope.productionContentStat : $scope.contentCopy.production;
                        break;
                }

                if (Object.keys($scope.currenContent).length > 0) {

                    if ($scope.currenContent.trusted_urls && $scope.currenContent.trusted_urls.length > 0) {

                        $scope.trustedUrlsParameters = [];
                        for (var i = 0; i < $scope.currenContent.trusted_urls.length; i++) {
                            var parameters = {};
                            parameters.urls = $scope.currenContent.trusted_urls[i].url;
                            parameters.headers = {};
                            parameters.headers.x_user_id = $scope.currenContent.trusted_urls[i].headers["X-User-Id"];
                            parameters.headers.x_tenant_id = $scope.currenContent.trusted_urls[i].headers["X-Tenant-Id"];
                            $scope.trustedUrlsParameters.push(parameters);
                        }
                    }
                    if ($scope.currenContent.route_template_urls && $scope.currenContent.route_template_urls.length > 0) {
                        $scope.routeParameters = [];
                        for (var i = 0; i < $scope.currenContent.route_template_urls.length; i++) {
                            var routeParameter = {};
                            routeParameter.route_template_urls = $scope.currenContent.route_template_urls[i];
                            $scope.routeParameters.push(routeParameter);
                        }
                    }
                    if ($scope.currenContent.imports) {
                        $scope.content.imports = {};
                        $scope.content.imports.css = {};
                        $scope.content.imports.css.before = [];
                        $scope.content.imports.css.after = [];
                        $scope.content.imports.js = {};
                        $scope.content.imports.js.before = [];
                        $scope.content.imports.js.after = [];

                        for (var i = 0; i < $scope.currenContent.imports.css.before.length; i++) {
                            var value = $scope.currenContent.imports.css.before[i];
                            if (value !== "")
                                $scope.content.imports.css.before += value + ";";
                        }

                        for (var i = 0; i < $scope.currenContent.imports.css.after.length; i++) {
                            var value = $scope.currenContent.imports.css.after[i];
                            if (value !== "")
                                $scope.content.imports.css.after += value + ";";
                        }

                        for (var i = 0; i < $scope.currenContent.imports.js.before.length; i++) {
                            var value = $scope.currenContent.imports.js.before[i];
                            if (value !== "")
                                $scope.content.imports.js.before += value + ";";
                        }

                        for (var i = 0; i < $scope.currenContent.imports.js.after.length; i++) {
                            var value = $scope.currenContent.imports.js.after[i];
                            if (value !== "")
                                $scope.content.imports.js.after += value + ";";
                        }
                    }
                    if ($scope.currenContent.configs) {

                        $scope.configParameters = [];

                        angular.forEach($scope.currenContent.configs, function (value, key) {
                            var parameter = {};
                            parameter.key = key;
                            parameter.value = value;
                            $scope.configParameters.push(parameter);
                        });
                    }
                }
            };

            var prepareConfigs = function () {
                var configArray = [];
                $scope.currenContent.configs = {};
                for (var i = 0; i < $scope.configParameters.length; i++) {
                    var configParameter = $scope.configParameters[i];
                    var query = configParameter.key + '":"' + configParameter.value + '"';
                    if (i === 0 && i !== $scope.configParameters.length - 1) {
                        query = '{"' + query;
                    } else if (i > 0 && i !== $scope.configParameters.length - 1) {
                        query = '"' + query;
                    } else if (i !== 0 && i === $scope.configParameters.length - 1) {
                        query = '"' + query + "}";
                    } else {
                        query = '{"' + query + '}';
                    }
                    configArray.push(query);
                }
                return configArray.toString();
            };

            var setPreviousContent = function (previousTabName, previousContent) {
                if (previousContent) {
                    previousContent = prepareContent(previousContent);
                    switch (previousTabName) {
                        case "development":
                            $scope.developmentContentState = previousContent ? previousContent : $scope.contentCopy.development;
                            break;
                        case "test":
                            $scope.testContentState = previousContent ? previousContent : $scope.contentCopy.test;
                            break;
                        case "production":
                            $scope.productionContentState = previousContent ? previousContent : $scope.contentCopy.production;
                            break;
                    }
                }
            };

            var prepareContent = function (content) {

                if ($scope.trustedUrlsParameters && $scope.trustedUrlsParameters.length > 0) {
                    $scope.currenContent.trusted_urls = [];
                    for (var i = 0; i < $scope.trustedUrlsParameters.length; i++) {

                        /*Tablar değiştiğinde daha önceden eklenmiş olan parametreleri silmemek için yazıldı*/
                        var parameters = {};
                        parameters.url = "";
                        parameters.headers = {};
                        parameters.headers["X-User-Id"] = "";
                        parameters.headers["X-Tenant-Id"] = "";
                        /*End*/

                        if ($scope.trustedUrlsParameters[i].urls && $scope.trustedUrlsParameters[i].urls !== "")
                            parameters.url = $scope.trustedUrlsParameters[i].urls;

                        if ($scope.trustedUrlsParameters[i].headers) {
                            if ($scope.trustedUrlsParameters[i].headers["x_user_id"]) {
                                parameters.headers["X-User-Id"] = $scope.trustedUrlsParameters[i].headers["x_user_id"];
                            }
                            if ($scope.trustedUrlsParameters[i].headers["x_tenant_id"]) {
                                parameters.headers["X-Tenant-Id"] = $scope.trustedUrlsParameters[i].headers["x_tenant_id"];
                            }
                        }
                        /*Tablar değiştiğinde daha önceden eklenmiş olan parametreleri silmemek için yazıldı*/
                        if ($scope.saving && (parameters.url !== "" || parameters.headers["X-User-Id"] !== "" || parameters.headers["X-Tenant-Id"] !== ""))
                            $scope.currenContent.trusted_urls.push(parameters);
                        else if (!$scope.saving)
                            $scope.currenContent.trusted_urls.push(parameters);
                        /*End*/
                    }
                }

                if ($scope.routeParameters && $scope.routeParameters.length > 0) {
                    $scope.currenContent.route_template_urls = [];
                    for (var i = 0; i < $scope.routeParameters.length; i++) {
                        $scope.currenContent.route_template_urls.push($scope.routeParameters[i].route_template_urls);
                    }
                }

                if (content && content.imports && content.imports.css) {

                    var cssBeforeArray = content.imports.css.before.length > 0 ? content.imports.css.before.split(';') : [];
                    $scope.currenContent.imports.css.before = [];
                    for (var i = 0; i < cssBeforeArray.length; i++) {
                        var value = cssBeforeArray[i];
                        if (value !== "")
                            $scope.currenContent.imports.css.before.push(value);
                    }

                    var cssAfterArray = content.imports.css.after.length > 0 ? content.imports.css.after.split(';') : [];
                    $scope.currenContent.imports.css.after = [];
                    for (var i = 0; i < cssAfterArray.length; i++) {
                        var value = cssAfterArray[i];
                        if (value !== "")
                            $scope.currenContent.imports.css.after.push(value);
                    }
                }
                if (content && content.imports && content.imports.js) {

                    var jsBeforeArray = content.imports.js.before.length > 0 ? content.imports.js.before.split(';') : [];
                    $scope.currenContent.imports.js.before = [];
                    for (var i = 0; i < jsBeforeArray.length; i++) {
                        var value = jsBeforeArray[i];
                        if (value !== "")
                            $scope.currenContent.imports.js.before.push(value);
                    }
                }


                var jsAfterArray = content.imports.js.after.length > 0 ? content.imports.js.after.split(';') : [];
                $scope.currenContent.imports.js.after = [];
                for (var i = 0; i < jsAfterArray.length; i++) {
                    var value = jsAfterArray[i];
                    if (value !== "")
                        $scope.currenContent.imports.js.after.push(value);
                }
                if ($scope.configParameters && $scope.configParameters.length > 0) {
                    var configsArrayString = prepareConfigs();
                    $scope.currenContent.configs = JSON.parse(configsArrayString);
                }
                return $scope.currenContent;
            };
        }
    ]);