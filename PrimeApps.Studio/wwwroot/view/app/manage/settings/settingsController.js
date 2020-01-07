'use strict';

angular.module('primeapps')

    .controller('SettingsController', ['$rootScope', '$scope', '$state', 'SettingsService', '$location', '$controller', '$filter', 'AppFormService',
        function ($rootScope, $scope, $state, SettingsService, $location, $controller, $filter, AppFormService) {

            $scope.publishModelContainer = {};
            $scope.checkedModule = {};
            $scope.wizardStep = 0;
            $scope.publishModel = {};
            $scope.publishInfoReady = false;
            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'settings';
            $rootScope.breadcrumblist[2].title = 'Settings';

            $scope.app = angular.copy($rootScope.currentApp);
            $scope.app.setting.options = JSON.parse($scope.app.setting.options);

            $scope.save = function () {
                $scope.loading = true;
                var appModel = {};
                appModel.setting = {};

                appModel.name = $scope.app.name;
                appModel.id = $scope.app.id;
                appModel.label = $scope.app.label;
                appModel.description = $scope.app.description;
                appModel.logo = $scope.app.logo;
                appModel.icon = $scope.app.icon;
                appModel.color = $scope.app.color;
                appModel.enable_registration = $scope.app.setting.options.enable_registration;
                appModel.enable_api_registration = $scope.app.setting.options.enable_api_registration;
                appModel.clear_all_records = $scope.app.setting.options.clear_all_records;

                appModel.app_domain = $scope.app.setting.app_domain;
                appModel.auth_domain = $scope.app.setting.auth_domain;

                AppFormService.update($rootScope.currentApp.id, appModel)
                    .then(function (response) {
                        $scope.loading = false;
                        toastr.success("App settings are updated successfully.");

                        $rootScope.currentApp.name = $scope.app.name;
                        $rootScope.currentApp.label = $scope.app.label;
                        $rootScope.currentApp.description = $scope.app.description;
                        $rootScope.currentApp.logo = $scope.app.logo;
                        $rootScope.currentApp.icon = $scope.app.icon;
                        $rootScope.currentApp.color = $scope.app.color;
                        $rootScope.currentApp.setting.options = response.data.setting.options;
                    })
                    .catch(function () {
                        toastr.error($filter('translate')('Common.Error'));
                        $scope.loading = false;
                    });
            }
        }
    ]);