'use strict';

angular.module('primeapps')

    .config(['$stateProvider', '$urlRouterProvider',
        function ($stateProvider, $urlRouterProvider) {
            if (token) {
                window.localStorage['access_token'] = token;
            }

            if (!window.localStorage.getItem('access_token')) {
                return;
            }
            //app
            $stateProvider
                .state('app', {
                    url: '/',
                    abstract: true,
                    templateUrl: 'view/layout.html',
                    controller: 'LayoutController',
                    resolve: {
                        LayoutService: 'LayoutService',
                        start: ['$rootScope', 'LayoutService',
                            function ($rootScope, LayoutService) {
                                if (!$rootScope.user)
                                    return LayoutService.getOrg();
                            }]
                    }
                });

            $stateProvider
                .state('app.home', {
                    url: '/home',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/home/home.html',
                            controller: 'HomeController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/home/homeController.js',
                                cdnUrl + 'view/directory/directoryDirective.js'
                            ]);
                        }]
                    }
                })
                .state('app.allApps', {
                    url: 'allApps',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/allapps/allApps.html',
                            controller: 'AllAppsController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/allapps/allAppsService.js',
                                cdnUrl + 'view/allapps/allAppsController.js'
                            ]);
                        }]
                    }
                })
                .state('app.Apps', {
                    url: 'apps?:organizationId',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/organization/apps/apps.html',
                            controller: 'AppsController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/organization/apps/AppsService.js',
                                cdnUrl + 'view/organization/apps/AppsController.js'
                            ]);
                        }]
                    }
                })
                .state('app.appsForm', {
                    url: 'appsForm?:organizationId',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/organization/appsform/appsForm.html',
                            controller: 'AppsFormController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/organization/apps/AppsFormService.js',
                                cdnUrl + 'view/organization/apps/AppsFormController.js'
                            ]);
                        }]
                    }
                })
                .state('app.collaborators', {
                    url: 'collaborators?:organizationId',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/organization/collaborators/collaborators.html',
                            controller: 'CollaboratorsController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/organization/collaborators/CollaboratorsService.js',
                                cdnUrl + 'view/organization/collaborators/CollaboratorsController.js'
                            ]);
                        }]
                    }
                })
                .state('app.teams', {
                    url: 'teams?:organizationId',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/organization/teams/teams.html',
                            controller: 'TeamsController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/organization/teams/TeamsService.js',
                                cdnUrl + 'view/organization/teams/TeamsController.js'
                            ]);
                        }]
                    }
                })
                .state('app.organizationForm', {
                    url: 'organization?:id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/organization/organizationform/organizationform.html',
                            controller: 'OrganizationFormController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/organization/organizationform/OrganizationFormService.js',
                                cdnUrl + 'view/organization/organizationform/OrganizationFormController.js'
                            ]);
                        }]
                    }
                })
                .state('app.account', {
                    url: 'account',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/account/account.html',
                            controller: 'AccountController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/account/AccountService.js',
                                cdnUrl + 'view/account/AccountController.js'
                            ]);
                        }]
                    }
                })
                .state('app.dashboard', {
                    url: 'dashboard',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/dashboard/dashboard.html',
                            controller: 'DashboardController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                'scripts/vendor/angular-fusioncharts.js',
                                cdnUrl + 'view/dashboard/dashboardService.js',
                                cdnUrl + 'view/dashboard/dashboardController.js'
                            ]);
                        }]
                    }
                })

                .state('app.viewForm', {
                    url: '/viewForm/:type?id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/view/viewForm.html',
                            controller: 'ViewFormController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/view/viewFormController.js',
                                cdnUrl + 'view/view/viewService.js'
                            ]);
                        }]
                    }
                })

                .state('app.reports', {
                    url: '/reports?categoryId?id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/reports/reportCategory.html',
                            controller: 'ReportsController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                'scripts/vendor/angular-fusioncharts.js',
                                cdnUrl + 'view/reports/reportsService.js',
                                cdnUrl + 'view/reports/reportCategoryController.js'
                            ]);
                        }]
                    }
                })

                .state('app.report', {
                    url: 'report',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/reports/createReport.html',
                            controller: 'CreateReportController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/reports/reportsService.js',
                                cdnUrl + 'view/reports/createReportController.js'
                            ]);
                        }]
                    }
                });

            //app.setup
            $stateProvider
                .state('app.setup', {
                    url: 'app/:appId',
                    abstract: true,
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/setup.html',
                            controller: 'SetupController'
                        }
                    },
                    resolve: {
                        start: ['$rootScope', '$q', '$state', 'LayoutService',
                            function ($rootScope, $q, $state, LayoutService) {
                                var deferred = $q.defer();

                                if ($rootScope.preview) {
                                    $state.go('app.allApps');
                                    deferred.resolve();
                                    return deferred.promise;
                                }

                                // if (!$rootScope.user) {
                                //     LayoutService.getMyAccount()
                                //         .then(function () {
                                //             deferred.resolve();
                                //         });
                                // }
                                // else {
                                //     deferred.resolve();
                                // }
                                //
                                // return deferred.promise;
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
                                cdnUrl + 'view/location/locationFormModalController.js'
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
                                cdnUrl + 'view/analytics/analyticsService.js',
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


            $urlRouterProvider.otherwise('/allApps');
        }]);