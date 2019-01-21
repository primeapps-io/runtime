'use strict';

angular.module('primeapps')

    .controller('IdentityController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'IdentityService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService,IdentityService, LayoutService, $http, config) {

            $scope.$parent.menuTopTitle = "Identity";
            $scope.$parent.activeMenu = 'identity';
            $scope.$parent.activeMenuItem = 'identity';
            $rootScope.breadcrumblist[2].title = 'Identity';

            console.log("IdentityController");

        }
    ]);