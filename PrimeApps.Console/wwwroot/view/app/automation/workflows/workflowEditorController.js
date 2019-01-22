'use strict';

angular.module('primeapps')

    .controller('WorkflowEditorController', ['$rootScope', '$scope', '$location', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'WorkflowsService', 'LayoutService', 'ModuleService', '$http', 'config', 'operators',
        function ($rootScope, $scope, $location, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService, WorkflowsService, LayoutService, ModuleService, $http, config, operators) {
            $scope.loading = true;
            $scope.saving = false;
            $scope.$parent.loadingFilter = false;
            $scope.id = $location.search().id;
            $scope.workflowModel = {};
            $scope.$parent.menuTopTitle = "Automation";
            $scope.$parent.activeMenu = 'automation';
            $scope.$parent.activeMenuItem = 'workflowEditor';
            $rootScope.subtoggleClass = 'full-toggled2';
            $scope.scheduleItems = WorkflowsService.getScheduleItems();
            $scope.dueDateItems = WorkflowsService.getDueDateItems();
            $rootScope.breadcrumblist[2].title = 'Workflows';

            //BPM element menu loading start
            angular.element(function () {
                window.initFunc();
            });

            $scope.triggerBpm = function () {
                angular.element(function () {
                    jQuery("#accordion").accordion({
                        activate: function (event, ui) {
                            window.myPaletteLevel1.requestUpdate();
                            window.myPaletteLevel2.requestUpdate();
                        }
                    });
                    if ($scope.id) {
                        WorkflowsService.get($scope.id)
                            .then(function (response) {
                                if (response) {
                                    var tempJson = JSON.parse(response.data.diagram_json);
                                    window.myDiagram.model.nodeDataArray = tempJson.nodeDataArray;
                                    window.myDiagram.model.linkDataArray = tempJson.linkDataArray;
                                    window.myDiagram.requestUpdate();
                                    window.myPaletteLevel1.requestUpdate();
                                }
                            });
                    }
                    else {
                        window.myDiagram.requestUpdate();
                        window.myPaletteLevel1.requestUpdate();
                    }
                });
            }

            $scope.triggerBpm();
            //BPM element menu loading end

            // BPM click Event and RightSide Menu Controller Start
            $scope.toogleSideMenu = function () {
                if ($scope.currentObj.subject) {
                    var node = $scope.currentObj.subject.part.data;

                    if (node) {
                        $scope.SelectedNodeItem = node;

                        if (node.data && node.ngModelName) {
                            $scope.workflowModel[node.ngModelName] = {}; //clear for new value

                            switch (node.ngModelName) {
                                case 'start':
                                    break;
                                case 'field_update':
                                    $scope.workflowModel[node.ngModelName] = node.data[node.ngModelName];
                                    //$scope.getUpdatableModules();
                                    break;
                                case 'webHook':
                                    $scope.workflowModel[node.ngModelName] = node.data[node.ngModelName];
                                    $scope.hookParameters = node.data[node.ngModelName].Parameters;
                                    break;
                                case 'send_notification':
                                    $scope.workflowModel[node.ngModelName] = node.data[node.ngModelName];
                                    $scope.workflowModel.send_notification_module = node.data[node.ngModelName].send_notification_module;
                                    $scope.workflowModel.send_notification_ccmodule = node.data[node.ngModelName].send_notification_ccmodule;
                                    $scope.workflowModel.send_notification_bccmodule = node.data[node.ngModelName].send_notification_bccmodule;
                                    break;
                                case 'data_read':
                                    $scope.workflowModel[node.ngModelName] = node.data[node.ngModelName];
                                    break;
                                case 'function':
                                    $scope.workflowModel[node.ngModelName] = node.data[node.ngModelName];

                                    if ($scope.workflowModel[node.ngModelName]['name'])
                                        $scope.workflowModel[node.ngModelName]['name'] = { name: $scope.workflowModel[node.ngModelName]['name'] };

                                    if ($scope.workflowModel[node.ngModelName]['methodType'])
                                        $scope.functionType = $scope.workflowModel[node.ngModelName]['methodType'] === 'post';
                                    break;
                                default:
                                    $scope.workflowModel[node.ngModelName] = node.data[node.ngModelName];
                                    break;
                            }
                        }
                        else if (node.isDefault === false) { //FOR Conditional Link
                            if (node.data)
                                $scope.workflowModel["condition"] = node.data.condition;
                            else
                                $scope.workflowModel["condition"] = null;
                        }
                        else {
                            $scope.workflowModel = {};

                            if (node.ngModelName === 'webHook') {
                                setWebHookModules();
                            }
                        }
                    }

                    if (node.sidebar)
                        $scope.showFormModal();
                }
                else
                    $scope.SelectedNodeItem = null;


            };
            // BPM click Event and RightSide Menu Controller End


            //Modal Start
            $scope.showFormModal = function () {


                $scope.formModal = $scope.formModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/automation/workflows/workflowModal.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.formModal.$promise.then(function () {
                    $scope.formModal.show();
                });

            };

            $scope.cancel = function () {
                angular.forEach($scope.currentRelation, function (value, key) {
                    $scope.currentRelation[key] = $scope.currentRelationState[key];
                });

                $scope.formModal.hide();
            };

            $scope.deleteSelectedItem = function () {


            };

            //Modal End

            $scope.selectModule = function () {
                $scope.loadingFilter = true;

                ModuleService.getModuleFields($scope.workflowModel.module.name)
                    .then(function (response) {
                        if (response) {
                            $scope.workflowModel.module.fields = response.data;
                        }

                        var module = angular.copy($scope.workflowModel.module);
                        $scope.module = ModuleService.getFieldsOperator(module, $rootScope.appModules, 0);

                        //TODO 
                        //if ($filter('filter')($rootScope.approvalProcesses, { module_id: $scope.module.id }, true)[0]) {
                        //    $scope.showProcessFilter = true;
                        //    $scope.workflowModel.processFilter = 'all';
                        //}
                        //else {
                        $scope.workflowModel.processFilter = 'none';
                        $scope.showProcessFilter = false;
                        //}

                        angular.forEach($scope.module.fields, function (field) {
                            if (field.data_type === 'lookup') {
                                field.operators = [];
                                field.operators.push(operators.equals);
                                field.operators.push(operators.not_equal);
                                field.operators.push(operators.empty);
                                field.operators.push(operators.not_empty);
                            }
                        });

                        ModuleService.getPickItemsLists($scope.module)
                            .then(function (picklists) {
                                $scope.modulePicklists = picklists;
                                $scope.filters = [];

                                for (var i = 0; i < 5; i++) {
                                    var filter = {};
                                    filter.id = i;
                                    filter.field = null;
                                    filter.operator = null;
                                    filter.value = null;
                                    filter.no = i + 1;

                                    $scope.filters.push(filter);
                                }

                                $scope.loadingFilter = false;
                            });

                        $scope.getUpdatableModules();
                        $scope.getSendNotificationUpdatableModules();
                        $scope.getDynamicFieldUpdateModules();
                        setWebHookModules();
                    });
            };

            $scope.getTagTextRaw = function (item, type) {
                if (item.name.indexOf("seperator") < 0) {
                    if (type === 'input')
                        return '{' + item.name + '}';
                    else
                        return '<i style="color:#0f1015;font-style:normal">' + '{' + item.name + '}' + '</i>';
                }
                return '';
            };

            $scope.operatorChanged = function (field, index) {
                var filterListItem = $scope.filters[index];

                if (!filterListItem || !filterListItem.operator)
                    return;

                if (filterListItem.operator.name === 'empty' || filterListItem.operator.name === 'not_empty') {
                    filterListItem.value = null;
                    filterListItem.disabled = true;
                }
                else {
                    filterListItem.disabled = false;
                }
            };

            $scope.searchTags = function (term) {
                if (!$scope.moduleFields)
                    $scope.moduleFields = RulesService.getFields($scope.module);

                var tagsList = [];
                angular.forEach($scope.moduleFields, function (item) {
                    if (item.name === "seperator")
                        return;
                    if (item.label.indexOf(term) >= 0) {
                        tagsList.push(item);
                    }
                });


                $scope.tags = tagsList;
                return tagsList;
            };

            var getFilterValue = function (filter) {
                var filterValue = '';

                if (filter.field.data_type === 'lookup' && filter.field.lookup_type === 'users') {
                    filterValue = filter.value[0].full_name;
                }
                else if (filter.field.data_type === 'lookup' && filter.field.lookup_type !== 'users') {
                    filterValue = filter.value.primary_value;
                }
                else if (filter.field.data_type === 'picklist') {
                    filterValue = filter.value.labelStr;
                }
                else if (filter.field.data_type === 'multiselect') {
                    filterValue = '';

                    angular.forEach(filter.value, function (picklistItem) {
                        filterValue += picklistItem.labelStr + '; ';
                    });

                    filterValue = filterValue.slice(0, -2);
                }
                else if (filter.field.data_type === 'tag') {
                    filterValue = '';

                    angular.forEach(filter.value, function (item) {
                        filterValue += item.text + '; ';
                    });
                }
                else if (filter.field.data_type === 'checkbox') {
                    filterValue = filter.value.label[$scope.language];
                }
                else {
                    ModuleService.formatFieldValue(filter.field, filter.value, $scope.picklistsActivity);
                    filterValue = angular.copy(filter.field.valueFormatted);
                }

                return filterValue;
            };

            $scope.validate = function (tabClick) {
                if (!$scope.workflowForm)
                    $scope.workflowForm = tabClick;

                $scope.workflowForm.$submitted = true;
                $scope.validateOperations(tabClick);

                if (!$scope.workflowForm.workflowName.$valid || !$scope.workflowForm.module.$valid || !$scope.workflowForm.operation.$valid)
                    return false;

                if ($scope.workflowModel.changed_field_checkbox && !$scope.workflowForm.changed_field.$valid)
                    return false;
                //TODO 
                return $scope.validateActions($scope.workflowForm);
            };

            $scope.validateOperations = function (tabClick) {
                if (!$scope.workflowForm)
                    $scope.workflowForm = tabClick;

                $scope.workflowForm.operation.$setValidity('operations', false);

                if (!$scope.workflowModel.operation)
                    return false;

                angular.forEach($scope.workflowModel.operation, function (value, key) {
                    if (value) {
                        $scope.workflowForm.operation.$setValidity('operations', true);
                        return true;
                    }
                });

                return false;
            };

            $scope.validateSendNotification = function () {
                return $scope.workflowModel.send_notification &&
                    (($scope.workflowModel.send_notification.recipients && $scope.workflowModel.send_notification.recipients.length) || $scope.workflowModel.send_notification.customRecipient) &&
                    $scope.workflowModel.send_notification.subject &&
                    $scope.workflowModel.send_notification.message;
            };

            $scope.validateCreateTask = function () {
                return $scope.workflowModel.create_task &&
                    $scope.workflowModel.create_task.owner &&
                    $scope.workflowModel.create_task.owner.length === 1 &&
                    $scope.workflowModel.create_task.subject &&
                    $scope.workflowModel.create_task.task_due_date;
            };

            $scope.validateUpdateField = function () {
                if ($scope.workflowModel.field_update) {
                    if ($scope.workflowModel.field_update.updateOption && $scope.workflowModel.field_update.updateOption === '1') {
                        return $scope.workflowModel.field_update.updateOption &&
                            $scope.workflowModel.field_update.module &&
                            $scope.workflowModel.field_update.field &&
                            ($scope.workflowModel.field_update.value !== undefined && $scope.workflowModel.field_update.value !== null);
                    } else if ($scope.workflowModel.field_update.updateOption && $scope.workflowModel.field_update.updateOption === '2') {
                        return $scope.workflowModel.field_update.updateOption &&
                            $scope.workflowModel.field_update.firstModule &&
                            $scope.workflowModel.field_update.secondModule &&
                            $scope.workflowModel.field_update.first_field &&
                            $scope.workflowModel.field_update.second_field;
                    }
                }
            };

            $scope.validateWebHook = function () {
                return $scope.workflowModel.webHook && $scope.workflowModel.webHook.callbackUrl && $scope.workflowModel.webHook.methodType;
            };

            $scope.sendNotificationIsNullOrEmpty = function () {
                if (!$scope.workflowModel.send_notification)
                    return true;

                if ((!$scope.workflowModel.send_notification.recipients || !$scope.workflowModel.send_notification.recipients.length) && !$scope.workflowModel.send_notification.subject && !$scope.workflowModel.send_notification.schedule && !$scope.workflowModel.send_notification.message)
                    return true;

                return false;
            };

            $scope.createTaskIsNullOrEmpty = function () {
                if (!$scope.workflowModel.create_task)
                    return true;

                if ((!$scope.workflowModel.create_task.owner || !$scope.workflowModel.create_task.owner.length) && !$scope.workflowModel.create_task.subject && !$scope.workflowModel.create_task.task_due_date && !$scope.workflowModel.create_task.task_status && !$scope.workflowModel.create_task.task_priority && !$scope.workflowModel.create_task.task_notification && !$scope.workflowModel.create_task.description)
                    return true;

                return false;
            };


            $scope.fieldUpdateIsNullOrEmpty = function () {
                if (!$scope.workflowModel.field_update)
                    return true;

                if ($scope.workflowModel.field_update.updateOption === '1') {
                    if (!$scope.workflowModel.field_update.module && !$scope.workflowModel.field_update.field && !$scope.workflowModel.field_update.value)
                        return true;
                } else {
                    if (!$scope.workflowModel.field_update.firstModule && !$scope.workflowModel.field_update.secondModule && !$scope.workflowModel.field_update.first_field && !$scope.workflowModel.field_update.second_field)
                        return true;
                }

                return false;
            };

            $scope.webHookIsNullOrEmpty = function () {
                if (!$scope.workflowModel.webHook)
                    return true;

                if (!$scope.workflowModel.webHook.callbackUrl && !$scope.workflowModel.webHook.methodType)
                    return true;

                return false;
            };

            $scope.getUpdatableModules = function () {
                $scope.updatableModules = [];
                ModuleService.getModuleFields($scope.workflowModel.module.name)
                    .then(function (response) {
                        if (response.data)
                            $scope.workflowModel.module.fields = response.data;

                        $scope.updatableModules.push($scope.workflowModel.module);

                        angular.forEach($scope.workflowModel.module.fields, function (field) {
                            if (field.lookup_type && field.lookup_type !== $scope.workflowModel.module.name && field.lookup_type !== 'users' && !field.deleted) {
                                var module = $filter('filter')($rootScope.appModules, { name: field.lookup_type }, true)[0];
                                $scope.updatableModules.push(module);
                            }
                        });

                        $scope.fieldUpdateModules = angular.copy($scope.updatableModules);
                        $scope.fieldUpdateModules.unshift($filter('filter')($rootScope.appModules, { name: 'users' }, true)[0]);
                    });
            };

            //upodatable modules for send_notification
            $scope.getSendNotificationUpdatableModules = function (module) {
                var updatableModulesForNotification = [];
                var notificationObj = {};

                var currentModule;
                if (module)
                    currentModule = module;
                else
                    currentModule = $scope.workflowModel.module;

                notificationObj.module = currentModule;
                notificationObj.name = currentModule['label_' + $scope.language + '_singular'];
                notificationObj.isSameModule = true;
                notificationObj.systemName = null;
                notificationObj.id = 1;
                updatableModulesForNotification.push(notificationObj);

                var id = 2;
                angular.forEach(currentModule.fields, function (field) {

                    if (field.lookup_type && field.lookup_type !== 'users' && !field.deleted && currentModule.name !== 'activities') {
                        var notificationObj = {};
                        if (field.lookup_type === currentModule.name) {
                            notificationObj.module = $filter('filter')($rootScope.appModules, { name: field.lookup_type }, true)[0];
                            notificationObj.name = field['label_' + $scope.language] + ' ' + '(' + notificationObj.module['label_' + $scope.language + '_singular'] + ')';
                            notificationObj.isSameModule = false;
                            notificationObj.systemName = field.name;
                            notificationObj.id = id;
                        } else {
                            notificationObj.module = $filter('filter')($rootScope.appModules, { name: field.lookup_type }, true)[0];
                            notificationObj.name = field['label_' + $scope.language] + ' ' + '(' + notificationObj.module['label_' + $scope.language + '_singular'] + ')';
                            notificationObj.isSameModule = false;
                            notificationObj.systemName = field.name;
                            notificationObj.id = id;
                        }
                        updatableModulesForNotification.push(notificationObj);
                        id++;
                    }
                });

                var fieldUpdateModulesForNotification = angular.copy(updatableModulesForNotification);

                notificationObj.module = $filter('filter')($rootScope.appModules, { name: 'users' }, true)[0];
                var filterResult = $filter('filter')($rootScope.appModules, { name: 'users' }, true)[0];

                if (filterResult)
                    notificationObj.name = filterResult['label_' + $scope.language + '_singular'];

                notificationObj.isSameModule = false;
                notificationObj.systemName = null;
                notificationObj.id = id;
                fieldUpdateModulesForNotification.unshift(notificationObj);

                $scope.fieldUpdateModulesForNotification = fieldUpdateModulesForNotification;
            };

            //upodatable modules for send_notification
            $scope.getDynamicFieldUpdateModules = function (module) {
                var dynamicfieldUpdateModules = [];
                var updateObj = {};

                var currentModule;
                if (module)
                    currentModule = module;
                else
                    currentModule = $scope.workflowModel.module;

                updateObj.module = currentModule;
                updateObj.name = currentModule['label_' + $scope.language + '_singular'];
                updateObj.isSameModule = true;
                updateObj.systemName = currentModule.name;
                updateObj.id = 1;
                dynamicfieldUpdateModules.push(updateObj);

                var id = 2;
                angular.forEach(currentModule.fields, function (field) {

                    if (field.lookup_type && field.lookup_type !== 'users' && !field.deleted && currentModule.name !== 'activities') {
                        var updateObj = {};
                        if (field.lookup_type === currentModule.name) {
                            updateObj.module = $filter('filter')($rootScope.appModules, { name: field.lookup_type }, true)[0];
                            updateObj.name = field['label_' + $scope.language] + ' ' + '(' + updateObj.module['label_' + $scope.language + '_singular'] + ')';
                            updateObj.isSameModule = false;
                            updateObj.systemName = field.name;
                            updateObj.id = id;
                        } else {
                            updateObj.module = $filter('filter')($rootScope.appModules, { name: field.lookup_type }, true)[0];
                            updateObj.name = field['label_' + $scope.language] + ' ' + '(' + updateObj.module['label_' + $scope.language + '_singular'] + ')';
                            updateObj.isSameModule = false;
                            updateObj.systemName = field.name;
                            updateObj.id = id;
                        }
                        dynamicfieldUpdateModules.push(updateObj);
                        id++;
                    }
                });

                $scope.dynamicfieldUpdateModules = angular.copy(dynamicfieldUpdateModules);
            };

            $scope.generateHookModules = function () {
                if ($scope.id && $scope.workflowModel.webHook && $scope.workflowModel.webHook.parameters) {
                    $scope.hookParameters = [];

                    var hookParameterArray = $scope.workflowModel.webHook.parameters.split(',');

                    angular.forEach(hookParameterArray, function (data) {
                        var parameter = data.split("|", 3);

                        var editParameter = {};
                        editParameter.parameterName = parameter[0];
                        editParameter.selectedModules = angular.copy($scope.updatableModules);
                        var selectedModule;

                        if ($scope.module.name === parameter[1]) {
                            selectedModule = $filter('filter')(editParameter.selectedModules, { name: parameter[1] }, true)[0];
                        }
                        else {
                            var lookupModuleName = $filter('filter')($scope.module.fields, { name: parameter[1] }, true)[0].lookup_type;
                            selectedModule = $filter('filter')(editParameter.selectedModules, { name: lookupModuleName }, true)[0];
                        }

                        if (!selectedModule)
                            return;

                        editParameter.selectedModule = selectedModule;
                        editParameter.selectedField = $filter('filter')(editParameter.selectedModule.fields, { name: parameter[2] }, true)[0];

                        $scope.hookParameters.push(editParameter);
                    })
                }
                else {
                    setWebHookModules();
                }
            };

            var resetUpdateValue = function () {
                $scope.workflowModel.field_update.value = null;
                $scope.$broadcast('angucomplete-alt:clearInput');

                if ($scope.workflowModel.field_update.field) {
                    if ($scope.workflowModel.field_update.field.data_type === 'image')
                        $scope.fileUploader('image');
                }

                if ($scope.workflowModel.field_update.field) {
                    if ($scope.workflowModel.field_update.field.data_type === 'document')
                        $scope.fileUploader('document');
                }

            };

            $scope.updateModuleChanged = function () {
                if (!$scope.workflowModel.field_update || !$scope.workflowModel.field_update.module) {
                    $scope.workflowModel.field_update.field = null;
                    resetUpdateValue();
                    return;
                }

                ModuleService.getPickItemsLists($scope.workflowModel.field_update.module)
                    .then(function (picklists) {
                        $scope.picklistsModule = picklists;
                    });

                $scope.workflowModel.field_update.field = null;
                resetUpdateValue();
            };

            $scope.optionChanged = function () {
                if ($scope.workflowModel.field_update && $scope.workflowModel.field_update.updateOption && $scope.workflowModel.field_update.updateOption === '1') {
                    if ($scope.workflowModel.field_update.module)
                        $scope.workflowModel.field_update.module = null;

                    if ($scope.workflowModel.field_update.field)
                        $scope.workflowModel.field_update.field = null;

                    if ($scope.workflowModel.field_update.value)
                        $scope.workflowModel.field_update.value = null;
                }

                if ($scope.workflowModel.field_update && $scope.workflowModel.field_update.updateOption && $scope.workflowModel.field_update.updateOption === '2') {
                    if ($scope.workflowModel.field_update.firstModule)
                        $scope.workflowModel.field_update.firstModule = null;

                    if ($scope.workflowModel.field_update.secondModule)
                        $scope.workflowModel.field_update.secondModule = null;

                    if ($scope.workflowModel.field_update.first_field)
                        $scope.workflowModel.field_update.first_field = null;

                    if ($scope.workflowModel.field_update.second_field)
                        $scope.workflowModel.field_update.second_field = null;
                }

            };

            $scope.SendNotificationModuleChanged = function () {
                if ($scope.workflowModel.send_notification_module && $scope.workflowModel.send_notification) {
                    if ($scope.workflowModel.send_notification.recipients)
                        $scope.workflowModel.send_notification.recipients = null;

                    if ($scope.workflowModel.send_notification.customRecipient)
                        $scope.workflowModel.send_notification.customRecipient = null;
                }
            };

            $scope.SendNotificationCCModuleChanged = function () {
                if ($scope.workflowModel.send_notification && $scope.workflowModel.send_notification.cc && $scope.workflowModel.send_notification.cc.length === 0)
                    $scope.workflowModel.send_notification.cc = null;

                if ($scope.workflowModel.send_notification_ccmodule && $scope.workflowModel.send_notification) {
                    if ($scope.workflowModel.send_notification.cc)
                        $scope.workflowModel.send_notification.cc = null;

                    if ($scope.workflowModel.send_notification.customCC)
                        $scope.workflowModel.send_notification.customCC = null;
                }
            };

            $scope.SendNotificationBccModuleChanged = function () {
                if ($scope.workflowModel.send_notification && $scope.workflowModel.send_notification.bcc && $scope.workflowModel.send_notification.bcc.length === 0)
                    $scope.workflowModel.send_notification.bcc = null;

                if ($scope.workflowModel.send_notification_bccmodule && $scope.workflowModel.send_notification) {
                    if ($scope.workflowModel.send_notification.bcc)
                        $scope.workflowModel.send_notification.bcc = null;

                    if ($scope.workflowModel.send_notification.customBcc)
                        $scope.workflowModel.send_notification.customBcc = null;
                }
            };

            $scope.operationUpdateChanged = function (status) {
                if (!status) {
                    $scope.workflowModel.frequency = 'continuous';

                    if ($scope.id)
                        $scope.workflowModel.delete_logs = false;
                }

                $scope.workflowModel.changed_field_checkbox = false;
                $scope.workflowModel.changed_field = null;

            };

            $scope.changeFieldCheckboxChanged = function (status) {
                if (!status) {
                    $scope.workflowModel.changed_field = null;
                }
            };

            $scope.frequencyChanged = function (frequency) {
                if ($scope.id) {
                    if (frequency === 'continuous')
                        $scope.workflowModel.delete_logs = false;
                }
            };

            $scope.updatableField = function (field) {
                if (field.data_type === 'lookup' && field.lookup_type === 'relation')
                    return false;

                if (field.validation && field.validation.readonly)
                    return false;

                return true;
            };

            $scope.updateFieldChanged = function () {
                resetUpdateValue();
            };

            var getLookupValue = function () {
                var deferred = $q.defer();

                ModuleService.getRecord($scope.workflowModel.field_update.field.lookup_type, $scope.workflowModel.field_update.value)
                    .then(function (lookupRecord) {
                        lookupRecord = lookupRecord.data;
                        if ($scope.workflowModel.field_update.field.lookup_type === 'users') {
                            lookupRecord.primary_value = lookupRecord['full_name'];
                        }
                        else {
                            var lookupModule = $filter('filter')($rootScope.appModules, { name: $scope.workflowModel.field_update.field.lookup_type }, true)[0];
                            var lookupPrimaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];
                            lookupRecord.primary_value = lookupRecord[lookupPrimaryField.name];
                        }

                        $scope.workflowModel.field_update.value = lookupRecord;
                        $scope.$broadcast('angucomplete-alt:changeInput', 'updateValue', lookupRecord);
                        deferred.resolve(lookupRecord);
                    })
                    .catch(function (reason) {
                        deferred.reject(reason.data);
                    });

                return deferred.promise;
            };

            $scope.updateFieldTabClicked = function () {
                if (!$scope.id)
                    return;

                if ($scope.workflowModel.field_update && $scope.workflowModel.field_update.field && $scope.workflowModel.field_update.field.data_type === 'lookup' && !angular.isObject($scope.workflowModel.field_update.value)) {
                    getLookupValue();
                }
            };

            $scope.prepareFilters = function () {
                angular.forEach($scope.filters, function (filter) {
                    if (filter.field && filter.field.data_type === 'lookup' && !angular.isObject(filter.value)) {
                        if (filter.operator.name === 'empty' || filter.operator.name === 'not_empty')
                            return;

                        var id = null;

                        if (!filter.value)
                            id = filter.valueState;
                        else
                            id = filter.value;

                        if (!id)
                            return;

                        if (filter.field.lookup_type === 'users' && id === 0) {
                            var user = {};
                            user.id = 0;
                            user.email = '[me]';
                            user.full_name = $filter('translate')('Common.LoggedInUser');
                            user.primary_value = user.full_name;
                            filter.value = [user];
                            return;
                        }

                        ModuleService.getRecord(filter.field.lookup_type, id)
                            .then(function (lookupRecord) {
                                lookupRecord = lookupRecord.data;
                                if (filter.field.lookup_type === 'users') {
                                    lookupRecord.primary_value = lookupRecord['full_name'];
                                    filter.value = [lookupRecord];
                                }
                                else {
                                    var lookupModule = $filter('filter')($rootScope.appModules, { name: filter.field.lookup_type }, true)[0];
                                    var lookupPrimaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];
                                    lookupRecord.primary_value = lookupRecord[lookupPrimaryField.name];
                                    filter.value = lookupRecord;
                                    $scope.$broadcast('angucomplete-alt:changeInput', 'filterLookup' + filter.no, lookupRecord);
                                }
                            });
                    }
                });
            };

            $scope.lookup = function (searchTerm) {
                if ($scope.currentLookupField.lookup_type === 'users' && !$scope.currentLookupField.lookupModulePrimaryField) {
                    var userModulePrimaryField = {};
                    userModulePrimaryField.data_type = 'text_single';
                    userModulePrimaryField.name = 'full_name';
                    $scope.currentLookupField.lookupModulePrimaryField = userModulePrimaryField;
                }

                if (($scope.currentLookupField.lookupModulePrimaryField.data_type === 'number' || $scope.currentLookupField.lookupModulePrimaryField.data_type === 'number_auto') && isNaN(parseFloat(searchTerm))) {
                    $scope.$broadcast('angucomplete-alt:clearInput', $scope.currentLookupField.name);
                    return $q.defer().promise;
                }

                return ModuleService.lookup(searchTerm, $scope.currentLookupField, null);
            };

            $scope.lookupUser = helper.lookupUser;

            $scope.multiselect = function (searchTerm, field) {
                var picklistItems = [];

                angular.forEach($scope.modulePicklists[field.picklist_id], function (picklistItem) {
                    if (picklistItem.inactive)
                        return;

                    if (picklistItem.labelStr.toLowerCase().indexOf(searchTerm) > -1)
                        picklistItems.push(picklistItem);
                });

                return picklistItems;
            };

            $scope.tags = function (searchTerm, field) {
                return $http.get(config.apiUrl + "tag/get_tag/" + field.id).then(function (response) {
                    var tags = response.data;
                    return tags.filter(function (tag) {
                        return tag.text.toLowerCase().indexOf(searchTerm.toLowerCase()) !== -1;
                    });
                });
            };

            $scope.setCurrentLookupField = function (field) {
                $scope.currentLookupField = field;
            };

            $scope.validateActions = function (tabClick) {
                if (!$scope.lastStepClicked && !tabClick) {
                    tabClick.$submitted = false;
                    return true;
                }
                var sendNotificationIsNullOrEmpty = $scope.sendNotificationIsNullOrEmpty();
                var createTaskIsNullOrEmpty = $scope.createTaskIsNullOrEmpty();
                var fieldUpdateIsNullOrEmpty = $scope.fieldUpdateIsNullOrEmpty();
                var webHookIsNullOrEmpty = $scope.webHookIsNullOrEmpty();

                tabClick.actions.$setValidity('actionRequired', true);

                if (sendNotificationIsNullOrEmpty && createTaskIsNullOrEmpty && fieldUpdateIsNullOrEmpty && webHookIsNullOrEmpty) {
                    if (tabClick) {
                        tabClick.$submitted = false;
                        return true;
                    }

                    tabClick.$submitted = false;

                    if (tabClick.recipients)
                        tabClick.recipients.$setValidity('minTags', true);

                    if (tabClick.customRecipient)
                        tabClick.customRecipient.$setValidity('required', true);

                    tabClick.subjectNotification.$setValidity('required', true);
                    tabClick.message.$setValidity('required', true);
                    tabClick.owner.$setValidity('required', true);
                    tabClick.subjectTask.$setValidity('required', true);
                    tabClick.dueDate.$setValidity('required', true);
                    tabClick.updateOption.$setValidity('required', true);
                    tabClick.callbackUrl.$setValidity('required', true);
                    tabClick.methodType.$setValidity('required', true);


                    if (tabClick.updateField && $scope.workflowModel.field_update.updateOption === '1')
                        tabClick.updateField.$setValidity('required', true);

                    if (tabClick.updateValue && $scope.workflowModel.field_update.updateOption === '1')
                        tabClick.updateValue.$setValidity('required', true);

                    tabClick.actions.$setValidity('actionRequired', false);
                    return false;
                }

                if (tabClick.subjectNotification.$pristine && tabClick.message.$pristine &&
                    tabClick.owner.$pristine && tabClick.subjectTask.$pristine && tabClick.dueDate.$pristine &&
                    (tabClick.updateField && tabClick.updateField.$pristine) && (tabClick.updateValue && tabClick.updateValue.$pristine) &&
                    tabClick.callbackUrl.$pristine) {
                    tabClick.$submitted = false;
                    return true;
                }

                if (!sendNotificationIsNullOrEmpty && (!$scope.workflowModel.send_notification.recipients || !$scope.workflowModel.send_notification.recipients.length) && !$scope.workflowModel.send_notification.customRecipient)
                    tabClick.recipients.$setValidity('minTags', false);

                if (!sendNotificationIsNullOrEmpty && !$scope.workflowModel.send_notification.subject)
                    tabClick.subjectNotification.$setValidity('required', false);

                if (!sendNotificationIsNullOrEmpty && !$scope.workflowModel.send_notification.message)
                    tabClick.message.$setValidity('required', false);

                if (!createTaskIsNullOrEmpty && (!$scope.workflowModel.create_task.owner || !$scope.workflowModel.create_task.owner.length))
                    tabClick.owner.$setValidity('minTags', false);

                if (!createTaskIsNullOrEmpty && !$scope.workflowModel.create_task.subject)
                    tabClick.subjectTask.$setValidity('required', false);

                if (!createTaskIsNullOrEmpty && !$scope.workflowModel.create_task.task_due_date)
                    tabClick.dueDate.$setValidity('required', false);

                if (!fieldUpdateIsNullOrEmpty && $scope.workflowModel.field_update.updateOption === '1' && $scope.workflowModel.field_update.module && !$scope.workflowModel.field_update.field)
                    tabClick.updateField.$setValidity('required', false);

                if (!fieldUpdateIsNullOrEmpty && $scope.workflowModel.field_update.updateOption === '1' && $scope.workflowModel.field_update.module && $scope.workflowModel.field_update.field && ($scope.workflowModel.field_update.value === undefined || $scope.workflowModel.field_update.value === null))
                    tabClick.updateValue.$setValidity('required', false);

                if (!webHookIsNullOrEmpty && !$scope.workflowModel.webHook.callbackUrl) {
                    tabClick.callbackUrl.$setValidity('required', false);
                }
                if (!webHookIsNullOrEmpty && !$scope.workflowModel.webHook.methodType) {
                    tabClick.methodType.$setValidity('required', false);
                }

                var isSendNotificationValid = $scope.validateSendNotification();
                var isCreateTaskValid = $scope.validateCreateTask();
                var isUpdateFieldValid = $scope.validateUpdateField();
                var isWebhookValid = $scope.validateWebHook();

                if ((isSendNotificationValid && isCreateTaskValid && isUpdateFieldValid && isWebhookValid) ||
                    (isSendNotificationValid && isCreateTaskValid) ||
                    (isSendNotificationValid && isUpdateFieldValid) ||
                    (isCreateTaskValid && isUpdateFieldValid) ||
                    (isWebhookValid && isSendNotificationValid) ||
                    (isWebhookValid && isUpdateFieldValid) ||
                    (isWebhookValid && isCreateTaskValid) ||
                    (isSendNotificationValid || isCreateTaskValid || isUpdateFieldValid || isWebhookValid)) {
                    tabClick.$submitted = false;
                    return true;
                }

                return false;
            };

            $scope.setFormValid = function (form) {
                if (!$scope.workflowForm)
                    $scope.workflowForm = form;

                $scope.workflowForm.$submitted = false;

                if ($scope.workflowForm.recipients)
                    $scope.workflowForm.recipients.$setValidity('minTags', true);

                $scope.workflowForm.subjectNotification.$setValidity('required', true);
                $scope.workflowForm.message.$setValidity('required', true);
                $scope.workflowForm.owner.$setValidity('minTags', true);
                $scope.workflowForm.subjectTask.$setValidity('required', true);
                $scope.workflowForm.dueDate.$setValidity('required', true);
                $scope.workflowForm.actions.$setValidity('actionRequired', true);
                //$scope.workflowForm.updateOption.$setValidity('required', true);
                $scope.workflowForm.callbackUrl.$setValidity('required', true);

                if ($scope.workflowForm.updateField && $scope.workflowModel.field_update.updateOption === '1')
                    $scope.workflowForm.updateField.$setValidity('required', true);

                if ($scope.workflowForm.updateValue && $scope.workflowModel.field_update.updateOption === '1')
                    $scope.workflowForm.updateValue.$setValidity('required', true);
            };

            var setWebHookModules = function () {
                $scope.hookParameters = [];

                $scope.hookModules = [];

                angular.forEach($scope.updatableModules, function (module) {
                    $scope.hookModules.push(module);
                });

                var parameter = {};
                parameter.parameterName = null;
                parameter.selectedModules = $scope.hookModules;
                parameter.selectedField = null;
                parameter.selectedModule = $scope.workflowModel.module;

                $scope.hookParameters.push(parameter);
            };

            $scope.getSummary = function () {
                if (!$scope.workflowModel.name || !$scope.workflowModel.module || !$scope.workflowModel.operation)
                    return;

                var getSummary = function () {
                    $scope.ruleTriggerText = '';
                    $scope.ruleFilterText = '';
                    $scope.ruleActionsText = '';
                    var andText = $filter('translate')('Common.And');
                    var orText = $filter('translate')('Common.Or');

                    if ($scope.workflowModel.operation.insert)
                        $scope.ruleTriggerText += $filter('translate')('Setup.Workflow.RuleTriggers.insertLabel') + ' ' + orText + ' ';

                    if ($scope.workflowModel.operation.update) {
                        var updateText = $filter('translate')('Setup.Workflow.RuleTriggers.updateLabel') + ' ' + orText + ' ';
                        $scope.ruleTriggerText += $scope.ruleTriggerText ? updateText.toLowerCase() : updateText;
                    }

                    if ($scope.workflowModel.operation.delete) {
                        var deleteText = $filter('translate')('Setup.Workflow.RuleTriggers.deleteLabel') + ' ' + orText + ' ';
                        $scope.ruleTriggerText += $scope.ruleTriggerText ? deleteText.toLowerCase() : deleteText;
                    }

                    $scope.ruleTriggerText = $scope.ruleTriggerText.slice(0, -(orText.length + 2));

                    angular.forEach($scope.filters, function (filter) {
                        if (!filter.field || !filter.operator || !filter.value)
                            return;

                        $scope.ruleFilterText += filter.field['label_' + $scope.language] + ' <b class="operation-highlight">' + filter.operator.label[$scope.language] + '</b> ' +
                            getFilterValue(filter) + ' <b class="operation-highlight">' + andText + '</b><br> ';
                    });

                    $scope.ruleFilterText = $scope.ruleFilterText.slice(0, -(andText.length + 41));
                    var isSendNotificationValid = $scope.validateSendNotification();
                    var isCreateTaskValid = $scope.validateCreateTask();
                    var isUpdateFieldValid = $scope.validateUpdateField();
                    var isWebhookFieldValid = $scope.validateWebHook();

                    if (isSendNotificationValid) {
                        $scope.ruleActionsText += '<b class="operation-highlight">' + $filter('translate')('Setup.Workflow.SendNotification') + '</b><br>';
                        $scope.ruleActionsText += $filter('translate')('Setup.Workflow.SendNotificationSummary', { subject: $scope.workflowModel.send_notification.subject }) + '<br>';

                        angular.forEach($scope.workflowModel.send_notification.recipients, function (recipient) {
                            $scope.ruleActionsText += recipient.full_name + ', ';
                        });

                        $scope.ruleActionsText = $scope.ruleActionsText.slice(0, -2) + '<br>';

                        if ($scope.workflowModel.send_notification.schedule)
                            $scope.ruleActionsText += $filter('translate')('Setup.Workflow.Schedule') + ': ' + $scope.workflowModel.send_notification.schedule.label;
                    }

                    if (isCreateTaskValid) {
                        if (isSendNotificationValid)
                            $scope.ruleActionsText += '<br><br>';

                        $scope.ruleActionsText += '<b class="operation-highlight">' + $filter('translate')('Setup.Workflow.AssignTask') + '</b><br>';
                        $scope.ruleActionsText += $filter('translate')('Setup.Workflow.AssignTaskSummary', { subject: $scope.workflowModel.create_task.subject }) + '<br>';
                        $scope.ruleActionsText += $scope.workflowModel.create_task.owner[0].full_name + '<br>';

                        if ($scope.workflowModel.create_task.task_due_date)
                            $scope.ruleActionsText += $filter('translate')('Setup.Workflow.Schedule') + ': ' + $scope.workflowModel.create_task.task_due_date.label;
                    }

                    if (isUpdateFieldValid) {
                        if (isSendNotificationValid || isCreateTaskValid)
                            $scope.ruleActionsText += '<br><br>';

                        $scope.ruleActionsText += '<b class="operation-highlight">' + $filter('translate')('Setup.Workflow.FieldUpdate') + '</b><br>';

                        var updateFieldRecordFake = {};

                        if ($scope.workflowModel.field_update.updateOption === '1') {
                            updateFieldRecordFake[$scope.workflowModel.field_update.field.name] = angular.copy($scope.workflowModel.field_update.value);
                            updateFieldRecordFake = ModuleService.prepareRecord(updateFieldRecordFake, $scope.workflowModel.field_update.module);
                            ModuleService.formatFieldValue($scope.workflowModel.field_update.field, $scope.updateFieldValue, $scope.picklistsModule);

                            var value = $scope.workflowModel.field_update.field.valueFormatted;

                            switch ($scope.workflowModel.field_update.field.data_type) {
                                case 'lookup':
                                    value = $scope.workflowModel.field_update.value.primary_value;
                                    $scope.updateFieldValue = $scope.workflowModel.field_update.value.id;
                                    break;
                                case 'picklist':
                                    $scope.updateFieldValue = $scope.workflowModel.field_update.value.label[$scope.language];
                                    value = $scope.updateFieldValue;
                                    break;
                                case 'multiselect':
                                    $scope.updateFieldValue = '';

                                    angular.forEach($scope.workflowModel.field_update.value, function (picklistItem) {
                                        var label = picklistItem.label[$scope.language];
                                        $scope.updateFieldValue += label + '|';
                                        value += label + '; ';
                                    });

                                    $scope.updateFieldValue = $scope.updateFieldValue.slice(0, -1);
                                    value = value.slice(0, -2);
                                    break;
                                case 'tag':
                                    $scope.updateFieldValue = '';

                                    angular.forEach($scope.workflowModel.field_update.value, function (item) {

                                        $scope.updateFieldValue += item.text + '|';
                                        value += item.text + '; ';
                                    });

                                    $scope.updateFieldValue = $scope.updateFieldValue.slice(0, -1);
                                    value = value.slice(0, -2);
                                    break;
                                case 'checkbox':
                                    var fieldValue = $scope.workflowModel.field_update.value;

                                    if (fieldValue === undefined)
                                        fieldValue = false;

                                    var yesNoPicklistItem = $filter('filter')($scope.picklistsModule['yes_no'], { system_code: fieldValue.toString() }, true)[0];
                                    value = yesNoPicklistItem.label[$scope.language];
                                    $scope.updateFieldValue = fieldValue;
                                    break;
                                default:
                                    $scope.updateFieldValue = updateFieldRecordFake[$scope.workflowModel.field_update.field.name];
                                    break;
                            }

                            $scope.ruleActionsText += $filter('translate')('Setup.Workflow.FieldUpdateSummary', { module: $scope.workflowModel.field_update.module['label_' + $scope.language + '_singular'], field: $scope.workflowModel.field_update.field['label_' + $scope.language], value: value }) + '<br>';
                        } else {
                            $scope.ruleActionsText += $filter('translate')('Setup.Workflow.FieldUpdateSummaryDynamic') + '<br>';

                        }


                    }

                    if (isWebhookFieldValid) {
                        if (isSendNotificationValid || isCreateTaskValid || isUpdateFieldValid)
                            $scope.ruleActionsText += '<br><br>';


                        $scope.ruleActionsText += '<b class="operation-highlight">' + $filter('translate')('Setup.Workflow.WebHook') + '</b><br>';
                        $scope.ruleActionsText += $filter('translate')('Setup.Workflow.WebhookSummary', { callbackUrl: $scope.workflowModel.webHook.callbackUrl, methodType: $scope.workflowModel.webHook.methodType }) + '<br>';

                        if ($scope.hookParameters.length > 0) {
                            $scope.ruleActionsText += '<br><b>' + $filter('translate')('Setup.Workflow.WebHookParameters') + ':</b><br>';
                            var lastHookParameter = $scope.hookParameters[$scope.hookParameters.length - 1];
                            if (lastHookParameter.parameterName && lastHookParameter.selectedField !== null && lastHookParameter.selectedModule) {
                                $scope.showWebhookSummaryTable = true;
                                //if any valid parameter for object
                                $scope.workflowModel.webHook.hookParameters = $scope.hookParameters;
                            }
                            else {
                                $scope.showWebhookSummaryTable = false;
                            }
                        }
                    }
                };

                if ($scope.workflowModel.field_update && $scope.workflowModel.field_update.field && $scope.workflowModel.field_update.field.data_type === 'lookup' && !angular.isObject($scope.workflowModel.field_update.value)) {
                    getLookupValue()
                        .then(function () {
                            getSummary();
                        });
                }
                else {
                    getSummary();
                }
            };

            var setValue = function (data) {
                var updateFieldRecordFake = {};
                updateFieldRecordFake[data.field.name] = angular.copy(data.value);
                updateFieldRecordFake = ModuleService.prepareRecord(updateFieldRecordFake, data.module);
                ModuleService.formatFieldValue(data.field, $scope.updateFieldValue, $scope.picklistsModule);

                var newValue = data.field.valueFormatted;

                switch (data.field.data_type) {
                    case 'lookup':
                        newValue = data.value.primary_value;
                        $scope.updateFieldValue = data.value.id;
                        break;
                    case 'picklist':
                        $scope.updateFieldValue = data.value.label[$rootScope.language];
                        newValue = $scope.updateFieldValue;
                        break;
                    case 'multiselect':
                        $scope.updateFieldValue = '';

                        angular.forEach(value, function (picklistItem) {
                            var label = picklistItem.label[$rootScope.language];
                            $scope.updateFieldValue += label + '|';
                            newValue += label + '; ';
                        });

                        $scope.updateFieldValue = $scope.updateFieldValue.slice(0, -1);
                        newValue = newValue.slice(0, -2);
                        break;
                    case 'checkbox':
                        var fieldValue = data.value;

                        if (fieldValue === undefined)
                            fieldValue = false;

                        var yesNoPicklistItem = $filter('filter')($scope.picklistsModule['yes_no'], { system_code: fieldValue.toString() }, true)[0];
                        newValue = yesNoPicklistItem.label[$rootScope.language];
                        $scope.updateFieldValue = fieldValue;
                        break;
                    default:
                        $scope.updateFieldValue = updateFieldRecordFake[data.field.name];
                        break;
                }
            };

            $scope.saveNodeData = function () {
                $scope.saving = true;

                var diagram = window.myDiagram.model;
                var currentNode = $scope.currentObj.subject.part.data;
                var bpmModel = $scope.workflowModel;
                var data = {};

                if (!diagram)
                    return false;

                switch (currentNode.ngModelName) {
                    case 'start':
                        data.module_id = bpmModel.module.id;
                        data.id = $scope.id;
                        data.name = bpmModel.name;
                        data.code = bpmModel.code;
                        data.frequency = bpmModel.frequency;
                        data.filters = [];

                        var filters = $scope.filters;

                        if (filters && filters.length) {
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
                                        filter.value = new Date(filterItem.value).getTime();

                                    if (field.data_type === 'picklist')
                                        filter.value = filterItem.value.id;

                                    if (field.data_type === 'multiselect') {
                                        var value = '';

                                        angular.forEach(filterItem.value, function (picklistItem) {
                                            value += picklistItem.id + ',';
                                        });

                                        filter.value = value.slice(0, -1);
                                    }

                                    if (field.data_type === 'lookup' && field.lookup_type !== 'users')
                                        filter.value = filterItem.value.id;

                                    if (field.data_type === 'lookup' && field.lookup_type === 'users')
                                        filter.value = filterItem.value[0].id;

                                    if (field.data_type === 'checkbox')
                                        filter.value = filterItem.value.system_code;
                                }
                                else {
                                    filter.value = '-';
                                }

                                data.filters.push(filter);
                            });

                            data.active = bpmModel.active;
                            data.process_filter = bpmModel.processFilter === "none" ? null : bpmModel.processFilter;
                            data.trigger_type = "record";

                            var loopArray = [];
                            angular.forEach(bpmModel.operation, function (value, key) {
                                if (value)
                                    this.push(key);
                            }, loopArray);

                            data.record_operations = loopArray.join();
                            data.canstartmanuel = false;
                            data.defitinion_json = {};

                        }
                        break;
                    case 'field_update':
                        //Static Data;
                        data[currentNode.ngModelName] = {};
                        data[currentNode.ngModelName] = bpmModel[currentNode.ngModelName];
                        data[currentNode.ngModelName].module_id = bpmModel[currentNode.ngModelName].module.id;
                        data[currentNode.ngModelName].field_id = bpmModel[currentNode.ngModelName].field.id;
                        setValue(bpmModel[currentNode.ngModelName]);
                        //For Bpmn editor
                        data[currentNode.ngModelName].value = bpmModel[currentNode.ngModelName].value;
                        //For Workflow Engine
                        data[currentNode.ngModelName].currentValue = $scope.updateFieldValue;
                        break;
                    case 'webHook':
                        data[currentNode.ngModelName] = bpmModel[currentNode.ngModelName];
                        data[currentNode.ngModelName].Parameters = angular.copy($scope.hookParameters);
                        break;
                    case 'send_notification':
                        var workflowModel = {};
                        workflowModel = bpmModel[currentNode.ngModelName];
                        data[currentNode.ngModelName] = bpmModel[currentNode.ngModelName];

                        data[currentNode.ngModelName].send_notification_module = bpmModel.send_notification_module;
                        data[currentNode.ngModelName].send_notification_ccmodule = bpmModel.send_notification_ccmodule;
                        data[currentNode.ngModelName].send_notification_bccmodule = bpmModel.send_notification_bccmodule;

                        workflowModel.send_notification_module = bpmModel.send_notification_module;
                        workflowModel.send_notification_ccmodule = bpmModel.send_notification_ccmodule;
                        workflowModel.send_notification_bccmodule = bpmModel.send_notification_bccmodule;

                        data[currentNode.ngModelName].subject = workflowModel.subject;
                        data[currentNode.ngModelName].message = workflowModel.message;

                        var cc = [];
                        if (workflowModel.cc) {
                            angular.forEach(workflowModel.send_notification.cc, function (user) {
                                cc.push(user.email);
                            });
                            data[currentNode.ngModelName].cc = cc;
                        }
                        else if (workflowModel.send_notification_ccmodule && workflowModel.customCC) {
                            workflowModel.cc = [];
                            if (workflowModel.send_notification_ccmodule.module.name === bpmModel.module.name && workflowModel.send_notification_ccmodule.isSameModule) {
                                data[currentNode.ngModelName].cc.push(workflowModel.customCC.name);
                            }
                            else if (workflowModel.send_notification_ccmodule.module.name === bpmModel.module.name && !workflowModel.send_notification_ccmodule.isSameModule) {
                                var sameModuleLookupsCC = $filter('filter')(bpmModel.module.fields, { lookup_type: workflowModel.send_notification_ccmodule.module.name }, true);
                                data[currentNode.ngModelName].cc.push($filter('filter')(sameModuleLookupsCC, { name: workflowModel.send_notification_ccmodule.systemName }, true)[0].name + '.' + workflowModel.customCC.name);
                            }
                            else {
                                var lookupsCC = $filter('filter')(bpmModel.module.fields, { lookup_type: workflowModel.send_notification_ccmodule.module.name }, true);
                                data[currentNode.ngModelName].cc.push($filter('filter')(lookupsCC, { name: workflowModel.send_notification_ccmodule.systemName }, true)[0].name + '.' + workflowModel.customCC.name);
                            }
                        }

                        var bcc = [];
                        if (workflowModel.bcc) {
                            // angular.forEach(workflowModel.send_notification.bcc, function (user) {
                            //     bcc.push(user.email);
                            // });
                            // data[currentNode.ngModelName].bcc = bcc;
                        }
                        else if (workflowModel.send_notification_bccmodule && workflowModel.customBcc) {
                            workflowModel.bcc = [];
                            if (workflowModel.send_notification_bccmodule.module.name === bpmModel.module.name && workflowModel.send_notification_bccmodule.isSameModule) {
                                data[currentNode.ngModelName].bcc.push(workflowModel.customBcc.name);
                            }
                            else if (workflowModel.send_notification_bccmodule.module.name === bpmModel.module.name && !workflowModel.send_notification_bccmodule.isSameModule) {
                                var sameModuleLookupsBcc = $filter('filter')(bpmModel.module.fields, { lookup_type: workflowModel.send_notification_bccmodule.module.name }, true);
                                data[currentNode.ngModelName].bcc.push($filter('filter')(sameModuleLookupsBcc, { name: workflowModel.send_notification_bccmodule.systemName }, true)[0].name + '.' + workflowModel.customBcc.name);
                            }
                            else {
                                var lookupsBcc = $filter('filter')(bpmModel.module.fields, { lookup_type: workflowModel.send_notification_bccmodule.module.name }, true);
                                data[currentNode.ngModelName].bcc.push($filter('filter')(lookupsBcc, { name: workflowModel.send_notification_bccmodule.systemName }, true)[0].name + '.' + workflowModel.customBcc.name);
                            }
                        }

                        // var recipients = [];

                        if (workflowModel.recipients) {
                            // angular.forEach(workflowModel.send_notification.recipients, function (user) {
                            //     recipients.push(user.email);
                            // });
                            // data[currentNode.ngModelName].recipients = recipients;
                        }
                        else {
                            if (!data[currentNode.ngModelName].recipients)
                                data[currentNode.ngModelName].recipients = [];

                            if (workflowModel.send_notification_module.module.name === bpmModel.module.name && workflowModel.send_notification_module.isSameModule) {
                                data[currentNode.ngModelName].recipients.push(workflowModel.customRecipient.name);
                            }
                            else if (workflowModel.send_notification_module.module.name === bpmModel.module.name && !workflowModel.send_notification_module.isSameModule) {
                                var sameModuleLookups = $filter('filter')(bpmModel.module.fields, { lookup_type: workflowModel.send_notification_module.module.name }, true);
                                data[currentNode.ngModelName].recipients.push($filter('filter')(sameModuleLookups, { name: workflowModel.send_notification_module.systemName }, true)[0].name + '.' + workflowModel.customRecipient.name);
                            }
                            else {
                                var lookups = $filter('filter')(bpmModel.module.fields, { lookup_type: workflowModel.send_notification_module.module.name }, true);

                                data[currentNode.ngModelName].recipients.push($filter('filter')(lookups, { name: workflowModel.send_notification_module.systemName }, true)[0].name + '.' + workflowModel.customRecipient.name);
                            }
                        }

                        break;
                    case 'data_read':
                        data[currentNode.ngModelName] = {};
                        data[currentNode.ngModelName] = bpmModel[currentNode.ngModelName];
                        data[currentNode.ngModelName].record_key = bpmModel[currentNode.ngModelName].field.name;
                        break;
                    default:
                        if (currentNode.isDefault === false) {
                            data['condition'] = $scope.workflowModel.condition;
                            diagram.linkDataArray.find(q => q.from === currentNode.from && q.to === currentNode.to).data = angular.copy(data);
                        }
                        break;
                }

                diagram.nodeDataArray.find(q => q.key === currentNode.key).data = angular.copy(data);

                setTimeout(function () {
                    $scope.saving = false;
                    $scope.triggerBpm();
                    $scope.$digest();
                    $scope.cancel();
                }, 2000);
            };






        }
    ]);