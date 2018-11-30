'use strict';

angular.module('primeapps')

    .controller('UserGroupFormController', ['$rootScope', '$scope', '$location', '$state', '$filter', 'ngToast', 'helper', 'UserGroupService',
        function ($rootScope, $scope, $location, $state, $filter, ngToast, helper, UserGroupService) {
            $scope.loading = true;
            var id = parseInt($location.search().id);
            var clone = parseInt($location.search().clone);
            $scope.lookupUser = helper.lookupUser;

            function getUserGroup() {
				UserGroupService.getAll()
					.then(function (userGroups) {
						$scope.userGroups = userGroups.data;

						if (id) {
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
                                $scope.userGroup = angular.copy($filter('filter')($scope.userGroups, { id: clone }, true)[0]);
                            else
                                $scope.userGroup = {};
                        }
                    })
                    .finally(function () {
                        $scope.loading = false;
                    });
            }

            getUserGroup();

            $scope.setFormValid = function () {
                $scope.userGroupForm.users.$setValidity('minTags', true);
            };

            function validate() {
                if(!$scope.userGroup.name)
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
                validate();

                if ($scope.userGroupForm.$valid) {
                    $scope.saving = true;
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
                            $state.go('app.setup.usergroups');
                            ngToast.create({ content: $filter('translate')('Setup.UserGroups.SubmitSuccess'), className: 'success' });
                        })
                        .finally(function () {
                            $scope.saving = false;
                        });
                }
            };
        }
    ]);