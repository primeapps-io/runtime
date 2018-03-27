'use strict';

angular.module('ofisim')

    .controller('AnalyticsController', ['$rootScope', '$scope', '$location', '$filter', '$timeout', '$state', 'ngToast', 'AnalyticsService',
        function ($rootScope, $scope, $location, $filter, $timeout, $state, ngToast, AnalyticsService) {
            $scope.id = $location.search().id;
            $scope.filterPaneEnabled = false;
            $scope.analyticsLoading = true;

            if (!$rootScope.user.hasAnalytics) {
                ngToast.create({ content: $filter('translate')('Common.Forbidden'), className: 'warning' });
                $state.go('app.crm.dashboard');
                return;
            }

            $scope.embedReport = function (analyticsCurrentReport) {
                $scope.loading = true;
                var report;

                if (!analyticsCurrentReport) {
                    if (!$scope.id)
                        report = $scope.analyticsReports[0];
                    else
                        report = $filter('filter')($scope.analyticsReports, { id: parseInt($scope.id) }, true)[0];
                }
                else {
                    report = analyticsCurrentReport;
                }

                $scope.analyticsCurrentReport = report;

                var config = {
                    type: 'report',
                    accessToken: report.access_token,
                    embedUrl: report.embed_url,
                    id: report.report_id,
                    settings: {
                        filterPaneEnabled: false,
                        navContentPaneEnabled: false
                    }
                };

                var reportContainer = angular.element(document.getElementById('report'))[0];
                $scope.reportEmbedded = powerbi.embed(reportContainer, config);

                $scope.reportEmbedded.off('loaded');
                $scope.reportEmbedded.on('loaded', function () {
                    $scope.reportEmbedded.getPages()
                        .then(function (pages) {
                            $timeout(function () {
                                $scope.pages = pages;
                                $scope.currentPage = pages[0];
                                $scope.loading = false;
                                $scope.analyticsLoading = false;
                            });
                        });
                });
            };

            $scope.delete = function (reportId) {
                AnalyticsService.delete(reportId)
                    .then(function () {
                        AnalyticsService.getReports()
                            .then(function (reports) {
                                reports = reports.data;

                                if (!reports || !reports.length)
                                    return;

                                $scope.analyticsReports = reports;
                                $scope.embedReport();
                            });
                    });
            };

            AnalyticsService.getReports()
                .then(function (reports) {
                    reports = reports.data;
                    $scope.analyticsLoading = false;

                    if (!reports || !reports.length)
                        return;

                    $scope.analyticsReports = reports;
                    $scope.embedReport();
                });

            $scope.changePage = function (page) {
                $scope.currentPage = page;
                $scope.reportEmbedded.setPage(page.name);
            };

            $scope.fullScreen = function () {
                $scope.reportEmbedded.fullscreen();
            };

            $scope.enableFilterPane = function (filterPaneEnabled) {
                $scope.reportEmbedded.updateSettings({ filterPaneEnabled: filterPaneEnabled });
                $scope.filterPaneEnabled = filterPaneEnabled;
            };

            $scope.cycle = function (interval) {
                if ($scope.cycling)

                    angular.forEach($scope.pages, function (page) {
                        $timeout(function () {
                            $scope.changePage(page);
                        }, interval);
                    });
            };

            $scope.print = function () {
                $scope.reportEmbedded.print();
            };
        }
    ]);