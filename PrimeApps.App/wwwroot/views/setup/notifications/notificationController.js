'use strict';

angular.module('ofisim')
    .controller('NotificationController', ['$rootScope', '$scope', 'NotificationService',
        function ($rootScope, $scope, NotificationService) {
            $scope.toggleTaskReminder = function () {
                $scope.isChanging = true;

                NotificationService.toggleTaskReminder()
                    .then(function () {
                        $rootScope.user.isTaskNotificationsEnabled != $rootScope.user.isTaskNotificationsEnabled;
                        $scope.isChanging = false;
                    });
            };

            $scope.toggleTaskCompleted = function () {
                $scope.isChanging = true;

                NotificationService.toggleTaskCompleted()
                    .then(function () {
                        $rootScope.user.isTaskCompletedNotificationsEnabled != $rootScope.user.isTaskCompletedNotificationsEnabled;
                        $scope.isChanging = false;
                    });
            };

            $scope.toggleTaskAssigned = function () {
                $scope.isChanging = true;

                NotificationService.toggleTaskAssigned()
                    .then(function () {
                        $rootScope.user.isTaskAssignedNotificationsEnabled != $rootScope.user.isTaskAssignedNotificationsEnabled;
                        $scope.isChanging = false;
                    });
            };

            $scope.toggleActivity = function () {
                $scope.isChanging = true;

                NotificationService.toggleActivity()
                    .then(function () {
                        $rootScope.user.isActivityNotificationsEnabled != $rootScope.user.isActivityNotificationsEnabled;
                        $scope.isChanging = false;
                    });
            };

            $scope.toggleNewNote = function () {
                $scope.isChanging = true;

                NotificationService.toggleNewNote()
                    .then(function () {
                        $rootScope.user.isNewNoteNotificationsEnabled != $rootScope.user.isNewNoteNotificationsEnabled;
                        $scope.isChanging = false;
                    });
            };
        }
    ]);