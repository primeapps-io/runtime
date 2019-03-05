'use strict';

angular.module('primeapps')

    .factory('UsersService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config) {

            return {

                count: function () {
                    return $http.get(config.apiUrl + 'app_draft_user/count');
                },
                find: function (model) {
                    return $http.post(config.apiUrl + 'app_draft_user/find', model);
                },
                get: function (id) {
                    return $http.get(config.apiUrl + 'app_draft_user/get/' + id);
                },
                getAll: function () {
                    return $http.get(config.apiUrl + 'app_draft_user/get_all');
                },
                send: function (model) {
                    return $http.post(config.apiUrl + 'app_draf_user/send_email', model);
                },
                create: function (model) {
                    return $http.post(config.apiUrl + 'app_draft_user/create', model);
                },
                update: function (id, model) {
                    return $http.put(config.apiUrl + 'app_draft_user/update/' + id, model);
                },
                delete: function (id) {
                    return $http.delete(config.apiUrl + 'app_draft_user/delete/' + id);
                },
                getAllProfiles: function () {
                    return $http.post(config.apiUrl + 'profile/get_all');
                },
                getAllRoles: function () {
                    return $http.post(config.apiUrl + 'role/get_all');
                }
            };
        }
    ]);