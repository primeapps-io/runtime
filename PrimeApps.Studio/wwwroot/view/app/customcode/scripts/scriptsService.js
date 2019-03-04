angular.module('primeapps')

    .factory('ScriptsService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config) {
            return {
                get: function () {
                    return $http.get(config.apiUrl + 'script/get/' + id);
                },
                count: function () {
                    return $http.get(config.apiUrl + 'script/count');
                },
                find: function (model) {
                    return $http.post(config.apiUrl + 'script/find', model);
                },
                create: function (model) {
                    return $http.post(config.apiUrl + 'script/create', model);
                },
                update: function (model) {
                    return $http.put(config.apiUrl + 'script/update', model);
                },
                delete: function (id) {
                    return $http.delete(config.apiUrl + 'script/delete/' + id);
                }
            };
        }]);