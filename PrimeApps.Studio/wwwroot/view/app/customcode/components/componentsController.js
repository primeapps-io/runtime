'use strict';

angular.module('primeapps')

    .controller('ComponentsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'ComponentsService', 'componentPlaces', 'componentPlaceEnums', 'componentTypes', 'componentTypeEnums', '$localStorage',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, ComponentsService, componentPlaces, componentPlaceEnums, componentTypes, componentTypeEnums, $localStorage) {
            $scope.appId = $state.params.appId;
            $scope.orgId = $state.params.orgId;

            $scope.$parent.menuTopTitle = $scope.currentApp.label;
            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'components';

            $scope.currentApp = $localStorage.get("current_app");

            /*if (!$scope.orgId || !$scope.appId) {
             $state.go('studio.apps', { organizationId: $scope.orgId });
             }*/

            $scope.modules = $filter('filter')($rootScope.appModules, { system_type: 'component' }, true);

            $scope.component = {};
            $scope.components = [];
            $scope.loading = true;
            $scope.componentPlaces = componentPlaces;
            $scope.componentTypes = componentTypes;
            $rootScope.breadcrumblist[2].title = 'Components';
            $scope.page = 1;
            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }
            };

            $scope.generator(10);

            $scope.requestModel = {
                limit: "10",
                offset: 0
            };

            $scope.closeModal = function () {
                $scope.component = {};
                $scope.createFormModal.hide();
            };

            $scope.reload = function () {
                ComponentsService.count()
                    .then(function (response) {
                        $scope.pageTotal = response.data;

                        if ($scope.requestModel.offset != 0 && ($scope.requestModel.offset * $scope.requestModel.limit) >= $scope.pageTotal) {
                            $scope.requestModel.offset = $scope.requestModel.offset - 1;
                        }

                        ComponentsService.find($scope.requestModel)
                            .then(function (response) {
                                $scope.components = response.data;
                                $scope.loading = false;
                            });
                    });
            };

            $scope.reload();

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;
                $scope.page = requestModel.offset + 1;
                ComponentsService.find(requestModel)
                    .then(function (response) {
                        $scope.components = response.data;
                        $scope.loading = false;
                    });

            };

            $scope.changeOffset = function () {
                $scope.changePage(1)
            };

            $scope.openModal = function () {
                $scope.createFormModal = $scope.createFormModal || $modal({
                        scope: $scope,
                        templateUrl: 'view/app/customcode/components/componentFormModal.html',
                        animation: 'am-fade-and-slide-right',
                        backdrop: 'static',
                        show: false
                    });
                $scope.createFormModal.$promise.then(function () {
                    $scope.createFormModal.show();
                });
            };

            $scope.save = function (componentFormValidation) {
                if (!componentFormValidation.$valid) {
                    toastr.error($filter('translate')('Module.RequiredError'));
                    return;
                }


                $scope.saving = true;

                var module = $filter('filter')($scope.modules, { id: $scope.component.module.id }, true)[0];

                $scope.component.place = 0;
                $scope.component.order = 0;
                $scope.component.name = module.name.replace(/_/g, '');
                $scope.component.module_id = module.id;

                ComponentsService.create($scope.component)
                    .then(function (response) {
                        $scope.saving = false;
                        $scope.createFormModal.hide();
                        toastr.success("Component is created successfully.");
                        $state.go('studio.app.componentDetail', { id: response.data });
                    })
                    .catch(function (response) {
                        if (response.status === 409) {
                            toastr.warning("Component already exist for module " + module['label_en_singular']);
                        }
                        $scope.saving = false;
                    });
            };

            $scope.getModuleName = function (id) {
                return $filter('filter')($scope.modules, { id: id }, true)[0]['label_en_singular'];
            };

            $scope.delete = function (id, event) {
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
                            if (id) {
                                ComponentsService.delete(id)
                                    .then(function (response) {
                                        toastr.success("Component is deleted successfully.", "Deleted!");
                                        angular.element(document.getElementsByClassName('ng-scope animated-background')).remove();
                                        $scope.reload();
                                    })
                                    .catch(function () {
                                        angular.element(document.getElementsByClassName('ng-scope animated-background')).removeClass('animated-background');
                                    });
                            }
                        }
                    });

            }
        }
    ]);