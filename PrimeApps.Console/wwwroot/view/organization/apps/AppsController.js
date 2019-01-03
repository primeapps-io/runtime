'use strict';

angular.module('primeapps')

    .controller('AppsController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', '$cache', 'activityTypes', 'AppsService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', '$location', 'ngToast', '$cookies',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, $cache, activityTypes, AppsService, $window, $state, $modal, dragularService, $timeout, $interval, $location, ngToast, $cookies) {

            var organizationId = $location.search().organizationId;

            if (!organizationId) {
                ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
                $state.go('app.allApps');
                return;
            }

            $cookies.put('organization_id', organizationId);

            $rootScope.breadcrumbListe = [
                {
                    title: 'First Organization',
                    link: '#/allApps'
                }
            ];

            $scope.appsFilter = {
                organization_id: $state.params.organizationId,
                search: null,
                page: null,
                status: 0
            };

            AppsService.getOrganizationApps($scope.appsFilter)
                .then(function (result) {
                    $scope.apps = result.data;
                });
        }
    ]);