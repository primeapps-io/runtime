'use strict';

angular.module('primeapps')

    .controller('RoleFormController', ['$rootScope', '$location', '$scope', '$filter', 'guidEmpty', 'blockUI', '$state', 'RoleService', 'mdToast',
        function ($rootScope, $location, $scope, $filter, guidEmpty, blockUI, $state, RoleService, mdToast) {
            $scope.hasAdminRight = $filter('filter')($rootScope.profiles, { id: $rootScope.user.profile.id }, true)[0].has_admin_rights;

            if (!$scope.hasAdminRight) {
                if (!helper.hasCustomProfilePermission('roles')) {
                    mdToast.error($filter('translate')('Common.Forbidden'));
                    $state.go('app.dashboard');
                }
            }

            if (!$scope.$parent.$parent.currentRole)
                return;

            $scope.loading = true;
            //$scope.id = parseInt($location.search().id);
            $scope.id = $scope.$parent.$parent.currentRole.id;
            var reportsTo = $scope.$parent.$parent.currentRole.reportsTo;
            $scope.roleUsers = [];
            $scope.role = {};
            $scope.role.share_data = false;
            $scope.role_change = false;
            $scope.reportsTo_disabled = $scope.id;

            RoleService.getAll()
                .then(function (response) {
                    $scope.allRoles = response.data;
                    $rootScope.processLanguages($scope.allRoles);
                    $scope.roles = $filter('filter')($scope.allRoles, { id: '!' + $scope.id });

                    if ($scope.id) {
                        checkChildRole($scope.id);
                        $scope.role = $filter('filter')($scope.allRoles, { id: $scope.id }, true)[0];
                        // $scope.role.label = $scope.role['label_' + $rootScope.language];
                        //$scope.role.description = $scope.role['description_' + $rootScope.language];

                        if (!$scope.role.master) {
                            $scope.role.reports_to = $filter('filter')($scope.allRoles, { id: $scope.role.reports_to }, true)[0].id;
                        }

                        if ($scope.role.share_data === undefined || $scope.role.share_data === null) {
                            $scope.role.share_data = false;
                        }

                        angular.forEach($scope.role.users, function (userId) {
                                var roleUser = $filter('filter')($rootScope.users, { id: userId, full_name: "!Integration User",deleted:false }, true)[0];
                                if (roleUser)
                                    $scope.roleUsers.push(roleUser);

                        });
                    }
                    else if (reportsTo) {
                        $scope.role.reports_to = reportsTo;
                    }

                    $scope.loading = false;
                });

            $scope.save = function () {

                if ($scope.roleForm.validate()) {
                    $scope.saving = true;

                    var role = angular.copy($scope.role);
                    var result = null;
                    var roleChange = $scope.role_change;
                    var successMess = 'Setup.Roles.SaveSuccess';

                    /*role.label_tr = role.label;
                    role.label_en = role.label;
                    role.description_tr = role.description;
                    role.description_en = role.description;*/

                    $rootScope.languageStringify(role);

                    if (!$scope.id) {
                        result = RoleService.create(role);
                    }
                    else {
                        result = RoleService.update(role, roleChange);
                    }

                    result.then(function () {
                        if (roleChange)
                            successMess = 'Setup.Roles.LongSaveSuccess';

                        mdToast.success($filter('translate')(successMess));
                        $rootScope.closeSide('sideModal');
                        $scope.$parent.$parent.$parent.loading = true;
                        $scope.$parent.$parent.$parent.load();
                    }).finally(function () {
                        $scope.saving = false;
                    }).catch(function () {
                        $scope.saving = false;
                        $rootScope.closeSide('sideModal');
                        $scope.$parent.$parent.$parent.load();
                    });
                } else {
                    mdToast.error($filter('translate')('Module.RequiredError'));
                }

            };

            $scope.roleUpdateChange = function (e) {
                //$scope.role.reports_to = $filter('filter')($scope.roles, { id: $scope.role.reports_to }, true)[0];
                $scope.role_change = true;
            };

            function checkChildRole(id) {
                //Gelen roleId'ye ait alt rollerin olup olmadığını kontrol ediyoruz
                var children = $filter('filter')($scope.roles, { reports_to: id });
                //Mevcut roller arasında resports_to idleri gelen rolün idsine eşit olanları filtreliyoruz.
                $scope.roles = $filter('filter')($scope.roles, { reports_to: '!' + id });

                angular.forEach(children, function (child) {
                    checkChildRole(child.id);
                });
            }

            $scope.reportToOptions = {
                dataSource: new kendo.data.HierarchicalDataSource({ data: $scope.$parent.$parent.tree }),
                dataTextField: "title",
                dataValueField: "id",
                select: function (e) {
                    var dataItem = e.sender.dataItem(e.node);
                    if (dataItem.id === $scope.id) {
                        e.preventDefault();
                        mdToast.warning($filter('translate')('Setup.Roles.Warning'));
                    }
                    else $scope.roleUpdateChange();
                },
                placeholder: $filter('translate')('Common.Select'),
            };

            $scope.usersGridOptions = {
                dataSource: {
                    data: $scope.roleUsers,
                    page: 1,
                    pageSize: 10,
                    serverPaging: false,
                    serverFiltering: false,
                    serverSorting: false,
                    schema: {
                        model: {
                            id: "id",
                            fields: {
                                full_name: { type: "string" },
                                email: { type: "string" }
                            }
                        }
                    }
                },
                scrollable: true,
                persistSelection: true,
                sortable: true,
                noRecords: true,
                pageable: {
                    refresh: true,
                    pageSize: 10,
                    pageSizes: [10, 25, 50, 100],
                    buttonCount: 5,
                    info: true
                },
                columns: [{
                    field: "email",
                    title: $filter('translate')('Setup.Users.UserEmail'),
                }, {
                    field: "full_name",
                    title: $filter('translate')('Setup.Users.UserFullName'),
                }, {
                    field: "",
                    title: $filter('translate')('Setup.Users.UserStatus'),
                    template: function (user) {
                        if (user.is_subscriber)
                            return '<span>' + $filter('translate')('Setup.Users.GroupOwner') + '</span>';
                        else if (user.id)
                            return '<span>' + $filter('translate')('Setup.Users.UserStatus1') + '</span>';
                        else
                            return '<span>' + $filter('translate')('Setup.Users.UserStatus2') + '</span>'
                    }
                }]
            };
        }
    ]);
