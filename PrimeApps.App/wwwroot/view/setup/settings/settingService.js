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
                        phone: user.phone
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
                },
                getThemes: function () {
                    return [
                        {
                            name: 'ofisim-crm',
                            color: '#1353ca'
                        },
                        {
                            name: 'green-theme',
                            color: '#b8c110'
                        },
                        {
                            name: 'blue-theme',
                            color: '#01a0e4'
                        },
                        {
                            name: 'green-dark-theme',
                            color: '#61b033'
                        },
                        {
                            name: 'orange-theme',
                            color: '#ee6d1a'
                        },
                        {
                            name: 'pink-theme',
                            color: '#e21550'
                        },
                        {
                            name: 'purple-theme',
                            color: '#b62f7c'
                        },
                        {
                            name: 'theme-1',
                            color: '#ce0404'
                        },
                        {
                            name: 'primeapps',
                            color: '#635fb3'
                        },
                        {
                            name: 'theme-3',
                            color: '#016d45'
                        },
                        {
                            name: 'ofisim-kobi',
                            color: '#49a43b'
                        },
                        {
                            name: 'theme-5',
                            color: '#F4511E'
                        },
                        {
                            name: 'theme-6',
                            color: '#607D8B'
                        },
                        {
                            name: 'theme-7',
                            color: '#3F5195'
                        },
                        {
                            name: 'theme-8',
                            color: '#2196F3'
                        },
                        {
                            name: 'ofisim-asistan',
                            color: '#e65946'
                        },
                        {
                            name: 'ofisim-ik',
                            color: '#3d3781'
                        },
                        {
                            name: 'ofisim-cagri',
                            color: '#517ee4'
                        }
                    ];
                }
            };
        }
    ]);