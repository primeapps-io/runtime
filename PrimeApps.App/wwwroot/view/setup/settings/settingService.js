'use strict';

angular.module('primeapps')
    .factory('SettingService', ['$http', 'config',
        function ($http, config) {
            return {

                editUser: function (user) {
                    return $http.post(config.apiUrl + 'User/Edit', {
                        id: user.id,
                        first_name: user.first_name,
                        last_name: user.last_name,
                        email: user.email,
                        password: user.password,
                        picture: user.picture,
                        phone: user.phone,
                        profile_picture: user.picture,
                        culture: user.culture,
                        language: user.language
                    });
                },

                removeUser: function (password) {
                    return $http.post(config.apiUrl + 'User/Remove', angular.toJson(password));
                },

                changePassword: function (currentPassword, newPassword, confirmPassword) {
                    return $http.post(config.apiUrl + 'account/change_password', {
                        old_password: currentPassword,
                        new_password: newPassword,
                        confirm_password: confirmPassword
                    });
                }
            };
        }
    ]);