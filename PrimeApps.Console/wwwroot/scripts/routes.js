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
                .state('studio', {
                    url: '/',
                    abstract: true,
                    templateUrl: 'view/layout.html',
                    controller: 'LayoutController',
                    resolve: {
                        studio: ['$rootScope', 'LayoutService', function ($rootScope, LayoutService) {
                            return LayoutService.getAll();
                        }]
                    }
                });

            $stateProvider

                .state('studio.allApps', {
                    url: 'all-apps',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/allapps/allApps.html',
                            controller: 'AllAppsController'
                        }
                    },
                    resolve: {
                        start: ['$rootScope', 'studio',
                            function ($rootScope, studio) {
                                $rootScope.currentAppId = null;
                            }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/allapps/allAppsService.js',
                                cdnUrl + 'view/allapps/allAppsController.js'
                            ]);
                        }]
                    }
                })

                .state('studio.apps', {
                    url: 'apps?:orgId',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/organization/apps/apps.html',
                            controller: 'AppsController'
                        }
                    },
                    resolve: {
                        start: ['$rootScope', 'studio',
                            function ($rootScope, studio) {
                                $rootScope.currentAppId = null;
                            }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/organization/apps/appsService.js',
                                cdnUrl + 'view/organization/apps/appsController.js'
                            ]);
                        }]
                    }
                })

                .state('studio.appsForm', {
                    url: 'appForm?:orgId',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/organization/appform/appForm.html',
                            controller: 'AppFormController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', 'studio', function ($$animateJs, $ocLazyLoad, studio) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/organization/appform/appFormService.js',
                                cdnUrl + 'view/organization/appform/appFormController.js'
                            ]);
                        }]
                    }
                })
                .state('studio.appTemplates', {
                    url: 'appTemplates',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/templates/appTemplates.html',
                            controller: 'AppTemplatesController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', 'studio', function ($$animateJs, $ocLazyLoad, studio) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/templates/appTemplatesController.js'
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
                        plugins: ['$$animateJs', '$ocLazyLoad', 'studio', function ($$animateJs, $ocLazyLoad, studio) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/organization/organizationform/organizationFormService.js',
                                cdnUrl + 'view/organization/organizationform/organizationFormController.js'
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
                        plugins: ['$$animateJs', '$ocLazyLoad', 'studio', function ($$animateJs, $ocLazyLoad, studio) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/account/accountService.js',
                                cdnUrl + 'view/account/accountController.js'
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
                        plugins: ['$$animateJs', '$ocLazyLoad', 'studio', function ($$animateJs, $ocLazyLoad, studio) {
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
                        plugins: ['$$animateJs', '$ocLazyLoad', 'studio', function ($$animateJs, $ocLazyLoad, studio) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/view/viewFormController.js',
                                cdnUrl + 'view/view/viewService.js'
                            ]);
                        }]
                    }
                })


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
                        organization: ['$rootScope', '$q', '$state', '$stateParams', '$filter', 'studio',
                            function ($rootScope, $q, $state, $stateParams, $filter, studio) {
                                $rootScope.currentOrgId = parseInt($stateParams.organizationId);

                                if (!$rootScope.currentOrgId) {
                                    $state.go('studio.allApps');
                                }

                                $rootScope.currentOrganization = $filter('filter')($rootScope.organizations, {id: $rootScope.currentOrgId})[0];

                                $rootScope.breadcrumblist = [{}, {}, {}];
                                $rootScope.breadcrumblist[0].title = $rootScope.currentOrganization.label;
                                $rootScope.breadcrumblist[0].link = '#/apps?orgId=' + $rootScope.currentOrganization.id;
                                $rootScope.breadcrumblist[1].title = "People";
                                $rootScope.breadcrumblist[1].link = '#/organization/' + $rootScope.currentOrganization.id + '/collaborators';
                                $rootScope.breadcrumblist[2].title = "Collaborators";

                                if (!$rootScope.currentOrganization) {
                                    $state.go('studio.allApps');
                                }
                            }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/organization/organizationController.js',
                                cdnUrl + 'view/organization/organizationService.js'
                            ]);
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
                        start: ['$rootScope', 'organization',
                            function ($rootScope, organization) {
                                $rootScope.currentAppId = null;
                            }],
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
                        start: ['$rootScope', 'organization',
                            function ($rootScope, organization) {
                                $rootScope.currentAppId = null;
                            }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/organization/collaborators/collaboratorsService.js',
                                cdnUrl + 'view/organization/collaborators/collaboratorsController.js'
                            ]);
                        }]
                    }
                });

            //studio.app
            $stateProvider
                .state('studio.app', {
                    url: 'org/:orgId/app/:appId',
                    abstract: true,
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/app.html',
                            controller: 'AppController'
                        }
                    },
                    resolve: {
                        app: ['$rootScope', 'LayoutService', '$stateParams', '$state', '$filter', 'studio',
                            function ($rootScope, LayoutService, $stateParams, $state, $filter, studio) {
                                $rootScope.currentAppId = parseInt($stateParams.appId);
                                $rootScope.currentOrgId = parseInt($stateParams.orgId);
                            }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/model/modules/moduleService.js'
                            ]);
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
                        overview: ['LayoutService', 'app', function (LayoutService, app) {

                        }],
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
                        modules: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/model/modules/moduleController.js',
                                cdnUrl + 'view/app/model/modules/moduleService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.moduledesigner', {
                    url: '/moduleDesigner?:id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/model/modules/moduleDesigner.html',
                            controller: 'moduleDesignerController'
                        }
                    },
                    resolve: {
                        moduledesigner: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/model/modules/moduleDesignerController.js',
                                cdnUrl + 'view/app/model/modules/moduleService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.relations', {
                    url: '/relations?:id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/model/relations/relations.html',
                            controller: 'RelationsController'
                        }
                    },
                    resolve: {
                        relations: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/model/relations/relationsController.js',
                                cdnUrl + 'view/app/model/relations/relationsService.js',
                                cdnUrl + 'view/app/model/modules/moduleService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.filters', {
                    url: '/filters?:id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/model/filters/filtersList.html',
                            controller: 'FiltersController'
                        }
                    },
                    resolve: {
                        filters: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/model/filters/filtersService.js',
                                cdnUrl + 'view/app/model/modules/moduleService.js',
                                cdnUrl + 'view/app/model/filters/filtersController.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.dependencies', {
                    url: '/dependencies?:id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/model/dependencies/dependencies.html',
                            controller: 'DependenciesController'
                        }
                    },
                    resolve: {
                        dependencies: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/model/dependencies/dependenciesController.js',
                                cdnUrl + 'view/app/model/dependencies/dependenciesService.js',
                                cdnUrl + 'view/app/model/modules/moduleService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.templatesEmail', {
                    url: '/templatesEmail',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/templates/emailtemplates/emailTemplates.html',
                            controller: 'EmailTemplatesController'
                        }
                    },
                    resolve: {
                        templatesEmail: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/templates/emailtemplates/emailTemplatesController.js',
                                cdnUrl + 'view/app/templates/emailtemplates/emailTemplatesService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.templatesExcel', {
                    url: '/templatesExcel',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/templates/exceltemplates/excelTemplates.html',
                            controller: 'ExcelTemplatesController'
                        }
                    },
                    resolve: {
                        templatesExcel: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/templates/exceltemplates/excelTemplatesController.js',
                                cdnUrl + 'view/app/templates/exceltemplates/excelTemplatesService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.templatesWord', {
                    url: '/templatesWord',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/templates/wordtemplates/wordTemplates.html',
                            controller: 'WordTemplatesController'
                        }
                    },
                    resolve: {
                        templatesWord: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/templates/wordtemplates/wordTemplatesController.js',
                                cdnUrl + 'view/app/templates/wordtemplates/wordTemplatesService.js'
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
                        templatesEmailGuide: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/templates/emailtemplates/templatesEmailGuideController.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.workflows', {
                    url: '/workflows',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/automation/workflows/workflows.html',
                            controller: 'WorkflowsController'
                        }
                    },
                    resolve: {
                        workflows: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/automation/workflows/workflowsController.js',
                                cdnUrl + 'view/app/automation/workflows/workflowsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.workflowEditor', {
                    url: '/workflowEditor?:id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/automation/workflows/workflowEditor.html',
                            controller: 'WorkflowEditorController'
                        }
                    },
                    resolve: {
                        workflowEditor: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/automation/workflows/workflowEditorController.js',
                                cdnUrl + 'view/app/automation/workflows/workflowsService.js',
                                cdnUrl + 'scripts/vendor/bpm/icons.js',
                                cdnUrl + 'scripts/vendor/bpm/BPMN.js',
                                cdnUrl + 'scripts/vendor/bpm/BPMNClasses.js'                                
                            ]);
                        }]
                    }
                })

                .state('studio.app.rules', {
                    url: '/rules',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/automation/rules/rules.html',
                            controller: 'RulesController'
                        }
                    },
                    resolve: {
                        rules: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/automation/rules/rulesController.js',
                                cdnUrl + 'view/app/automation/rules/rulesService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.processes', {
                    url: '/processes',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/automation/processes/processes.html',
                            controller: 'ProcessesController'
                        }
                    },
                    resolve: {
                        processes: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/automation/processes/processesController.js',
                                cdnUrl + 'view/app/automation/processes/processesService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.reports', {
                    url: '/reports',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/analytics/reports/reports.html',
                            controller: 'ReportsController'
                        }
                    },
                    resolve: {
                        reports: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/analytics/reports/reportsController.js',
                                cdnUrl + 'view/app/analytics/reports/reportsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.dashboards', {
                    url: '/dashboards',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/analytics/dashboards/dashboards.html',
                            controller: 'DashboardsController'
                        }
                    },
                    resolve: {
                        dashboards: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/analytics/dashboards/dashboardsController.js',
                                cdnUrl + 'view/app/analytics/dashboards/dashboardsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.warehouse', {
                    url: '/warehouse',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/analytics/warehouse/warehouse.html',
                            controller: 'WarehouseController'
                        }
                    },
                    resolve: {
                        warehouse: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/analytics/warehouse/warehouseController.js',
                                cdnUrl + 'view/app/analytics/warehouse/warehouseService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.bi', {
                    url: '/bi',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/analytics/bi/bi.html',
                            controller: 'BiController'
                        }
                    },
                    resolve: {
                        bi: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/analytics/bi/biController.js',
                                cdnUrl + 'view/app/analytics/bi/biService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.components', {
                    url: '/components',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/components/components.html',
                            controller: 'ComponentsController'
                        }
                    },
                    resolve: {
                        components: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/components/componentsController.js',
                                cdnUrl + 'view/app/components/componentsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.componentDetail', {
                    url: '/componentDetail?:id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/components/componentDetail.html',
                            controller: 'ComponentDetailController'
                        }
                    },
                    resolve: {
                        componentDetail: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/components/componentDetailController.js',
                                cdnUrl + 'view/app/components/componentsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.functions', {
                    url: '/functions',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/functions/functions.html',
                            controller: 'FunctionsController'
                        }
                    },
                    resolve: {
                        components: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/functions/functionsController.js',
                                cdnUrl + 'view/app/functions/functionsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.functionDetail', {
                    url: '/functionDetail?:id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/functions/functionDetail.html',
                            controller: 'FunctionDetailController'
                        }
                    },
                    resolve: {
                        componentDetail: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/functions/functionDetailController.js',
                                cdnUrl + 'view/app/functions/functionsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.profiles', {
                    url: '/profiles',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/authorization/profiles/profiles.html',
                            controller: 'ProfilesController'
                        }
                    },
                    resolve: {
                        profiles: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/authorization/profiles/profilesController.js',
                                cdnUrl + 'view/app/authorization/profiles/profilesService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.moduleprofilesettings', {
                    url: '/moduleprofilesettings/:module',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/model/modules/moduleProfileSettings.html',
                            controller: 'ModuleProfileSettingController'
                        }
                    },
                    resolve: {
                        moduleprofilesettings: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/model/modules/moduleProfileSettingsController.js',
                                cdnUrl + 'view/app/model/modules/moduleService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.roles', {
                    url: '/roles',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/authorization/roles/roles.html',
                            controller: 'RolesController'
                        }
                    },
                    resolve: {
                        roles: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/authorization/roles/rolesController.js',
                                cdnUrl + 'view/app/authorization/roles/rolesService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.menus', {
                    url: '/menus',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/menus/menus.html',
                            controller: 'MenusController'
                        }
                    },
                    resolve: {
                        menus: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/menus/menusController.js',
                                cdnUrl + 'view/app/menus/menusService.js',
                                cdnUrl + 'view/app/authorization/profiles/profilesService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.help', {
                    url: '/help',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/help/help.html',
                            controller: 'HelpController'
                        }
                    },
                    resolve: {
                        help: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/help/helpController.js',
                                cdnUrl + 'view/app/help/helpService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.deployment', {
                    url: '/deployment',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/deployment/deployment.html',
                            controller: 'DeploymentController'
                        }
                    },
                    resolve: {
                        deployment: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/deployment/deploymentController.js',
                                cdnUrl + 'view/app/deployment/deploymentService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.diagnostics', {
                    url: '/diagnostics',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/diagnostics/diagnostics.html',
                            controller: 'DiagnosticsController'
                        }
                    },
                    resolve: {
                        diagnostics: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/diagnostics/diagnosticsController.js',
                                cdnUrl + 'view/app/diagnostics/diagnosticsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.extensions', {
                    url: '/extensions',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/extensions/extensions.html',
                            controller: 'ExtensionsController'
                        }
                    },
                    resolve: {
                        extensions: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/extensions/extensionsController.js',
                                cdnUrl + 'view/app/extensions/extensionsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.certificates', {
                    url: '/certificates',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/security/certificates/certificates.html',
                            controller: 'CertificatesController'
                        }
                    },
                    resolve: {
                        certificates: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/security/certificates/certificatesController.js',
                                cdnUrl + 'view/app/security/certificates/certificatesService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.passwordpolicies', {
                    url: '/passwordPolicies',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/security/passwordpolicies/passwordpolicies.html',
                            controller: 'PasswordPoliciesController'
                        }
                    },
                    resolve: {
                        passwordpolicies: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/security/passwordpolicies/passwordpoliciesController.js',
                                cdnUrl + 'view/app/security/passwordpolicies/passwordpoliciesService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.audittrail', {
                    url: '/auditTrail',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/security/audittrail/audittrail.html',
                            controller: 'AuditTrailController'
                        }
                    },
                    resolve: {
                        audittrail: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/security/audittrail/auditTrailController.js',
                                cdnUrl + 'view/app/security/audittrail/auditTrailService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.cors', {
                    url: '/cors',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/security/cors/cors.html',
                            controller: 'CorsController'
                        }
                    },
                    resolve: {
                        cors: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/security/cors/corsController.js',
                                cdnUrl + 'view/app/security/cors/corsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.networkaccess', {
                    url: '/networkAccess',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/security/networkaccess/networkaccess.html',
                            controller: 'NetworkAccessController'
                        }
                    },
                    resolve: {
                        networkaccess: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/security/networkaccess/networkaccessController.js',
                                cdnUrl + 'view/app/security/networkaccess/networkaccessService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.authentication', {
                    url: '/authentication',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/identity/authentication/authentication.html',
                            controller: 'AuthenticationController'
                        }
                    },
                    resolve: {
                        authentication: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/identity/authentication/authenticationController.js',
                                cdnUrl + 'view/app/identity/authentication/authenticationService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.identity', {
                    url: '/identity',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/identity/identityprovider/identity.html',
                            controller: 'IdentityController'
                        }
                    },
                    resolve: {
                        identity: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/identity/identityprovider/identityController.js',
                                cdnUrl + 'view/app/identity/identityprovider/identityService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.singlesignon', {
                    url: '/singleSingOn',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/identity/singlesingon/singleSignOn.html',
                            controller: 'SingleSingOnController'
                        }
                    },
                    resolve: {
                        singlesignon: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/identity/singlesingon/singleSignOnController.js',
                                cdnUrl + 'view/app/identity/singlesingon/singleSignOnService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.appdetails', {
                    url: '/appDetails',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/settings/appdetails/appDetails.html',
                            controller: 'AppDetailsController'
                        }
                    },
                    resolve: {
                        appdetails: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/settings/appdetails/appDetailsController.js',
                                cdnUrl + 'view/app/settings/appdetails/appDetailsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.appcollaborators', {
                    url: '/appCollaborators',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/settings/collaborators/appCollaborators.html',
                            controller: 'AppCollaboratorsController'
                        }
                    },
                    resolve: {
                        appcollaborators: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/settings/collaborators/appCollaboratorsController.js',
                                cdnUrl + 'view/app/settings/collaborators/appCollaboratorsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.manage', {
                    url: 'manage',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/organization/manage/manage.html',
                            controller: 'ManageController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', 'studio', function ($$animateJs, $ocLazyLoad, studio) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/organization/manage/manageService.js',
                                cdnUrl + 'view/organization/manage/manageController.js',
                                cdnUrl + 'view/app/model/modules/moduleService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.notifications', {
                    url: '/notifications',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/settings/notifications/notifications.html',
                            controller: 'NotificationsController'
                        }
                    },
                    resolve: {
                        notifications: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/settings/notifications/notificationsController.js',
                                cdnUrl + 'view/app/settings/notifications/notificationsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.appEmailTemplates', {
                    url: '/appEmailTemplates',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/settings/appemailtemplates/appEmailTemplates.html',
                            controller: 'AppEmailTemplatesController'
                        }
                    },
                    resolve: {
                        templatesEmail: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/settings/appemailtemplates/appEmailTemplatesController.js',
                                cdnUrl + 'view/app/settings/appemailtemplates/appEmailTemplatesService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.moduleactions', {
                    url: '/actionButtons?:id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/model/modules/actionButtons.html',
                            controller: 'ActionButtonsController'
                        }
                    },
                    resolve: {
                        moduleactions: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', 'app', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/model/modules/actionButtonsController.js',
                                cdnUrl + 'view/app/model/modules/actionButtonsService.js',
                                cdnUrl + 'view/app/model/modules/moduleService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.report', {
                    url: '/report?:id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/analytics/reports/report.html',
                            controller: 'ReportController'
                        }
                    },
                    report: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                        if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                            $state.go('studio.app.overview', {
                                orgId: $rootScope.currentOrgId,
                                appId: $rootScope.currentAppId
                            });
                        }
                    }],
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', 'app', function ($$animateJs, $ocLazyLoad, app) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/analytics/reports/reportsService.js',
                                cdnUrl + 'view/app/analytics/reports/reportController.js'
                            ]);
                        }]
                    }
                })
                //settings
                .state('studio.settings', {
                    url: 'settings',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/settings/settings.html',
                            controller: 'SettingsController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', 'studio', function ($$animateJs, $ocLazyLoad, studio) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/settings/settingsService.js',
                                cdnUrl + 'view/settings/settingsController.js'
                            ]);
                        }]
                    }
                })

                .state('studio.settings.profile', {
                    url: '/profile',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/settings/profile/profile.html',
                            controller: 'ProfileController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', 'studio', function ($$animateJs, $ocLazyLoad, studio) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/settings/settingsService.js',
                                cdnUrl + 'view/settings/profile/profileController.js'
                            ]);
                        }]
                    }
                })

                .state('studio.settings.password', {
                    url: '/password',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/settings/password/password.html',
                            controller: 'PasswordController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', 'studio', function ($$animateJs, $ocLazyLoad, studio) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/settings/settingsService.js',
                                cdnUrl + 'view/settings/password/passwordController.js'
                            ]);
                        }]
                    }
                });

            $urlRouterProvider.otherwise('/all-apps');
        }])
;