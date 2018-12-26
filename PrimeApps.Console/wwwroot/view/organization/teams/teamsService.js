'use strict';

angular.module('primeapps')

    .factory('TeamsService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config) {
            return {
                get: function (id) {
                    return $http.get(config.apiUrl + 'team/get/' + id);
                },

                getAll: function () {
                    return $http.get(config.apiUrl + 'team/get_all');
                },

                create: function (team) {
                    return $http.post(config.apiUrl + 'team/create', team);
                },

                update: function (id, team) {
                    return $http.put(config.apiUrl + 'team/update/' + id, team);
                },

                delete: function (id) {
                    return $http.delete(config.apiUrl + 'team/delete/' + id);
                },

                deleteUser: function (id, team) {
                    return $http.post(config.apiUrl + 'team/team_user_delete/' + id, team);
                },

                userAddForTeam: function (userId, teamUser) {
                    return $http.post(config.apiUrl + 'team/team_user_add/' + userId, teamUser);
                },

                isUniqueName: function (name) {
                    return $http.get(config.apiUrl + 'team/is_unique_name?name=' + name);
                },

                //TODO 
                getOrganizationUsers: function (id) {
                    return $http.get(config.apiUrl + 'organization/get_users/' + id);
                }
            };
        }]);