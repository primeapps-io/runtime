'use strict';

angular.module('primeapps')

    .controller('ReportsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'ReportsService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService, ReportsService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.menuTopTitle = "Analytics";
            $scope.$parent.activeMenu = 'analytics';
            $scope.$parent.activeMenuItem = 'reports';

            console.log("ReportsController");

        }
    ]);