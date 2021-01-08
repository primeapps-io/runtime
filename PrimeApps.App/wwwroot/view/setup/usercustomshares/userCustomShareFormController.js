'use strict';

angular.module('primeapps')

    .controller('UserCustomShareFormController', ['$rootScope', '$location', '$scope', '$filter', 'guidEmpty', 'blockUI', '$state', 'UserCustomShareService', 'UserService', 'helper', 'mdToast',
        function ($rootScope, $location, $scope, $filter, guidEmpty, blockUI, $state, UserCustomShareService, UserService, helper, mdToast) {

            //$scope.id = parseInt($location.search().id);
            if (!$scope.$parent.$parent.selectedShare)
                return;

            $scope.id = $scope.$parent.$parent.selectedShare.id;
            $scope.language = $rootScope.language;
            $scope.userOwner = {};
            $scope.lookupUserAndGroup = helper.lookupUserAndGroup;
            var users = [];

            //For Kendo UI
            $scope.usersOptions = {
                dataSource: {
                    transport: {
                        read: {
                            url: "/api/User/get_all",
                            type: 'GET',
                            dataType: "json",
                            beforeSend: $rootScope.beforeSend(),
                        }
                    },
                    requestEnd: function (e) {
                        users = e.response;
                    }
                },
                filter: "contains",
                dataValueField: "id",
                dataTextField: "full_name",
                optionLabel: $filter('translate')('Setup.Workflow.ApprovelProcess.SelectUser'),
                template: "<span>{{dataItem.full_name}} - {{dataItem.email}}</span>",
                valueTemplate: "<span>{{dataItem.full_name}} - {{dataItem.email}}</span>",
            };



            UserCustomShareService.getAll()
                .then(function (response) {
                    $scope.userCustomShares = response.data;

                    if (!$scope.id) {
                        for (var i = 0; i < response.data.length; i++) {
                            var item = response.data[i];
                            if ($filter('filter')(users, { id: item.user_id }, true)[0]) {
                                var user = $filter('filter')(users, { id: item.user_id }, true)[0].email;
                                users = $filter('filter')(users, { email: '!' + user }, true);
                            }
                        }
                    }
                });

            $scope.multiselect = function () {
                return $filter('filter')($rootScope.modules, function (value, index, array) {
                    return (value.order !== 0);
                }, true);
            };

            if ($scope.id) {
                $scope.loadingModal = true;
                UserCustomShareService.get($scope.id)
                    .then(function (response) {
                        $scope.userOwner.user = response.data.user_id;
                        $scope.userOwner.shared_user = response.data.shared_user_id;
                        var owners = [];
                        var moduleList = [];

                        //user groups
                        if (response.data.user_groups_list.length > 0 && response.data.user_groups_list[0] !== '{}') {
                            UserCustomShareService.getUserGroups()
                                .then(function (userGroups) {
                                    var user_groups = response.data.user_groups.replace('{', '').replace('}', '').split(',');
                                    for (var i = 0; i < response.data.user_groups_list.length; i++) {
                                        var item = $filter('filter')(userGroups.data, { id: parseInt(user_groups[i]) }, true)[0];
                                        var userGroup = {
                                            description: 'User Group',
                                            id: item.id,
                                            name: item.name,
                                            type: "group"
                                        };
                                        owners.push(userGroup);
                                    }
                                })
                        }

                        //users
                        if (response.data.users_list.length > 0) {
                            for (var i = 0; i < response.data.users_list.length; i++) {
                                var item = $filter('filter')($rootScope.users, { id: parseInt(response.data.users_list[i]) }, true)[0];
                                var user = {
                                    description: item.Email,
                                    id: item.id,
                                    name: item.FullName,
                                    type: "user"
                                };
                                owners.push(user);
                            }
                        }

                        //modules
                        if (response.data.module_list.length > 0) {
                            for (var k = 0; k < response.data.module_list.length; k++) {
                                var module = $filter('filter')($rootScope.modules, { name: response.data.module_list[k] }, true)[0];
                                moduleList.push(module);
                            }
                        }

                        $scope.userOwner.modules = moduleList;
                        $scope.userOwner.owners = owners;
                        $scope.loadingModal = false;
                    });
            }

            $scope.save = function () {
                if ($scope.userOwnerForm.validate()) {
                    $scope.loadingModal = true;
                    var modules = null;

                    if ($scope.userOwner.owners && $scope.userOwner.owners.length) {
                        $scope.shared_users = null;
                        $scope.shared_user_groups = null;

                        for (var l = 0; l < $scope.userOwner.owners.length; l++) {
                            var shared = $scope.userOwner.owners[l];

                            if (shared.type === 'user') {
                                if ($scope.shared_users === null)
                                    $scope.shared_users = shared.id;
                                else
                                    $scope.shared_users += ',' + shared.id;
                            }

                            if (shared.type === 'group') {
                                if ($scope.shared_user_groups === null)
                                    $scope.shared_user_groups = shared.id;
                                else
                                    $scope.shared_user_groups += ',' + shared.id;
                            }
                        }
                    }

                    if ($scope.userOwner.modules && $scope.userOwner.modules.length) {
                        for (var j = 0; j < $scope.userOwner.modules.length; j++) {
                            var module = $scope.userOwner.modules[j];
                            if (modules === null)
                                modules = module.name;
                            else
                                modules += ',' + module.name;
                        }
                    }

                    var obj = {
                        user_id: $scope.userOwner.user,
                        shared_user_id: $scope.userOwner.shared_user,
                        users: $scope.shared_users,
                        user_groups: $scope.shared_user_groups,
                        modules: modules
                    };

                    if (!$scope.id) {
                        UserCustomShareService.create(obj)
                            .then(function onSuccess() {
                                mdToast.success($filter('translate')('Setup.UserCustomShares.SaveSuccess'));
                                $scope.$parent.$parent.grid.dataSource.read();
                                $rootScope.closeSide('sideModal');
                            })
                            .catch(function onError() {
                                //error(data, status);
                                $scope.loadingModal = false;
                            });
                    }
                    else {
                        UserCustomShareService.update($scope.id, obj)
                            .then(function onSuccess() {
                                mdToast.success($filter('translate')('Setup.UserCustomShares.EditSuccess'));
                                $rootScope.closeSide('sideModal');
                                $scope.$parent.$parent.grid.dataSource.read();
                            })
                            .catch(function onError() {
                                //error(data, status);
                                $rootScope.closeSide('sideModal');
                                $scope.loadingModal = false;
                            });
                    }
                } else {
                    mdToast.error($filter('translate')('Module.RequiredError'));
                }
            };

            $scope.modulesOptions = {
                dataSource: $filter('filter')($rootScope.modules, function (item) { return item.name !== 'users' && item.name !== 'profiles' && item.name !== 'roles' }),
                filter: "contains",
                dataTextField: "languages." + $rootScope.globalization.Label + ".label.plural",
                dataValueField: "id",
            }
        }
    ]);