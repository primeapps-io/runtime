'use strict';

angular.module('primeapps')

    .controller('RelationsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'RelationsService', 'LayoutService', '$http', 'config', 'ModuleService','$location',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, RelationsService, LayoutService, $http, config, ModuleService,$location) {

            $scope.$parent.menuTopTitle = "Models";
            $scope.$parent.activeMenu = "model";
            $scope.$parent.activeMenuItem = "relations";
            
            $rootScope.breadcrumblist[2].title = 'Relations';

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }

            };

            $scope.generator(10);

            $scope.id = $location.search().id ? $location.search().id : 0;

            $scope.loading = true;
            $scope.requestModel = {
                limit: '10',
                offset: 0
            };

            RelationsService.count($scope.id).then(function (response) {
                $scope.pageTotal = response.data;
            });
            RelationsService.find($scope.id, $scope.requestModel).then(function (response) {
                $scope.relations = response.data;
                angular.forEach($scope.relations, function (relation) {
                    var relationField = $filter('filter')(relation.relation_module.fields, { name: relation.relation_field, deleted: false }, true)[0];
                    if (!relationField && relation.relation_type === 'one_to_many') {
                        relation.deleted = true;
                        return;
                    }
                    if (relation.relation_type === 'one_to_many') {
                        relation.relationField = relationField;
                    }
                });
                $scope.relationsState = angular.copy($scope.relations);
                $scope.loading = false;
            });

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;


                RelationsService.find($scope.id, requestModel).then(function (response) {
                    $scope.relations = response.data;
                    angular.forEach($scope.relations, function (relation) {
                        var relationField = $filter('filter')(relation.relation_module.fields, { name: relation.relation_field, deleted: false }, true)[0];
                        if (!relationField && relation.relation_type === 'one_to_many') {
                            relation.deleted = true;
                            return;
                        }
                        if (relation.relation_type === 'one_to_many') {
                            relation.relationField = relationField;
                        }
                    });
                    $scope.relationsState = angular.copy($scope.relations);
                    $scope.loading = false;
                });

            };

            $scope.changeOffset = function () {
                $scope.changePage(1)
            };

            $scope.showFormModal = function (relation) {
                if (!relation) {
                    relation = {};
                    var sortOrders = [];

                    angular.forEach($scope.relations, function (relation) {
                        sortOrders.push(relation.order);
                    });

                    var maxOrder = Math.max.apply(null, sortOrders);
                    maxOrder = maxOrder < 0 ? 0 : maxOrder;
                    relation.order = maxOrder + 1;
                    relation.relation_type = 'one_to_many';
                    relation.isNew = true;
                }

                $scope.currentRelation = relation;
                $scope.currentRelation.hasRelationField = true;
                $scope.currentRelation.module = relation.parent_module;
                $scope.currentRelationState = angular.copy($scope.currentRelation);
                $scope.fields = RelationsService.getFields($scope.currentRelation, $scope.$parent.modules);

                //Module relations list remove itself
                var filter = {};
                if (relation.parent_module) {
                    filter['label_' + $scope.language + '_plural'] = '!' + relation.parent_module['label_' + $scope.language + '_plural'];
                }
                //Module relations list remove itself
                $scope.moduleLists = $filter('filter')($scope.$parent.modules, filter, true);

                $scope.addNewRelationsFormModal = $scope.addNewRelationsFormModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/model/relations/relationForm.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.addNewRelationsFormModal.$promise.then(function () {
                    if (!relation.isNew)
                        $scope.bindDragDrop();

                    $scope.addNewRelationsFormModal.show();
                });
            };

            var drakeAvailableFields;
            var drakeSelectedFields;

            $scope.bindDragDrop = function () {
                $timeout(function () {
                    if (drakeAvailableFields)
                        drakeAvailableFields.destroy();

                    if (drakeSelectedFields)
                        drakeSelectedFields.destroy();

                    var containerLeft = document.querySelector('#availableFields');
                    var containerRight = document.querySelector('#selectedFields');

                    drakeAvailableFields = dragularService([containerLeft], {
                        scope: $scope,
                        containersModel: [$scope.fields.availableFields],
                        classes: {
                            mirror: 'gu-mirror-option',
                            transit: 'gu-transit-option'
                        },
                        accepts: accepts,
                        moves: function (el, container, handle) {
                            return handle.classList.contains('dragable');
                        }
                    });

                    drakeSelectedFields = dragularService([containerRight], {
                        scope: $scope,
                        classes: {
                            mirror: 'gu-mirror-option',
                            transit: 'gu-transit-option'
                        },
                        containersModel: [$scope.fields.selectedFields]
                    });

                    function accepts(el, target, source) {
                        if (source != target) {
                            return true;
                        }
                    }
                }, 1000);
            };

            $scope.moduleChanged = function () {
                if ($scope.currentRelation.module) {
                    $scope.module = $scope.currentRelation.module;
                    var filter = {};
                    filter['label_' + $scope.language + '_plural'] = '!' + $scope.module['label_' + $scope.language + '_plural'];
                    //Module relations list remove itself
                    $scope.moduleLists = $filter('filter')($scope.$parent.modules, filter, true);
                }
            };

            $scope.relatedModuleChanged = function () {

                var relatedModuleName = $scope.currentRelation.relation_module.name;
                var filter = {};
                filter['label_' + $scope.language + '_plural'] = filter['label_' + $scope.language + '_plural'] = '!' + relatedModuleName['label_' + $scope.language + '_plural'];
                // $scope.parentModuleLists = filter['label_' + $rootScope.language + '_plural'] = '!' + relatedModuleName['label_' + $rootScope.language + '_plural'];
                $scope.moduleLists = $filter('filter')($scope.$parent.modules, filter, true);
                ModuleService.getModuleFields(relatedModuleName).then(function (response) {
                    $scope.currentRelation.relation_module.fields = response.data;
                    if ($scope.currentRelation.module) {
                        $scope.currentRelation.relationField = null;

                        if ($scope.currentRelation.relation_module)
                            $scope.currentRelation.hasRelationField = $filter('filter')($scope.currentRelation.relation_module.fields, { data_type: 'lookup', lookup_type: $scope.currentRelation.module.name, deleted: false }, true).length > 0;

                        $scope.currentRelation.display_fields = null;
                        $scope.fields = RelationsService.getFields($scope.currentRelation, $scope.$parent.modules);

                        if ($scope.currentRelation.relation_type === 'many_to_many')
                            $scope.bindDragDrop();

                        $scope.relationTypeChanged = function () {
                            if ($scope.currentRelation.relation_type === 'many_to_many')
                                $scope.bindDragDrop();
                        };
                    }
                });
            };

            $scope.save = function (relationForm) {
                if (!relationForm.$valid)
                    return;

                $scope.saving = true;
                if (relationForm.two_way)
                    var relation = relationForm;
                else
                    var relation = angular.copy($scope.currentRelation);
                relation.display_fields = [];

                if ($scope.fields.selectedFields && $scope.fields.selectedFields.length > 0) {
                    angular.forEach($scope.fields.selectedFields, function (selectedField) {
                        relation.display_fields.push(selectedField.name);

                        if (selectedField.lookup_type) {
                            var lookupModule = $filter('filter')($scope.$parent.modules, { name: selectedField.lookup_type }, true)[0];
                            var primaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];

                            relation.display_fields.push(selectedField.name + '.' + lookupModule.name + '.' + primaryField.name + '.primary');
                        }
                    });
                }
                else {
                    var primaryField = $filter('filter')(relation.relation_module.fields, { primary: true })[0];
                    relation.display_fields.push(primaryField.name);
                }

                if (relation.isNew) {
                    delete relation.isNew;

                    if (!$scope.relations)
                        $scope.relations = [];

                    if (relation.relation_type === 'many_to_many') {
                        relation.relationField = {};
                        relation.relationField.name = $scope.currentRelation.module.name;
                    }
                }

                RelationsService.prepareRelation(relation);

                //Create dynamic relation for related module in manytomany relation.
                var createRelationManyToManyModule = function () {
                    var relatedModul = {
                        display_fields: null,
                        hasRelationField: false,
                        isNew: true,
                        order: $scope.currentRelation.order,
                        relation_module: $scope.currentRelation.relation_module, //$scope.module,
                        relationField: null,
                        relation_type: "many_to_many",
                        $valid: true,
                        two_way: true,
                        mainModule: relation.related_module
                    };

                    if ($scope.currentRelation.hasOwnProperty("label_en_singular")) {
                        relatedModul["label_en_singular"] = $scope.module.label_en_singular;
                        relatedModul["label_en_plural"] = $scope.module.label_en_plural;
                    }
                    else {
                        relatedModul["label_tr_singular"] = $scope.module.label_tr_singular;
                        relatedModul["label_tr_plural"] = $scope.module.label_tr_plural;
                    }
                    $scope.fields.selectedFields = [];
                    $scope.save(relatedModul);
                };

                var success = function () {
                    if (!relation.two_way && relation.relation_type === 'many_to_many')
                        createRelationManyToManyModule();
                    else {
                        $scope.loading = true;
                        swal($filter('translate')('Setup.Modules.RelationSaveSuccess'), "", "success");
                        $scope.saving = false;
                        $scope.addNewRelationsFormModal.hide();
                        $scope.changePage(1);
                    }
                };

                var error = function () {
                    $scope.relations = $scope.relationsState;

                    if ($scope.addNewRelationsFormModal) {
                        $scope.addNewRelationsFormModal.hide();
                        $scope.saving = false;
                    }
                };

                if (!relation.id) {
                    if (relation.mainModule)
                        var mainModuleId = ($filter('filter')($scope.$parent.modules, { name: relation.mainModule }, true)[0]).id;
                    RelationsService.createModuleRelation(relation, (relation.two_way) ? mainModuleId : $scope.module.id)
                        .then(function () {
                            success();
                        })
                        .catch(function () {
                            error();
                        });
                }
                else {
                    RelationsService.updateModuleRelation(relation, $scope.currentRelation.module.id)
                        .then(function () {
                            success();
                        })
                        .catch(function () {
                            error();
                        });
                }
            };

            $scope.delete = function (relation) {
                const willDelete =
                    swal({
                        title: "Are you sure?",
                        text: "Are you sure that you want to delete this relation ?",
                        icon: "warning",
                        buttons: ['Cancel', 'Okey'],
                        dangerMode: true
                    }).then(function (value) {
                        if (value) {
                            RelationsService.deleteModuleRelation(relation.id)
                                .then(function () {
                                    //var relationToDeleteIndex = helper.arrayObjectIndexOf($scope.relations, relation);
                                    // $scope.relations.splice(relationToDeleteIndex, 1);
                                    swal($filter('translate')('Setup.Modules.RelationDeleteSuccess'), "", "success");
                                    //$scope.addNewRelationsFormModal.hide();
                                    $scope.changePage(1);
                                })
                                .catch(function () {
                                    $scope.relations = $scope.relationsState;

                                    if ($scope.addNewRelationsFormModal) {
                                        $scope.addNewRelationsFormModal.hide();
                                        $scope.saving = false;
                                    }
                                });
                        }
                    });
            };
        }
    ]);