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

            $scope.environments = ComponentsService.getEnvironments();
            $scope.modules = $filter('filter')($rootScope.appModules, { system_type: 'component' }, true);

            $scope.component = {};
            $scope.components = [];
            $scope.loading = true;
            $scope.componentPlaces = componentPlaces;
            $scope.componentTypes = componentTypes;
            $rootScope.breadcrumblist[2].title = 'Components';
            //$scope.page = 1;

            //$scope.generator = function (limit) {
            //    $scope.placeholderArray = [];
            //    for (var i = 0; i < limit; i++) {
            //        $scope.placeholderArray[i] = i;
            //    }
            //};

            //$scope.generator(10);

            //$scope.requestModel = {
            //    limit: "10",
            //    offset: 0
            //};

            $scope.closeModal = function () {
                $scope.component = {};
                $scope.createFormModal.hide();
            };

            //$scope.reload = function () {
            //    ComponentsService.count()
            //        .then(function (response) {
            //            $scope.pageTotal = response.data;

            //            if ($scope.requestModel.offset != 0 && ($scope.requestModel.offset * $scope.requestModel.limit) >= $scope.pageTotal) {
            //                $scope.requestModel.offset = $scope.requestModel.offset - 1;
            //            }

            //            ComponentsService.find($scope.requestModel)
            //                .then(function (response) {
            //                    $scope.components = response.data;
            //                    $scope.loading = false;
            //                });
            //        });
            //};

            //$scope.reload();

            $scope.environmentChange = function (env, index, otherValue) {
                otherValue = otherValue || false;

                if (index === 2) {
                    $scope.environments[1].selected = true;
                    $scope.environments[1].disabled = !!env.selected;

                    if (otherValue) {
                        $scope.environments[2].selected = otherValue;
                    }
                }
            };

            //$scope.changePage = function (page) {
            //    $scope.loading = true;
            //    var requestModel = angular.copy($scope.requestModel);
            //    requestModel.offset = page - 1;
            //    $scope.page = requestModel.offset + 1;
            //    ComponentsService.find(requestModel)
            //        .then(function (response) {
            //            $scope.components = response.data;
            //            $scope.loading = false;
            //        });

            //};

            //$scope.changeOffset = function () {
            //    $scope.changePage(1)
            //};

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
                $scope.component.environments = [];

                angular.forEach($scope.environments, function (env) {
                    if (env.selected)
                        $scope.component.environments.push(env.value);
                });

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

            //$scope.getModuleName = function (id) {
            //    return $filter('filter')($scope.modules, { id: id }, true)[0]['label_en_singular'];
            //};

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

            //For Kendo UI
            $scope.goUrl = function (item) {
                var selection = window.getSelection();
                if (selection.toString().length === 0) {
                    $state.go('studio.app.componentDetail', { id: item.id }); //click event.
                }
            };

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
                            url: "/api/component/find",
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
                        },
                        parse: function (data) {
                            for (var i = 0; i < data.items.length; i++) {
                                var module = $filter('filter')($scope.modules, { id: data.items[i].module_id }, true);
                                if (module && module.length > 0)
                                    data.items[i].module = angular.copy(module[0]);
                            }

                            return data;
                        }
                    }
                },
                scrollable: false,
                persistSelection: true,
                sortable: true,
                filterable: {
                    extra: false
                },
                rowTemplate: function (e) {
                    var trTemp = '<tr ng-click="goUrl(dataItem)">';
                    trTemp += '<td> <span>' + e.label + '</span></td > ';
                    trTemp += '<td> <span>' + e.name + '</span></td > ';
                    trTemp += '<td><span>' + e.module['label_' + $scope.language + '_plural'] + '</span></td>';
                    trTemp += '<td ng-click="$event.stopPropagation();"> <button ng-click="$event.stopPropagation(); delete(dataItem.id, $event);" type="button" class="action-button2-delete"><i class="fas fa-trash"></i></button></td></tr>';
                    return trTemp;
                },
                altRowTemplate: function (e) {
                    var trTemp = '<tr class="k-alt" ng-click="goUrl(dataItem)">';
                    trTemp += '<td> <span>' + e.label + '</span></td > ';
                    trTemp += '<td> <span>' + e.name + '</span></td > ';
                    trTemp += '<td><span>' + e.module['label_' + $scope.language + '_plural'] + '</span></td>';
                    trTemp += '<td ng-click="$event.stopPropagation();"> <button ng-click="$event.stopPropagation(); delete(dataItem.id, $event);" type="button" class="action-button2-delete"><i class="fas fa-trash"></i></button></td></tr>';
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
                        field: 'Label',
                        title: 'Label',
                    },
                    {
                        field: 'Name',
                        title: 'Identifier',
                    },
                    {
                        field: 'Module.Name',
                        title: $filter('translate')('Setup.Modules.Name'),
                    },
                    {
                        field: '',
                        title: '',
                        width: "90px"
                    }]
            };
            //For Kendo UI
        }
    ]);