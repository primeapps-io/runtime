'use strict';

angular.module('primeapps')

    .factory('AllAppService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config) {
            return {
                myApps: function (request) {
                    return $http.post(config.apiUrl + 'user/apps', request);
                }
            };
        }]);