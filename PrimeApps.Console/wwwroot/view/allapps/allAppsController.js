'use strict';

angular.module('primeapps')

    .controller('AllAppsController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', '$cache', 'activityTypes', 'AllAppsService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', '$aside',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, $cache, activityTypes, AllAppsService, $window, $state, $modal, dragularService, $timeout, $interval, $aside) {
            console.log("Allapps")

            $scope.apps = [];
            $scope.appsFilter = {
                search : null,
                page: null,
                status: 0
            };

            AllAppService.myApps($scope.appsFilter)
                .then(function (response) {
                    if (response.data) {
                        $scope.apps = response.data;
                    }
                });
        }
    ]);