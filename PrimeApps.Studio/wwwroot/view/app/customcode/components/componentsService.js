'use strict';

angular.module('primeapps')

    .factory('ComponentsService', ['$rootScope', '$http', 'config', '$filter', '$q', 'helper', '$cache', 'dataTypes', 'systemFields', 'environments',
        function ($rootScope, $http, config, $filter, $q, helper, $cache, dataTypes, systemFields, environments) {
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
                update: function (id, model) {
                    return $http.put(config.apiUrl + 'component/update/' + id, model);
                },
                getAllModulesBasic: function () {
                    return $http.get(config.apiUrl + 'module/get_all_basic');
                },
                getApp: function (id) {
                    return $http.get(config.apiUrl + 'app/get/' + id);
                },
                delete: function (id) {
                    return $http.delete(config.apiUrl + 'component/delete/' + id);
                },
                getFileList: function (id) {
                    return $http.get(config.apiUrl + 'component/all_files_names/' + id);
                },
                deploy: function (id) {
                    return $http.get(config.apiUrl + 'component/deploy/' + id);
                },
                getEnvironments: function () {
                    return environments.data;
                },
            };
        }]);

