'use strict';

angular.module('primeapps')

    .factory('MenuService', ['$rootScope', '$http', 'config',
        function ($rootScope, $http, config) {
            return {
                create: function (menu) {
                    return $http.post(config.apiUrl + 'menu/create', { menu: menu });
                },
                update: function (id, menu) {
                    return $http.put(config.apiUrl + 'menu/update/' + id,  menu );
                },
                delete: function (id) {
                    return $http.delete(config.apiUrl + 'menu/delete/' + id);
                },
                createMenuItems: function (module, profileId) {
                    return $http.post(config.apiUrl + 'menu/create/menu_items', { module: module, profileId: profileId });
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
                getMenuItem: function (profileId) {
                    return $http.get(config.apiUrl + 'menu/get/' + profileId);
                }
            };
        }]);