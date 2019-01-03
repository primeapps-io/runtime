'use strict';

angular.module('primeapps')

    .controller('OverviewController', ['$rootScope', '$scope',
        function ($rootScope, $scope) {

            //console.log("asdfasdf")

            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'overview';
            $scope.$parent.tabTitle = 'Overview';
            $rootScope.breadcrumbListe[2] = {title: "Overview"};
            if ($scope.$parent.setTopTitle) {
                $scope.$parent.setTopTitle();

            }


        }
    ]);