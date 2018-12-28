'use strict';

angular.module('primeapps')

    .controller('FunctionsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'FunctionsService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService,FunctionsService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.menuTopTitle = "App 1";
            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'functions';

            console.log("BiController");

        }
    ]);