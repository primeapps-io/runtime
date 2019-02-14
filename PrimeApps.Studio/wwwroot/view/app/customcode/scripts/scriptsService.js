angular.module('primeapps')

    .factory('ScriptsService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config) {
            return {

                getAll: function () {
                    return $http.post(config.apiUrl + 'view/create', view);
                }

            };
        }]);