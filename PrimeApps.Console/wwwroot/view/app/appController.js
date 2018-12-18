'use strict';

angular.module('primeapps')

    .controller('AppController', ['$rootScope', '$scope', '$filter', '$location', 'helper',
        function ($rootScope, $scope, $filter, $location, helper) {
            $scope.helper = helper;
            var path = $location.path();


            $scope.selectMenuItem = function (menuItem) {
                $rootScope.selectedSetupMenuLink = menuItem.link;
            };

            $scope.menuItems = [
                {link: '#/app/setup/settings', label: 'Setup.Nav.PersonalSettings', order: 1, app: 'crm'},
                {link: '#/app/setup/importhistory', label: 'Setup.Nav.Data', order: 7, app: 'crm'}
            ];


            var menuItemsAdmin = [
                {link: '#/app/setup/users', label: 'Setup.Nav.Users', order: 2, app: 'crm'},
                {
                    link: '#/app/setup/organization',
                    label: 'Setup.Nav.OrganizationSettings',
                    order: 3,
                    app: 'crm'
                },
                {link: '#/app/setup/modules', label: 'Setup.Nav.Customization', order: 6, app: 'crm'},
                {link: '#/app/setup/general', label: 'Setup.Nav.System', order: 8, app: 'crm'},
                {link: '#/app/setup/workflows', label: 'Setup.Nav.Workflow', order: 9, app: 'crm'},
                {
                    link: '#/app/setup/approvel_process',
                    label: 'Setup.Nav.ApprovelProcess',
                    order: 10,
                    app: 'crm'
                },
                {link: '#/app/setup/help', label: 'Setup.Nav.HelpGuide', order: 11, app: 'crm'},
                {link: '#/app/setup/menu_list', label: 'Setup.Nav.Menu', order: 12, app: 'crm'}
            ];
            menuItemsAdmin.push({
                link: '#/app/setup/warehouse',
                label: 'Setup.Nav.Warehouse',
                order: 11,
                app: 'crm'
            });

            var allMenuItemsAdmin = $scope.menuItems.concat(menuItemsAdmin);
            $scope.menuItems = $filter('orderBy')(allMenuItemsAdmin, 'order');


            var menuItem = $filter('filter')($scope.menuItems, {link: '#' + path})[0];

            if (menuItem)
                $scope.selectMenuItem(menuItem);
            else
                $scope.selectMenuItem($scope.menuItems[0]);

            angular.forEach($scope.menuItems, function (menuItem) {
                if (menuItem.app === 'common' || menuItem.app === $rootScope.app)
                    menuItem.active = true;
                else
                    menuItem.active = false;
            });

            $scope.menuItemClass = function (menuItem) {
                if ($rootScope.selectedSetupMenuLink === menuItem.link) {
                    return 'active';
                } else {
                    return '';
                }
            };
        }
    ]);