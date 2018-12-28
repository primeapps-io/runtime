'use strict';

angular.module('primeapps')
    .factory('ImportService', ['$http', '$filter', 'config',
        function ($http, $filter, config) {
            return {
                getLookupIds: function (request) {
                    return $http.post(config.apiUrl + 'record/get_lookup_ids', request);
                },

                import: function (records, moduleName) {
                    return $http.post(config.apiUrl + 'data/import/' + moduleName, records);
                },

                getMapping: function (request) {
                    return $http.post(config.apiUrl + 'data/import_get_mapping', request)
                },

                getAllMapping: function (moduleId) {
                    return $http.get(config.apiUrl + 'data/import_get_all_mappings/' + moduleId)
                },

                saveMapping: function (records) {
                    return $http.post(config.apiUrl + 'data/import_mapping_save', records);
                },

                updateMapping: function (id, request) {
                    return $http.post(config.apiUrl + 'data/import_mapping_update/' + id, request);
                },

                deleteMapping: function (request) {
                    return $http.post(config.apiUrl + 'data/import_mapping_delete', request);
                }
            };
        }]);

