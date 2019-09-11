'use strict';

angular.module('primeapps')

	.controller('ComponentEnvironmentSettingsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'ComponentsService', 'componentPlaces', 'componentPlaceEnums', 'componentTypeEnums', '$localStorage', 'ComponentsDeploymentService', '$sce',
		function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, ComponentsService, componentPlaces, componentPlaceEnums, componentTypeEnums, $localStorage, ComponentsDeploymentService, $sce) {

			/**Global Config id always 1*/
			$scope.id = 1;
			$scope.$parent.menuTopTitle = $scope.currentApp.label;
			$scope.$parent.activeMenu = 'app';
			$scope.$parent.activeMenuItem = 'components';
			$scope.app = $rootScope.currentApp;

			if (!$scope.id) {
				$state.go('studio.app.components');
			}

			$scope.loading = true;

			$scope.$parent.$parent.tabManage = {
				activeTab: "development"
			};

			$scope.configParameters = [];
			var parameter = {};
			parameter.key = null;
			parameter.value = null;
			$scope.configParameters.push(parameter);

			ComponentsService.get($scope.id)
				.then(function (response) {

					if (!response.data) {
						toastr.error('Component Not Found !');
						$state.go('studio.app.components');
					}

					$scope.content = {};
					/**development,test,production*/
					$scope.currenContent = {};
					$scope.componentCopy = angular.copy(response.data);
					$scope.component = response.data;

					var a = $scope.$parent.$parent.tabManage.activeTab;
					console.log(a);

					if ($scope.component.content) {
						$scope.component.content = JSON.parse($scope.component.content);

						//if ($scope.component.content.files) {
						//	$scope.component.content.files = $scope.component.content.files.join("\n");
						//}

						//var urlParameters = $scope.component.content.url.split('?');
						//$scope.content.url_parameters = urlParameters.length > 1 ? urlParameters[1] : null;

						//if ($scope.component.content.app) {
						//	if ($scope.component.content.app.templateFile && $scope.component.content.app.templateFile.contains('http')) {
						//		$scope.content.templateUrl = true;
						//	}
						//}
					}

					$scope.loading = false;
				});


			$scope.save = function (componentFormValidation) {

				if (!componentFormValidation.$valid) {
					toastr.error($filter('translate')('Module.RequiredError'));
					return;
				}

				$scope.saving = true;

				$scope.copyComponent = angular.copy($scope.component);

				if (!$scope.component.content) {
					$scope.copyComponent.content = {};
				}

				//if ($scope.component.content && $scope.component.content.files) {
				//	$scope.copyComponent.content.files = $scope.component.content.files.split("\n");
				//}

				//if (!$scope.content.templateUrl && $scope.component.content.app && $scope.component.content.app.templateFile) {
				//	$scope.copyComponent.content.app.templateUrl = $scope.component.content.app.templateFile;
				//}

				//$scope.copyComponent.content.url = $scope.content.url + (($scope.content.url_parameters) ? '?' + $scope.content.url_parameters : '');

				$scope.copyComponent.content = JSON.stringify($scope.copyComponent.content);

				ComponentsService.update($scope.id, $scope.copyComponent)
					.then(function (response) {
						$scope.saving = false;
						$scope.editing = false;
						toastr.success("Component updated successfully.");
					})
					.catch(function () {
						$scope.saving = false;
						$scope.editing = false;
						toastr.error("Component not updated successfully.");
					});
			};

			$scope.configParameterRemove = function (itemname) {
				var index = $scope.configParameters.indexOf(itemname);
				$scope.configParameters.splice(index, 1);
			};

			$scope.configParameterAdd = function (addItem) {

				var parameter = {};
				parameter.key = addItem.key;
				parameter.value = addItem.value;

				if (parameter.key && parameter.value) {
					if ($scope.configParameters.length <= 20) {
						$scope.configParameters.push(parameter);
					} else {
						toastr.warning($filter('translate')('Setup.Workflow.MaximumHookWarning'));
					}
				}
				var lastHookParameter = $scope.configParameters[$scope.configParameters.length - 1];
				lastHookParameter.key = null;
				lastHookParameter.value = null;
			};

			$scope.changeTab = function (tabName) {
				switch (tabName) {
					case "development":
						$scope.currenContent = $scope.component.content.development;
						break;
					case "test":
						$scope.currenContent = $scope.component.content.test;
						break;
					case "production":
						$scope.currenContent = $scope.component.content.production;
						break;
				}

				if (Object.keys($scope.currenContent).length > 0) {
					console.log($scope.currenContent);
				}

			};
		}

	]);