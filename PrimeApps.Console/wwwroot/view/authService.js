'use strict';

angular.module('primeapps')

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

                    return $http.post(config.apiUrl + 'token', data, {headers: {'Content-Type': 'application/x-www-form-urlencoded'}});
                },

                refresh: function () {
                    var deferred = $q.defer();
                    var refreshToken = $localStorage.get('refresh_token');
                    var data = 'grant_type=refresh_token&client_id=' + config.clientId + '&refresh_token=' + refreshToken;

                    $http.post(config.apiUrl + 'token', data, {headers: {'Content-Type': 'application/x-www-form-urlencoded'}})
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
  
                logoutComplete: function () {
                    $localStorage.remove('access_token');
                    $localStorage.remove('refresh_token');
                    $localStorage.remove('Workgroup');
                    $sessionStorage.clear();
                    $cache.removeAll();
                },

                isAuthenticated: function () {
                    return !!$localStorage.read('access_token');
                }
            };
        }]);