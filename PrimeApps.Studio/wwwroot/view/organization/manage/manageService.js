'use strict';

angular.module('primeapps')

    .factory('ManageService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config) {
            return {
                get: function (id) {
                    return $http.get(config.apiUrl + 'organization/get/' + id);
                },
                update: function (id, model) {
                    return $http.put(config.apiUrl + 'organization/update/' + id, model);
                },
                delete: function (id) {
                    return $http.delete(config.apiUrl + 'organization/delete/' + id);
                },
                myOrganizations: function () {
                    return $http.get(config.apiUrl + 'user/organizations');
                }
            };
        }]);