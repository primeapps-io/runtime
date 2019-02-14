'use strict';

angular.module('primeapps')

    .factory('FunctionsDeploymentService', ['$rootScope', '$http', 'config', '$filter', '$q', 'helper',   '$cache', 'dataTypes', 'systemFields',
        function ($rootScope, $http, config, $filter, $q, helper,  $cache, dataTypes, systemFields) {
            return {
                get: function (id) {
                    return $http.get(config.apiUrl + 'deployment_function/get/' + id);
                },
                create: function (data) {
                    return $http.post(config.apiUrl + 'deployment_function/create', data);
                },
                update: function (data) {
                    return $http.put(config.apiUrl + 'deployment_function/update', data);
                },
                count: function () {
                    return $http.get(config.apiUrl + 'deployment_function/count');
                },
                find: function (data) {
                    return $http.post(config.apiUrl + 'deployment_function/find', data);
                },
                delete: function (id) {
                    return $http.delete(config.apiUrl + 'deployment_function/delete/' + id);
                }
            };
        }]);

