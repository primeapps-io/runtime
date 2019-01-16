'use strict';

angular.module('primeapps')

    .controller('ProfilesController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'ProfilesService', 'LayoutService', '$http', 'config', '$popover','$location',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService, ProfilesService, LayoutService, $http, config, $popover,$location) {
            $scope.$parent.menuTopTitle = "Authorization";
            $scope.$parent.activeMenu = 'authorization';
            $scope.$parent.activeMenuItem = 'profiles';
            $scope.loading = true;
            var id = parseInt($location.search().id);
            var clone = parseInt($location.search().clone);

            $scope.moduleLead = $filter('filter')($scope.$parent.modules, { name: 'leads' }, true)[0];
            $scope.moduleIzinler = $filter('filter')($scope.$parent.modules, { name: 'izinler' }, true)[0];
            $scope.moduleRehber = $filter('filter')($scope.$parent.modules, { name: 'rehber' }, true)[0];

            $scope.startPageList = [
                {
                    "value": "Dashboard",
                    "valueLower": "dashboard",
                    "name": $filter('translate')('Layout.Menu.Dashboard')
                },
                {
                    "value": "Newsfeed",
                    "valueLower": "newsfeed",
                    "name": $filter('translate')('Layout.Menu.Newsfeed')
                },
                {
                    "value": "Tasks",
                    "valueLower": "tasks",
                    "name": $filter('translate')('Layout.Menu.Tasks')
                },
                {
                    "value": "Calendar",
                    "valueLower": "calendar",
                    "name": $filter('translate')('Layout.Menu.Calendar')
                }
            ];

            if ($scope.moduleRehber) {
                $scope.startPageList.push({
                    "value": "Home",
                    "valueLower": "home",
                    "name": $filter('translate')('Layout.Menu.Homepage')
                });
            }

            function getProfile() {
                ProfilesService.getAll()
                    .then(function (response) {
                        $scope.profiles = ProfilesService.getProfiles(response.data,$scope.$parent.modules, false);
                        $scope.profilesCopy = angular.copy($scope.profiles);

                        if (id) {
                            $scope.profile = $filter('filter')($scope.profiles, { id: id }, true)[0];

                            //Update
                            var setPageStart = $filter('filter')($scope.startPageList, { valueLower: $scope.profile.start_page }, true)[0];
                            $scope.profile.PageStart = setPageStart;

                            if ($scope.profile.parent_id != 0) {
                                $scope.profile.parent_id = $filter('filter')($scope.profiles, { id: $scope.profile.parent_id }, true)[0];
                            }
                        }
                        else {
                            $scope.profile = {};
                            if (clone) {
                                var profile = $filter('filter')($scope.profiles, { id: clone }, true)[0];
                                $scope.profile = profile;
                                delete  $scope.profile.name;
                                delete  $scope.profile.user_ids;
                                delete  $scope.profile.description;
                                delete  $scope.profile.is_persistent;
                                delete  $scope.profile.CreatedBy;
                                delete  $scope.profile.id;
                                var setPageStart = $filter('filter')($scope.startPageList, {valueLower: $scope.profile.start_page}, true)[0];
                                $scope.profile.PageStart = setPageStart;
                                $scope.profile.parent_id = $filter('filter')($scope.profiles, { id: profile.parent_id }, true)[0];
                            }
                            else {
                                $scope.profile.has_admin_rights = false;
                                $scope.profile.is_persistent = false;
                                $scope.profile.business_intelligence = false;
                                $scope.profile.send_email = false;
                                $scope.profile.send_sms = false;
                                $scope.profile.export_data = false;
                                $scope.profile.import_data = false;
                                $scope.profile.word_pdf_download = false;
                                $scope.profile.lead_convert = false;
                                $scope.profile.document_search = false;
                                $scope.profile.tasks = false;
                                $scope.profile.calendar = false;
                                $scope.profile.newsfeed = false;
                                $scope.profile.report = false;
                                $scope.profile.dashboard = true;
                                $scope.profile.home = false;
                                $scope.profile.collective_annual_leave = false;
                                $scope.profile.permissions = $filter('filter')($scope.profiles, { is_persistent: true, has_admin_rights: true })[0].permissions;
                                //Create
                                var dashboard = $filter('filter')($scope.startPageList, { value: "Dashboard" }, true)[0];
                                $scope.profile.PageStart = dashboard;
                            }
                        }

                        $scope.loading = false;
                    })
                    .catch(function () {
                        $scope.loading = false;
                    });
            }

            getProfile();

            $scope.SetStartPage = function () {

                var setValue = $scope.profile.PageStart.value;
                $scope.profile[setValue] = true;

                if ($scope.profile.PageStart.value === "Newsfeed") {
                    var startPageNewsfeedControl = $filter('filter')($scope.profile.Permissions, { Type: 3 }, true)[0];
                    startPageNewsfeedControl.Read = true;
                }

            };


            function validate() {
                var isValid = true;
                var existingProfile = null;

                if (!$scope.profile.id) {
                    existingProfile = $filter('filter')($scope.profilesCopy, { Name: $scope.profile.name }, true)[0];

                    if (existingProfile)
                        isValid = false;
                }
                else {
                    existingProfile = $filter('filter')($scope.profilesCopy, { Name: $scope.profile.name }, true)[0];

                    if (existingProfile && existingProfile.id !== $scope.profile.id)
                        isValid = false;
                }

                if (!isValid)
                    $scope.profileForm['name'].$setValidity('unique', false);

                return isValid;
            }

            $scope.submit = function () {
                validate();

                // if ($scope.profileForm.$valid) {
                    $scope.profileSubmit = true;
                    var result = null;

                    $scope.profile.start_page = $scope.profile.PageStart.valueLower;
                    var setPage = $filter('filter')($scope.startPageList, { value: $scope.profile.PageStart.value }, true)[0];

                    $scope.profile[setPage.value] = true;

                    if ($scope.profile.startpage === "newsfeed") {
                        var startPageNewsfeedControl = $filter('filter')($scope.profile.Permissions, { Type: 3 }, true)[0];
                        startPageNewsfeedControl.Read = true;
                    }

                    if ($scope.profile.parent_id) {
                        $scope.profile.parent_id = $scope.profile.parent_id.id;
                    } else {
                        $scope.profile.parent_id = 0;
                    }

                    if (!$scope.profile.id) {
                        result = ProfilesService.create($scope.profile);
                    }
                    else {
                        result = ProfilesService.update($scope.profile);
                    }

                    result.then(function () {
                        $scope.profileSubmit = false;
                        $state.go('app.setup.profiles');
                        ngToast.create({ content: $filter('translate')('Setup.Profiles.SubmitSuccess'), className: 'success' });

                        ProfilesService.getAllBasic()
                            .then(function (profilesBasic) {
                                $rootScope.profiles = profilesBasic.data;
                            });
                    }).catch(function () {
                        $scope.profileSubmit = false;
                    });
                // }
            };

            function getProfiles() {
                ProfilesService.getAll()
                    .then(function (response) {
                        $scope.profiles = response.data;
                        $scope.loading = false;
                    })
                    .catch(function () {
                        $scope.loading = false;
                    });
            }

            getProfiles();

            $scope.showDeleteForm = function (profile) {
                $scope.selectedProfile = profile;
                var transferProfiles = angular.copy($scope.profiles);
                var deleteProfile = angular.copy(profile);
                var index = helper.arrayObjectIndexOf(transferProfiles, deleteProfile);
                transferProfiles.splice(index, 1);
                $scope.transferProfiles = transferProfiles;

                $scope['deletePopover' + profile.id] = $scope['deletePopover' + profile.id] || $popover(angular.element(document.getElementById('deleteButton' + profile.id)), {
                        templateUrl: 'view/setup/profiles/profileDelete.html',
                        placement: 'left',
                        scope: $scope,
                        autoClose: true,
                        show: true
                    });
            };

            $scope.delete = function (transferProfileId) {
                if (!transferProfileId)
                    transferProfileId = $scope.transferProfiles[0].id;

                $scope.profileDeleting = true;

                ProfilesService.remove($scope.selectedProfile.id, transferProfileId, $rootScope.workgroup.tenant_id)
                    .then(function () {
                        $scope.profileDeleting = false;
                        ngToast.create({ content: $filter('translate')('Setup.Profiles.DeleteSuccess'), className: 'success' });
                        $scope['deletePopover' + $scope.selectedProfile.id].hide();

                        getProfiles();
                    })
                    .catch(function () {
                        $scope.profileDeleting = false;
                    });
            };

            $scope.showFormModal = function (template) {
                $scope.profileFormModal = $scope.profileFormModal || $modal({
                        scope: $scope,
                        templateUrl: 'view/app/authorization/profiles/profileForm.html',
                        animation: 'am-fade-and-slide-right',
                        backdrop: 'static',
                        show: false
                    });
                $scope.profileFormModal.$promise.then(function () {
                    $scope.profileFormModal.show();
                });
            };

        }
    ]);