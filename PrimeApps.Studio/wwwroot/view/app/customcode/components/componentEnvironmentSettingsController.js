'use strict';

angular.module('primeapps')

	.controller('ComponentEnvironmentSettingsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'ComponentsService', 'componentPlaces', 'componentPlaceEnums', 'componentTypeEnums', '$localStorage', 'ComponentsDeploymentService', '$sce',
		function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, ComponentsService, componentPlaces, componentPlaceEnums, componentTypeEnums, $localStorage, ComponentsDeploymentService, $sce) {

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

			ComponentsService.getGlobalConfig()
				.then(function (response) {

					if (!response.data) {
						toastr.error('Component Not Found !');
						$state.go('studio.app.components');
					}

					$scope.id = response.data.id;
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


			$scope.save = function (contentFormValidation) {

				$scope.saving = true;
				$scope.loading = true;

				prepareContent($scope.content);
				switch ($scope.$parent.$parent.tabManage.activeTab) {
					case "development":
						$scope.contentCopy.development = $scope.currenContent;
						$scope.contentCopy.test = $scope.testContentState ? $scope.testContentState : $scope.contentCopy.test;
						$scope.contentCopy.production = $scope.productionContentState ? $scope.productionContentState : $scope.contentCopy.production;
						break;
					case "test":
						$scope.contentCopy.test = $scope.currenContent;
						$scope.contentCopy.development = $scope.developmentContentState ? $scope.developmentContentState : $scope.contentCopy.development;
						$scope.contentCopy.production = $scope.productionContentState ? $scope.productionContentState : $scope.contentCopy.production;
						break;
					case "production":
						$scope.contentCopy.development = $scope.developmentContentState ? $scope.developmentContentState : $scope.contentCopy.development;
						$scope.contentCopy.test = $scope.testContentState ? $scope.testContentState : $scope.contentCopy.test;
						$scope.contentCopy.production = $scope.currenContent;
						break;
				}

				$scope.componentCopy.content = JSON.stringify($scope.contentCopy);

				ComponentsService.update($scope.id, $scope.componentCopy)
					.then(function () {
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
					}
				}
				var lastConfigParameter = $scope.configParameters[$scope.configParameters.length - 1];
				lastConfigParameter.key = null;
				lastConfigParameter.value = null;
			};

			$scope.changeTab = function (tabName, previousContent) {

				$scope.previousTabName = $scope.$parent.$parent.tabManage.activeTab;
				$scope.$parent.$parent.tabManage.activeTab = tabName;

				setPreviousContent($scope.previousTabName, previousContent);

				switch (tabName) {
					case "development":
						$scope.currenContent = $scope.developmentContentState ? $scope.developmentContentState : $scope.contentCopy.development;
						break;
					case "test":
						$scope.currenContent = $scope.testContentState ? $scope.testContentState : $scope.contentCopy.test;
						break;
					case "production":
						$scope.currenContent = $scope.productionContentStat ? $scope.productionContentStat : $scope.contentCopy.production;
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
						$scope.content.imports.css.before = [];
						$scope.content.imports.css.after = [];
						$scope.content.imports.js = {};
						$scope.content.imports.js.before = [];
						$scope.content.imports.js.after = [];

						angular.forEach($scope.currenContent.imports.css.before, function (value) {
							if (value !== "")
								$scope.content.imports.css.before += value + ";";
						});

						angular.forEach($scope.currenContent.imports.css.after, function (value) {
							if (value !== "")
								$scope.content.imports.css.after += value + ";";
						});

						angular.forEach($scope.currenContent.imports.js.before, function (value) {
							if (value !== "")
								$scope.content.imports.js.before += value + ";";
						});

						angular.forEach($scope.currenContent.imports.js.after, function (value) {
							if (value !== "")
								$scope.content.imports.js.after += value + ";";
						});
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

			var prepareConfigs = function () {
				var configArray = [];
				$scope.currenContent.configs = {};
				angular.forEach($scope.configParameters, function (configParameter, key) {
					var query = configParameter.key + '":"' + configParameter.value + '"';
					if (key === 0 && key !== $scope.configParameters.length - 1) {
						query = '{"' + query;
					}
					else if (key > 0 && key !== $scope.configParameters.length - 1) {
						query = '"' + query;
					}
					else if (key !== 0 && key === $scope.configParameters.length - 1) {
						query = '"' + query + "}";
					}
					else {
						query = '{"' + query + '}';
					}
					configArray.push(query);
				});
				return configArray.toString();
			};

			var setPreviousContent = function (previousTabName, previousContent) {
				if (previousContent) {
					previousContent = prepareContent(previousContent);
					switch (previousTabName) {
						case "development":
							$scope.developmentContentState = previousContent ? previousContent : $scope.contentCopy.development;
							break;
						case "test":
							$scope.testContentState = previousContent ? previousContent : $scope.contentCopy.test;
							break;
						case "production":
							$scope.productionContentState = previousContent ? previousContent : $scope.contentCopy.production;
							break;
					}
				}
			};

			var prepareContent = function (content) {

				if (content && content.trusted_urls && content.trusted_urls.url) {
					$scope.currenContent.trusted_urls[0].url = $scope.content.trusted_urls.url;
				}

				if (content && content.trusted_urls && content.trusted_urls.headers) {
					$scope.currenContent.trusted_urls[0].headers["X-User-Id"] = content.trusted_urls.headers.x_user_id;
					$scope.currenContent.trusted_urls[0].headers["X-Tenant-Id"] = content.trusted_urls.headers.x_tenant_id;
				}

				if (content && content.imports && content.imports.css) {

					var cssBeforeArray = content.imports.css.before.length > 0 ? content.imports.css.before.split(';') : [];
					$scope.currenContent.imports.css.before = [];
					angular.forEach(cssBeforeArray, function (value) {
						if (value !== "")
							$scope.currenContent.imports.css.before.push(value);
					});

					var cssAfterArray = content.imports.css.after.length > 0 ? content.imports.css.after.split(';') : [];
					$scope.currenContent.imports.css.after = [];
					angular.forEach(cssAfterArray, function (value) {
						if (value !== "")
							$scope.currenContent.imports.css.after.push(value);
					});
				}
				if (content && content.imports && content.imports.js) {

					var jsBeforeArray = content.imports.js.before.length > 0 ? content.imports.js.before.split(';') : [];
					$scope.currenContent.imports.js.before = [];
					angular.forEach(jsBeforeArray, function (value) {
						if (value !== "")
							$scope.currenContent.imports.js.before.push(value);
					});

					var jsAfterArray = content.imports.js.after.length > 0 ? content.imports.js.after.split(';') : [];
					$scope.currenContent.imports.js.after = [];
					angular.forEach(jsAfterArray, function (value) {
						if (value !== "")
							$scope.currenContent.imports.js.after.push(value);
					});
				}
				if ($scope.configParameters && $scope.configParameters.length > 0) {

					var configsArrayString = prepareConfigs();
					$scope.currenContent.configs = JSON.parse(configsArrayString);
				}

				return $scope.currenContent;
			};
		}
	]);