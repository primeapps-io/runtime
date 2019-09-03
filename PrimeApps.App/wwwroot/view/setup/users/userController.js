'use strict';

angular.module('primeapps')

    .controller('UserController', ['$rootScope', '$scope', '$filter', '$state', 'ngToast', 'guidEmpty', '$popover', 'helper', 'UserService', 'WorkgroupService', 'AppService', 'ProfileService', 'RoleService', 'LicenseService', '$q', 'officeHelper',
        function ($rootScope, $scope, $filter, $state, ngToast, guidEmpty, $popover, helper, UserService, WorkgroupService, AppService, ProfileService, RoleService, LicenseService, $q, officeHelper) {

            $scope.hasAdminRight = $filter('filter')($rootScope.profiles, { id: $rootScope.user.profile.id }, true)[0].has_admin_rights;
            if (!$scope.hasAdminRight) {
                if (!helper.hasCustomProfilePermission('users')) {
                    ngToast.create({ content: $filter('translate')('Common.Forbidden'), className: 'warning' });
                    $state.go('app.dashboard');
                }
            }

            $scope.loading = true;
            $scope.isOfficeConnected = false;
            //user add button popover controller
            $scope.officeUserReady = false;

            $scope.officeUsers = null;
            $scope.selectedOfficeUser = {};
            $scope.addUserModel = {};
            $scope.addUserForm = true;
            $scope.submitting = false;
            $scope.hideSendEmailToUser = false;

            $scope.officeUserChanged = function (selectedOfficeUser) {
                $scope.addUserModel.email = selectedOfficeUser.email;
                $scope.addUserModel.phone = selectedOfficeUser.phone;
                $scope.addUserModel.fullName = selectedOfficeUser.fullName;
                $scope.addUserModel.firstName = selectedOfficeUser.name;
                $scope.addUserModel.lastName = selectedOfficeUser.surname;
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
                var emailBody = $filter('translate')('Setup.Office.EmailNotification.Hello') + " " + $scope.addedUser.fullName + "<br />" + $filter('translate')('Setup.Office.EmailNotification.Created') + "<br />" + $filter('translate')('Setup.Office.EmailNotification.Email') + $scope.addedUser.email + "<br />" + $filter('translate')('Setup.Office.EmailNotification.Password') + $scope.userPassword;
                var requestMail = {};
                requestMail.Subject = $filter('translate')('Setup.Office.EmailNotification.Subject');
                requestMail.Template_With_Body = emailBody;
                requestMail.To_Addresses = [$scope.addedUser.email];
                requestMail.Template_Name='add_user'

                UserService.sendPasswordToOfficeUser(requestMail)
                    .then(function (response) {
                        $scope.closeUserInfoPopover();
                        ngToast.create({
                            content: $filter('translate')('Setup.Office.SendEmailSuccess'),
                            className: 'success',
                            timeout: 5000
                        });
                    }).catch(function (response) {
                        $scope.closeUserInfoPopover();
                        ngToast.create({
                            content: $filter('translate')('Setup.Office.SendEmailError'),
                            className: 'danger',
                            timeout: 5000
                        });
                    });

            };

            $scope.closeUserInfoPopover = function closePasswordPopup() {
                $scope.submitting = false;
                $scope.addUserForm = true;
                $scope.userPassword = null;
                $scope.hideSendEmailToUser = false;
                if ($scope.createOfficePopover) {
                    $scope.createOfficePopover.hide();
                } else if ($scope.createPopover) {
                    $scope.createPopover.hide();
                }

                $scope.addedUser = {};
            };

            //TODO Removed
            /*getOfficeUsers();

            function getOfficeUsers() {
                officeHelper.officeTenantInfo()
                    .then(function (adInfo) {
                        if (adInfo.data) {
                            $scope.isOfficeConnected = true;
                            UserService.getOfficeUsers()
                                .then(function (response) {
                                    if (response.data) {
                                        $scope.officeUsers = response.data;
                                        $scope.officeUserReady = true;
                                    }
                                });
                            $rootScope.user.azureDirectory = adInfo.data;
                        }
                    });
            }*/

            function getUsers() {
                var promises = [];

                promises.push(UserService.getAllUser());
                promises.push(ProfileService.getAll());
                promises.push(RoleService.getAll());
                promises.push(LicenseService.getUserLicenseStatus());


                $q.all(promises).then(function (data) {
                    var users = data[0].data,
                        responseProfiles = data[1].data,
                        responseRoles = data[2].data,
                        license = data[3].data;

                    //var workgroup = $filter('filter')($rootScope.workgroups, { tenant_id: $rootScope.user.tenant_id }, true)[0];
                    $rootScope.workgroup.users = users;

                    $scope.profiles = ProfileService.getProfiles(responseProfiles, $rootScope.workgroup.tenant_id, true);
                    if (!$rootScope.user.profile.has_admin_rights)
                        $scope.profiles = $filter('filter')($scope.profiles, { has_admin_rights: !true }, true);

                    $scope.roles = responseRoles;
                    $scope.users = UserService.getUsers(users, $scope.profiles, $scope.roles);
                    $scope.licensesBought = license.total || 0;
                    $scope.licensesUsed = license.used || 0;
                    $scope.licenseAvailable = $scope.licensesBought - $scope.licensesUsed;
                    $scope.loading = false;
                });

            }

            getUsers();

            $scope.showCreateForm = function () {
                $scope.addedUser = {};
                $scope.addUserForm = true;
                $scope.createPopover = $scope.createPopover || $popover(angular.element(document.getElementById('createButton')), {
                    templateUrl: 'view/setup/users/userCreate.html',
                    placement: 'bottom-right',
                    scope: $scope,
                    autoClose: true,
                    show: true
                });
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
                if (!inviteModel || !inviteModel.email || !inviteModel.profile || !inviteModel.role || !inviteModel.firstName || !inviteModel.lastName)
                    return;

                if (!inviteModel.fullName) {
                    inviteModel.fullName = inviteModel.firstName + " " + inviteModel.lastName;
                }

                $scope.userInviting = true;
                inviteModel.profileId = inviteModel.profile;
                inviteModel.roleId = inviteModel.role;

                $scope.addedUser = angular.copy(inviteModel);
                inviteModel = helper.SnakeToCamel(inviteModel);

                UserService.addUser(inviteModel)
                    .then(function (response) {
                        if (response.data) {
                            //TODO Removed
                            /*getOfficeUsers();*/
                            getUsers();

                            $scope.userInviting = false;
                            ngToast.create({
                                content: $filter('translate')('Setup.Users.NewUserSuccess'),
                                className: 'success'
                            });

                            $scope.userPassword = response.data.password;
                            $scope.hideSendEmailToUser = response.data.password.contains("***");
                            $scope.addUserForm = false;
                            $scope.addUserModel = {};
                        }
                    })
                    .catch(function (response) {
                        //$scope.inviteOfficeModel = {};
                        $scope.userInviting = false;
                        if (response.status === 409) {
                            ngToast.create({
                                content: $filter('translate')('Setup.Users.NewUserError'),
                                className: 'warning'
                            });
                        }
                    });
                /*$scope.userInviting = true;

                 var success = function (resultInvite, inviteModel) {
                 if (resultInvite.HasAccount) {
                 getUsers();

                 $scope.userInviting = false;
                 ngToast.create({
                 content: $filter('translate')('Setup.Users.NewUserSuccess'),
                 className: 'success'
                 });
                 $scope.createPopover.hide();
                 inviteModel = null;
                 return;
                 }

                 getUsers();

                 $scope.userInviting = false;
                 ngToast.create({
                 content: $filter('translate')('Setup.Users.NewUserSuccess'),
                 className: 'success'
                 });
                 $scope.createPopover.hide();
                 inviteModel = null;
                 };

                 UserService.isAvailableToInvite(inviteModel.email, $rootScope.workgroup.instanceID)
                 .then(function onSuccess(data) {
                 if (!data.data) {
                 $scope.inviteModel = null;
                 $scope.userInviting = false;
                 ngToast.create({
                 content: $filter('translate')('Setup.Users.NewUserError'),
                 className: 'warning'
                 });
                 return;
                 }

                 var culture = helper.getCulture();

                 UserService.invite(inviteModel.email, $rootScope.workgroup.instanceID, inviteModel.profile, inviteModel.role, culture, $rootScope.user.email)
                 .then(function onSuccess(resultInvite) {
                 if (!resultInvite.data) {
                 $filter('translate')('Common.Error');
                 return;
                 }

                 if (!resultInvite.data.Result) {
                 $scope.userInviting = false;
                 ngToast.create({
                 content: $filter('translate')('Setup.Users.LicenceLimitError'),
                 className: 'warning',
                 timeout: 6000
                 });
                 $scope.createPopover.hide();
                 return;
                 }

                 success(resultInvite.data, inviteModel);
                 })
                 .catch(function onError() {
                 $scope.userInviting = false;
                 });
                 });*/
            };

            $scope.showEditForm = function (user) {
                $scope.selectedUser = user;
                $scope.editModel = {};
                $scope.editModel.profile = user.profile.id;
                $scope.editModel.role = user.role.id;
                $scope.editModel.activeDirectoryEmail = user.activeDirectoryEmail;
                $scope.userHaveActiveDirectoryEmail = user.activeDirectoryEmail !== null && user.activeDirectoryEmail !== "null" && user.activeDirectoryEmail !== '';
                $scope.editModelState = angular.copy($scope.editModel);
                $scope['editPopover' + user.id] = $scope['editPopover' + user.id] || $popover(angular.element(document.getElementById('editButton' + user.id)), {
                    templateUrl: 'view/setup/users/userEdit.html',
                    placement: 'left',
                    scope: $scope,
                    autoClose: true,
                    show: true
                });

            };

            $scope.edit = function () {
                $scope.userEditing = true;

                if ($scope.editModel.profile === $scope.editModelState.profile &&
                    $scope.editModel.role === $scope.editModelState.role &&
                    $scope.editModel.activeDirectoryEmail === $scope.editModelState.activeDirectoryEmail) {
                    $scope.userEditing = false;

                    $scope.popover.hide();
                    //if need on alert for "no changed user" uncomment line
                    //ngToast.create({ content: $filter('translate')('Setup.Users.EditSuccess'), className: 'success' });
                    return;
                }

                var success = function () {
                    getUsers();
                    $scope.userEditing = false;
                    $scope.popover.hide();
                    ngToast.create({
                        content: $filter('translate')('Setup.Users.EditSuccess'),
                        className: 'success'
                    });
                };

                var updateActiveDirectoryEmail = function () {
                    UserService.updateActiveDirectoryEmail($scope.selectedUser.id, $scope.editModel.activeDirectoryEmail)
                        .then(function () {
                            //TODO Removed
                            /*getOfficeUsers();*/
                            success();
                        })
                        .catch(function (response) {
                            $scope.userEditing = false;
                            if (response.status === 409) {
                                ngToast.create({
                                    content: $filter('translate')('Setup.Users.NewUserError'),
                                    className: 'warning'
                                });
                            }
                        });
                };

                ProfileService.changeUserProfile($scope.selectedUser.id, $rootScope.workgroup.tenant_id, $scope.editModel.profile)
                    .then(function onSuccess() {
                        RoleService.updateUserRole($scope.selectedUser.id, $scope.editModel.role)
                            .then(function onSuccess() {
                                if (($scope.editModel.activeDirectoryEmail !== null || $scope.editModel.activeDirectoryEmail !== "") &&
                                    $scope.editModel.activeDirectoryEmail !== $scope.editModelState.activeDirectoryEmail) {
                                    updateActiveDirectoryEmail();
                                } else {
                                    success();
                                }
                            })
                            .catch(function onError() {
                                $scope.userEditing = false;
                            });
                    })
                    .catch(function onError() {
                        $scope.userEditing = false;
                    });
            };

            $scope.dismiss = function (user, index, close) {
                $scope.userDeleting = true;

                UserService.dismiss(user, $rootScope.workgroup.tenant_id)
                    .then(function onSuccess() {
                        $scope.users.splice(index, 1);
                        $scope.userDeleting = false;
                        ngToast.create({
                            content: $filter('translate')('Setup.Users.DismissSuccess'),
                            className: 'success'
                        });

                        AppService.getMyAccount(true);

                        LicenseService.getUserLicenseStatus().then(function onSuccess(license) {
                            $scope.licensesBought = license.data.total || 0;
                            $scope.licensesUsed = license.data.used || 0;
                            $scope.licenseAvailable = $scope.licensesBought - $scope.licensesUsed;
                        });
                        close();

                    })
                    .catch(function onError() {
                        $scope.userDeleting = false;
                        close();
                    });
            };

            $scope.gotoLicencePage = function () {
                var menuItem = $filter('filter')($scope.$parent.menuItems, { link: '#/app/setup/license' })[0];
                $scope.$parent.selectMenuItem(menuItem);

                $state.go('app.setup.license');
            };

        }
    ]);