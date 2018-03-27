'use strict';

angular.module('ofisim')

    .controller('ModuleFormController', ['$rootScope', '$scope', 'ngToast', '$filter', 'helper', '$location', '$state', '$stateParams', '$q', '$window', '$localStorage', '$cache', 'config', '$timeout', 'operations', '$modal', 'FileUploader', 'activityTypes', 'transactionTypes', 'ModuleService', 'DocumentService', '$http', 'resizeService', 'components',
        function ($rootScope, $scope, ngToast, $filter, helper, $location, $state, $stateParams, $q, $window, $localStorage, $cache, config, $timeout, operations, $modal, FileUploader, activityTypes, transactionTypes, ModuleService, DocumentService, $http, resizeService, components) {
            $scope.type = $stateParams.type;
            $scope.subtype = $stateParams.stype;
            $scope.id = $location.search().id;
            $scope.parentType = $location.search().ptype;
            $scope.parentId = $location.search().pid;
            $scope.returnTab = $location.search().rtab;
            $scope.previousParentType = $location.search().pptype;
            $scope.previousParentId = $location.search().ppid;
            $scope.previousReturnTab = $location.search().prtab;
            $scope.back = $location.search().back;
            $scope.many = $location.search().many;
            $scope.clone = $location.search().clone;
            $scope.revise = $location.search().revise;
            $scope.paramField = $location.search().field;
            $scope.paramValue = $location.search().value;
            $scope.operations = operations;
            $scope.hasPermission = helper.hasPermission;
            $scope.hasDocumentsPermission = helper.hasDocumentsPermission;
            $scope.hasRecordEditPermission = helper.hasRecordEditPermission;
            $scope.hasAdminRights = helper.hasAdminRights;
            $scope.hasFieldFullPermission = ModuleService.hasFieldFullPermission;
            $scope.hasSectionFullPermission = ModuleService.hasSectionFullPermission;
            $scope.hasActionButtonDisplayPermission = ModuleService.hasActionButtonDisplayPermission;
            $scope.lookupUserAndGroup = helper.lookupUserAndGroup;
            $scope.loading = true;
            $scope.image = {};

            if (!$scope.hasPermission($scope.type, $scope.operations.read)) {
                ngToast.create({ content: $filter('translate')('Common.Forbidden'), className: 'warning' });
                $state.go('app.crm.dashboard');
                return;
            }

            if ($scope.parentId)
                $window.scrollTo(0, 0);

            $scope.module = $filter('filter')($rootScope.modules, { name: $scope.type }, true)[0];
            if (!$scope.module) {
                ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
                $state.go('app.crm.dashboard');
                return;
            }

            $scope.dropdownFields = $filter('filter')($scope.module.fields, { data_type: 'lookup', show_as_dropdown: true }, true);
            $scope.dropdownFieldDatas = {};
            for (var i = 0; i < $scope.dropdownFields.length; i++) {
                $scope.dropdownFieldDatas[$scope.dropdownFields[i].name] = [];
            }

            if ((!$scope.id && !$scope.hasPermission($scope.type, $scope.operations.write)) || ($scope.id && !$scope.hasPermission($scope.type, $scope.operations.modify))) {
                ngToast.create({ content: $filter('translate')('Common.Forbidden'), className: 'warning' });
                $state.go('app.crm.dashboard');
                return;
            }

            $scope.primaryField = $filter('filter')($scope.module.fields, { primary: true })[0];
            $scope.currentUser = ModuleService.processUser($rootScope.user);
            $scope.currentDayMin = helper.getCurrentDateMin().toISOString();
            $scope.currentDayMax = helper.getCurrentDateMax().toISOString();
            $scope.currentHour = helper.floorMinutes(new Date());
            $scope.relatedToField = $filter('filter')($scope.module.fields, { name: 'related_to' }, true)[0];
            $scope.record = {};
            $scope.customLeaveFields = {};

            if (!$scope.id) {
                $scope.title = $filter('translate')('Module.New', { title: $scope.module['label_' + $rootScope.language + '_singular'] });
            }

            if ($scope.parentType) {
                if ($scope.type === 'activities' || $scope.type === 'mails' || $scope.many) {
                    $scope.parentModule = $scope.parentType;
                }
                else if ($scope.type === 'current_accounts') {
                    if ($scope.id) {
                        if ($scope.parentType === 'supplier')
                            $scope.parentModule = 'suppliers';
                        else if ($scope.parentType === 'customer')
                            $scope.parentModule = 'accounts';
                    } else {
                        $scope.parentModule = $scope.parentType;
                    }
                }
                else {
                    var parentTypeField = $filter('filter')($scope.module.fields, { name: $scope.parentType }, true)[0];

                    if (!parentTypeField) {
                        $scope.parentType = null;
                        $scope.parentId = null;
                    }
                    else {
                        $scope.parentModule = parentTypeField.lookup_type;
                    }
                }
            }

            if ($scope.type === 'quotes') {
                $scope.productModule = $filter('filter')($rootScope.modules, { name: 'products' }, true)[0];
                $scope.quoteProductModule = $filter('filter')($rootScope.modules, { name: 'quote_products' }, true)[0];
                $scope.productCurrencyField = $filter('filter')($scope.productModule.fields, { name: 'currency' }, true)[0];
            }
            else if ($scope.type === 'sales_orders') {
                $scope.productModule = $filter('filter')($rootScope.modules, { name: 'products' }, true)[0];
                $scope.orderProductModule = $filter('filter')($rootScope.modules, { name: 'order_products' }, true)[0];
                $scope.productCurrencyField = $filter('filter')($scope.productModule.fields, { name: 'currency' }, true)[0];
            }
            else if ($scope.type === 'purchase_orders') {
                $scope.productModule = $filter('filter')($rootScope.modules, { name: 'products' }, true)[0];
                $scope.purchaseProductModule = $filter('filter')($rootScope.modules, { name: 'purchase_order_products' }, true)[0];
                $scope.productCurrencyField = $filter('filter')($scope.productModule.fields, { name: 'currency' }, true)[0];
            }

            var isFreeze = function (record) {
                var type = false;

                if (record.process_status === 1)
                    type = true;

                if ($scope.module.dependencies.length > 0) {
                    var freezeDependencies = $filter('filter')($scope.module.dependencies, { dependency_type: 'freeze' }, true);
                    angular.forEach(freezeDependencies, function (dependency) {
                        if (!type) {
                            var freezeFields = $filter('filter')($scope.module.fields, { name: dependency.parent_field }, true);
                            angular.forEach(freezeFields, function (field) {
                                if (!type)
                                    angular.forEach(dependency.values_array, function (value) {
                                        if (record[field.name] && (value == record[field.name] || value == record[field.name].id))
                                            type = true;
                                    });
                            });
                        }
                    });
                }
                return type;
            };

            var setCurrencyCurrentAccounts = function () {
                if ($scope.module.name === 'current_accounts' && $scope.currencyField) {
                    var currencyValue;

                    if ($scope.record['customer'])
                        currencyValue = $scope.record['customer']['currency'];

                    if ($scope.record['supplier'])
                        currencyValue = $scope.record['supplier']['currency'];

                    if (currencyValue) {
                        if (!angular.isObject(currencyValue))
                            $scope.record['currency'] = $filter('filter')($scope.picklistsModule[$scope.currencyField.picklist_id], { labelStr: currencyValue }, true)[0];
                        else
                            $scope.record['currency'] = currencyValue;
                    }
                }
            };

            ModuleService.getPicklists($scope.module)
                .then(function (picklists) {
                    $scope.picklistsModule = picklists;
                    $scope.currencyField = $filter('filter')($scope.module.fields, { name: 'currency' }, true)[0];

                    var setFieldDependencies = function () {
                        angular.forEach($scope.module.fields, function (field) {
                            ModuleService.setDependency(field, $scope.module, $scope.record, $scope.picklistsModule, $scope);
                        });
                    };

                    if ($scope.module && $scope.module.name === 'izinler') {
                        var toField = $filter('filter')($scope.module.fields, { name: 'to_entry_type' }, true)[0];
                        var fromField = $filter('filter')($scope.module.fields, { name: 'from_entry_type' }, true)[0];
                        $scope.customLeaveFields['to_entry_type'] = toField;
                        $scope.customLeaveFields['from_entry_type'] = fromField;
                        if (!$scope.record['to_entry_type'] && !$scope.record['from_entry_type'] && toField) {
                            $scope.record['to_entry_type'] = picklists[toField.picklist_id][0];
                            $scope.record['from_entry_type'] = picklists[toField.picklist_id][0];
                        }
                    }

                    if (!$scope.id) {
                        $scope.loading = false;
                        $scope.record.owner = $scope.currentUser;

                        if ($scope.subtype) {
                            if ($scope.type == 'activities') {
                                $scope.record['activity_type'] = $filter('filter')(activityTypes, { system_code: $scope.subtype }, true)[0];
                                $scope.subtypeNameLang = $filter('translate')('Module.New', { title: $scope.record['activity_type'].label[$rootScope.language] });
                            } else if ($scope.type == 'current_accounts') {
                                $scope.record['transaction_type'] = $filter('filter')(transactionTypes, { system_code: $scope.subtype }, true)[0];
                                $scope.subtypeNameLang = $filter('translate')('Module.New', { title: $scope.record['transaction_type'].label[$rootScope.language] });
                            }
                        }

                        if (($scope.module.name === 'accounts' || $scope.module.name === 'current_accounts' || $scope.module.name === 'products' || $scope.module.name === 'quotes' || $scope.module.name === 'sales_orders' || $scope.module.name === 'purchase_orders') && $scope.currencyField) {
                            $scope.currencyField.validation.readonly = false;
                        }

                        if ($scope.parentId) {
                            var moduleParent = $filter('filter')($rootScope.modules, { name: $scope.parentModule }, true)[0];

                            ModuleService.getRecord($scope.parentModule, $scope.parentId)
                                .then(function onSuccess(parent) {
                                    var moduleParentPrimaryField = $filter('filter')(moduleParent.fields, { primary: true, deleted: false }, true)[0];
                                    var lookupRecord = {};
                                    lookupRecord.id = parent.data.id;
                                    lookupRecord.primary_value = parent.data[moduleParentPrimaryField.name];

                                    if (parent.data['currency']) {
                                        lookupRecord['currency'] = parent.data['currency'];
                                        setCurrencyCurrentAccounts();
                                    }

                                    if ($scope.parentModule === 'calisanlar') {
                                        lookupRecord['e_posta'] = parent.data['e_posta'];
                                    }

                                    if (($scope.type === 'activities' || $scope.type === 'mails') && $scope.relatedToField) {
                                        $scope.record['related_to'] = lookupRecord;
                                        $scope.record['related_module'] = $filter('filter')(picklists['900000'], { value: $scope.parentType }, true)[0];
                                    }
                                    else {
                                        if ($scope.parentModule === 'accounts' && $scope.type === 'current_accounts') {
                                            $scope.record['customer'] = lookupRecord;
                                        }
                                        else if ($scope.parentModule === 'suppliers' && $scope.type === 'current_accounts') {
                                            $scope.record['supplier'] = lookupRecord;
                                        }
                                        else {
                                            $scope.record[$scope.parentType] = lookupRecord;
                                        }

                                        var relatedDependency = $filter('filter')($scope.module.dependencies, { dependent_field: $scope.parentType }, true)[0];

                                        if (relatedDependency && relatedDependency.deleted != true) {
                                            var dependencyField = $filter('filter')($scope.module.fields, { name: relatedDependency.field }, true)[0];
                                            $scope.record[relatedDependency.field] = $filter('filter')($scope.picklistsModule[dependencyField.picklist_id], { id: relatedDependency.values[0] }, true)[0];

                                            var dependentField = $filter('filter')($scope.module.fields, { name: relatedDependency.dependent_field }, true)[0];
                                            dependentField.hidden = false;
                                        }

                                        setFieldDependencies();
                                    }
                                });
                        }
                        else {
                            setFieldDependencies();
                        }

                        ModuleService.setDefaultValues($scope.module, $scope.record, picklists);
                        ModuleService.setDisplayDependency($scope.module, $scope.record);

                        if ($scope.paramField) {
                            $scope.record[$scope.paramField] = $scope.paramValue;
                            $rootScope.hideSipPhone();
                        }

                        //MultiCurrency
                        if ($scope.module.name === 'quotes' || $scope.module.name === 'sales_orders' || $scope.module.name === 'purchase_orders') {
                            if ($scope.currencyField) {
                                $scope.showExchangeRates = true;

                                if ($scope.id)
                                    return;

                                ModuleService.getDailyRates()
                                    .then(function (response) {
                                        if (!response.data)
                                            return;

                                        var dailyRates = response.data;
                                        $scope.exchangeRatesDate = $filter('date')(dailyRates.date, 'dd MMMM yyyy') + ' 15:30';

                                        $scope.record.exchange_rate_try_usd = dailyRates.usd;
                                        $scope.record.exchange_rate_try_eur = dailyRates.eur;
                                        $scope.record.exchange_rate_usd_try = 1 / dailyRates.usd;
                                        $scope.record.exchange_rate_usd_eur = (1 / dailyRates.eur) * dailyRates.usd;
                                        $scope.record.exchange_rate_eur_try = 1 / dailyRates.eur;
                                        $scope.record.exchange_rate_eur_usd = (1 / dailyRates.usd) * dailyRates.eur;
                                    })
                            }
                        }

                        if ($scope.type === 'quotes') {
                            var quoteProduct = {};
                            quoteProduct.id = 0;
                            quoteProduct.order = 1;
                            quoteProduct.discount_type = 'percent';
                            $scope.quoteProducts = [];
                            $scope.quoteProducts.push(quoteProduct);
                        }

                        if ($scope.type === 'sales_orders') {
                            var orderProduct = {};
                            orderProduct.id = 0;
                            orderProduct.order = 1;
                            orderProduct.discount_type = 'percent';
                            $scope.orderProducts = [];
                            $scope.orderProducts.push(orderProduct);
                        }

                        if ($scope.type === 'purchase_orders') {
                            var purchaseProduct = {};
                            purchaseProduct.id = 0;
                            purchaseProduct.order = 1;
                            purchaseProduct.discount_type = 'percent';
                            $scope.purchaseProducts = [];
                            $scope.purchaseProducts.push(purchaseProduct);
                        }

                        return;
                    }

                    ModuleService.getRecord($scope.module.name, $scope.id)
                        .then(function onSuccess(recordData) {
                            var record = ModuleService.processRecordSingle(recordData.data, $scope.module, $scope.picklistsModule);

                            if (!$scope.hasRecordEditPermission(recordData.data) || isFreeze(record)) {
                                ngToast.create({
                                    content: $filter('translate')('Common.Forbidden'),
                                    className: 'warning'
                                });
                                $state.go('app.crm.dashboard');
                                return;
                            }

                            if (($scope.module.name === 'accounts' || $scope.module.name === 'current_accounts' || $scope.module.name === 'products' || $scope.module.name === 'quotes' || $scope.module.name === 'sales_orders' || $scope.module.name === 'purchase_orders') && $scope.currencyField) {
                                if (record['currency'])
                                    $scope.currencyField.validation.readonly = true;
                                else
                                    $scope.currencyField.validation.readonly = false;

                                if ($scope.clone) {
                                    $scope.currencyField.validation.readonly = false;
                                }
                            }

                            $scope.title = $scope.primaryField.valueFormatted;
                            $scope.recordState = angular.copy(record);
                            ModuleService.setDisplayDependency($scope.module, record);

                            setFieldDependencies();

                            if ($scope.relatedToField && record['related_to']) {
                                var lookupModule = $filter('filter')($rootScope.modules, { id: record[$scope.relatedToField.lookup_relation].id - 900000 }, true)[0];
                                var lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];

                                ModuleService.getRecord(lookupModule.name, record['related_to'], true)
                                    .then(function onSuccess(relatedRecord) {
                                        relatedRecord = relatedRecord.data;
                                        relatedRecord.primary_value = relatedRecord[lookupModulePrimaryField.name];
                                        record['related_to'] = relatedRecord;

                                        $scope.record = record;
                                        $scope.recordState = angular.copy($scope.record);
                                        $scope.loading = false;
                                    })
                                    .catch(function onError(response) {
                                        if (response.status === 404) {
                                            record['related_to'] = null;
                                            $scope.record = record;
                                            $scope.recordState = angular.copy($scope.record);
                                        }

                                        $scope.loading = false;
                                    });
                            }
                            else {
                                $scope.record = record;
                                if ($scope.clone) {
                                    $scope.record.owner = $scope.currentUser;
                                }
                                $scope.loading = false;
                            }

                            if ($scope.record.currency)
                                $scope.currencySymbol = $scope.record.currency.value || $rootScope.currencySymbol;

                            getProductRecords($scope.type);
                            ModuleService.customActions($scope.module, $scope.record);
                        })
                        .catch(function onError() {
                            $scope.loading = false;
                        });
                });

            $scope.lookup = function (searchTerm) {
                if ($scope.currentLookupField.lookup_type === 'users' && !$scope.currentLookupField.lookupModulePrimaryField) {
                    var userModulePrimaryField = {};
                    userModulePrimaryField.data_type = 'text_single';
                    userModulePrimaryField.name = 'full_name';
                    $scope.currentLookupField.lookupModulePrimaryField = userModulePrimaryField;
                }

                if ($scope.currentLookupField.lookup_type === 'relation') {
                    if (!$scope.record.related_module) {
                        $scope.$broadcast('angucomplete-alt:clearInput', $scope.currentLookupField.name);
                        return $q.defer().promise;
                    }

                    var relationModule = $filter('filter')($rootScope.modules, { name: $scope.record.related_module.value }, true)[0];

                    if (!relationModule) {
                        $scope.$broadcast('angucomplete-alt:clearInput', $scope.currentLookupField.name);
                        return $q.defer().promise;
                    }

                    $scope.currentLookupField.lookupModulePrimaryField = $filter('filter')(relationModule.fields, { primary: true }, true)[0];
                }

                if (($scope.currentLookupField.lookupModulePrimaryField.data_type === 'number' || $scope.currentLookupField.lookupModulePrimaryField.data_type === 'number_auto') && isNaN(parseFloat(searchTerm))) {
                    $scope.$broadcast('angucomplete-alt:clearInput', $scope.currentLookupField.name);
                    return $q.defer().promise;
                }

                if ($scope.module.name === 'quotes' || $scope.module.name === 'sales_orders' || $scope.module.name === 'purchase_orders') {
                    var discountField = null;

                    if ($scope.currentLookupField.lookup_type === 'contacts') {
                        $scope.contactModule = $filter('filter')($rootScope.modules, { name: 'contacts' }, true)[0];
                        discountField = $filter('filter')($scope.contactModule.fields, { name: 'discount', deleted: '!true' }, true)[0];
                    }
                    else if ($scope.currentLookupField.lookup_type === 'accounts') {
                        $scope.accountModule = $filter('filter')($rootScope.modules, { name: 'accounts' }, true)[0];
                        discountField = $filter('filter')($scope.accountModule.fields, { name: 'discount', deleted: '!true' }, true)[0];
                    }

                    if (discountField)
                        return ModuleService.lookup(searchTerm, $scope.currentLookupField, $scope.record, ['discount']);
                    else
                        return ModuleService.lookup(searchTerm, $scope.currentLookupField, $scope.record);
                }

                if ($scope.module.name === 'current_accounts' && $scope.currencyField) {
                    var parentModuleName = '';

                    if ($scope.currentLookupField.name === 'customer')
                        parentModuleName = 'accounts';
                    else if ($scope.currentLookupField.name === 'supplier')
                        parentModuleName = 'suppliers';

                    var parentModule = $filter('filter')($rootScope.modules, { name: parentModuleName }, true)[0];
                    var parentCurrencyField = $filter('filter')(parentModule.fields, { name: 'currency' }, true)[0];

                    if (parentCurrencyField) {
                        return ModuleService.lookup(searchTerm, $scope.currentLookupField, $scope.record, ['currency']);
                    }
                    else {
                        return ModuleService.lookup(searchTerm, $scope.currentLookupField, $scope.record);
                    }
                }

                if ($scope.currentLookupField.lookup_type === 'users')
                    return ModuleService.lookup(searchTerm, $scope.currentLookupField, $scope.record, ['email']);
                else if ($scope.currentLookupField.lookup_type === 'calisanlar')
                    return ModuleService.lookup(searchTerm, $scope.currentLookupField, $scope.record, ['e_posta']);
                else
                    return ModuleService.lookup(searchTerm, $scope.currentLookupField, $scope.record);

            };

            $scope.multiselect = function (searchTerm, field) {
                var picklistItems = [];

                angular.forEach($scope.picklistsModule[field.picklist_id], function (picklistItem) {
                    if (picklistItem.inactive || picklistItem.hidden)
                        return;

                    if (picklistItem.labelStr.toLowerCase().indexOf(searchTerm.toLowerCase()) > -1 || picklistItem.labelStr.toUpperCase().indexOf(searchTerm.toUpperCase()) > -1
                        || picklistItem.labelStr.toLowerCaseTurkish().indexOf(searchTerm.toLowerCaseTurkish()) > -1 || picklistItem.labelStr.toUpperCaseTurkish().indexOf(searchTerm.toUpperCaseTurkish()) > -1)
                        picklistItems.push(picklistItem);
                });

                return picklistItems;
            };

            $scope.setCurrentLookupField = function (field) {
                $scope.currentLookupField = field;
            };

            $scope.uploader = new FileUploader({
                url: config.apiUrl + 'Document/upload_large',
                headers: {
                    'Authorization': 'Bearer ' + $localStorage.read('access_token'),
                    "Content-Type": "application/json", "Accept": "application/json"
                }
            });

            $scope.entityIdFunc = function () {
                return $scope.recordId;
            };

            $scope.submit = function (record) {
                function validate() {
                    var isValid = true;

                    angular.forEach($scope.module.fields, function (field) {
                        if (!record[field.name])
                            return;

                        if (field.data_type === 'lookup' && typeof record[field.name] != 'object') {
                            $scope.moduleForm[field.name].$setValidity('object', false);
                            isValid = false;
                        }

                        if (field.data_type == 'image' && $scope.image[field.name] && $scope.image[field.name].UniqueName && record[field.name] != null) {
                            record[field.name] = $scope.image[field.name].UniqueName;
                            $http.post(config.apiUrl + 'Document/image_create', {
                                UniqueFileName: $scope.image[field.name].UniqueName,
                                MimeType: $scope.image[field.name].Type,
                                ChunkSize: 1,
                                instanceId: $rootScope.workgroup.instanceID
                            });
                        }

                    });

                    return isValid;
                }

                if (!$scope.moduleForm.$valid || !validate())
                    return;

                if ($scope.module.name === 'izinler') {
                    var val = ModuleService.customValidations($scope.module, record);
                    if (val != "") {
                        ngToast.create({
                            //content: $filter('translate')('Module.SuccessMessage', { title: $scope.module['label_' + $rootScope.language + '_singular'] }),
                            content: $filter('translate')('Leave.NotHaveLeave'),
                            className: 'warning'
                        });
                        return;
                    }
                }

                if (!$scope.id || $scope.clone) {
                    $scope.executeCode = false;
                    components.run('BeforeCreate', 'Script', $scope, record);
                    if ($scope.executeCode) {
                        return;
                    }
                } else {
                    components.run('BeforeUpdate', 'Script', $scope, record);
                }

                if ($scope.clone) {
                    angular.forEach($scope.module.fields, function (field) {
                        if (field.data_type === 'number_auto') {
                            delete record[field.name];
                        }
                    });
                    $scope.recordState = null;
                }

                $scope.submitting = true;

                if (record.discount_percent && record.discount_amount == null)
                    record.discount_amount = parseFloat(record.total - record.discounted_total);

                if (record.discount_amount && record.discount_percent == null)
                    record.discount_percent = parseFloat((100 * (record.total - record.discounted_total)) / record.total);

                record = ModuleService.prepareRecord(record, $scope.module, $scope.recordState);

                var recordCopy = angular.copy($scope.record);

                //gets activity_type_id for saveAndNew
                $scope.activity_type_id = $filter('filter')(activityTypes, { id: record.activity_type }, true)[0];

                if (record.activity_type_system || record.activity_type_system === null)
                    delete record.activity_type_system;

                if (record.transaction_type_system || record.transaction_type_system === null)
                    delete record.transaction_type_system;

                if (!$scope.id) {
                    ModuleService.insertRecord($scope.module.name, record)
                        .then(function onSuccess(response) {
                            var moduleProcesses = $filter('filter')($rootScope.approvalProcesses, { module_id: $scope.module.id }, true);
                            if (moduleProcesses) {
                                var isProcessInsert = false;
                                for (var i = 0; i < moduleProcesses.length; i++) {
                                    if ((moduleProcesses[i].user_id === 0 || moduleProcesses[i].user_id === $scope.currentUser.id) && moduleProcesses[i].operations.indexOf('insert') > -1)
                                        isProcessInsert = true;
                                }
                                if (isProcessInsert) {
                                    setTimeout(function () {
                                        result(response.data);
                                    }, 2000)
                                }
                                else
                                    result(response.data);
                            }
                            else
                                result(response.data);

                            components.run('AfterCreate', 'Script', $scope, record);
                        })
                        .catch(function onError(data) {
                            error(data.data, data.status);
                        })
                        .finally(function () {
                            var moduleProcesses = $filter('filter')($rootScope.approvalProcesses, { module_id: $scope.module.id }, true);
                            if (moduleProcesses) {
                                var isProcessInsert = false;
                                for (var i = 0; i < moduleProcesses.length; i++) {
                                    if ((moduleProcesses[i].user_id === 0 || moduleProcesses[i].user_id === $scope.currentUser.id) && moduleProcesses[i].operations.indexOf('insert') > -1)
                                        isProcessInsert = true;
                                }
                                if (isProcessInsert) {
                                    setTimeout(function () {
                                        $scope.submitting = false;
                                    }, 2000)
                                }
                                else
                                    $scope.submitting = false;
                            }
                            else
                                $scope.submitting = false;
                        });
                }
                else {
                    if ($scope.clone) {
                        if ($scope.revise) {
                            record.master_id = record.id;
                            var quoteStageField = $filter('filter')($scope.module.fields, { name: 'quote_stage' }, true)[0];
                            record.quote_stage = $filter('filter')($scope.picklistsModule[quoteStageField.picklist_id], { value: 'delivered' }, true)[0].id;
                        }

                        delete record.id;

                        //removes process approval fields
                        delete record.process_id;
                        delete record.process_status;
                        delete record.process_status_order;
                        delete record.operation_type;

                        if (record.auto_id) record.auto_id = "";

                        ModuleService.insertRecord($scope.module.name, record)
                            .then(function onSuccess(response) {
                                if (!record.master_id) {
                                    $scope.submitting = false;
                                    result(response.data);
                                }
                                else {
                                    //After record is revised, update the master record's stage
                                    ModuleService.getRecord($scope.module.name, record.master_id)
                                        .then(function onSuccess(recordDataMaster) {
                                            var masterRecord = ModuleService.processRecordSingle(recordDataMaster.data, $scope.module, $scope.picklistsModule);
                                            var masterRecordState = angular.copy(masterRecord);
                                            var quoteStageField = $filter('filter')($scope.module.fields, { name: 'quote_stage' }, true)[0];
                                            masterRecord.quote_stage = $filter('filter')($scope.picklistsModule[quoteStageField.picklist_id], { value: 'revised' }, true)[0];
                                            masterRecord = ModuleService.prepareRecord(masterRecord, $scope.module, masterRecordState);

                                            ModuleService.updateRecord($scope.module.name, masterRecord)
                                                .then(function () {
                                                    $scope.submitting = false;
                                                    result(response.data);
                                                });
                                        });
                                }
                                components.run('AfterCreate', 'Script', $scope, record);
                            })
                            .catch(function onError() {
                                error(data, status);
                                $scope.submitting = false;
                            });
                    }
                    else {

                        ModuleService.updateRecord($scope.module.name, record)
                            .then(function onSuccess(response) {
                                var moduleProcesses = $filter('filter')($rootScope.approvalProcesses, { module_id: $scope.module.id }, true);
                                if (moduleProcesses) {
                                    var isProcessUpdate = false;
                                    for (var i = 0; i < moduleProcesses.length; i++) {
                                        if ((moduleProcesses[i].user_id === 0 || moduleProcesses[i].user_id === $scope.currentUser.id) && moduleProcesses[i].operations.indexOf('update') > -1)
                                            isProcessUpdate = true;
                                    }
                                    if (isProcessUpdate) {
                                        setTimeout(function () {
                                            result(response.data);
                                        }, 2000)
                                    }
                                    else
                                        result(response.data);
                                }
                                else
                                    result(response.data);

                                components.run('AfterUpdate', 'Script', $scope, record);
                            })
                            .catch(function onError(data) {
                                error(data.data, data.status);
                            })
                            .finally(function () {
                                var moduleProcesses = $filter('filter')($rootScope.approvalProcesses, { module_id: $scope.module.id }, true);
                                if (moduleProcesses) {
                                    var isProcessUpdate = false;
                                    for (var i = 0; i < moduleProcesses.length; i++) {
                                        if ((moduleProcesses[i].user_id === 0 || moduleProcesses[i].user_id === $scope.currentUser.id) && moduleProcesses[i].operations.indexOf('update') > -1)
                                            isProcessUpdate = true;
                                    }
                                    if (isProcessUpdate) {
                                        setTimeout(function () {
                                            $scope.submitting = false;
                                        }, 2000)
                                    }
                                    else
                                        $scope.submitting = false;
                                }
                                else
                                    $scope.submitting = false;
                            });
                    }
                }

                function result(response) {
                    $scope.recordId = response.id;

                    if ($scope.uploader.queue.length > 0) {
                        $scope.uploader.onCompleteAll = function () {
                            success();
                        };
                        $scope.uploader.uploadAll();
                    }

                    //Need to iterate document upload names - unique object from fileuploader..
                    var documentFields = $filter('filter')($scope.module.fields, { data_type: 'document', deleted: false }, true);

                    if (documentFields) {
                        angular.forEach(documentFields, function (field) {

                            if ($scope.uploaderBasic[field.name]) {

                                if ($scope.uploaderBasic[field.name].queue.length > 0) {

                                    //set record id for insert, so on update also this way better.
                                    angular.forEach($scope.uploaderBasic[field.name].queue, function (item) {

                                        for (var i = 0; i < item.formData.length; i++) {
                                            if (item.formData[i].hasOwnProperty("recordid")) {
                                                item.formData[i].recordid = $scope.recordId;
                                            }
                                        }

                                    });

                                    $scope.uploaderBasic[field.name].onCompleteItem = function (fileItem, response, status, headers) {
                                        $scope.uploaderBasic[field.name].clearQueue();
                                    };

                                    $scope.uploaderBasic[field.name].uploadItem(0);

                                }
                            }

                        });
                    }

                    if ($scope.type === 'quotes') {
                        var quoteProducts = [];
                        var no = 1;
                        var quoteProductsOrders = $filter('orderBy')($scope.quoteProducts, 'order');
                        angular.forEach(quoteProductsOrders, function (quoteProduct) {
                            if (quoteProduct.deleted)
                                return;

                            var quote = {};
                            quote.id = $scope.recordId;
                            quote.primary_value = $scope.record[$scope.primaryField.name];
                            quoteProduct.quote = quote;
                            delete quoteProduct.vat;
                            if ($scope.clone) {
                                delete (quoteProduct.id);
                                delete (quoteProduct._rev);
                            }

                            if (!quoteProduct.separator && quoteProduct.no) {
                                quoteProduct.no = no++;
                            }
                            //Discount percent applied also calculate discount amount.
                            if (quoteProduct.discount_percent && quoteProduct.discount_amount == null)
                                quoteProduct.discount_amount = parseFloat((quoteProduct.unit_price * quoteProduct.quantity) - quoteProduct.amount);

                            //Discount amount applied also calculate discount percent.
                            if (quoteProduct.discount_amount && quoteProduct.discount_percent == null)
                                quoteProduct.discount_percent = parseFloat((100 * ((quoteProduct.unit_price * quoteProduct.quantity) - quoteProduct.amount)) / quoteProduct.unit_price);

                            quoteProducts.push(quoteProduct);
                        });

                        var quoteProductRecordsBulk = ModuleService.prepareRecordBulk(quoteProducts, $scope.quoteProductModule);

                        var insertRecords = function () {
                            ModuleService.insertRecordBulk($scope.quoteProductModule.name, quoteProductRecordsBulk)
                                .then(function onSuccess() {
                                    success();
                                });
                        };

                        if (!$scope.id || $scope.revise) {
                            if (quoteProducts.length) {
                                insertRecords();
                            }
                            else {
                                success();
                            }
                        }
                        else {
                            var ids = [];

                            angular.forEach($scope.quoteProducts, function (quoteProduct) {
                                if (quoteProduct.id)
                                    ids.push(quoteProduct.id);
                            });

                            if (ids.length) {
                                ModuleService.deleteRecordBulk($scope.quoteProductModule.name, ids)
                                    .then(function onSuccess() {
                                        if (quoteProducts.length) {
                                            insertRecords();
                                        }
                                        else {
                                            success();
                                        }
                                    });
                            }
                            else {
                                if ($scope.quoteProducts.length) {
                                    insertRecords();
                                }
                                else {
                                    success();
                                }
                            }
                        }
                    }
                    else if ($scope.type === 'sales_orders') {
                        var orderProducts = [];

                        angular.forEach($scope.orderProducts, function (orderProduct) {
                            if (!orderProduct.product || orderProduct.deleted)
                                return;

                            var sales_order = {};
                            sales_order.id = $scope.recordId;
                            sales_order.primary_value = $scope.record[$scope.primaryField.name];
                            orderProduct.sales_order = sales_order;
                            delete orderProduct.vat;

                            if ($scope.clone) {
                                delete (orderProduct.id);
                                delete (orderProduct._rev);
                            }
                            //Discount percent applied also calculate discount amount.
                            if (orderProduct.discount_percent && orderProduct.discount_amount == null)
                                orderProduct.discount_amount = parseFloat((orderProduct.unit_price * orderProduct.quantity) - orderProduct.amount);

                            //Discount amount applied also calculate discount percent.
                            if (orderProduct.discount_amount && orderProduct.discount_percent == null)
                                orderProduct.discount_percent = parseFloat((100 * ((orderProduct.unit_price * orderProduct.quantity) - orderProduct.amount)) / orderProduct.unit_price);

                            orderProducts.push(orderProduct);
                        });

                        var orderProductRecordsBulk = ModuleService.prepareRecordBulk(orderProducts, $scope.orderProductModule);

                        var insertRecords = function () {
                            ModuleService.insertRecordBulk($scope.orderProductModule.name, orderProductRecordsBulk)
                                .then(function onSuccess() {
                                    success();
                                });
                        };

                        if (!$scope.id || $scope.revise) {
                            if (orderProducts.length) {
                                insertRecords();
                            }
                            else {
                                success();
                            }
                        }
                        else {
                            var ids = [];

                            angular.forEach($scope.orderProducts, function (orderProduct) {
                                if (orderProduct.id)
                                    ids.push(orderProduct.id);
                            });

                            if (ids.length) {
                                ModuleService.deleteRecordBulk($scope.orderProductModule.name, ids)
                                    .then(function () {
                                        if (orderProducts.length) {
                                            insertRecords();
                                        }
                                        else {
                                            success();
                                        }
                                    });
                            }
                            else {
                                if ($scope.orderProducts.length) {
                                    insertRecords();
                                }
                                else {
                                    success();
                                }
                            }
                        }
                    }
                    else if ($scope.type === 'purchase_orders') {
                        var purchaseProducts = [];

                        angular.forEach($scope.purchaseProducts, function (purchaseProduct) {
                            if (!purchaseProduct.product || purchaseProduct.deleted)
                                return;

                            var purchase_order = {};
                            purchase_order.id = $scope.recordId;
                            purchase_order.primary_value = $scope.record[$scope.primaryField.name];
                            purchaseProduct.purchase_order = purchase_order;
                            delete purchaseProduct.vat;

                            if ($scope.clone) {
                                delete (purchaseProduct.id);
                                delete (purchaseProduct._rev);
                            }
                            //Discount percent applied also calculate discount amount.
                            if (purchaseProduct.discount_percent && purchaseProduct.discount_amount == null)
                                purchaseProduct.discount_amount = parseFloat((purchaseProduct.unit_price * purchaseProduct.quantity) - purchaseProduct.amount);

                            //Discount amount applied also calculate discount percent.
                            if (purchaseProduct.discount_amount && purchaseProduct.discount_percent == null)
                                purchaseProduct.discount_percent = parseFloat((100 * ((purchaseProduct.unit_price * purchaseProduct.quantity) - purchaseProduct.amount)) / purchaseProduct.unit_price);

                            purchaseProducts.push(purchaseProduct);
                        });

                        var purchaseProductRecordsBulk = ModuleService.prepareRecordBulk(purchaseProducts, $scope.purchaseProductModule);

                        var insertRecords = function () {
                            ModuleService.insertRecordBulk($scope.purchaseProductModule.name, purchaseProductRecordsBulk)
                                .then(function onSuccess() {
                                    success();
                                });
                        };

                        if (!$scope.id || $scope.revise) {
                            if (purchaseProducts.length) {
                                insertRecords();
                            }
                            else {
                                success();
                            }
                        }
                        else {
                            var ids = [];

                            angular.forEach($scope.purchaseProducts, function (purchaseProduct) {
                                if (purchaseProduct.id)
                                    ids.push(purchaseProduct.id);
                            });

                            if (ids.length) {
                                ModuleService.deleteRecordBulk($scope.purchaseProductModule.name, ids)
                                    .then(function () {
                                        if (purchaseProducts.length) {
                                            insertRecords();
                                        }
                                        else {
                                            success();
                                        }
                                    });
                            }
                            else {
                                if ($scope.purchaseProducts.length) {
                                    insertRecords();
                                }
                                else {
                                    success();
                                }
                            }
                        }
                    }
                    else {
                        if ($scope.uploader.queue.length < 1)
                            success();
                    }

                    function success() {
                        var params = { type: $scope.type, id: response.id };
                        if ($scope.saveAndNew) {
                            $scope.record = {};
                            $scope.moduleForm.$setPristine();
                            $window.scrollTo(0, 0);
                            ngToast.create({
                                content: $filter('translate')('Module.SuccessMessage', { title: $scope.module['label_' + $rootScope.language + '_singular'] }),
                                className: 'success'
                            });
                            $scope.uploader.clearQueue();

                            $scope.$broadcast('angucomplete-alt:clearInput');

                            if ($scope.module.name === 'activities') {
                                $scope.record.activity_type = $scope.activity_type_id;
                                $scope.record.related_module = recordCopy.related_module;
                                $scope.record.related_to = recordCopy.related_to;
                            }

                            if ($filter('filter')($scope.module.fields, { name: 'owner' }, true)[0]) {
                                $scope.record.owner = $scope.currentUser;
                                $scope.$broadcast('angucomplete-alt:changeInput', 'owner', $scope.currentUser);
                            }

                            //fills lookup inputs auto
                            angular.forEach($scope.module.fields, function (field) {
                                if (field.data_type === 'lookup') {
                                    if (!$filter('filter')($scope.module.dependencies, { child_field: field.name }, true)[0]) {
                                        $scope.record[field.name] = recordCopy[field.name];
                                        $scope.$broadcast('angucomplete-alt:changeInput', field.name, recordCopy[field.name]);
                                    }
                                }

                            });

                            ModuleService.setDefaultValues($scope.module, $scope.record, $scope.picklistsModule);
                        }
                        else {
                            if ($scope.parentId) {
                                params.type = $scope.parentModule;
                                params.id = $scope.parentId;
                                params.rptype = $scope.returnTab;

                                if ($scope.previousParentType) {
                                    params.rpptype = $scope.previousParentType;
                                    params.rppid = $scope.previousParentId;
                                    params.rprtab = $scope.previousReturnTab;
                                }
                            }

                            if ($scope.back)
                                params.back = $scope.back;
                        }

                        var cacheKey = $scope.module.name + '_' + $scope.module.name;

                        if (!$scope.parentId) {
                            $cache.remove(cacheKey);

                            if ($scope.module.name === 'opportunities')
                                $cache.remove('opportunity' + $scope.id + '_stage_history');
                        }
                        else {
                            cacheKey = (!$scope.relatedToField ? $scope.parentType : 'related_to') + $scope.parentId + '_' + (!$scope.many ? $scope.module.name : $scope.many);
                            var parentCacheKey = $scope.parentType + '_' + $scope.parentType;
                            $cache.remove(cacheKey);
                            $cache.remove(parentCacheKey);
                        }

                        if ($rootScope.activePages && $rootScope.activePages[$scope.module.name])
                            $rootScope.activePages[$scope.module.name] = null;

                        if ($scope.module.display_calendar || $scope.module.name === 'activities')
                            $cache.remove('calendar_events');

                        if ($scope.saveAndNew)
                            $scope.submitting = false;
                        else {
                            if ($scope.module.name === 'stock_transactions') {
                                setTimeout(function () {
                                    $state.go('app.crm.moduleDetail', params);
                                }, 500);
                            } else
                                $state.go('app.crm.moduleDetail', params);

                        }
                        $scope.izinTuruData = null;
                    }
                }

                function error(data, status) {
                    if (status === 409) {
                        $scope.moduleForm[data.field].$setValidity('unique', false);

                        if (data.field2)
                            $scope.moduleForm[data.field2].$setValidity('unique', false);
                    }
                }
            };

            $scope.calculate = function (field) {
                ModuleService.calculate(field, $scope.module, $scope.record);
            };

            $scope.fieldValueChange = function (field) {
                ModuleService.setDependency(field, $scope.module, $scope.record, $scope.picklistsModule, $scope);
                ModuleService.setDisplayDependency($scope.module, $scope.record);
                ModuleService.setCustomCalculations($scope.module, $scope.record, $scope.picklistsModule, $scope);
                ModuleService.customActions($scope.module, $scope.record, $scope.moduleForm, $scope.picklistsModule, $scope);
                components.run('FieldChange', 'Script', $scope, $scope.record);

                if ($scope.moduleForm[field.name].$error.unique)
                    $scope.moduleForm[field.name].$setValidity('unique', true);

                if ($scope.record.currency)
                    $scope.currencySymbol = $scope.record.currency.value || $rootScope.currencySymbol;

                if ($scope.module.name === 'current_accounts' && $scope.currencyField && (field.name === 'customer' || field.name === 'supplier'))
                    setCurrencyCurrentAccounts();
            };

            $scope.hideCreateNew = function (field) {
                if (field.lookup_type === 'users')
                    return true;

                if (field.lookup_type === 'relation' && !$scope.record.related_module)
                    return true;

                return false;
            };

            $scope.openFormModal = function (str) {
                $scope.primaryValueModal = str;

                $scope.formModal = $scope.formModal || $modal({
                    scope: $scope,
                    templateUrl: 'web/views/app/module/moduleFormModal.html',
                    animation: '',
                    backdrop: 'static',
                    show: false
                });

                $scope.formModal.$promise.then($scope.formModal.show);
            };

            function getVatList() {
                if (!$scope.record.vat_list)
                    return;

                var vatListStr = angular.copy($scope.record.vat_list);
                var vatList = vatListStr.split('|');
                $scope.vatList = [];

                angular.forEach(vatList, function (vatItem) {
                    var vatParts = vatItem.split(';');
                    var vat = {};
                    vat.percent = vatParts[0];
                    vat.total = vatParts[1];

                    $scope.vatList.push(vat);
                });
            }

            function getProductRecords(module) {
                ModuleService.getPicklists($scope.productModule)
                    .then(function (productModulePicklists) {
                        if (module === 'quotes') {
                            $scope.quoteProductsLoading = true;
                            var extraFields = ['unit_amount', 'separator', 'purchase_price', 'profit_amount', 'profit_percent'];
                            var additionalFields = [];
                            for (var i = 0; extraFields.length > i; i++) {
                                var field = $filter('filter')($scope.quoteProductModule.fields, { name: extraFields[i] }, true);
                                if (field.length > 0) {
                                    additionalFields.push(extraFields[i]);
                                }
                            }

                            ModuleService.getPicklists($scope.quoteProductModule)
                                .then(function (quoteProductModulePicklists) {
                                    var findRequest = {};
                                    findRequest.fields = ['quantity', 'usage_unit', 'unit_price', 'discount_percent', 'discount_amount', 'discount_type', 'amount', 'order', 'product.products.id', 'product.products.name.primary', 'product.products.unit_price', 'product.products.usage_unit', 'product.products.vat_percent', 'deleted'];
                                    findRequest.filters = [{ field: 'quote', operator: 'equals', value: $scope.id }];
                                    findRequest.sort_field = 'order';
                                    findRequest.sort_direction = 'asc';
                                    findRequest.limit = 1000;
                                    findRequest.fields = findRequest.fields.concat(additionalFields);

                                    if ($scope.productCurrencyField)
                                        findRequest.fields.push('product.products.currency');

                                    ModuleService.findRecords($scope.quoteProductModule.name, findRequest)
                                        .then(function (response) {
                                            $scope.quoteProducts = [];

                                            angular.forEach(response.data, function (quoteProductRecordData) {
                                                angular.forEach(quoteProductRecordData, function (value, key) {
                                                    if (key.indexOf('.') > -1) {
                                                        var keyParts = key.split('.');

                                                        quoteProductRecordData[keyParts[0] + '.' + keyParts[2]] = quoteProductRecordData[key];
                                                        delete quoteProductRecordData[key];
                                                    }
                                                });

                                                var quoteProductRecord = ModuleService.processRecordSingle(quoteProductRecordData, $scope.quoteProductModule, quoteProductModulePicklists);

                                                if (quoteProductRecord.product) {
                                                    quoteProductRecord.usage_unit = quoteProductRecord.product.usage_unit['label_' + $rootScope.language];

                                                    if (quoteProductRecord.product.currency && !angular.isObject(quoteProductRecord.product.currency)) {
                                                        var currencyField = $filter('filter')($scope.productModule.fields, { name: 'currency' }, true)[0];
                                                        var currencyPicklistItem = $filter('filter')(productModulePicklists[currencyField.picklist_id], { labelStr: quoteProductRecord.product.currency }, true)[0];
                                                        quoteProductRecord.product.currency = currencyPicklistItem;
                                                    }
                                                }

                                                if (angular.isObject(quoteProductRecord.usage_unit)) {
                                                    quoteProductRecord.usage_unit = quoteProductRecord.usage_unit['label_' + $rootScope.language];
                                                } else {
                                                    if (quoteProductRecord.product) {
                                                        quoteProductRecord.usage_unit = quoteProductRecord.product.usage_unit;
                                                    }
                                                }
                                                $scope.quoteProducts.push(quoteProductRecord);
                                            });
                                        })
                                        .finally(function () {
                                            $scope.quoteProductsLoading = false;
                                        });
                                });

                            getVatList();
                        }
                        else if (module === 'sales_orders') {
                            $scope.orderProductsLoading = true;

                            ModuleService.getPicklists($scope.orderProductModule)
                                .then(function (orderProductModulePicklists) {
                                    var findRequest = {};
                                    findRequest.fields = ['quantity', 'usage_unit', 'unit_price', 'discount_percent', 'discount_amount', 'discount_type', 'amount', 'order', 'product.products.id', 'product.products.name.primary', 'product.products.unit_price', 'product.products.usage_unit', 'product.products.vat_percent'];
                                    findRequest.filters = [{ field: 'sales_order', operator: 'equals', value: $scope.id }];
                                    findRequest.sort_field = 'order';
                                    findRequest.sort_direction = 'asc';
                                    findRequest.limit = 1000;

                                    if ($scope.productCurrencyField)
                                        findRequest.fields.push('product.products.currency');

                                    ModuleService.findRecords($scope.orderProductModule.name, findRequest)
                                        .then(function (response) {
                                            $scope.orderProducts = [];

                                            angular.forEach(response.data, function (orderProductRecordData) {
                                                angular.forEach(orderProductRecordData, function (value, key) {
                                                    if (key.indexOf('.') > -1) {
                                                        var keyParts = key.split('.');

                                                        orderProductRecordData[keyParts[0] + '.' + keyParts[2]] = orderProductRecordData[key];
                                                        delete orderProductRecordData[key];
                                                    }
                                                });

                                                var orderProductRecord = ModuleService.processRecordSingle(orderProductRecordData, $scope.orderProductModule, orderProductModulePicklists);

                                                if (orderProductRecord.product) {
                                                    orderProductRecord.usage_unit = orderProductRecord.product.usage_unit['label_' + $rootScope.language];

                                                    if (orderProductRecord.product.currency && !angular.isObject(orderProductRecord.product.currency)) {
                                                        var currencyField = $filter('filter')($scope.productModule.fields, { name: 'currency' }, true)[0];
                                                        var currencyPicklistItem = $filter('filter')(productModulePicklists[currencyField.picklist_id], { labelStr: orderProductRecord.product.currency }, true)[0];
                                                        orderProductRecord.product.currency = currencyPicklistItem;
                                                    }
                                                }

                                                if (angular.isObject(orderProductRecord.usage_unit)) {
                                                    orderProductRecord.usage_unit = orderProductRecord.usage_unit['label_' + $rootScope.language];
                                                }

                                                $scope.orderProducts.push(orderProductRecord);
                                            });
                                        })
                                        .finally(function () {
                                            $scope.orderProductsLoading = false;
                                        });
                                });

                            getVatList();
                        }
                        else if (module === 'purchase_orders') {
                            $scope.purchaseProductsLoading = true;

                            ModuleService.getPicklists($scope.purchaseProductModule)
                                .then(function (purchaseProductModulePicklists) {
                                    var findRequest = {};
                                    findRequest.fields = ['quantity', 'usage_unit', 'unit_price', 'discount_percent', 'discount_amount', 'discount_type', 'amount', 'order', 'product.products.id', 'product.products.name.primary', 'product.products.unit_price', 'product.products.usage_unit', 'product.products.vat_percent'];
                                    findRequest.filters = [{ field: 'purchase_order', operator: 'equals', value: $scope.id }];
                                    findRequest.sort_field = 'order';
                                    findRequest.sort_direction = 'asc';
                                    findRequest.limit = 1000;

                                    if ($scope.productCurrencyField)
                                        findRequest.fields.push('product.products.currency');

                                    ModuleService.findRecords($scope.purchaseProductModule.name, findRequest)
                                        .then(function (response) {
                                            $scope.purchaseProducts = [];

                                            angular.forEach(response.data, function (purchaseProductRecordData) {
                                                angular.forEach(purchaseProductRecordData, function (value, key) {
                                                    if (key.indexOf('.') > -1) {
                                                        var keyParts = key.split('.');

                                                        purchaseProductRecordData[keyParts[0] + '.' + keyParts[2]] = purchaseProductRecordData[key];
                                                        delete purchaseProductRecordData[key];
                                                    }
                                                });

                                                var purchaseProductRecord = ModuleService.processRecordSingle(purchaseProductRecordData, $scope.purchaseProductModule, purchaseProductModulePicklists);

                                                if (purchaseProductRecord.product) {
                                                    purchaseProductRecord.usage_unit = purchaseProductRecord.product.usage_unit['label_' + $rootScope.language];

                                                    if (purchaseProductRecord.product.currency && !angular.isObject(purchaseProductRecord.product.currency)) {
                                                        var currencyField = $filter('filter')($scope.productModule.fields, { name: 'currency' }, true)[0];
                                                        var currencyPicklistItem = $filter('filter')(productModulePicklists[currencyField.picklist_id], { labelStr: purchaseProductRecord.product.currency }, true)[0];
                                                        purchaseProductRecord.product.currency = currencyPicklistItem;
                                                    }
                                                }

                                                if (angular.isObject(purchaseProductRecord.usage_unit)) {
                                                    purchaseProductRecord.usage_unit = purchaseProductRecord.usage_unit['label_' + $rootScope.language];
                                                }

                                                $scope.purchaseProducts.push(purchaseProductRecord);
                                            });
                                        })
                                        .finally(function () {
                                            $scope.purchaseProductsLoading = false;
                                        });
                                });

                            getVatList();
                        }
                    });
            }

            ModuleService.getActionButtons($scope.module.id)
                .then(function (actionButtons) {
                    $scope.actionButtons = $filter('filter')(actionButtons, { trigger: '!Detail' }, true);
                });

            $scope.showModuleFrameModal = function (url) {
                if (new RegExp("https:").test(url)) {
                    var title, w, h;
                    title = 'myPop1';
                    w = document.body.offsetWidth - 200;
                    h = document.body.offsetHeight - 200;
                    var left = (screen.width / 2) - (w / 2);
                    var top = (screen.height / 2) - (h / 2);
                    window.open(url, title, 'toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=no, resizable=no, copyhistory=no, width=' + w + ', height=' + h + ', top=' + top + ', left=' + left);

                }
                else {
                    $scope.frameUrl = url;
                    $scope.frameModal = $scope.frameModal || $modal({
                        scope: $scope,
                        controller: 'ActionButtonFrameController',
                        templateUrl: 'web/views/app/actionbutton/actionButtonFrameModal.html',
                        backdrop: 'static',
                        show: false
                    });

                    $scope.frameModal.$promise.then($scope.frameModal.show);
                }

            };

            $scope.openLocationModal = function (filedName) {
                $scope.filedName = filedName;
                $scope.locationModal = $scope.frameModal || $modal({
                    scope: $scope,
                    controller: 'locationFormModalController',
                    templateUrl: 'web/views/app/location/locationFormModal.html',
                    backdrop: 'static',
                    show: false
                });
                $scope.locationModal.$promise.then($scope.locationModal.show);
            };

            $scope.removeDocument = function (field) {

                var data = {};
                data["module"] = $scope.module.name;
                data["recordId"] = $scope.record.id;
                data["fieldName"] = field.name;
                data["fileNameExt"] = helper.getFileExtension($scope.record[field.name]);
                data["instanceId"] = $rootScope.workgroup.instanceID;

                $scope.record[field.name] = null;
                if (field.data_type == 'document') {
                    $scope.uploaderBasic[field.name].queue = [];
                }
                if (field.data_type == 'image') {
                    $scope.uploaderImage[field.name].queue = [];
                }
                angular.forEach($scope.moduleForm[field.name].$error, function (value, key) {
                    $scope.moduleForm[field.name].$setValidity(key, value);
                });
            };

            $scope.checkUploadFile = function (event) {
                var files = event.target.files;
                var clickedInputId = event.target.id;
                var inputLabel = angular.element(document.getElementById('lbl_' + clickedInputId))[0];
                if (isAcceptedExtension(files[0])) {
                    inputLabel.innerText = files[0].name;
                }
            };

            $scope.fileLoadingCounter = 0;

            $scope.uploaderBasic = function (field) {
                var uploader_basic = $scope.uploaderBasic[field.name] = new FileUploader({
                    url: config.apiUrl + 'document/upload_document_file',
                    headers: {
                        'Authorization': 'Bearer ' + $localStorage.read('access_token'),
                        'Accept': 'application/json' /// we have to set accept header to provide consistency between browsers.
                    },
                    queueLimit: 1

                });

                uploader_basic.onAfterAddingFile = function (item) {
                    var selectedFile = item.uploader.queue[0].file.name;
                    $scope.record[field.name] = selectedFile;

                    var uniquefieldname = field.name;

                    item.formData.push({ name: uniquefieldname });
                    item.formData.push({ filename: $scope.record[field.name] });
                    item.formData.push({ recordid: $scope.record.id });
                    item.formData.push({ modulename: $scope.module.name })
                    item.formData.push({ container: $rootScope.workgroup.instanceID })
                    item.formData.push({ documentsearch: field.document_search })

                };

                uploader_basic.onWhenAddingFileFailed = function (item, filter, options) {
                    switch (filter.name) {
                        case 'docFilter':
                            ngToast.create({ content: $filter('translate')('Setup.Settings.DocumentTypeError'), className: 'warning' });
                            break;
                        case 'sizeFilter':
                            ngToast.create({ content: $filter('translate')('Setup.Settings.SizeError'), className: 'warning' });
                            break;
                    }
                };

                uploader_basic.filters.push({
                    name: 'docFilter',
                    fn: function (item) {
                        var extension = helper.getFileExtension(item.name);
                        return true ? (extension === 'txt' || extension == 'docx' || extension == 'pdf' || extension == 'doc') : false;
                    }
                });

                uploader_basic.filters.push({
                    name: 'sizeFilter',
                    fn: function (item) {
                        return item.size < 5242880;//5 mb
                    }
                });


                uploader_basic.onCompleteAll = function () {
                    uploader_basic.clearQueue();
                    $scope.fileLoadingCounter--;
                };

                return uploader_basic;

            };

            $scope.uploaderImage = function (field) {
                $scope.image[field.name] = {};
                var uploader_image = $scope.uploaderImage[field.name] = new FileUploader({
                    url: config.apiUrl + 'document/upload_large',
                    headers: {
                        'Authorization': 'Bearer ' + $localStorage.read('access_token'),
                        'Accept': 'application/json' /// we have to set accept header to provide consistency between browsers.
                    },
                    queueLimit: 1,
                });

                uploader_image.onAfterAddingFile = function (item) {
                    readFile(item._file)
                        .then(function (image) {
                            item.image = image;
                            var img = new Image();
                            resizeService.resizeImage(item.image, { width: 1024 }, function (err, resizedImage) {
                                if (err)
                                    return;

                                item._file = dataURItoBlob(resizedImage);
                                item.file.size = item._file.size;
                                $scope.fileLoadingCounter++;
                                var selectedFile = item.uploader.queue[0].file.name;
                                $scope.record[field.name] = selectedFile;
                                $scope.image[field.name]['Name'] = item.uploader.queue[0].file.name;
                                $scope.image[field.name]['Size'] = item.uploader.queue[0].file.size;
                                $scope.image[field.name]['Type'] = item.uploader.queue[0].file.type;
                                item.upload();
                            });
                        });
                };
                uploader_image.onWhenAddingFileFailed = function (item, filter, options) {
                    switch (filter.name) {
                        case 'imgFilter':
                            ngToast.create({
                                content: $filter('translate')('Setup.Settings.ImageError'),
                                className: 'warning'
                            });
                            break;
                        case 'sizeFilter':
                            ngToast.create({
                                content: $filter('translate')('Setup.Settings.SizeError'),
                                className: 'warning'
                            });
                            break;
                    }
                };

                uploader_image.filters.push({
                    name: 'imgFilter',
                    fn: function (item) {
                        var extension = helper.getFileExtension(item.name);
                        return true ? (extension === 'jpg' || extension == 'jpeg' || extension == 'png' || extension == 'doc' || extension == 'gif') : false;
                    }
                });

                uploader_image.filters.push({
                    name: 'sizeFilter',
                    fn: function (item) {
                        return item.size < 5242880;// 5mb
                    }
                });

                uploader_image.onSuccessItem = function (item, response) {
                    $scope.image[field.name]['UniqueName'] = response.UniqueName;
                    $scope.fileLoadingCounter--;
                };

                var dataURItoBlob = function (dataURI) {
                    var binary = atob(dataURI.split(',')[1]);
                    var mimeString = dataURI.split(',')[0].split(':')[1].split(';')[0];
                    var array = [];

                    for (var i = 0; i < binary.length; i++) {
                        array.push(binary.charCodeAt(i));
                    }

                    return new Blob([new Uint8Array(array)], { type: mimeString });
                };

                function readFile(file) {
                    var deferred = $q.defer();
                    var reader = new FileReader();

                    reader.onload = function (e) {
                        deferred.resolve(e.target.result);
                    };

                    reader.readAsDataURL(file);

                    return deferred.promise;
                }

                return uploader_image;

            };

            //webhook request func for action button
            $scope.webhookRequest = function (action) {
                var jsonData = {};
                var params = action.parameters.split(',');
                $scope.webhookRequesting = {};

                $scope.webhookRequesting[action.id] = true;

                angular.forEach(params, function (data) {

                    var dataObject = data.split('|');
                    var parameterName = dataObject[0];
                    var moduleName = dataObject[1];
                    var fieldName = dataObject[2];

                    if (moduleName != $scope.module.name) {
                        if ($scope.record[moduleName])
                            jsonData[parameterName] = $scope.record[moduleName][fieldName];
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
                    }
                    else {
                        if ($scope.record[fieldName])
                            jsonData[parameterName] = $scope.record[fieldName];
                        else
                            jsonData[parameterName] = null;
                    }

                });

                if (action.method_type === 'post') {

                    $http.post(action.url, jsonData, { headers: { 'Content-Type': 'application/json' } })
                        .then(function () {
                            ngToast.create({
                                content: $filter('translate')('Module.ActionButtonWebhookSuccess'),
                                className: 'success'
                            });
                            $scope.webhookRequesting[action.id] = false;
                        })
                        .catch(function () {
                            ngToast.create({
                                content: $filter('translate')('Module.ActionButtonWebhookFail'),
                                className: 'warning'
                            });
                            $scope.webhookRequesting[action.id] = false;
                        });

                }
                else if (action.method_type === 'get') {

                    var query = "";

                    for (var key in jsonData) {
                        query += key + "=" + jsonData[key] + "&";
                    }
                    if (query.length > 0) {
                        query = query.substring(0, query.length - 1);
                    }

                    $http.get(action.url + "?" + query)
                        .then(function () {
                            ngToast.create({
                                content: $filter('translate')('Module.ActionButtonWebhookSuccess'),
                                className: 'success'
                            });
                            $scope.webhookRequesting[action.id] = false;
                        })
                        .catch(function () {
                            ngToast.create({
                                content: $filter('translate')('Module.ActionButtonWebhookFail'),
                                className: 'warning'
                            });
                            $scope.webhookRequesting[action.id] = false;
                        });

                }
            };


            $scope.$on('$locationChangeStart', function (event) {
                if ($scope.moduleForm) {
                    if ($scope.moduleForm.$dirty) {
                        if (!confirm($filter('translate')('Module.LeavePageWarning'))) {
                            event.preventDefault();
                        }
                    }
                }
            });

            $scope.setDropdownData = function (field) {
                if ($scope.dropdownFieldDatas[field.name] && $scope.dropdownFieldDatas[field.name].length > 0)
                    return;

                $scope.currentLookupField = field;
                $scope.lookup()
                    .then(function (response) {
                        $scope.dropdownFieldDatas[field.name] = response;
                    });

            };

            $scope.getAttachments = function () {
                if (!helper.hasDocumentsPermission($scope.operations.read))
                    return;

                DocumentService.getEntityDocuments($rootScope.workgroup.instanceID, $scope.id, $scope.module.id)
                    .then(function (data) {
                        $scope.documentsResultSet = DocumentService.processDocuments(data.data, $rootScope.users);
                        $scope.documents = $scope.documentsResultSet.documentList;
                        $scope.loadingDocuments = false;
                    });
            };
        }
    ]);