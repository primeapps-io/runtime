'use strict';

angular.module('primeapps')

    .controller('ModuleProfileSettingController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', 'helper', '$cache', 'systemRequiredFields', 'systemReadonlyFields', 'ModuleService',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, helper, $cache, systemRequiredFields, systemReadonlyFields, ModuleService) {
            $scope.loading = true;
            var module = $filter('filter')($scope.$parent.modules, { name: $stateParams.module }, true)[0];

            if (!module) {
                toastr.warning($filter('translate')('Common.NotFound'));
                $state.go('app.dashboard');
                return;
            }

            $scope.requestModel = {
                limit: '10',
                offset: 0
            };

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }
            };
            $scope.generator(10);

            ModuleService.profileSettingsCount(module.id).then(function (response) {
                $scope.pageTotal = response.data;
            });

            //2 templateType Module
            ModuleService.profileSettingsFind($scope.requestModel, 2).then(function (response) {
                var templates = response.data;
                angular.forEach(templates, function (template) {
                    template.module = $filter('filter')($scope.$parent.modules, { name: template.module }, true)[0];
                });
                $scope.templates = templates;

            }).finally(function () {
                $scope.loading = false;
            });

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;

                ModuleService.profileSettingsCount(module.id).then(function (response) {
                    $scope.pageTotal = response.data;
                });

                ModuleService.profileSettingsFind(requestModel, 2).then(function (response) {
                    var data = $filter('filter')(response.data, { module_id: $scope.module.id }, true);
                    for (var i = 0; i < data.length; i++) {
                        for (var j = 0; j < data[i].profile_list.length; j++) {
                            var profileName = $filter('filter')($rootScope.appProfiles, { id: parseInt(data[i].profile_list[j]) }, true)[0].name;
                            if (!data[i].profileName)
                                data[i].profileName = profileName;
                            else
                                data[i].profileName += ', ' + profileName;
                        }
                    }
                    $scope.profileSettings = data;
                    $scope.profileSettingState = angular.copy($scope.profileSettings);
                    $scope.loading = false;

                }).finally(function () {
                    $scope.loading = false;
                });
            };

            $scope.changeOffset = function () {
                $scope.changePage(1);
            };

            $scope.module = angular.copy(module);

            var getModuleProfileSettings = function () {
                ModuleService.profileSettingsFind($scope.requestModel, 2).then(function (response) {
                    var data = $filter('filter')(response.data, { module_id: $scope.module.id }, true);
                    for (var i = 0; i < data.length; i++) {
                        for (var j = 0; j < data[i].profile_list.length; j++) {
                            var profileName = $filter('filter')($rootScope.appProfiles, { id: parseInt(data[i].profile_list[j]) }, true)[0].name;
                            if (!data[i].profileName)
                                data[i].profileName = profileName;
                            else
                                data[i].profileName += ', ' + profileName;
                        }
                    }
                    $scope.profileSettings = data;
                    $scope.profileSettingState = angular.copy($scope.profileSettings);
                    $scope.loading = false;
                })
                    .catch(function () {
                        $scope.loading = false;
                    });
            };

            getModuleProfileSettings();

            //show form modal
            $scope.showFormModal = function (profileSetting) {
                $scope.currentProfileSetting = {};
                $scope.icons = ModuleService.getIcons();

                $scope.multiselect = function () {
                    return $filter('filter')($rootScope.appProfiles, { deleted: false, has_admin_rights: false }, true);
                };

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
                    animation: '',
                    backdrop: 'static',
                    show: false
                });

                $scope.profileSettingsFormModal.$promise.then(function () {
                    $scope.profileSettingsFormModal.show();
                });
            };

            //submit
            $scope.save = function (moduleProfileSettingForm) {
                // if (!moduleProfileSettingForm.$valid)
                //     return;

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
                    getModuleProfileSettings();
                    toastr.success($filter('translate')('Setup.Modules.ModuleProfileSettingSaveSuccess')); 
                    $scope.saving = false;
                    $scope.profileSettingsFormModal.hide();
                    $scope.changePage(1);
                };

                if (!profileSetting.id) {
                    ModuleService.createModuleProfileSetting(obj)
                        .then(function () {
                            success();
                        })
                        .catch(function () {
                            $scope.profileSettings = $scope.profileSettingState;

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

            $scope.delete = function (profileSetting) {
                delete profileSetting.$$hashKey;
                var deleteModel = angular.copy($scope.profileSettings);
                var profileSettingIndex = helper.arrayObjectIndexOf(deleteModel, profileSetting);
                deleteModel.splice(profileSettingIndex, 1);

                var willDelete =
                    swal({
                        title: "Are you sure?",
                        text: "Are you sure that you want to delete this module profile setting?",
                        icon: "warning",
                        buttons: ['Cancel', 'Yes'],
                        dangerMode: true
                    }).then(function (value) {
                        if (value) {
                            ModuleService.deleteModuleProfileSetting(profileSetting.id)
                                .then(function () {
                                    var profileSettingIndex = helper.arrayObjectIndexOf($scope.profileSettings, profileSetting);
                                    $scope.profileSettings.splice(profileSettingIndex, 1);
                                    toastr.success("Profile setting is deleted successfully.", "Deleted!");

                                })
                                .catch(function () {
                                    $scope.profileSettings = $scope.profileSettingState;

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
        }
    ]);