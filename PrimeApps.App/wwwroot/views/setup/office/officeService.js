'use strict';

angular.module('ofisim')
    .factory('OfficeService', ['$http', 'config',
        function ($http, config) {
            return {
                getByKey: function (settingType, key, userId) {
                    return $http.get(config.apiUrl + 'settings/get_by_key/' + settingType + '/' + key + (userId ? '&user_id=' + userId : ''));
                },
                create: function (setting) {
                    return $http.post(config.apiUrl + 'settings/create', setting);
                },
                activeDirectoryInfo: function () {
                    return $http.get(config.apiUrl + 'User/ActiveDirectoryInfo');
                },
            };
        }
    ]);