'use strict';

angular.module('primeapps')

    .controller('FunctionsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'FunctionsService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService, FunctionsService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'functions';
            $rootScope.breadcrumblist[2].title = 'Functions';

            $scope.appId = $state.params.appId;

            $scope.function = {};
            $scope.functions = [];

            $scope.loading = true;

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

            FunctionsService.count()
                .then(function (response) {
                    $scope.pageTotal = response.data;
                    if ($scope.pageTotal > 0) {
                        FunctionsService.find($scope.requestModel)
                            .then(function (response) {
                                if (response) {
                                    $scope.function = response.data;
                                    $scope.loading = false;
                                } else {
                                    $scope.loading = false;
                                }
                            });
                    } else {
                        $scope.loading = false;
                    }
                });

            $scope.createModal = function () {
                //$scope.modalLoading = true;
                openModal();
                if ($scope.modules.length === 0) {
                    FunctionsService.getAllModulesBasic()
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
                        templateUrl: 'view/app/components/functionFormModal.html',
                        animation: 'am-fade-and-slide-right',
                        backdrop: 'static',
                        show: false
                    });
                $scope.createFormModal.$promise.then(function () {
                    $scope.createFormModal.show();
                });
            };

            $scope.save = function (functionFormValidation) {
                if (!functionFormValidation.$valid)
                    return;

                $scope.saving = true;

                if ($scope.component.type === 2) {
                    $scope.component.place = 0;
                    $scope.component.order = 0;
                }

                FunctionsService.create($scope.component)
                    .then(function (response) {
                        $scope.saving = false;
                        $scope.createFormModal.hide();
                        $state.go('studio.app.functionDetail', { id: response.data });
                    })
            };

            $scope.delete = function (id) {

            }

        }
    ]);