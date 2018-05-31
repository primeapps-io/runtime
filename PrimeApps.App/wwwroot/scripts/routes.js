'use strict';

angular.module('ofisim')

	.config(['$stateProvider', '$urlRouterProvider',
		function ($stateProvider, $urlRouterProvider) {

			if (window.location.hash.indexOf('#access_token') > -1) {
				var parseQueryString = function (queryString) {
					var data = {}, pairs, pair, separatorIndex, escapedKey, escapedValue, key, value;

					if (queryString === null) {
						return data;
					}

					pairs = queryString.split("&");

					for (var i = 0; i < pairs.length; i++) {
						pair = pairs[i];
						separatorIndex = pair.indexOf("=");

						if (separatorIndex === -1) {
							escapedKey = pair;
							escapedValue = null;
						} else {
							escapedKey = pair.substr(0, separatorIndex);
							escapedValue = pair.substr(separatorIndex + 1);
						}

						key = decodeURIComponent(escapedKey);
						value = decodeURIComponent(escapedValue);

						data[key] = value;
					}

					return data;
				};

				var queryString = parseQueryString(window.location.hash.substr(1));
				window.localStorage['access_token'] = queryString.access_token;
			}

			if (!window.localStorage.getItem('access_token'))
				return;

			//app
			$stateProvider
				.state('app', {
					url: '/app',
					abstract: true,
					templateUrl: 'views/app.html',
					controller: 'AppController'
				});

			//app.crm
			$stateProvider
				.state('app.crm', {
					url: '/crm',
					abstract: true,
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/app/crm.html',
							controller: 'CrmController'
						}
					},
					resolve: {
						AppService: 'AppService',
						start: ['$rootScope', 'AppService',
							function ($rootScope, AppService) {
								if (!$rootScope.user)
									return AppService.getMyAccount();
							}]
					}
				})

				.state('app.crm.home', {
					url: '/home',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/app/home/home.html',
							controller: 'HomeController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/app/home/homeController.js',
								cdnUrl + 'views/app/directory/directoryDirective.js'
							]);
						}]
					}
				})

				.state('app.crm.dashboard', {
					url: '/dashboard',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/app/dashboard/dashboard.html',
							controller: 'DashboardController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								'scripts/vendor/angular-fusioncharts.js',
								cdnUrl + 'views/app/dashboard/dashboardService.js',
								cdnUrl + 'views/app/dashboard/dashboardController.js'
							]);
						}]
					}
				})

				.state('app.crm.moduleList', {
					url: '/modules/:type?viewid',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/app/module/moduleList.html',
							controller: 'ModuleListController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/app/module/moduleListController.js',
								cdnUrl + 'views/app/module/moduleFormController.js',
								cdnUrl + 'views/app/email/bulkEMailController.js',
								cdnUrl + 'views/app/sms/bulkSMSController.js',
								cdnUrl + 'views/app/email/templateService.js',
								cdnUrl + 'views/app/leave/collectiveLeaveController.js'
							]);
						}]
					}
				})

				.state('app.crm.moduleDetail', {
					url: '/module/:type?id?ptype?pid?rptype?rtab?pptype?ppid?prtab?rpptype?rppid?rprtab?back?freeze',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/app/module/moduleDetail.html',
							controller: 'ModuleDetailController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/app/module/moduleDetailController.js',
								cdnUrl + 'views/app/module/moduleFormModalController.js',
								cdnUrl + 'views/app/email/bulkEMailController.js',
								cdnUrl + 'views/app/product/quoteProductsController.js',
								cdnUrl + 'views/app/product/quoteProductsService.js',
								cdnUrl + 'views/app/product/orderProductsController.js',
								cdnUrl + 'views/app/product/orderProductsService.js',
								cdnUrl + 'views/app/product/purchaseProductsController.js',
								cdnUrl + 'views/app/product/purchaseProductsService.js',
								cdnUrl + 'views/app/module/moduleAddModalController.js',
								cdnUrl + 'views/app/email/singleEmailController.js',
								cdnUrl + 'views/app/sms/singleSMSController.js',
								cdnUrl + 'views/app/actionbutton/actionButtonFrameController.js',
								cdnUrl + 'views/app/location/locationFormModalController.js',
								cdnUrl + 'views/app/email/templateService.js'
							]);
						}]
					}
				})

				.state('app.crm.moduleForm', {
					url: '/moduleForm/:type?stype?id?ptype?pid?rtab?pptype?ppid?prtab?back?clone?revise?many?field?value',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/app/module/moduleForm.html',
							controller: 'ModuleFormController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/app/module/moduleFormController.js',
								cdnUrl + 'views/app/module/moduleFormModalController.js',
								cdnUrl + 'views/app/product/quoteProductsController.js',
								cdnUrl + 'views/app/product/quoteProductsService.js',
								cdnUrl + 'views/app/product/orderProductsController.js',
								cdnUrl + 'views/app/product/orderProductsService.js',
								cdnUrl + 'views/app/product/purchaseProductsController.js',
								cdnUrl + 'views/app/product/purchaseProductsService.js',
								cdnUrl + 'views/app/actionbutton/actionButtonFrameController.js',
								cdnUrl + 'views/app/location/locationFormModalController.js',
								{
									type: 'js',
									path: 'https://maps.googleapis.com/maps/api/js?key=AIzaSyDxai8Lo5_z03O9am5WyP5XvYtITzC_l-o&libraries=places'
								}
							]);
						}]
					}
				})

				.state('app.crm.viewForm', {
					url: '/viewForm/:type?id',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/app/view/viewForm.html',
							controller: 'ViewFormController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/app/view/viewFormController.js',
								cdnUrl + 'views/app/view/viewService.js'
							]);
						}]
					}
				})

				.state('app.crm.tasks', {
					url: '/tasks',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/app/tasks/tasks.html',
							controller: 'TaskController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/app/tasks/taskController.js',
								cdnUrl + 'views/app/tasks/taskService.js',
								cdnUrl + 'views/app/tasks/taskDirective.js'
							]);
						}]
					}
				})

				.state('app.crm.documents', {
					url: '/documents/:type?id',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/app/documents/documents.html',
							controller: 'DocumentController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/app/documents/documentController.js'
							]);
						}]
					}

				})

				.state('app.crm.calendar', {
					url: '/calendar',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/app/calendar/calendar.html',
							controller: 'CalendarController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/app/calendar/calendarController.js',
								cdnUrl + 'views/app/module/moduleFormModalController.js'
							]);
						}]
					}
				})

				.state('app.crm.documentSearch', {
					url: '/documentSearch',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/app/documents/advDocumentSearch.html',
							controller: 'AdvDocumentSearchController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/app/documents/advDocumentSearchController.js'
							]);
						}]
					}

				})

				.state('app.crm.timesheet', {
					url: '/timesheet?user?project?month?ctype',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/app/timesheet/timesheet.html',
							controller: 'TimesheetController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/app/timesheet/timesheetController.js',
								cdnUrl + 'views/app/timesheet/timesheetModalController.js',
								cdnUrl + 'views/app/timesheet/timesheetFrameController.js'
							]);
						}]
					}

				})

				.state('app.crm.newsfeed', {
					url: '/newsfeed',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/app/newsfeed/newsfeed.html',
							controller: 'NewsfeedController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/app/newsfeed/newsfeedController.js',
								cdnUrl + 'views/app/module/moduleFormModalController.js'
							]);
						}]
					}
				})

				.state('app.crm.import', {
					url: '/import/:type',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/app/data/import.html',
							controller: 'ImportController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								'scripts/vendor/xlsx.core.min.js',
								cdnUrl + 'views/app/data/importController.js',
								cdnUrl + 'views/app/data/importService.js'
							]);
						}]
					}

				})

				.state('app.crm.importCsv', {
					url: '/importcsv/:type',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/app/data/csv/import.html',
							controller: 'ImportController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/app/data/csv/importController.js',
								cdnUrl + 'views/app/data/csv/importService.js'
							]);
						}]
					}

				})

				.state('app.crm.leadconvert', {
					url: '/leadconvert?id',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/app/convert/leadConvert.html',
							controller: 'LeadConvertController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/app/convert/leadConvertController.js',
								cdnUrl + 'views/app/convert/leadConvertService.js'
							]);
						}]
					}

				})

				.state('app.crm.personalconvert', {
					url: '/personalconvert?id',
					views: {
						'app': {
							templateUrl: 'views/app/convert/personalConvert.html',
							controller: 'PersonalConvertController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								'views/app/convert/personalConvertController.js' + '?v=' + version,
								'views/app/convert/personalConvertService.js' + '?v=' + version
							]);
						}]
					}

				})

				.state('app.crm.quoteconvert', {
					url: '/quoteconvert?id',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/app/convert/quoteConvert.html',
							controller: 'QuoteConvertController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/app/convert/quoteConvertController.js',
								cdnUrl + 'views/app/convert/quoteConvertService.js'
							]);
						}]
					}
				})

				.state('app.crm.analytics', {
					url: '/analytics?id',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/app/analytics/analytics.html',
							controller: 'AnalyticsController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/app/analytics/analyticsService.js',
								cdnUrl + 'views/app/analytics/analyticsController.js'
							]);
						}]
					}
				})

				.state('app.crm.analyticsForm', {
					url: '/analyticsForm?id',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/app/analytics/analyticsForm.html',
							controller: 'AnalyticsFormController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/app/analytics/analyticsService.js',
								cdnUrl + 'views/app/analytics/analyticsFormController.js'
							]);
						}]
					}
				})

				.state('app.crm.reports', {
					url: '/reports?categoryId?id',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/app/reports/reportCategory.html',
							controller: 'ReportsController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								'scripts/vendor/angular-fusioncharts.js',
								cdnUrl + 'views/app/reports/reportsService.js',
								cdnUrl + 'views/app/reports/reportCategoryController.js'
							]);
						}]
					}
				})

				.state('app.crm.timetracker', {
					url: '/timetracker?user?year?month?week',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/app/timesheet/timetracker.html',
							controller: 'TimetrackerController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/app/timesheet/timetrackerController.js',
								cdnUrl + 'views/app/timesheet/timetrackerModalController.js',
								cdnUrl + 'views/app/timesheet/timetrackerService.js'
							]);
						}]
					}
				})

				.state('app.crm.report', {
					url: '/report',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/app/reports/createReport.html',
							controller: 'CreateReportController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/app/reports/reportsService.js',
								cdnUrl + 'views/app/reports/createReportController.js'
							]);
						}]
					}
				})

				.state('app.crm.directory', {
					url: '/directory?id',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/app/directory/directoryDetail.html',
							controller: 'DirectoryDetailController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/app/directory/directoryDetailController.js',
								cdnUrl + 'views/app/directory/directoryDirective.js'
							]);
						}]
					}
				});

			//app.setup
			$stateProvider
				.state('app.setup', {
					url: '/setup',
					abstract: true,
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/setup.html',
							controller: 'SetupController'
						}
					},
					resolve: {
						start: ['$rootScope', '$q', '$state', 'AppService',
							function ($rootScope, $q, $state, AppService) {
								var deferred = $q.defer();

								if ($rootScope.preview) {
									$state.go('app.crm.dashboard');
									deferred.resolve();
									return deferred.promise;
								}

								if (!$rootScope.user) {
									AppService.getMyAccount()
										.then(function () {
											deferred.resolve();
										});
								}
								else {
									deferred.resolve();
								}

								return deferred.promise;
							}]
					}
				})

				.state('app.setup.settings', {
					url: '/settings',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/settings/settings.html',
							controller: 'SettingController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/settings/settingController.js',
								cdnUrl + 'views/setup/settings/settingService.js'
							]);
						}]
					}
				})

				.state('app.setup.general', {
					url: '/general',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/general/generalSettings.html',
							controller: 'GeneralSettingsController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/general/generalSettingsController.js',
								cdnUrl + 'views/setup/general/generalSettingsService.js'
							]);
						}]
					}
				})

				.state('app.setup.organization', {
					url: '/organization',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/organization/organization.html',
							controller: 'OrganizationController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/organization/organizationController.js',
								cdnUrl + 'views/setup/organization/organizationService.js'
							]);
						}]
					}
				})

				.state('app.setup.notifications', {
					url: '/notifications',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/notifications/notifications.html',
							controller: 'NotificationController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/notifications/notificationController.js',
								cdnUrl + 'views/setup/notifications/notificationService.js'
							]);
						}]
					}
				})

				.state('app.setup.users', {
					url: '/users',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/users/users.html',
							controller: 'UserController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/users/userController.js',
								cdnUrl + 'views/setup/users/userService.js',
								cdnUrl + 'views/setup/workgroups/workgroupService.js',
								cdnUrl + 'views/setup/profiles/profileService.js',
								cdnUrl + 'views/setup/roles/roleService.js',
								cdnUrl + 'views/setup/usercustomshares/userCustomShareService.js',
								cdnUrl + 'views/setup/license/licenseService.js'
							]);
						}]
					}
				})

				.state('app.setup.profiles', {
					url: '/profiles',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/profiles/profiles.html',
							controller: 'ProfileController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/profiles/profileController.js',
								cdnUrl + 'views/setup/profiles/profileService.js'
							]);
						}]
					}
				})

				.state('app.setup.profile', {
					url: '/profile?id&clone',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/profiles/profileForm.html',
							controller: 'ProfileFormController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/profiles/profileFormController.js',
								cdnUrl + 'views/setup/profiles/profileService.js'
							]);
						}]
					}
				})

				.state('app.setup.roles', {
					url: '/roles',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/roles/roles.html',
							controller: 'RoleController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/roles/roleController.js',
								cdnUrl + 'views/setup/roles/roleService.js'
							]);
						}]
					}
				})

				.state('app.setup.role', {
					url: '/role?id&reportsTo',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/roles/roleForm.html',
							controller: 'RoleFormController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/roles/roleFormController.js',
								cdnUrl + 'views/setup/roles/roleService.js'
							]);
						}]
					}
				})

				.state('app.setup.modules', {
					url: '/modules',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/modules/modulesSetup.html',
							controller: 'ModuleSetupController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/modules/moduleSetupController.js',
								cdnUrl + 'views/setup/modules/moduleSetupService.js'
							]);
						}]
					}
				})

				.state('app.setup.module', {
					url: '/module?id&clone&redirect',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/modules/moduleSetupForm.html',
							controller: 'ModuleFormSetupController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/modules/moduleSetupFormController.js',
								cdnUrl + 'views/setup/modules/moduleSetupLayoutController.js',
								cdnUrl + 'views/setup/modules/moduleSetupService.js',
								cdnUrl + 'views/app/location/locationFormModalController.js'
							]);
						}]
					}
				})

				.state('app.setup.modulerelations', {
					url: '/module/relations/:module',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/modules/moduleRelations.html',
							controller: 'ModuleRelationController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/modules/moduleRelationController.js',
								cdnUrl + 'views/setup/modules/moduleSetupService.js'
							]);
						}]
					}
				})

				.state('app.setup.moduledependencies', {
					url: '/module/dependencies/:module',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/modules/moduleDependencies.html',
							controller: 'ModuleDependencyController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/modules/moduleDependencyController.js',
								cdnUrl + 'views/setup/modules/moduleSetupService.js'
							]);
						}]
					}
				})

				.state('app.setup.actionButtons', {
					url: '/module/actionButtons/:module',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/modules/actionButtons.html',
							controller: 'ActionButtonsController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/modules/actionButtonsController.js',
								cdnUrl + 'views/setup/modules/moduleSetupService.js'
							]);
						}]
					}
				})

				.state('app.setup.moduleProfileSettings', {
					url: '/module/moduleProfileSettings/:module',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/modules/moduleProfileSettings.html',
							controller: 'ModuleProfileSettingController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/modules/moduleProfileSettingsController.js',
								cdnUrl + 'views/setup/modules/moduleSetupService.js'
							]);
						}]
					}
				})

				.state('app.setup.leadconvertmap', {
					url: '/leadconvertmap',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/convert/leadConvertMap.html',
							controller: 'LeadConvertMapController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/convert/leadConvertMapController.js',
								cdnUrl + 'views/setup/convert/convertMapService.js'
							]);
						}]
					}
				})

				.state('app.setup.candidateconvertmap', {
					url: '/candidateconvertmap',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/convert/candidateConvertMap.html',
							controller: 'CandidateConvertMapController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/convert/candidateConvertMapController.js',
								cdnUrl + 'views/setup/convert/convertMapService.js'
							]);
						}]
					}
				})
				.state('app.setup.quoteconvertmap', {
					url: '/quoteconvertmap',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/convert/quoteConvertMap.html',
							controller: 'quoteConvertMapController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/convert/quoteConvertMapController.js',
								cdnUrl + 'views/setup/convert/convertMapService.js'
							]);
						}]
					}
				})

				.state('app.setup.import', {
					url: '/importhistory',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/importhistory/importHistory.html',
							controller: 'ImportHistoryController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/importhistory/importHistoryController.js',
								cdnUrl + 'views/setup/importhistory/importHistoryService.js'
							]);
						}]
					}
				})

				.state('app.setup.messaging', {
					url: '/messaging',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/messaging/messaging.html',
							controller: 'MessagingController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/messaging/messagingController.js',
								cdnUrl + 'views/setup/messaging/messagingService.js'
							]);
						}]
					}
				})

				.state('app.setup.office', {
					url: '/office',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/office/office.html',
							controller: 'OfficeController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/office/officeController.js',
								cdnUrl + 'views/setup/office/officeService.js'
							]);
						}]
					}
				})

				.state('app.setup.phone', {
					url: '/phone',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/phone/phone.html',
							controller: 'PhoneSettingsController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/phone/phoneSettingsController.js',
								cdnUrl + 'views/setup/phone/phoneSettingsService.js'
							]);
						}]
					}
				})

				.state('app.setup.auditlog', {
					url: '/auditlog',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/auditlog/auditlogs.html',
							controller: 'AuditLogController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/auditlog/auditLogController.js',
								cdnUrl + 'views/setup/auditlog/auditLogService.js'
							]);
						}]
					}
				})

				.state('app.setup.templates', {
					url: '/templates',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/templates/templates.html',
							controller: 'TemplateController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/templates/templateService.js',
								cdnUrl + 'views/setup/templates/templateController.js'
							]);
						}]
					}
				})

				.state('app.setup.template', {
					url: '/template?id',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/templates/templateForm.html',
							controller: 'TemplateFormController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/templates/templateService.js',
								cdnUrl + 'views/setup/templates/templateFormController.js'
							]);
						}]
					}
				})

				.state('app.setup.templateguide', {
					url: '/templateguide',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/templates/templateGuide.html',
							controller: 'TemplateGuideController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/templates/templateGuideController.js'
							]);
						}]
					}
				})

				.state('app.setup.workflows', {
					url: '/workflows',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/workflow/workflows.html',
							controller: 'WorkflowController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/workflow/workflowController.js',
								cdnUrl + 'views/setup/workflow/workflowService.js'
							]);
						}]
					}
				})

				.state('app.setup.workflow', {
					url: '/workflow?id',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/workflow/workflowForm.html',
							controller: 'WorkflowFormController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/workflow/workflowFormController.js',
								cdnUrl + 'views/setup/workflow/workflowService.js'
							]);
						}]
					}
				})

				.state('app.setup.approvel_process', {
					url: '/approvel_process',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/approvel_process/approvelProcesses.html',
							controller: 'ApprovelProcessController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/approvel_process/approvelProcessController.js',
								cdnUrl + 'views/setup/approvel_process/approvelProcessService.js'
							]);
						}]
					}
				})

				.state('app.setup.help', {
					url: '/help',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/help/helpPage.html',
							controller: 'HelpController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/help/helpController.js',

							]);
						}]
					}
				})

				.state('app.setup.helpside', {
					url: '/helpside',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/help/helpPageSide.html',
							controller: 'HelpController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/help/helpController.js',

							]);
						}]
					}
				})

				.state('app.setup.helpsides', {
					url: '/helpsides',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/help/helpPageSides.html',
							controller: 'HelpController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/help/HelpController.js',

							]);
						}]
					}
				})


				.state('app.setup.approvel', {
					url: '/approvel?id',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/approvel_process/approvelProcessForm.html',
							controller: 'ApprovelProcessFormController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/approvel_process/approvelProcessFormController.js',
								cdnUrl + 'views/setup/approvel_process/approvelProcessService.js'
							]);
						}]
					}
				})

				.state('app.setup.warehouse', {
					url: '/warehouse',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/warehouse/warehouse.html',
							controller: 'WarehouseController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/app/analytics/analyticsService.js',
								cdnUrl + 'views/setup/warehouse/warehouseController.js'
							]);
						}]
					}
				})

				.state('app.setup.usergroups', {
					url: '/usergroups',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/usergroups/userGroups.html',
							controller: 'UserGroupController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/usergroups/userGroupController.js',
								cdnUrl + 'views/setup/usergroups/userGroupService.js'
							]);
						}]
					}
				})

				.state('app.setup.usercustomshares', {
					url: '/usercustomshares',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/usercustomshares/userCustomShares.html',
							controller: 'UserCustomShareController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/usercustomshares/userCustomShareController.js',
								cdnUrl + 'views/setup/usercustomshares/userCustomShareService.js'
							]);
						}]
					}
				})

				.state('app.setup.usercustomshare', {
					url: '/usercustomshare?id&clone',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/usercustomshares/userCustomShareForm.html',
							controller: 'UserCustomShareFormController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/usercustomshares/userCustomShareFormController.js',
								cdnUrl + 'views/setup/usercustomshares/userCustomShareService.js'
							]);
						}]
					}
				})

				.state('app.setup.outlook', {
					url: '/outlook',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/outlook/outlook.html',
							controller: 'OutlookController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/outlook/outlookController.js',
								cdnUrl + 'views/setup/outlook/outlookService.js'

							]);
						}]
					}
				})

				.state('app.setup.usergroup', {
					url: '/usergroup?id&clone',
					views: {
						'app': {
							templateUrl: cdnUrl + 'views/setup/usergroups/userGroupForm.html',
							controller: 'UserGroupFormController'
						}
					},
					resolve: {
						plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
							return $ocLazyLoad.load([
								cdnUrl + 'views/setup/usergroups/userGroupFormController.js',
								cdnUrl + 'views/setup/usergroups/userGroupService.js'
							]);
						}]
					}
				});

			//other
			$stateProvider
				.state('paymentform', {
					url: '/paymentform',
					templateUrl: cdnUrl + 'views/app/payment/paymentForm.html',
					controller: 'PaymentFormController'
				})

				.state('join', {
					url: '/join',
					templateUrl: cdnUrl + 'views/app/join/join.html',
					controller: 'JoinController'
				});

			$urlRouterProvider.otherwise('/app/crm/dashboard');
		}]);