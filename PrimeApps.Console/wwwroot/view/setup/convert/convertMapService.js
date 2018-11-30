'use strict';

angular.module('primeapps')
    .factory('ConvertMapService', ['$http', '$filter', 'config',
        function ($http, $filter, config) {
            return {
                createMapping: function (request) {
                    return $http.post(config.apiUrl + 'convert/create_mapping', request);
                },

                deleteMapping: function (request) {
                    return $http.post(config.apiUrl + 'convert/delete_mapping', request);
                },

                getMappings: function (moduleId) {
                    return $http.get(config.apiUrl + 'convert/get_mappings/' + moduleId);

                }
            };
        }]);