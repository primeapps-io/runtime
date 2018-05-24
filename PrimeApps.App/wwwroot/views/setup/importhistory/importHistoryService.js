'use strict';

angular.module('ofisim')
    .factory('ImportHistoryService', ['$http', '$filter', 'config',
        function ($http, $filter, config) {
            return {
                find: function (request) {
                    return $http.post(config.apiUrl + 'data/import_find', request);
                },

                revert: function (id) {
                    return $http.delete(config.apiUrl + 'data/import_revert/' + id);
                }
            };
        }]);

