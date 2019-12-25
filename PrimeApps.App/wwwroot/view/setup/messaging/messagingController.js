﻿'use strict';

angular.module('primeapps')

	.controller('MessagingController', ['$rootScope', '$scope', '$translate', '$localStorage', 'ngToast', 'config', '$window', '$timeout', '$filter', 'blockUI', 'MessagingService', 'ngTableParams', '$popover', 'AppService',
		function ($rootScope, $scope, $translate, $localStorage, ngToast, config, $window, $timeout, $filter, blockUI, MessagingService, ngTableParams, $popover, AppService) {
			$scope.hasAdminRight = $filter('filter')($rootScope.profiles, { id: $rootScope.user.profile.id }, true)[0].has_admin_rights;
			$scope.smsModel = angular.copy($rootScope.system.messaging.SMS);
			$scope.emailModel = angular.copy($rootScope.system.messaging.SystemEMail);
			$scope.newSender = {};
			$scope.smsEditMode = false;
			$scope.emailEditMode = false;

			if ($scope.emailModel == null || !$scope.emailModel.hasOwnProperty("provider")) {
				$scope.emailModel = {
					provider: "",
					user_name: "",
					password: "",
					senders: [],
					enable_ssl: false,
					send_bulk_email_result: true


				};
			}

			$scope.tableParams = new ngTableParams({
				page: 1,            // show first page
				count: 10           // count per page
			},
				{
					total: 0, // length of data
					getData: function ($defer, params) {
						if ($scope.emailModel.senders) {
							params.total($scope.emailModel.senders.length);
							$defer.resolve($scope.emailModel.senders.slice((params.page() - 1) * params.count(), params.page() * params.count()));
						}
					},
					$scope: $scope
				});

			$scope.tableParams.reload();

			$scope.editSMS = function () {
				if ($scope.smsForm.$valid) {
					$scope.smsUpdating = true;

					MessagingService.updateSMSSettings($scope.smsModel).then(function () {
						ngToast.create({ content: $filter('translate')('Setup.Settings.UpdateSuccess'), className: 'success' });

						if (!$rootScope.system.messaging.SMS)
							$rootScope.system.messaging.SMS = {};

						$rootScope.system.messaging.SMS.provider = $scope.smsModel.provider;
						$rootScope.system.messaging.SMS.user_name = $scope.smsModel.user_name;
						$rootScope.system.messaging.SMS.alias = $scope.smsModel.alias;
						$scope.smsUpdating = false;
					});
				}
			};

			$scope.showNewSenderForm = function () {
				$scope.senderPopover = $scope.senderPopover || $popover(angular.element(document.getElementsByName('addSender')), {
					templateUrl: 'view/setup/messaging/senderAdd.html',
					placement: 'left',
					scope: $scope,
					autoClose: true,
					show: true
				});
			};

			$scope.addNewSender = function (alias, email) {
				if (!this.senderForm.alias.$valid || !this.senderForm.email.$valid) return;

				if ($scope.emailModel.senders == null) {
					$scope.emailModel.senders = [];
				}

				$scope.emailModel.senders.push({
					"alias": alias,
					"email": email
				});
				$scope.senderPopover.hide();
				$scope.tableParams.reload();
				$scope.addNewSender = true;
				$scope.emailForm.$setValidity("noSender", true);
			};

			$scope.removeSender = function (sender) {
				if ($scope.emailModel.senders == null) return;
				var index = $scope.emailModel.senders.indexOf(sender);
				$scope.emailModel.senders.splice(index, 1);
				$scope.tableParams.reload();
			};

			$scope.editEMail = function () {
				if ($scope.emailModel.senders.length == 0) {
					$scope.emailForm.$setValidity("noSender", false);
				}

				if ($scope.emailForm.$valid) {
					$scope.emailUpdating = true;

					if ($scope.emailModel.host.indexOf("yandex") > -1) {
						$scope.emailModel.host = "smtp.yandex.ru";
						$scope.emailModel.port = "587";
						$scope.emailModel.enable_ssl = true;
					}

					MessagingService.updateEMailSettings($scope.emailModel).then(function () {
						ngToast.create({ content: $filter('translate')('Setup.Settings.UpdateSuccess'), className: 'success' });

						MessagingService.getSetting().then(function (response) {
							var messaging = response.data;

							if (messaging.SystemEMail)
								messaging.SystemEMail.enable_ssl = messaging.SystemEMail.enable_ssl === 'True';

							if (messaging.SystemEMail && messaging.SystemEMail.send_bulk_email_result)
								messaging.SystemEMail.send_bulk_email_result = messaging.SystemEMail.send_bulk_email_result === 'True';

							if (messaging.PersonalEMail)
								messaging.PersonalEMail.enable_ssl = messaging.PersonalEMail.enable_ssl === 'True';

							if (messaging.PersonalEMail && messaging.PersonalEMail.send_bulk_email_result)
								messaging.PersonalEMail.send_bulk_email_result = messaging.PersonalEMail.send_bulk_email_result === 'True';

							$rootScope.system.messaging = messaging;
						});

						$scope.emailUpdating = false;
					});
				}
			};


			$scope.resetEMailForm = function () {
				$scope.emailModel = angular.copy($rootScope.system.messaging.SystemEMail);
			};

			$scope.resetSMSForm = function () {
				$scope.smsModel = angular.copy($rootScope.system.messaging.SMS);
			};

			$scope.removeSMSSettings = function () {
				MessagingService.removeSMSSettings($scope.smsModel).then(function () {
					$scope.smsModel = null;
					$rootScope.system.messaging.SMS = null;
				});
			};

			$scope.removeEMailSettings = function () {
				MessagingService.removeEMailSettings($scope.emailModel).then(function () {
					$scope.emailModel = null;
					$rootScope.system.messaging.SystemEMail = null;
				});
			};
		}
	]);