'use strict';

angular.module('primeapps')

    .controller('SettingController', ['$rootScope', '$scope', '$translate', 'tmhDynamicLocale', '$localStorage', 'ngToast', 'config', '$window', '$timeout', '$filter', 'blockUI', 'FileUploader', 'SettingService', 'MessagingService', 'LayoutService', 'AuthService', 'ngTableParams', '$popover', '$cookies', '$state', 'officeHelper',
        function ($rootScope, $scope, $translate, tmhDynamicLocale, $localStorage, ngToast, config, $window, $timeout, $filter, blockUI, FileUploader, SettingService, MessagingService, LayoutService, AuthService, ngTableParams, $popover, $cookies, $state, officeHelper) {
            $scope.userModel = {};
            $scope.userModel.first_name = $rootScope.user.first_name;
            $scope.userModel.last_name = $rootScope.user.last_name;
            $scope.userModel.email = $rootScope.user.email;
            $scope.userModel.phone = $rootScope.user.phone;
            $scope.selectedLanguage = angular.copy($rootScope.language);
            $scope.selectedLocale = angular.copy($rootScope.locale);
            $scope.customLanguage = customLanguage;
            $scope.showPasswordControl = false;

            officeHelper.officeTenantInfo()
                .then(function (adInfo) {
                    if (!adInfo.data || (adInfo.data.email !== $rootScope.user.email)) {
                        $scope.showPasswordControl = true;
                    }
                });

            /// email configuration
            $scope.emailModel = angular.copy($rootScope.system.messaging.PersonalEMail);
            $scope.newSender = {};
            $scope.addingNewSender = false;

            if ($scope.emailModel == null || !$scope.emailModel.hasOwnProperty("provider")) {
                $scope.emailModel = {
                    provider: "",
                    user_name: "",
                    password: "",
                    senders: [],
                    enable_ssl: false
                };
            }

            $scope.tableParams = new ngTableParams({
                page: 1,            // show first page
                count: 10           // count per page
            },
                {
                    total: 0, // length of data
                    getData: function ($defer, params) {
                        if ($scope.emailModel.senders) {
                            params.total($scope.emailModel.senders.length);
                            $defer.resolve($scope.emailModel.senders.slice((params.page() - 1) * params.count(), params.page() * params.count()));
                        }
                    },
                    $scope: $scope
                });

            $scope.tableParams.reload();

            $scope.editUser = function (userModel) {
                if ($scope.userForm.$valid) {
                    var emailChanged = false;

                    if ($rootScope.user.email != userModel.email)
                        emailChanged = true;

                    if (emailChanged) {
                        AuthService.isUniqueEmail(userModel.email)
                            .then(function (data) {
                                if (!data.data) {
                                    $scope.userForm.$setValidity('uniqueEmail', false);
                                    $scope.userUpdating = false;
                                    return;
                                }

                                editUser();
                            })
                    }
                    else {
                        editUser();
                    }
                }

                function editUser() {
                    $scope.userUpdating = true;
                    userModel.id = $rootScope.user.id;

                    SettingService.editUser(userModel)
                        .then(function () {
                            $rootScope.user.first_name = userModel.first_name;
                            $rootScope.user.last_name = userModel.last_name;
                            $rootScope.user.email = userModel.email;
                            $rootScope.user.phone = userModel.phone;
                            $scope.userUpdating = false;

                            if (!emailChanged)
                                ngToast.create({ content: $filter('translate')('Setup.Settings.UpdateSuccess'), className: 'success' });
                            else
                                ngToast.create({ content: $filter('translate')('Setup.Settings.UpdateSuccessEmail'), className: 'success', timeout: 6000 });

                        })
                        .catch(function () {
                            $scope.userUpdating = false;
                        });
                }
            };

            $scope.changePassword = function (passwordModel) {
                if ($scope.passwordForm.$valid) {
                    $scope.passwordUpdating = true;

                    SettingService.changePassword(passwordModel.current, passwordModel.password, passwordModel.confirm)
                        .then(function () {
                            passwordModel.current = null;
                            passwordModel.password = null;
                            passwordModel.confirm = null;
                            $scope.passwordForm.current.$setPristine();
                            $scope.passwordForm.password.$setPristine();
                            $scope.passwordForm.confirm.$setPristine();
                            $scope.passwordForm.$setPristine();

                            $scope.passwordUpdating = false;
                            ngToast.create({ content: $filter('translate')('Setup.Settings.PasswordSuccess'), className: 'success' });
                        })
                        .catch(function (response) {
                            if (response.status === 400)
                                $scope.passwordForm.current.$setValidity('wrongPassword', false);

                            $scope.passwordUpdating = false;
                        });
                }
            };

            $scope.changeLanguage = function () {
                if ($scope.selectedLanguage == $rootScope.language)
                    return;

                blockUI.start();
                $localStorage.write('NG_TRANSLATE_LANG_KEY', $scope.selectedLanguage);
                $cookies.put('_lang', $scope.selectedLanguage);
                $window.location.reload();
            };

            $scope.changeLocale = function () {
                if ($scope.selectedLocale == $rootScope.locale)
                    return;

                blockUI.start();
                $localStorage.write('locale_key', $scope.selectedLocale);

                $window.location.reload();
            };

            $scope.changeCurrency = function (code) {
                SettingService.changeCurrency(code)
                    .then(function () {
                        $rootScope.user.currency = code;
                        ngToast.create({ content: $filter('translate')('Setup.Settings.CurrencySuccess'), className: 'success' });
                    });
            };

            $scope.deleteAccount = function (confirmPassword) {
                blockUI.start();
                $scope.accountDeleting = true;

                SettingService.removeUser(confirmPassword)
                    .then(function () {
                        $localStorage.remove('access_token');
                        $localStorage.remove('refresh_token');
                        $localStorage.remove('Workgroup');
                        $localStorage.remove('NG_TRANSLATE_LANG_KEY');
                        $localStorage.remove('currency');
                        $window.location.href = '#auth/login';

                        $timeout(function () {
                            $window.location.reload(true);
                        });
                    })
                    .catch(function (response) {
                        if (response.status === 400) {
                            ngToast.create({ content: $filter('translate')('Setup.Settings.InvalidPassword'), className: 'danger', timeout: 6000 });
                        }
                        $scope.accountDeleting = false;
                        blockUI.stop();
                    });
            };


            $scope.showNewSenderForm = function () {
                $scope.senderPopover = $scope.senderPopover || $popover(angular.element(document.getElementsByName('addSender')), {
                    templateUrl: 'view/setup/messaging/senderAdd.html',
                    placement: 'left',
                    scope: $scope,
                    autoClose: true,
                    show: true
                });
            };

            $scope.addNewSender = function (alias, email) {
                $scope.addingNewSender = true;

                if (!this.senderForm.alias.$valid || !this.senderForm.email.$valid) return;

                if ($scope.emailModel.senders == null) {
                    $scope.emailModel.senders = [];
                }

                $scope.emailModel.senders.push({
                    "alias": alias,
                    "email": email
                });
                $scope.senderPopover.hide();
                $scope.tableParams.reload();
                $scope.emailForm.$setValidity("noSender", true);
            };

            $scope.removeSender = function (sender) {
                if ($scope.emailModel.senders == null) return;
                var index = $scope.emailModel.senders.indexOf(sender);
                $scope.emailModel.senders.splice(index, 1);
                $scope.tableParams.reload();
            };

            $scope.editEMail = function () {
                if ($scope.addingNewSender) {
                    $scope.addingNewSender = false;
                    return;
                }

                if ($scope.emailModel.senders.length == 0) {
                    $scope.emailForm.$setValidity("noSender", false);
                }

                if ($scope.emailForm.$valid) {
                    $scope.emailUpdating = true;

                    if ($scope.emailModel.host.indexOf("yandex") > -1) {
                        $scope.emailModel.host = "smtp.yandex.ru";
                        $scope.emailModel.port = "587";
                        $scope.emailModel.enable_ssl = true;

                    }

                    MessagingService.updatePersonalEMailSettings($scope.emailModel).then(function () {
                        ngToast.create({ content: $filter('translate')('Setup.Settings.UpdateSuccess'), className: 'success' });

                        MessagingService.getSetting().then(function (response) {
                            var messaging = response.data;

                            if (messaging.SystemEMail)
                                messaging.SystemEMail.enable_ssl = messaging.SystemEMail.enable_ssl === 'True';

                            if (messaging.PersonalEMail)
                                messaging.PersonalEMail.enable_ssl = messaging.PersonalEMail.enable_ssl === 'True';

                            $rootScope.system.messaging = messaging;
                        });

                        $scope.emailUpdating = false;
                    });
                }
            };

            $scope.resetEMailForm = function () {
                $scope.emailModel = angular.copy($rootScope.system.messaging.PersonalEMail);
            };

            $scope.removePersonalEMailSettings = function () {
                MessagingService.removePersonalEMailSettings($scope.emailModel).then(function () {
                    $scope.emailModel = null;
                    $rootScope.system.messaging.PersonalEMail = null;
                });
            };

            var uploader = $scope.uploader = new FileUploader({
                url: 'storage/upload_profile_picture',
                queueLimit: 1
            });

            uploader.onCompleteItem = function (fileItem, response, status, headers) {
                if (status === 200) {
                    var userModel = angular.copy($rootScope.user);
                    userModel.picture = response;

                    SettingService.editUser(userModel)
                        .then(function () {
                            uploader.clearQueue();
                            LayoutService.getMyAccount(true);
                        });

                }
            };

            uploader.onWhenAddingFileFailed = function (item, filter, options) {
                switch (filter.name) {
                    case 'imageFilter':
                        ngToast.create({ content: $filter('translate')('Setup.Settings.ImageError'), className: 'warning' });
                        break;
                    case 'sizeFilter':
                        ngToast.create({ content: $filter('translate')('Setup.Settings.SizeError'), className: 'warning' });
                        break;
                }
            };

            uploader.onAfterAddingFile = function (item) {
                $scope.croppedImage = '';
                var reader = new FileReader();

                reader.onload = function (event) {
                    $scope.$apply(function () {
                        item.image = event.target.result;
                    });
                };

                reader.readAsDataURL(item._file);
            };

            uploader.onBeforeUploadItem = function (item) {
                item._file = dataURItoBlob($scope.croppedImage);
            };

            uploader.filters.push({
                name: 'imageFilter',
                fn: function (item, options) {
                    var type = '|' + item.type.slice(item.type.lastIndexOf('/') + 1) + '|';
                    return '|jpg|png|jpeg|bmp|'.indexOf(type) > -1;
                }
            });

            uploader.filters.push({
                name: 'sizeFilter',
                fn: function (item) {
                    return item.size < 5242880;//5 mb
                }
            });

            var dataURItoBlob = function (dataURI) {
                var binary = atob(dataURI.split(',')[1]);
                var mimeString = dataURI.split(',')[0].split(':')[1].split(';')[0];
                var array = [];

                for (var i = 0; i < binary.length; i++) {
                    array.push(binary.charCodeAt(i));
                }

                return new Blob([new Uint8Array(array)], { type: mimeString });
            };

            $scope.themes = SettingService.getThemes();
            $scope.selectedTheme = $localStorage.read('theme');
            $scope.setThemeColor = function (theme) {
                $localStorage.write('theme', theme.name);
                $scope.selectedTheme = theme.name;
                $rootScope.theme = theme.name;

            };

            $scope.removeProfileImage = function () {
                var userModel = $scope.userModel;
                userModel.id = $rootScope.user.id;
                userModel.picture = "";

                SettingService.editUser(userModel).then(function () {
                    $rootScope.user.picture = null;
                    $state.reload();
                });
            };


        }
    ]);