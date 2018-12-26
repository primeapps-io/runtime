'use strict';

angular.module('primeapps')

    .controller('RolesController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'RolesService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService,RolesService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.menuTopTitle = "Authorization";
            $scope.$parent.activeMenu = 'authorization';
            $scope.$parent.activeMenuItem = 'roles';

            console.log("RolesController");

        }
    ]);