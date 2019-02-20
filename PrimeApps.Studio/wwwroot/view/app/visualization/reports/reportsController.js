'use strict';

angular.module('primeapps')

    .controller('ReportsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'ReportsService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, ReportsService, LayoutService, $http, config) {

            $scope.$parent.activeMenuItem = 'reports';
            $rootScope.breadcrumblist[2].title = 'Reports';

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }

            };

            $scope.generator(10);


            $scope.reports = [];
            $scope.loading = true;
            $scope.requestModel = {
                limit: "10",
                offset: 0
            };

            ReportsService.getAllCategory().then(function (result) {
                $rootScope.reportCategory = result.data;
            });


            ReportsService.count().then(function (response) {
                $scope.pageTotal = response.data;
            });

            ReportsService.find($scope.requestModel).then(function (response) {
                $scope.reports = response.data;
                $scope.loading = false;
            });

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;
                ReportsService.find(requestModel).then(function (response) {
                    $scope.reports = response.data;
                    $scope.loading = false;
                });

            };

            $scope.changeOffset = function () {
                $scope.changePage(1)
            };


            $scope.openCategoryModal = function () {

                $scope.categoryModal = $scope.categoryModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/visualization/reports/categoryModal.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.categoryModal.$promise.then(function () {
                    $scope.categoryModal.show();
                });
            };

            $scope.openReportDetail = function () {
                $scope.reportModal = $scope.reportModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/visualization/reports/report.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false,
                    resolve: {
                        plugins: ['$$animateJs', '$ocLazyLoad', function ($$animateJs, $ocLazyLoad) {
                            return $ocLazyLoad.load([
                                cdnUrl + 'view/app/visualization/reports/reportsService.js',
                                cdnUrl + 'view/app/visualization/reports/reportController.js'
                            ]);
                        }]
                    },
                    controller: 'ReportController'

                });

                $scope.reportModal.$promise.then(function () {
                    $scope.reportModal.show();
                });
            }

            $scope.saveCategory = function (category) {

                category.saving = true;

                $timeout(function () {
                    category.saving = false;
                    category.edit = false;
                }, 2000);
            };

            $scope.deleteCategory = function (index, category) {
                category.deleted = true;

                var willDelete =
                    swal({
                        title: "Are you sure?",
                        text: " ",
                        icon: "warning",
                        buttons: ['Cancel', 'Yes'],
                        dangerMode: true
                    }).then(function (value) {
                        if (value) {

                            $timeout(function () {
                                category.deleted = false;
                                $rootScope.reportCategory.splice(index, 1);

                                $rootScope.$apply(function () {
                                });

                            }, 1000)

                        } else {
                            category.deleted = false;
                            $scope.$apply(function () {
                            });

                        }


                    });

            }

        }
    ]);
