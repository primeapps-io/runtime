'use strict';

angular.module('primeapps')

	.controller('ModuleFormController', ['$rootScope', '$scope', 'ngToast', '$filter', 'helper', '$location', '$state', '$stateParams', '$q', '$window', '$localStorage', '$cache', 'config', '$timeout', 'operations', '$modal', 'FileUploader', 'activityTypes', 'transactionTypes', 'ModuleService', 'DocumentService', '$http', 'resizeService', 'components',
		function ($rootScope, $scope, ngToast, $filter, helper, $location, $state, $stateParams, $q, $window, $localStorage, $cache, config, $timeout, operations, $modal, FileUploader, activityTypes, transactionTypes, ModuleService, DocumentService, $http, resizeService, components) {
			if (!$scope.$parent.$parent.formType) {
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
				$scope.isDisabled = false;
			} else {
				var parent = $scope.$parent.$parent;
				$scope.formType = parent.formType;
				$scope.type = parent.type;
				$scope.id = parent.id;
				$scope.subtype = parent.stype;
				$scope.parentType = parent.ptype;
				$scope.parentId = parent.pid;
				$scope.returnTab = parent.rtab;
				$scope.previousParentType = parent.pptype;
				$scope.previousParentId = parent.ppid;
				$scope.previousReturnTab = parent.prtab;
				$scope.back = parent.back;
				$scope.many = parent.many;
				$scope.clone = parent.clone;
				$scope.revise = parent.revise;
				$scope.paramField = parent.field;
				$scope.paramValue = parent.value;
				$scope.actionButtonDisabled = false;

			}

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
			$scope.document = {};
			$scope.hasProcessEditPermission = false;
			$scope.userAdded = false;

			if ($scope.parentId)
				$window.scrollTo(0, 0);

			$scope.module = $filter('filter')($rootScope.modules, { name: $scope.type }, true)[0];

			/**User isn't creating a sample data on module form. He creates a record and he has to change it on module detail form.
			 * We are changing module system section because if user will create a sample data he can create on module form to sample data.
			 * **/
			if ($scope.preview) {
				var section = $filter('filter')($scope.module.sections, { name: 'system' }, true)[0];
				section.display_form = true;
			}

			components.run('BeforeFormLoaded', 'script', $scope);
			$scope.currentSectionComponentsTemplate = currentSectionComponentsTemplate;

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

			$scope.dropdownFields = $filter('filter')($scope.module.fields, {
				data_type: 'lookup',
				show_as_dropdown: true
			}, true);
			$scope.dropdownFieldDatas = {};
			for (var i = 0; i < $scope.dropdownFields.length; i++) {
				$scope.dropdownFieldDatas[$scope.dropdownFields[i].name] = [];
			}

			if (!$scope.id && !$scope.hasPermission($scope.type, $scope.operations.write)) {
				if (!helper.hasCustomProfilePermission('bulk_update')) {
					ngToast.create({ content: $filter('translate')('Common.Forbidden'), className: 'warning' });
					$state.go('app.dashboard');
					return;
				}
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

			//<--TODO:COMPONENT
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
									request.filters = [{
										field: 'country',
										operator: 'equals',
										value: countryPicklistItemTr.labelStr,
										no: 1
									}];
								else
									request.filters = [{
										field: 'country',
										operator: 'is',
										value: countryPicklistItemEn.labelStr
									}];

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

			//TODO:COMPONENT -->
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

			//TODO:COMPONENT
			if ($scope.parentType) {
				if ($scope.type === 'activities' || $scope.type === 'mails' || $scope.many) {
					$scope.parentModule = $scope.parentType;
				} else {
					var parentTypeField = $filter('filter')($scope.module.fields, { name: $scope.parentType }, true)[0];

					if (!parentTypeField) {
						$scope.parentType = null;
						$scope.parentId = null;
					} else {
						$scope.parentModule = parentTypeField.lookup_type;
					}
				}
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
					return true;

				if ($scope.module.dependencies.length > 0) {
					var freezeDependencies = $filter('filter')($scope.module.dependencies, { dependency_type: 'freeze' }, true);
					angular.forEach(freezeDependencies, function (dependency) {
						var freezeFields = $filter('filter')($scope.module.fields, { name: dependency.parent_field }, true);
						angular.forEach(freezeFields, function (field) {
							angular.forEach(dependency.values_array, function (value) {
								if (record[field.name] && (value === record[field.name] || value === record[field.name].id))
									type = true;
							});
						});
					});
				}
				return type;
			};

			
			var checkBranchSettingsAvailable = function () {
				if ($rootScope.branchAvailable) {
					$scope.branchManager = $filter('filter')($rootScope.users, { role_id: parseInt($scope.record['branch']) }, true)[0];
					$scope.record.Authorities = [];
					if ($scope.branchManager) {
						$http.get(config.apiUrl + 'user_custom_shares/get_all_by_shared_user_id/' + $scope.branchManager.Id)
							.then(function (response) {
								$scope.authorities = response.data;
								angular.forEach($scope.authorities, function (authority) {
									var user = $filter('filter')($rootScope.users, { id: authority['user_id'] }, true)[0];
									$scope.record.Authorities.push({
										id: user.id,
										full_name: user.full_name,
										email: user.mail
									});
								});
								$scope.showBranchSettings = true;
							});
					}

					//$scope.authorities
				}
			};
			//TODO:COMPONENT --> 

			var checkEditPermission = function () {
				if (!$scope.hasProcessEditPermission) {
					if ($scope.id && (($scope.record.freeze && !$rootScope.user.profile.HasAdminRights) || ($scope.record.process_id && $scope.record.process_status !== 3 && !$rootScope.user.profile.HasAdminRights))) {
						ngToast.create({ content: $filter('translate')('Common.Forbidden'), className: 'warning' });
						$state.go('app.dashboard');
					}
				}

				checkBranchSettingsAvailable();
			};

			ModuleService.getPicklists($scope.module)
				.then(function (picklists) {
					$scope.picklistsModule = picklists;
					var ownerField = $filter('filter')($scope.module.fields, { name: 'owner' }, true)[0];

					var setFieldDependencies = function () {
						angular.forEach($scope.module.fields, function (field) {
							ModuleService.setDependency(field, $scope.module, $scope.record, $scope.picklistsModule, $scope);
							if ($scope.module.name !== 'activities') {
								if ((!$scope.record.id && field.default_value && field.data_type === 'picklist') || (field.default_value && field.data_type === 'picklist' && field.default_value === $scope.record[field.name].id)) {
									$scope.record[field.name] = $filter('filter')($scope.picklistsModule[field.picklist_id], { id: field.default_value })[0];
									$scope.fieldValueChange(field);
								}
							}
						});
					};

					components.run('BeforeFormPicklistLoaded', 'Script', $scope);

					//<--TODO:COMPONENT
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
					//TODO:COMPONENT --> 

					if (!$scope.id) {
						$scope.loading = false;
						$scope.record.owner = $scope.currentUser;

						//<--TODO:COMPONENT
						if (!$rootScope.branchAvailable) {
							var userCreateField = $filter('filter')($scope.module.fields, { name: 'kullanici_olustur' }, true)[0];
							if (userCreateField)
								userCreateField.hidden = false;
						}

						if ($rootScope.isEmployee === $scope.module.name || $scope.module.name === 'calisanlar') {
							ModuleService.getUserLicenseStatus()
								.then(function (response) {
									$scope.userLicenseControl = response.data;
									$scope.userLicenseKalan = $scope.userLicenseControl.total - $scope.userLicenseControl.used;
								});
						}
						
						if ($scope.subtype) {
							if ($scope.type === 'activities') {
								$scope.record['activity_type'] = $filter('filter')(activityTypes, { system_code: $scope.subtype }, true)[0];
								$scope.subtypeNameLang = $filter('translate')('Module.New', { title: $scope.record['activity_type'].label[$rootScope.language] });
							}
						}

						if ($scope.parentId) {
							var moduleParent = $filter('filter')($rootScope.modules, { name: $scope.parentModule }, true)[0];

							ModuleService.getRecord($scope.parentModule, $scope.parentId)
								.then(function onSuccess(parent) {

									var moduleParentPrimaryField = $filter('filter')(moduleParent.fields, {
										primary: true,
										deleted: false
									}, true)[0];
									var lookupRecord = {};
									lookupRecord.id = parent.data.id;
									lookupRecord.primary_value = parent.data[moduleParentPrimaryField.name];
									
									if ($scope.parentModule === 'calisanlar') {
										lookupRecord['e_posta'] = parent.data['e_posta'];
									}
									if ($rootScope.isEmployee && $scope.parentModule === $rootScope.isEmployee && $rootScope.newEpostaFieldName) {
										lookupRecord[$rootScope.newEpostaFieldName] = parent.data[$rootScope.newEpostaFieldName];
									}
									//TODO:COMPONENT --> 

									//<--TODO:COMPONENT

									if (($scope.type === 'activities' || $scope.type === 'mails') && $scope.relatedToField) {
										$scope.record['related_to'] = lookupRecord;
										$scope.record['related_module'] = $filter('filter')(picklists['900000'], { value: $scope.parentType }, true)[0];
									} else {
										
										$scope.record[$scope.parentType] = lookupRecord;
										
										var relatedDependency = $filter('filter')($scope.module.dependencies, { dependent_field: $scope.parentType }, true)[0];

										if (relatedDependency && relatedDependency.deleted !== true) {
											var dependencyField = $filter('filter')($scope.module.fields, { name: relatedDependency.field }, true)[0];
											$scope.record[relatedDependency.field] = $filter('filter')($scope.picklistsModule[dependencyField.picklist_id], { id: relatedDependency.values[0] }, true)[0];

											var dependentField = $filter('filter')($scope.module.fields, { name: relatedDependency.dependent_field }, true)[0];
											dependentField.hidden = false;
										}

										setFieldDependencies();
									}

									//TODO:COMPONENT --> 


								});
						} else {
							setFieldDependencies();
						}

						ModuleService.setDefaultValues($scope.module, $scope.record, picklists);
						ModuleService.setDisplayDependency($scope.module, $scope.record);

						if ($scope.paramField) {
							$scope.record[$scope.paramField] = $scope.paramValue;
							$rootScope.hideSipPhone();
						}
						components.run('FieldChange', 'Script', $scope, $scope.record, ownerField);

						return;
					}

					ModuleService.getRecord($scope.module.name, $scope.id)
						.then(function onSuccess(recordData) {

							if (Object.keys(recordData.data).length === 0) {
								ngToast.create({
									content: $filter('translate')('Common.Forbidden'),
									className: 'warning'
								});
								$state.go('app.dashboard');
								return;
							}

							var record = ModuleService.processRecordSingle(recordData.data, $scope.module, $scope.picklistsModule);

							//<--TODO:COMPONENT
							//Kullanıcı oluşturulduysa edit sayfasında kullanıcı oluşturma alanları gizleniyor.
							if ($rootScope.isEmployee === $scope.module.name || $scope.module.name === 'calisanlar') {
								var userCreateField = $filter('filter')($scope.module.fields, { name: 'kullanici_olustur' }, true)[0];
								if (userCreateField) {
									userCreateField.hidden = true;
									record[userCreateField.name] = false;
								}
							} else {
								//Düzenle butonundan Edit Sayfasına gidildiğinde Lisans kontrolü için Lisans bilgileri çekiliyor.
								if ($rootScope.isEmployee === $scope.module.name || $scope.module.name === 'calisanlar') {
									ModuleService.getUserLicenseStatus()
										.then(function (response) {
											$scope.userLicenseControl = response.data;
											$scope.userLicenseKalan = $scope.userLicenseControl.total - $scope.userLicenseControl.used;
										});
								}
							}
							//TODO:COMPONENT --> 

							if (!$scope.hasPermission($scope.type, $scope.operations.modify, recordData.data) || (isFreeze(record) && !$scope.hasAdminRights)) {
								ngToast.create({
									content: $filter('translate')('Common.Forbidden'),
									className: 'warning'
								});
								$state.go('app.dashboard');
								return;
							}


							components.run('BeforeFormRecordLoaded', 'Script', $scope, record);
							ModuleService.formatRecordFieldValues(angular.copy(recordData.data), $scope.module, $scope.picklistsModule);
							$scope.title = $scope.primaryField.valueFormatted;
							$scope.recordState = angular.copy(record);
							ModuleService.setDisplayDependency($scope.module, record);


							//encrypted fields
							for (var f = 0; f < $scope.module.fields.length; f++) {
								var field = $scope.module.fields[f];
								var showEncryptedInput = false;
								if (field.encrypted && field.encryption_authorized_users_list.length > 0 && record[field.name]) {
									for (var p = 0; p < field.encryption_authorized_users_list.length; p++) {
										var encryrptionPermission = field.encryption_authorized_users_list[p];
										if ($rootScope.user.id === parseInt(encryrptionPermission))
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


										if ($scope.module.name !== 'activities')
											setFieldDependencies();

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
							} else {
								$scope.record = record;
								if ($scope.clone) {
									$scope.record.owner = $scope.currentUser;
								}

								if ($scope.module.name !== 'activities')
									setFieldDependencies();

								$scope.loading = false;
							}

							if ($scope.record.currency)
								$scope.currencySymbol = $scope.record.currency.value || $rootScope.currencySymbol;

							ModuleService.customActions($scope.module, $scope.record);

							components.run('FieldChange', 'Script', $scope, $scope.record, ownerField);

							components.run('AfterFormRecordLoaded', 'Script', $scope);
						})
						.catch(function onError() {
							$scope.loading = false;
						});

					components.run('AfterFormPicklistLoaded', 'Script', $scope);
				});

			$scope.lookup = function (searchTerm) {
				$scope.searchTerm = searchTerm;
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
				
				$scope.customFilters = null;

				components.run('BeforeLookup', 'Script', $scope);

				if ($scope.lookupCurrentData) {
					return $scope.lookupCurrentData;
				}

				if ($scope.currentLookupField.lookup_type === 'users')
					return ModuleService.lookup(searchTerm, $scope.currentLookupField, $scope.record, ['email'], false, $scope.customFilters);
				else if ($scope.currentLookupField.lookup_type === 'profiles')
					return ModuleService.lookup(searchTerm, $scope.currentLookupField, $scope.record, null, false, $scope.customFilters);
				else if ($scope.currentLookupField.lookup_type === 'roles')
					return ModuleService.lookup(searchTerm, $scope.currentLookupField, $scope.record, null, false, $scope.customFilters);
				else if ($scope.currentLookupField.lookup_type === 'calisanlar')
					return ModuleService.lookup(searchTerm, $scope.currentLookupField, $scope.record, ['e_posta'], false, $scope.customFilters);
				else if ($rootScope.isEmployee && $scope.currentLookupField.lookup_type === $rootScope.isEmployee && $rootScope.newEpostaFieldName)
					return ModuleService.lookup(searchTerm, $scope.currentLookupField, $scope.record, [$rootScope.newEpostaFieldName], false, $scope.customFilters);
				else
					return ModuleService.lookup(searchTerm, $scope.currentLookupField, $scope.record, null, false, $scope.customFilters);

			};

			$scope.multiselect = function (searchTerm, field) {
				var picklistItems = [];

				angular.forEach($scope.picklistsModule[field.picklist_id], function (picklistItem) {
					if (picklistItem.inactive || picklistItem.hidden)
						return;

					picklistItem.labelLang = picklistItem.label[$rootScope.user.language];

					if (picklistItem.labelLang.toLowerCase().indexOf(searchTerm.toLowerCase()) > -1 || picklistItem.labelLang.toUpperCase().indexOf(searchTerm.toUpperCase()) > -1
						|| picklistItem.labelLang.toLowerCaseTurkish().indexOf(searchTerm.toLowerCaseTurkish()) > -1 || picklistItem.labelLang.toUpperCaseTurkish().indexOf(searchTerm.toUpperCaseTurkish()) > -1)
						picklistItems.push(picklistItem);
				});

				return picklistItems;
			};

			$scope.tags = function (searchTerm, field) {
				return $http.get(config.apiUrl + "tag/get_tag/" + field.id).then(function (response) {
					var tags = response.data;
					return tags.filter(function (tag) {
						return tag.text.toLowerCase().indexOf(searchTerm.toLowerCase()) !== -1;
					});
				});
			};

			$scope.setCurrentLookupField = function (field) {
				$scope.currentLookupField = field;
			};

			$scope.uploader = new FileUploader({
				url: 'storage/record_file_upload',
				headers: {
					appId: $rootScope.user.app_id,
					'X-App-Id': $rootScope.user.app_id
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
				if ($scope.userLicenseKalan === 0 || $scope.userLicenseKalan < 0) {
					if (!$rootScope.branchAvailable) {
						var userProfile = $filter('filter')($scope.module.fields, { name: 'kullanici_profili' }, true)[0];
						var userRole = $filter('filter')($scope.module.fields, { name: 'kullanici_rolu' }, true)[0];
						$scope.record['kullanici_olustur'] = false;
						$scope.record['kullanici_profili'] = null;
						$scope.record['kullanici_rolu'] = null;
						if (userProfile)
							userProfile.hidden = true;
						if (userRole)
							userRole.hidden = true;
					}

					ngToast.create({
						content: $filter('translate')('Setup.Users.LicenceRequired'),
						className: 'warning'
					});
					$scope.submitting = false;
					return;
				}

				//Sisteme kayıtlı bir user ile kullanıcı oluşturmak istenildiğinde yapılan user kontrolü.
				ModuleService.getUserEmailControl($rootScope.isEmployee && $rootScope.newEpostaFieldName ? $scope.record[$rootScope.newEpostaFieldName] : $scope.record.e_posta)
					.then(function (response) {
						var userEmail = response.data === 'Available';
						if (!userEmail) {
							ngToast.create({
								content: $filter('translate')('Setup.Users.NewUserError'),
								className: 'warning'
							});

							if (!$rootScope.branchAvailable) {
								var userProfile = $filter('filter')($scope.module.fields, { name: 'kullanici_profili' }, true)[0];
								var userRole = $filter('filter')($scope.module.fields, { name: 'kullanici_rolu' }, true)[0];
								$scope.record['kullanici_olustur'] = false;
								$scope.record['kullanici_profili'] = null;
								$scope.record['kullanici_rolu'] = null;
								if (userProfile)
									userProfile.hidden = true;
								if (userRole)
									userRole.hidden = true;
							}

							$scope.submitting = false;
							return;
						} else {
							var profileId = $scope.record.kullanici_profili ? $scope.record.kullanici_profili.id : null;
							var roleId = $scope.record.kullanici_rolu ? $scope.record.kullanici_rolu.id : null;

							var createUser = function (roleId, profileId, record) {
								var inviteModel = {};
								inviteModel.email = $rootScope.isEmployee && $rootScope.newEpostaFieldName ? $scope.record[$rootScope.newEpostaFieldName] : $scope.record.e_posta;
								inviteModel.first_name = $rootScope.newAdFieldName ? $scope.record[$rootScope.newAdFieldName] : $scope.record.ad;
								inviteModel.last_name = $rootScope.newSoyadFieldName ? $scope.record[$rootScope.newSoyadFieldName] : $scope.record.soyad;
								inviteModel.profile = profileId;
								inviteModel.role = roleId;
								inviteModel.full_name = inviteModel.first_name + " " + inviteModel.last_name;

								if (!inviteModel || !inviteModel.email || !inviteModel.profile || !inviteModel.role || !inviteModel.first_name || !inviteModel.last_name)
									return;

								$scope.userInviting = true;
								inviteModel.profile_id = inviteModel.profile;
								inviteModel.role_id = inviteModel.role;
								inviteModel.dont_send_mail = true;
								$scope.addedUser = angular.copy(inviteModel);

								ModuleService.addUser(inviteModel)
									.then(function (response) {
										if (response.data) {
											//User Lisans ve şifre bilgilerinin setlenmesi.
											$scope.userCreatePassword = response.data.password;
											$scope.userCreateEmail = $rootScope.isEmployee && $rootScope.newEpostaFieldName ? $scope.record[$rootScope.newEpostaFieldName] : $scope.record.e_posta;
											$scope.userLicenseAvailable = $scope.userLicenseKalan - 1;
											$scope.userLicensesBought = $scope.userLicenseControl.total;
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
													return value.id === account.user.id;
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
							};

							if ($rootScope.branchAvailable) {
								ModuleService.getRecord('branches', $scope.record.branch.id)
									.then(function (response) {
										roleId = response.data.branch;
										profileId = $scope.record.profile.id;
										createUser(roleId, profileId, record);
									}).catch(function (error) {
										$scope.submitting = false;
									});
							} else {
								createUser(roleId, profileId, record);
							}
						}
					});
			};

			$scope.submit = function (record) {
				function validate() {
					var isValid = true;
					
					angular.forEach($scope.module.fields, function (field) {
						if (!record[field.name])
							return;

						if (field.data_type === 'lookup' && typeof record[field.name] !== 'object') {
							$scope.moduleForm[field.name].$setValidity('object', false);
							isValid = false;
						}

						if (field.data_type === 'image' && $scope.image[field.name] && $scope.image[field.name].UniqueName && record[field.name] != null) {
							record[field.name] = $scope.image[field.name].UniqueName;
						}

						if (field.data_type === 'document' && $scope.document[field.name] && $scope.document[field.name].UniqueName && record[field.name] != null) {
							record[field.name] = $scope.document[field.name].UniqueName;
						}

					});

					return isValid;
				}

				$scope.submitting = true;

				if (!$scope.moduleForm.$valid || !validate()) {
					$scope.submitting = false;
					return;
				}

				components.run('BeforeFormSubmit', 'Script', $scope, record);

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

				if (!$scope.id && ($scope.module.name === 'calisanlar' || $scope.module.name === $rootScope.isEmployee) && ($scope.record['kullanici_olustur'] || $rootScope.branchAvailable) && !$scope.userAdded) {
					$scope.addUser(record);
					return;
				}

				if ($rootScope.branchAvailable && $scope.branchManager) {
					angular.forEach($scope.record.Authorities, function (authority) {
						var checker = $filter('filter')($scope.authorities, { user_id: authority.id }, true)[0];

						if (!checker) {
							$http.post(config.apiUrl + 'user_custom_shares/create/', {
								shared_user_id: $scope.branchManager.id,
								user_id: authority.id
							});
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
						$scope.submitting = false;
						return;
					}
				}

				if ($scope.clone) {

					delete $scope.record.created_at;
					delete $scope.record.created_by;
					delete $scope.record.updated_at;
					delete $scope.record.updated_by;

					angular.forEach($scope.module.fields, function (field) {
						if (field.data_type === 'number_auto') {
							delete record[field.name];
						}
					});
					$scope.recordState = null;
				}

				record = ModuleService.prepareRecord(record, $scope.module, $scope.recordState);

				var recordCopy = angular.copy($scope.record);

				//gets activity_type_id for saveAndNew
				$scope.activity_type_id = $filter('filter')(activityTypes, { id: record.activity_type }, true)[0];

				if (record.activity_type_system || record.activity_type_system === null)
					delete record.activity_type_system;

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
								} else
									result(response.data);
							} else
								result(response.data);

							record.id = response.data.id;
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
								} else
									$scope.submitting = false;
							} else
								$scope.submitting = false;
						});
				} else {
					//encrypted field
					for (var f = 0; f < $scope.module.fields.length; f++) {
						var field = $scope.module.fields[f];
						var showEncryptedInput = false;
						if (field.encrypted && field.encryption_authorized_users_list.length > 0 && record[field.name]) {
							for (var p = 0; p < field.encryption_authorized_users_list.length; p++) {
								var encryrptionPermission = field.encryption_authorized_users_list[p];
								if ($rootScope.user.id === parseInt(encryrptionPermission))
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
								} else if ($scope.module.name === 'quotes') {
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

								record.id = response.data.id;
								components.run('AfterCreate', 'Script', $scope, record);
							})
							.catch(function onError() {
								error(data, status);
								$scope.submitting = false;
							});
					} else {

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
									} else
										result(response.data);
								} else
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
									} else
										$scope.submitting = false;
								} else
									$scope.submitting = false;
							});
					}
				}

				function result(response) {
					$scope.addedUser = false;
					$scope.recordId = response.id;

					components.run('BeforeFormSubmitResult', 'Script', $scope, response);
					$scope.success = function () {
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
						} else {
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
						} else {
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
							if ($scope.type === 'activities') {
								$scope.submitting = false;
								$scope.record['activity_type'] = $filter('filter')(activityTypes, { system_code: $scope.subtype }, true)[0];
								$scope.subtypeNameLang = $filter('translate')('Module.New', { title: $scope.record['activity_type'].label[$rootScope.language] });
							}
						} else {
							if ($rootScope.isEmployee === $scope.module.name || $scope.module.name === 'calisanlar') {
								//Kullanıcı bilgilerinin yer aldığı modalda Detaya Git butonuna basılınca çalışacak fonksiyon.
								$scope.goModuleDetail = function () {
									$state.go('app.moduleDetail', params);
									$scope.userCreateModal.hide();
								};

								//Çalışanlar modülünde User oluşturulurken kullanıcı bilgilerinin yer aldığı Modalın
								//gösterilebilmesi için kullanıcının ModüleForm sayfasında bekletilmesi
								if (!$scope.id && ($scope.record['kullanici_olustur'] || $rootScope.branchAvailable)) {
									$scope.loading = true;
								} else
									$state.go('app.moduleDetail', params);
							} else
								$state.go('app.moduleDetail', params);
												}
						$scope.izinTuruData = null;
					};
					$scope.success();
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

				if ($scope.moduleForm[field.name] && $scope.moduleForm[field.name].$error)
					if ($scope.moduleForm[field.name].$error.unique)
						$scope.moduleForm[field.name].$setValidity('unique', true);

				if ($scope.record.currency)
					$scope.currencySymbol = $scope.record.currency.value || $rootScope.currencySymbol;
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

			ModuleService.getActionButtons($scope.module.id)
				.then(function (actionButtons) {
					$scope.actionButtons = $filter('filter')(actionButtons, function (actionButton) {
						return actionButton.trigger !== 'Detail' && actionButton.trigger !== 'List' && actionButton.trigger !== 'Relation';
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

				} else {
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
				data["instanceId"] = $rootScope.workgroup.tenant_id;

				$scope.record[field.name] = null;
				if (field.data_type === 'document') {
					$scope.uploaderBasic[field.name].queue = [];
				}
				if (field.data_type === 'image') {
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

				$scope.document[field.name] = {};

				var uploader_basic = $scope.uploaderBasic[field.name] = new FileUploader({
					url: 'storage/record_file_upload',
					headers: {
						appId: $rootScope.user.app_id,
						'X-App-Id': $rootScope.user.app_id
					},
					queueLimit: 1
				});

				uploader_basic.onAfterAddingFile = function (item) {

					$scope.fileLoadingCounter++;
					var selectedFile = item.uploader.queue[0].file.name;
					$scope.record[field.name] = selectedFile;
					$scope.document[field.name]['Name'] = item.uploader.queue[0].file.name;
					$scope.document[field.name]['Size'] = item.uploader.queue[0].file.size;
					$scope.document[field.name]['Type'] = item.uploader.queue[0].file.type;

					item.upload();

				};

				uploader_basic.onWhenAddingFileFailed = function (item, filter, options) {
					switch (filter.name) {
						case 'docFilter':
							ngToast.create({
								content: $filter('translate')('Setup.Settings.DocumentTypeError'),
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

				uploader_basic.filters.push({
					name: 'docFilter',
					fn: function (item) {
						var extension = helper.getFileExtension(item.name);
						return true ? (extension === 'txt' || extension === 'docx' || extension === 'pdf' || extension === 'doc' || extension === 'xlsx' || extension === 'xls') : false;
					}
				});

				uploader_basic.filters.push({
					name: 'sizeFilter',
					fn: function (item) {
						return item.size < 5242880;//5 mb
					}
				});

				uploader_basic.onSuccessItem = function (item, response) {
					$scope.document[field.name]['UniqueName'] = response.unique_name;
					$scope.fileLoadingCounter--;
				};

				return uploader_basic;

			};

			$scope.uploaderImage = function (field) {
				$scope.image[field.name] = {};
				var headers = {
					'Authorization': 'Bearer ' + $localStorage.read('access_token'),
					'Accept': 'application/json' /// we have to set accept header to provide consistency between browsers. 
				};

				if ($rootScope.preview) {
					headers['X-App-Id'] = $rootScope.user.app_id;
				} else {
					headers['X-Tenant-Id'] = $rootScope.user.tenant_id;
				}

				var uploader_image = $scope.uploaderImage[field.name] = new FileUploader({
					url: 'storage/record_file_upload',
					headers: headers,
					queueLimit: 1
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
						return true ? (extension === 'jpg' || extension === 'jpeg' || extension === 'png' || extension === 'doc' || extension === 'gif') : false;
					}
				});
				uploader_image.filters.push({
					name: 'sizeFilter',
					fn: function (item) {
						return item.size < 5242880;// 5mb
					}
				});
				uploader_image.onSuccessItem = function (item, response) {
					$scope.image[field.name]['UniqueName'] = response.public_url;
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
				var headersData = [];
				var params = action.parameters.split(',');
				var headers = action.headers.split(',');
				$scope.webhookRequesting = {};

				$scope.webhookRequesting[action.id] = true;

				angular.forEach(params, function (data) {

					var dataObject = data.split('|');
					var parameterName = dataObject[0];
					var moduleName = dataObject[1];
					var fieldName = dataObject[2];

					if (moduleName !== $scope.module.name) {
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
					} else {
						if ($scope.record[fieldName])
							jsonData[parameterName] = $scope.record[fieldName];
						else
							jsonData[parameterName] = null;
					}

				});

				angular.forEach(headers, function (data) {
					var tempHeader = data.split('|');
					var type = tempHeader[0];
					var moduleName = tempHeader[1];
					var key = tempHeader[2];
					var value = tempHeader[3];

					switch (type) {
						case 'module':
							var fieldName = value;
							if (moduleName !== $scope.module.name) {
								if ($scope.record[moduleName])
									headersData[key] = $scope.record[moduleName][fieldName];
								else
									headersData[key] = null;
							} else {
								if ($scope.record[fieldName])
									headersData[key] = $scope.record[fieldName];
								else
									headersData[key] = null;
							}
							break;
						case 'static':
							switch (value) {
								case '{:app:}':
									headersData[key] = $rootScope.user.app_id;
									break;
								case '{:tenant:}':
									headersData[key] = $rootScope.user.tenant_id;
									break;
								case '{:user:}':
									headersData[key] = $rootScope.user.id;
									break;
								default:
									headersData[key] = null;
									break;
							}
							break;
						case 'custom':
							headersData[key] = value;
							break;
						default:
							headersData[key] = null;
							break;
					}
				});

				if (action.method_type === 'post') {

					$http.post(action.url, jsonData, { headers: headersData })
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

				} else if (action.method_type === 'get') {

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

			components.run('AfterFormLoaded', 'script', $scope);

		}
	]);