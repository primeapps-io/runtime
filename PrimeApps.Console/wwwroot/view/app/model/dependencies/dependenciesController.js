'use strict';

angular.module('primeapps')

    .controller('DependenciesController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', 'helper', '$cache', 'systemRequiredFields', 'systemReadonlyFields', 'DependenciesService', 'LayoutService', 'ModuleService', '$timeout',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, helper, $cache, systemRequiredFields, systemReadonlyFields, DependenciesService, LayoutService, ModuleService, $timeout) {

            $scope.$parent.menuTopTitle = "Models";
            $scope.$parent.activeMenu = "model";
            $scope.$parent.activeMenuItem = "dependencies";
            $scope.loading = true;
            $scope.picklist = [];
            $scope.picklistsModule = {};

            $scope.requestModel = {
                limit: '10',
                offset: 1
            };

            DependenciesService.count().then(function (response) {
                $scope.pageTotal = response.data;
            });
            DependenciesService.find($scope.requestModel).then(function (response) {
                var dependencies = response.data;
                $scope.dependencies = DependenciesService.processDependencies(dependencies);
                $scope.dependenciesState = $scope.dependencies;
                $scope.loading = false;
            });

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;

                DependenciesService.find(requestModel).then(function (response) {
                    var dependencies = response.data;
                    $scope.dependencies = DependenciesService.processDependencies(dependencies);
                    $scope.dependenciesState = $scope.dependencies;
                    $scope.loading = false;
                });
            };

            $scope.changeOffset = function () {
                $scope.changePage(1)
            };

            $scope.moduleChanged = function () {
                var moduleName = $scope.currentDependency.module.name;
                $scope.sections = $scope.currentDependency.module.sections;
                ModuleService.getModuleByName(moduleName).then(function (response) {
                    $scope.module = response.data;
                    getFields();
                    getDependencyTypes();
                    getValueChangeTypes();
                    var dependency = $scope.currentDependency;
                    if (!dependency.isNew) {
                        var childField = $filter('filter')($scope.module.fields, { name: dependency.childField.name }, true)[0];
                        var sectionField = $filter('filter')($scope.module.sections, { name: dependency.sectionField.name }, true)[0];
                        $scope.affectedAreaType = dependency.child_section ? 'section' : 'field';

                        var childValueListFieldsExist = $filter('filter')($scope.childValueListFields, { name: childField.name }, true)[0];
                        if (!childValueListFieldsExist)
                            $scope.childValueListFields.push(childField);

                        var childValueTextFieldsExist = $filter('filter')($scope.childValueTextFields, { name: childField.name }, true)[0];
                        if (!childValueTextFieldsExist)
                            $scope.childValueTextFields.push(childField);

                        var childDisplayFieldExist = $filter('filter')($scope.childDisplayFields, { name: childField.name }, true)[0];
                        if (!childDisplayFieldExist)
                            $scope.childDisplayFields.push(childField);

                        $scope.getPicklist();
                    }
                });
            };
            $scope.affectedAreaType = "field";

            var getFields = function () {
                $scope.parentDisplayFields = [];
                $scope.parentValueFields = [];
                $scope.childValueListFields = [];
                $scope.childValueTextFields = [];
                $scope.childDisplayFields = [];
                $scope.picklistFields = [];

                angular.forEach($scope.module.fields, function (field) {

                    var existDisplayDependency = $filter('filter')($scope.dependencies, { childField: { name: field.name }, dependencyType: 'display' }, true)[0];
                    var existValueDependency = $filter('filter')($scope.dependencies, { childField: { name: field.name }, dependencyType: 'value' }, true)[0];

                    if (!existDisplayDependency)
                        $scope.childDisplayFields.push(field);

                    if (!existValueDependency) {
                        switch (field.data_type) {
                            case 'picklist':
                                $scope.parentDisplayFields.push(field);
                                $scope.parentValueFields.push(field);
                                $scope.childValueListFields.push(field);
                                break;
                            case 'multiselect':
                                $scope.parentDisplayFields.push(field);
                                $scope.childValueListFields.push(field);
                                break;
                            case 'checkbox':
                                $scope.parentDisplayFields.push(field);
                                $scope.childValueListFields.push(field);
                                break;
                            case 'text_single':
                            case 'text_multi':
                            case 'number':
                            case 'number_decimal':
                            case 'currency':
                            case 'email':
                                $scope.childValueTextFields.push(field);
                                break;
                        }
                    }

                    if (field.data_type === 'picklist')
                        $scope.picklistFields.push(field);
                });

            };

            var getDependencyTypes = function () {
                var dependencyTypeDisplay = {};
                dependencyTypeDisplay.value = 'display';
                dependencyTypeDisplay.label = $filter('translate')('Setup.Modules.DependencyTypeDisplay');

                var dependencyTypeValueChange = {};
                dependencyTypeValueChange.value = 'value';
                dependencyTypeValueChange.label = $filter('translate')('Setup.Modules.DependencyTypeValueChange');

                $scope.dependencyTypes = [];
                $scope.dependencyTypes.push(dependencyTypeDisplay);
                $scope.dependencyTypes.push(dependencyTypeValueChange);
            };

            var getValueChangeTypes = function () {
                var valueChangeTypeStandard = {};
                valueChangeTypeStandard.value = 'list_text';
                valueChangeTypeStandard.label = $filter('translate')('Setup.Modules.ValueChangeTypeStandard');

                var valueChangeTypeValueMapping = {};
                valueChangeTypeValueMapping.value = 'list_value';
                valueChangeTypeValueMapping.label = $filter('translate')('Setup.Modules.ValueChangeTypeValueMapping');

                var valueChangeTypeFieldMapping = {};
                valueChangeTypeFieldMapping.value = 'list_field';
                valueChangeTypeFieldMapping.label = $filter('translate')('Setup.Modules.ValueChangeTypeFieldMapping');

                $scope.valueChangeTypes = [];
                $scope.valueChangeTypes.push(valueChangeTypeStandard);
                $scope.valueChangeTypes.push(valueChangeTypeValueMapping);
                $scope.valueChangeTypes.push(valueChangeTypeFieldMapping);
            };

            $scope.dependencyTypeChanged = function () {
                if ($scope.currentDependency.dependencyType === 'value') {
                    $scope.currentDependency.type = 'list_text';
                }

                $scope.currentDependency.parent_field = null;
                $scope.currentDependency.child_field = null;
                $scope.currentDependency.child_section = null;
            };

            $scope.valueChangeTypeChanged = function () {
                switch ($scope.currentDependency.type) {
                    case 'list_value':
                        $scope.currentDependency.value_maps = {};
                        break;
                    case 'list_field':
                        $scope.currentDependency.field_map = {};
                        break;
                }
            };

            $scope.getParentFields = function () {
                switch ($scope.currentDependency.dependencyType) {
                    case 'display':
                        return $scope.parentDisplayFields;
                    case 'value':
                        if ($scope.currentDependency.type === 'list_value')
                            return $scope.picklistFields;

                        else
                            return $scope.parentValueFields;
                }
            };

            $scope.getChildFields = function () {

                switch ($scope.currentDependency.dependencyType) {
                    case 'display':
                        angular.forEach($scope.childDisplayFields, function (field) {
                            delete field.hidden;

                            //Silinen alanlar alan bagimliklarinda gelmeye devam ediyordu.
                            if (field.name === $scope.currentDependency.parent_field || field.deleted)
                                field.hidden = true;
                        });
                        return $scope.childDisplayFields;

                    case 'value':
                        if ($scope.currentDependency.type === 'list_text') {
                            angular.forEach($scope.childValueTextFields, function (field) {
                                delete field.hidden;

                                if (field.name === $scope.currentDependency.parent_field)
                                    field.hidden = true;
                            });

                            return $scope.childValueTextFields;
                        }
                        else {
                            angular.forEach($scope.childValueListFields, function (field) {
                                delete field.hidden;

                                if (field.name === $scope.currentDependency.parent_field)
                                    field.hidden = true;
                            });

                            return $scope.childValueListFields;
                        }
                }
            };

            $scope.getMappingOptions = function () {
                var parentField = $filter('filter')($scope.module.fields, { name: $scope.currentDependency.parent_field }, true)[0];
                var childField = $filter('filter')($scope.module.fields, { name: $scope.currentDependency.child_field }, true)[0];
                var childSection = $filter('filter')($scope.module.sections, { name: $scope.currentDependency.child_section }, true)[0];
                $scope.parentPicklist = [];
                if (parentField.picklist_id) {
                    DependenciesService.getPicklist(parentField.picklist_id).then(function (picklists) {
                            var copyPicklist = angular.copy(picklists.data.items);
                            $scope.parentPicklist = $filter('filter')(copyPicklist, { inactive: '!true' });
                            if (childField.picklist_id) {
                                DependenciesService.getPicklist(childField.picklist_id).then(function (picklists) {
                                    var copyPicklist = angular.copy(picklists.data.items);
                                    var childPicklist = $filter('filter')(copyPicklist, { inactive: '!true' });
                                    angular.forEach($scope.parentPicklist, function (picklistItem) {
                                        picklistItem.childPicklist = childPicklist;
                                    });
                                });
                            }
                            return $scope.parentPicklist;
                        }
                    );
                }
            };

            var setCurrentDependency = function (dependency) {
                $scope.currentDependency = dependency;
                $scope.currentDependency.hasRelationField = true;
                $scope.currentDependency.module = dependency.parent_module;
                $scope.currentDependencyState = angular.copy($scope.currentDependency);
            };

            $scope.showFormModal = function (dependency) {
                if (!dependency) {
                    dependency = {};
                    dependency.dependencyType = 'display';
                    dependency.isNew = true;
                    setCurrentDependency(dependency);
                }
                else {
                    setCurrentDependency(dependency);
                    $scope.moduleChanged();
                }

                $scope.addNewDependencyModal = $scope.addNewDependencyModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/setup/modules/dependencyForm.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.addNewDependencyModal.$promise.then(function () {
                    $scope.addNewDependencyModal.show();
                });
            };

            $scope.save = function (dependencyForm) {
                if (!dependencyForm.$valid)
                    return;

                $scope.saving = true;
                var dependency = angular.copy($scope.currentDependency);
                if (dependency.isNew) {
                    delete dependency.isNew;

                    if (!$scope.dependencies)
                        $scope.dependencies = [];

                    $scope.dependencies.push(dependency);
                }
                var relationModel = DependenciesService.prepareDependency(angular.copy(dependency), $scope.module);

                var success = function () {
                    $scope.loading = true;
                    ngToast.create({ content: $filter('translate')('Setup.Modules.DependencySaveSuccess'), className: 'success' });
                    $scope.saving = false;
                    $scope.addNewDependencyModal.hide();
                    DependenciesService.find($scope.requestModel).then(function (response) {
                        var dependencies = response.data;
                        $scope.dependencies = DependenciesService.processDependencies(dependencies);
                        $scope.dependenciesState = $scope.dependencies;
                        $scope.loading = false;
                    });
                };

                var error = function () {
                    $scope.dependencies = $scope.dependenciesState;

                    if ($scope.addNewDependencyModal) {
                        $scope.addNewDependencyModal.hide();
                        $scope.saving = false;
                    }
                };

                if (!relationModel.id) {
                    DependenciesService.createModuleDependency(relationModel, $scope.module.id)
                        .then(function () {
                            success();
                        })
                        .catch(function () {
                            error();
                        });
                }
                else {
                    DependenciesService.updateModuleDependency(relationModel, $scope.module.id)
                        .then(function () {
                            success();
                        })
                        .catch(function () {
                            error();
                        });
                }
            };

            $scope.delete = function (dependency) {

                DependenciesService.deleteModuleDependency(dependency.id)
                    .then(function () {
                        var dependencyIndex = helper.arrayObjectIndexOf($scope.dependencies, dependency);
                        $scope.dependencies.splice(dependencyIndex, 1);
                        ngToast.create({ content: $filter('translate')('Setup.Modules.DependencyDeleteSuccess'), className: 'success' });
                    })
                    .catch(function () {
                        $scope.dependencies = $scope.dependenciesState;

                        if ($scope.addNewDependencyModal) {
                            $scope.addNewDependencyModal.hide();
                            $scope.saving = false;
                        }
                    });
            };

            $scope.parentValueChanged = function () {
                $scope.currentDependency.values = [];
                $scope.getPicklist();
            };

            $scope.getPicklist = function () {
                $scope.picklist = [];
                var parentField = $filter('filter')($scope.module.fields, { name: $scope.currentDependency.parent_field }, true)[0];
                if (parentField.picklist_id) {
                    DependenciesService.getPicklist(parentField.picklist_id).then(function (picklists) {
                        var copyPicklist = angular.copy(picklists.data.items);
                        $scope.picklist = $filter('filter')(copyPicklist, { inactive: '!true' });
                    });
                }
            };
        }
    ])
;
