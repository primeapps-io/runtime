'use strict';

angular.module('primeapps')

    .controller('AppsController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', '$cache', 'activityTypes', 'AppsService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', '$location', '$stateParams',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, $cache, activityTypes, AppsService, $window, $state, $modal, dragularService, $timeout, $interval, $location, $stateParams) {

            $scope.loading = true;
            var orgId = $stateParams.orgId;

            if (orgId == 'orgId') {
                $state.go('studio.apps', {orgId: $rootScope.currentOrgId});
                return true;
            }

            $rootScope.currentOrgId = parseInt(orgId);
            $rootScope.menuOpen[orgId] = 'open';
            if (!$rootScope.currentOrgId && $rootScope.organizations) {
                var defaultOrg = $filter('filter')($rootScope.organizations, { default: true }, true)[0];
                window.location.href = '/#/apps?orgId=' + defaultOrg.id;
            }

            if ($rootScope.organizations)
                $rootScope.currentOrganization = $filter('filter')($rootScope.organizations, {id: parseInt($rootScope.currentOrgId)}, true)[0];


            $rootScope.breadcrumblist[0] = {title: $rootScope.currentOrganization.label};
            $rootScope.breadcrumblist[1] = {};
            $rootScope.breadcrumblist[2] = {};

            if (!$rootScope.currentOrgId) {
                toastr.warning($filter('translate')('Common.NotFound'));
                var defaultOrg = $filter('filter')($rootScope.organizations, { default: true }, true)[0];
                window.location.href = '/#/apps?orgId=' + defaultOrg.id;
                return;
            }

            $scope.appsFilter = {
                organization_id: $rootScope.currentOrgId,
                search: null,
                page: null,
                status: 0
            };
            
            
            $scope.openEditModal = function (app) {
                 $scope.appModel =app;
               
                $scope.appFormModal = $scope.appFormModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/organization/appform/newAppForm.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });
                
                $scope.appFormModal.$promise.then(function () {
                    $scope.appFormModal.show();
                });
                
            };
            
            
            AppsService.getOrganizationApps($scope.appsFilter)
                .then(function (result) {
                    $scope.apps = result.data;
                    $scope.loading = false;

                });
        }
    ]);