'use strict';

angular.module('primeapps')

    .controller('ProfilesController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'ProfilesService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService,ProfilesService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.menuTopTitle = "Authorization";
            $scope.$parent.activeMenu = 'authorization';
            $scope.$parent.activeMenuItem = 'profiles';

            console.log("ProfilesController");

        }
    ]);