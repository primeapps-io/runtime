'use strict';

angular.module('primeapps',
    [
        'ngAnimate',
        'ui.router',
        'oc.lazyLoad',
        'ngCookies',
        'mgcrea.ngStrap',
        'ui.bootstrap',
        'pascalprecht.translate',
        'tmh.dynamicLocale',
        'angular-ladda',
        'ui.utils',
        'ngTable',
        'xeditable',
        'angularFileUpload',
        'ui.bootstrap.showErrors',
        'ngToast',
        'blockUI',
        'vr.directives.slider',
        'ui.sortable',
        'ngImgCrop',
        'images-resizer',
        'ui.tree',
        'angular-plupload',
        'dragularModule',
        'angucomplete-alt',
        'ngTagsInput',
        'ui.tinymce',
        'ui.mask',
        'ui.ace',
        'ui.select',
        'ngSanitize',
        'angularResizable',
        'ngclipboard',
        'mentio',
        'mwl.calendar',
        'angular.filter',
        'bw.paging',
        'dndLists'
    ])

    .config(['$locationProvider', '$compileProvider', '$filterProvider', '$controllerProvider', '$provide', '$httpProvider', '$qProvider', '$sceDelegateProvider', '$translateProvider', 'tmhDynamicLocaleProvider', '$datepickerProvider', 'ngToastProvider', 'blockUIConfig', '$animateProvider', 'pluploadOptionProvider', 'config', 'uiSelectConfig',
        function ($locationProvider, $compileProvider, $filterProvider, $controllerProvider, $provide, $httpProvider, $qProvider, $sceDelegateProvider, $translateProvider, tmhDynamicLocaleProvider, $datepickerProvider, ngToastProvider, blockUIConfig, $animateProvider, pluploadOptionProvider, config, uiSelectConfig) {
            angular.module('primeapps').controller = $controllerProvider.register;
            angular.module('primeapps').service = $provide.service;
            angular.module('primeapps').factory = $provide.factory;
            angular.module('primeapps').directive = $compileProvider.directive;
            angular.module('primeapps').filter = $filterProvider.register;
            angular.module('primeapps').value = $provide.value;
            angular.module('primeapps').constant = $provide.constant;
            angular.module('primeapps').provider = $provide.provider;
           // $locationProvider.html5Mode(true).hashPrefix('*');

            $locationProvider.hashPrefix('');
            var whiteList = [];

            if (cdnUrl)
                whiteList.push(cdnUrl + '**');

            if (blobUrl)
                whiteList.push(blobUrl + '**');

            if (functionUrl)
                whiteList.push(functionUrl + '**');

            if (whiteList.length > 0) {
                whiteList.push('self');
                $sceDelegateProvider.resourceUrlWhitelist(whiteList);
            }

            $httpProvider.interceptors.push('genericInterceptor');

            var language = window.localStorage.getItem('NG_TRANSLATE_LANG_KEY');

            if (!language) {
                window.localStorage.setItem('NG_TRANSLATE_LANG_KEY', 'tr');
                language = 'tr';
            }
            moment.locale(language);

            $translateProvider.useStaticFilesLoader({
                prefix: cdnUrl + 'locales/',
                suffix: '.json'
            }).useLocalStorage().preferredLanguage('tr').useSanitizeValueStrategy(null);


            var locale = window.localStorage['locale_key'] || language;
            tmhDynamicLocaleProvider.defaultLocale(locale);
            tmhDynamicLocaleProvider.localeLocationPattern('scripts/vendor/locales/angular-locale_{{locale}}.js');

            angular.extend($datepickerProvider.defaults, {
                startWeek: 1
            });

            ngToastProvider.configure({
                verticalPosition: 'top',
                horizontalPosition: 'center',
                className: 'info',
                timeout: 5000,
                dismissButton: true
            });

            blockUIConfig.autoBlock = false;
            blockUIConfig.message = '';
            blockUIConfig.templateUrl = cdnUrl + 'view/common/blockui.html';

            $animateProvider.classNameFilter(/^(?:(?!ng-animate-disabled).)*$/);

            /// add correct mime type for csv.
            mOxie.Mime.mimes.csv = '.csv';

            /// plupload default upload configuration
            pluploadOptionProvider.setOptions({
                // General settings
                runtimes: 'html5',
                url: config.apiUrl + 'Document/Upload',
                chunk_size: '256kb',
                multipart: true,
                unique_names: true
            });

            //uiSelect config
            uiSelectConfig.theme = 'bootstrap';
            uiSelectConfig.resetSearchInput = true;
        }])

    .run(['$rootScope', '$location', '$state', '$q', '$window', 'AuthService', 'LayoutService', 'editableOptions', '$localStorage', '$translate', '$cache', 'helper',
        function ($rootScope, $location, $state, $q, $window, AuthService, LayoutService, editableOptions, $localStorage, $translate, $cache, helper) {
            var pending = false;
            editableOptions.theme = 'bs3';
            $rootScope.theme = $localStorage.read('theme');
            var queryString = helper.parseQueryString($window.location.hash.substr(2));
            var preview = queryString.preview;
            var lang = queryString.lang;
            var isAuthenticated = AuthService.isAuthenticated();

            if (preview) {
                $cache.removeAll();
                $rootScope.preview = true;
            }

            if (lang && (lang === 'en' || lang === 'tr')) {
                $localStorage.write('NG_TRANSLATE_LANG_KEY', lang);
                $translate.use(lang);
                $rootScope.language = lang;
            }

            if (!isAuthenticated) {
                $window.location.href = '/';
                return;
            }


            $rootScope.$on('$stateChangeStart', function (event, toState, toParams, fromState, fromParams) {
                try {
                    $rootScope.currentPath = $location.$$url;

                }
                catch (error) {
                    return;
                }
            });


        }]);