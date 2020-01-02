'use strict';

angular.module('primeapps')

    .controller('ModuleProfileSettingController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', 'ModuleService', '$localStorage',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, ModuleService, $localStorage) {
            $scope.loading = false;
            var module = $filter('filter')($rootScope.appModules, { name: $stateParams.module }, true)[0];
            $scope.activePage = 1;


            if (!module) {
                toastr.warning($filter('translate')('Common.NotFound'));
                $state.go('app.dashboard');
                return;
            }

            $scope.module = angular.copy(module);

            $scope.multiselect = function () {
                return $filter('filter')($rootScope.appProfiles, { deleted: false, has_admin_rights: false }, true);
            };

            //show form modal
            $scope.showFormModal = function (profileSetting) {
                $scope.currentProfileSetting = {};
                $scope.icons = ModuleService.getIcons();

                if (profileSetting) {
                    var profileList = [];
                    if (profileSetting.profile_list.length > 0) {
                        for (var k = 0; k < profileSetting.profile_list.length; k++) {
                            var profile = $filter('filter')($rootScope.appProfiles, { id: parseInt(profileSetting.profile_list[k]) }, true)[0];
                            profileList.push(profile);
                        }
                    }

                    $scope.currentProfileSetting.id = profileSetting.id;
                    $scope.currentProfileSetting.profiles = profileList;
                    $scope.currentProfileSetting.pluralName = profileSetting.label_tr_plural;
                    $scope.currentProfileSetting.singularName = profileSetting.label_tr_singular;
                    $scope.currentProfileSetting.menu_icon = profileSetting.menu_icon;
                    $scope.currentProfileSetting.display = profileSetting.display;

                }

                if (!profileSetting) {
                    $scope.currentProfileSetting.isNew = true;
                    $scope.currentProfileSetting.pluralName = $scope.module.label_tr_plural;
                    $scope.currentProfileSetting.singularName = $scope.module.label_tr_singular;
                    $scope.currentProfileSetting.menu_icon = $scope.module.menu_icon;
                    $scope.currentProfileSetting.display = true;
                }


                $scope.currentProfileSettingState = angular.copy($scope.currentProfileSetting);

                $scope.profileSettingsFormModal = $scope.profileSettingsFormModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/model/modules/moduleProfileSettingForm.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.profileSettingsFormModal.$promise.then(function () {
                    $scope.profileSettingsFormModal.show();
                });
            };

            //submit
            $scope.save = function (moduleProfileSettingForm) {

                if (!moduleProfileSettingForm.$valid) {
                    if (moduleProfileSettingForm.$error.required)
                        toastr.error($filter('translate')('Module.RequiredError'));
                    return;
                }

                $scope.saving = true;
                var profileSetting = angular.copy($scope.currentProfileSetting);

                if (profileSetting.isNew)
                    delete profileSetting.isNew;

                var profiles = null;
                if ($scope.currentProfileSetting.profiles && $scope.currentProfileSetting.profiles.length) {
                    for (var j = 0; j < $scope.currentProfileSetting.profiles.length; j++) {
                        var profile = $scope.currentProfileSetting.profiles[j];
                        if (profiles === null)
                            profiles = profile.id;
                        else
                            profiles += ',' + profile.id;
                    }
                }

                var menuIcon;
                if (angular.isObject($scope.currentProfileSetting.menu_icon))
                    $scope.currentProfileSetting.menu_icon = $scope.currentProfileSetting.menu_icon.value;
                else
                    menuIcon = $scope.module.menu_icon;

                var obj = {
                    module_id: $scope.module.id,
                    profiles: profiles,
                    label_en_singular: $scope.currentProfileSetting.singularName,
                    label_tr_singular: $scope.currentProfileSetting.singularName,
                    label_en_plural: $scope.currentProfileSetting.pluralName,
                    label_tr_plural: $scope.currentProfileSetting.pluralName,
                    menu_icon: menuIcon,
                    display: $scope.currentProfileSetting.display
                };


                var success = function () {
                    $scope.grid.dataSource.read();
                    toastr.success($filter('translate')('Setup.Modules.ModuleProfileSettingSaveSuccess'));
                    $scope.saving = false;
                    $scope.profileSettingsFormModal.hide();
                };

                if (!profileSetting.id) {
                    ModuleService.createModuleProfileSetting(obj)
                        .then(function () {
                            success();
                        })
                        .catch(function () {

                            if ($scope.profileSettingsFormModal) {
                                $scope.profileSettingsFormModal.hide();
                                $scope.saving = false;
                            }
                        });
                } else {
                    ModuleService.updateModuleProfileSetting(profileSetting.id, obj)
                        .then(function onSuccess() {
                            success();
                        })
                        .catch(function () {
                            $scope.actionButtons = $scope.actionButtonState;

                            if ($scope.profileSettingsFormModal) {
                                $scope.profileSettingsFormModal.hide();
                                $scope.saving = false;
                            }
                        });
                }
            };

            $scope.delete = function (profileSetting, event) {
                var willDelete =
                    swal({
                        title: "Are you sure?",
                        text: " ",
                        icon: "warning",
                        buttons: ['Cancel', 'Yes'],
                        dangerMode: true
                    }).then(function (value) {
                        if (value) {
                            ModuleService.deleteModuleProfileSetting(profileSetting.id)
                                .then(function () {
                                    $scope.grid.dataSource.read();
                                    toastr.success("Profile setting is deleted successfully.", "Deleted!");

                                })
                                .catch(function () {
                                    if ($scope.profileSettingsFormModal) {
                                        $scope.profileSettingsFormModal.hide();
                                        $scope.saving = false;
                                    }
                                });
                        }
                    });
            };

            //cancel
            $scope.cancel = function () {
                angular.forEach($scope.currentProfileSetting, function (value, key) {
                    $scope.currentProfileSetting[key] = $scope.currentProfileSettingState[key];
                });

                $scope.profileSettingsFormModal.hide();
            };

            //For Kendo UI
            $scope.profileFilterList = [];
            var profileDataList = $scope.multiselect();

            for (var i = 0; i < profileDataList.length; i++) {

                var data = {
                    text: profileDataList[i]['name_' + $scope.language],
                    value: profileDataList[i].id.toString(),
                };
                $scope.profileFilterList.push(data);
            }

            $scope.goUrl = function (item) {
                var selection = window.getSelection();
                if (selection.toString().length === 0) {
                    $scope.showFormModal(item);
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
                            url: "/api/module_profile_settings/find",
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
                            fields: {
                                Profiles: { type: "string" },
                                CreatedAt: { type: "date" }
                            }
                        },
                        parse: function (data) {
                            for (var i = 0; i < data.items.length; i++) {
                                var labels = [];

                                for (var a = 0; a < data.items[i].profile_list.length; a++) {
                                    var profile = $filter('filter')($rootScope.appProfiles, { id: parseInt(data.items[i].profile_list[a]) }, true)[0];

                                    if (profile)
                                        labels.push(profile['name_' + $scope.language]);
                                }
                                data.items[i].profile_names = angular.copy(labels);
                            }

                            return data;
                        }
                    }
                },
                scrollable: false,
                persistSelection: true,
                sortable: true,
                noRecords: true,
                filterable: true,
                filter: function (e) {
                    if (e.filter) {
                        for (var i = 0; i < e.filter.filters.length; i++) {
                            e.filter.filters[i].ignoreCase = true;
                        }
                    }
                },
                rowTemplate: function (item) {
                    var trTemp = '<tr ng-click="goUrl(dataItem)">';
                    trTemp += '<td class="text-left">' + item.profile_names.join(', ') + '</td>';
                    trTemp += '<td class="text-left">' + kendo.toString(kendo.parseDate(item.created_at), "dd/MM/yyyy HH:mm:ss") + '</td>';
                    trTemp += '<td ng-click="$event.stopPropagation();"> <button ng-click="$event.stopPropagation(); delete(dataItem, $event);" type="button" class="action-button2-delete"><i class="fas fa-trash"></i></button></td></tr>';
                    return trTemp;
                },
                altRowTemplate: function (item) {
                    var trTemp = '<tr class="k-alt" ng-click="goUrl(dataItem)">';
                    trTemp += '<td class="text-left">' + item.profile_names.join(', ') + '</td>';
                    trTemp += '<td class="text-left">' + kendo.toString(kendo.parseDate(item.created_at), "dd/MM/yyyy HH:mm:ss") + '</td>';
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
                        field: 'Profiles',
                        title: $filter('translate')('Setup.Modules.ModuleProfileSettingProfiles'),
                        headerAttributes: {
                            'class': 'text-left'
                        },
                        filterable: {
                            multi: true,
                            search: false,
                            operator: "contains"
                        },
                        values: $scope.profileFilterList
                    },

                    {
                        field: 'CreatedAt',
                        title: $filter('translate')('Setup.UserCustomShares.CreatedDate'),
                        headerAttributes: {
                            'class': 'text-left'
                        },
                        filterable: {
                            ui: "datetimepicker"
                        }
                    },
                    {
                        field: '',
                        title: '',
                        width: "90px"
                    }]
            };
        }
    ]);