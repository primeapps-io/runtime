'use strict';

angular.module('primeapps')

    .controller('PasswordController', ['$rootScope', '$scope', '$filter', '$modal', 'SettingService',
        function ($rootScope, $scope, $filter, $modal, SettingService) {

           // $scope.$parent.menuTopTitle = "Settings";
            $scope.$parent.activeMenuItem = 'password';

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
                            toastr.success($filter('translate')('Setup.Settings.PasswordSuccess'));
                        })
                        .catch(function (response) {
                            if (response.status === 400)
                                $scope.passwordForm.current.$setValidity('wrongPassword', false);

                            $scope.passwordUpdating = false;
                        });
                }
            };
        }
    ]);