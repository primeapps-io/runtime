'use strict';

angular.module('primeapps')

    .factory('ProfileService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config) {
            return {
                editUser: function (user) {
                    return $http.put(config.apiUrl + 'user/edit', {
                       // id: user.id,
                        first_name: user.first_name,
                        last_name: user.last_name,
                        email: user.email
                        //  password: user.password,
                        // picture: user.picture
                    });
                },

                changePassword: function (currentPassword, newPassword, confirmPassword) {
                    return $http.post(config.apiUrl + 'account/change_password', {
                        OldPassword: currentPassword,
                        NewPassword: newPassword,
                        ConfirmPassword: confirmPassword
                    });
                },
            };
        }]);

