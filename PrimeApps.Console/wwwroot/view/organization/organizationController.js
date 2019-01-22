'use strict';

angular.module('primeapps')

    .controller('OrganizationController', ['$rootScope', '$scope', '$filter', '$location', 'helper', '$http', 'config',
        function ($rootScope, $scope, $filter, $location, helper, $http, config) {

            // $scope.menuTopTitle ="XBrand CRM";
            // $scope.activeMenu= 'organization';
            // $scope.activeMenuItem = 'organization';
            // $scope.tabTitle='organization';


            $http.get(config.apiUrl + 'team/count/' + $rootScope.currenOrgId).then(function (response) {
                $scope.$parent.teamCount = response.data;
            });
        }
    ]);