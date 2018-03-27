'use strict';

angular.module('ofisim')

    .controller('OutlookController', ['$rootScope', '$scope', '$filter', 'ngToast', 'OutlookService', 'AppService',
        function ($rootScope, $scope, $filter, ngToast, OutlookService, AppService) {
            $scope.modulesHasEmail = [];
            $scope.hasAdminRight = $filter('filter')($rootScope.profiles, { Id: $rootScope.user.profile.ID }, true)[0].HasAdminRights;
            for (var i = 0; i < $rootScope.modules.length; i++) {
                var module = $rootScope.modules[i];
                var emailFields = $filter('filter')(module.fields, { data_type: 'email', deleted: '!true', display_form: true });

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
                            ngToast.create({ content: $filter('translate')('Setup.Settings.UpdateSuccessGeneralSettings'), className: 'success' });
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