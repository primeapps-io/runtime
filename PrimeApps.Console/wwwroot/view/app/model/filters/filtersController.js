'use strict';

angular.module('primeapps')

    .controller('FiltersController', ['$rootScope', '$scope', '$state', '$stateParams', '$location', 'ngToast', '$filter', '$cache', '$q', 'helper', 'dragularService', 'operators', 'FiltersService', '$http', 'config', '$modal', 'ModuleService', 'dataTypes', '$timeout',
        function ($rootScope, $scope, $state, $stateParams, $location, ngToast, $filter, $cache, $q, helper, dragularService, operators, FiltersService, $http, config, $modal, ModuleService, dataTypes, $timeout) {


            $scope.$parent.menuTopTitle = "Models";
            $scope.$parent.activeMenu = 'model';
            $scope.$parent.activeMenuItem = 'filters';
            $scope.loading = true;
            $scope.wizardStep = 0;
            $scope.requestModel = {
                limit: '10',
                offset: 1
            };

            FiltersService.count().then(function (response) {
                $scope.pageTotal = response.data;
            });

            FiltersService.find($scope.requestModel).then(function (response) {
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

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;

                FiltersService.find(requestModel).then(function (response) {
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
                if ($scope.customViews.length > 1) {
                    FiltersService.deleteView(id)
                        .then(function () {
                            $scope.customView = $filter('filter')($scope.customViews, { active: true })[0];
                            $scope.customViews.splice($scope.customViews.indexOf($scope.view), 1);
                        });
                }
                else {
                    ngToast.create({ content: $filter('translate')('Setup.Modules.OneView'), className: 'warning' });
                    return;
                }
            };

            // FiltersService.getViews().then(function (response) {
            //     var customViews = angular.copy(response.data);
            //     for (var i = customViews.length - 1; i >= 0; i--) {
            //         var parentModule = $filter('filter')($scope.$parent.modules, { id: customViews[i].module_id }, true)[0];
            //         if (parentModule) {
            //             customViews[i].parent_module = $filter('filter')($scope.$parent.modules, { id: customViews[i].module_id }, true)[0];
            //         } else {
            //             customViews.splice(i, 1);
            //         }
            //     }
            //
            //     $scope.customViews = customViews;
            //     $scope.loading = false;
            // });

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

                    if ($scope.view.filter_logic && $rootScope.language === 'tr')
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
                    module.fields = response.data;
                    $scope.fields = FiltersService.getFields(module, angular.copy($scope.view), $scope.$parent.modules);
                    FiltersService.getPicklists(module, true, $scope.$parent.modules)
                        .then(function (picklists) {
                            $scope.modulePicklists = picklists;
                            $scope.view.filterList = [];

                            for (var i = 0; i < 5; i++) {
                                var filter = {};
                                filter.field = null;
                                filter.operator = null;
                                filter.value = null;
                                filter.no = i + 1;

                                $scope.view.filterList.push(filter);
                            }

                            if ($scope.view.filters) {
                                $scope.view.filters = $filter('orderBy')($scope.view.filters, 'no');

                                for (var j = 0; j < $scope.view.filters.length; j++) {
                                    var name = $scope.view.filters[j].field;
                                    var value = $scope.view.filters[j].value;

                                    if (name.indexOf('.') > -1) {
                                        name = name.split('.')[0];
                                        $scope.view.filters[j].field = name;
                                    }

                                    var field = $filter('filter')(module.fields, { name: name }, true)[0];
                                    var fieldValue = null;

                                    if (!field)
                                        return;

                                    switch (field.data_type) {
                                        case 'picklist':
                                            fieldValue = $filter('filter')($scope.modulePicklists[field.picklist_id], { labelStr: value }, true)[0];
                                            break;
                                        case 'multiselect':
                                            fieldValue = [];
                                            var multiselectValue = value.split('|');

                                            angular.forEach(multiselectValue, function (picklistLabel) {
                                                var picklist = $filter('filter')($scope.modulePicklists[field.picklist_id], { labelStr: picklistLabel }, true)[0];

                                                if (picklist)
                                                    fieldValue.push(picklist);
                                            });
                                            break;
                                        case 'lookup':
                                            if (field.lookup_type === 'users') {
                                                var user = {};

                                                if (value === '0' || value === '[me]') {
                                                    user.id = 0;
                                                    user.email = '[me]';
                                                    user.full_name = $filter('translate')('Common.LoggedInUser');
                                                }
                                                else {
                                                    var userItem = $filter('filter')($rootScope.users, { Id: parseInt(value) }, true)[0];
                                                    user.id = userItem.Id;
                                                    user.email = userItem.Email;
                                                    user.full_name = userItem.FullName;

                                                    //TODO: $rootScope.users kaldirilinca duzeltilecek
                                                    // ModuleService.getRecord('users', value)
                                                    //     .then(function (lookupRecord) {
                                                    //         fieldValue = [lookupRecord.data];
                                                    //     });
                                                }

                                                fieldValue = [user];
                                            }
                                            else {
                                                fieldValue = value;
                                            }
                                            break;
                                        case 'date':
                                        case 'date_time':
                                        case 'time':
                                            fieldValue = new Date(value);
                                            break;
                                        case 'checkbox':
                                            fieldValue = $filter('filter')($scope.modulePicklists.yes_no, { system_code: value }, true)[0];
                                            break;
                                        default :
                                            fieldValue = value;
                                            break;
                                    }

                                    $scope.view.filterList[j].field = field;
                                    $scope.view.filterList[j].operator = operators[$scope.view.filters[j].operator];
                                    $scope.view.filterList[j].value = fieldValue;

                                    if ($scope.view.filters[j].operator === 'empty' || $scope.view.filters[j].operator === 'not_empty') {
                                        $scope.view.filterList[j].value = null;
                                        $scope.view.filterList[j].disabled = true;
                                    }
                                }
                            }
                            else {
                                angular.forEach(module.fields, function (field) {
                                    field.dataType = dataTypes[field.data_type];
                                    field.operators = [];
                                    if (field.data_type === 'lookup') {
                                        if (field.lookup_type != 'users' && field.lookup_type != 'profiles' && field.lookup_type != 'roles' && field.lookup_type != 'relation') {
                                            var lookupModule = $filter('filter')($scope.$parent.modules, { name: field.lookup_type }, true)[0];
                                            //TODO GETFIELDS
                                            field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { primary_lookup: true }, true)[0];

                                            if (!field.lookupModulePrimaryField)
                                                field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];

                                            var lookupModulePrimaryFieldDataType = dataTypes[field.lookupModulePrimaryField.data_type];

                                            for (var m = 0; m < lookupModulePrimaryFieldDataType.operators.length; m++) {
                                                var operatorIdLookup = lookupModulePrimaryFieldDataType.operators[m];
                                                var operatorLookup = operators[operatorIdLookup];
                                                field.operators.push(operatorLookup);
                                            }
                                        }
                                        else {
                                            field.operators.push(operators.equals);
                                            field.operators.push(operators.not_equal);
                                            field.operators.push(operators.empty);
                                            field.operators.push(operators.not_empty);

                                            // if (field.lookup_type === 'users') {
                                            //     var lookupModule = $filter('filter')(modules, { name: 'users' }, true)[0];
                                            //     field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];
                                            // }
                                            // else if (field.lookup_type === 'profiles') {
                                            //     var lookupModule = $filter('filter')(modules, { name: 'profiles' }, true)[0];
                                            //     field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];
                                            // }
                                            // else if (field.lookup_type === 'roles') {
                                            //     var lookupModule = $filter('filter')(modules, { name: 'roles' }, true)[0];
                                            //     field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];
                                            // }
                                        }

                                    }
                                    else {
                                        for (var n = 0; n < field.dataType.operators.length; n++) {
                                            var operatorId = field.dataType.operators[n];
                                            var operator = operators[operatorId];
                                            field.operators.push(operator);
                                        }
                                    }

                                });
                            }
                        });
                    //dragular();
                });
                $timeout(function () {
                    dragular();
                }, 1000);
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
                        var lookupModule = $filter('filter')($rootScope.modules, { name: field.lookup_type }, true)[0];
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
                            filter.value = filter.value.label[$rootScope.user.tenantLanguage];

                        if (field.data_type === 'multiselect') {
                            var value = '';

                            angular.forEach(filter.value, function (picklistItem) {
                                value += picklistItem.label[$rootScope.user.tenantLanguage] + '|';
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
                    $state.go('studio.app.filters');
                    $scope.addNewFiltersModal.hide();
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


            // FiltersService.getViews_(1)
            //     .then(function (views) {
            //         var a = views;
            //     });


            // $scope.changeView = function () {
            //     tableBlockUI.start();
            //     $scope.selectedView = $scope.view;
            //     var cache = $cache.get(cacheKey);
            //     var viewStateCache = cache.viewState;
            //     var viewState = viewStateCache || {};
            //     viewState.active_view = $scope.selectedView.id;
            //
            //     ModuleService.setViewState(viewState, $scope.module.id, viewState.id)
            //         .then(function () {
            //             tableBlockUI.stop();
            //             $scope.refresh(true);
            //         });
            // };


            /*$scope.moduleLists = $filter('filter')($rootScope.modules, { display: true }, true);
             $scope.setViews = function () {
             $scope.relations = [];
             for (var i = 0; i < $rootScope.modules.length; i++) {
             var module = angular.copy($rootScope.modules[i]);
             ModuleService.getViews(module.id, undefined, undefined)
             .then(function (views) {
             $scope.customViews = $scope.customViews.concat(views);
             });
             }
             };

             $scope.setViews();*/

            /*$scope.moduleChanged = function () {
             if ($scope.currentActionButton.module) {
             $scope.module = $scope.currentActionButton.module;
             webhookParameters();
             }
             };

             $scope.showFormModal = function (actionButton) {
             if (!actionButton) {
             actionButton = {};
             actionButton.type = 3;
             actionButton.triggerType = 1;
             actionButton.isNew = true;
             $scope.currentActionButton = actionButton;
             } else {
             $scope.currentActionButton = actionButton;
             $scope.currentActionButton.module = actionButton.parent_module;
             $scope.moduleChanged();
             webhookParameters();
             }
             $scope.currentActionButtonState = angular.copy($scope.currentActionButton);
             $scope.actionButtonTypes = [
             {
             type: "Modal",
             value: 3
             },
             {
             type: "Webhook",
             value: 2
             }
             ];

             $scope.displayPages = [
             {
             name: $filter('translate')('Setup.Modules.Detail'),
             value: 1
             },
             {
             name: "Form",
             value: 2
             },
             {
             name: $filter('translate')('Setup.Modules.All'),
             value: 3
             }
             ];

             if (actionButton.action_type === 'Webhook')
             actionButton.type = 2;

             if (actionButton.action_type === 'ModalFrame')
             actionButton.type = 3;

             if (actionButton.trigger === 'Detail')
             actionButton.triggerType = 1;

             if (actionButton.trigger === 'Form')
             actionButton.triggerType = 2;

             if (actionButton.trigger === 'All')
             actionButton.triggerType = 3;

             if (actionButton.type === 3) {
             $scope.hookParameters = [];

             $scope.hookModules = [];

             angular.forEach($scope.updatableModules, function (module) {
             $scope.hookModules.push(module);
             });

             var parameter = {};
             parameter.parameterName = null;
             parameter.selectedModules = $scope.hookModules;
             parameter.selectedField = null;

             $scope.hookParameters.push(parameter);
             }

             if (actionButton.method_type && actionButton.parameters && actionButton.type == 2) {
             $scope.hookParameters = [];

             var hookParameterArray = actionButton.parameters.split(',');

             angular.forEach(hookParameterArray, function (data) {
             var parameter = data.split("|", 3);

             var editParameter = {};
             editParameter.parameterName = parameter[0];
             editParameter.selectedModules = angular.copy($scope.updatableModules);
             var selectedModule;

             if ($scope.module.name === parameter[1]) {
             selectedModule = $filter('filter')(editParameter.selectedModules, { name: parameter[1] }, true)[0];
             }
             else {
             var lookupModuleName = $filter('filter')($scope.module.fields, { name: parameter[1] }, true)[0].lookup_type;
             selectedModule = $filter('filter')(editParameter.selectedModules, { name: lookupModuleName }, true)[0];
             }


             if (!selectedModule)
             return;

             editParameter.selectedModule = selectedModule;
             editParameter.selectedField = $filter('filter')(editParameter.selectedModule.fields, { name: parameter[2] }, true)[0];

             $scope.hookParameters.push(editParameter);
             })
             }

             $scope.formModal = $scope.formModal || $modal({
             scope: $scope,
             templateUrl: 'views/app/setup/crm/modules/actionButtonForm.html',
             animation: 'am-slide-right',
             });

             $scope.formModal.$promise.then(function () {
             $scope.formModal.show();
             });
             };

             $scope.save = function (actionButtonForm) {
             if (!actionButtonForm.$valid)
             return;

             $scope.saving = true;
             var actionButton = angular.copy($scope.currentActionButton);

             if (actionButton.isNew)
             delete actionButton.isNew;

             actionButton.module_id = $scope.module.id;
             actionButton.template = 'template';

             actionButton.trigger = actionButton.triggerType;

             delete actionButton.triggerType;

             if (actionButton.type === 2) {
             var hookArray = [];
             angular.forEach($scope.hookParameters, function (hookParameter) {
             var moduleName;
             if ($scope.module.name != hookParameter.selectedModule.name)
             moduleName = $filter('filter')($scope.module.fields, { lookup_type: hookParameter.selectedModule.name }, true)[0].name;
             else
             moduleName = hookParameter.selectedModule.name;

             var parameterString = hookParameter.parameterName + "|" + moduleName + "|" + hookParameter.selectedField.name;
             hookArray.push(parameterString);
             });

             if (hookArray.length > 0) {
             actionButton.parameters = hookArray.toString();
             }
             } else {
             actionButton.parameters = null;
             actionButton.method_type = null;
             }

             if (!actionButton.id) {
             ModuleSetupService.createActionButton(actionButton)
             .then(function (response) {
             if (!$scope.actionButtons)
             $scope.actionButtons = [];

             actionButton.action_type = response.data.type;
             actionButton.trigger = response.data.trigger;
             actionButton.id = response.data.id;
             $scope.actionButtons.push(actionButton);

             ngToast.create({
             content: $filter('translate')('Setup.Modules.ActionButtonSaveSuccess'),
             className: 'success'
             });
             $scope.saving = false;
             $scope.formModal.hide();

             }).catch(function () {
             $scope.actionButtons = $scope.actionbuttonState;

             if ($scope.formModal) {
             $scope.formModal.hide();
             $scope.saving = false;
             }
             });
             } else {
             ModuleSetupService.updateActionButton(actionButton)
             .then(function (response) {
             $filter('filter')($scope.actionButtons, { id: actionButton.id }, true)[0].action_type = response.data.type;
             $filter('filter')($scope.actionButtons, { id: actionButton.id }, true)[0].trigger = response.data.trigger;
             $filter('filter')($scope.actionButtons, { id: actionButton.id }, true)[0].method_type = response.data.method_type;
             $filter('filter')($scope.actionButtons, { id: actionButton.id }, true)[0].parameters = response.data.parameters;
             ngToast.create({
             content: $filter('translate')('Setup.Modules.ActionButtonSaveSuccess'),
             className: 'success'
             });
             $scope.saving = false;
             $scope.formModal.hide();
             }).catch(function () {
             $scope.actionButtons = $scope.actionbuttonState;

             if ($scope.formModal) {
             $scope.formModal.hide();
             $scope.saving = false;
             }
             });
             }
             };

             $scope.delete = function (actionButton) {
             delete actionButton.$$hashKey;
             var deleteModel = angular.copy($scope.actionButtons);
             var actionButtonIndex = helper.arrayObjectIndexOf(deleteModel, actionButton);
             deleteModel.splice(actionButtonIndex, 1);

             ModuleSetupService.deleteActionButton(actionButton.id)
             .then(function () {
             var actionButtonIndex = helper.arrayObjectIndexOf($scope.actionButtons, actionButton);
             $scope.actionButtons.splice(actionButtonIndex, 1);

             ngToast.create({
             content: $filter('translate')('Setup.Modules.ActionButtonDeleteSuccess'),
             className: 'success'
             });
             })
             .catch(function () {
             $scope.actionButtons = $scope.actionButtonState;

             if ($scope.formModal) {
             $scope.formModal.hide();
             $scope.saving = false;
             }
             });
             };

             $scope.cancel = function () {
             angular.forEach($scope.currentActionButton, function (value, key) {
             $scope.currentActionButton[key] = $scope.currentActionButtonState[key];
             });

             $scope.formModal.hide();
             }*/
        }
    ]);