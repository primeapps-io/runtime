'use strict';

angular.module('primeapps')

    .factory('AppFormService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config) {
            return {
                create: function(app){
                    return $http.post(config.apiUrl + 'app/create', app);
                },
                getApps: function () {
                    return $http.get(config.apiUrl + 'dashboard/get_dashlets?dashboard=' + dashboardId);
                }
            };
        }]);