'use strict';

angular.module('primeapps')
    .controller('LeadConvertMapController', ['$rootScope', '$scope', '$filter', '$cache', 'helper', 'ConvertMapService', 'ngToast',
        function ($rootScope, $scope, $filter, $cache, helper, ConvertMapService, ngToast) {
            $scope.$parent.collapsed = true;
            $scope.language = $rootScope.language;
            $scope.leadModule = $filter('filter')($rootScope.modules, { name: 'leads' }, true)[0];
            $scope.contactModule = $filter('filter')($rootScope.modules, { name: 'contacts' }, true)[0];
            $scope.opportunityModule = $filter('filter')($rootScope.modules, { name: 'opportunities' }, true)[0];
            $scope.accountModule = $filter('filter')($rootScope.modules, { name: 'accounts' }, true)[0];

            var getMappings = function () {
                var moduleId = $scope.leadModule.id;

                ConvertMapService.getMappings(moduleId)
                    .then(function (result) {
                        $scope.accountModule.selectedFields = {};
                        $scope.contactModule.selectedFields = {};
                        $scope.opportunityModule.selectedFields = {};

                        angular.forEach(result.data, function (mappedModule) {

                            var selectedAccountField = $filter('filter')($scope.accountModule.fields, { id: mappedModule.mapping_field_id }, true, $scope.accountModule, { id: mappedModule.mapping_module_id }, true)[0];
                            var selectedContactField = $filter('filter')($scope.contactModule.fields, { id: mappedModule.mapping_field_id }, true, $scope.contactModule, { id: mappedModule.mapping_module_id }, true)[0];
                            var selectedOpportunityField = $filter('filter')($scope.opportunityModule.fields, { id: mappedModule.mapping_field_id }, true, $scope.opportunityModule, { id: mappedModule.mapping_module_id }, true)[0];

                            if (selectedAccountField) {
                                $scope.accountModule.selectedFields[mappedModule.field_id] = selectedAccountField;
                            }
                            else if (selectedContactField) {
                                $scope.contactModule.selectedFields[mappedModule.field_id] = selectedContactField;
                            }
                            else if (selectedOpportunityField) {
                                $scope.opportunityModule.selectedFields[mappedModule.field_id] = selectedOpportunityField;
                            }
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
                conversionMapping["module_id"] = $scope.leadModule.id;
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