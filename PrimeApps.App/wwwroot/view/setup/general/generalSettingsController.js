'use strict';

angular.module('primeapps')

    .controller('GeneralSettingsController', ['$rootScope', '$scope', '$filter', 'GeneralSettingsService', '$localStorage', 'helper', '$state', 'mdToast', 'AppService',
        function ($rootScope, $scope, $filter, GeneralSettingsService, $localStorage, helper, $state, mdToast, AppService) {
            $scope.loading = false;

            AppService.checkPermission().then(function (res) {

                if (res && res.data) {
                    var profile = JSON.parse(res.data["profile"]);
                    var customProfilePermissions = undefined;
                    if (res.data["customProfilePermissions"])
                        customProfilePermissions = JSON.parse(res.data["customProfilePermissions"]);

                    if (!profile.HasAdminRights) {
                        var generalIsExist = undefined;
                        if (customProfilePermissions)
                            generalIsExist = customProfilePermissions.permissions.indexOf('general') > -1;

                        if (!generalIsExist) {
                            $state.go('app.setup.sms');
                        }
                    }
                }

                $rootScope.breadcrumblist = [
                    {
                        title: $filter('translate')('Layout.Menu.Dashboard'),
                        link: "#/app/dashboard"
                    },
                    {
                        title: $filter('translate')('Setup.Nav.System'),
                        link: '#/app/setup/sms'
                    },
                    {
                        title: $filter('translate')('Common.General')
                    }
                ];

                $scope.goUrl = function (url) {
                    window.location = url;
                };
                GeneralSettingsService.getByKey('module', 'detail_view_type')
                    .then(function (response) {
                        if (response.data) {
                            $scope.setting = response.data;
                            $scope.detailViewType = $scope.setting.value;
                        }
                    });

                $scope.saveDetailViewType = function () {
                    $scope.loading = true;

                    var success = function () {
                        $rootScope.detailViewType = $scope.detailViewType;
                        $scope.loading = false;
                        mdToast.success($filter('translate')('Setup.Settings.UpdateSuccessGeneralSettings'));
                    };

                    if ($scope.setting) {
                        $scope.setting.value = $scope.detailViewType;

                        GeneralSettingsService.update($scope.setting)
                            .then(function () {
                                success();
                            });
                    } else {
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
            });
        }
    ]);