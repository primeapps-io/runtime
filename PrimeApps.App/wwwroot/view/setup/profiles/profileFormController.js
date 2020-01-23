'use strict';

angular.module('primeapps')

    .controller('ProfileFormController', ['$rootScope', '$scope', '$location', '$state', '$filter', 'ngToast', 'ProfileService',
        function ($rootScope, $scope, $location, $state, $filter, ngToast, ProfileService) {
            $scope.loading = true;
            var id = parseInt($location.search().id);
            var clone = parseInt($location.search().clone);

            $scope.moduleLead = $filter('filter')($rootScope.modules, { name: 'leads' }, true)[0];
            $scope.moduleIzinler = $filter('filter')($rootScope.modules, { name: 'izinler' }, true)[0];
            $scope.moduleRehber = $filter('filter')($rootScope.modules, { name: 'rehber' }, true)[0];

            $scope.startPageList = [
                {
                    "value": "Dashboard",
                    "valueLower": "dashboard",
                    "name": $filter('translate')('Layout.Menu.Dashboard')
                }
                // ,
                // {
                //     "value": "Newsfeed",
                //     "valueLower": "newsfeed",
                //     "name": $filter('translate')('Layout.Menu.Newsfeed')
                // },
                // {
                //     "value": "Tasks",
                //     "valueLower": "tasks",
                //     "name": $filter('translate')('Layout.Menu.Tasks')
                // },
                // {
                //     "value": "Calendar",
                //     "valueLower": "calendar",
                //     "name": $filter('translate')('Layout.Menu.Calendar')
                // }
            ];

            if ($scope.moduleRehber) {
                $scope.startPageList.push({
                    "value": "Home",
                    "valueLower": "home",
                    "name": $filter('translate')('Layout.Menu.Homepage')
                });
            }

            function getProfile() {
                ProfileService.getAll()
                    .then(function (response) {
                        $scope.profiles = ProfileService.getProfiles(response.data, $rootScope.workgroup.tenant_id, false);
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
                            $scope.profile.tenant_id = $rootScope.workgroup.tenant_id;

                            if (clone) {
                                var profile = $filter('filter')($scope.profiles, { id: clone }, true)[0];
                                $scope.profile = profile;

                                delete $scope.profile.name;
                                delete $scope.profile.user_ids;
                                delete $scope.profile.description;
                                delete $scope.profile.is_persistent;
                                delete $scope.profile.CreatedBy;
                                delete $scope.profile.id;
                                delete $scope.profile.system_type;

                                var setPageStart = $filter('filter')($scope.startPageList, { valueLower: $scope.profile.start_page }, true)[0];
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
                                $scope.profile.close_smtp_settings = false;
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

                if ($scope.profileForm.$valid) {
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

                    //TODO new ui change.
                    $scope.profile.name_tr = $scope.profile.name;
                    $scope.profile.name_en = $scope.profile.name;
                    $scope.profile.description_tr = $scope.profile.description;
                    $scope.profile.description_en = $scope.profile.description;

                    if (!$scope.profile.id) {
                        result = ProfileService.create($scope.profile);
                    }
                    else {
                        result = ProfileService.update($scope.profile);
                    }

                    result.then(function () {
                        $scope.profileSubmit = false;
                        $state.go('app.setup.profiles');
                        ngToast.create({ content: $filter('translate')('Setup.Profiles.SubmitSuccess'), className: 'success' });

                        ProfileService.getAllBasic()
                            .then(function (profilesBasic) {
                                $rootScope.profiles = profilesBasic.data;
                            });
                    }).catch(function () {
                        $scope.profileSubmit = false;
                    });
                }
            };
        }
    ]);