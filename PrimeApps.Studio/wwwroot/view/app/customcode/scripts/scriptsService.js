angular.module('primeapps')

    .factory('ScriptsService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config) {
            return {
                get: function (id) {
                    return $http.get(config.apiUrl + 'script/get/' + id);
                },
                getByName: function (name) {
                    return $http.get(config.apiUrl + 'script/get_by_name/' + name);
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
                    return $http.put(config.apiUrl + 'script/update/' + model.id, model);
                },
                delete: function (id) {
                    return $http.delete(config.apiUrl + 'script/delete/' + id);
                },
                isUniqueName: function (name) {
                    return $http.get(config.apiUrl + 'script/is_unique_name?name=' + name);
                },
                deploy: function (name) {
                    return $http.get(config.apiUrl + 'script/deploy/' + name);
                }
            };
        }]);