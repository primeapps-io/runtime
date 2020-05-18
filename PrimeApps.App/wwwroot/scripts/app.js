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
		'angular.filter'
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

			$locationProvider.hashPrefix('');
			var whiteList = [];

			if (cdnUrl)
				whiteList.push(cdnUrl + '**');

			if (blobUrl)
				whiteList.push(blobUrl + '**');

			angular.forEach(trustedUrls, function (trustedUrl) {
				whiteList.push(trustedUrl.url + '**')
			});

			if (whiteList.length > 0) {
				whiteList.push('self');
				$sceDelegateProvider.resourceUrlWhitelist(whiteList);
			}

			$httpProvider.interceptors.push('genericInterceptor');

			//var language = window.localStorage.getItem('NG_TRANSLATE_LANG_KEY');
			var language = tenantLanguage;

			if (!language && customLanguage)
				language = customLanguage;
			else if (!language && !customLanguage) {
				//window.navigator.userLanguage working for only IE 10.
				var browserLang = window.navigator.language || window.navigator.userLanguage;
				language = browserLang === 'tr' || browserLang === 'tr-TR' ? 'tr' : 'en';

			}

			window.localStorage.setItem('NG_TRANSLATE_LANG_KEY', language);
			moment.locale(language);

			$translateProvider.useStaticFilesLoader({
				prefix: cdnUrl + 'locales/',
				suffix: '.json'
			}).useLocalStorage().preferredLanguage(language).useSanitizeValueStrategy(null);

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
				url: 'storage/upload',
				chunk_size: '5mb',
				multipart: true,
				unique_names: true
			});

			//uiSelect config
			uiSelectConfig.theme = 'bootstrap';
			uiSelectConfig.resetSearchInput = true;

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
		}])

	.run(['$rootScope', '$location', '$state', '$q', '$window', 'AuthService', 'AppService', 'editableOptions', '$localStorage', '$translate', '$cache', 'helper',
		function ($rootScope, $location, $state, $q, $window, AuthService, AppService, editableOptions, $localStorage, $translate, $cache, helper) {
			var pending = false;
			editableOptions.theme = 'bs3';
			$rootScope.theme = $localStorage.read('theme');
			var queryString = helper.parseQueryString($window.location.hash.substr(2));
			var lang = queryString.lang;
			var isAuthenticated = AuthService.isAuthenticated();

			$rootScope.preview = preview;

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
					//$window.yaCounter47616517.hit($location.path());
				}
				catch (error) {
					return;
				}
			});

			$rootScope.app = 'crm';

		}]);