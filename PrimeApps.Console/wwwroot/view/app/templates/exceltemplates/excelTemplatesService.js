angular.module('primeapps')

    .factory('ExcelTemplatesService', ['$rootScope', '$http', 'config', '$q', '$filter',
        function ($rootScope, $http, config, $q, $filter) {
            return {

                get: function (id) {
                    return $http.get(config.apiUrl + 'template/get/' + id);
                },

                getAll: function (templateType) {
                    return $http.get(config.apiUrl + 'template/get_all?TemplateType=' + templateType);
                },

                create: function (quoteTemplate) {
                    return $http.post(config.apiUrl + 'template/create', quoteTemplate);
                },
                update: function (quoteTemplate) {
                    return $http.put(config.apiUrl + 'template/update/' + quoteTemplate.id, quoteTemplate);
                },

                delete: function (id) {
                    return $http.delete(config.apiUrl + 'template/delete/' + id);
                },
                count: function (templateType) {
                    return $http.get(config.apiUrl + 'template/count?TemplateType=' + templateType);
                },
                find: function (data, templateType) {
                    return $http.post(config.apiUrl + 'template/find?TemplateType=' + templateType, data);
                }
            };
        }]);