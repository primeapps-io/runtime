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
                ProfileService.getAll()
                    .then(function (response) {
                        $scope.profiles = ProfileService.getProfiles(response.data, $rootScope.workgroup.instanceID, false);
                        $scope.profilesCopy = angular.copy($scope.profiles);

                        if (id) {
                            $scope.profile = $filter('filter')($scope.profiles, { ID: id }, true)[0];

                            //Update
                            var setPageStart = $filter('filter')($scope.startPageList, { valueLower: $scope.profile.StartPage }, true)[0];
                            $scope.profile.PageStart = setPageStart;


                        }
                        else {
                            $scope.profile = {};
                            $scope.profile.InstanceID = $rootScope.workgroup.instanceID;

                            if (clone) {
                                var profile = $filter('filter')($scope.profiles, { ID: clone }, true)[0];
                                $scope.profile = profile;
                                delete  $scope.profile.Name;
                                delete  $scope.profile.UserIDs;
                                delete  $scope.profile.Description;
                                delete  $scope.profile.IsPersistent;
                                delete  $scope.profile.CreatedBy;
                                delete  $scope.profile.ID;
                                var setPageStart = $filter('filter')($scope.startPageList, {valueLower: $scope.profile.StartPage}, true)[0];
                                $scope.profile.PageStart = setPageStart;
                            }
                            else {
                                $scope.profile.HasAdminRights = false;
                                $scope.profile.IsPersistent = false;
                                $scope.profile.BusinessIntelligence = false;
                                $scope.profile.SendEmail = false;
                                $scope.profile.SendSMS = false;
                                $scope.profile.ExportData = false;
                                $scope.profile.ImportData = false;
                                $scope.profile.WordPdfDownload = false;
                                $scope.profile.LeadConvert = false;
                                $scope.profile.DocumentSearch = false;
                                $scope.profile.Tasks = false;
                                $scope.profile.Calendar = false;
                                $scope.profile.Newsfeed = false;
                                $scope.profile.Report = false;
                                $scope.profile.Dashboard = true;
                                $scope.profile.Home = false;
                                $scope.profile.CollectiveAnnualLeave = false;
                                $scope.profile.Permissions = $filter('filter')($scope.profiles, { IsPersistent: true, HasAdminRights: true })[0].Permissions;
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

            };


            function validate() {
                var isValid = true;
                var existingProfile = null;

                if (!$scope.profile.ID) {
                    existingProfile = $filter('filter')($scope.profilesCopy, { Name: $scope.profile.Name }, true)[0];

                    if (existingProfile)
                        isValid = false;
                }
                else {
                    existingProfile = $filter('filter')($scope.profilesCopy, { Name: $scope.profile.Name }, true)[0];

                    if (existingProfile && existingProfile.ID !== $scope.profile.ID)
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

                    $scope.profile.startpage = $scope.profile.PageStart.valueLower;
                    var setPage = $filter('filter')($scope.startPageList, { value: $scope.profile.PageStart.value }, true)[0];

                    $scope.profile[setPage.value] = true;

                    if (!$scope.profile.ID) {
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