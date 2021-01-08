'use strict';

angular.module('primeapps')
    .factory('SmsService', ['$http', 'config',
        function ($http, config) {
            return {
                getSetting: function () {
                    return $http.get(config.apiUrl + 'messaging/get_config');
                },
                updateSMSSettings: function (smsModel) {
                    return $http.post(config.apiUrl + 'messaging/update_sms_settings', smsModel);
                },
                updateEMailSettings: function (emailModel) {
                    return $http.post(config.apiUrl + 'messaging/update_email_settings', emailModel);
                },
                updatePersonalEMailSettings: function (emailModel) {
                    return $http.post(config.apiUrl + 'messaging/update_personal_email_settings', emailModel);
                },
                removePersonalEMailSettings: function (emailModel) {
                    return $http.post(config.apiUrl + 'messaging/remove_personal_email_settings', emailModel);
                },
                removeEMailSettings: function (emailModel) {
                    return $http.post(config.apiUrl + 'messaging/remove_email_settings', emailModel);
                },
                removeSMSSettings: function (emailModel) {
                    return $http.post(config.apiUrl + 'messaging/remove_sms_settings', emailModel);
                }
            };
        }
    ]);