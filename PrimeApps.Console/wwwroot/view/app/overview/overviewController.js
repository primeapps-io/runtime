'use strict';

angular.module('primeapps')

    .controller('OverviewController', ['$rootScope', '$scope',
        function ($rootScope, $scope) {

            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'overview';
            $scope.$parent.tabTitle = 'Overview';


           
            $scope.$parent.menuTopTitle = $rootScope.currentApp.label;

            //$rootScope.breadcrumblist[0].link = '#/apps?organizationId=' + $rootScope.currentOrgId;
            //$rootScope.breadcrumblist[1] = {title:$scope.$parent.menuTopTitle};
            $rootScope.breadcrumblist[2] = {title: 'Overview'};

        }
    ]);