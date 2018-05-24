'use strict';

angular.module('ofisim')

    .factory('UserGroupService', ['$rootScope', '$http', 'config', '$filter',
        function ($rootScope, $http, config, $filter) {
            return {
                getAll: function () {
                    return $http.get(config.apiUrl + 'user_group/get_all');
                },

                create: function (userGroup) {
                    return $http.post(config.apiUrl + 'user_group/create', userGroup);
                },

                update: function (userGroup) {
                    return $http.put(config.apiUrl + 'user_group/update/' + userGroup.id, userGroup);
                },

                delete: function (id) {
                    return $http.delete(config.apiUrl + 'user_group/delete/' + id);
                },

                prepare: function (userGroup) {
                    userGroup.user_ids = [];

                    angular.forEach(userGroup.users, function (user) {
                        userGroup.user_ids.push(user.id);
                    });

                    delete userGroup.users;

                    return userGroup;
                }
            };
        }]);