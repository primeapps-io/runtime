'use strict';

angular.module('primeapps')

    .controller('ComponentFormController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'ComponentsService', 'componentPlaces', 'componentPlaceEnums', 'componentTypes', 'componentTypeEnums', '$localStorage',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService, ComponentsService, componentPlaces, componentPlaceEnums, componentTypes, componentTypeEnums, $localStorage) {
            $scope.modules = [];
            $scope.id = $state.params.id;

            $scope.$parent.menuTopTitle = "App 1";
            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'components';
            $scope.componentPlaces = componentPlaces;
            $scope.componentTypes = componentTypes;
            $scope.componentForm = {};
            $scope.loading = true;
            //var currentOrganization = $localStorage.get("currentApp");
            $scope.organization = $filter('filter')($rootScope.organizations, { id: 1 }, true)[0];
            /*$scope.aceOption = {
             mode: 'javascript',
             theme: 'tomorrow_night',
             onLoad: function (_ace) {
             // HACK to have the ace instance in the scope...
             $scope.modeChanged = function () {
             _ace.getSession().setMode("ace/mode/javascript");
             };
             }
             };*/

            if (!$scope.id) {
                $state.go('app.components');
            }

            ComponentsService.get($scope.id)
                .then(function (response) {
                    if (!response.data) {
                        ngToast.create({ content: 'Component Not Found !', className: 'danger' });
                        $state.go('app.components');
                    }

                    $scope.component = response.data;
                    $scope.component.place = componentPlaceEnums[$scope.component.place];
                    $scope.component.type = componentTypeEnums[$scope.component.type];
                    $scope.loading = false;
                });


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

            $scope.edit = function () {
                $scope.modalLoading = true;
                $scope.editing = true;

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

            $scope.save = function (componentFormValidation) {
                if (!componentFormValidation.$valid)
                    return;

                $scope.saving = true;

                if ($scope.componentForm.type === 2) {
                    $scope.componentForm.place = 0;
                    $scope.componentForm.order = 0;
                }

                ComponentsService.create($scope.componentForm)
                    .then(function (response) {
                        $scope.saving = false;
                        $scope.createFormModal.hide();
                        $scope.editing = false;
                    })
            }
        }
    ]);