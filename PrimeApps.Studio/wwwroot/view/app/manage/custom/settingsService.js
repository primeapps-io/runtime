'use strict';

angular.module('primeapps')

    .factory('SettingsService', ['$rootScope', '$http', 'config', 'helper',
        function ($rootScope, $http, config, helper) {
            return {
                count: function () {
                    return $http.get(config.apiUrl + 'setting/count');
                },
                find: function (model) {
                    return $http.post(config.apiUrl + 'setting/find', model);
                },
                getById: function (id) {
                    return $http.get(config.apiUrl + 'setting/get/' + id);
                },
                getAll: function () {
                    return $http.get(config.apiUrl + 'setting/get_all');
                },
                create: function (model) {
                    return $http.post(config.apiUrl + 'setting/create', model);
                },
                update: function (id, model) {
                    return $http.put(config.apiUrl + 'setting/update/' + id, model);
                },
                delete: function (id) {
                    return $http.delete(config.apiUrl + 'setting/delete/' + id);
                }
            }
        }
    ]);