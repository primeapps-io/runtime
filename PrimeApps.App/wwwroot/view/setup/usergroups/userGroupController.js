'use strict';

angular.module('primeapps')

    .controller('UserGroupController', ['$rootScope', '$scope', '$filter', 'helper', 'UserGroupService', 'mdToast', '$localStorage', '$mdDialog', '$state', 'AppService',
        function ($rootScope, $scope, $filter, helper, UserGroupService, mdToast, $localStorage, $mdDialog, $state, AppService) {
            $scope.loading = true;
            AppService.checkPermission().then(function (res) {

                if (res && res.data) {
                    var profile = JSON.parse(res.data["profile"]);
                    var customProfilePermissions = undefined;
                    if (res.data["customProfilePermissions"])
                        customProfilePermissions = JSON.parse(res.data["customProfilePermissions"]);

                    if (!profile.HasAdminRights) {
                        var userGroupsIsExist = undefined;
                        if (customProfilePermissions)
                            userGroupsIsExist = customProfilePermissions.permissions.indexOf('user_groups') > -1;

                        if (!userGroupsIsExist) {
                            mdToast.error($filter('translate')('Common.Forbidden'));
                            $state.go('app.dashboard');
                        }
                    }
                }

                $rootScope.breadcrumblist = [
                    {
                        title: $filter('translate')('Layout.Menu.Dashboard'),
                        link: "#/app/dashboard"
                    },
                    {
                        title: $filter('translate')('Setup.Nav.Users'),
                        link: '#/app/setup/users'
                    },
                    {
                        title: $filter('translate')('Setup.Nav.Tabs.UserGroups')
                    }
                ];


                $scope.selectedGroup = { id: null, clone: null };
                //function getUserGroups() {
                //    UserGroupService.getAll()
                //        .then(function (userGroups) {
                //            $scope.userGroups = userGroups.data;
                //        })
                //        .finally(function () {
                //            $scope.loading = false;
                //        });
                //};

                //getUserGroups();

                $scope.delete = function (id) {
                    var confirm = $mdDialog.confirm()
                        .title($filter('translate')('Common.AreYouSure'))
                        .ok($filter('translate')('Common.Yes'))
                        .cancel($filter('translate')('Common.No'));

                    $mdDialog.show(confirm).then(function () {
                        UserGroupService.delete(id)
                            .then(function () {
                                mdToast.success($filter('translate')('Setup.UserGroups.DeleteSuccess'));
                                $scope.grid.dataSource.read();
                            });
                    });
                };

                $scope.goUrl2 = function (id) {
                    var selection = window.getSelection();

                    if (selection.toString().length === 0) {
                        $scope.showSideModal(id, false);
                    }
                };

                var optionsTemplate = '<md-menu md-position-mode="target-right target">'
                    + '<md-button class="md-icon-button" aria-label=" " ng-click="$mdMenu.open()"> <i class="fas fa-ellipsis-v"></i></md-button>'
                    + '<md-menu-content width="2" class="md-dense">'
                    + '<md-menu-item>'
                    + '<md-button ng-click="showSideModal(dataItem.id,true)">'
                    + '<i class="fas fa-copy"></i> ' + $filter('translate')('Common.Copy') + '<span></span>'
                    + '</md-button>'
                    + '</md-menu-item>'
                    + '<md-menu-item>'
                    + '<md-button id="deleteButton-{{dataItem.id}}" ng-click="delete(dataItem.id)">'
                    + '<i class="fas fa-trash"></i> <span> ' + $filter('translate')('Common.Delete') + '</span>'
                    + '</md-button>'
                    + '</md-menu-item>'
                    + '</md-menu-content>'
                    + '</md-menu>';

                var columns = [
                    {
                        field: "name",
                        title: $filter('translate')('Setup.UserGroups.GroupName'),
                        media: "(min-width: 575px)"
                    },
                    {
                        field: "description",
                        title: $filter('translate')('Setup.UserGroups.GroupDescription'),
                        media: "(min-width: 575px)"
                    },
                    {
                        title: $filter('translate')('Setup.Nav.Tabs.UserGroups'),
                        media: "(max-width: 575px)"
                    },
                    {
                        field: "",
                        title: "",
                        width: "80px",
                    },
                ];


                function generateRowTemplate(e) {
                    return '<td class="hide-on-m2"><span>' + $rootScope.getLanguageValue(e.languages, 'name') + '</span></td>'
                        + '<td class="hide-on-m2"><span>' + $rootScope.getLanguageValue(e.languages, 'description')+ '</span></td>'
                        + '<td class="show-on-m2"><div><strong>' +  $rootScope.getLanguageValue(e.languages, 'name')+ '</strong></div>'
                        + '<div>' + $rootScope.getLanguageValue(e.languages, 'description') + '</div></td>'
                        + '<td ng-click="$event.stopPropagation();" class="position-relative"><span>' + optionsTemplate + '</span></td>'
                }

                var gridCreate = function () {

                    $scope.groupGridOptions = {
                        dataSource: new kendo.data.DataSource({
                            type: "odata-v4",
                            page: 1,
                            pageSize: 10,
                            serverPaging: true,
                            serverFiltering: true,
                            serverSorting: true,
                            transport: {
                                read: {
                                    url: '/api/user_group/find',
                                    type: 'GET',
                                    dataType: "json",
                                    beforeSend: $rootScope.beforeSend()
                                }
                            },
                            requestEnd: function (e) {
                                $rootScope.processLanguages(e.response.items || []);
                                $scope.loading = false;
                                if (!$rootScope.isMobile())
                                    $(".k-pager-wrap").removeClass("k-pager-sm");
                            },
                            schema: {
                                data: "items",
                                total: "count",
                                model: {
                                    id: "id",
                                    fields: {
                                        name: { type: "string" },
                                        description: { type: "string" }
                                    }
                                }
                            }
                        }),
                        rowTemplate: function (e) {
                            return '<tr ng-click="goUrl2(dataItem.id)">' + generateRowTemplate(e) + '</tr>';
                        },
                        altRowTemplate: function (e) {
                            return '<tr class="k-alt" ng-click="goUrl2(dataItem.id)">' + generateRowTemplate(e) + '</tr>';
                        },
                        scrollable: true,
                        sortable: true,
                        noRecords: true,
                        persistSelection: true,
                        pageable: {
                            refresh: true,
                            pageSize: 10,
                            pageSizes: [10, 25, 50, 100],
                            buttonCount: 5,
                            info: true,
                        },
                        filterable: true,
                        filter: function (e) {
                            if (e.filter) {
                                for (var i = 0; i < e.filter.filters.length; i++) {
                                    e.filter.filters[i].ignoreCase = true;
                                }
                            }
                        },
                        columns: columns,
                    };
                };

                $scope.showSideModal = function (id, clone) {
                    $rootScope.sideLoad = false;
                    $scope.userGroup = {};
                    $scope.selectedGroup = { id: id, clone: clone };
                    $rootScope.buildToggler('sideModal', 'view/setup/usergroups/userGroupForm.html');
                    $scope.loadingModal = false;
                };

                angular.element(document).ready(function () {
                    gridCreate();
                    //$scope.loading = false;
                });
            });
        }
    ]);