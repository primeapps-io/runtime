'use strict';

angular.module('ofisim')

    .factory('LicenseService', ['$http', 'config',
        function ($http, config) {
            return {

                getLicense: function () {
                    return $http.post(config.apiUrl + 'License/GetAll', {});
                },
                getLicenseStatus: function () {
                    return $http.post(config.apiUrl + 'License/GetLicenseStatus', {});
                }, 
                getUserLicenseStatus: function() {
                    return $http.post(config.apiUrl + 'License/GetUserLicenseStatus', {});
                },
                change: function (licenseId, frequency) {
                    return $http.post(config.apiUrl + 'License/Upgrade', {
                        LicenseID: "7673e999-18fb-497f-a958-84dca43031cc",
                        Frequency: frequency
                    });
                },

                addAddonLicense: function (userCount, storageCount) {
                    return $http.post(config.apiUrl + 'License/AddAddonLicense', {
                        UserCount: userCount,
                        StorageCount: storageCount
                    });
                },

                removeAddonLicense: function (type) {
                    var addonLicenseType = 1;//storage

                    if (type == 'user')
                        addonLicenseType = 3;//user

                    return $http.post(config.apiUrl + 'License/RemoveAddonLicense', angular.toJson(addonLicenseType));
                }

            };
        }]);