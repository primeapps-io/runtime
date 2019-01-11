'use strict';

angular.module('primeapps')

    .factory('RulesService', ['$rootScope', '$http', 'config', '$filter', '$q', 'helper',
        function ($rootScope, $http, config, $filter, $q, helper, ) {
            return {
                find: function (model) {
                    return $http.post(config.apiUrl + 'rule/find/', model);
                },

                count: function () {
                    return $http.get(config.apiUrl + 'rule/count/');
                },

                get: function (id) {
                    return $http.get(config.apiUrl + 'rule/get/' + id);
                },

                getAll: function () {
                    return $http.get(config.apiUrl + 'rule/get_all/');
                },

                create: function (model) {
                    return $http.post(config.apiUrl + 'rule/create/', model);
                },

                update: function (model) {
                    return $http.put(config.apiUrl + 'rule/update/', model);
                },

                delete: function (id) {
                    return $http.delete(config.apiUrl + 'rule/delete/' + id);
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

