'use strict';

angular.module('primeapps')

    .controller('RoleFormController', ['$rootScope', '$location', '$scope', '$filter', 'ngToast', 'guidEmpty', 'blockUI', '$state', 'RoleService',
        function ($rootScope, $location, $scope, $filter, ngToast, guidEmpty, blockUI, $state, RoleService) {
            $scope.loading = true;
            $scope.id = parseInt($location.search().id);
            var reportsTo = parseInt($location.search().reportsTo);
            $scope.roleUsers = [];
            $scope.role = {};
            $scope.role.share_data = false;

            $scope.reportsTo_disabled = true;
            if (!$scope.id)
                $scope.reportsTo_disabled = false;

            RoleService.getAll()
                .then(function (response) {
                    $scope.allRoles = response.data;
                    $scope.roles = $filter('filter')($scope.allRoles, { id: '!' + $scope.id });

                    if ($scope.id) {
                        $scope.role = $filter('filter')($scope.allRoles, { id: $scope.id }, true)[0];
                        $scope.role.label = $scope.role['label_' + $rootScope.language];
                        $scope.role.description = $scope.role['description_' + $rootScope.language];

                        if (!$scope.role.master) {
                            $scope.role.reports_to = $filter('filter')($scope.allRoles, { id: $scope.role.reports_to }, true)[0].id;
                        }

                        if ($scope.role.share_data == undefined || $scope.role.share_data == null) {
                            $scope.role.share_data = false;
                        }

                        angular.forEach($scope.role.users, function (userId) {
                            var user = $filter('filter')($rootScope.workgroup.users, {Id: userId}, true)[0];

                            if (user)
                                $scope.roleUsers.push($filter('filter')($rootScope.users, {Id: user.Id}, true)[0]);
                        });
                    }
                    else if (reportsTo) {
                        $scope.role.reports_to = reportsTo;
                    }

                    $scope.loading = false;
                });

            $scope.save = function () {
                if ($scope.roleForm.$valid) {
                    $scope.saving = true;

                    var role = angular.copy($scope.role);
                    var result = null;

                    role.label_tr = role.label;
                    role.label_en = role.label;
                    role.description_tr = role.description;
                    role.description_en = role.description;

                    if (!$scope.id) {
                        result = RoleService.create(role);
                    }
                    else {
                        result = RoleService.update(role, $scope.role.id);
                    }

                    result.then(function () {
                        ngToast.create({
                            content: $filter('translate')('Setup.Roles.SaveSuccess'),
                            className: 'success'
                        });
                        $state.go('app.setup.roles');
                    }).finally(function () {
                        $scope.saving = false;
                    });
                }
            };

            $scope.cancel = function () {
                $state.go('app.setup.roles');
            };
        }
    ]);