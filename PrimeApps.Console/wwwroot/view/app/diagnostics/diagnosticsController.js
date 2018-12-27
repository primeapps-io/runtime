'use strict';

angular.module('primeapps')

    .controller('DiagnosticsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'DiagnosticsService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService, DiagnosticsService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.menuTopTitle = "Xbrand";
            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'diagnostics';

            console.log("DiagnosticsController");

        }
    ]);