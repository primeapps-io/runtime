'use strict';

angular.module('primeapps')
    .controller('SignalNotificationController', ['$rootScope', '$scope', 'SignalNotificationService', 'helper', 'mdToast', '$filter', '$state',
        function ($rootScope, $scope, SignalNotificationService, helper, mdToast, $filter, $state) {

            if (!helper.hasAdminRights) {
                mdToast.error($filter('translate')('Common.Forbidden'));
                $state.go('app.dashboard');
            }

            $scope.loading = true;
            $rootScope.breadcrumblist = [
                {
                    title: $filter('translate')('Layout.Menu.Dashboard'),
                    link: "#/app/dashboard"
                },
                {
                    title: $filter('translate')('Setup.Nav.Notifications'),
                    link: '#/app/setup/signalnotification'
                },
                {
                    title: $filter('translate')('Setup.Nav.Notifications')
                }
            ];

            //For Kendo UI
            $scope.goUrl2 = function (item) {
                var selection = window.getSelection();
                if (selection.toString().length === 0) {
                    $scope.showSideModal(item.id, null);
                }
            };

            $scope.getTime = function (time) {
                return kendo.toString(kendo.parseDate(time), "g");
            };

            const createRow = function (e) {

                var moduleMessage = "-";
                if (e.module_id) {
                    $rootScope.processLanguage(e.module);
                    moduleMessage = $rootScope.getLanguageValue(e.module.languages, 'label', 'plural');
                }

                return '<td class="hide-on-m2"><span>' + $rootScope.getLanguageValue(e.languages, 'message') + '</span></td>'
                    + '<td class="hide-on-m2"><span>' + (e.user ? e.user.full_name : '') + '</span></td>'
                    + '<td class="hide-on-m2"><span>' + moduleMessage + '</span></td>'
                    + '<td class="hide-on-m2">' + (!e.record_id ? '-' : '<div class="grid-list-button"> <span>' + $filter('translate')('Setup.SignalNotification.RecordDetail') + '</span><a href="#/app/record/' + e.module.name + '?id=' + e.record_id + '"><i class= "fas fa-external-link-alt"></i></a> </div>') + '</td>'
                    + '<td class="hide-on-m2"><span>' + $filter('translate')('Setup.SignalNotification.TypeEnum.' + e.type) + '</span></td>'
                    + '<td class="hide-on-m2"><span>' + $scope.getTime(e.created_at) + '</span></td>'
                    + '<td class="hide-on-m2"><span>' + (e.status !== 'Unread' ? $filter('translate')('Setup.SignalNotification.StatusEnum.Read') : $filter('translate')('Setup.SignalNotification.StatusEnum.Unread')) + '</span></td>'
                    + '<td class="hide-on-m2"><span>' + (!e.updated_at && e.status === 'Unread' ? '-' : $scope.getTime(e.updated_at)) + '</td>'
                    + '<td class="show-on-m2">'
                    + '<div>' + $filter('translate')('Setup.SignalNotification.Message') + ': <strong>' + $rootScope.getLanguageValue(e.languages, 'message') + '</strong></div>'
                    + '<div>' + $filter('translate')('Setup.SignalNotification.User') + ': <strong>' + (e.user ? e.user.full_name : '') + '</strong></div>'
                    + '<div>' + $filter('translate')('Setup.SignalNotification.Module') + ': ' + moduleMessage + '</div>'
                    + '<div>' + $filter('translate')('Setup.SignalNotification.Record') + ': ' + (!e.record_id ? '-' : '</span><a href="#/app/record/' + e.module.name + '?id=' + e.record_id + '"><i class= "fas fa-external-link-alt"></i></a>') + '</div>'
                    + '<div>' + $filter('translate')('Setup.SignalNotification.Status') + ': ' + $filter('translate')('Setup.SignalNotification.TypeEnum.' + e.type) + '</div>'
                    + '<div>' + $filter('translate')('Setup.SignalNotification.CreatedAt') + ': ' + $scope.getTime(e.created_at) + '</div>'
                    + '<div>' + (e.status !== 'Unread' ? $filter('translate')('Setup.SignalNotification.StatusEnum.Read') : $filter('translate')('Setup.SignalNotification.StatusEnum.Unread')) + '</div>'
                    + '<div>' + $filter('translate')('Setup.SignalNotification.UpdatedAt') + ': ' + (!e.updated_at && e.status === 'Unread' ? '-' : $scope.getTime(e.updated_at)) + '</div>'
                    + '</td>';
            };

            var createGrid = function () {

                $scope.signalNotificationGridOptions = {
                    dataSource: {
                        type: "odata-v4",
                        page: 1,
                        pageSize: 10,
                        serverPaging: true,
                        serverFiltering: true,
                        serverSorting: true,
                        transport: {
                            read: {
                                url: "/api/signal_notification/find",
                                type: 'GET',
                                dataType: "json",
                                beforeSend: $rootScope.beforeSend()
                            }
                        },
                        requestEnd: function (e) {
                            $rootScope.processLanguages(e.response.items || []);
                            $scope.loading = false;
                        },
                        schema: {
                            data: "items",
                            total: "count",
                            model: {
                                id: "id",
                                fields: {
                                    Status: { type: "enums" },
                                    Type: { type: "enums" },
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
                        if (e.filter && e.field !== 'Status' && e.field !== 'Type') {
                            for (var i = 0; i < e.filter.filters.length; i++) {
                                e.filter.filters[i].ignoreCase = true;
                            }
                        }
                    },
                    rowTemplate: function (e) {
                        return '<tr>' + createRow(e) + '</tr>';
                    },
                    altRowTemplate: function (e) {
                        return '<tr class="k-alt">' + createRow(e) + '</tr>';
                    },
                    columns: [
                        {
                            media: "(min-width: 575px)",
                            field: "Message",
                            title: $filter('translate')('Setup.SignalNotification.Message'),
                        },
                        {
                            media: "(min-width: 575px)",
                            field: "User.FullName",
                            title: $filter('translate')('Setup.SignalNotification.User'),
                        },
                        {
                            media: "(min-width: 575px)",
                            field: "Module.LabelEnPlural",
                            title: $filter('translate')('Setup.SignalNotification.Module'),
                        },
                        {
                            media: "(min-width: 575px)",
                            field: "RecordId",
                            title: $filter('translate')('Setup.SignalNotification.Record'),
                        },
                        {
                            media: "(min-width: 575px)",
                            field: "Type",
                            title: $filter('translate')('Setup.SignalNotification.Type'),
                            values: [
                                { value: "Information", text: $filter('translate')('Setup.SignalNotification.TypeEnum.Information') },
                                { value: "Error", text: $filter('translate')('Setup.SignalNotification.TypeEnum.Error') },
                                { value: "Success", text: $filter('translate')('Setup.SignalNotification.TypeEnum.Success') },
                                { value: "Warning", text: $filter('translate')('Setup.SignalNotification.TypeEnum.Warning') }
                            ]
                        },
                        {
                            media: "(min-width: 575px)",
                            field: "CreatedAt",
                            title: $filter('translate')('Setup.SignalNotification.CreatedAt'),
                            filterable: {
                                ui: function (element) {
                                    element.kendoDateTimePicker({
                                        format: '{0: dd-MM-yyyy  hh:mm}'
                                    })
                                }
                            }
                        },
                        {
                            media: "(min-width: 575px)",
                            field: "Status",
                            title: $filter('translate')('Setup.SignalNotification.Status'),
                            values: [
                                { text: $filter('translate')('Setup.SignalNotification.StatusEnum.Read'), value: "Read" },
                                { text: $filter('translate')('Setup.SignalNotification.StatusEnum.Unread'), value: "Unread" }
                            ]
                        },
                        {
                            media: "(min-width: 575px)",
                            field: "UpdatedAt",
                            title: $filter('translate')('Setup.SignalNotification.UpdatedAt'),
                            filterable: {
                                ui: function (element) {
                                    element.kendoDateTimePicker({
                                        format: '{0: dd-MM-yyyy  hh:mm}'
                                    })
                                }
                            }
                        },
                        {
                            title: "Items",
                            media: "(max-width: 575px)"
                        },]

                };
            };

            angular.element(document).ready(function () {
                createGrid();
            });
        }
    ]);