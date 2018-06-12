'use strict';

angular.module('ofisim')

    .controller('DashboardController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', '$cache', 'activityTypes', 'DashboardService', 'ModuleService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', '$aside',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, $cache, activityTypes, DashboardService, ModuleService, $window, $state, $modal, dragularService, $timeout, $interval, $aside) {
            $scope.operations = operations;
            $scope.hasPermission = helper.hasPermission;
            $scope.isFullScreen = true;
            $scope.loading = true;
            $scope.showNewDashboardSetting = $filter('filter')($rootScope.moduleSettings, { key: 'new_dashboard' }, true)[0];//TODO: Delete after new dashboard development finished
            $window.scrollTo(0, 0);


            var startPageLower = $rootScope.user.profile.start_page.toLowerCase();

            if (startPageLower != 'dashboard') {
                window.location = '#/app/crm/' + startPageLower;
                return;
            };


            // $scope.show = true;
            //
            // $scope.openRejectApprovalModal = function () {
            //     $scope.rejectModal = $scope.rejectModal || $modal({
            //             scope: $scope,
            //             templateUrl: 'views/setup/help/helpPageModal.html',
            //             animation: '',
            //             backdrop: 'static',
            //             show: false,
            //             tag: 'createModal'
            //         });
            //
            //     $scope.rejectModal.$promise.then($scope.rejectModal.show);
            //
            //
            // };
            //
            // if($scope.show){
            //     $scope.openRejectApprovalModal();
            // }


            $scope.icons = ModuleService.getIcons();
            $scope.showFullScreen = function (dashletId) {
                if (!$scope.fullScreenDashlet) {
                    $scope.fullScreenDashlet = dashletId;
                }
                else {
                    $scope.fullScreenDashlet = null;
                }
                // tr
                //$scope.isFullScreen = true;
            };
            $scope.openMenu = function () {
                // $scope.dashletMenu=true
            };
            $scope.DashletTypes = [
                { label: $filter('translate')('Dashboard.Chart'), name: 'chart' },
                { label: $filter('translate')('Dashboard.Widget'), name: 'widget' }
            ];
            var colomn = $filter('translate')('Dashboard.Column');
            $scope.dashletWidths = [
                { label: '1 ' + colomn, value: 3 },
                { label: '2 ' + colomn, value: 6 },
                { label: '3 ' + colomn, value: 9 },
                { label: '4 ' + colomn, value: 12 }
            ];
            $scope.dashletHeights = [
                { label: '50 px', value: 80 },
                { label: '100 px', value: 130 },
                { label: '300 px', value: 330 },
                { label: '400 px', value: 430 },
                { label: '500 px', value: 530 },
                { label: '600 px', value: 630 },

            ];
            $scope.loadDashboard = function () {

                var setDashboard = function () {
                    DashboardService.getDashlets($scope.activeDashboard.id)
                        .then(function (dashlets) {
                            dashlets = dashlets.data;


                            for (var i = 0; i < dashlets.length; i++) {
                                var dashlet = dashlets[i];

                                if (dashlet.dashlet_type === 'chart') {
                                    dashlet.chart_item.config = {
                                        dataEmptyMessage: $filter('translate')('Dashboard.ChartEmptyMessage')
                                    };

                                    dashlet.chart_item.chart.showPercentValues = '1';
                                    dashlet.chart_item.chart.showPercentInTooltip = '0';
                                    dashlet.chart_item.chart.animateClockwise = '1';
                                    dashlet.chart_item.chart.enableMultiSlicing = '0';
                                    dashlet.chart_item.chart.isHollow = '0';
                                    dashlet.chart_item.chart.is2D = '0';
                                    dashlet.chart_item.chart.formatNumberScale = '0';

                                    if ($scope.locale === 'tr') {
                                        dashlet.chart_item.chart.decimalSeparator = ',';
                                        dashlet.chart_item.chart.thousandSeparator = '.';
                                    }

                                    var module = $filter('filter')($rootScope.modules, { id: dashlet.chart_item.chart.report_module_id }, true)[0];

                                    if (module) {
                                        var field;

                                        if (dashlet.chart_item.chart.report_aggregation_field.indexOf('.') < 0) {
                                            field = $filter('filter')(module.fields, { name: dashlet.chart_item.chart.report_aggregation_field }, true)[0];
                                        }
                                        else {
                                            var aggregationFieldParts = dashlet.chart_item.chart.report_aggregation_field.split('.');
                                            var lookupModule = $filter('filter')($rootScope.modules, { name: aggregationFieldParts[1] }, true)[0];
                                            field = $filter('filter')(lookupModule.fields, { name: aggregationFieldParts[2] }, true)[0];
                                        }

                                        if (field && field.data_type === 'currency')
                                            dashlet.chart_item.chart.numberPrefix = $rootScope.currencySymbol;

                                        if (field && (field.data_type === 'currency' || field.data_type === 'number_decimal'))
                                            dashlet.chart_item.chart.forceDecimals = '1';
                                    }
                                }
                            }

                            $scope.dashlets = dashlets;
                            $cache.put('dashlets', dashlets);

                        })
                        .finally(function () {
                            $scope.loading = false;
                        });
                };

                //TODO: Delete after new dashboard development finished
                // var setOldDashboard = function () {
                //     $http.get(config.apiUrl + 'dashboard/get_custom')
                //         .then(function (responseDashboard) {
                //             if (!responseDashboard.data)
                //                 getDashboard();
                //             else
                //                 getDashboard(responseDashboard.data);
                //         })
                //         .catch(function () {
                //             getDashboard();
                //         });
                //
                //     function getDashboard(responseDashboard) {
                //         $scope.dashboard = {};
                //
                //         var query = {
                //             sort_field: 'created_at',
                //             sort_direction: 'desc',
                //             offset: 0
                //         };
                //
                //         var query1 = angular.copy(query);
                //         var query2 = angular.copy(query);
                //         var query3 = angular.copy(query);
                //         var query4 = angular.copy(query);
                //
                //         var query1type = 'leads';
                //         var query2type = 'opportunities';
                //         var query3type = 'accounts';
                //         var query4type = 'contacts';
                //
                //         query1.limit = 5;
                //         query2.limit = 5;
                //         query3.limit = 5;
                //         query4.limit = 5;
                //
                //         $scope.dashboard.table1 = {};
                //         $scope.dashboard.table2 = {};
                //         $scope.dashboard.table3 = {};
                //         $scope.dashboard.table4 = {};
                //
                //         $scope.dashboard.table1.module = query1type;
                //         $scope.dashboard.table2.module = query2type;
                //         $scope.dashboard.table3.module = query3type;
                //         $scope.dashboard.table4.module = query4type;
                //
                //         $scope.dashboard.table2.field2 = {is_date: true};
                //
                //         if (responseDashboard) {
                //             query1type = responseDashboard.query1_type || query1type;
                //             query2type = responseDashboard.query2_type || query2type;
                //             query3type = responseDashboard.query3_type || query3type;
                //             query4type = responseDashboard.query4_type || query4type;
                //
                //             query1.limit = responseDashboard.query1_limit || 5;
                //             query2.limit = responseDashboard.query2_limit || 5;
                //             query3.limit = responseDashboard.query3_limit || 5;
                //             query4.limit = responseDashboard.query4_limit || 5;
                //
                //             $scope.dashboard.table1 = responseDashboard.table1;
                //             $scope.dashboard.table2 = responseDashboard.table2;
                //             $scope.dashboard.table3 = responseDashboard.table3;
                //             $scope.dashboard.table4 = responseDashboard.table4;
                //
                //             $scope.dashboard.table1.module = query1type;
                //             $scope.dashboard.table2.module = query2type;
                //             $scope.dashboard.table3.module = query3type;
                //             $scope.dashboard.table4.module = query4type;
                //
                //             var activityTypeList = ['task', 'event', 'call', 'gorev', 'etkinlik', 'arama'];
                //
                //             var getActivityTypePicklistItemLabel = function (value) {
                //                 switch (value) {
                //                     case 'gorev':
                //                         value = 'task';
                //                         break;
                //                     case 'etkinlik':
                //                         value = 'event';
                //                         break;
                //                     case 'arama':
                //                         value = 'call';
                //                         break;
                //                 }
                //
                //                 var activityTypePicklistItem = $filter('filter')(activityTypes, {value: value}, true)[0];
                //                 return activityTypePicklistItem.label[$rootScope.user.tenantLanguage];
                //             };
                //
                //             if (activityTypeList.indexOf(responseDashboard.query1_type) > -1) {
                //                 $scope.dashboard.table1.module = 'activities';
                //                 query1type = 'activities';
                //                 query1.filters = [{
                //                     field: 'activity_type',
                //                     operator: 'is',
                //                     value: getActivityTypePicklistItemLabel(responseDashboard.query1_type)
                //                 }]
                //             }
                //
                //             if (activityTypeList.indexOf(responseDashboard.query2_type) > -1) {
                //                 $scope.dashboard.table2.module = 'activities';
                //                 query2type = 'activities';
                //                 query2.filters = [{
                //                     field: 'activity_type',
                //                     operator: 'is',
                //                     value: getActivityTypePicklistItemLabel(responseDashboard.query2_type)
                //                 }]
                //             }
                //
                //             if (activityTypeList.indexOf(responseDashboard.query3_type) > -1) {
                //                 $scope.dashboard.table3.module = 'activities';
                //                 query3type = 'activities';
                //                 query3.filters = [{
                //                     field: 'activity_type',
                //                     operator: 'is',
                //                     value: getActivityTypePicklistItemLabel(responseDashboard.query3_type)
                //                 }]
                //             }
                //
                //             if (activityTypeList.indexOf(responseDashboard.query4_type) > -1) {
                //                 $scope.dashboard.table4.module = 'activities';
                //                 query4type = 'activities';
                //                 query4.filters = [{
                //                     field: 'activity_type',
                //                     operator: 'is',
                //                     value: getActivityTypePicklistItemLabel(responseDashboard.query4_type)
                //                 }]
                //             }
                //         }
                //
                //         if ($scope.hasPermission(query1type, $scope.operations.read)) {
                //             ModuleService.findRecords(query1type, query1)
                //                 .then(function (response) {
                //                     $scope.records1 = response.data;
                //                 });
                //         }
                //
                //         if ($scope.hasPermission(query2type, $scope.operations.read)) {
                //             ModuleService.findRecords(query2type, query2)
                //                 .then(function (response) {
                //                     $scope.records2 = response.data;
                //                 });
                //         }
                //
                //         if ($scope.hasPermission(query3type, $scope.operations.read)) {
                //             ModuleService.findRecords(query3type, query3)
                //                 .then(function (response) {
                //                     $scope.records3 = response.data;
                //                 });
                //         }
                //
                //         if ($scope.hasPermission(query4type, $scope.operations.read)) {
                //             ModuleService.findRecords(query4type, query4)
                //                 .then(function (response) {
                //                     $scope.records4 = response.data;
                //                 });
                //         }
                //     }
                // };


                if ($scope.showNewDashboardSetting === undefined || $scope.showNewDashboardSetting === null || $scope.showNewDashboardSetting.value === 'true') {
                    $scope.showNewDashboard = true;
                    setDashboard();
                }
                else//TODO: Delete after new dashboard development finished
                // setOldDashboard();

                    $scope.getSummaryJsonValue = function (data) {
                        var obj = angular.fromJson(data);
                        return obj.x;
                    };

                //Get all picklists
                for (var i = 0; i < $rootScope.modules.length; i++) {
                    ModuleService.getPicklists($rootScope.modules[i]);
                }

                $scope.$on('sample-data-removed', function (event, args) {
                    setDashboard();
                });

                $scope.widgetDetail = function (widget) {
                    if (widget.view_id != 0) {
                        //$state.go('app.crm.moduleList', {type: widget.widget_data.modulename, viewid: widget.view_id});
                        window.location = "#/app/crm/modules/" + widget.widget_data.modulename + "?viewid=" + widget.view_id;
                    } else {
                        // $state.go('app.crm.reports', {id: widget.report_id});
                        window.location = "#/app/crm/reports?id=" + widget.report_id;

                    }

                };
                $scope.chartDetail = function (reportid) {
                    if (reportid) {
                        window.location = "#/app/crm/reports?id=" + reportid;
                    }
                };

            };

            var dashletsCache = $cache.get('dashlets');
            var activeDashboard = $cache.get('activeDashboard');
            var dashboards = $cache.get('dashboards');
            var dashboardprofile = $cache.get('dashboardprofile');

            if (dashletsCache) {
                $scope.loading = false;
                $scope.dashlets = dashletsCache;
            }

            if (activeDashboard && dashboards) {
                $scope.dashboards = dashboards;
                $scope.activeDashboard = activeDashboard;
                $scope.dashboardprofile = dashboardprofile;
                $scope.loadDashboard();
            } else {
                DashboardService.getDashboards().then(function (result) {
                    $scope.dashboards = result.data;

                    $scope.activeDashboard = $filter('filter')($scope.dashboards, {
                        sharing_type: 'me',
                        user_id: $rootScope.user.ID
                    }, true)[0];

                    if (!$scope.activeDashboard) {

                        if ($rootScope.user.profile.has_admin_rights) {
                            $scope.activeDashboard = $filter('filter')($scope.dashboards, {
                                sharing_type: 'everybody'
                            }, true)[0];
                        }
                        else {
                            $scope.activeDashboard = $filter('filter')($scope.dashboards, {
                                sharing_type: 'profile',
                                profile_id: $rootScope.user.profile.id
                            }, true)[0];
                        }


                        if (!$scope.activeDashboard) {
                            $scope.activeDashboard = $filter('filter')($scope.dashboards, { sharing_type: 'everybody' }, true)[0];
                        }
                    }
                    $scope.dashboardprofile = [];
                    angular.forEach($rootScope.profiles, function (item) {
                        var profil = $filter('filter')($scope.dashboards, {
                            sharing_type: 'profile',
                            profile_id: item.Id
                        }, true)[0];
                        if (!profil)
                            $scope.dashboardprofile.push(item);

                    });
                    $cache.put('activeDashboard', $scope.activeDashboard);
                    $cache.put('dashboards', $scope.dashboards);
                    $cache.put('dashboardprofile', $scope.dashboardprofile);
                    $scope.loadDashboard();

                });

            }

            $scope.changeDashboard = function () {
                $scope.loading = true;
                $cache.put('activeDashboard', $scope.activeDashboard);
                $scope.loadDashboard();
            };

            $scope.dashboardformModal = function (dashboard) {
                if (dashboard)
                    $scope.currentDashboard = angular.copy(dashboard);
                else
                    $scope.currentDashboard = {};
                $scope.formModal = $scope.formModal || $modal({
                    scope: $scope,
                    templateUrl: 'views/app/dashboard/formModal.html',
                    animation: '',
                    backdrop: 'static',
                    show: false
                });
                $scope.formModal.$promise.then(function () {
                    $scope.formModal.show();
                });
            };
            $scope.bindDragDrop = function () {
                $timeout(function () {
                    if ($scope.drakePicklist) {
                        $scope.drakePicklist.destroy();
                        $scope.drakePicklist = null;
                    }

                    var dashletOrderContainer = document.querySelector('#dashletOrderContainer');
                    var dashletOptionContainer = document.querySelector('#dashletOptionContainer');
                    var rightTopBar = document.getElementById('rightTopBar');
                    var rightBottomBar = document.getElementById('rightBottomBar');
                    var timer;

                    $scope.drakePicklist = dragularService([dashletOrderContainer], {
                        scope: $scope,
                        containersModel: [$scope.dashlets],
                        classes: {
                            mirror: 'gu-mirror-option',
                            transit: 'gu-transit-option'
                        },
                        lockY: true,
                        moves: function (el, container, handle) {
                            return handle.classList.contains('option-handle');
                        }
                    });

                    angular.element(dashletOrderContainer).on('dragulardrop', function () {
                        //  var picklistSortOrder = $filter('filter')($scope.sortOrderTypes, {value: 'order'}, true)[0];
                        //$scope.currentField.picklist_sortorder = picklistSortOrder;
                    });

                    registerEvents(rightTopBar, dashletOptionContainer, -5);
                    registerEvents(rightBottomBar, dashletOptionContainer, 5);

                    function registerEvents(bar, container, inc, speed) {
                        if (!speed) {
                            speed = 10;
                        }

                        angular.element(bar).on('dragularenter', function () {
                            container.scrollTop += inc;

                            timer = $interval(function moveScroll() {
                                container.scrollTop += inc;
                            }, speed);
                        });

                        angular.element(bar).on('dragularleave dragularrelease', function () {
                            $interval.cancel(timer);
                        });
                    }
                }, 100);
            };

            $scope.dashboardOrderChangeModal = function () {
                $scope.currentDashlet = angular.copy($scope.dashlets);
                $scope.dashboardOrderModal = $scope.dashboardOrderModal || $aside({
                    scope: $scope,
                    templateUrl: 'views/app/dashboard/dashboardOrderChange.html',
                    animation: 'am-slide-left',
                    placement: 'left',
                    backdrop: false,
                    show: false,
                });
                $scope.dashboardOrderModal.$promise.then(function () {
                    $scope.dashboardOrderModal.show();
                    $scope.bindDragDrop();
                });
            };
            $scope.cancelChangeOrder = function () {
                $scope.dashlets = $scope.currentDashlet;
                $scope.dashboardOrderModal.hide();
            };
            $scope.saveDashboard = function (dashboardForm) {
                if (dashboardForm.$submitted && dashboardForm.$valid) {
                    {
                        var dahsbordModel = {
                            name: $scope.currentDashboard.name,
                            description: $scope.currentDashboard.description

                        };

                        if (!$scope.currentDashboard.id) {
                            dahsbordModel.profile_id = $scope.currentDashboard.profile_id;
                            dahsbordModel.sharing_type = 3;
                            $scope.loading = true;
                            var activeDashboard = angular.copy($scope.activeDashboard);
                            DashboardService.createDashbord(dahsbordModel).then(function (result) {
                                $scope.formModal.hide();
                                $cache.remove('dashlets');
                                $cache.remove('activeDashboard');
                                $cache.remove('dashboards');
                                $cache.remove('dashboardprofile');
                                $state.reload();

                            });
                        }


                    }

                }
            };
            $scope.saveDashlet = function (dashletFormModal) {
                if (dashletFormModal.$submitted && dashletFormModal.$valid) {
                    var dashletModel = {
                        name: $scope.currentDashlet.name,
                        dashlet_type: $scope.currentDashlet.dashlet_type,
                        dashboard_id: $scope.activeDashboard.id,
                        y_tile_length: $scope.currentDashlet.y_tile_length,
                        x_tile_height: $scope.currentDashlet.x_tile_height,
                    };

                    if ($scope.currentDashlet.dashlet_type === 'chart') {
                        dashletModel.chart_id = $scope.currentDashlet.board;
                    } else {
                        dashletModel.widget_id = $scope.currentDashlet.board;
                        dashletModel.y_tile_length = 3;
                        dashletModel.x_tile_height = 150;
                        dashletModel.view_id = $scope.currentDashlet.view_id;
                        dashletModel.color = $scope.currentDashlet.color;
                        dashletModel.icon = $scope.currentDashlet.icon;

                    }
                    $scope.formModalDashlet.hide();
                    $scope.loading = true;
                    if (!$scope.currentDashlet.id) {
                        dashletModel.order = $scope.dashlets ? $scope.dashlets.length : 0;
                        DashboardService.createDashlet(dashletModel).then(function (result) {
                            $scope.loadDashboard();
                        });
                    } else {
                        DashboardService.dashletUpdate($scope.currentDashlet.id, dashletModel).then(function (result) {
                            $scope.loadDashboard();
                        });
                    }
                }
            };
            $scope.dashletOrderSave = function () {
                $scope.loading = true;
                $scope.dashboardOrderModal.hide();
                DashboardService.dashletOrderChange($scope.dashlets).then(function (result) {
                    $scope.loading = false;

                });
            };


            $scope.openNewDashlet = function (dashlet) {
                $scope.currentDashlet = {};
                if (dashlet) {
                    $scope.currentDashlet = angular.copy(dashlet);
                    if (dashlet.chart_item) {
                        $scope.currentDashlet.name = $scope.currentDashlet.chart_item.chart.caption;
                        $scope.currentDashlet.board = $scope.currentDashlet.chart_item.chart.id;
                    }
                    else {
                        $scope.currentDashlet.name = $scope.currentDashlet.name;
                        $scope.currentDashlet.board = $scope.currentDashlet.widget.id;
                        if ($scope.currentDashlet.widget.view_id) {
                            $scope.currentDashlet.dataSource = 'view';
                            DashboardService.getView($scope.currentDashlet.widget.view_id).then(function (result) {
                                $scope.currentDashlet.module_id = result.data.module_id;
                                $scope.setViews();
                                $scope.currentDashlet.view_id = $scope.currentDashlet.widget.view_id;
                                $scope.currentDashlet.color = $scope.currentDashlet.widget.color;
                                $scope.currentDashlet.icon = $scope.currentDashlet.widget.icon;
                            });
                        } else {
                            $scope.currentDashlet.dataSource = 'report';
                        }
                    }
                    $scope.changeDashletType();
                }

                $scope.formModalDashlet = $scope.formModalDashlet || $modal({
                    scope: $scope,
                    templateUrl: 'views/app/dashboard/formModalDashlet.html',
                    animation: '',
                    backdrop: 'static',
                    show: false
                });
                $scope.formModalDashlet.$promise.then(function () {
                    $scope.formModalDashlet.show();
                });
            };

            $scope.changeDashletType = function () {

                if ($scope.currentDashlet.dashlet_type.name === 'chart' || $scope.currentDashlet.dashlet_type === 'chart') {
                    DashboardService.getCharts().then(function (result) {
                        $scope.boards = result.data;
                        $scope.boardLabel = $scope.DashletTypes[0].label;
                    });
                } else {
                    DashboardService.getWidgets().then(function (result) {
                        $scope.boards = result.data;
                        $scope.boardLabel = $filter('translate')('Report.Single');
                    });
                }

            };

            $scope.selectModule = function () {
                $scope.setViews();
            };

            $scope.setViews = function () {
                DashboardService.getViews($scope.currentDashlet.module_id).then(function (result) {
                    $scope.views = result.data;
                });
            };

            $scope.dashletDelete = function (id) {
                DashboardService.dashletDelete(id).then(function (result) {
                    $scope.loadDashboard();
                });
            };

            $scope.changeDashletMode = function () {
                $scope.dashletMode = $scope.dashletMode === true ? false : true;
            };
            $scope.changeView = function () {
                $scope.currentDashlet.name = $filter('filter')($scope.views, { id: $scope.currentDashlet.view_id }, true)[0]['label_' + $rootScope.language];
            };
            $scope.changeBoard = function () {
                $scope.currentDashlet.name = $filter('filter')($scope.boards, { id: $scope.currentDashlet.board }, true)[0].name;
            };

            if (typeof Tawk_API !== 'undefined') {
                Tawk_API.visitor = {
                    name: $rootScope.user.fullName,
                    email: $rootScope.user.email
                };
            }
        }
    ]);