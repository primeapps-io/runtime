'use strict';

angular.module('primeapps')

    .controller('ModuleController', ['$rootScope', '$scope', '$filter', '$state', '$dropdown', '$modal', 'helper', 'ModuleService', '$cache', 'LayoutService',
        function ($rootScope, $scope, $filter, $state, $dropdown, $modal, helper, ModuleService, $cache, LayoutService) {

            //$scope.$parent.menuTopTitle = "Models";

            $scope.$parent.activeMenuItem = 'modules';

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }

            };

            $scope.generator(10);
            $rootScope.breadcrumblist[2].title = 'Modules';

            $scope.modules = [];
            $scope.loading = true;
            $scope.requestModel = {
                limit: "10",
                offset: 0
            };

            ModuleService.count().then(function (response) {
                $scope.pageTotal = response.data;
                $rootScope.appModules.length = response.data;
                console.log($rootScope)
            });

            ModuleService.find($scope.requestModel).then(function (response) {
                $scope.modules = response.data;
                $scope.loading = false;
            });

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;
                ModuleService.find(requestModel).then(function (response) {
                    $scope.modules = response.data;
                    $scope.loading = false;
                });

            };

            $scope.changeOffset = function () {
                $scope.changePage(1)
            };


            // $scope.showDeleteForm = function (moduleId) {
            //     $scope.selectedModuleId = moduleId;
            //
            //     $scope.deleteModal = $scope.deleteModal || $modal({
            //         scope: $scope,
            //         template: 'view/app/model/modules/deleteForm.html',
            //         animation: 'am-fade-and-slide-right',
            //         backdrop: 'static',
            //         show: false
            //     });
            //
            //     $scope.deleteModal.$promise.then(function () {
            //         $scope.deleteModal.show();
            //     });
            // };

            $scope.delete = function (module) {
                var willDelete =
                    swal({
                        title: "Are you sure?",
                        text: " ",
                        icon: "warning",
                        buttons: ['Cancel', 'Yes'],
                        dangerMode: true
                    }).then(function (value) {
                        if (value) {
                            $scope.loading = true;
                            ModuleService.delete(module.id)
                                .then(function () {
                                    $scope.pageTotal--;
                                    $rootScope.appModules.length = $scope.pageTotal;
                                    $scope.changeOffset();
                                    $scope.loading = false;
                                    toastr.success("Module is deleted successfully.", "Deleted!");

                                })
                                .catch(function () {

                                });
                        }
                    });
            };
        }
    ]);