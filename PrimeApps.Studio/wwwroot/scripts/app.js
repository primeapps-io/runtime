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

    .config(['$locationProvider', '$compileProvider', '$filterProvider', '$controllerProvider', '$provide', '$httpProvider', '$qProvider', '$sceDelegateProvider', '$translateProvider', 'tmhDynamicLocaleProvider', '$datepickerProvider', 'blockUIConfig', '$animateProvider', 'pluploadOptionProvider', 'config', 'uiSelectConfig',
        function ($locationProvider, $compileProvider, $filterProvider, $controllerProvider, $provide, $httpProvider, $qProvider, $sceDelegateProvider, $translateProvider, tmhDynamicLocaleProvider, $datepickerProvider, blockUIConfig, $animateProvider, pluploadOptionProvider, config, uiSelectConfig) {
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
                window.localStorage.setItem('NG_TRANSLATE_LANG_KEY', 'en');
                language = 'en';
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
                url: 'storage/upload',
                chunk_size: '5mb',
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
            $rootScope.cacheMenuStatus = {
                "homeMenu": 'Open',
                "appMenu": 'Open',
                "status": false

            };
            
            $rootScope.toggleClass = '';
            $rootScope.subtoggleClass = '';

            $rootScope.$on('$stateChangeStart', function (event, toState, toParams, fromState, fromParams) {

                var currentUrl = toState.url;


                if (currentUrl.indexOf("moduleDesigner") > 0 || currentUrl.indexOf("workflowEditor") > 0) {

                    if ($rootScope.subtoggleClass == '') {
                        $rootScope.subtoggleClass = 'full-toggled2';
                        $rootScope.cacheMenuStatus.homeMenu = 'Closed';
                    }

                    if ($rootScope.toggleClass == '') {
                        $rootScope.toggleClass = 'toggled full-toggled';
                        $rootScope.cacheMenuStatus.appMenu = 'closed';
                    }
                    $rootScope.cacheMenuStatus.status = true;


                } else {
                    
                    if ($rootScope.cacheMenuStatus.status == true) {
                        if ($rootScope.cacheMenuStatus.homeMenu != 'Open')
                            $rootScope.subtoggleClass = '';

                        if ($rootScope.cacheMenuStatus.appMenu != 'Open')
                            $rootScope.toggleClass = '';

                        $rootScope.cacheMenuStatus.appMenu = 'Open';
                        $rootScope.cacheMenuStatus.homeMenu = 'Open';
                        $rootScope.cacheMenuStatus.status = false;
                    }

                }

                try {
                    $rootScope.currentPath = $location.$$url;
                    $window.scrollTo(0, 0);

                } catch (error) {
                    return;
                }
            });


        }]);