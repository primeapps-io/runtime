'use strict';

angular.module('primeapps')

    .factory('WorkflowsService', ['$rootScope', '$http', 'config', '$filter', '$q', 'helper', 'defaultLabels', '$cache', 'dataTypes', 'systemFields', 'operators',
        function ($rootScope, $http, config, $filter, $q, helper, defaultLabels, $cache, dataTypes, systemFields, operators) {
            return {
                find: function (model) {
                    return $http.post(config.apiUrl + 'bpm/find/', model);
                },

                count: function () {
                    return $http.get(config.apiUrl + 'bpm/count/');
                },

                get: function (id) {
                    return $http.get(config.apiUrl + 'bpm/get/' + id);
                },

                getByCode: function (code) {
                    return $http.get(config.apiUrl + 'bpm/get/' + code);
                },

                getAll: function () {
                    return $http.get(config.apiUrl + 'bpm/get_all');
                },

                create: function (bpmWorkflow) {
                    return $http.post(config.apiUrl + 'bpm/create', bpmWorkflow);
                },

                update: function (bpmWorkflow) {
                    return $http.put(config.apiUrl + 'bpm/update/' + bpmWorkflow.id, bpmWorkflow);
                },

                delete: function (id) {
                    return $http.delete(config.apiUrl + 'bpm/delete/' + id);
                },

                processWorkflow: function (workflow, module, picklistsModule, filters, scheduleItems, dueDateItems, picklistsActivity, taskFields, picklistUpdateModule) {
                    var workflowModel = {};
                    workflowModel.id = workflow.id;
                    workflowModel.created_by = workflow.created_by;
                    workflowModel.updated_by = workflow.updated_by;
                    workflowModel.created_at = workflow.created_at;
                    workflowModel.updated_at = workflow.updated_at;
                    workflowModel.deleted = workflow.deleted;
                    workflowModel.name = workflow.name;
                    workflowModel.code = workflow.code;
                    workflowModel.module = module;
                    workflowModel.active = workflow.active;
                    workflowModel.frequency = workflow.frequency || 'one_time';
                    workflowModel.operation = {};
                    window.diagramData = angular.fromJson(workflow.diagram_json);

                    // window.myDiagram.startTransaction("LoadModel");
                    //window.myDiagram.model = new go.GraphLinksModel(diagramData.nodeDataArray, diagramData.linkDataArray);
                    //window.myDiagram.requestUpdate();
                    // window.myDiagram.commitTransaction("LoadModel");

                    // angular.forEach(window.diagramData.nodeDataArray, function (node) {
                    //     if (node.ngModelName === 'send_notification')
                    //         workflow.send_notification = node.data.send_notification;
                    //     else if (node.ngModelName === 'create_task') {
                    //         workflow.create_task = {};
                    //         workflow.create_task = node.data.create_task;
                    //     }
                    //     else if (node.ngModelName === 'web_hook') {
                    //         workflow.web_hook = {};
                    //         workflow.web_hook = node.data.web_hook;
                    //     }
                    //     else if (node.ngModelName === 'field_update') {
                    //         workflow.field_update = {};
                    //         workflow.field_update.module_id = node.data.module_id;
                    //         workflow.field_update.field_id = node.data.field_id;
                    //         workflow.field_update.value = node.data.value;
                    //     }
                    // });
                    angular.forEach(workflow.record_operations.split(','), function (operation) {
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
                                    fieldValue = $filter('filter')(picklistsModule[field.picklist_id], { id: filter.value }, true)[0];
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
                            }

                            filters[i] = filter;
                        }
                    }

                    // if (workflow.send_notification) {
                    //     workflowModel.send_notification = {};
                    //     workflowModel.send_notification.subject = workflow.send_notification.subject;
                    //     workflowModel.send_notification.message = workflow.send_notification.message;
                    //     //workflowModel.send_notification.recipients = [];
                    //
                    //     // angular.forEach(workflow.send_notification.recipients, function (email) {
                    //     //     var recipient = {};
                    //     //
                    //     //     // if (email === '[owner]') {
                    //     //     //     recipient.id = 0;
                    //     //     //     recipient.email = '[owner]';
                    //     //     //     recipient.full_name = $filter('translate')('Common.RecordOwner');
                    //     //     // }
                    //     //     // else {
                    //     //     //     var recipientUser = $filter('filter')(workflow.send_notification.recipients, { email: email }, true)[0];
                    //     //     //
                    //     //     //     recipient.id = recipientUser.id;
                    //     //     //     recipient.email = recipientUser.email;
                    //     //     //     recipient.full_name = recipientUser.full_name;
                    //     //     // }
                    //     //
                    //     //     workflowModel.send_notification.recipients.push(recipient)
                    //     // });
                    //     workflowModel.send_notification.recipients = workflow.send_notification.recipients;
                    //
                    //     if (workflow.send_notification.schedule != null || workflow.send_notification.schedule != undefined) {
                    //         var schedule = angular.copy(workflow.send_notification.schedule);
                    //
                    //         if (workflow.send_notification.schedule === 0)
                    //             schedule = 'now';
                    //
                    //         workflowModel.send_notification.schedule = $filter('filter')(scheduleItems, { value: schedule }, true)[0];
                    //     }
                    // }
                    //
                    // if (workflow.create_task) {
                    //     workflowModel.create_task = {};
                    //     workflowModel.create_task.owner = [];
                    //     workflowModel.create_task.subject = workflow.create_task.subject;
                    //     var dueDate = angular.copy(workflow.create_task.task_due_date);
                    //
                    //     if (workflow.create_task.owner === 0) {
                    //         var owner = {};
                    //         owner.id = 0;
                    //         owner.email = '[owner]';
                    //         owner.full_name = $filter('translate')('Common.RecordOwner');
                    //         workflowModel.create_task.owner.push(owner);
                    //     }
                    //     else {
                    //         workflowModel.create_task.owner.push(workflow.create_task.owner_user);
                    //     }
                    //
                    //     if (workflow.create_task.task_due_date === 0)
                    //         dueDate = 'now';
                    //
                    //     workflowModel.create_task.task_due_date = $filter('filter')(dueDateItems, { value: dueDate }, true)[0];
                    //
                    //     if (workflow.create_task.task_status)
                    //         workflowModel.create_task.task_status = $filter('filter')(picklistsActivity[taskFields.task_status.picklist_id], { id: workflow.create_task.task_status }, true)[0];
                    //
                    //     if (workflow.create_task.task_priority)
                    //         workflowModel.create_task.task_priority = $filter('filter')(picklistsActivity[taskFields.task_priority.picklist_id], { id: workflow.create_task.task_priority }, true)[0];
                    //
                    //     if (workflow.create_task.task_notification)
                    //         workflowModel.create_task.task_notification = $filter('filter')(picklistsActivity.yes_no, { id: workflow.create_task.task_notification }, true)[0];
                    //
                    //     if (workflow.create_task.description)
                    //         workflowModel.create_task.description = workflow.create_task.description;
                    // }
                    //
                    // if (workflow.field_update) {
                    //     workflowModel.field_update = {};
                    //     workflowModel.field_update.module = $filter('filter')($rootScope.modules, { name: workflow.field_update.module }, true)[0];
                    //     workflowModel.field_update.field = $filter('filter')(workflowModel.field_update.module.fields, { name: workflow.field_update.field }, true)[0];
                    //
                    //     if (workflowModel.field_update.field.data_type === 'multiselect')
                    //         workflow.field_update.value = workflow.field_update.value.split(',');
                    //
                    //     var updateFieldRecordFake = {};
                    //     updateFieldRecordFake[workflow.field_update.field] = workflow.field_update.value;
                    //     ModuleService.processRecordField(updateFieldRecordFake, workflowModel.field_update.field, picklistUpdateModule);
                    //     workflowModel.field_update.value = updateFieldRecordFake[workflow.field_update.field];
                    //
                    //     if (workflowModel.field_update.field.data_type === 'lookup') {
                    //         // if (workflowModel.field_update.field.lookup_type === 'users')
                    //         //     workflowModel.field_update.value = [workflow.field_update.value];
                    //         // else
                    //         workflowModel.field_update.value = workflow.field_update.value;
                    //     }
                    // }
                    //
                    // if (workflow.web_hook) {
                    //     var webHook = {};
                    //     webHook.callback_url = workflowModel.webHook.callbackUrl;
                    //     webHook.method_type = workflowModel.webHook.methodType;
                    //
                    //     var hookArray = [];
                    //     angular.forEach(workflowModel.webHook.hookParameters, function (hookParameter) {
                    //         var moduleName;
                    //         if (workflowModel.module.name !== hookParameter.selectedModule.name)
                    //             moduleName = $filter('filter')(workflowModel.module.fields, { lookup_type: hookParameter.selectedModule.name }, true)[0].name;
                    //         else
                    //             moduleName = hookParameter.selectedModule.name;
                    //
                    //         var parameterString = hookParameter.parameterName + "|" + moduleName + "|" + hookParameter.selectedField.name;
                    //         hookArray.push(parameterString);
                    //     });
                    //
                    //     if (hookArray.length > 0) {
                    //         webHook.parameters = hookArray.toString();
                    //     }
                    //
                    //     workflow.actions.web_hook = webHook;
                    // }


                    return workflowModel;
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
                        if (field.data_type === 'lookup' && field.lookup_type !== 'relation') {
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

                        if (field.name && field.data_type !== 'lookup') {
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
                }


            };
        }]);

angular.module('primeapps')

    .constant('defaultLabels', {
        DefaultModuleNameEn: 'Module',
        DefaultModuleNameTr: 'Modül',
        DefaultSectionNameEn: 'Section',
        DefaultSectionNameTr: 'Bölüm',
        SystemInfoSectionNameEn: 'System Information',
        SystemInfoSectionNameTr: 'Sistem Bilgisi',
        DefaultFieldNameEn: 'Name',
        DefaultFieldNameTr: 'İsim',
        UserLookupFieldEn: 'User',
        UserLookupFieldTr: 'Kullanıcı',
        ProfileLookupFieldEn: 'Profile',
        ProfileLookupFieldTr: 'Profil',
        RoleLookupFieldEn: 'Role',
        RoleLookupFieldTr: 'Rol',
        CreatedByFieldEn: 'Created by',
        CreatedByFieldTr: 'Oluşturan',
        UpdatedByFieldEn: 'Updated by',
        UpdatedByFieldTr: 'Güncelleyen',
        CreatedAtFieldEn: 'Created at',
        CreatedAtFieldTr: 'Oluşturulma Tarihi',
        UpdatedAtFieldEn: 'Updated at',
        UpdatedAtFieldTr: 'Güncellenme Tarihi',
        DefaultPicklistItemEn: 'Option',
        DefaultPicklistItemTr: 'Seçenek',
        DataTypeCombinationEn: 'Combination',
        DataTypeCombinationTr: 'Birleşim',
        DataTypeCalculatedEn: 'Calculated',
        DataTypeCalculatedTr: 'Hesaplama'
    });