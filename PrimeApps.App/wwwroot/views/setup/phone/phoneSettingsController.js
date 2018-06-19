'use strict';

angular.module('primeapps')

    .controller('PhoneSettingsController', ['$rootScope', '$scope', '$translate', '$localStorage', 'ngToast', 'config', '$window', '$timeout', '$filter', 'blockUI', 'PhoneSettingsService', 'ngTableParams', '$popover',
        function ($rootScope, $scope, $translate, $localStorage, ngToast, config, $window, $timeout, $filter, blockUI, PhoneSettingsService, ngTableParams, $popover) {
            $scope.sipSettings = $rootScope.phoneSettings;
            $scope.sipLicenseAvailable = false;
            $scope.users = $rootScope.users;
            $scope.hasAdminRight = $filter('filter')($rootScope.profiles, { id: $rootScope.user.role.role_id }, true)[0].has_admin_rights;

            if ($scope.sipSettings) {
                renewSipUsers(false);
            }

            $scope.editSipProvider = function () {
                if (!$scope.sipProviderForm.$valid)
                    return;

                if ($scope.sipProvider) {
                    $scope.sipProviderUpdating = true;
                    var sipProvider = {};
                    sipProvider.provider = $scope.sipProvider;
                    sipProvider.CompanyKey = $scope.sipCompanyKey;
                    PhoneSettingsService.saveSipProvider(sipProvider).then(function () {
                        ngToast.create({ content: $filter('translate')('Setup.Settings.UpdateSuccess'), className: 'success' });
                        $scope.sipProviderUpdating = false;

                        $rootScope.phoneSettings = {};
                        $rootScope.phoneSettings.provider = $scope.sipProvider;
                        $scope.sipSettings = $rootScope.phoneSettings;
                    });
                }
            };

            $scope.recordDetailPhoneFieldNameFilter = function (field) {
                if (field.dataType.name === 'number' || field.dataType.name === 'text_single' && field.deleted === false) {
                    return field;
                }
            };

            $scope.sipAccountFilter = function (user) {
                if ($scope.sipUsers !== undefined) {
                    var sipuser = $filter('filter')($scope.sipUsers, { "userId": user.id });
                    if (sipuser.length === 0) return user;
                } else {
                    return user;
                }
            };

            $scope.showCreateAccountForm = function () {
                $scope.createSipPopover = $scope.createSipPopover || $popover(angular.element(document.getElementById('createButton')), {
                        templateUrl: 'views/setup/phone/sipAccountCreate.html',
                        placement: 'left',
                        scope: $scope,
                        autoClose: true,
                        show: true
                    });
            };

            $scope.showEditAccountForm = function (sipAccount) {
                var module = $filter('filter')($scope.modules, { name: sipAccount.recordDetailModuleName })[0];
                var field = $filter('filter')(module.fields, { name: sipAccount.recordDetailPhoneFieldName })[0];

                $scope.sipAccount = {
                    UserId: parseInt(sipAccount.userId),
                    Extension: sipAccount.extension,
                    Password: null,
                    IsAutoRegister: eval(sipAccount.isAutoRegister),
                    IsAutoRecordDetail: eval(sipAccount.isAutoRecordDetail),
                    RecordDetailModuleName: module,
                    RecordDetailPhoneFieldName: field
                };

                $scope.editSipPopover = $popover(angular.element(document.getElementById('editButton' + sipAccount.userId)), {
                    templateUrl: 'views/setup/phone/sipAccountEdit.html',
                    placement: 'left',
                    scope: $scope,
                    autoClose: true,
                    show: true
                });

            };

            $scope.addSipAccount = function (sipAccount) {
                if (sipAccount && sipAccount.Extension && sipAccount.Password && sipAccount.UserId && $scope.sipProvider) {
                    $scope.userInviting = true;
                    sipAccount.Connector = $scope.sipProvider;
                    sipAccount.company_key = $scope.sipCompanyKey;
                    sipAccount.UserId = parseInt(sipAccount.UserId);
                    sipAccount.RecordDetailModuleName = sipAccount.RecordDetailModuleName.name;
                    sipAccount.RecordDetailPhoneFieldName = sipAccount.RecordDetailPhoneFieldName.name;

                    PhoneSettingsService.saveSipAccount(sipAccount)
                        .then(function (response) {
                            renewSipUsers(true);
                            $scope.userInviting = false;
                            $scope.createSipPopover.hide();
                        });
                }
                else {
                    ngToast.create({ content: $filter('translate')('Setup.Phone.RequiredFields'), className: 'warning' });
                }
            };

            $scope.removeSipAccount = function (userId, index) {
                $scope.userDeleting = true;

                PhoneSettingsService.deleteSipAccount(userId)
                    .then(function onSuccess() {
                        $scope.sipUsers.splice(index, 1);
                        $scope.userDeleting = false;
                        ngToast.create({
                            content: $filter('translate')('Setup.Users.DismissSuccess'),
                            className: 'success'
                        });
                        renewSipUsers(true);
                    })
                    .catch(function onError() {
                        $scope.userDeleting = false;
                    });
            };

            function renewSipUsers(refresh) {
                $scope.sipProvider = $scope.sipSettings.provider;
                if (!refresh) {
                    $scope.sipUsers = $scope.sipSettings.sipUsers;
                    angular.forEach($scope.sipUsers, function (sipUser) {
                        var user = $filter('filter')($scope.users, { id: parseInt(sipUser.userId) }, true)[0];
                        sipUser.name = user.FullName;
                    });
                    $scope.sipCompanyKey = $scope.sipSettings.sipCompanyKey;

                    if ($scope.sipSettings.sipLicenseCount > 0) {
                        $scope.sipLicenseAvailable = true;
                        $scope.licensesBought = $scope.sipSettings.sipLicenseCount;
                        $scope.licensesUsed = $scope.sipUsers ? $scope.sipUsers.length : 0;
                        $scope.licenseAvailable = parseInt($scope.licensesBought) - parseInt($scope.licensesUsed);
                    }
                }
                else {
                    PhoneSettingsService.getSipConfig().then(function (response) {
                        $scope.sipUsers = response.data.sipUsers;
                        angular.forEach($scope.sipUsers, function (sipUser) {
                            var user = $filter('filter')($scope.users, { id: parseInt(sipUser.userId) }, true)[0];
                            sipUser.name = user.FullName;

                        });
                        $scope.sipCompanyKey = $scope.sipSettings.sipCompanyKey;
                        if ($scope.sipSettings.sipLicenseCount > 0) {
                            $scope.sipLicenseAvailable = true;
                            $scope.licensesBought = $scope.sipSettings.sipLicenseCount;
                            $scope.licensesUsed = $scope.sipUsers ? $scope.sipUsers.length : 0;
                            $scope.licenseAvailable = parseInt($scope.licensesBought) - parseInt($scope.licensesUsed);
                        }
                    });
                }
            }
        }
    ]);