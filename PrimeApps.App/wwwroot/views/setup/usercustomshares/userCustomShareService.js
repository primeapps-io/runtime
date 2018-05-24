'use strict';

angular.module('ofisim')

    .factory('UserCustomShareService', ['$http', 'config',
        function ($http, config) {
            return {
                get: function (id) {
                    return $http.get(config.apiUrl + 'user_custom_shares/get/' + id);
                },
                getAll: function () {
                    return $http.get(config.apiUrl + 'user_custom_shares/get_all');
                },

                create: function (userowner) {
                    return $http.post(config.apiUrl + 'user_custom_shares/create', userowner);
                },

                update: function (id, userowner) {
                    return $http.put(config.apiUrl + 'user_custom_shares/update/' + id, userowner);
                },

                delete: function (id) {
                    return $http.delete(config.apiUrl + 'user_custom_shares/delete/' + id);
                },

                getUserGroups: function () {
                    return $http.get(config.apiUrl + 'user_group/get_all');
                },

            };
        }]);