'use strict';

angular.module('primeapps')

    .factory('TeamUserService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config) {
            return {
                get: function (id) {
                    return $http.get(config.apiUrl + 'user/get/' + id);
                },

                getAll: function () {
                    return $http.get(config.apiUrl + 'user/get_all');
                },

                create: function (user) {
                    return $http.post(config.apiUrl + 'user/create', user);
                },

                update: function (id, user) {
                    return $http.put(config.apiUrl + 'user/update/' + id, user);
                },

                delete: function (id) {
                    return $http.delete(config.apiUrl + 'user/delete/' + id);
                }
            };
        }]);