'use strict';

angular.module('primeapps')

    .controller('OfficeController', ['$rootScope', '$scope', '$filter', 'ngToast', 'OfficeService', '$localStorage', '$window', '$interval', 'officeHelper',
        function ($rootScope, $scope, $filter, ngToast, OfficeService, $localStorage, $window, $interval, officeHelper) {

            $scope.hasAdminRight = $filter('filter')($rootScope.profiles, { id: $rootScope.user.profile.id }, true)[0].has_admin_rights;
            $scope.officeConnectionOpening = false;
            $scope.isConnected = false;
            $scope.loading = true;

            /*officeHelper.officeTenantInfo()
                .then(function(adInfo){
                    $scope.loading = false;
                    if(adInfo.data){
                        $rootScope.user.azureDirectory = adInfo.data;
                        $scope.adInfo = adInfo.data;
                        $scope.isConnected = true;
                    }
                });*/

            $scope.popupClosed = function () {
                OfficeService.activeDirectoryInfo()
                    .then(function(adInfo){
                        if(adInfo.data){
                            $rootScope.user.azureDirectory = adInfo.data;
                            $scope.adInfo = adInfo.data;
                            $scope.isConnected = true;
                        } else {
                            $scope.officeConnectionOpening = false;
                        }
                    });
            };

            $scope.officeConnection = function(){
                $scope.officeConnectionOpening = true;
                var popWindow = $window.open("/ActiveDirectory/SignUp",'popUpWindow','height=550,width=600,resizable=no,scrollbars=yes,menubar=no');

                var pollTimer = $interval(function() {
                    if (popWindow.closed !== false) {
                        $interval.cancel(pollTimer);
                        $scope.popupClosed();
                    }
                }, 200);
            };
            /*GeneralSettingsService.getByKey('module', 'detail_view_type')
                .then(function (response) {
                    if (response.data) {
                        $scope.setting = response.data;
                        $scope.detailViewType = $scope.setting.value;
                    }
                });*/

            /*$scope.saveDetailViewType = function () {
                $scope.savingDetailViewType = true;

                var success = function () {
                    $rootScope.detailViewType = $scope.detailViewType;
                    $scope.savingDetailViewType = false;
                    ngToast.create({ content: $filter('translate')('Setup.Settings.UpdateSuccessGeneralSettings'), className: 'success' });
                };

                if ($scope.setting) {
                    $scope.setting.value = $scope.detailViewType;

                    GeneralSettingsService.update($scope.setting)
                        .then(function () {
                            success();
                        });
                }
                else {
                    $scope.setting = {
                        type: 'module',
                        key: 'detail_view_type',
                        value: $scope.detailViewType
                    };

                    GeneralSettingsService.create($scope.setting)
                        .then(function () {
                            success();
                        });
                }
            */
        }
    ]);