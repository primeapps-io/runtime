'use strict';

angular.module('primeapps')

    .controller('ReportsController', ['$rootScope', '$scope', '$location', '$filter', '$popover', 'ModuleService', 'ReportsService', 'blockUI', '$stateParams', '$modal', '$state', 'moment',
        function ($rootScope, $scope, $location, $filter, $popover, ModuleService, ReportsService, blockUI, $stateParams, $modal, $state, moment) {
            $scope.reportSearch = "";
            $scope.limits = [10, 25, 50, 100];
            $scope.getNumber = function (num) {
                var a = [];
                for (var i = 1; i < num; i++) {
                    a.push(i);
                }
                return (a);
            };
            $scope.newFilter = {};

            window.outerWidth > 768 ? $scope.multiplePanels = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9] : "";

            var myBlockUI = blockUI.instances.get('myBlockUI');

            ReportsService.getAllReports().then(function (result) {
                $scope.Reports = $filter('filter')(result.data, { 'category_id': '!!' });

                if ($stateParams.id) {
                    var report = $filter('filter')($scope.Reports, { id: parseInt($stateParams.id) }, true)[0];
                    $scope.setReport(report);
                } else {
                    $scope.Reports.length > 0 ? $scope.setReport($scope.Reports[0]) : '';
                }
            });

            $scope.getAllCategory = function () {
                ReportsService.getAllCategory().then(function (result) {
                    $scope.ReportCateogryies = result.data;

                });
            };

            $scope.getAllCategory();

            $scope.categoryDelete = function (id) {
                ReportsService.categoryDelete(id).then(function (result) {
                    $scope.getAllCategory();
                });
            };

            $scope.collapseStatus = function (index) {
                if ($scope.reportSearch.length > 3) {
                    return true;
                }
                if ($scope.multiplePanels.indexOf(index) !== -1) {
                    return true;
                }
                return false;
            };

            $scope.changeReport = function (report) {
                $scope.setReport(report);
            };

            $scope.setReport = function (report) {
                if (!report)
                    return;

                switch (report.report_type) {
                    case "summary":
                        $scope.currentReport = report;
                        $scope.setSummary(report);
                        break;
                    case "tabular":
                        $scope.currentReport = report;
                        $scope.setReportTable(report);
                        break;
                    case "single":
                        $scope.currentReport = report;
                        $scope.setSingleReport(report);
                        break;
                    default:
                        break;
                }
                document.getElementById("reportlist").scrollIntoView({ block: "end" });
            };

            $scope.setReportTable = function (report) {
                $scope.currentTable = {
                    request: {},
                    activePage: 1,
                    total: 0,
                    totalPage: 0,
                    activeLimit: $scope.limits[0],
                    loading: true
                };
                myBlockUI.start();

                $scope.reportView = report.fields;

                $scope.getReportData = ReportsService.getReportData(report.module_id, $scope.reportView);

                var totalrequest = {
                    fields: [
                        "total_count()"
                    ],
                    "limit": 1,
                    "offset": 0,
                    "filters": ReportsService.getFilters(report.filters,$rootScope.user),
                };

                $scope.currentTable.module = $scope.getReportData.module;
                $scope.getdateFileds();

                ModuleService.findRecords($scope.currentTable.module.name, totalrequest).then(function (response) {

                    if (response.data[0]) {
                        $scope.currentTable.total = response.data[0].total_count;
                    } else {
                        myBlockUI.stop();
                        $scope.currentTable.total = -1;
                        return false;
                    }


                    $scope.currentTable.request.fields = [];
                    $scope.currentTable.request.filters = ReportsService.getFilters(report.filters,$rootScope.user);

                    $scope.defaultFilter = angular.copy($scope.currentTable.request.filters);

                    $scope.currentTable.request.sort_direction = report.sort_direction;
                    $scope.currentTable.request.sort_field = report.sort_field;
                    $scope.currentTable.request.limit = $scope.limits[0];


                    if (($scope.currentTable.total % $scope.currentTable.request.limit) > 0) {
                        $scope.currentTable.totalPage = Math.ceil(($scope.currentTable.total / $scope.currentTable.request.limit) + 1);
                    } else {
                        $scope.currentTable.totalPage = Math.ceil($scope.currentTable.total / $scope.currentTable.request.limit);
                    }

                    $scope.reportView.forEach(function (item) {
                        $scope.currentTable.request.fields.push(item.field);
                    });


                    ModuleService.findRecords($scope.getReportData.module.name, $scope.currentTable.request).then(function (response) {
                        var response = response.data;
                        $scope.currentTable.displayFileds = $scope.getReportData.displayFileds;
                        ModuleService.getPicklists($scope.getReportData.module).then(function (picklist) {

                            $scope.currentTable.data = ModuleService.processRecordMulti(response, $scope.currentTable.module, picklist, $scope.reportView, $scope.currentTable.module.name);

                            $scope.aggregations = report.aggregations;

                            $scope.currentTable.aggregationsFields = ReportsService.getAggregationsFields($scope.aggregations, $scope.getReportData.module.name, $scope.getReportData.displayFileds, report.filters);
                            myBlockUI.stop();
                        });
                    });
                });
            };

            $scope.setSummary = function (report) {
                myBlockUI.start();
                $scope.reportSummary = {
                    config: {
                        dataEmptyMessage: $filter('translate')('Dashboard.ChartEmptyMessage')
                    }
                };

                $scope.current = {
                    field: "",
                    direction: ""
                };
                ReportsService.getChart(report.id).then(function (response) {
                    $scope.reportSummary = response.data;
                    $scope.reportSummary.config = {
                        dataEmptyMessage: $filter('translate')('Dashboard.ChartEmptyMessage')
                    };

                    $scope.reportSummary.chart.showPercentValues = '1';
                    $scope.reportSummary.chart.showPercentInTooltip = '0';
                    $scope.reportSummary.chart.animateClockwise = '1';
                    $scope.reportSummary.chart.enableMultiSlicing = '0';
                    $scope.reportSummary.chart.isHollow = '0';
                    $scope.reportSummary.chart.is2D = '0';
                    $scope.reportSummary.chart.formatNumberScale = '0';
                    $scope.reportSummary.chart.exportEnabled = '1';
                    $scope.reportSummary.chart.exportTargetWindow = '_self';
                    $scope.reportSummary.chart.exportFileName = $scope.reportSummary.chart['caption_'+$rootScope.user.language];
                    $scope.reportSummary.chart.exportFormats = 'PNG=' + $filter('translate')('Report.ExportImage') + '|PDF=' + $filter('translate')('Report.ExportPdf') + '|XLS=Export Chart Data';
                    $scope.reportSummary.chart['xaxisname']= $scope.reportSummary.chart['xaxisname_'+$rootScope.user.language]
                    $scope.reportSummary.chart['yaxisname']= $scope.reportSummary.chart['yaxisname_'+$rootScope.user.language];
                    $scope.reportSummary.chart['caption'] = $scope.reportSummary.chart['caption_'+$rootScope.user.language]
                    if ($scope.locale === 'tr') {
                        $scope.reportSummary.chart.decimalSeparator = ',';
                        $scope.reportSummary.chart.thousandSeparator = '.';
                    }

                    var module = $filter('filter')($rootScope.modules, { id: report.module_id }, true)[0];

                    if (module) {
                        if (report.group_field.indexOf('.') < 0) {
                            var fieldGroup = $filter('filter')(module.fields, { name: report.group_field }, true)[0];
                            $scope.reportSummary.groupField = fieldGroup['label_' + $rootScope.language];
                        }
                        else {
                            var groupFieldParts = report.group_field.split('.');
                            var lookupModuleGroup = $filter('filter')($rootScope.modules, { name: groupFieldParts[1] }, true)[0];
                            var lookupField = $filter('filter')(module.fields, { name: groupFieldParts[0] }, true)[0];
                            var fieldRelated = $filter('filter')(lookupModuleGroup.fields, { name: groupFieldParts[2] }, true)[0];
                            $scope.reportSummary.groupField = fieldRelated['label_' + $rootScope.language] + ' (' + lookupField['label_' + $rootScope.language] + ')';
                        }

                        var fieldAggregation;

                        if ($scope.reportSummary.chart.report_aggregation_field.indexOf('.') < 0) {
                            fieldAggregation = $filter('filter')(module.fields, { name: $scope.reportSummary.chart.report_aggregation_field }, true)[0];
                        }
                        else {
                            var aggregationFieldParts = $scope.reportSummary.chart.report_aggregation_field.split('.');
                            var lookupModuleAggregation = $filter('filter')($rootScope.modules, { name: aggregationFieldParts[1] }, true)[0];
                            fieldAggregation = $filter('filter')(lookupModuleAggregation.fields, { name: aggregationFieldParts[2] }, true)[0];
                        }

                        if (fieldAggregation && fieldAggregation.data_type === 'currency')
                            $scope.reportSummary.chart.numberPrefix = $rootScope.currencySymbol;

                        if (fieldAggregation && (fieldAggregation.data_type === 'currency' || fieldAggregation.data_type === 'number_decimal'))
                            $scope.reportSummary.chart.forceDecimals = '1';

                        if (fieldGroup.lookup_type === 'users') {
                            for (var i = 0; i < $scope.reportSummary.data.length; i++) {
                                $scope.reportSummary.data[i].label = $scope.getUser(parseInt($scope.reportSummary.data[i].label));
                            }

                        }
                    }
                    myBlockUI.stop();
                }
                );
            };

            $scope.getUser = function (id) {
                var user = $filter('filter')($rootScope.users, { 'id': id }, true)[0];
                if (user.full_name)
                    return user.full_name;
                return id;
            };

            $scope.setSingleReport = function (report) {
                myBlockUI.start();
                ReportsService.getWidget(report.id).then(function (response) {
                    $scope.reportSingle = response.data;
                    if ($scope.reportSingle[0].type) {
                        $scope.reportSingle[0].type = $filter('translate')('Report.' + $scope.reportSingle[0].type);
                    } else {
                        $scope.reportSingle[0].type = report.name;
                    }
                    myBlockUI.stop();

                });

            };



            $scope.table = {
                limitChange: function (limit) {
                    $scope.currentTable.request.limit = limit;
                    if (($scope.currentTable.total % $scope.currentTable.request.limit) > 0) {
                        $scope.currentTable.totalPage = Math.ceil(($scope.currentTable.total / $scope.currentTable.request.limit) + 1);
                    } else {
                        $scope.currentTable.totalPage = Math.ceil($scope.currentTable.total / $scope.currentTable.request.limit);
                    }
                    this.reset();
                    this.run();
                },
                pageChange: function (pageNumber) {
                    $scope.currentTable.request.offset = ((pageNumber - 1) * $scope.currentTable.request.limit);
                    $scope.currentTable.activePage = pageNumber;
                    this.run();
                },
                shortChange: function (fieldName) {
                    if ($scope.currentTable.request.sort_field === fieldName) {
                        $scope.currentTable.request.sort_direction = $scope.currentTable.request.sort_direction == "desc" ? 'asc' : 'desc';
                    } else {
                        $scope.currentTable.request.sort_direction = "desc";
                        $scope.currentTable.request.sort_field = fieldName;
                    }
                    this.reset();
                    this.run();
                },
                isSortBy: function (fieldName, direction) {
                    if ($scope.currentTable.request.sort_field == fieldName && $scope.currentTable.request.sort_direction == direction) {
                        return true;
                    }
                    return false;

                },
                reset: function () {
                    $scope.currentTable.request.offset = 0;
                    $scope.currentTable.activePage = 1;

                },
                filterChange: function () {
                    myBlockUI.start();

                    var totalrequest = {
                        fields: [
                            "total_count()"
                        ],
                        "limit": 1,
                        "offset": 0,
                        "filters": $scope.currentTable.request.filters,
                    };

                    ModuleService.findRecords($scope.currentTable.module.name, totalrequest).then(function (response) {

                        if (response.data[0]) {
                            $scope.currentTable.total = response.data[0].total_count;
                        } else {
                            myBlockUI.stop();
                            $scope.currentTable.total = -1;
                            return false;
                        }

                        if (($scope.currentTable.total % $scope.currentTable.request.limit) > 0) {
                            $scope.currentTable.totalPage = Math.ceil(($scope.currentTable.total / $scope.currentTable.request.limit) + 1);
                        } else {
                            $scope.currentTable.totalPage = Math.ceil($scope.currentTable.total / $scope.currentTable.request.limit);
                        }

                        ModuleService.findRecords($scope.getReportData.module.name, $scope.currentTable.request).then(function (response) {
                            var response = response.data;

                            $scope.currentTable.data = ModuleService.processRecordMulti(response, $scope.currentTable.module, null, $scope.reportView, $scope.currentTable.module.name);

                            $scope.currentTable.aggregationsFields = ReportsService.getAggregationsFields($scope.aggregations, $scope.getReportData.module.name, $scope.getReportData.displayFileds, $scope.currentTable.request.filters);
                            myBlockUI.stop();
                        });


                    });

                },
                run: function () {
                    myBlockUI.start();
                    ModuleService.findRecords($scope.currentTable.module.name, $scope.currentTable.request).then(function (response) {
                        var response = response.data;
                        $scope.currentTable.data = ModuleService.processRecordMulti(response, $scope.currentTable.module, null, $scope.reportView, $scope.currentTable.module.name);
                        myBlockUI.stop();
                    });

                },
            };
            $scope.shortChange = function (fieldName, reverse) {
                $scope.current.reverse = reverse;
                $scope.current.field = fieldName;
                $scope.reportSummary.data = $filter('orderBy')($scope.reportSummary.data, $scope.current.field, $scope.current.reverse);
            };

            //Kategori
            $scope.categoryEditModalOpen = function (type, category) {
                $scope.ReportCategory = category;
                $scope.ReportCategory.type = type
                $scope.ReportCategory. user_id=category.user_id ? category.user_id : "0",                

                $scope.categoryEditModal = $scope.categoryEditModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/reports/common/createCategoryReport.html',
                    show: false,
                    placement: 'top'
                });

                $scope.categoryEditModal.$promise.then($scope.categoryEditModal.show);
            };

            $scope.reportCategoryCreate = function (categorCreateForm, ReportCategory) {
                if (!categorCreateForm.$valid)
                    return false;
                if ($scope.ReportCategory.user_id === "0")
                    $scope.ReportCategory.user_id = "";
debugger;
                if ($scope.ReportCategory.type === 'create') {
                    ReportsService.createCategory(ReportCategory).then(function (result) {
                        if ($scope.ReportCateogryies) {
                            $scope.ReportCateogryies.push(result.data);
                            $scope.categoryEditModal.hide();
                        }
                    });
                }
                if ($scope.ReportCategory.type === 'update') {
                    ReportsService.updateCategory(ReportCategory).then(function (result) {
                        $state.reload()
                        $scope.categoryEditModal.hide();
                    });
                }
                ;

            };

            $scope.deleteReport = function (id) {
                ReportsService.deleteReport(id).then(function (result) {
                    $state.reload();
                });
            }

            $scope.getdateFileds = function () {
                $scope.dateFields = [];
                angular.forEach($scope.currentTable.module.fields, function (field) {
                    if (field.data_type === "date" || field.data_type === "date_time") {
                        $scope.dateFields.push(field);
                    }
                });

            };

            $scope.dateFiltes = [
                {
                    name: "lastYear",
                    label: $filter('translate')('Report.LastYear')
                },
                {
                    name: "thisYear",
                    label: $filter('translate')('Report.ThisYear')

                },
                {
                    name: "nextYear",
                    label: $filter('translate')('Report.NextYear')

                },
                {
                    name: "yesterDay",
                    label: $filter('translate')('Report.YesterDay')

                },
                {
                    name: "tomorrow",
                    label: $filter('translate')('Report.Tomorrow')

                },
                {
                    name: "today",
                    label: $filter('translate')('Report.Today')

                },
                {
                    name: "lastWeek",
                    label: $filter('translate')('Report.LastWeek')

                },
                {
                    name: "thisWeek",
                    label: $filter('translate')('Report.ThisWeek')

                },
                {
                    name: "nextWeek",
                    label: $filter('translate')('Report.NextWeek')

                },
                {
                    name: "lastMonth",
                    label: $filter('translate')('Report.LastMonth')

                },
                {
                    name: "thisMonth",
                    label: $filter('translate')('Report.ThisMonth')

                },
                {
                    name: "nextMonth",
                    label: $filter('translate')('Report.NextMonth')

                },
                {
                    name: "nextMonth(3)",
                    label: $filter('translate')('Report.Next3Month')

                },
                {
                    name: "nextMonth(6)",
                    label: $filter('translate')('Report.Next6Month')

                }, {
                    name: "nextMonth(12)",
                    label: $filter('translate')('Report.Next12Month')

                },
                {
                    name: "prevMonth(3)",
                    label: $filter('translate')('Report.Prev3Month')

                },
                {
                    name: "prevMonth(6)",
                    label: $filter('translate')('Report.Prev6Month')

                },
                {
                    name: "prevMonth(12)",
                    label: $filter('translate')('Report.Prev12Month')

                },
                {
                    name: "lastday(7)",
                    label: $filter('translate')('Report.Last7Day')

                },
                {
                    name: "lastday(30)",
                    label: $filter('translate')('Report.Last30Day')

                },
                {
                    name: "lastday(60)",
                    label: $filter('translate')('Report.Last60Day')

                },
                {
                    name: "lastday(90)",
                    label: $filter('translate')('Report.Last90Day')

                },
                {
                    name: "nextday(7)",
                    label: $filter('translate')('Report.Next7Day')

                },
                {
                    name: "nextday(30)",
                    label: $filter('translate')('Report.Next30Day')

                },
                {
                    name: "nextday(60)",
                    label: $filter('translate')('Report.Next60Day')

                },
                {
                    name: "nextday(90)",
                    label: $filter('translate')('Report.Next90Day')

                },
                {
                    name: "costume",
                    label: $filter('translate')('Report.Costume')
                }
            ];

            $scope.changeFilter = function () {
                var currentDate = new Date(new Date().getTime() + 24 * 60 * 60 * 1000);
                switch ($scope.newFilter.dateFilter) {
                    case "lastYear":
                        $scope.newFilter.startDate = new Date(currentDate.getFullYear() - 1, 0, 1, 0, 0, 0, 0, 0);
                        $scope.newFilter.endDate = new Date(currentDate.getFullYear() - 1, 12, 0, 0, 0, 0, 0, 0);
                        break;
                    case "thisYear":
                        $scope.newFilter.startDate = new Date(currentDate.getFullYear(), 0, 1, 0, 0, 0, 0, 0);
                        $scope.newFilter.endDate = new Date(currentDate.getFullYear(), 12, 0, 0, 0, 0, 0, 0);
                        break;
                    case "nextYear":
                        $scope.newFilter.startDate = new Date(currentDate.getFullYear() + 1, 0, 1, 0, 0, 0, 0, 0);
                        $scope.newFilter.endDate = new Date(currentDate.getFullYear() + 1, 12, 0, 0, 0, 0, 0, 0);
                        break;
                    case "yesterDay":
                        $scope.newFilter.startDate = new Date(currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate() - 2, 0, 0, 0, 0, 0);
                        $scope.newFilter.endDate = new Date(currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate() - 2, 23, 59, 0, 0, 0);
                        break;
                    case "tomorrow":
                        $scope.newFilter.startDate = new Date(currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate(), 0, 0, 0, 0, 0);
                        $scope.newFilter.endDate = new Date(currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate(), 23, 59, 0, 0, 0);
                        break;
                    case "today":
                        $scope.newFilter.startDate = new Date(currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate() - 1, 0, 0, 0, 0, 0);
                        $scope.newFilter.endDate = new Date(currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate() - 1, 23, 59, 0, 0, 0);
                        break;
                    case "lastWeek":
                        $scope.newFilter.startDate = moment().subtract(1, 'weeks').startOf('isoWeek');
                        $scope.newFilter.endDate = moment().subtract(1, 'weeks').endOf('isoWeek');

                        break;
                    case "thisWeek":
                        $scope.newFilter.startDate = moment().startOf('week');
                        $scope.newFilter.endDate = moment().endOf('week');
                        break;
                    case "nextWeek":
                        $scope.newFilter.startDate = moment().subtract(-1, 'weeks').startOf('isoWeek');
                        $scope.newFilter.endDate = moment().subtract(-1, 'weeks').endOf('isoWeek');
                        break;
                    case "thisMonth":
                        $scope.newFilter.startDate = moment().startOf('month');
                        $scope.newFilter.endDate = moment().endOf('month');
                        break;
                    case "nextMonth":
                        $scope.newFilter.startDate = moment().subtract(-1, 'month').startOf('month');
                        $scope.newFilter.endDate = moment().subtract(-1, 'month').endOf('month');
                        break;
                    case "nextMonth(3)":
                        $scope.newFilter.startDate = moment().subtract(-1, 'month').startOf('month');
                        $scope.newFilter.endDate = moment().subtract(-4, 'month').endOf('month');
                        break;
                    case "nextMonth(6)":
                        $scope.newFilter.startDate = moment().subtract(-1, 'month').startOf('month');
                        $scope.newFilter.endDate = moment().subtract(-6, 'month').endOf('month');
                        break;
                    case "nextMonth(12)":
                        $scope.newFilter.startDate = moment().subtract(-1, 'month').startOf('month');
                        $scope.newFilter.endDate = moment().subtract(-12, 'month').endOf('month');
                        break;
                    case "lastMonth":
                        $scope.newFilter.startDate = moment().subtract(1, 'month').startOf('month');
                        $scope.newFilter.endDate = moment().subtract(1, 'month').endOf('month');
                        break;
                    case "prevMonth(3)":
                        $scope.newFilter.startDate = moment().subtract(3, 'month').startOf('month');
                        $scope.newFilter.endDate = moment().subtract(1, 'month').endOf('month');
                        break;
                    case "prevMonth(6)":
                        $scope.newFilter.startDate = moment().subtract(6, 'month').startOf('month');
                        $scope.newFilter.endDate = moment().subtract(1, 'month').endOf('month');
                        break;
                    case "prevMonth(12)":
                        $scope.newFilter.startDate = moment().subtract(12, 'month').startOf('month');
                        $scope.newFilter.endDate = moment().subtract(1, 'month').endOf('month');
                        break;
                    case "lastday(7)":
                        $scope.newFilter.startDate = moment().subtract(6, 'day');
                        $scope.newFilter.endDate = moment().subtract(0, 'day');
                        break;
                    case "lastday(30)":
                        $scope.newFilter.startDate = moment().subtract(29, 'day');
                        $scope.newFilter.endDate = moment().subtract(0, 'day');
                        break;
                    case "lastday(60)":
                        $scope.newFilter.startDate = moment().subtract(59, 'day');
                        $scope.newFilter.endDate = moment().subtract(0, 'day');
                        break;
                    case "lastday(90)":
                        $scope.newFilter.startDate = moment().subtract(89, 'day');
                        $scope.newFilter.endDate = moment().subtract(0, 'day');
                        break;
                    case "nextday(7)":
                        $scope.newFilter.startDate = moment().subtract(0, 'day');
                        $scope.newFilter.endDate = moment().subtract(-6, 'day');
                        break;
                    case "nextday(30)":
                        $scope.newFilter.startDate = moment().subtract(0, 'day');
                        $scope.newFilter.endDate = moment().subtract(-29, 'day');
                        break;
                    case "nextday(60)":
                        $scope.newFilter.startDate = moment().subtract(0, 'day');
                        $scope.newFilter.endDate = moment().subtract(-59, 'day');
                        break;
                    case "nextday(90)":
                        $scope.newFilter.startDate = moment().subtract(0, 'day');
                        $scope.newFilter.endDate = moment().subtract(-89, 'day');
                        break;
                    default:
                        $scope.newFilter.startDate = null;
                        $scope.newFilter.endDate = null;
                        break

                }
            };


            $scope.setFilter = function () {
                var filters = [
                    {
                        field: $scope.newFilter.dateField,
                        operator: "greater_equal",
                        value: $scope.newFilter.startDate
                    },
                    {
                        field: $scope.newFilter.dateField,
                        operator: "less_equal",
                        value: $scope.newFilter.endDate
                    }

                ];

                $scope.currentTable.request.filters = filters.concat($scope.defaultFilter);
                $scope.table.filterChange();
            };

            $scope.clearFilter = function () {
                $scope.currentTable.request.filters = $scope.defaultFilter;
                $scope.table.filterChange();
                $scope.newFilter = {};
            };

            $scope.dateChange = function () {
                if ($scope.newFilter.dateFilter != "costume") {
                    $scope.newFilter.dateFilter = "costume";
                }
            }

        }
    ]);