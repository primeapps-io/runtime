'use strict';

angular.module('primeapps')

    .factory('ComponentsService', ['$rootScope', '$http', 'config', '$filter', '$q', 'helper', '$cache', 'dataTypes', 'systemFields',
        function ($rootScope, $http, config, $filter, $q, helper, $cache, dataTypes, systemFields) {
            return {
                get: function (id) {
                    return $http.get(config.apiUrl + 'component/get/' + id);
                },
                create: function (data) {
                    return $http.post(config.apiUrl + 'component/create', data);
                },
                count: function () {
                    return $http.get(config.apiUrl + 'component/count');
                },
                find: function (data) {
                    return $http.post(config.apiUrl + 'component/find', data);
                },
                getAllModulesBasic: function () {
                    return $http.get(config.apiUrl + 'module/get_all_basic');
                },
                getApp: function (id) {
                    return $http.get(config.apiUrl + 'app/get/' + id);
                }
            };
        }]);

