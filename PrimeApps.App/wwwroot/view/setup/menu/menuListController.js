'use strict';

angular.module('primeapps')

    .controller('MenuListController', ['$rootScope', '$scope', '$filter', 'ngToast', '$dropdown', 'MenuService', '$window', 'ModuleService', 'AppService',
        function ($rootScope, $scope, $filter, ngToast, $dropdown, MenuService, $window, ModuleService, AppService) {


            // $scope.menuList = [];
            $scope.loading = true;

            function getAll() {
                MenuService.getAllMenus().then(function (response) {
                    $scope.menuList = response.data;
                    for (var i = 0; i < response.data.length; i++) {
                        $scope.menuList[i].id = response.data[i].id;
                        $scope.menuList[i].name = response.data[i].name;
                        $scope.menuList[i].profile_name = $filter('filter')($scope.profiles, { id: response.data[i].profile_id }, true)[0].name;
                    }
                    if (response.data.length < 1)
                        getView();

                    $scope.loading = false;
                });
            }

            getAll();

            $scope.delete = function (id) {
                //First delete Menu
                MenuService.delete(id).then(function (res) {
                    //Then, delete MenuItems
                    ngToast.create({ content: $filter('translate')('Menu.DeleteSuccess'), className: 'success' });//content: $filter('translate')('Setup.UserGroups.DeleteSuccess'), className: 'success' });
                    getAll();
                });
            };

            $scope.openDropdown = function (menuItem) {
                $scope['dropdown' + menuItem.name] = $scope['dropdown' + menuItem.name] || $dropdown(angular.element(document.getElementById('actionButton-' + menuItem.name)), {
                    placement: 'bottom-right',
                    scope: $scope,
                    animation: '',
                    show: true
                });

                var menuItems = [
                    {
                        'text': $filter('translate')('Common.Edit'),
                        'href': '#/app/setup/menu?id=' + menuItem.id
                    },
                    {
                        'text': $filter('translate')('Common.Copy'),
                        'href': '#app/setup/menu?id=' + menuItem.id + '&clone=true'
                    }
                ];

                if (!menuItem.default) {
                    menuItems.push(
                        {
                            'text': $filter('translate')('Common.Delete'),
                            'click': 'delete(\'' + menuItem.id + '\')',
                        }
                    );
                }

                $scope['dropdown' + menuItem.name].$scope.content = menuItems;

            };

            function getView() {
                AppService.getMyAccount(true);
            }

        }]);
