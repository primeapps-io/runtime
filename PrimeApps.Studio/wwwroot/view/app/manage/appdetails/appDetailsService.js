'use strict';

angular.module('primeapps')

    .factory('AppDetailsService', ['$rootScope', '$http', 'config', '$filter', '$q', 'helper', '$cache', 'dataTypes', 'systemFields',
        function ($rootScope, $http, config, $filter, $q, helper, $cache, dataTypes, systemFields) {
            return {
                get: function (id) {
                    return $http.get(config.apiUrl + 'app/get/' + id);
                },
                update: function (id, model) {
                    return $http.put(config.apiUrl + 'app/update/' + id, model);
                },
                isUniqueName: function (name) {
                    return $http.get(config.apiUrl + 'app/is_unique_name?name=' + name);
                }
            };
        }]);

