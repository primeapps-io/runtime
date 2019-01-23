'use strict';

angular.module('primeapps')

    .factory('AppCollaboratorsService', ['$rootScope', '$http', 'config', '$filter', '$q', 'helper',   '$cache', 'dataTypes', 'systemFields',
        function ($rootScope, $http, config, $filter, $q, helper,  $cache, dataTypes, systemFields) {
            return {
                getCollaborators: function (id) {
                    return $http.get(config.apiUrl + 'app/get_collaborators/' + id);
                }
            };
        }]);

