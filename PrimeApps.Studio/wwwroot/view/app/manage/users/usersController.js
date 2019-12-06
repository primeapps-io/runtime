'use strict';

angular.module('primeapps')

    .controller('UsersController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'UsersService', '$localStorage',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, UsersService, $localStorage) {
            function validateEmail(email) {
                var re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
                return re.test(email);
            }

            $scope.$parent.activeMenuItem = 'users';
            $rootScope.breadcrumblist[2].title = 'Users';
            $scope.roles = [];
            $scope.profiles = [];
            $scope.users = [];
            $scope.userModel = {};
            $scope.loading = true;
            $scope.userModel.auto_pass = "false";
            var keylist = "xNXRKA0IOYmPP7jgN088zdEwNqqi7erpdMhsoqAVJ0r0sBaeDDCq4sbS2FkDRnEWa5FyYerM);2$0ZUA;I:1^]Rs1LGiS:v;!SO,#jqTq0<1B[.rwtn8K:-0FO:O,";

            $scope.generatePass = function () {

                var temp = '';
                for (var i = 0; i < 8; i++) {
                    temp += keylist.charAt(Math.floor(Math.random() * keylist.length));
                }
                $scope.userModel.password = temp;
                ;
            };

            $scope.passwordChange = function () {
                if ($scope.userModel.auto_pass) {
                    $scope.userModel.auto_pass = false;
                }
            };

            UsersService.getAllRoles()
                .then(function (response) {
                    $scope.roles = response.data;
                });

            UsersService.getAllProfiles()
                .then(function (response) {
                    $scope.profiles = response.data;
                    $scope.profiles[0].name = 'Administrator';
                    $scope.profiles[1] ? $scope.profiles[1].name = 'Standard' : null;
                });

            $scope.delete = function (id, event) {
                var willDelete =
                    swal({
                        title: "Are you sure?",
                        text: " ",
                        icon: "warning",
                        buttons: ['Cancel', 'Yes'],
                        dangerMode: true
                    }).then(function (value) {
                        if (value) {

                            UsersService.delete(id)
                                .then(function () {
                                    $scope.grid.dataSource.read();
                                    toastr.success("User is deleted successfully.", "Deleted!");

                                })
                                .catch(function () {
                                    toastr.error("User is not deleted successfully.", "Deleted!");
                                });

                        }
                    });
            };

            $scope.save = function (userForm) {
                if (!userForm.$valid) {
                    toastr.error($filter('translate')('Module.RequiredError'));
                    return;
                }

                $scope.saving = true;

                if (!$scope.editing) {

                    $scope.userModel.created_at = new Date();
                    $scope.userModel.is_active = true;

                    UsersService.create($scope.userModel)
                        .then(function (response) {
                            $scope.pageTotal++;
                            if (response.data) {
                                toastr.success('User is saved successfully');
                                $scope.grid.dataSource.read();
                                if ($scope.resultModel.sendPassword) {
                                    $scope.sendEmailPassword($scope.userModel, $scope.resultModel);
                                }

                                $scope.saving = false;
                                $scope.closeModal();
                            }
                        })
                        .catch(function (response) {
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
                    if ($scope.passwordEvent === 'change') {
                        if ($scope.changePasswordModel.new_password != $scope.changePasswordModel.confirm_password) {
                            toastr.warning("New password is not matching !");
                            return;
                        }

                        $scope.changePasswordModel.email = $scope.userModel.email;

                        UsersService.updatePassword($scope.changePasswordModel)
                            .then(function (response) {
                                updateUser();
                            })
                            .catch(function (error) {
                                toastr.error("Your password has not been successfully updated !");
                                $scope.saving = false;
                            });
                    }
                    else if ($scope.passwordEvent === 'reset') {
                        if ($scope.changePasswordModel.new_password != $scope.changePasswordModel.confirm_password) {
                            toastr.warning("New password is not matching !");
                            return;
                        }

                        $scope.changePasswordModel.email = $scope.userModel.email;

                        UsersService.resetPassword($scope.changePasswordModel)
                            .then(function (response) {
                                updateUser();
                            })
                            .catch(function (error) {
                                toastr.error("Your password has not been successfully updated !");
                                $scope.saving = false;
                            });
                    }
                    else {
                        updateUser();
                    }

                }

            };

            var updateUser = function () {
                UsersService.update($scope.userModel.id, $scope.userModel)
                    .then(function (response) {
                        toastr.success('User is saved successfully');
                        $scope.closeModal();
                        $scope.grid.dataSource.read();
                        $scope.saving = false;

                    })
                    .catch(function () {
                        toastr.error($filter('translate')('Common.Error'));
                        $scope.saving = false;
                    });
            };

            $scope.showFormModal = function (id) {

                if (id) {
                    $scope.userModel = angular.copy($filter('filter')($scope.users, { id: id }, true)[0]);
                    $scope.editing = true;
                }
                else {
                    $scope.userModel = {
                        profile_id: 1,
                        role_id: 1
                    };
                    $scope.userModel.auto_pass = "true";
                    $scope.editing = false;
                    $scope.resultModel = {
                        sendPassword: true,
                        recipient: $rootScope.me.email + ';'
                    };
                }

                $scope.userFormModal = $scope.userFormModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/manage/users/userFormModal.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.userFormModal.$promise.then(function () {
                    $scope.userFormModal.show();
                });
            };

            $scope.closeModal = function () {
                $scope.userFormModal.hide();
                $scope.changePasswordModel = null;
                delete $scope.passwordEvent;
            };

            $scope.sendEmailPassword = function (userModel, resultModel) {
                var sendEmailData = {};
                sendEmailData.app_id = 2;
                sendEmailData.culture = "en";
                sendEmailData.display_name = userModel.first_name + ' ' + userModel.last_name;
                sendEmailData.password = userModel.password;
                var emails = resultModel.recipient.split(";");

                for (var i = 0; i < emails.length; i++) {
                    if (validateEmail(emails[i])) {
                        sendEmailData.email = emails[i];
                        UsersService.sendEmail(angular.copy(sendEmailData), i)
                    }
                }
            };

            $scope.unCheckControl = function (event) {
                var element = angular.element(event.target);
                var value = event.target.value;
                var group = angular.element(event.target.closest('div')).find('input');

                if (element.attr('marked') && $scope.passwordEvent === value) {
                    delete $scope.passwordEvent;
                    group.removeAttr('marked');
                }
                else {
                    group.removeAttr('marked');
                    element.attr('marked', true)
                }
            };

            //For Kendo UI
            $scope.goUrl = function (item) {
                var selection = window.getSelection();
                if (selection.toString().length === 0) {
                    $scope.showFormModal(item.id); //click event.
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
                            url: "/api/app_draft_user/find",
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
                        },
                        parse: function (data) {
                            $scope.users = data.items;
                            return data;
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
                    trTemp += '<td class="text-left"> <span>' + e.full_name + '</span></td > ';
                    trTemp += '<td> <span>' + e.email + '</span> <span ng-click="$event.stopPropagation();" class="copy-button pull-right" title="{{"Common.Copy" | translate}}" ngclipboard data-clipboard-text="' + e.email + '"><i class="copy-icon fa fa-clipboard"></i></span></td > ';
                    trTemp += '<td> <span>' + e.role['label_' + $scope.language] + '</span></td > ';
                    trTemp += e.profile_id === 1 ? '<td> <span>Administrator</span></td>' : e.profile_id === 2 ? '<td> <span>Standart</span></td>' : '<td> <span>' + e.profile['name_' + $scope.language] + '</span></td>';
                    trTemp += '<td ng-click="$event.stopPropagation();"> <button ng-click="$event.stopPropagation(); delete(dataItem.id, $event);" type="button" class="action-button2-delete"><i class="fas fa-trash"></i></button></td></tr>';
                    return trTemp;
                },
                altRowTemplate: function (e) {
                    var trTemp = '<tr class="k-alt" ng-click="goUrl(dataItem)">';
                    trTemp += '<td class="text-left"> <span>' + e.full_name + '</span></td > ';
                    trTemp += '<td> <span>' + e.email + '</span> <span ng-click="$event.stopPropagation();" class="copy-button pull-right" title="{{"Common.Copy" | translate}}" ngclipboard data-clipboard-text="' + e.email + '"><i class="copy-icon fa fa-clipboard"></i></span></td > ';
                    trTemp += '<td> <span>' + e.role['label_' + $scope.language] + '</span></td > ';
                    trTemp += e.profile_id === 1 ? '<td> <span>Administrator</span></td>' : e.profile_id === 2 ? '<td> <span>Standart</span></td>' : '<td> <span>' + e.profile['name_' + $scope.language] + '</span></td>';
                    trTemp += '<td ng-click="$event.stopPropagation();"> <button ng-click="$event.stopPropagation(); delete(dataItem.id, $event);" type="button" class="action-button2-delete"><i class="fas fa-trash"></i></button></td></tr>';
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
                        field: 'FullName',
                        title: 'Name',
                        headerAttributes: {
                            'class': 'text-left'
                        },
                    },
                    {
                        field: 'Email',
                        title: 'Email',
                    },
                    {
                        field: 'Role.Label' + $scope.language,
                        title: 'Role',
                    },
                    {
                        field: 'Profile.Name' + $scope.language,
                        title: 'Profile',
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