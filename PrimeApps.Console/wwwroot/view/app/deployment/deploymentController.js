'use strict';

angular.module('primeapps')

    .controller('DeploymentController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'DeploymentService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService, DeploymentService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.menuTopTitle = "Xbrand";
            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'deployment';

            console.log("DeploymentController");

        }
    ]);