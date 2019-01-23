'use strict';

angular.module('primeapps')

    .controller('ComponentsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'ComponentsService', 'componentPlaces', 'componentPlaceEnums', 'componentTypes', 'componentTypeEnums', '$localStorage',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService, ComponentsService, componentPlaces, componentPlaceEnums, componentTypes, componentTypeEnums, $localStorage) {
            $scope.appId = $state.params.appId;
            $scope.orgId = $state.params.orgId;

            $scope.$parent.menuTopTitle = $scope.currentApp.label
            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'components';

            $scope.currentApp = $localStorage.get("current_app");

            /*if (!$scope.orgId || !$scope.appId) {
                $state.go('studio.apps', { organizationId: $scope.orgId });
            }*/

            $scope.modules = [];

            $scope.component = {};
            $scope.components = [];
            $scope.loading = true;
            $scope.componentPlaces = componentPlaces;
            $scope.componentTypes = componentTypes;
            $rootScope.breadcrumblist[2].title = 'Components';

            $scope.requestModel = {
                limit: "10",
                offset: 0
            };

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }
            };

            $scope.generator(10);

            ComponentsService.count()
                .then(function (response) {
                    $scope.pageTotal = response.data;
                    if ($scope.pageTotal > 0) {
                        ComponentsService.find($scope.requestModel)
                            .then(function (response) {
                                $scope.components = response.data;
                                $scope.loading = false;
                            });
                    } else {
                        $scope.loading = false;
                    }
                });

            $scope.createModal = function () {
                //$scope.modalLoading = true;
                openModal();
                if ($scope.modules.length === 0) {
                    ComponentsService.getAllModulesBasic()
                        .then(function (response) {
                            $scope.modules = response.data;
                            //$scope.modalLoading = false;
                        })
                } else {
                    //$scope.modalLoading = false;
                }
            };

            var openModal = function () {
                $scope.createFormModal = $scope.createFormModal || $modal({
                        scope: $scope,
                        templateUrl: 'view/app/components/componentFormModal.html',
                        animation: 'am-fade-and-slide-right',
                        backdrop: 'static',
                        show: false
                    });
                $scope.createFormModal.$promise.then(function () {
                    $scope.createFormModal.show();
                });
            };

            $scope.save = function (componentFormValidation) {
                if (!componentFormValidation.$valid)
                    return;

                $scope.saving = true;

                if ($scope.component.type === 2) {
                    $scope.component.place = 0;
                    $scope.component.order = 0;
                }

                ComponentsService.create($scope.component)
                    .then(function (response) {
                        $scope.saving = false;
                        $scope.createFormModal.hide();
                        $state.go('studio.app.componentDetail', { id: response.data });
                    })
            };

            $scope.delete = function (id) {

            }
        }
    ]);