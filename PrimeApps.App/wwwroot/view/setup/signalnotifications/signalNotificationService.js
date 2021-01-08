'use strict';

angular.module('primeapps')

    .factory('SignalNotificationService', ['$http', 'config',
        function ($http, config) {
            return {

                getAll: function (limit) {
                    if (limit)
                        return $http.get(config.apiUrl + 'signal_notification/get_unreads/' + limit);
                    else
                        return $http.get(config.apiUrl + 'signal_notification/get_unreads');
                },

                read: function (id) {
                    return $http.put(config.apiUrl + 'signal_notification/read/' + id);
                },

                hide: function (id) {
                    return $http.put(config.apiUrl + 'signal_notification/hide/' + id);
                }
            };
        }]);