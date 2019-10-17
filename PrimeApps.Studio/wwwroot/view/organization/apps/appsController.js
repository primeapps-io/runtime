'use strict';

angular.module('primeapps')

    .controller('AppsController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', '$cache', 'activityTypes', 'AppsService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', '$location', '$stateParams', 'LayoutService', 'AppFormService',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, $cache, activityTypes, AppsService, $window, $state, $modal, dragularService, $timeout, $interval, $location, $stateParams, LayoutService, AppFormService) {

            $scope.loading = true;
            var orgId = $stateParams.orgId;

            if (orgId == 'orgId') {
                $state.go('studio.apps', {orgId: $rootScope.currentOrgId});
                return true;
            }
            $scope.icons = LayoutService.getIcons(2);

            $rootScope.currentOrgId = parseInt(orgId);
            $rootScope.menuOpen[orgId] = 'open';
            if (!$rootScope.currentOrgId && $rootScope.organizations) {
                var defaultOrg = $filter('filter')($rootScope.organizations, {default: true}, true)[0];
                window.location.href = '/#/apps?orgId=' + defaultOrg.id;
            }

            if ($rootScope.organizations)
                $rootScope.currentOrganization = $filter('filter')($rootScope.organizations, {id: parseInt($rootScope.currentOrgId)}, true)[0];

            if (!$rootScope.runningPackages)
                $rootScope.runningPackages = {};
            $rootScope.breadcrumblist[0] = {title: $rootScope.currentOrganization.label};
            $rootScope.breadcrumblist[1] = {};
            $rootScope.breadcrumblist[2] = {};

            if (!$rootScope.currentOrgId) {
                toastr.warning($filter('translate')('Common.NotFound'));
                var defaultOrg = $filter('filter')($rootScope.organizations, {default: true}, true)[0];
                window.location.href = '/#/apps?orgId=' + defaultOrg.id;
                return;
            }

            $scope.appsFilter = {
                organization_id: $rootScope.currentOrgId,
                search: null,
                page: null
            };

            $scope.openEditModal = function (app) {
                $scope.currentApp = app;
                $scope.appModel = angular.copy($scope.currentApp);

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

            $scope.save = function (newAppForm) {
                if (!newAppForm.$valid) {
                    toastr.error($filter('translate')('Module.RequiredError'));
                    return false;
                }
                $scope.appSaving = true;
                if ($scope.appModel.icon && $scope.appModel.icon.value) {
                    $scope.appModel.icon = $scope.appModel.icon.value;
                }

                AppFormService.update($scope.currentApp.id, $scope.appModel)
                    .then(function (response) {
                        $scope.appSaving = false;
                        $scope.currentApp.icon = $scope.appModel.icon;
                        $scope.currentApp.color = $scope.appModel.color;
                        //$scope.currentApp.name = $scope.appModel.name;
                        $scope.currentApp.label = $scope.appModel.label;
                        $scope.currentApp.description = $scope.appModel.description;
                        $scope.closeModal();
                    });
            };

            $scope.closeModal = function () {
                $scope.appFormModal.hide();
            };

            AppsService.getOrganizationApps($scope.appsFilter)
                .then(function (result) {
                    $scope.apps = result.data;
                    $scope.loading = false;

                });
        }
    ]);