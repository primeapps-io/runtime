'use strict';

angular.module('primeapps')

    .controller('ProfileController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.menuTopTitle = "Settings";

            $scope.$parent.activeMenuItem = 'profile';

            console.log("ProfileController");

        }
    ]);