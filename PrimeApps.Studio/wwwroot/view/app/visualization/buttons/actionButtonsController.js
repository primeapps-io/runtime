'use strict';

angular.module('primeapps')

    .controller('ActionButtonsController', ['$rootScope', '$scope', '$filter', '$modal', 'helper', '$cache', 'ModuleService', '$location', 'ActionButtonsService', '$localStorage',
        function ($rootScope, $scope, $filter, $modal, helper, $cache, ModuleService, $location, ActionButtonsService, $localStorage) {

            $scope.id = $location.search().id ? $location.search().id : 0;
            if ($scope.id > 0)
                $scope.$parent.$parent.$parent.$parent.openSubMenu('visualization');

            $rootScope.breadcrumblist[2].title = 'Buttons';
            $scope.$parent.activeMenuItem = 'buttons';
            $scope.environments = angular.copy(ActionButtonsService.getEnvironments());
            $scope.actionButtons = [];

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

            $scope.moduleChanged = function (isNew, actionButton) {
                if ($scope.currentActionButton.module) {
                    $scope.module = $scope.currentActionButton.module;
                    webhookParameters(isNew, actionButton);
                }
            };

            $scope.showFormModal = function (actionButton) {

                if (!actionButton) {
                    actionButton = {};
                    actionButton.type = 3;
                    actionButton.triggerType = 1;
                    actionButton.isNew = true;
                    $scope.currentActionButton = actionButton;
                    $scope.hookParameters = [];
                    $scope.hookHeaders = [];
                    $scope.hookModules = [];
                    var parameter = {};
                    parameter.parameterName = null;
                    parameter.selectedModules = $scope.hookModules;
                    parameter.selectedField = null;
                    emptyEnvironment();
                    $scope.environments[0].selected = true;
                    $scope.hookParameters.push(parameter);

                    setWebHookHeaders();

                } else {
                   
                    $scope.currentActionButton = actionButton;
                    $scope.currentActionButton.action_button_name = actionButton['name_en'];
                    $scope.currentActionButton.action_button_url = actionButton.url;
                    $scope.currentActionButton.module = actionButton.parent_module || actionButton.module;
                    $scope.currentActionButton.trigger = actionButton.trigger_clone;
                    $scope.currentActionButton.action_type = actionButton.type;

                    if (actionButton.environment && actionButton.environment.indexOf(',') > -1)
                        $scope.currentActionButton.environments = actionButton.environment.split(',');
                    else
                        $scope.currentActionButton.environments = actionButton.environment;

                    if ($scope.currentActionButton.environments === '' || !$scope.currentActionButton.environments)
                        emptyEnvironment();
                    else
                        angular.forEach($scope.currentActionButton.environments, function (envValue) {
                            $scope.environmentChange($scope.environments[envValue - 1], envValue - 1, true);
                        });

                    $scope.moduleChanged(false, actionButton);
                }
                $scope.currentActionButtonState = angular.copy($scope.currentActionButton);
                $scope.actionButtonTypes = [
                    {
                        text: "Modal",
                        name: "ModalFrame",
                        value: 3
                    },
                    {
                        text: "Webhook",
                        name: "Webhook",
                        value: 2
                    },
                    {
                        text: "Script",
                        name: "Script",
                        value: 1
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
                        name: $filter('translate')('Setup.Modules.List'),
                        value: 4
                    },
                    {
                        name: $filter('translate')('Setup.Modules.Relation'),
                        value: 5
                    },
                    {
                        name: $filter('translate')('Setup.Modules.All'),
                        value: 3
                    }
                ];

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

                if ($scope.id) {
                    var module = $filter('filter')($rootScope.appModules, { id: parseInt($scope.id) }, true)[0];
                    $scope.currentActionButton.module = module;
                }

                var addNewPermissions = function (actionButton) {
                    $scope.actionButtonPermission = [];
                    if (actionButton.isNew)
                        actionButton.permissions = [];

                    angular.forEach($rootScope.appProfiles, function (profile) {
                        if (profile.deleted)
                            return;

                        if (profile.is_persistent && profile.has_admin_rights)
                            profile.name = $filter('translate')('Setup.Profiles.Administrator');

                        if (profile.is_persistent && !profile.has_admin_rights)
                            profile.name = $filter('translate')('Setup.Profiles.Standard');

                        $scope.actionButtonPermission.push({ profile_id: profile.id, profile_name: profile.name, type: 'full', profile_is_admin: profile.has_admin_rights });
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
                            var profile = $filter('filter')($rootScope.appProfiles, { id: permission.profile_id }, true)[0];
                            permission.profile_name = profile.name;
                            permission.profile_is_admin = profile.has_admin_rights;
                        });
                    }
                    else {
                        addNewPermissions(actionButton);
                    }
                }
            };

            $scope.save = function (actionButtonForm) {
                $scope.actionButtons = $scope.grid.dataSource.data();

                if (!actionButtonForm.$valid) {
                    if (actionButtonForm.$error.required)
                        toastr.error($filter('translate')('Setup.Modules.RequiredError'));

                    return;
                }

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

                    var hookHeaderArray = [];

                    angular.forEach($scope.hookHeaders, function (header) {
                        if (!header.key || !header.type || !header.value)
                            return;
                        var headerString = header.type + '|' + $scope.currentActionButton.module.name + '|' + header.key;

                        switch (header.type) {

                            case 'module':
                                headerString += '|' + header.value.name;
                                break;
                            case 'custom':
                            case 'static':
                            default:
                                headerString += '|' + header.value;
                                break;
                        }

                        hookHeaderArray.push(headerString);
                    });

                    if (hookHeaderArray.length > 0) {
                        actionButton.headers = hookHeaderArray.toString();
                    }

                } else {
                    actionButton.parameters = null;
                    actionButton.headers = null;
                    actionButton.method_type = null;
                }

                actionButton.environments = [];
                angular.forEach($scope.environments, function (env) {
                    if (env.selected)
                        actionButton.environments.push(env.value);
                });

                delete actionButton.environment;
                delete actionButton.environment_list;

                //TODOOO

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
                            //$scope.pageTotal++;
                            //$scope.changePage($scope.activePage);
                            toastr.success($filter('translate')('Setup.Modules.ActionButtonSaveSuccess'));
                            $scope.grid.dataSource.read();

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
                            // $scope.changePage($scope.activePage);
                            toastr.success($filter('translate')('Setup.Modules.ActionButtonSaveSuccess'));
                            $scope.grid.dataSource.read();
                        }).catch(function () {
                            $scope.actionButtons = $scope.actionbuttonState;

                            if ($scope.formModal) {
                                $scope.formModal.hide();
                                $scope.saving = false;
                            }
                        });
                }
            };

            $scope.delete = function (actionButton, event) {
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
                                    toastr.success($filter('translate')('Setup.Modules.ActionButtonDeleteSuccess'));
                                    $scope.grid.dataSource.read();
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
                emptyEnvironment();
                $scope.formModal.hide();
            };

            var emptyEnvironment = function () {
                $scope.environments = [];
                $scope.environments = angular.copy(ActionButtonsService.getEnvironments());
            }

            //Webhook
            var webhookParameters = function (isNew, actionButton) {
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

                    if (isNew) {
                        var parameter = {};
                        parameter.parameterName = null;
                        parameter.selectedModules = $scope.hookModules;
                        parameter.selectedField = null;
                        $scope.hookParameters.push(parameter);
                    } else
                        conditions(actionButton);

                });
            };


            $scope.webhookParameterAdd = function (addItem) {

                var parameter = {};
                parameter.parameterName = addItem.parameterName;
                parameter.selectedModules = addItem.selectedModules;
                parameter.selectedField = addItem.selectedField;

                if (parameter.parameterName && parameter.selectedModules && parameter.selectedField) {
                    if ($scope.hookParameters.length <= 20) {
                        $scope.hookParameters.push(parameter);
                    } else {
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

            $scope.webHookHeaderAdd = function (addItem) {

                var header = {};
                header.type = addItem.type;
                header.key = addItem.key;
                header.value = addItem.value;

                if (header.type && header.key && header.value) {
                    $scope.hookHeaders.push(header);
                }

                var lastHookHeader = $scope.hookHeaders[$scope.hookHeaders.length - 1];
                lastHookHeader.type = null;
                lastHookHeader.key = null;
                lastHookHeader.value = null;
            };

            $scope.webHookHeaderRemove = function (item) {
                var index = $scope.hookHeaders.indexOf(item);
                $scope.hookHeaders.splice(index, 1);
            };

            var setWebHookHeaders = function () {
                $scope.hookHeaders = [];

                var header = {};
                header.type = null;
                header.key = null;
                header.value = null;

                $scope.hookHeaders.push(header);
            };

            var conditions = function (actionButton) {

                if (actionButton.action_type === 'Scripting')
                    actionButton.type = 1;

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

                if (actionButton.trigger === 'Relation')
                    actionButton.triggerType = 5;

                if (actionButton.type === 3) {
                    $scope.hookParameters = [];
                    $scope.hookHeaders = [];
                    $scope.hookModules = [];

                    angular.forEach($scope.updatableModules, function (module) {
                        $scope.hookModules.push(module);
                    });

                    var parameter = {};
                    parameter.parameterName = null;
                    parameter.selectedModules = $scope.hookModules;
                    parameter.selectedField = null;

                    $scope.hookParameters.push(parameter);

                    setWebHookHeaders();
                }

                if (actionButton.method_type && actionButton.parameters && actionButton.type === 2) {
                    // $timeout(function () {

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
                        } else {
                            var lookupModuleName = $filter('filter')($scope.module.fields, { name: parameter[1] }, true)[0].lookup_type;
                            selectedModule = $filter('filter')(editParameter.selectedModules, { name: lookupModuleName }, true)[0];
                        }

                        if (!selectedModule)
                            return;

                        editParameter.selectedModule = selectedModule;
                        editParameter.selectedField = $filter('filter')(editParameter.selectedModule.fields, { name: parameter[2] }, true)[0];

                        $scope.hookParameters.push(editParameter);
                    });

                    $scope.hookHeaders = [];
                    var hookHeaderArray = actionButton.headers.split(',');

                    angular.forEach(hookHeaderArray, function (data) {
                        var header = data.split('|');
                        var tempHeader = {};

                        tempHeader.type = header[0];
                        var moduleName = header[1];
                        tempHeader.key = header[2];
                        var value = header[3];

                        if (tempHeader.type === 'module') {
                            if ($scope.module.name === moduleName) {
                                var field = $filter('filter')($scope.module.fields, { name: value }, true)[0];
                                tempHeader.value = field;
                            }
                        }
                        else {
                            tempHeader.value = value;
                        }

                        $scope.hookHeaders.push(tempHeader);
                    });
                    // }, 1000);
                }
            };

            //For Kendo UI
            $scope.goUrl = function (actionButton) {
                var selection = window.getSelection();
                if (selection.toString().length === 0) {
                    $scope.showFormModal(actionButton); //click event.
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
                            url: "/api/action_button/find/" + $scope.id,
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
                                Module: { type: "string" },
                                Name: { type: "string" },
                                Type: { type: "enums" },
                                Trigger: { type: "enums" }
                            }
                        },
                        parse: function (data) {
                            for (var i = 0; i < data.items.length; i++) {
                                data.items[i].trigger_clone = angular.copy(data.items[i].trigger);
                                delete data.items[i].trigger;
                            }

                            return data;
                        }
                    }

                },
                scrollable: false,
                persistSelection: true,
                sortable: true,
                filterable: true,
                filter: function (e) {
                    if (e.filter && e.field !== 'Type' && e.field !== 'Trigger') {
                        for (var i = 0; i < e.filter.filters.length; i++) {
                            e.filter.filters[i].ignoreCase = true;
                        }
                    }
                },
                rowTemplate: function (e) {
                    var trTemp = '<tr ng-click="goUrl(dataItem)">';
                    trTemp += '<td class="text-left"><span>' + e.module['label_' + $scope.language + '_plural'] + '</span></td>';
                    trTemp += '<td class="text-left"><span>' + e['name_' + $scope.language] + '</span></td>';
                    trTemp += e.type === 'Scripting' ? '<td class="text-capitalize"> <span>Script</span></td > ' : e.type === 'ModalFrame' ? '<td class="text-capitalize"> <span>Modal</span></td > ' : '<td class="text-capitalize"> <span>' + e.type + '</span></td > ';
                    trTemp += '<td class="text-capitalize"> <span>' + e.trigger_clone + '</span></td > ';
                    trTemp += '<td ng-click="$event.stopPropagation();"> <button ng-click="$event.stopPropagation(); delete(dataItem, $event);" type="button" class="action-button2-delete"><i class="fas fa-trash"></i></button></td></tr>';
                    return trTemp;
                },
                altRowTemplate: function (e) {
                    var trTemp = '<tr ng-click="goUrl(dataItem)">';
                    trTemp += '<td class="text-left"><span>' + e.module['label_' + $scope.language + '_plural'] + '</span></td>';
                    trTemp += '<td class="text-left"><span>' + e['name_' + $scope.language] + '</span></td>';
                    trTemp += e.type === 'Scripting' ? '<td class="text-capitalize"> <span>Script</span></td > ' : e.type === 'ModalFrame' ? '<td class="text-capitalize"> <span>Modal</span></td > ' : '<td class="text-capitalize"> <span>' + e.type + '</span></td > ';
                    trTemp += '<td class="text-capitalize"> <span>' + e.trigger_clone + '</span></td > ';
                    trTemp += '<td ng-click="$event.stopPropagation();"> <button ng-click="$event.stopPropagation(); delete(dataItem, $event);" type="button" class="action-button2-delete"><i class="fas fa-trash"></i></button></td></tr>';
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
                        field: 'Module.LabelEnPlural',
                        title: $filter('translate')('Setup.Modules.Name'),
                        headerAttributes: {
                            'class': 'text-left'
                        },
                    },
                    {
                        field: 'Name' + $scope.language,
                        title: $filter('translate')('Setup.Modules.ActionButtonLabel'),
                        headerAttributes: {
                            'class': 'text-left'
                        },
                    },
                    {
                        field: 'Type',
                        title: $filter('translate')('Setup.Modules.ActionType'),
                        values: [
                            { text: 'Modal', value: 'ModalFrame' },
                            { text: 'Webhook', value: 'Webhook' },
                            { text: 'Script', value: 'Scripting' }]
                    },
                    {
                        field: 'Trigger',
                        title: $filter('translate')('Setup.Modules.DisplayPage'),
                        values: [
                            { text: 'Detail', value: 'Detail' },
                            { text: 'All', value: 'All' },
                            { text: 'List', value: 'List' },
                            { text: 'Relation', value: 'Relation' }]
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