'use strict';
angular.module('primeapps').controller('AppController', ['$rootScope', '$scope', 'mdToast', '$mdToast', '$location', '$state', '$cookies', '$localStorage', '$window', '$filter', '$anchorScroll', 'config', 'entityTypes', 'guidEmpty', 'component', 'helper', 'operations', 'blockUI', '$cache', 'AppService', 'AuthService', '$sessionStorage', 'HelpService', '$sce', '$mdSidenav', '$mdDialog', '$mdMedia', 'icons2', 'GeneralSettingsService', 'SignalNotificationService', 'NotificationService', '$timeout', 'SettingService',
    function ($rootScope, $scope, mdToast, $mdToast, $location, $state, $cookies, $localStorage, $window, $filter, $anchorScroll, config, entityTypes, guidEmpty, component, helper, operations, blockUI, $cache, AppService, AuthService, $sessionStorage, HelpService, $sce, $mdSidenav, $mdDialog, $mdMedia, icons2, GeneralSettingsService, SignalNotificationService, NotificationService, $timeout, SettingService) {

        $scope.disablePasswordChange = disablePasswordChange;
        $scope.useUserSettings = useUserSettings;
        $rootScope.expressionRunOrderData = { Value: [], Validation: [], ValidateTime: [] }
        $rootScope.globalLoading = false;
        $rootScope.authorizeList = [];
        $rootScope.authorizedList = [];
        $rootScope.usersList = $filter('filter')(angular.copy($rootScope.users), function (user) {
            return user.id !== $rootScope.user.id;
        }, true);
        $rootScope.usersList = $filter('orderBy')($rootScope.usersList, 'full_name');
        $scope.selectedLanguage = $filter('filter')(languages, { label: globalization.Label, value: globalization.Culture }, true)[0];

        $scope.languageOptions = {
            dataSource: languages,
            dataTextField: 'text',
            dataValueField: "value",
            ingoreCase: true,
            filter: languages.length > 5 ? 'contains' : null,
            autoBind: false,
        };

        $rootScope.authorizationUserOptions = {
            dataSource: new kendo.data.DataSource({
                transport: {
                    read: function (o) {
                        o.success($rootScope.usersList)
                    }
                }
            }),
            dataTextField: 'full_name',
            dataValueField: "id",
            ingoreCase: true,
            filter: 'contains',
            autoBind: false,
            optionLabel: $filter('translate')('Auth.AddUser'),
            change: function (e) {
                if (!e || !e.sender || !e.sender.$angular_scope || !e.sender.$angular_scope.authorizationUser ||  !e.sender.$angular_scope.authorizationUser.id) return;

                var selectUser=  e.sender.$angular_scope.authorizationUser;
                const authorizationUser = $filter('filter')($rootScope.authorizeList, { id: selectUser.id }, true)[0];
                if (authorizationUser) return;

                const authorizeUser = {
                    full_name:selectUser.full_name,
                    id: selectUser.id,
                    picture: selectUser.picture,
                    email: selectUser.email
                };
                $rootScope.authorizeList.push(authorizeUser);

                var user = $filter('filter')($rootScope.usersList, { id: selectUser.id }, true)[0];
                if (user) {
                    const index = $rootScope.usersList.indexOf(user);
                    if (index > -1) {
                        SettingService.updateAuthorize({ id: $rootScope.user.id, authorize: user.id }).then(function () {
                            $rootScope.usersList.splice(index, 1);
                            $rootScope.authorizationUserOptions.dataSource.read();
                        }).catch(function () {
                            mdToast.error($filter('translate')('Common.Error'));
                            const authorizeIndex = $rootScope.authorizeList.indexOf(authorizeUser);
                            if (authorizeIndex > -1)
                                $rootScope.authorizeList.splice(authorizeIndex, 1);
                        });
                    }
                }
            }
        };

        $rootScope.fastRecordAddModal = function (moduleName, fastAddRecord, lookupValue, lookupName, id, customScope) {
            $scope.modalCustomScopeRecord = customScope.record;

            if (id) {
                $scope.id = id;
            }
            $scope.type = moduleName;
            $scope.formType = 'modal';
            $scope.fastRecordModal = true;
            $scope.lookupName = lookupName;

            if (fastAddRecord) {
                $scope.recordModal = {};
                $scope.moduleModal = $filter('filter')($rootScope.modules, { name: moduleName }, true)[0];

                if (!$scope.moduleModal) {
                    mdToast.warning($filter('translate')('Common.NotFound'));
                    $state.go('app.dashboard');
                    return;
                }

                $scope.dropdownFields = $filter('filter')($scope.moduleModal.fields, {
                    data_type: 'lookup',
                    show_as_dropdown: true
                }, true);
                $scope.dropdownFieldDatas = {};
                for (var i = 0; i < $scope.dropdownFields.length; i++) {
                    $scope.dropdownFieldDatas[$scope.dropdownFields[i].name] = [];
                }

                $scope.setDropdownData = function (field) {
                    if (field.filters && field.filters.length > 0)
                        $scope.dropdownFieldDatas[field.name] = null;
                    else if ($scope.dropdownFieldDatas[field.name] && $scope.dropdownFieldDatas[field.name].length > 0)
                        return;

                    $scope.currentLookupFieldModal = field;
                    $scope.lookupModal()
                        .then(function (response) {
                            $scope.dropdownFieldDatas[field.name] = response;
                        });

                };

                if (!$scope.hasPermission(moduleName, $scope.operations.modify)) {
                    $scope.forbidden = true;
                    $scope.loadingModal = false;
                    return;
                }

                $scope.primaryFieldModal = $filter('filter')($scope.moduleModal.fields, { primary_lookup: true })[0];

                if (!$scope.primaryFieldModal)
                    $scope.primaryFieldModal = $filter('filter')($scope.moduleModal.fields, { primary: true })[0];

                if (lookupValue) {
                    if ($scope.primaryFieldModal.combination) {
                        var primaryValueParts = lookupValue.split(' ');

                        if (primaryValueParts.length === 1) {
                            $scope.recordModal[$scope.primaryFieldModal.combination.field1] = primaryValueParts[0];
                        } else if (primaryValueParts.length === 2) {
                            $scope.recordModal[$scope.primaryFieldModal.combination.field1] = primaryValueParts[0];
                            $scope.recordModal[$scope.primaryFieldModal.combination.field2] = primaryValueParts[1];
                        } else {
                            $scope.recordModal[$scope.primaryFieldModal.combination.field1] = '';

                            for (var i = 0; i < primaryValueParts.length; i++) {
                                if (i < primaryValueParts.length - 1)
                                    $scope.recordModal[$scope.primaryFieldModal.combination.field1] = $scope.recordModal[$scope.primaryFieldModal.combination.field1] + primaryValueParts[i] + ' ';
                            }

                            $scope.recordModal[$scope.primaryFieldModal.combination.field1] = $scope.recordModal[$scope.primaryFieldModal.combination.field1].slice(0, -1);
                            $scope.recordModal[$scope.primaryFieldModal.combination.field2] = primaryValueParts[primaryValueParts.length - 1];
                        }
                    } else {
                        $scope.recordModal[$scope.primaryFieldModal.name] = lookupValue;
                    }
                }
            }


            var parentEl = angular.element(document.body);
            $mdDialog.show({
                parent: parentEl,
                controller: 'RecordController',
                templateUrl: 'view/app/module/recordDetail.html',
                clickOutsideToClose: true,
                scope: $scope,
                preserveScope: true

            });
        };

        $scope.administrationMenuActive = $scope.administrationMenuActive || false;

        $.extend(true, kendo.ui.Grid.prototype.options.messages, {
            noRecords: $filter('translate')('Common.NoItemDisplay'),
        });
        $.extend(true, kendo.ui.DropDownList.prototype.options.messages, {
            noData: $filter('translate')('Common.NoDataFound')
        });
        $.extend(true, kendo.ui.MultiSelect.prototype.options.messages, {
            noData: $filter('translate')('Common.NoDataFound')
        });
        $.extend(true, kendo.ui.Pager.prototype.options.messages, {
            empty: '',
            itemsPerPage: $filter('translate')('Common.ItemsPerPage'),
            display: $filter('translate')('Common.ItemDisplayCount')
        });

        if (profileConfigs && profileConfigs.start_page) {
            $rootScope.start_page = '#/app/' + profileConfigs.start_page;
        } else {
            $rootScope.start_page = '#/app/dashboard';
        }

        $scope.adminMenuActive = function () {
            if ($scope.administrationMenuActive)
                $scope.administrationMenuActive = false;
            else
                $scope.administrationMenuActive = true;
        };

        $scope.adminMenuItemHideMenu = function (menu) {
            if (menu != undefined && menu[0] != undefined) {
                menu.forEach(function (itm) {
                    itm.active = false;
                });
            }
            $rootScope.closeSide("menuModal");
        };

        //Mobile Hide Menu
        $scope.MenuItemHideMenu = function () {
            $rootScope.closeSide("menuModal");
            $scope.administrationMenuActive = false;
        };

        //Mobile menu
        if (!$rootScope.customMenu) {
            var allModules = $filter('orderBy')($rootScope.modules, 'order');
            if ($scope.user.profile.dashboard)
                $scope.dashboardShow = true;

            if ($scope.dashboardShow)
                $scope.mobileMenus = allModules.slice(0, 3);
            else
                $scope.mobileMenus = allModules.slice(0, 4);
        } else {
            if ($rootScope.menu.length > 0) {
                var allCustomModules = $filter('orderBy')($rootScope.menu, 'order');
                $scope.mobileMenus = allCustomModules.slice(0, 4);
            }
        }

        $scope.closeMobileMenu = function (item, arrayData) {
            $rootScope.closeSide("menuModal");
        };

        $scope.refreshPage = function () {
            $window.location.reload();
        };

        var accessToken = $localStorage.read('access_token');
        $rootScope.beforeSend = function () {
            return function (req) {
                req.setRequestHeader('Authorization', 'Bearer ' + accessToken);
                if (preview)
                    req.setRequestHeader('x-app-id', $rootScope.user.app_id);
                else
                    req.setRequestHeader('X-Tenant-Id', $rootScope.user.tenant_id);
            }
        }
        if (!$scope.lastRunUpdateNotification)
            $scope.lastRunUpdateNotification = $localStorage.read('update_time') ? parseInt($localStorage.read('update_time')) : 0;


        window.newVersion = function () {
            if ($scope.lastRunUpdateNotification <= (Date.now() - 600000)) {
                $mdToast.show({
                    hideDelay: 0,
                    toastClass: 'new-version',
                    controller: 'AppController',
                    position: 'bottom right',
                    template: '<md-toast role="alert" aria-relevant="all">' +
                        '<span class="md-toast-text" flex>New version is available !</span>' +
                        '<md-button class="md-highlight" ng-click="refreshPage();" style="color: white;background: #25A65B;">' +
                        ' Refresh' +
                        '</md-button>' +
                        '<md-button ng-click="closeToast()">' +
                        ' Cancel' +
                        '</md-button>' +
                        '</md-toast>'
                });
                $scope.lastRunUpdateNotification = Date.now();
                $localStorage.write('update_time', $scope.lastRunUpdateNotification)
            }
        }

        $scope.closeToast = function () {
            $mdToast.hide();
        }

        $scope.hasPermission = helper.hasPermission;
        $scope.entityTypes = entityTypes;
        $scope.operations = operations;
        $scope.sidebar = angular.element(document.getElementById('wrapper'));
        $scope.navbar = angular.element(document.getElementById('navbar-wrapper'));
        $scope.bottomlinks = angular.element(document.getElementsByClassName('sidebar-bottom-link'));
        $scope.appLauncher = angular.element(document.getElementById('app-launcher'));
        $scope.appId = $location.search().app || 1;
        $scope.appLogo = $rootScope.workgroup.logo_url ? blobUrl + '/' + $rootScope.workgroup.logo_url : appLogo;
        $scope.addingApp = false;
        $scope.tenants = $rootScope.multiTenant;
        $scope.componentModules = $filter('filter')($rootScope.modules, { system_type: 'component' }, true);

        if ($rootScope.customProfilePermissions) {
            var customProfilePermissionsForLoggedUser = $filter('filter')($rootScope.customProfilePermissions, { profileId: $scope.user.profile.id }, true)[0];
            if (customProfilePermissionsForLoggedUser) {
                var permissions = customProfilePermissionsForLoggedUser.permissions;
                for (var j = 0; j < permissions.length; j++) {
                    switch (permissions[j]) {
                        case 'users':
                        case 'user_groups':
                            $scope.showUsers = true;
                            break;
                        case 'profiles':
                        case 'roles':
                        case 'user_custom_shares':
                            //if he has profile permission, he can available Templates
                            var isProfile = permissions[j] === 'profiles';
                            $scope.showAccessControl = true;
                            break;
                        case 'organization':
                            $scope.showCompanySettings = true;
                            break;
                        case 'import_history':
                        case 'audit_log':
                            $scope.showDataAdministration = true;
                            break;
                        case 'general':
                        case 'sms':
                        case 'email':
                            $scope.showSystemSettings = true;
                            break;
                    }
                }
            }
        }

        $scope.showTemplates = $scope.user.profile.send_email || $scope.user.profile.send_sms || $scope.user.profile.export_data || $scope.user.profile.word_pdf_download || isProfile;
        $scope.showAdministration = $scope.showUsers || $scope.showAccessControl || $scope.showCompanySettings || $scope.showDataAdministration || $scope.showSystemSettings;

        $rootScope.selectIconOptions = {
            dataSource: {
                data: icons2.icons,
                pageSize: 10
            },
            filter: "contains",
            dataTextField: "label",
            dataValueField: "value",
            valueTemplate: '<span class="selected-value icon30" ng-bind-html="dataItem.label"> </span>',
            template: '<span class="k-state-default icon30" ng-bind-html="dataItem.label"> </span>',
        };

        $scope.sideMenuOpen = $rootScope.buildToggler2('menuModal');

        angular.element($window).bind('resize', function () {
            $rootScope.showTooltip = false;
            if (window.innerWidth > 768 && window.innerWidth < 992) {
                $rootScope.showTooltip = true;
            }
            if (window.innerWidth > 768) {
                angular.element('#wrapper').addClass('hide-sidebar');
            }
        });

        $rootScope.isMobile = function () {
            var check = false;
            (function (a) {
                if (/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino/i.test(a) || /1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(a.substr(0, 4)) || $(window).width() < 768) check = true;
            })(navigator.userAgent || navigator.vendor || window.opera);
            return check;
        };

        $scope.addApp = function (appId) {
            if ($scope.addingApp) {
                return;
            }
            $scope.addingApp = true;
            AppService.addApp(appId)
                .then(function (response) {
                    if (response.status === 200) {
                        AppService.getMyAccount()
                            .then(function (response) {
                                $scope.addingApp = false;
                            });
                    }
                });
        };

        $scope.goBack = function () {
            if ($state.current.name === "app.moduleList" || $state.current.name.includes("app.setup")) {
                window.location = $rootScope.start_page;

                if ($rootScope.customMenu && this.isMobile()) {

                    if ($scope.mobileMenus !== undefined && $scope.mobileMenus[0] !== undefined) {
                        for (var i = 0; i < $scope.mobileMenus.length; i++) {
                            $scope.mobileMenus[i].active = false;
                        }
                    }
                    var item = $filter('filter')($scope.mobileMenus, { route: 'dashboard' })[0];
                    item.active = true;
                }
            } else {
                $mdDialog.hide();
                window.history.back();
            }
        };

        $scope.logout = function () {
            AuthService.logoutComplete();

            if ($rootScope.preview)
                window.location = '/logout?preview=' + encodeURIComponent($location.$$absUrl.replace('#' + $location.$$url, '').split('preview=')[1]);
            else
                window.location = '/logout';
        };

        $scope.go = function (link) {
            $window.location.href = link;
        };

        $scope.gotoSetup = function (link) {
            $rootScope.selectedSetupMenuLink = link;
            $window.location.href = link;
            $location.hash('top');
            $anchorScroll();
        };

        $scope.removeSampleData = function () {
            $scope.sampleRemoving = true;

            AppService.removeSampleData()
                .then(function () {
                    $rootScope.workgroup.hasSampleData = false;

                    $scope.sampleRemoving = false;

                    mdToast.success($filter('translate')('Layout.SampleDataRemoveSuccess'));

                    $rootScope.$broadcast('sample-data-removed');
                    $window.location.href = $rootScope.start_page;
                });
        };

        var windowWidth = window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth;

        $scope.isAvailableForSmallDevice = function () {
            return windowWidth < 1024;
        };

        $scope.isAvailableForSmallDevice();

        $scope.routingPrism = function (url, canReload) {
            if (windowWidth < 1024) {
                $scope.toggleLeftMenu();
            }

            var currentUrl = $state.$current.url.source;

            angular.forEach($state.params, function (value, key) {
                var index = currentUrl.indexOf(key);

                if (currentUrl.charAt(index - 1) === ':') {
                    currentUrl = value ? currentUrl.replace(':' + key, value) : currentUrl.replace(':' + key, '');
                } else if (currentUrl.charAt(index - 1) === '?') {
                    currentUrl = value ? currentUrl.replace('?' + key, '?' + key + '=' + value) : currentUrl.replace('?' + key, '');
                } else if (currentUrl.charAt(index - 1) === '&') {
                    currentUrl = value ? currentUrl.replace('&' + key, '&' + key + '=' + value) : currentUrl.replace('&' + key, '');
                }
            });

            if (canReload && url.includes(currentUrl)) {
                $state.reload();
            }
        };

        $scope.openSubMenu = function (item, arrayData) {
            if (item.active && (item.route === "" || item.route === undefined)) {
                item.active = false;
                return false;
            }

            if (arrayData != undefined && arrayData[0] != undefined) {
                for (var i = 0; i < arrayData.length; i++) {
                    arrayData[i].active = false;
                }
            }

            item.active = true;
            $scope.administrationMenuActive = false;

            if (item.route === "" || item.route === undefined) {
                //open sub menu
            } else {
                $rootScope.closeSide("menuModal");
            }
        };

        $scope.toggleFullLeftMenu = function () {
            if (window.innerWidth > 768) {
                angular.element('#wrapper').toggleClass('hide-sidebar');
            }
        };

        $scope.toggleAppMenu = function ($timeout) {
            angular.element($scope.appLauncher).toggleClass('toggled');
        };

        $scope.showAppLauncher = function () {
            return $rootScope.user.has_analytics;
        };

        $scope.getHelpUrl = function () {
            var hash = window.location.hash;

            if (hash.indexOf('?') > -1)
                hash = window.location.hash.split('?')[0];

            var help = $filter('filter')(helps.maps, {
                route: hash,
                language: $rootScope.language,
                appId: $rootScope.user.appId
            }, true)[0];

            if (help) {
                return help.help;
            } else {
                if (hash.indexOf('#/app/modules/') > -1) {
                    help = $filter('filter')(helps.maps, {
                        route: '#/app/modules/',
                        language: $rootScope.language,
                        appId: $rootScope.user.appId
                    }, true)[0];
                }

                if (hash.indexOf('#/app/module/') > -1) {
                    help = $filter('filter')(helps.maps, {
                        route: '#/app/module/',
                        language: $rootScope.language,
                        appId: $rootScope.user.appId
                    }, true)[0];
                }

                if (hash.indexOf('#/app/moduleForm/') > -1) {
                    help = $filter('filter')(helps.maps, {
                        route: '#/app/moduleForm/',
                        language: $rootScope.language,
                        appId: $rootScope.user.appId
                    }, true)[0];
                }

                if (hash.indexOf('#/app/setup/') > -1) {
                    help = $filter('filter')(helps.maps, {
                        route: 'default-setup',
                        language: $rootScope.language,
                        appId: $rootScope.user.appId
                    }, true)[0];
                }

                if (hash.indexOf('#/app/import/') > -1) {
                    help = $filter('filter')(helps.maps, {
                        route: '#/app/import/',
                        language: $rootScope.language,
                        appId: $rootScope.user.appId
                    }, true)[0];
                }

                if (help) {
                    return help.help;
                } else {
                    help = $filter('filter')(helps.maps, {
                        route: 'default',
                        language: $rootScope.language,
                        appId: $rootScope.user.appId
                    }, true)[0];
                    return help.help;
                }
            }
        };

        $scope.dropdownHide = function () {
            angular.element(document.getElementsByClassName('dropdown-menu'))[0].click();
            angular.element(document.getElementsByClassName('dropdown-menu'))[1].click();
        };

        $scope.reload = function () {
            $scope.reloading = true;

            AppService.getMyAccount(true)
                .then(function () {
                    $scope.reloading = false;

                    if ($state.current.name !== 'app.dashboard') {
                        $state.go('app.dashboard');
                        $scope.$parent.$parent.currentPath = '/app/dashboard';
                    } else {
                        $rootScope.$broadcast('sample-data-removed');
                        mdToast.success($filter('translate')('Layout.ReloadSuccess'));
                    }
                });
        };

        $rootScope.openHelp = function (id) {
            HelpService.getById(id).then(function (response) {
                $scope.helpLinkTemplate = response.data;
                $scope.helpTemplateSideModal = $sce.trustAsHtml($scope.helpLinkTemplate.template);

            })
        };

        //Yardım sayfasını ilgili yerde açma
        $scope.helpSide = function (id) {

            if (!id) {
                var hash = window.location.hash;
                var moduleName;
                var isModuleDetail;
                var isModuleList;
                var help = undefined;

                if (hash.indexOf('/app/modules/') > -1) {
                    isModuleList = true;
                    moduleName = $state.params.type || hash.split('/')[3];

                }

                if (hash.indexOf('/app/record/') > -1) {
                    isModuleDetail = true;
                    moduleName = hash.split('/')[3].split('?')[0];
                }

                var module = moduleName ? $filter('filter')($rootScope.modules, { name: moduleName }, true)[0] : undefined;

                if (module) {
                    if (isModuleList) {
                        help = $filter('filter')(module.helps, {
                            modal_type: 'side_modal',
                            module_type: 'module_list'
                        }, true);

                        help = help.sort(function (a, b) { return new Date(b.created_at).getTime() - new Date(a.created_at).getTime() })[0];

                        $scope.helpTemplatesSide = help;
                        if ($scope.helpTemplatesSide && $scope.helpTemplatesSide.show_type === "publish") {
                            $scope.noneHelpTemplate = false;
                            $scope.helpTemplateSideModal = $sce.trustAsHtml($scope.helpTemplatesSide.template);

                        } else {
                            $scope.helpTemplateSideModal = null;
                            $scope.noneHelpTemplate = true;
                        }
                        $rootScope.buildToggler('sideModal', 'view/common/help.html');
                    } else if (isModuleDetail) {

                        var helps = $filter('filter')(module.helps, function (help) {
                            return help.modal_type === 'side_modal' && help.module_type === 'module_detail'
                        }, true);

                        if (helps.length === 1) {
                            $scope.helpTemplatesSide = helps[0];
                        }
                        /**Old customers maybe have added on studio a few help record(s) and maybe that(s) have set  'module_ type' = 'module_form'. We have to check it.
                         * if customer didn't add module_ type = module_detail we have to chek does it have module_ type = module_form.
                         * if That has module_ type = module_form we will set it.
                         * But if old customers have added on studio  module_type = module_detail and  module_ type = module_form we will accept module_detail because we will deprecated  module_form on studio**/
                        else if (helps.length === 2) {
                            help = $filter('filter')(helps, { module_type: 'module_detail' }, true)[0];
                            $scope.helpTemplatesSide = help;
                        }

                        if ($scope.helpTemplatesSide && $scope.helpTemplatesSide.show_type === "publish") {
                            $scope.noneHelpTemplate = false;
                            $scope.helpTemplateSideModal = $sce.trustAsHtml($scope.helpTemplatesSide.template);

                        } else {
                            $scope.helpTemplateSideModal = null;
                            $scope.noneHelpTemplate = true;
                        }
                        $rootScope.buildToggler('sideModal', 'view/common/help.html');
                    }
                } else {
                    $scope.helpTemplatesSide = $rootScope.dashboardHelpSide;
                    if ($scope.helpTemplatesSide && $scope.helpTemplatesSide.show_type === "publish") {
                        $scope.noneHelpTemplate = false;
                        $scope.helpTemplateSideModal = $sce.trustAsHtml($scope.helpTemplatesSide.template);

                    } else {
                        $scope.helpTemplateSideModal = null;
                        $scope.noneHelpTemplate = true;
                    }
                    $rootScope.buildToggler('sideModal', 'view/common/help.html');
                }
            } else {
                HelpService.getById(id)
                    .then(function (response) {
                        $scope.helpTemplatesSide = response.data;

                        if ($scope.helpTemplatesSide && $scope.helpTemplatesSide.show_type === "publish") {
                            $scope.noneHelpTemplate = false;
                            $scope.helpTemplateSideModal = $sce.trustAsHtml($scope.helpTemplatesSide.template);

                        } else {
                            $scope.helpTemplateSideModal = null;
                            $scope.noneHelpTemplate = true;
                        }
                    });
            }

            $scope.modalButtonShow = false;
        };

        //Yardım sayfası içinde butonla ilgili modal açma
        $scope.showHelpModal = function () {

            $scope.dontShowAgain = true;
            var hash = window.location.hash;
            var moduleName;
            var isModuleDetail;
            var isModuleList;

            if (hash.indexOf('/app/modules/') > -1)
                isModuleList = true;
            moduleName = hash.split('/')[3];

            if (hash.indexOf('/app/record/') > -1) {
                isModuleDetail = true;
                moduleName = hash.split('/')[3].split('?')[0];
            }

            if (moduleName) {
                var module = $filter('filter')($rootScope.modules, { name: moduleName }, true)[0];
                if (module != null) {
                    if (isModuleList) {
                        HelpService.getModuleType('modal', 'modulelist', module.id)
                            .then(function (response) {
                                $scope.helpTemplatesSide = response.data;
                                if ($scope.helpTemplatesSide && $scope.helpTemplatesSide.show_type === "publish") {
                                    $rootScope.helpTemplate = $sce.trustAsHtml($scope.helpTemplatesSide.template);
                                }

                            }
                            );
                    } else {
                        HelpService.getModuleType('modal', 'modulelist', module.id)
                            .then(function (response) {
                                $scope.helpTemplatesSide = response.data;
                                if ($scope.helpTemplatesSide && $scope.helpTemplatesSide.show_type === "publish") {
                                    $rootScope.helpTemplate = $sce.trustAsHtml($scope.helpTemplatesSide.template);
                                }
                            }
                            );
                    }
                }
            } else {
                var route = window.location.hash.split('#')[1];
                HelpService.getByType('modal', null, route)
                    .then(function (response) {
                        $scope.helpTemplatesSide = response.data;

                        if ($scope.helpTemplatesSide && $scope.helpTemplatesSide.show_type === "publish") {
                            $rootScope.helpTemplate = $sce.trustAsHtml($scope.helpTemplatesSide.template);
                        }

                    });
            }
        };

        $rootScope.changeTheme = function (color, theme) {
            document.cookie = "theme=" + theme;
            document.cookie = "color=" + color;
            $rootScope.appTheme = theme;
            $("#theme-css").attr("href", "/styles/color-" + theme + ".css");
            GeneralSettingsService.getByKey("custom", "ui_theme", $rootScope.user.id).then(function (response) {
                if (!response.data) {
                    GeneralSettingsService.create({
                        setting_type: "custom",
                        key: "ui_theme",
                        value: color,
                        user_id: $rootScope.user.id
                    });
                } else {
                    response.data.value = color;
                    GeneralSettingsService.update(response.data);
                }
            })
        }

        //#region Notification
        $scope.notificationLoading = true;
        $scope.unReadNotificationCount = 0;
        $scope.signalNotifications = [];
        $rootScope.notificationModalOpen = false;

        // $mdSidenav("sideModal2", true).then(function (instance) {
        //     // On close callback to handle close, backdrop click, or escape key pressed.
        //     // Callback happens BEFORE the close action occurs.
        //     instance.onClose(function () {
        //         $scope.notificationModalOpen = false;
        //     });
        // });


        $scope.getTime = function (time) {
            return kendo.toString(kendo.parseDate(time), "g");
        };

        $scope.notificationShowModal = function () {

            $rootScope.notificationModalOpen = $mdSidenav('sideModal').isOpen();

            if (!$rootScope.notificationModalOpen) {
                $rootScope.closeSide("menuModal");
                $rootScope.notificationModalOpen = true;
                $rootScope.buildToggler('sideModal', 'view/notificationModal.html');
                $scope.notificationLoading = false;
                $timeout(function () {

                    var area = $('.md-sidenav-right').innerHeight() - $('.md-sidenav-right md-toolbar').innerHeight();
                    //$('.notification-box').height();
                    $scope.notificationListViewOptions = {
                        scrollable: "endless",
                        height: area,
                        remove: function (e) {
                            if (e.model) {
                                $scope.notificationRead(e.model, null, true)
                            }
                        }
                    }
                }, 500)
                $rootScope.isViewFilterOpen = true;
                $localStorage.write('is_view_open', $rootScope.isViewFilterOpen);
            } else {
                $scope.closeSide('sideModal');
                $rootScope.notificationModalOpen = false;
                $scope.notificationShowModal();
            }
        };

        $scope.notificationRead = function (notification, id, clear, url) {
            if (notification)
                id = notification.id;

            if (clear) {

                SignalNotificationService.hide(id)
                    .then(function (response) {
                        $scope.unReadNotificationCount = $scope.unReadNotificationCount - 1;
                    });
            } else {
                if (notification.status === "Read")
                    return false;

                SignalNotificationService.read(id)
                    .then(function (response) {
                        if (response) {
                            $scope.unReadNotificationCount = $scope.unReadNotificationCount - 1;
                            if (notification) {
                                notification.status = "Read";

                                if (notification.module && notification.record_id) {
                                    $scope.closeSide('sideModal');
                                    // var newUrl = '#/app/record/' + notification.module.name + '?id=' + notification.record_id;
                                    var newUrl = '#/app/';
                                    if (notification.module.system_type === 'component')
                                        newUrl += notification.module.name + '?id=' + notification.record_id;
                                    else
                                        newUrl += 'record/' + notification.module.name + '?id=' + notification.record_id;

                                    if (newUrl === window.location.hash)
                                        $state.reload();
                                    window.location = newUrl;
                                }
                            } else if (url) {
                                if (url === window.location.hash)
                                    $state.reload();
                                window.location = url;
                            }
                        }
                    });
            }

        };

        function getNotifications() {
            SignalNotificationService.getAll()
                .then(function (response) {
                    if (response.data) {
                        $rootScope.processLanguages(response.data);
                        $scope.signalNotifications = response.data;
                        $scope.source = new kendo.data.DataSource({
                            data: response.data,
                            pageSize: 15
                        });
                        $scope.unReadNotificationCount = $filter('filter')(response.data, { status: 'Unread' }, true).length;
                    }
                });
        }

        // Here we get function call from server-side. If there is a data sent from server-side, we can get it from "data" parameter.
        NotificationService.Event('notification_step', function (data) {

            var mdToastValues = {
                content: data.message,
                actionTxt: null,
                position: 'top right',
                actionKey: 'z',
                timeout: 5000,
                scope: $scope.$new()
            };

            if (data.moduleName && data.recordId > 0) {
                mdToastValues.template = '<md-toast md-theme="toast-::type::" style="cursor:pointer;" ng-click="notificationRead(null,' + data.id + ',false, \'#/app/record/' + data.moduleName + '?id=' + data.recordId + '\')"><div class="md-toast-content" > <span class="md-toast-text">' + data.message + '</span></div></md-toast>'
            }

            switch (data.type) {
                case 'Information':
                    if (mdToastValues.template)
                        mdToastValues.template = mdToastValues.template.replace('::type::', 'info');
                    mdToast.info(mdToastValues);
                    break;
                case 'Success':
                    if (mdToastValues.template)
                        mdToastValues.template = mdToastValues.template.replace('::type::', 'success');
                    mdToast.success(mdToastValues);
                    break;
                case 'Warning':
                    if (mdToastValues.template)
                        mdToastValues.template = mdToastValues.template.replace('::type::', 'warning');
                    mdToast.warning(mdToastValues);
                    break;
                case 'Error':
                    if (mdToastValues.template)
                        mdToastValues.template = mdToastValues.template.replace('::type::', 'error');
                    mdToast.error(mdToastValues);
                    break;
            }

            getNotifications();
        });

        getNotifications();

        //#endregion Notification

        $scope.allRead = function () {
            $scope.unReadNotificationCount = 0;
            SignalNotificationService.allRead().then(function () {
                getNotifications();
            });
        }

        $scope.allHide = function () {

            $scope.unReadNotificationCount = 0;
            $scope.signalNotifications = [];
            SignalNotificationService.allHide();

        }

        $scope.closeSideModal = function () {
            if ($rootScope.isViewFilterOpen && $rootScope.isDocked)
                $rootScope.closeSide('sideModal');
        }


        if ($scope.menu && $rootScope.currentPath !== '/app/dashboard') {
            for (var i = 0; i < $scope.menu.length; i++) {
                if ('/app/modules/' + $scope.menu[i].route === $rootScope.currentPath ||
                    '/app/record/' + $scope.menu[i].route === $rootScope.currentPath ||
                    '/app/' + $scope.menu[i].route === $rootScope.currentPath) {
                    $scope.menu[i].active = true;
                } else {
                    $scope.menu[i].active = false;
                }
                if ($scope.menu[i].menu_items && $scope.menu[i].menu_items.length > 0) {
                    for (var j = 0; j < $scope.menu[i].menu_items.length; j++) {
                        var subMenu = $scope.menu[i].menu_items[j];
                        if ('/app/modules/' + subMenu.route === $rootScope.currentPath ||
                            '/app/record/' + subMenu.route === $rootScope.currentPath ||
                            '/app/' + subMenu.route === $rootScope.currentPath) {
                            $scope.menu[i].active = true;
                            subMenu.active = true;
                        } else {
                            $scope.menu[i].active = false;
                            subMenu.active = false;
                        }
                    }
                }
            }
        }

        $scope.passwordNotMatch = function (password, passwordAgain) {
            if (password !== passwordAgain)
                $scope.showError('compareTo')
        };

        $scope.showError = function (error) {
            switch (error) {
                case 'compareTo':
                    mdToast.error($filter('translate')('Setup.Settings.PasswordNotMatch'));
                    break;
                case 'minlength':
                    mdToast.error($filter('translate')('Setup.Settings.PasswordMinimum'));
                    break;
                case 'wrongPassword':
                    mdToast.error($filter('translate')('Setup.Settings.PasswordWrong'));
                    break;
            }
        };

        $scope.changePassword = function (passwordModel, passwordForm) {

            passwordForm.options.rules['range'] = getRangeRule();
            if (passwordForm.validate()) {
                SettingService.changePassword(passwordModel.current, passwordModel.password, passwordModel.confirm)
                    .then(function () {
                        mdToast.success($filter('translate')('Setup.Settings.PasswordSuccess'));
                    })
                    .catch(function (response) {
                        if (response.status === 400) {
                            mdToast.error($filter('translate')('Setup.Settings.InvalidPassword'))
                        }
                        else
                            mdToast.error($filter('translate')('Common.Error'));
                    });
            } else
                mdToast.error($filter('translate')('Module.RequiredError'));

        };

        function getRangeRule() {
            return function (input) {
                const min = parseInt(input[0]['ariaValueMin'], 10) || 6;
                const max = parseInt(input[0]["ariaValueMax"], 10) || 20;
                const minLength = parseInt(input[0]['minLength'], 10) || 6;
                const maxLength = parseInt(input[0]['maxLength'], 10) || 20;
                const valueAsString = input.val();
                const value = parseInt(valueAsString, 10);

                if (!isNaN(minLength) && !isNaN(maxLength) && valueAsString && minLength > -1 && maxLength > -1) {
                    return minLength <= valueAsString.length && valueAsString.length <= maxLength;
                }

                if (isNaN(min) || isNaN(max) || isNaN(minLength) || isNaN(maxLength) || minLength === -1 || maxLength === -1) {
                    return true;
                }

                if (isNaN(value)) {
                    return true;
                }
                return min <= value && value <= max && minLength <= valueAsString.length && valueAsString.length <= maxLength;
            }
        }

        $scope.checkLength = function (value) {
            if (value && value.length < 6) {
                $scope.showError('minlength');
            }
        };

        $scope.removeAuthorizeUser = function (user) {
            if (!user) return;
            const index = $rootScope.authorizeList.indexOf(user);
            if (index < 0) return;
            $rootScope.authorizeList.splice(index, 1);

            const result = $filter('filter')($rootScope.users, { id: user.id }, true)[0];
            if (result) {
                SettingService.removeAuthorize({ id: $rootScope.user.id, authorize: result.id }).then(function () {
                    $rootScope.usersList.push(result);
                    $rootScope.usersList = $filter('orderBy')($rootScope.usersList, 'full_name');
                    $rootScope.authorizationUserOptions.dataSource.read();
                }).catch(function () {
                    mdToast.error($filter('translate')('Common.Error'));
                    $rootScope.authorizeList.push(user);
                });
            }
        };

        $scope.login = function (authorizedUser) {
            if (!authorizedUser) return;
            SettingService.isAuthorized({ id: authorizedUser.id, authorize: $scope.user.id }).then(function (response) {
                const data = response.data;
                if (data === true) {
                    $cookies.put('x-accessed-user', authorizedUser.email);
                    $window.location.hash = '';
                    $window.location.reload();
                }
            }).catch(function () {
                $cookies.remove('x-accessed-user');
            });
        };

        $scope.generateList = function (event) {
            if (event.target.className !== "fas fa-plus" && !event.delegateTarget.className.contains("collapsed")) return;
            SettingService.getUserAsSimple().then(function (response) {
                if (!response.data) return;
                const result = response.data;
                if (!result) return;
                loop(result);
            });
        };

        function loop(obj) {

            for (var key in obj) {
                if (obj.hasOwnProperty(key) && (key === 'authorized' || key === 'authorize')) {

                    var array = obj[key];

                    //refresh list if anyone remove authorized for logged user
                    if (key === 'authorized' && $rootScope[key + "List"].length > 0) {
                        var addedArray = angular.copy(array);
                        $rootScope[key + "List"] = $filter('filter')($rootScope[key + "List"], function (item) {

                            if (array.indexOf(item.id) < 0) {
                                var index = $rootScope.authorizedList.indexOf(item);
                                if (index > -1) $rootScope.authorizedList.splice(index, 1)
                            }

                            var addedIndex = addedArray.indexOf(item.id);
                            if (addedIndex > -1) {
                                addedArray.splice(addedIndex, 1);
                            }

                            return array.indexOf(item.id) > -1;
                        }, true);

                        array = angular.copy(addedArray);
                    }

                    $rootScope[key + "List"] = $filter('orderBy')($rootScope[key + "List"], 'full_name');
                    if (key === 'authorize' && $rootScope[key + "List"].length > 0) continue;
                    for (var o = 0; o < array.length; o++) {
                        const user = $filter('filter')($rootScope.users, { id: array[o] }, true)[0];
                        if (!user) continue;

                        var result = $filter('filter')($rootScope[key + "List"], { id: user.id }, true)[0];
                        if (!result) {
                            $rootScope[key + "List"].push({
                                full_name: user.full_name,
                                id: user.id,
                                picture: user.picture,
                                email: user.email
                            });
                        }

                        if (key === 'authorize') {
                            //We have to remove this user from list
                            $rootScope.usersList = $filter('filter')($rootScope.usersList, function (userItem) {
                                return userItem.id !== user.id;
                            }, true);
                        }
                    }
                }
            }

        }

        $scope.clear = function () {
            $rootScope.passwordModel = {}
        };

        $scope.getPreviewUser = function () {
            const user = $scope.userExist();
            if (!user) return;
            return $filter('translate')('Auth.Message', { onbehalfof_user: user.full_name });
        };

        $scope.userExist = function () {
            const email = $cookies.get('x-accessed-user');
            if (!email) return false;
            return $filter('filter')($rootScope.users, { email: email }, true)[0];
        };

        $scope.switchBack = function () {
            $cookies.remove('x-accessed-user');
            $window.location.hash = '';
            $window.location.reload();
        };

        $scope.getResult = function () {
            const email = $cookies.get('x-accessed-user');
            return !email;
        };

        $scope.changeLanguage = function (culture) {
            if (!culture) return;
            const language = culture.split('-')[0];
            SettingService.changeLanguage(culture).then(function (response) {
                $localStorage.write('NG_TRANSLATE_LANG_KEY', language);
                $localStorage.write('locale_key', language);
                const currentCoreCulture = $cookies.get('.AspNetCore.Culture') || "c=en-US|uic=en-US";
                if (currentCoreCulture) {
                    const authCulture = currentCoreCulture.split('uic=')[1];
                    if (authCulture && authCulture !== response.data)
                        document.cookie = ".AspNetCore.Culture=" + encodeURIComponent(currentCoreCulture.replaceAll(authCulture, culture));
                } else {
                    document.cookie = ".AspNetCore.Culture=" + encodeURIComponent(currentCoreCulture);
                }
                $window.location.reload();
            }).catch(function () {
                mdToast.error($filter('translate')('Common.Error'));
                $window.location.reload();
            });
        };
    }
]);
