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

            };
        }]);

