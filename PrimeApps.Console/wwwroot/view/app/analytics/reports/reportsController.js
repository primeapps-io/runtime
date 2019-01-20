'use strict';

angular.module('primeapps')

    .controller('ReportsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'ReportsService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService, ReportsService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.menuTopTitle = "Analytics";
            $scope.$parent.activeMenu = 'analytics';
            $scope.$parent.activeMenuItem = 'reports';
            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }

            };

            $scope.generator(10);

            $rootScope.breadcrumblist[0].link = '#/apps?organizationId=' + $rootScope.currentOrganization.id;
            $rootScope.breadcrumblist[1].link = '#/org/' + $rootScope.currentOrganization.id + '/app/' + $rootScope.appId + '/overview';
            $rootScope.breadcrumblist[2].title = 'Reports';

            $scope.reports = [];
            $scope.loading = true;
            $scope.requestModel = {
                limit: "10",
                offset: 0
            };

            ReportsService.count().then(function (response) {
                $scope.pageTotal = response.data;
            });

            ReportsService.find($scope.requestModel).then(function (response) {
                $scope.reports = response.data;
                $scope.loading = false;
            });

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;
                ReportsService.find(requestModel).then(function (response) {
                    $scope.reports = response.data;
                    $scope.loading = false;
                });

            };

            $scope.changeOffset = function () {
                $scope.changePage(1)
            };


        }
    ]);