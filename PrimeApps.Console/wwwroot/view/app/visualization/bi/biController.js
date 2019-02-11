'use strict';

angular.module('primeapps')

    .controller('BiController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'BiService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, BiService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            //$scope.$parent.menuTopTitle = "Analytics";
            //$scope.$parent.activeMenu = 'analytics';
            $scope.$parent.activeMenuItem = 'bi';
            $rootScope.breadcrumblist[2].title = 'Bi';

            console.log("BiController");

        }
    ]);