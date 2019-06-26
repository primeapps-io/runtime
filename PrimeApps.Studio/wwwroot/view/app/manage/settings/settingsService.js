'use strict';

angular.module('primeapps')

    .factory('SettingsService', ['$rootScope', '$http', 'config', '$filter', '$q', 'helper', '$cache', 'dataTypes', 'systemFields',
        function ($rootScope, $http, config, $filter, $q, helper, $cache, dataTypes, systemFields) {
            return {
                getLastDeployment: function () {
                    return $http.get(config.apiUrl + 'publish/get_last_deployment');
                }
            };
        }]);

