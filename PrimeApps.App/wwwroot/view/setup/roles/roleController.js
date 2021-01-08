'use strict';

angular.module('primeapps')

    .controller('RoleController', ['$rootScope', '$scope', '$filter', 'RoleService', 'AppService', '$state', '$mdDialog', 'mdToast', 'helper',
        function ($rootScope, $scope, $filter, RoleService, AppService, $state, $mdDialog, mdToast, helper) {
            $scope.loading = true;
            AppService.checkPermission().then(function (res) {

                if (res && res.data) {
                    var profile = JSON.parse(res.data["profile"]);
                    var customProfilePermissions = undefined;
                    if (res.data["customProfilePermissions"])
                        customProfilePermissions = JSON.parse(res.data["customProfilePermissions"]);

                    if (!profile.HasAdminRights) {
                        var roleIsExist = undefined;
                        if (customProfilePermissions)
                            roleIsExist = customProfilePermissions.permissions.indexOf('roles') > -1;

                        if (!roleIsExist) {
                            $state.go('app.setup.usercustomshares');
                        }
                    }
                }

                $rootScope.breadcrumblist = [
                    {
                        title: $filter('translate')('Layout.Menu.Dashboard'),
                        link: "#/app/dashboard"
                    },
                    {
                        title: $filter('translate')('Setup.Nav.AccessControl'),
                        link: '#/app/setup/profiles'
                    },
                    {
                        title: $filter('translate')('Setup.Nav.Tabs.Roles')
                    }
                ];

                $scope.load = function () {
                    RoleService.getAll().then(function (response) {

                        $rootScope.processLanguages(response.data);
                        $scope.roles = response.data;
                        $scope.tree = $scope.rolesToTree(response.data);
                        $scope.loading = false;
                        createTree();

                    });
                };

                $scope.load();
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
                    return $filter('filter')(roles, { reports_to: root.id }, true);
                };

                $scope.addChild = function (parent, child) {
                    return parent.items.push(child);
                };

                $scope.getItem = function (item) {
                    return {
                        "id": item.id,
                        "title": item.languages[$rootScope.globalization.Label]["label"],
                        "items": [],
                        "expanded": true,
                        "system_type": item.system_type,
                        "master": item.master

                    };
                };

                $scope.showDeleteForm = function (role) {
                    $scope.selectedRoleId = role.id;
                    $scope.selectedRoleIsSystem = role.system_type === 'system';
                    $scope.transferRoles = $filter('filter')($scope.roles, { id: '!' + role.id });
                    $scope.transferRoles = $filter('filter')($scope.transferRoles, { reports_to: '!' + role.id });

                    $scope.transferRolesOptions = {
                        dataSource: new kendo.data.HierarchicalDataSource({
                            data: $scope.tree
                        }),
                        dataTextField: "title",
                        dataValueField: "id",
                        select: function (e) {
                            var dataItem = e.sender.dataItem(e.node);

                            if (dataItem.id === $scope.selectedRoleId) {
                                e.preventDefault();
                                mdToast.warning($filter('translate')('Setup.Roles.Warning'));
                            }
                        },
                        filter: "contains",
                        placeholder: $filter('translate')('Common.Select'),
                    };

                    $scope.transferRole = $scope.transferRoles = $filter('filter')($scope.roles, { master: true }, true)[0];

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

                    $scope.deleteModal = function () {
                        var parentEl = angular.element(document.body);
                        $mdDialog.show({
                            parent: parentEl,
                            templateUrl: 'view/setup/roles/roleDelete.html',
                            clickOutsideToClose: false,
                            scope: $scope,
                            preserveScope: true

                        });
                    };

                    $scope.deleteModal();
                };

                $scope.delete = function () {
                    if (!$scope.transferRole || angular.isObject($scope.transferRole))
                        $scope.transferRole = $scope.transferRoles.id;

                    $scope.roleDeleting = true;

                    RoleService.delete($scope.selectedRoleId, $scope.transferRole)
                        .then(function () {
                            RoleService.getAll().then(function (response) {
                                $scope.roles = response.data;
                                $rootScope.processLanguages(response.data);
                                $scope.tree = $scope.rolesToTree(response.data);

                                $scope.roleDeleting = false;
                                mdToast.success($filter('translate')('Setup.Roles.DeleteSuccess'));
                                AppService.getMyAccount(true);
                                $scope.close();
                                $scope.loading = true;
                                $scope.load();
                            });
                        })
                        .catch(function () {
                            $scope.roleDeleting = false;
                        });
                };

                $scope.showSideModal = function (id, reportId) {
                    $rootScope.sideLoad = false;
                    $scope.currentRole = {
                        id: id,
                        reportsTo: reportId
                    };

                    $rootScope.buildToggler('sideModal', 'view/setup/roles/roleForm.html');
                };

                $scope.close = function () {
                    $mdDialog.hide();
                };

                //For Kendo UI
                var createTree = function () {
                    if ($scope.rolesOptions)
                        $scope.rolesOptions.dataSource.data($scope.tree);
                    else
                        $scope.rolesOptions = {
                            dataSource: new kendo.data.HierarchicalDataSource({
                                data: $scope.tree
                            }),
                            template: function (data) {
                                return '<strong style="cursor:pointer;" ng-click="showSideModal(' + data.item.id + ',null)" flex md-truncate>' + data.item.title + '</strong > ' +
                                    '<md-button class="md-icon-button" aria-label="Delete" ng-if="!dataItem.master" ng-click="showDeleteForm(dataItem)" ng-disabled="dataItem.system_type === \'system\'"> <i class="fas fa-trash"></i></md-button>' +
                                    '<md-button class="md-icon-button" aria-label="Edit" ng-click="showSideModal(' + data.item.id + ',null)" ng-disabled="dataItem.system_type === \'system\'"> <i class="fas fa-pen"></i></md-button>' +
                                    '<md-button class="md-icon-button" aria-label="Add" ng-click="showSideModal(null,' + data.item.id + ')"> <i class="fas fa-plus"></i></md-button>'
                            },
                            dragAndDrop: false,
                            autoBind: true,
                            dataTextField: "title",
                            dataValueField: "id"
                        };
                };
            });
        }
    ]);