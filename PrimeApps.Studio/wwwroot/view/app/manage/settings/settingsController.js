'use strict';

angular.module('primeapps')

	.controller('SettingsController', ['$rootScope', '$scope', '$state', 'SettingsService', '$location', '$controller', '$filter', 'AppFormService', 'ModuleService', 'PackageService', '$q',
		function ($rootScope, $scope, $state, SettingsService, $location, $controller, $filter, AppFormService, ModuleService, PackageService, $q) {

			$scope.publishModelContainer = {};
			$scope.checkedModule = {};
			$scope.wizardStep = 0;
			$scope.publishModel = {};
			$scope.publishInfoReady = false;
			$scope.$parent.activeMenu = 'app';
			$scope.$parent.activeMenuItem = 'settings';
			$rootScope.breadcrumblist[2].title = 'Settings';

			$scope.app = angular.copy($rootScope.currentApp);
			$scope.app.setting.options = JSON.parse($scope.app.setting.options);
			getPackageSettings();

			$scope.save = function () {

				$scope.loading = true;
				var appModel = {};
				appModel.setting = {};

				appModel.name = $scope.app.name;
				appModel.id = $scope.app.id;
				appModel.label = $scope.app.label;
				appModel.description = $scope.app.description;
				appModel.logo = $scope.app.logo;
				appModel.icon = $scope.app.icon;
				appModel.color = $scope.app.color;
				appModel.enable_registration = $scope.app.setting.options.enable_registration;
				appModel.enable_api_registration = $scope.app.setting.options.enable_api_registration;
				appModel.enable_ldap = $scope.app.setting.options.enable_ldap;

				preparePackage();

				appModel.protect_modules = $scope.app.setting.options.protect_modules;
				appModel.selected_modules = $scope.app.setting.options.selected_modules;

				appModel.app_domain = $scope.app.setting.app_domain;
				appModel.auth_domain = $scope.app.setting.auth_domain;

				AppFormService.update($rootScope.currentApp.id, appModel)
					.then(function (response) {
						$scope.loading = false;
						toastr.success("App settings are updated successfully.");

						$rootScope.currentApp.name = $scope.app.name;
						$rootScope.currentApp.label = $scope.app.label;
						$rootScope.currentApp.description = $scope.app.description;
						$rootScope.currentApp.logo = $scope.app.logo;
						$rootScope.currentApp.icon = $scope.app.icon;
						$rootScope.currentApp.color = $scope.app.color;
						getPackageSettings();
						$rootScope.currentApp.setting.options = response.data.setting.options;
					})
					.catch(function () {
						toastr.error($filter('translate')('Common.Error'));
						$scope.loading = false;
					});
			};

			function preparePackage() {
				var copyRelations = angular.copy($scope.allModulesRelations);
				if ($scope.app.setting.options.protect_modules === "DontTransfer" || $scope.app.setting.options.protect_modules === "AllModules") {
					$scope.errorList = [];
					$scope.app.setting.options.selected_modules = [];
					$scope.app.setting.options.modulesRelations = copyRelations;	
				} else {
					for (var i = 0; i < $scope.app.setting.options.selected_modules.length; i++) {
						var selectedModule = $scope.app.setting.options.selected_modules[i];
						$scope.app.setting.options.selected_modules[i] = selectedModule.name;
						//$scope.app.setting.options.selected_modules[i] = {};
						//$scope.app.setting.options.selected_modules[i][selectedModule.name] = copyRelations[selectedModule.name];
						delete copyRelations[selectedModule.name];
					}
					$scope.app.setting.options.modulesRelations = copyRelations;
				}
			}

			function getPackageSettings() {
				$scope.app.setting.options.protect_modules = $scope.app.setting.options.protect_modules || 'DontTransfer';
				$scope.packageModules = angular.copy($rootScope.appModules);
				$scope.errorList = [];
				$scope.allModulesRelations = {};
				$scope.packageCount = 0;
				$scope.packageModulesRelations = {};
				var promises = [];

				PackageService.count().then(function (res) {
					$scope.packageCount = res.data;
				});

				angular.forEach($scope.packageModules, function (module) {
					var deferred = $q.defer();
					if (!module.fields)
						ModuleService.getModuleFields(module.name).then(function (response) {
							module.fields = response.data;
							var lookupList = $filter('filter')(module.fields, function (field) {
								return field.data_type === 'lookup' && field.lookup_type !== 'users' && field.lookup_type !== 'profiles' && field.lookup_type !== 'roles';
							});

							$scope.packageModulesRelations[module.name] = [];
							$scope.allModulesRelations[module.name] = [];

							for (var k = 0; k < lookupList.length; k++) {
								$scope.packageModulesRelations[module.name].push(lookupList[k]);
								$scope.allModulesRelations[module.name].push({
									name: lookupList[k].name,
									lookup_type: lookupList[k].lookup_type
								});
							}
							deferred.resolve(response);
						});
					promises.push(deferred.promise);
				});

				$scope.checkModules = function (selectedModules) {
					PackageService.checkModules(selectedModules, $scope.errorList, $scope.packageModulesRelations);
					$scope.getErrorText();
				};

				$scope.getErrorText = function () {
					return PackageService.getErrorText($scope.errorList, $scope.packageModules);
				};

				$q.all(promises).then(function () {
					var selectedModules = PackageService.preparePackage($scope.app.setting.options.selected_modules, $scope.packageModules) || [];
					$scope.checkModules(selectedModules);
					$scope.app.setting.options.selected_modules = selectedModules;
				});
			}
		}
	]);