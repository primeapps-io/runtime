'use strict';

angular.module('primeapps')

    .controller('AppsController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', '$cache', 'activityTypes', 'AppsService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', '$location', 'ngToast', '$cookies',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, $cache, activityTypes, AppsService, $window, $state, $modal, dragularService, $timeout, $interval, $location, ngToast, $cookies) {

            var organizationId = $location.search().organizationId;

            if (!organizationId) {
                ngToast.create({content: $filter('translate')('Common.NotFound'), className: 'warning'});
                $state.go('app.allApps');
                return;
            }

            $cookies.put('organization_id', organizationId);

            $rootScope.breadcrumbListe[0] =
                {
                    title: 'First Organization',
                    link: '#/allApps'
                }
            ;

            AppsService.getOrganizationApps($state.params.organizationId)
                .then(function (result) {
                    $scope.apps = result.data;
                });
        }
    ]);