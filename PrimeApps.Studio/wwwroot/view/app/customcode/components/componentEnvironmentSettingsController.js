'use strict';

angular.module('primeapps')

    .controller('ComponentEnvironmentSettingsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'ComponentsService', 'componentPlaces', 'componentPlaceEnums', 'componentTypeEnums', '$localStorage', 'ComponentsDeploymentService', '$sce',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, ComponentsService, componentPlaces, componentPlaceEnums, componentTypeEnums, $localStorage, ComponentsDeploymentService, $sce) {

            $scope.$parent.menuTopTitle = $scope.currentApp.label;
            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'components';
            $scope.app = $rootScope.currentApp;
            /*prepare headersArray for development, test, production*/
            $scope.headersArray = {
                'development0': ['X-User-Id', 'X-Tenant-Id', 'X-App-Id', 'X-Auth-Key', 'X-Branch-Id', 'X-Tenant-Language', 'Custom']
            };
            $scope.headersArray['test0'] = angular.copy($scope.headersArray['development0']);
            $scope.headersArray['production0'] = angular.copy($scope.headersArray['development0']);

            $scope.copyheadersArray = angular.copy($scope.headersArray);

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

                prepareAutoValues($scope.contentCopy);
                $scope.componentCopy.content = JSON.stringify($scope.contentCopy);

                ComponentsService.update($scope.id, $scope.componentCopy)
                    .then(function () {
                        $scope.saving = false;
                        toastr.success("Global Config updated successfully.");
                        $scope.loading = false;
                        $state.go('studio.app.components');
                    })
                    .catch(function () {
                        $scope.saving = false;
                        $scope.loading = false;
                        toastr.error("Global Config didn't update.");
                        $state.go('studio.app.components');
                    });
            };

            $scope.parameterRemove = function (index, parameterArray, key) {

                /*Objectse gelen key'e objec'den siliyoruz*/
                if (key || key === "") {
                    delete parameterArray[key];
                    $scope.headerChange(key, parameterArray);
                    if (!key.contains('Custom'))
                        $scope.headersArray[$scope.activeTab + $scope.currentIndex].push(key);
                    else
                        delete $scope.customInputObject[$scope.activeTab][key];
                } else {
                    parameterArray.splice(index, 1);
                    delete $scope.headersArray[$scope.activeTab + index]
                }
            };

            $scope.parameterAdd = function (addItem, no, index) {

                var parameter = {};
                var parameterArray = [];
                switch (no) {
                    case 1:
                        parameter.urls = addItem ? addItem.urls : "";
                        parameter.headers = {};

                        if ($scope.trustedUrlsParameters && $scope.trustedUrlsParameters.length > 0) {
                            $scope.headersArray[$scope.activeTab + $scope.trustedUrlsParameters.length] = angular.copy($scope.copyheadersArray[$scope.activeTab + '0']);
                        } else {
                            $scope.headersArray[$scope.activeTab + '0'] = angular.copy($scope.copyheadersArray[$scope.activeTab + '0']);
                        }
                        parameterArray = $scope.trustedUrlsParameters ? $scope.trustedUrlsParameters : parameterArray;
                        break;
                    case 2:
                        parameter.route_template_urls = addItem ? addItem.route_template_urls : "";
                        parameterArray = $scope.routeParameters ? $scope.routeParameters : parameterArray;
                        break;
                    case 3:
                        parameter.key = addItem ? addItem.key : "";
                        parameter.value = addItem ? addItem.value : "";
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
                        if (angular.isObject(lastConfigParameter[key])) {
                            for (var subKey in lastConfigParameter[key]) {
                                if (lastConfigParameter[key].hasOwnProperty(subKey))
                                    lastConfigParameter[key][subKey] = !subKey.contains('Custom') ? 'Auto set' : null;
                            }
                        } else
                            lastConfigParameter[key] = null;
                    }
                }
            };

            $scope.changeTab = function (tabName, previousContent) {

                $scope.activeTab = tabName;
                $scope.previousTabName = $scope.$parent.$parent.tabManage.activeTab;
                $scope.$parent.$parent.tabManage.activeTab = tabName;
                $scope.customIndex = 0;

                setPreviousContent($scope.previousTabName, previousContent);

                switch (tabName) {
                    case "development":
                        $scope.currenContent = $scope.developmentContentState ? $scope.developmentContentState : $scope.contentCopy.development;
                        break;
                    case "test":
                        $scope.currenContent = $scope.testContentState ? $scope.testContentState : $scope.contentCopy.test;
                        break;
                    case "production":
                        $scope.currenContent = $scope.productionContentState ? $scope.productionContentState : $scope.contentCopy.production;
                        break;
                }

                if (Object.keys($scope.currenContent).length > 0) {
                    $scope.trustedUrlsParameters = [];
                    if ($scope.currenContent.trusted_urls && $scope.currenContent.trusted_urls.length > 0) {

                        setCustomInputObject($scope.currenContent.trusted_urls);

                        for (var i = 0; i < $scope.currenContent.trusted_urls.length; i++) {
                            var parameters = {};
                            parameters.urls = $scope.currenContent.trusted_urls[i].url;
                            parameters.headers = {};
                            for (var key in $scope.currenContent.trusted_urls[i].headers) {
                                if ($scope.currenContent.trusted_urls[i].headers.hasOwnProperty(key))
                                    parameters.headers[key] = key.contains('Custom') ? $scope.currenContent.trusted_urls[i].headers[key] : 'Auto set';
                            }

                            $scope.trustedUrlsParameters.push(parameters);
                        }
                    }

                    $scope.routeParameters = [];
                    if ($scope.currenContent.route_template_urls && $scope.currenContent.route_template_urls.length > 0) {
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
                        /*End*/

                        if ($scope.trustedUrlsParameters[i].urls && $scope.trustedUrlsParameters[i].urls !== "")
                            parameters.url = $scope.trustedUrlsParameters[i].urls;

                        if ($scope.trustedUrlsParameters[i].headers) {
                            for (var key in $scope.trustedUrlsParameters[i].headers) {
                                if ($scope.trustedUrlsParameters[i].headers.hasOwnProperty(key))
                                    parameters.headers[key] = $scope.trustedUrlsParameters[i].headers[key];
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

            $scope.addHeader = function (header, headers) {

                if (header === 'Custom') {
                    $scope.headerChange('Custom', headers);
                    $scope.changeCustomInput($scope.resChangedKey, '');
                } else {
                    headers[header] = 'Auto set';
                    var index = $scope.headersArray[$scope.activeTab + $scope.currentIndex].indexOf(header);
                    $scope.headersArray[$scope.activeTab + $scope.currentIndex].splice(index, 1);
                }
            };

            $scope.headerChange = function (key, headers) {

                if (key === 'Custom') {
                    var newKey = undefined;
                    if ($scope.customInputObject)
                        for (var k in $scope.customInputObject[$scope.activeTab]) {
                            if (!newKey && k.contains('Custom'))
                                newKey = k;
                            else if (newKey && k.contains('Custom'))
                                newKey = newKey.length > k.length ? newKey : k;
                        }

                    if (newKey && newKey.length > 6 && !$scope.customIndex > 0) {
                        $scope.customIndex = parseInt(newKey.slice(6)) + 1;
                    } else if (newKey && newKey.length === 6 && !$scope.customIndex > 0) {
                        $scope.customIndex = 1;
                    } else if ($scope.customIndex > 0) {
                        $scope.customIndex += 1;
                    }

                    var nKey = $scope.customIndex > 0 ? key + $scope.customIndex : key;
                    $scope.resChangedKey = nKey;
                    headers[nKey] = '';
                }
            };

            $scope.setIndex = function (index) {
                $scope.currentIndex = index;
            };
            $scope.changeCustomInput = function (key, custom) {

                if (!$scope.customInputObject) {
                    $scope.customInputObject = {};
                }
                if (!$scope.customInputObject.hasOwnProperty($scope.activeTab))
                    $scope.customInputObject[$scope.activeTab] = {};

                $scope.customInputObject[$scope.activeTab][key] = custom;
            };

            var prepareAutoValues = function (allContent) {
                //k = development-test-production
                for (var k in allContent) {
                    if (allContent.hasOwnProperty(k))
                        for (var k1 in allContent[k]) {
                            if (allContent[k].hasOwnProperty(k1) && k1 === 'trusted_urls') {
                                //trusted_urls array'i
                                for (var o = 0; o < allContent[k][k1].length; o++) {
                                    for (var k2 in allContent[k][k1][o].headers) {
                                        //k2 -Custom,Custom1,X-tenant-Id vb. keyler
                                        if (allContent[k][k1][o].headers.hasOwnProperty(k2) && k2.contains('Custom')) {
                                            /*customInputObject'ten ilgili değeri mapliyoruz*/
                                            var oldVal = allContent[k][k1][o].headers[k2];
                                            var nKey = $scope.customInputObject[k][k2];

                                            /*Eski değeri silip, yeni keyi eklemeliyiz*/
                                            delete allContent[k][k1][o].headers[k2];
                                            allContent[k][k1][o].headers[nKey] = oldVal;
                                        } else
                                            allContent[k][k1][o].headers[k2] = allContent[k][k1][o].headers[k2] === 'Auto set' ? '::dynamic' : allContent[k][k1][o].headers[k2];
                                    }
                                }
                            }
                        }
                }
            };

            var setCustomInputObject = function (trustedUrls) {

                if (!$scope.customInputObject)
                    $scope.customInputObject = {};

                if (!$scope.customInputObject.hasOwnProperty($scope.activeTab))
                    $scope.customInputObject[$scope.activeTab] = {};

                for (var o = 0; o < trustedUrls.length; o++) {
                    for (var k in trustedUrls[o].headers) {
                        if (trustedUrls[o].headers.hasOwnProperty(k)) {
                            if (!$scope.headersArray[o > 0 ? $scope.activeTab + o : $scope.activeTab + o])
                                $scope.headersArray[o > 0 ? $scope.activeTab + o : $scope.activeTab + o] = angular.copy($scope.copyheadersArray[$scope.activeTab + '0']);

                            var index = $scope.headersArray[o > 0 ? $scope.activeTab + o : $scope.activeTab + o].indexOf(k);

                            if (index === -1 && !k.contains('Custom')) {

                                var oldVal = trustedUrls[o].headers[k];
                                var num = Object.keys($scope.customInputObject[$scope.activeTab]).length;
                                var nKey = num > 0 && num !== 'NaN' ? 'Custom' + num : 'Custom';
                                /*Daha önceden ekli olan CustomInput datasını siliyoruz
                                * d1 : "development1" değerini
                                * Custom:"development1" olarak değiştiriyoruz
                                * */
                                delete trustedUrls[o].headers[k];
                                trustedUrls[o].headers[nKey] = oldVal === '::dynamic' ? 'Auto set' : oldVal;
                                $scope.customInputObject[$scope.activeTab][nKey] = k;
                            } else if (!k.contains('Custom')) {
                                trustedUrls[o].headers[k] = 'Auto set';
                                $scope.headersArray[$scope.activeTab + o].splice(index, 1);
                            }
                        }
                    }
                }

            };
        }
    ]);