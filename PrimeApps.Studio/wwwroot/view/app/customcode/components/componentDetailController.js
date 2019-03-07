'use strict';

angular.module('primeapps')

    .controller('ComponentDetailController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'ComponentsService', 'componentPlaces', 'componentPlaceEnums', 'componentTypeEnums', '$localStorage', 'ComponentsDeploymentService', '$sce',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, ComponentsService, componentPlaces, componentPlaceEnums, componentTypeEnums, $localStorage, ComponentsDeploymentService, $sce) {
            $scope.modules = [];
            $scope.id = $state.params.id;
            $scope.orgId = $state.params.orgId;

            $scope.$parent.menuTopTitle = $scope.currentApp.label;
            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'components';

            $scope.app = $rootScope.currentApp;
            $scope.modules = $rootScope.appModules;

            /*if (!$scope.orgId || !$scope.appId) {
             $state.go('studio.apps', { organizationId: $scope.orgId });
             }*/

            $scope.loading = true;
            //var currentOrganization = $localStorage.get("currentApp");
            $scope.organization = $filter('filter')($rootScope.organizations, {id: $scope.orgId})[0];
            $scope.giteaUrl = giteaUrl;

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
            
            if (!$scope.id) {
                $state.go('studio.app.components');
            }

            $scope.reload = function () {
                $scope.loadingDeployments = true;
                ComponentsDeploymentService.count($scope.id)
                    .then(function (response) {
                        $scope.pageTotal = response.data;

                        if ($scope.requestModel.offset !== 0 && ($scope.requestModel.offset * $scope.requestModel.limit) >= $scope.pageTotal) {
                            $scope.requestModel.offset = $scope.requestModel.offset - 1;
                        }

                        ComponentsDeploymentService.find($scope.component.id, $scope.requestModel)
                            .then(function (response) {
                                $scope.deployments = response.data;
                                $scope.loadingDeployments = false;
                            });
                    });
            };
            
            ComponentsService.getFileList($scope.id)
                .then(function (response) {
                    $scope.files = [];
                    angular.forEach(response.data, function (file) {
                        var path = {'path': file.path, 'value': file.path.replace('components/' + $scope.component.name + '/', '')};
                        $scope.files.push(path)
                    });
                })
                .catch(function (response) {
                    console.log('error: ' + response);
                });

            ComponentsService.get($scope.id)
                .then(function (response) {
                    if (!response.data) {
                        toastr.error('Component Not Found !');
                        $state.go('studio.app.components');
                    }
                    $scope.reload();
                    $scope.content = {};
                    $scope.componentCopy = angular.copy(response.data);
                    $scope.component = response.data;
                    $scope.content.url = $filter('filter')($scope.modules, {id: $scope.component.module_id})[0]['name'];

                    if ($scope.component.content) {
                        $scope.component.content = JSON.parse($scope.component.content);

                        if ($scope.component.content.files) {
                            $scope.component.content.files = $scope.component.content.files.join("\n");
                        }

                        var urlParameters = $scope.component.content.url.split('?');
                        $scope.content.url_parameters = urlParameters.length > 1 ? urlParameters[1] : null;

                        if ($scope.component.content.app) {
                            if ($scope.component.content.app.templateUrl && $scope.component.content.app.templateUrl.contains('http')) {
                                $scope.content.templateUrl = true;
                            }
                        }
                    }

                    $scope.loading = false;
                });

            $scope.isTemplateFile = function () {
                return function (item) {
                    return item.value.contains('.html');
                };
            };

            $scope.save = function (componentFormValidation) {
                if (!componentFormValidation.$valid)
                    return;

                $scope.saving = true;

                if (!$scope.component.content) {
                    $scope.component.content = {};
                }

                if ($scope.component.content.files) {
                    $scope.component.content.files = $scope.component.content.files.split("\n");
                }

                $scope.component.content.url = $scope.content.url + (($scope.content.url_parameters) ? '?' + $scope.content.url_parameters : null);

                $scope.component.content = JSON.stringify($scope.component.content);

                ComponentsService.update($scope.id, $scope.component)
                    .then(function (response) {
                        $scope.saving = false;
                        $scope.editing = false;
                    })
            };

            $scope.runDeployment = function () {
                toastr.success("Deployment Started");
                ComponentsService.deploy($scope.id)
                    .then(function (response) {
                        //setAceOption($scope.record.runtime);
                        $scope.reload();
                    })
                    .catch(function (response) {
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
        }
    ]);