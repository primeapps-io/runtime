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
                $scope.settingForm.$submitted = false;
                $scope.settingForm.$error = {};
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
                settingForm.$submitted = false;

                if ($scope.editing) {
                    SettingsService.update($scope.settingModel.id, $scope.settingModel)
                        .then(function (response) {
                            if (response.data) {
                                toastr.success("Setting is saved successfully");
                                $scope.grid.dataSource.read();
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
                                $scope.grid.dataSource.read();
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
                                    $scope.grid.dataSource.read();
                                    toastr.success("Setting is deleted successfully.", "Deleted!");

                                })
                                .catch(function () {
                                    toastr.error("Setting is not deleted successfully.", "Deleted!");
                                });
                        }

                    }
                });
            };

            //For Kendo UI
            $scope.goUrl = function (item) {
                var selection = window.getSelection();
                if (selection.toString().length === 0) {
                    $scope.showFormModal(item); //click event.
                }
            };

            var accessToken = $localStorage.read('access_token');

            $scope.mainGridOptions = {
                dataSource: {
                    type: "odata-v4",
                    page: 1,
                    pageSize: 10,
                    serverPaging: true,
                    serverFiltering: true,
                    serverSorting: true,
                    transport: {
                        read: {
                            url: "/api/setting/find",
                            type: 'GET',
                            dataType: "json",
                            beforeSend: function (req) {
                                req.setRequestHeader('Authorization', 'Bearer ' + accessToken);
                                req.setRequestHeader('X-App-Id', $rootScope.currentAppId);
                                req.setRequestHeader('X-Organization-Id', $rootScope.currentOrgId);
                            }
                        }
                    },
                    schema: {
                        data: "items",
                        total: "count",
                        model: {
                            id: "id", 
                        }
                    }
                },
                scrollable: false,
                persistSelection: true,
                sortable: true,
                filterable: {
                    extra: false
                },
                rowTemplate: function (e) {
                    var trTemp = '<tr ng-click="goUrl(dataItem)">';
                    trTemp += '<td> <span>' + e.key + '</span></td > ';
                    trTemp += '<td><span>' + e.value + '</span></td>';
                    trTemp += '<td ng-click="$event.stopPropagation();"> <button ng-click="$event.stopPropagation(); delete(dataItem, $event);" type="button" class="action-button2-delete"><i class="fas fa-trash"></i></button></td></tr>';
                    return trTemp;
                },
                altRowTemplate: function (e) {
                    var trTemp = '<tr class="k-alt" ng-click="goUrl(dataItem)">';
                    trTemp += '<td> <span>' + e.key + '</span></td > ';
                    trTemp += '<td><span>' + e.value + '</span></td>';
                    trTemp += '<td ng-click="$event.stopPropagation();"> <button ng-click="$event.stopPropagation(); delete(dataItem, $event);" type="button" class="action-button2-delete"><i class="fas fa-trash"></i></button></td></tr>';
                    return trTemp;
                },
                pageable: {
                    refresh: true,
                    pageSize: 10,
                    pageSizes: [10, 25, 50, 100],
                    buttonCount: 5,
                    info: true,
                },
                columns: [
                    {
                        field: 'Key',
                        title: 'Key',
                    },
                    {
                        field: 'Value',
                        title: 'Value',
                    },
                    {
                        field: '',
                        title: '',
                        width: "90px"
                    }]
            };
            //For Kendo UI


        }
    ]);