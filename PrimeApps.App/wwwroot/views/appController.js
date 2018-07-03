'use strict';

angular.module('primeapps').controller('AppController', ['$rootScope', '$scope', '$location', '$state', '$cookies', '$localStorage', '$window', '$filter', '$anchorScroll', 'config', '$popover', 'ngToast', 'entityTypes', 'guidEmpty', 'component', 'convert', 'helper', 'sipHelper', 'operations', 'blockUI', '$cache', 'helps', 'AppService', 'AuthService', '$sessionStorage', 'HelpService', '$sce', '$modal',
    function ($rootScope, $scope, $location, $state, $cookies, $localStorage, $window, $filter, $anchorScroll, config, $popover, ngToast, entityTypes, guidEmpty, component, convert, helper, sipHelper, operations, blockUI, $cache, helps, AppService, AuthService, $sessionStorage, HelpService, $sce, $modal) {
        $scope.hasPermission = helper.hasPermission;
        $scope.entityTypes = entityTypes;
        $scope.operations = operations;
        $scope.sidebar = angular.element(document.getElementById('wrapper'));
        $scope.navbar = angular.element(document.getElementById('navbar-wrapper'));
        $scope.bottomlinks = angular.element(document.getElementsByClassName('sidebar-bottom-link'));
        $scope.appLauncher = angular.element(document.getElementById('app-launcher'));
        $scope.appId = $location.search().app || 1;
        $scope.isCustomDomain = isCustomDomain;
        $scope.addingApp = false;
        $scope.tenants = $rootScope.multiTenant;
        $scope.isTimetrackerExist = false;

        $rootScope.isMobile = function () {
            var check = false;
            (function (a) {
                if (/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino/i.test(a) || /1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(a.substr(0, 4))) check = true;
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

        $scope.isTenantActive = function (tenant) {
            var host = window.location.hostname;

            if (host.indexOf('localhost') < 0) {
                if (host.indexOf('primeapps.io') > -1)
                    return false;
                else if ((host.indexOf('crm.ofisim.com') > -1 || host.indexOf('crm-test.ofisim.com') > -1 || host.indexOf('crm-dev.ofisim.com') > -1) && tenant.AppId === 1)
                    return true;
                else if ((host.indexOf('ik.ofisim.com') > -1 || host.indexOf('ik-test.ofisim.com') > -1 || host.indexOf('ik-dev.ofisim.com') > -1) && tenant.AppId === 4)
                    return true;
                else if ((host.indexOf('hr.ofisim.com') > -1 || host.indexOf('hr-test.ofisim.com') > -1 || host.indexOf('hr-dev.ofisim.com') > -1) && tenant.AppId === 8)
                    return true;
                else
                    return false;
            }
            return false;
        };

        $scope.changeTenant = function (tenant) {
            if ($scope.isTenantActive(tenant))
                return;

            $scope.confirming = false;
            AppService.changeTenant($rootScope.user.id, tenant.TenantId, tenant.AppId, tenant.Email)
                .then(function (response) {
                    if (response.status === 200) {
                        var domain = 'http://localhost:5554';
                        if (host.indexOf('localhost') < 0) {
                            if (host.indexOf('ik.ofisim.com') > -1)
                                domain = 'https://crm.ofisim.com';
                            else if (host.indexOf('ik-test.ofisim.com') > -1)
                                domain = 'https://test.ofisim.com';
                            else if (host.indexOf('ik-dev.ofisim.com') > -1)
                                domain = 'https://dev.ofisim.com';
                            else if (host.indexOf('crm.ofisim.com') > -1)
                                domain = 'https://ik.ofisim.com';
                            else if (host.indexOf('test.ofisim.com') > -1)
                                domain = 'https://ik-test.ofisim.com';
                            else if (host.indexOf('dev.ofisim.com') > -1)
                                domain = 'https://ik-dev.ofisim.com';
                            else
                                domain = 'https://crm.ofisim.com';
                        }
                        $window.location.href = domain;
                    }
                });
        };

        var host = window.location.hostname;

        if (host.indexOf('localhost') < 0) {
            if (host.indexOf('primeapps.io') > -1)
                $scope.appLogo = 'primeapps';
            else if (host.indexOf('kobi.ofisim.com') > -1 || host.indexOf('kobi-test.ofisim.com') > -1)
                $scope.appLogo = 'kobi';
            else if (host.indexOf('asistan.ofisim.com') > -1 || host.indexOf('asistan-test.ofisim.com') > -1)
                $scope.appLogo = 'asistan';
            else if (host.indexOf('ik.ofisim.com') > -1 || host.indexOf('ik-test.ofisim.com') > -1 || host.indexOf('ik-dev.ofisim.com') > -1)
                $scope.appLogo = 'ik';
            else if (host.indexOf('hr.ofisim.com') > -1 || host.indexOf('hr-test.ofisim.com') > -1 || host.indexOf('hr-dev.ofisim.com') > -1)
                $scope.appLogo = 'ik';
            else if (host.indexOf('cagri.ofisim.com') > -1 || host.indexOf('cagri-test.ofisim.com') > -1)
                $scope.appLogo = 'cagri';
            else if (host.indexOf('crm.appsila.com') > -1 || host.indexOf('appsila-test.ofisim.com') > -1)
                $scope.appLogo = 'appsila';
            else if (host.indexOf('crm.livasmart.com') > -1 || host.indexOf('livasmart-test.ofisim.com') > -1)
                $scope.appLogo = 'livasmart';
            else
                $scope.appLogo = 'crm';
        }
        else {
            if ($scope.appId === '1')
                $scope.appLogo = 'crm';
            else if ($scope.appId === '2')
                $scope.appLogo = 'asistan';
            else if ($scope.appId === '3')
                $scope.appLogo = 'ik';
            else if ($scope.appId === '4')
                $scope.appLogo = 'cagri';
            else if ($scope.appId === '5')
                $scope.appLogo = 'kobi';
            else
                $scope.appLogo = 'primeapps';
        }

        $scope.logout = function () {
            blockUI.start();

            AuthService.logout()
                .then(function (response) {
                    $rootScope.app = 'crm';
                    AuthService.logoutComplete();
                    $cookies.remove('tenant_id')
                    //$state.go('auth.login');
                    window.location = response.data['redirect_url'];
                    blockUI.stop();
                });
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

                    $cache.remove('leads_leads');
                    $cache.remove('accounts_accounts');
                    $cache.remove('contacts_contacts');
                    $cache.remove('opportunities_opportunities');
                    $cache.remove('activities_activities');
                    $cache.remove('products_products');
                    $cache.remove('quotes_quotes');
                    $cache.remove('sales_orders_sales_orders');
                    $cache.remove('purchase_orders_purchase_orders');
                    $cache.remove('current_accounts_current_accounts');
                    $cache.remove('suppliers_suppliers');
                    $cache.remove('calendar_events');

                    $scope.sampleRemoving = false;
                    ngToast.create({ content: $filter('translate')('Layout.SampleDataRemoveSuccess'), className: 'success' });
                    $rootScope.$broadcast('sample-data-removed');
                    $window.location.href = '#/app/dashboard';
                });
        };

        var windowWidth = window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth;

        $scope.isAvailableForSmallDevice = function () {
            return windowWidth < 1024;
        };

        $scope.isAvailableForSmallDevice();

        $scope.routingPrism = function (url) {
            if (windowWidth < 1024) {
                $scope.toggleLeftMenu();
            }
        };

        $scope.toggleLeftMenu = function () {
            angular.element($scope.sidebar).toggleClass('toggled');
            angular.element($scope.sidebar).toggleClass('full-toggled');
            angular.element($scope.navbar).toggleClass('toggled');
            angular.element($scope.navbar).toggleClass('full-toggled');
            angular.element($scope.bottomlinks).toggleClass('hidden');

            $scope.isAvailableForSmallDevice();

            $scope.toggled = !$scope.toggled;
        };

        $scope.toggleFullLeftMenu = function () {
            angular.element($scope.sidebar).toggleClass('full-toggled');
            angular.element($scope.sidebar).toggleClass('toggled');
            angular.element($scope.navbar).toggleClass('full-toggled');
            angular.element($scope.navbar).toggleClass('toggled');
            angular.element($scope.bottomlinks).toggleClass('hidden');

            var dropdownMenus = angular.element(document.getElementsByClassName('dropdown-menu'));

            for (var i = 0; i < dropdownMenus.length; i++) {
                angular.element(document.getElementsByClassName('dropdown-menu'))[i].click();
            }
        };

        $scope.toggleAppMenu = function ($timeout) {
            angular.element($scope.appLauncher).toggleClass('toggled');
        };

        $scope.changeApp = function (app) {
            $rootScope.app = app;

            switch (app) {
                case 'crm':
                    $state.go('app.dashboard');
                    break;
                case 'analytics':
                    $state.go('app.analytics.report');
                    break;
                case 'sync':
                    $state.go('app.sync.dashboard');
                    break;
            }

            angular.element($scope.appLauncher).toggleClass('toggled');
        };

        $scope.showAppLauncher = function () {
            return $rootScope.user.has_analytics;
        };

        $rootScope.showSipPhone = function () {
            sipHelper.showSipPhone();
        };

        $rootScope.hideSipPhone = function () {
            $rootScope.sipPhone[$rootScope.app].hide();
        };

        $scope.getHelpUrl = function () {
            var hash = window.location.hash;

            if (hash.indexOf('?') > -1)
                hash = window.location.hash.split('?')[0];

            var help = $filter('filter')(helps.maps, { route: hash, language: $rootScope.language, appId: $rootScope.user.appId }, true)[0];

            if (help) {
                return help.help;
            }
            else {
                if (hash.indexOf('#/app/modules/') > -1) {
                    help = $filter('filter')(helps.maps, { route: '#/app/modules/', language: $rootScope.language, appId: $rootScope.user.appId }, true)[0];
                }

                if (hash.indexOf('#/app/module/') > -1) {
                    help = $filter('filter')(helps.maps, { route: '#/app/module/', language: $rootScope.language, appId: $rootScope.user.appId }, true)[0];
                }

                if (hash.indexOf('#/app/moduleForm/') > -1) {
                    help = $filter('filter')(helps.maps, { route: '#/app/moduleForm/', language: $rootScope.language, appId: $rootScope.user.appId }, true)[0];
                }

                if (hash.indexOf('#/app/setup/') > -1) {
                    help = $filter('filter')(helps.maps, { route: 'default-setup', language: $rootScope.language, appId: $rootScope.user.appId }, true)[0];
                }

                if (hash.indexOf('#/app/import/') > -1) {
                    help = $filter('filter')(helps.maps, { route: '#/app/import/', language: $rootScope.language, appId: $rootScope.user.appId }, true)[0];
                }

                if (help) {
                    return help.help;
                }
                else {
                    help = $filter('filter')(helps.maps, { route: 'default', language: $rootScope.language, appId: $rootScope.user.appId }, true)[0];
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

                    if ($state.current.name != 'app.dashboard')
                        $state.go('app.dashboard');
                    else
                        $rootScope.$broadcast('sample-data-removed');

                    ngToast.create({ content: $filter('translate')('Layout.ReloadSuccess'), className: 'success' });
                });
        };

        //timetracker modülünü gösterme
        if ($filter('filter')($rootScope.modules, { name: 'timetrackers' }, true).length > 0)
            $scope.isTimetrackerExist = true;

        $rootScope.openHelp = function (id) {
            HelpService.getById(id).then(function (response) {
                $scope.helpLinkTemplate = response.data;
                $scope.helpTemplateSideModal = $sce.trustAsHtml($scope.helpLinkTemplate.template);

            })
        };

        //Yardım sayfasını ilgili yerde açma
        $scope.helpSide = function (id) {


            HelpService.getByType('sidemodal', null, null)
                .then(function (response) {
                    $scope.helps = response.data;

                });

            if (!id) {
                var hash = window.location.hash;
                var moduleName;
                var isModuleDetail;
                var isModuleList;

                if (hash.indexOf('/app/modules/') > -1)
                    isModuleList = true;
                moduleName = hash.split('/')[4];

                if (hash.indexOf('/app/module/') > -1) {
                    isModuleDetail = true;
                    moduleName = hash.split('/')[4].split('?')[0];
                }

                if (hash.indexOf('/app/moduleForm/') > -1) {
                    moduleName = hash.split('/')[4].split('?')[0];
                }

                var module = $filter('filter')($rootScope.modules, { name: moduleName }, true)[0];


                if (moduleName) {
                    var module = $filter('filter')($rootScope.modules, { name: moduleName }, true)[0];

                    if (isModuleList) {
                        HelpService.getModuleType('sidemodal', 'modulelist', module.id)
                            .then(function (response) {
                                    $scope.helpTemplatesSide = response.data;
                                    if ($scope.helpTemplatesSide && $scope.helpTemplatesSide.show_type === "publish") {
                                        $scope.noneHelpTemplate = false;
                                        $scope.helpTemplateSideModal = $sce.trustAsHtml($scope.helpTemplatesSide.template);

                                    }
                                    else {
                                        $scope.helpTemplateSideModal = null;
                                        $scope.noneHelpTemplate = true;
                                    }
                                }
                            );
                    }
                    else if (isModuleDetail) {
                        HelpService.getModuleType('sidemodal', 'moduledetail', module.id)
                            .then(function (response) {
                                    $scope.helpTemplatesSide = response.data;
                                    if ($scope.helpTemplatesSide && $scope.helpTemplatesSide.show_type === "publish") {
                                        $scope.noneHelpTemplate = false;
                                        $scope.helpTemplateSideModal = $sce.trustAsHtml($scope.helpTemplatesSide.template);

                                    }
                                    else {
                                        $scope.helpTemplateSideModal = null;
                                        $scope.noneHelpTemplate = true;
                                    }
                                }
                            );
                    }
                    else {
                        HelpService.getModuleType('sidemodal', 'moduleform', module.id)
                            .then(function (response) {
                                    $scope.helpTemplatesSide = response.data;
                                    if ($scope.helpTemplatesSide && $scope.helpTemplatesSide.show_type === "publish") {
                                        $scope.noneHelpTemplate = false;
                                        $scope.helpTemplateSideModal = $sce.trustAsHtml($scope.helpTemplatesSide.template);

                                    }
                                    else {
                                        $scope.helpTemplateSideModal = null;
                                        $scope.noneHelpTemplate = true;
                                    }
                                }
                            );
                    }
                }
                else {
                    var route = window.location.hash.split('#')[1];

                    HelpService.getByType('sidemodal', null, route)
                        .then(function (response) {
                            $scope.helpTemplatesSide = response.data;

                            if ($scope.helpTemplatesSide && $scope.helpTemplatesSide.show_type === "publish") {
                                $scope.noneHelpTemplate = false;
                                $scope.helpTemplateSideModal = $sce.trustAsHtml($scope.helpTemplatesSide.template);

                            }
                            else {
                                $scope.helpTemplateSideModal = null;
                                $scope.noneHelpTemplate = true;
                            }
                        });
                }
            }
            else {
                HelpService.getById(id)
                    .then(function (response) {
                        $scope.helpTemplatesSide = response.data;

                        if ($scope.helpTemplatesSide && $scope.helpTemplatesSide.show_type === "publish") {
                            $scope.noneHelpTemplate = false;
                            $scope.helpTemplateSideModal = $sce.trustAsHtml($scope.helpTemplatesSide.template);

                        }
                        else {
                            $scope.helpTemplateSideModal = null;
                            $scope.noneHelpTemplate = true;
                        }
                    });
            }

            $scope.modalButtonShow = false;
            HelpService.getModuleType('modal', 'modulelist', module.id)
                .then(function (response) {
                    $scope.helpTemplateModalButton = response.data;

                    if ($scope.helpTemplateModalButton && $scope.helpTemplateModalButton.show_type === 'publish') {
                        $scope.modalButtonShow = true;
                    }
                    else {
                        if (route) {
                            HelpService.getByType('modal', null, route)
                                .then(function (response) {
                                    $scope.helpTemplatesSideButton = response.data;
                                    if ($scope.helpTemplatesSideButton && $scope.helpTemplatesSideButton.show_type === 'publish') {
                                        $scope.modalButtonShow = true;
                                    }
                                });
                        }
                    }
                });

        };

        //Yardım sayfası içinde butonla ilgili modal açma
        $scope.showHelpModal = function () {

            $scope.openHelpModal = function () {
                $scope.helpModal = $scope.helpModal || $modal({
                    scope: $scope,
                    templateUrl: 'views/setup/help/helpPageModal.html',
                    animation: 'am-fade',
                    backdrop: true,
                    show: false,
                    tag: 'helpModal',
                    container: 'body'
                });

                $scope.helpModal.$promise.then($scope.helpModal.show);
            };

            $scope.dontShowAgain = true;
            var hash = window.location.hash;
            var moduleName;
            var isModuleDetail;
            var isModuleList;

            if (hash.indexOf('/app/modules/') > -1)
                isModuleList = true;
            moduleName = hash.split('/')[4];

            if (hash.indexOf('/app/module/') > -1) {
                isModuleDetail = true;
                moduleName = hash.split('/')[4].split('?')[0];
            }

            if (hash.indexOf('/app/moduleForm/') > -1) {
                moduleName = hash.split('/')[4].split('?')[0];
            }

            if (moduleName) {
                var module = $filter('filter')($rootScope.modules, { name: moduleName }, true)[0];

                if (isModuleList) {
                    HelpService.getModuleType('modal', 'modulelist', module.id)
                        .then(function (response) {
                                $scope.helpTemplatesSide = response.data;
                                if ($scope.helpTemplatesSide && $scope.helpTemplatesSide.show_type === "publish") {
                                    $rootScope.helpTemplate = $sce.trustAsHtml($scope.helpTemplatesSide.template);
                                    $scope.openHelpModal();
                                }

                            }
                        );
                }
                else {
                    HelpService.getModuleType('modal', 'modulelist', module.id)
                        .then(function (response) {
                                $scope.helpTemplatesSide = response.data;
                                if ($scope.helpTemplatesSide && $scope.helpTemplatesSide.show_type === "publish") {
                                    $rootScope.helpTemplate = $sce.trustAsHtml($scope.helpTemplatesSide.template);
                                    $scope.openHelpModal();
                                }
                            }
                        );
                }
            }
            else {
                var route = window.location.hash.split('#')[1];
                HelpService.getByType('modal', null, route)
                    .then(function (response) {
                        $scope.helpTemplatesSide = response.data;

                        if ($scope.helpTemplatesSide && $scope.helpTemplatesSide.show_type === "publish") {
                            $rootScope.helpTemplate = $sce.trustAsHtml($scope.helpTemplatesSide.template);
                            $scope.openHelpModal();
                        }

                    });
            }
        };

    }

]);