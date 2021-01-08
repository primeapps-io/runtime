'use strict';

angular.module('primeapps')

    .controller('OutlookController', ['$rootScope', '$scope', '$filter', 'OutlookService', 'AppService', 'mdToast',
        function ($rootScope, $scope, $filter, OutlookService, AppService, mdToast) {
            $scope.modulesHasEmail = [];
            $scope.hasAdminRight = $filter('filter')($rootScope.profiles, { id: $rootScope.user.profile.id }, true)[0].has_admin_rights;
            for (var i = 0; i < $rootScope.modules.length; i++) {
                var module = $rootScope.modules[i];
                var emailFields = $filter('filter')(module.fields, { data_type: 'email', deleted: '!true', display_detail: true });

                if (emailFields.length > 0)
                    $scope.modulesHasEmail.push(angular.copy(module));
            }

            var getSettings = function () {
                OutlookService.getSettings()
                    .then(function (response) {
                        var settings = response.data;

                        if (settings) {
                            $scope.outlookSetting = {};
                            var settingsModule = $filter('filter')(settings, { key: 'outlook_module' }, true)[0];
                            var settingsEmailField = $filter('filter')(settings, { key: 'outlook_email_field' }, true)[0];

                            if (settingsModule)
                                $scope.outlookSetting.module = $filter('filter')($scope.modulesHasEmail, { name: settingsModule.value }, true)[0];

                            if (settingsEmailField)
                                $scope.outlookSetting.emailField = $filter('filter')($scope.outlookSetting.module.fields, { name: settingsEmailField.value }, true)[0];
                        }
                    });
            };

            getSettings();

            $scope.save = function () {
                if (!$scope.outlookSettingsForm.$valid)
                    return;

                $scope.saving = true;

                var saveSettings = function () {
                    var settings = {};
                    settings.module = $scope.outlookSetting.module.name;
                    settings.email_field = $scope.outlookSetting.emailField.name;

                    OutlookService.saveSettings(settings)
                        .then(function () {
                            AppService.getMyAccount(true);
                            mdToast.success($filter('translate')('Setup.Settings.UpdateSuccessGeneralSettings'));
                            $scope.saving = false;
                        });
                };

                var mailModule = $filter('filter')($rootScope.modules, { name: 'mails' }, true)[0];

                if (!mailModule) {
                    OutlookService.createMailModule()
                        .then(function () {
                            saveSettings();
                        });
                }
                else {
                    saveSettings();
                }
            }
        }
    ]);