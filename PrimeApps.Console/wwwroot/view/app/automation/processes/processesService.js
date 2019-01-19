'use strict';

angular.module('primeapps')

    .factory('ProcessesService', ['$rootScope', '$http', 'config', '$filter', '$q', 'helper', 'defaultLabels', '$cache', 'dataTypes', 'systemFields', 'ModuleService',
        function ($rootScope, $http, config, $filter, $q, helper, defaultLabels, $cache, dataTypes, systemFields, ModuleService) {
            return {
                find: function (model) {
                    return $http.post(config.apiUrl + 'process/find/', model);
                },

                count: function () {
                    return $http.get(config.apiUrl + 'process/count/');
                },

                get: function (id) {
                    return $http.get(config.apiUrl + 'process/get/' + id);
                },

                getAll: function () {
                    return $http.get(config.apiUrl + 'process/get_all');
                },

                create: function (process) {
                    return $http.post(config.apiUrl + 'process/create', process);
                },

                update: function (process) {
                    return $http.put(config.apiUrl + 'process/update/' + process.id, process);
                },

                delete: function (id) {
                    return $http.delete(config.apiUrl + 'process/delete/' + id);
                },

                getAllProcess: function () {
                    return $http.get(config.apiUrl + 'process/get_all');
                },

                getAllProcessRequests: function (id) {
                    return $http.get(config.apiUrl + 'process_request/get_requests/' + id);
                },

                process: function (workflows) {
                    angular.forEach(workflows, function (workflow) {
                        var module = $filter('filter')($rootScope.modules, { id: workflow.module_id }, true)[0];

                        if (!module)
                            return;

                        workflow.moduleStr = module['label_' + $rootScope.language + '_singular'];
                        workflow.statusStr = workflow.active ? $filter('translate')('Common.Yes') : $filter('translate')('Common.No');
                        workflow.operationsStr = '';
                        var operations = workflow.operations.split(',');

                        angular.forEach(operations, function (operation) {
                            workflow.operationsStr += $filter('translate')('Setup.Workflow.RuleTriggers.' + operation) + ', ';
                        });

                        workflow.operationsStr = workflow.operationsStr.slice(0, -2);
                    });

                    return workflows;
                },

                getScheduleItems: function () {
                    var scheduleItems = [];

                    var scheduleItem0 = {};
                    scheduleItem0.label = $filter('translate')('Setup.Workflow.ScheduleItem0');
                    scheduleItem0.value = 'now';
                    scheduleItems.push(scheduleItem0);

                    var scheduleItem1 = {};
                    scheduleItem1.label = $filter('translate')('Setup.Workflow.ScheduleItem1');
                    scheduleItem1.value = 1;
                    scheduleItems.push(scheduleItem1);


                    for (var i = 2; i < 31; i++) {
                        var scheduleItem = {};
                        scheduleItem.label = $filter('translate')('Setup.Workflow.ScheduleItemMany', { day: i });
                        scheduleItem.value = i;
                        scheduleItems.push(scheduleItem);
                    }

                    return scheduleItems;
                },

                getDueDateItems: function () {
                    var dueDateItems = [];

                    var dueDateItem0 = {};
                    dueDateItem0.label = $filter('translate')('Setup.Workflow.DueDateItem0');
                    dueDateItem0.value = 'now';
                    dueDateItems.push(dueDateItem0);

                    var dueDateItem1 = {};
                    dueDateItem1.label = $filter('translate')('Setup.Workflow.DueDateItem1');
                    dueDateItem1.value = 1;
                    dueDateItems.push(dueDateItem1);


                    for (var i = 2; i < 31; i++) {
                        var dueDateItem = {};
                        dueDateItem.label = $filter('translate')('Setup.Workflow.DueDateItemMany', { day: i });
                        dueDateItem.value = i;
                        dueDateItems.push(dueDateItem);
                    }

                    return dueDateItems;
                },

                processWorkflow: function (workflow, module, picklistsModule, filters, scheduleItems, dueDateItems, picklistsActivity, taskFields, picklistUpdateModule, dynamicprocessModules) {

                    var workflowModel = {};
                    workflowModel.id = workflow.id;
                    workflowModel.created_by = workflow.created_by;
                    workflowModel.updated_by = workflow.updated_by;
                    workflowModel.created_at = workflow.created_at;
                    workflowModel.updated_at = workflow.updated_at;
                    workflowModel.deleted = workflow.deleted;
                    workflowModel.name = workflow.name;
                    workflowModel.module = module;
                    workflowModel.active = workflow.active;
                    workflowModel.frequency = workflow.frequency || 'one_time';
                    workflowModel.trigger_time = workflow.trigger_time;
                    workflowModel.approver_type = workflow.approver_type;

                    var profileList = [];
                    if (workflow.profile_list.length > 0) {
                        for (var k = 0; k < workflow.profile_list.length; k++) {
                            var profile = $filter('filter')($rootScope.profiles, { id: parseInt(workflow.profile_list[k]) }, true)[0];
                            profileList.push(profile);
                        }
                    }

                    workflowModel.profiles = profileList;

                    if (workflowModel.approver_type === 'dynamicApprover') {
                        var moduleObj = {};
                        var processApproverField = workflow.approver_field.split(',');

                        var lookupField = processApproverField[0].split('.');
                        var lookupModule = $filter('filter')(workflowModel.module.fields, { name: lookupField[0] }, true)[0];
                        var approverModule = $filter('filter')($rootScope.modules, { name: lookupModule.lookup_type }, true)[0];
                        ModuleService.getModuleFields(approverModule.name)
                            .then(function (response1) {
                                if (response1.data) {
                                    approverModule.fields = response1.data;
                                }

                                var paramModule = $filter('filter')(dynamicprocessModules, { systemName: lookupField[0] }, true)[0];
                                moduleObj.module = approverModule;
                                moduleObj.name = lookupModule['label_' + $rootScope.language] + ' ' + '(' + moduleObj.module['label_' + $rootScope.language + '_singular'] + ')';
                                moduleObj.isSameModule = paramModule.isSameModule;
                                moduleObj.systemName = lookupField[0];
                                moduleObj.id = paramModule.id;
                                workflowModel.firstApproverModule = moduleObj;
                                workflowModel.first_approver_lookup = $filter('filter')(approverModule.fields, { name: lookupField[1] }, true)[0];
                                var firstDynamicApproverFields = $filter('filter')($rootScope.modules, { name: workflowModel.first_approver_lookup.lookup_type }, true)[0];
                                ModuleService.getModuleFields(firstDynamicApproverFields.name)
                                    .then(function (response2) {
                                        if (response2.data) {
                                            firstDynamicApproverFields.fields = response2.data;
                                        }
                                        workflowModel.first_approver_field = $filter('filter')(firstDynamicApproverFields.fields, { name: lookupField[2] }, true)[0];

                                        if (processApproverField.length > 1) {
                                            var secondModuleObj = {};
                                            var secondLookupField = processApproverField[1].split('.');
                                            var secondLookupModule = $filter('filter')(workflowModel.module.fields, { name: secondLookupField[0] }, true)[0];
                                            var secondApproverModule = $filter('filter')($rootScope.modules, { name: secondLookupModule.lookup_type }, true)[0];
                                            ModuleService.getModuleFields(secondApproverModule.name)
                                                .then(function (response3) {
                                                    if (response3.data) {
                                                        secondApproverModule.fields = response3.data;
                                                    }

                                                    var secondParamModule = $filter('filter')(dynamicprocessModules, { systemName: secondLookupField[0] }, true)[0];
                                                    secondModuleObj.module = secondApproverModule;
                                                    secondModuleObj.name = secondLookupModule['label_' + $rootScope.language] + ' ' + '(' + secondModuleObj.module['label_' + $rootScope.language + '_singular'] + ')';
                                                    secondModuleObj.isSameModule = secondParamModule.isSameModule;
                                                    secondModuleObj.systemName = secondLookupField[0];
                                                    secondModuleObj.id = secondParamModule.id;
                                                    workflowModel.secondApproverModule = secondModuleObj;
                                                    workflowModel.second_approver_lookup = $filter('filter')(secondApproverModule.fields, { name: secondLookupField[1] }, true)[0];
                                                    var secondDynamicApproverFields = $filter('filter')($rootScope.modules, { name: workflowModel.second_approver_lookup.lookup_type }, true)[0];
                                                    workflowModel.second_approver_field = $filter('filter')(secondDynamicApproverFields.fields, { name: secondLookupField[2] }, true)[0];

                                                });
                                        }
                                    });
                            });
                    }

                    workflowModel.operation = {};

                    angular.forEach(workflow.operations_array, function (operation) {
                        workflowModel.operation[operation] = true;
                    });
                    workflowModel.user = workflow.user_id;


                    if (workflow.filters) {
                        workflow.filters = $filter('orderBy')(workflow.filters, 'no');

                        for (var i = 0; i < workflow.filters.length; i++) {
                            var filter = workflow.filters[i];
                            var field = $filter('filter')(workflowModel.module.fields, { name: filter.field }, true)[0];
                            var fieldValue = null;

                            if (!field)
                                return;

                            switch (field.data_type) {
                                case 'picklist':
                                    fieldValue = $filter('filter')(picklistsModule[field.picklist_id], { labelStr: filter.value }, true)[0];
                                    break;
                                case 'multiselect':
                                    var picklistItems = filter.value.split('|');
                                    fieldValue = [];

                                    angular.forEach(picklistItems, function (picklistLabel) {
                                        var picklist = $filter('filter')(picklistsModule[field.picklist_id], { labelStr: picklistLabel }, true)[0];

                                        if (picklist)
                                            fieldValue.push(picklist);
                                    });
                                    break;
                                case 'lookup':
                                    if (field.lookup_type === 'users') {
                                        fieldValue = null;
                                        filter.valueState = angular.copy(filter.value);
                                    }
                                    else
                                        fieldValue = filter.value;
                                    break;
                                case 'checkbox':
                                    fieldValue = $filter('filter')(picklistsModule['yes_no'], { system_code: filter.value })[0];
                                    break;
                                default:
                                    fieldValue = filter.value;
                                    break;
                            }

                            filter.field = field;
                            filter.operator = operators[filter.operator];
                            filter.value = fieldValue;

                            if (field.data_type === 'lookup') {
                                field.operators = [];
                                field.operators.push(operators.equals);
                                field.operators.push(operators.not_equal);
                                field.operators.push(operators.empty);
                                field.operators.push(operators.not_empty);
                            }

                            filters[i] = filter;
                        }
                    }

                    return workflowModel;
                },

                prepareWorkflow: function (workflowModel, filters) {
                    var workflow = {};
                    workflow.module_id = workflowModel.module.id;
                    workflow.name = workflowModel.name;
                    workflow.user_id = workflowModel.user;
                    workflow.frequency = workflowModel.frequency;
                    workflow.active = workflowModel.active;
                    workflow.trigger_time = workflowModel.trigger_time;
                    workflow.approver_type = workflowModel.approver_type;

                    var profiles = null;
                    if (workflowModel.profiles && workflowModel.profiles.length) {
                        for (var j = 0; j < workflowModel.profiles.length; j++) {
                            var profile = workflowModel.profiles[j];
                            if (profiles === null)
                                profiles = profile.id;
                            else
                                profiles += ',' + profile.id;
                        }
                    }

                    workflow.profiles = profiles;


                    if (workflowModel.approver_type === 'dynamicApprover') {
                        workflow.approver_field = workflowModel.firstApproverModule.systemName + '.' + workflowModel.first_approver_lookup.name + '.' + workflowModel.first_approver_field.name;
                        if (workflowModel.second_approver_field) {
                            var secondApproverField = workflowModel.secondApproverModule.systemName + '.' + workflowModel.second_approver_lookup.name + '.' + workflowModel.second_approver_field.name;
                            workflow.approver_field += ',' + secondApproverField;
                        }
                    }

                    workflow.operations = [];
                    angular.forEach(workflowModel.operation, function (value, key) {
                        if (value)
                            workflow.operations.push(key);
                    });

                    if (filters && filters.length) {
                        workflow.filters = [];

                        angular.forEach(filters, function (filterItem) {
                            if (!filterItem.field || !filterItem.operator)
                                return;

                            if (!(filterItem.operator.name === 'empty' || filterItem.operator.name === 'not_empty') && (filterItem.value == null || filterItem.value == undefined))
                                return;

                            var field = filterItem.field;
                            var filter = {};
                            filter.field = field.name;
                            filter.operator = filterItem.operator.name;
                            filter.value = filterItem.value;
                            filter.no = filterItem.no;

                            if (!(filterItem.operator.name === 'empty' || filterItem.operator.name === 'not_empty')) {
                                if (field.data_type === 'date' || field.data_type === 'date_time' || field.data_type === 'time')
                                    filter.value = new Date(filter.value).getTime();

                                if (field.data_type === 'picklist')
                                    filter.value = filter.value.id;

                                if (field.data_type === 'multiselect') {
                                    var value = '';

                                    angular.forEach(filter.value, function (picklistItem) {
                                        value += picklistItem.id + ',';
                                    });

                                    filter.value = value.slice(0, -1);
                                }

                                if (field.data_type === 'lookup' && field.lookup_type != 'users')
                                    filter.value = filter.value.id;

                                if (field.data_type === 'lookup' && field.lookup_type === 'users')
                                    filter.value = filter.value[0].id;

                                if (field.data_type === 'checkbox')
                                    filter.value = filter.value.system_code;
                            }
                            else {
                                filter.value = '-';
                            }

                            workflow.filters.push(filter);
                        });
                    }

                    if (workflowModel.approvers) {

                        workflow.approvers = workflowModel.approvers;

                    }

                    return workflow;
                }
            };
        }]);

angular.module('primeapps')

