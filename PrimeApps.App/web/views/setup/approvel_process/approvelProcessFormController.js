'use strict';

angular.module('ofisim')

    .controller('ApprovelProcessFormController', ['$rootScope', '$location', '$scope', '$filter', 'ngToast', 'helper', 'blockUI', '$state', 'operators', '$q', 'ModuleService', 'ApprovelProcessService',
        function ($rootScope, $location, $scope, $filter, ngToast, helper, blockUI, $state, operators, $q, ModuleService, ApprovelProcessService) {
            $scope.loading = true;
            $scope.wizardStep = 0;
            $scope.id = $location.search().id;
            $scope.hookParameters = [];
            $scope.approvers = $rootScope.workgroup.users;
            $scope.approversLength = $rootScope.workgroup.users.length;
            $scope.scheduleItems = ApprovelProcessService.getScheduleItems();
            $scope.dueDateItems = ApprovelProcessService.getDueDateItems();
            $scope.isEdit = false;
            $scope.isChosenModule = true;
            $scope.users = angular.copy($rootScope.workgroup.users);
            $scope.$parent.collapsed = true;

            if(!$filter('filter')($scope.users, { Id: 0 }, true)[0])
                $scope.users.unshift({Id: 0, email: $filter('translate')('Setup.Workflow.ApprovelProcess.AllUsers')});

            var activityModule = $filter('filter')($rootScope.modules, { name: 'activities' }, true)[0];

            var setTaskFields = function () {
                $scope.taskFields = {};
                $scope.taskFields.owner = $filter('filter')(activityModule.fields, { name: 'owner' }, true)[0];
                $scope.taskFields.subject = $filter('filter')(activityModule.fields, { name: 'subject' }, true)[0];
                $scope.taskFields.task_due_date = $filter('filter')(activityModule.fields, { name: 'task_due_date' }, true)[0];
                $scope.taskFields.task_status = $filter('filter')(activityModule.fields, { name: 'task_status' }, true)[0];
                $scope.taskFields.task_priority = $filter('filter')(activityModule.fields, { name: 'task_priority' }, true)[0];
                $scope.taskFields.task_notification = $filter('filter')(activityModule.fields, { name: 'task_notification' }, true)[0];
                $scope.taskFields.description = $filter('filter')(activityModule.fields, { name: 'description' }, true)[0];
            };

            setTaskFields();

            ModuleService.getPicklists(activityModule, false)
                .then(function (picklistsActivity) {
                    $scope.picklistsActivity = picklistsActivity;

                    if (!$scope.id) {
                        $scope.workflowModel = {};
                        $scope.workflowModel.active = true;
                        $scope.workflowModel.frequency = 'continuous';
                        $scope.workflowModel.trigger_time = 'instant';
                        $scope.loading = false;
                        $scope.filteredModules = $scope.modules;

                        ModuleService.getAllProcess()
                            .then(function (response) {
                                var processes = response.data;

                                for(var i=0; i<processes.length; i++){
                                    var process = processes[i];
                                    var processName = $filter('filter')($scope.modules, { id: process.module_id }, true)[0].name;
                                    if(processName && process.user_id === 0){
                                        $scope.filteredModules = $filter('filter')($scope.filteredModules, { name: '!'+processName }, true);
                                    }
                                }
                            });


                    }
                    else {
                        ApprovelProcessService.get($scope.id)
                            .then(function (workflow) {
                                workflow = workflow.data;
                                $scope.module = $filter('filter')($rootScope.modules, { id: workflow.module_id }, true)[0];

                                ModuleService.getPicklists($scope.module)
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
                                        $scope.filteredModules = $scope.modules;
                                        $scope.picklistsModule = picklists;
                                        $scope.getDynamicProcessModules($scope.module, workflow, true);
                                        $scope.workflowModel = ApprovelProcessService.processWorkflow(workflow, $scope.module, $scope.modulePicklists, $scope.filters, $scope.scheduleItems, $scope.dueDateItems, $scope.picklistsActivity, $scope.taskFields, picklists, $scope.dynamicprocessModules);
                                        $scope.getUpdatableModules();
                                        $scope.generateHookModules(workflow);
                                        $scope.firstApproverLookupChange(true, workflow);
                                        $scope.secondApproverLookupChange(true, workflow);
                                        $scope.prepareFilters();
                                        $scope.isEdit = true;
                                        $scope.lastStepClicked = true;
                                        $scope.loading = false;
                                    });
                            });
                    }
                });

            $scope.selectModule = function (module) {
                $scope.loadingFilter = true;
                $scope.isChosenModule = false;
                $scope.module = angular.copy($scope.workflowModel.module);
                $scope.users = angular.copy($rootScope.workgroup.users);


                if(!$filter('filter')($scope.users, { Id: 0 }, true)[0])
                    $scope.users.unshift({Id: 0, email: $filter('translate')('Setup.Workflow.ApprovelProcess.AllUsers')});

                $scope.fakeUsers=angular.copy($scope.users);

                for(var i=0; i<$rootScope.approvalProcesses.length; i++){
                    var process = $rootScope.approvalProcesses[i];
                    var user = $filter('filter')($scope.fakeUsers, { Id: process.user_id }, true)[0].email;
                    if(process && module.id === process.module_id){
                        $scope.users = $filter('filter')($scope.users, { email: '!'+user }, true);
                    }
                }

                if($scope.users.length !== $scope.fakeUsers.length)
                    $scope.users.shift();

                angular.forEach($scope.module.fields, function (field) {
                    if (field.data_type === 'lookup') {
                        field.operators = [];
                        field.operators.push(operators.equals);
                        field.operators.push(operators.not_equal);
                        field.operators.push(operators.empty);
                        field.operators.push(operators.not_empty);
                    }
                });

                ModuleService.getPicklists($scope.module)
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
                $scope.getDynamicProcessModules();
                setWebHookModules();

            };

            var getFilterValue = function (filter) {
                var filterValue = '';

                if (filter.field.data_type === 'lookup' && filter.field.lookup_type === 'users') {
                    filterValue = filter.value[0].full_name;
                }
                else if (filter.field.data_type === 'lookup' && filter.field.lookup_type != 'users') {
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
                else if (filter.field.data_type === 'checkbox') {
                    filterValue = filter.value.label[$rootScope.language];
                }
                else {
                    ModuleService.formatFieldValue(filter.field, filter.value, $scope.picklistsActivity);
                    filterValue = angular.copy(filter.field.valueFormatted);
                }

                return filterValue;
            };

            $scope.validate = function (tabClick) {
                $scope.workflowForm.$submitted = true;
                $scope.validateOperations();

                if (!$scope.workflowForm.workflowName.$valid || !$scope.workflowForm.module.$valid || !$scope.workflowForm.user.$valid || !$scope.workflowForm.operation.$valid)
                    return false;

                return $scope.validateActions(tabClick);
            };

            $scope.validateOperations = function () {
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

            $scope.getUpdatableModules = function () {
                $scope.updatableModules = [];
                $scope.updatableModules.push($scope.workflowModel.module);

                angular.forEach($scope.workflowModel.module.fields, function (field) {
                    if (field.lookup_type && field.lookup_type !== $scope.workflowModel.module.name && field.lookup_type !== 'users' && !field.deleted) {
                        var module = $filter('filter')($rootScope.modules, {name: field.lookup_type}, true)[0];
                        $scope.updatableModules.push(module);
                    }
                });
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

                        if (filter.field.lookup_type === 'users' && id == 0) {
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
                                    var lookupModule = $filter('filter')($rootScope.modules, { name: filter.field.lookup_type }, true)[0];
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

            $scope.multiselectProfiles = function () {
                return $filter('filter')($rootScope.profiles, { Deleted: false, HasAdminRights:false}, true);
            };

            $scope.setCurrentLookupField = function (field) {
                $scope.currentLookupField = field;
            };

            $scope.validateActions = function (tabClick) {
                if (!$scope.lastStepClicked && !tabClick) {
                    $scope.workflowForm.$submitted = false;
                    return true;
                }

                if($scope.workflowModel.approver_type === 'dynamicApprover'){
                    if(($scope.workflowModel.firstApproverModule && $scope.workflowModel.first_approver_lookup && $scope.workflowModel.first_approver_field)){
                        if($scope.workflowModel.secondApproverModule){
                            if($scope.workflowModel.secondApproverModule && $scope.workflowModel.second_approver_field && $scope.workflowModel.second_approver_lookup){
                                $scope.workflowModel.$submitted = false;
                                return true;
                            }
                        }else{
                            $scope.workflowModel.$submitted = false;
                            return true;
                        }
                    }
                }else if($scope.workflowModel.approver_type === 'staticApprover'){
                    if($scope.hookParameters[0].approver){
                        $scope.workflowModel.$submitted = false;
                        return true;
                    }
                }

                return false;
            };

            $scope.getDynamicProcessModules = function (module, process, isEdit) {
                if(isEdit && process.approver_type === 'staticApprover')
                    return;

                var dynamicprocessModules = [];
                // var processObj = {};

                var currentModule;
                if(module)
                    currentModule = module;
                else
                    currentModule = $scope.workflowModel.module;

                var id = 1;
                angular.forEach(currentModule.fields, function (field) {

                    if (field.lookup_type && field.lookup_type !== 'users' && !field.deleted) {
                        var processObj = {};
                        if(field.lookup_type === currentModule.name){
                            processObj.module = $filter('filter')($rootScope.modules, { name: field.lookup_type }, true)[0];
                            processObj.name = field['label_' + $rootScope.language]+ ' ' + '(' +processObj.module['label_' + $rootScope.language + '_singular']+ ')';
                            processObj.isSameModule = false;
                            processObj.systemName = field.name;
                            processObj.id = id;
                        }else{
                            processObj.module = $filter('filter')($rootScope.modules, { name: field.lookup_type }, true)[0];
                            processObj.name = field['label_' + $rootScope.language]+ ' ' + '(' +processObj.module['label_' + $rootScope.language + '_singular']+ ')';
                            processObj.isSameModule = false;
                            processObj.systemName = field.name;
                            processObj.id = id;
                        }
                        dynamicprocessModules.push(processObj);
                        id ++;
                    }
                });

                $scope.dynamicprocessModules = angular.copy(dynamicprocessModules);
            };

            $scope.firstApproverLookupChange = function (isEdit, process) {
                if(isEdit && process.approver_type === 'staticApprover')
                    return;

                $scope.firstDynamicApproverFields = $filter('filter')($rootScope.modules, { name: $scope.workflowModel.first_approver_lookup.lookup_type }, true)[0];

                if(!isEdit){
                    if($scope.workflowModel.first_approver_field)
                        $scope.workflowModel.first_approver_field = null;
                }

            };

            $scope.secondApproverLookupChange = function (isEdit, process) {
                if(isEdit && process.approver_type === 'staticApprover')
                    return;

                $scope.secondDynamicApproverFields = $filter('filter')($rootScope.modules, { name: $scope.workflowModel.first_approver_lookup.lookup_type }, true)[0];

                if(!isEdit){
                    if($scope.workflowModel.second_approver_field)
                        $scope.workflowModel.second_approver_field = null;
                }

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

                    // if ($scope.workflowModel.operation === "insert"){
                    //     $scope.workflowModel.frequency = "continuous";
                    //     $scope.ruleTriggerText += $filter('translate')('Setup.Workflow.RuleTriggers.insertLabel') + ' ' + orText + ' ';
                    // }
                    //
                    // if ($scope.workflowModel.operation === "update") {
                    //     var updateText = $filter('translate')('Setup.Workflow.RuleTriggers.updateLabel') + ' ' + orText + ' ';
                    //     $scope.ruleTriggerText += $scope.ruleTriggerText ? updateText.toLowerCase() : updateText;
                    // }

                    if ($scope.workflowModel.operation.insert)
                        $scope.ruleTriggerText += $filter('translate')('Setup.Workflow.RuleTriggers.insertLabel') + ' ' + orText + ' ';

                    if ($scope.workflowModel.operation.update) {
                        var updateText = $filter('translate')('Setup.Workflow.RuleTriggers.updateLabel') + ' ' + orText + ' ';
                        $scope.ruleTriggerText += $scope.ruleTriggerText ? updateText.toLowerCase() : updateText;
                    }

                    $scope.ruleTriggerText = $scope.ruleTriggerText.slice(0, -(orText.length + 2));

                    angular.forEach($scope.filters, function (filter) {
                        if (!filter.field || !filter.operator || !filter.value)
                            return;

                        $scope.ruleFilterText += filter.field['label_' + $rootScope.language] + ' <b class="operation-highlight">' + filter.operator.label[$rootScope.language] + '</b> ' +
                            getFilterValue(filter) + ' <b class="operation-highlight">' + andText + '</b><br> ';
                    });

                    $scope.ruleFilterText = $scope.ruleFilterText.slice(0, -(andText.length + 41));

                    if($scope.hookParameters){
                        var approvers = [];
                        for(var i=0; i<$scope.hookParameters.length; i++){
                            if($scope.hookParameters[i].approver){
                                var approverObj = {};
                                approverObj.user_id = $scope.hookParameters[i].approver.Id;
                                approverObj.order = i+1;
                                approvers.push(approverObj);
                            }
                        }

                        $scope.workflowModel.approvers = approvers;
                    }
                };

                getSummary();
            };

            $scope.save = function () {
                $scope.saving = true;

                var process = ApprovelProcessService.prepareWorkflow($scope.workflowModel, $scope.filters);

                var success = function () {
                        if(!$scope.id && process.approver_type === 'dynamicApprover' && $filter('filter')($scope.module.fields, { name: 'custom_approver' }, true).length < 1){
                            var processModule = angular.copy($scope.module);
                            for(var i = 1; i < 3 ; i++){
                                var approverField = {};
                                approverField.data_type = 'email';
                                approverField.deleted = false;
                                approverField.display_detail = false;
                                approverField.display_form = false;
                                approverField.display_list = false;
                                approverField.editable = false;
                                approverField.label_en = i+'. Approver';
                                approverField.label_tr = i+'. Onaylayıcı';
                                approverField.name = i === 1 ? 'custom_approver' : 'custom_approver_2';
                                approverField.order = $scope.module.fields.length + i;
                                approverField.primary = false;
                                approverField.section = $filter('filter')($scope.module.fields, { name: 'created_by' }, true)[0].section;
                                approverField.section_column = 2;
                                approverField.show_label = true;
                                approverField.system_type = 1;
                                approverField.validation = {
                                    readonly:false,
                                    required:false
                                };
                                processModule.fields.push(approverField);
                            }
                            ModuleService.update(processModule, processModule.id).then(function () {
                                $scope.saving = false;
                                $state.go('app.setup.approvel_process');
                                ngToast.create({ content: $filter('translate')('Setup.Workflow.ApprovelProcess.SubmitSuccess'), className: 'success' });
                            })
                        }else{
                            $scope.saving = false;
                            $state.go('app.setup.approvel_process');
                            ngToast.create({ content: $filter('translate')('Setup.Workflow.ApprovelProcess.SubmitSuccess'), className: 'success' });
                        }
                };

                if (!$scope.id) {
                    ApprovelProcessService.create(process)
                        .then(function () {
                            success();
                        })
                        .catch(function () {
                            $scope.saving = false;
                        });
                }
                else {
                    process.id = $scope.workflowModel.id;
                    process._rev = $scope.workflowModel._rev;
                    process.type = $scope.workflowModel.type;
                    process.created_by = $scope.workflowModel.created_by;
                    process.updated_by = $scope.workflowModel.updated_by;
                    process.created_at = $scope.workflowModel.created_at;
                    process.updated_at = $scope.workflowModel.updated_at;
                    process.deleted = $scope.workflowModel.deleted;

                    ApprovelProcessService.update(process)
                        .then(function () {
                            success();
                        })
                        .catch(function () {
                            $scope.saving = false;
                        });
                }
            };

            $scope.generateHookModules = function (workflow) {
                if ($scope.id) {
                    $scope.hookParameters = [];
                    var hookParameterArray = workflow.approvers;

                    angular.forEach(hookParameterArray, function (data) {

                        var editParameter = {};
                        editParameter.selectedUsers = $scope.approvers;

                        for(var i=0; i<editParameter.selectedUsers.length; i++){
                            var currentEl = editParameter.selectedUsers[i];
                            currentEl.isSelected = false;
                        }
                        editParameter.approver = $filter('filter')($rootScope.workgroup.users, { Id: data.id }, true)[0];
                        var order = data.order;

                        $scope.hookParameters[order-1]=editParameter;
                    })
                }
            };

            var setWebHookModules = function () {
                $scope.hookParameters = [];

                var parameter = {};
                parameter.selectedUsers = $scope.approvers;
                for(var i=0; i<parameter.selectedUsers.length; i++){
                    var currentEl = parameter.selectedUsers[i];
                    currentEl.isSelected = false;
                }

                $scope.hookParameters.push(parameter);

            };

            //when add a new user, drops it from the current user array
            $scope.addNewApprover = function (addItem, index) {

                var arr = angular.copy(addItem.selectedUsers);
                var parameter = {};

                var currentEl = $filter('filter')(arr, { email: addItem.approver.email }, true)[0];
                currentEl.isSelected = true;

                parameter.selectedUsers = arr;

                for(var i=0; i<$scope.hookParameters.length; i++){
                    if(i !== index){
                        var subEl = $filter('filter')($scope.hookParameters[i].selectedUsers, { email: addItem.approver.email }, true)[0];
                        subEl.isSelected = true;
                    }
                }

                if (arr.length) {
                    if ($scope.hookParameters.length <= 10) {
                        $scope.hookParameters.push(parameter);
                    }
                    else {
                        ngToast.create({ content: $filter('translate')('Setup.Workflow.MaximumHookWarning'), className: 'warning' });
                    }
                }
            };

            //when removes the element, appends the current element to users array
            $scope.removeApprover = function (itemname, ind) {

                var index = $scope.hookParameters.indexOf(itemname);

                if(ind+1 !== $scope.approversLength){
                    for(var i=0; i < $scope.hookParameters.length; i++){
                        if(itemname.approver){
                            var currentEl = $filter('filter')($scope.hookParameters[i].selectedUsers, { email: itemname.approver.email }, true)[0];
                            currentEl.isSelected = false;
                        }

                    }
                }

                $scope.hookParameters.splice(index, 1);
            };

            $scope.approverTypeChanged = function () {
                if ($scope.workflowModel.approver_type && $scope.workflowModel.approver_type === 'dynamicApprover'){
                    if($scope.hookParameters){
                        setWebHookModules();
                    }
                }else{
                    if($scope.workflowModel.firtsApproverLookup)
                        delete $scope.workflowModel.firtsApproverLookup;

                    if($scope.workflowModel.first_approver_field)
                        delete $scope.workflowModel.first_approver_field;
                }

            };

            $scope.firstApproverModuleChanged = function () {
                if($scope.workflowModel.first_approver_lookup)
                    $scope.workflowModel.first_approver_lookup = null;

                if($scope.workflowModel.first_approver_field)
                    $scope.workflowModel.first_approver_field = null;

                if($scope.workflowModel.secondApproverModule)
                    $scope.workflowModel.secondApproverModule = null;

                if($scope.workflowModel.second_approver_lookup)
                    $scope.workflowModel.second_approver_lookup = null;

                if($scope.workflowModel.second_approver_field)
                    $scope.workflowModel.second_approver_field = null;
            }

            $scope.secondApproverModuleChanged = function () {

                if($scope.workflowModel.second_approver_lookup)
                    $scope.workflowModel.second_approver_lookup = null;

                if($scope.workflowModel.second_approver_field)
                    $scope.workflowModel.second_approver_field = null;
            }
        }
    ]);