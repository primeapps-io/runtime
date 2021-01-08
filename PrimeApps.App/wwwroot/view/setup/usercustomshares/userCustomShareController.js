'use strict';

angular.module('primeapps')

    .controller('UserCustomShareController', ['$rootScope', '$scope', '$filter', 'helper', 'UserCustomShareService', 'mdToast', '$localStorage', '$mdDialog', '$state', 'AppService',
        function ($rootScope, $scope, $filter, helper, UserCustomShareService, mdToast, $localStorage, $mdDialog, $state, AppService) {
            $scope.loading = true;
            AppService.checkPermission().then(function (res) {

                if (res && res.data) {
                    var profile = JSON.parse(res.data["profile"]);
                    var customProfilePermissions = undefined;
                    if (res.data["customProfilePermissions"])
                        customProfilePermissions = JSON.parse(res.data["customProfilePermissions"]);

                    if (!profile.HasAdminRights) {
                        var userCustomSharesIsExist = undefined;
                        if (customProfilePermissions)
                            userCustomSharesIsExist = customProfilePermissions.permissions.indexOf('user_custom_shares') > -1;

                        if (!userCustomSharesIsExist) {
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
                        title: $filter('translate')('Setup.Nav.AccessControl'),
                        link: '#/app/setup/profiles'
                    },
                    {
                        title: $filter('translate')('Setup.UserCustomShares.Title')
                    }
                ]; 
                
                $scope.delete = function (id) {
                    var confirm = $mdDialog.confirm()
                        .title($filter('translate')('Common.AreYouSure'))
                        .ok($filter('translate')('Common.Yes'))
                        .cancel($filter('translate')('Common.No'));

                    $mdDialog.show(confirm).then(function () {
                        UserCustomShareService.delete(id)
                            .then(function () {
                                mdToast.success($filter('translate')('Setup.UserCustomShares.DeleteSuccess'));
                                $scope.grid.dataSource.read();
                                //getUserOwners();
                            });
                    });
                };


                $scope.goUrl2 = function (id) {
                    var selection = window.getSelection();

                    if (selection.toString().length === 0) {
                        $scope.showSideModal(id);
                    }
                };

                var optionsTemplate = '<md-button class="md-icon-button" id="deleteButton-{{dataItem.id}}" ng-click="delete(dataItem.id)">'
                    + ' <i class="fas fa-trash"></i> <md-tooltip md-autohide="true" md-direction="bottom">' + $filter('translate')('Common.Delete') + '</md-tooltip>'
                    + '</md-button>';
                var columns = [
                    {
                        field: "User.FullName",
                        title: $filter('translate')('Setup.UserCustomShares.UserAtList'),
                        media: "(min-width: 575px)"
                    },
                    {
                        field: "User.Email",
                        title: $filter('translate')('Setup.UserCustomShares.UserEmail'),
                        media: "(min-width: 575px)"
                    },
                    {
                        field: "CreatedAt",
                        title: $filter('translate')('Setup.UserCustomShares.CreatedDate'),
                        media: "(min-width: 575px)",
                        filterable: {
                            ui: "datetimepicker"
                        }
                    },
                    {
                        title: $filter('translate')('Setup.UserCustomShares.Title'),
                        media: "(max-width: 575px)"
                    },
                    {
                        field: "",
                        title: "",
                        width: "80px",
                    },
                ];

                var rowTempalte = '<td class="hide-on-m2"><span>{{ dataItem.user.full_name }}</span></td> '
                    + '<td class="hide-on-m2"><span>{{ dataItem.user.email }}</span></td> '
                    + '<td class="hide-on-m2"><span>#=kendo.toString(kendo.parseDate(created_at),"g")#</span></td>'
                    + '<td class="show-on-m2">'
                    + '<div><strong>{{ dataItem.user.full_name }}</strong></div> '
                    + '<div>{{ dataItem.user.email }}</div> '
                    + '<td ng-click="$event.stopPropagation();" class="position-relative"><span>' + optionsTemplate + '</span></td>';


                var createGrid = function () {
                    var dataSource = new kendo.data.DataSource({
                        type: "odata-v4",
                        page: 1,
                        pageSize: 10,
                        serverPaging: true,
                        serverFiltering: true,
                        serverSorting: true,
                        transport: {
                            read: {
                                url: '/api/user_custom_shares/find',
                                type: 'GET',
                                dataType: "json",
                                beforeSend: $rootScope.beforeSend()
                            }
                        },
                        schema: {
                            data: "items",
                            total: "count",
                            model: {
                                id: "id",
                                fields: {
                                    FullName: { type: "string" },
                                    Email: { type: "string" },
                                    CreatedAt: { type: "date" }
                                }
                            }
                        }
                    });
                    $scope.sharesGridOptions = {
                        dataSource: dataSource,
                        rowTemplate: '<tr ng-click="goUrl2(dataItem.id)">' + rowTempalte + '</tr>',
                        altRowTemplate: '<tr class="k-alt" ng-click="goUrl2(dataItem.id)">' + rowTempalte + '</tr>',
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
                    //After from service success
                    dataSource.fetch(function() {
                        $scope.loading = false;
                        if(!$rootScope.isMobile())
                            $(".k-pager-wrap").removeClass("k-pager-sm");
                    });
                };

                $scope.showSideModal = function (id) {
                    $rootScope.sideLoad = false;
                    $scope.userOwner = {};
                    $scope.selectedShare = { id: id };
                    $rootScope.buildToggler('sideModal', 'view/setup/usercustomshares/userCustomShareForm.html');
                    $scope.loadingModal = false;
                };

                angular.element(document).ready(function () {
                    createGrid();
                    //$scope.loading = false;
                });
            });
        }
    ]);