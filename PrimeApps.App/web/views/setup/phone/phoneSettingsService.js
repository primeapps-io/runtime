angular.module('ofisim')
    .factory('PhoneSettingsService', ['$http', 'config',
        function ($http, config) {
            return {
                saveSipProvider: function (provider) {
                    return $http.post(config.apiUrl + 'phone/save_provider', provider);
                },
                deleteSipSettings: function () {
                    return $http.post(config.apiUrl + 'phone/delete_all_settings');
                },
                getSipConfig: function () {
                    return $http.get(config.apiUrl + 'phone/get_config');
                },
                saveSipAccount: function (sipAccount) {
                    return $http.post(config.apiUrl + 'phone/save_sip_account', sipAccount);
                },
                deleteSipAccount: function (userId) {
                    return $http.delete(config.apiUrl + 'phone/delete_sip_account/' + userId);
                },
                getSipPassword: function () {
                    return $http.get(config.apiUrl + 'phone/get_sip_password');
                }
            };
        }
    ]);