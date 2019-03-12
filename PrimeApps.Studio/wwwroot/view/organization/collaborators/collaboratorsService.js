'use strict';

angular.module('primeapps')

    .factory('CollaboratorsService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config) {
            return {
                find: function (model, id) {
                    return $http.post(config.apiUrl + 'organization/find/' + id, model);
                },

                count: function (id) {
                    return $http.get(config.apiUrl + 'organization/count/' + id);
                },

                get: function (filter) {
                    return $http.post(config.apiUrl + 'organization/collaborators', filter);
                },

                getTeams: function (filter) {
                    return $http.post(config.apiUrl + 'organization/teams', filter);
                },

                save: function (colobotaros) {
                    return $http.post(config.apiUrl + 'organization/add_user', colobotaros);
                },

                update: function (colobotaros) {
                    return $http.put(config.apiUrl + 'organization/update_user', colobotaros);
                },

                delete: function (colobotaros) {
                    return $http.post(config.apiUrl + 'organization/delete_user', colobotaros);
                },
                sendEmail: function (data) {
                    return $http.post(config.apiUrl + 'organization/send_email_password/', data);
                }
            };
        }]);