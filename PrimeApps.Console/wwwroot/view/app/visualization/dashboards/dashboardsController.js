'use strict';

angular.module('primeapps')

    .controller('DashboardsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'DashboardsService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, DashboardsService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            //$scope.$parent.menuTopTitle = "Analytics";
            // $scope.$parent.activeMenu = 'analytics';
            $scope.$parent.activeMenuItem = 'dashboards';
            $rootScope.breadcrumblist[2].title = 'Dashboards';

            console.log("DashboardsController");

        }
    ]);