'use strict';

angular.module('primeapps')

    .controller('ComponentsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'ComponentsService', 'componentPlaces', 'componentPlaceEnums', 'componentTypes', 'componentTypeEnums',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService, ComponentsService, componentPlaces, componentPlaceEnums, componentTypes, componentTypeEnums) {
            $scope.appId = $state.params.appId;
            $scope.modules = [];
            $scope.$parent.menuTopTitle = "App 1";
            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'components';
            $scope.component = {};
            $scope.components = [];
            $scope.loading = true;
            $scope.componentPlaces = componentPlaces;
            $scope.componentTypes = componentTypes;

            $scope.requestModel = {
                limit: "10",
                offset: 0
            };

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
                $scope.modalLoading = true;
                openModal();
                if ($scope.modules.length === 0) {
                    ComponentsService.getAllModulesBasic()
                        .then(function (response) {
                            $scope.modules = response.data;
                            $scope.modalLoading = false;
                        })
                } else {
                    $scope.modalLoading = false;
                }

            };

            var openModal = function () {
                $scope.createFormModal = $scope.createFormModal || $modal({
                        scope: $scope,
                        templateUrl: 'view/app/components/formComponentModal.html',
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
                        $state.go('app.componentForm', { id: response.data });
                    })
            }

            $scope.delete = function (id) {

            }
        }
    ]);