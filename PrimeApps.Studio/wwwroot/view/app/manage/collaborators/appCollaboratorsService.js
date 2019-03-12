'use strict';

angular.module('primeapps')

    .factory('AppCollaboratorsService', ['$rootScope', '$http', 'config', '$filter', '$q', 'helper',   '$cache', 'dataTypes', 'systemFields',
        function ($rootScope, $http, config, $filter, $q, helper,  $cache, dataTypes, systemFields) {
            return {
                getCollaborators: function (id) {
                    return $http.get(config.apiUrl + 'app/get_collaborators/' + id);
                },
                getTeamsByOrganizationId: function (id) {
                    return $http.get(config.apiUrl + 'team/get_by_organization_id/' + id);
                },
                getUsersByOrganizationId: function (id) {
                    return $http.get(config.apiUrl + 'organization/get_users/' + id);
                },
                addAppCollaborator: function (item) {
                    return $http.post(config.apiUrl + 'app_collaborator/create', item);
                },
                updateAppCollaborator: function (id, item) {
                    return $http.put(config.apiUrl + 'app_collaborator/update/' + id, item);
                },
                delete: function (id) {
                    return $http.delete(config.apiUrl + 'app_collaborator/delete/' + id);
                }
            };
        }]);

