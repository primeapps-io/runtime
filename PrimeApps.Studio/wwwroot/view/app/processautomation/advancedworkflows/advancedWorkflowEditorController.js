'use strict';

angular.module('primeapps')

    .controller('WorkflowEditorController', ['$rootScope', '$scope', '$location', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'AdvancedWorkflowsService', 'LayoutService', 'ModuleService', '$http', 'config', 'operators', '$localStorage', '$cookies',
        function ($rootScope, $scope, $location, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, AdvancedWorkflowsService, LayoutService, ModuleService, $http, config, operators, $localStorage, $cookies) {
            $scope.loading = true;
            $scope.saving = false;
            $scope.gridValue = true;
            $scope.snapValue = true;
            $scope.modalLoading = false;
            $scope.id = $location.search().id;
            $scope.workflowModel = {};
            $scope.workflowStartModel = { active: true, frequency: "continuous" };
            $scope.operationValue = true;
            // $scope.$parent.menuTopTitle = "Automation";
            //$scope.$parent.activeMenu = 'automation';
            $scope.$parent.activeMenuItem = 'advancedWorkflows';
            $rootScope.subtoggleClass = 'full-toggled2';
            $rootScope.toggleClass = 'toggled';
            $scope.scheduleItems = AdvancedWorkflowsService.getScheduleItems();
            $scope.dueDateItems = AdvancedWorkflowsService.getDueDateItems();
            $rootScope.breadcrumblist[2].title = 'Advanced Workflows';
            $scope.modules = $rootScope.appModules;

            var activityModule = $filter('filter')($rootScope.appModules, { name: 'activities' }, true)[0];

            //BPM element menu loading start
            angular.element(function () {
                window.initFunc();
            });

            $scope.gridChange = function (value) {
                angular.element(function () {
                    window.updateGridOption(value);
                });
            };
            $scope.snapeGridChange = function (value) {
                angular.element(function () {
                    window.updateSnapOption(value);
                });
            };

            $scope.gridChange($scope.gridValue);
            $scope.snapeGridChange($scope.snapValue);

            $scope.triggerBpm = function () {
                angular.element(function () {
                    jQuery("#accordion").accordion({
                        activate: function (event, ui) {
                            window.myPaletteLevel1.requestUpdate();
                            window.myPaletteLevel2.requestUpdate();
                        }
                    });
                    if ($scope.id) {
                        AdvancedWorkflowsService.get($scope.id)
                            .then(function (response) {
                                if (response) {
                                    var tempJson = JSON.parse(response.data.diagram_json);
                                    window.myDiagram.model.nodeDataArray = tempJson.nodeDataArray;
                                    window.myDiagram.model.linkDataArray = tempJson.linkDataArray;

                                    window.myDiagram.requestUpdate();
                                    window.myPaletteLevel1.requestUpdate();

                                    var startNode = $filter('filter')(window.myDiagram.model.nodeDataArray, { ngModelName: 'start' }, true)[0];
                                    if (startNode) {
                                        $scope.workflowStartModel = angular.copy(startNode.data);
                                        $scope.operationValue = $scope.workflowStartModel.operator ? true : false;
                                        //ModuleService.getPickItemsLists($scope.module)
                                        //    .then(function (picklists) {
                                        //        $scope.modulePicklists = picklists;
                                        //    });
                                        $scope.selectModule();
                                    }
                                }
                            });
                    }
                    else {
                        window.myDiagram.requestUpdate();
                        window.myPaletteLevel1.requestUpdate();
                    }
                });
            };

            $timeout(function () {
                $scope.triggerBpm();

            }, 500);
            //BPM element menu loading end

            // BPM click Event and RightSide Menu Controller Start
            $scope.toogleSideMenu = function () {
                if ($scope.currentObj.subject) {
                    $scope.modalLoading = true;
                    var node = $scope.currentObj.subject.part.data;

                    if (node) {
                        $scope.SelectedNodeItem = node;

                        if (node.data && node.ngModelName) {
                            $scope.workflowModel[node.ngModelName] = {}; //clear for new value

                            if (!$scope.workflowStartModel) {
                                $scope.workflowStartModel = { active: true, frequency: "continuous" };
                                if (node.ngModelName !== 'start') {
                                    var startNode = $filter('filter')(window.myDiagram.model.nodeDataArray, { ngModelName: 'start' }, true)[0];
                                    if (!startNode)
                                        return false;

                                    //$scope.workflowStartModel = startNode.data;
                                    //$scope.workflowStartModel = processWorkflow();

                                }
                            }

                            switch (node.ngModelName) {
                                case 'start':
                                    $scope.workflowStartModel = {};
                                    $scope.workflowStartModel = node.data;
                                    $scope.selectModule();
                                    $scope.workflowStartModel.operation = {};
                                    var filters = node.data.filters;

                                    if (node.data.record_operations.split(',') > 0 || node.data.record_operations.split(',') !== '') {
                                        angular.forEach(node.data.record_operations.split(','), function (item) {
                                            $scope.workflowStartModel.operation[item] = true;
                                        });
                                    }

                                    ModuleService.getModuleById(node.data.module_id)
                                        .then(function (response) {
                                            if (response.data) {
                                                var module = response.data;
                                                ModuleService.getModuleFields(module.name)
                                                    .then(function (result) {
                                                        if (result.data) {
                                                            module.fields = result.data;
                                                            module = ModuleService.getFieldsOperator(module, $rootScope.appModules, 0);
                                                            $scope.module = module;
                                                            $scope.workflowStartModel.module = module;
                                                            //Filter
                                                            if (filters) {
                                                                filters = $filter('orderBy')(filters, 'no');

                                                                for (var i = 0; i < filters.length; i++) {
                                                                    var filter = angular.copy(filters[i]);
                                                                    var field = $filter('filter')($scope.workflowStartModel.module.fields, { name: filter.field }, true)[0];
                                                                    var fieldValue = null;

                                                                    if (!field)
                                                                        return;

                                                                    switch (field.data_type) {
                                                                        case 'picklist':
                                                                            fieldValue = $filter('filter')($scope.modulePicklists[field.picklist_id], { id: filter.value }, true)[0];
                                                                            break;
                                                                        case 'multiselect':
                                                                            var picklistItems = filter.value.split('|');
                                                                            fieldValue = [];

                                                                            angular.forEach(picklistItems, function (picklistLabel) {
                                                                                var picklist = $filter('filter')($scope.modulePicklists[field.picklist_id], { labelStr: picklistLabel }, true)[0];

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
                                                                            fieldValue = $filter('filter')($scope.modulePicklists['yes_no'], { system_code: filter.value })[0];
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

                                                                    $scope.filters[i] = filter;
                                                                }
                                                            }//Filter end 
                                                            $scope.modalLoading = false;;
                                                        }
                                                    });
                                            }
                                        });
                                    break;
                                case 'field_update':
                                    $scope.workflowModel[node.ngModelName] = node.data[node.ngModelName];
                                    $scope.getUpdatableModules();
                                    $scope.modalLoading = false;
                                    break;
                                case 'webHook':
                                    $scope.workflowModel[node.ngModelName] = node.data[node.ngModelName];
                                    $scope.hookParameters = node.data[node.ngModelName].Parameters;
                                    $scope.modalLoading = false;
                                    break;
                                case 'send_notification':
                                    $scope.workflowModel[node.ngModelName] = node.data[node.ngModelName];
                                    $scope.workflowModel.send_notification_module = node.data[node.ngModelName].send_notification_module;
                                    $scope.workflowModel.send_notification_ccmodule = node.data[node.ngModelName].send_notification_ccmodule;
                                    $scope.workflowModel.send_notification_bccmodule = node.data[node.ngModelName].send_notification_bccmodule;
                                    $scope.modalLoading = false;
                                    break;
                                case 'data_read':
                                    $scope.workflowModel[node.ngModelName] = node.data[node.ngModelName];
                                    $scope.modalLoading = false;
                                    break;
                                case 'function':
                                    $scope.workflowModel[node.ngModelName] = node.data[node.ngModelName];

                                    if ($scope.workflowModel[node.ngModelName]['name'])
                                        $scope.workflowModel[node.ngModelName]['name'] = { name: $scope.workflowModel[node.ngModelName]['name'] };

                                    if ($scope.workflowModel[node.ngModelName]['methodType'])
                                        $scope.functionType = $scope.workflowModel[node.ngModelName]['methodType'] === 'post';
                                    $scope.modalLoading = false;
                                    break;
                                default:
                                    $scope.workflowModel[node.ngModelName] = node.data[node.ngModelName];
                                    $scope.modalLoading = false;
                                    break;
                            }
                        }
                        else if (node.isDefault === false) { //FOR Conditional Link
                            if (node.data)
                                $scope.workflowModel["condition"] = node.data.condition;
                            else
                                $scope.workflowModel["condition"] = null;

                            $scope.showFormModal();
                            $scope.modalLoading = false;
                        }
                        else {
                            $scope.workflowModel = {};

                            if (node.ngModelName === 'webHook') {
                                $scope.workflowModel.webHook = { methodType: 'post' };
                                setWebHookModules();
                            }
                            $scope.modalLoading = false;
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
                    templateUrl: 'view/app/processautomation/advancedworkflows/advancedworkflowModal.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.formModal.$promise.then(function () {
                    $scope.formModal.show();
                });

            };

            $scope.cancel = function () {
                //angular.forEach($scope.currentRelation, function (value, key) {
                //    $scope.currentRelation[key] = $scope.currentRelationState[key];
                //});
                $scope.formModal.hide();
                $scope.currentObj = null;
                //var modelName = $scope.SelectedNodeItem.ngModelName;

                //if (modelName === 'start')
                //    $scope.workflowStartModel = {};
                //else
                //    $scope.workflowModel[modelName] = {};


            };

            $scope.deleteSelectedItem = function () {


            };

            //Modal End

            var setTaskFields = function () {
                $scope.taskFields = {};

                if (!activityModule)
                    return;

                ModuleService.getModuleFields(activityModule.name)
                    .then(function (response) {
                        if (response) {
                            activityModule.fields = response.data;

                            ModuleService.getPickItemsLists(activityModule, false)
                                .then(function (pickList) {
                                    if (pickList)
                                        $scope.picklistsActivity = pickList;

                                    $scope.taskFields.owner = $filter('filter')(activityModule.fields, { name: 'owner' }, true)[0];
                                    $scope.taskFields.subject = $filter('filter')(activityModule.fields, { name: 'subject' }, true)[0];
                                    $scope.taskFields.task_due_date = $filter('filter')(activityModule.fields, { name: 'task_due_date' }, true)[0];
                                    $scope.taskFields.task_status = $filter('filter')(activityModule.fields, { name: 'task_status' }, true)[0];
                                    $scope.taskFields.task_priority = $filter('filter')(activityModule.fields, { name: 'task_priority' }, true)[0];
                                    $scope.taskFields.task_notification = $filter('filter')(activityModule.fields, { name: 'task_notification' }, true)[0];
                                    $scope.taskFields.description = $filter('filter')(activityModule.fields, { name: 'description' }, true)[0];
                                });
                        }
                    });
            };

            setTaskFields();

            $scope.selectModule = function () {
                $scope.modalLoading = true;

                ModuleService.getModuleFields($scope.workflowStartModel.module.name)
                    .then(function (response) {
                        if (response) {
                            $scope.workflowStartModel.module.fields = response.data;
                        }

                        var module = angular.copy($scope.workflowStartModel.module);
                        $scope.module = ModuleService.getFieldsOperator(module, $rootScope.appModules, 0);

                        //TODO 
                        //if ($filter('filter')($rootScope.approvalProcesses, { module_id: $scope.module.id }, true)[0]) {
                        //    $scope.showProcessFilter = true;
                        //    $scope.workflowModel.processFilter = 'all';
                        //}
                        //else {
                        $scope.workflowStartModel.processFilter = 'none';
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

                                if ($scope.id) {
                                    var startNode = $filter('filter')(window.myDiagram.model.nodeDataArray, { ngModelName: 'start' }, true)[0];
                                    var tempData = processWorkflow(startNode.data);

                                    if (tempData)
                                        $scope.workflowStartModel = tempData;
                                }
                                else
                                    filterReload();

                                $scope.modalLoading = false;
                            });


                        $scope.getUpdatableModules();
                        $scope.getSendNotificationUpdatableModules();
                        $scope.getDynamicFieldUpdateModules();
                        setWebHookModules();
                    });
            };

            var filterReload = function () {

                $scope.filters = [];

                for (var i = 0; i < 5; i++) {
                    var filter = {};
                    filter.id = i;
                    filter.no = i + 1;
                    filter.field = null;
                    filter.operator = null;
                    filter.value = null;

                    $scope.filters.push(filter);
                }
            };

            var processWorkflow = function (data) {
                $scope.modalLoading = true;
                var workflowModel = angular.copy(data);
                //workflowModel.id = data.id;
                //workflowModel.created_by = data.created_by;
                //workflowModel.updated_by = data.updated_by;
                //workflowModel.created_at = data.created_at;
                //workflowModel.updated_at = data.updated_at;
                //workflowModel.deleted = data.deleted;
                //workflowModel.name = data.name;
                //workflowModel.code = data.code;
                //workflowModel.module = $scope.module;
                //workflowModel.active = data.active;
                //workflowModel.frequency = data.frequency || 'one_time';
                //workflowModel.operation = {};
                //window.diagramData = angular.fromJson(data.diagram_json);

                if (data.record_operations.split(',').length > 0 || data.record_operations.split(',') !== '') {
                    angular.forEach(data.record_operations.split(','), function (operation) {
                        workflowModel.operation[operation] = true;
                    });
                }

                if ($scope.workflowStartModel.filters) {
                    filterReload();

                    $scope.workflowStartModel.filters = $filter('orderBy')(data.filters, 'no');

                    for (var i = 0; i < data.filters.length; i++) {
                        var filter = angular.copy(data.filters[i]);
                        if (filter.field.name)
                            filter.field = filter.field.name;

                        var field = $filter('filter')(workflowModel.module.fields, { name: filter.field }, true)[0];
                        var fieldValue = null;

                        if (!field)
                            return;

                        switch (field.data_type) {
                            case 'picklist':
                                fieldValue = $filter('filter')($scope.modulePicklists[field.picklist_id], { id: filter.value }, true)[0];
                                break;
                            case 'multiselect':
                                var picklistItems = filter.value.split('|');
                                fieldValue = [];

                                angular.forEach(picklistItems, function (picklistLabel) {
                                    var picklist = $filter('filter')($scope.modulePicklists[field.picklist_id], { labelStr: picklistLabel }, true)[0];

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
                                fieldValue = $filter('filter')($scope.modulePicklists['yes_no'], { system_code: filter.value })[0];
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

                        $scope.filters[i] = filter;
                    }
                }
                $scope.modalLoading = false;
                return workflowModel;
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
                    filterValue = filter.value.label[$rootScope.language];
                }
                else {
                    ModuleService.formatFieldValue(filter.field, filter.value, $scope.picklistsActivity);
                    filterValue = angular.copy(filter.field.valueFormatted);
                }

                return filterValue;
            };

            ////$scope.validate = function (tabClick) {
            ////    if (!$scope.workflowForm)
            ////        $scope.workflowForm = tabClick;

            ////    $scope.workflowForm.$submitted = true;
            ////    $scope.validateOperations(tabClick);

            ////    if (!$scope.workflowForm.workflowName.$valid || !$scope.workflowForm.module.$valid || !$scope.workflowForm.operation.$valid)
            ////        return false;

            ////    if ($scope.workflowStartModel.changed_field_checkbox && !$scope.workflowForm.changed_field.$valid)
            ////        return false;
            ////    //TODO 
            ////    return $scope.validateActions($scope.workflowForm);
            ////};

            $scope.validateOperations = function (tabClick) {
                if (!$scope.workflowForm)
                    $scope.workflowForm = tabClick;

                $scope.operationValue = true;
                $scope.workflowForm.operation.$setValidity('invalid', true);

                if (!$scope.workflowStartModel.operation || !Object.keys($scope.workflowStartModel.operation).length) {
                    $scope.operationValue = true;
                    return false;
                }
                else if ($scope.workflowStartModel.operation.insert || $scope.workflowStartModel.operation.delete || $scope.workflowStartModel.operation.update) {
                    $scope.operationValue = false;
                    return true;
                }

                //angular.forEach($scope.workflowStartModel.operation, function (value, key) {
                //    if (value) {
                //        $scope.workflowForm.operation.$setValidity('operations', true);
                //        return true;
                //    }
                //});

                return false;
            };

            ////$scope.validateSendNotification = function () {
            ////    return $scope.workflowModel.send_notification &&
            ////        (($scope.workflowModel.send_notification.recipients && $scope.workflowModel.send_notification.recipients.length) || $scope.workflowModel.send_notification.customRecipient) &&
            ////        $scope.workflowModel.send_notification.subject &&
            ////        $scope.workflowModel.send_notification.message;
            ////};

            ////$scope.validateCreateTask = function () {
            ////    return $scope.workflowModel.create_task &&
            ////        $scope.workflowModel.create_task.owner &&
            ////        $scope.workflowModel.create_task.owner.length === 1 &&
            ////        $scope.workflowModel.create_task.subject &&
            ////        $scope.workflowModel.create_task.task_due_date;
            ////};

            ////$scope.validateUpdateField = function () {
            ////    if ($scope.workflowModel.field_update) {
            ////        if ($scope.workflowModel.field_update.updateOption && $scope.workflowModel.field_update.updateOption === '1') {
            ////            return $scope.workflowModel.field_update.updateOption &&
            ////                $scope.workflowModel.field_update.module &&
            ////                $scope.workflowModel.field_update.field &&
            ////                ($scope.workflowModel.field_update.value !== undefined && $scope.workflowModel.field_update.value !== null);
            ////        } else if ($scope.workflowModel.field_update.updateOption && $scope.workflowModel.field_update.updateOption === '2') {
            ////            return $scope.workflowModel.field_update.updateOption &&
            ////                $scope.workflowModel.field_update.firstModule &&
            ////                $scope.workflowModel.field_update.secondModule &&
            ////                $scope.workflowModel.field_update.first_field &&
            ////                $scope.workflowModel.field_update.second_field;
            ////        }
            ////    }
            ////};

            ////$scope.validateWebHook = function () {
            ////    return $scope.workflowModel.webHook && $scope.workflowModel.webHook.callbackUrl && $scope.workflowModel.webHook.methodType;
            ////};

            ////$scope.sendNotificationIsNullOrEmpty = function () {
            ////    if (!$scope.workflowModel.send_notification)
            ////        return true;

            ////    if ((!$scope.workflowModel.send_notification.recipients || !$scope.workflowModel.send_notification.recipients.length) && !$scope.workflowModel.send_notification.subject && !$scope.workflowModel.send_notification.schedule && !$scope.workflowModel.send_notification.message)
            ////        return true;

            ////    return false;
            ////};

            ////$scope.createTaskIsNullOrEmpty = function () {
            ////    if (!$scope.workflowModel.create_task)
            ////        return true;

            ////    if ((!$scope.workflowModel.create_task.owner || !$scope.workflowModel.create_task.owner.length) && !$scope.workflowModel.create_task.subject && !$scope.workflowModel.create_task.task_due_date && !$scope.workflowModel.create_task.task_status && !$scope.workflowModel.create_task.task_priority && !$scope.workflowModel.create_task.task_notification && !$scope.workflowModel.create_task.description)
            ////        return true;

            ////    return false;
            ////};


            ////$scope.fieldUpdateIsNullOrEmpty = function () {
            ////    if (!$scope.workflowModel.field_update)
            ////        return true;

            ////    if ($scope.workflowModel.field_update.updateOption === '1') {
            ////        if (!$scope.workflowModel.field_update.module && !$scope.workflowModel.field_update.field && !$scope.workflowModel.field_update.value)
            ////            return true;
            ////    } else {
            ////        if (!$scope.workflowModel.field_update.firstModule && !$scope.workflowModel.field_update.secondModule && !$scope.workflowModel.field_update.first_field && !$scope.workflowModel.field_update.second_field)
            ////            return true;
            ////    }

            ////    return false;
            ////};

            ////$scope.webHookIsNullOrEmpty = function () {
            ////    if (!$scope.workflowModel.webHook)
            ////        return true;

            ////    if (!$scope.workflowModel.webHook.callbackUrl && !$scope.workflowModel.webHook.methodType)
            ////        return true;

            ////    return false;
            ////};

            $scope.getUpdatableModules = function () {
                $scope.updatableModules = [];
                ModuleService.getModuleFields($scope.workflowStartModel.module.name)
                    .then(function (response) {
                        if (response.data)
                            $scope.workflowStartModel.module.fields = response.data;

                        $scope.updatableModules.push($scope.workflowStartModel.module);

                        angular.forEach($scope.workflowStartModel.module.fields, function (field) {
                            if (field.lookup_type && field.lookup_type !== $scope.workflowStartModel.module.name && field.lookup_type !== 'users' && !field.deleted) {
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
                    currentModule = $scope.workflowStartModel.module;

                notificationObj.module = currentModule;
                notificationObj.name = currentModule['label_' + $rootScope.language + '_singular'];
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
                            notificationObj.name = field['label_' + $rootScope.language] + ' ' + '(' + notificationObj.module['label_' + $rootScope.language + '_singular'] + ')';
                            notificationObj.isSameModule = false;
                            notificationObj.systemName = field.name;
                            notificationObj.id = id;
                        } else {
                            notificationObj.module = $filter('filter')($rootScope.appModules, { name: field.lookup_type }, true)[0];
                            notificationObj.name = field['label_' + $rootScope.language] + ' ' + '(' + notificationObj.module['label_' + $rootScope.language + '_singular'] + ')';
                            notificationObj.isSameModule = false;
                            notificationObj.systemName = field.name;
                            notificationObj.id = id;
                        }
                        updatableModulesForNotification.push(notificationObj);
                        id++;
                    }
                });

                var fieldUpdateModulesForNotification = angular.copy(updatableModulesForNotification);

                notificationObj.module = getFakeUserModule();
                notificationObj.name = "User";
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
                    currentModule = $scope.workflowStartModel.module;

                updateObj.module = currentModule;
                updateObj.name = currentModule['label_' + $rootScope.language + '_singular'];
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
                            updateObj.name = field['label_' + $rootScope.language] + ' ' + '(' + updateObj.module['label_' + $rootScope.language + '_singular'] + ')';
                            updateObj.isSameModule = false;
                            updateObj.systemName = field.name;
                            updateObj.id = id;
                        } else {
                            updateObj.module = $filter('filter')($rootScope.appModules, { name: field.lookup_type }, true)[0];
                            updateObj.name = field['label_' + $rootScope.language] + ' ' + '(' + updateObj.module['label_' + $rootScope.language + '_singular'] + ')';
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
                    $scope.workflowStartModel.frequency = 'continuous';

                    if ($scope.id)
                        $scope.workflowStartModel.delete_logs = false;
                }

                $scope.workflowStartModel.changed_field_checkbox = false;
                $scope.workflowStartModel.changed_field = null;

            };

            $scope.changeFieldCheckboxChanged = function (status) {
                if (!status) {
                    $scope.workflowStartModel.changed_field = null;
                }
            };

            $scope.frequencyChanged = function (frequency) {
                if ($scope.id) {
                    if (frequency === 'continuous')
                        $scope.workflowStartModel.delete_logs = false;
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

            ////$scope.validateActions = function (tabClick) {
            ////    if (!$scope.lastStepClicked && !tabClick) {
            ////        tabClick.$submitted = false;
            ////        return true;
            ////    }
            ////    var sendNotificationIsNullOrEmpty = $scope.sendNotificationIsNullOrEmpty();
            ////    var createTaskIsNullOrEmpty = $scope.createTaskIsNullOrEmpty();
            ////    var fieldUpdateIsNullOrEmpty = $scope.fieldUpdateIsNullOrEmpty();
            ////    var webHookIsNullOrEmpty = $scope.webHookIsNullOrEmpty();

            ////    tabClick.actions.$setValidity('actionRequired', true);

            ////    if (sendNotificationIsNullOrEmpty && createTaskIsNullOrEmpty && fieldUpdateIsNullOrEmpty && webHookIsNullOrEmpty) {
            ////        if (tabClick) {
            ////            tabClick.$submitted = false;
            ////            return true;
            ////        }

            ////        tabClick.$submitted = false;

            ////        if (tabClick.recipients)
            ////            tabClick.recipients.$setValidity('minTags', true);

            ////        if (tabClick.customRecipient)
            ////            tabClick.customRecipient.$setValidity('required', true);

            ////        tabClick.subjectNotification.$setValidity('required', true);
            ////        tabClick.message.$setValidity('required', true);
            ////        tabClick.owner.$setValidity('required', true);
            ////        tabClick.subjectTask.$setValidity('required', true);
            ////        tabClick.dueDate.$setValidity('required', true);
            ////        tabClick.updateOption.$setValidity('required', true);
            ////        tabClick.callbackUrl.$setValidity('required', true);
            ////        tabClick.methodType.$setValidity('required', true);


            ////        if (tabClick.updateField && $scope.workflowModel.field_update.updateOption === '1')
            ////            tabClick.updateField.$setValidity('required', true);

            ////        if (tabClick.updateValue && $scope.workflowModel.field_update.updateOption === '1')
            ////            tabClick.updateValue.$setValidity('required', true);

            ////        tabClick.actions.$setValidity('actionRequired', false);
            ////        return false;
            ////    }

            ////    if (tabClick.subjectNotification.$pristine && tabClick.message.$pristine &&
            ////        tabClick.owner.$pristine && tabClick.subjectTask.$pristine && tabClick.dueDate.$pristine &&
            ////        (tabClick.updateField && tabClick.updateField.$pristine) && (tabClick.updateValue && tabClick.updateValue.$pristine) &&
            ////        tabClick.callbackUrl.$pristine) {
            ////        tabClick.$submitted = false;
            ////        return true;
            ////    }

            ////    if (!sendNotificationIsNullOrEmpty && (!$scope.workflowModel.send_notification.recipients || !$scope.workflowModel.send_notification.recipients.length) && !$scope.workflowModel.send_notification.customRecipient)
            ////        tabClick.recipients.$setValidity('minTags', false);

            ////    if (!sendNotificationIsNullOrEmpty && !$scope.workflowModel.send_notification.subject)
            ////        tabClick.subjectNotification.$setValidity('required', false);

            ////    if (!sendNotificationIsNullOrEmpty && !$scope.workflowModel.send_notification.message)
            ////        tabClick.message.$setValidity('required', false);

            ////    if (!createTaskIsNullOrEmpty && (!$scope.workflowModel.create_task.owner || !$scope.workflowModel.create_task.owner.length))
            ////        tabClick.owner.$setValidity('minTags', false);

            ////    if (!createTaskIsNullOrEmpty && !$scope.workflowModel.create_task.subject)
            ////        tabClick.subjectTask.$setValidity('required', false);

            ////    if (!createTaskIsNullOrEmpty && !$scope.workflowModel.create_task.task_due_date)
            ////        tabClick.dueDate.$setValidity('required', false);

            ////    if (!fieldUpdateIsNullOrEmpty && $scope.workflowModel.field_update.updateOption === '1' && $scope.workflowModel.field_update.module && !$scope.workflowModel.field_update.field)
            ////        tabClick.updateField.$setValidity('required', false);

            ////    if (!fieldUpdateIsNullOrEmpty && $scope.workflowModel.field_update.updateOption === '1' && $scope.workflowModel.field_update.module && $scope.workflowModel.field_update.field && ($scope.workflowModel.field_update.value === undefined || $scope.workflowModel.field_update.value === null))
            ////        tabClick.updateValue.$setValidity('required', false);

            ////    if (!webHookIsNullOrEmpty && !$scope.workflowModel.webHook.callbackUrl) {
            ////        tabClick.callbackUrl.$setValidity('required', false);
            ////    }
            ////    if (!webHookIsNullOrEmpty && !$scope.workflowModel.webHook.methodType) {
            ////        tabClick.methodType.$setValidity('required', false);
            ////    }

            ////    var isSendNotificationValid = $scope.validateSendNotification();
            ////    var isCreateTaskValid = $scope.validateCreateTask();
            ////    var isUpdateFieldValid = $scope.validateUpdateField();
            ////    var isWebhookValid = $scope.validateWebHook();

            ////    if ((isSendNotificationValid && isCreateTaskValid && isUpdateFieldValid && isWebhookValid) ||
            ////        (isSendNotificationValid && isCreateTaskValid) ||
            ////        (isSendNotificationValid && isUpdateFieldValid) ||
            ////        (isCreateTaskValid && isUpdateFieldValid) ||
            ////        (isWebhookValid && isSendNotificationValid) ||
            ////        (isWebhookValid && isUpdateFieldValid) ||
            ////        (isWebhookValid && isCreateTaskValid) ||
            ////        (isSendNotificationValid || isCreateTaskValid || isUpdateFieldValid || isWebhookValid)) {
            ////        tabClick.$submitted = false;
            ////        return true;
            ////    }

            ////    return false;
            ////};

            ////$scope.setFormValid = function (form) {
            //if (!$scope.workflowForm)
            //    $scope.workflowForm = form;

            //$scope.workflowForm.$submitted = false;

            //if ($scope.workflowForm.recipients)
            //    $scope.workflowForm.recipients.$setValidity('minTags', true);

            //$scope.workflowForm.subjectNotification.$setValidity('required', true);
            //$scope.workflowForm.message.$setValidity('required', true);
            //$scope.workflowForm.owner.$setValidity('minTags', true);
            //$scope.workflowForm.subjectTask.$setValidity('required', true);
            //$scope.workflowForm.dueDate.$setValidity('required', true);
            //$scope.workflowForm.actions.$setValidity('actionRequired', true);
            ////$scope.workflowForm.updateOption.$setValidity('required', true);
            //$scope.workflowForm.callbackUrl.$setValidity('required', true);

            //if ($scope.workflowForm.updateField && $scope.workflowModel.field_update.updateOption === '1')
            //    $scope.workflowForm.updateField.$setValidity('required', true);

            //if ($scope.workflowForm.updateValue && $scope.workflowModel.field_update.updateOption === '1')
            //    $scope.workflowForm.updateValue.$setValidity('required', true);
            //// };

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
                parameter.selectedModule = $scope.workflowStartModel.module;

                $scope.hookParameters.push(parameter);
            };

            $scope.workflowModuleParameterAdd = function (addItem, workflowForm) {
                workflowForm.$submitted = true;

                var parameter = {};
                parameter.parameterName = addItem.parameterName;
                parameter.selectedModules = addItem.selectedModules;
                parameter.selectedField = addItem.selectedField;

                if (parameter.parameterName && parameter.selectedModules && parameter.selectedField) {
                    if ($scope.hookParameters.length <= 10) {
                        $scope.hookParameters.push(parameter);
                    }
                    else {
                        toastr.warning($filter('translate')('Setup.BpmWorkflow.MaximumHookWarning'));
                    }
                }
                var lastHookParameter = $scope.hookParameters[$scope.hookParameters.length - 1];
                lastHookParameter.parameterName = null;
                lastHookParameter.selectedField = null;
                lastHookParameter.selectedModule = $scope.workflowStartModel.module;


            };

            $scope.workflowModuleParameterRemove = function (itemName) {
                var index = $scope.hookParameters.indexOf(itemName);
                $scope.hookParameters.splice(index, 1);
            };

            $scope.getSummary = function () {
                if (!$scope.workflowStartModel.name || !$scope.workflowStartModel.module || !$scope.workflowStartModel.operation)
                    return;

                var getSummary = function () {
                    $scope.ruleTriggerText = '';
                    $scope.ruleFilterText = '';
                    $scope.ruleActionsText = '';
                    var andText = $filter('translate')('Common.And');
                    var orText = $filter('translate')('Common.Or');

                    if ($scope.workflowStartModel.operation.insert)
                        $scope.ruleTriggerText += $filter('translate')('Setup.Workflow.RuleTriggers.insertLabel') + ' ' + orText + ' ';

                    if ($scope.workflowStartModel.operation.update) {
                        var updateText = $filter('translate')('Setup.Workflow.RuleTriggers.updateLabel') + ' ' + orText + ' ';
                        $scope.ruleTriggerText += $scope.ruleTriggerText ? updateText.toLowerCase() : updateText;
                    }

                    if ($scope.workflowStartModel.operation.delete) {
                        var deleteText = $filter('translate')('Setup.Workflow.RuleTriggers.deleteLabel') + ' ' + orText + ' ';
                        $scope.ruleTriggerText += $scope.ruleTriggerText ? deleteText.toLowerCase() : deleteText;
                    }

                    $scope.ruleTriggerText = $scope.ruleTriggerText.slice(0, -(orText.length + 2));

                    angular.forEach($scope.filters, function (filter) {
                        if (!filter.field || !filter.operator || !filter.value)
                            return;

                        $scope.ruleFilterText += filter.field['label_' + $rootScope.language] + ' <b class="operation-highlight">' + filter.operator.label[$rootScope.language] + '</b> ' +
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
                                    $scope.updateFieldValue = $scope.workflowModel.field_update.value.label[$rootScope.language];
                                    value = $scope.updateFieldValue;
                                    break;
                                case 'multiselect':
                                    $scope.updateFieldValue = '';

                                    angular.forEach($scope.workflowModel.field_update.value, function (picklistItem) {
                                        var label = picklistItem.label[$rootScope.language];
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
                                    value = yesNoPicklistItem.label[$rootScope.language];
                                    $scope.updateFieldValue = fieldValue;
                                    break;
                                default:
                                    $scope.updateFieldValue = updateFieldRecordFake[$scope.workflowModel.field_update.field.name];
                                    break;
                            }

                            $scope.ruleActionsText += $filter('translate')('Setup.Workflow.FieldUpdateSummary', { module: $scope.workflowModel.field_update.module['label_' + $rootScope.language + '_singular'], field: $scope.workflowModel.field_update.field['label_' + $rootScope.language], value: value }) + '<br>';
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

            //var stepValidateControl = function (nodeName, workflowForm) {
            //    switch (nodeName) {
            //        case 'start':
            //            if (!workflowForm)
            //                return false;

            //            if (!workflowForm.$valid)
            //                return false;

            //        case 'field_update':

            //        default:
            //            break;
            //    }
            //};

            $scope.saveNodeData = function (workflowForm) {
                workflowForm.$submitted = true;
                var currentNode = $scope.currentObj.subject.part.data;
                var nodeName = currentNode.ngModelName;

                if (!workflowForm.$valid) {
                    if (workflowForm.$error.required)
                        toastr.warning($filter('translate')('Module.RequiredError'));
                    else if (workflowForm.$error.maxlength)
                        toastr.warning($filter('translate')('Module.MaxError'));
                    else
                        toastr.warning($filter('translate')('Module.RequiredError'));

                    return false;
                }

                $scope.saving = true;
                $scope.modalLoading = true;
                var diagram = window.myDiagram.model;

                var bpmModel = $scope.workflowModel;
                var startModel = $scope.workflowStartModel;
                var data = {};

                if (!diagram)
                    return false;

                switch (currentNode.ngModelName) {
                    case 'start':
                        data = startModel;
                        data.module_id = startModel.module.id;
                        data.id = $scope.id;
                        data.filters = [];

                        var filters = angular.copy($scope.filters);

                        if (filters && filters.length) {
                            angular.forEach(filters, function (filterItem) {
                                if (!filterItem.field || !filterItem.operator)
                                    return;

                                if (!(filterItem.operator.name === 'empty' || filterItem.operator.name === 'not_empty') && (filterItem.value === null || filterItem.value === undefined))
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
                        }


                        data.process_filter = startModel.processFilter === "none" ? null : startModel.processFilter;
                        data.trigger_type = "record";

                        var loopArray = [];
                        angular.forEach(startModel.operation, function (value, key) {
                            if (value)
                                this.push(key);
                        }, loopArray);

                        data.record_operations = loopArray.join();
                        data.canstartmanuel = false;
                        data.defitinion_json = {};


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
                            angular.forEach(data[currentNode.ngModelName].cc, function (user) {
                                cc.push(user.email);
                            });
                            data[currentNode.ngModelName].cc = cc;
                        }
                        else if (workflowModel.send_notification_ccmodule && workflowModel.customCC) {
                            workflowModel.cc = [];
                            if (workflowModel.send_notification_ccmodule.module.name === startModel.module.name && workflowModel.send_notification_ccmodule.isSameModule) {
                                data[currentNode.ngModelName].cc.push(workflowModel.customCC.name);
                            }
                            else if (workflowModel.send_notification_ccmodule.module.name === startModel.module.name && !workflowModel.send_notification_ccmodule.isSameModule) {
                                var sameModuleLookupsCC = $filter('filter')(startModel.module.fields, { lookup_type: workflowModel.send_notification_ccmodule.module.name }, true);
                                data[currentNode.ngModelName].cc.push($filter('filter')(sameModuleLookupsCC, { name: workflowModel.send_notification_ccmodule.systemName }, true)[0].name + '.' + workflowModel.customCC.name);
                            }
                            else {
                                var lookupsCC = $filter('filter')(startModel.module.fields, { lookup_type: workflowModel.send_notification_ccmodule.module.name }, true);
                                data[currentNode.ngModelName].cc.push($filter('filter')(lookupsCC, { name: workflowModel.send_notification_ccmodule.systemName }, true)[0].name + '.' + workflowModel.customCC.name);
                            }
                        }

                        var bcc = [];
                        if (workflowModel.bcc) {
                            angular.forEach(data[currentNode.ngModelName].bcc, function (user) {
                                bcc.push(user.email);
                            });
                            data[currentNode.ngModelName].bcc = bcc;
                        }
                        else if (workflowModel.send_notification_bccmodule && workflowModel.customBcc) {
                            workflowModel.bcc = [];
                            if (workflowModel.send_notification_bccmodule.module.name === startModel.module.name && workflowModel.send_notification_bccmodule.isSameModule) {
                                data[currentNode.ngModelName].bcc.push(workflowModel.customBcc.name);
                            }
                            else if (workflowModel.send_notification_bccmodule.module.name === startModel.module.name && !workflowModel.send_notification_bccmodule.isSameModule) {
                                var sameModuleLookupsBcc = $filter('filter')(startModel.module.fields, { lookup_type: workflowModel.send_notification_bccmodule.module.name }, true);
                                data[currentNode.ngModelName].bcc.push($filter('filter')(sameModuleLookupsBcc, { name: workflowModel.send_notification_bccmodule.systemName }, true)[0].name + '.' + workflowModel.customBcc.name);
                            }
                            else {
                                var lookupsBcc = $filter('filter')(startModel.module.fields, { lookup_type: workflowModel.send_notification_bccmodule.module.name }, true);
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

                            if (workflowModel.send_notification_module.module.name === startModel.module.name && workflowModel.send_notification_module.isSameModule) {
                                data[currentNode.ngModelName].recipients.push(workflowModel.customRecipient.name);
                            }
                            else if (workflowModel.send_notification_module.module.name === startModel.module.name && !workflowModel.send_notification_module.isSameModule) {
                                var sameModuleLookups = $filter('filter')(startModel.module.fields, { lookup_type: workflowModel.send_notification_module.module.name }, true);
                                data[currentNode.ngModelName].recipients.push($filter('filter')(sameModuleLookups, { name: workflowModel.send_notification_module.systemName }, true)[0].name + '.' + workflowModel.customRecipient.name);
                            }
                            else {
                                var lookups = $filter('filter')(startModel.module.fields, { lookup_type: workflowModel.send_notification_module.module.name }, true);

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
                    $scope.modalLoading = false;
                    //$scope.triggerBpm();
                    $scope.$digest();
                    $scope.cancel();
                }, 2000);
            };

            $scope.save = function () {
                $scope.saving = true;
                var diagram = window.myDiagram.model;
                var bpmModel = $scope.workflowModel;
                var startModel = $scope.workflowStartModel;

                if (diagram.nodeDataArray.length > 0 && diagram.linkDataArray.length > 0) {
                    var startControl = $filter('filter')(diagram.nodeDataArray, { item: 'Start' }, true)[0];

                    if (!startControl) {
                        toastr.warning($filter('translate')('Setup.BpmWorkflow.MustBeStartNode'));
                        $scope.saving = false;
                        return;
                    }
                    else {
                        if (!$scope.workflowStartModel.module && !$scope.workflowStartModel.name && !$scope.workflowStartModel.code) {
                            toastr.warning($filter('translate')('Setup.BpmWorkflow.MustBeStartNode'));//TODO translate
                            $scope.saving = false;
                            return;
                        }
                    }

                    var data = {};
                    data.active = startModel.active;
                    data.name = startModel.name;
                    data.code = startModel.code;
                    data.module_id = startModel.module.id;
                    data.id = $scope.id;
                    data.trigger_type = "record";
                    data.frequency = startModel.frequency;
                    data.canstartmanuel = false;
                    data.defitinion_json = {};
                    data.diagram_json = diagram.toJSON();

                    var loopArray = [];

                    angular.forEach(startModel.operation, function (value, key) {
                        if (value)
                            this.push(key);
                    }, loopArray);

                    data.record_operations = loopArray.join();
                    var filters = $scope.filters;

                    if (filters && filters.length) {
                        data.filters = [];

                        angular.forEach(filters, function (filterItem) {
                            if (!filterItem.field || !filterItem.operator)
                                return;

                            if (!(filterItem.operator.name === 'empty' || filterItem.operator.name === 'not_empty') && (filterItem.value === null || filterItem.value === undefined))
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

                                if (field.data_type === 'lookup' && field.lookup_type !== 'users')
                                    filter.value = filter.value.id;

                                if (field.data_type === 'lookup' && field.lookup_type === 'users')
                                    filter.value = filter.value[0].id;

                                if (field.data_type === 'checkbox')
                                    filter.value = filter.value.system_code;
                            }
                            else {
                                filter.value = '-';
                            }

                            data.filters.push(filter);
                        });
                    }

                    if ($scope.id) {
                        AdvancedWorkflowsService.update(data)
                            .then(function (response) {
                                if (response.data) {
                                    success();
                                }
                            }).catch(function onError() {
                                $scope.saving = false;
                            });
                    }
                    else {
                        AdvancedWorkflowsService.getByCode(data.code)
                            .then(function (response) {
                                if (response.data.length > 1)
                                    $scope.saving = false;
                                else {
                                    if (!$scope.id) {
                                        AdvancedWorkflowsService.create(data)
                                            .then(function onSuccess() {
                                                success();
                                            }).catch(function onError() {
                                                $scope.saving = false;
                                            });
                                    }
                                }
                            });
                    }
                }
                else {
                    toastr.warning($filter('translate')('Setup.BpmWorkflow.NotEmptyModel'));
                    $scope.saving = false;
                }
            };

            var success = function () {
                $scope.saving = false;
                $state.go('studio.app.advancedWorkflows');
                toastr.success($filter('translate')('Setup.BpmWorkflow.SubmitSuccess'));
            };

            $scope.workflowCodeGenerate = function () {
                if (!$scope.workflowStartModel.name)
                    $scope.workflowStartModel.code = '';
                else {
                    var tempCode = $scope.workflowStartModel.name.trim();
                    $scope.workflowStartModel.code = tempCode.replace(/ /g, '_');
                }

            };


            //For Editor
            $scope.searchTags = function (term) {

                if (!$scope.moduleFields)
                    $scope.moduleFields = AdvancedWorkflowsService.getFields($scope.module);

                var tagsList = [];
                angular.forEach($scope.moduleFields, function (item) {
                    if (item.name === "seperator")
                        return;

                    if (item.name.match('seperator'))
                        item.name = item.label;

                    if (item.name && item.name.indexOf(term) >= 0) {
                        tagsList.push(item);
                    }
                });

                $scope.tags = tagsList;
                return tagsList;
            };

            var dialog_uid = plupload.guid();

            // uploader configuration for image files.
            $scope.imgUpload = {
                settings: {
                    multi_selection: false,
                    url: config.apiUrl + 'Document/upload_attachment',
                    headers: {
                        'Authorization': 'Bearer ' + $localStorage.read('access_token'),
                        'Accept': 'application/json',
                        'X-Tenant-Id': $cookies.get('tenant_id')
                    },
                    multipart_params: {
                        container: dialog_uid
                    },
                    filters: {
                        mime_types: [
                            { title: "Image files", extensions: "jpg,gif,png" },
                        ],
                        max_file_size: "2mb"
                    },
                    resize: { quality: 90 }
                },
                events: {
                    filesAdded: function (uploader, files) {
                        uploader.start();
                        tinymce.activeEditor.windowManager.open({
                            title: $filter('translate')('Common.PleaseWait'),
                            width: 50,
                            height: 50,
                            body: [
                                {
                                    type: 'container',
                                    name: 'container',
                                    label: '',
                                    html: '<span>' + $filter('translate')('EMail.UploadingAttachment') + '</span>'
                                },
                            ],
                            buttons: []
                        });
                    },
                    uploadProgress: function (uploader, file) {
                    },
                    fileUploaded: function (uploader, file, response) {
                        tinymce.activeEditor.windowManager.close();
                        var resp = JSON.parse(response.response);
                        uploadSuccessCallback(resp.public_url, { alt: file.name });
                        uploadSuccessCallback = null;
                    },
                    error: function (file, error) {
                        switch (error.code) {
                            case -600:
                                tinymce.activeEditor.windowManager.alert($filter('translate')('EMail.MaxImageSizeExceeded'));
                                break;
                            default:
                                break;
                        }
                        if (uploadFailedCallback) {
                            uploadFailedCallback();
                            uploadFailedCallback = null;
                        }
                    }
                }
            };

            $scope.fileUpload = {
                settings: {
                    multi_selection: false,
                    unique_names: false,
                    url: config.apiUrl + 'Document/upload_attachment',
                    headers: {
                        'Authorization': 'Bearer ' + $localStorage.read('access_token'),
                        'Accept': 'application/json',
                        'X-Tenant-Id': $cookies.get('tenant_id')
                    },
                    multipart_params: {
                        container: dialog_uid
                    },
                    filters: {
                        mime_types: [
                            { title: "Email Attachments", extensions: "pdf,doc,docx,xls,xlsx,csv" },
                        ],
                        max_file_size: "50mb"
                    }
                },
                events: {
                    filesAdded: function (uploader, files) {
                        uploader.start();
                        tinymce.activeEditor.windowManager.open({
                            title: $filter('translate')('Common.PleaseWait'),
                            width: 50,
                            height: 50,
                            body: [
                                {
                                    type: 'container',
                                    name: 'container',
                                    label: '',
                                    html: '<span>' + $filter('translate')('EMail.UploadingAttachment') + '</span>'
                                },
                            ],
                            buttons: []
                        });
                    },
                    uploadProgress: function (uploader, file) {
                    },
                    fileUploaded: function (uploader, file, response) {
                        var resp = JSON.parse(response.response);
                        uploadSuccessCallback(resp.public_url, { alt: file.name });
                        uploadSuccessCallback = null;
                        tinymce.activeEditor.windowManager.close();
                    },
                    error: function (file, error) {
                        switch (error.code) {
                            case -600:
                                tinymce.activeEditor.windowManager.alert($filter('translate')('EMail.MaxFileSizeExceeded'));
                                break;
                            default:
                                break;
                        }
                        if (uploadFailedCallback) {
                            uploadFailedCallback();
                            uploadFailedCallback = null;
                        }
                    }
                }
            };

            $scope.iframeElement = {};
            /// tinymce editor configuration.
            $scope.tinymceOptions = function (scope) {
                $scope[scope] = {
                    setup: function (editor) {
                        editor.addButton('addParameter', {
                            type: 'button',
                            text: $filter('translate')('EMail.AddParameter'),
                            onclick: function () {
                                tinymce.activeEditor.execCommand('mceInsertContent', false, '#');
                            }
                        });
                        editor.on("init", function () {
                            $scope.loadingModal = false;
                        });
                    },
                    onChange: function (e) {
                        debugger;
                        // put logic here for keypress and cut/paste changes
                    },
                    inline: false,
                    height: 300,
                    language: $rootScope.language,
                    plugins: [
                        "advlist autolink lists link image charmap print preview anchor table",
                        "searchreplace visualblocks code fullscreen",
                        "insertdatetime table contextmenu paste imagetools wordcount textcolor colorpicker"
                    ],
                    imagetools_cors_hosts: ['crm.ofisim.com', 'test.ofisim.com', 'ofisimcomdev.blob.core.windows.net'],
                    toolbar: "addParameter | styleselect | bold italic underline strikethrough | forecolor backcolor | alignleft aligncenter alignright alignjustify | table bullist numlist | link image imagetools |  cut copy paste | undo redo searchreplace | outdent indent | blockquote hr insertdatetime charmap | visualblocks code preview fullscreen",
                    menubar: 'false',
                    templates: [
                        { title: 'Test template 1', content: 'Test 1' },
                        { title: 'Test template 2', content: 'Test 2' }
                    ],
                    skin: 'lightgray',
                    theme: 'modern',

                    file_picker_callback: function (callback, value, meta) {
                        // Provide file and text for the link dialog
                        uploadSuccessCallback = callback;

                        if (meta.filetype == 'file') {
                            var uploadButton = document.getElementById('uploadFile');
                            uploadButton.click();
                        }

                        // Provide image and alt text for the image dialog
                        if (meta.filetype == 'image') {
                            var uploadButton = document.getElementById('uploadImage');
                            uploadButton.click();
                        }
                    },
                    image_advtab: true,
                    file_browser_callback_types: 'image file',
                    paste_data_images: true,
                    paste_as_text: true,
                    spellchecker_language: $rootScope.language,
                    images_upload_handler: function (blobInfo, success, failure) {
                        var blob = blobInfo.blob();
                        uploadSuccessCallback = success;
                        uploadFailedCallback = failure;
                        $scope.imgUpload.uploader.addFile(blob);
                        ///TODO: in future will be implemented to upload pasted data images into server.
                    },
                    init_instance_callback: function (editor) {
                        $scope.iframeElement[scope] = editor.iframeElement;
                    },
                    resize: false,
                    width: '99,9%',
                    toolbar_items_size: 'small',
                    statusbar: false,
                    convert_urls: false,
                    remove_script_host: false
                };
            };

            $scope.tinymceOptions('tinymceTemplate');
            $scope.tinymceOptions('tinymceTemplateEdit');
            //For Editor




            var getFakeUserModule = function () {
                var userModule = {};
                userModule.id = 999;
                userModule.name = 'users';
                userModule.system_type = 'system';
                userModule.order = 999;
                userModule.display = false;
                userModule.label_en_singular = 'User';
                userModule.label_en_plural = 'Users';
                userModule.label_tr_singular = 'Kullanıcı';
                userModule.label_tr_plural = 'Kullanıcılar';
                userModule.menu_icon = 'fa fa-users';
                userModule.sections = [];
                userModule.fields = [];

                var section = {};
                section.name = 'user_information';
                section.system_type = 'system';
                section.order = 1;
                section.column_count = 1;
                section.label_en = 'User Information';
                section.label_tr = 'Kullanıcı Bilgisi';
                section.display_form = true;
                section.display_detail = true;

                var fieldEmail = {};
                fieldEmail.name = 'email';
                fieldEmail.system_type = 'system';
                fieldEmail.data_type = 'email';
                fieldEmail.order = 2;
                fieldEmail.section = 1;
                fieldEmail.section_column = 1;
                fieldEmail.primary = false;
                fieldEmail.inline_edit = true;
                fieldEmail.label_en = 'Email';
                fieldEmail.label_tr = 'Eposta';
                fieldEmail.display_list = true;
                fieldEmail.display_form = true;
                fieldEmail.display_detail = true;
                userModule.fields.push(fieldEmail);

                var fieldFirstName = {};
                fieldFirstName.name = 'first_name';
                fieldFirstName.system_type = 'system';
                fieldFirstName.data_type = 'text_single';
                fieldFirstName.order = 3;
                fieldFirstName.section = 1;
                fieldFirstName.section_column = 1;
                fieldFirstName.primary = false;
                fieldFirstName.inline_edit = true;
                fieldFirstName.editable = true;
                fieldFirstName.show_label = true;
                fieldFirstName.label_en = 'First Name';
                fieldFirstName.label_tr = 'Adı';
                fieldFirstName.display_list = true;
                fieldFirstName.display_form = true;
                fieldFirstName.display_detail = true;
                userModule.fields.push(fieldFirstName);

                var fieldLastName = {};
                fieldLastName.name = 'last_name';
                fieldLastName.system_type = 'system';
                fieldLastName.data_type = 'text_single';
                fieldLastName.order = 4;
                fieldLastName.section = 1;
                fieldLastName.section_column = 1;
                fieldLastName.primary = false;
                fieldLastName.inline_edit = true;
                fieldLastName.editable = true;
                fieldLastName.show_label = true;
                fieldLastName.label_en = 'Last Name';
                fieldLastName.label_tr = 'Soyadı';
                fieldLastName.display_list = true;
                fieldLastName.display_form = true;
                fieldLastName.display_detail = true;
                userModule.fields.push(fieldLastName);

                var fieldFullName = {};
                fieldFullName.name = 'full_name';
                fieldFullName.system_type = 'system';
                fieldFullName.data_type = 'text_single';
                fieldFullName.order = 5;
                fieldFullName.section = 1;
                fieldFullName.section_column = 1;
                fieldFullName.primary = true;
                fieldFullName.inline_edit = true;
                fieldFullName.editable = true;
                fieldFullName.show_label = true;
                fieldFullName.label_en = 'Name';
                fieldFullName.label_tr = 'Adı Soyadı';
                fieldFullName.display_list = true;
                fieldFullName.display_form = true;
                fieldFullName.display_detail = true;
                fieldFullName.combination = {};
                fieldFullName.combination.field_1 = 'first_name';
                fieldFullName.combination.field_2 = 'last_name';
                userModule.fields.push(fieldFullName);

                var fieldPhone = {};
                fieldPhone.name = 'phone';
                fieldPhone.system_type = 'system';
                fieldPhone.data_type = 'text_single';
                fieldPhone.order = 6;
                fieldPhone.section = 1;
                fieldPhone.section_column = 1;
                fieldPhone.primary = false;
                fieldPhone.inline_edit = true;
                fieldPhone.label_en = 'Phone';
                fieldPhone.label_tr = 'Telefon';
                fieldPhone.display_list = true;
                fieldPhone.display_form = true;
                fieldPhone.display_detail = true;
                userModule.fields.push(fieldPhone);

                var fieldProfileId = {};
                fieldProfileId.name = 'profile_id';
                fieldProfileId.system_type = 'system';
                fieldProfileId.data_type = 'number';
                fieldProfileId.order = 6;
                fieldProfileId.section = 1;
                fieldProfileId.section_column = 1;
                fieldProfileId.primary = false;
                fieldProfileId.inline_edit = true;
                fieldProfileId.editable = true;
                fieldProfileId.show_label = true;
                fieldProfileId.label_en = 'Profile Id';
                fieldProfileId.label_tr = 'Profile Id';
                fieldProfileId.display_list = true;
                fieldProfileId.display_form = true;
                fieldProfileId.display_detail = true;
                userModule.fields.push(fieldProfileId);

                var fieldRoleId = {};
                fieldRoleId.name = 'role_id';
                fieldRoleId.system_type = 'system';
                fieldRoleId.data_type = 'number';
                fieldRoleId.order = 7;
                fieldRoleId.section = 1;
                fieldRoleId.section_column = 1;
                fieldRoleId.primary = false;
                fieldRoleId.inline_edit = true;
                fieldRoleId.editable = true;
                fieldRoleId.show_label = true;
                fieldRoleId.label_en = 'Role Id';
                fieldRoleId.label_tr = 'Role Id';
                fieldRoleId.display_list = true;
                fieldRoleId.display_form = true;
                fieldRoleId.display_detail = true;
                userModule.fields.push(fieldRoleId);

                return userModule;
            };
        }
    ]);