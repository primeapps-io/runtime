'use strict';

angular.module('primeapps')

    .factory('ReportsService', ['$http', 'config',
        function ($http, config) {
            return {
                count: function () {
                    return $http.get(config.apiUrl + 'report/count');
                },
                find: function (data) {
                    return $http.post(config.apiUrl + 'report/find', data);
                },
                getAllCategory: function () {
                    return $http.get(config.apiUrl + 'report/get_categories');
                }

            };
        }]);
