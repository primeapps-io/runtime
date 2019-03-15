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

            $scope.requestModel = {
                limit: "10",
                offset: 0
            };

            $scope.reload = function () {
                $scope.loadingDeployments = true;
                ScriptsDeploymentService.count($scope.script.id)
                    .then(function (response) {
                        $scope.pageTotal = response.data;

                        if ($scope.requestModel.offset != 0 && ($scope.requestModel.offset * $scope.requestModel.limit) >= $scope.pageTotal) {
                            $scope.requestModel.offset = $scope.requestModel.offset - 1;
                        }

                        ScriptsDeploymentService.find($scope.script.id, $scope.requestModel)
                            .then(function (response) {
                                $scope.deployments = response.data;
                                $scope.loadingDeployments = false;
                            });
                    });
            };

            $scope.changePage = function (page) {
                $scope.loading = true;

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
                ScriptsDeploymentService.find(requestModel)
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

                    if (!$scope.script.place_value)
                        $scope.script.place_value = $scope.script.place;

                    $scope.script.place = $scope.componentPlaceEnums[$scope.script.place_value];
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
                if (!FormValidation.$valid)
                    return;

                $scope.saving = true;

                ScriptsService.update($scope.script)
                    .then(function (response) {
                        if (response.data)
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
                toastr.success("Deployment Started");
                ScriptsService.deploy($scope.script.name)
                    .then(function (response) {
                        //setAceOption($scope.record.runtime);
                        $scope.reload();
                    })
                    .catch(function (response) {
                        toastr.error($filter('translate')('Common.Error'));
                    });
            };
        }
    ]);