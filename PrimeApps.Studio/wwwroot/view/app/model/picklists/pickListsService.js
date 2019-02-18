'use strict';

angular.module('primeapps')

    .factory('PickListsService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config) {
            return { 
                get: function (id) {
                    return $http.get(config.apiUrl + "picklist/get/" + id);
                },
                getAll: function () {
                    return $http.get(config.apiUrl + 'picklist/get_all');
                },
                find: function (model) {
                    return $http.post(config.apiUrl + 'picklist/find', model);
                },
                getPage: function (model) {
                    return $http.post(config.apiUrl + 'picklist/get_page', model);
                },
                count: function () {
                    return $http.get(config.apiUrl + 'picklist/count');
                },
                create: function (model) {
                    return $http.post(config.apiUrl + 'picklist/create', model);
                },
                update: function (model) {
                    return $http.put(config.apiUrl + 'picklist/update/' + id, model);
                },
                delete: function (id) {
                    return $http.delete(config.apiUrl + 'picklist/delete/' + id);
                } 
            };
        }]);