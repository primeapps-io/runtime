'use strict';

angular.module('primeapps')
    .factory('GeneralSettingsService', ['$http', 'config',
        function ($http, config) {
            return {
                getByKey: function (settingType, key, userId) {
                    return $http.get(config.apiUrl + 'settings/get_by_key/' + settingType + '/' + key + (userId ? '?userId=' + userId : ''));
                },
                create: function (setting) {
                    return $http.post(config.apiUrl + 'settings/create', setting);
                },
                update: function (setting) {
                    return $http.put(config.apiUrl + 'settings/update/' + setting.id, setting);
                }                 
            };
        }
    ]);