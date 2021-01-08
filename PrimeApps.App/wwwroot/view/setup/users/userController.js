'use strict';

angular.module('primeapps')

    .controller('UserController', ['$rootScope', '$cookies', 'AuthService', '$scope', '$filter', '$state', 'guidEmpty', 'helper', 'UserService', 'WorkgroupService', 'AppService', 'ProfileService', 'RoleService', '$q', 'officeHelper', '$localStorage', 'mdToast', '$mdDialog',
        function ($rootScope, $cookies, AuthService, $scope, $filter, $state, guidEmpty, helper, UserService, WorkgroupService, AppService, ProfileService, RoleService, $q, officeHelper, $localStorage, mdToast, $mdDialog) {
            $scope.loading = true;
            AppService.checkPermission().then(function (res) {

                if (res && res.data) {
                    var profile = JSON.parse(res.data["profile"]);
                    var customProfilePermissions = undefined;
                    if (res.data["customProfilePermissions"])
                        customProfilePermissions = JSON.parse(res.data["customProfilePermissions"]);

                    if (!profile.HasAdminRights) {
                        var usersIsExist = undefined;
                        if (customProfilePermissions)
                            usersIsExist = customProfilePermissions.permissions.indexOf('users') > -1;

                        if (!usersIsExist) {
                            $state.go('app.setup.usergroups');
                        }
                    }
                }

                $rootScope.breadcrumblist = [
                    {
                        title: $filter('translate')('Layout.Menu.Dashboard'),
                        link: "#/app/dashboard"
                    },
                    {
                        title: $filter('translate')('Setup.Nav.Users'),
                        link: '#/app/setup/users'
                    },
                    {
                        title: $filter('translate')('Setup.Nav.Tabs.Users')
                    }
                ];


                $scope.isOfficeConnected = false;
                //user add button popover controller
                $scope.officeUserReady = false;

                $scope.officeUsers = null;
                $scope.selectedOfficeUser = {};
                $scope.addUserModel = {};
                $scope.addUserForm = true;
                $scope.submitting = false;
                $scope.hideSendEmailToUser = false;
                $scope.selectedRows = [];
                $scope.selectedUsers = [];
                $scope.isAllSelected = false;
                $scope.field = null;
                $scope.transferValue = null;

                $scope.officeUserChanged = function (selectedOfficeUser) {
                    $scope.addUserModel.email = selectedOfficeUser.email;
                    $scope.addUserModel.phone = selectedOfficeUser.phone;
                    $scope.addUserModel.full_name = selectedOfficeUser.fullName;
                    $scope.addUserModel.first_bame = selectedOfficeUser.name;
                    $scope.addUserModel.last_name = selectedOfficeUser.surname;
                };

                $scope.changeUserIsActive = function () {
                    $scope.addUserModel.is_active = !$scope.addUserModel.is_active;
                };

                $scope.sendResetPassword = function () {
                    var appId = $cookies.get('app_id');
                    var tenantId = $cookies.get('tenant_id');

                    AuthService.forgotPassword($scope.addUserModel.email, appId, tenantId)
                        .then(function (response) {
                            mdToast.success({
                                content: $filter('translate')('Setup.Office.SendEmailSuccess'),
                                timeout: 5000
                            });
                        }).catch(function (response) {
                            mdToast.error({
                                content: $filter('translate')('Setup.Office.SendEmailError'),
                                timeout: 5000
                            });
                        });
                };

                $scope.sendOfficeUserPassword = function () {
                    var app = 'crm';
                    switch ($rootScope.user.appId) {
                        case 2:
                            app = 'kobi';
                            break;
                        case 3:
                            app = 'asistan';
                            break;
                        case 4:
                            app = 'ik';
                            break;

                        case 5:
                            app = 'cagri';
                            break;
                        default:
                            app = 'crm';
                    }


                    $scope.submitting = true;
                    // var emailBody = $filter('translate')('Setup.Office.EmailNotification.Hello') + " " + $scope.addedUser.fullName + "<br />" + $filter('translate')('Setup.Office.EmailNotification.Created') + "<br />" + $filter('translate')('Setup.Office.EmailNotification.Email') + $scope.addedUser.email + "<br />" + $filter('translate')('Setup.Office.EmailNotification.Password') + $scope.userPassword;
                    var requestMail = {};
                    requestMail.full_name = $scope.addedUser.first_name + ' ' + $scope.addedUser.last_name;
                    requestMail.password = $scope.userPassword;
                    requestMail.email = $scope.addedUser.email;

                    UserService.sendPasswordToOfficeUser(requestMail)
                        .then(function (response) {
                            $scope.closeUserInfoPopover();
                            mdToast.success({
                                content: $filter('translate')('Setup.Office.SendEmailSuccess'),
                                timeout: 5000
                            });
                        }).catch(function (response) {
                            $scope.closeUserInfoPopover();
                            mdToast.error({
                                content: $filter('translate')('Setup.Office.SendEmailError'),
                                timeout: 5000
                            });
                        });

                };

                $scope.closeUserInfoPopover = function closePasswordPopup() {
                    $scope.submitting = false;
                    $scope.addUserForm = true;
                    $scope.userPassword = null;
                    $scope.hideSendEmailToUser = false;

                    //if ($scope.createOfficePopover) {
                    //    $scope.createOfficePopover.hide();
                    //} else if ($scope.createPopover) {
                    //    $scope.createPopover.hide();
                    //}
                    $rootScope.closeSide('sideModal');
                    $scope.addedUser = {};
                    $scope.grid.dataSource.read();
                };


                function getUsers() {
                    var promises = [];

                    //promises.push(UserService.getAllUser());
                    promises.push(ProfileService.getAll());
                    promises.push(RoleService.getAll());
                    //var profiles = ProfileService.getAll();
                    //$scope.roles = RoleService.getAll();
                    $q.all(promises).then(function (data) {
                        // var users = data[0].data,
                        var responseProfiles = data[0].data;
                        $scope.roles = angular.copy(data[1].data);
                        $rootScope.processLanguages($scope.roles);
                        //        license = data[3].data;

                        //    //var workgroup = $filter('filter')($rootScope.workgroups, { tenant_id: $rootScope.user.tenant_id }, true)[0];
                        //    $rootScope.workgroup.users = users;

                        $scope.profiles = ProfileService.getProfiles(responseProfiles, $rootScope.workgroup.tenant_id, true);
                        $rootScope.processLanguages($scope.profiles);
                        if (!$rootScope.user.profile.has_admin_rights)
                            $scope.profiles = $filter('filter')($scope.profiles, { has_admin_rights: !true }, true);

                        //    $scope.users = UserService.getUsers(users, $scope.profiles, $scope.roles);
                        //    $scope.licensesBought = license.total || 0;
                        //    $scope.licensesUsed = license.used || 0;
                        //    $scope.licenseAvailable = $scope.licensesBought - $scope.licensesUsed;
                        // $scope.loading = false;
                        createGrid();
                    });

                }

                getUsers();

                $scope.showCreateForm = function () {
                    $scope.addedUser = {};
                    $scope.addUserModel = {};
                    $scope.addUserForm = true;
                    $scope.addNewUser = true;
                    $scope.addUserModel.is_active = true;
                    $scope.showSideModal();
                };

                $scope.showOfficeUserCreateForm = function () {
                    $scope.addedUser = {};
                    $scope.addUserForm = true;
                    $scope.createOfficePopover = $scope.createOfficePopover || $popover(angular.element(document.getElementById('officeCreateButton')), {
                        templateUrl: 'view/setup/users/officeUserCreate.html',
                        placement: 'bottom-right',
                        scope: $scope,
                        autoClose: true,
                        show: true
                    });
                };

                $scope.addUser = function (inviteModel) {
                    if (!inviteModel || !inviteModel.email || !inviteModel.profile || !inviteModel.role || !inviteModel.first_name || !inviteModel.last_name) {
                        mdToast.error($filter('translate')('Module.RequiredError'));
                        return;
                    }

                    $scope.loadingModal = true;
                    var user = {};

                    if (inviteModel.id) { //For edit mode
                        $scope.edit(inviteModel);
                        if(inviteModel.phone.toString() !== $scope.editModelState.phone.toString())
                            UserService.updateUserPhone(inviteModel).then(function () {
                                UserService.getAllUser().then(function (users) {
                                    $rootScope.users = users.data;
                                });
                            })
                        return;
                    }

                    //if (!inviteModel.fullName) {
                    //    user.fullName = inviteModel.first_name + " " + inviteModel.last_name;
                    //}

                    $scope.userInviting = true;
                    user.firstName = inviteModel.first_name;
                    user.LastName = inviteModel.last_name;
                    user.email = inviteModel.email;
                    user.profileId = inviteModel.profile.id;
                    //user.roleId = inviteModel.role.id;
                    user.phone = inviteModel.phone;

                    $scope.addedUser = angular.copy(inviteModel);
                    user = helper.SnakeToCamel(user);

                    UserService.addUser(user)
                        .then(function (response) {
                            if (response.data) {

                                getUsers();

                                $scope.userInviting = false;
                                mdToast.success($filter('translate')('Setup.Users.NewUserSuccess'));
                                $scope.grid.dataSource.read();
                                $scope.loadingModal = false;
                                $scope.userPassword = response.data.password;
                                $scope.hideSendEmailToUser = response.data.password.contains("***");
                                $scope.addUserForm = false;
                                $scope.addUserModel = {};
                            }
                            UserService.getAllUser().then(function (users) {
                                $rootScope.users = users.data;
                            });
                        })
                        .catch(function (response) {
                            //$scope.inviteOfficeModel = {};
                            $scope.userInviting = false;
                            $scope.loadingModal = false;
                            if (response.status === 409) {
                                mdToast.warning($filter('translate')('Setup.Users.NewUserError'));
                            }
                        });
                };

                $scope['numberOptions'] = {
                    format: "#",
                    decimals: 0,
                    spinners : false
                };

                $scope.showEditForm = function (user) {
                    $scope.addNewUser = false;
                    $scope.addUserForm = true;
                    $scope.loadingModal = true;
                    $scope.selectedUser = angular.copy(user);
                    $scope.addUserModel = user;
                    $scope.addUserModel.pictureData = user.picture ? blobUrl + '/' + angular.copy(user.picture) : null;
                    $scope.editModel = {};
                    $scope.editModel.profile = user.profile.id;
                   // $scope.editModel.role = user.role.id;
                    $scope.editModel.phone = user.phone;
                    $scope.editModel.activeDirectoryEmail = user.activeDirectoryEmail;
                    $scope.userHaveActiveDirectoryEmail = user.activeDirectoryEmail !== null && user.activeDirectoryEmail !== "null" && user.activeDirectoryEmail !== '';
                    $scope.editModelState = angular.copy($scope.editModel);
                    $scope.showSideModal();

                };

                $scope.edit = function (user) {
                    $scope.loadingModal = true;
                    $scope.editModel = user;

                    if ($scope.editModel.profile === $scope.editModelState.profile &&
                        $scope.editModel.role === $scope.editModelState.role &&
                        $scope.editModel.activeDirectoryEmail === $scope.editModelState.activeDirectoryEmail) {
                        $scope.loadingModal = false;

                        //$scope.popover.hide();
                        //if need on alert for "no changed user" uncomment line
                        //ngToast.create({ content: $filter('translate')('Setup.Users.EditSuccess'), className: 'success' });
                        return;
                    }

                    var success = function () {
                        getUsers();
                        $scope.loadingModal = false;
                        $scope.closeUserInfoPopover();
                        mdToast.success($filter('translate')('Setup.Users.EditSuccess'));
                    };

                    var updateActiveDirectoryEmail = function () {
                        UserService.updateActiveDirectoryEmail($scope.selectedUser.id, $scope.editModel.activeDirectoryEmail)
                            .then(function () {

                                success();
                            })
                            .catch(function (response) {
                                $scope.loadingModal = false;
                                if (response.status === 409) {
                                    mdToast.warning($filter('translate')('Setup.Users.NewUserError'));
                                }
                            });
                    };
                    UserService.updateUserStatus({ email: $scope.editModel.email, is_active: $scope.addUserModel.is_active });
                    ProfileService.changeUserProfile($scope.selectedUser.id, $rootScope.workgroup.tenant_id, $scope.editModel.profile.id)
                        .then(function onSuccess() {
                            RoleService.updateUserRole($scope.selectedUser.id, $scope.editModel.role.id)
                                .then(function onSuccess() {
                                    if (($scope.editModel.activeDirectoryEmail !== null || $scope.editModel.activeDirectoryEmail !== "") &&
                                        $scope.editModel.activeDirectoryEmail !== $scope.editModelState.activeDirectoryEmail) {
                                        updateActiveDirectoryEmail();
                                    } else {
                                        success();
                                    }
                                })
                                .catch(function onError() {
                                    $scope.loadingModal = false;
                                    $scope.userEditing = false;
                                });
                        })
                        .catch(function onError() {
                            $scope.loadingModal = false;
                            $scope.userEditing = false;
                        });
                };

                $scope.dismiss = function (id) {
                    if (!id)
                        return;

                    var user = $filter('filter')($scope.users, { id: id }, true)[0];

                    UserService.dismiss(user, $rootScope.workgroup.tenant_id)
                        .then(function onSuccess() {
                            $scope.closeUserInfoPopover();
                            mdToast.success($filter('translate')('Setup.Users.DismissSuccess'));

                            AppService.getMyAccount(true);

                            //LicenseService.getUserLicenseStatus().then(function onSuccess(license) {
                            //    $scope.licensesBought = license.data.total || 0;
                            //    $scope.licensesUsed = license.data.used || 0;
                            //    $scope.licenseAvailable = $scope.licensesBought - $scope.licensesUsed;
                            //});

                        })
                        .catch(function onError() {
                        });
                };

                $scope.showConfirm = function (item, ev) {
                    var confirm = $mdDialog.confirm($filter('translate')('Setup.Users.UserDeleteMessage'))
                        .title($filter('translate')('Common.AreYouSure'))
                        .targetEvent(ev)
                        .ok($filter('translate')('Common.Yes'))
                        .cancel($filter('translate')('Common.No'));

                    $mdDialog.show(confirm).then(function () {
                        $scope.dismiss(item);
                    }, function () {

                    });

                };

                //$scope.gotoLicencePage = function () {
                //    var menuItem = $filter('filter')($scope.$parent.menuItems, { link: '#/app/setup/license' })[0];
                //    $scope.$parent.selectMenuItem(menuItem);

                //    $state.go('app.setup.license');
                //};


                $scope.showSideModal = function () {
                    $rootScope.sideLoad = false;
                    $rootScope.buildToggler('sideModal', 'view/setup/users/userSideModal.html');
                    $scope.loadingModal = false;
                };

                $scope.copySuccess = function () {
                    mdToast.success($filter('translate')('Setup.Users.PasswordCopySuccess'));
                };

                $scope.showBulkUpdate = function (ev) {
                    var parentEl = angular.element(document.body);

                    $mdDialog.show({
                        parent: parentEl,
                        templateUrl: 'view/setup/users/userBulkUpdateModal.html',
                        clickOutsideToClose: false,
                        targetEvent: ev,
                        scope: $scope,
                        preserveScope: true

                    });
                };

                $scope.updateSelected = function (form) {
                    form.$submitted = true;

                    if (form.$invalid || !$scope.field || !$scope.transferValue) {
                        mdToast.error($filter('translate')('Module.RequiredError'));
                        return;
                    }

                    if (!$scope.field || $scope.selectedRows.length < 1 || !$scope.transferValue)
                        return;

                    if ($scope.field.value === 'profile') {
                        ProfileService.changeUsersProfile($scope.selectedRows, $rootScope.workgroup.tenant_id, $scope.transferValue.id)
                            .then(function onSuccess() {
                                $scope.closeLightBox();
                                getUsers();
                                $scope.grid.dataSource.read();
                                mdToast.success($filter('translate')('Setup.Users.EditsSuccess'));
                            });
                    }

                    if ($scope.field.value === 'role') {
                        RoleService.roleChangeUsers($scope.selectedRows, $scope.transferValue.id)
                            .then(function onSuccess() {
                                $scope.closeLightBox();
                                getUsers();
                                $scope.grid.dataSource.read();
                                mdToast.success($filter('translate')('Setup.Users.EditsSuccess'));
                            })
                    }
                };

                $scope.deleteSelecteds = function (ev) {
                    if (!$scope.selectedRows || !$scope.selectedRows.length) {
                        mdToast.warning($filter('translate')('Module.NoRecordSelected'));
                        return;
                    }

                    var confirm = $mdDialog.confirm()
                        .title($filter('translate')('Common.AreYouSure'))
                        .textContent($filter('translate')('Setup.Users.SelectedUsersCount') + $scope.selectedRows.length)
                        .targetEvent(ev)
                        .ok($filter('translate')('Common.Yes'))
                        .cancel($filter('translate')('Common.No'));

                    $mdDialog.show(confirm).then(function () {

                        UserService.dismissUsers($scope.selectedRows)
                            .then(function () {
                                getUsers();
                                $scope.grid.dataSource.read();
                                $scope.closeLightBox();
                                mdToast.success($filter('translate')('Setup.Users.DismissesSuccess'));
                            });
                    }, function () {
                        $scope.status = 'You decided to keep your debt.';
                    });
                };

                //For Kendo UI
                $scope.closeLightBox = function () {
                    $mdDialog.hide();
                    $scope.field = null;
                    $scope.isAllSelected = false;
                    $scope.transferValue = null;
                    $scope.selectedRows = [];
                };

                $scope.selectAll = function ($event, data) {

                    $scope.selectedRows = [];

                    if ($scope.isAllSelected) {
                        $scope.isAllSelected = false;
                        for (var i = 0; i < data.length; i++) {
                            if (data[i].selected)
                                data[i].selected = false;
                        }
                    } else {
                        $scope.isAllSelected = true;
                        for (var i = 0; i < data.length; i++) {
                            data[i].selected = true;
                            $scope.selectedRows.push(data[i].id);
                        }
                    }
                };

                $scope.selectRow = function ($event, user) {
                    /*selects or unselects records*/
                    if ($event.target.checked) {
                        user.selected = true;
                        $scope.selectedRows.push(user.id);

                        return;
                    } else {
                        user.selected = false;
                        $scope.selectedRows = $scope.selectedRows.filter(function (selectedItem) {
                            return selectedItem !== user.id;
                        });
                    }

                    $scope.isAllSelected = false;
                };

                $scope.isRowSelected = function (id) {
                    return $scope.selectedRows.filter(function (selectedItem) {
                        return selectedItem === id;
                    }).length > 0;
                };

                $scope.goUrl2 = function (item) {
                    var selection = window.getSelection();
                    if (selection.toString().length === 0) {
                        $scope.showEditForm(angular.copy(item));
                    }
                };

                function generateRow(e) {
                    return '<td ng-click="$event.stopPropagation();" class="position-relative"><input ng-click="selectRow($event,dataItem);$event.stopPropagation();" ng-checked="isRowSelected(dataItem.id) || dataItem.selected"  type="checkbox" id="{{dataItem.id}}" class="k-checkbox row-checkbox"><label class="k-checkbox-label" for="{{dataItem.id}}"></label></td>'
                        + '<td class="hide-on-m2"><span ng-if="' + e.has_account + '">' + e.first_name + " " + e.last_name + '</span></td>'
                        + '<td class="hide-on-m2"><span>' + e.email + '</span></td>'
                        + '<td class="hide-on-m2"><span>' + ($rootScope.getLanguageValue(e.profile.languages, 'name') || '-') + '</span></td>'
                        + '<td class="hide-on-m2"><span>' + ($rootScope.getLanguageValue(e.role != null ? e.role.languages : null, 'label') || '-') + '</span></td>'
                        + '<td class="hide-on-m2"><span>' + $scope.getStatus(e.is_active) + '</span></td>'
                        + '<td class="show-on-m2">'
                        + '<div><strong ng-if="' + e.has_account + '">' + e.user_name + '</strong></div>'
                        + '<strong class="k-info-colored paddingl5 paddingr5">' + ($rootScope.getLanguageValue(e.profile.languages, 'name') || '-') + '</strong>'
                        + '<div><span>' + e.email + '</span></div></td>'
                        + '<td ng-click="$event.stopPropagation();"><span><md-button class="md-icon-button" aria-label=" " ng-disabled="' + (e.is_admin || (e.profile.has_admin_rights && e.id === $scope.user.id)) + '" ng-click="showConfirm(' + e.id + ')"><i class="fas fa-trash"></i> </md-button></span></td>'
                }

                var createGrid = function () {
                    $scope.usersGridOptions = {
                        dataSource: new kendo.data.DataSource({
                            type: "odata-v4",
                            page: 1,
                            pageSize: 10,
                            serverPaging: true,
                            serverFiltering: true,
                            serverSorting: true,
                            transport: {
                                read: {
                                    url: "/api/user/find_users",
                                    type: 'GET',
                                    dataType: "json",
                                    beforeSend: $rootScope.beforeSend()
                                }
                            },
                            requestEnd: function (e) {

                                $scope.loading = false;
                                if (!$rootScope.isMobile())
                                    $(".k-pager-wrap").removeClass("k-pager-sm");

                                var data = e.response;
                                if (data && data.count > 0) {
                                    for (var o = 0; o < data.count; o++) {
                                        $rootScope.processLanguage(data.items[o]);
                                    }
                                }
                            },
                            schema: {
                                data: "items",
                                total: "count",
                                model: {
                                    id: "id",
                                    fields: {
                                        email: { type: "string" },
                                        user_name: { type: "string" },
                                        first_name: { type: "string" },
                                        last_name: { type: "string" },
                                    }
                                }
                            }
                        }),
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
                            if (e.filter && e.field !== 'Role.Label' + $scope.language && e.field !== 'Profile.Name') {
                                for (var i = 0; i < e.filter.filters.length; i++) {
                                    e.filter.filters[i].ignoreCase = true;
                                }
                            }
                        },
                        rowTemplate: function (e) {
                            return '<tr ng-click="goUrl2(dataItem)">' + generateRow(e) + '</tr>';
                        },
                        altRowTemplate: function (e) {
                            return '<tr class="k-alt" ng-click="goUrl2(dataItem)">' + generateRow(e) + '</tr>';
                        },
                        columns: [{
                            width: "40px",
                            headerTemplate: "<input type='checkbox' ng-if='grid.dataSource.data().length>0' ng-checked='isAllSelected' ng-click='selectAll($event, grid.dataSource.data())' id='header-chb' class='k-checkbox header-checkbox'><label class='k-checkbox-label' for='header-chb'></label>",
                        },
                        {
                            field: "UserName",
                            title: $filter('translate')('Setup.Users.UserFullName'),
                            media: "(min-width: 575px)"
                        },
                        {
                            field: "Email",
                            title: $filter('translate')('Setup.Users.UserEmail'),
                            media: "(min-width: 575px)"
                        },
                        {
                            field: "Profile.Name",
                            title: $filter('translate')('Setup.Users.Profile'),
                            media: "(min-width: 575px)",
                        },
                        {
                            field: "Role.Label" + $scope.language,
                            title: $filter('translate')('Setup.Users.Role'),
                            media: "(min-width: 575px)",
                        },
                        {
                            field: "",
                            title: $filter('translate')('Setup.Users.UserStatus'),
                            filterable: false,
                            media: "(min-width: 575px)"
                        },
                        {
                            title: $filter('translate')('Setup.Nav.Tabs.Users'),
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

                $scope.fieldOptions = {
                    dataSource: [{ label: $filter('translate')('Setup.Users.Profile'), value: 'profile' },
                    { label: $filter('translate')('Setup.Users.Role'), value: 'role' }],
                    dataTextField: "label",
                    dataValueField: "value"
                };

                $scope.profileOptions = {
                    dataSource: {
                        transport: {
                            read: function (o) {
                                o.success($filter("orderBy")($scope.profiles, "languages." + $rootScope.globalization.Label + ".name"))
                            }
                        }
                    },
                    filter: "contains",
                    dataTextField: "languages." + $rootScope.globalization.Label + ".name",
                    dataValueField: "id"
                };

                $scope.getStatus = function (status) {
                    return status ? $filter('translate')('Common.Active') : $filter('translate')('Common.Deactivated');
                };

                $scope.roleOptions = {
                    dataSource: {
                        transport: {
                            read: function (o) {
                                o.success($filter("orderBy")($scope.roles, "languages." + $rootScope.globalization.Label + ".label"))
                            }
                        }
                    },
                    filter: "contains",
                    dataTextField: "languages." + $rootScope.globalization.Label + ".label",
                    dataValueField: "id"
                };
            });
        }
    ]);
