'use strict';

angular.module('primeapps')

    .factory('genericInterceptor', ['$q', '$injector', '$window', '$localStorage', '$filter', '$cookies', '$rootScope',
        function ($q, $injector, $window, $localStorage, $filter, $cookies, $rootScope) {
            return {
                request: function (config) {
                    config.headers = config.headers || {};

                    var accessToken = $localStorage.read('access_token');

                    if ((cdnUrl && config.url.indexOf(cdnUrl) > -1) || (blobUrl && config.url.indexOf(blobUrl) > -1) || (functionUrl && config.url.indexOf(functionUrl) > -1))
                        config.headers['Access-Control-Allow-Origin'] = '*';
                    else if (accessToken && config.url.indexOf('/token') < 0 && (blobUrl === '' || config.url.indexOf(blobUrl) < 0) && (functionUrl === '' || config.url.indexOf(functionUrl) < 0))
                        config.headers['Authorization'] = 'Bearer ' + accessToken;

                    if (functionUrl && config.url.indexOf(functionUrl) > -1) {
                        config.headers['user_id'] = $rootScope.user.ID;
                        config.headers['tenant_id'] = $rootScope.user.tenantId;

                        if ($rootScope.branchAvailable) {
                            config.headers['branch_id'] = $rootScope.user.branchId;
                        }
                    }

                    // if ($rootScope.currentOrganization)
                    //     config.headers['X-Organization-Id'] = $rootScope.currentOrgId;

                    var organizationId = $rootScope.currentOrgId ? $rootScope.currentOrgId : null;

                    var appId = $rootScope.currentAppId != "undefined" && $rootScope.currentAppId ? $rootScope.currentAppId : null;

                    var tenantId = $cookies.get('tenant_id');

                    if (organizationId)
                        config.headers['X-Organization-Id'] = organizationId;

                    if (appId && appId != "undefined")
                        config.headers['X-App-Id'] = appId;

                    if (tenantId)
                        config.headers['X-Tenant-Id'] = tenantId;

                    return config;
                },
                responseError: function (rejection) {
                    if (rejection.status === 401) {
                        if (rejection.config.url.indexOf('/token') > -1)
                            return $q.reject(rejection);

                        if (rejection.statusText === 'Unauthorized') {
                            $localStorage.remove('access_token');
                            $localStorage.remove('refresh_token');
                            // $window.location.href = '/auth/SignOut';
                        } else {
                            $window.location.href = '/';
                        }


                        return;
                    }

                    if (rejection.status === 500 && rejection.config.url.indexOf('/User/MyAccount') > -1) {
                        $localStorage.remove('access_token');
                        $localStorage.remove('refresh_token');

                        $window.location.href = '/auth/SignOut';

                        return $q.reject(rejection);
                    }

                    if (rejection.status === 402) {
                        $window.location.href = '#/paymentform';
                        return $q.reject(rejection);
                    }

                    if (rejection.status === 403) {
                        $window.location.href = '#/app/dashboard';
                        toastr.error($filter('translate')('Common.Forbidden'));

                        return $q.reject(rejection);
                    }

                    if (rejection.status === 404) {
                        if (!rejection.config.ignoreNotFound) {
                            $window.location.href = '#/app/dashboard';
                            toastr.warning($filter('translate')(rejection.config.url.indexOf('/module') > -1 ? 'Common.NotFoundRecord' : 'Common.NotFound'));
                        }

                        return $q.reject(rejection);
                    }

                    if (!navigator.onLine || rejection.status === 421 || rejection.status === 429) {
                        toastr.warning($filter('translate')('Common.NetworkError'));

                        return $q.reject(rejection);
                    }

                    if (rejection.status === 400 || rejection.status === 409) {
                        return $q.reject(rejection);
                    }
                    
                    if(rejection.status === 503 && rejection.config.url.contains('functions/run')){
                        return $q.reject(rejection);
                    }

                    toastr.error($filter('translate')('Common.Error'));

                    return $q.reject(rejection);
                }
            }
        }]);