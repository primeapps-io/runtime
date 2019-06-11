'use strict';

angular.module('primeapps')
    .controller('ModuleController',
        [
            '$rootScope', '$scope', '$filter', '$state', '$dropdown', '$modal', 'helper', 'ModuleService', '$cache',
            'LayoutService',
            function ($rootScope,
                $scope,
                $filter,
                $state,
                $dropdown,
                $modal,
                helper,
                ModuleService,
                $cache,
                LayoutService) {
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

                $scope.activePage = 1;

                ModuleService.count()
                    .then(function (response) {
                        $scope.pageTotal = response.data;
                        $scope.changePage(1);
                    });

                $scope.changePage = function (page) {
                    $scope.loading = true;

                    if (page !== 1) {
                        var difference = Math.ceil($scope.pageTotal / $scope.requestModel.limit);

                        if (page > difference) {
                            if (Math.abs(page - difference) < 1)
                                --page;
                            else
                                page = page - Math.abs(page - Math.ceil($scope.pageTotal / $scope.requestModel.limit))
                        }
                    }

                    $scope.activePage = page;
                    var requestModel = angular.copy($scope.requestModel);
                    requestModel.offset = page - 1;

                    ModuleService.find(requestModel)
                        .then(function (response) {
                            $scope.modules = response.data;
                            $scope.loading = false;
                        });

                };

                $scope.changeOffset = function () {
                    $scope.changePage($scope.activePage);
                };

                $scope.delete = function (module, event) {
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
                                ModuleService.delete(module.id)
                                    .then(function () {
                                        $scope.pageTotal--;
                                        var index = $rootScope.appModules.indexOf(module);
                                        $rootScope.appModules.splice(index, 1);

                                        angular.element(document.getElementsByClassName('ng-scope animated-background'))
                                            .remove();
                                        $scope.changePage($scope.activePage);
                                        toastr.success("Module is deleted successfully.", "Deleted!");

                                    })
                                    .catch(function () {
                                        angular.element(document.getElementsByClassName('ng-scope animated-background'))
                                            .removeClass('animated-background');
                                    });

                            }
                        });
                };

                $scope.showEditModal = function (moduleId) {
                    $scope.modalLoading = true;
                    $scope.editModal = $scope.editModal ||
                        $modal({
                            scope: $scope,
                            templateUrl: 'view/app/model/modules/editForm.html',
                            animation: 'am-fade-and-slide-right',
                            backdrop: 'static',
                            show: false
                        });
                    $scope.icons = ModuleService.getIcons();
                    // $scope.module = $filter('filter')($scope.modules, {id: moduleId}, true)[0];
                    // $scope.module.is_component = angular.equals($scope.module.system_type, "component");
                    ModuleService.getModuleById(moduleId)
                        .then(function (result) {
                            $scope.module = result.data;
                            $scope.module.is_component = angular.equals($scope.module.system_type, "component");
                            $scope.modalLoading = false;
                        });
                    $scope.editModal.$promise.then($scope.editModal.show);
                };

                $scope.cancelModule = function () {
                    $scope.editModal.hide();
                };

                $scope.saveSettings = function (editForm) {
                    if (editForm.$invalid)
                        return;

                    $scope.saving = true;
                    if (angular.isObject($scope.module.menu_icon))
                        $scope.module.menu_icon = $scope.module.menu_icon.value;

                    ModuleService.moduleUpdate($scope.module, $scope.module.id)
                        .then(function () {
                            toastr.success($filter('translate')('Setup.Modules.SaveSuccess'));
                            $scope.editModal.hide();
                            $scope.changePage($scope.activePage);
                        }).finally(function () {
                            $scope.saving = false;

                        });
                }

                $scope.moduleListFilter = function(item){
                    return item.name !== 'users' && item.name !== 'profiles' && item.name !== 'roles';
                };
            }
        ]);