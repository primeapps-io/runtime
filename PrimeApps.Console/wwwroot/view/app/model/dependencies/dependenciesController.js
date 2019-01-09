'use strict';

angular.module('primeapps')

    .controller('DependenciesController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', 'helper', '$cache', 'systemRequiredFields', 'systemReadonlyFields', 'DependenciesService', 'LayoutService', 'ModuleService',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, helper, $cache, systemRequiredFields, systemReadonlyFields, DependenciesService, LayoutService, ModuleService) {

            $scope.$parent.menuTopTitle = "Models";
            $scope.$parent.activeMenu = 'model';
            $scope.$parent.activeMenuItem = 'dependencies';
            $scope.loading = true;
            $scope.picklistsModule = {};
            // $scope.moduleLists = $scope.$parent.modules;//$filter('filter')(, { display: true }, true);
            $scope.requestModel = {
                limit: 10,
                offset: 1
            };

            DependenciesService.count().then(function (response) {
                $scope.pageTotal = response.data;
            });
            DependenciesService.find($scope.requestModel).then(function (response) {
                var dependencies = response.data;
                angular.forEach(dependencies, function (dependency) {
                    dependency.parentField = $filter('filter')(dependency.module.fields, { name: dependency.parent_field, deleted: '!true' })[0];
                    dependency.childField = $filter('filter')(dependency.module.fields, { name: dependency.child_field, deleted: '!true' })[0];
                    dependency.parent_module = dependency.module;
                    dependency.dependencyType = dependency.dependency_type === 'display' ? 'display' : 'value';
                    dependency.type = dependency.dependency_type;
                    dependency.sectionField = $filter('filter')(dependency.module.sections, {
                        name: dependency.child_section,
                        deleted: '!true'
                    })[0];
                });
                $scope.dependencies = dependencies;
                $scope.loading = false;
            });

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;

                DependenciesService.find($scope.requestModel).then(function (response) {
                    var dependencies = response.data;
                    angular.forEach(dependencies, function (dependency) {
                        dependency.parentField = $filter('filter')(dependency.module.fields, { name: dependency.parent_field, deleted: '!true' })[0];
                        dependency.childField = $filter('filter')(dependency.module.fields, { name: dependency.child_field, deleted: '!true' })[0];
                        dependency.parent_module = dependency.module;
                        dependency.dependencyType = dependency.dependency_type === 'display' ? 'display' : 'value';
                        dependency.type = dependency.dependency_type;
                        dependency.sectionField = $filter('filter')(dependency.module.sections, {
                            name: dependency.child_section,
                            deleted: '!true'
                        })[0];
                    });
                    $scope.dependencies = dependencies;
                    $scope.loading = false;
                });

            };

            $scope.picklistsModule = {};

            $scope.moduleChanged = function () {

                $scope.parentDisplayFields = [];
                $scope.parentValueFields = [];
                $scope.childValueListFields = [];
                $scope.childValueTextFields = [];
                $scope.childDisplayFields = [];
                $scope.picklistFields = [];

                $scope.module = $scope.currentDependency.module;
                $scope.sections = $scope.currentDependency.module.sections;
                ModuleService.getModuleFields($scope.module.name).then(function (response) {
                    $scope.module.fields = response.data;
                    getFields();
                    getDependencyTypes();
                    getValueChangeTypes();
                });
            };

            //$scope.module = angular.copy(module);
            //$scope.dependencies = $filter('filter')(ModuleSetupService.processDependencies($scope.module), { deleted: false }, true);
            //$scope.dependenciesState = angular.copy($scope.dependencies);
            //$scope.sections = $scope.module.sections;
            $scope.affectedAreaType = "field";


            $scope.getPicklist = function () {
                var parentField = $filter('filter')($scope.module.fields, { name: $scope.currentDependency.parent_field }, true)[0];
                var picklist = $filter('filter')($scope.picklistsModule[parentField.picklist_id], { inactive: '!true' });

                return picklist;
            };

            var getFields = function () {
                /*$scope.parentDisplayFields = [];
                $scope.parentValueFields = [];
                $scope.childValueListFields = [];
                $scope.childValueTextFields = [];
                $scope.childDisplayFields = [];
                $scope.picklistFields = [];*/

                angular.forEach($scope.module.fields, function (field) {
                    if (isSystemField(field))
                        return;

                    var existDisplayDependency = $filter('filter')($scope.dependencies, {
                        childField: { name: field.name },
                        dependencyType: 'display'
                    }, true)[0];
                    var existValueDependency = $filter('filter')($scope.dependencies, {
                        childField: { name: field.name },
                        dependencyType: 'value'
                    }, true)[0];

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

                function isSystemField(field) {
                    if (systemRequiredFields.all.indexOf(field.name) > -1 || (systemRequiredFields[$scope.module.name] && systemRequiredFields[$scope.module.name].indexOf(field.name) > -1))
                        return true;

                    return false;
                }
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
            /*getFields();
            getDependencyTypes();
            getValueChangeTypes();
            */
            $scope.dependencyTypeChanged = function () {
                if ($scope.currentDependency.dependencyType === 'value') {
                    $scope.currentDependency.type = 'list_text';
                }

                $scope.currentDependency.parent_field = null;
                $scope.currentDependency.child_field = null;
                // $scope.currentDependency.child_section = null;
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
                        break;
                    case 'value':
                        if ($scope.currentDependency.type === 'list_value')
                            return $scope.picklistFields;
                        else
                            return $scope.parentValueFields;
                        break;
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
                        break;
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
                        break;
                }
            };

            $scope.getMappingOptions = function () {
                var parentField = $filter('filter')($scope.module.fields, { name: $scope.currentDependency.parent_field }, true)[0];
                var childField = $filter('filter')($scope.module.fields, { name: $scope.currentDependency.child_field }, true)[0];
                var childSection = $filter('filter')($scope.module.sections, { name: $scope.currentDependency.child_section }, true)[0];
                var parentPicklist = $filter('filter')($scope.picklistsModule[parentField.picklist_id], { inactive: '!true' });
                var childPicklist = $filter('filter')($scope.picklistsModule[childField.picklist_id], { inactive: '!true' });

                angular.forEach(parentPicklist, function (picklistItem) {
                    picklistItem.childPicklist = childPicklist;
                });

                return parentPicklist;
            };

            var setCurrentDependency = function (dependency) {
                $scope.currentDependency = dependency;
                $scope.currentDependency.hasRelationField = true;
                $scope.currentDependency.module = dependency.module;
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

                    $scope.module = angular.copy($filter('filter')($rootScope.modules, { name: $stateParams.module }, true)[0]);
                    // $scope.dependencies = $filter('filter')(DependenciesService.processDependencies($scope.module), { deleted: false }, true);
                    // ModuleService.getModules().then(function (response) {
                    //
                    //     $rootScope.modules = response.data;
                    //     $scope.moduleLists = $filter('filter')($rootScope.modules, { display: true }, true);
                    //     //$scope.dependencies = $filter('filter')(DependenciesService.processDependencies($scope.module), { deleted: false }, true);
                    //     $scope.setDependencies();
                    //     angular.forEach($scope.dependencies, function (dependency) {
                    //         if (dependency.type && (dependency.type === 'list_field' || dependency.type === 'list_value'))
                    //             $cache.remove('picklist_' + dependency.childField.picklist_id);
                    //     });
                    //
                    //     ngToast.create({
                    //         content: $filter('translate')('Setup.Modules.DependencySaveSuccess'),
                    //         className: 'success'
                    //     });
                    //     $scope.saving = false;
                    //     $scope.addNewDependencyModal.hide();
                    // });
                    // AppService.getMyAccount(true)
                    //     .then(function () {
                    //         /*$scope.module = angular.copy($filter('filter')($rootScope.modules, { name: $stateParams.module }, true)[0]);
                    //         $scope.dependencies = $filter('filter')(ModuleSetupService.processDependencies($scope.module), { deleted: false }, true);
                    //         angular.forEach($scope.dependencies, function (dependency) {
                    //             if (dependency.type && (dependency.type === 'list_field' || dependency.type === 'list_value'))
                    //                 $cache.remove('picklist_' + dependency.childField.picklist_id);
                    //         });*/
                    //         $scope.setDependencies();
                    //
                    //         ngToast.create({ content: $filter('translate')('Setup.Modules.DependencySaveSuccess'), className: 'success' });
                    //         $scope.saving = false;
                    //         $scope.addNewDependencyModal.hide();
                    //     });
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
                /*delete dependency.$$hashKey;
                var deleteModel = angular.copy($scope.dependencies);
                var dependencyIndex = helper.arrayObjectIndexOf(deleteModel, dependency);
                deleteModel.splice(dependencyIndex, 1);*/

                DependenciesService.deleteModuleDependency(dependency.id)
                    .then(function () {
                        var dependencyIndex = helper.arrayObjectIndexOf($scope.dependencies, dependency);
                        $scope.dependencies.splice(dependencyIndex, 1);

                        if (dependency.type && (dependency.type === 'list_field' || dependency.type === 'list_value'))
                            $cache.remove('picklist_' + dependency.childField.picklist_id);

                        ngToast.create({
                            content: $filter('translate')('Setup.Modules.DependencyDeleteSuccess'),
                            className: 'success'
                        });
                        // AppService.getMyAccount(true)
                        //     .then(function () {
                        //         var dependencyIndex = helper.arrayObjectIndexOf($scope.dependencies, dependency);
                        //         $scope.dependencies.splice(dependencyIndex, 1);
                        //
                        //         /*if (dependency.type && (dependency.type === 'list_field' || dependency.type === 'list_value'))
                        //             $cache.remove('picklist_' + dependency.childField.picklist_id);*/
                        //
                        //         ngToast.create({ content: $filter('translate')('Setup.Modules.DependencyDeleteSuccess'), className: 'success' });
                        //     });
                    })
                    .catch(function () {
                        $scope.dependencies = $scope.dependenciesState;

                        if ($scope.addNewDependencyModal) {
                            $scope.addNewDependencyModal.hide();
                            $scope.saving = false;
                        }
                    });

                $scope.parentValueChanged = function () {
                    $scope.currentDependency.child_field = null;
                    $scope.currentDependency.child_section = null;
                    $scope.currentDependency.value_maps = {};
                    $scope.currentDependency.values = [];
                    $scope.currentDependency.field_map = {};
                };
            };

        }
    ]);
