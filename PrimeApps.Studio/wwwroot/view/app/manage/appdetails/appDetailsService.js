'use strict';

angular.module('primeapps')

    .factory('AppDetailsService', ['$rootScope', '$http', 'config', '$filter', '$q', 'helper', '$cache', 'dataTypes', 'systemFields',
        function ($rootScope, $http, config, $filter, $q, helper, $cache, dataTypes, systemFields) {
            return {
                get: function (id) {
                    return $http.get(config.apiUrl + 'app/get/' + id);
                },
                update: function (id, model) {
                    return $http.put(config.apiUrl + 'app/update/' + id, model);
                },
                isUniqueName: function (name) {
                    return $http.get(config.apiUrl + 'app/is_unique_name?name=' + name);
                },
                updateAuthTheme: function (id, model) {
                    return $http.put(config.apiUrl + 'app/update_auth_theme/' + id, model);
                },
                getAuthTheme: function (id) {
                    return $http.get(config.apiUrl + 'app/get_auth_theme/' + id);
                },
                updateAppTheme: function (id, model) {
                    return $http.put(config.apiUrl + 'app/update_app_theme/' + id, model);
                },
                getAppTheme: function (id) {
                    return $http.get(config.apiUrl + 'app/get_app_theme/' + id);
                },
                addAppUser: function (appUser) {
                    return $http.post(config.apiUrl + 'app_draft_user/create', appUser);
                }

            };
        }]);

