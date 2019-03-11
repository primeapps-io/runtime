angular.module('primeapps')

    .factory('AppEmailTemplatesService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config,) {
            return {
                create: function (appTemplate, currentAppName) {
                    return $http.post(config.apiUrl + 'template/create_app_email_template/' + currentAppName, appTemplate);
                },
                update: function (appTemplate) {
                    return $http.put(config.apiUrl + 'template/update_app_email_template/' + appTemplate.id, appTemplate);
                },
                count: function (currentAppName) {
                    return $http.get(config.apiUrl + 'template/count_app_email_template?currentAppName=' + currentAppName);
                },
                find: function (data, currentAppName) {
                    return $http.post(config.apiUrl + 'template/find_app_email_template?currentAppName=' + currentAppName, data);
                }
            };
        }]);