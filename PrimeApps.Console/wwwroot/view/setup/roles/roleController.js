'use strict';

angular.module('primeapps')

    .controller('RoleController', ['$rootScope', '$scope', '$filter', 'ngToast', 'guidEmpty', '$modal', 'RoleService', 'AppService', '$state',
        function ($rootScope, $scope, $filter, ngToast, guidEmpty, $modal, RoleService, AppService, $state) {
            $scope.loading = true;

            if ($rootScope.branchAvailable && !$rootScope.user.profile.has_admin_rights) {
                ngToast.create({
                    content: $filter('translate')('Common.Forbidden'),
                    className: 'warning'
                });
                $state.go('app.dashboard');
                return;
            }

            RoleService.getAll().then(function (response) {
                $scope.roles = response.data;
                $scope.tree = $scope.rolesToTree(response.data);

                $scope.loading = false;
            });

            $scope.toggle = function (item) {
                item.toggle();
            };

            $scope.rolesToTree = function (roles) {
                var root = $filter('filter')(roles, {master: true})[0];
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
                    }
                    else {
                        var child = $scope.traverseTree(roles, subs2, subItem);
                        $scope.addChild(root, child);
                    }
                });

                return root;
            };

            $scope.getChildren = function (roles, root) {
                var children = $filter('filter')(roles, {reports_to: root.id}, true);
                return children;
            };

            $scope.addChild = function (parent, child) {
                return parent.nodes.push(child);
            };

            $scope.getItem = function (item) {
                var subItem = {
                    "id": item.id,
                    "title": item["label_"+$scope.language],
                    "nodes": []
                };
                return subItem;
            };

            $scope.showDeleteForm = function (roleId) {
                $scope.selectedRoleId = roleId;
                $scope.transferRoles = $filter('filter')($scope.roles, {id: '!' + roleId});
                $scope.transferRoles = $filter('filter')($scope.transferRoles, {reports_to: '!' + roleId});

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
                        templateUrl: 'view/setup/roles/roleDelete.html',
                        animation: '',
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

                RoleService.delete($scope.selectedRoleId, transferRoleId)
                    .then(function () {
                        RoleService.getAll().then(function (response) {
                            $scope.roles = response.data;
                            $scope.tree = $scope.rolesToTree(response.data);

                            $scope.roleDeleting = false;
                            ngToast.create({
                                content: $filter('translate')('Setup.Roles.DeleteSuccess'),
                                className: 'success'
                            });

                            AppService.getMyAccount(true);

                            $scope.deleteModal.hide();
                        });
                    })
                    .catch(function () {
                        $scope.roleDeleting = false;
                    });
            }
        }
    ]);