'use strict';

angular.module('primeapps')

	.controller('SmsController', ['$rootScope', '$scope', '$filter', 'SmsService', '$mdDialog', 'mdToast', '$stateParams', 'helper', '$state', 'AppService',
		function ($rootScope, $scope, $filter, SmsService, $mdDialog, mdToast, $stateParams, helper, $state, AppService) {
			$scope.loading = false;
			AppService.checkPermission().then(function (res) {

				if (res && res.data) {
					var profile = JSON.parse(res.data["profile"]);
					var customProfilePermissions = undefined;
					if (res.data["customProfilePermissions"])
						customProfilePermissions = JSON.parse(res.data["customProfilePermissions"]);

					if (!profile.HasAdminRights) {
						var smsIsExist = undefined;
						if (customProfilePermissions)
							smsIsExist = customProfilePermissions.permissions.indexOf('sms') > -1;

						if (!smsIsExist) {
							$state.go('app.setup.email');
						}
					}
				}

				$rootScope.breadcrumblist = [
					{
						title: $filter('translate')('Layout.Menu.Dashboard'),
						link: "#/app/dashboard"
					},
					{
						title: $filter('translate')('Setup.Nav.System'),
						link: '#/app/setup/sms'
					},
					{
						title: $filter('translate')('Setup.Messaging.SMS.Title')
					}
				];

				$scope.smsModel = angular.copy($rootScope.system.messaging.SMS) || {};
				$scope.goUrl = function (url) {
					window.location = url;
				};


				$scope.editSMS = function () {
					if ($scope.systemForm.$valid) {
						$scope.loading = true;

						SmsService.updateSMSSettings($scope.smsModel)
							.then(function () {
								mdToast.success($filter('translate')('Setup.Settings.UpdateSuccess'));

								if (!$rootScope.system.messaging.SMS)
									$rootScope.system.messaging.SMS = {};

								$rootScope.system.messaging.SMS.provider = $scope.smsModel.provider;
								$rootScope.system.messaging.SMS.user_name = $scope.smsModel.user_name;
								$rootScope.system.messaging.SMS.alias = $scope.smsModel.alias;
								$scope.loading = false;
							})
							.catch(function (e) {
								$scope.loading = false;
								$scope.systemForm.$submitted = false;
								mdToast.error($filter('translate')('Common.Error'));
							});
						;
					}
				};



				$scope.resetSMSForm = function () {
					$scope.smsModel = angular.copy($rootScope.system.messaging.SMS);
				};

				$scope.removeSMSSettings = function () {
					SmsService.removeSMSSettings($scope.smsModel).then(function () {
						$scope.smsModel = null;
						$rootScope.system.messaging.SMS = null;
					});
				};


				$scope.close = function () {
					$mdDialog.hide();
				};

				$scope.submitGeneral = function () {
					if (!$scope.systemForm.$valid) {
						mdToast.error($filter('translate')('Module.RequiredError'));
						return;
					}

					$scope.editSMS($scope.smsModel);
				};
			});
		}
	]);