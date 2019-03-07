'use strict';

angular.module('primeapps')

    .controller('ProfileController', ['$rootScope', '$scope', 'SettingService', '$filter', '$modal', 'AuthService', 'blockUI', 'FileUploader',
        function ($rootScope, $scope, SettingService, $filter, $modal, AuthService, blockUI, FileUploader) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            //$scope.$parent.menuTopTitle = "Settings";
            $scope.$parent.activeMenuItem = 'profile';

            var user = $scope.$parent.$parent.me;
            $scope.userModel = {};
            $scope.userModel.firstName = user.first_name; //$rootScope.user.firstName;
            $scope.userModel.lastName = user.last_name;//$rootScope.user.lastName;
            $scope.userModel.email = user.email; //$rootScope.user.email;
            $scope.userModel.picture = user.picture;
            $scope.selectedLanguage = angular.copy($scope.language);
            // $scope.selectedLocale = angular.copy($rootScope.locale);

            var uploader = $scope.uploader = new FileUploader({
                url: 'api/user/upload_profile_picture/' + $rootScope.me.id,
                headers: {
                    'Authorization': 'Bearer ' + window.localStorage.getItem('access_token'),//$localStorage.get('access_token'),
                    'Accept': 'application/json'
                },
                queueLimit: 1
            });

            uploader.onWhenAddingFileFailed = function (item, filter, options) {
                switch (filter.name) {
                    case 'imageFilter':
                        toastr.warning($filter('translate')('Setup.Settings.ImageError'));
                        break;
                    case 'sizeFilter':
                        toastr.warning($filter('translate')('Setup.Settings.SizeError'));
                        break;
                }
            };

            uploader.onAfterAddingFile = function (item) {
                $scope.uploadImage = true;
                $scope.croppedImage = '';
                var reader = new FileReader();
                $scope.userModel.picture = item.file.name;

                reader.onload = function (event) {
                    $scope.$apply(function () {
                        item.image = event.target.result;
                    });
                };
                reader.readAsDataURL(item._file);
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

            $scope.logoRemove = function () {
                if (uploader.queue[0]) {
                    //uploader.queue[0].image = null;
                    uploader.queue[0].remove();
                }
            };

            /// email configuration
            // $scope.emailModel = angular.copy($rootScope.system.messaging.PersonalEMail);
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

            $scope.editUser = function (userModel) {
                if ($scope.userForm.$valid) {
                    var emailChanged = false;

                    if (user.email != userModel.email)
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
                            });
                    }
                    else {
                        editUser();
                    }
                }

                function editUser() {
                    $scope.userUpdating = true;
                    if ($scope.uploadImage) {
                        uploader.queue[0].upload();
                        uploader.onCompleteItem = function (fileItem, pictureUrl, status) {
                            if (status === 200) {
                                $scope.userModel.picture = pictureUrl;
                                SettingService.editUser(userModel)
                                    .then(function () {
                                        $scope.userModel.firstName = userModel.firstName;
                                        $scope.userModel.lastName = userModel.lastName;
                                        $scope.userModel.email = userModel.email;
                                        $scope.userModel.picture = userModel.picture;
                                        $rootScope.me.firstName = userModel.firstName;
                                        $rootScope.me.lastName = userModel.lastName;
                                        $rootScope.me.picture = userModel.picture;
                                        $scope.userUpdating = false;
                                        $scope.$parent.$parent.me.full_name = userModel.firstName + ' ' + userModel.lastName;

                                        if (!emailChanged) {
                                            toastr.success($filter('translate')('Setup.Settings.UpdateSuccess'));
                                        }
                                        else
                                            toastr.success($filter('translate')('Setup.Settings.UpdateSuccessEmail'));

                                    })
                                    .catch(function () {
                                        $scope.userUpdating = false;
                                    });
                            }
                        };
                    }
                    else {
                        SettingService.editUser(userModel)
                            .then(function () {
                                $scope.userModel.firstName = userModel.firstName;
                                $scope.userModel.lastName = userModel.lastName;
                                $scope.userModel.email = userModel.email;
                                $scope.userModel.picture = userModel.picture;
                                $rootScope.me.firstName = userModel.firstName;
                                $rootScope.me.lastName = userModel.lastName;
                                $rootScope.me.picture = userModel.picture;
                                $scope.userUpdating = false;
                                $scope.$parent.$parent.me.full_name = userModel.firstName + ' ' + userModel.lastName;

                                if (!emailChanged) {
                                    toastr.success($filter('translate')('Setup.Settings.UpdateSuccess'));
                                }
                                else
                                    toastr.success($filter('translate')('Setup.Settings.UpdateSuccessEmail'));

                            })
                            .catch(function () {
                                $scope.userUpdating = false;
                            });
                    }
                }
            };
        }
    ]);