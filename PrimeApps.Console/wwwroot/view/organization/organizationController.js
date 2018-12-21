'use strict';

angular.module('primeapps')

    .controller('OrganizationController', ['$rootScope', '$scope', '$filter', '$location', 'helper',
        function ($rootScope, $scope, $filter, $location, helper) {

            $scope.menuTopTitle ="XBrand CRM";
            $scope.activeMenu= 'app';
            $scope.activeMenuItem = 'overview';
            $scope.tabTitle='Overview';


        }
    ]);