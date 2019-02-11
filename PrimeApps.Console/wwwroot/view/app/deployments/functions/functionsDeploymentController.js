'use strict';

angular.module('primeapps')

    .controller('FunctionsDeploymentController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'FunctionsDeploymentService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, FunctionsDeploymentService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'functionsDeployment';
            $rootScope.breadcrumblist[2].title = 'Functions Deployment';




            console.log("functionsDeploymentController");

        }
    ]);