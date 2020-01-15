'use strict';

angular.module('primeapps')

    .factory('PackageService', ['$rootScope', '$http', 'config', '$filter', '$q', 'helper', '$cache', 'dataTypes', 'systemFields',
        function ($rootScope, $http, config, $filter, $q, helper, $cache, dataTypes, systemFields) {
            return {
                create: function (data) {
                    return $http.post(config.apiUrl + 'package/create', data);
                },
                count: function () {
                    return $http.get(config.apiUrl + 'package/count');
                },
                find: function (request) {
                    return $http.post(config.apiUrl + 'package/find', request);
                },
                get: function (id) {
                    return $http.get(config.apiUrl + 'package/get/' + id);
                },
                log: function (id) {
                    return $http.get(config.apiUrl + 'package/log/' + id);
                },
                getActiveProcess: function () {
                    return $http.get(config.apiUrl + 'package/get_active_process');
                }
            };
        }]);

