'use strict';

angular.module('primeapps')

    .controller('AppsController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', '$cache', 'activityTypes', 'AppsService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', '$location', 'ngToast', '$cookies',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, $cache, activityTypes, AppsService, $window, $state, $modal, dragularService, $timeout, $interval, $location, ngToast, $cookies) {
            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }

            };

            $scope.generator(3);
            $scope.loading = true;
            var organizationId = parseInt($location.search().organizationId);
            $rootScope.currenAppId = null;
            if (angular.isObject($rootScope.currentOrganization)) {
                $rootScope.currentOrganization.id = organizationId;
            } else {
                $rootScope.currentOrganization = {};
                $rootScope.currentOrganization.id = organizationId;
            }

         
            

            $rootScope.breadcrumblist[0] = {title: $rootScope.currentOrganization.name};
            $rootScope.breadcrumblist[1] = {};
            $rootScope.breadcrumblist[2] = {};

            if (!organizationId) {
                ngToast.create({content: $filter('translate')('Common.NotFound'), className: 'warning'});
                $state.go('app.allApps');
                return;
            }

            $cookies.put('organization_id', organizationId);


            $scope.appsFilter = {
                organization_id: $state.params.organizationId,
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