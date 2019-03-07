'use strict';

angular.module('primeapps')

    .factory('RolesService', ['$rootScope', '$http', 'config', '$filter', '$q', 'helper', '$cache', 'dataTypes', 'systemFields',
        function ($rootScope, $http, config, $filter, $q, helper, $cache, dataTypes, systemFields) {
            return {

                getAll: function () {
                    return $http.post(config.apiUrl + 'role/get_all', {});
                },

                create: function (role) {
                    return $http.post(config.apiUrl + 'role/create', role);
                },

                update: function (role, role_change) {
                    return $http.put(config.apiUrl + 'role/update?roleChange=' + role_change, role);
                },

                delete: function (id, transferRoleId) {
                    return $http.delete(config.apiUrl + 'role/delete?id=' + id + '&transferRoleId=' + transferRoleId);
                },

                updateUserRole: function (userId, roleId) {
                    return $http.put(config.apiUrl + 'role/update_user_role?user_Id=' + userId + '&role_Id=' + roleId);
                }
            };
        }]);

