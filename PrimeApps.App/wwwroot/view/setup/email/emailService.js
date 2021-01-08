'use strict';

angular.module('primeapps')
    .factory('EmailService', ['$http', 'config',
        function ($http, config) {
            return {
                getSetting: function () {
                    return $http.get(config.apiUrl + 'messaging/get_config');
                },
                updateEMailSettings: function (emailModel) {
                    return $http.post(config.apiUrl + 'messaging/update_email_settings', emailModel);
                },
                updatePersonalEMailSettings: function (emailModel) {
                    return $http.post(config.apiUrl + 'messaging/update_personal_email_settings', emailModel);
                },
                removeEMailSettings: function (emailModel) {
                    return $http.post(config.apiUrl + 'messaging/remove_email_settings', emailModel);
                },
                removePersonalEMailSettings: function (emailModel) {
                    return $http.post(config.apiUrl + 'messaging/remove_personal_email_settings', emailModel);
                }
            };
        }
    ]);