'use strict';

angular.module('primeapps')

    .factory('AppFormService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config) {
            return {
                create: function (app) {
                    return $http.post(config.apiUrl + 'app/create', app);
                },
                getApps: function () {
                    return $http.get(config.apiUrl + 'dashboard/get_dashlets?dashboard=');
                },
                isUniqueName: function (name) {
                    return $http.get(config.apiUrl + 'app/is_unique_name?name=' + name);
                },
                update: function (id, model) {
                    return $http.put(config.apiUrl + 'app/update/' + id, model);
                }
            };
        }]);