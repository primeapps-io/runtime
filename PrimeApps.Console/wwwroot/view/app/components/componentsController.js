'use strict';

angular.module('primeapps')

    .controller('ComponentsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'ComponentsService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService,ComponentsService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.menuTopTitle = "App 1";
            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'components';

            console.log("ComponentsController");

        }
    ]);