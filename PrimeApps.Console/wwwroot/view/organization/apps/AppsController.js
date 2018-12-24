'use strict';

angular.module('primeapps')

    .controller('AppsController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', '$cache', 'activityTypes', 'AppsService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, $cache, activityTypes, AppsService, $window, $state, $modal, dragularService, $timeout, $interval) {

            $rootScope.breadcrumbListe = [
                {
                    title: 'First Organization',
                    link: '#/allApps'
                }
            ];
            AppsService.getOrganizationApps($state.params.organizationId).then(function (result) {
                $scope.apps = result.data;
            });
        }
    ]);