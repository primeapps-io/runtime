'use strict';

angular.module('primeapps')

	.controller('ModuleFormController', ['$rootScope', '$scope', 'ngToast', '$filter', 'helper', '$location', '$state', '$stateParams', '$q', '$window', '$localStorage', '$cache', 'config', '$timeout', 'operations', '$modal', 'FileUploader', 'activityTypes', 'transactionTypes', 'ModuleService', 'DocumentService', '$http', 'resizeService', 'components', '$cookies',
		function ($rootScope, $scope, ngToast, $filter, helper, $location, $state, $stateParams, $q, $window, $localStorage, $cache, config, $timeout, operations, $modal, FileUploader, activityTypes, transactionTypes, ModuleService, DocumentService, $http, resizeService, components, $cookies) {
			$scope.type = $stateParams.type;
			$scope.subtype = $stateParams.stype;
			$scope.id = $location.search().id;
			$scope.parentType = $location.search().ptype;
			$scope.parentId = $location.search().pid;
			$scope.returnTab = $location.search().rtab;
			$scope.previousParentType = $location.search().pptype;
			$scope.previousParentId = $location.search().ppid;
			$scope.previousReturnTab = $location.search().prtab;
			$scope.back = $location.search().back;
			$scope.many = $location.search().many;
			$scope.clone = $location.search().clone;
			$scope.revise = $location.search().revise;
			$scope.paramField = $location.search().field;
			$scope.paramValue = $location.search().value;
			$scope.operations = operations;
			$scope.hasPermission = helper.hasPermission;
			$scope.hasDocumentsPermission = helper.hasDocumentsPermission;
			$scope.hasAdminRights = helper.hasAdminRights;
			$scope.hasFieldFullPermission = ModuleService.hasFieldFullPermission;
			$scope.hasSectionFullPermission = ModuleService.hasSectionFullPermission;
			$scope.hasActionButtonDisplayPermission = ModuleService.hasActionButtonDisplayPermission;
            $scope.lookupUserAndGroup = helper.lookupUserAndGroup;
            $scope.lookupUser = helper.lookupUser;
			$scope.loading = true;
            $scope.image = {};
            $scope.hasProcessEditPermission = false;
            $scope.userAdded = false;

			if ($scope.parentId)
				$window.scrollTo(0, 0);

			$scope.module = $filter('filter')($rootScope.modules, { name: $scope.type }, true)[0];

			//allow encrypted fields
			if (!$scope.id) {
				for (var f = 0; f < $scope.module.fields.length; f++) {
					var field = $scope.module.fields[f];
					if (field.show_lock) {
						field.show_lock = false;
					}
				}
			}

			$rootScope.activeModuleName = $scope.parentType;
			if (!$scope.module) {
				ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
				$state.go('app.dashboard');
				return;
			}

			$scope.dropdownFields = $filter('filter')($scope.module.fields, { data_type: 'lookup', show_as_dropdown: true }, true);
			$scope.dropdownFieldDatas = {};
			for (var i = 0; i < $scope.dropdownFields.length; i++) {
				$scope.dropdownFieldDatas[$scope.dropdownFields[i].name] = [];
			}

            if (!$scope.id && !$scope.hasPermission($scope.type, $scope.operations.write)) {
                ngToast.create({ content: $filter('translate')('Common.Forbidden'), className: 'warning' });
                $state.go('app.crm.dashboard');
                return;
            }

			$scope.primaryField = $filter('filter')($scope.module.fields, { primary: true })[0];
			$scope.currentUser = ModuleService.processUser($rootScope.user);
			$scope.currentDayMin = helper.getCurrentDateMin().toISOString();
			$scope.currentDayMax = helper.getCurrentDateMax().toISOString();
			$scope.currentHour = helper.floorMinutes(new Date());
			$scope.relatedToField = $filter('filter')($scope.module.fields, { name: 'related_to' }, true)[0];
			$scope.record = {};
			$scope.customLeaveFields = {};

			if (!$scope.id) {
				$scope.title = $filter('translate')('Module.New', { title: $scope.module['label_' + $rootScope.language + '_singular'] });
			}

			//Sets holidays to business days
			var setHolidays = function () {
				if ($scope.module.name === 'leaves' || $scope.module.name === 'izinler') {
					var holidaysModule = $filter('filter')($rootScope.modules, { name: 'holidays' }, true)[0];

					if (holidaysModule) {
						var countryField = $filter('filter')(holidaysModule.fields, { name: 'country' }, true)[0];

						helper.getPicklists([countryField.picklist_id])
							.then(function (picklists) {
								var countryPicklist = picklists[countryField.picklist_id];
								var countryPicklistItemTr = $filter('filter')(countryPicklist, { value: 'tr' }, true)[0];
								var countryPicklistItemEn = $filter('filter')(countryPicklist, { value: 'en' }, true)[0];
								var language = window.localStorage['NG_TRANSLATE_LANG_KEY'] || 'tr';
								var request = {};
								request.limit = 1000;

								if ($rootScope.language === 'tr')
									request.filters = [{ field: 'country', operator: 'equals', value: countryPicklistItemTr.labelStr, no: 1 }];
								else
									request.filters = [{ field: 'country', operator: 'is', value: countryPicklistItemEn.labelStr }];

								ModuleService.findRecords('holidays', request)
									.then(function (response) {
										var data = response.data;
										var holidays = [];
										for (var i = 0; i < data.length; i++) {
											if (!data[i].half_day) {
												var date = moment(data[i].date).format('DD-MM-YYYY');
												holidays.push(date);
											}
										}
										var workingWeekdays = [1, 2, 3, 4, 5];

										var workSaturdays = $filter('filter')($rootScope.moduleSettings, { key: 'work_saturdays' }, true);
										if (workSaturdays.length > 0 && workSaturdays[0].value === 't') {
											workingWeekdays.push(6);
										}
										$rootScope.holidaysData = data;

										moment.locale(language, {
											week: { dow: 1 }, // Monday is the first day of the week.
											workingWeekdays: workingWeekdays, // Set working weekdays.
											holidays: holidays,
											holidayFormat: 'DD-MM-YYYY'
										});
									});
							});
					}
				}
			};

            setHolidays();

            //checks if user has permission for editin process record
            for (var j = 0; j < $rootScope.approvalProcesses.length; j++) {
                var currentProcess = $rootScope.approvalProcesses[j];
                if (currentProcess.module_id === $scope.module.id)
                    $scope.currentModuleProcess = currentProcess;
            }

            if ($scope.currentModuleProcess) {
                var profileIds = $scope.currentModuleProcess.profiles.split(',');
                for (var i = 0; i < profileIds.length; i++) {
                    if (profileIds[i] === $rootScope.user.profile.id.toString())
                        $scope.hasProcessEditPermission = true;
                }
            }

			if ($scope.parentType) {
				if ($scope.type === 'activities' || $scope.type === 'mails' || $scope.many) {
					$scope.parentModule = $scope.parentType;
				}
				else if ($scope.type === 'current_accounts') {
					if ($scope.id) {
						if ($scope.parentType === 'supplier')
							$scope.parentModule = 'suppliers';
						else if ($scope.parentType === 'customer')
							$scope.parentModule = 'accounts';
					} else {
						$scope.parentModule = $scope.parentType;
					}
				}
				else {
					var parentTypeField = $filter('filter')($scope.module.fields, { name: $scope.parentType }, true)[0];

					if (!parentTypeField) {
						$scope.parentType = null;
						$scope.parentId = null;
					}
					else {
						$scope.parentModule = parentTypeField.lookup_type;
					}
				}
			}

			if ($scope.type === 'quotes') {
				$scope.productModule = $filter('filter')($rootScope.modules, { name: 'products' }, true)[0];
				$scope.quoteProductModule = $filter('filter')($rootScope.modules, { name: 'quote_products' }, true)[0];
				$scope.productCurrencyField = $filter('filter')($scope.productModule.fields, { name: 'currency' }, true)[0];
			}
			else if ($scope.type === 'sales_orders') {
				$scope.productModule = $filter('filter')($rootScope.modules, { name: 'products' }, true)[0];
				$scope.orderProductModule = $filter('filter')($rootScope.modules, { name: 'order_products' }, true)[0];
				$scope.productCurrencyField = $filter('filter')($scope.productModule.fields, { name: 'currency' }, true)[0];
			}
			else if ($scope.type === 'purchase_orders') {
				$scope.productModule = $filter('filter')($rootScope.modules, { name: 'products' }, true)[0];
				$scope.purchaseProductModule = $filter('filter')($rootScope.modules, { name: 'purchase_order_products' }, true)[0];
				$scope.productCurrencyField = $filter('filter')($scope.productModule.fields, { name: 'currency' }, true)[0];
			}
			else if ($scope.type === 'sales_invoices') {
				$scope.productModule = $filter('filter')($rootScope.modules, { name: 'products' }, true)[0];
				$scope.salesInvoiceProductModule = $filter('filter')($rootScope.modules, { name: 'sales_invoices_products' }, true)[0];
				$scope.productCurrencyField = $filter('filter')($scope.productModule.fields, { name: 'currency' }, true)[0];
			}
			else if ($scope.type === 'purchase_invoices') {
				$scope.productModule = $filter('filter')($rootScope.modules, { name: 'products' }, true)[0];
				$scope.purchaseInvoiceProductModule = $filter('filter')($rootScope.modules, { name: 'purchase_invoices_products' }, true)[0];
				$scope.productCurrencyField = $filter('filter')($scope.productModule.fields, { name: 'currency' }, true)[0];
			}

			$scope.picklistFilter = function (param) {
				return function (item) {
					$scope.componentFilter = {};
					$scope.componentFilter.item = item;
					$scope.componentFilter.result = true;
					components.run('PicklistFilter', 'Script', $scope);
					return !item.hidden && !item.inactive && $scope.componentFilter.result;
				};
			};

			var isFreeze = function (record) {
				var type = false;

				if (record.process_status === 1)
					type = true;

				if ($scope.module.dependencies.length > 0) {
					var freezeDependencies = $filter('filter')($scope.module.dependencies, { dependency_type: 'freeze' }, true);
					angular.forEach(freezeDependencies, function (dependency) {
						if (!type) {
							var freezeFields = $filter('filter')($scope.module.fields, { name: dependency.parent_field }, true);
							angular.forEach(freezeFields, function (field) {
								if (!type)
									angular.forEach(dependency.values_array, function (value) {
										if (record[field.name] && (value == record[field.name] || value == record[field.name].id))
											type = true;
									});
							});
						}
					});
				}
				return type;
			};

			var setCurrencyCurrentAccounts = function () {
				if ($scope.module.name === 'current_accounts' && $scope.currencyField) {
					var currencyValue;

					if ($scope.record['customer'])
						currencyValue = $scope.record['customer']['currency'];

					if ($scope.record['supplier'])
						currencyValue = $scope.record['supplier']['currency'];

					if (currencyValue) {
						if (!angular.isObject(currencyValue))
							$scope.record['currency'] = $filter('filter')($scope.picklistsModule[$scope.currencyField.picklist_id], { labelStr: currencyValue }, true)[0];
						else
							$scope.record['currency'] = currencyValue;
					}
				}
            };

            var checkBranchSettingsAvailable = function () {
                if ($rootScope.branchAvailable) {
                    $scope.branchManager = $filter('filter')($rootScope.users, { RoleId: parseInt($scope.record['branch']) }, true)[0];
                    $scope.record.Authorities = [];
                    if ($scope.branchManager) {
                        $http.get(config.apiUrl + 'user_custom_shares/get_all_by_shared_user_id/' + $scope.branchManager.Id)
                            .then(function (response) {
                                $scope.authorities = response.data;
                                angular.forEach($scope.authorities, function (authority) {
                                    var user = $filter('filter')($rootScope.users, { Id: authority['user_id'] }, true)[0];
                                    $scope.record.Authorities.push({ id: user.Id, full_name: user.FullName, email: user.Email });
                                });
                                $scope.showBranchSettings = true;
                            });
                    }

                    //$scope.authorities
                }
            };

            var checkEditPermission = function () {
                if (!$scope.hasProcessEditPermission) {
                    if ($scope.id && (($scope.record.freeze && !$rootScope.user.profile.HasAdminRights) || ($scope.record.process_id && $scope.record.process_status != 3 && !$rootScope.user.profile.HasAdminRights))) {
                        ngToast.create({ content: $filter('translate')('Common.Forbidden'), className: 'warning' });
                        $state.go('app.crm.dashboard');
                    }
                }

                checkBranchSettingsAvailable();
            };

			ModuleService.getPicklists($scope.module)
				.then(function (picklists) {
					$scope.picklistsModule = picklists;
					$scope.currencyField = $filter('filter')($scope.module.fields, { name: 'currency' }, true)[0];
					var ownerField = $filter('filter')($scope.module.fields, { name: 'owner' }, true)[0];

					var setFieldDependencies = function () {
						angular.forEach($scope.module.fields, function (field) {
							ModuleService.setDependency(field, $scope.module, $scope.record, $scope.picklistsModule, $scope);
						});
					};

					if ($scope.module && $scope.module.name === 'izinler') {
						var toField = $filter('filter')($scope.module.fields, { name: 'to_entry_type' }, true)[0];
						var fromField = $filter('filter')($scope.module.fields, { name: 'from_entry_type' }, true)[0];
						$scope.customLeaveFields['to_entry_type'] = toField;
						$scope.customLeaveFields['from_entry_type'] = fromField;
						if (!$scope.record['to_entry_type'] && !$scope.record['from_entry_type'] && toField) {
							$scope.record['to_entry_type'] = picklists[toField.picklist_id][0];
							$scope.record['from_entry_type'] = picklists[toField.picklist_id][0];
						}
					}

                    if (!$scope.id) {
                        //Çalışanlar Create sayfası açıldığında Kullanıcı Ekle checkboxında lisans kontrolü yapmak için Lisans bilgileri çekiliyor.
                        //Düzenle sayfasına gidildiğinde gizlenen fieldların gözükmesi sağlanıyor.
                        if ($scope.module.name === 'calisanlar') {
                            if (!$rootScope.branchAvailable) {
                                var userCreateField = $filter('filter')($scope.module.fields, { name: 'kullanici_olustur' }, true)[0];
                                var userProfileField = $filter('filter')($scope.module.fields, { name: 'kullanici_profili' }, true)[0];
                                var userRoleField = $filter('filter')($scope.module.fields, { name: 'kullanici_rolu' }, true)[0];
                                if (userCreateField)
                                    userCreateField.hidden = false;
                                if (userProfileField)
                                    userProfileField.hidden = false;
                                if (userRoleField)
                                    userRoleField.hidden = false;
                            }

                            ModuleService.getUserLicenseStatus()
                                .then(function (response) {
                                    $scope.userLicenseControl = response.data;
                                    $scope.userLicenseKalan = $scope.userLicenseControl.Total - $scope.userLicenseControl.Used;
                                });
                        }

						$scope.loading = false;
						$scope.record.owner = $scope.currentUser;

						if ($scope.subtype) {
							if ($scope.type == 'activities') {
								$scope.record['activity_type'] = $filter('filter')(activityTypes, { system_code: $scope.subtype }, true)[0];
								$scope.subtypeNameLang = $filter('translate')('Module.New', { title: $scope.record['activity_type'].label[$rootScope.language] });
							} else if ($scope.type == 'current_accounts') {
								$scope.record['transaction_type'] = $filter('filter')(transactionTypes, { system_code: $scope.subtype }, true)[0];
								$scope.subtypeNameLang = $filter('translate')('Module.New', { title: $scope.record['transaction_type'].label[$rootScope.language] });
							}
						}

						if (($scope.module.name === 'accounts' || $scope.module.name === 'current_accounts' || $scope.module.name === 'products' || $scope.module.name === 'quotes' || $scope.module.name === 'sales_orders' || $scope.module.name === 'purchase_orders' || $scope.module.name === 'sales_invoices' || $scope.module.name === 'purchase_invoices') && $scope.currencyField) {
							if (!$scope.currencyField.validation)
								$scope.currencyField.validation = {};

							$scope.currencyField.validation.readonly = false;
						}

						if ($scope.parentId) {
							var moduleParent = $filter('filter')($rootScope.modules, { name: $scope.parentModule }, true)[0];

							ModuleService.getRecord($scope.parentModule, $scope.parentId)
								.then(function onSuccess(parent) {
									var moduleParentPrimaryField = $filter('filter')(moduleParent.fields, { primary: true, deleted: false }, true)[0];
									var lookupRecord = {};
									lookupRecord.id = parent.data.id;
									lookupRecord.primary_value = parent.data[moduleParentPrimaryField.name];

									if (parent.data['currency']) {
										lookupRecord['currency'] = parent.data['currency'];
										setCurrencyCurrentAccounts();
									}

									if ($scope.parentModule === 'calisanlar') {
										lookupRecord['e_posta'] = parent.data['e_posta'];
									}

									if (($scope.type === 'activities' || $scope.type === 'mails') && $scope.relatedToField) {
										$scope.record['related_to'] = lookupRecord;
										$scope.record['related_module'] = $filter('filter')(picklists['900000'], { value: $scope.parentType }, true)[0];
									}
									else {
										if ($scope.parentModule === 'accounts' && $scope.type === 'current_accounts') {
											$scope.record['customer'] = lookupRecord;
										}
										else if ($scope.parentModule === 'suppliers' && $scope.type === 'current_accounts') {
											$scope.record['supplier'] = lookupRecord;
										}
										else {
											$scope.record[$scope.parentType] = lookupRecord;
										}

										var relatedDependency = $filter('filter')($scope.module.dependencies, { dependent_field: $scope.parentType }, true)[0];

										if (relatedDependency && relatedDependency.deleted != true) {
											var dependencyField = $filter('filter')($scope.module.fields, { name: relatedDependency.field }, true)[0];
											$scope.record[relatedDependency.field] = $filter('filter')($scope.picklistsModule[dependencyField.picklist_id], { id: relatedDependency.values[0] }, true)[0];

											var dependentField = $filter('filter')($scope.module.fields, { name: relatedDependency.dependent_field }, true)[0];
											dependentField.hidden = false;
										}

										setFieldDependencies();
									}
								});
						}
						else {
							setFieldDependencies();
						}

						ModuleService.setDefaultValues($scope.module, $scope.record, picklists);
						ModuleService.setDisplayDependency($scope.module, $scope.record);

						if ($scope.paramField) {
							$scope.record[$scope.paramField] = $scope.paramValue;
							$rootScope.hideSipPhone();
						}

						if ($scope.type === 'quotes') {
							var quoteProduct = {};
							quoteProduct.id = 0;
							quoteProduct.order = 1;
							quoteProduct.discount_type = 'percent';
							$scope.quoteProducts = [];
							$scope.quoteProducts.push(quoteProduct);
						}
						if ($scope.type === 'sales_invoices') {
							var salesInvoiceProduct = {};
							salesInvoiceProduct.id = 0;
							salesInvoiceProduct.order = 1;
							salesInvoiceProduct.discount_type = 'percent';
							$scope.salesInvoiceProducts = [];
							$scope.salesInvoiceProducts.push(salesInvoiceProduct);
						}
						if ($scope.type === 'purchase_invoices') {
							var purchaseInvoiceProduct = {};
							purchaseInvoiceProduct.id = 0;
							purchaseInvoiceProduct.order = 1;
							purchaseInvoiceProduct.discount_type = 'percent';
							$scope.purchaseInvoiceProducts = [];
							$scope.purchaseInvoiceProducts.push(purchaseInvoiceProduct);
						}

						if ($scope.type === 'sales_orders') {
							var orderProduct = {};
							orderProduct.id = 0;
							orderProduct.order = 1;
							orderProduct.discount_type = 'percent';
							$scope.orderProducts = [];
							$scope.orderProducts.push(orderProduct);
						}

						if ($scope.type === 'purchase_orders') {
							var purchaseProduct = {};
							purchaseProduct.id = 0;
							purchaseProduct.order = 1;
							purchaseProduct.discount_type = 'percent';
							$scope.purchaseProducts = [];
							$scope.purchaseProducts.push(purchaseProduct);
						}

						$scope.multiCurrency();
						components.run('FieldChange', 'Script', $scope, $scope.record, ownerField);
						return;
					}

					ModuleService.getRecord($scope.module.name, $scope.id)
                        .then(function onSuccess(recordData) {
                            if (Object.keys(recordData.data).length === 0) {
                                ngToast.create({ content: $filter('translate')('Common.Forbidden'), className: 'warning' });
                                $state.go('app.crm.dashboard');
                                return;
                            }

							var record = ModuleService.processRecordSingle(recordData.data, $scope.module, $scope.picklistsModule);

                            //Kullanıcı oluşturulduysa edit sayfasında kullanıcı oluşturma alanları gizleniyor.
                            if ($scope.module.name === 'calisanlar' && record['kullanici_olustur']) {
                                var userCreateField = $filter('filter')($scope.module.fields, { name: 'kullanici_olustur' }, true)[0];
                                var userProfileField = $filter('filter')($scope.module.fields, { name: 'kullanici_profili' }, true)[0];
                                var userRoleField = $filter('filter')($scope.module.fields, { name: 'kullanici_rolu' }, true)[0];
                                if (userCreateField)
                                    userCreateField.hidden = true;
                                if (userProfileField)
                                    userProfileField.hidden = true;
                                if (userRoleField)
                                    userRoleField.hidden = true;
                            }
                            else {
                                //Düzenle butonundan Edit Sayfasına gidildiğinde Lisans kontrolü için Lisans bilgileri çekiliyor.
                                if ($scope.module.name === 'calisanlar') {
                                    ModuleService.getUserLicenseStatus()
                                        .then(function (response) {
                                            $scope.userLicenseControl = response.data;
                                            $scope.userLicenseKalan = $scope.userLicenseControl.Total - $scope.userLicenseControl.Used;
                                        });
                                }
                            }

                            if (!$scope.hasPermission($scope.type, $scope.operations.modify, recordData.data) || (isFreeze(record) && !$scope.hasAdminRights)) {
								ngToast.create({
									content: $filter('translate')('Common.Forbidden'),
									className: 'warning'
								});
								$state.go('app.dashboard');
								return;
							}

							$scope.multiCurrency();
							if (($scope.module.name === 'accounts' || $scope.module.name === 'current_accounts' || $scope.module.name === 'products' || $scope.module.name === 'quotes' || $scope.module.name === 'sales_orders' || $scope.module.name === 'purchase_orders') && $scope.currencyField) {

								var currencyValidationSet = function (status) {
									if ($scope.currencyField.validation)
										$scope.currencyField.validation.readonly = status;
									else
										$scope.currencyField["validation"] = [{ readonly: status }];
								};

								if (record['currency']) {
									currencyValidationSet(true);
								}
								else {

									if ($scope.currencyField.data_type === 'picklist') {
										record['currency'] = $filter('filter')($scope.picklistsModule[$scope.currencyField.picklist_id], { value: $rootScope.currencySymbol })[0];
									}

									currencyValidationSet(false);
								}

								if ($scope.clone) {
									currencyValidationSet(false);
								}
							}

							$scope.title = $scope.primaryField.valueFormatted;
							$scope.recordState = angular.copy(record);
							ModuleService.setDisplayDependency($scope.module, record);

							setFieldDependencies();

							//encrypted fields
							for (var f = 0; f < $scope.module.fields.length; f++) {
								var field = $scope.module.fields[f];
								var showEncryptedInput = false;
								if (field.encrypted && field.encryption_authorized_users_list.length > 0 && record[field.name]) {
									for (var p = 0; p < field.encryption_authorized_users_list.length; p++) {
										var encryrptionPermission = field.encryption_authorized_users_list[p];
										if ($rootScope.user.id == parseInt(encryrptionPermission))
											showEncryptedInput = true;
									}
								}

								if (field.encrypted && !showEncryptedInput)
									field.show_lock = true;
								else
									field.show_lock = false;
							}

							if ($scope.relatedToField && record['related_to']) {
								var lookupModule = $filter('filter')($rootScope.modules, { id: record[$scope.relatedToField.lookup_relation].id - 900000 }, true)[0];
								var lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];

								ModuleService.getRecord(lookupModule.name, record['related_to'], true)
									.then(function onSuccess(relatedRecord) {
										relatedRecord = relatedRecord.data;
										relatedRecord.primary_value = relatedRecord[lookupModulePrimaryField.name];
										record['related_to'] = relatedRecord;

										$scope.record = record;
                                        $scope.recordState = angular.copy($scope.record);

                                        /*
                                        * Record edit denildiğinde sayfa ilk yüklendiğinde fieldValueChange in tetiklenmediği durumlar var.
                                        * (ex: Admin olmayan bir kullanıcı izinler modülünde ki bir record a edit dediğinde fieldValueChange metodu sayfa ilk açıldığında tetiklenmediği için görsel değişiklikler gerçekleşmiyor)
                                        * Eğer field ların içinde form da gözüken bir lookup alan var ise (show as dropdown olmayan) fieldValueChange otomatik olarak tetikleniyor. Ama böyle bir alan mevcut değil ise tetiklenmiyor.
                                        * Çözüm olarak fieldValueChange in içerisin de ki function ların çalışması için fake bir şekilde ilk field ı kullanarak fieldValueChange metodunu tetikliyoruz.
                                        * */
                                        if ($scope.module && $scope.module.fields && $scope.module.fields.length > 0) {
                                            $scope.fieldValueChange($scope.module.fields[0]);
                                        }

										$scope.loading = false;
									})
									.catch(function onError(response) {
										if (response.status === 404) {
											record['related_to'] = null;
											$scope.record = record;
											$scope.recordState = angular.copy($scope.record);
										}

										$scope.loading = false;
									});
							}
							else {
								$scope.record = record;
								if ($scope.clone) {
									$scope.record.owner = $scope.currentUser;
								}
								$scope.loading = false;
							}

							if ($scope.record.currency)
								$scope.currencySymbol = $scope.record.currency.value || $rootScope.currencySymbol;

							getProductRecords($scope.type);
							ModuleService.customActions($scope.module, $scope.record);

							components.run('FieldChange', 'Script', $scope, $scope.record, ownerField);
						})
						.catch(function onError() {
							$scope.loading = false;
						});
				});

			//MultiCurrency
			$scope.multiCurrency = function () {
				if ($scope.module.name === 'quotes' || $scope.module.name === 'sales_orders' || $scope.module.name === 'purchase_orders' || $scope.module.name === 'sales_invoices' || $scope.module.name === 'purchase_invoices') {
					if ($scope.currencyField) {
						$scope.showExchangeRates = true;


						ModuleService.getDailyRates()
							.then(function (response) {
								if (!response.data)
									return;

								if ($scope.id && ($scope.record.exchange_rate_try_usd != null || !angular.isDefined($scope.record.exchange_rate_try_usd)))
									return;

								var dailyRates = response.data;
								$scope.exchangeRatesDate = $filter('date')(dailyRates.date, 'dd MMMM yyyy') + ' 15:30';

								$scope.record.exchange_rate_try_usd = dailyRates.usd;
								$scope.record.exchange_rate_try_eur = dailyRates.eur;
								$scope.record.exchange_rate_usd_try = 1 / dailyRates.usd;
								$scope.record.exchange_rate_usd_eur = (1 / dailyRates.usd) * dailyRates.eur;
								$scope.record.exchange_rate_eur_try = 1 / dailyRates.eur;
								$scope.record.exchange_rate_eur_usd = (1 / dailyRates.eur) * dailyRates.usd;
							})
					}
				}
			};

			$scope.lookup = function (searchTerm) {
				if ($scope.currentLookupField.lookup_type === 'users' && !$scope.currentLookupField.lookupModulePrimaryField) {
					var userModulePrimaryField = {};
					userModulePrimaryField.data_type = 'text_single';
					userModulePrimaryField.name = 'full_name';
					$scope.currentLookupField.lookupModulePrimaryField = userModulePrimaryField;
                }

                if ($scope.currentLookupField.lookup_type === 'profiles' && !$scope.currentLookupField.lookupModulePrimaryField) {
                    var userModulePrimaryField = {};
                    userModulePrimaryField.data_type = 'text_single';
                    userModulePrimaryField.name = 'name';
                    $scope.currentLookupField.lookupModulePrimaryField = userModulePrimaryField;
                }

                if ($scope.currentLookupField.lookup_type === 'roles' && !$scope.currentLookupField.lookupModulePrimaryField) {
                    var userModulePrimaryField = {};
                    userModulePrimaryField.data_type = 'text_single';
                    userModulePrimaryField.name = 'label_' + $rootScope.user.tenantLanguage;
                    $scope.currentLookupField.lookupModulePrimaryField = userModulePrimaryField;
                }

				if ($scope.currentLookupField.lookup_type === 'relation') {
					if (!$scope.record.related_module) {
						$scope.$broadcast('angucomplete-alt:clearInput', $scope.currentLookupField.name);
						return $q.defer().promise;
					}

					var relationModule = $filter('filter')($rootScope.modules, { name: $scope.record.related_module.value }, true)[0];

					if (!relationModule) {
						$scope.$broadcast('angucomplete-alt:clearInput', $scope.currentLookupField.name);
						return $q.defer().promise;
					}

					$scope.currentLookupField.lookupModulePrimaryField = $filter('filter')(relationModule.fields, { primary: true }, true)[0];
				}

				if (($scope.currentLookupField.lookupModulePrimaryField.data_type === 'number' || $scope.currentLookupField.lookupModulePrimaryField.data_type === 'number_auto') && isNaN(parseFloat(searchTerm))) {
					$scope.$broadcast('angucomplete-alt:clearInput', $scope.currentLookupField.name);
					return $q.defer().promise;
				}

				if ($scope.module.name === 'quotes' || $scope.module.name === 'sales_orders' || $scope.module.name === 'purchase_orders' || $scope.module.name === 'sales_invoices' || $scope.module.name === 'purchase_invoices') {
					var discountField = null;

					if ($scope.currentLookupField.lookup_type === 'contacts') {
						$scope.contactModule = $filter('filter')($rootScope.modules, { name: 'contacts' }, true)[0];
						discountField = $filter('filter')($scope.contactModule.fields, { name: 'discount', deleted: '!true' }, true)[0];
					}
					else if ($scope.currentLookupField.lookup_type === 'accounts') {
						$scope.accountModule = $filter('filter')($rootScope.modules, { name: 'accounts' }, true)[0];
						discountField = $filter('filter')($scope.accountModule.fields, { name: 'discount', deleted: '!true' }, true)[0];
					}

					if (discountField)
						return ModuleService.lookup(searchTerm, $scope.currentLookupField, $scope.record, ['discount']);
					else
						return ModuleService.lookup(searchTerm, $scope.currentLookupField, $scope.record);
				}

				if ($scope.module.name === 'current_accounts' && $scope.currencyField) {
					var parentModuleName = '';

					if ($scope.currentLookupField.name === 'customer')
						parentModuleName = 'accounts';
					else if ($scope.currentLookupField.name === 'supplier')
						parentModuleName = 'suppliers';

					if (parentModuleName) {
						var parentModule = $filter('filter')($rootScope.modules, { name: parentModuleName }, true)[0];
						var parentCurrencyField = $filter('filter')(parentModule.fields, { name: 'currency' }, true)[0];

						if (parentCurrencyField) {
							return ModuleService.lookup(searchTerm, $scope.currentLookupField, $scope.record, ['currency']);
						}
						else {
							return ModuleService.lookup(searchTerm, $scope.currentLookupField, $scope.record);
						}
					}
				}

                $scope.customFilters = null;

                components.run('BeforeLookup', 'Script', $scope);

                if ($scope.currentLookupField.lookup_type === 'users')
                    return ModuleService.lookup(searchTerm, $scope.currentLookupField, $scope.record, ['email'], false, $scope.customFilters);
                else if ($scope.currentLookupField.lookup_type === 'profiles')
                    return ModuleService.lookup(searchTerm, $scope.currentLookupField, $scope.record, null, false, $scope.customFilters);
                else if ($scope.currentLookupField.lookup_type === 'roles')
                    return ModuleService.lookup(searchTerm, $scope.currentLookupField, $scope.record, null, false, $scope.customFilters);
                else if ($scope.currentLookupField.lookup_type === 'calisanlar')
                    return ModuleService.lookup(searchTerm, $scope.currentLookupField, $scope.record, ['e_posta'], false, $scope.customFilters);
                else
					return ModuleService.lookup(searchTerm, $scope.currentLookupField, $scope.record);
			};

			$scope.multiselect = function (searchTerm, field) {
				var picklistItems = [];

				angular.forEach($scope.picklistsModule[field.picklist_id], function (picklistItem) {
					if (picklistItem.inactive || picklistItem.hidden)
						return;

					if (picklistItem.labelStr.toLowerCase().indexOf(searchTerm.toLowerCase()) > -1 || picklistItem.labelStr.toUpperCase().indexOf(searchTerm.toUpperCase()) > -1
						|| picklistItem.labelStr.toLowerCaseTurkish().indexOf(searchTerm.toLowerCaseTurkish()) > -1 || picklistItem.labelStr.toUpperCaseTurkish().indexOf(searchTerm.toUpperCaseTurkish()) > -1)
						picklistItems.push(picklistItem);
				});

				return picklistItems;
			};

			$scope.tags = function (searchTerm, field) {
				return $http.get(config.apiUrl + "tag/get_tag/" + field.id).then(function (response) {
					var tags = response.data;
					return tags.filter(function (tag) {
						return tag.text.toLowerCase().indexOf(searchTerm.toLowerCase()) != -1;
					});
				});
			};

			$scope.setCurrentLookupField = function (field) {
				$scope.currentLookupField = field;
			};

			$scope.uploader = new FileUploader({
				url: config.apiUrl + 'Document/upload_large',
				headers: {
					'Authorization': 'Bearer ' + $localStorage.read('access_token'),
					"Content-Type": "application/json", "Accept": "application/json",
					'X-Tenant-Id': $cookies.get('tenant_id')
				}
			});

			$scope.entityIdFunc = function () {
				return $scope.recordId;
			};

            //Çalışan kaydı oluşurken otomatik kullanıcı oluşmasını sağlayan method.
            $scope.addUser = function (record) {
                $scope.openCreateUserModal = function () {
                    $scope.userCreateModal = $scope.userCreateModal || $modal({
                        scope: $scope,
                        templateUrl: 'view/app/module/createUserModal.html',
                        animation: '',
                        backdrop: 'static',
                        show: false,
                        tag: 'createModal',
                        keyboard: false
                    });

                    $scope.userCreateModal.$promise.then($scope.userCreateModal.show);
                };

                //Kullanıcı oluştur checkboxında lisans kontrolü.
                if ($scope.userLicenseKalan == 0 || $scope.userLicenseKalan < 0) {
                    if (!$rootScope.branchAvailable)
                        $scope.record['kullanici_olustur'] = false;

                    ngToast.create({ content: $filter('translate')('Setup.Users.LicenceRequired'), className: 'warning' });
                    $scope.submitting = false;
                    return;
                }

                //Sisteme kayıtlı bir user ile kullanıcı oluşturmak istenildiğinde yapılan user kontrolü.
                ModuleService.getUserEmailControl($scope.record.e_posta)
                    .then(function (response) {
                        var userEmail = response.data;
                        if (userEmail) {
                            ngToast.create({
                                content: $filter('translate')('Setup.Users.NewUserError'),
                                className: 'warning'
                            });

                            if (!$rootScope.branchAvailable)
                                $scope.record['kullanici_olustur'] = false;

                            $scope.submitting = false;
                            return;
                        }
                        else {
                            var profileId = $scope.record.kullanici_profili ? $scope.record.kullanici_profili.id : null;
                            var roleId = $scope.record.kullanici_rolu ? $scope.record.kullanici_rolu.id : null;

                            var createUser = function (roleId, profileId, record) {
                                var inviteModel = {};
                                inviteModel.email = $scope.record.e_posta;
                                inviteModel.firstName = $scope.record.ad;
                                inviteModel.lastName = $scope.record.soyad;
                                inviteModel.profile = profileId;
                                inviteModel.role = roleId;
                                inviteModel.fullName = inviteModel.firstName + " " + inviteModel.lastName;


                                if (!inviteModel || !inviteModel.email || !inviteModel.profile || !inviteModel.role || !inviteModel.firstName || !inviteModel.lastName)
                                    return;

                                $scope.userInviting = true;
                                inviteModel.profileId = inviteModel.profile;
                                inviteModel.roleId = inviteModel.role;
                                $scope.addedUser = angular.copy(inviteModel);

                                ModuleService.addUser(inviteModel)
                                    .then(function (response) {
                                        if (response.data) {
                                            //User Lisans ve şifre bilgilerinin setlenmesi.
                                            $scope.userCreatePassword = response.data;
                                            $scope.userCreateEmail = $scope.record.e_posta;
                                            $scope.userLicenseAvailable = $scope.userLicenseKalan - 1;
                                            $scope.userLicensesBought = $scope.userLicenseControl.Total;
                                            $scope.userAdded = true;

                                            var promises = [];

                                            promises.push(ModuleService.myAccount());
                                            promises.push(ModuleService.getAllUser());

                                            $q.all(promises).then(function (data) {
                                                var account = data[0].data,
                                                    users = data[1].data;

                                                $rootScope.user = account.user;
                                                $rootScope.workgroups = account.instances;
                                                var workgroupId = $localStorage.read('Workgroup');
                                                $rootScope.workgroup = account.instances[0];

                                                if (workgroupId) {
                                                    var workgroup = $filter('filter')(account.instances, { instanceID: workgroupId }, true)[0];

                                                    if (workgroup)
                                                        $rootScope.workgroup = workgroup;
                                                }
                                                var isDemo = account.user.isDemo || false;

                                                $rootScope.users = !isDemo ? users : $filter('filter')(users, function (value) {
                                                    return value.Id == account.user.ID;
                                                }, true);

                                            });

                                            $scope.submit(record);
                                            $scope.openCreateUserModal();
                                        }
                                    })
                                    .catch(function (response) {
                                        $scope.submitting = false;
                                        if (response.status === 409) {
                                            ngToast.create({
                                                content: $filter('translate')('Setup.Users.NewUserError'),
                                                className: 'warning'
                                            });
                                        }
                                    });
                            }

                            if ($rootScope.branchAvailable) {
                                ModuleService.getRecord('branchs', $scope.record.branch.id)
                                    .then(function (response) {
                                        roleId = response.data.branch;
                                        profileId = $scope.record.profile.id;
                                        createUser(roleId, profileId, record);
                                    }).catch(function (error) {
                                        $scope.submitting = false;
                                    });
                            }
                            else {
                                createUser(roleId, profileId, record);
                            }
                        }
                    });
            };

			$scope.submit = function (record) {
				function validate() {
					var isValid = true;

					if ($scope.module.name === 'current_accounts')
						var hasShownWarning = false;

					angular.forEach($scope.module.fields, function (field) {
						if (!record[field.name])
							return;

						if (field.data_type === 'lookup' && typeof record[field.name] != 'object') {
							$scope.moduleForm[field.name].$setValidity('object', false);
							isValid = false;
						}

						if (field.data_type == 'image' && $scope.image[field.name] && $scope.image[field.name].UniqueName && record[field.name] != null) {
							record[field.name] = $scope.image[field.name].UniqueName;
							$http.post(config.apiUrl + 'Document/image_create', {
								UniqueFileName: $scope.image[field.name].UniqueName,
								MimeType: $scope.image[field.name].Type,
								ChunkSize: 1,
								instanceId: $rootScope.workgroup.instanceID
							});
						}

						if ($scope.module.name === 'current_accounts') {
							if (record.odendi && (record.payment_method.system_code === 'cheque' || record.payment_method.system_code === 'bill')) {
								if (record['kasa'] && record['banka']) {
									if (!hasShownWarning) {
										ngToast.create({ content: $filter('translate')('Common.BankAndCaseChosen'), className: 'warning' });
										hasShownWarning = true;
									}

									delete record['kasa'];
									delete record['banka'];
									isValid = false;
								}
								else if (!record['kasa'] && !record['banka']) {
									if (!hasShownWarning) {
										ngToast.create({ content: $filter('translate')('Common.ChooseBankOrCase'), className: 'warning' });
										hasShownWarning = true;
									}

									delete record['kasa'];
									delete record['banka'];
									isValid = false;
								}
							}
						}


					});

					return isValid;
				}

               $scope.submitting = true;

                if (!$scope.moduleForm.$valid || !validate()) {
                    $scope.submitting = false;
                    return;
                }
					

				if (!$scope.id || $scope.clone) {
					$scope.executeCode = false;
					components.run('BeforeCreate', 'Script', $scope, record);

                    if ($scope.executeCode) {
                        $scope.submitting = false;
						return;
					}
				} else {
					$scope.executeCode = false;
					components.run('BeforeUpdate', 'Script', $scope, record);
                    if ($scope.executeCode) {
                        $scope.submitting = false;
						return;
					}
                }

                //Çalışan modülünde record kayıt edilirken record da kullanıcı oluştur seçiliyse AddUser methodu çalışacak.
                if (!$scope.id && $scope.module.name === 'calisanlar' && ($scope.record['kullanici_olustur'] || $rootScope.branchAvailable) && !$scope.userAdded) {
                    $scope.addUser(record);
                    return;
                }

                if ($rootScope.branchAvailable && $scope.branchManager) {
                    angular.forEach($scope.record.Authorities, function (authority) {
                        var checker = $filter('filter')($scope.authorities, { user_id: authority.id }, true)[0];

                        if (!checker) {
                            $http.post(config.apiUrl + 'user_custom_shares/create/', { shared_user_id: $scope.branchManager.Id, user_id: authority.id })
                        }
                    });

                    angular.forEach($scope.authorities, function (authority) {
                        var checker = $filter('filter')($scope.record.Authorities, { id: authority.user_id }, true)[0];

                        if (!checker) {
                            $http.delete(config.apiUrl + 'user_custom_shares/delete/' + authority.id)
                        }
                    });

                }

                delete record['Authorities'];

                if ($scope.module.name === 'izinler') {
                    var val = "";
                    /*
                     * skipValidation parametresi component içinde setlenerek validasyonların atlanması sağlanıyor.
                     * #2438 nolu task için geliştirildi.
                     * */
                    if (!$scope.skipValidation)
                        val = ModuleService.customValidations($scope.module, record);
                    else {
                        delete record['goreve_baslama_tarihi'];
                        delete record['calisan_data']
                        delete record['dogum_tarihi'];
                        delete record['izin_turu_data'];
                        delete record['alinan_izinler'];
                    }

                    if (val != "") {
                        ngToast.create({
                            //content: $filter('translate')('Module.SuccessMessage', { title: $scope.module['label_' + $rootScope.language + '_singular'] }),
                            content: val,
                            className: 'warning',
                            timeout: 8000
                        });
                        return;
                    }
                }

				if ($scope.clone) {
					angular.forEach($scope.module.fields, function (field) {
						if (field.data_type === 'number_auto') {
							delete record[field.name];
						}
					});
					$scope.recordState = null;
				}

				if (record.discount_percent && record.discount_amount == null)
					record.discount_amount = parseFloat(record.total - record.discounted_total);

				if (record.discount_amount && record.discount_percent == null)
					record.discount_percent = parseFloat((100 * (record.total - record.discounted_total)) / record.total);

				record = ModuleService.prepareRecord(record, $scope.module, $scope.recordState);

				var recordCopy = angular.copy($scope.record);

				//gets activity_type_id for saveAndNew
				$scope.activity_type_id = $filter('filter')(activityTypes, { id: record.activity_type }, true)[0];

				if (record.activity_type_system || record.activity_type_system === null)
					delete record.activity_type_system;

				if (record.transaction_type_system || record.transaction_type_system === null)
					delete record.transaction_type_system;

				if (!$scope.id) {
					ModuleService.insertRecord($scope.module.name, record)
						.then(function onSuccess(response) {
							var moduleProcesses = $filter('filter')($rootScope.approvalProcesses, { module_id: $scope.module.id }, true);
							if (moduleProcesses) {
								var isProcessInsert = false;
								for (var i = 0; i < moduleProcesses.length; i++) {
									if ((moduleProcesses[i].user_id === 0 || moduleProcesses[i].user_id === $scope.currentUser.id) && moduleProcesses[i].operations.indexOf('insert') > -1)
										isProcessInsert = true;
								}
								if (isProcessInsert) {
									setTimeout(function () {
										result(response.data);
									}, 2000)
								}
								else
									result(response.data);
							}
							else
								result(response.data);

							components.run('AfterCreate', 'Script', $scope, record);
						})
						.catch(function onError(data) {
							error(data.data, data.status);
						})
						.finally(function () {
							var moduleProcesses = $filter('filter')($rootScope.approvalProcesses, { module_id: $scope.module.id }, true);
							if (moduleProcesses) {
								var isProcessInsert = false;
								for (var i = 0; i < moduleProcesses.length; i++) {
									if ((moduleProcesses[i].user_id === 0 || moduleProcesses[i].user_id === $scope.currentUser.id) && moduleProcesses[i].operations.indexOf('insert') > -1)
										isProcessInsert = true;
								}
								if (isProcessInsert) {
									setTimeout(function () {
										$scope.submitting = false;
									}, 2000)
								}
								else
									$scope.submitting = false;
							}
							else
								$scope.submitting = false;
						});
				}
				else {
					//encrypted field
					for (var f = 0; f < $scope.module.fields.length; f++) {
						var field = $scope.module.fields[f];
						var showEncryptedInput = false;
						if (field.encrypted && field.encryption_authorized_users_list.length > 0 && record[field.name]) {
							for (var p = 0; p < field.encryption_authorized_users_list.length; p++) {
								var encryrptionPermission = field.encryption_authorized_users_list[p];
								if ($rootScope.user.id == parseInt(encryrptionPermission))
									showEncryptedInput = true;
							}
						}

						if (field.encrypted && !showEncryptedInput)
							delete record[field.name];
					}

					if ($scope.clone) {
						if ($scope.revise) {
							record.master_id = record.id;
							var quoteStageField = $filter('filter')($scope.module.fields, { name: 'quote_stage' }, true)[0];
							record.quote_stage = $filter('filter')($scope.picklistsModule[quoteStageField.picklist_id], { value: 'delivered' }, true)[0].id;
						}

						delete record.id;

						//removes process approval fields
						delete record.process_id;
						delete record.process_status;
						delete record.process_status_order;
						delete record['process_request_updated_at'];
						delete record['process_request_updated_by'];
						delete record.operation_type;

						if (record.auto_id) record.auto_id = "";

						ModuleService.insertRecord($scope.module.name, record)
							.then(function onSuccess(response) {
								if (!record.master_id) {
									$scope.submitting = false;
									result(response.data);
								}
								else if ($scope.module.name === 'quotes') {
									//After record is revised, update the master record's stage
									ModuleService.getRecord($scope.module.name, record.master_id)
										.then(function onSuccess(recordDataMaster) {
											var masterRecord = ModuleService.processRecordSingle(recordDataMaster.data, $scope.module, $scope.picklistsModule);
											var masterRecordState = angular.copy(masterRecord);
											var quoteStageField = $filter('filter')($scope.module.fields, { name: 'quote_stage' }, true)[0];
											masterRecord.quote_stage = $filter('filter')($scope.picklistsModule[quoteStageField.picklist_id], { value: 'revised' }, true)[0];
											masterRecord = ModuleService.prepareRecord(masterRecord, $scope.module, masterRecordState);

											ModuleService.updateRecord($scope.module.name, masterRecord)
												.then(function () {
													$scope.submitting = false;
													result(response.data);
												});
										});
								}
								components.run('AfterCreate', 'Script', $scope, record);
							})
							.catch(function onError() {
								error(data, status);
								$scope.submitting = false;
							});
					}
					else {

						ModuleService.updateRecord($scope.module.name, record)
							.then(function onSuccess(response) {
								var moduleProcesses = $filter('filter')($rootScope.approvalProcesses, { module_id: $scope.module.id }, true);
								if (moduleProcesses) {
									var isProcessUpdate = false;
									for (var i = 0; i < moduleProcesses.length; i++) {
										if ((moduleProcesses[i].user_id === 0 || moduleProcesses[i].user_id === $scope.currentUser.id) && moduleProcesses[i].operations.indexOf('update') > -1)
											isProcessUpdate = true;
									}
									if (isProcessUpdate) {
										setTimeout(function () {
											result(response.data);
										}, 2000)
									}
									else
										result(response.data);
								}
								else
									result(response.data);

								components.run('AfterUpdate', 'Script', $scope, record);
							})
							.catch(function onError(data) {
								error(data.data, data.status);
							})
							.finally(function () {
								var moduleProcesses = $filter('filter')($rootScope.approvalProcesses, { module_id: $scope.module.id }, true);
								if (moduleProcesses) {
									var isProcessUpdate = false;
									for (var i = 0; i < moduleProcesses.length; i++) {
										if ((moduleProcesses[i].user_id === 0 || moduleProcesses[i].user_id === $scope.currentUser.id) && moduleProcesses[i].operations.indexOf('update') > -1)
											isProcessUpdate = true;
									}
									if (isProcessUpdate) {
										setTimeout(function () {
											$scope.submitting = false;
										}, 2000)
									}
									else
										$scope.submitting = false;
								}
								else
									$scope.submitting = false;
							});
					}
				}

                function result(response) {
                    $scope.addedUser = false;
					$scope.recordId = response.id;

					if ($scope.uploader.queue.length > 0) {
						$scope.uploader.onCompleteAll = function () {
							success();
						};
						$scope.uploader.uploadAll();
					}

					//Need to iterate document upload names - unique object from fileuploader..
					var documentFields = $filter('filter')($scope.module.fields, { data_type: 'document', deleted: false }, true);

					if (documentFields) {
						angular.forEach(documentFields, function (field) {

							if ($scope.uploaderBasic[field.name]) {

								if ($scope.uploaderBasic[field.name].queue.length > 0) {

									//set record id for insert, so on update also this way better.
									angular.forEach($scope.uploaderBasic[field.name].queue, function (item) {

										for (var i = 0; i < item.formData.length; i++) {
											if (item.formData[i].hasOwnProperty("recordid")) {
												item.formData[i].recordid = $scope.recordId;
											}
										}

									});

									$scope.uploaderBasic[field.name].onCompleteItem = function (fileItem, response, status, headers) {
										$scope.uploaderBasic[field.name].clearQueue();
									};

									$scope.uploaderBasic[field.name].uploadItem(0);

								}
							}

						});
					}

					if ($scope.type === 'quotes') {
						var quoteProducts = [];
						var no = 1;
						var quoteProductsOrders = $filter('orderBy')($scope.quoteProducts, 'order');
						angular.forEach(quoteProductsOrders, function (quoteProduct) {
							if (quoteProduct.deleted)
								return;

							var quote = {};
							quote.id = $scope.recordId;
							quote.primary_value = $scope.record[$scope.primaryField.name];
							quoteProduct.quote = quote;
							delete quoteProduct.vat;
							delete quoteProduct.currencyConvertList;
							delete quoteProduct.defaultCurrency;
							if ($scope.clone) {
								delete (quoteProduct.id);
								delete (quoteProduct._rev);
							}

							if (!quoteProduct.separator && quoteProduct.no) {
								quoteProduct.no = no++;
							}
							//Discount percent applied also calculate discount amount.
							if (quoteProduct.discount_percent && quoteProduct.discount_amount == null)
								quoteProduct.discount_amount = parseFloat((quoteProduct.unit_price * quoteProduct.quantity) - quoteProduct.amount);

							//Discount amount applied also calculate discount percent.
							if (quoteProduct.discount_amount && quoteProduct.discount_percent == null)
								quoteProduct.discount_percent = parseFloat((100 * ((quoteProduct.unit_price * quoteProduct.quantity) - quoteProduct.amount)) / quoteProduct.unit_price);

							quoteProducts.push(quoteProduct);
						});

						var quoteProductRecordsBulk = ModuleService.prepareRecordBulk(quoteProducts, $scope.quoteProductModule);

						var insertRecords = function () {
							ModuleService.insertRecordBulk($scope.quoteProductModule.name, quoteProductRecordsBulk)
								.then(function onSuccess() {
									success();
								});
						};

						if (!$scope.id || $scope.revise) {
							if (quoteProducts.length) {
								insertRecords();
							}
							else {
								success();
							}
						}
						else {
							var ids = [];

							angular.forEach($scope.quoteProducts, function (quoteProduct) {
								if (quoteProduct.id)
									ids.push(quoteProduct.id);
							});

							if (ids.length) {
								ModuleService.deleteRecordBulk($scope.quoteProductModule.name, ids)
									.then(function onSuccess() {
										if (quoteProducts.length) {
											insertRecords();
										}
										else {
											success();
										}
									});
							}
							else {
								if ($scope.quoteProducts.length) {
									insertRecords();
								}
								else {
									success();
								}
							}
						}
					}
					if ($scope.type === 'sales_invoices') {
						var salesInvoiceProducts = [];
						var no = 1;
						var salesInvoiceProductsOrders = $filter('orderBy')($scope.salesInvoiceProducts, 'order');
						angular.forEach(salesInvoiceProductsOrders, function (salesInvoiceProduct) {
							if (salesInvoiceProduct.deleted)
								return;

							var salesInvoice = {};
							salesInvoice.id = $scope.recordId;
							salesInvoice.primary_value = $scope.record[$scope.primaryField.name];
							salesInvoiceProduct.sales_invoice = salesInvoice;
							delete salesInvoiceProduct.vat;
							delete salesInvoiceProduct.currencyConvertList;
							delete salesInvoiceProduct.defaultCurrency;
							if ($scope.clone) {
								delete (salesInvoiceProduct.id);
								delete (salesInvoiceProduct._rev);
							}

							if (!salesInvoiceProduct.separator && salesInvoiceProduct.no) {
								salesInvoiceProduct.no = no++;
							}
							//Discount percent applied also calculate discount amount.
							if (salesInvoiceProduct.discount_percent && salesInvoiceProduct.discount_amount == null)
								salesInvoiceProduct.discount_amount = parseFloat((salesInvoiceProduct.unit_price * salesInvoiceProduct.quantity) - salesInvoiceProduct.amount);

							//Discount amount applied also calculate discount percent.
							if (salesInvoiceProduct.discount_amount && salesInvoiceProduct.discount_percent == null)
								salesInvoiceProduct.discount_percent = parseFloat((100 * ((salesInvoiceProduct.unit_price * salesInvoiceProduct.quantity) - salesInvoiceProduct.amount)) / salesInvoiceProduct.unit_price);

							salesInvoiceProducts.push(salesInvoiceProduct);
						});

						var salesInvoiceProductRecordsBulk = ModuleService.prepareRecordBulk(salesInvoiceProducts, $scope.salesInvoiceProductModule);

						var insertRecords = function () {
							ModuleService.insertRecordBulk($scope.salesInvoiceProductModule.name, salesInvoiceProductRecordsBulk)
								.then(function onSuccess() {
									success();
								});
						};

						if (!$scope.id || $scope.revise) {
							if (salesInvoiceProducts.length) {
								insertRecords();
							}
							else {
								success();
							}
						}
						else {
							var ids = [];

							angular.forEach($scope.salesInvoiceProducts, function (salesInvoiceProduct) {
								if (salesInvoiceProduct.id)
									ids.push(salesInvoiceProduct.id);
							});

							if (ids.length) {
								ModuleService.deleteRecordBulk($scope.salesInvoiceProductModule.name, ids)
									.then(function onSuccess() {
										if (salesInvoiceProducts.length) {
											insertRecords();
										}
										else {
											success();
										}
									});
							}
							else {
								if ($scope.salesInvoiceProducts.length) {
									insertRecords();
								}
								else {
									success();
								}
							}
						}
					}
					if ($scope.type === 'purchase_invoices') {
						var purchaseInvoiceProducts = [];
						var no = 1;
						var purchaseInvoiceProductsOrders = $filter('orderBy')($scope.purchaseInvoiceProducts, 'order');
						angular.forEach(purchaseInvoiceProductsOrders, function (purchaseInvoiceProduct) {
							if (purchaseInvoiceProduct.deleted)
								return;

							var purchaseInvoice = {};
							purchaseInvoice.id = $scope.recordId;
							purchaseInvoice.primary_value = $scope.record[$scope.primaryField.name];
							purchaseInvoiceProduct.purchase_invoice = purchaseInvoice;
							delete purchaseInvoiceProduct.vat;
							delete purchaseInvoiceProduct.currencyConvertList;
							delete purchaseInvoiceProduct.defaultCurrency;
							if ($scope.clone) {
								delete (purchaseInvoiceProduct.id);
								delete (purchaseInvoiceProduct._rev);
							}

							if (!purchaseInvoiceProduct.separator && purchaseInvoiceProduct.no) {
								purchaseInvoiceProduct.no = no++;
							}
							//Discount percent applied also calculate discount amount.
							if (purchaseInvoiceProduct.discount_percent && purchaseInvoiceProduct.discount_amount == null)
								purchaseInvoiceProduct.discount_amount = parseFloat((purchaseInvoiceProduct.unit_price * purchaseInvoiceProduct.quantity) - purchaseInvoiceProduct.amount);

							//Discount amount applied also calculate discount percent.
							if (purchaseInvoiceProduct.discount_amount && purchaseInvoiceProduct.discount_percent == null)
								purchaseInvoiceProduct.discount_percent = parseFloat((100 * ((purchaseInvoiceProduct.unit_price * purchaseInvoiceProduct.quantity) - purchaseInvoiceProduct.amount)) / purchaseInvoiceProduct.unit_price);

							purchaseInvoiceProducts.push(purchaseInvoiceProduct);
						});

						var purchaseInvoiceProductRecordsBulk = ModuleService.prepareRecordBulk(purchaseInvoiceProducts, $scope.purchaseInvoiceProductModule);

						var insertRecords = function () {
							ModuleService.insertRecordBulk($scope.purchaseInvoiceProductModule.name, purchaseInvoiceProductRecordsBulk)
								.then(function onSuccess() {
									success();
								});
						};

						if (!$scope.id || $scope.revise) {
							if (purchaseInvoiceProducts.length) {
								insertRecords();
							}
							else {
								success();
							}
						}
						else {
							var ids = [];

							angular.forEach($scope.purchaseInvoiceProducts, function (purchaseInvoiceProduct) {
								if (purchaseInvoiceProduct.id)
									ids.push(purchaseInvoiceProduct.id);
							});

							if (ids.length) {
								ModuleService.deleteRecordBulk($scope.purchaseInvoiceProductModule.name, ids)
									.then(function onSuccess() {
										if (purchaseInvoiceProducts.length) {
											insertRecords();
										}
										else {
											success();
										}
									});
							}
							else {
								if ($scope.purchaseInvoiceProducts.length) {
									insertRecords();
								}
								else {
									success();
								}
							}
						}
					}
					else if ($scope.type === 'sales_orders') {
						var orderProducts = [];
                        var no = 1;
                        var orderProductsOrder = $filter('orderBy')($scope.orderProducts, 'order');
                        angular.forEach(orderProductsOrder, function (orderProduct) {
                            if (orderProduct.deleted)
                                return;

							var sales_order = {};
							sales_order.id = $scope.recordId;
							sales_order.primary_value = $scope.record[$scope.primaryField.name];
							orderProduct.sales_order = sales_order;
							delete orderProduct.vat;
							delete orderProduct.currencyConvertList;
							delete orderProduct.defaultCurrency;
							if ($scope.clone) {
								delete (orderProduct.id);
								delete (orderProduct._rev);
                            }

                            if (!orderProduct.separator && orderProduct.no) {
                                orderProduct.no = no++;
                            }
							//Discount percent applied also calculate discount amount.
							if (orderProduct.discount_percent && orderProduct.discount_amount == null)
								orderProduct.discount_amount = parseFloat((orderProduct.unit_price * orderProduct.quantity) - orderProduct.amount);

							//Discount amount applied also calculate discount percent.
							if (orderProduct.discount_amount && orderProduct.discount_percent == null)
								orderProduct.discount_percent = parseFloat((100 * ((orderProduct.unit_price * orderProduct.quantity) - orderProduct.amount)) / orderProduct.unit_price);

							orderProducts.push(orderProduct);
						});

						var orderProductRecordsBulk = ModuleService.prepareRecordBulk(orderProducts, $scope.orderProductModule);

						var insertRecords = function () {
							ModuleService.insertRecordBulk($scope.orderProductModule.name, orderProductRecordsBulk)
								.then(function onSuccess() {
									success();
								});
						};

						if (!$scope.id || $scope.revise) {
							if (orderProducts.length) {
								insertRecords();
							}
							else {
								success();
							}
						}
						else {
							var ids = [];

							angular.forEach($scope.orderProducts, function (orderProduct) {
								if (orderProduct.id)
									ids.push(orderProduct.id);
							});

							if (ids.length) {
								ModuleService.deleteRecordBulk($scope.orderProductModule.name, ids)
									.then(function () {
										if (orderProducts.length) {
											insertRecords();
										}
										else {
											success();
										}
									});
							}
							else {
								if ($scope.orderProducts.length) {
									insertRecords();
								}
								else {
									success();
								}
							}
						}
					}
					else if ($scope.type === 'purchase_orders') {
						var purchaseProducts = [];
                        var no = 1;
                        var purchaseProductsOrders = $filter('orderBy')($scope.purchaseProducts, 'order');
                        angular.forEach(purchaseProductsOrders, function (purchaseProduct) {
                            if (purchaseProduct.deleted)
								return;

							var purchase_order = {};
							purchase_order.id = $scope.recordId;
							purchase_order.primary_value = $scope.record[$scope.primaryField.name];
							purchaseProduct.purchase_order = purchase_order;
							delete purchaseProduct.vat;
							delete purchaseProduct.currencyConvertList;
							delete purchaseProduct.defaultCurrency;
							if ($scope.clone) {
								delete (purchaseProduct.id);
								delete (purchaseProduct._rev);
                            }

                            if (!purchaseProduct.separator && purchaseProduct.no) {
                                purchaseProduct.no = no++;
                            }

							//Discount percent applied also calculate discount amount.
							if (purchaseProduct.discount_percent && purchaseProduct.discount_amount == null)
								purchaseProduct.discount_amount = parseFloat((purchaseProduct.unit_price * purchaseProduct.quantity) - purchaseProduct.amount);

							//Discount amount applied also calculate discount percent.
							if (purchaseProduct.discount_amount && purchaseProduct.discount_percent == null)
								purchaseProduct.discount_percent = parseFloat((100 * ((purchaseProduct.unit_price * purchaseProduct.quantity) - purchaseProduct.amount)) / purchaseProduct.unit_price);

							purchaseProducts.push(purchaseProduct);
						});

						var purchaseProductRecordsBulk = ModuleService.prepareRecordBulk(purchaseProducts, $scope.purchaseProductModule);

						var insertRecords = function () {
							ModuleService.insertRecordBulk($scope.purchaseProductModule.name, purchaseProductRecordsBulk)
								.then(function onSuccess() {
									success();
								});
						};

						if (!$scope.id || $scope.revise) {
							if (purchaseProducts.length) {
								insertRecords();
							}
							else {
								success();
							}
						}
						else {
							var ids = [];

							angular.forEach($scope.purchaseProducts, function (purchaseProduct) {
								if (purchaseProduct.id)
									ids.push(purchaseProduct.id);
							});

							if (ids.length) {
								ModuleService.deleteRecordBulk($scope.purchaseProductModule.name, ids)
									.then(function () {
										if (purchaseProducts.length) {
											insertRecords();
										}
										else {
											success();
										}
									});
							}
							else {
								if ($scope.purchaseProducts.length) {
									insertRecords();
								}
								else {
									success();
								}
							}
						}
					}
					else {
						if ($scope.uploader.queue.length < 1)
							success();
					}

					function success() {
						var params = { type: $scope.type, id: response.id };
						if ($scope.saveAndNew) {
							$scope.record = {};
							$scope.moduleForm.$setPristine();
							$window.scrollTo(0, 0);
							ngToast.create({
								content: $filter('translate')('Module.SuccessMessage', { title: $scope.module['label_' + $rootScope.language + '_singular'] }),
								className: 'success'
							});
							$scope.uploader.clearQueue();

							$scope.$broadcast('angucomplete-alt:clearInput');

							if ($scope.module.name === 'activities') {
								$scope.record.activity_type = $scope.activity_type_id;
								$scope.record.related_module = recordCopy.related_module;
								$scope.record.related_to = recordCopy.related_to;
							}

							if ($filter('filter')($scope.module.fields, { name: 'owner' }, true)[0]) {
								$scope.record.owner = $scope.currentUser;
								$scope.$broadcast('angucomplete-alt:changeInput', 'owner', $scope.currentUser);
							}

							//fills lookup inputs auto
							angular.forEach($scope.module.fields, function (field) {
								if (field.data_type === 'lookup') {
									if (!$filter('filter')($scope.module.dependencies, { child_field: field.name }, true)[0]) {
										$scope.record[field.name] = recordCopy[field.name];
										$scope.$broadcast('angucomplete-alt:changeInput', field.name, recordCopy[field.name]);
									}
								}

							});

							ModuleService.setDefaultValues($scope.module, $scope.record, $scope.picklistsModule);
						}
						else {
							if ($scope.parentId) {
								params.type = $scope.parentModule;
								params.id = $scope.parentId;
								params.rptype = $scope.returnTab;

								if ($scope.previousParentType) {
									params.rpptype = $scope.previousParentType;
									params.rppid = $scope.previousParentId;
									params.rprtab = $scope.previousReturnTab;
								}
							}

							if ($scope.back)
								params.back = $scope.back;
						}

						var cacheKey = $scope.module.name + '_' + $scope.module.name;

						if (!$scope.parentId) {
							$cache.remove(cacheKey);

							if ($scope.module.name === 'opportunities')
								$cache.remove('opportunity' + $scope.id + '_stage_history');
						}
						else {
							cacheKey = (!$scope.relatedToField ? $scope.parentType : 'related_to') + $scope.parentId + '_' + (!$scope.many ? $scope.module.name : $scope.many);
							var parentCacheKey = $scope.parentType + '_' + $scope.parentType;
							$cache.remove(cacheKey);
							$cache.remove(parentCacheKey);
						}

						if ($rootScope.activePages && $rootScope.activePages[$scope.module.name])
							$rootScope.activePages[$scope.module.name] = null;

						if ($scope.module.display_calendar || $scope.module.name === 'activities')
							$cache.remove('calendar_events');

						if ($scope.saveAndNew) {
							if ($scope.type === 'quotes') {
								ModuleService.getDailyRates()
									.then(function (response) {
										if (!response.data)
											return;

										var dailyRates = response.data;
										$scope.exchangeRatesDate = $filter('date')(dailyRates.date, 'dd MMMM yyyy') + ' 15:30';

                                        $scope.record.exchange_rate_try_usd = dailyRates.usd;
                                        $scope.record.exchange_rate_try_eur = dailyRates.eur;
                                        $scope.record.exchange_rate_usd_try = 1 / dailyRates.usd;
                                        $scope.record.exchange_rate_usd_eur = (1 / dailyRates.usd) * dailyRates.eur;
                                        $scope.record.exchange_rate_eur_try = 1 / dailyRates.eur;
                                        $scope.record.exchange_rate_eur_usd = (1 / dailyRates.eur) * dailyRates.usd;
									})
							}

							if ($scope.type == 'activities') {
								$scope.submitting = false;
								$scope.record['activity_type'] = $filter('filter')(activityTypes, { system_code: $scope.subtype }, true)[0];
								$scope.subtypeNameLang = $filter('translate')('Module.New', { title: $scope.record['activity_type'].label[$rootScope.language] });
							} else if ($scope.type == 'current_accounts') {
								$scope.record['transaction_type'] = $filter('filter')(transactionTypes, { system_code: $scope.subtype }, true)[0];
								$scope.subtypeNameLang = $filter('translate')('Module.New', { title: $scope.record['transaction_type'].label[$rootScope.language] });
							}
						}

						else {
							if ($scope.module.name === 'stock_transactions') {
								setTimeout(function () {
									$state.go('app.moduleDetail', params);
								}, 500);
                            }
                            else {
                                if ($scope.module.name === 'calisanlar') {
                                    //Kullanıcı bilgilerinin yer aldığı modalda Detaya Git butonuna basılınca çalışacak fonksiyon.
                                    $scope.goModuleDetail = function () {
                                        $state.go('app.moduleDetail', params);
                                        $scope.userCreateModal.hide();
                                    };

                                    //Çalışanlar modülünde User oluşturulurken kullanıcı bilgilerinin yer aldığı Modalın
                                    //gösterilebilmesi için kullanıcının ModüleForm sayfasında bekletilmesi
                                    if ($scope.record['kullanici_olustur'] || $rootScope.branchAvailable) {
                                        $scope.loading = true;
                                    }
                                    else
                                        $state.go('app.moduleDetail', params);
                                }
                                else
                                    $state.go('app.moduleDetail', params);
                            }

						}
						$scope.izinTuruData = null;
					}
				}

                function error(data, status) {
                    $scope.addedUser = false;
					if (status === 409) {
						$scope.moduleForm[data.field].$setValidity('unique', false);

						if (data.field2)
							$scope.moduleForm[data.field2].$setValidity('unique', false);
					}
				}
			};

			$scope.calculate = function (field) {
				ModuleService.calculate(field, $scope.module, $scope.record);
			};

			$scope.fieldValueChange = function (field) {

				if ($scope.module.name === 'activities' && field.name === 'related_module') {
					$scope.dropdownFieldDatas['related_to'] = [];
				}

				if (field.valueChangeDontRun) {
					delete field.valueChangeDontRun;
					return;
				}

				ModuleService.setDependency(field, $scope.module, $scope.record, $scope.picklistsModule, $scope);
				ModuleService.setDisplayDependency($scope.module, $scope.record);
				ModuleService.setCustomCalculations($scope.module, $scope.record, $scope.picklistsModule, $scope);
				ModuleService.customActions($scope.module, $scope.record, $scope.moduleForm, $scope.picklistsModule, $scope);
				components.run('FieldChange', 'Script', $scope, $scope.record, field);

				if ($scope.moduleForm[field.name].$error.unique)
					$scope.moduleForm[field.name].$setValidity('unique', true);

				if ($scope.record.currency)
					$scope.currencySymbol = $scope.record.currency.value || $rootScope.currencySymbol;

				if ($scope.module.name === 'current_accounts' && $scope.currencyField && (field.name === 'customer' || field.name === 'supplier'))
					setCurrencyCurrentAccounts();
			};

			$scope.hideCreateNew = function (field) {
				if (field.lookup_type === 'users')
					return true;

				if (field.lookup_type === 'relation' && !$scope.record.related_module)
					return true;

				return false;
			};

			$scope.openFormModal = function (str) {
				$scope.primaryValueModal = str;

				$scope.formModal = $scope.formModal || $modal({
					scope: $scope,
					templateUrl: 'view/app/module/moduleFormModal.html',
					animation: '',
					backdrop: 'static',
					show: false
				});

				$scope.formModal.$promise.then($scope.formModal.show);
			};

			function getVatList() {
				if (!$scope.record.vat_list)
					return;

				var vatListStr = angular.copy($scope.record.vat_list);
				var vatList = vatListStr.split('|');
				$scope.vatList = [];

				angular.forEach(vatList, function (vatItem) {
					var vatParts = vatItem.split(';');
					var vat = {};
					vat.percent = vatParts[0];
					vat.total = vatParts[1];

					$scope.vatList.push(vat);
				});
			}

			function getProductRecords(module) {
				ModuleService.getPicklists($scope.productModule)
					.then(function (productModulePicklists) {
						if (module === 'quotes') {
							$scope.quoteProductsLoading = true;
							var extraFields = ['unit_amount', 'separator', 'purchase_price', 'profit_amount', 'profit_percent'];
							var additionalFields = [];
							for (var i = 0; extraFields.length > i; i++) {
								var field = $filter('filter')($scope.quoteProductModule.fields, { name: extraFields[i] }, true);
								if (field.length > 0) {
									additionalFields.push(extraFields[i]);
								}
							}

							ModuleService.getPicklists($scope.quoteProductModule)
								.then(function (quoteProductModulePicklists) {
									var findRequest = {};
									findRequest.fields = ['quantity', 'currency', 'usage_unit', 'vat_percent', 'unit_price', 'discount_percent', 'discount_amount', 'discount_type', 'amount', 'order', 'product.products.id', 'product.products.name.primary', 'product.products.unit_price', 'product.products.usage_unit', 'product.products.vat_percent', 'deleted'];
									findRequest.filters = [{ field: 'quote', operator: 'equals', value: $scope.id }];
									findRequest.sort_field = 'order';
									findRequest.sort_direction = 'asc';
									findRequest.limit = 1000;
									findRequest.fields = findRequest.fields.concat(additionalFields);

									if ($scope.productCurrencyField)
										findRequest.fields.push('product.products.currency');

									ModuleService.findRecords($scope.quoteProductModule.name, findRequest)
										.then(function (response) {
											$scope.quoteProducts = [];

											angular.forEach(response.data, function (quoteProductRecordData) {
												angular.forEach(quoteProductRecordData, function (value, key) {
													if (key.indexOf('.') > -1) {
														var keyParts = key.split('.');

														quoteProductRecordData[keyParts[0] + '.' + keyParts[2]] = quoteProductRecordData[key];
														delete quoteProductRecordData[key];
													}
												});

												var quoteProductRecord = ModuleService.processRecordSingle(quoteProductRecordData, $scope.quoteProductModule, quoteProductModulePicklists);

												if (quoteProductRecord.product) {
													if (quoteProductRecord.usage_unit == null || !quoteProductRecord.usage_unit) {
														if (angular.isArray(quoteProductRecord.product.usage_unit)) {
															quoteProductRecord.usage_unit = quoteProductRecord.product.usage_unit['label_' + $rootScope.language];
														} else {
															quoteProductRecord.usage_unit = quoteProductRecord.usage_unit;
														}
													} else {
														quoteProductRecord.product.usage_unit = quoteProductRecord.usage_unit;
														quoteProductRecord.product.purchase_price = quoteProductRecord.purchase_price;
													}

													if (quoteProductRecord.vat_percent == null || !quoteProductRecord.vat_percent) {
														quoteProductRecord.vat_percent = quoteProductRecord.product.vat_percent;
													} else {
														quoteProductRecord.product.vat_percent = quoteProductRecord.vat_percent;
													}

													if (quoteProductRecord.currency == null || !quoteProductRecord.currency) {
														if (quoteProductRecord.product.currency && !angular.isObject(quoteProductRecord.product.currency)) {
															var currencyField = $filter('filter')($scope.productModule.fields, { name: 'currency' }, true)[0];
															var currencyPicklistItem = $filter('filter')(productModulePicklists[currencyField.picklist_id], { labelStr: quoteProductRecord.product.currency }, true)[0];
															quoteProductRecord.product.currency = currencyPicklistItem;
														}
													}
													else {
														quoteProductRecord.product.currency = quoteProductRecord.currency;
														quoteProductRecord.product.unit_price = quoteProductRecord.unit_price;
													}

												}


												$scope.quoteProducts.push(quoteProductRecord);
											});
										})
										.finally(function () {
											$scope.quoteProductsLoading = false;
										});
								});

							getVatList();
						}
						else if (module === 'sales_orders') {
							$scope.orderProductsLoading = true;

							var extraFields = ['unit_amount', 'separator', 'purchase_price', 'profit_amount', 'profit_percent'];
							var additionalFields = [];
							for (var i = 0; extraFields.length > i; i++) {
								var field = $filter('filter')($scope.orderProductModule.fields, { name: extraFields[i] }, true);
								if (field.length > 0) {
									additionalFields.push(extraFields[i]);
								}
							}
							ModuleService.getPicklists($scope.orderProductModule)
								.then(function (orderProductModulePicklists) {
									var findRequest = {};
                                    findRequest.fields = ['quantity', 'currency', 'usage_unit', 'vat_percent', 'unit_price', 'discount_percent', 'discount_amount', 'discount_type', 'amount', 'order', 'product.products.id', 'product.products.name.primary', 'product.products.unit_price', 'product.products.usage_unit', 'product.products.vat_percent', 'deleted'];
									findRequest.filters = [{ field: 'sales_order', operator: 'equals', value: $scope.id }];
									findRequest.sort_field = 'order';
									findRequest.sort_direction = 'asc';
									findRequest.limit = 1000;
                                    findRequest.fields = findRequest.fields.concat(additionalFields);

									if ($scope.productCurrencyField)
										findRequest.fields.push('product.products.currency');

									ModuleService.findRecords($scope.orderProductModule.name, findRequest)
										.then(function (response) {
											$scope.orderProducts = [];

											angular.forEach(response.data, function (orderProductRecordData) {
												angular.forEach(orderProductRecordData, function (value, key) {
													if (key.indexOf('.') > -1) {
														var keyParts = key.split('.');

														orderProductRecordData[keyParts[0] + '.' + keyParts[2]] = orderProductRecordData[key];
														delete orderProductRecordData[key];
													}
												});

												var orderProductRecord = ModuleService.processRecordSingle(orderProductRecordData, $scope.orderProductModule, orderProductModulePicklists);

												if (orderProductRecord.product) {
													if (orderProductRecord.usage_unit == null || !orderProductRecord.usage_unit) {
														orderProductRecord.usage_unit = orderProductRecord.product.usage_unit['label_' + $rootScope.language];
													} else {
														orderProductRecord.product.usage_unit = orderProductRecord.usage_unit;
														orderProductRecord.product.purchase_price = orderProductRecord.purchase_price;
													}

													if (orderProductRecord.vat_percent == null || !orderProductRecord.vat_percent) {
														orderProductRecord.vat_percent = orderProductRecord.product.vat_percent;
													} else {
														orderProductRecord.product.vat_percent = orderProductRecord.vat_percent;
													}

													if (orderProductRecord.currency == null || !orderProductRecord.currency) {
														if (orderProductRecord.product.currency && !angular.isObject(orderProductRecord.product.currency)) {
															var currencyField = $filter('filter')($scope.productModule.fields, { name: 'currency' }, true)[0];
															var currencyPicklistItem = $filter('filter')(productModulePicklists[currencyField.picklist_id], { labelStr: orderProductRecord.product.currency }, true)[0];
															orderProductRecord.product.currency = currencyPicklistItem;
														}
													}
													else {
														orderProductRecord.product.currency = orderProductRecord.currency;
														orderProductRecord.product.unit_price = orderProductRecord.unit_price;
													}

												}

												$scope.orderProducts.push(orderProductRecord);
											});
										})
										.finally(function () {
											$scope.orderProductsLoading = false;
										});
								});

							getVatList();
						}
						else if (module === 'purchase_orders') {
							$scope.purchaseProductsLoading = true;
							var extraFields = ['unit_amount', 'separator', 'purchase_price', 'profit_amount', 'profit_percent'];
							var additionalFields = [];
							for (var i = 0; extraFields.length > i; i++) {
								var field = $filter('filter')($scope.purchaseProductModule.fields, { name: extraFields[i] }, true);
								if (field.length > 0) {
									additionalFields.push(extraFields[i]);
								}
							}
							ModuleService.getPicklists($scope.purchaseProductModule)
								.then(function (purchaseProductModulePicklists) {
									var findRequest = {};
									findRequest.fields = ['quantity', 'currency', 'usage_unit', 'vat_percent', 'unit_price', 'discount_percent', 'discount_amount', 'discount_type', 'amount', 'order', 'product.products.id', 'product.products.name.primary', 'product.products.unit_price', 'product.products.usage_unit', 'product.products.vat_percent', 'deleted'];
									findRequest.filters = [{ field: 'purchase_order', operator: 'equals', value: $scope.id }];
									findRequest.sort_field = 'order';
									findRequest.sort_direction = 'asc';
									findRequest.limit = 1000;
									findRequest.fields = findRequest.fields.concat(additionalFields);
									if ($scope.productCurrencyField)
										findRequest.fields.push('product.products.currency');

									ModuleService.findRecords($scope.purchaseProductModule.name, findRequest)
										.then(function (response) {
											$scope.purchaseProducts = [];

											angular.forEach(response.data, function (purchaseProductRecordData) {
												angular.forEach(purchaseProductRecordData, function (value, key) {
													if (key.indexOf('.') > -1) {
														var keyParts = key.split('.');

														purchaseProductRecordData[keyParts[0] + '.' + keyParts[2]] = purchaseProductRecordData[key];
														delete purchaseProductRecordData[key];
													}
												});

												var purchaseProductRecord = ModuleService.processRecordSingle(purchaseProductRecordData, $scope.purchaseProductModule, purchaseProductModulePicklists);

												if (purchaseProductRecord.product) {
													if (purchaseProductRecord.usage_unit == null || !purchaseProductRecord.usage_unit) {
														purchaseProductRecord.usage_unit = purchaseProductRecord.product.usage_unit['label_' + $rootScope.language];
													} else {
														purchaseProductRecord.product.usage_unit = purchaseProductRecord.usage_unit;
														purchaseProductRecord.product.purchase_price = purchaseProductRecord.purchase_price;
													}

													if (purchaseProductRecord.vat_percent == null || !purchaseProductRecord.vat_percent) {
														purchaseProductRecord.vat_percent = purchaseProductRecord.product.vat_percent;
													} else {
														purchaseProductRecord.product.vat_percent = purchaseProductRecord.vat_percent;
													}

													if (purchaseProductRecord.currency == null || !purchaseProductRecord.currency) {
														if (purchaseProductRecord.product.currency && !angular.isObject(purchaseProductRecord.product.currency)) {
															var currencyField = $filter('filter')($scope.productModule.fields, { name: 'currency' }, true)[0];
															var currencyPicklistItem = $filter('filter')(productModulePicklists[currencyField.picklist_id], { labelStr: purchaseProductRecord.product.currency }, true)[0];
															purchaseProductRecord.product.currency = currencyPicklistItem;
														}
													}
													else {
														purchaseProductRecord.product.currency = purchaseProductRecord.currency;
														purchaseProductRecord.product.unit_price = purchaseProductRecord.unit_price;
													}

												}

												$scope.purchaseProducts.push(purchaseProductRecord);
											});
										})
										.finally(function () {
											$scope.purchaseProductsLoading = false;
										});
								});

							getVatList();
						}
						else if (module === 'purchase_invoices') {
							$scope.purchaseInvoiceProductsLoading = true;
							var extraFields = ['unit_amount', 'separator', 'purchase_price', 'profit_amount', 'profit_percent'];
							var additionalFields = [];
							for (var i = 0; extraFields.length > i; i++) {
								var field = $filter('filter')($scope.purchaseInvoiceProductModule.fields, { name: extraFields[i] }, true);
								if (field.length > 0) {
									additionalFields.push(extraFields[i]);
								}
							}

							ModuleService.getPicklists($scope.purchaseInvoiceProductModule)
								.then(function (purchaseInvoiceProductModulePicklists) {
									var findRequest = {};
									findRequest.fields = ['quantity', 'currency', 'usage_unit', 'vat_percent', 'unit_price', 'discount_percent', 'discount_amount', 'discount_type', 'amount', 'order', 'product.products.id', 'product.products.name.primary', 'product.products.unit_price', 'product.products.usage_unit', 'product.products.vat_percent', 'deleted'];
									findRequest.filters = [{ field: 'purchase_invoice', operator: 'equals', value: $scope.id }];
									findRequest.sort_field = 'order';
									findRequest.sort_direction = 'asc';
									findRequest.limit = 1000;
									findRequest.fields = findRequest.fields.concat(additionalFields);

									if ($scope.productCurrencyField)
										findRequest.fields.push('product.products.currency');

									ModuleService.findRecords($scope.purchaseInvoiceProductModule.name, findRequest)
										.then(function (response) {
											$scope.purchaseInvoiceProducts = [];

											angular.forEach(response.data, function (purchaseInvoiceProductRecordData) {
												angular.forEach(purchaseInvoiceProductRecordData, function (value, key) {
													if (key.indexOf('.') > -1) {
														var keyParts = key.split('.');

														purchaseInvoiceProductRecordData[keyParts[0] + '.' + keyParts[2]] = purchaseInvoiceProductRecordData[key];
														delete purchaseInvoiceProductRecordData[key];
													}
												});

												var purchaseInvoiceProductRecord = ModuleService.processRecordSingle(purchaseInvoiceProductRecordData, $scope.purchaseInvoiceProductModule, purchaseInvoiceProductModulePicklists);

												if (purchaseInvoiceProductRecord.product) {
													if (purchaseInvoiceProductRecord.usage_unit == null || !purchaseInvoiceProductRecord.usage_unit) {
														purchaseInvoiceProductRecord.usage_unit = purchaseInvoiceProductRecord.product.usage_unit['label_' + $rootScope.language];
													} else {
														purchaseInvoiceProductRecord.product.usage_unit = purchaseInvoiceProductRecord.usage_unit;
														purchaseInvoiceProductRecord.product.purchase_price = purchaseInvoiceProductRecord.purchase_price;
													}

													if (purchaseInvoiceProductRecord.vat_percent == null || !purchaseInvoiceProductRecord.vat_percent) {
														purchaseInvoiceProductRecord.vat_percent = purchaseInvoiceProductRecord.product.vat_percent;
													} else {
														purchaseInvoiceProductRecord.product.vat_percent = purchaseInvoiceProductRecord.vat_percent;
													}

													if (purchaseInvoiceProductRecord.currency == null || !purchaseInvoiceProductRecord.currency) {
														if (purchaseInvoiceProductRecord.product.currency && !angular.isObject(purchaseInvoiceProductRecord.product.currency)) {
															var currencyField = $filter('filter')($scope.productModule.fields, { name: 'currency' }, true)[0];
															var currencyPicklistItem = $filter('filter')(productModulePicklists[currencyField.picklist_id], { labelStr: purchaseInvoiceProductRecord.product.currency }, true)[0];
															purchaseInvoiceProductRecord.product.currency = currencyPicklistItem;
														}
													}
													else {
														purchaseInvoiceProductRecord.product.currency = purchaseInvoiceProductRecord.currency;
														purchaseInvoiceProductRecord.product.unit_price = purchaseInvoiceProductRecord.unit_price;
													}

												}


												$scope.purchaseInvoiceProducts.push(purchaseInvoiceProductRecord);
											});
										})
										.finally(function () {
											$scope.purchaseInvoiceProductsLoading = false;
										});
								});

							getVatList();
						}
						else if (module === 'sales_invoices') {
							$scope.salesInvoiceProductsLoading = true;
							var extraFields = ['unit_amount', 'separator', 'purchase_price', 'profit_amount', 'profit_percent'];
							var additionalFields = [];
							for (var i = 0; extraFields.length > i; i++) {
								var field = $filter('filter')($scope.salesInvoiceProductModule.fields, { name: extraFields[i] }, true);
								if (field.length > 0) {
									additionalFields.push(extraFields[i]);
								}
							}

							ModuleService.getPicklists($scope.salesInvoiceProductModule)
								.then(function (salesInvoiceProductModulePicklists) {
									var findRequest = {};
									findRequest.fields = ['quantity', 'currency', 'usage_unit', 'vat_percent', 'unit_price', 'discount_percent', 'discount_amount', 'discount_type', 'amount', 'order', 'product.products.id', 'product.products.name.primary', 'product.products.unit_price', 'product.products.usage_unit', 'product.products.vat_percent', 'deleted'];
									findRequest.filters = [{ field: 'sales_invoice', operator: 'equals', value: $scope.id }];
									findRequest.sort_field = 'order';
									findRequest.sort_direction = 'asc';
									findRequest.limit = 1000;
									findRequest.fields = findRequest.fields.concat(additionalFields);

									if ($scope.productCurrencyField)
										findRequest.fields.push('product.products.currency');

									ModuleService.findRecords($scope.salesInvoiceProductModule.name, findRequest)
										.then(function (response) {
											$scope.salesInvoiceProducts = [];

											angular.forEach(response.data, function (salesInvoiceProductRecordData) {
												angular.forEach(salesInvoiceProductRecordData, function (value, key) {
													if (key.indexOf('.') > -1) {
														var keyParts = key.split('.');

														salesInvoiceProductRecordData[keyParts[0] + '.' + keyParts[2]] = salesInvoiceProductRecordData[key];
														delete salesInvoiceProductRecordData[key];
													}
												});

												var salesInvoiceProductRecord = ModuleService.processRecordSingle(salesInvoiceProductRecordData, $scope.salesInvoiceProductModule, salesInvoiceProductModulePicklists);

												if (salesInvoiceProductRecord.product) {
													if (salesInvoiceProductRecord.usage_unit == null || !salesInvoiceProductRecord.usage_unit) {
														salesInvoiceProductRecord.usage_unit = salesInvoiceProductRecord.product.usage_unit['label_' + $rootScope.language];
													} else {
														salesInvoiceProductRecord.product.usage_unit = salesInvoiceProductRecord.usage_unit;
														salesInvoiceProductRecord.product.purchase_price = salesInvoiceProductRecord.purchase_price;
													}

													if (salesInvoiceProductRecord.vat_percent == null || !salesInvoiceProductRecord.vat_percent) {
														salesInvoiceProductRecord.vat_percent = salesInvoiceProductRecord.product.vat_percent;
													} else {
														salesInvoiceProductRecord.product.vat_percent = salesInvoiceProductRecord.vat_percent;
													}

													if (salesInvoiceProductRecord.currency == null || !salesInvoiceProductRecord.currency) {
														if (salesInvoiceProductRecord.product.currency && !angular.isObject(salesInvoiceProductRecord.product.currency)) {
															var currencyField = $filter('filter')($scope.productModule.fields, { name: 'currency' }, true)[0];
															var currencyPicklistItem = $filter('filter')(productModulePicklists[currencyField.picklist_id], { labelStr: salesInvoiceProductRecord.product.currency }, true)[0];
															salesInvoiceProductRecord.product.currency = currencyPicklistItem;
														}
													}
													else {
														salesInvoiceProductRecord.product.currency = salesInvoiceProductRecord.currency;
														salesInvoiceProductRecord.product.unit_price = salesInvoiceProductRecord.unit_price;
													}

												}


												$scope.salesInvoiceProducts.push(salesInvoiceProductRecord);
											});
										})
										.finally(function () {
											$scope.salesInvoiceProductsLoading = false;
										});
								});

							getVatList();
						}

					});
			}

			ModuleService.getActionButtons($scope.module.id)
				.then(function (actionButtons) {
					$scope.actionButtons = $filter('filter')(actionButtons, function (actionButton) {
						return actionButton.trigger !== 'Detail' && actionButton.trigger !== 'List';
					}, true);
				});

			$scope.showModuleFrameModal = function (url) {
				if (new RegExp("https:").test(url)) {
					var title, w, h;
					title = 'myPop1';
					w = document.body.offsetWidth - 200;
					h = document.body.offsetHeight - 200;
					var left = (screen.width / 2) - (w / 2);
					var top = (screen.height / 2) - (h / 2);
					window.open(url, title, 'toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=no, resizable=no, copyhistory=no, width=' + w + ', height=' + h + ', top=' + top + ', left=' + left);

				}
				else {
					$scope.frameUrl = url;
					$scope.frameModal = $scope.frameModal || $modal({
						scope: $scope,
						controller: 'ActionButtonFrameController',
						templateUrl: 'view/app/actionbutton/actionButtonFrameModal.html',
						backdrop: 'static',
						show: false
					});

					$scope.frameModal.$promise.then($scope.frameModal.show);
				}

			};

			$scope.openLocationModal = function (filedName) {
				$scope.filedName = filedName;
				$scope.locationModal = $scope.frameModal || $modal({
					scope: $scope,
					controller: 'locationFormModalController',
					templateUrl: 'view/app/location/locationFormModal.html',
					backdrop: 'static',
					show: false
				});
				$scope.locationModal.$promise.then($scope.locationModal.show);
			};

			$scope.removeDocument = function (field) {

				var data = {};
				data["module"] = $scope.module.name;
				data["recordId"] = $scope.record.id;
				data["fieldName"] = field.name;
				data["fileNameExt"] = helper.getFileExtension($scope.record[field.name]);
				data["instanceId"] = $rootScope.workgroup.instanceID;

				$scope.record[field.name] = null;
				if (field.data_type == 'document') {
					$scope.uploaderBasic[field.name].queue = [];
				}
				if (field.data_type == 'image') {
					$scope.uploaderImage[field.name].queue = [];
				}
				angular.forEach($scope.moduleForm[field.name].$error, function (value, key) {
					$scope.moduleForm[field.name].$setValidity(key, value);
				});
			};

			$scope.checkUploadFile = function (event) {
				var files = event.target.files;
				var clickedInputId = event.target.id;
				var inputLabel = angular.element(document.getElementById('lbl_' + clickedInputId))[0];
				if (isAcceptedExtension(files[0])) {
					inputLabel.innerText = files[0].name;
				}
			};

			$scope.fileLoadingCounter = 0;

			$scope.uploaderBasic = function (field) {
				var uploader_basic = $scope.uploaderBasic[field.name] = new FileUploader({
					url: config.apiUrl + 'document/upload_document_file',
					headers: {
						'Authorization': 'Bearer ' + $localStorage.read('access_token'),
						'Accept': 'application/json' /// we have to set accept header to provide consistency between browsers.
					},
					queueLimit: 1

				});

				uploader_basic.onAfterAddingFile = function (item) {
					var selectedFile = item.uploader.queue[0].file.name;
					$scope.record[field.name] = selectedFile;

					var uniquefieldname = field.name;

					item.formData.push({ name: uniquefieldname });
					item.formData.push({ filename: $scope.record[field.name] });
					item.formData.push({ recordid: $scope.record.id });
					item.formData.push({ modulename: $scope.module.name })
					item.formData.push({ container: $rootScope.workgroup.instanceID })
					item.formData.push({ documentsearch: field.document_search })

				};

				uploader_basic.onWhenAddingFileFailed = function (item, filter, options) {
					switch (filter.name) {
						case 'docFilter':
							ngToast.create({ content: $filter('translate')('Setup.Settings.DocumentTypeError'), className: 'warning' });
							break;
						case 'sizeFilter':
							ngToast.create({ content: $filter('translate')('Setup.Settings.SizeError'), className: 'warning' });
							break;
					}
				};

				uploader_basic.filters.push({
					name: 'docFilter',
					fn: function (item) {
						var extension = helper.getFileExtension(item.name);
						return true ? (extension === 'txt' || extension == 'docx' || extension == 'pdf' || extension == 'doc') : false;
					}
				});

				uploader_basic.filters.push({
					name: 'sizeFilter',
					fn: function (item) {
						return item.size < 5242880;//5 mb
					}
				});


				uploader_basic.onCompleteAll = function () {
					uploader_basic.clearQueue();
					$scope.fileLoadingCounter--;
				};

				return uploader_basic;

			};

			$scope.uploaderImage = function (field) {
				$scope.image[field.name] = {};
				var uploader_image = $scope.uploaderImage[field.name] = new FileUploader({
					url: config.apiUrl + 'document/upload_large',
					headers: {
						'Authorization': 'Bearer ' + $localStorage.read('access_token'),
						'X-Tenant-Id': $cookies.get('tenant_id'),
						'Accept': 'application/json' /// we have to set accept header to provide consistency between browsers.
					},
					queueLimit: 1,
				});

				uploader_image.onAfterAddingFile = function (item) {
					readFile(item._file)
						.then(function (image) {
							item.image = image;
							var img = new Image();
							resizeService.resizeImage(item.image, { width: 1024 }, function (err, resizedImage) {
								if (err)
									return;

								item._file = dataURItoBlob(resizedImage);
								item.file.size = item._file.size;
								$scope.fileLoadingCounter++;
								var selectedFile = item.uploader.queue[0].file.name;
								$scope.record[field.name] = selectedFile;
								$scope.image[field.name]['Name'] = item.uploader.queue[0].file.name;
								$scope.image[field.name]['Size'] = item.uploader.queue[0].file.size;
								$scope.image[field.name]['Type'] = item.uploader.queue[0].file.type;
								item.upload();
							});
						});
				};
				uploader_image.onWhenAddingFileFailed = function (item, filter, options) {
					switch (filter.name) {
						case 'imgFilter':
							ngToast.create({
								content: $filter('translate')('Setup.Settings.ImageError'),
								className: 'warning'
							});
							break;
						case 'sizeFilter':
							ngToast.create({
								content: $filter('translate')('Setup.Settings.SizeError'),
								className: 'warning'
							});
							break;
					}
				};

				uploader_image.filters.push({
					name: 'imgFilter',
					fn: function (item) {
						var extension = helper.getFileExtension(item.name);
						return true ? (extension === 'jpg' || extension == 'jpeg' || extension == 'png' || extension == 'doc' || extension == 'gif') : false;
					}
				});

				uploader_image.filters.push({
					name: 'sizeFilter',
					fn: function (item) {
						return item.size < 5242880;// 5mb
					}
				});

				uploader_image.onSuccessItem = function (item, response) {
					$scope.image[field.name]['UniqueName'] = response.UniqueName;
					$scope.fileLoadingCounter--;
				};

				var dataURItoBlob = function (dataURI) {
					var binary = atob(dataURI.split(',')[1]);
					var mimeString = dataURI.split(',')[0].split(':')[1].split(';')[0];
					var array = [];

					for (var i = 0; i < binary.length; i++) {
						array.push(binary.charCodeAt(i));
					}

					return new Blob([new Uint8Array(array)], { type: mimeString });
				};

				function readFile(file) {
					var deferred = $q.defer();
					var reader = new FileReader();

					reader.onload = function (e) {
						deferred.resolve(e.target.result);
					};

					reader.readAsDataURL(file);

					return deferred.promise;
				}

				return uploader_image;

			};

			//webhook request func for action button
			$scope.webhookRequest = function (action) {
				var jsonData = {};
				var params = action.parameters.split(',');
				$scope.webhookRequesting = {};

				$scope.webhookRequesting[action.id] = true;

				angular.forEach(params, function (data) {

					var dataObject = data.split('|');
					var parameterName = dataObject[0];
					var moduleName = dataObject[1];
					var fieldName = dataObject[2];

					if (moduleName != $scope.module.name) {
						if ($scope.record[moduleName])
							jsonData[parameterName] = $scope.record[moduleName][fieldName];
						else
							jsonData[parameterName] = null;

						// if page is form;
						// if($scope.record[moduleName][fieldName]){
						//     jsonData[parameterName] = $scope.record[moduleName][fieldName];
						// }
						// else{
						//     ModuleService.getRecord('accounts', $scope.record[moduleName].id)
						//         .then(function (response) {
						//             jsonData[parameterName] = response.data[fieldName];
						//         })
						// }
					}
					else {
						if ($scope.record[fieldName])
							jsonData[parameterName] = $scope.record[fieldName];
						else
							jsonData[parameterName] = null;
					}

				});

				if (action.method_type === 'post') {

					$http.post(action.url, jsonData, { headers: { 'Content-Type': 'application/json' } })
						.then(function () {
							ngToast.create({
								content: $filter('translate')('Module.ActionButtonWebhookSuccess'),
								className: 'success'
							});
							$scope.webhookRequesting[action.id] = false;
						})
						.catch(function () {
							ngToast.create({
								content: $filter('translate')('Module.ActionButtonWebhookFail'),
								className: 'warning'
							});
							$scope.webhookRequesting[action.id] = false;
						});

				}
				else if (action.method_type === 'get') {

					var query = "";

					for (var key in jsonData) {
						query += key + "=" + jsonData[key] + "&";
					}
					if (query.length > 0) {
						query = query.substring(0, query.length - 1);
					}

					$http.get(action.url + "?" + query)
						.then(function () {
							ngToast.create({
								content: $filter('translate')('Module.ActionButtonWebhookSuccess'),
								className: 'success'
							});
							$scope.webhookRequesting[action.id] = false;
						})
						.catch(function () {
							ngToast.create({
								content: $filter('translate')('Module.ActionButtonWebhookFail'),
								className: 'warning'
							});
							$scope.webhookRequesting[action.id] = false;
						});

				}
			};


			$scope.$on('$locationChangeStart', function (event) {
				if ($scope.moduleForm) {
					if ($scope.moduleForm.$dirty) {
						if (!confirm($filter('translate')('Module.LeavePageWarning'))) {
							event.preventDefault();
						}
					}
				}
			});

			$scope.setDropdownData = function (field) {
				if (field.filters && field.filters.length > 0)
					$scope.dropdownFieldDatas[field.name] = null;
				else if ($scope.dropdownFieldDatas[field.name] && $scope.dropdownFieldDatas[field.name].length > 0)
					return;

				$scope.currentLookupField = field;
				$scope.lookup()
					.then(function (response) {
						$scope.dropdownFieldDatas[field.name] = response;
					});

			};

			$scope.getAttachments = function () {
				if (!helper.hasDocumentsPermission($scope.operations.read))
					return;

				DocumentService.getEntityDocuments($rootScope.workgroup.tenant_id, $scope.id, $scope.module.id)
					.then(function (data) {
						$scope.documentsResultSet = DocumentService.processDocuments(data.data, $rootScope.users);
						$scope.documents = $scope.documentsResultSet.documentList;
						$scope.loadingDocuments = false;
					});
			};
		}
	]);