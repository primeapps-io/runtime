'use strict';

angular.module('ofisim')
    .factory('GeneralSettingsService', ['$http', 'config',
        function ($http, config) {
            return {
                getByKey: function (settingType, key, userId) {
                    return $http.get(config.apiUrl + 'settings/get_by_key/' + settingType + '/' + key + (userId ? '&user_id=' + userId : ''));
                },
                create: function (setting) {
                    return $http.post(config.apiUrl + 'settings/create', setting);
                },
                update: function (setting) {
                    return $http.put(config.apiUrl + 'settings/update/' + setting.id, setting);
                },
                getThemes: function () {
                    return [
                        {
                            name: 'ofisim-crm',
                            color: '#d5190f'
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
                            color: '#009688'
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
                        }
                    ];
                }
            };
        }
    ]);