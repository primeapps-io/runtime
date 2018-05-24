'use strict';

angular.module('ofisim')
    .factory('ReportsService', ['$rootScope', '$http', '$filter', 'config', '$q', 'ModuleService',
        function ($rootScope, $http, $filter, config, $q, ModuleService) {

            var that = {
                getAllReports: function () {
                    return $http.get(config.apiUrl + 'report/get_all');
                },
                getAllCategory: function () {
                    return $http.get(config.apiUrl + 'report/get_categories');
                },
                getReportData: function (moduleId, displayFileds) {
                    var data = {
                        displayFileds: [],
                        module: {}
                    };


                    $rootScope.modules.forEach(function (module) {
                        if (moduleId === module.id) {
                            data.module = module;
                            module.fields.forEach(function (modulefield) {

                                displayFileds.forEach(function (field) {
                                    if (field.field == modulefield.name) {
                                        var modulefieldCopy = modulefield;
                                        modulefieldCopy['colorder'] = field.order;
                                        data.displayFileds.push(modulefieldCopy);
                                    }

                                    if (modulefield.data_type === "lookup") {
                                        var parseField = field.field.split(".")

                                        if (parseField.length > 1) {
                                            if (modulefield.name == parseField[0]) {
                                                var modulefieldCopy = modulefield;
                                                modulefieldCopy['colorder'] = field.order;
                                                data.displayFileds.push(modulefieldCopy);
                                            }
                                        }
                                    }
                                });
                            });

                        }
                    });
                    return data;
                },
                getAggregationsFields: function (aggregations, moduleName, displayFileds, filters) {
                    aggregations.forEach(function (field) {
                        var req = {
                            'fields': [field.aggregation_type + '(' + field.field + ')'],
                            'limit': 1,
                            'offset': 0,
                            'filters': filters
                        };

                        ModuleService.findRecords(moduleName, req).then(function (response) {
                            var res = {
                                name: field.field,
                                type: field.aggregation_type,
                                value: response.data[0][field.aggregation_type],
                                label: $filter('translate')('Report.' + field.aggregation_type)
                            };

                            displayFileds.forEach(function (displayFiled) {
                                if (res.name == displayFiled.name) {
                                    that.formatFieldValue(displayFiled, res.value);
                                    displayFiled['aggregation'] = res;
                                }

                            });
                        });
                    });

                    return displayFileds;
                },
                formatFieldValue: function (field, value) {
                    field.valueFormatted = '';

                    if (value === undefined || value === null)
                        return;

                    switch (field.data_type) {
                        case 'number_decimal':
                            field.valueFormatted = $filter('number')(value, field.decimal_places || 2);
                            break;
                        case 'number_auto':
                            var valueFormatted = value.toString();

                            if (field.auto_number_prefix)
                                valueFormatted = field.auto_number_prefix + valueFormatted;

                            if (field.auto_number_suffix)
                                valueFormatted += field.auto_number_suffix;

                            field.valueFormatted = valueFormatted;
                            break;
                        case 'currency':
                            field.valueFormatted = $filter('currency')(value, field.currency_symbol || $rootScope.currencySymbol, field.decimal_places || 2);
                            break;
                        default:
                            field.valueFormatted = value;
                            break;
                    }
                },
                getChart: function (reportId) {
                    return $http.get(config.apiUrl + 'report/get_chart/' + reportId);
                },
                getWidget: function (reportId) {
                    return $http.get(config.apiUrl + 'report/get_widget/' + reportId);
                },
                getFilters: function (filter) {
                    var data = [];
                    filter.forEach(function (item) {
                        data.push({
                            field: item.field,
                            id: item.id,
                            operator: item.operator,
                            value: item.value
                        });
                    });
                    return data;
                },
                createCategory: function (data) {
                    return $http.post(config.apiUrl + 'report/create_category', data);
                },
                updateCategory: function (data) {
                    return $http.put(config.apiUrl + 'report/update_category/' + data.id, data);
                },
                categoryDelete: function (id) {
                    return $http.delete(config.apiUrl + '/report/delete_category/' + id);
                },
                createReport: function (data) {
                    return $http.post(config.apiUrl + "report/create", data);
                },
                deleteReport:function (id) {
                    return $http.delete(config.apiUrl + '/report/delete/' + id);
                },
                getReport: function (id) {
                    return $http.get(config.apiUrl + 'report/get_report/' + id);
                },
                updateReport: function (data) {
                    return $http.put(config.apiUrl + 'report/update/' + data.id, data);
                }
            };
            return that;
        }]);

