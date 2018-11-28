'use strict';

angular.module('primeapps')

    .controller('ActionButtonsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', 'helper', '$cache', 'systemRequiredFields', 'systemReadonlyFields', 'ModuleSetupService', 'ModuleService', 'AppService',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, helper, $cache, systemRequiredFields, systemReadonlyFields, ModuleSetupService, ModuleService, AppService) {
            var module = $filter('filter')($rootScope.modules, { name: $stateParams.module }, true)[0];

            if (!module) {
                ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
                $state.go('app.dashboard');
                return;
            }

            $scope.module = angular.copy(module);

            var getActionButtons = function (refresh) {
                ModuleService.getActionButtons($scope.module.id, refresh)
                    .then(function (actionButtons) {
                        $scope.actionButtons = $filter('filter')(actionButtons, { action_type: '!Scripting' }, true);
                        $scope.actionButtonState = angular.copy($scope.actionButtons);
                    });
            };

            getActionButtons();

            $scope.showFormModal = function (actionButton) {
                if (!actionButton) {
                    actionButton = {};
                    actionButton.type = 3;
                    actionButton.triggerType = 1;
                    actionButton.isNew = true;
                }

                $scope.actionButtonTypes = [
                    {
                        type: 'Modal',
                        value: 3
                    },
                    {
                        type: 'Webhook',
                        value: 2
                    }
                ];

                $scope.displayPages = [
                    {
                        name: $filter('translate')('Setup.Modules.Detail'),
                        value: 1
                    },
                    {
                        name: 'Form',
                        value: 2
					},
					{
						name: $filter('translate')('Setup.Modules.List'),
						value: 4
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

				if (actionButton.trigger === 'List')
					actionButton.triggerType = 4;

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

                var addNewPermissions = function (actionButton) {
                    $scope.actionButtonPermission = [];
                    if (actionButton.isNew)
                        actionButton.permissions = [];

                    angular.forEach($rootScope.profiles, function (profile) {
                        if (profile.IsPersistent && profile.HasAdminRights)
                            profile.Name = $filter('translate')('Setup.Profiles.Administrator');

                        if (profile.IsPersistent && !profile.HasAdminRights)
                            profile.Name = $filter('translate')('Setup.Profiles.Standard');

                        $scope.actionButtonPermission.push({ profile_id: profile.Id, profile_name: profile.Name, type: 'full', profile_is_admin: profile.HasAdminRights });
                    });
                };

                if (!actionButton.isNew) {
                    addNewPermissions(actionButton);
                    if ($scope.actionButtonPermission.length != actionButton.permissions.length) {
                        for (var i = actionButton.permissions.length; i < $scope.actionButtonPermission.length; i++) {
                            actionButton.permissions.push($scope.actionButtonPermission[i]);
                        }
                    }
                }

                if (actionButton.isNew) {
                    addNewPermissions(actionButton);
                    actionButton.permissions = $scope.actionButtonPermission;
                }
                else {
                    if (actionButton.permissions && actionButton.permissions.length > 0) {
                        angular.forEach(actionButton.permissions, function (permission) {
                            var profile = $filter('filter')($rootScope.profiles, { id: permission.profile_id }, true)[0];
                            permission.profile_name = profile.name;
                            permission.profile_is_admin = profile.has_admin_rights
                        });
                    }
                    else {
                        addNewPermissions(actionButton);
                    }
                }

                $scope.currentActionButton = actionButton;
                $scope.currentActionButtonState = angular.copy($scope.currentActionButton);

                $scope.formModal = $scope.formModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/setup/modules/actionButtonForm.html',
                    animation: '',
                    backdrop: 'static',
                    show: false
                });

                $scope.formModal.$promise.then(function () {
                    $scope.formModal.show();
                });
            };

            $scope.save = function (actionButtonForm, profile) {
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
                }
                else {
                    actionButton.parameters = null;
                    actionButton.method_type = "";
                }

                if (!actionButton.css_class)
                    actionButton.css_class = 'btn-sm btn-custom';

                var success = function () {
                    getActionButtons(true);

                    ngToast.create({
                        content: $filter('translate')('Setup.Modules.ActionButtonSaveSuccess'),
                        className: 'success'
                    });

                    $scope.saving = false;
                    $scope.formModal.hide();
                };

                if (!actionButton.id) {
                    ModuleSetupService.createActionButton(actionButton)
                        .then(function (response) {
                            if (!$scope.actionButtons)
                                $scope.actionButtons = [];

                            actionButton.action_type = response.data.type;
                            actionButton.trigger = response.data.trigger;
                            actionButton.id = response.data.id;

                            success();
                        })
                        .catch(function () {
                            $scope.actionButtons = $scope.actionButtonState;

                            if ($scope.formModal) {
                                $scope.formModal.hide();
                                $scope.saving = false;
                            }
                        });
                } else {
                    ModuleSetupService.updateActionButton(actionButton)
                        .then(function () {
                            success();
                        })
                        .catch(function () {
                            $scope.actionButtons = $scope.actionButtonState;

                            if ($scope.formModal) {
                                $scope.formModal.hide();
                                $scope.saving = false;
                            }
                        });
                }
            };

            $scope.delete = function (actionButton) {
                delete actionButton.$$hashKey;

                ModuleSetupService.deleteActionButton(actionButton.id)
                    .then(function () {
                        getActionButtons(true);
                        $scope.saving = false;
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
            };

            //Webhook
            var webhookParameters = function () {
                $scope.updatableModules = [];
                $scope.updatableModules.push($scope.module);

                angular.forEach($scope.module.fields, function (field) {
                    if (field.lookup_type && field.lookup_type != $scope.module.name && field.lookup_type != 'users' && !field.deleted) {
                        var module = $filter('filter')($rootScope.modules, { name: field.lookup_type }, true)[0];
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
            };

            webhookParameters();

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
                        ngToast.create({
                            content: $filter('translate')('Setup.Workflow.MaximumHookWarning'),
                            className: 'warning'
                        });
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