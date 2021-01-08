'use strict';


function getCode(url, callback) {
    var xhr = new XMLHttpRequest();
    xhr.open('GET', url, false);
    xhr.onload = function () {
        if (this.status === 200) {
            return callback(xhr.responseText)
        }
    };
    xhr.send({});
}

var runController = function (code, callback) {
    if (!angular.isObject(code)) {
        getCode(code.url, function (result) {
            eval(result);
            return callback(result)
        });
    } else {
        eval(code);
        return callback(true)
    }
};

angular.module('primeapps')

    .controller('DashboardController', ['$rootScope', '$scope', 'helper', '$filter', '$cache', 'DashboardService', 'ModuleService', '$window', '$state', '$mdDialog', '$timeout', 'HelpService', '$sce', '$mdSidenav', 'mdToast', 'operations',
        function ($rootScope, $scope, helper, $filter, $cache, DashboardService, ModuleService, $window, $state, $mdDialog, $timeout, HelpService, $sce, $mdSidenav, mdToast, operations) {

            //We couldn't use mdToast library in the interceptors.js so we have to show error messages'  in the dashboardController.js
            $rootScope.$on('error', function (event, data) {
                mdToast.warning($filter('translate')(data));
            });

            //we have to check currentPath because sometimes user can get warrning from the other pages. When user get warning from the other pages we change the state to 'app.dashboard'
            if ($scope.currentPath.indexOf('/dashboard') < 0) {
                if ($scope.menu) {
                    var item = $filter('filter')($scope.menu, {route: 'dashboard'}, true)[0];
                    $scope.openSubMenu(item, $scope.menu);
                } else
                    $scope.$parent.currentPath = '/app/dashboard';
            }
            $scope.hasPermission = helper.hasPermission;
            $scope.isFullScreen = true;
            $scope.loading = true;
            $scope.loadingchanges = false;
            $scope.disableSaveBtn = true;
            $scope.showNewDashboardSetting = $filter('filter')($rootScope.moduleSettings, {key: 'new_dashboard'}, true)[0];//TODO: Delete after new dashboard development finished
            $rootScope.sideinclude = false;
            $rootScope.breadcrumblist = [];

            if (!$rootScope.dashboardHelpSide) {
                HelpService.getByType('sidemodal', null, '/app/dashboard')
                    .then(function (response) {
                        $rootScope.dashboardHelpSide = response.data;
                    });
            }

            $window.scrollTo(0, 0);

            var startPageLower = $rootScope.user.profile.start_page.toLowerCase();

            if (startPageLower !== 'dashboard') {
                window.location = '#/app/' + startPageLower;
                return;
            }

            $scope.colorPaletOptions = {
                columns: 6,
                palette: [
                    "#D24D57", "#BE90D4", "#5AABE3", "#87D37C", "#F4D03E", "#B8BEC2",
                    "#DC3023", "#8E44AD", "#19B5FE", "#25A65B", "#FFB61E", "#959EA4",
                    "#C3272B", "#763668", "#1F4688", "#006442", "#CA6924", "#4D5C66",
                ]
            };

            $scope.sideModalLeft = function () {
                $rootScope.buildToggler('sideModal', 'view/app/dashboard/dashboardOrderChange.html', $scope);
            };

            $scope.showConfirm = function (ev, id) {
                // Appending dialog to document.body to cover sidenav in docs app
                var confirm = $mdDialog.confirm()
                    .title($filter('translate')('Common.AreYouSure'))
                    .targetEvent(ev)
                    .ok($filter('translate')('Common.Yes'))
                    .cancel($filter('translate')('Common.No'));

                $mdDialog.show(confirm).then(function () {
                    $scope.dashletDelete(id);
                }, function () {
                    $scope.status = 'You decided to keep your debt.';
                });
            };


            $scope.endHandler = function (e) {
                var sortable = e.sender;
                // prevent the sortable from modifying the DOM
                e.sender.draggable.dropped = true;
                e.preventDefault();

                // update the model and let Angular update the DOM
                $timeout(function () {
                    $scope.$apply(function () {
                        $scope.dashlets.splice(
                            e.newIndex, 0, $scope.dashlets.splice(e.oldIndex, 1)[0]
                        );
                    });
                });
                $scope.disableSaveBtn = false;
            };

            $rootScope.sortableOptions = {
                placeholder: function (element) {
                    return element
                        .clone()
                        .addClass("sortable-list-placeholder")
                        .text(element.innerText);
                },
                hint: function (element) {
                    return element
                        .clone()
                        .addClass("sortable-list-hint");
                },
                cursorOffset: {
                    top: -10,
                    left: 20
                }

            };

            $scope.showFullScreen = function (dashletId) {
                if (!$scope.fullScreenDashlet) {
                    $scope.fullScreenDashlet = dashletId;
                } else {
                    $scope.fullScreenDashlet = null;
                }
                // tr
                //$scope.isFullScreen = true;
            };
            $scope.openMenu = function () {
                // $scope.dashletMenu=true
            };
            $scope.DashletTypes = [
                {label: $filter('translate')('Dashboard.Chart'), name: 'chart'},
                {label: $filter('translate')('Dashboard.Widget'), name: 'widget'}
            ];

            var colomn = $filter('translate')('Dashboard.Column');
            $scope.dashletWidths = [
                {label: '1 ' + colomn, value: 3},
                {label: '2 ' + colomn, value: 6},
                {label: '3 ' + colomn, value: 9},
                {label: '4 ' + colomn, value: 12}
            ];
            $scope.dashletHeights = [
                {label: '300 px', value: 330},
                {label: '400 px', value: 430},
                {label: '500 px', value: 530},
                {label: '600 px', value: 630}
            ];

            $scope.getUser = function (id) {
                var user = $filter('filter')($rootScope.users, {'id': id}, true)[0];
                if (user.full_name)
                    return user.full_name;
                return id;
            };

            $scope.showComponent = [];
            var componentsListe = [];
            if (components) {
                var com = JSON.parse(components);
                for (var i = 0; i < com.length; i++) {
                    componentsListe['component-' + com[i].Id] = com[i];
                }
            }

            $scope.loadDashboard = function () {

                var setDashboard = function () {
                    $scope.activeDashboard = $cache.get('activeDashboard');
                    DashboardService.getDashlets($scope.activeDashboard.id)
                        .then(function (dashlets) {

                            dashlets = dashlets.data;
                            $rootScope.processLanguages(dashlets);

                            for (var i = 0; i < dashlets.length; i++) {
                                var dashlet = dashlets[i];

                                if (dashlet.dashlet_type === 'component') {

                                    if (!$cache.get('component-' + dashlet.id)) {
                                        runController(false, function (code) {
                                            $scope.showComponent[dashlet.id] = true;

                                            $cache.put($cache.get('component-' + id), code);
                                        });
                                    } else {
                                        runController($cache.get($scope.showComponent[dashlet.id]), function (code) {
                                            $scope.componetiGoster = true;
                                        });
                                    }
                                }

                                if (dashlet.dashlet_type === 'chart') {
                                    dashlet.chart_item.config = {
                                        dataEmptyMessage: $filter('translate')('Dashboard.ChartEmptyMessage')
                                    };

                                    dashlet.chart_item.chart.showPercentValues = '1';
                                    dashlet.chart_item.chart.showPercentInTooltip = '0';
                                    dashlet.chart_item.chart.animateClockwise = '1';
                                    dashlet.chart_item.chart.enableMultiSlicing = '0';
                                    dashlet.chart_item.chart.isHollow = '0';
                                    dashlet.chart_item.chart.is2D = '0';
                                    dashlet.chart_item.chart.formatNumberScale = '0';
                                    dashlet.chart_item.chart['xaxisname'] = dashlet.chart_item.chart.languages[$rootScope.globalization.Label]['xaxis_name'];
                                    dashlet.chart_item.chart['yaxisname'] = dashlet.chart_item.chart.languages[$rootScope.globalization.Label]['yaxis_name'];
                                    dashlet.chart_item.chart['caption'] = dashlet.chart_item.chart.languages[$rootScope.globalization.Label]['caption'];
                                    $rootScope.languageStringify(dashlet.chart_item.chart);

                                    if ($scope.locale === 'tr') {
                                        dashlet.chart_item.chart.decimalSeparator = ',';
                                        dashlet.chart_item.chart.thousandSeparator = '.';
                                    }

                                    var module = $filter('filter')($rootScope.modules, {id: dashlet.chart_item.chart.report_module_id}, true)[0];

                                    if (module) {
                                        var field;

                                        if (dashlet.chart_item.chart.report_aggregation_field.indexOf('.') < 0) {
                                            field = $filter('filter')(module.fields, {name: dashlet.chart_item.chart.report_aggregation_field}, true)[0];
                                        } else {
                                            var aggregationFieldParts = dashlet.chart_item.chart.report_aggregation_field.split('.');
                                            var lookupModule = $filter('filter')($rootScope.modules, {name: aggregationFieldParts[1]}, true)[0];
                                            field = $filter('filter')(lookupModule.fields, {name: aggregationFieldParts[2]}, true)[0];
                                        }

                                        if (field && field.data_type === 'currency')
                                            dashlet.chart_item.chart.numberPrefix = $rootScope.currencySymbol;

                                        if (field && (field.data_type === 'currency' || field.data_type === 'number_decimal'))
                                            dashlet.chart_item.chart.forceDecimals = '1';
                                    }
                                }
                            }

                            $scope.dashlets = dashlets;
                            $cache.put('dashlets', dashlets);

                        })
                        .finally(function () {
                            $scope.loading = false;
                        });
                };


                if ($scope.showNewDashboardSetting === undefined || $scope.showNewDashboardSetting === null || $scope.showNewDashboardSetting.value === 'true') {
                    $scope.showNewDashboard = true;
                    setDashboard();
                } else//TODO: Delete after new dashboard development finished
                    // setOldDashboard();

                    $scope.getSummaryJsonValue = function (data) {
                        var obj = angular.fromJson(data);
                        return obj.x;
                    };

                //Get all picklists
                for (var i = 0; i < $rootScope.modules.length; i++) {
                    ModuleService.getPicklists($rootScope.modules[i]);
                }

                $scope.$on('sample-data-removed', function (event, args) {
                    setDashboard();
                });

                $scope.goDetail = function (viewId, moduleName) {
                    if (viewId !== 0) {
                        window.location = "#/app/modules/" + moduleName + "?viewid=" + viewId;
                    }
                };


            };

            var dashletsCache = $cache.get('dashlets');
            var activeDashboard = $cache.get('activeDashboard');
            var dashboards = $cache.get('dashboards');
            var dashboardprofile = $cache.get('dashboardprofile');

            if (dashletsCache) {
                $scope.loading = false;
                $scope.dashlets = dashletsCache;
            }

            if (activeDashboard && dashboards) {
                $scope.dashboards = dashboards;
                $rootScope.processLanguages($scope.dashboards);
                $scope.activeDashboard = activeDashboard;
                $scope.dashboardprofile = dashboardprofile;
                $scope.loadDashboard();
            } else {
                DashboardService.getDashboards().then(function (result) {
                    $scope.dashboards = result.data;

                    $rootScope.processLanguages($scope.dashboards);

                    $scope.activeDashboard = $filter('filter')($scope.dashboards, {
                        sharing_type: 'me',
                        user_id: $rootScope.user.id
                    }, true)[0];

                    if (!$scope.activeDashboard) {

                        if ($rootScope.user.profile.has_admin_rights) {
                            $scope.activeDashboard = $filter('filter')($scope.dashboards, {
                                sharing_type: 'everybody'
                            }, true)[0];
                        } else {
                            $scope.activeDashboard = $filter('filter')($scope.dashboards, {
                                sharing_type: 'profile',
                                profile_id: $rootScope.user.profile.id
                            }, true)[0];
                        }

                        if (!$scope.activeDashboard) {
                            $scope.activeDashboard = $filter('filter')($scope.dashboards, {sharing_type: 'everybody'}, true)[0];
                        }
                    }
                    $scope.dashboardprofile = [];
                    angular.forEach($rootScope.profiles, function (item) {
                        var profil = $filter('filter')($scope.dashboards, {
                            sharing_type: 'profile',
                            profile_id: item.id
                        }, true)[0];
                        if (!profil) {
                            item.name = $rootScope.getLanguageValue(item.languages, 'name');
                            $scope.dashboardprofile.push(item);
                        }

                    });
                    $cache.put('activeDashboard', $scope.activeDashboard);
                    $cache.put('dashboards', $scope.dashboards);
                    $cache.put('dashboardprofile', $scope.dashboardprofile);
                    $scope.loadDashboard();

                });

            }

            $scope.changeDashboard = function (dashhboard) {
                $scope.loading = true;
                $cache.put('activeDashboard', dashhboard);
                $scope.loadDashboard();
            };

            $scope.hide = function () {
                $mdDialog.hide();
            };

            $scope.cancel = function () {
                $mdDialog.cancel();
            };

            $scope.dashboardformModal = function (ev, dashboard) {

                var languages = {};
                languages[$rootScope.globalization.Label] = {name: '', description: ''};

                dashboard ? $scope.currentDashboard = angular.copy(dashboard) : $scope.currentDashboard = {languages: languages};

                var parentEl = angular.element(document.body);
                $mdDialog.show({
                    parent: parentEl,
                    templateUrl: 'view/app/dashboard/formModal.html',
                    clickOutsideToClose: true,
                    targetEvent: ev,
                    scope: $scope,
                    preserveScope: true
                });
            };


            $scope.cancelChangeOrder = function () {
                $scope.dashlets = $scope.currentDashlet;
            };

            $scope.saveDashboard = function (dashboardForm, event) {
                event.preventDefault();

                if (dashboardForm.$submitted && dashboardForm.$valid) {
                    {
                        var dashboardModel = {
                            languages: {}
                        };

                        dashboardModel.languages[$rootScope.globalization.Label] = {
                            name: $rootScope.getLanguageValue($scope.currentDashboard.languages, 'name'),
                            description: $rootScope.getLanguageValue($scope.currentDashboard.languages, 'description')
                        };

                        $rootScope.languageStringify(dashboardModel);

                        if (!$scope.currentDashboard.id) {

                            dashboardModel.profile_id = $scope.currentDashboard.profile_id;
                            dashboardModel.sharing_type = 3;
                            $scope.loading = true;
                            var activeDashboard = angular.copy($scope.activeDashboard);
                            DashboardService.createDashbord(dashboardModel).then(function (result) {
                                $scope.hide();
                                $cache.remove('dashlets');
                                $cache.remove('activeDashboard');
                                $cache.remove('dashboards');
                                $cache.remove('dashboardprofile');
                                $state.reload();
                                $mdDialog.cancel();
                                mdToast.success($filter('translate')('Dashboard.DashboardSaveSucces'));

                            });
                        } else {
                            dashboardModel.id = $scope.currentDashboard.id;
                            DashboardService.updateDashboard(dashboardModel).then(function (response) {
                                for (var i = 0; i < $scope.dashboards.length; i++) {
                                    if ($scope.dashboards[i].id === response.data.id) {
                                        $rootScope.processLanguage(response.data);
                                        $scope.dashboards[i] = response.data;
                                    }
                                }

                                $mdDialog.cancel();
                                mdToast.success($filter('translate')('Dashboard.DashboardSaveSucces'));
                            })
                        }
                    }
                } else {
                    if (!dashboardForm.dash.$valid)
                        mdToast.error($filter('translate')('Dashboard.ProfileRequired'));
                }
            };
            $scope.saveDashlet = function (dashletFormModal, event) {
                event.preventDefault();

                if ($scope.validator.validate()) {
                    var dashletModel = {
                        dashlet_type: $scope.currentDashlet.dashlet_type,
                        dashboard_id: $scope.activeDashboard.id,
                        y_tile_length: $scope.currentDashlet.y_tile_length,
                        x_tile_height: $scope.currentDashlet.x_tile_height,
                        languages: {}
                    };

                    dashletModel.languages[$rootScope.globalization.Label] = {
                        name: $rootScope.getLanguageValue($scope.currentDashlet.languages, 'name')
                    };

                    if ($scope.currentDashlet.dashlet_type === 'chart') {
                        dashletModel.chart_id = $scope.currentDashlet.board;
                    } else {
                        if (!$scope.currentDashlet.dataSource) {
                            mdToast.error($filter('translate')('Module.RequiredError'));
                            return
                        }

                        dashletModel.widget_id = $scope.currentDashlet.board;
                        dashletModel.y_tile_length = 3;
                        dashletModel.x_tile_height = 150;
                        dashletModel.view_id = $scope.currentDashlet.view_id;
                        dashletModel.color = $scope.currentDashlet.color;
                        dashletModel.icon = $scope.currentDashlet.icon;

                    }
                    $scope.hide();
                    $scope.loading = true;
                    $rootScope.languageStringify(dashletModel);
                    if (!$scope.currentDashlet.id) {
                        dashletModel.order = $scope.dashlets ? $scope.dashlets.length : 0;
                        DashboardService.createDashlet(dashletModel).then(function (result) {
                            $scope.loadDashboard();
                            mdToast.success($filter('translate')('Dashboard.DashletSaveSucces'));

                        });
                    } else {
                        DashboardService.dashletUpdate($scope.currentDashlet.id, dashletModel).then(function (result) {
                            $scope.loadDashboard();
                            mdToast.success($filter('translate')('Dashboard.DashletUpdateSucces'));
                        });
                    }
                } else {
                    mdToast.error($filter('translate')('Module.RequiredError'));
                }
            };
            $scope.dashletOrderSave = function () {
                $scope.loadingchanges = true;
                $scope.sideModalLeft();
                DashboardService.dashletOrderChange($scope.dashlets).then(function (result) {
                    $scope.loadDashboard();
                    $scope.loadingchanges = false;
                    mdToast.success($filter('translate')('Dashboard.DashletUpdateSucces'));
                    $rootScope.closeSide('sideModal');

                });
                $scope.disableSaveBtn = true;
            };


            $scope.openNewDashlet = function (ev, dashlet) {

                $scope.currentDashlet = {
                    languages: {}
                };

                $scope.currentDashlet.languages[$rootScope.globalization.Label] = {
                    name: '',
                        description: ''
                };

                if (dashlet) {
                    $scope.currentDashlet = angular.copy(dashlet);
                    if (dashlet.chart_item) {
                        $scope.currentDashlet.name = $scope.currentDashlet.chart_item.chart.caption;
                        $scope.currentDashlet.board = $scope.currentDashlet.chart_item.chart.id;
                    } else {
                        //$scope.currentDashlet.name = $scope.currentDashlet.name;
                        $scope.currentDashlet.board = $scope.currentDashlet.widget.id;
                        if ($scope.currentDashlet.widget.view_id) {
                            $scope.currentDashlet.dataSource = 'view';
                            DashboardService.getView($scope.currentDashlet.widget.view_id).then(function (result) {
                                $scope.currentDashlet.module_id = result.data.module_id;
                                $scope.setViews();
                                $scope.currentDashlet.view_id = $scope.currentDashlet.widget.view_id;
                                $scope.currentDashlet.color = $scope.currentDashlet.widget.color;
                                $scope.currentDashlet.icon = $scope.currentDashlet.widget.icon;
                            });
                        } else {
                            $scope.currentDashlet.dataSource = 'report';
                        }
                    }
                    $scope.changeDashletType();
                }


                var parentEl = angular.element(document.body);
                $mdDialog.show({
                    parent: parentEl,
                    templateUrl: 'view/app/dashboard/formModalDashlet.html',
                    clickOutsideToClose: true,
                    targetEvent: ev,
                    scope: $scope,
                    preserveScope: true
                });
            };

            $scope.changeDashletType = function () {

                if ($scope.currentDashlet.dashlet_type && ($scope.currentDashlet.dashlet_type.name === 'chart' || $scope.currentDashlet.dashlet_type === 'chart')) {
                    DashboardService.getCharts().then(function (result) {
                        $scope.boards = result.data;
                        $rootScope.processLanguages($scope.boards);
                        $scope.boardLabel = $scope.DashletTypes[0].label;
                    });
                } else {
                    DashboardService.getWidgets().then(function (result) {
                        $scope.boards = result.data;
                        $rootScope.processLanguages($scope.boards);
                        $scope.boardLabel = $filter('translate')('Report.Single');
                    });
                }
            };

            $scope.selectModule = function () {
                $scope.setViews();
            };

            $scope.setViews = function () {
                DashboardService.getViews($scope.currentDashlet.module_id).then(function (result) {
                    $scope.views = result.data;
                    $rootScope.processLanguages($scope.views);
                });
            };

            $scope.dashletDelete = function (id) {
                DashboardService.dashletDelete(id).then(function (result) {
                    $scope.loadDashboard();
                    mdToast.success($filter('translate')('Dashboard.DashletDeletedSucces'));
                });
            };

            $scope.changeDashletMode = function () {
                $scope.dashletMode = $scope.dashletMode !== true;
            };
            $scope.changeView = function () {
                const res = $filter('filter')($scope.views, {id: $scope.currentDashlet.view_id}, true)[0];
                $scope.currentDashlet.languages[$rootScope.globalization.Label]['name'] = res ? $rootScope.getLanguageValue(res.languages, 'label') : undefined;
            };
            $scope.changeBoard = function () {
                const res = $filter('filter')($scope.boards, {id: $scope.currentDashlet.board}, true)[0];
                $scope.currentDashlet.languages[$rootScope.globalization.Label]['name'] = res ? $rootScope.getLanguageValue(res.languages, 'name') : undefined;
            };

            if (typeof Tawk_API !== 'undefined') {
                Tawk_API.visitor = {
                    name: $rootScope.user.full_name,
                    email: $rootScope.user.email
                };
            }

            $scope.modulesOpt = $filter('filter')($scope.modules, function (item) {
                return item.name !== 'roles' && item.name !== 'users' && item.name !== 'profiles' && item.system_type !== "component" && helper.hasPermission(item.name, operations.read);
            });

        }
    ]);
