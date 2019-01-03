'use strict';

angular.module('primeapps')

    .factory('AppsService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config) {
            return {
                getOrganizationApps: function (request) {
                    return $http.post(config.apiUrl + 'organization/apps', request);
                }
            };
        }]);