'use strict';

angular.module('ofisim')
    .factory('AnalyticsService', ['$rootScope', '$http', '$filter', 'config',
        function ($rootScope, $http, $filter, config) {
            return {

                get: function (id) {
                    return $http.get(config.apiUrl + 'analytics/get/' + id);
                },

                getAll: function () {
                    return $http.get(config.apiUrl + 'analytics/get_all');
                },

                getReports: function () {
                    return $http.get(config.apiUrl + 'analytics/get_reports');
                },

                create: function (report) {
                    return $http.post(config.apiUrl + 'analytics/create', report);
                },

                update: function (report) {
                    return $http.put(config.apiUrl + 'analytics/update/' + report.id, report);
                },

                delete: function (id) {
                    return $http.delete(config.apiUrl + 'analytics/delete/' + id);
                },

                getWarehouseInfo: function () {
                    return $http.get(config.apiUrl + 'analytics/get_warehouse_info');
                },

                changeWarehousePassword: function (password) {
                    return $http.put(config.apiUrl + 'analytics/change_warehouse_password', { database_password: password });
                }

            };
        }]);

