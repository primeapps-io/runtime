'use strict';

angular.module('primeapps')
    .controller('quoteConvertMapController', ['$rootScope', '$scope', '$filter', '$cache', 'helper', 'ConvertMapService', 'ngToast',
        function ($rootScope, $scope, $filter, $cache, helper, ConvertMapService, ngToast) {
            $scope.$parent.collapsed = true;
            $scope.language = $rootScope.language;
            $scope.quoteModule = $filter('filter')($rootScope.modules, { name: 'quotes' }, true)[0];
            $scope.salesOrderModule = $filter('filter')($rootScope.modules, { name: 'sales_orders' }, true)[0];

            var getMappings = function () {
                var moduleId = $scope.quoteModule.id;

                ConvertMapService.getMappings(moduleId)
                    .then(function (result) {
                        $scope.salesOrderModule.selectedFields = {};

                        angular.forEach(result.data, function (mappedModule) {
                            $scope.salesOrderModule.selectedFields[mappedModule.field_id] = $filter('filter')($scope.salesOrderModule.fields, { id: mappedModule.mapping_field_id }, $scope.salesOrderModule, { id: mappedModule.mapping_module_id }, true)[0];

                        });

                    })
                    .catch(function (error) {
                        ngToast.create({ content: $filter('translate')('Common.Error'), className: 'danger' });
                    });
            };

            getMappings();

            $scope.customFilter = function (leadField) {
                return function (field) {
                    if (field.data_type === 'lookup' && leadField.data_type === 'lookup') {
                        if (field.lookup_type === leadField.lookup_type) {
                            return true;
                        }
                        return false;
                    }
                    else if (field.data_type === 'picklist' && leadField.data_type === 'picklist') {
                        if (field.picklist_id === leadField.picklist_id) {
                            return true;
                        }
                        return false;
                    }
                    else {
                        return true;
                    }
                };
            };

            $scope.mappingModuleFieldChanged = function (module, leadField, lastSelection) {
                var conversionMapping = {};
                conversionMapping["module_id"] = $scope.quoteModule.id;
                conversionMapping["mapping_module_id"] = module.id;
                conversionMapping["field_id"] = leadField.id;
                conversionMapping["mapping_field_id"] = null;

                if (module.selectedFields[leadField.id]) {

                    var selectedMapping = module.selectedFields[leadField.id];
                    conversionMapping["mapping_field_id"] = selectedMapping.id;

                    ConvertMapService.createMapping(conversionMapping)
                        .then(function () {
                            $scope.showSuccessIcon = {};
                            $scope.showSuccessIcon[leadField.id] = {};
                            $scope.showSuccessIcon[leadField.id][module.id] = true;
                        })
                        .catch(function (error) {
                            ngToast.create({ content: $filter('translate')('Common.Error'), className: 'danger' });
                        });
                }
                else {
                    conversionMapping["mapping_field_id"] = lastSelection.id;

                    ConvertMapService.deleteMapping(conversionMapping)
                        .then(function () {
                            $scope.showSuccessIcon = {};
                            $scope.showSuccessIcon[leadField.id] = true;
                        })
                        .catch(function (error) {
                            ngToast.create({ content: $filter('translate')('Common.Error'), className: 'danger' });
                        });
                    //delete service here
                }
            };
        }
    ]);