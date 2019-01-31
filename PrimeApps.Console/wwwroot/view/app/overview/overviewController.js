'use strict';

angular.module('primeapps')

    .controller('OverviewController', ['$rootScope', '$scope', '$state', 'LayoutService',
        function ($rootScope, $scope, $state, LayoutService) {

            $rootScope.appLoading = true;
            LayoutService.getAppData()
                .then(function (response) {

                    $rootScope.appLoading = false;
                    $scope.$parent.menuTopTitle = $rootScope.currentApp.label;
                });

            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'overview';
            $scope.$parent.tabTitle = 'Overview';

            //$rootScope.breadcrumblist[0].link = '#/apps?organizationId=' + $rootScope.currentOrgId;
            //$rootScope.breadcrumblist[1] = {title:$scope.$parent.menuTopTitle};
            $rootScope.breadcrumblist[2] = {title: 'Overview'};

            $scope.gotoAppDetails = function () {
                $state.go('studio.app.appdetails', {orgId: $rootScope.currentOrgId, appId: $rootScope.currentAppId});
            }
        }
    ]);