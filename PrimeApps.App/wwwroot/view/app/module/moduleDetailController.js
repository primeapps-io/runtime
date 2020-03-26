'use strict';

angular.module('primeapps')

    .controller('ModuleDetailController', ['$rootScope', '$scope', 'ngToast', '$filter', 'helper', 'sipHelper', '$location', '$state', '$stateParams', '$q', '$window', '$localStorage', '$cache', 'entityTypes', 'operations', 'config', 'guidEmpty', '$popover', '$timeout', '$modal', '$sce', 'yesNo', 'activityTypes', 'transactionTypes', '$anchorScroll', 'blockUI', 'FileUploader', 'DocumentService', 'ModuleService', '$http', 'components',
        function ($rootScope, $scope, ngToast, $filter, helper, sipHelper, $location, $state, $stateParams, $q, $window, $localStorage, $cache, entityTypes, operations, config, guidEmpty, $popover, $timeout, $modal, $sce, yesNo, activityTypes, transactionTypes, $anchorScroll, blockUI, FileUploader, DocumentService, ModuleService, $http, components) {
            $scope.type = $stateParams.type;
            $scope.id = $location.search().id;
            $scope.parentType = $location.search().ptype;
            $scope.parentId = $location.search().pid;
            $scope.returnParentType = $location.search().rptype;
            $scope.returnTab = $location.search().rtab;
            $scope.previousParentType = $location.search().pptype;
            $scope.previousParentId = $location.search().ppid;
            $scope.previousReturnTab = $location.search().prtab;
            $scope.returnPreviousParentType = $location.search().rpptype;
            $scope.returnPreviousParentId = $location.search().rppid;
            $scope.returnPreviousReturnTab = $location.search().rprtab;
            $scope.back = $location.search().back;
            $scope.module = $filter('filter')($rootScope.modules, {name: $scope.type}, true)[0];
            $scope.operations = operations;
            $scope.hasPermission = helper.hasPermission;
            $scope.hasDocumentsPermission = helper.hasDocumentsPermission;
            $scope.hasFieldDisplayPermission = ModuleService.hasFieldDisplayPermission;
            $scope.hasFieldFullPermission = ModuleService.hasFieldFullPermission;
            $scope.hasSectionDisplayPermission = ModuleService.hasSectionDisplayPermission;
            $scope.hasSectionFullPermission = ModuleService.hasSectionFullPermission;
            $scope.hasActionButtonDisplayPermission = ModuleService.hasActionButtonDisplayPermission;
            $scope.loading = true;
            $scope.pdfUrl = '';
            $scope.isDetail = true;
            $scope.refreshSubModules = {};
            $scope.editLookup = {};
            $scope.tab = 'general';
            $scope.activityTypes = activityTypes;
            $scope.transactionTypes = transactionTypes;
            $scope.lookupUser = helper.lookupUser;
            $scope.isActive = {};
            $scope.waitingForApproval = false;
            $scope.isProcessRecord = false;
            $scope.isApproved = false;
            $scope.isRejectedRequest = false;
            $scope.appId = $rootScope.user.app_id;
            $scope.hasManuelProcess = false;
            $scope.manuelApproveRequest = false;
            $scope.freeze = $location.search().freeze;
            $scope.currentModuleProcess = null;
            $scope.customHide = false;
            $scope.googleMapsApiKey = googleMapsApiKey;
            $scope.currentSectionComponentsTemplate = currentSectionComponentsTemplate;
            $scope.scriptRunning = {};
            $scope.pageBlockUI = blockUI.instances.get('pageBlockUI');
            $scope.actionButtonDisabled = false;

            if (!$scope.module) {
                ngToast.create({content: $filter('translate')('Common.NotFound'), className: 'warning'});
                $state.go('app.dashboard');
                return;
            }

            if (!$scope.id) {
                ngToast.create({content: $filter('translate')('Common.NotFound'), className: 'warning'});
                $state.go('app.dashboard');
                return;
            }

            components.run('BeforeDetailLoaded', 'script', $scope);
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

            $scope.trustAsHtml = function (value) {
                return $sce.trustAsHtml(value);
            };

            $scope.dropdownFields = $filter('filter')($scope.module.fields, {data_type: 'lookup', show_as_dropdown: true}, true);
            $scope.dropdownFieldDatas = {};
            for (var i = 0; i < $scope.dropdownFields.length; i++) {
                $scope.dropdownFieldDatas[$scope.dropdownFields[i].name] = [];
            }

            if ($scope.module.detail_view_type) {
                if ($scope.module.detail_view_type != 'flat')
                    $scope.tabconfig = true; else $scope.tabconfig = false;
            }
            else {
                if ($rootScope.detailViewType != 'flat')
                    $scope.tabconfig = true; else $scope.tabconfig = false;
            }

            if ($scope.returnPreviousParentType && $scope.returnPreviousParentId != $scope.id) {
                $scope.parentType = angular.copy($scope.returnPreviousParentType);
                $scope.parentId = angular.copy($scope.returnPreviousParentId);
                $scope.returnTab = angular.copy($scope.returnPreviousReturnTab);
            }

            var setScrollHeight = function () {
                if ($scope.tabconfig)
                    $scope.scrollHeightTab = {height: ($window.innerHeight - 255) + 'px'};
                else
                    $scope.scrollHeight = {height: ($window.innerHeight - 165) + 'px'};
            };

            setScrollHeight();

            angular.element($window).on('resize', function () {
                $timeout(function () {
                    setScrollHeight();
                });
            });

            $scope.primaryField = $filter('filter')($scope.module.fields, {primary: true})[0];
            $scope.currentUser = ModuleService.processUser($rootScope.user);
            $scope.currentDayMin = helper.getCurrentDateMin().toISOString();
            $scope.currentDayMax = helper.getCurrentDateMax().toISOString();
            $scope.currentHour = helper.floorMinutes(new Date());
            $scope.entityTypes = entityTypes;
            $scope.guidEmpty = guidEmpty;
            $scope.relatedToField = $filter('filter')($scope.module.fields, {name: 'related_to'}, true)[0];
            $scope.record = {};
            $scope.allFields = [];
            $scope.showEditor = false;
            $scope.isAdmin = $rootScope.user.profile.has_admin_rights;

            angular.forEach($scope.module.fields, function (field) {
                if (ModuleService.hasFieldDisplayPermission(field))
                    $scope.allFields.push(angular.copy(field));
            });

            //current process
            for (var j = 0; j < $rootScope.approvalProcesses.length; j++) {
                var currentProcess = $rootScope.approvalProcesses[j];
                if (currentProcess.module_id === $scope.module.id)
                    $scope.currentModuleProcess = currentProcess;
            }

            if ($scope.currentModuleProcess) {
                var profileIds = $scope.currentModuleProcess.profiles.split(',');
                $scope.hasProcessEditPermission = false;
                for (var i = 0; i < profileIds.length; i++) {
                    if (profileIds[i] === $rootScope.user.profile.id.toString())
                        $scope.hasProcessEditPermission = true;
                }
            }

            //Get All Record Count in Related Modules
            if ($scope.module.relations) {
                angular.forEach($scope.module.relations, function (relation) {
                    if (relation.deleted)
                        return;
                    var relatedModule = $filter('filter')($rootScope.modules, {name: relation.related_module}, true)[0];
                    var filterRequest = {};
                    //Get relation count for activities for one to many
                    if ((relation.related_module === 'activities' || relation.related_module === 'mails') && relation.relation_type !== 'many_to_many') {
                        filterRequest = {
                            fields: ['total_count()'],
                            filters: [
                                {field: relation.relation_field, operator: 'equals', value: $scope.id, no: 1},
                                {
                                    field: 'related_module',
                                    operator: 'is',
                                    value: $scope.module["label_" + $rootScope.user.tenant_language + "_singular"],
                                    no: 1
                                }],
                            limit: 1,
                            offset: 0
                        };
                    }
                    else if (relation.relation_type === 'many_to_many') {
                        if (!relatedModule) return;
                        var fieldPrimaryManyToMany = $filter('filter')(relatedModule.fields, {primary: true}, true)[0];
                        var fieldNamePrimaryManyToMany = relation.related_module + '_id.' + relation.related_module + '.' + fieldPrimaryManyToMany.name;
                        filterRequest = {
                            fields: ['total_count()', fieldNamePrimaryManyToMany],
                            filters: [{field: $scope.module.name + '_id', operator: 'equals', value: $scope.id, no: 1}],
                            limit: 1,
                            offset: 0,
                            many_to_many: $scope.module.name
                        };
                    }
                    else
                        filterRequest = {
                            fields: ['total_count()'],
                            filters: [{field: relation.relation_field, operator: 'equals', value: $scope.id, no: 1}],
                            limit: 1,
                            offset: 0
                        };
                    ModuleService.findRecords(relation.related_module, filterRequest)
                        .then(function (response) {
                            var data = response.data;
                            if (data[0] && data[0].total_count)
                                relation.total = data[0].total_count;
                            else
                                delete relation.total;
                            if (relation.detail_view_type === 'flat') {
                                $scope.changeTab(relation);
                                if ($scope.returnParentType == relation.id) {
                                    $scope.activeType = 'tab';
                                    $scope.tab = 'general';
                                }
                            }

                        }).then(function () {
                        $scope.isActive[$state.params.rptype] = true;
                    });
                });
            }

            if ($scope.parentType) {
                if ($scope.type === 'activities' || $scope.type === 'mails' || $scope.many) {
                    $scope.parentModule = $scope.parentType;
                }
                else {
                    var parentTypeField = $filter('filter')($scope.module.fields, {name: $scope.parentType}, true)[0];

                    if (parentTypeField) {
                        $scope.parentModule = parentTypeField.lookup_type;
                    }
                    else {
                        $scope.parentModule = $scope.parentType;
                        $scope.returnType = 'general';
                    }
                }

                if (!$scope.previousParentType) {
                    $scope.previousParentType = angular.copy($scope.parentType);
                    $scope.previousParentId = angular.copy($scope.parentId);

                    if (!$scope.previousReturnTab && $scope.returnTab) {
                        $scope.previousReturnTab = angular.copy($scope.returnTab);
                    }
                }
            }

            if ($scope.module.default_tab)
                $scope.tab = $scope.module.default_tab;

            if ($scope.returnParentType) {
                $scope.tab = $scope.returnParentType;

                if (!$scope.tabconfig) {
                    $scope.isActive[$scope.returnParentType] = true;
                    $location.hash('relation' + $scope.returnParentType);
                    $anchorScroll();
                }
            }

            var setRecordValidationData = function () {
                if ($scope.module.name === 'izinler' &&
                    (
                        ($scope.hasManuelProcess && ($scope.record.owner.id === $scope.currentUser.id || $scope.hasProcessRights) && !$scope.record.process_id) ||
                        ($scope.record.process_status === 3 && $scope.record.owner.id === $scope.currentUser.id && !$scope.waitingForApproval)
                    ) && $scope.record['izin_turu']) {
                    var startOf = moment().date(1).month(1).year(moment().year()).format('YYYY-MM-DD');

                    $scope.record['goreve_baslama_tarihi'] = $scope.record['calisan']['ise_baslama_tarihi'];
                    $scope.record['izin_turu_data'] = $scope.record['izin_turu'];
                    $scope.record['dogum_tarihi'] = $scope.record['calisan']['dogum_tarihi'];
                    $scope.record['calisan_data'] = $scope.record['calisan'];

                    //Yıllık izin seçilmiş ise işe bağladığı tarih dikkate alınarak 1 yıllık kullandığı izinleri çekmek için tarih hesaplanıyor.
                    if ($scope.record['izin_turu_data']['yillik_izin']) {
                        var jobStart = moment($scope.record['calisan_data']['ise_baslama_tarihi']);
                        var jobDay = jobStart.get('date');
                        var jobMonth = jobStart.get('month');
                        var currentYear = moment().get('year');

                        var currentDate = moment().date(jobDay).month(jobMonth).year(currentYear).format('YYYY-MM-DD');

                        if (moment(currentDate).isAfter(moment().format('YYYY-MM-DD'))) {
                            currentYear -= 1;
                        }

                        startOf = moment().date(jobDay).month(jobMonth).year(currentYear).format('YYYY-MM-DD');
                    }

                    if ($scope.record['izin_turu_data']['her_ay_yenilenir']) {
                        startOf = moment().date(1).month(moment().month()).year(moment().year()).format('YYYY-MM-DD');
                    }

                    var filterRequest = {
                        fields: ['hesaplanan_alinacak_toplam_izin', 'baslangic_tarihi', 'bitis_tarihi', 'izin_turu', 'process.process_requests.process_status', 'system_code'],
                        filters: [
                            {field: 'calisan', operator: 'equals', value: $scope.record['calisan'].id, no: 1},
                            {field: 'baslangic_tarihi', operator: 'greater_equal', value: startOf, no: 2},
                            {field: 'deleted', operator: 'equals', value: false, no: 3},
                            {field: 'process.process_requests.process_status', operator: 'not_equal', value: 3, no: 4}
                        ],
                        limit: 999999
                    };

                    ModuleService.findRecords('izinler', filterRequest)
                        .then(function (response) {
                            if (response.data.length > 0) {
                                $scope.record['alinan_izinler'] = response.data;
                            }
                        });
                }
            };

            $scope.picklistFilter = function (param) {
                return function (item) {
                    $scope.componentFilter = {};
                    $scope.componentFilter.item = item;
                    $scope.componentFilter.result = true;
                    components.run('PicklistFilter', 'Script', $scope);
                    return !item.hidden && !item.inactive && $scope.componentFilter.result;
                };
            };

            var loadRecord = function () {
                ModuleService.getPicklists($scope.module)
                    .then(function (picklists) {
                        $scope.picklistsModule = picklists;

                        ModuleService.getRecord($scope.module.name, $scope.id)
                            .then(function (recordData) {
                                if (Object.keys(recordData.data).length === 0) {
                                    ngToast.create({content: $filter('translate')('Common.Forbidden'), className: 'warning'});
                                    $state.go('app.dashboard');
                                }
                                if ($scope.module.name != 'activities') {
                                    //If Set default value for picklist field, we set dependency value
                                    angular.forEach($scope.module.fields, function (field) {
                                        ModuleService.setDependency(field, $scope.module, $scope.record, $scope.picklistsModule);
                                        if (field.default_value && field.data_type == 'picklist') {
                                            $scope.record[field.name] = $filter('filter')($scope.picklistsModule[field.picklist_id], {id: field.default_value})[0]
                                            $scope.fieldValueChange(field);
                                        }
                                    });
                                }

                                var record = ModuleService.processRecordSingle(recordData.data, $scope.module, $scope.picklistsModule);

                                for (var i = 0; i < $rootScope.approvalProcesses.length; i++) {
                                    var currentProcess = $rootScope.approvalProcesses[i];
                                    if (currentProcess.module_id === $scope.module.id && currentProcess.trigger_time === 'manuel')
                                        $scope.hasManuelProcess = true;

                                    if (currentProcess.module_id === $scope.module.id)
                                        $scope.currentModuleProcess = currentProcess;
                                }

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
                                        field.show_lock = true;
                                    else
                                        field.show_lock = false;
                                }

                                //Approval Process
                                if (record.process_status) {
                                    if (record.process_status === 2)
                                        $scope.isApproved = true;

                                    if (record.process_status === 1 || record.process_status === 2 || (record.process_status === 3 && record.created_by.id != $scope.currentUser.id))
                                        record.freeze = true;

                                    ModuleService.getProcess(record.process_id)
                                        .then(function (response) {
                                            var approvers = response.data.approvers;

                                            if (approvers.length > 0) {
                                                var currentApprover = $filter('filter')(approvers, {id: $scope.currentUser.id}, true)[0];

                                                if (!currentApprover && record.process_status !== 3)
                                                    $scope.waitingForApproval = true;

                                                if (currentApprover) {
                                                    if (currentApprover.order === record.process_status_order)
                                                        $scope.isApprovalRecord = true;
                                                    else {
                                                        if (record.process_status !== 3)
                                                            $scope.waitingForApproval = true;
                                                    }
                                                }
                                            }
                                            else {
                                                if (record.process_status_order === 1) {
                                                    var customApprover = record.custom_approver;
                                                    if (customApprover === $rootScope.user.email)
                                                        $scope.isApprovalRecord = true;
                                                    else if (customApprover !== $rootScope.user.email && record.process_status !== 3) {
                                                        $scope.waitingForApproval = true;
                                                    }
                                                }
                                                else if (record.process_status_order === 2) {
                                                    var customApprover2 = record.custom_approver_2;
                                                    if (customApprover2 === $rootScope.user.email)
                                                        $scope.isApprovalRecord = true;
                                                    else if (customApprover2 !== $rootScope.user.email && record.process_status !== 3) {
                                                        $scope.waitingForApproval = true;
                                                    }
                                                }
                                                else if (record.process_status_order === 3) {
                                                    var customApprover3 = record.custom_approver_3;
                                                    if (customApprover3 === $rootScope.user.email)
                                                        $scope.isApprovalRecord = true;
                                                    else if (customApprover3 !== $rootScope.user.email && record.process_status !== 3) {
                                                        $scope.waitingForApproval = true;
                                                    }
                                                }
                                                else if (record.process_status_order === 4) {
                                                    var customApprover4 = record.custom_approver_4;
                                                    if (customApprover4 === $rootScope.user.email)
                                                        $scope.isApprovalRecord = true;
                                                    else if (customApprover4 !== $rootScope.user.email && record.process_status !== 3) {
                                                        $scope.waitingForApproval = true;
                                                    }
                                                }
                                                else if (record.process_status_order === 5) {
                                                    var customApprover5 = record.custom_approver_5;
                                                    if (customApprover5 === $rootScope.user.email)
                                                        $scope.isApprovalRecord = true;
                                                    else if (customApprover5 !== $rootScope.user.email && record.process_status !== 3) {
                                                        $scope.waitingForApproval = true;
                                                    }
                                                }

                                            }

                                            if (record.operation_type === 0 && record.process_status === 2) {
                                                for (var i = 0; i < response.data.operations_array.length; i++) {
                                                    var process = response.data.operations_array[i];

                                                    if (process === "update")
                                                        record.freeze = false;
                                                }
                                            }

                                            if (record.operation_type === 1 && record.process_status === 2) {
                                                record.freeze = false;
                                            }

                                            if ($scope.module.name === 'izinler')
                                                setRecordValidationData();

                                        });
                                }

                                if ($scope.module.dependencies.length > 0) {
                                    var freezeDependencies = $filter('filter')($scope.module.dependencies, {dependency_type: 'freeze', deleted: false}, true);
                                    angular.forEach(freezeDependencies, function (dependencie) {
                                        var freezeFields = $filter('filter')($scope.module.fields, {name: dependencie.parent_field}, true);
                                        angular.forEach(freezeFields, function (field) {
                                            angular.forEach(dependencie.values_array, function (value) {
                                                if (record[field.name] && (value == record[field.name] || value == record[field.name].id))
                                                    record.freeze = true;
                                            });
                                        });
                                    });
                                }

                                if ($scope.freeze)
                                    record.freeze = true;

                                if ($scope.currentModuleProcess !== null) {
                                    if ($scope.currentModuleProcess.profile_list && $scope.currentModuleProcess.profile_list.length > 0) {
                                        for (var k = 0; k < $scope.currentModuleProcess.profile_list.length; k++) {
                                            var profile = $scope.currentModuleProcess.profile_list[k];
                                            if (parseInt(profile) === $rootScope.user.profile.id)
                                                record.freeze = false;
                                        }
                                    }

                                    if ($rootScope.user.profile.has_admin_rights)
                                        record.freeze = false;

                                }

                                ModuleService.formatRecordFieldValues(angular.copy(recordData.data), $scope.module, $scope.picklistsModule);
                                $scope.title = $scope.primaryField.valueFormatted;

                                var success = function (record) {
                                    $scope.record = record;
                                    $scope.recordState = angular.copy($scope.record);
                                    ModuleService.setDisplayDependency($scope.module, $scope.record);

                                    if ($scope.record.currency)
                                        $scope.currencySymbol = $scope.record.currency.value || $rootScope.currencySymbol;

                                    components.run('AfterRecordLoaded', 'Script', $scope, $scope.record);

                                    if ($scope.currentModuleProcess && $scope.currentModuleProcess.profile_list) {
                                        if ($scope.currentModuleProcess.profile_list.indexOf($rootScope.user.profile.ID.toString()) > -1)
                                            $scope.hasProcessRights = true;
                                        else
                                            $scope.hasProcessRights = false;
                                    }

                                    if ($scope.module.name === 'izinler')
                                        setRecordValidationData();
                                };

                                //generate action buttons
                                ModuleService.getActionButtons($scope.module.id)
                                    .then(function (actionButtons) {
                                        $scope.actionButtons = $filter('filter')(actionButtons, function (actionButton) {
                                            return actionButton.trigger !== 'List' && actionButton.trigger !== 'Form' && actionButton.trigger !== 'Relation';
                                        }, true);
                                        //dependency control for action buttons
                                        angular.forEach($scope.actionButtons, function (item) {
                                            item.isShown = false;
                                            if (item.dependent_field) {
                                                if (record[item.dependent_field] && record[item.dependent_field].labelStr == item.dependent)
                                                    item.isShown = true;
                                            }
                                            else {
                                                item.isShown = true;
                                            }
                                        });
                                    });

                                if ($scope.relatedToField && record['related_to'] && record[$scope.relatedToField.lookup_relation]) {
                                    var lookupModule = $filter('filter')($rootScope.modules, {id: record[$scope.relatedToField.lookup_relation].id - 900000}, true)[0];
                                    var lookupModulePrimaryField = $filter('filter')(lookupModule.fields, {primary: true}, true)[0];

                                    ModuleService.getRecord(lookupModule.name, record['related_to'], true)
                                        .then(function (relatedRecord) {
                                            relatedRecord = relatedRecord.data;
                                            relatedRecord.primary_value = relatedRecord[lookupModulePrimaryField.name];
                                            record['related_to'] = relatedRecord;

                                            success(record);
                                            $scope.loading = false;
                                            $scope.refreshing = false;
                                            $scope.pageBlockUI.stop();
                                        })
                                        .catch(function (data) {
                                            if (data.status === 404) {
                                                record['related_to'] = null;
                                                success(record);
                                            }

                                            $scope.loading = false;
                                            $scope.refreshing = false;
                                            $scope.pageBlockUI.stop();
                                        });
                                }
                                else {
                                    success(record);
                                    $scope.loading = false;
                                    $scope.refreshing = false;
                                    $scope.pageBlockUI.stop();
                                }
                            })
                            .catch(function () {
                                $scope.loading = false;
                                $scope.refreshing = false;
                                $scope.pageBlockUI.stop();
                            });

                        $scope.yesNo = ModuleService.getYesNo(yesNo);
                    });
            };

            loadRecord();

            $scope.refreshSubModules[$scope.type] = {};
            $scope.refresh = function () {
                $scope.refreshing = true;
                $scope.pageBlockUI.start();
                loadRecord();
                angular.forEach($scope.module.relations, function (item) {
                    $scope.refreshSubModules[$scope.type][item.related_module] = $scope.getCurrentTime();
                });
            };

            $scope.changeTab = function (relatedModule) {
                if (relatedModule.detail_view_type != 'flat') {
                    $scope.tab = angular.copy(relatedModule.id.toString());
                    $scope.activeType = 'flat'
                }
                else {
                    $scope.activeType = 'tab';
                }

                $scope.isActive = [];
                $scope.isActive[relatedModule.id] = true;

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

                    var relationModule = $filter('filter')($rootScope.modules, {name: $scope.record.related_module.value}, true)[0];

                    if (!relationModule) {
                        $scope.$broadcast('angucomplete-alt:clearInput', $scope.currentLookupField.name);
                        return $q.defer().promise;
                    }

                    $scope.currentLookupField.lookupModulePrimaryField = $filter('filter')(relationModule.fields, {primary: true}, true)[0];
                }

                if (($scope.currentLookupField.lookupModulePrimaryField.data_type === 'number' || $scope.currentLookupField.lookupModulePrimaryField.data_type === 'number_auto') && isNaN(parseFloat(searchTerm))) {
                    $scope.$broadcast('angucomplete-alt:clearInput', $scope.currentLookupField.name);
                    return $q.defer().promise;
                }

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

            $scope.setCurrentLookupField = function (field) {
                $scope.currentLookupField = field;
            };

            $scope.validate = function (data, dataType, validation) {
                if (!validation)
                    return;

                if (validation.required && !data)
                    return $filter('translate')('Module.Required');

                if (dataType === 'lookup' && data && typeof data != 'object')
                    return $filter('translate')('Module.RequiredAutoCompleteError');

                if (dataType === 'email' && data) {
                    var email = /^[a-z0-9!#$%&'*+\/=?^_`{|}~.-]+@[a-z0-9]([a-z0-9-]*[a-z0-9])?(\.[a-z0-9]([a-z0-9-]*[a-z0-9])?)*$/i;

                    if (!email.test(data))
                        return $filter('translate')('Module.EmailInvalid');
                }

                if (dataType === 'url' && data) {
                    var url = /(http|https|file|ftp):\/\/(\w+:{0,1}\w*@)?(\S+)(:[0-9]+)?(\/|\/([\w#!:.?+=&%@!\-\/]))?/;

                    if (!url.test(data))
                        return $filter('translate')('Module.UrlInvalid');
                }

                if (dataType === "checkbox") {
                    if (data === undefined) {
                        data = false;
                    }
                }

                if (validation.min && data) {
                    var dataParsedMin = parseFloat(data);
                    var min = parseFloat(validation.min);

                    if (dataParsedMin < min)
                        return $filter('translate')('Module.MinError');
                }

                if (validation.min && data) {
                    var dataParsedMax = parseFloat(data);
                    var max = parseFloat(validation.max);

                    if (dataParsedMax < max)
                        return $filter('translate')('Module.MaxError');
                }

                if (validation.min_length && data && data.length < validation.min_length)
                    return $filter('translate')('Module.MinLengthError');

                return true;
            };

            $scope.update = function () {
                $scope.updating = true;
                var record = ModuleService.prepareRecord($scope.record, $scope.module, $scope.recordState);

                if (record.activity_type_system)
                    delete record.activity_type_system;
                
                //components.run(componentTypes.place.beforeUpdate, componentTypes.type.script, $scope.module, record);
                ModuleService.updateRecord($scope.module.name, record)
                    .then(function (response) {

                        var result = function () {
                            $scope.recordState = angular.copy($scope.record);

                            clearCache();

                            //controls actionbutton dependancy
                            angular.forEach($scope.actionButtons, function (item) {
                                item.isShown = false;
                                if (item.dependent_field) {
                                    if (response[item.dependent_field] && response[item.dependent_field] == item.dependent)
                                        item.isShown = true;
                                }
                                else {
                                    item.isShown = true;
                                }
                            });
                        };

                        //if approval process exist for this module and crud condition
                        var moduleProcesses = $filter('filter')($rootScope.approvalProcesses, {module_id: $scope.module.id}, true);
                        if (moduleProcesses) {
                            var isProcessUpdate = false;
                            for (var i = 0; i < moduleProcesses.length; i++) {
                                if ((moduleProcesses[i].user_id === 0 || moduleProcesses[i].user_id === $scope.currentUser.id) && moduleProcesses[i].operations.indexOf('update') > -1)
                                    isProcessUpdate = true;
                            }
                            if (isProcessUpdate && $scope.record.process_status !== 3) {
                                $scope.record.freeze = true;
                                setTimeout(function () {
                                    ModuleService.getRecord($scope.module.name, $scope.record.id)
                                        .then(function (recordData) {
                                            var record = ModuleService.processRecordSingle(recordData.data, $scope.module, $scope.picklistsModule);
                                            //Approval Process
                                            if (record.process_status) {
                                                if (record.process_status === 2)
                                                    $scope.isApproved = true;
                                                else if (record.process_status === 1)
                                                    $scope.isApproved = false;

                                                if (record.process_status === 1 || record.process_status === 2 || (record.process_status === 3 && record.created_by.id != $scope.currentUser.id))
                                                    record.freeze = true;

                                                ModuleService.getProcess(record.process_id)
                                                    .then(function (response) {
                                                        var approvers = response.data.approvers;

                                                        if (approvers.length > 0) {
                                                            var currentApprover = $filter('filter')(approvers, {id: $scope.currentUser.id}, true)[0];

                                                            if (!currentApprover && record.process_status !== 3)
                                                                $scope.waitingForApproval = true;

                                                            if (currentApprover) {
                                                                if (currentApprover.order === record.process_status_order)
                                                                    $scope.isApprovalRecord = true;
                                                                else {
                                                                    if (record.process_status !== 3)
                                                                        $scope.waitingForApproval = true;
                                                                }
                                                            }
                                                        }
                                                        else {
                                                            if (record.process_status_order === 1) {
                                                                var customApprover = record.custom_approver;
                                                                if (customApprover === $rootScope.user.email)
                                                                    $scope.isApprovalRecord = true;
                                                                else if (customApprover !== $rootScope.user.email && record.process_status !== 3) {
                                                                    $scope.waitingForApproval = true;
                                                                }
                                                            }
                                                            else if (record.process_status_order === 2) {
                                                                var customApprover2 = record.custom_approver_2;
                                                                if (customApprover2 === $rootScope.user.email)
                                                                    $scope.isApprovalRecord = true;
                                                                else if (customApprover2 !== $rootScope.user.email && record.process_status !== 3) {
                                                                    $scope.waitingForApproval = true;
                                                                }
                                                            }
                                                            else if (record.process_status_order === 3) {
                                                                var customApprover3 = record.custom_approver_3;
                                                                if (customApprover3 === $rootScope.user.email)
                                                                    $scope.isApprovalRecord = true;
                                                                else if (customApprover3 !== $rootScope.user.email && record.process_status !== 3) {
                                                                    $scope.waitingForApproval = true;
                                                                }
                                                            }
                                                            else if (record.process_status_order === 4) {
                                                                var customApprover4 = record.custom_approver_4;
                                                                if (customApprover4 === $rootScope.user.email)
                                                                    $scope.isApprovalRecord = true;
                                                                else if (customApprover4 !== $rootScope.user.email && record.process_status !== 3) {
                                                                    $scope.waitingForApproval = true;
                                                                }
                                                            }
                                                            else if (record.process_status_order === 5) {
                                                                var customApprover5 = record.custom_approver_5;
                                                                if (customApprover5 === $rootScope.user.email)
                                                                    $scope.isApprovalRecord = true;
                                                                else if (customApprover5 !== $rootScope.user.email && record.process_status !== 3) {
                                                                    $scope.waitingForApproval = true;
                                                                }
                                                            }

                                                        }

                                                        if (record.operation_type === 0 && record.process_status === 2) {
                                                            for (var i = 0; i < response.data.operations_array.length; i++) {
                                                                var process = response.data.operations_array[i];

                                                                if (process === "update")
                                                                    record.freeze = false;
                                                            }
                                                        }

                                                        if (record.operation_type === 1 && record.process_status === 2) {
                                                            record.freeze = false;
                                                        }

                                                    });
                                            }

                                            if ($scope.module.dependencies.length > 0) {
                                                var freezeDependencies = $filter('filter')($scope.module.dependencies, {dependency_type: 'freeze'}, true);
                                                angular.forEach(freezeDependencies, function (dependencie) {
                                                    var freezeFields = $filter('filter')($scope.module.fields, {name: dependencie.parent_field}, true);
                                                    angular.forEach(freezeFields, function (field) {
                                                        angular.forEach(dependencie.values_array, function (value) {
                                                            if (record[field.name] && (value == record[field.name] || value == record[field.name].id))
                                                                record.freeze = true;
                                                        });
                                                    });
                                                });
                                            }

                                            if ($scope.currentModuleProcess !== null) {
                                                if ($scope.currentModuleProcess.profile_list && $scope.currentModuleProcess.profile_list.length > 0) {
                                                    for (var k = 0; k < $scope.currentModuleProcess.profile_list.length; k++) {
                                                        var profile = $scope.currentModuleProcess.profile_list[k];
                                                        if (parseInt(profile) === $rootScope.user.profile.id)
                                                            record.freeze = false;
                                                    }
                                                }

                                            }

                                            ModuleService.formatRecordFieldValues(angular.copy(recordData.data), $scope.module, $scope.picklistsModule);
                                            $scope.title = $scope.primaryField.valueFormatted;

                                            var success = function (record) {
                                                $scope.record = record;
                                                $scope.recordState = angular.copy($scope.record);
                                                ModuleService.setDisplayDependency($scope.module, $scope.record);

                                                if ($scope.record.currency)
                                                    $scope.currencySymbol = $scope.record.currency.value || $rootScope.currencySymbol;

                                                getProducts($scope.type);
                                            };

                                            //controls actionbutton dependancy
                                            angular.forEach($scope.actionButtons, function (item) {
                                                item.isShown = false;
                                                if (item.dependent_field) {
                                                    if (response[item.dependent_field] && response[item.dependent_field] == item.dependent)
                                                        item.isShown = true;
                                                }
                                                else {
                                                    item.isShown = true;
                                                }
                                            });

                                            if ($scope.relatedToField && record['related_to']) {
                                                var lookupModule = $filter('filter')($rootScope.modules, {id: record[$scope.relatedToField.lookup_relation].id - 900000}, true)[0];
                                                var lookupModulePrimaryField = $filter('filter')(lookupModule.fields, {primary: true}, true)[0];

                                                ModuleService.getRecord(lookupModule.name, record['related_to'], true)
                                                    .then(function (relatedRecord) {
                                                        relatedRecord = relatedRecord.data;
                                                        relatedRecord.primary_value = relatedRecord[lookupModulePrimaryField.name];
                                                        record['related_to'] = relatedRecord;

                                                        success(record);
                                                        $scope.updating = false;
                                                    })
                                                    .catch(function (data) {
                                                        if (data.status === 404) {
                                                            record['related_to'] = null;
                                                            success(record);
                                                        }

                                                        $scope.updating = false;
                                                    });
                                            }
                                            else {
                                                success(record);
                                                $scope.updating = false;
                                            }
                                        })
                                        .catch(function () {
                                            $scope.updating = false;
                                        });
                                }, 2000);
                            }
                            else
                                result();
                        }
                        else
                            result();

                        //components.run(componentTypes.place.afterUpdate, componentTypes.type.script, $scope.module, record);
                    })
                    .catch(function (data, status) {
                        if (status === 409) {
                            ngToast.create({content: $filter('translate')('Module.UniqueError'), className: 'danger'});
                            $scope.record[data.field] = $scope.recordState[data.field];
                        }
                    });
            };

            $scope.delete = function () {
                $scope.executeCode = false;
                components.run('BeforeDelete', 'Script', $scope, $scope.record);

                if ($scope.executeCode)
                    return;

                ModuleService.deleteRecord($scope.module.name, $scope.record.id)
                    .then(function () {
                        components.run('AfterDelete', 'Script', $scope, $scope.record);
                        clearCache();

                        if ($scope.parentId) {
                            $state.go('app.moduleDetail', {type: $scope.parentModule, id: $scope.parentId});
                            return;
                        }

                        $state.go('app.moduleList', {type: $scope.type});
                    });
            };

            var clearCache = function () {
                var cacheKey = $scope.module.name + '_' + $scope.module.name;

                if (!$scope.parentId) {
                    $cache.remove(cacheKey);
                }
                else {
                    cacheKey = (!$scope.relatedToField ? $scope.parentType : 'related_to') + $scope.parentId + '_' + $scope.module.name;
                    var parentCacheKey = $scope.parentType + '_' + $scope.parentType;
                    $cache.remove(cacheKey);
                    $cache.remove(parentCacheKey);
                }

                if ($rootScope.activePages && $rootScope.activePages[$scope.module.name])
                    $rootScope.activePages[$scope.module.name] = null;

                if ($scope.module.display_calendar || $scope.module.name === 'activities')
                    $cache.remove('calendar_events');
            };

            $scope.calculate = function (field) {
                ModuleService.calculate(field, $scope.module, $scope.record);
            };

            $scope.fieldValueChange = function (field) {
                ModuleService.setDependency(field, $scope.module, $scope.record, $scope.picklistsModule);

                if ($scope.record.currency)
                    $scope.currencySymbol = $scope.record.currency.value || $rootScope.currencySymbol;
            };

            $scope.reformatFieldValues = function () {
                ModuleService.formatRecordFieldValues($scope.record, $scope.module, $scope.picklistsModule);
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
                $scope.formModalShown = true;

                $scope.formModal = $scope.formModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/module/moduleFormModal.html',
                    animation: '',
                    backdrop: 'static',
                    show: false,
                    tag: 'formModal'
                });

                $scope.formModal.$promise.then($scope.formModal.show);
            };

            $scope.$on('modal.hide', function (e, target) {
                if (target.$options.tag == 'formModal') {
                    $scope.formModalShown = false;

                    if (typeof $scope.record[$scope.currentLookupField.name] != 'object')
                        $scope.lookupFocusOut();
                    else {
                        delete $scope.editLookup[$scope.currentLookupField.name];
                        $scope.update();
                    }
                }
                if (target.$options.tag == 'relatedModuleInModal')
                    $scope.selectedRelatedModule['relatedModuleInModal'] = false;
            });

            $scope.lookupFocusOut = function () {
                $timeout(function () {
                    if ($scope.formModalShown || $scope.updating)
                        return;

                    $scope.record[$scope.currentLookupField.name] = $scope.recordState[$scope.currentLookupField.name];
                    $scope.$broadcast('angucomplete-alt:changeInput', $scope.currentLookupField.name, $scope.recordState[$scope.currentLookupField.name]);
                    delete $scope.editLookup[$scope.currentLookupField.name]
                }, 200);
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

            $scope.getAttachments();
            
            $scope.showActivityButtons = function () {
                $scope.activityButtonsPopover = $scope.activityButtonsPopover || $popover(angular.element(document.getElementById('activityButtons')), {
                    templateUrl: 'view/common/newactivity.html',
                    placement: 'bottom',
                    autoClose: true,
                    scope: $scope,
                    show: true
                });
            };

            $scope.getCurrentTime = function () {
                var dt = new Date();
                return dt.getTime();
            };

            $scope.openExportDialog = function () {
                $scope.pdfCreating = true;

                var openPdfModal = function () {
                    $scope.PdfModal = $scope.PdfModal || $modal({
                        scope: $scope,
                        templateUrl: 'view/app/module/modulePdfModal.html',
                        animation: '',
                        backdrop: 'static',
                        show: false
                    });

                    $scope.PdfModal.$promise.then($scope.PdfModal.show);
                };

                if ($scope.quoteTemplates) {
                    openPdfModal();
                    $scope.quoteTemplate = $scope.quoteTemplates[0];
                }

                ModuleService.getTemplates($scope.module.name, 'module')
                    .then(function (templateResponse) {
                        if (templateResponse.data.length == 0) {
                            if (!$rootScope.preview)
                                ngToast.create({content: $filter('translate')('Setup.Templates.TemplateNotFound'), className: 'warning'});
                            else
                                ngToast.create({content: $filter('translate')('Setup.Templates.TemplateDefined'), className: 'warning'});

                            $scope.pdfCreating = false;
                        }
                        else {
                            var templateWord = templateResponse.data;
                            $scope.quoteTemplates = $filter('filter')(templateWord, {active: true}, true);
                            $scope.isShownWarning = true;
                            for (var i = 0; i < $scope.quoteTemplates.length; i++) {
                                var quoteTemplate = $scope.quoteTemplates[i];
                                if (quoteTemplate.permissions.length > 0) {
                                    var currentQuoteTemplate = $filter('filter')(quoteTemplate.permissions, {profile_id: $rootScope.user.profile.id}, true)[0];
                                    if (currentQuoteTemplate.type === 'none') {
                                        quoteTemplate.isShown = false;
                                    }
                                    else {
                                        quoteTemplate.isShown = true;
                                    }
                                    if (quoteTemplate.isShown == true) {
                                        $scope.isShownWarning = false;
                                    }
                                }
                                else {
                                    quoteTemplate.isShown = true;
                                    $scope.isShownWarning = false;
                                }
                            }
                            $scope.quoteTemplate = $scope.quoteTemplates[0];
                            $scope.PdfModal = $scope.PdfModal || $modal({
                                scope: $scope,
                                templateUrl: 'view/app/module/modulePdfModal.html',
                                animation: '',
                                backdrop: 'static',
                                show: false
                            });

                            openPdfModal();
                        }
                    })
                    .catch(function () {
                        $scope.pdfCreating = false;
                    });

            };

            $scope.getDownloadUrl = function (format) {
                $window.open("/attach/export?module=" + $scope.type + "&id=" + $scope.id + "&templateId=" + $scope.quoteTemplate.id + "&access_token=" + $localStorage.read('access_token') + '&format=' + format + '&locale=' + $rootScope.locale + '&timezoneOffset=' + new Date().getTimezoneOffset() * -1, "_blank");
                ngToast.create({content: $filter('translate')('Module.ExcelDesktop'), className: 'success'});
            };

            // $scope.getDownloadUrl = function (format) {
            //     var url = config.apiUrl + 'attach/export?module=' + $scope.type + '&id=' + $scope.id + "&templateId=" + $scope.quoteTemplate.id + '&access_token=' + $localStorage.read('access_token') + '&format=' + format + '&locale=' + $rootScope.locale + '&timezoneOffset=' + new Date().getTimezoneOffset() * -1;
            //     $window.open(url, '_blank');
            // };

            $scope.openAddModal = function (relatedModule) {
                $scope.selectedRelatedModule = relatedModule;
                $scope.selectedRelatedModule['relatedModuleInModal'] = true;
                $scope.addModal = $scope.addModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/module/moduleAddModal.html',
                    animation: '',
                    backdrop: 'static',
                    show: false,
                    tag: 'relatedModuleInModal'
                });

                $scope.addModal.$promise.then($scope.addModal.show);
            };

            $scope.showEMailModal = function (notShow, email) {
                if (!$rootScope.system.messaging.SystemEMail && !$rootScope.system.messaging.PersonalEMail) {

                    var emailUrl = "mailto:" + email + "";
                    var myWindow = $window.open(emailUrl, '_blank');
                    myWindow.document.title = 'Ofisim.com';
                    return;
                }

                /*Generates and displays modal form for the mail*/
                $scope.mailModal = $scope.mailModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/email/singleEmailModal.html',
                    backdrop: 'static',
                    show: false
                });

                if (!notShow)
                    $scope.mailModal.$promise.then($scope.mailModal.show);
            };

            $scope.showQuoteEMailModal = function (notShow, email) {
                if (!$rootScope.system.messaging.SystemEMail && !$rootScope.system.messaging.PersonalEMail) {
                    ngToast.create({content: $filter('translate')('EMail.NoProvider'), className: 'warning'});
                    return;
                }

                /*Generates and displays modal form for the mail*/
                $scope.mailModal = $scope.mailModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/email/singleEmailModal.html',
                    backdrop: 'static',
                    show: false
                });

                if (!notShow)
                    $scope.mailModal.$promise.then($scope.mailModal.show);
            };

            $scope.showSingleSMSModal = function () {
                if (!$rootScope.system.messaging.SMS) {
                    ngToast.create({content: $filter('translate')('SMS.NoProvider'), className: 'warning'});
                    return;
                }

                /*Generates and displays modal form for the mail*/
                $scope.smsModal = $scope.smsModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/sms/singleSMSModal.html',
                    backdrop: 'static',
                    show: false
                });

                $scope.smsModal.$promise.then($scope.smsModal.show);
            };

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

            $scope.dropdownHide = function () {
                angular.element(document.getElementsByClassName('dropdown-menu'))[0].click();
                angular.element(document.getElementsByClassName('dropdown-menu'))[1].click();
            };

            $scope.documentDownload = function (fileName, fieldName) {

                $window.location = "/storage/record_file_download?fileName=" + fileName;

            };
            
            $scope.getmoduledownloadurl = function (fileName, fieldName) {
                if (!fileName)
                    return;
                return config.apiUrl + 'Document/download_module_document?module=' + $scope.module.name + '&fileNameExt=' + helper.getFileExtension(fileName) + "&fileName=" + fileName + "&fieldName=" + fieldName + "&recordId=" + $scope.record.id + '&access_token=' + $localStorage.read('access_token');
            };

            // //calls export func from subTable directive
            // $scope.export = function (relatedModule, tableParams) {
            //     $scope.$broadcast('export');
            //     console.log(relatedModule);
            //     console.log(tableParams);
            //
            // };
            //
            // //calls delete selecteds func from subTable directive
            // $scope.deleteSelectedsSubTable = function () {
            //     $scope.$broadcast('deleteSelectedsSubTable');
            // };

            $scope.showHideSipPhone = function (numberToDial) {
                function dialHelper(numberToDial) {
                    var session = $rootScope.sipUser.session;

                    if (!session) {
                        $rootScope.sipUser.numberToDial = numberToDial;

                        if ($rootScope.sipUser.numberToDial.length > 2) {
                            $rootScope.sipUser.lineInfo.PhoneStatus = $filter('translate')('Setup.Phone.Dialing');
                            $rootScope.sipUser.lineInfo.State = "Dialing";
                            $rootScope.sipUser.lineInfo.TalkingNumber = $rootScope.sipUser.numberToDial;
                            sipHelper.dial($rootScope.sipUser.lineInfo.TalkingNumber, false);
                            $rootScope.sipUser.activePhoneScreen = "connectingScreen";
                        }
                        else {
                            ngToast.create({content: $filter('translate')('Phone.NoNumber'), className: 'warning'});
                        }
                    }
                    else {
                        ngToast.create({content: $filter('translate')('Phone.AlreadyInCall'), className: 'warning'});
                    }
                }

                if ($rootScope.sipUser && $rootScope.sipPhone && $rootScope.sipPhone[$rootScope.app]) {
                    $rootScope.sipUser.lineInfo.State = "Dialing";
                    dialHelper(numberToDial);
                    $rootScope.sipPhone.crm.show();
                }
                else {

                    $timeout(function () {
                        if ($rootScope.sipPhone) {
                            $rootScope.sipPhone.crm.show();
                        }
                        else {
                            $rootScope.showSipPhone("call");
                        }
                        dialHelper(numberToDial);
                    }, 1000);
                }
            };
            $scope.showLightBox = function (ImageUrl) {
                $scope.lightBox = true;
                $scope.ImageUrl = ImageUrl;
            };

            $scope.closeLightBox = function () {
                $scope.lightBox = false
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
            //webhook request func for action button
            $scope.webhookRequest = function (action) {
                var action = angular.copy(action);
                var http = new XMLHttpRequest();
                http.onreadystatechange = function () {
                    if (this.readyState == 4) {
                        $timeout(function () {
                            $scope.webhookRequesting[action.id] = false;
                        });

                        if (this.status == 200) {
                            ngToast.create({
                                content: $filter('translate')('Module.ActionButtonWebhookSuccess'),
                                className: 'success'
                            });

                        }
                        else {
                            ngToast.create({
                                content: $filter('translate')('Module.ActionButtonWebhookFail'),
                                className: 'warning'
                            });

                        }

                    }

                };

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

                    if (moduleName != $scope.module.name) {
                        if ($scope.record[moduleName])
                            jsonData[parameterName] = $scope.record[moduleName][fieldName];
                        else
                            jsonData[parameterName] = null;
                    }
                    else {
                        if ($scope.record[fieldName])
                            jsonData[parameterName] = $scope.record[fieldName];
                        else
                            jsonData[parameterName] = null;
                    }

                });

                angular.forEach(headers, function (data) {
                    var newValue = null;
                    var tempHeader = data.split('|');
                    var type = tempHeader[0];
                    var moduleName = tempHeader[1];
                    var key = tempHeader[2];
                    var value = tempHeader[3];
                    var newHeader = {};

                    switch (type) {
                        case 'module':
                            var fieldName = value;
                            if (moduleName != $scope.module.name) {
                                if ($scope.record[moduleName])
                                    newValue = $scope.record[moduleName][fieldName];
                                else
                                    newValue = $scope.record[moduleName][fieldName];
                            }
                            else {
                                if ($scope.record[fieldName])
                                    newValue = $scope.record[fieldName];
                                else
                                    newValue = null;
                            }
                            break;
                        case 'static':
                            switch (value) {
                                case '{:app:}':
                                    newValue = $rootScope.user.app_id;
                                    break;
                                case '{:tenant:}':
                                    newValue = $rootScope.user.tenant_id;
                                    break;
                                case '{:user:}':
                                    newValue = $rootScope.user.id;
                                    break;
                                default:
                                    newValue = null;
                                    break;
                            }
                            break;
                        case 'custom':
                            newValue = value;
                            break;
                        default:
                            newValue = null;
                            break;
                    }

                    newHeader['key'] = key;
                    newHeader['value'] = newValue;
                    headersData.push(newHeader);
                });

                if (action.method_type === 'post') {
                    http.open("POST", action.url, true);
                    if (action.url.indexOf(window.location.host) > -1) {
                        http.setRequestHeader("Authorization", 'Bearer ' + $localStorage.read('access_token'));
                    }
                    http.setRequestHeader("Content-Type", 'application/json');

                    angular.forEach(headersData, function (header) {
                        http.setRequestHeader(header.key, header.value);
                    });

                    http.send(JSON.stringify(jsonData));
                }
                else if (action.method_type === 'get') {

                    var query = "";

                    for (var key in jsonData) {
                        query += key + "=" + jsonData[key] + "&";
                    }
                    if (query.length > 0) {
                        query = query.substring(0, query.length - 1);
                    }

                    if (action.url.indexOf("?") < 0)
                        action.url += "?";
                    else
                        action.url += "&";

                    http.open("GET", action.url + query, true);

                    if (action.url.indexOf(window.location.host) > -1) {
                        http.setRequestHeader("Authorization", 'Bearer ' + $localStorage.read('access_token'));
                    }

                    angular.forEach(headersData, function (header) {
                        http.setRequestHeader(header.key, header.value);
                    });

                    http.send();
                }

            };

            //APPROVAL PROCESS
            $scope.approveProcess = function () {
                //record onaylanırken çalışanın kalan hakkı yeterlimi diye extra kontrol ediliyor.
                /*if ($scope.module.name === 'izinler') {
                 var val = ModuleService.customValidations($scope.module, $scope.record);
                 if (val != "") {
                 ngToast.create({
                 //content: $filter('translate')('Module.SuccessMessage', { title: $scope.module['label_' + $rootScope.language + '_singular'] }),
                 content: val,
                 className: 'warning'
                 });
                 return;
                 }
                 }*/
                $scope.executeCode = false;
                components.run('BeforeApproveProcess', 'Script', $scope);

                if ($scope.executeCode) {
                    return;
                }

                $scope.approving = true;

                ModuleService.approveProcessRequest($scope.record.operation_type, $scope.module.name, $scope.record.id)
                    .then(function (response) {
                        if (response.data.status === "approved") {
                            $scope.isApproved = true;
                            $scope.record.freeze = true;

                            components.run('AfterApproveProcess', 'Script', $scope);
                        }
                        else
                            $scope.record.process_status_order++;

                        $scope.approving = false;
                        $scope.waitingForApproval = true;
                    }).catch(function onError() {
                    $scope.approving = false;
                });
            }

            $scope.rejectProcess = function (message) {
                $scope.executeCode = false;
                components.run('BeforeRejectProcess', 'Script', $scope);

                if ($scope.executeCode) {
                    return;
                }
                $scope.rejecting = true;

                ModuleService.rejectProcessRequest($scope.record.operation_type, $scope.module.name, message, $scope.record.id)
                    .then(function () {
                        $scope.isRejectedRequest = true;
                        $scope.rejecting = false;
                        $scope.record.process_status = 3;
                        $scope.rejectModal.hide();
                    }).catch(function onError() {
                    $scope.rejecting = false;
                });
            }

            $scope.reApproveProcess = function () {
                $scope.reapproving = true;

                ModuleService.send_approval($scope.record.operation_type, $scope.module.name, $scope.record.id)
                    .then(function () {
                        $scope.waitingForApproval = true;
                        $scope.record.freeze = true;
                        $scope.reapproving = false;
                        $scope.record.process_status = 1;
                        $scope.record.process_status_order++;
                    }).catch(function onError() {
                    $scope.reapproving = false;
                });
            }

            $scope.openRejectApprovalModal = function () {
                $scope.rejectModal = $scope.rejectModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/module/rejectProcessModal.html',
                    animation: '',
                    backdrop: 'static',
                    show: false,
                    tag: 'createModal'
                });

                $scope.rejectModal.$promise.then($scope.rejectModal.show);
            };

            $scope.sendToProcessApproval = function () {
                $scope.executeCode = false;

                components.run('BeforeSendToProcessApproval', 'Script', $scope, $scope.record);

                if ($scope.executeCode) {
                    return;
                }

                if ($scope.module.name === 'izinler') {
                    var val = "";
                    /*
                     * skipValidation parametresi component içinde setlenerek validasyonların atlanması sağlanıyor.
                     * #2438 nolu task için geliştirildi.
                     * */
                    if (!$scope.skipValidation)
                        val = ModuleService.customValidations($scope.module, $scope.record);

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

                $scope.manuelApproveRequest = true;
                var request = {
                    "record_id": $scope.record.id,
                    "module_id": $scope.module.id
                };

                ModuleService.sendApprovalManuel(request)
                    .then(function () {
                        components.run('AfterSendToProcessApproval', 'Script', $scope, $scope.record);
                        $scope.hasManuelProcess = false;
                        $scope.waitingForApproval = true;
                        $scope.record.freeze = true;
                        $scope.manuelApproveRequest = false;
                        $scope.record.process_status = 1;
                        $scope.record.process_status_order++;
                    }).catch(function onError(response) {
                    $scope.manuelApproveRequest = false;
                    if (response.status === 400) {
                        if (response.data.model_state && response.data.model_state['filters_not_match'])
                            ngToast.create({
                                content: $filter('translate')('Common.FiltersNotMatched'),
                                className: 'warning'
                            });
                        if (response.data.model_state && response.data.model_state['approver_not_found']) {
                            ngToast.create({
                                content: $filter('translate')('Common.ApproverNotFound'),
                                className: 'warning'
                            });
                        }
                    }
                });
            };

            if ($scope.module.relations.length > 0) {
                $scope.relationActionButtons = [];
                var moduleRelation = $filter('filter')($scope.module.relations, {deleted: false}, true);

                var relationButtonIsShown = function (actionButton) {
                    actionButton.isShown = false;

                    if (actionButton.dependent_field) {
                        if (record[actionButton.dependent_field] && record[actionButton.dependent_field].labelStr == actionButton.dependent)
                            actionButton.isShown = true;
                    }
                    else {
                        actionButton.isShown = true;
                    }
                }

                if (moduleRelation.length > 0) {
                    for (var i = 0; i < moduleRelation.length; i++) {
                        var module = $filter('filter')($rootScope.modules, {name: moduleRelation[i].related_module}, true)[0];

                        ModuleService.getActionButtons(module.id)
                            .then(function (actionButtons) {
                                if (actionButtons.length > 0) {
                                    var relationActionButtons = $filter('filter')(actionButtons, function (actionButton) {
                                        return actionButton.trigger !== 'Detail' && actionButton.trigger !== 'Form' && actionButton.trigger !== 'List';
                                    }, true);

                                    if (relationActionButtons) {
                                        for (var j = 0; j < relationActionButtons.length; j++) {
                                            var relationActionButton = relationActionButtons[j];
                                            relationActionButton.module_name = module.name;
                                            $scope.relationActionButtons.push(relationActionButton);
                                            relationButtonIsShown(relationActionButton);
                                        }
                                    }
                                }
                            });
                    }
                }
            }
            components.run('AfterDetailLoaded', 'script', $scope);

            var markdownLinkRegex = /\[([^\[\]]+)\]\(([^)]+)/;

            $scope.getUrlFieldText = function (fieldValue) {
                if (!fieldValue)
                    return fieldValue;

                var match = fieldValue.match(markdownLinkRegex);

                if (!match)
                    return fieldValue;

                return match[1];
            };

            $scope.getUrlFieldLink = function (fieldValue) {
                if (!fieldValue)
                    return fieldValue;
                
                var match = fieldValue.match(markdownLinkRegex);

                if (!match)
                    return fieldValue;

                return match[2];
            };
        }
    ]);