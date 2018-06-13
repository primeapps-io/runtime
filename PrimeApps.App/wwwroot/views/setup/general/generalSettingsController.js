'use strict';

angular.module('ofisim')

    .controller('GeneralSettingsController', ['$rootScope', '$scope', '$filter', 'ngToast', 'GeneralSettingsService', '$localStorage',
        function ($rootScope, $scope, $filter, ngToast, GeneralSettingsService, $localStorage) {

            $scope.hasAdminRight = $filter('filter')($rootScope.profiles, { id: $rootScope.user.profile.id }, true)[0].has_admin_rights;
            GeneralSettingsService.getByKey('module', 'detail_view_type')
                .then(function (response) {
                    if (response.data) {
                        $scope.setting = response.data;
                        $scope.detailViewType = $scope.setting.value;
                    }
                });

            $scope.saveDetailViewType = function () {
                $scope.savingDetailViewType = true;

                var success = function () {
                    $rootScope.detailViewType = $scope.detailViewType;
                    $scope.savingDetailViewType = false;
                    ngToast.create({ content: $filter('translate')('Setup.Settings.UpdateSuccessGeneralSettings'), className: 'success' });
                };

                if ($scope.setting) {
                    $scope.setting.value = $scope.detailViewType;

                    GeneralSettingsService.update($scope.setting)
                        .then(function () {
                            success();
                        });
                }
                else {
                    $scope.setting = {
                        type: 'module',
                        key: 'detail_view_type',
                        value: $scope.detailViewType
                    };

                    GeneralSettingsService.create($scope.setting)
                        .then(function () {
                            success();
                        });
                }
            }
        }
    ]);