'use strict';

angular.module('primeapps')

    .factory('ComponentsService', ['$rootScope', '$http', 'config', '$filter', '$q', 'helper', '$cache', 'dataTypes', 'systemFields',
        function ($rootScope, $http, config, $filter, $q, helper, $cache, dataTypes, systemFields) {
            return {
                get: function (id) {
                    return $http.get(config.apiUrl + 'component/get/' + id);
                },
                count: function () {
                    return $http.get(config.apiUrl + 'component/count');
                },
                find: function (model) {
                    return $http.post(config.apiUrl + 'component/find', model);
                },
                create: function (model) {
                    return $http.post(config.apiUrl + 'component/create', model);
                },
                update: function (model) {
                    return $http.put(config.apiUrl + 'component/update', model);
                },
                getAllModulesBasic: function () {
                    return $http.get(config.apiUrl + 'module/get_all_basic');
                },
                getApp: function (id) {
                    return $http.get(config.apiUrl + 'app/get/' + id);
                },
                delete: function (id) {
                    return $http.delete(config.apiUrl + 'component/delete/' + id);
                }
            };
        }]);

