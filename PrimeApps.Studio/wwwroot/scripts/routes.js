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
                    url: 'org?:id',
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
                });

            //app.organization
            $stateProvider
                .state('studio.organization', {
                    url: 'org/:organizationId',
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

                                $rootScope.currentOrganization = $filter('filter')($rootScope.organizations, { id: $rootScope.currentOrgId })[0];

                                $rootScope.breadcrumblist = [{}, {}, {}];
                                $rootScope.breadcrumblist[0].title = $rootScope.currentOrganization.label;
                                $rootScope.breadcrumblist[0].link = '#/apps?orgId=' + $rootScope.currentOrganization.id;
                                $rootScope.breadcrumblist[1].title = "People";
                                $rootScope.breadcrumblist[1].link = '#/org/' + $rootScope.currentOrganization.id + '/collaborators';
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

                .state('studio.app.moduleDesigner', {
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

                .state('studio.app.pickLists', {
                    url: '/picklists?:id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/model/picklists/picklists.html',
                            controller: 'pickListsController'
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
                                cdnUrl + 'view/app/model/picklists/pickListsController.js',
                                cdnUrl + 'view/app/model/picklists/pickListsService.js',
                                cdnUrl + 'view/app/model/modules/moduleService.js'
                            ]);
                        }]
                    }
                })                .state('studio.app.relations', {
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

                .state('studio.app.views', {
                    url: '/views?:id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/visualization/views/viewsList.html',
                            controller: 'ViewsController'
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
                                cdnUrl + 'view/app/visualization/views/viewsService.js',
                                cdnUrl + 'view/app/model/modules/moduleService.js',
                                cdnUrl + 'view/app/visualization/views/viewsController.js'
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
                                cdnUrl + 'view/app/templates/emailtemplates/emailTemplatesService.js',
                                cdnUrl + 'view/app/model/modules/moduleService.js'
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

                .state('studio.app.advancedWorkflows', {
                    url: '/advancedWorkflows',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/processautomation/advancedworkflows/advancedWorkflows.html',
                            controller: 'AdvancedWorkflowsController'
                        }
                    },
                    resolve: {
                        advancedWorkflows: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/processautomation/advancedworkflows/advancedWorkflowsController.js',
                                cdnUrl + 'view/app/processautomation/advancedworkflows/advancedWorkflowsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.workflowEditor', {
                    url: '/workflowEditor?:id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/processautomation/advancedworkflows/advancedworkflowEditor.html',
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
                                cdnUrl + 'view/app/processautomation/advancedworkflows/advancedWorkflowEditorController.js',
                                cdnUrl + 'view/app/processautomation/advancedworkflows/advancedWorkflowsService.js',
                                cdnUrl + 'scripts/vendor/bpm/icons.js',
                                cdnUrl + 'scripts/vendor/bpm/BPMN.js',
                                cdnUrl + 'scripts/vendor/bpm/BPMNClasses.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.simpleWorkflows', {
                    url: '/simpleworkflows',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/processautomation/simpleworkflows/simpleWorkflows.html',
                            controller: 'SimpleWorkflowsController'
                        }
                    },
                    resolve: {
                        simpleworkflows: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/processautomation/simpleworkflows/simpleWorkflowsController.js',
                                cdnUrl + 'view/app/processautomation/simpleworkflows/simpleWorkflowsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.processes', {
                    url: '/approvalprocesses',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/processautomation/processes/processes.html',
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
                                cdnUrl + 'view/app/processautomation/processes/processesController.js',
                                cdnUrl + 'view/app/processautomation/processes/processesService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.reports', {
                    url: '/reports',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/visualization/reports/reports.html',
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
                                cdnUrl + 'view/app/visualization/reports/reportsController.js',
                                cdnUrl + 'view/app/visualization/reports/reportsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.dashboards', {
                    url: '/dashboards',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/visualization/dashboards/dashboards.html',
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
                                cdnUrl + 'view/app/visualization/dashboards/dashboardsController.js',
                                cdnUrl + 'view/app/visualization/dashboards/dashboardsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.warehouse', {
                    url: '/warehouse',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/visualization/warehouse/warehouse.html',
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
                                cdnUrl + 'view/app/visualization/warehouse/warehouseController.js',
                                cdnUrl + 'view/app/visualization/warehouse/warehouseService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.bi', {
                    url: '/bi',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/visualization/bi/bi.html',
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
                                cdnUrl + 'view/app/visualization/bi/biController.js',
                                cdnUrl + 'view/app/visualization/bi/biService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.components', {
                    url: '/components',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/customcode/components/components.html',
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
                                cdnUrl + 'view/app/customcode/components/componentsController.js',
                                cdnUrl + 'view/app/customcode/components/componentsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.componentDetail', {
                    url: '/componentDetail?:id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/customcode/components/componentDetail.html',
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
                                cdnUrl + 'view/app/customcode/components/componentDetailController.js',
                                cdnUrl + 'view/app/customcode/components/componentsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.functions', {
                    url: '/functions',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/customcode/functions/functions.html',
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
                                cdnUrl + 'view/app/customcode/functions/functionsController.js',
                                cdnUrl + 'view/app/customcode/functions/functionsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.scripts', {
                    url: '/scripts',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/customcode/scripts/scripts.html',
                            controller: 'ScriptsController'
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
                                cdnUrl + 'view/app/customcode/scripts/scriptsController.js',
                                cdnUrl + 'view/app/customcode/scripts/scriptsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.functionDetail', {
                    url: '/functionDetail?:name',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/customcode/functions/functionDetail.html',
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
                                cdnUrl + 'view/app/customcode/functions/functionDetailController.js',
                                cdnUrl + 'view/app/customcode/functions/functionsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.profiles', {
                    url: '/profiles',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/accesscontrol/profiles/profiles.html',
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
                                cdnUrl + 'view/app/accesscontrol/profiles/profilesController.js',
                                cdnUrl + 'view/app/accesscontrol/profiles/profilesService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.moduleProfileSettings', {
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
                            templateUrl: cdnUrl + 'view/app/accesscontrol/roles/roles.html',
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
                                cdnUrl + 'view/app/accesscontrol/roles/rolesController.js',
                                cdnUrl + 'view/app/accesscontrol/roles/rolesService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.menus', {
                    url: '/menus',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/visualization/menus/menus.html',
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
                                cdnUrl + 'view/app/visualization/menus/menusController.js',
                                cdnUrl + 'view/app/visualization/menus/menusService.js',
                                cdnUrl + 'view/app/accesscontrol/profiles/profilesService.js'
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
                            templateUrl: cdnUrl + 'view/app/deployments/deployment/deployment.html',
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
                                /*cdnUrl + 'view/app/deployments/deployment/deploymentController.js',
                                cdnUrl + 'view/app/deployments/deployment/deploymentService.js'*/
                            ]);
                        }]
                    }
                })

                .state('studio.app.componentsDeployment', {
                    url: '/componentsDeployment',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/manage/security/passwordpolicies/passwordpolicies.html',
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
                                cdnUrl + 'view/app/manage/security/passwordpolicies/passwordpoliciesController.js',
                                cdnUrl + 'view/app/manage/security/passwordpolicies/passwordpoliciesService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.functionsDeployment', {
                    url: '/functionsDeployment',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/deployments/functions/functionsDeployment.html',
                            controller: 'FunctionsDeploymentController'
                        }
                    },
                    resolve: {
                        functionsdeployment: ['$rootScope', '$state', 'app', function ($rootScope, $state, app) {
                            if (!$rootScope.appModules || !$rootScope.appProfiles || !$rootScope.currentApp) {
                                $state.go('studio.app.overview', {
                                    orgId: $rootScope.currentOrgId,
                                    appId: $rootScope.currentAppId
                                });
                            }
                        }],
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/deployments/functions/functionsDeploymentController.js',
                                cdnUrl + 'view/app/deployments/functions/functionsDeploymentService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.appDeployment', {
                    url: '/passwordPolicies',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/manage/security/passwordpolicies/passwordpolicies.html',
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
                                cdnUrl + 'view/app/manage/security/passwordpolicies/passwordpoliciesController.js',
                                cdnUrl + 'view/app/manage/security/passwordpolicies/passwordpoliciesService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.diagnostics', {
                    url: '/diagnostics',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/manage/diagnostics/diagnostics.html',
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
                                cdnUrl + 'view/app/manage/diagnostics/diagnosticsController.js',
                                cdnUrl + 'view/app/manage/diagnostics/diagnosticsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.extensions', {
                    url: '/extensions',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/manage/extensions/extensions.html',
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
                                cdnUrl + 'view/app/manage/extensions/extensionsController.js',
                                cdnUrl + 'view/app/manage/extensions/extensionsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.certificates', {
                    url: '/certificates',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/manage/security/certificates/certificates.html',
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
                                cdnUrl + 'view/app/manage/security/certificates/certificatesController.js',
                                cdnUrl + 'view/app/manage/security/certificates/certificatesService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.passwordPolicies', {
                    url: '/passwordPolicies',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/manage/security/passwordpolicies/passwordpolicies.html',
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
                                cdnUrl + 'view/app/manage/security/passwordpolicies/passwordpoliciesController.js',
                                cdnUrl + 'view/app/manage/security/passwordpolicies/passwordpoliciesService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.auditTrail', {
                    url: '/auditTrail',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/manage/security/audittrail/audittrail.html',
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
                                cdnUrl + 'view/app/manage/security/audittrail/auditTrailController.js',
                                cdnUrl + 'view/app/manage/security/audittrail/auditTrailService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.cors', {
                    url: '/cors',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/manage/security/cors/cors.html',
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
                                cdnUrl + 'view/app/manage/security/cors/corsController.js',
                                cdnUrl + 'view/app/manage/security/cors/corsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.networkAccess', {
                    url: '/networkAccess',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/manage/security/networkaccess/networkaccess.html',
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
                                cdnUrl + 'view/app/manage/security/networkaccess/networkaccessController.js',
                                cdnUrl + 'view/app/manage/security/networkaccess/networkaccessService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.authentication', {
                    url: '/authentication',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/manage/identity/authentication/authentication.html',
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
                                cdnUrl + 'view/app/manage/identity/authentication/authenticationController.js',
                                cdnUrl + 'view/app/manage/identity/authentication/authenticationService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.identity', {
                    url: '/identity',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/manage/identity/identityprovider/identity.html',
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
                                cdnUrl + 'view/app/manage/identity/identityprovider/identityController.js',
                                cdnUrl + 'view/app/manage/identity/identityprovider/identityService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.singleSignOn', {
                    url: '/singleSingOn',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/manage/identity/singlesingon/singleSignOn.html',
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
                                cdnUrl + 'view/app/manage/identity/singlesingon/singleSignOnController.js',
                                cdnUrl + 'view/app/manage/identity/singlesingon/singleSignOnService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.appDetails', {
                    url: '/appDetails',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/manage/appdetails/appDetails.html',
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
                                cdnUrl + 'view/app/manage/appdetails/appDetailsController.js',
                                cdnUrl + 'view/app/manage/appdetails/appDetailsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.appCollaborators', {
                    url: '/appCollaborators',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/manage/collaborators/appCollaborators.html',
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
                                cdnUrl + 'view/app/manage/collaborators/appCollaboratorsController.js',
                                cdnUrl + 'view/app/manage/collaborators/appCollaboratorsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.orgSettings', {
                    url: 'orgsettings',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/organization/orgsettings/orgSettings.html',
                            controller: 'OrgSettingsController'
                        }
                    },
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', 'studio', function ($$animateJs, $ocLazyLoad, studio) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/organization/orgsettings/orgSettingsService.js',
                                cdnUrl + 'view/organization/orgsettings/orgSettingsController.js',
                                cdnUrl + 'view/app/model/modules/moduleService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.notifications', {
                    url: '/notifications',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/manage/notifications/notifications.html',
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
                                cdnUrl + 'view/app/manage/notifications/notificationsController.js',
                                cdnUrl + 'view/app/manage/notifications/notificationsService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.appEmailTemplates', {
                    url: '/appEmailTemplates',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/manage/appemailtemplates/appEmailTemplates.html',
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
                                cdnUrl + 'view/app/manage/appemailtemplates/appEmailTemplatesController.js',
                                cdnUrl + 'view/app/manage/appemailtemplates/appEmailTemplatesService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.moduleActions', {
                    url: '/actionButtons?:id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/visualization/buttons/actionButtons.html',
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
                                cdnUrl + 'view/app/visualization/buttons/actionButtonsController.js',
                                cdnUrl + 'view/app/visualization/buttons/actionButtonsService.js',
                                cdnUrl + 'view/app/model/modules/moduleService.js'
                            ]);
                        }]
                    }
                })

                .state('studio.app.report', {
                    url: '/report?:id',
                    views: {
                        'app': {
                            templateUrl: cdnUrl + 'view/app/visualization/reports/report.html',
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
                                cdnUrl + 'view/app/visualization/reports/reportsService.js',
                                cdnUrl + 'view/app/visualization/reports/reportController.js'
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