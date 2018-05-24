'use strict';

angular.module('ofisim')

    .directive('taskList', ['$filter', 'entityTypes', 'helper', 'operations', 'TaskService', 'ModuleService', 'activityTypes',
        function ($filter, entityTypes, helper, operations, TaskService, ModuleService, activityTypes) {
            return {
                restrict: 'EA',
                scope: {
                    hideUser: '@',
                    entityId: '=',
                    entityType: '=',
                    isAll: '=',
                    showAll: '=',
                    isLimitless: '='
                },
                templateUrl: cdnUrl + 'views/app/tasks/taskList.html',
                controller: ['$rootScope', '$scope', '$filter', '$location', '$state',
                    function ($rootScope, $scope, $filter, $location, $state) {
                        $scope.editedTask = null;
                        $scope.taskState = {};
                        $scope.taskUpdating = false;
                        $scope.hasPermission = helper.hasPermission;
                        $scope.entityTypes = entityTypes;
                        $scope.operations = operations;
                        $scope.module = $filter('filter')($scope.$root.modules, { name: 'activities' }, true)[0];
                        $scope.views = TaskService.getViews();
                        $scope.view = $scope.views[0];
                        $scope.filter = {};
                        $scope.loading = true;
                        $scope.pagingIcon = 'fa-chevron-right';
                        $scope.currentPage = 1;
                        var copyUsers = angular.copy($rootScope.users);
                        $scope.users = $filter('filter')(copyUsers, { IsActive: 'true' });
                        $scope.users = $filter('orderBy')($scope.users, 'FullName');
                        var userAll = { Id: 0, FullName: $filter('translate')('Tasks.AllUsers') };
                        $scope.users.unshift(userAll);
                        $scope.filter.assignedTo = userAll;

                        var findRequest = {};
                        findRequest.limit = 20;
                        findRequest.offset = 0;

                        $scope.getTasks = function () {
                            ModuleService.getPicklists($scope.module)
                                .then(function (picklists) {
                                    $scope.picklistsModule = picklists;
                                    $scope.taskSubjectField = $filter('filter')($scope.module.fields, { name: 'subject' }, true)[0];
                                    $scope.taskStatusField = $filter('filter')($scope.module.fields, { name: 'task_status' }, true)[0];
                                    $scope.taskStatusCompletedPicklistItem = $filter('filter')($scope.picklistsModule[$scope.taskStatusField.picklist_id], { system_code: 'completed' }, true)[0];

                                    var taskActivityType = $filter('filter')(activityTypes, { system_code: 'task' }, true)[0];
                                    findRequest.fields = ['subject', 'task_due_date', 'task_status', 'task_priority', 'updated_at', 'owner.users.full_name', 'created_by.users.full_name'];
                                    findRequest.filters = [{
                                        field: 'activity_type',
                                        operator: 'is',
                                        value: taskActivityType.label[$rootScope.user.tenantLanguage],
                                        no: 1
                                    }];
                                    findRequest.sort_field = 'task_due_date';
                                    findRequest.sort_direction = 'asc';

                                    findRequest.filter_logic = "((1 and 2) &)";

                                    var operator = 'is_not';

                                    if ($scope.view && $scope.view.type === 'completed')
                                        operator = 'is';

                                    findRequest.filters.push({
                                        field: 'task_status',
                                        operator: operator,
                                        value: $scope.taskStatusCompletedPicklistItem.label[$rootScope.user.tenantLanguage],
                                        no: 2
                                    });

                                    if ($scope.view && $scope.view.type != 'completed') {
                                        findRequest.filters.push({
                                            field: 'task_status',
                                            operator: 'empty',
                                            value: '-',
                                            no: 3
                                        });
                                        findRequest.filter_logic = findRequest.filter_logic.replace("2", "(2 or 3)");
                                    }

                                    if ($scope.filter && $scope.filter.assignedTo && $scope.filter.assignedTo.Id != 0) {
                                        findRequest.filters.push({
                                            field: 'owner',
                                            operator: 'equals',
                                            value: $scope.filter.assignedTo.Id,
                                            no: findRequest.filters.length + 1
                                        });
                                        findRequest.filter_logic = findRequest.filter_logic.replace("&", "and " + findRequest.filters.length + " &");
                                    }

                                    if ($scope.filter && $scope.filter.subject) {
                                        findRequest.filters.push({
                                            field: 'subject',
                                            operator: 'contains',
                                            value: $scope.filter.subject,
                                            no: findRequest.filters.length + 1
                                        });
                                        findRequest.filter_logic = findRequest.filter_logic.replace("&", "and " + findRequest.filters.length + " &");
                                        $scope.searching = true;
                                    }

                                    if ($scope.currentPage === 1)
                                        findRequest.offset = 0;

                                    findRequest.filter_logic = findRequest.filter_logic.replace(" &", "");

                                    ModuleService.findRecords('activities', findRequest)
                                        .then(function (tasks) {
                                            tasks = tasks.data;
                                            $scope.pagingIcon = 'fa-chevron-right';
                                            tasks = TaskService.processTasks(tasks, $scope.taskStatusCompletedPicklistItem);

                                            if ($scope.currentPage === 1)
                                                $scope.tasks = tasks;
                                            else
                                                $scope.tasks = $scope.tasks.concat(tasks);

                                            if (tasks.length < 1 || tasks.length < findRequest.limit)
                                                $scope.allTasksLoaded = true;
                                        })
                                        .finally(function () {
                                            $scope.loading = false;
                                            $scope.searching = false;
                                        });
                                });
                        };
                        $scope.getTasks();

                        $scope.edit = function (task) {
                            $scope.editedTask = task;
                            $scope.taskState = angular.copy(task);
                            task.editing = true;
                        };

                        $scope.cancelEdit = function () {
                            this.task = $scope.taskState;
                            $scope.editedTask = null;
                        };

                        $scope.update = function (task) {
                            if (!task || !task.subject || !task.subject.trim())
                                return;

                            $scope.taskUpdating = true;

                            var taskModel = TaskService.prepareTask(task);

                            ModuleService.updateRecord('activities', taskModel)
                                .then(function () {
                                    task['owner.users.id'] = task.assignedTo.Id;
                                    task['owner.users.full_name'] = task.assignedTo.FullName;

                                    TaskService.processTask(task, $scope.taskStatusCompletedPicklistItem);

                                    task.editing = false;
                                })
                                .finally(function () {
                                    $scope.taskUpdating = false;
                                });
                        };

                        $scope.mark = function (task) {
                            task.marking = true;
                            var taskModel = {};
                            taskModel.id = task.id;
                            taskModel.task_status = $scope.taskStatusCompletedPicklistItem.id;

                            ModuleService.updateRecord('activities', taskModel)
                                .then(function () {
                                    $scope.tasks.splice($scope.tasks.indexOf(task), 1)
                                })
                                .catch(function (data) {
                                    if (data.status === 409) {
                                        ngToast.create({ content: $filter('translate')('Module.UniqueError'), className: 'danger' });
                                    }
                                })
                                .finally(function () {
                                    $scope.marking = false;
                                });
                        };

                        $scope.remove = function (taskId, index) {
                            ModuleService.deleteRecord('activities', taskId)
                                .then(function () {
                                    $scope.tasks.splice(index, 1);
                                });
                        };

                        $scope.loadMore = function () {
                            if ($scope.allTasksLoaded)
                                return;

                            $scope.pagingIcon = 'fa-spinner fa-spin';
                            findRequest.offset = $scope.currentPage * findRequest.limit;

                            ModuleService.findRecords('activities', findRequest)
                                .then(function (tasks) {
                                    tasks = tasks.data;
                                    $scope.pagingIcon = 'fa-chevron-right';
                                    $scope.currentPage = $scope.currentPage + 1;
                                    tasks = TaskService.processTasks(tasks, $scope.taskStatusCompletedPicklistItem);

                                    if ($scope.currentPage === 1)
                                        $scope.tasks = tasks;
                                    else
                                        $scope.tasks = $scope.tasks.concat(tasks);

                                    if (tasks.length < 1 || tasks.length < findRequest.limit)
                                        $scope.allTasksLoaded = true;
                                });
                        };

                        $scope.filterChanged = function () {
                            $scope.currentPage = 1;
                            findRequest.offset = 0;
                            if ($scope.filter && $scope.filter.subject)
                                $scope.filter.subject = null;

                            $scope.allTasksLoaded = false;

                            $scope.getTasks();
                        };


                    }]
            };
        }])

    .directive('taskForm', ['$filter', 'ngToast', 'TaskService', 'ModuleService',
        function ($filter, ngToast, TaskService, ModuleService) {
            return {
                restrict: 'EA',
                scope: {
                    isAll: '=',
                    show: '=',
                    taskDate: '=',
                    taskCreated: '@'
                },
                templateUrl: cdnUrl + 'views/app/tasks/taskForm.html',
                controller: ['$scope',
                    function ($scope) {
                        $scope.now = new Date().getTime();
                        $scope.taskCreating = false;
                        $scope.module = $filter('filter')($scope.$root.modules, { name: 'activities' }, true)[0];

                        var newTask = function () {
                            $scope.task = {};
                            $scope.task.task_due_date = (new Date()).setHours(0, 0, 0, 0);
                            $scope.task.assignedTo = $filter('filter')($scope.$root.users, { Id: $scope.$root.user.ID }, true)[0];
                        };

                        ModuleService.getPicklists($scope.module)
                            .then(function (picklists) {
                                $scope.picklistsModule = picklists;
                                $scope.taskSubjectField = $filter('filter')($scope.module.fields, { name: 'subject' }, true)[0];
                                $scope.taskStatusField = $filter('filter')($scope.module.fields, { name: 'task_status' }, true)[0];
                                $scope.taskStatusNotStartedPicklistItem = $filter('filter')($scope.picklistsModule[$scope.taskStatusField.picklist_id], { system_code: 'not_started' }, true)[0];

                                newTask();
                            });

                        $scope.create = function (task) {
                            if (!task || !task.subject || !task.subject.trim())
                                return;

                            $scope.taskCreating = true;

                            var taskModel = TaskService.prepareTask(task, $scope.taskStatusNotStartedPicklistItem);

                            ModuleService.insertRecord('activities', taskModel)
                                .then(function () {
                                    if ($scope.$parent.views)
                                        $scope.$parent.view = $scope.$parent.views[0];

                                    $scope.$parent.currentPage = 1;
                                    $scope.$parent.getTasks();
                                    $scope.$parent.allTasksLoaded = false;
                                    $scope.$parent.showTaskForm = false;

                                    newTask();
                                })
                                .catch(function (data) {
                                    if (data.status === 409) {
                                        ngToast.create({ content: $filter('translate')('Module.UniqueError'), className: 'danger' });
                                    }
                                })
                                .finally(function () {
                                    $scope.taskCreating = false;
                                });
                        };

                        $scope.cancelCreate = function (task) {
                            task.dueDate = 4132252800000;
                            task.taskText = null;
                            $scope.selectedUser = $filter('filter')($scope.$root.users, { EntityID: $scope.$root.user.ID }, true)[0];
                            $scope.show = false;
                            if ($scope.taskCreated) {
                                $scope.taskCreated();
                            }
                        }
                    }]
            };
        }]);