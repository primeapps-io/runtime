'use strict';

angular.module('primeapps')

    .controller('ScriptDetailController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'ScriptsService', '$localStorage', '$sce', '$window', 'ScriptsDeploymentService', 'componentPlaces', 'componentPlaceEnums',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, ScriptsService, $localStorage, $sce, $window, ScriptsDeploymentService, componentPlaces, componentPlaceEnums) {
            $scope.loadingDeployments = true;
            $scope.loading = true;
            $scope.modalLoading = false;
            $scope.showConsole = false;
            $scope.refreshLogs = false;
            $scope.modules = $rootScope.appModules;
            $scope.componentPlaces = componentPlaces;
            $scope.componentPlaceEnums = componentPlaceEnums;

            $scope.name = $state.params.name;
            $scope.orgId = $state.params.orgId;

            $scope.$parent.menuTopTitle = $scope.currentApp.label;
            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'scripts';

            $scope.script = {};
            $scope.run = {};
            $scope.response = null;

            $scope.pageTotal = 0;
            $scope.activePage = 1;

            $scope.app = $rootScope.currentApp;
            $scope.organization = $filter('filter')($rootScope.organizations, { id: $scope.orgId })[0];
            $scope.giteaUrl = giteaUrl;
            $scope.environments = ScriptsService.getEnvironments();

            if (!$scope.name) {
                $state.go('studio.app.scripts');
            }

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

            $scope.environmentChange = function (env, index, otherValue) {
                otherValue = otherValue || false;

                if (!env || index === 0) {
                    $scope.environments[0].selected = env.selected || otherValue;
                    return;
                }


                if (index === 1) {
                    $scope.environments[0].disabled = env.selected || otherValue;
                    $scope.environments[0].selected = env.selected || otherValue;

                    if (otherValue) {
                        $scope.environments[1].selected = otherValue;
                    }
                }
                else if (index === 2) {
                    $scope.environments[0].disabled = env.selected || otherValue;
                    $scope.environments[0].selected = env.selected || otherValue;
                    $scope.environments[1].disabled = env.selected || otherValue;
                    $scope.environments[1].selected = env.selected || otherValue;

                    if (otherValue) {
                        $scope.environments[2].selected = otherValue;
                    }
                }
            };

            $scope.requestModel = {
                limit: "10",
                offset: 0
            };

            $scope.changePage = function (page) {
                $scope.loadingDeployments = true;
                if (page !== 1) {
                    var difference = Math.ceil($scope.pageTotal / $scope.requestModel.limit);

                    if (page > difference) {
                        if (Math.abs(page - difference) < 1)
                            --page;
                        else
                            page = page - Math.abs(page - Math.ceil($scope.pageTotal / $scope.requestModel.limit))
                    }
                }

                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;
                ScriptsDeploymentService.find($scope.script.id, requestModel)
                    .then(function (response) {
                        $scope.deployments = response.data;
                        $scope.loadingDeployments = false;
                    })
                    .catch(function (response) {
                        toastr.error($filter('translate')('Common.Error'));
                        $scope.loadingDeployments = false;
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
                $scope.changePage($scope.activePage);
            };

            ScriptsService.getByName($scope.name)
                .then(function (response) {
                    if (!response.data) {
                        toastr.error('Script Not Found !');
                        $state.go('studio.app.scripts');
                    }

                    $scope.scriptCopy = angular.copy(response.data);
                    $scope.script = response.data;

                    if ($scope.script.custom_url)
                        $scope.tabManage.activeTab = 'settings';

                    ScriptsDeploymentService.count($scope.script.id)
                        .then(function (response) {
                            $scope.pageTotal = response.data;
                        });

                    if (!$scope.script.place_value)
                        $scope.script.place_value = $scope.script.place;

                    if ($scope.script.environment && $scope.script.environment.indexOf(',') > -1)
                        $scope.script.environments = $scope.script.environment.split(',');
                    else
                        $scope.script.environments = $scope.script.environment;

                    angular.forEach($scope.script.environments, function (envValue) {
                        $scope.environmentChange($scope.environments[envValue - 1], envValue - 1, true);
                    });

                    $scope.script.place = $scope.componentPlaceEnums[$scope.script.place_value];
                    $scope.changePage(1);
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

            $scope.closeModal = function () {
                $scope.script = angular.copy($scope.scriptCopy);
                $scope.showFormModal.hide();
            };

            $scope.asHtml = function () {
                return $sce.trustAsHtml($scope.logs);
            };

            $scope.createIdentifier = function () {
                if (!$scope.script || !$scope.script.label) {
                    $scope.script.name = null;
                    return;
                }

                $scope.script.name = helper.getSlug($scope.script.label, '-');
                $scope.scriptNameBlur($scope.script);
            };

            $scope.scriptNameBlur = function (script) {
                if ($scope.isScriptNameBlur && $scope.scriptNameValid)
                    return;

                $scope.isScriptNameBlur = true;
                $scope.checkScriptName(script ? script : null);
            };

            $scope.checkScriptName = function (script) {
                if (!script || !script.name)
                    return;

                script.name = script.name.replace(/\s/g, '');
                script.name = script.name.replace(/[^a-zA-Z0-9\-]/g, '');

                if (!$scope.isScriptNameBlur)
                    return;

                $scope.scriptNameChecking = true;
                $scope.scriptNameValid = null;

                if (!script.name || script.name === '') {
                    $scope.scriptNameChecking = false;
                    $scope.scriptNameValid = false;
                    return;
                }

                ScriptsService.isUniqueName(script.name)
                    .then(function (response) {
                        $scope.scriptNameChecking = false;
                        if (response.data) {
                            $scope.scriptNameValid = true;
                        }
                        else {
                            $scope.scriptNameValid = false;
                        }
                    })
                    .catch(function () {
                        $scope.scriptNameValid = false;
                        $scope.scriptNameChecking = false;
                    });
            };

            $scope.save = function (FormValidation) {
                if (!FormValidation.$valid) {
                    if (FormValidation.custom_url.$invalid)
                        toastr.error("Please enter a valid url.");
                    else
                        toastr.error($filter('translate')('Setup.Modules.RequiredError'));

                    return;
                }

                $scope.saving = true;

                $scope.script.environments = [];
                angular.forEach($scope.environments, function (env) {
                    if (env.selected)
                        $scope.script.environments.push(env.value);
                });

                delete $scope.script.environment;
                delete $scope.script.environment_list;

                ScriptsService.update($scope.script)
                    .then(function (response) {
                        toastr.success("Script saved successfully.");
                        $scope.scriptCopy = angular.copy($scope.script);
                        $scope.saving = false;
                    })
                    .catch(function (reason) {
                        toastr.error($filter('translate')('Common.Error'));
                        $scope.saving = false;
                    });
            };

            $scope.runDeployment = function () {
                $scope.loadingDeployments = true;
                ScriptsService.deploy($scope.script.name)
                    .then(function (response) {
                        toastr.success("Deployment Started");
                        $scope.pageTotal = $scope.pageTotal + 1;
                        $scope.activePage = 1;
                        $scope.changePage(1);
                    })
                    .catch(function (response) {
                        $scope.loadingDeployments = false;
                        if (response.status === 409) {
                            toastr.warning(response.data);
                        }
                        else {
                            toastr.error($filter('translate')('Common.Error'));
                        }
                    });
            };
        }
    ]);