'use strict';

angular.module('primeapps')

    .controller('MenusController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'MenusService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService,MenussService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.menuTopTitle = "Xbrand";
            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'menus';

            console.log("MenusController");

        }
    ]);