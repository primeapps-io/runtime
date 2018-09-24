'use strict';

angular.module('primeapps')

    .factory('WorkflowService', ['$rootScope', '$http', 'config', '$filter', 'operators', 'helper', 'ModuleService',
        function ($rootScope, $http, config, $filter, operators, helper, ModuleService) {
            return {

                get: function (id) {
                    return $http.get(config.apiUrl + 'workflow/get/' + id);
                },

                getAll: function () {
                    return $http.get(config.apiUrl + 'workflow/get_all');
                },

                create: function (workflow) {
                    return $http.post(config.apiUrl + 'workflow/create', workflow);
                },

                update: function (workflow) {
                    return $http.put(config.apiUrl + 'workflow/update/' + workflow.id, workflow);
                },

                delete: function (id) {
                    return $http.delete(config.apiUrl + 'workflow/delete/' + id);
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


                    for (var i = 2; i < 181; i++) {
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

                getFields: function (module) {
                    var moduleFields = angular.copy(module.fields);
                    var fields = [];
                    moduleFields = $filter('filter')(moduleFields, { display_list: true, lookup_type: '!relation' }, true);

                    var seperatorFieldMain = {};
                    seperatorFieldMain.name = 'seperator-main';
                    seperatorFieldMain.label = $rootScope.language === 'tr' ? module.label_tr_singular : module.label_en_singular;
                    seperatorFieldMain.order = 0;
                    seperatorFieldMain.seperator = true;
                    moduleFields.push(seperatorFieldMain);
                    var seperatorLookupOrder = 0;

                    angular.forEach(moduleFields, function (field) {
                        if (field.data_type === 'lookup' && field.lookup_type != 'relation') {
                            var lookupModule = angular.copy($filter('filter')($rootScope.modules, { name: field.lookup_type }, true)[0]);
                            seperatorLookupOrder += 100;
                            if (lookupModule === null || lookupModule === undefined) return;
                            var seperatorFieldLookup = {};
                            seperatorFieldLookup.name = 'seperator-' + lookupModule.name;
                            seperatorFieldLookup.order = seperatorLookupOrder;
                            seperatorFieldLookup.seperator = true;

                            if ($rootScope.language === 'tr')
                                seperatorFieldLookup.label = lookupModule.label_tr_singular + ' (' + field.label_tr + ')';
                            else
                                seperatorFieldLookup.label = lookupModule.label_en_singular + ' (' + field.label_en + ')';

                            moduleFields.push(seperatorFieldLookup);

                            var lookupModuleFields = angular.copy(lookupModule.fields);
                            lookupModuleFields = $filter('filter')(lookupModuleFields, { display_list: true }, true);

                            angular.forEach(lookupModuleFields, function (fieldLookup) {
                                if (fieldLookup.data_type === 'lookup')
                                    return;

                                fieldLookup.label = $rootScope.language === 'tr' ? fieldLookup.label_tr : fieldLookup.label_en;
                                fieldLookup.labelExt = '(' + field.label + ')';
                                fieldLookup.name = field.name + '.' + fieldLookup.name;
                                fieldLookup.order = parseInt(fieldLookup.order) + seperatorLookupOrder;
                                fieldLookup.parent_type = field.lookup_type;
                                moduleFields.push(fieldLookup);
                            });
                        }
                    });

                    angular.forEach(moduleFields, function (field) {
                        if (field.deleted || !ModuleService.hasFieldDisplayPermission(field))
                            return;

                        if (field.name && field.data_type != 'lookup') {
                            var newField = {};
                            newField.name = field.name;
                            newField.label = field.label;
                            newField.labelExt = field.labelExt;
                            newField.order = field.order;
                            newField.lookup_type = field.lookup_type;
                            newField.seperator = field.seperator;
                            newField.multiline_type = field.multiline_type;
                            newField.data_type = field.data_type;
                            newField.parent_type = field.parent_type;
                            fields.push(newField);
                        }

                    });

                    fields = $filter('orderBy')(fields, 'order');

                    return fields;
                },

                processWorkflow: function (workflow, module, picklistsModule, filters, scheduleItems, dueDateItems, picklistsActivity, taskFields, picklistUpdateModule, notificationFields, dynamicFiledUpdatefields) {
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
                    workflowModel.delete_logs = workflow.delete_logs;
                    workflowModel.processFilter = workflow.process_filter;
                    workflowModel.frequency = workflow.frequency || 'one_time';

                    if (workflow.changed_field) {
                        workflowModel.changed_field_checkbox = true;
                        workflowModel.changed_field = $filter('filter')(module.fields, { name: workflow.changed_field }, true)[0];
                    }

                    workflowModel.operation = {};

                    angular.forEach(workflow.operations_array, function (operation) {
                        workflowModel.operation[operation] = true;
                    });

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
                                case 'tag':
                                    var Items = filter.value.split('|');
                                    fieldValue = [];

                                    angular.forEach(Items, function (item) {
                                        fieldValue.push(item);
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
                                default :
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

                    if (workflow.send_notification) {
                        workflowModel.send_notification = {};
                        workflowModel.send_notification.subject = workflow.send_notification.subject;
                        workflowModel.send_notification.message = workflow.send_notification.message;

                        workflowModel.send_notification.recipients = [];
                        workflowModel.send_notification.cc = [];
                        workflowModel.send_notification.bcc = [];

                        if (workflow.send_notification.recipients_array) {
                            angular.forEach(workflow.send_notification.recipients_array, function (email) {
                                var recipient = {};

                                if (email === '[owner]') {
                                    var moduleObj = {};
                                    moduleObj.module = $filter('filter')($rootScope.modules, { name: 'users' }, true)[0];
                                    moduleObj.name = moduleObj.module['label_' + $rootScope.language + '_singular'];
                                    moduleObj.isSameModule = false;
                                    moduleObj.systemName = null;
                                    moduleObj.id = $filter('filter')(notificationFields, { name: moduleObj.name }, true)[0].id;


                                    workflowModel.send_notification_module = moduleObj;
                                    recipient.id = 0;
                                    recipient.email = '[owner]';
                                    recipient.full_name = $filter('translate')('Common.RecordOwner');
                                    workflowModel.send_notification.recipients.push(recipient);
                                }
                                else {
                                    var moduleObj = {};
                                    if (email.indexOf('@') > -1) {
                                        moduleObj.module = $filter('filter')($rootScope.modules, { name: 'users' }, true)[0];
                                        moduleObj.name = moduleObj.module['label_' + $rootScope.language + '_singular'];
                                        moduleObj.isSameModule = false;
                                        moduleObj.systemName = null;
                                        moduleObj.id = $filter('filter')(notificationFields, { name: moduleObj.name }, true)[0].id;
                                        workflowModel.send_notification_module = moduleObj;
                                        var recipientUser = $filter('filter')(workflow.send_notification.recipient_list, { email: email }, true)[0];
                                        recipient.id = recipientUser.id;
                                        recipient.email = recipientUser.email;
                                        recipient.full_name = recipientUser.full_name;
                                        workflowModel.send_notification.recipients.push(recipient);
                                    } else {
                                        if (email.indexOf('.') > -1) {
                                            var lookupField = email.split('.');
                                            var lookupModule = $filter('filter')(workflowModel.module.fields, { name: lookupField[0] }, true)[0];
                                            var recipientModule = $filter('filter')($rootScope.modules, { name: lookupModule.lookup_type }, true)[0];
                                            var paramModule = $filter('filter')(notificationFields, { systemName: lookupField[0] }, true)[0];
                                            moduleObj.module = recipientModule;
                                            moduleObj.name = lookupModule['label_' + $rootScope.language] + ' ' + '(' + moduleObj.module['label_' + $rootScope.language + '_singular'] + ')';
                                            moduleObj.isSameModule = paramModule.isSameModule;
                                            moduleObj.systemName = lookupField[0];
                                            moduleObj.id = paramModule.id;
                                            workflowModel.send_notification_module = moduleObj;
                                            workflowModel.send_notification.customRecipient = $filter('filter')(recipientModule.fields, { name: lookupField[1] }, true)[0];
                                        }
                                        else {
                                            moduleObj.module = workflow.module;
                                            moduleObj.name = moduleObj.module['label_' + $rootScope.language + '_singular'];
                                            moduleObj.isSameModule = true;
                                            moduleObj.systemName = null;
                                            moduleObj.id = $filter('filter')(notificationFields, { name: moduleObj.name }, true)[0].id;
                                            workflowModel.send_notification_module = moduleObj;
                                            workflowModel.send_notification.customRecipient = $filter('filter')(workflow.module.fields, { name: email }, true)[0];
                                        }
                                    }

                                }


                            });
                        }

                        if (workflow.send_notification.cc_array) {
                            angular.forEach(workflow.send_notification.cc_array, function (email) {
                                var ccObj = {};

                                if (email === '[owner]') {
                                    var moduleObj = {};
                                    moduleObj.module = $filter('filter')($rootScope.modules, { name: 'users' }, true)[0];
                                    moduleObj.name = moduleObj.module['label_' + $rootScope.language + '_singular'];
                                    moduleObj.isSameModule = false;
                                    moduleObj.systemName = null;
                                    moduleObj.id = $filter('filter')(notificationFields, { name: moduleObj.name }, true)[0].id;

                                    workflowModel.send_notification_ccmodule = moduleObj;
                                    ccObj.id = 0;
                                    ccObj.email = '[owner]';
                                    ccObj.full_name = $filter('translate')('Common.RecordOwner');
                                    workflowModel.send_notification.cc.push(ccObj);
                                }
                                else {
                                    var moduleObj = {};
                                    if (email.indexOf('@') > -1) {
                                        moduleObj.module = $filter('filter')($rootScope.modules, { name: 'users' }, true)[0];
                                        moduleObj.name = moduleObj.module['label_' + $rootScope.language + '_singular'];
                                        moduleObj.isSameModule = false;
                                        moduleObj.systemName = null;
                                        moduleObj.id = $filter('filter')(notificationFields, { name: moduleObj.name }, true)[0].id;
                                        workflowModel.send_notification_ccmodule = moduleObj;
                                        var ccUser = $filter('filter')(workflow.send_notification.cc_list, { email: email }, true)[0];

                                        ccObj.id = ccUser.id;
                                        ccObj.email = ccUser.email;
                                        ccObj.full_name = ccUser.full_name;
                                        workflowModel.send_notification.cc.push(ccObj)
                                    } else {
                                        if (email.indexOf('.') > -1) {
                                            var lookupField = email.split('.');
                                            var lookupModule = $filter('filter')(workflowModel.module.fields, { name: lookupField[0] }, true)[0];
                                            var ccModule = $filter('filter')($rootScope.modules, { name: lookupModule.lookup_type }, true)[0];
                                            var paramModule = $filter('filter')(notificationFields, { systemName: lookupField[0] }, true)[0];
                                            moduleObj.module = ccModule;
                                            moduleObj.name = lookupModule['label_' + $rootScope.language] + ' ' + '(' + moduleObj.module['label_' + $rootScope.language + '_singular'] + ')';
                                            moduleObj.isSameModule = paramModule.isSameModule;
                                            moduleObj.systemName = lookupField[0];
                                            moduleObj.id = paramModule.id;
                                            workflowModel.send_notification_ccmodule = moduleObj;
                                            workflowModel.send_notification.customCC = $filter('filter')(ccModule.fields, { name: lookupField[1] }, true)[0];
                                        }
                                        else {
                                            moduleObj.module = workflow.module;
                                            moduleObj.name = moduleObj.module['label_' + $rootScope.language + '_singular'];
                                            moduleObj.isSameModule = true;
                                            moduleObj.systemName = null;
                                            moduleObj.id = $filter('filter')(notificationFields, { name: moduleObj.name }, true)[0].id;
                                            workflowModel.send_notification_ccmodule = moduleObj;
                                            workflowModel.send_notification.customCC = $filter('filter')(workflow.module.fields, { name: email }, true)[0];
                                        }
                                    }

                                }


                            });
                        }

                        if (workflow.send_notification.bcc_array) {
                            angular.forEach(workflow.send_notification.bcc_array, function (email) {
                                var bccObj = {};

                                if (email === '[owner]') {
                                    var moduleObj = {};
                                    moduleObj.module = $filter('filter')($rootScope.modules, { name: 'users' }, true)[0];
                                    moduleObj.name = moduleObj.module['label_' + $rootScope.language + '_singular'];
                                    moduleObj.isSameModule = false;
                                    moduleObj.systemName = null;
                                    moduleObj.id = $filter('filter')(notificationFields, { name: moduleObj.name }, true)[0].id;

                                    workflowModel.send_notification_bccmodule = moduleObj;
                                    bccObj.id = 0;
                                    bccObj.email = '[owner]';
                                    bccObj.full_name = $filter('translate')('Common.RecordOwner');
                                    workflowModel.send_notification.bcc.push(bccObj);
                                }
                                else {
                                    var moduleObj = {};
                                    if (email.indexOf('@') > -1) {
                                        moduleObj.module = $filter('filter')($rootScope.modules, { name: 'users' }, true)[0];
                                        moduleObj.name = moduleObj.module['label_' + $rootScope.language + '_singular'];
                                        moduleObj.isSameModule = false;
                                        moduleObj.systemName = null;
                                        moduleObj.id = $filter('filter')(notificationFields, { name: moduleObj.name }, true)[0].id;
                                        workflowModel.send_notification_bccmodule = moduleObj;
                                        var bccUser = $filter('filter')(workflow.send_notification.bcc_list, { email: email }, true)[0];

                                        bccObj.id = bccUser.id;
                                        bccObj.email = bccUser.email;
                                        bccObj.full_name = bccUser.full_name;
                                        workflowModel.send_notification.bcc.push(bccObj);
                                    } else {
                                        if (email.indexOf('.') > -1) {
                                            var lookupField = email.split('.');
                                            var lookupModule = $filter('filter')(workflowModel.module.fields, { name: lookupField[0] }, true)[0];
                                            var bccModule = $filter('filter')($rootScope.modules, { name: lookupModule.lookup_type }, true)[0];
                                            var paramModule = $filter('filter')(notificationFields, { systemName: lookupField[0] }, true)[0];
                                            moduleObj.module = bccModule;
                                            moduleObj.name = moduleObj.module['label_' + $rootScope.language + '_singular'];
                                            moduleObj.isSameModule = paramModule.isSameModule;
                                            moduleObj.systemName = paramModule.systemName;
                                            moduleObj.id = paramModule.id;
                                            workflowModel.send_notification_bccmodule = moduleObj;
                                            workflowModel.send_notification.customBcc = $filter('filter')(bccModule.fields, { name: lookupField[1] }, true)[0];
                                        }
                                        else {
                                            moduleObj.module = workflow.module;
                                            moduleObj.name = moduleObj.module['label_' + $rootScope.language + '_singular'];
                                            moduleObj.isSameModule = true;
                                            moduleObj.systemName = null;
                                            moduleObj.id = $filter('filter')(notificationFields, { name: moduleObj.name }, true)[0].id;

                                            workflowModel.send_notification_bccmodule = moduleObj;
                                            workflowModel.send_notification.customBcc = $filter('filter')(workflow.module.fields, { name: email }, true)[0];
                                        }
                                    }

                                }


                            });
                        }


                        if (workflow.send_notification.schedule != null || workflow.send_notification.schedule != undefined) {
                            var schedule = angular.copy(workflow.send_notification.schedule);

                            if (workflow.send_notification.schedule === 0)
                                schedule = 'now';

                            workflowModel.send_notification.schedule = $filter('filter')(scheduleItems, { value: schedule }, true)[0];
                        }
                    }

                    if (workflow.create_task) {
                        workflowModel.create_task = {};
                        workflowModel.create_task.owner = [];
                        workflowModel.create_task.subject = workflow.create_task.subject;
                        var dueDate = angular.copy(workflow.create_task.task_due_date);

                        if (workflow.create_task.owner === 0) {
                            var owner = {};
                            owner.id = 0;
                            owner.email = '[owner]';
                            owner.full_name = $filter('translate')('Common.RecordOwner');
                            workflowModel.create_task.owner.push(owner);
                        }
                        else {
                            workflowModel.create_task.owner.push(workflow.create_task.owner_user);
                        }

                        if (workflow.create_task.task_due_date === 0)
                            dueDate = 'now';

                        workflowModel.create_task.task_due_date = $filter('filter')(dueDateItems, { value: dueDate }, true)[0];

                        if (workflow.create_task.task_status)
                            workflowModel.create_task.task_status = $filter('filter')(picklistsActivity[taskFields.task_status.picklist_id], { id: workflow.create_task.task_status }, true)[0];

                        if (workflow.create_task.task_priority)
                            workflowModel.create_task.task_priority = $filter('filter')(picklistsActivity[taskFields.task_priority.picklist_id], { id: workflow.create_task.task_priority }, true)[0];

                        if (workflow.create_task.task_notification)
                            workflowModel.create_task.task_notification = $filter('filter')(picklistsActivity.yes_no, { id: workflow.create_task.task_notification }, true)[0];

                        if (workflow.create_task.description)
                            workflowModel.create_task.description = workflow.create_task.description;
                    }

                    if (workflow.field_update) {
                        workflowModel.field_update = {};

                        if (workflow.field_update.module.split(',').length > 1) {
                            workflowModel.field_update.updateOption = '2';
                            var firstModule = workflow.field_update.module.split(',')[0];
                            var secondModule = workflow.field_update.module.split(',')[1];
                            var firstModuleObj = {};
                            var secondModuleObj = {};

                            if (firstModule === module.name) {
                                firstModuleObj.module = module;
                                firstModuleObj.name = firstModuleObj.module['label_' + $rootScope.language + '_singular'];
                                firstModuleObj.isSameModule = true;
                                firstModuleObj.systemName = firstModule;
                                firstModuleObj.id = $filter('filter')(notificationFields, { name: firstModuleObj.name }, true)[0].id;
                            } else {
                                var firstMainModule = $filter('filter')(workflowModel.module.fields, { name: firstModule }, true)[0];
                                var firstCurrentModule = $filter('filter')($rootScope.modules, { name: firstMainModule.lookup_type }, true)[0];
                                var firstParamModule = $filter('filter')(dynamicFiledUpdatefields, { systemName: firstModule }, true)[0];
                                firstModuleObj.module = firstCurrentModule;
                                firstModuleObj.name = firstMainModule['label_' + $rootScope.language] + ' ' + '(' + firstModuleObj.module['label_' + $rootScope.language + '_singular'] + ')';
                                firstModuleObj.isSameModule = firstParamModule.isSameModule;
                                firstModuleObj.systemName = firstModule;
                                firstModuleObj.id = firstParamModule.id;
                            }

                            if (secondModule === module.name) {
                                secondModuleObj.module = module;
                                secondModuleObj.name = secondModuleObj.module['label_' + $rootScope.language + '_singular'];
                                secondModuleObj.isSameModule = true;
                                secondModuleObj.systemName = secondModule;
                                secondModuleObj.id = $filter('filter')(notificationFields, { name: secondModuleObj.name }, true)[0].id;
                            }
                            else {
                                var secondMainModule = $filter('filter')(workflowModel.module.fields, { name: secondModule }, true)[0];
                                var secondCurrentModule = $filter('filter')($rootScope.modules, { name: secondMainModule.lookup_type }, true)[0];
                                var secondParamModule = $filter('filter')(dynamicFiledUpdatefields, { systemName: secondModule }, true)[0];
                                secondModuleObj.module = secondCurrentModule;
                                secondModuleObj.name = secondMainModule['label_' + $rootScope.language] + ' ' + '(' + secondModuleObj.module['label_' + $rootScope.language + '_singular'] + ')';
                                secondModuleObj.isSameModule = secondParamModule.isSameModule;
                                secondModuleObj.systemName = secondModule;
                                secondModuleObj.id = secondParamModule.id;
                            }

                            workflowModel.field_update.firstModule = firstModuleObj;
                            workflowModel.field_update.first_field = $filter('filter')(firstModuleObj.module.fields, { name: workflow.field_update.value }, true)[0];
                            workflowModel.field_update.secondModule = secondModuleObj;
                            workflowModel.field_update.second_field = $filter('filter')(secondModuleObj.module.fields, { name: workflow.field_update.field }, true)[0];
                        } else {
                            workflowModel.field_update.updateOption = '1';
                            workflowModel.field_update.module = $filter('filter')($rootScope.modules, { name: workflow.field_update.module }, true)[0];
                            workflowModel.field_update.field = $filter('filter')(workflowModel.field_update.module.fields, { name: workflow.field_update.field }, true)[0];
                            if (workflowModel.field_update.field.data_type === 'multiselect') {
                                var picklistItems = workflow.field_update.value.split('|');
                                workflow.field_update.value = [];

                                angular.forEach(picklistItems, function (picklistLabel) {
                                    var picklist = $filter('filter')(picklistsModule[workflowModel.field_update.field.picklist_id], { labelStr: picklistLabel }, true)[0];

                                    if (picklist)
                                        workflow.field_update.value.push(picklist.labelStr);
                                });
                            }

                            if (workflowModel.field_update.field.data_type === 'tag') {
                                var itemValue = workflow.field_update.value.split('|');
                                workflow.field_update.value = [];
                                angular.forEach(itemValue, function (item) {
                                    workflow.field_update.value.push(item);
                                });
                            }


                            var updateFieldRecordFake = {};
                            updateFieldRecordFake[workflow.field_update.field] = workflow.field_update.value;
                            ModuleService.processRecordField(updateFieldRecordFake, workflowModel.field_update.field, picklistUpdateModule);
                            workflowModel.field_update.value = updateFieldRecordFake[workflow.field_update.field];

                            if (workflowModel.field_update.field.data_type === 'lookup') {
                                // if (workflowModel.field_update.field.lookup_type === 'users')
                                //     workflowModel.field_update.value = [workflow.field_update.value];
                                // else
                                workflowModel.field_update.value = workflow.field_update.value;
                            }
                        }
                    }

                    if (workflow.web_hook) {
                        workflowModel.webHook = {};
                        workflowModel.webHook.callbackUrl = workflow.web_hook.callback_url;
                        workflowModel.webHook.methodType = workflow.web_hook.method_type;
                        workflowModel.webHook.parameters = workflow.web_hook.parameters;
                        //params
                    }

                    return workflowModel;
                },

                prepareWorkflow: function (workflowModel, filters, updateFieldValue) {
                    var workflow = {};
                    workflow.module_id = workflowModel.module.id;
                    workflow.name = workflowModel.name;
                    workflow.frequency = workflowModel.frequency;
                    workflow.active = workflowModel.active;
                    workflow.delete_logs = workflowModel.delete_logs;
                    workflow.process_filter = workflowModel.processFilter;

                    if (workflowModel.changed_field)
                        workflow.changed_field = workflowModel.changed_field.name;
                    else workflow.changed_field = null;

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
                                if (field.data_type === 'tag') {
                                    var value = '';
                                    angular.forEach(filter.value, function (item) {
                                        value += item.text + ',';
                                    });
                                }

                                if (field.data_type === 'lookup' && field.lookup_type != 'users')
                                    filter.value = filter.value.id;

                                if (field.data_type === 'lookup' && field.lookup_type === 'users')
                                    filter.value = filter.value[0].id;

                                if (field.data_type === 'checkbox')
                                    filter.value = filter.value.system_code;

                                if (field.data_type === 'date') {
                                    if (filter.value === undefined || filter.value === null)
                                        return;

                                    filter.value = new Date(filter.value);
                                }
                            }
                            else {
                                filter.value = '-';
                            }

                            workflow.filters.push(filter);
                        });
                    }

                    workflow.actions = {};

                    if (workflowModel.send_notification) {
                        var sendNotification = {};
                        sendNotification.subject = workflowModel.send_notification.subject;
                        sendNotification.message = workflowModel.send_notification.message;

                        sendNotification.cc = [];
                        if (workflowModel.send_notification.cc) {
                            angular.forEach(workflowModel.send_notification.cc, function (user) {
                                sendNotification.cc.push(user.email);
                            });
                        } else if (workflowModel.send_notification_ccmodule && workflowModel.send_notification.customCC) {
                            if (workflowModel.send_notification_ccmodule.module.name === workflowModel.module.name && workflowModel.send_notification_ccmodule.isSameModule) {
                                sendNotification.cc.push(workflowModel.send_notification.customCC.name);
                            }
                            else if (workflowModel.send_notification_ccmodule.module.name === workflowModel.module.name && !workflowModel.send_notification_ccmodule.isSameModule) {
                                var sameModuleLookupsCC = $filter('filter')(workflowModel.module.fields, { lookup_type: workflowModel.send_notification_ccmodule.module.name }, true);
                                sendNotification.cc.push($filter('filter')(sameModuleLookupsCC, { name: workflowModel.send_notification_ccmodule.systemName }, true)[0].name + '.' + workflowModel.send_notification.customCC.name);
                            }
                            else {
                                var lookupsCC = $filter('filter')(workflowModel.module.fields, { lookup_type: workflowModel.send_notification_ccmodule.module.name }, true);
                                sendNotification.cc.push($filter('filter')(lookupsCC, { name: workflowModel.send_notification_ccmodule.systemName }, true)[0].name + '.' + workflowModel.send_notification.customCC.name);
                            }
                        }

                        sendNotification.bcc = [];
                        if (workflowModel.send_notification.bcc) {
                            angular.forEach(workflowModel.send_notification.bcc, function (user) {
                                sendNotification.bcc.push(user.email);
                            });
                        } else if (workflowModel.send_notification_bccmodule && workflowModel.send_notification.customBcc) {
                            if (workflowModel.send_notification_bccmodule.module.name === workflowModel.module.name && workflowModel.send_notification_bccmodule.isSameModule) {
                                sendNotification.bcc.push(workflowModel.send_notification.customBcc.name);
                            }
                            else if (workflowModel.send_notification_bccmodule.module.name === workflowModel.module.name && !workflowModel.send_notification_bccmodule.isSameModule) {
                                var sameModuleLookupsBcc = $filter('filter')(workflowModel.module.fields, { lookup_type: workflowModel.send_notification_bccmodule.module.name }, true);
                                sendNotification.bcc.push($filter('filter')(sameModuleLookupsBcc, { name: workflowModel.send_notification_bccmodule.systemName }, true)[0].name + '.' + workflowModel.send_notification.customBcc.name);
                            }
                            else {
                                var lookupsBcc = $filter('filter')(workflowModel.module.fields, { lookup_type: workflowModel.send_notification_bccmodule.module.name }, true);
                                sendNotification.bcc.push($filter('filter')(lookupsBcc, { name: workflowModel.send_notification_bccmodule.systemName }, true)[0].name + '.' + workflowModel.send_notification.customBcc.name);
                            }
                        }

                        sendNotification.recipients = [];

                        if (workflowModel.send_notification.recipients) {
                            angular.forEach(workflowModel.send_notification.recipients, function (user) {
                                sendNotification.recipients.push(user.email);
                            });
                        } else {
                            if (workflowModel.send_notification_module.module.name === workflowModel.module.name && workflowModel.send_notification_module.isSameModule) {
                                sendNotification.recipients.push(workflowModel.send_notification.customRecipient.name);
                            }
                            else if (workflowModel.send_notification_module.module.name === workflowModel.module.name && !workflowModel.send_notification_module.isSameModule) {
                                var sameModuleLookups = $filter('filter')(workflowModel.module.fields, { lookup_type: workflowModel.send_notification_module.module.name }, true);
                                sendNotification.recipients.push($filter('filter')(sameModuleLookups, { name: workflowModel.send_notification_module.systemName }, true)[0].name + '.' + workflowModel.send_notification.customRecipient.name);
                            }
                            else {
                                var lookups = $filter('filter')(workflowModel.module.fields, { lookup_type: workflowModel.send_notification_module.module.name }, true);
                                sendNotification.recipients.push($filter('filter')(lookups, { name: workflowModel.send_notification_module.systemName }, true)[0].name + '.' + workflowModel.send_notification.customRecipient.name);
                            }
                        }

                        if (workflowModel.send_notification.schedule) {
                            if (workflowModel.send_notification.schedule.value === 'now')
                                sendNotification.schedule = 0;
                            else
                                sendNotification.schedule = workflowModel.send_notification.schedule.value;
                        }

                        workflow.actions.send_notification = sendNotification;
                    }

                    if (workflowModel.create_task) {
                        var createTask = {};
                        createTask.owner = workflowModel.create_task.owner[0].id;
                        createTask.subject = workflowModel.create_task.subject;

                        if (workflowModel.create_task.task_due_date.value === 'now')
                            createTask.task_due_date = 0;
                        else
                            createTask.task_due_date = workflowModel.create_task.task_due_date.value;

                        if (workflowModel.create_task.task_status)
                            createTask.task_status = workflowModel.create_task.task_status.id;

                        if (workflowModel.create_task.task_priority)
                            createTask.task_priority = workflowModel.create_task.task_priority.id;

                        if (workflowModel.create_task.task_notification)
                            createTask.task_notification = workflowModel.create_task.task_notification.id;

                        if (workflowModel.create_task.description)
                            createTask.description = workflowModel.create_task.description;

                        workflow.actions.create_task = createTask;
                    }

                    if (workflowModel.field_update) {
                        var fieldUpdate = {};
                        if (workflowModel.field_update.updateOption === '1') {
                            fieldUpdate.module = workflowModel.field_update.module.name;
                            fieldUpdate.field = workflowModel.field_update.field.name;
                            fieldUpdate.value = updateFieldValue;
                        } else {
                            fieldUpdate.module = workflowModel.field_update.firstModule.systemName + "," + workflowModel.field_update.secondModule.systemName;
                            fieldUpdate.field = workflowModel.field_update.second_field.name;
                            fieldUpdate.value = workflowModel.field_update.first_field.name;
                        }


                        workflow.actions.field_update = fieldUpdate;
                    }

                    if (workflowModel.webHook) {
                        var webHook = {};
                        webHook.callback_url = workflowModel.webHook.callbackUrl;
                        webHook.method_type = workflowModel.webHook.methodType;

                        var hookArray = [];
                        angular.forEach(workflowModel.webHook.hookParameters, function (hookParameter) {
                            var moduleName;
                            if (workflowModel.module.name != hookParameter.selectedModule.name)
                                moduleName = $filter('filter')(workflowModel.module.fields, { lookup_type: hookParameter.selectedModule.name }, true)[0].name;
                            else
                                moduleName = hookParameter.selectedModule.name;

                            var parameterString = hookParameter.parameterName + "|" + moduleName + "|" + hookParameter.selectedField.name;
                            hookArray.push(parameterString);
                        });

                        if (hookArray.length > 0) {
                            webHook.parameters = hookArray.toString();
                        }

                        workflow.actions.web_hook = webHook;
                    }

                    return workflow;
                }
            };
        }]);