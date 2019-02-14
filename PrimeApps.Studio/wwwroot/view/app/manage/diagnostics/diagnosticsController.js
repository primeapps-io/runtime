'use strict';

angular.module('primeapps')

    .controller('DiagnosticsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'DiagnosticsService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, DiagnosticsService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'diagnostics';
            $rootScope.breadcrumblist[2].title = 'Diagnostics';



            console.log("DiagnosticsController");

        }
    ]);