'use strict';

angular.module('ofisim')
    .factory('OutlookService', ['$http', 'config',
        function ($http, config) {
            return {
                getSettings: function () {
                    return $http.get(config.apiUrl + 'outlook/get_settings');
                },
                saveSettings: function (outlookSetting) {
                    return $http.post(config.apiUrl + 'outlook/save_settings', outlookSetting);
                },
                createMailModule: function () {
                    return $http.post(config.apiUrl + 'outlook/create_mail_module', {});
                }
            };
        }
    ]);