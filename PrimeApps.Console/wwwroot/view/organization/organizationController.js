'use strict';

angular.module('primeapps')

    .controller('OrganizationController', ['$rootScope', '$scope', '$filter', '$location', 'helper', '$http', 'config',
        function ($rootScope, $scope, $filter, $location, helper, $http, config) {

            // $scope.menuTopTitle ="XBrand CRM";
            // $scope.activeMenu= 'organization';
            // $scope.activeMenuItem = 'organization';
            // $scope.tabTitle='organization';

            var organitzationId = $rootScope.currentOrganization ? $rootScope.currentOrganization.id : 1;

            $http.get(config.apiUrl + 'team/count/' + organitzationId).then(function (response) {
                $scope.$parent.teamCount = response.data;
            });
        }
    ]);