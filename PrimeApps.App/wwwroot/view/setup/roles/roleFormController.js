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
            $scope.role_change = false;
            $scope.reportsTo_disabled = true;
            if (!$scope.id)
                $scope.reportsTo_disabled = false;

            RoleService.getAll()
                .then(function (response) {
                    $scope.allRoles = response.data;
                    $scope.roles = $filter('filter')($scope.allRoles, { id: '!' + $scope.id });

                    if ($scope.id) {
                        checkChildRole($scope.id);
                        $scope.role = $filter('filter')($scope.allRoles, { id: $scope.id }, true)[0];
                        $scope.role.label = $scope.role['label_' + $rootScope.language];
                        $scope.role.description = $scope.role['description_' + $rootScope.language];

                        if (!$scope.role.master) {
                            $scope.role.reports_to = $filter('filter')($scope.allRoles, { id: $scope.role.reports_to }, true)[0].id;
                        }

                        if ($scope.role.share_data === undefined || $scope.role.share_data === null) {
                            $scope.role.share_data = false;
                        }

                        angular.forEach($scope.role.users, function (userId) {
                            var user = $filter('filter')($rootScope.workgroup.users, {id: userId}, true)[0];

                            if (user)
                                $scope.roleUsers.push($filter('filter')($rootScope.users, {id: user.Id}, true)[0]);
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
                    var roleChange = $scope.role_change;
                    var successMess = 'Setup.Roles.SaveSuccess';

                    role.label_tr = role.label;
                    role.label_en = role.label;
                    role.description_tr = role.description;
                    role.description_en = role.description;

                    if (!$scope.id) {
                        result = RoleService.create(role);
                    }
                    else {
                        result = RoleService.update(role, roleChange);
                    }

                    result.then(function () {
                        if (roleChange)
                            successMess = 'Setup.Roles.LongSaveSuccess';

                        ngToast.create({
                            content: $filter('translate')(successMess),
                            className: 'success'
                        });
                        $state.go('app.setup.roles');
                    }).finally(function () {
                        $scope.saving = false;
                    });
                }
            };

            $scope.roleUpdateChange = function () {
                $scope.role_change = true;
            };

            $scope.cancel = function () {
                $state.go('app.setup.roles');
            };

            function checkChildRole(id) {
                //Gelen roleId'ye ait alt rollerin olup olmadığını kontrol ediyoruz
                var children = $filter('filter')($scope.roles, { reports_to: id });
                //Mevcut roller arasında resports_to idleri gelen rolün idsine eşit olanları filtreliyoruz.
                $scope.roles = $filter('filter')($scope.roles, { reports_to: '!' + id });

                angular.forEach(children, function (child) {
                    checkaChildRole(child.id);
                });
            }
        }
    ]);