'use strict';

angular.module('primeapps')

    .controller('RolesController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'RolesService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, RolesService, $http, config) {

            //$scope.$parent.menuTopTitle = "Authorization";
            //$scope.$parent.activeMenu = 'authorization';
            $scope.$parent.activeMenuItem = 'roles';
            $rootScope.breadcrumblist[2].title = 'Roles';
            $scope.loading = true;

            RolesService.getAll().then(function (response) {
                $scope.roles = response.data;
                $scope.rolesState = angular.copy(response.data);
                $scope.allRoles = angular.copy(response.data);
                $scope.tree = $scope.rolesToTree(response.data);
                $scope.loading = false;
            });

            $scope.toggle = function (item) {
                item.toggle();
            };

            $scope.rolesToTree = function (roles) {
                var root = $filter('filter')(roles, { master: true })[0];
                var rootItem = $scope.getItem(root);
                var subs = $scope.getChildren(roles, rootItem);
                var tree = [];
                var items = $scope.traverseTree(roles, subs, rootItem);
                tree.push(items);

                return tree;
            };

            $scope.traverseTree = function (roles, subs, root) {
                angular.forEach(subs, function (item) {
                    var subItem = $scope.getItem(item);
                    var subs2 = $scope.getChildren(roles, subItem);

                    if (subs2.length === 0) {
                        $scope.addChild(root, subItem);
                    } else {
                        var child = $scope.traverseTree(roles, subs2, subItem);
                        $scope.addChild(root, child);
                    }
                });

                return root;
            };

            $scope.getChildren = function (roles, root) {
                var children = $filter('filter')(roles, { reports_to: root.id }, true);
                return children;
            };

            $scope.addChild = function (parent, child) {
                return parent.nodes.push(child);
            };

            $scope.getItem = function (item) {
                var subItem = {
                    "id": item.id,
                    "title": item["label_" + $scope.language],
                    "nodes": [],
                    "master": item.master
                };
                return subItem;
            };

            $scope.showDeleteForm = function (roleId) {

                $scope.selectedRoleId = roleId;
                //Delete butonuna basmadan önce adam edit butonuna basma filtreden dolayı roller tam gelmiyor.Bu yüzden tekrardan roles = rolesState yapıyoruz
                $scope.roles = angular.copy($scope.rolesState);
                $scope.transferRoles = $filter('filter')($scope.roles, { id: '!' + roleId });
                $scope.transferRoles = $filter('filter')($scope.transferRoles, { reports_to: '!' + roleId });

                ////TODO: Add loop here
                //var reportsTo = $filter('filter')($scope.transferRoles, {reports_to: roleId})[0];
                //
                //if (reportsTo)
                //    $scope.transferRoles = $filter('filter')($scope.transferRoles, {reports_to: '!' + reportsTo.id});
                //
                //reportsTo = $filter('filter')($scope.transferRoles, {reports_to: reportsTo.id})[0];
                //
                //if (reportsTo)
                //    $scope.transferRoles = $filter('filter')($scope.transferRoles, {reports_to: '!' + reportsTo.id});

                $scope.deleteModal = $scope.deleteModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/accesscontrol/roles/roleDelete.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.deleteModal.$promise.then(function () {
                    $scope.deleteModal.show();
                });
            };

            $scope.delete = function (transferRoleId) {
                if (!transferRoleId)
                    transferRoleId = $scope.transferRoles[0].id;

                $scope.roleDeleting = true;

                RolesService.delete($scope.selectedRoleId, transferRoleId)
                    .then(function () {
                        RolesService.getAll().then(function (response) {
                            $scope.roles = response.data;
                            $scope.rolesState = angular.copy(response.data);
                            $scope.allRoles = angular.copy(response.data);
                            $scope.tree = $scope.rolesToTree(response.data);

                            $scope.roleDeleting = false;
                            toastr.success($filter('translate')('Setup.Roles.DeleteSuccess'));

                            // LayoutService.getMyAccount(true);

                            $scope.deleteModal.hide();
                        });
                    })
                    .catch(function () {
                        $scope.roleDeleting = false;
                    });

            };

            $scope.showFormModal = function (id, reportsTo) {
                if (!reportsTo)
                    reportsTo = false;

                if (id) {
                    $scope.loading = true;
                    $scope.id = reportsTo ? undefined : id;//parseInt($location.search().id);
                    reportsTo = reportsTo ? id : undefined;//parseInt($location.search().reportsTo);
                    $scope.roleUsers = [];
                    $scope.role = {};
                    $scope.role.share_data = false;
                    $scope.role_change = false;
                    $scope.reportsTo_disabled = true;
                    $scope.role.editable = $scope.role.system_type === 'custom' ? true : false;

                    if (!$scope.id)
                        $scope.reportsTo_disabled = false;

                    // RolesService.getAll()
                    // .then(function (response) {
                    //  $scope.allRoles = response.data;
                    $scope.roles = $filter('filter')($scope.allRoles, { id: '!' + $scope.id });

                    if ($scope.id) {
                        checkChildRole($scope.id);
                        $scope.role = $filter('filter')($scope.allRoles, { id: $scope.id }, true)[0];
                        $scope.role.label = $scope.role['label_' + $scope.language];
                        $scope.role.description = $scope.role['description_' + $scope.language];

                        if (!$scope.role.master) {
                            $scope.role.reports_to = $filter('filter')($scope.allRoles, { id: $scope.role.reports_to }, true)[0].id;
                        }

                        if ($scope.role.share_data === undefined || $scope.role.share_data === null) {
                            $scope.role.share_data = false;
                        }

                        // angular.forEach($scope.role.users, function (userId) {
                        //     var user = $filter('filter')($rootScope.workgroup.users, { id: userId }, true)[0];
                        //
                        //     if (user)
                        //         $scope.roleUsers.push($filter('filter')($rootScope.users, { id: user.Id }, true)[0]);
                        // });
                    } else if (reportsTo) {
                        $scope.role.reports_to = reportsTo;
                    }
                    $scope.loading = false;
                    // });
                } else {
                    //Editte roller filtrelendiği için rolleri tekrardan eski değerine eşitliyoruz
                    $scope.roles = angular.copy($scope.rolesState);
                    $scope.role = {};
                    $scope.role.share_data = false;
                }

                $scope.addNewRoleFormModal = $scope.addNewRoleFormModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/accesscontrol/roles/roleForm.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.addNewRoleFormModal.$promise.then(function () {
                    $scope.addNewRoleFormModal.show();
                });
            };

            $scope.save = function (roleForm) {

                if (roleForm.$valid) {
                    $scope.saving = true;

                    $scope.changeRoleType($scope.role.editable);
                    var role = angular.copy($scope.role);
                    var result = null;
                    var roleChange = $scope.role_change;
                    var successMess = 'Setup.Roles.SaveSuccess';

                    role.label_tr = role.label;
                    role.label_en = role.label;
                    role.description_tr = role.description;
                    role.description_en = role.description;

                    if (!$scope.id) {
                        result = RolesService.create(role);
                    } else {
                        result = RolesService.update(role, roleChange);
                    }

                    result.then(function () {
                        if (roleChange)
                            successMess = 'Setup.Roles.LongSaveSuccess';

                        toastr.success($filter('translate')(successMess));
                        $scope.addNewRoleFormModal.hide();
                        $state.go('studio.app.roles');
                        $scope.loading = true;
                        RolesService.getAll().then(function (response) {
                            $scope.roles = response.data;
                            $scope.tree = $scope.rolesToTree(response.data);
                            $scope.rolesState = angular.copy(response.data);
                            $scope.allRoles = angular.copy(response.data);
                            $scope.loading = false;
                        });
                    }).finally(function () {
                        $scope.saving = false;
                    });
                } else if (roleForm.$invalid) {

                    if (roleForm.$error.required)
                        toastr.error($filter('translate')('Module.RequiredError'));

                    return;
                }
            };

            $scope.roleUpdateChange = function () {
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

            $scope.changeRoleType = function (value) {
                if (value)
                    $scope.role.system_type = 'custom';
                else
                    $scope.role.system_type = "system";
            };
        }
    ]);