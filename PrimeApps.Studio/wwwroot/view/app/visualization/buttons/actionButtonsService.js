'use strict';

angular.module('primeapps')

    .factory('ActionButtonsService', ['$http', 'config', 'environments',
        function ($http, config, environments) {
            return {
                getActionButtons: function (moduleId) {
                    return $http.get(config.apiUrl + 'action_button/get/' + moduleId);
                },
                createActionButton: function (actionButton) {
                    return $http.post(config.apiUrl + 'action_button/create', actionButton);
                },

                updateActionButton: function (actionButton) {
                    return $http.put(config.apiUrl + 'action_button/update/' + actionButton.id, actionButton);
                },

                deleteActionButton: function (id) {
                    return $http.delete(config.apiUrl + 'action_button/delete/' + id);
                },
                count: function (id) {
                    return $http.get(config.apiUrl + 'action_button/count/' + id);
                },
                find: function (id, data) {
                    return $http.post(config.apiUrl + 'action_button/find/' + id, data);
                },
                getEnvironments: function () {
                    return environments.data;
                },
            }
        }
    ]);