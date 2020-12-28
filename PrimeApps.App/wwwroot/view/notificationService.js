'use strict';

angular.module('primeapps')
    // Provides Signalr integration for AngularJS.
    .factory('NotificationService', ['$http', 'config', '$localStorage', '$window', '$location', '$timeout', '$rootScope', '$q', '$sessionStorage', '$cache',
        function ($http, config, $localStorage, $window, $location, $timeout, $rootScope, $q, $sessionStorage, $cache) {

            var notificationHub = new signalR.HubConnectionBuilder()
                .withUrl("/notification?X-App-Id=" + $rootScope.user.app_id + "&X-Tenant-Id=" + $rootScope.user.tenant_id, {
                    accessTokenFactory: function () {
                      return  $localStorage.read('access_token');
                    }
                })
                .configureLogging(function (logging) {
                    logging.SetMinimumLevel(LogLevel.Error);
                })
                .withAutomaticReconnect()
                .build();

            function start() {
                try {
                    notificationHub.start();
                    console.log("connected");
                } catch (err) {
                    console.log(err);
                    setTimeout(function () { return start(); }, 5000);
                }
            };

            notificationHub.onclose(function () {
                return start(); 
            });

            // Start the connection.
            start();

            return {
                Event: function (functionName, callback) {
                    notificationHub.on(functionName, function (data) { return callback(data); });
                }
            };
        }]);