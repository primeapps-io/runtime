'use strict';

angular.module('primeapps')

    .factory('TeamsService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config) {
            return {
                get: function (id) {
                    return $http.get(config.apiUrl + 'team/get/' + id);
                },

                getAll: function () {
                    return $http.get(config.apiUrl + 'team/get_all');
                },

                create: function (team) {
                    return $http.post(config.apiUrl + 'team/create', team);
                },

                update: function (id, team) {
                    return $http.put(config.apiUrl + 'team/update/' + id, team);
                },

                delete: function (id) {
                    return $http.delete(config.apiUrl + 'team/delete/' + id);
                }
            };
        }]);