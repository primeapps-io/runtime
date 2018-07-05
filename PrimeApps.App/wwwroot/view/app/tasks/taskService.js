'use strict';

angular.module('primeapps')

    .factory('TaskService', ['$rootScope', '$filter', 'taskDate',
        function ($rootScope, $filter, taskDate) {
            return {
                getViews: function () {
                    var view1 = {};
                    view1.id = 1;
                    view1.label = $rootScope.language === 'tr' ? 'Açık Görevler' : 'Open Tasks';
                    view1.type = 'open';

                    var view2 = {};
                    view2.id = 1;
                    view2.label = $rootScope.language === 'tr' ? 'Tamamlanan Görevler' : 'Completed Tasks';
                    view2.type = 'completed';

                    var views = [];
                    views.push(view1);
                    views.push(view2);

                    return views;
                },

                processTask: function (task, taskStatusCompletedPicklistItem) {
                    var assignedTo = $filter('filter')($rootScope.users, { id: task['owner.users.id'] }, true)[0];

                    if (!assignedTo)
                        return;

                    task.assignedTo = assignedTo;
                    task.isCompleted = task.task_status === taskStatusCompletedPicklistItem.label[$rootScope.user.tenant_language];

                    if (task.task_due_date)
                        task.dueDate = moment.utc(task.task_due_date).toDate();
                    else
                        task.dueDate = taskDate.future;

                    if (task.isCompleted) {
                        task.completeDate = moment.utc(task.updated_at).toDate();
                    }
                },

                processTasks: function (tasks, taskStatusCompletedPicklistItem) {
                    var that = this;

                    angular.forEach(tasks, function (task) {
                        that.processTask(task, taskStatusCompletedPicklistItem);
                    });

                    return tasks;
                },

                prepareTask: function (task, taskStatusNotStartedPicklistItem) {
                    var taskModel = {};
                    taskModel.subject = task.subject;
                    taskModel.owner = task.assignedTo.id;

                    var dateParts = moment(task.task_due_date).format().split('+');
                    taskModel.task_due_date = dateParts[0];

                    if (task.id) {
                        taskModel.id = task.id;
                    }
                    else {
                        taskModel.activity_type = 1;
                        taskModel.task_status = taskStatusNotStartedPicklistItem.id;
                    }

                    return taskModel;
                }
            };
        }]);