'use strict';

angular.module('primeapps')

    .controller('EmailController', ['$rootScope', '$scope', '$filter', 'EmailService', '$mdDialog', 'mdToast', '$stateParams', 'helper', '$state', 'AppService',
        function ($rootScope, $scope, $filter, EmailService, $mdDialog, mdToast, $stateParams, helper, $state, AppService) {

            AppService.checkPermission().then(function (res) {

                if (res && res.data) {
                    var profile = JSON.parse(res.data["profile"]);
                    var customProfilePermissions = undefined;
                    if (res.data["customProfilePermissions"])
                        customProfilePermissions = JSON.parse(res.data["customProfilePermissions"]);

                    if (!profile.HasAdminRights) {
                        var emailIsExist = undefined;
                        if (customProfilePermissions)
                            emailIsExist = customProfilePermissions.permissions.indexOf('email') > -1;

                        if (!emailIsExist) {
                            mdToast.error($filter('translate')('Common.Forbidden'));
                            $state.go('app.dashboard');
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
                        title: $filter('translate')('Setup.Messaging.EMail.Title')
                    }
                ];

                $scope.emailModel = angular.copy($rootScope.system.messaging.SystemEMail) || {};
                $scope.newSender = {};
                $scope.loading = false;

                $scope.goUrl = function (url) {
                    window.location = url;
                };

                if ($scope.emailModel == null || !$scope.emailModel.hasOwnProperty("provider")) {
                    $scope.emailModel = {
                        provider: "",
                        user_name: "",
                        password: "",
                        senders: [],
                        enable_ssl: false,
                        dont_send_bulk_email_result: false
                    };
                }


                $scope.showNewSenderForm = function () {

                    $scope.alias = null;
                    $scope.email = null;

                    var parentEl = angular.element(document.body);
                    $mdDialog.show({
                        parent: parentEl,
                        templateUrl: 'view/setup/email/senderAdd.html',
                        clickOutsideToClose: true,
                        scope: $scope,
                        preserveScope: true
                    });

                };

                $scope.addNewSender = function (alias, email) {
                    if (!this.senderForm.alias.$valid || !this.senderForm.email.$valid) {
                        mdToast.error($filter('translate')('Module.RequiredError'));
                        return;
                    }

                    if ($scope.emailModel.senders == null) {
                        $scope.emailModel.senders = [];
                    }

                    $scope.emailModel.senders.push({
                        "alias": alias,
                        "email": email
                    });

                    $scope.close();
                    $scope.systemForm.$setValidity("noSender", true);
                };

                $scope.removeSender = function (sender) {
                    if ($scope.emailModel.senders == null) return;
                    var index = $scope.emailModel.senders.indexOf(sender);
                    $scope.emailModel.senders.splice(index, 1);
                };

                $scope.editEMail = function () {
                    if ($scope.emailModel.senders.length == 0) {
                        $scope.systemForm.$setValidity("noSender", false);
                    }

                    if ($scope.systemForm.$valid) {
                        $scope.loading = true;

                        if ($scope.emailModel.host.indexOf("yandex") > -1) {
                            $scope.emailModel.host = "smtp.yandex.ru";
                            $scope.emailModel.port = "587";
                            $scope.emailModel.enable_ssl = true;
                        } else
                            $scope.emailModel.provider = 'smtp';

                        EmailService.updateEMailSettings($scope.emailModel).then(function () {
                            mdToast.success($filter('translate')('Setup.Settings.UpdateSuccess'));

                            EmailService.getSetting().then(function (response) {
                                var messaging = response.data;

                                if (messaging.SystemEMail)
                                    messaging.SystemEMail.enable_ssl = messaging.SystemEMail.enable_ssl === 'True';

                                if (messaging.SystemEMail && messaging.SystemEMail.dont_send_bulk_email_result)
                                    messaging.SystemEMail.dont_send_bulk_email_result = messaging.SystemEMail.dont_send_bulk_email_result === 'True';

                                if (messaging.PersonalEMail)
                                    messaging.PersonalEMail.enable_ssl = messaging.PersonalEMail.enable_ssl === 'True';

                                if (messaging.PersonalEMail && messaging.PersonalEMail.dont_send_bulk_email_result)
                                    messaging.PersonalEMail.dont_send_bulk_email_result = messaging.PersonalEMail.dont_send_bulk_email_result === 'True';

                                $rootScope.system.messaging = messaging;
                            });

                            $scope.loading = false;
                        });
                    }
                };


                $scope.resetEMailForm = function () {
                    $scope.emailModel = angular.copy($rootScope.system.messaging.SystemEMail);
                };


                $scope.removeEMailSettings = function () {
                    EmailService.removeEMailSettings($scope.emailModel).then(function () {
                        $scope.emailModel = null;
                        $rootScope.system.messaging.SystemEMail = null;
                    });
                };

                $scope.close = function () {
                    $mdDialog.hide();
                };

                $scope.submitGeneral = function () {
                    if (!$scope.systemForm.$valid) {
                        mdToast.error($filter('translate')('Module.RequiredError'));
                        return;
                    }

                    $scope.editEMail($scope.emailModel);
                };
            });
        }
    ]);