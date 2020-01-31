'use strict';

angular.module('primeapps')

	.controller('PackageController', ['$rootScope', '$scope', '$state', 'PackageService', '$timeout', '$sce', '$location', '$filter', '$localStorage', '$modal', 'ModuleService',
		function ($rootScope, $scope, $state, PackageService, $timeout, $sce, $location, $filter, $localStorage, $modal, ModuleService) {
			$scope.loading = true;
			$scope.activePage = 1;
			$rootScope.runningPackages[$rootScope.currentApp.name] = { status: true };

			$scope.$parent.activeMenu = 'app';
			$scope.$parent.activeMenuItem = 'packages';
			$rootScope.breadcrumblist[2].title = 'Packages';

			$scope.app = $rootScope.currentApp;
			$scope.packageModules = angular.copy($rootScope.appModules);
			$scope.packageModulesRelations = {};
			$scope.errorList = [];
			$scope.package = {};
			$scope.package.allModulesRelations = {};
			$scope.appPackageInformation = JSON.parse($scope.app.setting.options);

			PackageService.getActiveProcess()
				.then(function (response) {
					var activeProcess = response.data;

					if (activeProcess) {
						$rootScope.runningPackages[$scope.app.name] = { status: true };
						$scope.openWS(activeProcess.id);
					} else {
						$rootScope.runningPackages[$scope.app.name] = { status: false };
					}
				})
				.catch(function (response) {
					$rootScope.runningPackages[$scope.app.name] = { status: false };
				});

			$scope.openWS = function (id) {
				if ($rootScope.sockets && $rootScope.sockets[$scope.app.name] && $rootScope.sockets[$scope.app.name].readyState !== WebSocket.CLOSED)
					return;

				if (!$rootScope.sockets)
					$rootScope.sockets = {};

				var isHttps = location.protocol === 'https:';
				$rootScope.sockets[$scope.app.name] = new WebSocket((isHttps ? 'wss' : 'ws') + '://' + location.host + '/log_stream');
				$rootScope.sockets[$scope.app.name].onopen = function (e) {
					$rootScope.sockets[$scope.app.name].send(JSON.stringify({
						'X-App-Id': $scope.app.id,
						'X-Tenant-Id': $rootScope.currentTenantId,
						'X-Organization-Id': $scope.app.organization_id,
						'package_id': id
					}));
				};
				$rootScope.sockets[$scope.app.name].onclose = function (e) {
					var packagesPageActive = $location.$$path.contains('/packages');

					if ($rootScope.runningPackages[$scope.app.name] && $rootScope.runningPackages[$scope.app.name].logs && $rootScope.runningPackages[$scope.app.name].logs.contains('********** Package Created **********')) {
						toastr.success("Your package is ready for app " + $scope.app.label + ".");

						if (packagesPageActive) {
							$rootScope.$broadcast('package-created');
						}

						$rootScope.runningPackages[$scope.app.name].status = false;
						$rootScope.runningPackages[$scope.app.name].logs = "";
						$scope.grid.dataSource.read();
						$timeout(function () {
							$scope.$apply();
						});
					} else {
						if (id) {
							PackageService.get(id)
								.then(function (response) {
									if (response.data) {
										if (response.data.status !== 'running') {
											if (response.data.status === 'succeed') {
												toastr.success("Your package is ready for app " + $scope.app.label + ".");
											} else {
												toastr.error("An unexpected error occurred while creating a package for app " + $scope.app.label + ".");
											}

											if (packagesPageActive) {
												$rootScope.$broadcast('package-created');
											}
											$rootScope.runningPackages[$scope.app.name].status = false;
											$scope.grid.dataSource.read();
											$timeout(function () {
												$scope.$apply();
											});
										} else {
											$scope.openWS($scope.packageId);
										}
									}
								});
						}
					}
				};
				$rootScope.sockets[$scope.app.name].onerror = function (e) {
					console.log(e);
					toastr.error($filter('translate')('Common.Error'));
					$scope.loading = false;
				};
				$rootScope.sockets[$scope.app.name].onmessage = function (e) {
					if (!$rootScope.runningPackages[$scope.app.name].status) {
						$rootScope.runningPackages[$scope.app.name].status = true;
					}

					$rootScope.runningPackages[$scope.app.name].logs = e.data;
					$timeout(function () {
						$scope.$apply();
					});

				};
			};

			$scope.createPackage = function () {
				$scope.errorList = [];
				$scope.package.protectModules = $scope.appPackageInformation["protect_modules"] || 'DontTransfer';
				$scope.package.modules = $scope.appPackageInformation["selected_modules"] || [];
				if ($scope.grid.dataSource.data().length < 1)
					prepareModal($scope.package.modules);

				$scope.packagePopup = $scope.packagePopup || $modal({
					scope: $scope,
					templateUrl: 'view/app/manage/package/packagePopup.html',
					show: false
				});

				$scope.packagePopup.$promise.then(function () {
					$scope.packagePopup.show();
				});
			};

			$scope.getTime = function (time) {
				return moment(time).format("DD-MM-YYYY HH:mm");
			};

			$scope.asHtml = function () {
				return $sce.trustAsHtml($rootScope.runningPackages[$scope.app.name] ? $rootScope.runningPackages[$scope.app.name].logs : '');
			};

			$scope.getIcon = function (status) {
				switch (status) {
					case 'running':
						return $sce.trustAsHtml('<i style="color:#0d6faa;" class="fas fa-clock"></i>');
					case 'failed':
						return $sce.trustAsHtml('<i style="color:rgba(218,10,0,1);" class="fas fa-times"></i>');
					case 'succeed':
						return $sce.trustAsHtml('<i style="color:rgba(16,124,16,1);" class="fas fa-check"></i>');
				}
			};

			//For Kendo UI
			$scope.goUrl = function (item) {
				var selection = window.getSelection();
				if (selection.toString().length === 0) {
					//click event.
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
							url: "/api/package/find",
							type: 'GET',
							dataType: "json",
							beforeSend: function (req) {
								req.setRequestHeader('Authorization', 'Bearer ' + accessToken);
								req.setRequestHeader('X-App-Id', $rootScope.currentAppId);
								req.setRequestHeader('X-Organization-Id', $rootScope.currentOrgId);
							},
							complete: function () {
								$scope.loadingDeployments = false;
								$scope.loading = false;
								if ($scope.grid.dataSource.data().length < 1)
									PackageService.getModulesFields($scope.packageModules, $scope.packageModulesRelations, $scope.package.allModulesRelations);
							},
						}
					},
					schema: {
						data: "items",
						total: "count",
						model: {
							id: "id",
							fields: {
								Version: { type: "string" },
								StartTime: { type: "date" },
								EndTime: { type: "date" },
								Status: { type: "enums" }
							}
						}
					}

				},
				scrollable: false,
				persistSelection: true,
				sortable: true,
				noRecords: true,
				filterable: true,
				filter: function (e) {
					if (e.filter && e.field !== 'Status') {
						for (var i = 0; i < e.filter.filters.length; i++) {
							e.filter.filters[i].ignoreCase = true;
						}
					}
				},
				rowTemplate: function (e) {
					var trTemp = '<tr>';
					trTemp += '<td> <span>' + '<div style="padding:12px 0px;">' + e.version + '</div>' + '</span></td > ';
					trTemp += '<td><span>' + $scope.getTime(e.start_time) + '</span></td>';
					trTemp += '<td> <span>' + $scope.getTime(e.end_time) + '</span></td > ';
					trTemp += '<td style="text-align: center;">' + $scope.getIcon(e.status) + '</td></tr>';
					return trTemp;
				},
				altRowTemplate: function (e) {
					var trTemp = '<tr class="k-alt">';
					trTemp += '<td> <span>' + '<div style="padding:12px 0px;">' + e.version + '</div>' + '</span></td > ';
					trTemp += '<td><span>' + $scope.getTime(e.start_time) + '</span></td>';
					trTemp += '<td> <span>' + $scope.getTime(e.end_time) + '</span></td > ';
					trTemp += '<td style="text-align: center;">' + $scope.getIcon(e.status) + '</td></tr>';
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
						field: 'Version',
						title: 'Version'
					},
					{
						field: 'StartTime',
						title: 'Start Time',
						filterable: {
							ui: function (element) {
								element.kendoDateTimePicker({
									format: '{0: dd-MM-yyyy  hh:mm}'
								})
							}
						}
					},
					{
						field: 'EndTime',
						title: 'End Time',
						filterable: {
							ui: function (element) {
								element.kendoDateTimePicker({
									format: '{0: dd-MM-yyyy  hh:mm}'
								})
							}
						}
					},
					{
						field: 'Status',
						title: 'Status',
						values: [
							{ text: 'Running', value: 'Running' },
							{ text: 'Failed', value: 'Failed' },
							{ text: 'Succeed', value: 'Succeed' }
						]
					}]
			};

			$scope.create = function () {

				var copyRelations = angular.copy($scope.package.allModulesRelations);
				$scope.package.selectedModules = [];
				if ($scope.package.protectModules === "DontTransfer" || $scope.package.protectModules === "AllModules") {
					$scope.errorList = [];				
					$scope.package.modulesRelations = copyRelations;
				} else {
					$scope.package.selected_modules = [];
					for (var i = 0; i < $scope.package.modules.length; i++) {
						var selectedModule = $scope.package.modules[i];
						$scope.package.selectedModules[i] = {};
						$scope.package.selected_modules.push(selectedModule.name);
						$scope.package.selectedModules[i][selectedModule.name] = copyRelations[selectedModule.name];
						delete copyRelations[selectedModule.name];
					}
					$scope.package.modulesRelations = copyRelations;
				}

				PackageService.create($scope.package)
					.then(function (response) {
						toastr.success("Package creation started.");
						$scope.loading = false;
						$scope.packageId = response.data;
						$scope.grid.dataSource.read();
						$scope.openWS(response.data);

						// if ($location.$$path.contains('/packages'))
						//     $scope.grid.dataSource.read();

						$rootScope.runningPackages[$scope.app.name] = { status: true };
						$scope.appPackageInformation.protect_modules = $scope.package.protectModules;
						$scope.appPackageInformation.selected_modules = $scope.package.selected_modules || [];
						$rootScope.currentApp.setting.options = JSON.stringify($scope.appPackageInformation);
					})
					.catch(function (response) {
						$scope.loading = false;

						if (response.status === 409) {
							toastr.error(response.data);
						} else {
							toastr.error($filter('translate')('Common.Error'));
							console.log(response);
						}
					}).finally(function () {
						$scope.packagePopup.hide();
						$scope.package.selectedModules = [];
						$scope.package.modulesRelations = {};
						$scope.errorList = [];
					});
			};

			$scope.checkModules = function (selectedModules) {
				$scope.errorList = PackageService.checkModules(selectedModules, $scope.errorList, $scope.packageModulesRelations);
				$scope.getErrorText();
			};

			$scope.getErrorText = function () {
				return PackageService.getErrorText($scope.errorList, $scope.packageModules);
			};

			function prepareModal(selectedModulesArray) {
				if (selectedModulesArray.length > 0) {
					var selectedModules = PackageService.preparePackage(selectedModulesArray, $scope.packageModules);
					$scope.checkModules(selectedModules);
					$scope.package.modules = selectedModules;
				}
			}
		}
	]);