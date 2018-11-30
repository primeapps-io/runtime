'use strict';

angular.module('primeapps')

	.controller('ModuleSetupController', ['$rootScope', '$scope', '$filter', '$state', 'ngToast', '$dropdown', '$modal', 'helper', 'ModuleService', '$cache', 'AppService', 'LicenseService',
		function ($rootScope, $scope, $filter, $state, ngToast, $dropdown, $modal, helper, ModuleService, $cache, AppService, LicenseService) {
			var getModules = function () {
				$scope.modulesSetup = [];

				angular.forEach($rootScope.modules, function (module) {
					if (module.order != 0)
						$scope.modulesSetup.push(module);
				});

				$scope.customModules = $filter('filter')($scope.modulesSetup, { system_type: 'custom' });
			};

			$scope.user = $rootScope.user;

			getModules();

			$scope.openDropdown = function (moduleItem) {
				$scope['dropdown' + moduleItem.name] = $scope['dropdown' + moduleItem.name] || $dropdown(angular.element(document.getElementById('actionButton-' + moduleItem.name)), {
					placement: 'bottom-right',
					scope: $scope,
					animation: '',
					show: true
				});

				var menuItems = [
					{
						'text': $filter('translate')('Common.Edit'),
						'href': '#app/setup/module?id=' + moduleItem.name
					}
				];

				if (moduleItem.name != 'opportunities' && moduleItem.name != 'activities' && moduleItem.name != 'mails' && moduleItem.name != 'quotes' && moduleItem.name != 'sales_orders' && moduleItem.name != 'purchase_orders') {
					menuItems.push(
						{
							'text': $filter('translate')('Common.Copy'),
							'click': 'moduleLicenseCopyCountLimit(\'' + moduleItem.name + '\')'
						}
					);
				}

				if (moduleItem.system_type != 'system') {
					menuItems.push(
						{
							'text': $filter('translate')('Common.Delete'),
							'click': 'showDeleteForm(\'' + moduleItem.id + '\')'
						}
					);
				}

				if (moduleItem.name === 'leads') {
					menuItems.push(
						{
							'text': $filter('translate')('Setup.Conversion.ConversionMapping'),
							'href': '#app/setup/leadconvertmap'
						}
					);
				}
				if (moduleItem.name === 'quotes') {
					menuItems.push(
						{
							'text': $filter('translate')('Setup.Conversion.ConversionMapping'),
							'href': '#app/setup/quoteconvertmap'
						}
					);
				}

				if (moduleItem.name === 'adaylar') {
					menuItems.push(
						{
							'text': $filter('translate')('Setup.Conversion.ConversionMapping'),
							'href': '#app/setup/candidateconvertmap'
						}
					);
				}

				menuItems.push(
					{
						'divider': true
					},
					{
						'text': $filter('translate')('Setup.Modules.ModuleRelations'),
						'href': '#app/setup/module/relations/' + moduleItem.name
					},
					{
						'text': $filter('translate')('Setup.Modules.FieldsDependencies'),
						'href': '#app/setup/module/dependencies/' + moduleItem.name
					},
					{
						'text': $filter('translate')('Setup.Modules.ActionButtons'),
						'href': '#app/setup/module/actionButtons/' + moduleItem.name
					},
					{
						'text': $filter('translate')('Setup.Modules.ModuleProfileSettings'),
						'href': '#app/setup/module/moduleProfileSettings/' + moduleItem.name
					}
				);

				$scope['dropdown' + moduleItem.name].$scope.content = menuItems;
			};

			$scope.showDeleteForm = function (moduleId) {
				$scope.selectedModuleId = moduleId;

				$scope.deleteModal = $scope.deleteModal || $modal({
					scope: $scope,
					template: 'view/setup/modules/deleteForm.html',
					animation: '',
					backdrop: 'static',
					show: false
				});

				$scope.deleteModal.$promise.then(function () {
					$scope.deleteModal.show();
				});
			};

			$scope.moduleLicenseCopyCountLimit = function (moduleItem) {
				//$scope.moduleLicenseCount = 0;

				//if ($rootScope.user.module_license_count && $rootScope.user.module_license_count > 0) {
				//    $scope.moduleLicenseCount = $rootScope.user.module_license_count;
				//}
				//else {
				//    switch ($rootScope.user.appId) {
				//        case 1:
				//        case 5:
				//            $scope.moduleLicenseCount = 2;
				//            break;
				//        default:
				//            $scope.moduleLicenseCount = 1;
				//            break;
				//    }
				//}
				/**
				* Commentlenen kısımda User'ın module lisansı kontrol ediliyordu
				* Olması gereken Tenant'ın module lisans kontrolü
				*/

				LicenseService.getModuleLicenseCount()
					.then(function (response) {
						var tenantModuleLicenseCount = response.data;
						if ($scope.customModules.length >= tenantModuleLicenseCount) {
							ngToast.create({ content: $filter('translate')('Setup.License.ModuleLicanse', { count: tenantModuleLicenseCount }), className: 'warning' });
						}
						else {
							window.location = "#app/setup/module?clone=" + moduleItem
						}
					})
			};


			$scope.moduleLicenseCountLimit = function () {
				//$scope.moduleLicenseCount = 0;

				//if ($rootScope.user.module_license_count && $rootScope.user.module_license_count > 0) {
				//    $scope.moduleLicenseCount = $rootScope.user.module_license_count;
				//}
				//else {
				//    switch ($rootScope.user.appId) {
				//        case 1:
				//        case 5:
				//            $scope.moduleLicenseCount = 2;
				//            break;
				//        default:
				//            $scope.moduleLicenseCount = 1;
				//            break;
				//    }
				//}

				/**
				* Commentlenen kısımda User'ın module lisansı kontrol ediliyordu
				* Olması gereken Tenant'ın module lisans kontrolü
				*/
				LicenseService.getModuleLicenseCount()
					.then(function (response) {
						var tenantModuleLicenseCount = response.data;
						if ($scope.customModules.length >= tenantModuleLicenseCount) {
							ngToast.create({ content: $filter('translate')('Setup.License.ModuleLicanse', { count: tenantModuleLicenseCount }), className: 'warning' });
						}
						else {
							$state.go('app.setup.module');
						}
					})
			};


			$scope.delete = function () {
				$scope.deleting = true;

				ModuleService.delete($scope.selectedModuleId)
					.then(function () {
						var deletedModule = $filter('filter')($rootScope.modules, { id: parseInt($scope.selectedModuleId) }, true)[0];
						deletedModule.display = false;
						deletedModule.order = 0;
						//Disable another module fields that are linked to the deleted module.
						for (var moduleKey = $rootScope.modules.length - 1; moduleKey >= 0; moduleKey--) {
							for (var fieldKey = $rootScope.modules[moduleKey].fields.length - 1; fieldKey >= 0; fieldKey--) {
								if ($rootScope.modules[moduleKey].fields[fieldKey].lookup_type == deletedModule.name) {
									$rootScope.modules[moduleKey].fields.splice(fieldKey, 1);
									var cacheKey = $rootScope.modules[moduleKey].name;
									$cache.remove(cacheKey + "_" + cacheKey)
								}
							}
						}
						getModules();
						helper.getPicklists([0], true);

						ngToast.create({
							content: $filter('translate')('Setup.Modules.DeleteSuccess'),
							className: 'success'
						});

						$scope.deleting = false;
						$scope.deleteModal.hide();

					})
					.catch(function () {
						$scope.deleteModal.hide();
					});
			}
		}
	]);