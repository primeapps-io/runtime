'use strict';

angular.module('primeapps')

    .controller('OrgSettingsController', ['$rootScope', '$scope', '$filter', '$location', 'helper', 'OrgSettingsService', 'ModuleService', '$state',
        function ($rootScope, $scope, $filter, $location, helper, OrgSettingsService, ModuleService, $state) {
            $scope.pageLoading = true;

            if ($rootScope.currentOrganization && $rootScope.currentOrganization.role != 'administrator') {
                toastr.warning($filter('translate')('Common.Forbidden'));
                $state.go('studio.allApps');
                return;
            }

            $scope.colors = [
                { value: '#D72A20' },
                { value: '#833CA3' },
                { value: '#17ACFE' },
                { value: '#33ffff' },
                { value: '#229C51' },
                { value: '#FFAD1C' },
                { value: '#1C3E7D' },
                { value: '#C35E21' },
                { value: '#F3C937' },
                { value: '#6B2F5D' },
            ];

            $scope.orgModel = {};
            $scope.icons = ModuleService.getIcons(2);

            $rootScope.breadcrumblist[1].link = "";
            $rootScope.breadcrumblist[1].title = 'Manage';
            $rootScope.breadcrumblist[2] = {};
            $rootScope.breadcrumblist[3] = {};

            $scope.orgDeleteDisabled = false;

            OrgSettingsService.get($scope.$parent.$parent.$parent.currentOrgId).then(function (response) {
                var data = response.data;
                $scope.orgModel.icon = data.icon || 'fas fa-building';
                $scope.orgModel.label = data.label;
                $scope.orgModel.name = data.name;
                $scope.orgModel.id = data.id;
                $scope.orgModel.color = data.color || '#9F5590';
                $scope.pageLoading = false;
            });

            var getMyOrganizations = function () {
                OrgSettingsService.myOrganizations()
                    .then(function (response) {
                        if (response.data) {
                            $rootScope.organizations = response.data;
                            $state.go('studio.allApps');
                            //$scope.menuOpen[$scope.organizations[0].id] = true;
                        }
                    });
            };

            $scope.changeIcon = function () {
                $scope.orgModel.icon = $scope.orgModel.icon.value;
            };

            $scope.deleteButtonControl = function () {
                var currentOrg = $filter('filter')($rootScope.organizations, { id: $scope.$parent.$parent.$parent.currentOrgId }, true)[0];
                if (currentOrg.role != 'administrator' || currentOrg.default === true)
                    $scope.orgDeleteDisabled = true;
            };
            $scope.deleteButtonControl();

            $scope.requiredField = function () {
                if ($scope.orgModel.label)
                    $scope.requiredLabel = null;
                else
                    $scope.requiredLabel = "background-color: rgba(206, 4, 4, 0.15) !important";
            };

            $scope.save = function (appDetails) {
                if (!appDetails.$valid) {
                    $scope.requiredLabel = "background-color: rgba(206, 4, 4, 0.15) !important";
                    return;
                }
                else {
                    $scope.requiredLabel = null;
                }

                $scope.saving = true;
                if (angular.isObject($scope.orgModel.icon))
                    $scope.orgModel.icon = $scope.orgModel.icon.value;

                OrgSettingsService.update($scope.$parent.$parent.$parent.currentOrgId, $scope.orgModel)
                    .then(function (response) {
                        $rootScope.currentOrganization.label = $scope.orgModel.label;
                        $rootScope.currentOrganization.icon = $scope.orgModel.icon;
                        $rootScope.currentOrganization.color = $scope.orgModel.color;
                        toastr.success($filter('translate')('Organization is updated successfully.'));
                        $scope.saving = false;
                    });
            };

            $scope.delete = function (orgId) {
                var willDelete =
                    swal({
                        title: "Are you sure?",
                        text: " ",
                        icon: "warning",
                        buttons: ['Cancel', 'Yes'],
                        dangerMode: true
                    }).then(function (value) {
                        if (value) {
                            OrgSettingsService.delete(orgId)
                                .then(function () {
                                    toastr.success("Organization is deleted successfully.", "Deleted!");
                                    getMyOrganizations();

                                });
                        }
                    });
            };
        }
    ]);