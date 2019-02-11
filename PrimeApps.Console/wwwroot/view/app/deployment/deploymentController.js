'use strict';

angular.module('primeapps')

    .controller('DeploymentController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'DeploymentService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, DeploymentService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'deployments';
            $rootScope.breadcrumblist[2].title = 'Deployments';




            console.log("DeploymentController");

        }
    ]);