'use strict';

angular.module('primeapps')

    .controller('RelationsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'RelationsService', 'LayoutService', '$http', 'config', 'ModuleService',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService, RelationsService, LayoutService, $http, config, ModuleService) {

            $scope.$parent.menuTopTitle = "Models";
            $scope.$parent.activeMenu = 'model';
            $scope.$parent.activeMenuItem = 'relations';
            $rootScope.modules = [];

            $scope.loading = true;
            $scope.requestModel = {
                limit: 10,
                offset: 1
            };

            RelationsService.count().then(function (response) {
                $scope.pageTotal = response.data;
            });
            RelationsService.find($scope.requestModel).then(function (response) {
                $scope.relations = response.data;
                $scope.loading = false;
            });

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;


                RelationsService.find(requestModel).then(function (response) {
                    $scope.relations = response.data;
                    $scope.loading = false;
                });

            };

            $scope.changeOffset = function () {
                $scope.changePage(1)
            };

            // var promiseResult = ModuleService.getModules().then(function (response) {
            //     //.find($scope.requestModel).then(function (response) {
            //     $rootScope.modules = response.data;
            // });
            //
            // $scope.setRelations = function () {
            //     $scope.relations = [];
            //     for (var i = 0; i < $rootScope.modules.length; i++) {
            //         var module = angular.copy($rootScope.modules[i]);
            //         if (module.relations) {
            //             for (var j = 0; j < module.relations.length; j++) { //
            //                 module.relations[j].parent_module = module;
            //             }
            //             $scope.relations = $scope.relations.concat(module.relations);
            //         }
            //     }
            //     $scope.relations = RelationsService.processRelations($scope.relations);
            //     $scope.relationsState = angular.copy($scope.relations);
            //     $rootScope.loading = false;
            // };
            //
            // promiseResult.then(function onSuccess() {
            //     $scope.setRelations();
            // });

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
                $scope.fields = RelationsService.getFields($scope.currentRelation);

                //Module relations list remove itself
                var filter = {};
                if (relation.parent_module) {
                    filter['label_' + $rootScope.language + '_plural'] = '!' + relation.parent_module['label_' + $rootScope.language + '_plural'];
                }
                //Module relations list remove itself
                $scope.parentModuleLists = $rootScope.modules;
                $scope.moduleLists = $filter('filter')($rootScope.modules, filter, true);

                $scope.addNewRelationsFormModal = $scope.addNewRelationsFormModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/setup/modules/relationForm.html',
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
                }, 100);
            };

            $scope.moduleChanged = function () {
                if ($scope.currentRelation.module) {
                    $scope.module = $scope.currentRelation.module;
                    var filter = {};
                    filter['label_' + $rootScope.language + '_plural'] = '!' + $scope.module['label_' + $rootScope.language + '_plural'];
                    //Module relations list remove itself
                    $scope.moduleLists = $filter('filter')($rootScope.modules, filter, true);
                }
            };

            $scope.relatedModuleChanged = function () {

                if ($scope.currentRelation.module) {

                    $scope.currentRelation.relationField = null;

                    if ($scope.currentRelation.relatedModule)
                        $scope.currentRelation.hasRelationField = $filter('filter')($scope.currentRelation.relatedModule.fields, { data_type: 'lookup', lookup_type: $scope.currentRelation.module.name, deleted: false }, true).length > 0;

                    $scope.currentRelation.display_fields = null;
                    $scope.fields = RelationsService.getFields($scope.currentRelation);

                    if ($scope.currentRelation.relation_type === 'many_to_many')
                        $scope.bindDragDrop();

                    $scope.relationTypeChanged = function () {
                        if ($scope.currentRelation.relation_type === 'many_to_many')
                            $scope.bindDragDrop();
                    };
                }
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
                            var lookupModule = $filter('filter')($rootScope.modules, { name: selectedField.lookup_type }, true)[0];
                            var primaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];

                            relation.display_fields.push(selectedField.name + '.' + lookupModule.name + '.' + primaryField.name + '.primary');
                        }
                    });
                }
                else {
                    var primaryField = $filter('filter')(relation.relatedModule.fields, { primary: true })[0];
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
                        relatedModule: $scope.module,
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
                        $rootScope.loading = true;
                        ModuleService.getModules().then(function (response) {
                            //.find($scope.requestModel).then(function (response) {
                            $rootScope.modules = response.data;
                            $scope.setRelations();
                            $rootScope.loading = false;
                        });
                        ngToast.create({
                            content: $filter('translate')('Setup.Modules.RelationSaveSuccess'),
                            className: 'success'
                        });
                        $scope.saving = false;
                        $scope.addNewRelationsFormModal.hide();
                    }
                    //     LayoutService.getMyAccount(true)
                    // .then(function () {
                    //     $scope.setRelations();
                    //     ngToast.create({
                    //         content: $filter('translate')('Setup.Modules.RelationSaveSuccess'),
                    //         className: 'success'
                    //     });
                    //     $scope.saving = false;
                    //     $scope.addNewRelationsFormModal.hide();
                    // });
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
                        var mainModuleId = ($filter('filter')($rootScope.modules, { name: relation.mainModule }, true)[0]).id;
                    ModuleService.createModuleRelation(relation, (relation.two_way) ? mainModuleId : $scope.module.id)
                        .then(function () {
                            success();
                        })
                        .catch(function () {
                            error();
                        });
                }
                else {
                    ModuleService.updateModuleRelation(relation, $scope.currentRelation.module.id)
                        .then(function () {
                            success();
                        })
                        .catch(function () {
                            error();
                        });
                }
            };

            $scope.delete = function (relation) {
                ModuleService.deleteModuleRelation(relation.id)
                    .then(function () {
                        var relationToDeleteIndex = helper.arrayObjectIndexOf($scope.relations, relation);
                        $scope.relations.splice(relationToDeleteIndex, 1);
                        ngToast.create({ content: $filter('translate')('Setup.Modules.RelationDeleteSuccess'), className: 'success' });
                        // LayoutService.getMyAccount(true)
                        //     .then(function () {
                        //         var relationToDeleteIndex = helper.arrayObjectIndexOf($scope.relations, relation);
                        //         $scope.relations.splice(relationToDeleteIndex, 1);
                        //         ngToast.create({ content: $filter('translate')('Setup.Modules.RelationDeleteSuccess'), className: 'success' });
                        //     });
                    })
                    .catch(function () {
                        $scope.relations = $scope.relationsState;

                        if ($scope.addNewRelationsFormModal) {
                            $scope.addNewRelationsFormModal.hide();
                            $scope.saving = false;
                        }
                    });
            };
        }
    ]);