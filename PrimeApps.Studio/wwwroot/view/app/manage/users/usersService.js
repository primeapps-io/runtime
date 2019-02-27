'use strict';

angular.module('primeapps')

    .factory('UsersService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config) {

            return {

                count: function () {
                    return $http.get(config.apiUrl + 'user/count/');
                },
                find: function (model) {
                    return $http.post(config.apiUrl + 'user/find', model);
                },
                get: function (id) {
                    return $http.get(config.apiUrl + 'user/get/' + id);
                },
                getAll: function () {
                    return $http.get(config.apiUrl + 'user/get_all');
                },
                create: function (model) {
                    return $http.post(config.apiUrl + 'user/create', model);
                },
                update: function (id, model) {
                    return $http.put(config.apiUrl + 'user/update/' + id, model);
                },
                delete: function (id) {
                    return $http.delete(config.apiUrl + 'user/delete/' + id);
                },
            };
        }
    ]);