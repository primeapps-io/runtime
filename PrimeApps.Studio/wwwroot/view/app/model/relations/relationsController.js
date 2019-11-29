'use strict';

angular.module('primeapps')

    .controller('RelationsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'RelationsService', 'LayoutService', '$http', 'config', 'ModuleService', '$location', '$localStorage',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, RelationsService, LayoutService, $http, config, ModuleService, $location, $localStorage) {

            $scope.$parent.activeMenuItem = "relations";
            $rootScope.breadcrumblist[2].title = 'Relations';
            $scope.id = $location.search().id ? $location.search().id : 0;
             
            $scope.showFormModal = function (relation) {
                $scope.moduleLists = [];
                $scope.background_color = "background-color: #fbfbfb";
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
                } else {
                    relation.isNew = false;
                    var filter = {};
                    filter['label_' + $scope.language + '_plural'] = '!' + relation.parent_module['label_' + $scope.language + '_plural'];
                    $scope.moduleLists = $filter('filter')($rootScope.appModules, filter, true);
                }

                $scope.currentRelation = angular.copy(relation);
                $scope.currentRelation.hasRelationField = true;
                $scope.currentRelation.module = angular.copy(relation.parent_module);
                $scope.currentRelationState = angular.copy($scope.currentRelation);

                if (!$scope.currentRelation.detail_view_type)
                    $scope.currentRelation.detail_view_type = 'tab';

                if ($scope.currentRelation.related_module) {

                    $scope.fields = {};
                    $scope.module = $scope.currentRelation.module;
                    $scope.relatedModuleChanged();
                } else
                    $scope.fields = RelationsService.getFields($scope.currentRelation, $rootScope.appModules);

                //Module relations list remove itself
                filter = {};
                if (relation.parent_module) {
                    filter['label_' + $scope.language + '_plural'] = '!' + relation.parent_module['label_' + $scope.language + '_plural'];
                }
                //Module relations list remove itself


                $scope.addNewRelationsFormModal = $scope.addNewRelationsFormModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/model/relations/relationForm.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.addNewRelationsFormModal.$promise.then(function () {
                    $scope.addNewRelationsFormModal.show();
                });

                if ($scope.id) {
                    var module = $filter('filter')($rootScope.appModules, { id: parseInt($scope.id) }, true)[0];
                    $scope.currentRelation.module = module;
                    $scope.moduleChanged();
                }
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

                    if (containerLeft) {
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
                    }

                    if (containerRight) {
                        drakeSelectedFields = dragularService([containerRight], {
                            scope: $scope,
                            classes: {
                                mirror: 'gu-mirror-option',
                                transit: 'gu-transit-option'
                            },
                            containersModel: [$scope.fields.selectedFields]
                        });
                    }


                    function accepts(el, target, source) {
                        if (source !== target) {
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
                    $scope.moduleLists = $filter('filter')($rootScope.appModules, filter, true);
                }
            };

            $scope.relatedModuleChanged = function () {
                $scope.fields = {};
                $scope.currentRelation.relationField = null;
                if ($scope.currentRelation.related_module) {
                    /////var relatedModuleName = $scope.currentRelation.related_module.name;
                    var relatedModuleName = $scope.currentRelation.related_module;
                    $scope.modalLoading = true;
                    ModuleService.getModuleFields(relatedModuleName).then(function (response) {
                        $scope.currentRelation.related_module = $filter('filter')($rootScope.appModules, { name: relatedModuleName }, true)[0];
                        $scope.currentRelation.related_module.fields = response.data;

                        if ($scope.currentRelation.related_module)
                            $scope.currentRelation.hasRelationField = $filter('filter')($scope.currentRelation.related_module.fields, {
                                data_type: 'lookup',
                                lookup_type: $scope.currentRelation.module.name,
                                deleted: false
                            }, true).length > 0;

                        if ($scope.currentRelation.related_module && ($scope.currentRelation.related_module.name === 'activities' || $scope.currentRelation.related_module.name === 'mails') && ($scope.module.name !== 'activities' || $scope.module.name !== 'mails') && $scope.currentRelation.relation_type === 'one_to_many')
                            $scope.currentRelation.relationField = $filter('filter')($scope.currentRelation.related_module.fields, { name: 'related_to' }, true)[0];

                        else if ($scope.currentRelation.hasRelationField && !$scope.currentRelation.isNew)
                            $scope.currentRelation.relationField = $filter('filter')($scope.currentRelation.related_module.fields, {
                                name: $scope.currentRelation.relation_field,
                                deleted: false
                            }, true)[0];

                        $scope.currentRelation.display_fields = null;
                        RelationsService.getFields($scope.currentRelation, $rootScope.appModules).then(function (fields) {

                            $scope.fields = fields;

                            if ($scope.currentRelation.relationField) {
                                $scope.bindDragDrop();
                            }

                            if ($scope.currentRelation.relation_type === 'many_to_many')
                                $scope.bindDragDrop();

                            $scope.modalLoading = false;
                        });
                    });
                }
            };

            $scope.save = function (relationForm) {
                if (!relationForm.$valid || $scope.fields.selectedFields.length < 1) {

                    if (relationForm.$error.required)
                        toastr.error($filter('translate')('Setup.Modules.RequiredError'));

                    if ($scope.currentRelation.related_module && $scope.fields.selectedFields && $scope.fields.selectedFields.length < 1)
                        toastr.error($filter('translate')('View.FieldError'));

                    return;
                }

                $scope.saving = true;

                if (relationForm.two_way)
                    var relation = relationForm;
                else
                    relation = angular.copy($scope.currentRelation);

                relation.display_fields = [];

                if ($scope.fields.selectedFields && $scope.fields.selectedFields.length > 0) {
                    angular.forEach($scope.fields.selectedFields, function (selectedField) {
                        relation.display_fields.push(selectedField.name);

                        if (selectedField.lookup_type) {
                            var lookupModule = $filter('filter')($rootScope.appModules, { name: selectedField.lookup_type }, true)[0];
                            //TODO user gelirse patlıyor user module olarak eklendiğinde düzeltilecek
                            if (lookupModule)
                                ModuleService.getModuleFields(lookupModule.name).then(function (response) {
                                    lookupModule.fields = response.data;
                                    var primaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];
                                    relation.display_fields.push(selectedField.name + '.' + lookupModule.name + '.' + primaryField.name + '.primary');
                                });
                        }
                    });
                } else {
                    var primaryField = $filter('filter')(relation.related_module.fields, { primary: true })[0];
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
                    var relatedModule = {
                        display_fields: null,
                        hasRelationField: false,
                        isNew: true,
                        order: $scope.currentRelation.order,
                        related_module: $scope.currentRelation.related_module, //$scope.module,
                        relationField: null,
                        relation_type: "many_to_many",
                        $valid: true,
                        two_way: true,
                        mainModule: relation.related_module
                    };

                    if ($scope.currentRelation.hasOwnProperty("label_en_singular")) {
                        relatedModule["label_en_singular"] = $scope.module.label_en_singular;
                        relatedModule["label_en_plural"] = $scope.module.label_en_plural;
                    } else {
                        relatedModule["label_tr_singular"] = $scope.module.label_tr_singular;
                        relatedModule["label_tr_plural"] = $scope.module.label_tr_plural;
                    }
                    $scope.fields.selectedFields = [];
                    $scope.save(relatedModule);
                };

                var success = function () {
                    if (!relation.two_way && relation.relation_type === 'many_to_many' && relation.isNew)
                        createRelationManyToManyModule();
                    else {
                        $scope.loading = true;
                        toastr.success($filter('translate')('Setup.Modules.RelationSaveSuccess'));
                        $scope.addNewRelationsFormModal.hide();  
                        $scope.grid.dataSource.read();
                    }
                };

                var error = function () {
                    $scope.relations = $scope.relationsState;

                    if ($scope.addNewRelationsFormModal) {
                        $scope.addNewRelationsFormModal.hide();
                    }
                };

                if (!relation.id) {
                    if (relation.mainModule)
                        var mainModuleId = ($filter('filter')($rootScope.appModules, { name: relation.mainModule }, true)[0]).id;
                    RelationsService.createModuleRelation(relation, relation.two_way ? mainModuleId : $scope.module.id)
                        .then(function () {
                            success(); 
                        })
                        .catch(function () {
                            error();
                        }).finally(function () {
                            $scope.saving = false;
                        });
                } else {
                    RelationsService.updateModuleRelation(relation, $scope.currentRelation.module.id)
                        .then(function () {
                            success();
                        })
                        .catch(function () {
                            error();
                        }).finally(function () {
                            $scope.saving = false;
                        });
                }
            };

            $scope.delete = function (relation, event) {
                var willDelete =
                    swal({
                        title: "Are you sure?",
                        text: " ",
                        icon: "warning",
                        buttons: ['Cancel', 'Yes'],
                        dangerMode: true
                    }).then(function (value) {
                        if (value) {
                         

                            RelationsService.deleteModuleRelation(relation.id)
                                .then(function () { 
                                     toastr.success($filter('translate')('Setup.Modules.RelationDeleteSuccess')); 
                                    $scope.grid.dataSource.read();
                                })
                                .catch(function () {
                                    if ($scope.addNewRelationsFormModal) {
                                        $scope.addNewRelationsFormModal.hide();
                                        $scope.saving = false;
                                    }
                                });
                        }
                    });
            };

            $scope.relationTypeChanged = function () {

                if ($scope.currentRelation.relation_type === 'many_to_many' && $scope.currentRelation.related_module)
                    $scope.bindDragDrop();
            };

            $scope.relationFieldChange = function () {
                if ($scope.currentRelation.related_module.fields && $scope.currentRelation.relationField)
                    $scope.bindDragDrop();
            };

            $scope.$on('dragulardrop', function (e, el) {
                if ($scope.fields.selectedFields.length < 1) {
                    $scope.background_color = "background-color: #eed3d7";
                    toastr.error($filter('translate')('View.FieldError'));
                } else
                    $scope.background_color = "background-color: #fbfbfb";
            });

            //For Kendo UI
            $scope.goUrl = function (relation) {
                var selection = window.getSelection();
                if (selection.toString().length === 0) {
                    $scope.showFormModal(relation); //click event.
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
                            url: "/api/relation/find/" + $scope.id,
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
                                LabelEnPlural: { type: "string" },
                                ParentModule: { type: "string" },
                                RelatedModule: { type: "string" },
                                RelationType: { type: "enums" }
                            }
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
                    trTemp += '<td><span>' + e['label_' + $scope.language + '_plural'] + '</span></td>';
                    trTemp += '<td><span>' + e.parent_module['label_' + $scope.language + '_plural'] + '</span></td>';
                    trTemp += '<td class="text-capitalize"> <span>' + e.related_module + '</span></td > ';
                    trTemp += e.relation_type === "one_to_many" ? '<td ><span>' + $filter('translate')('Setup.Modules.OneToMany') + '</span></td>' : '<td><span>' + $filter('translate')('Setup.Modules.ManyToMany') + '</span></td>';
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
                field: 'LabelEnPlural',
                title: $filter('translate')('Setup.Modules.RelationName'),
            },
            {
                field: 'ParentModule.LabelEnPlural',
                title: $filter('translate')('Setup.Modules.Name'), 
            },
            {
                field: 'RelatedModule',
                title: $filter('translate')('Setup.Modules.RelatedModule'),
            },
            {
                field: 'RelationType',
                title: $filter('translate')('Setup.Modules.RelationType'),
                values: [
                    { text: 'One to many', value: 'OneToMany' },
                    { text: 'Many to many', value: 'ManyToMany' }]
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