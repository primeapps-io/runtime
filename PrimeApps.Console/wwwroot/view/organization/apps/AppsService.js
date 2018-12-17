'use strict';

angular.module('primeapps')

    .factory('AppsService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config) {
            return {
                getApps: function () {
                    return $http.get(config.apiUrl + 'dashboard/get_dashlets?dashboard=' + dashboardId);
                }
            };
        }]);