'use strict';

angular.module('ofisim')

    .factory('RoleService', ['$http', 'config',
        function ($http, config) {
            return {

                getAll: function () {
                    return $http.post(config.apiUrl + 'role/find_all', {});
                },

                create: function (role) {
                    return $http.post(config.apiUrl + 'role/create', role);
                },

                update: function (role) {
                    return $http.put(config.apiUrl + 'role/update', role);
                },

                delete: function (id, transferRoleId) {
                    return $http.delete(config.apiUrl + 'role/delete?id=' + id + '&transferRoleId=' + transferRoleId);
                },

                updateUserRole: function (userId, roleId) {
                    return $http.put(config.apiUrl + 'role/update_user_role?userId=' + userId + '&roleId=' + roleId);
                }

            };
        }]);