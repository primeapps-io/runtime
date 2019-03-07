'use strict';

angular.module('primeapps')

    .controller('ScriptDetailController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'ScriptsService', '$localStorage', '$sce', '$window', 'ScriptsDeploymentService',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, ScriptsService, $localStorage, $sce, $window, ScriptsDeploymentService) {
            $scope.loadingDeployments = true;
            $scope.loading = true;
            $scope.modalLoading = false;
            $scope.showConsole = false;
            $scope.refreshLogs = false;

            $scope.name = $state.params.name;
            $scope.orgId = $state.params.orgId;

            $scope.$parent.menuTopTitle = $scope.currentApp.label;
            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'scripts';

            $scope.script = {};
            $scope.run = {};
            $scope.response = null;

            $scope.pageTotal = 0;

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

                        ScriptsDeploymentService.find($scope.function.id, $scope.requestModel)
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
                $scope.changePage(1)
            };

            ScriptsService.getByName($scope.name)
                .then(function (response) {
                    if (!response.data) {
                        toastr.error('Script Not Found !');
                        $state.go('studio.app.scripts');
                    }
                    $scope.scriptCopy = angular.copy(response.data);
                    $scope.script = response.data;
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
                return $filter('filter')($scope.runtimes, { value: $scope.script.runtime })[0].type;
            };

            $scope.closeModal = function () {
                $scope.script = angular.copy($scope.scriptCopy);
                $scope.showFormModal.hide();
            };

            $scope.runScript = function (run) {
                $scope.running = true;
                $scope.response = null;
                ScriptsService.run($scope.name, run.type, run.body)
                    .then(function (response) {
                        $scope.response = response.data;
                        $scope.running = false;
                    })
                    .catch(function (response) {
                        if (response.status === 503) {
                            toastr.error('No endpoints available for service ' + $scope.script.name);
                        }
                        else {
                            toastr.error('An error occurred while running the script !');
                        }
                        $scope.running = false;
                    });
            };

            $scope.asHtml = function () {
                return $sce.trustAsHtml($scope.logs);
            };

            $scope.checkScriptHandler = function (script) {
                if (script.handler) {
                    script.handler = script.handler.replace(/\s/g, '');
                    script.handler = script.handler.replace(/[^a-zA-Z\.]/g, '');
                    var dotIndex = script.handler.indexOf('.');
                    if (dotIndex > -1) {
                        if (dotIndex == 0) {
                            script.handler = script.handler.split('.').join('');
                        }
                        else {
                            script.handler = script.handler.split('.').join('');
                            script.handler = script.handler.slice(0, dotIndex) + "." + script.handler.slice(dotIndex);
                        }
                    }
                }
            };

            $scope.save = function (FormValidation) {
                if (!FormValidation.$valid)
                    return;

                $scope.saving = true;

                ScriptsService.update($scope.name, $scope.script)
                    .then(function (response) {
                        $scope.scriptCopy = angular.copy($scope.script);
                        $scope.saving = false;
                        toastr.success("Script saved successfully.");
                        $scope.saving = false;

                    });
            };
        }
    ]);