'use strict';

angular.module('primeapps')

    .controller('ReportController', ['$rootScope', '$scope', '$location', '$filter', '$timeout', '$state', 'helper', 'ModuleService', 'dragularService', 'ReportsService', 'operators',
        function ($rootScope, $scope, $location, $filter, $timeout, $state, helper, ModuleService, dragularService, ReportsService, operators) {

            $scope.reportModel = {};
            //$scope.reportModel.category_id = parseInt($location.search().categoryId);
            //$scope.ReportId = parseInt($location.search().Id);
            $scope.isEdit = $scope.ReportId > 0;
            $scope.clone = $location.search().clone;
            $scope.icons = ModuleService.getIcons();
            $scope.reportModel.chart = {};
            $scope.chartTypes = [
                {
                    label: $filter('translate')('Report.Chart.ColumnChart2d'),
                    name: "column2d",
                },
                {
                    label: $filter('translate')('Report.Chart.ColumnChart3d'),
                    name: "column3d",
                },
                {
                    label: $filter('translate')('Report.Chart.LineChart'),
                    name: "line",
                },
                {
                    label: $filter('translate')('Report.Chart.AreaChart'),
                    name: "area2d",
                },
                {
                    label: $filter('translate')('Report.Chart.BarChart2d'),
                    name: "bar2d",
                },
                {
                    label: $filter('translate')('Report.Chart.BarChart3d'),
                    name: "bar3d",
                },
                {
                    label: $filter('translate')('Report.Chart.PieChart'),
                    name: "pie3d",
                },
                {
                    label: $filter('translate')('Report.Chart.DoughnutChart2d'),
                    name: "doughnut2d",
                },
                {
                    label: $filter('translate')('Report.Chart.DoughnutChart3d'),
                    name: "doughnut3d",
                },
                {
                    label: $filter('translate')('Report.Chart.ScrollColumnChart'),
                    name: "scrollcolumn2d",
                },
                {
                    label: $filter('translate')('Report.Chart.ScrollLineChart'),
                    name: "scrollline2d",
                },
                {
                    label: $filter('translate')('Report.Chart.ScrollAreaChart'),
                    name: "scrollarea2d",
                },
                {
                    label: $filter('translate')('Report.Chart.FunnelChart'),
                    name: "funnel",
                },
                {
                    label: $filter('translate')('Report.Chart.PyramidChart'),
                    name: "pyramid",
                }

            ];

            $scope.wizardStep = 0;
            $scope.lookupUser = helper.lookupUser;
            $scope.costumeDate = "this_day()";
            $scope.dateFormat = [
                {
                    label: $filter('translate')('View.Second'),
                    value: "s"
                },
                {
                    label: $filter('translate')('View.Minute'),
                    value: "m"
                },
                {
                    label: $filter('translate')('View.Hour'),
                    value: "h"
                },
                {
                    label: $filter('translate')('View.Day'),
                    value: "D"
                },
                {
                    label: $filter('translate')('View.Week'),
                    value: "W"
                },
                {
                    label: $filter('translate')('View.Month'),
                    value: "M"
                },
                {
                    label: $filter('translate')('View.Year'),
                    value: "Y"
                }
            ];

            $scope.costumeDateFilter = [
                {
                    name: "thisNow",
                    label: $filter('translate')('View.Now'),
                    value: "now()"
                },
                {
                    name: "thisToday",
                    label: $filter('translate')('View.StartOfTheDay'),
                    value: "today()"
                },
                {
                    name: "thisWeek",
                    label: $filter('translate')('View.StartOfThisWeek'),
                    value: "this_week()"
                },
                {
                    name: "thisMonth",
                    label: $filter('translate')('View.StartOfThisMonth'),
                    value: "this_month()"
                },
                {
                    name: "thisYear",
                    label: $filter('translate')('View.StartOfThisYear'),
                    value: "this_year()"
                },
                {
                    name: "costume",
                    label: $filter('translate')('View.CustomDate'),
                    value: "costume"
                },
                {
                    name: "todayNextPrev",
                    label: $filter('translate')('View.FromTheBeginningOfTheDay'),
                    value: "costumeN",
                    nextprevdatetype: "D"
                },
                {
                    name: "weekNextPrev",
                    label: $filter('translate')('View.FromTheBeginningOfTheWeek'),
                    value: "costumeW",
                    nextprevdatetype: "M"
                },
                {
                    name: "monthNextPrev",
                    label: $filter('translate')('View.FromTheBeginningOfTheMonth'),
                    value: "costumeM",
                    nextprevdatetype: "M"
                },
                {
                    name: "yearNextPrev",
                    label: $filter('translate')('View.FromTheBeginningOfTheYear'),
                    value: "costumeY",
                    nextprevdatetype: "Y"
                }
            ];

            $scope.setEdit = function (reportId) {
                ReportsService.getReport(reportId).then(function (result) {
                    $scope.currentReport = result.data;
                    $scope.setStep1();
                });
            }


            $scope.dateChange = function (filter) {
                if (filter.costumeDate != 'costume' && filter.costumeDate != 'costumeN' && filter.costumeDate != 'costumeW' && filter.costumeDate != 'costumeM' && filter.costumeDate != 'costumeY') {
                    filter.value = filter.costumeDate;
                }
                if (filter.costumeDate === 'costumeN' || filter.costumeDate === 'costumeW' || filter.costumeDate === 'costumeM' || filter.costumeDate === 'costumeY') {
                    filter.value = "";
                    filter.valueX = "";
                    filter.nextprevdatetype = "";

                }

            };

            $scope.nextPrevDateChange = function (filter) {
                $scope.setCostumDate(filter);
            };

            $scope.nextPrevDateTypeChange = function (filter) {
                $scope.setCostumDate(filter);
            };

            $scope.setCostumDate = function (filter) {
                if (filter.valueX === null || filter.valueX === "" || filter.valueX === undefined) {
                    filter.value = "";
                    return false;
                }
                if (filter.nextprevdatetype === undefined) {
                    filter.nextprevdatetype = $scope.dateFormat[0].value;
                }
                switch (filter.costumeDate) {
                    case "costumeN":
                        filter.value = "today(" + filter.valueX + filter.nextprevdatetype + ")";
                        break;
                    case "costumeM":
                        filter.value = "this_month(" + filter.valueX + filter.nextprevdatetype + ")";
                        break;
                    case "costumeW":
                        filter.value = "this_week(" + filter.valueX + filter.nextprevdatetype + ")";
                        break;
                    case "costumeY":
                        filter.value = "this_year(" + filter.valueX + filter.nextprevdatetype + ")";
                        break;
                }

            };

            $scope.selectModule = function () {
                $scope.loadingFilter = true;
                $scope.numberField = [];
                $scope.reportModel.aggregations = [];
                $scope.module = angular.copy($filter('filter')($rootScope.appModules, {id: $scope.reportModel.module_id}, true)[0]);
                $scope.fields = {
                    "availableFields": [],
                    "selectedFields": []
                };
                if (!$scope.ReportId) {
                    ModuleService.getModuleFields($scope.module.name).then(function (response) {
                        var fields = response.data;
                        $scope.module.fields = ModuleService.processFields(fields);

                        angular.forEach(fields, function (item) {
                            if (item.deleted && item.multiline_type != 'large')
                                return;

                            if (item.data_type === 'lookup' && item.lookup_type != 'users') {
                                if (item.lookupModulePrimaryField) {
                                    item.name = item.name + "." + item.lookup_type + "." + item.lookupModulePrimaryField.name;
                                }
                            }

                            $scope.fields.availableFields.push(item);

                        });
                        $scope.fields.availableFields = $filter('orderBy')($scope.fields.availableFields, 'order');
                        angular.forEach(fields, function (field) {
                            if (field.data_type === 'numeric' || field.data_type === 'number' || field.data_type === 'number_auto' || field.data_type === 'currency' || field.data_type === 'number_decimal') {
                                $scope.numberField.push(angular.copy(field));
                            }
                        });

                        ModuleService.getPickItemsLists($scope.module)
                            .then(function (picklists) {
                                $scope.modulePicklists = picklists;
                                $scope.reportModel.filterList = [];

                                for (var i = 0; i < 10; i++) {
                                    var filter = {};
                                    filter.field = null;
                                    filter.operator = null;
                                    filter.value = null;
                                    filter.no = i + 1;

                                    $scope.reportModel.filterList.push(filter);
                                }

                                if ($scope.reportModel.filters) {
                                    $scope.reportModel.filters = $filter('orderBy')($scope.reportModel.filters, 'no');

                                    for (var j = 0; j < $scope.reportModel.filters.length; j++) {
                                        var name = $scope.reportModel.filters[j].field;
                                        var value = $scope.reportModel.filters[j].value;

                                        if (name.indexOf('.') > -1) {
                                            name = name.split('.')[0];
                                            $scope.reportModel.filters[j].field = name;
                                        }

                                        var field = $filter('filter')($scope.module.fields, {name: name}, true)[0];
                                        var fieldValue = null;

                                        if (!field)
                                            return;

                                        switch (field.data_type) {
                                            case 'picklist':
                                                fieldValue = $filter('filter')($scope.modulePicklists[field.picklist_id], {labelStr: value}, true)[0];
                                                break;
                                            case 'multiselect':
                                                fieldValue = [];
                                                var multiselectValue = value.split('|');

                                                angular.forEach(multiselectValue, function (picklistLabel) {
                                                    var picklist = $filter('filter')($scope.modulePicklists[field.picklist_id], {labelStr: picklistLabel}, true)[0];

                                                    if (picklist)
                                                        fieldValue.push(picklist);
                                                });
                                                break;
                                            case 'lookup':
                                                if (field.lookup_type === 'users') {
                                                    var user = {};

                                                    if (value === '0' || value === '[me]') {
                                                        user.id = 0;
                                                        user.email = '[me]';
                                                        user.full_name = $filter('translate')('Common.LoggedInUser');
                                                    } else {
                                                        if (value != '-') {
                                                            var userItem =
                                                                $filter('filter')($rootScope.users, {id: parseInt(value)}, true)[0
                                                                    ];
                                                            user.id = userItem.id;
                                                            user.email = userItem.email;
                                                            user.full_name = userItem.full_name;
                                                        }

                                                        //TODO: $rootScope.users kaldirilinca duzeltilecek
                                                        // ModuleService.getRecord('users', value)
                                                        //     .then(function (lookupRecord) {
                                                        //         fieldValue = [lookupRecord.data];
                                                        //     });
                                                    }

                                                    fieldValue = [user];
                                                } else {
                                                    fieldValue = value;
                                                }
                                                break;
                                            case 'date':
                                            case 'date_time':
                                            case 'time':
                                                if (!$scope.isCostumeDate($scope.reportModel.filters[j])) {
                                                    fieldValue = new Date(value);
                                                    $scope.reportModel.filterList[j].costumeDate = "costume";
                                                    $scope.reportModel.filters[j].costumeDate = "costume";
                                                } else {
                                                    fieldValue = $scope.reportModel.filters[j].value;
                                                    $scope.reportModel.filterList[j].costumeDate = $scope.reportModel.filters[j].costumeDate;
                                                    $scope.reportModel.filterList[j].valueX = $scope.reportModel.filters[j].valueX;
                                                    $scope.reportModel.filterList[j].nextprevdatetype = $scope.reportModel.filters[j].nextprevdatetype;
                                                }

                                                break;
                                            case 'checkbox':
                                                fieldValue = $filter('filter')($scope.modulePicklists.yes_no, {system_code: value}, true)[0];
                                                break;
                                            default :
                                                fieldValue = value;
                                                break;
                                        }

                                        $scope.reportModel.filterList[j].field = field;
                                        $scope.reportModel.filterList[j].operator = operators[$scope.reportModel.filters[j].operator];
                                        $scope.reportModel.filterList[j].value = fieldValue;

                                        if ($scope.reportModel.filters[j].operator === 'empty' || $scope.reportModel.filters[j].operator === 'not_empty') {
                                            $scope.reportModel.filterList[j].value = null;
                                            $scope.reportModel.filterList[j].disabled = true;
                                        }
                                    }
                                }
                                $scope.loadingFilter = false;
                            })
                            .finally(function () {
                                $scope.loadingModal = false;
                            });
                        
                        if($scope.reportModel.report_type ==='tabular'){
                            $scope.setFields();
                        }

                    });


                }


                $scope.multiselect = function (searchTerm, field) {
                    var picklistItems = [];

                    angular.forEach($scope.modulePicklists[field.picklist_id], function (picklistItem) {
                        if (picklistItem.inactive)
                            return;

                        if (picklistItem.labelStr.toLowerCase().indexOf(searchTerm) > -1)
                            picklistItems.push(picklistItem);
                    });

                    return picklistItems;
                };

                $scope.lookupUser = helper.lookupUser;

                var dateTimeChanged = function (filterListItem) {
                    if (filterListItem.operator) {
                        var newValue = new Date(filterListItem.value);

                        switch (filterListItem.operator.name) {
                            case 'greater':
                                newValue.setHours(23);
                                newValue.setMinutes(59);
                                newValue.setSeconds(59);
                                newValue.setMilliseconds(99);
                                break;
                            case 'less':
                                newValue.setHours(0);
                                newValue.setMinutes(0);
                                newValue.setSeconds(0);
                                newValue.setMilliseconds(0);
                                break;
                        }

                        filterListItem.value = newValue;
                    }
                };

                $scope.dateTimeChanged = function (field) {
                    dateTimeChanged(field);
                };

                $scope.isCostumeDate = function (filter) {
                    var getNumberRegex = /[^\d.-]/g;
                    if (filter.value.indexOf('now(') > -1) {
                        filter.costumeDate = "now()";
                        return true;
                    }
                    if (filter.value.indexOf('today(') > -1) {
                        if (/\d+/.test(filter.value)) {
                            filter.costumeDate = "costumeN";
                            filter.valueX = parseFloat(filter.value.replace(/[^\d.-]/g, ''));
                            filter.nextprevdatetype = filter.value.match(/([A-z])\)/g, '')[0].match(/[A-z]/g, '')[0];
                            return true;
                        } else {
                            filter.costumeDate = "today()";
                            return true;
                        }
                    }
                    if (filter.value.indexOf('this_week(') > -1) {
                        if (/\d+/.test(filter.value)) {
                            filter.costumeDate = "costumeW";
                            filter.valueX = parseFloat(filter.value.replace(/[^\d.-]/g, ''));
                            filter.nextprevdatetype = filter.value.match(/([A-z])\)/g, '')[0].match(/[A-z]/g, '')[0];
                            return true;
                        } else {
                            filter.costumeDate = "this_week()";
                            return true;
                        }
                    }

                    if (filter.value.indexOf('this_month(') > -1) {
                        if (/\d+/.test(filter.value)) {
                            filter.costumeDate = "costumeM";
                            filter.valueX = parseFloat(filter.value.replace(/[^\d.-]/g, ''));
                            filter.nextprevdatetype = filter.value.match(/([A-z])\)/g, '')[0].match(/[A-z]/g, '')[0];
                            return true;
                        } else {
                            filter.costumeDate = "this_month()";
                            return true;
                        }
                    }

                    if (filter.value.indexOf('this_year(') > -1) {
                        if (/\d+/.test(filter.value)) {
                            filter.costumeDate = "costumeY";
                            filter.valueX = parseFloat(filter.value.replace(/[^\d.-]/g, ''));
                            filter.nextprevdatetype = filter.value.match(/([A-z])\)/g, '')[0].match(/[A-z]/g, '')[0];
                            return true;
                        } else {
                            filter.costumeDate = "this_year()";
                            return true;
                        }
                    }
                    return false;

                };

                $scope.operatorChanged = function (field, index) {
                    var filterListItem = $scope.reportModel.filterList[index];

                    if (!filterListItem || !filterListItem.operator)
                        return;

                    if (field.data_type === 'date_time' && filterListItem.value && filterListItem.costumeDate === 'costume')
                        dateTimeChanged(filterListItem);

                    if (filterListItem.operator.name === 'empty' || filterListItem.operator.name === 'not_empty') {
                        filterListItem.value = null;
                        filterListItem.disabled = true;
                    } else {
                        filterListItem.disabled = false;
                    }
                };


            };

       
            
            $scope.setFields = function () {
                var containerLeft = document.querySelector('#availableFields');
                var containerRight = document.querySelector('#selectedFields');

                if ($scope.availableFields_ && $scope.selectedFields_) {
                    $scope.availableFields_.destroy();
                    $scope.selectedFields_.destroy();
                    $scope.availableFields_ = null;
                    $scope.selectedFields_ = null;
                }

                $scope.availableFields_ = dragularService([containerLeft], {
                    scope: $scope,
                    containersModel: [$scope.fields.availableFields],
                    classes: {
                        mirror: 'gu-mirror-view',
                        transit: 'gu-transit-view'
                    },
                    accepts: accepts,
                    moves: function (el, container, handle) {
                        return handle.classList.contains('dragable');
                    }
                });

                $scope.selectedFields_ = dragularService([containerRight], {
                    scope: $scope,
                    classes: {
                        mirror: 'gu-mirror-view',
                        transit: 'gu-transit-view'
                    },
                    containersModel: [$scope.fields.selectedFields]
                });
                
                function accepts(el, target, source) {
                    if (source != target) {
                        return true;
                    }
                }

                $scope.$on('dragulardrop', function (e, el) {
                    $scope.reportForm.$setValidity('field', true);
                });
            };

            $scope.aggregationChange = function (obj, field) {

                if (obj.aggregation_type === 'count' && ($scope.reportModel.report_type === 'summary' || $scope.reportModel.report_type === 'single')) {
                    $scope.reportModel.aggregations = [
                        {
                            field: "created_by",
                            aggregation_type: "count"

                        }];
                    $scope.reportModel.chart.yaxis_name = $filter('translate')('Report.count');
                    return true;
                }

                $scope.reportModel.chart.yaxis_name = field["label_" + $rootScope.language];
                if ($scope.reportModel.report_type === 'summary' || $scope.reportModel.report_type === 'single') {
                    $scope.reportModel.aggregations = [];
                } else {
                    var item = $filter('filter')($scope.reportModel.aggregations, {field: obj.field}, true)[0];
                    if (item) {
                        item.aggregation_type = obj.aggregation_type;
                        return true;
                    }

                }

                $scope.reportModel.aggregations.push(obj);
                //console.log($scope.reportModel);
            };

            $scope.removeSelectAggregation = function (field) {
                field.Aggregation = null;
                var index = $scope.reportModel.aggregations.indexOf(field);
                $scope.reportModel.aggregations.splice(index, 1);
            };

            $scope.validate = function (tab) {
                $scope.reportForm.$submitted = true;
                if (tab === 'info') {
                    $scope.wizardStep = 0;
                    return true;
                }


                if (tab === 'filter' || tab === 'area' || tab === 'summary') {
                    if ((tab === 'area' || tab === 'filter') && $scope.reportForm.report_type.$valid && $scope.reportForm.module_id.$valid && $scope.reportForm.category_id.$valid && $scope.reportForm.name.$valid) {

                        if (tab === 'filter')
                            $scope.wizardStep = 1;

                        if (tab === 'area') {
                            $scope.wizardStep = 2;
                            $scope.reportForm.$submitted = false;
                            return true
                        }

                        return true;
                    }

                    if (tab === 'summary' && $scope.reportForm.$valid) {
                        $scope.wizardStep = 3;
                        return true;
                    }

                }

                return false;
            };

            $scope.getFilterModel = function () {
                var filters = [];
                var filterList = angular.copy($scope.reportModel.filterList);

                angular.forEach(filterList, function (filterItem) {

                    if (!filterItem.field || !filterItem.operator)
                        return;

                    if (!(filterItem.operator.name === 'empty' || filterItem.operator.name === 'not_empty') && (filterItem.value == null || filterItem.value == undefined))
                        return;

                    var field = filterItem.field;
                    var fieldName = field.name;

                    var filter = {};
                    filter.field = fieldName;
                    filter.operator = filterItem.operator.name;
                    filter.value = filterItem.value;
                    filter.no = filterItem.no;

                    field = !filterItem.field.lookupModulePrimaryField || filterItem.field.lookup_type === 'users' ? filterItem.field : filterItem.field.lookupModulePrimaryField;

                    if (!(filterItem.operator.name === 'empty' || filterItem.operator.name === 'not_empty')) {
                        if (field.data_type === 'picklist')
                            filter.value = filter.value.label[$rootScope.user.tenant_language];

                        if (field.data_type === 'multiselect') {
                            var value = '';

                            angular.forEach(filter.value, function (picklistItem) {
                                value += picklistItem.label[$rootScope.user.tenant_language] + '|';
                            });

                            filter.value = value.slice(0, -1);
                        }

                        if (field.data_type === 'lookup' && field.lookup_type === 'users') {
                            if (filter.value[0].id === 0)
                                filter.value = '[me]';
                            else
                                filter.value = filter.value[0].id;
                        }

                        if (field.data_type === 'checkbox')
                            filter.value = filter.value.system_code;
                    } else {
                        filter.value = '-';
                    }

                    filters.push(filter);
                });
                return filters;
            };

            $scope.getFieldModel = function () {
                var fields = [];
                var counter = 1;
                var aggregations = angular.copy($scope.reportModel.aggregations);


                angular.forEach($scope.fields.selectedFields, function (item) {
                    fields.push(
                        {
                            "field": item.name,
                            "order": counter++
                        }
                    );
                });

                if ($scope.reportModel.report_type === 'tabular') {
                    angular.forEach(aggregations, function (item) {
                        var field = $filter('filter')($scope.fields.selectedFields, {name: item.field}, true)[0];
                        if (!field) {
                            fields.push(
                                {
                                    "field": item.field,
                                    "order": counter++
                                }
                            );
                        }
                    });
                }

                return fields;
            };


            $scope.save = function () {
                if (!$scope.reportForm.$valid)
                    return false;

                $scope.saving = true;
                var report = {};

                report.category_id = $scope.reportModel.category_id;
                report.module_id = $scope.reportModel.module_id;
                report.filters = $scope.getFilterModel();
                report.name = $scope.reportModel.name;
                report.report_type = $scope.reportModel.report_type;
                report.aggregations = $scope.reportModel.aggregations;

                if (report.report_type === 'summary') {
                    report.chart = $scope.reportModel.chart;
                    report.chart.caption = report.name;
                    report.group_field = $scope.reportModel.group_field;
                    report.sort_field = $scope.reportModel.sort_field;
                    report.sort_direction = "asc";

                    if (report.aggregations.length < 1) {
                        report.aggregations = [
                            {
                                field: "created_by",
                                aggregation_type: "count"

                            }
                        ];
                    }
                }
                if (report.report_type === 'single') {

                    if (report.aggregations.length < 1) {
                        report.aggregations = [
                            {
                                field: "created_by",
                                aggregation_type: "count"

                            }
                        ];
                    }

                    report.widget = {
                        name: report.name,
                        color: $scope.reportModel.widget ? $scope.reportModel.widget.color : null,
                        icon: $scope.reportModel.widget ? $scope.reportModel.widget.icon : null
                    }
                }
                if (report.report_type === 'tabular') {
                    report.fields = $scope.getFieldModel();
                }

                if ($scope.reportModel.sharing_type === 'custom') {
                    report.shares = [];
                    report.sharing_type = "custom";
                    angular.forEach($scope.reportModel.shares, function (user) {
                        report.shares.push(user.id);
                    });
                } else {
                    report.sharing_type = $scope.reportModel.sharing_type;
                }

                if ($scope.ReportId && !$scope.clone) {
                    report.id = $scope.ReportId;
                    ReportsService.updateReport(report).then(function (result) {
                        $scope.saving = false;
                        window.location = "#/app/reports?id=" + result.data.id;
                    });
                } else {
                    ReportsService.createReport(report).then(function (result) {
                        $scope.saving = false;
                        window.location = "#/app/reports?id=" + result.data.id;

                    });
                }


            };


            $scope.step1 = function () {
                $scope.reportForm.$submitted = true;
                if ($scope.reportForm.$valid) {
                    $scope.wizardStep = 1;
                    return true;
                }


                return false;
            };

            $scope.step2 = function () {
                $scope.reportForm.$submitted = true;
                if ($scope.reportForm.$valid) {
                    $scope.wizardStep = 2;
                    return true;
                }
                return false;
            };

            $scope.step3 = function () {
                $scope.reportForm.$submitted = true;
                if ($scope.reportForm.$valid) {
                    $scope.wizardStep = 3;

                }

            };

            $scope.stepBack = function (step) {
                $scope.wizardStep = step;
            };

            $scope.setValideStep3 = function () {
                switch ($scope.reportModel.report_type) {
                    case  "tabular":
                        break;
                    case "summary" :
                        $scope.setValide("group_field");
                        $scope.setValide("chartTypes");
                        $scope.setValide("yaxis_name");
                        $scope.setValide("xaxis_name");
                        break;
                    case "single" :
                        break;

                }

            };

            $scope.setValide = function (field) {
                // $scope.reportForm[field].$setValidity('required', false);
            };

            $scope.changeGroupField = function () {
                var field = $filter('filter')($scope.fields.availableFields, {name: $scope.reportModel.group_field}, true)[0];
                if (field)
                    $scope.reportModel.chart.xaxis_name = field["label_" + $rootScope.language];
            };

            $scope.setStep1 = function () {
                $scope.reportModel.report_type = $scope.currentReport.report_type;
                $scope.reportModel.module_id = $scope.currentReport.module_id;
                $scope.reportModel.sharing_type = $scope.currentReport.sharing_type;
                $scope.reportModel.shares = $scope.currentReport.shares;
                $scope.reportModel.name = $scope.currentReport.name;
                $scope.reportModel.filters = $scope.currentReport.filters;
                $scope.selectModule();
                if ($scope.currentReport.fields) {
                    $scope.fields.availableFields = [];
                    $scope.fields.selectedFields = [];
                    angular.forEach($scope.module.fields, function (item) {
                        if (item.deleted || !ModuleService.hasFieldDisplayPermission(item) && item.multiline_type != 'large')
                            return;
                        var field = $filter('filter')($scope.currentReport.fields, {field: item.name}, true)[0];
                        if (field) {
                            $scope.fields.selectedFields.push(item);
                        } else {
                            $scope.fields.availableFields.push(item);
                        }
                    });

                    $scope.fields.availableFields = $filter('orderBy')($scope.fields.availableFields, 'order');

                }
                if ($scope.currentReport.group_field) {
                    $scope.reportModel.group_field = $scope.currentReport.group_field;
                    $scope.reportModel.sort_field = $scope.currentReport.sort_field;
                    ReportsService.getChart($scope.currentReport.id).then(function (result) {
                        var chart = result.data.chart;
                        $scope.reportModel.chart = {
                            type: chart.chart_type,
                            yaxis_name: chart.yaxisname,
                            xaxis_name: chart.xaxisname
                        };

                    });
                }
                if ($scope.currentReport.report_type === "single") {
                    ReportsService.getWidget($scope.currentReport.id).then(function (result) {
                        var widget = result.data[0];
                        $scope.reportModel.widget = {
                            color: widget.color,
                            icon: widget.icon
                        };
                    });
                }
                $scope.reportModel.aggregations = $scope.currentReport.aggregations;


                angular.forEach($scope.numberField, function (item) {
                    var aggregation = $filter('filter')($scope.currentReport.aggregations, {field: item.name}, true)[0];
                    if (aggregation) {
                        item.Aggregation = aggregation.aggregation_type + "-" + aggregation.field;
                    }
                });
                if ($scope.currentReport.report_type === "single" || $scope.currentReport.report_type === "summary")
                    $scope.reportModel.aggregations[0].aggregation_type === 'count' ? $scope.countField = {Aggregation: 'count-created_by'} : '';

            };
            if ($scope.ReportId || $scope.clone) {
                $scope.setEdit($scope.ReportId);
            }

        }
    ]);