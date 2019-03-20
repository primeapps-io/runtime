'use strict';

angular.module('primeapps')

    .factory('OrganizationService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config) {
            return {
                teamCount: function () {
                    return $http.get(config.apiUrl + 'team/count');
                },

                collaboratorCount: function (id) {
                    return $http.get(config.apiUrl + 'organization/count/' + id);
                },

            };
        }]);