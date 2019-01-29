'use strict';

angular.module('primeapps')

    .controller('NetworkAccessController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'NetworkAccessService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService,NetworkAccessService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.menuTopTitle = "Security";
            //$scope.$parent.activeMenu = 'security';
            $scope.$parent.activeMenuItem = 'networkAccess';
            $rootScope.breadcrumblist[2].title = 'Network Access';

            console.log("NetworkAccessController");

        }
    ]);