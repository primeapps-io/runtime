'use strict';

angular.module('primeapps')

    .factory('ManageService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config) {
            return {
                teamCount: function (id) {
                    return $http.get(config.apiUrl + 'team/count/' + id);
                },

                collaboratorCount: function (id) {
                    return $http.get(config.apiUrl + 'organization/count/' + id);
                },

            };
        }]);