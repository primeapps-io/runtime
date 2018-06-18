'use strict';

angular.module('primeapps')

    .factory('NoteService', ['$rootScope', '$http', 'config', '$filter',
        function ($rootScope, $http, config, $filter) {
            return {
                get: function (id) {
                    return $http.get(config.apiUrl + 'note/get/' + id);
                },
                like: function (request) {
                    return $http.post(config.apiUrl + 'note/like', request);
                },
                find: function (request) {
                    return $http.post(config.apiUrl + 'note/find', request);
                },
                count: function (request) {
                    return $http.post(config.apiUrl + 'note/count', request);
                },

                create: function (note) {
                    return $http.post(config.apiUrl + 'note/create', note);
                },

                update: function (note) {
                    return $http.put(config.apiUrl + 'note/update/' + note.id, note);
                },

                delete: function (id) {
                    return $http.delete(config.apiUrl + 'note/delete/' + id);
                }
            };
        }]);