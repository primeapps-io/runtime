'use strict';

angular.module('primeapps')

    .controller('AllAppsController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', '$cache', 'activityTypes', 'AllAppsService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', '$aside',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, $cache, activityTypes, AllAppsService, $window, $state, $modal, dragularService, $timeout, $interval, $aside) {

            $rootScope.breadcrumblist[0] = { title: 'All Apps' };
            $rootScope.breadcrumblist[1] = {};
            $rootScope.breadcrumblist[2] = {};

            $scope.loading = true;

            $scope.apps = [];
            $scope.appsFilter = {
                search: null,
                page: null,
                status: 0
            };

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
                    page: null,
                    status: status || 0
                };

                AllAppsService.myApps($scope.appsFilter)
                    .then(function (response) {
                        $scope.apps = response.data;
                    });
            };
        }
    ]);