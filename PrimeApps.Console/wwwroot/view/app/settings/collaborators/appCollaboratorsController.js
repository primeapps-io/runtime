'use strict';

angular.module('primeapps')

    .controller('AppCollaboratorsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'AppCollaboratorsService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService,AppCollaboratorsService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.menuTopTitle = "Settings";
            $scope.$parent.activeMenu = 'settings';
            $scope.$parent.activeMenuItem = 'appCollaborators';

            console.log("AppCollaboratorsController");

        }
    ]);