'use strict';

angular.module('ofisim')

    .factory('AuthService', ['$http', 'config', '$localStorage', '$window', '$location', '$timeout', '$rootScope', '$q', '$sessionStorage', '$cache',
        function ($http, config, $localStorage, $window, $location, $timeout, $rootScope, $q, $sessionStorage, $cache) {
            return {
                isUniqueEmail: function (email) {
                    return $http.get(config.apiUrl + 'Public/IsUniqueEmail?email=' + email);
                },

                register: function (userModel) {
                    return $http.post(config.apiUrl + 'account/register', userModel);
                },

                activate: function (userId, token, culture) {
                    return $http.get(config.apiUrl + 'account/activate?userId=' + userId + '&token=' + token + '&culture=' + culture);
                },

                token: function (email, password) {
                    var data = 'grant_type=password&client_id=' + config.clientId + '&username=' + email + '&password=' + password;

                    return $http.post(config.apiUrl + 'token', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } });
                },

                refresh: function () {
                    var deferred = $q.defer();
                    var refreshToken = $localStorage.get('refresh_token');
                    var data = 'grant_type=refresh_token&client_id=' + config.clientId + '&refresh_token=' + refreshToken;

                    $http.post(config.apiUrl + 'token', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } })
                        .then(function (response) {
                            $localStorage.set('access_token', response.data.access_token);
                            $localStorage.set('refresh_token', response.data.refresh_token);

                            deferred.resolve(response.data);
                        })
                        .catch(function (response) {
                            $localStorage.remove('access_token');
                            $localStorage.remove('refresh_token');
                            deferred.reject(response.data);
                        });

                    return deferred.promise;
                },

                logout: function () {
                    return $http.post(config.apiUrl + 'account/logout', {});
                },

                checkCampaignCode: function (code, licenseId) {
                    return $http.post(config.apiUrl + 'Public/CheckCampaignCode', {
                        Code: code,
                        LicenseID: licenseId
                    });
                },

                resendActivationMail: function (email, culture) {
                    return $http.get(config.apiUrl + 'account/resend_activation?email=' + email + '&culture=' + culture);
                },

                changePassword: function (oldPassword, newPassword, confirmPassword) {
                    return $http.post(config.apiUrl + 'account/change_password', {
                        OldPassword: oldPassword,
                        NewPassword: newPassword,
                        ConfirmPassword: confirmPassword
                    });
                },

                forgotPassword: function (email, culture) {
                    return $http.get(config.apiUrl + 'account/forgot_password?email=' + email + '&culture=' + culture);
                },

                resetPassword: function (userId, token, password, confirmPassword) {
                    return $http.post(config.apiUrl + 'account/reset_password', {
                        UserId: userId,
                        Token: token,
                        Password: password,
                        ConfirmPassword: confirmPassword
                    });
                },

                logoutComplete: function () {
                    $localStorage.remove('access_token');
                    $localStorage.remove('refresh_token');
                    $localStorage.remove('Workgroup');
                    $sessionStorage.clear();
                    $cache.removeAll();
                },

                isAuthenticated: function () {
                    return !!$localStorage.read('access_token');
                },

                resetScope: function () {
                    var authScope = ['authLogo', 'currentPath', 'pageTitle', 'theme', 'app'];
                    for (var prop in $rootScope) {
                        if (prop.substring(0, 1) !== '$') {
                            if (authScope.indexOf(prop) === -1)
                                delete $rootScope[prop];
                        }
                    }
                },

                getCustomInfo: function (domain) {
                    return $http.get(config.apiUrl + 'Public/GetCustomInfo?customDomain=' + domain);
                }
            };
        }]);