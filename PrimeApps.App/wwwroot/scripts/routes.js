'use strict';
var sectionComponents = {};
var currentSectionComponentsTemplate = [];
angular.module('primeapps')

    .config(['$stateProvider', '$urlRouterProvider', '$sceDelegateProvider',
        function ($stateProvider, $urlRouterProvider, $sceDelegateProvider) {

            // $sceDelegateProvider.resourceUrlWhitelist([
            //     // Allow same origin resource loads.
            //     'self',
            //     // Allow loading from our assets domain. **.
            //     'http://gitea-dev.primeapps.io:3000/**'
            // ]);

            /*if (window.location.hash.indexOf('#access_token') > -1) {
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
             }*/

            if (token) {
                window.localStorage['access_token'] = token;
            }

            if (!window.localStorage.getItem('access_token')) {
                return;
            }

            //app
            $stateProvider
                .state('app', {
                    url: '/app',
                    abstract: true,
                    templateUrl: 'view/app.html',
                    controller: 'AppController',
                    resolve: {
                        AppService: 'AppService',
                        start: ['$rootScope', 'AppService',
                            function ($rootScope, AppService) {
                                if (!$rootScope.user)
                                    return AppService.getMyAccount();
                            }]
                    }
                });

            //app.crm
            $stateProvider
                .state('app.home', {
                    url: '/home',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/home/home.html',
                            controller: 'HomeController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/home/homeController.js',
                                cdnUrl + 'view/app/directory/directoryDirective.js'
                            ]);
                        }]
                    }
                })

                .state('app.dashboard', {
                    url: '/dashboard',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/dashboard/dashboard.html',
                            controller: 'DashboardController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                'scripts/vendor/angular-fusioncharts.js',
                                cdnUrl + 'view/app/dashboard/dashboardService.js',
                                cdnUrl + 'view/app/dashboard/dashboardController.js'
                            ]);
                        }]
                    }
                })

                .state('app.moduleList', {
                    url: '/modules/:type?viewid',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/module/moduleList.html',
                            controller: 'ModuleListController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/module/moduleListController.js',
                                cdnUrl + 'view/app/module/moduleFormController.js',
                                cdnUrl + 'view/app/email/bulkEMailController.js',
                                cdnUrl + 'view/app/sms/bulkSMSController.js',
                                cdnUrl + 'view/app/actionbutton/actionButtonFrameController.js',
                                cdnUrl + 'view/app/email/templateService.js',
                                cdnUrl + 'view/app/leave/collectiveLeaveController.js',
                                cdnUrl + 'view/app/module/exportDataController.js'
                            ]);
                        }]
                    }
                })

                .state('app.moduleDetail', {
                    url: '/module/:type?id?ptype?pid?rptype?rtab?pptype?ppid?prtab?rpptype?rppid?rprtab?back?freeze',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/module/moduleDetail.html',
                            controller: 'ModuleDetailController'
                        }
                    },
                    resolve: {
                        plugins: ['$rootScope', '$state','$$animateJs', '$ocLazyLoad','$filter', function ($rootScope, $state,$$animateJs, $ocLazyLoad,$filter) {

                            var files = [
                                cdnUrl + 'view/app/module/moduleDetailController.js',
                                cdnUrl + 'view/app/module/moduleFormModalController.js',
                                cdnUrl + 'view/app/email/bulkEMailController.js',                          
                                cdnUrl + 'view/app/module/moduleAddModalController.js',
                                cdnUrl + 'view/app/email/singleEmailController.js',
                                cdnUrl + 'view/app/sms/singleSMSController.js',
                                cdnUrl + 'view/app/actionbutton/actionButtonFrameController.js',
                                cdnUrl + 'view/app/location/locationFormModalController.js',
                                cdnUrl + 'view/app/email/templateService.js',                             
                            ];

                            currentSectionComponentsTemplate = [];
                            var moduleId = $filter('filter')($rootScope.modules, {name: $state.params.type}, true)[0].id;

                            if (sectionComponents['component' + moduleId]) {
                                var sectionComponent = sectionComponents['component' + moduleId];

                                for (var i = 0; i < sectionComponent.length; i++) {
                                    var sectionFiles = angular.fromJson(sectionComponent[i].content).files;
                                    angular.forEach(sectionFiles, function (item) {
                                        files.push(item)
                                    });


                                    currentSectionComponentsTemplate.push(angular.fromJson(sectionComponent[i].content).app.templateUrl);
                                }
                            }



                            return $ocLazyLoad.load(files);
                        }]
                    }
                })

                .state('app.moduleForm', {
                    url: '/moduleForm/:type?stype?id?ptype?pid?rtab?pptype?ppid?prtab?back?clone?revise?many?field?value',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/module/moduleForm.html',
                            controller: 'ModuleFormController'
                        }
                    },
                    resolve: {
                        plugins: ['$rootScope', '$state', '$$animateJs', '$ocLazyLoad', '$filter', function ($rootScope, $state, $$animateJs, $ocLazyLoad, $filter) {


                            var files = [
                                cdnUrl + 'view/app/module/moduleFormController.js',
                                cdnUrl + 'view/app/module/moduleFormModalController.js',
                                cdnUrl + 'view/app/actionbutton/actionButtonFrameController.js',
                            ];

                            currentSectionComponentsTemplate = [];
                            var moduleId = $filter('filter')($rootScope.modules, {name: $state.params.type}, true)[0].id;

                            if (sectionComponents['component' + moduleId]) {
                                var sectionComponent = sectionComponents['component' + moduleId];

                                for (var i = 0; i < sectionComponent.length; i++) {
                                    var sectionFiles = angular.fromJson(sectionComponent[i].content).files;
                                    angular.forEach(sectionFiles, function (item) {
                                        files.push(item)
                                    });


                                    currentSectionComponentsTemplate.push(angular.fromJson(sectionComponent[i].content).app.templateUrl);
                                }
                            }


                            if (googleMapsApiKey) {
                                files.push({
                                    type: 'js',
                                    path: 'https://maps.googleapis.com/maps/api/js?key=' + googleMapsApiKey + '&libraries=places'
                                });
                            }

                            return $ocLazyLoad.load(files);
                        }]
                    }
                })

                .state('app.viewForm', {
                    url: '/viewForm/:type?id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/view/viewForm.html',
                            controller: 'ViewFormController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/view/viewFormController.js',
                                cdnUrl + 'view/app/view/viewService.js'
                            ]);
                        }]
                    }
                })

                .state('app.tasks', {
                    url: '/tasks',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/tasks/tasks.html',
                            controller: 'TaskController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/tasks/taskController.js',
                                cdnUrl + 'view/app/tasks/taskService.js',
                                cdnUrl + 'view/app/tasks/taskDirective.js'
                            ]);
                        }]
                    }
                })

                .state('app.documents', {
                    url: '/documents/:type?id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/documents/documents.html',
                            controller: 'DocumentController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/documents/documentController.js'
                            ]);
                        }]
                    }

                })

                .state('app.calendar', {
                    url: '/calendar',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/calendar/calendar.html',
                            controller: 'CalendarController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/calendar/calendarController.js',
                                cdnUrl + 'view/app/module/moduleFormModalController.js'
                            ]);
                        }]
                    }
                })

                .state('app.documentSearch', {
                    url: '/documentSearch',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/documents/advDocumentSearch.html',
                            controller: 'AdvDocumentSearchController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/documents/advDocumentSearchController.js'
                            ]);
                        }]
                    }

                })

                .state('app.timesheet', {
                    url: '/timesheet?user?project?month?ctype',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/timesheet/timesheet.html',
                            controller: 'TimesheetController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/timesheet/timesheetController.js',
                                cdnUrl + 'view/app/timesheet/timesheetModalController.js',
                                cdnUrl + 'view/app/timesheet/timesheetFrameController.js'
                            ]);
                        }]
                    }

                })

                .state('app.timesheetList', {
                    url: '/timesheetList',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/timesheet/timesheetList.html',
                            controller: 'TimesheetListController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/timesheet/timesheetListController.js'
                            ]);
                        }]
                    }
                })

                .state('app.newsfeed', {
                    url: '/newsfeed',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/newsfeed/newsfeed.html',
                            controller: 'NewsfeedController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/newsfeed/newsfeedController.js',
                                cdnUrl + 'view/app/module/moduleFormModalController.js'
                            ]);
                        }]
                    }
                })

                .state('app.import', {
                    url: '/import/:type',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/data/import.html',
                            controller: 'ImportController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                'scripts/vendor/xlsx.core.min.js',
                                cdnUrl + 'view/app/data/importController.js',
                                cdnUrl + 'view/app/data/importService.js'
                            ]);
                        }]
                    }

                })

                .state('app.importCsv', {
                    url: '/importcsv/:type',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/data/csv/import.html',
                            controller: 'ImportController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/data/csv/importController.js',
                                cdnUrl + 'view/app/data/csv/importService.js'
                            ]);
                        }]
                    }

                })

                .state('app.leadconvert', {
                    url: '/leadconvert?id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/convert/leadConvert.html',
                            controller: 'LeadConvertController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/convert/leadConvertController.js',
                                cdnUrl + 'view/app/convert/leadConvertService.js'
                            ]);
                        }]
                    }

                })

                .state('app.personalconvert', {
                    url: '/personalconvert?id',
                    views: {
                        'app': {
                            templateUrl: 'view/app/convert/personalConvert.html',
                            controller: 'PersonalConvertController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                'view/app/convert/personalConvertController.js' + '?v=' + version,
                                'view/app/convert/personalConvertService.js' + '?v=' + version
                            ]);
                        }]
                    }

                })

                .state('app.quoteconvert', {
                    url: '/quoteconvert?id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/convert/quoteConvert.html',
                            controller: 'QuoteConvertController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/convert/quoteConvertController.js',
                                cdnUrl + 'view/app/convert/quoteConvertService.js'
                            ]);
                        }]
                    }
                })

                .state('app.analytics', {
                    url: '/analytics?id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/analytics/analytics.html',
                            controller: 'AnalyticsController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/analytics/analyticsService.js',
                                cdnUrl + 'view/app/analytics/analyticsController.js'
                            ]);
                        }]
                    }
                })

                .state('app.analyticsForm', {
                    url: '/analyticsForm?id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/analytics/analyticsForm.html',
                            controller: 'AnalyticsFormController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/analytics/analyticsService.js',
                                cdnUrl + 'view/app/analytics/analyticsFormController.js'
                            ]);
                        }]
                    }
                })

                .state('app.reports', {
                    url: '/reports?categoryId?id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/reports/reportCategory.html',
                            controller: 'ReportsController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                'scripts/vendor/angular-fusioncharts.js',
                                cdnUrl + 'view/app/reports/reportsService.js',
                                cdnUrl + 'view/app/reports/reportCategoryController.js'
                            ]);
                        }]
                    }
                })

                .state('app.timetracker', {
                    url: '/timetracker?user?year?month?week',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/timesheet/timetracker.html',
                            controller: 'TimetrackerController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/timesheet/timetrackerController.js',
                                cdnUrl + 'view/app/timesheet/timetrackerModalController.js',
                                cdnUrl + 'view/app/timesheet/timetrackerService.js'
                            ]);
                        }]
                    }
                })

                .state('app.expense', {
                    url: '/expense?user?year?month?week',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/expensesheet/expense.html',
                            controller: 'ExpenseController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/expensesheet/expenseController.js',
                                cdnUrl + 'view/app/module/moduleFormModalController.js'
                            ]);
                        }]
                    }
                })


                .state('app.report', {
                    url: '/report',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/reports/createReport.html',
                            controller: 'CreateReportController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/reports/reportsService.js',
                                cdnUrl + 'view/app/reports/createReportController.js'
                            ]);
                        }]
                    }
                })

                .state('app.directory', {
                    url: '/directory?id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/directory/directoryDetail.html',
                            controller: 'DirectoryDetailController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/directory/directoryDetailController.js',
                                cdnUrl + 'view/app/directory/directoryDirective.js'
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
                            templateUrl: cdnUrl + 'view/setup/setup.html',
                            controller: 'SetupController'
                        }
                    },
                    resolve: {
                        start: ['$rootScope', '$q', '$state', 'AppService',
                            function ($rootScope, $q, $state, AppService) {
                                var deferred = $q.defer();

                                if (!$rootScope.user) {
                                    AppService.getMyAccount()
                                        .then(function () {
                                            deferred.resolve();
                                        });
                                } else {
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
                            templateUrl: cdnUrl + 'view/setup/settings/settings.html',
                            controller: 'SettingController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/settings/settingController.js',
                                cdnUrl + 'view/setup/settings/settingService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.general', {
                    url: '/general',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/general/generalSettings.html',
                            controller: 'GeneralSettingsController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/general/generalSettingsController.js',
                                cdnUrl + 'view/setup/general/generalSettingsService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.organization', {
                    url: '/organization',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/organization/organization.html',
                            controller: 'OrganizationController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/organization/organizationController.js',
                                cdnUrl + 'view/setup/organization/organizationService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.notifications', {
                    url: '/notifications',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/notifications/notifications.html',
                            controller: 'NotificationController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/notifications/notificationController.js',
                                cdnUrl + 'view/setup/notifications/notificationService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.users', {
                    url: '/users',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/users/users.html',
                            controller: 'UserController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/users/userController.js',
                                cdnUrl + 'view/setup/users/userService.js',
                                cdnUrl + 'view/setup/workgroups/workgroupService.js',
                                cdnUrl + 'view/setup/profiles/profileService.js',
                                cdnUrl + 'view/setup/roles/roleService.js',
                                cdnUrl + 'view/setup/usercustomshares/userCustomShareService.js',
                                cdnUrl + 'view/setup/license/licenseService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.profiles', {
                    url: '/profiles',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/profiles/profiles.html',
                            controller: 'ProfileController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/profiles/profileController.js',
                                cdnUrl + 'view/setup/profiles/profileService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.profile', {
                    url: '/profile?id&clone',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/profiles/profileForm.html',
                            controller: 'ProfileFormController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/profiles/profileFormController.js',
                                cdnUrl + 'view/setup/profiles/profileService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.roles', {
                    url: '/roles',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/roles/roles.html',
                            controller: 'RoleController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/roles/roleController.js',
                                cdnUrl + 'view/setup/roles/roleService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.role', {
                    url: '/role?id&reportsTo',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/roles/roleForm.html',
                            controller: 'RoleFormController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/roles/roleFormController.js',
                                cdnUrl + 'view/setup/roles/roleService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.modules', {
                    url: '/modules',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/modules/modulesSetup.html',
                            controller: 'ModuleSetupController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/modules/moduleSetupController.js',
                                cdnUrl + 'view/setup/modules/moduleSetupService.js',
                                cdnUrl + 'view/setup/license/licenseService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.module', {
                    url: '/module?id&clone&redirect',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/modules/moduleSetupForm.html',
                            controller: 'ModuleFormSetupController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/modules/moduleSetupFormController.js',
                                cdnUrl + 'view/setup/modules/moduleSetupLayoutController.js',
                                cdnUrl + 'view/setup/modules/moduleSetupService.js',
                                cdnUrl + 'view/app/location/locationFormModalController.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.modulerelations', {
                    url: '/module/relations/:module',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/modules/moduleRelations.html',
                            controller: 'ModuleRelationController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/modules/moduleRelationController.js',
                                cdnUrl + 'view/setup/modules/moduleSetupService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.moduledependencies', {
                    url: '/module/dependencies/:module',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/modules/moduleDependencies.html',
                            controller: 'ModuleDependencyController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/modules/moduleDependencyController.js',
                                cdnUrl + 'view/setup/modules/moduleSetupService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.actionButtons', {
                    url: '/module/actionButtons/:module',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/modules/actionButtons.html',
                            controller: 'ActionButtonsController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/modules/actionButtonsController.js',
                                cdnUrl + 'view/setup/modules/moduleSetupService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.moduleProfileSettings', {
                    url: '/module/moduleProfileSettings/:module',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/modules/moduleProfileSettings.html',
                            controller: 'ModuleProfileSettingController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/modules/moduleProfileSettingsController.js',
                                cdnUrl + 'view/setup/modules/moduleSetupService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.leadconvertmap', {
                    url: '/leadconvertmap',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/convert/leadConvertMap.html',
                            controller: 'LeadConvertMapController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/convert/leadConvertMapController.js',
                                cdnUrl + 'view/setup/convert/convertMapService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.candidateconvertmap', {
                    url: '/candidateconvertmap',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/convert/candidateConvertMap.html',
                            controller: 'CandidateConvertMapController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/convert/candidateConvertMapController.js',
                                cdnUrl + 'view/setup/convert/convertMapService.js'
                            ]);
                        }]
                    }
                })
                .state('app.setup.quoteconvertmap', {
                    url: '/quoteconvertmap',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/convert/quoteConvertMap.html',
                            controller: 'quoteConvertMapController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/convert/quoteConvertMapController.js',
                                cdnUrl + 'view/setup/convert/convertMapService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.import', {
                    url: '/importhistory',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/importhistory/importHistory.html',
                            controller: 'ImportHistoryController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/importhistory/importHistoryController.js',
                                cdnUrl + 'view/setup/importhistory/importHistoryService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.messaging', {
                    url: '/messaging',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/messaging/messaging.html',
                            controller: 'MessagingController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/messaging/messagingController.js',
                                cdnUrl + 'view/setup/messaging/messagingService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.office', {
                    url: '/office',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/office/office.html',
                            controller: 'OfficeController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/office/officeController.js',
                                cdnUrl + 'view/setup/office/officeService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.phone', {
                    url: '/phone',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/phone/phone.html',
                            controller: 'PhoneSettingsController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/phone/phoneSettingsController.js',
                                cdnUrl + 'view/setup/phone/phoneSettingsService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.auditlog', {
                    url: '/auditlog',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/auditlog/auditlogs.html',
                            controller: 'AuditLogController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/auditlog/auditLogController.js',
                                cdnUrl + 'view/setup/auditlog/auditLogService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.templates', {
                    url: '/templates',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/templates/templates.html',
                            controller: 'TemplateController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/templates/templateService.js',
                                cdnUrl + 'view/setup/templates/templateController.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.template', {
                    url: '/template?id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/templates/templateForm.html',
                            controller: 'TemplateFormController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/templates/templateService.js',
                                cdnUrl + 'view/setup/templates/templateFormController.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.templateguide', {
                    url: '/templateguide',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/templates/templateGuide.html',
                            controller: 'TemplateGuideController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/templates/templateGuideController.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.menu', {
                    url: '/menu',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/menu/menu.html',
                            controller: 'MenuController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/menu/menuController.js',
                                cdnUrl + 'view/setup/menu/menuService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.menu_list', {
                    url: '/menu_list',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/menu/menuList.html',
                            controller: 'MenuListController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/menu/menuListController.js',
                                cdnUrl + 'view/setup/menu/menuService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.workflows', {
                    url: '/workflows',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/workflow/workflows.html',
                            controller: 'WorkflowController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/workflow/workflowController.js',
                                cdnUrl + 'view/setup/workflow/workflowService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.workflow', {
                    url: '/workflow?id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/workflow/workflowForm.html',
                            controller: 'WorkflowFormController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/workflow/workflowFormController.js',
                                cdnUrl + 'view/setup/workflow/workflowService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.approvel_process', {
                    url: '/approvel_process',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/approvel_process/approvelProcesses.html',
                            controller: 'ApprovelProcessController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/approvel_process/approvelProcessController.js',
                                cdnUrl + 'view/setup/approvel_process/approvelProcessService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.help', {
                    url: '/help',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/help/helpPage.html',
                            controller: 'HelpController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/help/helpController.js',

                            ]);
                        }]
                    }
                })

                .state('app.setup.helpside', {
                    url: '/helpside',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/help/helpPageSide.html',
                            controller: 'HelpController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/help/helpController.js',

                            ]);
                        }]
                    }
                })

                .state('app.setup.helpsides', {
                    url: '/helpsides',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/help/helpPageSides.html',
                            controller: 'HelpController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/help/helpController.js',

                            ]);
                        }]
                    }
                })

                .state('app.setup.approvel', {
                    url: '/approvel?id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/approvel_process/approvelProcessForm.html',
                            controller: 'ApprovelProcessFormController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/approvel_process/approvelProcessFormController.js',
                                cdnUrl + 'view/setup/approvel_process/approvelProcessService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.warehouse', {
                    url: '/warehouse',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/warehouse/warehouse.html',
                            controller: 'WarehouseController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/analytics/analyticsService.js',
                                cdnUrl + 'view/setup/warehouse/warehouseController.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.usergroups', {
                    url: '/usergroups',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/usergroups/userGroups.html',
                            controller: 'UserGroupController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/usergroups/userGroupController.js',
                                cdnUrl + 'view/setup/usergroups/userGroupService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.usercustomshares', {
                    url: '/usercustomshares',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/usercustomshares/userCustomShares.html',
                            controller: 'UserCustomShareController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/usercustomshares/userCustomShareController.js',
                                cdnUrl + 'view/setup/usercustomshares/userCustomShareService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.usercustomshare', {
                    url: '/usercustomshare?id&clone',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/usercustomshares/userCustomShareForm.html',
                            controller: 'UserCustomShareFormController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/usercustomshares/userCustomShareFormController.js',
                                cdnUrl + 'view/setup/usercustomshares/userCustomShareService.js'
                            ]);
                        }]
                    }
                })

                .state('app.setup.outlook', {
                    url: '/outlook',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/outlook/outlook.html',
                            controller: 'OutlookController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/outlook/outlookController.js',
                                cdnUrl + 'view/setup/outlook/outlookService.js'

                            ]);
                        }]
                    }
                })

                .state('app.setup.usergroup', {
                    url: '/usergroup?id&clone',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/usergroups/userGroupForm.html',
                            controller: 'UserGroupFormController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/usergroups/userGroupFormController.js',
                                cdnUrl + 'view/setup/usergroups/userGroupService.js'
                            ]);
                        }]
                    }
                });

            //other
            $stateProvider
                .state('paymentform', {
                    url: '/paymentform',
                    templateUrl: cdnUrl + 'view/app/payment/paymentForm.html',
                    controller: 'PaymentFormController'
                });

            if (components !== '') {
                var _components = angular.fromJson(components);

                angular.forEach(_components, function (component) {
                    if (!component.content)
                        return;


                    var files = [];
                    var componentContent = angular.fromJson(component.content);

                    if (component.place === 1001) {
                        if (sectionComponents['component' + component.module_id]) {
                            sectionComponents.push(component);

                        } else {
                            sectionComponents['component' + component.module_id] = [];
                            sectionComponents['component' + component.module_id].push(component);
                            // console.log(sectionComponents['component' + component.module_id][0].content);
                        }
                        return;
                    }


                    var replaceDynamicValues = function (str) {
                        var splitUrls = str.split('{appConfigs.');

                        if (splitUrls.length > 1) {
                            for (var i in splitUrls) {
                                if (splitUrls.hasOwnProperty(i)) {
                                    if (!splitUrls[i])
                                        continue;

                                    var configObj = splitUrls[i].split('}')[0];
                                    str = str.replace('{appConfigs.' + configObj + '}', appConfigs[configObj]);
                                }
                            }
                        }

                        return str;
                    };

                    componentContent.app.templateUrl = replaceDynamicValues(componentContent.app.templateUrl);

                    var url = componentContent.local === 't' ? 'views/app/' + component.name + '/' : blobUrl + '/components/' + (componentContent.level === 'app' || preview ? 'app-' + applicationId : 'tenant-' + tenantId) + '/' + component.name + '/';

                    for (var i = 0; i < componentContent.files.length; i++) {
                        componentContent.files[i] = replaceDynamicValues(componentContent.files[i]);

                        files.push(componentContent.files[i].lastIndexOf('http', 0) === 0 ? componentContent.files[i] : url + componentContent.files[i]);
                    }

                    $stateProvider
                        .state('app.' + component.name, {
                            cache: false,
                            url: '/' + componentContent.url,
                            views: {
                                'app': {
                                    templateUrl: function ($stateParams) {
                                        var str = "?";

                                        for (var p in $stateParams) {
                                            if ($stateParams[p]) {
                                                str += p + '=' + $stateParams[p] + '&';
                                            }
                                        }
                                        str = str.substring(0, str.length - 1);

                                        var fUrl = componentContent.app.templateUrl.lastIndexOf('http', 0) === 0 ? componentContent.app.templateUrl : url + componentContent.app.templateUrl;

                                        if (str.length > 1) {
                                            fUrl += str;
                                        }

                                        return fUrl;
                                    },
                                    controller: componentContent.app.controller
                                }
                            },
                            resolve: {
                                plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                                    return $ocLazyLoad.load(files);
                                }]
                            }

                        });
                });
            }

            $urlRouterProvider.otherwise('/app/dashboard');
        }]);