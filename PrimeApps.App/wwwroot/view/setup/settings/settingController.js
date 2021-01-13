'use strict';

angular.module('primeapps')

    .controller('SettingController', ['$rootScope', '$scope', '$localStorage', '$window', '$timeout', '$filter', 'blockUI', 'FileUploader', 'SettingService', 'EmailService', 'AppService', 'AuthService', '$cookies', '$state', 'officeHelper', 'mdToast', '$mdDialog',
        function ($rootScope, $scope, $localStorage, $window, $timeout, $filter, blockUI, FileUploader, SettingService, EmailService, AppService, AuthService, $cookies, $state, officeHelper, mdToast, $mdDialog) {

            $scope.activeTabHeaderTitle = $filter('translate')('Setup.Nav.PersonalSettings');
            $scope.activeTab = $state.params.tab ? $state.params.tab : 'email';//'general';

            if ($scope.activeTab === 'general' || $scope.activeTab === 'security') {
                mdToast.warning($filter('translate')('Common.NotFound'));
                $state.go('app.dashboard');
            }

            $scope.tempPicture = $rootScope.user.picture ? blobUrl + '/' + angular.copy($rootScope.user.picture) : null;
            $scope.disablePasswordChange = disablePasswordChange;
            const globalizations = $filter('filter')($rootScope.globalizations, { Status: 1 }, true);//actives
            $rootScope.breadcrumblist = [
                {
                    title: $filter('translate')('Layout.Menu.Dashboard'),
                    link: "#/app/dashboard"
                },
                {
                    title: $filter('translate')('Setup.Nav.PersonalSettings'),
                    link: '#/app/setup/settings'
                },
                {
                    title: null
                }
            ];

            $scope.loading = false;
            $scope.loadingModal = false;
            $scope.changedCulture = false;
            $scope.croppedImage = null;
            //$scope.languageArray = [{ value: 'tr', label: $filter('translate')('Setup.Settings.Turkish') },
            //{ value: 'en', label: $filter('translate')('Setup.Settings.English') }];
            $scope.formatArray = [{ value: 'tr', label: $filter('translate')('Setup.Settings.Turkey') },
            { value: 'en', label: $filter('translate')('Setup.Settings.UnitedStates') }];

            $scope.changeTab = function (tabKey) {
                $scope.activeTab = tabKey;
                $rootScope.breadcrumblist[2].title = $filter('translate')('Setup.Nav.Tabs.' + tabKey.charAt(0).toUpperCase() + tabKey.substr(1).toLowerCase());
                $state.go('app.setup.settings', { tab: tabKey }, { notify: false });
            };


            setUserModel();
            //$scope.selectedLanguage = angular.copy($rootScope.language);
            $scope.selectedLanguage = getSelectedLanguage($rootScope.language);
            $scope.selectedLocale = angular.copy($rootScope.locale);
            //$scope.multiLanguage = multiLanguage;
            $scope.useUserSettings = useUserSettings;
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
                    provider: "smtp",
                    user_name: "",
                    password: "",
                    senders: [],
                    enable_ssl: true,
                    dont_send_bulk_email_result: false
                };
            }

            $scope.editUser = function (userModel, fromModal) {
                if ($scope.userForm.$valid) {
                    var emailChanged = false;

                    if ($rootScope.user.email !== userModel.email)
                        emailChanged = true;

                    if (emailChanged) {
                        AuthService.isUniqueEmail(userModel.email, $scope.user.app_id)
                            .then(function (response) {
                                if (!response.data || response.data === 'NotAvailable') {
                                    if (!fromModal) {
                                        $scope.userForm.$setValidity('uniqueEmail', false);
                                    }
                                    $scope.userModel = $scope.copyUserModel;
                                    $scope.userUpdating = false;
                                    mdToast.warning($filter('translate')('Setup.Settings.EmailWarning'));
                                    return;
                                }

                                editUser();
                            })
                    } else {
                        editUser();
                    }
                }

                function editUser() {
                    $scope.cancel();
                    if (emailChanged || !fromModal) {
                        $scope.userUpdating = true;
                        userModel.id = $rootScope.user.id;
                        $scope.loading = true;
                        SettingService.editUser(userModel)
                            .then(function () {
                                $rootScope.user.first_name = userModel.first_name;
                                $rootScope.user.last_name = userModel.last_name;
                                $rootScope.user.email = userModel.email;
                                $rootScope.user.phone = userModel.phone;
                                $rootScope.user.picture = userModel.picture;
                                $scope.tempPicture = $rootScope.user.picture ? blobUrl + '/' + angular.copy($rootScope.user.picture) : null;
                                $rootScope.userPicture = $rootScope.user.picture ? blobUrl + '/' + angular.copy($rootScope.user.picture) : null;
                                $scope.userUpdating = false;
                                $scope.loading = false;
                                $scope.newUserEmail = "";
                                $scope.croppedImage = null;
                                $scope.$parent.croppedImage = null;

                                if (!emailChanged) {
                                    mdToast.success($filter('translate')('Setup.Settings.UpdateSuccess'));
                                } else {
                                    logout();
                                    mdToast.success($filter('translate')('Setup.Settings.UpdateSuccessEmail'));
                                }

                                if ($scope.changedCulture)
                                    $window.location.reload();
                                else
                                    $state.reload();
                            })
                            .catch(function () {
                                $scope.loading = false;
                                $scope.userForm.$submitted = false;
                                $scope.userModel = $scope.copyUserModel;
                                mdToast.error($filter('translate')('Common.Error'));
                            });
                    }
                    else {
                        mdToast.warning($filter('translate')('Setup.Settings.EmailWarning'))
                    }
                }
            };

            $scope.passwordModel = {};

            $scope.changePassword = function (passwordModel) {
                if ($scope.userForm.$valid) {
                    $scope.loading = true;
                    SettingService.changePassword(passwordModel.current, passwordModel.password, passwordModel.confirm)
                        .then(function () {
                            mdToast.success($filter('translate')('Setup.Settings.PasswordSuccess'));
                            $scope.userForm.$setPristine();
                            $scope.passwordModel = {};
                            $scope.userForm.$setUntouched();
                            $scope.loading = false;
                        })
                        .catch(function (response) {
                            if (response.status === 400)
                                $scope.userForm.current.$setValidity('wrongPassword', false);

                            $scope.loading = false;
                        });
                }
            };

            $scope.passwordNotMatch = function (password, passwordAgain) {
                if (password !== passwordAgain)
                    $scope.showError('compareTo')
            };

            $scope.changeLanguage = function (culture) {
                if (culture) {
                    const result = culture.split('-');
                    const language = result[0];
                    if (language === $rootScope.language)
                        return;

                    $scope.userModel.language = language;

                    //SettingService.editUser($scope.userModel)
                    //    .then(function onSuccess() {
                    //blockUI.start();
                    $rootScope.language = language;
                    $scope.changedCulture = true;
                    $localStorage.write('NG_TRANSLATE_LANG_KEY', language);
                    $cookies.put('_lang', language);
                    //$window.location.reload();
                    // });
                }
            };

            $scope.changeLocale = function (locale) {
                if (locale === $rootScope.locale)
                    return;

                $scope.changedCulture = true;
                $scope.userModel.culture = locale === 'tr' ? 'tr-TR' : 'en-US';
                //blockUI.start();
                $localStorage.write('locale_key', locale);

                //$window.location.reload();
            };

            $scope.changeCurrency = function (code) {
                SettingService.changeCurrency(code)
                    .then(function () {
                        $rootScope.user.currency = code;
                        mdToast.success($filter('translate')('Setup.Settings.CurrencySuccess'));
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
                            mdToast.error($filter('translate')('Setup.Settings.InvalidPassword'))
                        }
                        $scope.accountDeleting = false;
                        blockUI.stop();
                    });
            };

            $scope.showNewSenderForm = function () {
                $scope.email = null;
                $scope.alias = null;

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
                $scope.loadingModal = true;

                if (!this.senderForm.alias.$valid || !this.senderForm.email.$valid) {
                    $scope.loadingModal = false;
                    return;
                }

                if ($scope.emailModel.senders === null) {
                    $scope.emailModel.senders = [];
                }

                $scope.emailModel.senders.push({
                    "alias": alias,
                    "email": email
                });

                $scope.close();
                $scope.loadingModal = false;
                $scope.userForm.$setValidity("noSender", true);
            };

            $scope.removeSender = function (ev, sender) {

                var confirm = $mdDialog.confirm()
                    .title($filter('translate')('Common.AreYouSure'))
                    .targetEvent(ev)
                    .ok($filter('translate')('Common.Yes'))
                    .cancel($filter('translate')('Common.No'));

                $mdDialog.show(confirm)
                    .then(function () {
                        if ($scope.emailModel.senders === null) return;
                        var index = $scope.emailModel.senders.indexOf(sender);
                        $scope.emailModel.senders.splice(index, 1);
                    }, function () {
                        $scope.status = 'You decided to keep your debt.';
                    });
            };

            $scope.editEMail = function () {
                if ($scope.addingNewSender) {
                    $scope.addingNewSender = false;
                    return;
                }

                if ($scope.emailModel.senders.length === 0) {
                    $scope.userForm.$setValidity("noSender", false);
                }

                if ($scope.userForm.$valid) {
                    $scope.emailUpdating = true;
                    $scope.emailModel.provider = "smtp";

                    if ($scope.emailModel.host.indexOf("yandex") > -1) {
                        $scope.emailModel.host = "smtp.yandex.ru";
                        $scope.emailModel.port = "587";
                        $scope.emailModel.enable_ssl = true;
                    }

                    EmailService.updatePersonalEMailSettings($scope.emailModel).then(function () {

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

                        $scope.emailUpdating = false;
                    });
                }
            };

            $scope.resetEMailForm = function () {
                $scope.emailModel = angular.copy($rootScope.system.messaging.PersonalEMail);
            };

            $scope.removePersonalEMailSettings = function () {
                EmailService.removePersonalEMailSettings($scope.emailModel).then(function () {
                    $scope.emailModel = null;
                    $rootScope.system.messaging.PersonalEMail = null;
                });
            };

            var appId = $cookies.get(preview ? 'preview_app_id' : 'app_id');
            var tenantId = $cookies.get(preview ? 'preview_tenant_id' : 'tenant_id');

            var uploader = $scope.uploader = new FileUploader({
                url: 'storage/upload_profile_picture',
                headers: {
                    'Authorization': 'Bearer ' + window.localStorage.getItem('access_token'),//$localStorage.get('access_token'),
                    'Accept': 'application/json',
                    'Content-Type': 'application/x-www-form-urlencoded;charset=utf-8',
                    'X-Tenant-Id': tenantId,
                    'X-App-Id': appId
                },
                queueLimit: 1
            });

            uploader.onCompleteItem = function (fileItem, response, status, headers) {
                if (status === 200) {
                    $scope.userModel.picture = response;
                    $scope.croppedImage = null;
                    $scope.$parent.croppedImage = null;
                    uploader.clearQueue();
                }
            };

            uploader.onWhenAddingFileFailed = function (item, filter, options) {
                switch (filter.name) {
                    case 'imageFilter':
                        mdToast.warning($filter('translate')('Setup.Settings.ImageError'));
                        break;
                    case 'sizeFilter':
                        mdToast.warning($filter('translate')('Setup.Settings.SizeError'))
                        break;
                }
            };

            uploader.onAfterAddingFile = function (item) {
                var reader = new FileReader();

                reader.onload = function (event) {
                    $scope.$apply(function () {
                        item.image = event.target.result;
                    });
                };

                reader.readAsDataURL(item._file);
            };

            //uploader.onBeforeUploadItem = function (item) {
            //    item._file = dataURItoBlob($scope.croppedImage);
            //};

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

            $scope.removeProfileImage = function () {
                $scope.tempPicture = null;
                $scope.croppedImage = null;
                $scope.$parent.croppedImage = null;
                $scope.userModel.id = $rootScope.user.id;
                $scope.userModel.picture = null;
            };

            $scope.submitGeneral = function () {

                if (!$scope.userForm.$valid) {
                    mdToast.error($filter('translate')('Module.RequiredError'));
                    return;
                }

                switch ($scope.activeTab) {
                    case 'general':
                        $scope.editUser($scope.userModel);
                        break;
                    case 'security':
                        $scope.changePassword($scope.passwordModel);
                        break;
                    case 'email':
                        $scope.editEMail($scope.emailModel);
                        break;
                }
            };

            $scope.close = function () {
                $mdDialog.hide();
            };

            //For Kendo UI
            $scope.languageOptions = {
                dataSource: globalizations,
                dataTextField: "Language",
                dataValueField: "Culture",
                filter: globalizations > 10 ? 'startswith' : null,
                optionLabel: $filter('translate')('Common.Select'),
                change: function (e) {
                    return $scope.changeLanguage(this.value());
                }
            };

            $scope.formatOptions = {
                dataSource: $scope.formatArray,
                dataTextField: "label",
                dataValueField: "value",
                change: function (e) {
                    return $scope.changeLanguage(this.value());
                }
            };
            //For Kendo UI

            $scope.changeEmailModal = function (ev) {

                var parentEl = angular.element(document.body);
                $mdDialog.show({
                    parent: parentEl,
                    templateUrl: 'view/setup/settings/pages/changeEmail.html',
                    clickOutsideToClose: true,
                    scope: $scope,
                    preserveScope: true
                });
            };

            $scope.changeEmail = function (changeEmailForm) {
                if (!changeEmailForm.validate()) {
                    mdToast.error($filter('translate')('Module.RequiredError'));
                    return;
                }

                $scope.userModel.email = $scope.newUserEmail;

                $scope.editUser($scope.userModel, true);

            };

            function setUserModel() {
                $scope.userModel = {};
                $scope.userModel.first_name = $rootScope.user.first_name;
                $scope.userModel.last_name = $rootScope.user.last_name;
                $scope.userModel.email = $rootScope.user.email;
                $scope.userModel.phone = $rootScope.user.phone;
                $scope.userModel.picture = $rootScope.user.picture;
                $rootScope.processLanguage($rootScope.user.profile);
                $scope.userModel.profileName = $rootScope.getLanguageValue($rootScope.user.profile.languages, 'name');
                $scope.copyUserModel = angular.copy($scope.userModel);
                var title = document.getElementsByTagName('title')[0];
                $scope.userModel.appName = title.textContent;
            }

            $scope.cancel = function () {
                $mdDialog.cancel();
            };

            function logout() {

                AuthService.logoutComplete();

                window.location = '/logout';
            };

            $scope.showError = function (error) {
                switch (error) {
                    case 'compareTo':
                        mdToast.error($filter('translate')('Setup.Settings.PasswordNotMatch'));
                        break;
                    case 'minlength':
                        mdToast.error($filter('translate')('Setup.Settings.PasswordMinimum'));
                        break;
                    case 'wrongPassword':
                        mdToast.error($filter('translate')('Setup.Settings.PasswordWrong'));
                        break;
                }
            };

            $scope.uploadFunc = function (item, image) {
                item._file = dataURItoBlob(image);
                item.upload();
            };

            function getSelectedLanguage(language) {
                const result = $filter('filter')(globalizations, function (globalizaton) {
                    return globalizaton.Culture.contains(language + '-');
                }, true)[0];

                return result ? result.Culture : '';
            }
        }
    ]);