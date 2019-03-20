'use strict';

angular.module('primeapps')

    .controller('OrganizationController', ['$rootScope', '$scope', '$filter', '$location', 'helper', 'OrganizationService',
        function ($rootScope, $scope, $filter, $location, helper, OrganizationService) {

            // $scope.menuTopTitle ="XBrand CRM";
            // $scope.activeMenu= 'organization';
            // $scope.activeMenuItem = 'organization';
            // $scope.tabTitle='organization';

            OrganizationService.teamCount()
                .then(function (response) {
                    $scope.$parent.teamCount = response.data;
                });

            OrganizationService.collaboratorCount($rootScope.currentOrgId)
                .then(function (response) {
                    $scope.$parent.collaboratorCount = response.data;
                });
        }
    ]);