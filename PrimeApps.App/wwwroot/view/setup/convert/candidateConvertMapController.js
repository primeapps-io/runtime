'use strict';

angular.module('primeapps')
    .controller('CandidateConvertMapController', ['$rootScope', '$scope', '$filter', '$cache', 'helper', 'ConvertMapService',
        function ($rootScope, $scope, $filter, $cache, helper, ConvertMapService) {
            $scope.$parent.collapsed = true;
            $scope.language = $rootScope.language;
            $scope.candidateModule = $filter('filter')($rootScope.modules, { name: 'adaylar' }, true)[0];
            $scope.employeeModule = $filter('filter')($rootScope.modules, { name: 'calisanlar' }, true)[0];

            var getMappings = function () {
                var moduleId = $scope.candidateModule.id;

                ConvertMapService.getMappings(moduleId)
                    .then(function (result) {
                        $scope.employeeModule.selectedFields = {};

                        angular.forEach(result.data, function (mappedModule) {
                            $scope.employeeModule.selectedFields[mappedModule.field_id] = $filter('filter')($scope.employeeModule.fields, { id: mappedModule.mapping_field_id }, $scope.employeeModule, { id: mappedModule.mapping_module_id }, true)[0];

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
                conversionMapping["module_id"] = $scope.candidateModule.id;
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