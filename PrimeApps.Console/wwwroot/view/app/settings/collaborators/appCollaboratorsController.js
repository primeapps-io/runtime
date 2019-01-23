'use strict';

angular.module('primeapps')

    .controller('AppCollaboratorsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'AppCollaboratorsService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService,AppCollaboratorsService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.menuTopTitle = "Settings";
            $scope.$parent.activeMenu = 'settings';
            $scope.$parent.activeMenuItem = 'appCollaborators';    
            $rootScope.breadcrumblist[2].title = 'App Collaborators';

            $scope.appCollaborators = [];
            AppCollaboratorsService.getCollaborators($scope.$parent.appId).then(function (response) {
                $scope.appCollaborators = response.data;
                var organization = $filter('filter')($rootScope.organizations, { id: $rootScope.currentOrganization.id }, true)[0];

                for (var i = 0; i < $scope.appCollaborators.length; i++) {
                    var appCollaborator = $scope.appCollaborators[i];
                    if (appCollaborator.user_id) {

                    }

                    if (appCollaborator.team_id) {

                    }
                }
            });
            console.log($scope)

        }
    ]);