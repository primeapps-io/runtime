﻿'use strict';
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
                .state('studio', {
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
                .state('studio.home', {
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

                .state('studio.allApps', {
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

                .state('studio.Apps', {
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

                .state('studio.appsForm', {
                    url: 'appForm?:organizationId',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/organization/appform/appForm.html',
                            controller: 'AppFormController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/organization/appform/appFormService.js',
                                cdnUrl + 'view/organization/appform/appFormController.js'
                            ]);
                        }]
                    }
                })

                .state('studio.organizationForm', {
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

                .state('studio.account', {
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

                .state('studio.dashboard', {
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

                .state('studio.viewForm', {
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

                .state('studio.reports', {
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

                .state('studio.report', {
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

            //app.organization
            $stateProvider
                .state('studio.organization', {
                    url: 'organization/:organizationId',
                    abstract: true,
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/organization/organization.html',
                            controller: 'OrganizationController'
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
                            }]
                    }
                })
                .state('studio.organization.teams', {
                    url: '/teams',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/organization/teams/teams.html',
                            controller: 'TeamsController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/organization/teams/teamsService.js',
                                cdnUrl + 'view/organization/teams/teamsController.js'
                            ]);
                        }]
                    }
                })
                .state('studio.organization.collaborators', {
                    url: '/collaborators',
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
                });

            //studio.app
            $stateProvider
                .state('studio.app', {
                    url: 'app/:appId',
                    abstract: true,
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/app.html',
                            controller: 'AppController'
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

                .state('studio.app.overview', {
                    url: '/overview?:id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/overview/overview.html',
                            controller: 'OverviewController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/overview/overviewController.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.modules', {
                    url: '/modules?:id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/model/modules/modules.html',
                            controller: 'ModuleController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/model/modules/moduleController.js',
                                cdnUrl + 'view/app/model/modules/moduleService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.relations', {
                    url: '/relations',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/model/relations/relations.html',
                            controller: 'RelationsController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/model/relations/relationsController.js',
                                cdnUrl + 'view/app/model/relations/relationsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.filters', {
                    url: '/filters',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/model/filters/filtersList.html',
                            controller: 'FiltersController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/model/filters/filtersController.js',
                                cdnUrl + 'view/app/model/filters/filtersService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.dependencies', {
                    url: '/dependencies',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/model/dependencies/dependencies.html',
                            controller: 'DependenciesController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/model/dependencies/dependenciesController.js',
                                cdnUrl + 'view/app/model/dependencies/dependenciesService.js'
                            ]);
                        }]
                    }
                })


                .state('studio.app.templatesEmail', {
                    url: '/templatesEmail?:id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/templates/emailtemplates/emailTemplatesList.html',
                            controller: 'EmailTemplatesController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/templates/emailtemplates/emailTemplatesController.js',
                                cdnUrl + 'view/app/templates/emailtemplates/emailTemplatesService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.templatesEmailGuide', {
                    url: '/templatesEmailGuide',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/templates/emailtemplates/templatesEmailGuide.html',
                            controller: 'TemplatesEmailGuideController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/templates/emailtemplates/templatesEmailGuideController.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.settings', {
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

                .state('studio.app.general', {
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

                .state('studio.app.organization', {
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

                .state('studio.app.notifications', {
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

                .state('studio.app.users', {
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

                .state('studio.app.profiles', {
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

                .state('studio.app.profile', {
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

                .state('studio.app.roles', {
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

                .state('studio.app.role', {
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

                .state('studio.app.module', {
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

                .state('studio.app.moduledependencies', {
                    url: '/module/dependencies/:module',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/setup/modules/moduleDependencies.html',
                            controller: 'DependencyController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/setup/modules/dependenciesController.js',
                                cdnUrl + 'view/setup/modules/dependenciesService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.actionButtons', {
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

                .state('studio.app.moduleProfileSettings', {
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

                .state('studio.app.leadconvertmap', {
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

                .state('studio.app.candidateconvertmap', {
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

                .state('studio.app.quoteconvertmap', {
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

                .state('studio.app.import', {
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

                .state('studio.app.messaging', {
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

                .state('studio.app.office', {
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

                .state('studio.app.phone', {
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

                .state('studio.app.auditlog', {
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

                .state('studio.app.templates', {
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

                .state('studio.app.template', {
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

                .state('studio.app.templateguide', {
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

                .state('studio.app.menu', {
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

                .state('studio.app.menu_list', {
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

                .state('studio.app.workflows', {
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

                .state('studio.app.workflow', {
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

                .state('studio.app.approvel_process', {
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

                .state('studio.app.help', {
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

                .state('studio.app.helpside', {
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

                .state('studio.app.helpsides', {
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

                .state('studio.app.approvel', {
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

                .state('studio.app.warehouse', {
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

                .state('studio.app.usergroups', {
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

                .state('studio.app.usercustomshares', {
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

                .state('studio.app.usercustomshare', {
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

                .state('studio.app.outlook', {
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

                .state('studio.app.usergroup', {
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