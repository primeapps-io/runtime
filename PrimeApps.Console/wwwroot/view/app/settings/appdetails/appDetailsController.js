'use strict';

angular.module('primeapps')

    .controller('AppDetailsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'AppDetailsService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService,AppDetailsService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.menuTopTitle = "Settings";
            $scope.$parent.activeMenu = 'settings';
            $scope.$parent.activeMenuItem = 'appDetails';

            console.log("AppDetailsController");

        }
    ]);