'use strict';

angular.module('primeapps')

    .controller('ProfileController', ['$rootScope', '$scope', 'SettingService', '$filter', '$modal',
        function ($rootScope, $scope, SettingService, $filter, $modal) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.menuTopTitle = "Settings";
            $scope.$parent.activeMenuItem = 'profile';

            var user = $scope.$parent.$parent.me;
            $scope.userModel = {};
            $scope.userModel.firstName = user.first_name; //$rootScope.user.firstName;
            $scope.userModel.lastName = user.last_name;//$rootScope.user.lastName;
            $scope.userModel.email = user.email; //$rootScope.user.email;
            $scope.selectedLanguage = angular.copy($scope.language);
            // $scope.selectedLocale = angular.copy($rootScope.locale);

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
                            })
                    }
                    else {
                        editUser();
                    }
                }

                function editUser() {
                    $scope.userUpdating = true;

                    SettingService.editUser(userModel)
                        .then(function () {
                            $scope.userModel.firstName = userModel.firstName;
                            $scope.userModel.lastName = userModel.lastName;
                            $scope.userModel.email = userModel.email;
                            $scope.userUpdating = false;
                            $scope.$parent.$parent.me.full_name = userModel.firstName + ' ' + userModel.lastName;

                            if (!emailChanged) {
                                swal($filter('translate')('Setup.Settings.UpdateSuccess'), "", "success");
                            }
                            else
                                swal($filter('translate')('Setup.Settings.UpdateSuccessEmail'), "", "success");

                        })
                        .catch(function () {
                            $scope.userUpdating = false;
                        });
                }
            };


        }
    ]);