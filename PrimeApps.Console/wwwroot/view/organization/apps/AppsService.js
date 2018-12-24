'use strict';

angular.module('primeapps')

    .factory('AppsService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config) {
            return {
                getOrganizationApps: function (organizationId) {
                    return $http.post(config.apiUrl + 'organization/apps',{organization_id : organizationId});
                }
            };
        }]);