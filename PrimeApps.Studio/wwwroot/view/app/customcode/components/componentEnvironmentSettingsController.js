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
					$scope.component = response.data;
					$scope.componentCopy = angular.copy(response.data);

					if ($scope.component.content) {

						$scope.component.content = JSON.parse($scope.component.content);
						$scope.contentCopy = angular.copy($scope.component.content);

						var activeTab = $scope.$parent.$parent.tabManage.activeTab;
						$scope.changeTab(activeTab);
					}

					$scope.loading = false;
				});


			$scope.save = function (componentFormValidation) {

				//if (!componentFormValidation.$valid) {
				//	toastr.error($filter('translate')('Module.RequiredError'));
				//	return;
				//}

				$scope.saving = true;
				$scope.loading = true;


				if (!$scope.content) {
					$scope.content = {};
				}

				if ($scope.content && $scope.content.trusted_urls && $scope.content.trusted_urls.url) {
					$scope.currenContent.trusted_urls[0].url = $scope.content.trusted_urls.url;
				}

				if ($scope.content && $scope.content.trusted_urls && $scope.content.trusted_urls.headers) {
					$scope.currenContent.trusted_urls[0].headers["X-User-Id"] = $scope.content.trusted_urls.headers.x_user_id;
					$scope.currenContent.trusted_urls[0].headers["X-Tenant-Id"] = $scope.content.trusted_urls.headers.x_tenant_id;
				}

				if ($scope.content && $scope.content.imports && $scope.content.imports.css) {

					var cssBeforeArray = $scope.content.imports.css.before.length > 0 ? $scope.content.imports.css.before.split(';') : [];
					angular.forEach(cssBeforeArray, function (value) {
						$scope.currenContent.imports.css.before.push(value);
					});

					var cssAfterArray = $scope.content.imports.css.after.length > 0 ? $scope.content.imports.css.after.split(';') : [];
					angular.forEach(cssAfterArray, function (value) {
						$scope.currenContent.imports.css.after.push(value);
					});
				}
				if ($scope.content && $scope.content.imports && $scope.content.imports.js) {

					var jsBeforeArray = $scope.content.imports.js.length > 0 ? $scope.content.imports.js.before.split(';') : [];
					angular.forEach(jsBeforeArray, function (value) {
						$scope.currenContent.imports.js.before.push(value);
					});

					var jsAfterArray = $scope.content.imports.js.before.length > 0 ? $scope.content.imports.css.before.split(';') : [];
					angular.forEach(jsAfterArray, function (value) {
						$scope.currenContent.imports.js.after.push(value);
					});
				}
				if ($scope.configParameters && $scope.configParameters.length > 0) {
					var array = [];
					$scope.currenContent.configs = {};
					angular.forEach($scope.configParameters, function (configParameter, key) {
						var query = configParameter.key + '":"' + configParameter.value + '"';
						if (key === 0) {
							query = '{"' + query;
						}
						else if (key > 0 && key !== $scope.configParameters.length - 1) {
							query = '"' + query;
						}
						else {
							query = '"' + query + "}";
						}
						array.push(query);
					});

					$scope.currenContent.configs = JSON.parse(array.toString());
				}

				switch ($scope.$parent.$parent.tabManage.activeTab) {
					case "development":
						$scope.contentCopy.development = $scope.currenContent;
						break;
					case "test":
						$scope.contentCopy.test = $scope.currenContent;
						break;
					case "production":
						$scope.contentCopy.production = $scope.currenContent;
						break;
				}

				$scope.componentCopy.content = JSON.stringify($scope.contentCopy);

				ComponentsService.update($scope.id, $scope.componentCopy)
					.then(function (response) {
						$scope.saving = false;
						toastr.success("Global Config updated successfully.");
						$scope.loading = false;
					})
					.catch(function () {
						$scope.saving = false;
						$scope.loading = false;
						toastr.error("Global Config not updated successfully.");
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
				var lastConfigParameter = $scope.configParameters[$scope.configParameters.length - 1];
				lastConfigParameter.key = null;
				lastConfigParameter.value = null;
			};

			$scope.changeTab = function (tabName) {

				$scope.$parent.$parent.tabManage.activeTab = tabName;

				switch (tabName) {
					case "development":
						$scope.currenContent = $scope.contentCopy.development;
						break;
					case "test":
						$scope.currenContent = $scope.contentCopy.test;
						break;
					case "production":
						$scope.currenContent = $scope.contentCopy.production;
						break;
				}

				if (Object.keys($scope.currenContent).length > 0) {

					if ($scope.currenContent.trusted_urls) {
						$scope.content.trusted_urls = {};
						$scope.content.trusted_urls.headers = {};
						$scope.content.trusted_urls.url = $scope.currenContent.trusted_urls[0].url;
						$scope.content.trusted_urls.headers.x_user_id = $scope.currenContent.trusted_urls[0].headers["X-User-Id"];
						$scope.content.trusted_urls.headers.x_tenant_id = $scope.currenContent.trusted_urls[0].headers["X-Tenant-Id"];
					}
					if ($scope.currenContent.route_template_urls) {
						$scope.content.route_template_urls = $scope.currenContent.route_template_urls;
					}
					if ($scope.currenContent.imports) {
						$scope.content.imports = {};
						$scope.content.imports.css = {};
						$scope.content.imports.js = {};
						$scope.content.imports.css.before = $scope.currenContent.imports.css.before;
						$scope.content.imports.css.after = $scope.currenContent.imports.css.after;
						$scope.content.imports.js.before = $scope.currenContent.imports.js.before;
						$scope.content.imports.js.after = $scope.currenContent.imports.js.after;
					}
					if ($scope.currenContent.configs) {

						$scope.configParameters = [];

						angular.forEach($scope.currenContent.configs, function (value, key) {
							var parameter = {};
							parameter.key = key;
							parameter.value = value;
							$scope.configParameters.push(parameter);
						});
					}
				}
			};
		}
	]);