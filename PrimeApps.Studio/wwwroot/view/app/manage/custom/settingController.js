'use strict';

angular.module('primeapps')

    .controller('SettingController', ['$rootScope', '$scope', '$filter', '$state', '$http', 'config', '$localStorage', 'SettingsService', '$q', '$window', 'helper', '$modal',
        function ($rootScope, $scope, $filter, $state, $http, config, $localStorage, SettingsService, $q, $window, helper, $modal) {
            $scope.$parent.activeMenuItem = 'customsettings';
            $rootScope.breadcrumblist[2].title = 'Custom Settings';

            $scope.settings = [];
            $scope.loading = true;
            $scope.modalLoading = false;
            $scope.settingModel = {};

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }
            };

            $scope.generator(10);

            $scope.requestModel = {
                limit: "10",
                offset: 0
            };

            SettingsService.count()
                .then(function (response) {
                    $scope.pageTotal = response.data;
                    $scope.changePage(1);
                });

            $scope.changePage = function (page) {
                $scope.loading = true;

                if (page !== 1) {
                    var difference = Math.ceil($scope.pageTotal / $scope.requestModel.limit);

                    if (page > difference) {
                        if (Math.abs(page - difference) < 1)
                            --page;
                        else
                            page = page - Math.abs(page - Math.ceil($scope.pageTotal / $scope.requestModel.limit))
                    }
                }

                $scope.activePage = page;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;

                SettingsService.find(requestModel)
                    .then(function (response) {
                        $scope.settings = response.data;
                        $scope.loading = false;
                    });

            };

            $scope.changeOffset = function () {
                $scope.changePage($scope.activePage, true)
            };

            $scope.showFormModal = function (setting) {

                if (setting) {
                    $scope.modalLoading = true;
                    SettingsService.getById(setting.id)
                        .then(function (response) {
                            if (response.data) {
                                $scope.settingModel = angular.copy(response.data);
                                $scope.editing = true;
                                $scope.modalLoading = false;
                            }
                            else {
                                toastr.error("Current setting is not found.");
                                $scope.closeModal();
                            }
                        });

                }
                else {
                    $scope.editing = false;
                    $scope.settingModel = {};
                }

                $scope.settingFormModal = $scope.settingFormModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/manage/custom/settingFormModal.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.settingFormModal.$promise.then(function () {
                    $scope.settingFormModal.show();
                });

            };

            $scope.closeModal = function () {
                $scope.settingFormModal.hide();
                $scope.editing = false;
                $scope.settingModel = {};
            };

            $scope.save = function (settingForm) {
                if (!settingForm.$valid) {
                    toastr.error($filter('translate')('Module.RequiredError'));
                    return;
                }

                if (!$scope.settingModel || !$scope.settingModel.key || !$scope.settingModel.value) {
                    toastr.error($filter('translate')('Module.RequiredError'));
                    return;
                }

                $scope.saving = true;

                if ($scope.editing) {
                    SettingsService.update($scope.settingModel.id, $scope.settingModel)
                        .then(function (response) {
                            if (response.data) {
                                toastr.success("Setting is saved successfully");
                                $scope.changePage(1);
                                $scope.saving = false;
                                $scope.closeModal();
                            }
                        })
                        .catch(function (e) {
                            if (response.data && response.data.message) {
                                toastr.error(response.data.message);
                            }
                            else {
                                toastr.error($filter('translate')('Common.Error'));
                            }
                            $scope.saving = false;
                            $scope.closeModal();
                        });

                }
                else {
                    SettingsService.create($scope.settingModel)
                        .then(function (response) {
                            if (response.data) {
                                toastr.success("Setting is saved successfully");
                                $scope.changePage(1);
                                $scope.saving = false;
                                $scope.closeModal();
                            }
                        })
                        .catch(function (e) {
                            if (response.data && response.data.message) {
                                toastr.error(response.data.message);
                            }
                            else {
                                toastr.error($filter('translate')('Common.Error'));
                            }
                            $scope.saving = false;
                            $scope.closeModal();
                        });
                }
            };

            $scope.delete = function (setting, event) {
                swal({
                    title: "Are you sure?",
                    text: " ",
                    icon: "warning",
                    buttons: ['Cancel', 'Yes'],
                    dangerMode: true
                }).then(function (value) {
                    if (value) {
                        if (setting) {
                            SettingsService.delete(setting.id)
                                .then(function () {
                                    $scope.changePage($scope.activePage);
                                    toastr.success("Setting is deleted successfully.", "Deleted!");

                                })
                                .catch(function () {
                                    toastr.error("Setting is not deleted successfully.", "Deleted!");
                                });
                        }

                    }
                });
            };

        }
    ]);