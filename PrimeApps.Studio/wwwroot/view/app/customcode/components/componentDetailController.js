'use strict';

angular.module('primeapps')

    .controller('ComponentDetailController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'ComponentsService', 'componentPlaces', 'componentPlaceEnums', 'componentTypeEnums', '$localStorage', '$sce',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, ComponentsService, componentPlaces, componentPlaceEnums, componentTypeEnums, $localStorage, $sce) {
            $scope.modules = [];
            $scope.id = $state.params.id;
            $scope.orgId = $state.params.orgId;

            $scope.$parent.menuTopTitle = $scope.currentApp.label;
            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'components';

            $scope.app = $rootScope.currentApp;
            $scope.modules = $rootScope.appModules;
            $scope.environments = ComponentsService.getEnvironments();

            if (!$scope.id) {
                $state.go('studio.app.components');
            }

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

            $scope.loading = true;


            $scope.$parent.$parent.tabManage = {
                activeTab: "overview"
            };

            //var currentOrganization = $localStorage.get("currentApp");
            $scope.organization = $filter('filter')($rootScope.organizations, { id: $scope.orgId })[0];
            //$scope.giteaUrl = giteaUrl;

            $scope.deployments = [];

            //$scope.getFileList();

            ComponentsService.get($scope.id)
                .then(function (response) {
                    if (!response.data) {
                        toastr.error('Component Not Found !');
                        $state.go('studio.app.components');
                    }
                    $scope.content = {};
                    $scope.componentCopy = angular.copy(response.data);
                    $scope.component = response.data;
                    $scope.content.url = $filter('filter')($scope.modules, { id: $scope.component.module_id })[0]['name'];

                    if ($scope.component.content) {
                        $scope.component.content = JSON.parse($scope.component.content);

                        if ($scope.component.content.files) {
                            $scope.component.content.files = $scope.component.content.files.join("\n");
                        }

                        var urlParameters = $scope.component.content.url.split('?');
                        $scope.content.url_parameters = urlParameters.length > 1 ? urlParameters[1] : null;

                        if ($scope.component.content.app) {
                            if ($scope.component.content.app.templateFile && $scope.component.content.app.templateFile.contains('http')) {
                                $scope.content.templateUrl = true;
                            }
                        }
                    }

                    if ($scope.component.environment && $scope.component.environment.indexOf(',') > -1)
                        $scope.component.environments = $scope.component.environment.split(',');
                    else
                        $scope.component.environments = $scope.component.environment;

                    angular.forEach($scope.component.environments, function (envValue) {
                        $scope.environmentChange($scope.environments[envValue - 1], envValue - 1, true);
                    });

                    $scope.loading = false;
                });

            $scope.save = function (componentFormValidation) {
                if (!componentFormValidation.$valid) {
                    toastr.error($filter('translate')('Module.RequiredError'));
                    return;
                }


                $scope.saving = true;

                $scope.copyComponent = angular.copy($scope.component);

                if (!$scope.component.content) {
                    $scope.copyComponent.content = {};
                }

                if ($scope.component.content && $scope.component.content.files) {
                    $scope.copyComponent.content.files = $scope.component.content.files.split("\n");
                }

                if (!$scope.content.templateUrl && $scope.component.content.app && $scope.component.content.app.templateFile) {
                    $scope.copyComponent.content.app.templateUrl = $scope.component.content.app.templateFile;
                }

                $scope.copyComponent.content.url = $scope.content.url + (($scope.content.url_parameters) ? '?' + $scope.content.url_parameters : '');

                $scope.copyComponent.content = JSON.stringify($scope.copyComponent.content);

                $scope.copyComponent.environments = [];
                angular.forEach($scope.environments, function (env) {
                    if (env.selected)
                        $scope.component.environments.push(env.value);
                });

                delete $scope.copyComponent.environment;
                delete $scope.copyComponent.environment_list;


                ComponentsService.update($scope.id, $scope.copyComponent)
                    .then(function (response) {
                        $scope.saving = false;
                        $scope.editing = false;
                        toastr.success("Component updated successfully.");
                    })
                    .catch(function () {
                        $scope.saving = false;
                        $scope.editing = false;
                        toastr.error("Component not updated successfully.");
                    })
            };

            $scope.getTime = function (time) {
                return moment(time).format("DD-MM-YYYY HH:ss");
            };
        }
    ]);