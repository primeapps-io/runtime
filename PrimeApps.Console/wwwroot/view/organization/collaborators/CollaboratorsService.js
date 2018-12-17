'use strict';

angular.module('primeapps')

    .factory('CollaboratorsService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config) {
            return {
                getApps: function () {
                    return $http.get(config.apiUrl + 'dashboard/get_dashlets?dashboard=' + dashboardId);
                }
            };
        }]);