'use strict';

angular.module('primeapps')

    .controller('OverviewController', ['$rootScope', '$scope',
        function ($rootScope, $scope) {

            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'overview';
            $scope.$parent.tabTitle = 'Overview';

            if ($scope.$parent.setTopTitle) {
                $scope.$parent.setTopTitle();
            }

            //$rootScope.breadcrumblist[0].link = '#/apps?organizationId=' + $rootScope.currentOrganization.id;
            //$rootScope.breadcrumblist[1] = {title:$scope.$parent.menuTopTitle};
            $rootScope.breadcrumblist[2] = { title:'Overview' };

        }
    ]);