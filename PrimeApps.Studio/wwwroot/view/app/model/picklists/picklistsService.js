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
                find: function (ids) {
                    return $http.post(config.apiUrl + 'picklist/find', ids);
                },
                getPage: function (model) {
                    return $http.post(config.apiUrl + 'picklist/get_page', model);
                },
                getItemPage: function (id, model) {
                    return $http.post(config.apiUrl + 'picklist/get_item_page/' + id, model);
                },
                count: function () {
                    return $http.get(config.apiUrl + 'picklist/count');
                },
                countItems: function (id) {
                    return $http.get(config.apiUrl + 'picklist/count_items/' + id);
                },
                create: function (model) {
                    return $http.post(config.apiUrl + 'picklist/create', model);
                },
                createItem: function (id, model) {
                    return $http.post(config.apiUrl + 'picklist/add_item/' + id, model);
                },
                update: function (model) {
                    return $http.put(config.apiUrl + 'picklist/update/' + model.id, model);
                },
                updateItem: function (id, model) {
                    return $http.put(config.apiUrl + 'picklist/update_item/' + id, model);
                },
                delete: function (id) {
                    return $http.delete(config.apiUrl + 'picklist/delete/' + id);
                },
                deleteItem: function (itemId) {
                    return $http.delete(config.apiUrl + 'picklist/delete_item/' + itemId);
                },
                isUniqueCheck: function (name) {
                    return $http.get(config.apiUrl + 'picklist/is_unique_check/' + name);
                }
            };
        }]);