'use strict';

angular.module('primeapps')

    .controller('ReportsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'ReportsService', 'LayoutService', '$http', 'config', '$interval', '$localStorage',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, ReportsService, LayoutService, $http, config, $interval, $localStorage) {

            $scope.$parent.activeMenuItem = 'reports';
            $rootScope.breadcrumblist[2].title = 'Reports';
            $scope.activePage = 1;
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

            //ReportsService.count()
            //    .then(function (response) {
            //        $scope.pageTotal = response.data;
            //        $scope.changePage(1);
            //    });

            //$scope.changePage = function (page) {
            //    $scope.loading = true;

            //    if (page !== 1) {
            //        var difference = Math.ceil($scope.pageTotal / $scope.requestModel.limit);

            //        if (page > difference) {
            //            if (Math.abs(page - difference) < 1)
            //                --page;
            //            else
            //                page = page - Math.abs(page - Math.ceil($scope.pageTotal / $scope.requestModel.limit))
            //        }
            //    }

            //    $scope.activePage = page;
            //    var requestModel = angular.copy($scope.requestModel);
            //    requestModel.offset = page - 1;

            //    ReportsService.find(requestModel)
            //        .then(function (response) {
            //            $scope.reports = response.data;
            //            $scope.loading = false;
            //        });

            //};

            //$scope.changeOffset = function () {
            //    $scope.changePage($scope.activePage)
            //};


            $scope.openCategoryModal = function () {
                $scope.bindPicklistDragDrop();
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

            $scope.openReportDetail = function (report) {
                $scope.reportModel = {};
                if (report) {
                    $scope.ReportId = report.id;
                    $scope.reportModel.category_id = parseInt(report.category_id);
                } else {
                    $scope.ReportId = null;
                }

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

            $scope.deleteReport = function (id, event) {
                var willDelete =
                    swal({
                        title: "Are you sure?",
                        text: " ",
                        icon: "warning",
                        buttons: ['Cancel', 'Yes'],
                        dangerMode: true
                    }).then(function (value) {
                        if (value) {
                            var elem = angular.element(event.srcElement);
                            angular.element(elem.closest('tr')).addClass('animated-background');
                            ReportsService.deleteReport(id)
                                .then(function () {
                                    $scope.pageTotal--;
                                    //var index = $rootScope.appModules.indexOf(module);
                                    // $rootScope.appModules.splice(index, 1);
                                    $scope.grid.dataSource.read()
                                    angular.element(document.getElementsByClassName('ng-scope animated-background')).remove();
                                    //$scope.changePage($scope.activePage);
                                    toastr.success("Report is deleted successfully.", "Deleted!");

                                })
                                .catch(function () {
                                    angular.element(document.getElementsByClassName('ng-scope animated-background')).removeClass('animated-background');
                                });

                        }
                    });
            };

            $scope.addTemplateCategory = function () {
                var category = {
                    "name_tr": '',
                    "name_en": '',
                    order: 0,
                    edit: true
                };
                
                if (!angular.isArray($rootScope.reportCategory)) {
                    $rootScope.reportCategory = [];
                }
                $rootScope.reportCategory.push(category);
            };

            $scope.cancelCategory = function (index) {
                $rootScope.reportCategory.splice(index, 1);
            };

            $scope.saveCategory = function (category) {
                category.saving = true;

                var categoryModel = {
                    name_tr: category.name_en,
                    name_en: category.name_en,
                    id: category.id
                };

                if (!category.id) {
                    ReportsService.createCategory(categoryModel).then(function (result) {
                        var resultCategory = result.data;
                        category.id = resultCategory.id;
                        toastr.success("Report category  is saved successfully.");
                        category.saving = false;
                        category.edit = false;
                    });
                } else {
                    ReportsService.updateCategory(categoryModel).then(function (result) {
                        var resultCategory = result.data;
                        category.id = resultCategory.id;
                        toastr.success("Report category  is saved successfully.");
                        category.saving = false;
                        category.edit = false;
                    });
                }
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
                            ReportsService.deleteCategory(category.id).then(function () {
                                category.deleted = false;
                                $rootScope.reportCategory.splice(index, 1);
                                toastr.success("Report category  is deleted successfully.", "Deleted!");

                            });
                        } else {
                            category.deleted = false;
                            $scope.$apply(function () {
                            });

                        }


                    });

            };

            // Drag & Drop For Items list
            $scope.bindPicklistDragDrop = function () {
                $timeout(function () {
                    if ($scope.drakePicklist) {
                        $scope.drakePicklist.destroy();
                        $scope.drakePicklist = null;
                    }

                    var picklistContainer = document.querySelector('#picklistContainer');
                    var picklistOptionContainer = document.querySelector('#picklistOptionContainer');
                    var rightTopBar = document.getElementById('rightTopBar');
                    var rightBottomBar = document.getElementById('rightBottomBar');
                    var timer;

                    $scope.drakePicklist = dragularService([picklistContainer], {
                        scope: $scope,
                        containersModel: [$scope.reportCategory],
                        classes: {
                            mirror: 'gu-mirror-option pickitemcopy',
                            transit: 'gu-transit-option'
                        },
                        lockY: true,
                        moves: function (el, container, handle) {
                            return handle.classList.contains('option-handle');
                        }

                    });

                    registerEvents(rightTopBar, picklistOptionContainer, -5);
                    registerEvents(rightBottomBar, picklistOptionContainer, 5);

                    function registerEvents(bar, container, inc, speed) {
                        if (!speed) {
                            speed = 10;
                        }

                        angular.element(bar).on('dragularenter', function () {
                            container.scrollTop += inc;

                            timer = $interval(function moveScroll() {
                                container.scrollTop += inc;
                            }, speed);
                        });

                        angular.element(bar).on('dragularleave dragularrelease', function () {
                            $interval.cancel(timer);
                        });
                    }
                }, 100);
            };

            $scope.goUrl = function (report) {
                var selection = window.getSelection();
                if (selection.toString().length === 0) {
                    $scope.openReportDetail(report);
                }
            };

            //For Kendo UI
            var accessToken = $localStorage.read('access_token');

            $scope.mainGridOptions = {
                dataSource: {
                    type: "odata-v4",
                    page: 1,
                    pageSize: 10,
                    serverPaging: true,
                    serverFiltering: true,
                    serverSorting: true,
                    transport: {
                        read: {
                            url: "/api/report/find",
                            type: 'GET',
                            dataType: "json",
                            beforeSend: function (req) {
                                req.setRequestHeader('Authorization', 'Bearer ' + accessToken);
                                req.setRequestHeader('X-App-Id', $rootScope.currentAppId);
                                req.setRequestHeader('X-Organization-Id', $rootScope.currentOrgId);
                            }
                        }
                    },
                    schema: {
                        data: "items",
                        total: "count",
                        model: {
                            id: "id",
                            fields: {
                                Name: { type: "string" },
                                Category: { type: "string" },
                                ReportType: { type: "string" }
                            }
                        }
                    }
                },
                scrollable: false,
                persistSelection: true,
                sortable: true,
                filterable: {
                    extra: false
                },
                rowTemplate: function (report) {
                    var trTemp = '<tr ng-click="goUrl(dataItem)">';
                    trTemp += '<td>' + report.name_en + '</td>';
                    trTemp += '<td>' + report.category.name_en + '</td>';
                    trTemp += '<td>' + report.report_type + '</td>';
                    trTemp += '<td ng-click="$event.stopPropagation();"> <button ng-click="$event.stopPropagation(); deleteReport(dataItem.id, $event);" type="button" class="action-button2-delete"><i class="fas fa-trash"></i></button></td></tr>';
                    return trTemp;
                },
                pageable: {
                    refresh: true,
                    pageSize: 10,
                    pageSizes: [10, 25, 50, 100],
                    buttonCount: 5,
                    info: true,
                },
                columns: [

                    {
                        field: 'Name',
                        title: $filter('translate')('Setup.Report.ReportName'),
                    },

                    {
                        field: 'Category.Name',
                        title: $filter('translate')('Setup.Report.ReportCategory'),
                    },
                    {
                        field: 'ReportType',
                        title: 'Type',
                        values: [
                            { text: 'Summary', value: 'Summary' },
                            { text: 'Tabular', value: 'Tabular' },
                            { text: 'Single', value: 'Single' }
                        ]
                    },
                    {
                        field: '',
                        title: '',
                        width: "90px"
                    }]
            };

        }
    ]);
