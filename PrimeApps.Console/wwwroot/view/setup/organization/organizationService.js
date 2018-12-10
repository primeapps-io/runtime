'use strict';

angular.module('primeapps')
    .factory('OrganizationService', ['$http', 'config',
        function ($http, config) {
            return {

                editCompany: function (company) {
                    return $http.post(config.apiUrl + 'Instance/Edit', company);
                }

            };
        }
    ]);