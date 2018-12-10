'use strict';

angular.module('primeapps')

	.controller('AnalyticsFormController', ['$rootScope', '$scope', '$cookies', '$state', '$location', '$localStorage', 'ngToast', 'config', '$window', '$timeout', '$filter', 'blockUI', 'helper', 'FileUploader', 'AnalyticsService', 'ModuleService',
		function ($rootScope, $scope, $cookies, $state, $location, $localStorage, ngToast, config, $window, $timeout, $filter, blockUI, helper, FileUploader, AnalyticsService, ModuleService) {
			$scope.id = $location.search().id;
			$scope.title = $filter('translate')('Setup.Report.NewReport');
			$scope.lookupUser = helper.lookupUser;

			var icons = ModuleService.getIcons();
			$scope.icons = $filter('orderBy')(icons, 'chart');

			if (!$rootScope.user.has_analytics) {
				ngToast.create({ content: $filter('translate')('Common.Forbidden'), className: 'warning' });
				$state.go('app.dashboard');
				return;
			}

			AnalyticsService.getReports()
				.then(function (reports) {
					$scope.analyticsReports = reports.data;
				});

			if ($scope.id) {
				$scope.title = $filter('translate')('Setup.Report.EditReport');

				AnalyticsService.get($scope.id)
					.then(function (report) {
						$scope.reportModel = report.data;

						var fileUrl = $scope.reportModel.pbix_url;
						$scope.reportFileName = fileUrl.slice(fileUrl.indexOf('--') + 2);
					});
			}
			else {
				$scope.reportModel = {};
				$scope.reportModel.sharing_type = 'everybody';
				$scope.reportModel.icon = 'fa fa-bar-chart';
			}

			var success = function (id) {
				$scope.saving = false;
				$state.go('app.analytics', { id: id });
			};

			var uploader = $scope.uploader = new FileUploader({
				url: config.apiUrl + 'analytics/save_pbix',
				headers: {
					'Authorization': 'Bearer ' + $localStorage.read('access_token'),
					'X-Tenant-Id': $cookies.get('tenant_id'),
					'Accept': 'application/json'
				},
				queueLimit: 1
			});

			uploader.onCompleteItem = function (fileItem, response, status, headers) {
				if (status === 200) {
					$scope.report.pbix_url = response.result;
					if (!$scope.id) {
						AnalyticsService.create($scope.report)
							.then(function (response) {
								success(response.data.id);
							})
							.catch(function () {
								$scope.saving = false;
							});
					}
					else {
						AnalyticsService.update($scope.report)
							.then(function () {
								success($scope.report.id);
							})
							.catch(function () {
								$scope.saving = false;
							});
					}
				}
			};

			uploader.onWhenAddingFileFailed = function (item, filter, options) {
				switch (filter.name) {
					case 'pbixFilter':
						ngToast.create({ content: $filter('translate')('Setup.Report.FormatError'), className: 'warning' });
						break;
					case 'sizeFilter':
						ngToast.create({ content: $filter('translate')('Setup.Report.SizeError'), className: 'warning' });
						break;
				}
			};

			uploader.filters.push({
				name: 'pbixFilter',
				fn: function (item, options) {
					var extension = helper.getFileExtension(item.name);
					return extension === 'pbix';
				}
			});

			uploader.filters.push({
				name: 'sizeFilter',
				fn: function (item) {
					return item.size < 2097152;//2 mb
				}
			});

			$scope.clearReportFile = function () {
				uploader.clearQueue();
				$scope.reportFileName = undefined;
				$scope.reportFileCleared = true;
			};

			$scope.save = function () {
				if (!$scope.analyticsForm.$valid || ((!$scope.id && !uploader.queue.length) || ($scope.id && $scope.reportFileCleared && !uploader.queue.length)))
					return;

				$scope.saving = true;
				$scope.report = angular.copy($scope.reportModel);

				if ($scope.report.shares && $scope.report.shares.length) {
					var shares = angular.copy($scope.report.shares);
					$scope.report.shares = [];

					angular.forEach(shares, function (user) {
						$scope.report.shares.push(user.id);
					});
				}

				if (!$scope.id || $scope.reportFileCleared) {
					uploader.uploadAll();
				}
				else {
					AnalyticsService.update($scope.report)
						.then(function () {
							success($scope.report.id);
						})
						.catch(function () {
							$scope.saving = false;
						});
				}
			};

			$scope.cancel = function () {
				$state.go('app.analytics');
			};
		}
	]);