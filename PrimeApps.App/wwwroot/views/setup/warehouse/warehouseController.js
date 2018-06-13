'use strict';

angular.module('primeapps')

    .controller('WarehouseController', ['$rootScope', '$scope', '$filter', 'ngToast', '$state', 'AnalyticsService',
        function ($rootScope, $scope, $filter, ngToast, $state, AnalyticsService) {
            $scope.loading = true;

            if (!$rootScope.workgroup.hasAnalytics) {
                ngToast.create({ content: $filter('translate')('Common.Forbidden'), className: 'warning' });
                $state.go('app.dashboard');
                $rootScope.app = 'crm';
                return;
            }

            AnalyticsService.getWarehouseInfo()
                .then(function (warehouseInfo) {
                    $scope.warehouseInfo = warehouseInfo.data;
                    $scope.loading = false;
                });

            $scope.changePassword = function () {
                if (!$scope.databasePassword) {
                    $scope.warehouseForm.databasePassword.$setValidity('required', false);
                }

                if (!$scope.warehouseForm.$valid)
                    return;

                $scope.changing = true;

                AnalyticsService.changeWarehousePassword($scope.databasePassword)
                    .then(function () {
                        ngToast.create({ content: $filter('translate')('Setup.Warehouse.PaswordSuccess'), className: 'success' });
                    })
                    .catch(function (data) {
                        if (data.status === 400) {
                            $scope.warehouseForm.databasePassword.$setValidity('password', false);
                        }
                    })
                    .finally(function () {
                        $scope.changing = false;
                    });
            };

            $scope.setValid = function () {
                $scope.warehouseForm.databasePassword.$setValidity('required', true);
                $scope.warehouseForm.databasePassword.$setValidity('password', true);
            }
        }
    ]);