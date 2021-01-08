'use strict';

angular.module('primeapps')
    .controller('ImportHistoryController', ['$rootScope', '$scope', '$cache', 'helper', 'ImportHistoryService', '$localStorage', '$filter', 'mdToast', '$state', '$mdDialog', 'AppService',
        function ($rootScope, $scope, $cache, helper, ImportHistoryService, $localStorage, $filter, mdToast, $state, $mdDialog, AppService) {

            AppService.checkPermission().then(function (res) {

                if (res && res.data) {
                    var profile = JSON.parse(res.data["profile"]);
                    var customProfilePermissions = undefined;
                    if (res.data["customProfilePermissions"])
                        customProfilePermissions = JSON.parse(res.data["customProfilePermissions"]);

                    if (!profile.HasAdminRights) {
                        var importHistoryIsExist = undefined;
                        if (customProfilePermissions)
                            importHistoryIsExist = customProfilePermissions.permissions.indexOf('import_history') > -1;

                        if (!importHistoryIsExist) {
                            $state.go('app.setup.auditlog');
                        }
                    }
                }

                $rootScope.breadcrumblist = [
                    {
                        title: $filter('translate')('Layout.Menu.Dashboard'),
                        link: "#/app/dashboard"
                    },
                    {
                        title: $filter('translate')('Setup.Nav.Data'),
                        link: '#/app/setup/importhistory'
                    },
                    {
                        title: $filter('translate')('Setup.Nav.Tabs.ImportHistory')
                    }
                ];

                $scope.loading = true;
                $scope.importHistoryFilter = {};

                $scope.revert = function (id) {
                    if (!id)
                        return;

                    var confirm = $mdDialog.confirm()
                        .title($filter('translate')('Setup.ImportHistory.RevertMessage'))
                        .ok($filter('translate')('Common.Yes'))
                        .cancel($filter('translate')('Common.No'));

                    $mdDialog.show(confirm).then(function () {
                        ImportHistoryService.revert(id)
                            .then(function () {
                                $scope.grid.dataSource.read();
                                //var cacheKey = imprt.module.name + '_' + imprt.module.name;
                                //$cache.remove(cacheKey);
                            });
                    });
                };

                //For Kendo UI 
                function generateRowTemplate(e) {
                    return '<td class="hide-on-m2"><span>{{dataItem.created_by.full_name}}</span></td>'
                        + '<td class="hide-on-m2"><span>' + $rootScope.getLanguageValue(e.module.languages, 'label', 'plural') + '</span></td>'
                        + '<td class="hide-on-m2"><div class="grid-list-button"> <span>{{dataItem.file_name}}</span><a href="{{dataItem.file_url}}"><i class= "fas fa-download"></i></a> </div></td>'
                        + '<td class="hide-on-m2"><span>{{dataItem.total_count}}</span></td>'
                        + '<td class="hide-on-m2"><span>' + kendo.toString(kendo.parseDate(e.created_at), "g") + '</span></td>'

                        + '<td class="show-on-m2">'
                        + '<div>' + $filter('translate')('Common.Module') + ': <strong>' + $rootScope.getLanguageValue(e.module.languages, 'label', 'plural') + '</strong></div>'
                        + '<div>' + $filter('translate')('Setup.ImportHistory.ImportedBy') + ': <strong>{{dataItem.created_by.full_name}}</strong></div>'
                        + '<div>' + $filter('translate')('Setup.ImportHistory.TotalCount') + ': <strong>{{dataItem.total_count}}</strong></div>'
                        + '<div>' + $filter('translate')('Setup.ImportHistory.ImportDate') + ': <strong>' + kendo.toString(kendo.parseDate(e.created_at), "g") + '</strong></div>'
                        + '<div>' + $filter('translate')('Setup.ImportHistory.FileName') + ': <strong><a href="dataItem.file_url" target="_blank"> {{dataItem.file_name}} <i class= "fas fa-download"></i></md-a></strong></div>'
                        + '</td>'
                        + '<td><md-button class="md-icon-button btn btn-sm btn-secondary" ng-click="revert(dataItem.id)"><i class="fas fa-undo"></i> <md-tooltip md-autohide="true" md-direction="bottom">' + $filter('translate')('Setup.ImportHistory.Revert') + '</md-tooltip></md-button></td>';
                }

                var createGrid = function () {
                    $scope.profileGridOptions = {
                        dataSource: {
                            type: "odata-v4",
                            page: 1,
                            pageSize: 10,
                            serverPaging: true,
                            serverFiltering: true,
                            serverSorting: true,
                            transport: {
                                read: {
                                    url: "/api/data/import_history",
                                    type: 'GET',
                                    dataType: "json",
                                    beforeSend: $rootScope.beforeSend(),
                                    data: {
                                        moduleId: $scope.importHistoryFilter.moduleId,
                                        userId: $scope.importHistoryFilter.userId
                                    }
                                }
                            },
                            requestEnd: function (e) {

                                $scope.loading = false;
                                if (!$rootScope.isMobile())
                                    $(".k-pager-wrap").removeClass("k-pager-sm");

                                var data = e.response;
                                if (data && data.count > 0) {
                                    for (var o = 0; o < data.count; o++) {
                                        $rootScope.processLanguage(data.items[o]);
                                    }
                                }
                            },
                            schema: {
                                data: "items",
                                total: "count",
                                parse: function (data) {
                                    var results = data.items;

                                    for (var i = 0; i < results.length; i++) {
                                        var url = decodeURIComponent(results[i].excel_url);
                                        results[i].file_name = url.slice(url.indexOf('--') + 2);
                                        results[i].file_url = results[i].excel_url ? blobUrl + '/' + results[i].excel_url.slice(0, results[i].excel_url.indexOf('--')) : '';
                                        results[i].created_at = kendo.parseDate(results[i].created_at, "");
                                    }

                                    data.items = results;
                                    return data;
                                },
                                model: {
                                    id: "id",
                                    fields: {
                                        TotalCount: { type: "number" },
                                        CreatedAt: {
                                            type: "date"
                                        }
                                    }
                                }
                            }
                        },
                        scrollable: false,
                        persistSelection: true,
                        sortable: true,
                        noRecords: true,
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
                        rowTemplate: function (e) { return '<tr>' + generateRowTemplate(e) + '</tr>' },
                        altRowTemplate: function (e) { return '<tr class="k-alt">' + generateRowTemplate(e) + '</tr>' },
                        columns: [
                            {
                                field: "CreatedBy.FullName",
                                title: $filter('translate')('Setup.ImportHistory.ImportedBy'),
                                media: "(min-width: 575px)"
                            },
                            {
                                field: "Module.Label" + $scope.language + "Plural",
                                title: $filter('translate')('Common.Module'),
                                media: "(min-width: 575px)"
                            },
                            {
                                title: $filter('translate')('Setup.ImportHistory.FileName'),
                                media: "(min-width: 575px)",
                            },
                            {
                                field: "TotalCount",
                                title: $filter('translate')('Setup.ImportHistory.TotalCount'),
                                media: "(min-width: 575px)"
                            },
                            {
                                field: "CreatedAt",
                                title: $filter('translate')('Setup.ImportHistory.ImportDate'),
                                media: "(min-width: 575px)",
                                filterable: {
                                    ui: "datetimepicker"
                                }
                            },

                            {
                                title: "Items",
                                media: "(max-width: 575px)"
                            },
                            {
                                field: "",
                                title: "",
                                filterable: false,
                                width: "80px"
                            }
                        ]
                    };
                };

                angular.element(document).ready(function () {
                    createGrid();
                });
            });
        }
    ]);