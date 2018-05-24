'use strict';

angular.module('ofisim')

    .factory('NotificationService', ['$http', 'config',
        function ($http, config) {
            return {

                toggleTaskReminder: function () {
                    return $http.post(config.apiUrl + 'User/ToggleTaskReminderService', {});
                },

                toggleTaskCompleted: function () {
                    return $http.post(config.apiUrl + 'User/ToggleTaskCompletedNotificationService', {});
                },

                toggleTaskAssigned: function () {
                    return $http.post(config.apiUrl + 'User/ToggleTaskAssignedNotificationService', {});
                },

                toggleActivity: function () {
                    return $http.post(config.apiUrl + 'User/ToggleActivityReminderService', {});
                },

                toggleNewNote: function () {
                    return $http.post(config.apiUrl + 'User/ToggleNewNoteNotificationService', {});
                }

            };
        }]);