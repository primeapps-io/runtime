'use strict';

angular.module('primeapps')

	.controller('WorkflowsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'WorkflowsService', 'LayoutService', '$http', 'config',
		function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService, WorkflowsService, LayoutService, $http, config) {

			//$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

			$scope.$parent.menuTopTitle = "Automation";
			$scope.$parent.activeMenu = 'automation';
			$scope.$parent.activeMenuItem = 'workflows';

			console.log("RelationsController");

			$scope.relationsState = angular.copy($scope.relations);


			$scope.showFormModal = function (relation) {
				if (!relation) {
					relation = {};
					var sortOrders = [];

					angular.forEach($scope.relations, function (item) {
						sortOrders.push(item.order);
					});

					var maxOrder = Math.max.apply(null, sortOrders);
					maxOrder = maxOrder < 0 ? 0 : maxOrder;
					relation.order = maxOrder + 1;
					relation.relation_type = 'one_to_many';
					relation.isNew = true;
				}

				$scope.currentRelation = relation;
				$scope.currentRelation.hasRelationField = true;
				$scope.currentRelationState = angular.copy($scope.currentRelation);
				//$scope.fields = ModuleSetupService.getFields($scope.currentRelation);
				if (!$scope.currentRelation.detail_view_type)
					$scope.currentRelation.detail_view_type = 'tab';
				//Module relations list remove itself
				var filter = {};
				//filter['label_' + $rootScope.language + '_plural'] = '!' + $scope.module['label_' + $rootScope.language + '_plural'];
				//$scope.moduleLists = $filter('filter')($rootScope.modules, filter, true);

				$scope.formModal = $scope.formModal || $modal({
					scope: $scope,
					templateUrl: 'view/setup/modules/relationForm.html',
					animation: '',
					backdrop: 'static',
					show: false
				});

				$scope.formModal.$promise.then(function () {
					if (!relation.isNew)
						$scope.bindDragDrop();

					$scope.formModal.show();
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

			$scope.relatedModuleChanged = function () {
				$scope.currentRelation.relationField = null;

				if ($scope.currentRelation.relatedModule)
					$scope.currentRelation.hasRelationField = $filter('filter')($scope.currentRelation.relatedModule.fields, { data_type: 'lookup', lookup_type: $stateParams.module, deleted: false }, true).length > 0;

				$scope.currentRelation.display_fields = null;
				$scope.fields = ModuleSetupService.getFields($scope.currentRelation);

				if ($scope.currentRelation.relation_type === 'many_to_many')
					$scope.bindDragDrop();
			};

			$scope.relationTypeChanged = function () {
				if ($scope.currentRelation.relation_type === 'many_to_many')
					$scope.bindDragDrop();
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
						relation.relationField.name = module.name;
					}
				}

				ModuleSetupService.prepareRelation(relation);

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
				}

				var success = function () {
					if (!relation.two_way && relation.relation_type === 'many_to_many' && $scope.currentRelation.isNew)
						createRelationManyToManyModule();
					else
						LayoutService.getMyAccount(true)
							.then(function () {
								$scope.module = angular.copy($filter('filter')($rootScope.modules, { name: $stateParams.module }, true)[0]);
								$scope.relations = ModuleSetupService.processRelations($scope.module.relations);
								ngToast.create({
									content: $filter('translate')('Setup.Modules.RelationSaveSuccess'),
									className: 'success'
								});
								$scope.saving = false;
								$scope.formModal.hide();
							});
				};

				var error = function () {
					$scope.relations = $scope.relationsState;

					if ($scope.formModal) {
						$scope.formModal.hide();
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
					ModuleService.updateModuleRelation(relation, $scope.module.id)
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
						LayoutService.getMyAccount(true)
							.then(function () {
								var relationToDeleteIndex = helper.arrayObjectIndexOf($scope.relations, relation);
								$scope.relations.splice(relationToDeleteIndex, 1);
								ngToast.create({ content: $filter('translate')('Setup.Modules.RelationDeleteSuccess'), className: 'success' });
							});
					})
					.catch(function () {
						$scope.relations = $scope.relationsState;

						if ($scope.formModal) {
							$scope.formModal.hide();
							$scope.saving = false;
						}
					});
			};

			$scope.cancel = function () {
				angular.forEach($scope.currentRelation, function (value, key) {
					$scope.currentRelation[key] = $scope.currentRelationState[key];
				});

				$scope.formModal.hide();
			}
		}
	]);