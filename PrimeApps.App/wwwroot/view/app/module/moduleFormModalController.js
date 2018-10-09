'use strict';

angular.module('primeapps')

    .controller('ModuleFormModalController', ['$rootScope', '$scope', 'ngToast', '$filter', 'helper', '$location', '$state', '$stateParams', '$q', '$window', '$localStorage', '$cache', 'operations', 'activityTypes', 'ModuleService',
        function ($rootScope, $scope, ngToast, $filter, helper, $location, $state, $stateParams, $q, $window, $localStorage, $cache, operations, activityTypes, ModuleService) {
            $scope.operations = operations;
            $scope.hasPermission = helper.hasPermission;
            $scope.hasFieldFullPermission = ModuleService.hasFieldFullPermission;
            $scope.loadingModal = true;
            var lookupType = $scope.$parent.currentLookupField.lookup_type;

            if (lookupType === 'relation')
                lookupType = $scope.$parent.record.related_module.value;

            if (lookupType == null)
                return;

            $scope.moduleModal = $filter('filter')($rootScope.modules, { name: lookupType }, true)[0];

            if (!$scope.moduleModal) {
                $scope.formModal.hide();
                ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
                $state.go('app.dashboard');
                return;
            }

            $scope.dropdownFields = $filter('filter')($scope.moduleModal.fields, { data_type: 'lookup', show_as_dropdown: true }, true);
            $scope.dropdownFieldDatas = {};
            for(var i = 0; i < $scope.dropdownFields.length; i++) {
                $scope.dropdownFieldDatas[$scope.dropdownFields[i].name] = [];
            }

            $scope.setDropdownData = function(field){
                if (field.filters && field.filters.length > 0)
                    $scope.dropdownFieldDatas[field.name] = null;
                else if ($scope.dropdownFieldDatas[field.name] && $scope.dropdownFieldDatas[field.name].length > 0)
                    return;

                $scope.currentLookupFieldModal = field;
                $scope.lookupModal()
                    .then(function(response){
                        $scope.dropdownFieldDatas[field.name] = response;
                    });

            };

            if (!$scope.hasPermission(lookupType, $scope.operations.modify)) {
                $scope.forbidden = true;
                $scope.loadingModal = false;
                return;
            }

            $scope.primaryFieldModal = $filter('filter')($scope.moduleModal.fields, { primary_lookup: true })[0];

            if (!$scope.primaryFieldModal)
                $scope.primaryFieldModal = $filter('filter')($scope.moduleModal.fields, { primary: true })[0];

            $scope.currentUser = ModuleService.processUser($rootScope.user);
            $scope.currentDayMin = helper.getCurrentDateMin().toISOString();
            $scope.currentDayMax = helper.getCurrentDateMax().toISOString();
            $scope.currentHour = helper.floorMinutes(new Date());
            $scope.recordModal = {};
            $scope.recordModal.owner = $scope.currentUser;

            if ($scope.$parent.primaryValueModal) {
                if ($scope.primaryFieldModal.combination) {
                    var primaryValueParts = $scope.$parent.primaryValueModal.split(' ');

                    if (primaryValueParts.length === 1) {
                        $scope.recordModal[$scope.primaryFieldModal.combination.field_1] = primaryValueParts[0];
                    }
                    else if (primaryValueParts.length === 2) {
                        $scope.recordModal[$scope.primaryFieldModal.combination.field_1] = primaryValueParts[0];
                        $scope.recordModal[$scope.primaryFieldModal.combination.field_2] = primaryValueParts[1];
                    }
                    else {
                        $scope.recordModal[$scope.primaryFieldModal.combination.field_1] = '';

                        for (var i = 0; i < primaryValueParts.length; i++) {
                            if (i < primaryValueParts.length - 1)
                                $scope.recordModal[$scope.primaryFieldModal.combination.field_1] = $scope.recordModal[$scope.primaryFieldModal.combination.field_1] + primaryValueParts[i] + ' ';
                        }

                        $scope.recordModal[$scope.primaryFieldModal.combination.field_1] = $scope.recordModal[$scope.primaryFieldModal.combination.field_1].slice(0, -1);
                        $scope.recordModal[$scope.primaryFieldModal.combination.field_2] = primaryValueParts[primaryValueParts.length - 1];
                    }
                }
                else {
                    $scope.recordModal[$scope.primaryFieldModal.name] = $scope.$parent.primaryValueModal;
                }
            }

            if ($scope.$parent.calendarDate) {
                var startDateField = $filter('filter')($scope.moduleModal.fields, { calendar_date_type: 'start_date' }, true)[0];
                var endDateField = $filter('filter')($scope.moduleModal.fields, { calendar_date_type: 'end_date' }, true)[0];

                if (!startDateField || !endDateField) {
                    if ($scope.moduleModal.name != 'activities')
                        return;

                    startDateField = $filter('filter')($scope.moduleModal.fields, { name: 'event_start_date' }, true)[0];
                    endDateField = $filter('filter')($scope.moduleModal.fields, { name: 'event_end_date' }, true)[0];
                    $scope.recordModal['activity_type'] = $filter('filter')(activityTypes, { system_code: 'event' }, true)[0];
                }

                var startDate = angular.copy($scope.$parent.calendarDate);
                var endDate = angular.copy($scope.$parent.calendarDate);

                $scope.recordModal[startDateField.name] = startDate.hour(8).toDate();
                $scope.recordModal[endDateField.name] = endDate.hour(9).toDate();
            }

            ModuleService.getPicklists($scope.moduleModal)
                .then(function (picklists) {
					$scope.picklistsModuleModal = angular.copy(picklists);
					ModuleService.setDefaultValues($scope.moduleModal, $scope.recordModal, picklists);
					/**
					 * Calender üzerinde etkinlik oluşturulacağında Aktivite Tipi picklistinde sadece "Etkinlik seçeneği olmalıdır"
					 */
					if ($scope.moduleModal['name'] === 'activities') {
						var field = $filter('filter')($scope.moduleModal.fields, { name: 'activity_type' }, true)[0];
						if ($scope.picklistsModuleModal[field.picklist_id] && field) {
							angular.forEach($scope.picklistsModuleModal[field.picklist_id], function (item) {
								if (item.value != 'event' && item.system_code != 'event') {
									item.hidden = true;
								}
							});
						}
					}

                    $scope.loadingModal = false;
                });

            $scope.lookupModal = function (searchTerm) {
                if ($scope.currentLookupFieldModal.lookup_type === 'users' && !$scope.currentLookupFieldModal.lookupModulePrimaryField) {
                    var userModulePrimaryField = {};
                    userModulePrimaryField.data_type = 'text_single';
                    userModulePrimaryField.name = 'full_name';
                    $scope.currentLookupFieldModal.lookupModulePrimaryField = userModulePrimaryField;
                }

                if ($scope.currentLookupField.lookup_type === 'relation' || $scope.currentLookupField.lookup_type === 'activities') {
                    if (!$scope.recordModal.related_module) {
                        $scope.$broadcast('angucomplete-alt:clearInput', $scope.currentLookupField.name);
                        return $q.defer().promise;
                    }

                    var relationModule = $filter('filter')($rootScope.modules, { name: $scope.recordModal.related_module.value }, true)[0];

                    if (!relationModule) {
                        $scope.$broadcast('angucomplete-alt:clearInput', $scope.currentLookupField.name);
                        return $q.defer().promise;
                    }

                    $scope.currentLookupFieldModal.lookupModulePrimaryField = $filter('filter')(relationModule.fields, { primary: true }, true)[0];
                }

                if (($scope.currentLookupFieldModal.lookupModulePrimaryField.data_type === 'number' || $scope.currentLookupFieldModal.lookupModulePrimaryField.data_type === 'number_auto') && isNaN(parseFloat(searchTerm))) {
                    $scope.$broadcast('angucomplete-alt:clearInput', $scope.currentLookupFieldModal.name);
                    return $q.defer().promise;
                }

                return ModuleService.lookup(searchTerm, $scope.currentLookupFieldModal, $scope.recordModal);
            };


            $scope.multiselectModal = function (searchTerm, field) {
                var picklistItems = [];

                angular.forEach($scope.picklistsModule[field.picklist_id], function (picklistItem) {
                    if (picklistItem.inactive)
                        return;

                    if (picklistItem.labelStr.toLowerCase().indexOf(searchTerm.toLowerCase()) > -1 || picklistItem.labelStr.toUpperCase().indexOf(searchTerm.toUpperCase()) > -1
                        || picklistItem.labelStr.toLowerCaseTurkish().indexOf(searchTerm.toLowerCaseTurkish()) > -1 || picklistItem.labelStr.toUpperCaseTurkish().indexOf(searchTerm.toUpperCaseTurkish()) > -1)
                        picklistItems.push(picklistItem);
                });

                return picklistItems;
            };

            $scope.setCurrentLookupFieldModal = function (field) {
                $scope.currentLookupFieldModal = field;
            };

            $scope.submitModal = function (recordModal) {
                function validate() {
                    var isValid = true;

                    angular.forEach($scope.moduleModal.fields, function (field) {
                        if (!recordModal[field.name])
                            return;

                        if (field.data_type === 'lookup' && typeof recordModal[field.name] != 'object') {
                            $scope.moduleModalForm[field.name].$setValidity('object', false);
                            isValid = false;
                        }
                    });

                    return isValid;
                }

                if (!$scope.moduleModalForm.$valid || !validate())
                    return;

                $scope.submittingModal = true;
                recordModal = ModuleService.prepareRecord(recordModal, $scope.moduleModal);

                ModuleService.insertRecord($scope.moduleModal.name, recordModal)
                    .then(function (response) {
                        if ($scope.$parent.isNewsfeed)
                            ngToast.create({ content: 'Etkinlik Eklendi', className: 'success' });

                        var cacheKey = $scope.moduleModal.name + '_' + $scope.moduleModal.name;
                        $cache.remove(cacheKey);

                        if ($rootScope.activePages && $rootScope.activePages[$scope.moduleModal.name])
                            $rootScope.activePages[$scope.moduleModal.name] = null;


                        var lookupValue = {};
                        lookupValue.id = response.data.id;
                        lookupValue.primary_value = $scope.$parent.primaryValueModal;

                        if ($scope.$parent.record)
                            $scope.$parent.record[$scope.$parent.currentLookupField.name] = lookupValue;

                        if ($scope.$parent.formModalSuccess)
                            $scope.$parent.formModalSuccess();
                        if ($scope.$parent.currentLookupField.special_type == "quate_products") {
                            ModuleService.getPicklists($scope.moduleModal).then(function (picklists) {
                                var record = response.data;
                                var quoteProductRecord = ModuleService.processRecordSingle(record, $scope.moduleModal, picklists);
                                var quoteProduct = $scope.$parent.currentLookupField.currentproduct;
                                quoteProduct.product = quoteProductRecord;
                                quoteProduct.product.primary_value = quoteProductRecord.name;
                                $scope.$parent.currentLookupField.currentproduct = quoteProduct;
                                $scope.$parent.currentLookupField.selectProduct($scope.$parent.currentLookupField.currentproduct);
                                delete $scope.$parent.record.product;

                            });
                        }
                        if ($scope.$parent.currentLookupField.special_type == "order_products") {
                            ModuleService.getPicklists($scope.moduleModal).then(function (picklists) {
                                var record = response.data;
                                var orderProductRecord = ModuleService.processRecordSingle(record, $scope.moduleModal, picklists);
                                var orderProduct = $scope.$parent.currentLookupField.currentproduct;
                                orderProduct.product = orderProductRecord;
                                orderProduct.product.primary_value = orderProductRecord.name;
                                $scope.$parent.currentLookupField.currentproduct = orderProduct;
                                $scope.$parent.currentLookupField.selectProduct($scope.$parent.currentLookupField.currentproduct);
                                delete $scope.$parent.record.product;

                            });
                        }
                        if ($scope.$parent.currentLookupField.special_type == "purchase_products") {
                            ModuleService.getPicklists($scope.moduleModal).then(function (picklists) {
                                var record = response.data;
                                var purchaseProductRecord = ModuleService.processRecordSingle(record, $scope.moduleModal, picklists);
                                var purchaseProduct = $scope.$parent.currentLookupField.currentproduct;
                                purchaseProduct.product = purchaseProductRecord;
                                purchaseProduct.product.primary_value = purchaseProductRecord.name;
                                $scope.$parent.currentLookupField.currentproduct = purchaseProduct;
                                $scope.$parent.currentLookupField.selectProduct($scope.$parent.currentLookupField.currentproduct);
                                delete $scope.$parent.record.product;

                            });
                        }

                        $scope.submittingModal = false;
                        $scope.formModal.hide();
                    })
                    .catch(function (data) {
                        if (data.status === 409) {
                            $scope.moduleModalForm[data.data.field].$setValidity('unique', false);
                        }
                    })
                    .finally(function () {
                        $scope.submittingModal = false;
                    });
            };

            $scope.calculate = function (field) {
                ModuleService.calculate(field, $scope.moduleModal, $scope.recordModal);
            };

            $scope.fieldValueChange = function (field) {
                ModuleService.setDependency(field, $scope.moduleModal, $scope.recordModal, $scope.picklistsModuleModal);
                ModuleService.setDisplayDependency($scope.moduleModal, $scope.recordModal);

                if ($scope.moduleModalForm[field.name].$error.unique)
                    $scope.moduleModalForm[field.name].$setValidity('unique', true);
            };

            $scope.isModalField = function (field) {

                if ($scope.moduleModal.name != 'products') {
                    if (field.validation.required && field.display_form && !field.deleted) {
                        return true;
                    }
                    return false;
                }
                if (field.display_form && !field.deleted) {
                    return true;
                }
                return false;

            }
        }
    ]);