'use strict';

angular.module('primeapps')

    .factory('UsersService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config) {

            return {

                count: function () {
                    return $http.get(config.apiUrl + 'users/count/');
                },
                find: function (model) {
                    return $http.post(config.apiUrl + 'users/find', model);
                },
                get: function (id) {
                    return $http.get(config.apiUrl + 'users/get/' + id);
                },
                getAll: function () {
                    return $http.get(config.apiUrl + 'users/get_all');
                },
                create: function (model) {
                    return $http.post(config.apiUrl + 'users/create', model);
                },
                update: function (id, model) {
                    return $http.put(config.apiUrl + 'users/update/' + id, model);
                },
                delete: function (id) {
                    return $http.delete(config.apiUrl + 'users/delete/' + id);
                },
            };
        }
    ]);