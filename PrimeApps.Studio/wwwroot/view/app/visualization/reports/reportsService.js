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
                },
                createCategory: function (data) {
                    return $http.post(config.apiUrl + 'report/create_category', data);
                },
                updateCategory: function (data) {
                    return $http.put(config.apiUrl + 'report/update_category/' + data.id, data);
                },
                deleteCategory: function (id) {
                    return $http.delete(config.apiUrl + 'report/delete_category/' + id);
                },
                getReport: function (id) {
                    return $http.get(config.apiUrl + 'report/get_report/' + id);
                },
                createReport: function (data) {
                    return $http.post(config.apiUrl + "report/create", data);
                },
                updateReport: function (data) {
                    return $http.put(config.apiUrl + 'report/update/' + data.id, data);
                },
                deleteReport: function (id) {
                    return $http.delete(config.apiUrl + 'report/delete/' + id);
                },
                getChart: function (reportId) {
                    return $http.get(config.apiUrl + 'report/get_chart/' + reportId);
                },
                getWidget: function (reportId) {
                    return $http.get(config.apiUrl + 'report/get_widget/' + reportId);
                },

            };
        }]);
