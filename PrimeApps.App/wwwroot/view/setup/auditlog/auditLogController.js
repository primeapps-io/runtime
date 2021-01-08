'use strict';

angular.module('primeapps')
    .controller('AuditLogController', ['$rootScope', '$scope', 'config', '$filter', 'AuditLogService', '$sce', 'mdToast', '$state', 'helper', 'AppService',
        function ($rootScope, $scope, config, $filter, AuditLogService, $sce, mdToast, $state, helper, AppService) {
            $scope.loading = true;
            AppService.checkPermission().then(function (res) {

                if (res && res.data) {
                    var profile = JSON.parse(res.data["profile"]);
                    var customProfilePermissions = undefined;
                    if (res.data["customProfilePermissions"])
                        customProfilePermissions = JSON.parse(res.data["customProfilePermissions"]);

                    if (!profile.HasAdminRights) {
                        var auditLogIsExist = undefined;
                        if (customProfilePermissions)
                            auditLogIsExist = customProfilePermissions.permissions.indexOf('audit_log') > -1;

                        if (!auditLogIsExist) {
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
                        title: $filter('translate')('Setup.Nav.Data'),
                        link: '#/app/setup/importhistory'
                    },
                    {
                        title: $filter('translate')('Setup.Nav.Tabs.AuditLog')
                    }
                ];

                if (!$scope.allModules) {
                    AuditLogService.getDeletedModules()
                        .then(function (deletedModules) {
                            $scope.allModules = $rootScope.modules.concat(deletedModules.data);
                        });
                }

                $scope.getTime = function (time) {
                    return kendo.toString(kendo.parseDate(time), "g");
                };

                $scope.getAction = function (action) {
                    if (action.indexOf('module_') > -1) {
                        action = action.replace('module_', '');
                    }
                    return action.charAt(0).toUpperCase() + action.slice(1);
                };

                $scope.getRelatedRecord = function (dataItem) {
                    var row = "";
                    if (dataItem.audit_type === 'record' && dataItem.record_action_type !== 'deleted') {
                        row = '<div class="grid-list-button"> <span>{{dataItem.record_name}}</span><a href="#/app/record/{{dataItem.module.name}}?id={{dataItem.record_id}}"><i class= "fas fa-external-link-alt"></i></a> </div>'
                    } else if (dataItem.record_action_type === 'deleted' || dataItem.audit_type === 'setup') {
                        row = '<div class="grid-list-button"> <span>{{dataItem.record_name}}</span></div>'
                    }
                    return $sce.trustAsHtml(row);
                };

                function rowTemplate(e) {
                    return '<td class="hide-on-m2"><span>' + e.created_by.full_name + '</span></td>'
                        + '<td class="hide-on-m2"><span>' +  $rootScope.getLanguageValue(e.module.languages, 'label', 'plural')  + '</span></td>'
                        + '<td class="hide-on-m2"><span>' + $scope.getAction(e.record_action_type || e.setup_action_type) + '</span></td>'
                        + '<td class="hide-on-m2">' + $scope.getRelatedRecord(e) + '</td>'
                        + '<td class="hide-on-m2"><span>' + $scope.getTime(e.created_at) + '</span></td>'

                        + '<td class="show-on-m2">'
                        + '<div>' + $filter('translate')('Setup.AuditLog.Date') + ': <strong>' + $scope.getTime(e.created_at) + '</strong></div>'
                        + '<div>' + $filter('translate')('Setup.AuditLog.User') + ': <strong>' + e.created_by.full_name + '</strong></div>'
                        + '<div>' + $filter('translate')('Common.Module') + ': <strong>' + $rootScope.getLanguageValue(e.module.languages, 'label', 'plural') + '</strong></div>'
                        + '<div>' + $filter('translate')('Setup.AuditLog.RelatedRecord') + ': <strong>' + $scope.getRelatedRecord(e) + '</strong></div>'
                        + '<div>' + $filter('translate')('Setup.AuditLog.ActionType') + ': <strong>' + $scope.getAction(e.record_action_type || e.setup_action_type) + '</strong></div>'
                        + '</td>';
                }

                var createGrid = function () {

                    $scope.gridOptions = {
                        dataSource: {
                            type: "odata-v4",
                            page: 1,
                            pageSize: 10,
                            serverPaging: true,
                            serverFiltering: true,
                            serverSorting: true,
                            transport: {
                                read: {
                                    url: "/api/data/find_audit_logs",
                                    type: 'GET',
                                    dataType: "json",
                                    beforeSend: $rootScope.beforeSend(),
                                    data: {
                                        //userId: $scope.auditLogFilter.userId
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
                                    data.items = AuditLogService.process(data.items, $scope.allModules);
                                    return data;
                                },
                                model: {
                                    id: "id",
                                    fields: {
                                        CreatedAt: { type: "date" },
                                        RecordActionType: { type: "enums" }
                                    }
                                }
                            }
                        },
                        scrollable: false,
                        persistSelection: true,
                        noRecords: true,
                        sortable: true,
                        pageable: {
                            refresh: true,
                            pageSize: 10,
                            pageSizes: [10, 25, 50, 100],
                            buttonCount: 5,
                            info: true,
                        },
                        filterable: true,
                        filter: function (e) {
                            if (e.filter && e.field !== 'RecordActionType') {
                                for (var i = 0; i < e.filter.filters.length; i++) {
                                    e.filter.filters[i].ignoreCase = true;
                                }
                            }
                        },
                        rowTemplate: function (e) {
                            return '<tr>' + rowTemplate(e) + '</tr>';
                        },
                        altRowTemplate: function (e) {
                            return '<tr class="k-alt">' + rowTemplate(e) + '</tr>';
                        },
                        columns: [
                            {
                                field: "CreatedBy.FullName",
                                title: $filter('translate')('Setup.AuditLog.User'),
                                media: "(min-width: 575px)"
                            },
                            {
                                field: "Module.Label" + $scope.language + "plural",
                                title: $filter('translate')('Common.Module'),
                                media: "(min-width: 575px)"
                            },

                            {
                                field: "RecordActionType",
                                title: $filter('translate')('Setup.AuditLog.ActionType'),
                                values: [
                                    { text: 'Inserted', value: 'Inserted' },
                                    { text: 'Updated', value: 'Updated' },
                                    { text: 'Deleted', value: 'Deleted' }
                                ],
                                media: "(min-width: 575px)"
                            },
                            {
                                field: "RecordName",
                                title: $filter('translate')('Setup.AuditLog.RelatedRecord'),
                                media: "(min-width: 575px)"
                            },
                            {
                                field: "CreatedAt",
                                title: $filter('translate')('Setup.AuditLog.Date'),
                                filterable: {
                                    ui: function (element) {
                                        element.kendoDateTimePicker({
                                            format: '{0: dd-MM-yyyy  hh:mm}'
                                        })
                                    }
                                },
                                media: "(min-width: 575px)"
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