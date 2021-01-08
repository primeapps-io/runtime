'use strict';

angular.module('primeapps')

    .controller('UserGroupFormController', ['$rootScope', '$scope', '$location', '$state', '$filter', 'helper', 'UserGroupService', 'mdToast',
        function ($rootScope, $scope, $location, $state, $filter, helper, UserGroupService, mdToast) {
            $scope.loadingModal = true;
            $scope.userGroup = {
                languages: {}
            };
            
            $scope.userGroup.languages[$rootScope.globalization.Label] = {
                name: '',
                description: ''
            };

            if (!$scope.$parent.$parent.selectedGroup)
                return;

            var id = $scope.$parent.$parent.selectedGroup.id;
            var clone = $scope.$parent.$parent.selectedGroup.clone;
            $scope.lookupUser = helper.lookupUser;

            function getUserGroup() {
                UserGroupService.getAll()
                    .then(function (userGroups) {
                        $scope.userGroups = userGroups.data;
                        $rootScope.processLanguages($scope.userGroups);
                        if (id && !clone) {
                            $scope.userGroup = angular.copy($filter('filter')($scope.userGroups, { id: id }, true)[0]);
                            var users = angular.copy($scope.userGroup.users);
                            $scope.userGroup.users = [];
                            for (var i = 0; i < users.length; i++) {
                                var user = users[i].user;
                                $scope.userGroup.users.push(user);
                            }
                        }
                        else {
                            if (clone)
                                $scope.userGroup = angular.copy($filter('filter')($scope.userGroups, { id: id }, true)[0]);
                        }
                    })
                    .finally(function () {
                        $scope.loadingModal = false;
                    });
            }

            getUserGroup();

            $scope.setFormValid = function () {
                $scope.userGroupForm.users.$setValidity('minTags', true);
            };

            function validate() {
                if (!$scope.userGroup.name)
                    return;

                var isUnique = true;
                var existingUserGroup = $filter('filter')($scope.userGroups, { name: $scope.userGroup.name }, true)[0];

                if (existingUserGroup && !$scope.userGroup.id)
                    isUnique = false;
                else if (existingUserGroup && existingUserGroup.id !== $scope.userGroup.id)
                    isUnique = false;

                if (!isUnique)
                    $scope.userGroupForm['name'].$setValidity('unique', false);
            }

            $scope.submit = function () {
                //validate();

                if ($scope.validator.validate()) {
                    $scope.loadingModal = true;
                    var result = null;
                    var userGroupModel = angular.copy($scope.userGroup);
                    userGroupModel = UserGroupService.prepare(userGroupModel);

                    if (!$scope.userGroup.id || clone) {
                        result = UserGroupService.create(userGroupModel);
                    }
                    else {
                        result = UserGroupService.update(userGroupModel);
                    }

                    result
                        .then(function () {
                            mdToast.success($filter('translate')('Setup.UserGroups.SubmitSuccess'));
                            $rootScope.closeSide('sideModal');
                            $scope.$parent.$parent.grid.dataSource.read();
                        })
                        .finally(function () {
                            $scope.loadingModal = false;
                        }).catch(function () {
                            $scope.loadingModal = false;
                            $rootScope.closeSide('sideModal');
                            $scope.$parent.$parent.grid.dataSource.read();
                        });
                } else {
                    mdToast.error($filter('translate')('Module.RequiredError'));
                }
            };


            //For Kendo UI
            $scope.users = $rootScope.users;

            $scope.usersOptions = {
                dataSource: $scope.users,
                filter: "contains",
                dataTextField: "full_name",
                dataValueField: "id",
            };
            //For Kendo UI

        }
    ]);