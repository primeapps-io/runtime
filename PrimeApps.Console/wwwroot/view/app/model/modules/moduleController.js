'use strict';

angular.module('primeapps')

    .controller('ModuleController', ['$rootScope', '$scope', '$filter', '$state', 'ngToast', '$dropdown', '$modal', 'helper', 'ModuleService', '$cache', 'LayoutService',
        function ($rootScope, $scope, $filter, $state, ngToast, $dropdown, $modal, helper, ModuleService, $cache, LayoutService) {

            $scope.$parent.menuTopTitle = "Models";
            $scope.$parent.activeMenu = 'model';
            $scope.$parent.activeMenuItem = 'modules';

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }

            };

            $scope.generator(10);

            $rootScope.breadcrumblist[0].link = '#/apps?organizationId=' + $rootScope.currentOrganization.id;
            $rootScope.breadcrumblist[1].link = '#/org/' + $rootScope.currentOrganization.id + '/app/' + $rootScope.appId + '/overview';
            $rootScope.breadcrumblist[2].title = 'Modules';

            $scope.modules = [];
            $scope.loading = true;
            $scope.requestModel = {
                limit: "10",
                offset: 0
            };

            ModuleService.count().then(function (response) {
                $scope.pageTotal = response.data;
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


            $scope.showDeleteForm = function (moduleId) {
                $scope.selectedModuleId = moduleId;

                $scope.deleteModal = $scope.deleteModal || $modal({
                    scope: $scope,
                    template: 'view/setup/modules/deleteForm.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.deleteModal.$promise.then(function () {
                    $scope.deleteModal.show();
                });
            };

            $scope.delete = function () {
                $scope.deleting = true;

                ModuleService.delete($scope.selectedModuleId)
                    .then(function () {
                        $scope.changePage(1);
                    })
                    .catch(function () {
                        $scope.deleteModal.hide();
                    });
            }
        }
    ]);