'use strict';

angular.module('primeapps')

    .controller('ModuleListController', ['$rootScope', '$scope', '$sce', '$filter', 'helper', '$location', '$state', '$stateParams', '$q', '$window', '$localStorage', '$cache', 'config', 'exportFile', 'operations', 'ModuleService', '$http', 'components', 'HelpService', 'mdToast', '$mdDialog', 'customScripting', '$timeout',
        function ($rootScope, $scope, $sce, $filter, helper, $location, $state, $stateParams, $q, $window, $localStorage, $cache, config, exportFile, operations, ModuleService, $http, components, HelpService, mdToast, $mdDialog, customScripting, $timeout) {

            $scope.type = $stateParams.type;
            $scope.operations = operations;
            $scope.hasPermission = helper.hasPermission;
            $scope.loading = true;
            $scope.module = $scope.modulus[$scope.type];
            $scope.lookupUser = helper.lookupUser;
            $scope.lookupProfile = helper.lookupProfile;
            $scope.lookupRole = helper.lookupRole;
            $scope.searchingDocuments = false;
            $scope.isAdmin = $rootScope.user.profile.has_admin_rights;
            $scope.hasActionButtonDisplayPermission = ModuleService.hasActionButtonDisplayPermission;
            $scope.hideDeleteAll = $filter('filter')($rootScope.deleteAllHiddenModules, $scope.type + '|' + $scope.type, true)[0];
            $scope.actionButtonDisabled = false;
            $scope.showExportButton = true;
            $scope.hasViewPermission = false;
            $scope.views = [];
            $scope.chartTypes = ModuleService.getChartsTypes();
            $scope.showHelp = false;
            $scope.primaryField = $scope.module.primaryField.name;
            $scope.hasBulkUpdatePermission = false;
            $scope.buttonsParametersData = {};
            $scope.tenantLanguage = tenantLanguage;
            $scope.viewChangeStatus = false;
            $scope.gridGroupBy = [];

            var help = $filter('filter')($scope.module.helps, {
                modal_type: 'side_modal',
                module_type: 'module_list'
            }, true)[0];

            if (help)
                $scope.showHelp = true;

            $scope.goUrl = function (url) {
                window.location = url;
            };

            $scope.goUrl2 = function (id) {
                var selection = window.getSelection();

                if (selection.toString().length === 0) {
                    if ($scope.module.primaryField && $scope.module.primaryField.external_link) {
                        window.location = $scope.module.primaryField.external_link + '?id=' + id;
                    } else {
                        window.location = '#/app/record/' + $scope.module.name + '?id=' + id;
                    }


                }
            };

            var locale = $scope.locale || $scope.language;

            if (!$scope.isAdmin) {
                if (helper.hasCustomProfilePermission('view'))
                    $scope.hasViewPermission = true;

                if (helper.hasCustomProfilePermission('record_bulk_update'))
                    $scope.hasBulkUpdatePermission = true;
            }

            $scope.reportTypeOptions = {
                dataSource: [
                    {
                        value: "summary",
                        title: $filter('translate')('Report.SummaryReport')
                    },
                    {
                        value: "tabular",
                        title: $filter('translate')('Report.ListViewReport')
                    },
                    {
                        value: "single",
                        title: $filter('translate')('Report.Single')
                    }
                ],
                dataTextField: "title",
                dataValueField: "value",
                change: function (e) {
                    //console.log(e);
                }
            };

            if (!$scope.module) {
                mdToast.warning($filter('translate')('Common.NotFound'));
                $state.go('app.dashboard');
                return;
            }

            $rootScope.breadcrumblist = [
                {
                    title: $filter('translate')('Layout.Menu.Dashboard'),
                    link: "#/app/dashboard"
                },
                {
                    title: $rootScope.getLanguageValue($scope.module.languages, 'label', 'plural')
                }
            ];


            $scope.bulkUpdate = {};
            $scope.filter = {};

            if (!$scope.hasPermission($scope.type, $scope.operations.read)) {
                mdToast.error($filter('translate')('Common.Forbidden'));
                $state.go('app.dashboard');
                return;
            }

            $scope.changeAggregationType = function () {
                $scope.changeView(false, true);
                changeViewState();
            };

            ModuleService.getActionButtons($scope.module.id)
                .then(function (actionButtons) {
                    $scope.actionButtons = actionButtons;
                    $scope.toolbarOptions = {
                        resizable: true,
                        items: []
                    };
                    $rootScope.processLanguages(actionButtons);

                    for (var i = 0; actionButtons.length > i; i++) {
                        const name = $rootScope.getLanguageValue(actionButtons[i].languages, 'label');

                        if (actionButtons[i].trigger !== 'Detail' && actionButtons[i].trigger !== 'Form' && actionButtons[i].trigger !== 'Relation') {
                            if (actionButtons[i].type === 'Scripting' && $scope.hasActionButtonDisplayPermission(actionButtons[i], true)) {
                                var item = {
                                    template: '<md-button class="btn ' + actionButtons[i].color + '"   ng-click="runScript(' + i + ')"  aria-label="' + name + '" > <i class="' + actionButtons[i].icon + '"></i> <span>' + name + '</span></md-button>',
                                    overflowTemplate: '<md-button ng-click="runScript(' + i + ')"   class="action-dropdown-item"><i class="' + actionButtons[i].icon + '"></i><span> ' + name + '</span></md-button>',
                                    overflow: "auto"
                                };

                                $scope.toolbarOptions.items.push(item);
                            }

                            if (actionButtons[i].type === 'Webhook' && $scope.hasActionButtonDisplayPermission(actionButtons[i], true)) {
                                var item = {
                                    template: '<md-button class="btn ' + actionButtons[i].color + '"  ng-click="webhookRequest(' + i + ')"    aria-label="' + name + '" > <i class="' + actionButtons[i].icon + '"></i> <span>' + name + '</span></md-button>',
                                    overflowTemplate: '<md-button ng-click="webhookRequest(' + i + ')"  class="action-dropdown-item" ><i class="' + actionButtons[i].icon + '"></i><span> ' + name + ' </span></md-button>',
                                    overflow: "auto"
                                };

                                $scope.toolbarOptions.items.push(item);
                            }

                            if (actionButtons[i].type === 'ModalFrame' && $scope.hasActionButtonDisplayPermission(actionButtons[i], true)) {
                                var item = {
                                    template: '<md-button class="btn ' + actionButtons[i].color + '"  ng-click="showModuleFrameModal(\'' + actionButtons[i].url + '\')"   aria-label="' + name + '" > <i class="' + actionButtons[i].icon + '"></i> <span>' + name + '</span></md-button>',
                                    overflowTemplate: '<md-button  ng-click="showModuleFrameModal(\'' + actionButtons[i].url + '\')"   class="action-dropdown-item"><i class="' + actionButtons[i].icon + '"></i><span> ' + name + ' </span></md-button>',
                                    overflow: "auto"
                                };

                                $scope.toolbarOptions.items.push(item);
                            }

                            if (actionButtons[i].type === 'CallMicroflow' && $scope.hasActionButtonDisplayPermission(actionButtons[i], true)) {
                                var item = {
                                    template: '<md-button class="btn ' + actionButtons[i].color + '"  ng-click="runMicroflow(' + actionButtons[i].microflow_id + ',' + i + ')"   aria-label="' + name + '" > <i class="' + actionButtons[i].icon + '"></i> <span>' + name + '</span></md-button>',
                                    overflowTemplate: '<md-button ng-click="runMicroflow(' + actionButtons[i].microflow_id + ',' + i + ')"   class="action-dropdown-item"><i class="' + actionButtons[i].icon + '"></i><span> ' + name + '</span></md-button>',
                                    overflow: "auto"
                                };

                                $scope.toolbarOptions.items.push(item);
                            }
                        }

                    }

                    // $scope.actionButtons = $filter('filter')(actionButtons, function (actionButton) {
                    //     return actionButton.trigger !== 'Detail' && actionButton.trigger !== 'Form' && actionButton.trigger !== 'Relation';
                    // }, true);
                });


            $scope.fields = [];
            $scope.fieldskey = {};
            $scope.selectedRows = [];
            $scope.selectedRecords = [];
            $scope.isAllSelected = false;
            $scope.currentUser = ModuleService.processUser($rootScope.user);

            $scope.currentDayMin = helper.getCurrentDateMin().toISOString();
            $scope.currentDayMax = helper.getCurrentDateMax().toISOString();
            $scope.currentHour = helper.floorMinutes(new Date());


            //var cacheKey = $scope.module.name + '_' + $scope.module.name;

            $scope.allFields = [];
            $scope.numberFields = [];
            $scope.picklistFields = [];
            $scope.dateFields = [];
            $scope.calculationFields = [];
            $scope.aggregationTypes = [
                {
                    label: $filter('translate')('Report.sum'),
                    value: "sum"
                },
                {
                    label: $filter('translate')('Report.avg'),
                    value: "avg"
                },
                {
                    label: $filter('translate')('Report.min'),
                    value: "min"
                },
                {
                    label: $filter('translate')('Report.max'),
                    value: "max"
                }
            ];

            for (var i = 0; i < $scope.module.fields.length; i++) {
                var field = $scope.module.fields[i];

                if (ModuleService.hasFieldDisplayPermission(field)) {
                    $scope.allFields.push(Object.assign({}, field));
                    $scope.fieldskey[field.name] = field;
                    if (field.data_type === 'numeric' || field.data_type === 'number' || field.data_type === 'number_auto' || field.data_type === 'currency' || field.data_type === 'number_decimal') {
                        $scope.numberFields.push(field);
                    } else if (field.data_type === 'date' || field.data_type === 'date_time') {
                        $scope.dateFields.push(field);
                    } else if (field.data_type === 'picklist') {
                        $scope.picklistFields.push(field);
                    }
                }
            }

            $scope.parseInt = function (number) {
                return parseInt(number);
            };

            $scope.colorPaletOptions = {
                columns: 6,
                palette: [
                    "#D24D57", "#BE90D4", "#5AABE3", "#87D37C", "#F4D03E", "#B8BEC2",
                    "#DC3023", "#8e44ad", "#19B5FE", "#25A65B", "#FFB61E", "#959EA4",
                    "#C3272B", "#763668", "#1F4688", "#006442", "#CA6924", "#4D5C66",
                ]
            };


            function getFreezeFields(freezeDependencies, records, isForFindRequest) {
                var freezeFields = $scope.freezeFields || [];
                for (var j = 0; j < freezeDependencies.length; j++) {
                    var dependency = freezeDependencies[j];
                    var field = $filter('filter')($scope.module.fields, {
                        name: dependency.parent_field,
                        deleted: false
                    }, true)[0];

                    if (field && isForFindRequest) {
                        freezeFields.push(field);
                    } else if (field) {
                        getRecordsForFreeze(records, dependency, field);
                    }
                }
                if (isForFindRequest)
                    return freezeFields;
            }

            function getRecordsForFreeze(records, dependency, field) {
                for (var o = 0; o < records.length; o++) {
                    if (!records[o].freeze) {
                        var processRecord = ModuleService.processRecordSingle(angular.copy(records[o]), $scope.module, $scope.modulePicklists);
                        var type = false;

                        if (processRecord.process_status === 1)
                            return;

                        if (dependency.values_array && dependency.values_array.length > 0) {
                            for (var l = 0; l < dependency.values_array.length; l++) {
                                var value = parseInt(dependency.values_array[l]);
                                if (processRecord[field.name] && (value === processRecord[field.name] || value === processRecord[field.name].id))
                                    type = true;
                            }
                        } else if (processRecord[field.name] === 'Yes') {
                            //checkbox
                            type = true;
                        }

                        records[o].freeze = type;
                    }
                }
            }

            function getFreezeDependencies(dependencies) {
                return $filter('filter')($scope.module.dependencies, { dependency_type: 'freeze', deleted: false }, true);
            }

            var isFreeze = function (records) {
                var freezeDependencies = $scope.freezeDependencies || getFreezeDependencies($scope.module.dependencies);

                if (freezeDependencies && freezeDependencies.length > 0) {
                    getFreezeFields(freezeDependencies, records, false);

                    if ($scope.isAllSelected) {
                        $scope.isAllSelected = false;
                        $scope.selectAll(undefined, records);
                    }
                }
            };

            $scope.chartFilter = function (element) {

                if (!$rootScope.activeView.aggregation.aggregation_type || !$rootScope.activeView.aggregation.field) {
                    $rootScope.activeView.aggregation = {
                        aggregation_type: "count",
                        field: "created_by"
                    };

                    $scope.totalCount = true;
                }

                var groupField = $scope.fieldskey[$rootScope.activeView.group_field];
                if (element === 'groupField') {
                    $scope.reportSummary.chart.xaxisname = $rootScope.getLanguageValue(groupField.languages, 'label');
                    $scope.reportSummary.chart.yaxisname = $scope.totalCount ? $filter('translate')('Report.count') : ('Report.' + $rootScope.activeView.aggregation.aggregation_type);
                }

                if (element === 'aggregationType')
                    $scope.reportSummary.chart.yaxisname = $scope.totalCount ? $filter('translate')('Report.count') : $filter('translate')('Report.' + $rootScope.activeView.aggregation.aggregation_type);

                if (groupField && groupField.data_type === 'lookup') {
                    var groupFieldName = groupField.name + "." + groupField.lookup_type + "." + groupField.lookupModulePrimaryField.name;
                }

                var filterModel = {
                    module_name: $scope.module.name,
                    aggregation_field: $rootScope.activeView.aggregation.aggregation_type + "(" + $rootScope.activeView.aggregation.field + ")",
                    chart_type: $scope.reportSummary.chart.chart_type,
                    aggregation_type: $rootScope.activeView.aggregation.aggregation_type,
                    group_by: groupFieldName || $rootScope.activeView.group_field,
                    filters: $rootScope.activeView.filters,
                    filter_logic: $rootScope.activeView.filter_logic
                };

                ModuleService.chartFilter(filterModel).then(function (result) {
                    $scope.reportSummary.data = result.data;
                    if (!$scope.reportSummary.isNew) {
                        $scope.initilazSummaryReport();
                    }
                });
            };

            $scope.widgetFilter = function () {

                if (!$rootScope.activeView.aggregation.aggregation_type || !$rootScope.activeView.aggregation.field) {
                    $rootScope.activeView.aggregation = {
                        aggregation_type: "count",
                        field: "created_by"
                    };
                    $scope.totalCount = true;
                }


                var aggregationFiled = $rootScope.activeView.aggregation.aggregation_type + "(" + $rootScope.activeView.aggregation.field + ")";
                var requestModel = {
                    fields: [aggregationFiled],
                    limit: 1,
                    offset: 0,
                    filters: $rootScope.activeView.filters
                };

                ModuleService.findRecords($scope.module.name, requestModel).then(function (res) {
                    $rootScope.activeView.value = res.data[0][$rootScope.activeView.aggregation.aggregation_type];
                    $scope.loading = false;
                });
            };

            $scope.filtera = {
                "group": {
                    "logic": "and",
                    "filters": [],
                    "level": 1
                }
            };

            var filterIndex = 0;
            var newfilters = [];
            $scope.count = 0;

            function convertHelper(obj) {
                if (obj['group'] != null && obj['group']['filters'] != null && obj['group']['filters'].length > 0) {
                    $scope.logic = '';
                    var filtersCount = $filter('filter')(obj['group']['filters'], function (filter) {
                        return filter.key;
                    }, true);
                    for (var index = 0; index < obj['group']['filters'].length; index++) {
                        var filter = obj['group']['filters'][index];
                        if (filter['group'] != null) {
                            $scope.logic += (filter['group']['level'] !== 1 && index !== 0 ? ' ' + filter['group']['logic'] + ' ' : '');
                            $scope.logic += convertHelper(filter);
                        } else {
                            if (filter.operator != null && filter.operator !== "") {
                                filterIndex++;
                                var filter = $scope.processViewFilter(filter, filterIndex);
                                if (!filter)
                                    continue;

                                newfilters.push(filter);
                                if (obj['group'].level === 1 && index === 0 && index !== filtersCount.length - 1) $scope.logic += '(';
                                $scope.logic += (index === 0 ? '' : ' ' + obj['group']['logic'] + ' ') + newfilters.length;
                                if (obj['group'].level === 1 && index === filtersCount.length - 1 && index !== 0) $scope.logic += ')';
                            }

                        }
                    }
                }
                if ($scope.logic && !$scope.logic.contains('undefined'))
                    $scope.logic = '(' + $scope.logic + ')';

                return $scope.logic;
            }

            $scope.processViewFilter = function (filter, index) {
                var field = $scope.fieldskey[filter.field];
                var newFilterObj = {
                    field: filter.field,
                    operator: filter.operator,
                    no: index
                };

                switch (field.data_type) {
                    case "date_time":
                    case "date":
                        if (filter.value && filter.value.name) {
                            switch (filter.value.value) {
                                case "costumeN":
                                    if (!filter.valueX && !filter.nextprevdatetype && !filter.nextprevdatetype.value)
                                        return false;
                                    newFilterObj.value = "today(" + filter.valueX + filter.nextprevdatetype.value + ")";
                                    break;
                                case "costumeM":
                                    if (!filter.valueX && !filter.nextprevdatetype && !filter.nextprevdatetype.value)
                                        return false;
                                    newFilterObj.value = "this_month(" + filter.valueX + filter.nextprevdatetype.value + ")";
                                    break;
                                case "costumeW":
                                    if (!filter.valueX && !filter.nextprevdatetype && !filter.nextprevdatetype.value)
                                        return false;
                                    newFilterObj.value = "this_week(" + filter.valueX + filter.nextprevdatetype.value + ")";
                                    break;
                                case "costumeY":
                                    if (!filter.valueX && !filter.nextprevdatetype && !filter.nextprevdatetype.value)
                                        return false;
                                    newFilterObj.value = "this_year(" + filter.valueX + filter.nextprevdatetype.value + ")";
                                    break;
                                default:
                                    if (filter.value.name === 'costume') {
                                        if (!filter.costumeDate)
                                            return false;

                                        newFilterObj.value = filter.costumeDate;
                                    } else {
                                        newFilterObj.value = filter.value.value;
                                    }
                                    break;
                            }
                        }
                        break;
                    case "multiselect":
                        var value = [];
                        for (var i = 0; i < filter.value.length; i++) {
                            var picklistItem = filter.value[i];
                            value.push($rootScope.getLanguageValue(picklistItem.languages, 'label'));
                        }
                        newFilterObj.value = value;
                        break;
                    case "tag":
                        var value = '';
                        for (var i = 0; i < filter.value.length; i++) {
                            value += filter.value[i].text + '|';
                        }
                        newFilterObj.value = value.slice(0, -1);
                        break;
                    case "lookup":
                        if (field.lookup_type === 'users') {
                            newFilterObj.value = filter.value.id;
                        } else {
                            newFilterObj.field = field.name + "." + field.lookup_type + "." + field.lookupModulePrimaryField.name;
                            newFilterObj.value = filter.value;
                        }
                        break;
                    case "picklist":
                        newFilterObj.value = filter.value.labelStr;
                        break;
                    default:
                        newFilterObj.value = filter.value;
                        break
                }

                if (newFilterObj.operator === 'empty' || newFilterObj.operator === 'not_empty')
                    newFilterObj.value = '-';

                return newFilterObj;
            };


            $scope.setListGrid = function (type) {
                var viewGrid = $rootScope.activeView.report_type === "tabular" ? $rootScope.activeView : $scope.newGridData;

                if (type === "report") {
                    viewGrid.report_type = "tabular";
                    viewGrid.view_type = "report";
                } else {
                    viewGrid.report_type = "";
                    viewGrid.view_type = "grid";
                }

                viewGrid.aggregations = null;
                viewGrid.aggregation = null;
                $scope.changeView(viewGrid, false);
            };

            $scope.reportTypeChange = function () {
                changeViewState();
                switch ($rootScope.activeView.report_type) {
                    case "tabular":
                        $scope.setListGrid("report");
                        break;
                    case "summary":
                        $scope.reportSummary = {
                            config: {
                                dataEmptyMessage: $filter('translate')('Dashboard.ChartEmptyMessage')
                            }
                        };

                        $scope.totalCount = true;

                        $rootScope.activeView.aggregation = {
                            field: "",
                            aggregation_type: ""
                        };

                        $scope.reportSummary.config = {
                            dataEmptyMessage: $filter('translate')('Dashboard.ChartEmptyMessage')
                        };
                        $scope.reportSummary.isNew = true;

                        $scope.reportSummary.chart = {
                            languages: {}
                        };

                        $scope.reportSummary.chart[$rootScope.globalization.Label] = {
                            xaxisname: '',
                            yaxisname: ''
                        };

                        $scope.reportSummary.chart.chart_type = "column3d";
                        $scope.reportSummary.chart.report_aggregation_field = $rootScope.activeView.aggregation.field;


                        $scope.reportSummary.chart.showToolTip = $rootScope.isMobile() ? '0' : '1';
                        $scope.reportSummary.chart.showPercentValues = '1';
                        $scope.reportSummary.chart.showPercentInTooltip = '0';
                        $scope.reportSummary.chart.animateClockwise = '1';
                        $scope.reportSummary.chart.enableMultiSlicing = '0';
                        $scope.reportSummary.chart.isHollow = '0';
                        $scope.reportSummary.chart.is2D = '0';
                        $scope.reportSummary.chart.formatNumberScale = '0';
                        $scope.reportSummary.chart.exportEnabled = '1';
                        $scope.reportSummary.chart.exportTargetWindow = '_self';
                        $scope.reportSummary.chart.exportFileName = $scope.reportSummary.chart.languages ? $rootScope.getLanguageValue($scope.reportSummary.chart.languages, 'caption') : '';
                        $scope.reportSummary.chart.exportFormats = 'PNG=' + $filter('translate')('Report.ExportImage') + '|PDF=' + $filter('translate')('Report.ExportPdf') + '|XLS=Export Chart Data';
                        break;
                    case "single":
                        $scope.totalCount = true;
                        $rootScope.activeView.aggregation = {
                            field: "created_by",
                            aggregation_type: "count"
                        };
                        $scope.widgetFilter();
                        break;
                }
            };

            $scope.changeViewType = function (type) {

                if ($rootScope.activeView.view_type === type)
                    return false;

                if (type === 'grid') {
                    $rootScope.activeView.groups_json = null;
                    $rootScope.activeView.aggregations = [];
                    $scope.gridGroupBy = [];
                    $scope.setListGrid("grid");
                } else if (type === 'report') {
                    $rootScope.activeView.view_type = 'report';
                    $rootScope.activeView.report_type = 'tabular';
                    $rootScope.activeView.groups_json = null;
                    $scope.gridGroupBy = [];
                    $scope.setListGrid("report");
                    $scope.setViewAggregationsFields($scope.viewFields.selectedFields);
                } else if (type === 'calendar') {
                    $rootScope.activeView.view_type = 'calendar';
                    $rootScope.activeView.report_type = "";
                    $rootScope.activeView.groups_json = null;
                    $scope.gridGroupBy = [];
                    $scope.setViewCalendar();
                } else if (type === 'kanban') {
                    $rootScope.activeView.view_type = 'kanban';
                    $rootScope.activeView.report_type = "";
                    $rootScope.activeView.groups_json = null;
                    $scope.gridGroupBy = [];
                    $rootScope.activeView.settings = null;
                    $scope.setViewKanban();
                }

                changeViewState();
            };

            $scope.viewFilter = function () {
                filterIndex = 0;
                newfilters = [];
                var filterLogic = convertHelper($scope.filtera);

                $rootScope.activeView.filters = newfilters;
                $rootScope.activeView.filter_logic = newfilters.length > 1 ? $scope.logic : "";
                $scope.changeView(false, true);

                if ($rootScope.activeView.report_type === 'single') {
                    $scope.widgetFilter();
                    return true;
                }

                if ($rootScope.activeView.report_type === 'summary') {
                    $scope.chartFilter();
                    return true;
                }

                if ($rootScope.activeView.view_type === 'calendar') {
                    $scope.refreshViewCalendar();
                    return false;
                }

            };

            //$scope.widgetFilter();

            $scope.initilazSummaryReport = function () {
                if ($scope.module) {
                    if ($rootScope.activeView.group_field.indexOf('.') < 0) {
                        var fieldGroup = $scope.fieldskey[$rootScope.activeView.group_field];
                        if (fieldGroup)
                            $scope.reportSummary.groupField = $rootScope.getLanguageValue(fieldGroup.languages, 'label')
                    } else {
                        var groupFieldParts = $rootScope.activeView.group_field.split('.');
                        var lookupModuleGroup = $filter('filter')($rootScope.modules, { name: groupFieldParts[1] }, true)[0];
                        var lookupField = $filter('filter')($scope.module.fields, { name: groupFieldParts[0] }, true)[0];
                        var fieldRelated = $filter('filter')(lookupModuleGroup.fields, { name: groupFieldParts[2] }, true)[0];
                        $scope.reportSummary.groupField = $rootScope.getLanguageValue(fieldRelated.languages, 'label') + ' (' + $rootScope.getLanguageValue(lookupField.languages, 'label') + ')'
                    }

                    if ($rootScope.activeView.aggregations && angular.isArray($rootScope.activeView.aggregations)) {
                        $rootScope.activeView.aggregation = $rootScope.activeView.aggregations[0];
                    }

                    var fieldAggregation;

                    if ($rootScope.activeView.aggregation.field.indexOf('.') < 0) {
                        fieldAggregation = $scope.fieldskey[$rootScope.activeView.aggregation.field]
                    } else {
                        var aggregationFieldParts = $rootScope.activeView.aggregation.field.split('.');
                        var lookupModuleAggregation = $scope.module[aggregationFieldParts[1]];
                        fieldAggregation = $filter('filter')(lookupModuleAggregation.fields, { name: aggregationFieldParts[2] }, true)[0];
                    }

                    if (fieldAggregation && fieldAggregation.data_type === 'currency')
                        $scope.reportSummary.chart.numberPrefix = $rootScope.currencySymbol;

                    if (fieldAggregation && (fieldAggregation.data_type === 'currency' || fieldAggregation.data_type === 'number_decimal'))
                        $scope.reportSummary.chart.forceDecimals = '1';

                }
                $scope.loading = false;
            };

            $scope.setSummary = function (view) {

                $scope.reportSummary = {
                    config: {
                        dataEmptyMessage: $filter('translate')('Dashboard.ChartEmptyMessage')
                    }
                };
                //Burası ortak katman yapılabilir

                $scope.current = {
                    field: "",
                    direction: ""
                };

                ModuleService.getChart(view.id)
                    .then(function (response) {

                        var languages = angular.isObject(response.data.chart.languages) ? response.data.chart.languages : JSON.parse(response.data.chart.languages);
                        $scope.reportSummary = response.data;
                        $scope.reportSummary.config = {
                            dataEmptyMessage: $filter('translate')('Dashboard.ChartEmptyMessage')
                        };
                        $scope.reportSummary.chart.showToolTip = $rootScope.isMobile() ? '0' : '1';
                        $scope.reportSummary.chart.showPercentValues = '1';
                        $scope.reportSummary.chart.showPercentInTooltip = '0';
                        $scope.reportSummary.chart.animateClockwise = '1';
                        $scope.reportSummary.chart.enableMultiSlicing = '0';
                        $scope.reportSummary.chart.isHollow = '0';
                        $scope.reportSummary.chart.is2D = '0';
                        $scope.reportSummary.chart.formatNumberScale = '0';
                        $scope.reportSummary.chart.exportEnabled = '1';
                        $scope.reportSummary.chart.exportTargetWindow = '_self';
                        $scope.reportSummary.chart.exportFileName = $rootScope.getLanguageValue($scope.reportSummary.chart.languages, 'caption');
                        $scope.reportSummary.chart.exportFormats = 'PNG=' + $filter('translate')('Report.ExportImage') + '|PDF=' + $filter('translate')('Report.ExportPdf') + '|XLS=Export Chart Data';
                        $scope.reportSummary.chart.xaxisname = languages[$rootScope.globalization.Label]['xaxis_name'];
                        $scope.reportSummary.chart.yaxisname = languages[$rootScope.globalization.Label]['yaxis_name'];

                        if ($scope.locale === 'tr') {
                            $scope.reportSummary.chart.decimalSeparator = ',';
                            $scope.reportSummary.chart.thousandSeparator = '.';
                        }

                        $scope.initilazSummaryReport();
                    }
                    );
            };

            $scope.setSingleReport = function (view) {
                ModuleService.getWidget(view.id)
                    .then(function (response) {
                        var singleReportData = response.data[0];
                        $rootScope.activeView.color = singleReportData.color;
                        $rootScope.activeView.field = singleReportData.field;
                        $rootScope.activeView.icon = singleReportData.icon;
                        $rootScope.activeView.type = singleReportData.type;
                        $rootScope.activeView.value = singleReportData.value;

                        $rootScope.activeView.aggregation = {
                            field: singleReportData.field,
                            aggregation_type: singleReportData.type
                        }

                        $scope.loading = false;

                    });
            };

            $scope.setView = function (view) {
                if (!view)
                    return;
                $scope.viewChangeStatus = false;
                $state.go('app.moduleList', { type: $scope.module.name, viewid: view.id }, { notify: false });
                // $scope.isDirty = true;
                $rootScope.activeView = view;

                if (view.view_type === 'report') {
                    if (view.report_type === "summary" || view.report_type === "single") {
                        //sometimes it can be null, some times length 0
                        if (view.aggregations && view.aggregations.length > 0)
                            view.aggregations[0].aggregation_type === "count" ? $scope.totalCount = true : $scope.totalCount = false;
                    }

                    switch (view.report_type) {
                        case "summary":
                            $scope.setSummary(view);
                            break;
                        case "tabular":
                            $rootScope.activeView = view;
                            if (view.aggregations) {
                                for (var i = 0; i < view.fields.length; i++) {
                                    if ($scope.fieldskey[view.fields[i].field] && ($scope.fieldskey[view.fields[i].field].data_type === 'numeric' || $scope.fieldskey[view.fields[i].field].data_type === 'number' || $scope.fieldskey[view.fields[i].field].data_type === 'number_auto' || $scope.fieldskey[view.fields[i].field].data_type === 'currency' || $scope.fieldskey[view.fields[i].field].data_type === 'number_decimal')) {

                                        var field = $filter('filter')(view.aggregations, { field: view.fields[i].field })[0];
                                        if (!field) {
                                            view.aggregations.push({
                                                field: view.fields[i].field
                                            })
                                        }
                                    }

                                }
                            }

                            $scope.changeView($rootScope.activeView);
                            break;
                        case "single":
                            $scope.setSingleReport(view);
                            break;
                        default:
                            break;
                    }
                } else if (view.view_type === 'grid') {
                    $scope.gridGroupBy = $rootScope.activeView.groups_json ? JSON.parse($rootScope.activeView.groups_json) : [];
                    $scope.changeView($rootScope.activeView);
                } else if (view.view_type === 'calendar') {

                    if ($rootScope.activeView.settings && !angular.isObject($rootScope.activeView.settings))
                        $rootScope.activeView.settings = JSON.parse($rootScope.activeView.settings);
                    $scope.loading = false;
                    $scope.setViewCalendar();

                } else if (view.view_type === 'kanban') {
                    if ($rootScope.activeView.settings && !angular.isObject($rootScope.activeView.settings))
                        $rootScope.activeView.settings = JSON.parse($rootScope.activeView.settings);
                    $scope.loading = false;
                    ModuleService.getPicklists($scope.module)
                        .then(function (picklists) {
                            $rootScope.processPicklistLanguages(picklists);
                            $scope.picklistsModule = picklists;
                            $scope.setViewKanban();
                        });
                }

                if ($rootScope.activeView.view_type === 'report' && $rootScope.activeView.report_type === 'tabular' && $rootScope.activeView.aggregations && $rootScope.activeView.aggregations.length > 0 && $rootScope.activeView.aggregations[0].aggregation_type) {
                    $rootScope.activeView.aggregation = $rootScope.activeView.aggregations[0]
                }

                if ($rootScope.activeView.filter_logic_json)
                    $scope.filtera = JSON.parse($rootScope.activeView.filter_logic_json);

            };

            $scope.getProfilisByIds = function (ids) {

                var profileList = [];
                for (var i = 0; i < ids.length; i++) {
                    var profile = $filter('filter')($rootScope.profiles, { id: parseInt(ids[i]) }, true);
                    if (profile) {
                        profileList.push(profile[0]);
                    }

                }
                return profileList;
            };

            $scope.saveModal = function (filterForm) {
                if (!filterForm.validate() || !$scope.validSelectedFields()) {
                    mdToast.error($filter('translate')('Module.RequiredError'));
                    return true;
                }
                if (!$rootScope.user.profile.has_admin_rights)
                    $rootScope.activeView.sharing_type = "me";

                if ($rootScope.activeView.sharing_type === 'profile') {
                    $rootScope.activeView.profile = $scope.getProfilisByIds($rootScope.activeView.profiles);
                }

                if ($rootScope.activeView.view_type === 'report' && $rootScope.activeView.report_type === 'summary' && !$rootScope.activeView.group_field) {
                    mdToast.error($filter('translate')('Module.RequiredError'));
                    return;
                }

                if ($rootScope.preview)
                    $rootScope.activeView.editable = $rootScope.activeView.system_type === 'custom' ? true : false;

                $scope.viewIsSaveing = false;

                var parentEl = angular.element(document.body);
                $mdDialog.show({
                    parent: parentEl,
                    templateUrl: 'view/app/module/common/viewSaveModal.html',
                    clickOutsideToClose: false,
                    scope: $scope,
                    preserveScope: true
                });

            };

            $scope.viewNameMax = function () {
                if ($scope.viewName.length > 50) {
                    $scope.viewName = $rootScope.getLanguageValue($rootScope.activeView.languages, 'label');
                    mdToast.error($filter('translate')('View.ViewNameMaxLength', { maxLength: 50 }));
                }
            };

            $scope.SaveView = function (type) {

                if (!$scope.viewSaveModalForm.validate()) {
                    mdToast.error($filter('translate')('Module.RequiredError'));
                    $scope.viewIsSaveing = false;
                    return;
                }
                if (type === 'add')
                    $scope.viewIsSaveing = true;

                if ($scope.viewFields && $scope.viewFields.selectedFields) {
                    $rootScope.activeView.fields = [];
                    for (var i = 0; i < $scope.viewFields.selectedFields.length; i++) {
                        var name = $scope.viewFields.selectedFields[i].name;

                        if ($scope.viewFields.selectedFields[i].data_type === 'lookup' && !$scope.viewFields.selectedFields[i].labelExt) {
                            name = name.split('.')[0];

                            $rootScope.activeView.fields.push({
                                field: name,
                                order: i
                            });

                            $rootScope.activeView.fields.push({
                                field: $scope.viewFields.selectedFields[i].name + '.primary',
                                order: i
                            });
                        } else {
                            $rootScope.activeView.fields.push({
                                field: $scope.viewFields.selectedFields[i].name,
                                order: i
                            });
                        }
                    }
                }

                $rootScope.activeView.filter_logic_json = JSON.stringify($scope.filtera);

                var viewModel = {
                    filters: $rootScope.activeView.filters,
                    module_id: $rootScope.activeView.module_id,
                    sharing_type: $rootScope.activeView.sharing_type,
                    filter_logic_json: $rootScope.activeView.filter_logic_json,
                    view_type: $rootScope.activeView.view_type,
                    languages: $rootScope.activeView.languages
                };

                viewModel.languages[$rootScope.globalization.Label]["label"] = $scope.viewName || $rootScope.getLanguageValue($rootScope.activeView.languages, 'label');

                if ($rootScope.activeView.view_type === 'report') {
                    viewModel.report_type = $rootScope.activeView.report_type;
                }
                else if ($rootScope.activeView.view_type === 'grid') {
                    viewModel.fields = $rootScope.activeView.fields;
                    viewModel.report_type = "";
                    viewModel.aggregations = $rootScope.activeView.aggregations.length > 0 ? $filter('filter')($rootScope.activeView.aggregations, { active: true }) : null;
                    viewModel.groups_json = $scope.gridGroupBy.length > 0 ? JSON.stringify($scope.gridGroupBy) : null;
                }
                else if (viewModel.view_type === "calendar" || viewModel.view_type === "kanban") {
                    viewModel.fields = $rootScope.activeView.fields;
                    viewModel.settings = JSON.stringify($rootScope.activeView.settings);
                    viewModel.report_type = "";
                    viewModel.aggregations = null;
                    viewModel.groups_json = null;
                }

                if (type === 'edit')
                    viewModel.id = $rootScope.activeView.id;

                if ($rootScope.activeView.sharing_type === 'custom') {
                    viewModel.shares = [];
                    if ($rootScope.activeView.shares.length < 1) {
                        mdToast.error($filter('translate')('Module.RequiredError'));
                        $scope.viewIsSaveing = false;
                        return false;
                    }
                    for (var k = 0; k < $rootScope.activeView.shares.length; k++) {
                        viewModel.shares.push($rootScope.activeView.shares[k].id);
                    }

                }

                if ($rootScope.activeView.sharing_type === 'profile') {
                    viewModel.profiles = [];
                    if (!$rootScope.activeView.profile) {
                        $scope.viewIsSaveing = false;
                        return
                    }
                    for (var i = 0; i < $rootScope.activeView.profile.length; i++) {
                        viewModel.profiles.push($rootScope.activeView.profile[i].id);
                    }
                }

                switch ($rootScope.activeView.report_type) {
                    case "summary":
                        viewModel.group_field = $rootScope.activeView.group_field;
                        viewModel.sort_field = $rootScope.activeView.sort_field;
                        viewModel.sort_direction = 'asc';

                        viewModel.aggregations = [{
                            field: $rootScope.activeView.aggregation.field,
                            aggregation_type: $rootScope.activeView.aggregation.aggregation_type
                        }];

                        viewModel.chart = {
                            type: $scope.reportSummary.chart.chart_type,
                            languages: {}
                        };
                        viewModel.chart.languages[$rootScope.globalization.Label] = {
                            caption: viewModel.languages[$rootScope.globalization.Label]['label'],
                            name: viewModel.languages[$rootScope.globalization.Label]['label'],
                            xaxis_name: $scope.reportSummary.chart['xaxisname'],
                            yaxis_name: $scope.reportSummary.chart['yaxisname']
                        };
                        break;
                    case "tabular":
                        viewModel.fields = $rootScope.activeView.fields;
                        viewModel.aggregations = $rootScope.activeView.aggregations;
                        break;
                    case "single":
                        viewModel.aggregations = [{
                            field: $rootScope.activeView.aggregation.field || 'created_by',
                            aggregation_type: $rootScope.activeView.aggregation.aggregation_type || 'count'
                        }];


                        viewModel.widget = {
                            color: $rootScope.activeView.color,
                            icon: $rootScope.activeView.icon,
                            languages: {}
                        };

                        viewModel.widget.languages[$rootScope.globalization.Label] = {
                            name: $rootScope.getLanguageValue($rootScope.activeView.languages, 'label')
                        };

                        break;
                }

                if ($rootScope.preview) {
                    viewModel.default = $rootScope.activeView.default;
                    viewModel.system_type = $rootScope.activeView.editable ? "custom" : "system";
                }

                $rootScope.languageStringify(viewModel);

                ModuleService.saveView(type, viewModel).then(function (reponse) {
                    mdToast.success($filter('translate')('View.ViewUpdateSucces'));
                    ModuleService.getViews($scope.module).then(function (result) {

                        $rootScope.processLanguages(result);
                        $scope.views = result;
                        $scope.views.reverse();
                        //last index is last item
                        $rootScope.activeView = $scope.views[0];
                        $cache.put($scope.module.name + "-activeView", $rootScope.activeView);
                        var defaultView = $filter('filter')($scope.views, { default: true }, true)[0];
                        if (defaultView) {
                            var indexOfActive = $scope.views.indexOf(defaultView);
                            $scope.views[indexOfActive] = $rootScope.activeView;
                            $scope.views[0] = defaultView;
                        }

                        if ($rootScope.activeView.shares) {
                            var shares = [];
                            for (var a = 0; a < $rootScope.activeView.shares.length; a++)
                                shares.push({
                                    id: $rootScope.activeView.shares[a].user_id,
                                    full_name: $rootScope.activeView.shares[a].user.full_name
                                });
                            $rootScope.activeView.shares = shares;
                        }

                        $scope.setView($rootScope.activeView);
                    });
                    $scope.closeLightBox();
                    $rootScope.closeSide('sideModal');
                });
            };

            $scope.sharesOptions = {
                dataSource: $scope.users,
                filter: "contains",
                dataTextField: "full_name",
                dataValueField: "id",
                optionLabel: $filter('translate')('Common.Select')
            };

            $scope.profilesOptions = {
                dataSource: $rootScope.profiles,
                filter: "contains",
                dataTextField: 'languages.' + $rootScope.globalization.Label + '.name',
                dataValueField: "id",
                optionLabel: $filter('translate')('Common.Select')

            };

            $scope.reloadFieldSelection = function () {
                var listBox = $("#available-fields").data("kendoListBox");
                var listBox1 = $("#selected-fields").data("kendoListBox");
                if (listBox) {
                    $scope.selectedFieldsOptions.dataSource = new kendo.data.DataSource({
                        data: $scope.viewFields.selectedFields
                    });

                    $scope.availableFieldsOptions.dataSource = new kendo.data.DataSource({
                        data: $scope.preview ? $scope.viewFields.availableFields : $filter('filter')($scope.viewFields.availableFields, { name: '!is_sample' }, true)
                    });

                    listBox.setDataSource($scope.availableFieldsOptions.dataSource);
                    listBox1.setDataSource($scope.selectedFieldsOptions.dataSource);

                    for (i = 0; listBox.options.dataSource._data.length > i; i++) {
                        if (listBox.options.dataSource._data[i].name.includes("seperator-")) {
                            listBox.enable($(".k-item").eq(i), false);
                        }
                    }
                }
            };

            $scope.refreshColumn = function (active, numberField) {
                setTimeout(function () {
                    changeViewState();
                    var selected = $("#selected-fields").data("kendoListBox");
                    var available = $("#available-fields").data("kendoListBox");

                    var selectedHtmlItems = selected.wrapper.find(".k-item");
                    var availableHtmlItems = available.wrapper.find(".k-item");

                    var selectedOrderedItems = [];
                    var availableOrderedItems = [];

                    $.each(selectedHtmlItems, function (idx, item) {
                        selectedOrderedItems.push(selected.dataItem(item));
                    });

                    $.each(availableHtmlItems, function (idx, item) {
                        availableOrderedItems.push(available.dataItem(item));
                    });

                    $scope.viewFields.selectedFields = selectedOrderedItems;
                    $scope.viewFields.availableFields = $filter('orderBy')(availableOrderedItems, 'order');
                    //var listBox = $("#available-fields").data("kendoListBox");
                    //listBox.dataSource.data($scope.viewFields.availableFields);
                    if (!active) // && $rootScope.activeView.view_type !== 'kanban')//?? TODO bakılacak
                        $scope.reloadFieldSelection();

                    if ($rootScope.activeView.report_type === 'tabular')
                        $scope.setViewAggregationsFields($scope.viewFields.selectedFields);
                    else if ($rootScope.activeView.view_type === 'grid') {
                        $scope.gridGroupAggregationFields($scope.viewFields.selectedFields)
                        $scope.gridGroupFields($scope.viewFields.selectedFields)
                    }

                    $scope.changeView(false, true);

                }, 200);
            };

            $scope.changeView = function (view, filter) {
                //clear selected rows
                if ($scope.selectedRows.length > 0) {
                    $scope.isAllSelected = false;
                    $scope.selectedRows = [];
                }

                if (!filter) {

                    $rootScope.activeView = view;
                    // $scope.primaryField = "";
                    if (view.filter_logic_json)
                        $scope.filtera = JSON.parse(view.filter_logic_json);

                    $scope.viewFields = ModuleService.getViewFields($scope.module, $rootScope.activeView);

                    $scope.availableFieldsOptions = {
                        disabled: true,
                        draggable: true,
                        dataTextField: "label",
                        dataValueField: "name",
                        connectWith: "selected-fields",
                        autoScroll: true,
                        dataSource: $scope.preview ? $scope.viewFields.availableFields : $filter('filter')($scope.viewFields.availableFields, { name: '!is_sample' }, true),
                        dropSources: ["selected-fields"],
                        template: "<div class='list-title'>#:label#</div>",
                        enable: function (e) {
                            void 0;
                        },
                        reorder: function (e) {
                            e.preventDefault();
                        },
                        add: function (e) {
                            /*Kullanılabilir Alanlar*/

                            var field = e.dataItems[0];

                            if (field.labelExt) {
                                var lookupName = field.name.substring(0, field.name.lastIndexOf('.'));
                                var result = $filter('filter')($scope.viewFields.availableFields, function (item) {
                                    return item.name.indexOf(lookupName) > -1 && item.labelExt === field.labelExt
                                });

                                if (result && result.length > 0) {
                                    field.parent_id = result[0].parent_id;
                                    field.order += field.parent_id;
                                }
                            } else
                                field.parent_id = 0;

                            $scope.refreshColumn();

                        }
                    };

                    $scope.selectedFieldsOptions = {
                        draggable: true,
                        dataTextField: "label",
                        dataValueField: "name",
                        autoScroll: true,
                        template: "<div class='list-title'>#:label# #:labelExt ?? ''#</div>",
                        connectWith: "available-fields",
                        dataSource: $scope.viewFields.selectedFields,
                        dropSources: ["available-fields"],
                        reorder: function () {
                            $scope.refreshColumn(true);
                        },
                        add: function (e) {
                            //$scope.currentFieldId = e.dataItems[0].id;
                            /*Görünen Alanlar*/

                            $scope.refreshColumn();
                        },
                    };
                    $scope.reloadFieldSelection();
                }
                if ($rootScope.activeView.view_type !== 'grid') {
                    $scope.gridGroupBy = [];
                } else {
                    $rootScope.activeView.groups_json = $scope.gridGroupBy.length > 0 ? JSON.stringify($scope.gridGroupBy) : null;
                    $scope.gridGroupBy = $rootScope.activeView.groups_json ? JSON.parse($rootScope.activeView.groups_json) : [];
                }

                var tableConig = { moduleName: $scope.module.name };
                if ($rootScope.activeView.view_type === 'report' && $rootScope.activeView.report_type) {
                    tableConig.activeView = $rootScope.activeView;
                }
                else if ($rootScope.activeView.view_type === 'grid') {
                    tableConig.activeView = $rootScope.activeView;
                    tableConig.gridGroupBy = $scope.gridGroupBy;
                }


                if ($rootScope.isMobile()) {
                    var selectedFieldsMobile = [];
                    var length = $scope.viewFields.selectedFields.length > 3 ? 3 : $scope.viewFields.selectedFields.length;
                    for (var i = 0; i < length; i++) {
                        selectedFieldsMobile.push($scope.viewFields.selectedFields[i]);
                    }

                    $scope.viewFields.selectedFields = selectedFieldsMobile;
                }

                var table = ModuleService.generatRowtmpl($scope.viewFields.selectedFields, false, tableConig);

                if ($rootScope.activeView.view_type === 'report' && $rootScope.activeView.report_type === 'tabular' && $rootScope.activeView.aggregations) {
                    table.columns.map(function (column) {
                        for (var j = 0; j < $rootScope.activeView.aggregations.length; j++) {
                            if (column.field === $rootScope.activeView.aggregations[j].field && $rootScope.activeView.aggregations[j].aggregation_type) {
                                var aggertionFiled = $rootScope.activeView.aggregations[j].aggregation_type + '(' + $rootScope.activeView.aggregations[j].field + ')';
                                column.footerTemplate = "<div> {{ 'Report." + $rootScope.activeView.aggregations[j].aggregation_type + "' | translate  }} : {{ " + aggertionFiled.replace("(", "_").replace(")", "") + "}} </div>";
                                var aggregationsFindRequest = {
                                    fields: [aggertionFiled],
                                    limit: 1,
                                    offset: 0,
                                    filters: $rootScope.activeView.filters
                                };

                                ModuleService.findRecords($scope.module.name, aggregationsFindRequest).then(function (result) {
                                    $scope[result.config.data.fields[0].replace("(", "_").replace(")", "")] = result.data[0][[result.config.data.fields[0].split("(")[0]]];
                                });

                            }
                        }
                    })
                }

                $scope.findRequest = {
                    "module": $scope.module.name,
                    //"group_by": "string",
                    //"logic_type": "and, or",
                    //"two_way": "bool",
                    // "many_to_many": "string",
                    //"sort_direction": "asc, desc",
                    //"sort_field": "string",
                    "filter_logic": $rootScope.activeView.filter_logic,
                    "filters": $rootScope.activeView.filters,
                    "convert": true,
                    "fields": table.requestFields
                };

                checkFiltersExistLookup();

                $scope.freezeDependencies = getFreezeDependencies($scope.module.dependencies);
                if ($scope.freezeDependencies) {
                    $scope.freezeFields = getFreezeFields($scope.freezeDependencies, undefined, true);
                    if ($scope.freezeFields)
                        for (var o = 0; o < $scope.freezeFields.length; o++) {
                            if ($scope.findRequest.fields.indexOf($scope.freezeFields[o].name) < 0)
                                $scope.findRequest.fields.push($scope.freezeFields[o].name);
                        }
                }

                $cache.put($scope.module.name + "-activeView", $rootScope.activeView);
                $cache.put($scope.module.name + "-activeViewFields", $scope.viewFields.selectedFields);
                $rootScope.activeView.filter_logic_json = JSON.stringify($scope.filtera);
                $scope.loading = true;
                $scope.executeCode = false;
                components.run('BeforeListRequest', 'Script', $scope);
                if ($scope.executeCode)
                    return;

                if ($rootScope.activeView.report_type === 'tabular') {
                    $scope.mainGridTabularOptions = {
                        dataSource: {
                            serverPaging: true,
                            serverFiltering: true,
                            serverSorting: true,
                            transport: {
                                read: function (options) {
                                    $.ajax({
                                        url: '/api/record/find_custom?locale=' + locale,
                                        contentType: 'application/json',
                                        dataType: 'json',
                                        type: 'POST',
                                        data: JSON.stringify(Object.assign($scope.findRequest, options.data)),
                                        success: function (result) {
                                            if (result.data && result.data.length > 0) {
                                                isFreeze(result.data);
                                            }
                                            options.success(result);
                                            $scope.loading = false;
                                            if (!$rootScope.isMobile())
                                                $(".k-pager-wrap").removeClass("k-pager-sm");
                                            if ($rootScope.activeView.view_type === "report" && result.total_count < 1) {
                                                $(".k-grid-footer").addClass("hide");
                                            } else {
                                                var aggregationTdShow = false;
                                                if ($rootScope.activeView.aggregations) {
                                                    for (var i = 0; i < $rootScope.activeView.aggregations.length; i++) {
                                                        var aggregation = $rootScope.activeView.aggregations[i];
                                                        if (aggregation.field && aggregation.aggregation_type) {
                                                            aggregationTdShow = true;
                                                        }
                                                    }
                                                }
                                                !aggregationTdShow ? $(".k-grid-footer").addClass("hide") : "";
                                            }
                                        },
                                        beforeSend: $rootScope.beforeSend()
                                    })
                                }
                            },
                            schema: {
                                data: "data",
                                total: "total",
                                model: { id: "id" }
                            }
                        },
                        rowTemplate: '<tr ng-click="goUrl2(dataItem.id)">' + table.rowtempl + '</tr>',
                        altRowTemplate: '<tr class="k-alt" ng-click="goUrl2(dataItem.id)">' + table.rowtempl + '</tr>',
                        scrollable: false,
                        sortable: true,
                        noRecords: true,
                        pageable: {
                            refresh: true,
                            pageSize: 10,
                            pageSizes: [10, 25, 50, 100, 500],
                            buttonCount: 5,
                            info: true,
                        },
                        columns: table.columns,
                    };
                }
                else if ($rootScope.activeView.view_type === 'grid') {
                    $scope.gridGroupAggregationFields($scope.viewFields.selectedFields)
                    $scope.aggregationsOrder();
                    $scope.findRequest.aggregations = $filter('filter')($rootScope.activeView.aggregations, { active: true });
                    $scope.findRequest.type = "grid";
                    $scope.findRequest.groupDate = $scope.gridGroupBy.length > 0 ? $scope.gridGroupBy : undefined;
                    $scope.mainGridOptions = {
                        dataSource: {
                            serverPaging: true,
                            serverFiltering: true,
                            serverSorting: true,
                            serverGrouping: true,
                            serverAggregates: true,
                            group: $scope.gridGroupBy.length > 0 ? $scope.gridGroupBy : undefined,
                            transport: {
                                read: function (options) {
                                    $.ajax({
                                        url: '/api/record/find_custom?locale=' + locale,
                                        contentType: 'application/json',
                                        dataType: 'json',
                                        type: 'POST',
                                        data: JSON.stringify(Object.assign($scope.findRequest, options.data)),
                                        success: function (result) {
                                            if (result.data && result.data.length > 0) {
                                                isFreeze(result.data);
                                            }

                                            options.success(result);
                                            $scope.loading = false;

                                            if (!$rootScope.isMobile())
                                                $(".k-pager-wrap").removeClass("k-pager-sm");

                                            if ($scope.gridGroupBy.length === 0)
                                                $(".k-grouping-header").hide();
                                            else
                                                $(".k-grouping-header").show();

                                        },
                                        beforeSend: $rootScope.beforeSend()
                                    })
                                }
                            },
                            schema: {
                                data: "data",
                                total: "total",
                                model: { id: "id" },
                                groups: "groups",
                                parse: function (result) {
                                    result.groups = $scope.parseGridData(result);
                                    $scope.gridData = result;
                                    return result;
                                }
                            }
                        },
                        // dataBound: function (e) {
                        //     var grid = this;
                        //     $(".k-grouping-row").each(function (e) {
                        //         grid.collapseGroup(this);
                        //     });
                        // },
                        rowTemplate: '<tr ng-click="goUrl2(dataItem.id)">' + table.rowtempl + '</tr>',
                        altRowTemplate: '<tr class="k-alt" ng-click="goUrl2(dataItem.id)">' + table.rowtempl + '</tr>',
                        scrollable: false,
                        groupable: true,
                        sortable: $scope.gridGroupBy.length > 0 ? 0 : 1,
                        noRecords: true,
                        pageable: {
                            refresh: true,
                            pageSize: 10,
                            pageSizes: [10, 25, 50, 100, 500],
                            buttonCount: 5,
                            info: true,
                        },
                        columns: table.columns,
                    };
                }
                else if ($rootScope.activeView.view_type === 'kanban' && $rootScope.activeView.settings && $rootScope.activeView.settings.kanbanPicklist) {
                    $scope.kanbanBoards();

                    $scope.kanbanSelectPicklist = $filter('filter')($scope.picklistFields, { picklist_id: parseInt($rootScope.activeView.settings.kanbanPicklist) }, true)[0];
                    if (table.requestFields.indexOf($scope.kanbanSelectPicklist.name) === -1)
                        table.requestFields.push($scope.kanbanSelectPicklist.name)

                    $scope.findRequestKanban = {
                        "module": $scope.module.name,
                        // "take": 6,
                        // "pageSize": 6,
                        "filter_logic": $rootScope.activeView.filter_logic,
                        "filters": $rootScope.activeView.filters,
                        "convert": true,
                        "fields": table.requestFields,
                        "picklist_name": $scope.kanbanSelectPicklist.name,
                        "exclude_count": "no"
                    };

                    if ($scope.findRequestKanban.fields.length <= 3)
                        $scope.getItemCount = 15;
                    else
                        $scope.getItemCount = 10;

                    if ($scope.boards && $scope.boards.length > 0) {
                        $scope.boards.forEach(function (item) {
                            $scope.kanbanListViewAltOptionsSet(item)
                        });
                    }

                    $scope.kanbanMainOptions = {
                        template: $scope.boardTemplateMain(),
                        dataSource: $scope.boards
                    }

                    $timeout(function () {
                        var area = $('#modul-area-app').innerHeight();
                        var area2 = $('#modul-area-app-menu').innerHeight();
                        $('#kanban-view').height(area - area2 - 50);
                    }, 250)

                    $scope.sortableKanbanOptions = {
                        filter: '.card',
                        container: '.board',
                        connectWith: '.k-listview-content',
                        cursor: 'grabbing',
                        placeholder: function (element) {
                            return $('<div class="card"></div>').css({
                                background: '#fff',
                                height: element[0].offsetHeight + 'px',
                                margin: '10px 20px'
                            });
                        },
                        autoScroll: true,
                        change: function (e) {
                            if (e.action === 'receive') { // return receive and remove
                                var recordId = parseInt(e.item[0].id.split('-')[2]);
                                var newPicklistValue = parseInt(e.sender.element[0].parentElement.id.split('-')[2]);
                                var picklist = $filter('filter')($scope.picklistFields, { picklist_id: $rootScope.activeView.settings.kanbanPicklist })[0];
                                var recordObj = { id: recordId };

                                if (newPicklistValue !== 0)//data set
                                    recordObj[picklist.name] = newPicklistValue;
                                else
                                    recordObj[picklist.name] = null;

                                ModuleService.updateRecord($scope.module.name, recordObj).then(function (res) {
                                    mdToast.success($filter('translate')('Module.UpdateRecordBulkSuccess'));
                                    // var boardArea2 = $('#list-kanban-'+newPicklistValue).data('kendoListView');
                                    // boardArea2.dataSource.read();
                                }).catch(function (res) {
                                    mdToast.error($filter('translate')('Common.Error'));
                                })
                            } else {
                                var oldPicklistValue = parseInt(e.sender.element[0].parentElement.id.split('-')[2]);
                                var items = $('#list-kanban-' + oldPicklistValue + " .k-listview-content .card");
                                if (items && items.length < 10 && !$scope["getItems" + oldPicklistValue]) {
                                    $scope["getItems" + oldPicklistValue] = true;
                                    $scope["page" + oldPicklistValue]++;
                                    var boardArea = $('#list-kanban-' + oldPicklistValue);
                                    var listView = boardArea.data('kendoListView');
                                    $scope["scrollData" + oldPicklistValue].query({
                                        page: $scope["page" + oldPicklistValue],
                                        pageSize: $scope.getItemCount
                                    }).then(function () {
                                        var data = $scope["scrollData" + oldPicklistValue].data();
                                        if (data.length > 0) {
                                            for (var i = 0; i < data.length; i++) {
                                                //     //listView.dataSource.add(x);
                                                listView.dataSource.pushCreate(data[i]);
                                            }
                                            $scope["scroll" + oldPicklistValue] = true;
                                        }
                                    });
                                }
                            }
                        },

                        end: function (e) {
                            $(".left-right-to-items").remove();
                            $(".item-block-same-area").remove();
                        },
                        hint: function (element) {
                            var kanbanArea = $("#board .k-listview-content:first"); // sağ sol hareketleri için
                            kanbanArea.prepend("<div style='height: 100%; width: 12%; position:fixed; z-index: 10000; left:0;' class='list-header left-right-to-items'></div> " +
                                "<div style='height: 100%; width: 12%; position:fixed; z-index: 10000; right:0;' class='list-header left-right-to-items'></div>")

                            var picklistValue = parseInt(element[0].parentElement.parentElement.id.split('-')[2]); //itemın bulunduğu listede sort edilmesini engeller.
                            $("#list-kanban-" + picklistValue).prepend("<div style='height: 100%; width: 100%; position:absolute; z-index: 10000;' class='item-block-same-area'></div>")

                            return element.clone().css({
                                width: '17em',
                                background: 'white',
                                boxShadow: '0 0.5rem 0.75rem rgba(0, 0, 0, 0.075)'
                            })

                        }
                    };
                }
            };

            ModuleService.getViews($scope.module).then(function (result) {

                $rootScope.processLanguages(result);
                $scope.views = result;
                $scope.shadowViews = angular.copy(result);
                if ($scope.views.length < 1) {
                    $scope.loading = false;
                    return;
                }

                $scope.views.reverse();
                var isComeFromRecordForm = false;
                var defaultView = $filter('filter')($scope.views, { default: true }, true)[0] || $scope.views[0];
                if (!$rootScope.activeView || ($rootScope.activeView && $rootScope.activeView.module_id !== $scope.module.id)) {
                    $rootScope.activeView = defaultView;
                } else {
                    //if we have set active view previously, we have to filter it's again because it's not equal object with current view
                    $rootScope.activeView = $filter('filter')($scope.views, { id: $rootScope.activeView.id }, true)[0];
                    isComeFromRecordForm = true
                }

                var indexOfActive = -1;
                if (!isComeFromRecordForm) {
                    indexOfActive = $scope.views.indexOf($rootScope.activeView);
                    $scope.views[indexOfActive] = $scope.views[0];
                    $scope.views[0] = $rootScope.activeView;
                } else {
                    indexOfActive = $scope.views.indexOf(defaultView);
                    $scope.views[indexOfActive] = $scope.views[0];
                    $scope.views[0] = defaultView;
                }

                for (var i = 0; i < $scope.views.length; i++) {
                    if ($scope.views[i].shares && $scope.views[i].shares.length > 0) {
                        var shares = [];

                        for (var a = 0; a < $scope.views[i].shares.length; a++)
                            shares.push({
                                id: $scope.views[i].shares[a].user_id,
                                full_name: $scope.views[i].shares[a].user.full_name
                            });

                        $scope.views[i].shares = shares;
                    }
                }

                if ($stateParams.viewid) {
                    $scope.viewid = $stateParams.viewid;
                    $rootScope.activeView = $filter('filter')($scope.views, { id: parseInt($scope.viewid) }, true)[0];

                    if ($rootScope.activeView.filter_logic_json)
                        $scope.filtera = JSON.parse($rootScope.activeView.filter_logic_json);

                    $scope.setView($rootScope.activeView);
                    return true;
                }

                var viewCache = $cache.get($scope.module.name + '-activeView');

                if (viewCache) {
                    $scope.setView(viewCache);
                    return true;
                }

                $scope.setView($rootScope.activeView);

            });

            $scope.parseGridData = function (result) {
                var groups = [];
                $scope.gruopFilters = [];
                if (result.data && result.data.length > 0 && $scope.gridGroupBy.length === 1) {
                    var first = $scope.gridGroupBy[0].field;
                    for (var s = 0; s < result.data.length; s++) {
                        var groupName = "";
                        if ($scope.gridGroupBy[0].dateType !== null && ($scope.gridGroupBy[0].data_type === "date_time" || $scope.gridGroupBy[0].data_type === "date" || $scope.gridGroupBy[0].data_type === "time")) {
                            groupName = $scope.groupDateSortName($scope.gridGroupBy[0].dateType, result.data[s][first]);
                        } else {
                            groupName = result.data[s][first];
                        }
                        groupName = groupName.toString();

                        var filterData = $filter('filter')(groups, { value: groupName }, true)[0];
                        if (filterData) {
                            filterData.items.push(result.data[s])
                        } else {
                            var groupOne = {
                                field: first,
                                value: groupName,
                                items: [result.data[s]],
                                hasSubgroups: false
                            }
                            groups.push(groupOne);
                            var field = $filter('filter')($scope.module.fields, { name: first.split('.')[0] }, true)[0];

                            var firstFieldData = first.split('.');
                            if (firstFieldData.length > 1)
                                firstFieldData = firstFieldData[0] + "." + firstFieldData[1] + ".id";
                            var value = result.data[s][firstFieldData].toString();
                            if (field.data_type === 'currency')
                                value = value.slice(1);

                            $scope.gruopFilters.push({ field: first.split('.')[0], value: value, operator: field.operators[0].name, no: 1 })
                        }
                    }
                } else if (result.data && result.data.length > 0 && $scope.gridGroupBy.length === 2) {
                    var firstField = $scope.gridGroupBy[0].field;
                    for (var h = 0; h < result.data.length; h++) {
                        var groupName = "";
                        if ($scope.gridGroupBy[0].dateType !== null && ($scope.gridGroupBy[0].data_type === "date_time" || $scope.gridGroupBy[0].data_type === "date" || $scope.gridGroupBy[0].data_type === "time")) {
                            groupName = $scope.groupDateSortName($scope.gridGroupBy[0].dateType, result.data[h][firstField]);
                        } else {
                            groupName = result.data[h][firstField];
                        }
                        groupName = groupName.toString();

                        var filter = $filter('filter')(groups, { value: groupName }, true)[0];
                        if (!filter) {
                            var group = {
                                field: firstField,
                                value: groupName,
                                items: [],
                                hasSubgroups: true
                            }
                            groups.push(group);
                            var field2 = $filter('filter')($scope.module.fields, { name: firstField.split('.')[0] }, true)[0];

                            var firstFieldData = firstField.split('.');
                            if (firstFieldData.length > 1)
                                firstFieldData = firstFieldData[0] + "." + firstFieldData[1] + ".id";
                            var value = result.data[h][firstFieldData].toString();
                            if (field2.data_type === 'currency')
                                value = value.slice(1);


                            $scope.gruopFilters.push({ field: firstField.split('.')[0], value: value, operator: field2.operators[0].name, no: 1 })
                        }
                    }
                    var secondField = $scope.gridGroupBy[1].field;
                    for (var k = 0; k < result.data.length; k++) {
                        var groupName2 = "";
                        if ($scope.gridGroupBy[0].dateType !== null && ($scope.gridGroupBy[0].data_type === "date_time" || $scope.gridGroupBy[0].data_type === "date" || $scope.gridGroupBy[0].data_type === "time")) {
                            groupName2 = $scope.groupDateSortName($scope.gridGroupBy[0].dateType, result.data[k][firstField]);
                        } else {
                            groupName2 = result.data[k][firstField];
                        }
                        groupName2 = groupName2.toString();
                        var firstFilter = $filter('filter')(groups, { value: groupName2 }, true)[0];
                        if (firstFilter) {
                            var groupName2alt = "";
                            if ($scope.gridGroupBy[1].dateType !== null && ($scope.gridGroupBy[1].data_type === "date_time" || $scope.gridGroupBy[1].data_type === "date" || $scope.gridGroupBy[1].data_type === "time")) {
                                groupName2alt = $scope.groupDateSortName($scope.gridGroupBy[1].dateType, result.data[k][secondField]);
                            } else {
                                groupName2alt = result.data[k][secondField];
                            }

                            var secondFilter = $filter('filter')(firstFilter.items, { value: groupName2alt }, true)[0];
                            if (!secondFilter) {
                                var secondGroup = {
                                    field: secondField,
                                    value: groupName2alt,
                                    items: [result.data[k]],
                                    hasSubgroups: false
                                }
                                firstFilter.items.push(secondGroup);
                            } else {
                                secondFilter.items.push(result.data[k])
                            }
                        }
                    }
                }

                //fix agg yokken
                if ($rootScope.activeView.aggregations && $scope.gruopFilters.length > 0 && $rootScope.activeView.view_type === 'grid' && result.aggregations && result.aggregations.length > 0) {
                    $timeout(function () {
                        var groupHeaders = $("tbody tr td:only-child p.k-reset");

                        if (groupHeaders.length > 0 && groupHeaders.length === result.aggregations.length) {
                            for (var i = 0; i < result.aggregations.length; i++) {
                                for (var j = 0; j < result.aggregations[i].fields.length; j++) {

                                    if (i === result.aggregations[i].group_value) {
                                        var fieldAndType = result.aggregations[i].fields[j].split("::")[0];
                                        var type = fieldAndType.split("(")[0];
                                        var fieldName = fieldAndType.split("(")[1];
                                        fieldName = fieldName.slice(0, fieldName.length - 1)
                                        var field = $filter('filter')($scope.module.fields, { name: fieldName }, true)[0];
                                        var label = result.aggregations[i].label[j];
                                        groupHeaders[i].innerHTML += $scope.aggregationsParseText(type, label, result.aggregations[i].data[fieldAndType], field, result.data);
                                    }
                                }
                            }
                            //Grup secimleri için selectBox ..
                            // for (var f = 0; f < groupHeaders.length; f++){
                            //     if(f < groupHeaders.length - 1)
                            //         groupHeaders[f].innerHTML ="<input type='checkbox' ng-checked='isGroupSelected' ng-click='selectGroups($event,"+'"'+result.aggregations[f].group_name+'",'+'"'+result.aggregations[f].group_value+'"'+"); $event.stopPropagation();' class='k-checkbox header-checkbox mr-3'>" + groupHeaders[f].innerHTML;
                            //     else
                            //         groupHeaders[f].innerHTML ="<input type='checkbox' ng-checked='isGroupSelected' ng-click='selectGroups($event,"+'"'+result.aggregations[f].group_name+'",'+'"'+result.aggregations[f].group_value+'",'+'"'+true+'"'+"); $event.stopPropagation();' class='k-checkbox header-checkbox mr-3'>" + groupHeaders[f].innerHTML;
                            // }
                            // $compile(groupHeaders)($scope);
                        }
                    }, 100)
                }

                return groups;
            };

            $scope.groupDateSortName = function (type, date) {
                var groupName = "";
                var dateHours = new Date(date);
                if (isNaN(dateHours) || date === "" || date === null || date === undefined)
                    return groupName;

                var parts = date.match(/(\d+)/g);
                var dateFormatArray = kendo.cultures.current.calendar.patterns.d.split(/[./\s,]+/);
                var dateObj = {};

                for (var i = 0; i < dateFormatArray.length; i++) {
                    switch (dateFormatArray[i]) {
                        case 'DD':
                        case 'dd':
                        case 'D':
                        case 'd':
                            dateObj.d = parts[i];
                            break;
                        case 'MM':
                        case 'mm':
                        case 'M':
                        case 'm':
                            dateObj.m = parts[i];
                            break;
                        case 'YYYY':
                        case 'yyyy':
                        case 'YY':
                        case 'yy':
                            dateObj.y = parts[i];
                            break;
                    }
                }

                date = new Date(dateObj.y, dateObj.m - 1, dateObj.d, dateHours.getHours())

                switch (type) {
                    case "year":
                        groupName = dateObj.y;
                        break;
                    case "ym":
                        var options = { year: 'numeric', month: 'long' };
                        groupName = new Intl.DateTimeFormat(kendo.cultures.current.name, options).format(date);
                        break;
                    case "ymd":
                        var options = { year: 'numeric', month: 'long', weekday: 'long', day: 'numeric' };
                        groupName = new Intl.DateTimeFormat(kendo.cultures.current.name, options).format(date);
                        break;
                    case "all":
                        var options = { year: 'numeric', month: 'long', weekday: 'long', day: 'numeric' };
                        groupName = new Intl.DateTimeFormat(kendo.cultures.current.name, options).format(date) + ", " + date.getHours() + ":00-" + date.getHours() + ":59";
                        break;
                }
                return groupName;
            }

            $scope.aggregationsParseText = function (type, label, resultData, field) {
                if (resultData === null)
                    resultData = 0;
                if (!Number.isInteger(resultData))
                    resultData = parseFloat(resultData).toFixed(2);

                if (field && field.data_type && field.data_type === "currency") {
                    var symbol = field.currency_symbol ? field.currency_symbol : null;
                    if (!symbol)
                        symbol = "$";

                    if (symbol === "₺") {
                        resultData += symbol;
                    } else {
                        resultData = symbol + resultData;
                    }
                }

                var template = "";
                switch (type) {
                    case "sum":
                        template = "<div class='ml-3 badge standart badge-secondary'>" + label + " (" + $filter('translate')('Report.sum') + "): <span>" + resultData + "</span></div>"
                        break;
                    case "avg":
                        template = "<div class='ml-3 badge standart badge-secondary'>" + label + " (" + $filter('translate')('Report.avg') + "): <span>" + resultData + "</span></div>"
                        break;
                    case "max":
                        template = "<div class='ml-3 badge standart badge-secondary'>" + label + " (" + $filter('translate')('Report.max') + "): <span>" + resultData + "</span></div>"
                        break;
                    case "min":
                        template = "<div class='ml-3 badge standart badge-secondary'>" + label + " (" + $filter('translate')('Report.min') + "): <span>" + resultData + "</span></div>"
                        break;
                    case "count":
                        template = "<div class='ml-3 badge standart badge-secondary'>" + label + ": <span>" + resultData + "</span></div>"
                        break;
                }
                return template;
            };

            $scope.gridGroupFields = function (data) {
                if ($rootScope.activeView.view_type === "grid" && $scope.gridGroupBy.length > 0) {
                    for (var i = 0; i < $scope.gridGroupBy.length; i++) {
                        var fieldName = $scope.gridGroupBy[i].field;
                        if (!$filter('filter')(data, { name: fieldName }, true)[0]) {
                            $scope.gridGroupBy.splice(i, 1);
                            if ($scope.gridGroupBy.length === 0) {
                                $scope.gridGroupBy = [];
                            }
                        }
                    }
                }
            };

            $scope.aggregationsOrder = function () {
                var aggregations = Object.assign([], $rootScope.activeView.aggregations);
                var filterCount = $filter('filter')(aggregations, { aggregation_type: "count" }, true)[0];
                var index = aggregations.indexOf(filterCount);
                aggregations.splice(index, 1);
                aggregations.push(filterCount);
                $rootScope.activeView.aggregations = aggregations;
            };

            $scope.gridGroupAggregationFields = function (data) {
                if ($rootScope.activeView.view_type === "grid" && $scope.gridGroupBy.length > 0) {
                    var objCount = {
                        label: $filter('translate')('Report.count'),
                        aggregation_type: "count",
                        field: "undefined",
                        active: false
                    }

                    if ($rootScope.activeView.aggregations === null)
                        $rootScope.activeView.aggregations = [];

                    for (var t = 0; t < $rootScope.activeView.aggregations.length; t++) {
                        var agg = $rootScope.activeView.aggregations[t];
                        if (!agg) {
                            var index = $rootScope.activeView.aggregations.indexOf(agg);
                            $rootScope.activeView.aggregations.splice(index, 1)
                        }
                    }

                    for (var i = 0; i < data.length; i++) {
                        var field = data[i];
                        if ((field.data_type === 'numeric' || field.data_type === 'number' || field.data_type === 'number_auto' || field.data_type === 'currency' || field.data_type === 'number_decimal') && !field.name.contains(".") && !field.deleted) {
                            if ($rootScope.activeView.aggregations && $rootScope.activeView.aggregations.length > 0)
                                var aggregationField = $filter('filter')($rootScope.activeView.aggregations, { field: field.name }, true)[0];

                            if (aggregationField) {
                                aggregationField.label = field.label
                            }
                            else {
                                $rootScope.activeView.aggregations.push({ field: field.name, aggregation_type: "sum", active: false, label: field.label })
                            }
                        }
                    }

                    var aggregationForCount = $filter('filter')($rootScope.activeView.aggregations, { aggregation_type: "count" }, true)[0];
                    if (!aggregationForCount) {
                        $rootScope.activeView.aggregations.push(objCount);
                    } else {
                        aggregationForCount.label = $filter('translate')('Report.count');
                        aggregationForCount.field = "undefined";
                    }

                    for (var k = 0; k < $rootScope.activeView.aggregations.length; k++) {
                        var fieldDataGrid = $rootScope.activeView.aggregations[k];
                        if (!$filter('filter')(data, { name: fieldDataGrid.field }, true)[0] && fieldDataGrid.aggregation_type !== "count") {
                            var selectIndex = $rootScope.activeView.aggregations.indexOf(fieldDataGrid);
                            $rootScope.activeView.aggregations.splice(selectIndex, 1);
                        }
                    }
                }
            };

            $scope.switchChange = function (aggregation) {
                if (aggregation) {
                    var filter = $filter('filter')($rootScope.activeView.aggregations, { field: aggregation.field }, true)[0];
                    if (filter) {
                        filter.active = aggregation.active;
                        filter.aggregation_type = aggregation.aggregation_type;
                    }
                }
                if (aggregation.active === true && aggregation.aggregation_type)
                    $scope.changeAggregationType();
                else if (aggregation.active === false)
                    $scope.changeAggregationType();
            }

            $scope.addGridGroup = function () {
                var newGroup = {};
                if ($scope.gridGroupBy.length === 0) {
                    newGroup.fieldArea = $scope.viewFields.selectedFields[0].name;
                    newGroup.field = $scope.viewFields.selectedFields[0].name;
                    newGroup.data_type = $scope.viewFields.selectedFields[0].data_type;
                    newGroup.dir = "asc";

                    if (newGroup.data_type === 'date' || newGroup.data_type === 'date_time' || newGroup.data_type === 'time') {
                        newGroup.dateType = "all";
                        newGroup.dateTypeArea = "all";
                    }
                } else {
                    if ($scope.viewFields.selectedFields.length > 1) {
                        var index = 1;
                        if ($scope.gridGroupBy[0].field === $scope.viewFields.selectedFields[index].name)
                            index = 0;
                        newGroup.fieldArea = $scope.viewFields.selectedFields[index].name;
                        newGroup.field = $scope.viewFields.selectedFields[index].name;
                        newGroup.data_type = $scope.viewFields.selectedFields[index].data_type;
                        newGroup.dir = "asc";
                        if (newGroup.data_type === 'date' || newGroup.data_type === 'date_time' || newGroup.data_type === 'time') {
                            newGroup.dateType = "all";
                            newGroup.dateTypeArea = "all";
                        }
                    }
                }

                if (Object.keys(newGroup).length > 0) {
                    $scope.gridGroupBy.push(newGroup);
                }

                $scope.changeAggregationType();
            };

            $scope.changeGridGroupSort = function (group) {
                if (group.dir === 'asc')
                    group.dir = "desc";
                else
                    group.dir = "asc";

                $scope.changeAggregationType();
            };

            $scope.gridGroupDelete = function (index, gridGroupBy) {
                gridGroupBy.splice(index, 1);
                if (gridGroupBy.length === 0) {
                    $scope.gridGroupBy = [];
                    for (var i = 0; i < $scope.activeView.aggregations.length; i++) {
                        $scope.activeView.aggregations[i].active = false;
                    }
                }
                $scope.changeAggregationType();
            };

            $scope.changeGridGroupField = function (group) {

                var field = $filter('filter')($scope.viewFields.selectedFields, { name: group.fieldArea }, true)[0];
                if (!$filter('filter')($scope.gridGroupBy, { field: field.name }, true)[0]) {
                    group.data_type = field.data_type;
                    group.field = field.name;
                    delete group.dateType;
                    if (field.data_type === 'date' || field.data_type === 'date_time' || field.data_type === 'time') {
                        group.dateType = "all";
                        group.dateTypeArea = "all";
                        $scope.loading = true;
                        return;
                    }

                }
                else {
                    group.data_type = field.data_type;
                    group.field = field.name;
                    delete group.dateType;
                    if (field.data_type === 'date' || field.data_type === 'date_time' || field.data_type === 'time') {
                        group.dateType = "all";
                        group.dateTypeArea = "all";
                        $scope.loading = true;
                        return;
                    }

                }

                if (field.data_type === 'date' || field.data_type === 'date_time' || field.data_type === 'time') {
                    $scope.loading = true;
                } else {
                    delete group.dateType;
                    $scope.changeAggregationType();
                }
            };

            $scope.gruopsForDateArea = [
                { label: $filter('translate')('View.DateAll'), type: "all" },
                { label: $filter('translate')('View.DateYear'), type: "year" },
                { label: $filter('translate')('View.DateYM'), type: "ym" },
                { label: $filter('translate')('View.DateYMD'), type: "ymd" }
            ];

            $scope.groupsForDate = function (group) {
                var type = $filter('filter')($scope.gruopsForDateArea, { type: group.dateTypeArea }, true)[0];
                group.dateType = type.type;
            }


            $scope.kanbanBoards = function () {
                $scope.boards = [];
                var obj = {
                    id: 0,
                    label: $filter('translate')('View.Uncategorized')
                }

                if ($scope.picklistsModule && $rootScope.activeView && $rootScope.activeView.settings && $rootScope.activeView.settings.kanbanPicklist) {
                    $scope.boards = Object.assign([], $scope.picklistsModule[$rootScope.activeView.settings.kanbanPicklist]);
                    $scope.boards.unshift(obj);
                }
            };

            $scope.setViewKanban = function () {

                ModuleService.getPicklists($scope.module)
                    .then(function (picklists) {
                        $rootScope.processPicklistLanguages(picklists);
                        $scope.picklistsModule = picklists;
                    });

                $scope.boards = [];

                if (!$rootScope.activeView.settings || !$rootScope.activeView.settings.kanbanPicklist) {
                    $scope.kanbanMainOptions = null;
                }
                else {
                    var viewGrid = $rootScope.activeView.view_type === "kanban" ? $rootScope.activeView : $scope.newGridData;
                    $scope.changeView(viewGrid, false);

                    if ($scope.boards && $scope.boards.length > 0) {
                        $scope.boards.forEach(function (item) {
                            $scope.kanbanListViewAltOptionsSet(item)
                        })
                    }
                }
            }

            $scope.boardTemplateMain = function () {
                return "<div class='col'><div class='list-wrapper'>" +
                    "        <div class='list-header'>" +
                    "          <span class='list-title'>#: label #</span>" +
                    "        </div>" +
                    "        <div id='list-kanban-#: id #' kendo-list-view k-rebind='kanbanListViewAltOptions#: id #' k-options='kanbanListViewAltOptions#: id #' class='list'></div>" +
                    "      </div></div>"
            };

            $scope.getKanbanDataLenght = 1;
            $scope.kanbanListViewAltOptionsSet = function (board) {
                var key = "kanbanListViewAltOptions" + board.id;
                var filterKey = "kanbanFilter" + board.id;
                var filterObj = {};

                $scope[filterKey] = angular.copy($scope.findRequestKanban);
                if (board.id === 0)
                    filterObj = { field: $scope[filterKey].picklist_name, operator: "empty", no: 1, value: "-" }
                else
                    filterObj = { field: $scope[filterKey].picklist_name, operator: "is", no: 1, value: board.labelStr ? board.labelStr : board.value }

                if ($scope[filterKey].filters && $scope[filterKey].filters.length === 1) {
                    filterObj.no = 2;
                    $scope[filterKey].filters.push(filterObj);
                    $scope[filterKey].filter_logic = "(1 and 2)";
                }
                else if ($scope[filterKey].filters && $scope[filterKey].filters.length > 1) {
                    filterObj.no = $scope[filterKey].filters.length + 1;
                    $scope[filterKey].filters.push(filterObj);
                    $scope[filterKey].filter_logic = "(" + filterObj.no + " and " + $scope[filterKey].filter_logic + ")";
                }
                else {
                    $scope[filterKey].filters = [filterObj];
                    $scope[filterKey].filter_logic = "";
                }

                $scope[key] = {
                    theme: 'default',
                    template: function (dataItem) {
                        var dynamicArea = "";
                        $scope.viewFields.selectedFields.forEach(function (item, index) {
                            if (index === 0) {
                                dynamicArea += "<div class='primary md-truncate'> <i class='fas fa-grip-vertical'></i> " + dataItem[item.name] + " </div>" //<i style='float: right; cursor: default; margin:0;' class='fas fa-chevron-circle-right'></i>
                            } else {
                                dynamicArea += "<div class='others md-truncate'><span>" + item.label + ":</span> <span class='label-data' ng-if='" + !!dataItem[item.name] + "'> " + dataItem[item.name] + " </span></div>"
                            }
                        });

                        return "<div id='kanban-sortable-" + dataItem.id + "' name='" + board.id + "' class='card kanban-card' ng-click='goUrl2(" + dataItem.id + ")'>" +
                            dynamicArea +
                            "</div>";
                    },
                    dataSource: {
                        serverPaging: true,
                        serverFiltering: true,
                        serverSorting: true,
                        pageSize: $scope.getItemCount,
                        transport: {
                            read: function (options) {
                                $.ajax({
                                    url: '/api/record/find_custom?locale=' + locale,
                                    contentType: 'application/json',
                                    dataType: 'json',
                                    type: 'POST',
                                    data: JSON.stringify(Object.assign($scope[filterKey], options.data)),
                                    success: function (result) {
                                        if (result.data) {
                                            //var data = $filter('orderBy')(result.data, 'id');
                                            options.success(result.data);
                                        } else {
                                            $(".k-loading-mask").remove();
                                        }

                                        if ($scope.boards && $scope.boards.length > 0 && $scope.boards.length === $scope.getKanbanDataLenght) {
                                            //boardların hepsinden data geldikten sonra ekranda gösteriminin saglanması ve sortable ataması

                                            if (!$scope.kanbanSelectPicklist.validation || !$scope.kanbanSelectPicklist.validation.readonly)
                                                $(".list .k-listview-content").kendoSortable($scope.sortableKanbanOptions);

                                            $scope.getKanbanDataLenght = 1;
                                            $scope.loading = false;
                                        }
                                        else {
                                            $scope.getKanbanDataLenght += 1;
                                        }

                                    },
                                    beforeSend: $rootScope.beforeSend()
                                })
                            }
                        },
                        schema: {
                            id: "id"
                        }
                    },
                }

                // jQuery.fn[key] = function(elem) {
                //     $("#list-kanban-"+board.id+ " .k-listview-content").scrollTop($(this).scrollTop() - $(this).offset().top + elem.offset().top);
                //     return this;
                // };

                $scope["scrollData" + board.id] = new kendo.data.DataSource({
                    serverPaging: true,
                    serverFiltering: true,
                    serverSorting: true,
                    pageSize: $scope.getItemCount,
                    transport: {
                        read: function (options) {
                            $.ajax({
                                url: '/api/record/find_custom?locale=' + locale,
                                contentType: 'application/json',
                                dataType: 'json',
                                type: 'POST',
                                data: JSON.stringify(Object.assign($scope[filterKey], options.data)),
                                success: function (result) {
                                    if (result.data) {
                                        options.success(result.data);
                                    }
                                    $scope.loading = false;
                                },
                                beforeSend: $rootScope.beforeSend()
                            })
                        }
                    },
                    schema: {
                        total: "total",
                        id: "id"
                    }
                });

                $timeout(function () {
                    $scope["page" + board.id] = 1;
                    $scope["scroll" + board.id] = true;
                    $("#list-kanban-" + board.id + " .k-listview-content").on('scroll', function () {

                        if ($(this).scrollTop() + $(this).innerHeight() + $(this)[0].scrollHeight * 25 / 100 >= $(this)[0].scrollHeight && $scope["scroll" + board.id]
                            || $(this).scrollTop() + $(this).innerHeight() >= $(this)[0].scrollHeight && $scope["scroll" + board.id]) {

                            $scope["scroll" + board.id] = false
                            $scope["page" + board.id]++;
                            var boardArea = $('#list-kanban-' + board.id);
                            //var lastItem = boardArea.last();
                            var listView = boardArea.data('kendoListView');
                            $scope["scrollData" + board.id].query({
                                page: $scope["page" + board.id],
                                pageSize: $scope.getItemCount
                            }).then(function () {
                                var data = $scope["scrollData" + board.id].data();
                                if (data.length > 0) {
                                    for (var i = 0; i < data.length; i++) {
                                        //     //listView.dataSource.add(x);
                                        listView.dataSource.pushCreate(data[i]);
                                    }
                                    $scope["scroll" + board.id] = true;
                                }

                                //$('#list-kanban-'+board.id+ " .k-listview-content")[key](lastItem);
                            });
                        }
                    })
                }, 800)

            }

            $scope.showModuleFrameModal = function (url) {

                var title, w, h;
                title = 'myPop1';
                w = document.body.offsetWidth - 200;
                h = document.body.offsetHeight - 200;
                var left = (screen.width / 2) - (w / 2);
                var top = (screen.height / 2) - (h / 2);
                window.open(url, title, 'toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=no, resizable=no, copyhistory=no, width=' + w + ', height=' + h + ', top=' + top + ', left=' + left);

            };

            $scope.trustAsHtml = function (value) {
                return $sce.trustAsHtml(value);
            };

            //webhook request func for action button
            $scope.webhookRequest = function (index) {
                if (!$scope.selectedRows || $scope.selectedRows.length < 1) {
                    mdToast.warning($filter('translate')('Module.NoRecordSelected'));
                    return;
                }

                var action = $scope.actionButtons[index];
                var params = action.parameters.split(',');
                var headers = action.headers.split(',');
                $scope.webhookRequesting = {};

                $scope.webhookRequesting[action.id] = true;

                ModuleService.getRecords($scope.module.name, $scope.selectedRows)
                    .then(function (recordsData) {
                        if (!recordsData || !recordsData.data || recordsData.data.length < 1) {
                            mdToast.error($filter('translate')('Common.NotFoundRecord'));
                            return;
                        }

                        for (var i = 0; i < recordsData.data.length; i++) {
                            var jsonData = {};
                            var headersData = { 'Content-Type': 'application/json' };
                            var record = ModuleService.processRecordSingle(recordsData.data[i], $scope.module, $scope.modulePicklists);

                            angular.forEach(params, function (data) {
                                var dataObject = data.split('|');
                                var parameterName = dataObject[0];
                                var moduleName = dataObject[1];
                                var fieldName = dataObject[2];

                                if (moduleName !== $scope.module.name) {
                                    if (record[moduleName])
                                        jsonData[parameterName] = record[moduleName][fieldName];
                                    else
                                        jsonData[parameterName] = null;

                                    // if page is form;
                                    // if($scope.record[moduleName][fieldName]){
                                    //     jsonData[parameterName] = $scope.record[moduleName][fieldName];
                                    // }
                                    // else{
                                    //     ModuleService.getRecord('accounts', $scope.record[moduleName].id)
                                    //         .then(function (response) {
                                    //             jsonData[parameterName] = response.data[fieldName];
                                    //         })
                                    // }
                                } else {
                                    if (record[fieldName])
                                        jsonData[parameterName] = record[fieldName];
                                    else
                                        jsonData[parameterName] = null;
                                }

                            });

                            angular.forEach(headers, function (data) {
                                var tempHeader = data.split('|');
                                var type = tempHeader[0];
                                var moduleName = tempHeader[1];
                                var key = tempHeader[2];
                                var value = tempHeader[3];

                                switch (type) {
                                    case 'module':
                                        var fieldName = value;
                                        if (moduleName !== $scope.module.name) {
                                            if (record[moduleName])
                                                headersData[key] = record[moduleName][fieldName];
                                            else
                                                headersData[key] = null;
                                        } else {
                                            if (record[fieldName])
                                                headersData[key] = record[fieldName];
                                            else
                                                headersData[key] = null;
                                        }
                                        break;
                                    case 'static':
                                        switch (value) {
                                            case '{:app:}':
                                                headersData[key] = $rootScope.user.app_id;
                                                break;
                                            case '{:tenant:}':
                                                headersData[key] = $rootScope.user.tenant_id;
                                                break;
                                            case '{:user:}':
                                                headersData[key] = $rootScope.user.id;
                                                break;
                                            default:
                                                headersData[key] = null;
                                                break;
                                        }
                                        break;
                                    case 'custom':
                                        headersData[key] = value;
                                        break;
                                    default:
                                        headersData[key] = null;
                                        break;
                                }

                            });

                            if (action.method_type === 'post') {

                                $http.post(action.url, jsonData, { headers: headersData })
                                    .then(function () {
                                        //mdToast.success($filter('translate')('Module.ActionButtonWebhookSuccess'));
                                        $scope.webhookRequesting[action.id] = false;
                                    })
                                    .catch(function () {
                                        mdToast.warning($filter('translate')('Module.ActionButtonWebhookFail'));
                                        $scope.webhookRequesting[action.id] = false;
                                    });

                            } else if (action.method_type === 'get') {

                                var query = "";

                                for (var key in jsonData) {
                                    query += key + "=" + jsonData[key] + "&";
                                }
                                if (query.length > 0) {
                                    query = query.substring(0, query.length - 1);
                                }

                                $http.get(action.url + "?" + query)
                                    .then(function () {

                                        // mdToast.success($filter('translate')('Module.ActionButtonWebhookSuccess'));
                                        $scope.webhookRequesting[action.id] = false;
                                    })
                                    .catch(function () {
                                        mdToast.warning($filter('translate')('Module.ActionButtonWebhookFail'));
                                        $scope.webhookRequesting[action.id] = false;
                                    });
                            }
                        }
                        mdToast.success($filter('translate')('Module.ActionButtonWebhookSuccess'));
                    });


            };

            $scope.runMicroflow = function (workflowId, index) {
                var button = $scope.actionButtons[index];

                if ($scope.selectedRows.length === 0 && button.record_name) {
                    mdToast.error($filter('translate')('Module.RequiredRecord'));
                    return;
                }

                $scope.buttonsParametersData = {};
                $scope.actionButtonsData = {
                    data: {
                        "module_id": $scope.module.id
                    },
                    "workflow_id": workflowId,
                    "button": button
                };

                //action button uzerinde eger bir record name setliyse bilgileri setliyoruz.
                //record'suz bir sekilde manual flowu calistirabilmesi icin bu kontrolu yapiyoruz.
                if (button.record_name) {
                    $scope.actionButtonsData.data.record_name = button.record_name;
                    $scope.actionButtonsData.data.record_ids = $scope.selectedRows;
                }


                if (button.parameters) {
                    $scope.showMicroflowParameters = true;
                    $scope.showScriptParameters = false;
                    $scope.buttonParameterNameTitle = $rootScope.getLanguageValue(button.languages, "label");

                    $scope.buttonsParameters = JSON.parse(button.parameters);

                    $scope.picklistItems = {};

                    angular.forEach($scope.buttonsParameters, function (parameter) {
                        if (parameter.type === 3 || parameter.type === 4) {
                            $http.get(config.apiUrl + 'picklist/get/' + parameter.lowerType)
                                .then(function (response) {
                                    $scope.picklistItems[parameter.key] = response.data;
                                    $rootScope.processLanguage($scope.picklistItems)
                                });
                        }
                    });

                    var parentEl = angular.element(document.body);
                    $mdDialog.show({
                        parent: parentEl,
                        templateUrl: 'view/app/module/actionButtonsParameterModal.html',
                        clickOutsideToClose: true,
                        //targetEvent: ev,
                        scope: $scope,
                        preserveScope: true
                    });

                } else {
                    ModuleService.runMicroflow(workflowId, $scope.actionButtonsData.data)
                        .then(function (res) {
                            var btnMsg = $rootScope.getLanguageValue(button.languages, 'message');
                            if (res.status === 200 && button.message_type === 'popup' && btnMsg) {
                                mdToast.success(btnMsg);
                            }
                        }).catch(function (res) {
                            if (res.status === 400)
                                mdToast.error($filter('translate')('Module.RequiredRecord'));
                        })
                }
            };

            $scope.runMicroflowParameters = function (form) {
                if (!form.validate())
                    return;

                $scope.actionButtonsData.data.parameters = $scope.buttonsParametersData;

                ModuleService.runMicroflow($scope.actionButtonsData.workflow_id, $scope.actionButtonsData.data)
                    .then(function (res) {
                        var btnMsg = $rootScope.getLanguageValue($scope.actionButtonsData.button.languages, 'message');
                        if (res.status === 200 && $scope.actionButtonsData.button.message_type === 'popup' && btnMsg) {
                            mdToast.success(btnMsg);
                        }
                    }).catch(function (res) {
                        if (res.status === 400)
                            mdToast.error($filter('translate')('Module.RequiredRecord'));
                    });

                $scope.closeLightBox();
            };

            $scope['numberOptionsButtons'] = {
                format: "{0:n0}",
                decimals: 0,
            };

            $scope.recordDelete = function (ev, id) {

                // Appending dialog to document.body to cover sidenav in docs app
                var confirm = $mdDialog.confirm()
                    .title($filter('translate')('Common.AreYouSure'))
                    .targetEvent(ev)
                    .ok($filter('translate')('Common.Yes'))
                    .cancel($filter('translate')('Common.No'));

                $mdDialog.show(confirm).then(function () {
                    ModuleService.getRecord($scope.module.name, id)
                        .then(function (recordData) {
                            if (!$scope.hasPermission($scope.type, operations.remove, recordData.data)) {
                                mdToast.error($filter('translate')('Common.Forbidden'));
                                return;
                            }

                            var record = ModuleService.processRecordSingle(recordData.data, $scope.module, $scope.modulePicklists);

                            $scope.executeCode = false;
                            components.run('BeforeDelete', 'Script', $scope, record);

                            if ($scope.executeCode)
                                return;

                            ModuleService.deleteRecord($scope.module.name, id)
                                .then(function () {
                                    $scope.refreshGrid();
                                    mdToast.success($filter('translate')('Module.DeleteMessage'));
                                });
                        });
                }, function () {
                    $scope.status = 'You decided to keep your debt.';
                });

            };

            $scope.hideCreateNew = function (field) {
                if (field.lookup_type === 'users')
                    return true;

                return field.lookup_type === 'relation' && !$scope.record.related_module;
            };

            $scope.deleteView = function (ev) {

                var confirm = $mdDialog.confirm()
                    .title($filter('translate')('Common.AreYouSure'))
                    .targetEvent(ev)
                    .ok($filter('translate')('Common.Yes'))
                    .cancel($filter('translate')('Common.No'));

                $mdDialog.show(confirm).then(function () {
                    ModuleService.deleteView($rootScope.activeView.id)
                        .then(function () {
                            mdToast.success({
                                content: $filter('translate')('View.DeleteSuccess'),
                                timeout: 5000
                            });
                            $scope.closeLightBox();
                            $cache.remove($scope.module.name + "-activeView");

                            ModuleService.getViews($scope.module).then(function (result) {
                                $rootScope.processLanguages(result);
                                $scope.views = result;
                                $scope.views.reverse();
                                $rootScope.activeView = $filter('filter')($scope.views, { default: true }, true)[0] || $scope.views[0];
                                $rootScope.closeSide('sideModal');
                                if ($scope.views.length === 0)
                                    window.location = "#/app/modules/" + $scope.module.name
                            });
                        });

                }, function () {
                    $scope.status = 'You decided to keep your debt.';
                });


            };

            $scope.export = function () {
                if ($scope.tableParams.total() < 1)
                    return;

                var isFileSaverSupported = false;

                try {
                    isFileSaverSupported = !!new Blob;
                } catch (e) {
                }

                if (!isFileSaverSupported) {
                    mdToast.warning({
                        content: $filter('translate')('Module.ExportUnsupported'),
                        timeout: 8000
                    });
                    return;
                }

                if ($scope.tableParams.total() > 3000) {
                    mdToast.warning({
                        content: $filter('translate')('Module.ExportWarning'),
                        timeout: 8000
                    });
                    return;
                }

                var fileName = $rootScope.getLanguageValue($scope.module.languages, 'label', 'plural') + '-' + $filter('date')(new Date(), 'dd-MM-yyyy') + '.xls';
                $scope.exporting = true;

                ModuleService.getCSVData($scope, $scope.type)
                    .then(function (csvData) {

                        mdToast.success({
                            content: $filter('translate')('Module.ExcelExportSuccess'),
                            timeout: 5000
                        });
                        exportFile.excel(csvData, fileName);
                        $scope.exporting = false;
                    });
            };

            $scope.showActivityButtons = function () {
                //TODO popover
                // $scope.activityButtonsPopover = $scope.activityButtonsPopover || $popover(angular.element(document.getElementById('activityButtons')), {
                //         templateUrl: 'view/common/newactivity.html',
                //         placement: 'bottom',
                //         autoClose: true,
                //         scope: $scope,
                //         show: true
                //     });
            };

            $scope.showDataTransferButtons = function () {
                //TODO popover
                // $scope.dataTransferButtonsPopover = $scope.dataTransferButtonsPopover || $popover(angular.element(document.getElementById('dataTransferButtons')), {
                //         template: 'view/common/datatransfer.html',
                //         placement: 'bottom',
                //         autoClose: true,
                //         scope: $scope,
                //         show: true
                //     });
            };

            $scope.openExportDialog = function () {
                $scope.pdfCreating = true;
                $scope.loadingModal = true;

                var openPdfModal = function () {
                    var parentEl = angular.element(document.body);
                    $mdDialog.show({
                        parent: parentEl,
                        templateUrl: 'view/app/module/modulePdfModal.html',
                        clickOutsideToClose: false,
                        scope: $scope,
                        preserveScope: true

                    });
                };

                ModuleService.getTemplates($scope.module.name, 'module')
                    .then(function (templateResponse) {
                        if (templateResponse.data.length === 0) {
                            if (!$rootScope.preview)
                                mdToast.warning($filter('translate')('Setup.Templates.TemplateNotFound'));
                            else
                                mdToast.warning($filter('translate')('Setup.Templates.TemplateDefined'));

                            $scope.pdfCreating = false;
                        } else {
                            var templateWord = templateResponse.data;
                            $scope.quoteTemplates = $filter('filter')(templateWord, { active: true }, true);

                            $scope.isShownWarning = true;
                            for (var i = 0; i < $scope.quoteTemplates.length; i++) {
                                var quoteTemplate = $scope.quoteTemplates[i];
                                if (quoteTemplate.permissions.length > 0) {
                                    var currentQuoteTemplate = $filter('filter')(quoteTemplate.permissions, { profile_id: $rootScope.user.profile.id }, true)[0];
                                    if (currentQuoteTemplate.type === 'none') {
                                        quoteTemplate.isShown = false;
                                    } else {
                                        quoteTemplate.isShown = true;
                                    }
                                    if (quoteTemplate.isShown === true) {
                                        $scope.isShownWarning = false;
                                    }
                                } else {
                                    quoteTemplate.isShown = true;
                                    $scope.isShownWarning = false;
                                }
                            }

                            $scope.quoteTemplatesOptions = {
                                dataSource: $filter('filter')($scope.quoteTemplates, { isShown: true }, true),
                                dataTextField: "name",
                                dataValueField: "id"
                            };
                            $scope.quoteTemplate = $scope.quoteTemplates[0];
                            $scope.loadingModal = false;
                            openPdfModal();
                        }
                    })
                    .catch(function () {
                        $scope.pdfCreating = false;
                    });

            };

            $scope.selectRow = function ($event, record) {
                /*selects or unselects records*/
                if ($event.target.checked) {
                    record.selected = true;
                    $scope.selectedRows.push(record.id);

                    $scope.selectedRecords.push({
                        id: record.id,
                        displayName: record[$scope.module.primaryField.name]
                    });
                    return;

                } else {
                    record.selected = false;
                    $scope.selectedRows = $scope.selectedRows.filter(function (selectedItem) {
                        return selectedItem !== record.id;
                    });
                }
                $scope.isAllSelected = false;
            };

            $scope.isRowSelected = function (id) {
                return $scope.selectedRows.filter(function (selectedItem) {
                    return selectedItem === id;
                }).length > 0;
            };

            $scope.selectGroups = function ($event, groupsName, groupValue, isLastGroup) {
                //$scope.isGroupSelected = true;
                //console.log(isLastGroup)
                $scope.isAllSelected = false;
                var dataGr = $('#kendo-grid').data("kendoGrid").dataSource.data();

                for (var i = 0; i < dataGr.length; i++) {
                    if (!dataGr[i].hasSubgroups) {
                        for (var j = 0; j < dataGr[i].items.length; j++) {
                            if (dataGr[i].items[j][groupsName] === groupValue) {
                                if ($event.target.checked) {
                                    if (!dataGr[i].items[j].freeze || $scope.user.profile.has_admin_rights) {
                                        if (!$scope.selectedRows.includes(dataGr[i].items[j].id)) {
                                            dataGr[i].items[j].selected = true;
                                            $scope.selectRow($event, dataGr[i].items[j]);
                                        }
                                    }
                                }
                                else {
                                    var index = $scope.selectedRows.indexOf(dataGr[i].items[j].id);
                                    $scope.selectedRows.splice(index, 1);
                                    dataGr[i].items[j].selected = false;
                                }
                            }
                        }
                    }
                    else {//alt group
                        for (var j = 0; j < dataGr[i].items.length; j++) {
                            for (var k = 0; k < dataGr[i].items[j].items.length; k++) {
                                if (dataGr[i].items[j].items[k][groupsName] === groupValue) {
                                    if ($event.target.checked) {
                                        if (!dataGr[i].items[j].items[k].freeze || $scope.user.profile.has_admin_rights) {
                                            if (!$scope.selectedRows.includes(dataGr[i].items[j].items[k].id)) {
                                                dataGr[i].items[j].items[k].selected = true;
                                                $scope.selectRow($event, dataGr[i].items[j].items[k]);
                                            }
                                        }
                                    }
                                    else {
                                        var index = $scope.selectedRows.indexOf(dataGr[i].items[j].items[k].id);
                                        $scope.selectedRows.splice(index, 1);
                                        dataGr[i].items[j].items[k].selected = false;
                                    }
                                }
                            }
                        }
                    }
                }

                if (isLastGroup) {
                    //TODO grubun sonraki sonrafalarında elemanlarının secim işlemleri yapılacak, suan grup secimleri pasif...
                }
            };

            $scope.selectAll = function ($event, data) {
                $scope.selectedRows = [];
                $scope.isGroupSelected = false;

                if ($scope.isAllSelected) {
                    $scope.isAllSelected = false;
                    for (var i = 0; i < data.length; i++) {
                        if (data[i].items) {
                            for (var j = 0; j < data[i].items.length; j++) {
                                if (data[i].items[j].items) {
                                    for (var k = 0; k < data[i].items[j].items.length; k++) {
                                        if (data[i].items[j].items[k].selected)
                                            data[i].items[j].items[k].selected = false;
                                    }
                                } else {
                                    if (data[i].items[j].selected)
                                        data[i].items[j].selected = false;
                                }
                            }
                        } else {
                            if (data[i].selected)
                                data[i].selected = false;
                        }
                    }
                } else {
                    $scope.isAllSelected = true;
                    for (var i = 0; i < data.length; i++) {
                        if (data[i].items) {
                            for (var j = 0; j < data[i].items.length; j++) {//group one
                                if (data[i].items[j].items) {
                                    for (var k = 0; k < data[i].items[j].items.length; k++) {// group two
                                        if (!data[i].items[j].items[k].freeze || $scope.user.profile.has_admin_rights) {
                                            data[i].items[j].items[k].selected = true;
                                            $scope.selectRow($event, data[i].items[j].items[k]);
                                        } else
                                            data[i].items[j].items[k].selected = false;
                                    }
                                } else {
                                    if (!data[i].items[j].freeze || $scope.user.profile.has_admin_rights) {
                                        data[i].items[j].selected = true;
                                        $scope.selectRow($event, data[i].items[j]);
                                    } else
                                        data[i].items[j].selected = false;
                                }
                            }
                        } else {
                            if (!data[i].freeze || $scope.user.profile.has_admin_rights) {
                                data[i].selected = true;
                                $scope.selectRow($event, data[i]);
                            } else
                                data[i].selected = false;
                        }
                    }
                }
            };

            $scope.deleteSelecteds = function (ev) {

                if (!$scope.selectedRows || !$scope.selectedRows.length) {
                    mdToast.warning($filter('translate')('Module.NoRecordSelected'));
                    return;
                }

                var confirm = $mdDialog.confirm()
                    .title($filter('translate')('Common.AreYouSure'))
                    .textContent($filter('translate')('Module.SelectedRecordsCount') + $scope.selectedRows.length)
                    .targetEvent(ev)
                    .ok($filter('translate')('Common.Yes'))
                    .cancel($filter('translate')('Common.No'));

                $mdDialog.show(confirm).then(function () {

                    ModuleService.deleteRecordBulk($scope.module.name, $scope.selectedRows)
                        .then(function () {

                            mdToast.success($filter('translate')('Module.DeleteMessage'));
                            $scope.selectedRows = [];
                            $scope.isAllSelected = false;
                            $scope.refreshGrid();
                        });
                }, function () {
                    $scope.status = 'You decided to keep your debt.';
                });


            };

            $scope.addCustomField = function ($event, customField) {
                /// adds custom fields to the html template.
                tinymce.activeEditor.execCommand('mceInsertContent', false, "{" + customField.name + "}");
            };

            $scope.showEMailModal = function () {
                $scope.executeCode = false;
                if (!$rootScope.system.messaging.SystemEMail && !$rootScope.system.messaging.PersonalEMail) {
                    mdToast.warning($filter('translate')('EMail.NoProvider'));
                    return;
                }

                if ($scope.selectedRows.length == 0 && !$scope.isAllSelected) {
                    mdToast.warning($filter('translate')('Module.NoRecordSelected'));
                    return;
                }
                components.run('BeforeModalLoad', 'Script', $scope);

                /*Generates and displays modal form for the mail*/
                var parentEl = angular.element(document.body);
                if (!$scope.executeCode) {
                    $mdDialog.show({
                        parent: parentEl,
                        templateUrl: 'view/app/email/bulkEMailModal.html',
                        clickOutsideToClose: false,
                        scope: $scope,
                        preserveScope: true
                    });
                }


            };

            $scope.showSMSModal = function () {
                if (!$rootScope.system.messaging.SMS) {
                    mdToast.warning($filter('translate')('SMS.NoProvider'));
                    return;
                }

                if ($scope.selectedRows.length == 0 && !$scope.isAllSelected) {
                    mdToast.warning($filter('translate')('Module.NoRecordSelected'));
                    return;
                }

                /*Generates and displays modal form for the mail*/
                var parentEl = angular.element(document.body);
                $mdDialog.show({
                    parent: parentEl,
                    templateUrl: 'view/app/sms/bulkSMSModal.html',
                    clickOutsideToClose: false,
                    scope: $scope,
                    preserveScope: true

                });

            };

            $scope.collectiveApproval = function () {

                $scope.loading = true;
                var arrayApprove = [];
                angular.forEach($scope.selectedRows, function (value) {
                    var record = $filter('filter')($scope.grid.dataSource.data(), { id: value }, true)[0];
                    if (!angular.isUndefined(record))
                        if (record['process.process_requests.process_status'] !== 2)
                            arrayApprove.push(record.id)
                });

                if (arrayApprove.length > 0) {
                    ModuleService.approveMultipleProcessRequest(arrayApprove, $scope.module.name)
                        .then(function (response) {
                            mdToast.success(arrayApprove.length + $filter('translate')('Module.ApprovedRecordCount'));

                            $scope.loading = false;
                            $scope.refresh(true);

                        });
                }
            };

            $scope.showCollectiveApproval = function () {

                $scope.selected = $scope.selectedRows.length;

                if (!$scope.selectedRows || $scope.selectedRows.length > 0) {
                    //TODO modal
                    // $scope.collectiveApprovalModal = $scope.collectiveApprovalModal || $modal({
                    //         scope: $scope,
                    //         templateUrl: 'view/app/module/collectiveApproveAlert.html',
                    //         animation: '',
                    //         backdrop: 'static',
                    //         show: false,
                    //         tag: 'createModal'
                    //     });
                    // $scope.collectiveApprovalModal.$promise.then($scope.collectiveApprovalModal.show);
                } else
                    mdToast.warning($filter('translate')('Module.NoRecordSelected'));
            };

            $scope.dropdownHide = function () {
                var element = angular.element(document.getElementById('dropdownMenu1'));
                if (element[0]) element[0].click();
                if (element[1]) element[1].click();
            };

            $scope.closeLightBox = function () {
                $mdDialog.hide();
            };

            $scope.showLightBox = function (ev, record, isImage, fieldName, relation) {

                if (!record)
                    return;

                $scope.showImageData = {};
                $scope.multiSelectAndTagDatas = {};
                var module = relation ? $rootScope.modulus[relation.related_module] : $scope.module;
                var field = fieldName ? $filter('filter')(module.fields, {
                    name: fieldName,
                    deleted: false
                })[0] : $scope.module.primaryField.name;

                //location & image srcs
                if (isImage) {
                    $scope.showImageData = {
                        url: ev.target.src,
                        title: record[field] || $rootScope.getLanguageValue(field.languages, 'label'),
                        type: fieldName ? 'location' : 'image',
                        map_url: "http://www.google.com/maps/place/" + record[field.name]
                    };
                } else if (record[fieldName]) {
                    field = $filter('filter')(module.fields, { name: fieldName, deleted: false })[0];
                    $scope.multiSelectAndTagDatas = {
                        array: record[fieldName].split(';'),
                        title: record[field] || $rootScope.getLanguageValue(field.languages, 'label')
                    };
                }

                $mdDialog.show({
                    contentElement: '#mdLightbox',
                    parent: angular.element(document.body),
                    targetEvent: ev,
                    clickOutsideToClose: true,
                    fullscreen: false
                });

            };


            $scope.setCurrentLookupField = function (field) {
                //$scope.currentLookupField = field;
            };

            var prepareLookupSearchField = function () {
                if ($scope.field.lookup_type === 'users' && !$scope.field.lookupModulePrimaryField) {
                    var userModulePrimaryField = {};
                    userModulePrimaryField.data_type = 'text_single';
                    userModulePrimaryField.name = 'full_name';
                    $scope.field.lookupModulePrimaryField = userModulePrimaryField;
                }

                if ($scope.field.lookup_type === 'profiles' && !$scope.field.lookupModulePrimaryField) {
                    var userModulePrimaryField = {};
                    userModulePrimaryField.data_type = 'text_single';
                    userModulePrimaryField.name = 'name';
                    $scope.field.lookupModulePrimaryField = userModulePrimaryField;
                }

                if ($scope.field.lookup_type === 'roles' && !$scope.field.lookupModulePrimaryField) {
                    var userModulePrimaryField = {};
                    userModulePrimaryField.data_type = 'text_single';
                    userModulePrimaryField.name = 'label_' + $rootScope.user.tenantLanguage;
                    $scope.field.lookupModulePrimaryField = userModulePrimaryField;
                }

                if ($scope.field.lookup_type === 'relation') {
                    if (!$scope.record.related_module) {
                        return $q.defer().promise;
                    }

                    var relationModule = $filter('filter')($rootScope.modules, { name: $scope.record.related_module.value }, true)[0];

                    if (!relationModule) {

                        return $q.defer().promise;
                    }

                    $scope.field.lookupModulePrimaryField = $filter('filter')(relationModule.fields, { primary: true }, true)[0];
                }

            };

            $scope.inputReset = function () {
                $scope.bulkUpdate.value = null;

                if ($scope.field.data_type === 'picklist') {
                    $scope.optionId = $scope.field.picklist_id;
                }

                if ($scope.field.data_type === 'lookup') {
                    $scope.currentLookupField = $scope.field;

                    var dataSource = new kendo.data.DataSource({
                        serverFiltering: true,
                        transport: {
                            read: function (options) {
                                var findRequest = {
                                    module: $scope.currentLookupField.lookup_type,
                                    convert: false
                                };
                                if (!options.data.filter || (options.data.filter && options.data.filter.filters.length === 0)) {
                                    options.data.filter = {};
                                    options.data.filter.filters = [];
                                    options.data.filter.logic = 'and';
                                    prepareLookupSearchField();
                                }
                                if (options.data.filter.filters.length > 0) {
                                    var operator = options.data.filter.filters[0].operator;
                                    if (operator.contains('_'))
                                        options.data.filter.filters[0].operator = operator !== "" ? operator.replace('_', '') : "startswith";

                                    $scope.lookupSearchValue = options.data.filter.filters[0].value;
                                }
                                $.ajax({
                                    url: '/api/record/find_custom',
                                    contentType: 'application/json',
                                    dataType: 'json',
                                    type: 'POST',
                                    data: JSON.stringify(Object.assign(findRequest, options.data)),
                                    success: function (result) {
                                        options.success(result);
                                    },
                                    beforeSend: $rootScope.beforeSend()
                                })
                            }
                        },
                        schema: {
                            data: "data",
                            total: "total",
                            model: { id: "id" }
                        },
                    });

                    $scope['lookupOptions' + $scope.field.id] = {
                        dataSource: dataSource,
                        optionLabel: $filter('translate')('Common.Select'),
                        dataTextField: $scope.field.lookupModulePrimaryField.name,
                        dataValueField: "id"
                    };
                    var lookupInstance = angular.element(document.getElementById($scope.field.name)).data("kendoDropDownList");
                    if (lookupInstance) {
                        lookupInstance.setOptions({ dataTextField: $scope.field.lookupModulePrimaryField.name });
                        lookupInstance.setDataSource(dataSource);
                    }
                }
            };

            $scope.updateSelected = function (bulkUpdateModalForm) {
                if (!$scope.selectedRows || !$scope.selectedRows.length)
                    return;

                if (!bulkUpdateModalForm.validate())
                    return;

                $scope.submittingModal = true;
                var request = {};
                var fieldName = $scope.field.name;
                request.ids = $scope.selectedRows;
                var bulkUpdateRecord = {};
                request.records = [];
                if ($scope.field.data_type !== 'picklist' && $scope.field.data_type !== 'multiselect' && $scope.field.data_type !== 'date' && $scope.field.data_type !== 'tag') {
                    bulkUpdateRecord[fieldName] = $scope.bulkUpdate.value;
                } else {
                    switch ($scope.field.data_type) {
                        case "picklist":
                            bulkUpdateRecord[fieldName] = $scope.bulkUpdate.value.id;
                            break;
                        case "multiselect":
                            var ids = [];
                            for (var i = 0; i < $scope.bulkUpdate.value.length; i++) {
                                ids.push($scope.bulkUpdate.value[i].id);
                            }
                            bulkUpdateRecord[fieldName] = ids;
                            break;
                        case "tag":
                            var tags = [];
                            for (var j = 0; j < $scope.bulkUpdate.value.length; j++) {
                                var tag = $filter('filter')($scope.field.tag.dataSource._view, { id: parseInt($scope.bulkUpdate.value[j]) }, true)[0];
                                if (tag)
                                    tags.push(tag.text);
                            }
                            bulkUpdateRecord[fieldName] = tags.toString();

                            break;
                        case "date":
                            var dateParts = moment($scope.bulkUpdate.value).format().split('+');
                            bulkUpdateRecord[fieldName] = dateParts[0];
                            break;
                    }

                }

                request.records.push(bulkUpdateRecord);

                ModuleService.updateRecordBulk($scope.module.name, request)
                    .then(function () {
                        $scope.closeLightBox();
                        //$scope.submittingModal = false;
                        mdToast.success($filter('translate')('Module.UpdateRecordBulkSuccess'));
                        $scope.refreshGrid();
                        $scope.isAllSelected = false;
                        $scope.selectedRows = [];
                        $scope.selectedRecords = [];
                        $scope.bulkUpdate.value = null;
                        $scope.field = null;
                    });
            };

            $scope.openCalendar = function (field) {
                ModuleService.openCalendar(field);
            };

            $scope.openExcelTemplate = function () {
                $scope.excelCreating = true;
                $scope.hasQuoteTemplateDisplayPermission = ModuleService.hasQuoteTemplateDisplayPermission;

                var openExcelModal = function () {
                    var parentEl = angular.element(document.body);
                    $mdDialog.show({
                        parent: parentEl,
                        templateUrl: 'view/app/module/moduleExcelModal.html',
                        clickOutsideToClose: false,
                        scope: $scope,
                        preserveScope: true
                    });
                };

                if ($scope.quoteTemplates) {
                    openExcelModal();
                    $scope.quoteTemplate = $scope.quoteTemplates[0];
                }

                ModuleService.getTemplates($scope.module.name, 'excel')
                    .then(function (templateResponse) {
                        if (templateResponse.data.length === 0) {
                            if (!$rootScope.preview)
                                mdToast.warning($filter('translate')('Setup.Templates.TemplateNotFound'));
                            else
                                mdToast.warning($filter('translate')('Setup.Templates.TemplateDefined'));

                            $scope.excelCreating = false;
                        } else {
                            var templateExcel = templateResponse.data;
                            $scope.quoteTemplates = $filter('filter')(templateExcel, { active: true }, true);
                            $scope.isShownWarning = true;
                            for (var i = 0; i < $scope.quoteTemplates.length; i++) {
                                var quoteTemplate = $scope.quoteTemplates[i];
                                if (quoteTemplate.sharing_type === "everybody")
                                    quoteTemplate.isShown = true;
                                else if (quoteTemplate.sharing_type === "me") {
                                    quoteTemplate.isShown = quoteTemplate.created_by_id === $rootScope.user.id;
                                }
                                else if (quoteTemplate.sharing_type === "profile") {
                                    quoteTemplate.isShown = quoteTemplate.profile_list[$rootScope.user.profile.id];
                                }

                                if (quoteTemplate.isShown === true)
                                    $scope.isShownWarning = false;
                            }

                            $scope.quoteTemplate = $scope.quoteTemplates[0];
                            openExcelModal();
                        }
                    })
                    .catch(function () {
                        $scope.excelCreating = false;
                    });
            };

            $scope.UpdateMultiselect = function (searchTerm, field) {
                var picklistItems = [];

                angular.forEach($scope.modulePicklists[field.picklist_id], function (picklistItem) {
                    if (picklistItem.inactive)
                        return;

                    //if (picklistItem.labelStr.toLowerCase().indexOf(searchTerm) > -1)
                    var item = $rootScope.getLanguageValue(picklistItem.languages, 'label');
                    if (item && item.toLowerCase().indexOf(searchTerm) > -1)
                        picklistItems.push(picklistItem);
                });

                return picklistItems;
            };

            ModuleService.getPicklists($scope.module, true)
                .then(function (picklists) {
                    var lookupUsersOptions = angular.copy($scope.sharesOptions);

                    lookupUsersOptions.dataSource.push({
                        full_name: $filter('translate')('Common.LoggedInUser'),
                        id: "[me]"
                    });

                    $rootScope.processPicklistLanguages(picklists);
                    $scope.modulePicklists = picklists;
                    $scope.picklistOptions = [];
                    for (var i = 0; i < $scope.module.fields.length; i++) {
                        var field = $scope.module.fields[i];
                        switch (field.data_type) {
                            case 'picklist':
                                if (field.data_type === 'picklist' && !field.deleted) {
                                    field.picklist = $scope.modulePicklists[field.picklist_id];
                                }
                                break;
                            case 'multiselect':
                                field.options = {
                                    dataSource: $scope.modulePicklists[field.picklist_id],
                                    filter: "contains",
                                    dataTextField: 'languages.' + $rootScope.globalization.Label + '.label',
                                    dataValueField: "id",
                                    optionLabel: $filter('translate')('Common.Select')
                                };
                                break;
                            case "tag":
                                var dataSource = new kendo.data.DataSource({
                                    transport: {
                                        read: {
                                            url: "/api/tag/get_tag/" + field.id,
                                            type: 'GET',
                                            dataType: "json",
                                            beforeSend: $rootScope.beforeSend(),
                                        }
                                    }
                                });
                                dataSource.read();
                                field.tag = {
                                    dataSource: dataSource,
                                    dataTextField: "text",
                                    dataValueField: "id",
                                    valuePrimitive: true,
                                    filter: "contains",
                                    tagMode: "multiple",
                                    change: function () {
                                        var selectedValues = this.value();
                                        var currentTagMode = this.options.tagMode;
                                        var newTagMode = currentTagMode;
                                        if (selectedValues.length <= 5) {
                                            newTagMode = "multiple";
                                        } else {
                                            newTagMode = "single"
                                        }
                                        if (newTagMode !== currentTagMode) {
                                            this.value([]);
                                            this.setOptions({
                                                tagMode: newTagMode
                                            });
                                            this.value(selectedValues);
                                        }
                                    }
                                };
                                break;
                            case 'lookup':
                                if (field.lookup_type === 'users') {
                                    field.dataOptions = lookupUsersOptions;
                                } else {
                                    //fieldValue = value;
                                }
                                break;
                            case 'date':
                            case 'date_time':
                            case 'time':
                                //fieldValue = new Date(value);
                                break;
                            default:
                                // fieldValue = value;
                                break;
                        }
                    }
                });

            $scope.showUpdateModal = function (ev) {
                if (!$scope.selectedRows || !$scope.selectedRows.length) {
                    mdToast.warning($filter('translate')('Module.NoRecordSelected'));
                    return;
                }

                if ($scope.selectedRows.length > 100) {
                    mdToast.warning($filter('translate')('Module.RecordLimit'));
                    return;
                }

                var fields = [];

                var permissionCheck = function () {
                    fields = angular.copy($scope.module.fields);

                    if ($scope.isAdmin || $scope.hasBulkUpdatePermission)
                        return;

                    for (var i = 0; i < $scope.module.sections.length; i++) {
                        var section = $scope.module.sections[i];

                        if (!ModuleService.hasSectionFullPermission(section) || !ModuleService.hasSectionDisplayPermission(section) || ModuleService.hasSectionReadOnlyPermission(section))
                            fields = $filter('filter')(fields, function (field) {
                                return field.section !== section.name
                            }, true);
                    }

                    for (var i = 0; i < fields.length; i++) {
                        if (!ModuleService.hasFieldFullPermission(fields[i]) || !ModuleService.hasFieldDisplayPermission(fields[i]) || ModuleService.hasFieldReadOnlyPermission(fields[i])) {
                            var index = fields.indexOf(fields[i]);
                            fields.splice(index, 1);
                        }
                    }
                };

                permissionCheck();

                $scope["fieldOptions"] = {
                    dataSource: $filter('filter')(fields, function (field) {
                        if (field.data_type === 'number' && !field.deleted) {
                            field = ModuleService.setMinMaxValueForField(field);
                            $scope['numberOptions' + field.id] = {
                                format: "{0:n0}",
                                decimals: 0
                            };
                        } else if (field.data_type === 'number_decimal' && !field.deleted) {
                            field = ModuleService.setMinMaxValueForField(field);
                        } else if (field.data_type === 'currency' && !field.deleted) {

                            field = ModuleService.setMinMaxValueForField(field);
                            $scope['currencyOptions' + field.id] = {
                                culture: kendo.cultures.current.name,
                                format: "c",
                                decimals: kendo.cultures.current.numberFormat.currency.decimals
                            };
                        }
                        return !(field.data_type === 'checkbox' && !preview && field.name === 'is_sample') && field.data_type !== 'image' && field.data_type !== 'location' && field.data_type !== 'document' && field.data_type !== 'number_auto' && field.name !== 'updated_at' && field.name !== 'updated_by' && field.name !== 'created_at' && field.name !== 'created_by' && (field.validation && !field.validation.readonly) && !field.custom_label;
                    }, true),
                    dataTextField: 'languages.' + $rootScope.globalization.Label + '.label',
                    dataValueField: "id",
                    autoBind: false,
                    filter: 'contains',
                    optionLabel: $filter('translate')('Common.Select')
                };

                if ($scope.picklistsModule)
                    ModuleService.getPicklists($scope.module)
                        .then(function (picklists) {
                            $rootScope.processPicklistLanguages(picklists);
                            $scope.picklistsModule = picklists;
                        });

                $scope.selected = $scope.selectedRows.length;
                var parentEl = angular.element(document.body);

                $mdDialog.show({
                    parent: parentEl,
                    templateUrl: 'view/app/module/bulkUpdateModal.html',
                    clickOutsideToClose: true,
                    targetEvent: ev,
                    scope: $scope,
                    preserveScope: true

                });

                angular.element(document).ready(function () {
                    $scope.bulkUpdateModalForm.options.rules['range'] = ModuleService.rangeRuleForForms();
                });

            };

            $scope.showDeleteModal = function () {
                $scope.selected = $scope.selectedRows.length;
            };

            $scope.showExportDataModal = function (ev) {

                $scope.export.moduleAllColumn = null;

                var parentEl = angular.element(document.body);
                $mdDialog.show({
                    parent: parentEl,
                    templateUrl: 'view/app/module/exportData.html',
                    clickOutsideToClose: false,
                    targetEvent: ev,
                    scope: $scope,
                    preserveScope: true

                });

            };

            //kaydın process detaylarını gösterme
            $scope.recordProcessDetail = function (record) {

                if ($scope.previousApprovers)
                    delete $scope.previousApprovers;

                if ($scope.processOrderParam)
                    delete $scope.processOrderParam;

                if ($scope.currentApprover)
                    delete $scope.currentApprover;

                if ($scope.updateTime)
                    delete $scope.updateTime;

                if ($scope.rejectApprover)
                    delete $scope.rejectApprover;

                var currentModuleProcess;


                if (currentModuleProcess.approver_type === 'dynamicApprover') {
                    $scope.loadingProcessPopup = true;
                    $scope.processStatusParam = record["process.process_requests.process_status"];

                    ModuleService.getRecord($scope.module.name, record.id)
                        .then(function (recordData) {
                            var processOrderParam, currentApprover, updateTime, rejectApprover;
                            var record = recordData.data;
                            var previousApprovers = [];
                            if (record.process_status === 1) {
                                processOrderParam = record.process_status_order;
                                if (record.process_status_order === 1) {
                                    currentApprover = $filter('filter')($rootScope.users, { email: record.custom_approver }, true)[0].full_name;
                                } else {
                                    currentApprover = $filter('filter')($rootScope.users, { email: record['custom_approver_' + record.process_status_order] }, true)[0].full_name;
                                    var firstApprover = $filter('filter')($rootScope.users, { email: record.custom_approver }, true)[0].full_name;
                                    previousApprovers.push(firstApprover);
                                    for (var i = 2; i < record.process_status_order; i++) {
                                        previousApprovers.push($filter('filter')($rootScope.users, { email: record['custom_approver_' + i] }, true)[0].full_name);
                                    }
                                    $scope.previousApprovers = previousApprovers;
                                }
                                $scope.processOrderParam = processOrderParam;
                                $scope.currentApprover = currentApprover;
                            } else if (record.process_status === 2) {
                                updateTime = record["process_request_updated_at"];
                                var firstApprover = $filter('filter')($rootScope.users, { email: record.custom_approver }, true)[0].full_name;
                                previousApprovers.push(firstApprover)
                                for (var i = 2; i < record.process_status_order + 1; i++) {
                                    previousApprovers.push($filter('filter')($rootScope.users, { email: record['custom_approver_' + i] }, true)[0].full_name);
                                }
                                $scope.previousApprovers = previousApprovers;
                                $scope.updateTime = moment(updateTime).utc().format("DD-MM-YYYY HH:mm");
                            } else if (record.process_status === 3) {
                                updateTime = record["process_request_updated_at"];
                                rejectApprover = $filter('filter')($rootScope.users, { id: record["process_request_updated_by"] }, true)[0].full_name;
                                $scope.rejectApprover = rejectApprover;
                                $scope.updateTime = moment(updateTime).utc().format("DD-MM-YYYY HH:mm");
                            }
                            $scope.loadingProcessPopup = false;
                        })
                }
            };

            $scope.filterModalOpen = function (type) {
                $rootScope.buildToggler('sideModal', 'view/app/module/filter-view.html');
                setTimeout(function () {
                    var listBox = $("#available-fields").data("kendoListBox");
                    if (listBox) {
                        for (i = 0; listBox.options.dataSource.length > i; i++) {
                            if (listBox.options.dataSource[i].name.includes("seperator-")) {
                                listBox.enable($(".k-item").eq(i), false);
                            }
                        }
                    }
                }, 200);

            };

            $scope.refreshGrid = function () {
                $("#kendo-grid").data("kendoGrid").dataSource.read();
            };

            $scope.goToRecord = function (item, lookupType, showAnchor, dataItem, externalLink) {
                ModuleService.goToRecord(item, lookupType, showAnchor, dataItem, externalLink);
            };

            $scope.getTagAndMultiDatas = function (dataItem, fieldName) {
                if (dataItem[fieldName]) {
                    var dataArray = dataItem[fieldName].split(';');
                    return ModuleService.getTagAndMultiDatas(dataItem, dataArray);
                }
            };

            $scope.getImageStyle = function (fieldName) {
                if (fieldName) {
                    return ModuleService.getImageStyle(fieldName, $scope.module.name);
                }
            };

            $scope.getRatingCount = function (fieldName) {
                if (fieldName) {
                    return ModuleService.getRatingCount(fieldName, $scope.module.name);
                }
            };

            $scope.getLocationUrl = function (coordinates) {
                if (coordinates)
                    return ModuleService.getLocationUrl(coordinates);
            };

            $scope.newGridData = {
                module_id: $scope.module.id,
                languages: {},
                //label_en: "All " + $scope.module.label_en_plural,
                //label_tr: "Tüm " + $scope.module.label_tr_plural,
                active: true,
                sharing_type: "everybody",
                isNew: true,
                filter_logic_json: '{"group":{"logic":"and","filters":[],"level":1}}',
                default: true,
                view_type: "grid",
                report_feed: "module",
                report_type: "",
                sort_direction: "",
                filters: [],
                fields: [
                    {
                        field: $scope.module.primaryField.name,
                        order: 1
                    }
                ]
            };
            $scope.newGridData.languages[$rootScope.globalization.Label] = {
                label: "All" + $rootScope.getLanguageValue($scope.module.languages, 'label', 'plural')
            };
            $scope.openNewDashlet = function () {
                $scope.views = [];
                $scope.activeView = angular.copy($scope.newGridData);
                $scope.views.push($scope.activeView);
                $scope.changeView($scope.activeView);
                $scope.filterModalOpen();
            };

            $scope.changeTotalCount = function (state) {
                $scope.totalCount = state;
                if (state) {
                    $rootScope.activeView.aggregation = {
                        aggregation_type: "count",
                        field: "created_by"
                    };
                    $rootScope.activeView.aggregations[0] = $rootScope.activeView.aggregation;
                    $rootScope.activeView.report_type === "summary" ? $scope.chartFilter() : $scope.widgetFilter();
                }
            };

            $scope.setViewAggregationsFields = function (data) {
                if ($rootScope.activeView.view_type !== "report")
                    return false;
                var aggregations = [];

                if ($rootScope.activeView.report_type === 'tabular') {
                    for (var i = 0; i < data.length; i++) {
                        var field = data[i];
                        if ((field.data_type === 'numeric' || field.data_type === 'number' || field.data_type === 'number_auto' || field.data_type === 'currency' || field.data_type === 'number_decimal') && !field.deleted) {
                            if ($rootScope.activeView.aggregations && $rootScope.activeView.aggregations.length > 0)
                                var aggregationField = $filter('filter')($rootScope.activeView.aggregations, { field: field.name })[0];

                            if (aggregationField)
                                aggregations.push(aggregationField);
                            else
                                aggregations.push({ field: field.name, aggregation_type: "" })

                        }
                    }
                    $rootScope.activeView.aggregations = aggregations;
                }
            };

            $scope.controlRemove = function (dataItem, relatedModuleName) {
                return dataItem.showRemove = $scope.hasPermission(relatedModuleName ? relatedModuleName : $scope.module.name, operations.remove);
            };

            $scope.controlCopy = function (dataItem, relatedModuleName) {
                return dataItem.showCopy = $scope.hasPermission(relatedModuleName ? relatedModuleName : $scope.module.name, operations.write);
            };

            $scope.controlEdit = function (dataItem, relatedModuleName) {
                return dataItem.showEdit = $scope.hasPermission(relatedModuleName ? relatedModuleName : $scope.module.name, operations.modify);
            };

            $scope.validSelectedFields = function () {
                if ($scope.activeView.view_type === 'view' || $scope.activeView.report_type === 'tabular')
                    return $scope.viewFields.selectedFields.length >= 1;

                return true;
            };

            $scope.runScript = function (index) {
                var action = $scope.actionButtons[index];

                if (action.parameters) {
                    $scope.showMicroflowParameters = false;
                    $scope.showScriptParameters = true;
                    $scope.buttonParameterNameTitle = $rootScope.getLanguageValue(action.languages, "label");

                    $scope.buttonsParameters = JSON.parse(action.parameters);

                    $scope.picklistItems = {};
                    angular.forEach($scope.buttonsParameters, function (parameter) {
                        if (parameter.type === 3 || parameter.type === 4) {
                            $http.get(config.apiUrl + 'picklist/get/' + parameter.lowerType)
                                .then(function (response) {
                                    $scope.picklistItems[parameter.key] = response.data;
                                    $rootScope.processLanguage($scope.picklistItems)
                                });
                        }
                    });

                    var parentEl = angular.element(document.body);
                    $mdDialog.show({
                        parent: parentEl,
                        templateUrl: 'view/app/module/actionButtonsParameterModal.html',
                        clickOutsideToClose: true,
                        //targetEvent: ev,
                        scope: $scope,
                        preserveScope: true
                    });

                } else {
                    customScripting.run($scope, action.template);

                    var btnMsg = $rootScope.getLanguageValue(action.languages, 'message')
                    if (action.message_type === 'popup' && btnMsg) {
                        mdToast.success(btnMsg);
                    }
                }
            };

            $scope.runScriptParameters = function (form) {
                if (!form.validate())
                    return;

                $scope.actionButtonsData.data.parameters = $scope.buttonsParametersData;

                customScripting.run($scope, $scope.actionButtonsData.button.template);

                var btnMsg = $rootScope.getLanguageValue($scope.actionButtonsData.button.languages, 'message')
                if ($scope.actionButtonsData.button.message_type === 'popup' && btnMsg) {
                    mdToast.success(btnMsg);
                }

                $scope.closeLightBox();
            };

            $scope.shortChange = function (fieldName, reverse) {
                $scope.reportSummary.fieldName = fieldName;
                $scope.reportSummary.reverse = reverse;

                $scope.reportSummary.data = $filter('orderBy')($scope.reportSummary.data, $scope.reportSummary.fieldName, $scope.reportSummary.reverse);
            };

            $scope.downloadImg = function (url) {
                if (url) {
                    var splitArray = url.split('/');
                    var fileName = splitArray[splitArray.length - 1];
                    $http({
                        method: 'GET',
                        url: url,
                        responseType: 'arraybuffer'
                    }).then(function (response) {

                        if (response.data) {
                            var array = new Uint8Array(response.data);
                            var blob = new Blob([array], {
                                type: 'application/octet-stream'
                            });

                            saveAs(blob, fileName);
                        }
                    }, function (response) {
                        void 0;
                    });
                }
            };

            $scope.revertFilter = function () {
                var currentView = $filter('filter')($scope.shadowViews, { id: $rootScope.activeView.id }, true)[0];
                $scope.setView(Object.assign({}, currentView));
            };

            $scope.setViewCalendar = function (filter) {

                if (!$rootScope.activeView.settings || !$rootScope.activeView.settings.startdatefield)
                    return false;

                $scope.findRequest = {
                    "module": $scope.module.name,
                    //"group_by": "string",
                    //"logic_type": "and, or",
                    //"two_way": "bool",
                    // "many_to_many": "string",
                    //"sort_direction": "asc, desc",
                    //"sort_field": "string",
                    "take": 5000,
                    "filter_logic": $rootScope.activeView.filter_logic,
                    "filters": $rootScope.activeView.filters,
                    "convert": true,
                    "fields": [$scope.primaryField, $rootScope.activeView.settings.startdatefield]
                };
                $scope.enddateisAvailable = false;
                if (!$rootScope.activeView.settings.enddatefield || $rootScope.activeView.settings.enddatefield === "end") {
                    $rootScope.activeView.settings.enddatefield = "end";

                } else {
                    $scope.findRequest.fields.push($rootScope.activeView.settings.enddatefield);
                    $scope.enddateisAvailable = true;
                }

                // if($scope.calendarOptions)
                // {
                //     $scope.refreshViewCalendar();
                //     return false;
                // }

                $scope.calenderDefaultDate = new Date();
                $scope.calendarOptions = {
                    add: function (e) {
                        e.preventDefault();
                    },
                    edit: function (e) {
                        window.location = "#/app/record/" + $scope.module.name + "?id=" + e.event.id;
                        e.preventDefault();
                    },
                    resize: function (e) {
                        if ($rootScope.activeView.settings.enddatefield === "end") {
                            e.preventDefault();
                        }
                        return false;
                    },
                    resizeEnd: function (e) {
                        if (!$scope.enddateisAvailable)
                            e.preventDefault();
                        return false;
                    },
                    navigate: function (e) {
                        if (e.date.getMonth() === $scope.calenderDefaultDate.getMonth() && $scope.calenderDefaultDate.getFullYear() === e.date.getFullYear()) {
                            return false;
                        }
                    },
                    delete: function (e) {
                        e.preventDefault();
                        $scope.recordDelete(e, 5);
                    },
                    editable: {
                        destroy: false
                    },
                    views: [
                        "day",
                        { type: "month", selected: true },
                        "week",
                        "agenda",
                        "timeline"
                    ],
                    allDaySlot: false,
                    dataSource: {
                        transport: {
                            read: function (options) {
                                $.ajax({
                                    url: '/api/record/find_custom?locale=' + locale,
                                    contentType: 'application/json',
                                    dataType: 'json',
                                    type: 'POST',
                                    data: JSON.stringify(Object.assign($scope.findRequest, options.data)),
                                    success: function (result) {
                                        $scope.loading = false;
                                        if (result.total_count < 1) {
                                            options.success([]);
                                            return false;
                                        }

                                        var data = ModuleService.kendoCaleanderDataProcces(result.data, $rootScope.activeView.settings, $scope.primaryField, $scope.enddateisAvailable);
                                        options.success(data);
                                        return true;

                                    },
                                    beforeSend: $rootScope.beforeSend()
                                })
                            },
                            update: function (options) {
                                var obj = options.data;
                                var record = { id: obj.id };
                                record[$rootScope.activeView.settings.startdatefield] = obj[$rootScope.activeView.settings.startdatefield];

                                if ($scope.enddateisAvailable)
                                    record[$rootScope.activeView.settings.enddatefield] = obj[$rootScope.activeView.settings.enddatefield];

                                ModuleService.updateRecord($scope.module.name, record).then(function () {
                                    options.success(options.data);
                                });

                            }
                        },
                        schema: {
                            model: {
                                id: "id",
                                fields: {
                                    id: { type: "number" },
                                    start: { type: "date", from: $rootScope.activeView.settings.startdatefield },
                                    end: { type: "date", from: $rootScope.activeView.settings.enddatefield },
                                    isAllDay: { type: "boolean" }
                                }
                            }
                        }
                    },

                };

            }

            $scope.refreshViewCalendar = function () {
                $scope.findRequest = {
                    "module": $scope.module.name,
                    "take": 5000,
                    "filter_logic": $rootScope.activeView.filter_logic,
                    "filters": $rootScope.activeView.filters,
                    "convert": true,
                    "fields": [$scope.primaryField, $rootScope.activeView.settings.startdatefield]
                };

                var scheduler = $("#viewCalendar").data("kendoScheduler").dataSource.read();
                void 0;
            };

            //Klavyeden kısa yollar için tanımlandı
            // document.onkeyup = function (e) {
            //
            //     if (e.shiftKey && e.which === 78) {
            //         $scope.goUrl('#/app/record/' + $scope.module.name)
            //     }
            //
            //     if (e.shiftKey && e.which === 70) {
            //         $scope.filterModalOpen();
            //     }
            //
            //     if (e.shiftKey && e.which === 69) {
            //         $scope.showExportDataModal();
            //     }
            //
            // };

            $scope.languageOptions = ModuleService.getLanguageOptions();

            $scope.getPicklistValue = function (fieldName, value) {
                if (fieldName && value) {
                    var field = $filter('filter')($scope.module.fields, { name: fieldName }, true)[0];
                    if (field) {
                        var picklists = $scope.modulePicklists[field.picklist_id];
                        for (var v = 0; v < picklists.length; v++) {
                            var result = ModuleService.getPicklistValue(picklists[v], value);
                            if (result && result !== true)
                                return result;
                        }
                    }
                }
            };


            function changeViewState() {
                if (!$scope.viewChangeStatus) {
                    $scope.viewChangeStatus = true
                }
            }

            function checkFiltersExistLookup() {
                if (Array.isArray($rootScope.activeView.filters)) {
                    for (var k = 0; k < $rootScope.activeView.filters.length; k++) {
                        const filter = $rootScope.activeView.filters[k];
                        if (filter) {
                            var field = $filter('filter')($scope.viewFields.selectedFields, { name: filter.field }, true)[0];
                            if (!field) {
                                field = $filter('filter')($scope.viewFields.availableFields, { name: filter.field }, true)[0];
                                if (field && field.data_type === 'lookup') {
                                    $scope.findRequest.fields.push(field.name);
                                }
                            }
                        }
                    }
                }
            }
        }
    ]);
