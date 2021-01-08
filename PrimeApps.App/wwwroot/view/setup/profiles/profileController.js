'use strict';

angular.module('primeapps')

    .controller('ProfileController', ['$rootScope', '$scope', '$filter', 'helper', 'ProfileService', '$localStorage', '$mdDialog', 'mdToast', '$state',
        function ($rootScope, $scope, $filter, helper, ProfileService, $localStorage, $mdDialog, mdToast, $state) {
            $scope.hasAdminRight = $filter('filter')($rootScope.profiles, { id: $rootScope.user.profile.id }, true)[0].has_admin_rights;

            if (!$scope.hasAdminRight) {
                if (!helper.hasCustomProfilePermission('profile')) {
                    mdToast.error($filter('translate')('Common.Forbidden'));
                    $state.go('app.dashboard');
                }
            }

            $rootScope.breadcrumblist = [
                {
                    title: $filter('translate')('Layout.Menu.Dashboard'),
                    link: "#/app/dashboard"
                },
                {
                    title: $filter('translate')('Setup.Nav.AccessControl'),
                    link: '#/app/setup/profiles'
                },
                {
                    title: $filter('translate')('Setup.Nav.Tabs.Profiles')
                }
            ];

            $scope.loading = true;
            $scope.profileForForm = { id: null, clone: null };

            $scope.showDeleteForm = function (profile) {
                if (!profile) {
                    mdToast.warning($filter('translate')('Common.NotFoundRecord'));
                    return;
                }
                $scope.profileDeleteLabel = $filter('translate')('Setup.Profiles.ProfileDeleteLabel', { name: profile['name_' + $rootScope.language] });
                ProfileService.getAll()
                    .then(function (response) {
                        $rootScope.processLanguages(response.data);
                        $scope.profiles = ProfileService.getProfiles(response.data, $rootScope.workgroup.tenant_id, false);

                        $scope.selectedProfile = $filter('filter')($scope.profiles, { id: profile.id }, true)[0];
                        var transferProfiles = angular.copy($scope.profiles);
                        var deleteProfile = angular.copy($scope.selectedProfile);
                        var index = 0;

                        for (var i = 0; i < transferProfiles.length; i++) {
                            if (transferProfiles[i].id === deleteProfile.id) {
                                index = i;
                                break;
                            }
                        }

                        transferProfiles.splice(index, 1);
                        $scope.transferProfiles = transferProfiles;

                        var parentEl = angular.element(document.body);
                        $mdDialog.show({
                            parent: parentEl,
                            templateUrl: 'view/setup/profiles/profileDelete.html',
                            clickOutsideToClose: true,
                            scope: $scope,
                            preserveScope: true

                        });
                    });
            };

            $scope.delete = function (transferProfile) {
                if (!transferProfile)
                    transferProfile = $scope.transferProfiles[0];

                $scope.profileDeleting = true;

                ProfileService.remove($scope.selectedProfile.id, transferProfile.id, $rootScope.workgroup.tenant_id)
                    .then(function () {
                        $scope.profileDeleting = false;
                        mdToast.success($filter('translate')('Setup.Profiles.DeleteSuccess'));
                        $scope.close();
                        $scope.grid.dataSource.read();
                        // getProfiles();
                    })
                    .catch(function () {
                        $scope.profileDeleting = false;
                    });
            };

            $scope.showSideModal = function (id, type) {
                $scope.profileForForm = {};
                $scope.profileForForm.id = id;
                $scope.profileForForm.clone = type;

                $scope.moduleLead = $filter('filter')($rootScope.modules, { name: 'leads' }, true)[0];
                $scope.moduleIzinler = $filter('filter')($rootScope.modules, { name: 'izinler' }, true)[0];
                $scope.moduleRehber = $filter('filter')($rootScope.modules, { name: 'rehber' }, true)[0];


                function getProfile() {
                    $scope.formLoading = true;
                    ProfileService.getAll()
                        .then(function (response) {
                            $scope.profiles = ProfileService.getProfiles(response.data, $rootScope.workgroup.tenant_id, false);
                            $rootScope.processLanguages($scope.profiles);

                            $scope.profilesCopy = angular.copy($scope.profiles);
                            if (id) {
                                $scope.profile = $filter('filter')($scope.profiles, { id: id }, true)[0];
                                if ($scope.profile.parent_id !== 0) {
                                    $scope.profile.parent_id = $filter('filter')($scope.profiles, { id: $scope.profile.parent_id }, true)[0];
                                }
                            }
                            else {
                                $scope.profile = {
                                    languages: {}
                                };

                                $scope.profile.languages[$rootScope.globalization.Label] = {
                                    name: '',
                                    description: ''
                                };
                                $scope.profile.tenant_id = $rootScope.workgroup.tenant_id;

                                if ($scope.profileForForm.clone) {
                                    var profile = $filter('filter')($scope.profiles, { id: $scope.profileForForm.clone }, true)[0];
                                    $scope.profile = profile;

                                    delete $scope.profile.name_tr;
                                    delete $scope.profile.name_en;
                                    delete $scope.profile.user_ids;
                                    delete $scope.profile.description_tr;
                                    delete $scope.profile.description_en;
                                    delete $scope.profile.is_persistent;
                                    delete $scope.profile.created_by_id;
                                    delete $scope.profile.id;
                                    delete $scope.profile.system_type;

                                   // var setPageStart = $filter('filter')($scope.startPageList, { valueLower: $scope.profile.start_page }, true)[0];
                                    //$scope.profile.PageStart = setPageStart;
                                    $scope.profile.parent_id = $filter('filter')($scope.profiles, { id: profile.parent_id }, true)[0];
                                }
                                else {
                                    $scope.profile.has_admin_rights = false;
                                    $scope.profile.is_persistent = false;
                                    $scope.profile.send_email = false;
                                    $scope.profile.send_sms = false;
                                    $scope.profile.export_data = false;
                                    $scope.profile.import_data = false;
                                    $scope.profile.word_pdf_download = false;
                                    $scope.profile.smtp_settings = false;
                                    $scope.profile.change_email = false;
                                    $scope.profile.dashboard = true;
                                    $scope.profile.permissions = $filter('filter')($scope.profiles, { is_persistent: true, has_admin_rights: true })[0].permissions;

                                }
                            }

                            $scope.formLoading = false;
                        })
                        .catch(function () {
                            $scope.formLoading = false;
                        });
                }

                getProfile();


                function validate() {
                    var isValid = true;
                    var existingProfile = null;

                    //if (!$scope.profile.id) {
                    //    existingProfile = $filter('filter')($scope.profilesCopy, { Name: $scope.profile['name_' + $scope.language] }, true)[0];

                    //    if (existingProfile)
                    //        isValid = false;
                    //}
                    //else {
                    //    existingProfile = $filter('filter')($scope.profilesCopy, { Name: $scope.profile['name_' + $scope.language] }, true)[0];

                    //    if (existingProfile && existingProfile.id !== $scope.profile.id)
                    //        isValid = false;
                    //}

                    //if (!isValid)
                    //    $scope.profileForm['name'].$setValidity('unique', false);

                    return isValid;
                }

                $scope.submit = function (profileForm) {
                    validate();

                    if (profileForm.$valid) {
                        $scope.profileSubmit = true;
                        $scope.isProfileCreated = false;
                        var result = null;

                        if ($scope.profile.startpage === "newsfeed") {
                            var startPageNewsfeedControl = $filter('filter')($scope.profile.Permissions, { Type: 3 }, true)[0];
                            startPageNewsfeedControl.Read = true;
                        }

                        if ($scope.profile.parent_id) {
                            $scope.profile.parent_id = $scope.profile.parent_id.id;
                        } else {
                            $scope.profile.parent_id = 0;
                        }

                        var profileModel = angular.copy($scope.profile);
                        $rootScope.languageStringify(profileModel);

                        if (!profileModel.id) {
                            result = ProfileService.create(profileModel);
                            $scope.isProfileCreated = true;
                        }
                        else {
                            result = ProfileService.update(profileModel);
                        }

                        result.then(function () {
                            $scope.profileSubmit = false;
                            $rootScope.closeSide('sideModal');
                            // $state.go('app.setup.profiles');
                            $scope.grid.dataSource.read();
                            ProfileService.getAll().then(function (res) {

                                $rootScope.processLanguages(res.data);
                                $rootScope.profiles = res.data;
                            });

                            if ($rootScope.preview && $scope.isProfileCreated)
                                mdToast.success($filter('translate')('Setup.Profiles.SubmitSuccessForPreview'));
                            else if(!$rootScope.preview && $scope.isProfileCreated)
                                mdToast.success($filter('translate')('Setup.Profiles.SubmitSuccess'));
                            else//update msg
                                mdToast.success($filter('translate')('Setup.Profiles.SubmitSuccessUpdated'));

                        }).catch(function () {
                            $scope.profileSubmit = false;
                            $rootScope.closeSide('sideModal');
                            $scope.grid.dataSource.read();
                        });
                    } else {
                        mdToast.error($filter('translate')('Module.RequiredError'));
                    }
                };


                $scope.pageStartOptions = {
                    dataSource: $scope.startPageList,
                    dataTextField: "value",
                    dataValueField: "valueLower"
                };

                $rootScope.buildToggler('sideModal', 'view/setup/profiles/profileForm.html');

                $scope.formLoading = true;
            };

            $scope.close = function () {
                $mdDialog.hide();
            };

            //For Kendo UI
            $scope.goUrl2 = function (item) {
                var selection = window.getSelection();
                if (selection.toString().length === 0) {
                    $scope.showSideModal(item.id, null);
                }
            };

            var optionalMenu = '<md-menu md-position-mode="target-right target">' +
                ' <md-button class="md-icon-button" aria-label=" " ng-click="$mdMenu.open()"> <i class="fas fa-ellipsis-v"></i></md-button>' +
                '<md-menu-content width="2" class="md-dense">' +
                '<md-menu-item>' +
                '<md-button ng-disabled="dataItem.is_persistent || dataItem.system_type === \'system\'" ng-click="showSideModal(null, dataItem.id)">' +
                '<i class="fas fa-copy"></i> ' + $filter('translate')('Common.Copy') + ' <span></span>' +
                '</md-button>' +
                '</md-menu-item>' +
                '<md-menu-item>' +
                '<md-button id="deleteButton{{dataItem.id}}" ng-disabled="dataItem.is_persistent || dataItem.system_type === \'system\'" ng-click="showDeleteForm(dataItem)">' +
                '<i class="fas fa-trash"></i> <span> ' + $filter('translate')('Common.Delete') + '</span>' +
                '</md-button>' +
                '</md-menu-item>' +
                '</md-menu-content>' +
                '</md-menu>';


            function generateRowTemplate(e) {
                return '<td class="hide-on-m2"><span>' +  $rootScope.getLanguageValue(e.languages, 'name') + '</span></td>'
                    + '<td class="hide-on-m2"><span>' + $rootScope.getLanguageValue(e.languages, 'description') + '</span></td>'
                    + '<td class="show-on-m2">'
                    + '<div> <strong>' +  $rootScope.getLanguageValue(e.languages, 'name') + '</strong></div>'
                    + '<div>' + $rootScope.getLanguageValue(e.languages, 'description') + '</div></td>'
                    + '<td ng-click="$event.stopPropagation();"><span>' + optionalMenu + '</span></td>';
            }

            var createGrid = function () {

                $scope.profileGridOptions = {
                    dataSource: {
                        type: "odata-v4",
                        page: 1,
                        pageSize: 10,
                        serverPaging: true,
                        serverFiltering: true,
                        serverSorting: true,
                        transport: {
                            read: {
                                url: "/api/profile/find",
                                type: 'GET',
                                dataType: "json",
                                beforeSend: $rootScope.beforeSend()
                            }
                        },
                        requestEnd: function (e) {
                            $rootScope.processLanguages(e.response.items || []);
                        },
                        schema: {
                            data: "items",
                            total: "count",
                            model: {
                                id: "id",
                                fields: {
                                    name: { type: "string" },
                                    description: { type: "string" }
                                }
                            }
                        }
                    },
                    scrollable: false,
                    persistSelection: true,
                    sortable: true,
                    noRecords: true,
                    pageable: {
                        refresh: true,
                        pageSize: 10,
                        pageSizes: [10, 25, 50, 100],
                        buttonCount: 5,
                        info: true,
                    },
                    filterable: true,
                    filter: function (e) {
                        if (e.filter) {
                            for (var i = 0; i < e.filter.filters.length; i++) {
                                e.filter.filters[i].ignoreCase = true;
                            }
                        }
                    },
                    rowTemplate: function (e) {
                        return '<tr ng-click="goUrl2(dataItem)">' + generateRowTemplate(e) + '</tr>';
                    },
                    altRowTemplate: function (e) {
                        return '<tr class="k-alt" ng-click="goUrl2(dataItem)">' + generateRowTemplate(e) + '</tr>';
                    },
                    columns: [
                        {
                            media: "(min-width: 575px)",
                            field: "Name" + $scope.language,
                            title: $filter('translate')('Setup.Profiles.ProfileName'),
                        },
                        {
                            media: "(min-width: 575px)",
                            field: "Description" + $rootScope.user.language,
                            title: $filter('translate')('Setup.Profiles.ProfileDescription'),
                        },

                        {
                            title: $filter('translate')('Setup.Nav.Tabs.Profiles'),
                            media: "(max-width: 575px)"
                        },
                        {
                            field: "",
                            title: "",
                            filterable: false,
                            width: "40px",
                        }]

                };
            };

            angular.element(document).ready(function () {
                createGrid();
                $scope.loading = false;
            });

            $scope.transferProfileOptions = {
                dataSource: {
                    transport: {
                        read: function (o) {
                            o.success($scope.transferProfiles)
                        }
                    }
                },
                autoBind: false,
                dataTextField: 'languages.' + $rootScope.globalization.Label + '.name',
                dataValueField: "id",
            };
            //For Kendo UI
        }
    ]);
