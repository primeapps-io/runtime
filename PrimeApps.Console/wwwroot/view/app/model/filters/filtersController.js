'use strict';

angular.module('primeapps')

    .controller('FiltersController', ['$rootScope', '$scope', '$state', '$stateParams', '$location', 'ngToast', '$filter', '$cache', '$q', 'helper', 'dragularService', 'operators', 'FiltersService', '$http', 'config', '$modal', 'ModuleService',
        function ($rootScope, $scope, $state, $stateParams, $location, ngToast, $filter, $cache, $q, helper, dragularService, operators, FiltersService, $http, config, $modal, ModuleService) {


            $scope.$parent.menuTopTitle = "Models";
            $scope.$parent.activeMenu = 'model';
            $scope.$parent.activeMenuItem = 'filters';

            $rootScope.breadcrumblist[2].title = 'Filters';

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }
            };

            $scope.generator(10);

            $scope.id = $location.search().id ? $location.search().id : 0;

            $scope.loading = true;
            $scope.wizardStep = 0;
            $scope.requestModel = {
                limit: '10',
                offset: 1
            };

            FiltersService.count($scope.id).then(function (response) {
                $scope.pageTotal = response.data;
            });

            FiltersService.find($scope.id, $scope.requestModel).then(function (response) {
                var customViews = angular.copy(response.data);
                for (var i = customViews.length - 1; i >= 0; i--) {
                    var parentModule = $filter('filter')($scope.$parent.modules, { id: customViews[i].module_id }, true)[0];
                    if (parentModule) {
                        customViews[i].parent_module = $filter('filter')($scope.$parent.modules, { id: customViews[i].module_id }, true)[0];
                    } else {
                        customViews.splice(i, 1);
                    }
                }
                $scope.customViews = customViews;
                $scope.customViewsState = customViews;
                $scope.loading = false;
            });

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;

                FiltersService.find($scope.id, requestModel).then(function (response) {
                    var customViews = angular.copy(response.data);
                    for (var i = customViews.length - 1; i >= 0; i--) {
                        var parentModule = $filter('filter')($scope.$parent.modules, { id: customViews[i].module_id }, true)[0];
                        if (parentModule) {
                            customViews[i].parent_module = $filter('filter')($scope.$parent.modules, { id: customViews[i].module_id }, true)[0];
                        } else {
                            customViews.splice(i, 1);
                        }
                    }
                    $scope.customViews = customViews;
                    $scope.loading = false;
                });
            };

            $scope.changeOffset = function () {
                $scope.changePage(1)
            };

            $scope.deleteView = function (id) {
                const willDelete =
                    swal({
                        title: "Are you sure?",
                        text: "Are you sure that you want to delete this filter ?",
                        icon: "warning",
                        buttons: ['Cancel', 'Okey'],
                        dangerMode: true
                    }).then(function (value) {
                        if (value) {
                            if (id) {
                                FiltersService.deleteView(id)
                                    .then(function () {
                                        $scope.changePage(1);
                                        swal("Deleted!", "Your  filters has been deleted!", "success");
                                    }).catch(function () {
                                    $scope.customViews = $scope.customViewsState;

                                    if ($scope.addNewFiltersModal) {
                                        $scope.addNewFiltersModal.hide();
                                        $scope.saving = false;
                                    }
                                });
                            }
                            else {
                                swal($filter('translate')('Setup.Modules.OneView'), "", "warning");
                                return;
                            }
                        }
                    });
            };

            $scope.showFormModal = function (view) {
                if (view) {
                    $scope.view = angular.copy(view);
                    var module = view.parent_module;
                    $scope.module = module;
                    $scope.view.label = $scope.view['label_' + $scope.language];
                    $scope.view.edit = true;
                    // $scope.isOwner = $scope.view.created_by === $rootScope.user.ID;

                    // if (!$scope.view) {
                    //     TODO
                    //     $state.go('app.crm.moduleList', { type: module.name });
                    //     return;
                    // }

                    if ($scope.view.filter_logic && $scope.language === 'tr')
                        $scope.view.filter_logic = $scope.view.filter_logic.replace('or', 'veya').replace('and', 've');

                    moduleChanged(module, false);
                }
                else {
                    $scope.view = {};
                    //moduleChanged($scope.module, true);
                }
                $scope.addNewFiltersModal = $scope.addNewFiltersModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/model/filters/filtersForm.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false,
                    controller: function ($scope) {
                        $scope.$on('dragulardrop', function (e, el) {
                            $scope.viewForm.$setValidity('field', true);
                        });
                    }
                });

                $scope.addNewFiltersModal.$promise.then(function () {
                    $scope.addNewFiltersModal.show();
                });
            };

            $scope.selectedModuleChanged = function (module) {
                $scope.module = module;
                moduleChanged($scope.module, true);
            };

            var dragular = function () {
                var containerLeft = document.querySelector('#availableFields');
                var containerRight = document.querySelector('#selectedFields');

                dragularService.cleanEnviroment();

                dragularService([containerLeft], {
                    scope: $scope,
                    containersModel: [$scope.fields.availableFields],
                    classes: {
                        mirror: 'gu-mirror-view',
                        transit: 'gu-transit-view'
                    },
                    accepts: accepts,
                    moves: function (el, container, handle) {
                        return handle.classList.contains('dragable');
                    }
                });

                dragularService([containerRight], {
                    scope: $scope,
                    classes: {
                        mirror: 'gu-mirror-view',
                        transit: 'gu-transit-view'
                    },
                    containersModel: [$scope.fields.selectedFields]
                });

                function accepts(el, target, source) {
                    if (source != target) {
                        return true;
                    }
                }
            };

            var moduleChanged = function (module, setView) {
                $scope.lookupUser = helper.lookupUser;

                if (setView) {
                    $scope.view = {};
                    $scope.view.system_type = 'custom';
                    $scope.view.sharing_type = 'me';
                }

                /*var cacheKey = module.name + '_' + module.name;
                 var cache = $cache.get(cacheKey);

                 if (!cache || !cache['views'] || cache['views'].length < 1) {
                 $state.go('app.crm.moduleList', { type: module.name });
                 return;
                 }*/
                ModuleService.getModuleFields(module.name).then(function (response) {

                    $scope.module.fields = response.data;
                    $scope.module = ModuleService.getFieldsOperator(module, $scope.$parent.modules, 0);
                    $scope.fields = FiltersService.getFields($scope.module, angular.copy($scope.view), $scope.$parent.modules);

                    ModuleService.getPickItemsLists($scope.module)
                        .then(function (picklists) {
                            $scope.modulePicklists = picklists;
                            $scope.view.filterList = [];

                            for (var i = 0; i < 5; i++) {
                                var filter = {};
                                filter.id = i;
                                filter.field = null;
                                filter.operator = null;
                                filter.value = null;
                                filter.no = i + 1;

                                $scope.view.filterList.push(filter);
                            }

                            dragular();
                        });
                });
            };

            $scope.multiselect = function (searchTerm, field) {
                var picklistItems = [];

                angular.forEach($scope.modulePicklists[field.picklist_id], function (picklistItem) {
                    if (picklistItem.inactive)
                        return;

                    if (picklistItem.labelStr.toLowerCase().indexOf(searchTerm) > -1)
                        picklistItems.push(picklistItem);
                });

                return picklistItems;
            };

            var dateTimeChanged = function (filterListItem) {
                if (filterListItem.operator) {
                    var newValue = new Date(filterListItem.value);

                    switch (filterListItem.operator.name) {
                        case 'greater':
                            newValue.setHours(23);
                            newValue.setMinutes(59);
                            newValue.setSeconds(59);
                            newValue.setMilliseconds(99);
                            break;
                        case 'less':
                            newValue.setHours(0);
                            newValue.setMinutes(0);
                            newValue.setSeconds(0);
                            newValue.setMilliseconds(0);
                            break;
                    }

                    filterListItem.value = newValue;
                }
            };

            $scope.dateTimeChanged = function (field) {
                dateTimeChanged(field);
            };

            $scope.operatorChanged = function (field, index) {
                var filterListItem = $scope.view.filterList[index];

                if (!filterListItem || !filterListItem.operator)
                    return;

                if (field.data_type === 'date_time' && filterListItem.value)
                    dateTimeChanged(filterListItem);

                if (filterListItem.operator.name === 'empty' || filterListItem.operator.name === 'not_empty') {
                    filterListItem.value = null;
                    filterListItem.disabled = true;
                }
                else {
                    filterListItem.disabled = false;
                }
            };

            $scope.save = function (viewForm) {

                if (!viewForm.$valid || !$scope.validate(viewForm))
                    return;

                $scope.saving = true;

                var view = {};
                view.module_id = $scope.module.id;
                view.label = $scope.view.label;
                view.sharing_type = $scope.view.sharing_type;
                view.fields = [];
                view.filters = [];

                if ($scope.view.filter_logic) {
                    view.filter_logic = $scope.view.filter_logic.replace('veya', 'or').replace('ve', 'and');

                    if (!(view.filter_logic.charAt(0) === '(' && view.filter_logic.charAt(view.filter_logic.length - 1) === ')'))
                        view.filter_logic = '(' + view.filter_logic + ')';
                }

                for (var i = 0; i < $scope.fields.selectedFields.length; i++) {
                    var selectedField = $scope.fields.selectedFields[i];
                    var field = {};
                    field.field = selectedField.name;
                    field.order = i + 1;

                    view.fields.push(field);

                    if (selectedField.lookup_type && selectedField.lookup_type != 'relation') {
                        var lookupModule = $filter('filter')($scope.$parent.modules, { name: selectedField.lookup_type }, true)[0];
                        var primaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];
                        var fieldPrimary = {};
                        fieldPrimary.field = selectedField.name + '.' + lookupModule.name + '.' + primaryField.name + '.primary';
                        fieldPrimary.order = i + 1;

                        view.fields.push(fieldPrimary);
                    }
                }

                var filterList = angular.copy($scope.view.filterList);

                angular.forEach(filterList, function (filterItem) {

                    if (!filterItem.field || !filterItem.operator)
                        return;

                    if (!(filterItem.operator.name === 'empty' || filterItem.operator.name === 'not_empty') && (filterItem.value == null || filterItem.value == undefined))
                        return;

                    var field = filterItem.field;
                    var fieldName = field.name;

                    if (field.data_type === 'lookup' && field.lookup_type != 'users') {
                        var lookupModule = $filter('filter')($scope.$parent.modules, { name: field.lookup_type }, true)[0];
                        var lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];
                        fieldName = field.name + '.' + field.lookup_type + '.' + lookupModulePrimaryField.name;
                    }

                    var filter = {};
                    filter.field = fieldName;
                    filter.operator = filterItem.operator.name;
                    filter.value = filterItem.value;
                    filter.no = filterItem.no;

                    field = !filterItem.field.lookupModulePrimaryField ? filterItem.field : filterItem.field.lookupModulePrimaryField;

                    if (!(filterItem.operator.name === 'empty' || filterItem.operator.name === 'not_empty')) {
                        if (field.data_type === 'picklist')
                            filter.value = filter.value.label[$scope.language];

                        if (field.data_type === 'multiselect') {
                            var value = '';

                            angular.forEach(filter.value, function (picklistItem) {
                                value += picklistItem.label[$scopelanguage] + '|';
                            });

                            filter.value = value.slice(0, -1);
                        }

                        if (field.data_type === 'lookup' && field.lookup_type === 'users') {
                            if (filter.value[0].id === 0)
                                filter.value = '[me]';
                            else
                                filter.value = filter.value[0].id;
                        }

                        if (field.data_type === 'checkbox')
                            filter.value = filter.value.system_code;
                    }
                    else {
                        filter.value = '-';
                    }

                    view.filters.push(filter);
                });

                if ($scope.view.sharing_type === 'custom') {
                    if (!$scope.view.shares) {
                        view.sharing_type = 'me';
                    }
                    else {
                        view.shares = [];

                        angular.forEach($scope.view.shares, function (user) {
                            view.shares.push(user.id);
                        });
                    }
                }

                if (!$scope.view.id) {
                    ViewService.create(view)
                        .then(function (response) {
                            //var viewState = cache.viewState;
                            var viewState;

                            if (!viewState) {
                                viewState = {};
                                viewState.sort_field = 'created_at';
                                viewState.sort_direction = 'desc';
                                viewState.row_per_page = 10;
                            }

                            viewState.active_view = response.data.id;

                            FiltersService.setViewState(viewState, $scope.module.id, viewState.id)
                                .then(function () {
                                    success();
                                })
                                .finally(function () {
                                    $scope.saving = false;
                                });
                        })
                        .catch(function (data) {
                            error(data.data, data.status);
                        })
                        .finally(function () {
                            $scope.saving = false;
                        });
                }
                else {
                    FiltersService.update(view, $scope.view.id, $scope.view._rev)
                        .then(function () {
                            success();
                        })
                        .catch(function (data) {
                            error(data.data, data.status);
                        })
                        .finally(function () {
                            $scope.saving = false;
                        });
                }

                function success() {
                    //swal("Good job!", "You clicked the button!", "success");
                    swal("İşlem Başarıyla Gerçekleştirilmiştir!", "", "success");
                    //$state.go('studio.app.filters');
                    $scope.addNewFiltersModal.hide();
                    $scope.changePage(1);
                }

                function error(data, status) {
                    if (status === 400) {
                        if (data.model_state && data.model_state['view._filter_logic'])
                            $scope.viewForm.filterLogic.$setValidity('filterLogic', false);

                        if (data.model_state && data.model_state['request._filter_logic'])
                            $scope.viewForm.filterLogic.$setValidity('filterLogicFilters', false);
                    }
                }
            }

            $scope.validate = function (viewForm, wizardStep) {

                viewForm.$submitted = true;

                if (!viewForm.label.$valid || !viewForm.module.$valid) {
                    return false;
                }

                if ($scope.fields.selectedFields.length < 1 && wizardStep != 0) {
                    viewForm.$setValidity('field', false);
                    return false;
                }

                if (!viewForm.filterLogic.$valid) {
                    return false;
                }

                return true;
            };

        }
    ]);