'use strict';

angular.module('primeapps')

    .controller('AppsController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', '$cache', 'activityTypes', 'AppsService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', '$location', '$stateParams',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, $cache, activityTypes, AppsService, $window, $state, $modal, dragularService, $timeout, $interval, $location, $stateParams) {

            $scope.loading = true;

            $rootScope.currentOrgId = parseInt($stateParams.orgId);

            if (!$rootScope.currentOrgId && $rootScope.organizations) {
                $state.go('studio.allApps');
            }

            if ($rootScope.organizations)
                $rootScope.currentOrganization = $filter('filter')($rootScope.organizations, { id: parseInt($rootScope.currentOrgId) }, true)[0];


            $rootScope.breadcrumblist[0] = { title: $rootScope.currentOrganization.label };
            $rootScope.breadcrumblist[1] = {};
            $rootScope.breadcrumblist[2] = {};

            if (!$rootScope.currentOrgId) {
                toastr.warning($filter('translate')('Common.NotFound'));
                $state.go('studio.allApps');
                return;
            }

            $scope.appsFilter = {
                organization_id: $rootScope.currentOrgId,
                search: null,
                page: null,
                status: 0
            };

            AppsService.getOrganizationApps($scope.appsFilter)
                .then(function (result) {
                    $scope.apps = result.data;
                    $scope.loading = false;

                });
        }
    ]);