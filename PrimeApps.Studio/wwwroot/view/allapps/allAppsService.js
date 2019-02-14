'use strict';

angular.module('primeapps')

    .factory('AllAppsService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config) {
            return {
                myApps: function (request) {
                    return $http.post(config.apiUrl + 'user/apps', request);
                }
            };
        }]);