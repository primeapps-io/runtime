'use strict';

angular.module('ofisim')

    .controller('ModuleAddModalController', ['$rootScope', '$scope', 'ngToast', '$filter', 'helper', '$location', '$state', '$stateParams', '$q', '$window', '$localStorage', '$cache', 'operations', '$modal', 'ModuleService',
        function ($rootScope, $scope, ngToast, $filter, helper, $location, $state, $stateParams, $q, $window, $localStorage, $cache, operations, $modal, ModuleService) {
            $scope.loadingModal = true;
            $scope.operations = operations;
            $scope.hasPermission = helper.hasPermission;
            $scope.selectedRelatedModule = angular.copy($scope.$parent.$parent.selectedRelatedModule);
            $scope.relatedModule = {};
            $scope.relatedModule.related_module = $scope.selectedRelatedModule.related_module;
            $scope.relatedModule.relation_field = $scope.selectedRelatedModule.relation_field;
            $scope.relatedModule.display_fields = $scope.selectedRelatedModule.display_fields;
            $scope.relatedModule.readonly = true;
            $scope.relationKey = $scope.selectedRelatedModule.relation_field + '_' + $scope.selectedRelatedModule.related_module;

            $scope.addRecords = function () {
                $scope.selectedRows = $scope['selectedRows' + $scope.relatedModule.related_module];

                if (!$scope.selectedRows.length) {
                    ngToast.create({ content: $filter('translate')('Module.NoRecordSelected'), className: 'warning' });
                    return;
                }

                var currentTotalCount = $scope.$parent.$parent['currentTotalCount' + $scope.relatedModule.related_module];

                if ($scope.selectedRows.length + currentTotalCount > 200) {
                    ngToast.create({ content: $filter('translate')('Module.MaxRecordSelected', { count: ($scope.selectedRows.length + currentTotalCount) - 200 }), className: 'warning' });
                    return;
                }

                $scope.recordsAdding = true;
                var relations = [];

                angular.forEach($scope.selectedRows, function (selectedRow) {
                    var relation = {};

                    if ($scope.$parent.$parent.type != $scope.selectedRelatedModule.related_module) {
                        relation[$scope.$parent.$parent.type + '_id'] = $scope.$parent.$parent.id;
                        relation[$scope.selectedRelatedModule.related_module + '_id'] = selectedRow.id;
                    }
                    else {
                        relation[$scope.$parent.$parent.type + '1_id'] = $scope.$parent.$parent.id;
                        relation[$scope.selectedRelatedModule.related_module + '2_id'] = selectedRow.id;
                    }

                    relations.push(relation);
                });

                ModuleService.addRelations($scope.$parent.$parent.type, $scope.selectedRelatedModule.related_module, relations)
                    .then(function (count) {
                        $scope.recordsAdding = false;
                        $scope.$parent.$hide();
                        var dt = new Date();
                        $scope.$parent.$parent.refreshSubModules[$scope.$parent.$parent.type] = {};
                        $scope.$parent.$parent.refreshSubModules[$scope.$parent.$parent.type][$scope.selectedRelatedModule.related_module] = dt.getTime();

                        //Update record number in tab.
                        var totalCount = ($filter('filter')($scope.$parent.$parent.module.relations, {
                            related_module: $scope.relatedModule.related_module,
                            deleted: false
                        }, true)[0]);
                        if (totalCount.total === undefined || totalCount.total === null)
                            totalCount.total = 0;
                        totalCount.total += count.data;

                        $cache.remove($scope.$$childHead.cacheKey);
                    });
            };

            $scope.openCreateModal = function (module) {
                $scope.currentLookupField = { lookup_type: module };

                $scope.formModal = $scope.formModal || $modal({
                        scope: $scope,
                        templateUrl: 'web/views/app/module/moduleFormModal.html',
                        animation: '',
                        backdrop: 'static',
                        show: false,
                        tag: 'createModal'
                    });

                $scope.formModal.$promise.then($scope.formModal.show);
            };

            $scope.refresh = function () {
                var dt = new Date();
                $scope.refreshSubModules[$scope.relationKey] = dt.getTime();
            };

            $scope.formModalSuccess = function () {
                $scope.refresh();
            };
        }
    ]);