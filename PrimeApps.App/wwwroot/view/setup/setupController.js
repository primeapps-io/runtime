'use strict';

angular.module('primeapps')

    .controller('SetupController', ['$rootScope', '$scope', '$filter', '$location', 'helper',
        function ($rootScope, $scope, $filter, $location, helper) {
            $scope.helper = helper;

            $scope.selectMenuItem = function (menuItem) {
                $rootScope.selectedSetupMenuLink = menuItem.link;
            };

            $scope.setMenuItems = function () {
                $scope.menuItems = [
                    {link: '#/app/setup/settings', label: 'Setup.Nav.PersonalSettings', order: 1, app: 'crm'},
                    {link: '#/app/setup/importhistory', label: 'Setup.Nav.Data', order: 7, app: 'crm'}
                ];

                if (helper.hasAdminRights() && !$rootScope.preview) {
                    var menuItemsAdmin = [
                        {link: '#/app/setup/users', label: 'Setup.Nav.Users', order: 2, app: 'crm'},
                        {link: '#/app/setup/organization', label: 'Setup.Nav.OrganizationSettings', order: 3, app: 'crm'},
                        //{ link: '#/app/setup/modules', label: 'Setup.Nav.Customization', order: 6, app: 'crm' },
                        {link: '#/app/setup/general', label: 'Setup.Nav.System', order: 8, app: 'crm'},
                        //{ link: '#/app/setup/workflows', label: 'Setup.Nav.Workflow', order: 9, app: 'crm' },
                        //{ link: '#/app/setup/approvel_process', label: 'Setup.Nav.ApprovelProcess', order: 10, app: 'crm' },
                        //{ link: '#/app/setup/help', label: 'Setup.Nav.HelpGuide', order: 11, app: 'crm' },
                        //{ link: '#/app/setup/menu_list', label: 'Setup.Nav.Menu', order: 12, app: 'crm' }
                    ];

                    //if ($rootScope.workgroup.has_analytics)
                    //    menuItemsAdmin.push({link: '#/app/setup/warehouse', label: 'Setup.Nav.Warehouse', order: 11, app: 'crm'});

                    var allMenuItemsAdmin = $scope.menuItems.concat(menuItemsAdmin);
                    $scope.menuItems = $filter('orderBy')(allMenuItemsAdmin, 'order');
                }

                if (!helper.hasAdminRights()) {
                    if ($rootScope.customProfilePermissions && $rootScope.customProfilePermissions.length > 0) {
                        for (var i = 0; i < $rootScope.customProfilePermissions.length; i++) {
                            var profilePermission = $rootScope.customProfilePermissions[i];
                            if (profilePermission.profileId == $rootScope.user.profile.id) {
                                var customPermissionMenuItems = [];
                                for (var j = 0; j < profilePermission.permissions.length; j++) {
                                    var permission = profilePermission.permissions[j];
                                    if (permission == 'users')
                                        customPermissionMenuItems.push({ link: '#/app/setup/users', label: 'Setup.Nav.Users', order: 2, app: 'crm' })

                                    if (permission == 'general')
                                        customPermissionMenuItems.push({ link: '#/app/setup/general', label: 'Setup.Nav.System', order: 8, app: 'crm' })

                                    if (permission == 'organization')
                                        customPermissionMenuItems.push({ link: '#/app/setup/users', label: 'Setup.Nav.OrganizationSettings', order: 3, app: 'crm' })
                                }
                                var allMenuItems = $scope.menuItems.concat(customPermissionMenuItems);
                                $scope.menuItems = $filter('orderBy')(allMenuItems, 'order');

                            }
                        }
                    }
                }

                // Disabled due to removal of license and payment system
                //if (!$rootScope.licenseStatus.License.IsSingleWorkgroupLimited) {
                //    var menuItemsMember = [
                //        { link: '#/app/setup/license', label: 'Setup.Nav.LicenseInformation', order: 4, app: 'crm' },
                //        { link: '#/app/setup/payment', label: 'Setup.Nav.Payment', order: 5, app: 'crm' }
                //    ];

                //    var allMenuItemsUser = $scope.menuItems.concat(menuItemsMember);
                //    $scope.menuItems = $filter('orderBy')(allMenuItemsUser, 'order');
                //}

                var path = $location.path();

                switch (path) {
                    case '/app/setup/paymenthistory':
                        path = '/app/setup/payment';
                        break;
                    case '/app/setup/profiles':
                    case '/app/setup/profile':
                        path = '/app/setup/users';
                        break;
                    case 'app/setup/reportForm':
                        path = '/app/setup/reports';
                }

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
            };

            $scope.setMenuItems();

            $scope.menuItemClass = function (menuItem) {
                if ($rootScope.selectedSetupMenuLink === menuItem.link) {
                    return 'active';
                }
                else {
                    return '';
                }
            };
        }
    ]);