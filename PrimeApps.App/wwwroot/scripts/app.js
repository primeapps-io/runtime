'use strict';
angular.module('primeapps',
    [
        'ngAnimate',
        'ui.router',
        'oc.lazyLoad',
        'ngCookies',
        'pascalprecht.translate',
        'tmh.dynamicLocale',
        'ui.utils',
        'angularFileUpload',
        'blockUI',
        'ngImgCrop',
        'images-resizer',
        'angular-plupload',
        'ui.tinymce',
        'ui.mask',
        'ngSanitize',
        'mentio',
        'angular.filter',
        'kendo.directives',
        'ngMaterial',
    ])

    .config(['$locationProvider', '$compileProvider', '$filterProvider', '$controllerProvider', '$provide', '$httpProvider', '$qProvider', '$sceDelegateProvider', '$translateProvider', 'tmhDynamicLocaleProvider', 'blockUIConfig', '$animateProvider', 'pluploadOptionProvider', 'config', '$mdThemingProvider',
        function ($locationProvider, $compileProvider, $filterProvider, $controllerProvider, $provide, $httpProvider, $qProvider, $sceDelegateProvider, $translateProvider, tmhDynamicLocaleProvider, blockUIConfig, $animateProvider, pluploadOptionProvider, config, $mdThemingProvider) {
            angular.module('primeapps').controller = $controllerProvider.register;
            angular.module('primeapps').service = $provide.service;
            angular.module('primeapps').factory = $provide.factory;
            angular.module('primeapps').directive = window.origin.contains('localhost') ? $compileProvider.directive : $compileProvider.debugInfoEnabled(false);
            angular.module('primeapps').filter = $filterProvider.register;
            angular.module('primeapps').value = $provide.value;
            angular.module('primeapps').constant = $provide.constant;
            angular.module('primeapps').provider = $provide.provider;

            $mdThemingProvider.definePalette('red', { '50': 'fbe6e5', '100': 'f5c1bd', '200': 'ee9891', '300': 'e76e65', '400': 'e14f44', '500': 'dc3023', '600': 'd82b1f', '700': 'd3241a', '800': 'ce1e15', '900': 'c5130c', 'A100': 'fff1f1', 'A200': 'ffbfbe', 'A400': 'ff8e8b', 'A700': 'ff7571', 'contrastDefaultColor': 'light', 'contrastDarkColors': ['50', '100', '200', 'A100', 'A200'], 'contrastLightColors': ['300', '400', '500', '600', '700', '800', '900', 'A400', 'A700'] });
            $mdThemingProvider.definePalette('purple', { '50': 'f1e9f5', '100': 'ddc7e6', '200': 'c7a2d6', '300': 'b07cc6', '400': '9f60b9', '500': '8e44ad', '600': '863ea6', '700': '7b359c', '800': '712d93', '900': '5f1f83', 'A100': 'e7bfff', 'A200': 'd38cff', 'A400': 'bf59ff', 'A700': 'b640ff', 'contrastDefaultColor': 'light', 'contrastDarkColors': ['50', '100', '200', 'A100', 'A200'], 'contrastLightColors': ['300', '400', '500', '600', '700', '800', '900', 'A400', 'A700'] });
            $mdThemingProvider.definePalette('blue', { '50': 'e4e9f1', '100': 'bcc8db', '200': '8fa3c4', '300': '627eac', '400': '41629a', '500': '1f4688', '600': '1b3f80', '700': '173775', '800': '122f6b', '900': '0a2058', 'A100': '8ca7ff', 'A200': '5980ff', 'A400': '2658ff', 'A700': '0d45ff', 'contrastDefaultColor': 'light', 'contrastDarkColors': ['50', '100', '200', 'A100', 'A200'], 'contrastLightColors': ['300', '400', '500', '600', '700', '800', '900', 'A400', 'A700'] });
            $mdThemingProvider.definePalette('light-blue', { '50': 'e3f6ff', '100': 'bae9ff', '200': '8cdaff', '300': '5ecbfe', '400': '3cc0fe', '500': '19b5fe', '600': '16aefe', '700': '12a5fe', '800': '0e9dfe', '900': '088dfd', 'A100': 'ffffff', 'A200': 'f2f9ff', 'A400': 'bfdfff', 'A700': 'a6d3ff', 'contrastDefaultColor': 'light', 'contrastDarkColors': ['50', '100', '200', 'A100', 'A200'], 'contrastLightColors': ['300', '400', '500', '600', '700', '800', '900', 'A400', 'A700'] });
            $mdThemingProvider.definePalette('green', { '50': 'e5f4eb', '100': 'bee4ce', '200': '92d3ad', '300': '66c18c', '400': '46b374', '500': '25a65b', '600': '219e53', '700': '1b9549', '800': '168b40', '900': '0d7b2f', 'A100': 'adffc2', 'A200': '7aff9d', 'A400': '47ff77', 'A700': '2dff64', 'contrastDefaultColor': 'light', 'contrastDarkColors': ['50', '100', '200', 'A100', 'A200'], 'contrastLightColors': ['300', '400', '500', '600', '700', '800', '900', 'A400', 'A700'] });
            $mdThemingProvider.definePalette('orange', { '50': 'fff3e0', '100': 'ffe2b3', '200': 'ffce80', '300': 'ffba4d', '400': 'ffac26', '500': 'ff9d00', '600': 'ff9500', '700': 'ff8b00', '800': 'ff8100', '900': 'ff6f00', 'A100': 'ffffff', 'A200': 'fff7f2', 'A400': 'ffd7bf', 'A700': 'ffc7a6', 'contrastDefaultColor': 'light', 'contrastDarkColors': ['50', '100', '200', 'A100', 'A200'], 'contrastLightColors': ['300', '400', '500', '600', '700', '800', '900', 'A400', 'A700'] });
            $mdThemingProvider.definePalette('gray', { '50': 'eaebed', '100': 'caced1', '200': 'a6aeb3', '300': '828d94', '400': '68747d', '500': '4d5c66', '600': '46545e', '700': '3d4a53', '800': '344149', '900': '253038', 'A100': '82ccff', 'A200': '4fb7ff', 'A400': '1ca2ff', 'A700': '0397ff', 'contrastDefaultColor': 'light', 'contrastDarkColors': ['50', '100', '200', 'A100', 'A200'], 'contrastLightColors': ['300', '400', '500', '600', '700', '800', '900', 'A400', 'A700'] });
            $mdThemingProvider.definePalette('secondarypalet', { '50': 'e0e0e0', '100': 'b3b3b3', '200': '808080', '300': '4d4d4d', '400': '262626', '500': '000000', '600': '000000', '700': '000000', '800': '000000', '900': '000000', 'A100': 'a6a6a6', 'A200': '8c8c8c', 'A400': '737373', 'A700': '666666', 'contrastDefaultColor': 'light', 'contrastDarkColors': ['50', '100', '200', 'A100', 'A200'], 'contrastLightColors': ['300', '400', '500', '600', '700', '800', '900', 'A400', 'A700'] });

            $mdThemingProvider.theme('red').primaryPalette('red').accentPalette('secondarypalet');
            $mdThemingProvider.theme('purple').primaryPalette('purple').accentPalette('secondarypalet');
            $mdThemingProvider.theme('blue').primaryPalette('blue').accentPalette('secondarypalet');
            $mdThemingProvider.theme('light-blue').primaryPalette('light-blue').accentPalette('secondarypalet');
            $mdThemingProvider.theme('green').primaryPalette('green').accentPalette('secondarypalet');
            $mdThemingProvider.theme('orange').primaryPalette('orange').accentPalette('secondarypalet');
            $mdThemingProvider.theme('gray').primaryPalette('gray').accentPalette('secondarypalet');

            $mdThemingProvider.theme('toast-success');
            $mdThemingProvider.theme('toast-error');
            $mdThemingProvider.theme('toast-warning');
            $mdThemingProvider.theme('toast-info');
            $mdThemingProvider.alwaysWatchTheme(true);



            $locationProvider.hashPrefix('');
            var whiteList = [];

            if (blobUrl)
                whiteList.push(blobUrl + '**');

            if (containerDomain)
                whiteList.push(containerDomain + '**');

            angular.forEach(trustedUrls, function (trustedUrl) {
                whiteList.push(trustedUrl.url + '**')
            });

            if (whiteList.length > 0) {
                whiteList.push('self');
                $sceDelegateProvider.resourceUrlWhitelist(whiteList);
            }

            $httpProvider.interceptors.push('genericInterceptor');

            window.localStorage.setItem('NG_TRANSLATE_LANG_KEY', language);
            moment.locale(language);

            $translateProvider.useUrlLoader('/api/localization/get_language_variables?label=' + globalization.Label);

            // Tell the module what language to use by default
            $translateProvider.preferredLanguage(language);

            // Tell the module to store the language in the cookies
            $translateProvider.useCookieStorage();

            var locale = window.localStorage['locale_key'] || language;
            tmhDynamicLocaleProvider.defaultLocale(locale);
            tmhDynamicLocaleProvider.localeLocationPattern('scripts/vendor/locales/angular-locale_{{locale}}.js');


            blockUIConfig.autoBlock = false;
            blockUIConfig.message = '';
            blockUIConfig.templateUrl = 'view/common/blockui.html';

            $animateProvider.classNameFilter(/^(?:(?!ng-animate-disabled).)*$/);

            /// add correct mime type for csv.
            mOxie.Mime.mimes.csv = '.csv';

            /// plupload default upload configuration
            pluploadOptionProvider.setOptions({
                // General settings
                runtimes: 'html5',
                url: 'storage/upload',
                chunk_size: '5mb',
                multipart: true,
                unique_names: true
            });

            //uiSelect config
            // uiSelectConfig.theme = 'bootstrap';
            // uiSelectConfig.resetSearchInput = true;

            //cache-buster for components+functions
            function templateFactoryDecorator($delegate) {
                var fromUrl = angular.bind($delegate, $delegate.fromUrl);

                $delegate.fromUrl = function (url, params) {
                    if (url !== null && angular.isDefined(url)) {
                        if (typeof url == 'function') {
                            url = url.call(url, params);
                        }

                        if (angular.isString(url) && routeTemplateUrls && routeTemplateUrls.length > 0) {
                            for (var i = 0; i < routeTemplateUrls.length; i++) {
                                if (url.indexOf(routeTemplateUrls[i]) > -1) {
                                    url += (url.indexOf('?') < 0 ? '?' : '&');
                                    url += 'v=' + new Date().getTime() / 1000;
                                }
                            }
                        }
                    }

                    return fromUrl(url, params);
                };

                return $delegate;
            }

            $provide.decorator('$templateFactory', ['$delegate', templateFactoryDecorator]);


            //mentio inside div element
            $provide.decorator('mentioMenuDirective', mentionMenuDecorator);
            mentionMenuDecorator.$inject = ['$delegate'];

            function mentionMenuDecorator($delegate) {
                var directive = $delegate[0];
                var link = directive.link;

                directive.compile = function () {
                    return function ($scope, $element) {
                        var modal = $element.closest('.mentiofix');

                        link.apply(this, arguments);

                        if (modal.length) {
                            modal.append($element);
                        }
                    };
                };

                return $delegate;
            }
            //mentio inside div element


        }])

    .run(['$rootScope', '$location', '$state', '$q', '$window', 'AuthService', 'AppService', '$localStorage', '$translate', '$cache', 'helper', '$mdSidenav',
        function ($rootScope, $location, $state, $q, $window, AuthService, AppService, $localStorage, $translate, $cache, helper, $mdSidenav) {
            var isAuthenticated = AuthService.isAuthenticated();

            if (!isAuthenticated) {
                $window.location.href = '/';
                return;
            }

            $rootScope.appTheme = appTheme;
            $rootScope.globalization = globalization;
            $rootScope.globalizations = globalizations;

            var queryString = helper.parseQueryString($window.location.hash.substr(2));
            var lang = queryString.lang;

            $rootScope.preview = preview;
            $rootScope.sideLoad = false;

            AppService.getOperators($rootScope.globalization.Label).then(function (res) {
                if (res && res.data)
                    $rootScope.operators = res.data;
            });

            $rootScope.buildToggler = function (componentId, url, data) {
                $rootScope.sideinclude = true;
                $rootScope.url = url;
                $rootScope.mdSidenavScope = data;

                setTimeout(function () {
                    $rootScope.sideLoad = true;
                }, 250);

                setTimeout(function () {
                    $mdSidenav(componentId).open();
                }, 100);

            };

            $rootScope.buildToggler2 = function (componentId) {
                return function () {
                    $mdSidenav(componentId).toggle();
                    angular.element('#wrapper').removeClass('hide-sidebar');
                };
            };

            $rootScope.closeSide = function (componentId) {
                $mdSidenav(componentId).close();
                $rootScope.sideinclude = false;
                $rootScope.sideLoad = false;
                $rootScope.notificationModalOpen = false;
            };

            $mdSidenav("sideModal", true).then(function (instance) {
                // On close callback to handle close, backdrop click, or escape key pressed.
                // Callback happens BEFORE the close action occurs.
                instance.onClose(function () {
                    $rootScope.sideinclude = false;
                    $rootScope.sideLoad = false;
                    $rootScope.notificationModalOpen = false;
                });
            });

            $rootScope.sideModaldock = function () {
                $rootScope.isDocked = !$rootScope.isDocked;
            };

            if (lang && (lang === 'en' || lang === 'tr')) {
                $localStorage.write('NG_TRANSLATE_LANG_KEY', lang);
                $translate.use(lang);
                $rootScope.language = lang;
            }

            $rootScope.$on('$stateChangeStart', function (event, toState, toParams, fromState, fromParams) {
                try {
                    $rootScope.currentPath = $location.$$url;
                    $rootScope.administrationMenuActive = $rootScope.currentPath.indexOf('/app/setup/') > -1;
                } catch (error) {
                }

            });

        }]);

