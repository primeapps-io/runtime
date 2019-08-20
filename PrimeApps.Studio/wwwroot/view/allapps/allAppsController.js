'use strict';

angular.module('primeapps')

    .controller('AllAppsController', ['$rootScope', '$scope', '$state', '$filter', 'AllAppsService',
        function ($rootScope, $scope, $state, $filter, AllAppsService) {

            $rootScope.breadcrumblist[0] = {title: 'All Apps'};
            $rootScope.breadcrumblist[1] = {};
            $rootScope.breadcrumblist[2] = {};
            $rootScope.menuOpen = [];
            $scope.loading = true;

            $scope.apps = [];
            $scope.appsFilter = null;
            $rootScope.defaultorganization = $filter('filter')($rootScope.organizations, {default: true}, true)[0];
            AllAppsService.myApps($scope.appsFilter)
                .then(function (response) {
                    if (response.data) {
                        $scope.apps = response.data;
                        $scope.loading = false;
                    }
                });

            $scope.appFilter = function (search, status) {
                $scope.appsFilter = {
                    search: search || null,
                    page: null
                };

                AllAppsService.myApps($scope.appsFilter)
                    .then(function (response) {
                        $scope.apps = response.data;
                    });
            };

            $scope.gotoAppForm = function () {
                $state.go('studio.appsForm', {orgId: $filter('filter')($rootScope.organizations, {default: true}, true)[0].id});
            }
        }
    ]);