'use strict';

angular.module('primeapps')

    .controller('ActionButtonsController', ['$rootScope', '$scope', '$filter', '$modal', 'helper', '$cache', 'ModuleService', '$location', 'ActionButtonsService',
        function ($rootScope, $scope, $filter, $modal, helper, $cache, ModuleService, $location, ActionButtonsService) {

            $rootScope.breadcrumblist[2].title = 'Buttons';
            $scope.$parent.activeMenuItem = 'buttons';

            $scope.actionButtons = [];
            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }
            };
            $scope.generator(10);

            $scope.loading = true;
            $scope.id = $location.search().id ? $location.search().id : 0;

            $scope.requestModel = {
                limit: "10",
                offset: 0
            };

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;
                ActionButtonsService.find($scope.id, requestModel).then(function (response) {
                    $scope.actionButtons = response.data;
                    for (var i = 0; i < response.data.length; i++) {
                        $scope.actionButtons[i].parent_module = $filter('filter')($rootScope.appModules, { id: response.data[i].module_id }, true)[0];
                        $scope.actionButtons[i].action_type = $scope.actionButtons[i].type;
                    }
                    $scope.actionbuttonState = angular.copy($scope.actionButtons);
                    $scope.loading = false;
                });
            };

            $scope.changeOffset = function () {
                $scope.changePage(1)
            };


            // if ($scope.id) {
            ActionButtonsService.count($scope.id).then(function (response) {
                $scope.pageTotal = response.data;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = requestModel.offset-1;
                ActionButtonsService.find($scope.id, requestModel)
                    .then(function (actionButtons) {
                        $scope.actionButtons = actionButtons.data;
                        for (var i = 0; i < actionButtons.data.length; i++) {
                            $scope.actionButtons[i].parent_module = $filter('filter')($rootScope.appModules, { id: actionButtons.data[i].module_id }, true)[0];
                            $scope.actionButtons[i].action_type = $scope.actionButtons[i].type;
                        }
                        $scope.actionbuttonState = angular.copy($scope.actionButtons);
                        $scope.loading = false;
                    });
            });
            // }


            $scope.moduleChanged = function () {
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

                if (actionButton.method_type && actionButton.parameters && actionButton.type === 2) {
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
                    templateUrl: 'view/app/visualization/buttons/actionButtonForm.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
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

               // $scope.id = $scope.module.id;

                actionButton.module_id = $scope.module.id;
                actionButton.template = 'template';

                actionButton.trigger = actionButton.triggerType;

                delete actionButton.triggerType;

                if (actionButton.type === 2) {
                    var hookArray = [];
                    angular.forEach($scope.hookParameters, function (hookParameter) {
                        var moduleName;
                        if ($scope.module.name !== hookParameter.selectedModule.name)
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
                    ModuleService.createActionButton(actionButton)
                        .then(function (response) {
                            if (!$scope.actionButtons)
                                $scope.actionButtons = [];

                            actionButton.action_type = response.data.type;
                            actionButton.trigger = response.data.trigger;
                            actionButton.id = response.data.id;
                            actionButton.parent_module = actionButton.module;

                            $scope.saving = false;
                            $scope.formModal.hide();
                            $scope.changePage(1);
                            toastr.success($filter('translate')('Setup.Modules.ActionButtonSaveSuccess'));


                        }).catch(function () {
                        $scope.actionButtons = $scope.actionbuttonState;

                        if ($scope.formModal) {
                            $scope.formModal.hide();
                            $scope.saving = false;
                        }
                    });
                } else {
                    ModuleService.updateActionButton(actionButton)
                        .then(function (response) {
                            $scope.saving = false;
                            $scope.formModal.hide();
                            $scope.changePage(1);
                            toastr.success($filter('translate')('Setup.Modules.ActionButtonSaveSuccess'));

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
                // delete actionButton.$$hashKey;
                // var deleteModel = angular.copy($scope.actionButtons);
                // var actionButtonIndex = helper.arrayObjectIndexOf(deleteModel, actionButton);
                // deleteModel.splice(actionButtonIndex, 1);
                var willDelete =
                    swal({
                        title: "Are you sure?",
                        text: " ",
                        icon: "warning",
                        buttons: ['Cancel', 'Yes'],
                        dangerMode: true
                    }).then(function (value) {
                        if (value) {
                            ModuleService.deleteActionButton(actionButton.id)
                                .then(function () {
                                    // var actionButtonIndex = helper.arrayObjectIndexOf($scope.actionButtons, actionButton);
                                    // $scope.actionButtons.splice(actionButtonIndex, 1);
                                    $scope.changePage(1);
                                    toastr.success($filter('translate')('Setup.Modules.ActionButtonDeleteSuccess'));
                                })
                                .catch(function () {
                                    $scope.actionButtons = $scope.actionButtonState;

                                    if ($scope.formModal) {
                                        $scope.formModal.hide();
                                        $scope.saving = false;
                                    }
                                });
                        }
                    });
            };

            $scope.cancel = function () {
                angular.forEach($scope.currentActionButton, function (value, key) {
                    $scope.currentActionButton[key] = $scope.currentActionButtonState[key];
                });

                $scope.formModal.hide();
            };

            //Webhook
            var webhookParameters = function () {
                $scope.updatableModules = [];
                $scope.updatableModules.push($scope.module);

                ModuleService.getModuleFields($scope.module.name).then(function (fields) {

                    $scope.module.fields = fields.data;

                    angular.forEach($scope.module.fields, function (field) {
                        if (field.lookup_type && field.lookup_type !== $scope.module.name && field.lookup_type !== 'users' && !field.deleted) {
                            var module = $filter('filter')($rootScope.appModules, { name: field.lookup_type }, true)[0];
                            $scope.updatableModules.push(module);
                        }
                    });

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
                });
            };


            $scope.webhookParameterAdd = function (addItem) {

                var parameter = {};
                parameter.parameterName = addItem.parameterName;
                parameter.selectedModules = addItem.selectedModules;
                parameter.selectedField = addItem.selectedField;

                if (parameter.parameterName && parameter.selectedModules && parameter.selectedField) {
                    if ($scope.hookParameters.length <= 10) {
                        $scope.hookParameters.push(parameter);
                    }
                    else {
                        toastr.warning($filter('translate')('Setup.Workflow.MaximumHookWarning'));
                    }
                }
                var lastHookParameter = $scope.hookParameters[$scope.hookParameters.length - 1];
                lastHookParameter.parameterName = null;
                lastHookParameter.selectedField = null;

            };

            $scope.webhookParameterRemove = function (itemname) {
                var index = $scope.hookParameters.indexOf(itemname);
                $scope.hookParameters.splice(index, 1);
            };
        }
    ]);