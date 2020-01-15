'use strict';

angular.module('primeapps')

    .factory('MenusService', ['$http', 'config', '$filter',
        function ($http, config, $filter) {
            return {
                create: function (menu) {
                    return $http.post(config.apiUrl + 'menu/create', menu);
                },
                update: function (id, menu) {
                    return $http.put(config.apiUrl + 'menu/update/' + id, menu);
                },
                delete: function (id) {
                    return $http.delete(config.apiUrl + 'menu/delete/' + id);
                },
                createMenuItems: function (module, id) {
                    return $http.post(config.apiUrl + 'menu/create/menu_items', { module: module, menuId: id });
                },
                updateMenuItems: function (menuLabel) {
                    return $http.put(config.apiUrl + 'menu/update/menu_items', { menuLabel: menuLabel });
                },
                deleteMenuItems: function (ids) {
                    return $http({
                        method: 'DELETE',
                        url: config.apiUrl + 'menu/delete/menu_items',
                        data: ids,
                        headers: { 'Content-type': 'application/json;charset=utf-8' }
                    });
                },
                getMenuById: function (id) {
                    return $http.get(config.apiUrl + 'menu/get_menu/' + id);
                },
                getAllMenus: function () {
                    return $http.get(config.apiUrl + 'menu/get_all');
                },
                getMenuItem: function (id) {
                    return $http.get(config.apiUrl + 'menu/get/' + id);
                },

                getIcons: function () {
                    return icons.icons;
                },

                count: function () {
                    return $http.get(config.apiUrl + 'menu/count');
                },

                find: function (data) {
                    return $http.post(config.apiUrl + 'menu/find', data);
                },
                getNotUsedProfiles: function (id) {
                    return $http.get(config.apiUrl + 'menu/get_not_used_profiles' + (id ? '/?id=' + id : ''));
                },
            };
        }]);

