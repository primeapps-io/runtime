'use strict';

angular.module('primeapps')

    .factory('DashboardService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config) {
            return {
                getDashlets: function (dashboardId) {
                    return $http.get(config.apiUrl + 'dashboard/get_dashlets?dashboard=' + dashboardId);
                },
                getDashboards: function () {
                    return $http.get(config.apiUrl + 'dashboard/get_all');
                },
                createDashbord: function (data) {
                    return $http.post(config.apiUrl + 'dashboard/create', data);
                },
                createDashlet: function (data) {
                    return $http.post(config.apiUrl + 'dashboard/create_dashlet', data);
                },
                getCharts: function () {
                    return $http.get(config.apiUrl + 'dashboard/get_charts');
                },
                getWidgets: function () {
                    return $http.get(config.apiUrl + 'dashboard/get_widgets');
                },
                dashletUpdate: function (id, data) {
                    return $http.put(config.apiUrl + 'dashboard/update_dashlet/' + id, data);
                },
                dashletDelete: function (id, data) {
                    return $http.delete(config.apiUrl + 'dashboard/delete_dashlet/' + id);
                },
                dashletOrderChange: function (data) {
                    return $http.put(config.apiUrl + 'dashboard/change_order_dashlet/', data);
                },
                getViews: function (moduleId) {
                    return $http.get(config.apiUrl + 'view/get_all/' + moduleId);
                },
                getView: function (viewId) {
                    return $http.get(config.apiUrl + 'view/get/' + viewId);
                },
                updateDashboard: function (dashboardModel) {
                    return $http.put(config.apiUrl + 'dashboard/update?id='+dashboardModel.id, dashboardModel);
                }

            };
        }]);