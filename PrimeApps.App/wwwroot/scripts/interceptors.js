'use strict';

angular.module('ofisim')

    .factory('genericInterceptor', ['$q', '$injector', '$window', '$localStorage', '$filter', 'ngToast',
        function ($q, $injector, $window, $localStorage, $filter, ngToast) {
            return {
                request: function (config) {
                    config.headers = config.headers || {};

                    var accessToken = $localStorage.read('access_token');

                    if (cdnUrl && config.url.indexOf(cdnUrl) > -1)
                        config.headers['Access-Control-Allow-Origin'] = '*';
                    else if (accessToken && config.url.indexOf('/token') < 0)
                        config.headers['Authorization'] = 'Bearer ' + accessToken;

                    return config;
                },
                responseError: function (rejection) {
                    if (rejection.status === 401) {
                        if (rejection.config.url.indexOf('/token') > -1)
                            return $q.reject(rejection);

                        $window.location.href = '/auth/authorize?client_id=self&response_type=token&state=' + encodeURIComponent('/#/app/crm/dashboard');

                        return;
                    }

                    if (rejection.status === 500 && rejection.config.url.indexOf('/User/MyAccount') > -1) {
                        $localStorage.remove('access_token');
                        $localStorage.remove('refresh_token');

                        $window.location.href = '/Auth/SignOut';

                        return $q.reject(rejection);
                    }

                    if (rejection.status === 402) {
                        $window.location.href = '#/paymentform';
                        return $q.reject(rejection);
                    }

                    if (rejection.status === 403) {
                        $window.location.href = '#/app/crm/dashboard';
                        ngToast.create({ content: $filter('translate')('Common.Forbidden'), className: 'danger' });
                        return $q.reject(rejection);
                    }

                    if (rejection.status === 404) {
                        if (!rejection.config.ignoreNotFound) {
                            $window.location.href = '#/app/crm/dashboard';
                            ngToast.create({ content: $filter('translate')(rejection.config.url.indexOf('/module') > -1 ? 'Common.NotFoundRecord' : 'Common.NotFound'), className: 'warning' });
                        }

                        return $q.reject(rejection);
                    }

                    if (!navigator.onLine || rejection.status === 421 || rejection.status === 429) {
                        ngToast.create({ content: $filter('translate')('Common.NetworkError'), className: 'warning' });
                        return $q.reject(rejection);
                    }

                    if (rejection.status === 400 || rejection.status === 409) {
                        return $q.reject(rejection);
                    }

                    ngToast.create({ content: $filter('translate')('Common.Error'), className: 'danger' });

                    return $q.reject(rejection);
                }
            }
        }]);