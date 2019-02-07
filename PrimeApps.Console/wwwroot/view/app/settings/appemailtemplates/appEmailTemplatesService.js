angular.module('primeapps')

    .factory('AppEmailTemplatesService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config,) {
            return {
                create: function (appTemplate) {
                    return $http.post(config.apiUrl + 'template/create_app_email_template', appTemplate);
                },
                update: function (appTemplate) {
                    return $http.put(config.apiUrl + 'template/update_app_email_template/' + appTemplate.id, appTemplate);
                },

                count: function () {
                    return $http.get(config.apiUrl + 'template/count');
                },
                find: function (data) {
                    return $http.post(config.apiUrl + 'template/find', data);
                }
            };
        }]);