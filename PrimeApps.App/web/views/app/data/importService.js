'use strict';

angular.module('ofisim')
    .factory('ImportService', ['$http', '$filter', 'config',
        function ($http, $filter, config) {
            return {
                getLookupIds: function (request) {
                    return $http.post(config.apiUrl + 'record/get_lookup_ids', request);
                },

                import: function (records, moduleName) {
                    return $http.post(config.apiUrl + 'data/import/' + moduleName, records);
                }
            };
        }]);

