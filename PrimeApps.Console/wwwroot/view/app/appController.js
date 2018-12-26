'use strict';

angular.module('primeapps')

    .controller('AppController', ['$rootScope', '$scope', '$filter', '$location', 'helper',
        function ($rootScope, $scope, $filter, $location, helper) {

            $scope.menuTopTitle = "XBrand CRM";
            $scope.activeMenu = 'app';
            $scope.activeMenuItem = 'overview';
            $scope.tabTitle = 'Overview';
            $rootScope.breadcrumbListe = [
                {
                    title: 'First Organization',
                    link: "asdasd"
                },
                {
                    title: 'XBrand CRM'
                }
            ];

        }
    ]);