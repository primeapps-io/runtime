'use strict';

angular.module('primeapps')

    .controller('timesheetModalController', ['$rootScope', '$scope', 'ngToast', '$filter', 'helper', '$location', '$state', '$stateParams', '$q', '$window', '$localStorage', '$cache', 'operations', 'activityTypes', 'ModuleService',
        function ($rootScope, $scope, ngToast, $filter, helper, $location, $state, $stateParams, $q, $window, $localStorage, $cache, operations, activityTypes, ModuleService) {
            $scope.operations = operations;
            $scope.hasPermission = helper.hasPermission;
            $scope.hasFieldFullPermission = ModuleService.hasFieldFullPermission;
            $scope.isExpert = $scope.$parent.isExpert;
            $scope.loadingModal = true;

            var lookupType = $scope.$parent.currentLookupField.lookup_type;

            if (lookupType === 'relation')
                lookupType = $scope.$parent.$parent.$parent.record.related_module.value;

            if (lookupType == null)
                return;

            $scope.moduleModal = $filter('filter')($rootScope.modules, { name: lookupType }, true)[0];

            $scope.dropdownFields = $filter('filter')($scope.moduleModal.fields, { data_type: 'lookup', show_as_dropdown: true }, true);
            $scope.dropdownFieldDatas = {};
            for(var i = 0; i < $scope.dropdownFields.length; i++) {
                $scope.dropdownFieldDatas[$scope.dropdownFields[i].name] = [];
            }

            $scope.setDropdownData = function(field){
                if (field.filters && field.filters.length > 0)
                    $scope.dropdownFieldDatas[field.name] = null;
                else if ($scope.dropdownFieldDatas[field.name] && $scope.dropdownFieldDatas[field.name].length > 0)
                    return;

                $scope.currentLookupFieldModal = field;
                $scope.lookupModal()
                    .then(function(response){
                        $scope.dropdownFieldDatas[field.name] = response;
                    });

            };

            $scope.timesheetArr = [];
            var timesheetFields = ['entry_type', 'charge_type', 'selected_project', 'selected_company', 'place_of_performance', 'comment2', 'please_specify', 'please_specify_country', 'per_diem'];
            angular.forEach(timesheetFields, function (field) {
                $scope.timesheetArr.push($filter('filter')($scope.moduleModal.fields, { name: field }, true)[0]);
            });


            if (!$scope.moduleModal) {
                $scope.formModal.hide();
                ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
                $state.go('app.dashboard');
                return;
            }

            // if (!$scope.hasPermission(lookupType, $scope.operations.modify)) {
            //     $scope.forbidden = true;
            //     $scope.loadingModal = false;
            //     return;
            // }

            $scope.primaryFieldModal = $filter('filter')($scope.moduleModal.fields, { primary_lookup: true })[0];

            if (!$scope.primaryFieldModal)
                $scope.primaryFieldModal = $filter('filter')($scope.moduleModal.fields, { primary: true })[0];

            $scope.currentUser = ModuleService.processUser($rootScope.user);
            $scope.currentDayMin = helper.getCurrentDateMin().toISOString();
            $scope.currentDayMax = helper.getCurrentDateMax().toISOString();
            $scope.currentHour = helper.floorMinutes(new Date());
            $scope.recordModal = {};
            $scope.recordModal.owner = $scope.currentUser;
            $scope.perDiemEntryTypeId = $filter('filter')($scope.$parent.$parent.$parent.workedTimeItems, {system_code: 'per_diem_only'}, true)[0].id;
            $scope.headerTitleCase = $scope.$parent.requestType;

            //if openEditModal func works, requestType becomes 'edit' to specify 'update' request.
            if ($scope.$parent.$parent.$parent.requestType == 'edit') {
                $scope.editChargeType = $scope.$parent.$parent.$parent.editData.chargeType;
                ModuleService.getRecord($scope.moduleModal.name, $scope.$parent.$parent.$parent.editData.id)
                    .then(function (recordData) {
                        $scope.editModuleLoading = false;
                        var record = ModuleService.processRecordSingle(recordData.data, $scope.moduleModal, $scope.picklistsModuleModal);
                        ModuleService.setDisplayDependency($scope.moduleModal, record);
                        angular.forEach($scope.moduleModal.fields, function (field) {
                            ModuleService.setDependency(field, $scope.moduleModal, angular.copy(record), $scope.picklistsModuleModal);
                        });
                        $scope.recordModal = record;
                        $scope.recordModalState = angular.copy(record);
                    })
            }

            if ($scope.$parent.$parent.$parent.primaryValueModal) {
                if ($scope.primaryFieldModal.combination) {
                    var primaryValueParts = $scope.$parent.$parent.$parent.primaryValueModal.split(' ');

                    if (primaryValueParts.length === 1) {
                        $scope.recordModal[$scope.primaryFieldModal.combination.field_1] = primaryValueParts[0];
                    }
                    else if (primaryValueParts.length === 2) {
                        $scope.recordModal[$scope.primaryFieldModal.combination.field_1] = primaryValueParts[0];
                        $scope.recordModal[$scope.primaryFieldModal.combination.field_2] = primaryValueParts[1];
                    }
                    else {
                        $scope.recordModal[$scope.primaryFieldModal.combination.field_1] = '';

                        for (var i = 0; i < primaryValueParts.length; i++) {
                            if (i < primaryValueParts.length - 1)
                                $scope.recordModal[$scope.primaryFieldModal.combination.field_1] = $scope.recordModal[$scope.primaryFieldModal.combination.field_1] + primaryValueParts[i] + ' ';
                        }

                        $scope.recordModal[$scope.primaryFieldModal.combination.field_1] = $scope.recordModal[$scope.primaryFieldModal.combination.field_1].slice(0, -1);
                        $scope.recordModal[$scope.primaryFieldModal.combination.field_2] = primaryValueParts[primaryValueParts.length - 1];
                    }
                }
                else {
                    $scope.recordModal[$scope.primaryFieldModal.name] = $scope.$parent.$parent.$parent.primaryValueModal;
                }
            }

            if ($scope.$parent.$parent.$parent.calendarDate) {
                var startDate = angular.copy($scope.$parent.$parent.$parent.calendarDate);
                $scope.recordModal['selected_date'] = startDate.hour(8).toDate();
            }

            ModuleService.getPicklists($scope.moduleModal)
                .then(function (picklists) {
                    var picklistsModuleModal = angular.copy(picklists);

                    if ($scope.isExpert) {
                        var chargeTypeField = $filter('filter')($scope.moduleModal.fields, { name: 'charge_type' }, true)[0];
                        var chargeTypePicklist = picklistsModuleModal[chargeTypeField.picklist_id];

                        angular.forEach(chargeTypePicklist, function (picklistItem) {
                            picklistItem.hidden = picklistItem.value2 !== 'pm_billable' && picklistItem.value2 !== 'pm_non_billable';
                        });
                    }
                    else {
                        angular.forEach(chargeTypePicklist, function (picklistItem) {
                            picklistItem.hidden = false;
                        });
                    }

                    //Day görünümünde entry_type ın seçili gelmesi
                    if ($scope.$parent.$parent.$parent.calendarView === 'day') {
                        var entryTypeField = $filter('filter')($scope.moduleModal.fields, {name: 'entry_type'}, true)[0];
                        var entryTypePicklist = picklistsModuleModal[entryTypeField.picklist_id];
                        var entryTypeId;
                        if ($scope.$parent.$parent.$parent.selectedDayTime === '1') {
                            entryTypeId = $filter('filter')(entryTypePicklist, {system_code: 'morning_only'}, true)[0];
                            $scope.recordModal['entry_type'] = entryTypeId;
                        } else if ($scope.$parent.$parent.$parent.selectedDayTime === '2') {
                            entryTypeId = $filter('filter')(entryTypePicklist, {system_code: 'afternoon_only'}, true)[0];
                            $scope.recordModal['entry_type'] = entryTypeId;
                        }
                    }

                    $scope.picklistsModuleModal = picklistsModuleModal;
                    ModuleService.setDefaultValues($scope.moduleModal, $scope.recordModal, picklists);
                    ModuleService.setDisplayDependency($scope.moduleModal, $scope.recordModal);
                    $scope.loadingModal = false;
                });

            $scope.lookupModal = function (searchTerm) {
                if ($scope.currentLookupFieldModal.lookup_type === 'users' && !$scope.currentLookupFieldModal.lookupModulePrimaryField) {
                    var userModulePrimaryField = {};
                    userModulePrimaryField.data_type = 'text_single';
                    userModulePrimaryField.name = 'full_name';
                    $scope.currentLookupFieldModal.lookupModulePrimaryField = userModulePrimaryField;
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

                if (($scope.currentLookupFieldModal.lookupModulePrimaryField.data_type === 'number' || $scope.currentLookupFieldModal.lookupModulePrimaryField.data_type === 'number_auto') && isNaN(parseFloat(searchTerm))) {
                    $scope.$broadcast('angucomplete-alt:clearInput', $scope.currentLookupFieldModal.name);
                    return $q.defer().promise;
                }

                return ModuleService.lookup(searchTerm, $scope.currentLookupFieldModal, $scope.recordModal);
            };

            $scope.multiselectModal = function (searchTerm, field) {
                var picklistItems = [];

                angular.forEach($scope.picklistsModule[field.picklist_id], function (picklistItem) {
                    if (picklistItem.inactive)
                        return;

                    if (picklistItem.labelStr.toLowerCase().indexOf(searchTerm.toLowerCase()) > -1 || picklistItem.labelStr.toUpperCase().indexOf(searchTerm.toUpperCase()) > -1
                        || picklistItem.labelStr.toLowerCaseTurkish().indexOf(searchTerm.toLowerCaseTurkish()) > -1 || picklistItem.labelStr.toUpperCaseTurkish().indexOf(searchTerm.toUpperCaseTurkish()) > -1)
                        picklistItems.push(picklistItem);
                });

                return picklistItems;
            };

            $scope.setCurrentLookupFieldModal = function (field) {
                $scope.currentLookupFieldModal = field;
            };

            //gets months and years picklists
            var timesheetModule = $filter('filter')($rootScope.modules, { name: 'timesheet' }, true)[0];
            var monthField = $filter('filter')(timesheetModule.fields, { name: 'term' }, true)[0];
            var yearField = $filter('filter')(timesheetModule.fields, { name: 'year' }, true)[0];
            var statusField = $filter('filter')(timesheetModule.fields, { name: 'status' }, true)[0];

            ModuleService.getPicklists(timesheetModule)
                .then(function (picklists) {
                    $scope.months = picklists[monthField.picklist_id];
                    $scope.years = picklists[yearField.picklist_id];
                    $scope.statuses = picklists[statusField.picklist_id];
                    $scope.statusDraft = $filter('filter')($scope.statuses, { value: 'draft' }, true)[0];
                });

            $scope.specifyCountry = false;
            $scope.isTurkeyHoliday = false;
            $scope.isTurkeyHalfHoliday = false;
            var month = parseInt(moment($scope.$parent.calendarDay).month()) + 1;
            var year = moment($scope.$parent.calendarDay).year();
            var date = angular.copy($scope.$parent.$parent.$parent.calendarDate);
            var day = moment(date).date();
            for(var i =0; i<$scope.$parent.$parent.$parent.turkeyHolidays.length; i++){
                var holiday = $scope.$parent.$parent.$parent.turkeyHolidays[i];
                if(holiday.day === day && holiday.month === month && holiday.year === year){
                    if(!holiday.half_time)
                        $scope.isTurkeyHoliday = true;
                    else
                        $scope.isTurkeyHalfHoliday = true;
                }
            }

            $scope.submitModal = function (recordModal) {
                function validate() {
                    var isValid = true;

                    angular.forEach($scope.moduleModal.fields, function (field) {
                        if (!recordModal[field.name])
                            return;

                        if (field.data_type === 'lookup' && typeof recordModal[field.name] != 'object') {
                            $scope.moduleModalForm[field.name].$setValidity('object', false);
                            isValid = false;
                        }
                    });

                    return isValid;
                }

                if (!$scope.moduleModalForm.$valid || !validate())
                    return;

                $scope.submittingModal = true;
                recordModal = ModuleService.prepareRecord(recordModal, $scope.moduleModal, $scope.recordModalState);

                if ($scope.$parent.$parent.$parent.requestType == 'create') {
                    //create
                    var findTimesheet = {};
                    findTimesheet.fields = ['term', 'year'];
                    findTimesheet.filters = [{ field: 'owner', operator: 'equals', value: $scope.owner.id, no: 1 }];
                    findTimesheet.sort_field = 'created_at';
                    findTimesheet.sort_direction = 'desc';
                    findTimesheet.limit = 2000;

                    ModuleService.findRecords('timesheet', findTimesheet)
                        .then(function (response) {
                            response = response.data;
                            var request = {};
                            var month = false;
                            var year = false;
                            var currentMonth = parseInt(moment($scope.$parent.calendarDay).month()) + 1;
                            var currentYear = moment($scope.$parent.calendarDay).year();

                            angular.forEach(response, function (eventItem) {
                                if (eventItem.term == currentMonth && eventItem.year == currentYear) {
                                    month = true;
                                    year = true;
                                }
                            });

                            var daysWorkedExist = false;
                            var warnMessage = '';

                            if (!month || !year) {
                                if (currentMonth < 10)
                                    currentMonth = '0' + currentMonth;
                                else
                                    currentMonth = currentMonth.toString();

                                request.term = $filter('filter')($scope.months, { labelStr: currentMonth }, true)[0].id;
                                request.year = $filter('filter')($scope.years, { labelStr: currentYear.toString() }, true)[0].id;
                                request.status = $scope.statusDraft.id;
                                request.owner = $rootScope.user.id;

                                if ($scope.$parent.$parent.$parent.day.events.length < 2 || ( $scope.$parent.$parent.$parent.day.events.length == 2 && !$scope.$parent.$parent.$parent.day.events[0].code )) {
                                    var recordModelEntryType = $filter('filter')($scope.$parent.$parent.$parent.workedTimeItems, {id: recordModal.entry_type}, true)[0].system_code;
                                    angular.forEach($scope.$parent.$parent.$parent.day.events, function (workedTime) {
                                        if (workedTime.entry_type) {
                                            var workType = $filter('filter')($scope.$parent.$parent.$parent.workedTimeItems, {labelStr: workedTime.entry_type}, true)[0].system_code;

                                            if (workType == 'full_day' || workType == 'per_diem_only') {
                                                daysWorkedExist = true;
                                                warnMessage = 'Day is Full'
                                            }
                                            else if (recordModelEntryType == 'full_day' && recordModelEntryType != workType) {
                                                daysWorkedExist = true;
                                                warnMessage = 'Day is Full'
                                            }
                                            else if (recordModelEntryType == 'per_diem_only' && recordModelEntryType != workType) {
                                                daysWorkedExist = true;
                                                warnMessage = 'Day is Full'
                                            }
                                            else if (recordModelEntryType == workType) {
                                                daysWorkedExist = true;
                                                warnMessage = 'Already Exist'
                                            }
                                        }
                                    });
                                }
                                else {
                                    daysWorkedExist = true;
                                    warnMessage = 'Day is Full'
                                }

                                if (!daysWorkedExist) {
                                    ModuleService.insertRecord('timesheet', request)
                                        .then(function (response) {
                                            recordModal.related_timesheet = response.data.id;
                                            if (recordModal.entry_type == $scope.perDiemEntryTypeId)
                                                recordModal.per_diem = true;

                                            ModuleService.insertRecord($scope.moduleModal.name, recordModal)
                                                .then(function (response) {

                                                    var cacheKey = $scope.moduleModal.name + '_' + $scope.moduleModal.name;
                                                    $cache.remove(cacheKey);

                                                    if ($rootScope.activePages && $rootScope.activePages[$scope.moduleModal.name])
                                                        $rootScope.activePages[$scope.moduleModal.name] = null;

                                                    var lookupValue = {};
                                                    lookupValue.id = response.data.id;
                                                    lookupValue.primary_value = $scope.$parent.$parent.$parent.primaryValueModal;

                                                    if ($scope.$parent.$parent.$parent.record)
                                                        $scope.$parent.$parent.$parent.record[$scope.$parent.currentLookupField.name] = lookupValue;

                                                    if ($scope.$parent.$parent.$parent.formModalSuccess)
                                                        $scope.$parent.$parent.$parent.formModalSuccess();

                                                    $scope.submittingModal = false;
                                                    $scope.formModal.hide();
                                                })
                                                .catch(function (data, status) {
                                                    if (status === 409) {
                                                        $scope.moduleModalForm[data.field].$setValidity('unique', false);
                                                    }
                                                })
                                                .finally(function () {
                                                    $scope.submittingModal = false;
                                                });
                                        })
                                }
                                else {
                                    ngToast.create({ content: warnMessage, className: 'warning' });
                                    $scope.submittingModal = false;
                                }
                            }
                            else {
                                angular.forEach(response, function (eventItem) {
                                    if (eventItem.term == currentMonth && eventItem.year == currentYear)
                                        recordModal.related_timesheet = eventItem.id;
                                });

                                if ($scope.$parent.$parent.$parent.day.events.length < 2 || ( $scope.$parent.$parent.$parent.day.events.length == 2 && !$scope.$parent.$parent.$parent.day.events[0].code )) {
                                    var recordModelEntryType = $filter('filter')($scope.$parent.$parent.$parent.workedTimeItems, {id: recordModal.entry_type}, true)[0].system_code;
                                    angular.forEach($scope.$parent.$parent.$parent.day.events, function (workedTime) {
                                        if (workedTime.entry_type) {
                                            var workType = $filter('filter')($scope.$parent.$parent.$parent.workedTimeItems, {labelStr: workedTime.entry_type}, true)[0].system_code;

                                            if (workType == 'full_day' || workType == 'per_diem_only') {
                                                daysWorkedExist = true;
                                                warnMessage = 'Day is Full'
                                            }
                                            else if (recordModelEntryType == 'full_day' && recordModelEntryType != workType) {
                                                daysWorkedExist = true;
                                                warnMessage = 'Day is Full'
                                            }
                                            else if (recordModelEntryType == 'per_diem_only' && recordModelEntryType != workType) {
                                                daysWorkedExist = true;
                                                warnMessage = 'Day is Full'
                                            }
                                            else if (recordModelEntryType == workType) {
                                                daysWorkedExist = true;
                                                warnMessage = 'Already Exist'
                                            }

                                        }

                                    });
                                }
                                else {
                                    daysWorkedExist = true;
                                    warnMessage = 'Day is Full'
                                }

                                if (!daysWorkedExist) {
                                    if (recordModal.entry_type == $scope.perDiemEntryTypeId)
                                        recordModal.per_diem = true;

                                    ModuleService.insertRecord($scope.moduleModal.name, recordModal)
                                        .then(function (response) {

                                            var cacheKey = $scope.moduleModal.name + '_' + $scope.moduleModal.name;
                                            $cache.remove(cacheKey);

                                            if ($rootScope.activePages && $rootScope.activePages[$scope.moduleModal.name])
                                                $rootScope.activePages[$scope.moduleModal.name] = null;

                                            var lookupValue = {};
                                            lookupValue.id = response.data.id;
                                            lookupValue.primary_value = $scope.$parent.$parent.$parent.primaryValueModal;

                                            if ($scope.$parent.$parent.$parent.record)
                                                $scope.$parent.$parent.$parent.record[$scope.$parent.currentLookupField.name] = lookupValue;

                                            if ($scope.$parent.$parent.$parent.formModalSuccess)
                                                $scope.$parent.$parent.$parent.formModalSuccess();

                                            $scope.submittingModal = false;
                                            $scope.formModal.hide();
                                        })
                                        .catch(function (data) {
                                            if (data.status === 409) {
                                                $scope.moduleModalForm[data.data.field].$setValidity('unique', false);
                                            }
                                        })
                                        .finally(function () {
                                            $scope.submittingModal = false;
                                        });
                                }
                                else {
                                    ngToast.create({ content: warnMessage, className: 'warning' });
                                    $scope.submittingModal = false;
                                }

                            }
                        });
                }
                else {
                    //Update
                    var daysWorkedExist = false;
                    var warnMessage = '';

                    if ($scope.$parent.$parent.$parent.day.events.length < 2 || ( $scope.$parent.$parent.$parent.day.events.length === 2 && !$scope.$parent.$parent.$parent.day.events[0].code )) {
                        daysWorkedExist = false;
                    } else {
                        var recordModelEntryType = $filter('filter')($scope.$parent.$parent.$parent.workedTimeItems, {id: $scope.recordModal.entry_type.id}, true)[0].system_code;
                        var currentEntryType = $scope.$parent.$parent.$parent.editData.code;
                        if (recordModelEntryType == 'full_day' || recordModelEntryType == 'per_diem_only') {
                            daysWorkedExist = true;
                            warnMessage = 'Day is Full'
                        } else if (recordModelEntryType != currentEntryType) {
                            daysWorkedExist = true;
                            warnMessage = 'Already Exist'
                        }
                    }

                    if (!daysWorkedExist) {
                        if (recordModal.entry_type == $scope.perDiemEntryTypeId)
                            recordModal.per_diem = true;

                        recordModal.id = $scope.$parent.$parent.$parent.editData.id;
                        ModuleService.updateRecord($scope.moduleModal.name, recordModal)
                            .then(function (response) {

                                var cacheKey = $scope.moduleModal.name + '_' + $scope.moduleModal.name;
                                $cache.remove(cacheKey);

                                if ($rootScope.activePages && $rootScope.activePages[$scope.moduleModal.name])
                                    $rootScope.activePages[$scope.moduleModal.name] = null;

                                var lookupValue = {};
                                lookupValue.id = response.data.id;
                                lookupValue.primary_value = $scope.$parent.$parent.$parent.primaryValueModal;

                                if ($scope.$parent.$parent.$parent.record)
                                    $scope.$parent.$parent.$parent.record[$scope.$parent.$parent.$parent.currentLookupField.name] = lookupValue;

                                if ($scope.$parent.$parent.$parent.formModalSuccess)
                                    $scope.$parent.$parent.$parent.formModalSuccess();

                                $scope.submittingModal = false;
                                $scope.formModal.hide();
                            })
                            .catch(function (data) {
                                if (data.status === 409) {
                                    $scope.moduleModalForm[data.data.field].$setValidity('unique', false);
                                }
                            })
                            .finally(function () {
                                $scope.submittingModal = false;
                            });
                    } else {
                        ngToast.create({ content: warnMessage, className: 'warning' });
                        $scope.submittingModal = false;
                    }
                }
            };

            $scope.calculate = function (field) {
                ModuleService.calculate(field, $scope.moduleModal, $scope.recordModal);
            };

            $scope.fieldValueChange = function (field, record) {
                ModuleService.setDependency(field, $scope.moduleModal, $scope.recordModal, $scope.picklistsModuleModal);
                ModuleService.setDisplayDependency($scope.moduleModal, $scope.recordModal);
                if ($scope.moduleModalForm && $scope.moduleModalForm[field.name] && $scope.moduleModalForm[field.name].$error.unique)
                    $scope.moduleModalForm[field.name].$setValidity('unique', true);

                var month = parseInt(moment($scope.$parent.calendarDay).month()) + 1;
                var year = moment($scope.$parent.calendarDay).year();
                var date = angular.copy($scope.$parent.$parent.$parent.calendarDate);
                var day = moment(date).date();

                if(field.name === 'charge_type'){
                    $scope.isManagement = false;
                    if(record.value === 'management'){
                        $scope.isProjectHoliday = false;
                        $scope.isManagement = true;
                    }

                }

                if(field.name === 'place_of_performance'){
                    $scope.specifyCountry = false;
                    $scope.isManagementHalfHoliday = false;
                    $scope.recordModal.please_specify_country='';
                    if(record.system_code === 'other')
                        $scope.specifyCountry = true;
                }

                if(field.name === 'please_specify_country'){
                    $scope.isManagementHoliday = false;
                    $scope.isManagementHalfHoliday = false;
                        var filter = {
                            fields: ['total_count()','day','month','year','half_time'],
                            filters: [{ field: 'country', operator: 'is', value: record.system_code, no: 1 }],
                            limit: 1,
                            offset: 0
                        };
                        ModuleService.findRecords('holidays', filter)
                            .then(function (response) {
                                if(response.data.length>0){
                                    for(var i = 0; i<response.data.length; i++){
                                        if(response.data[i]['day'] === day && response.data[i]['month'] === month && response.data[i]['year'] === year){
                                            if(!response.data[i]['half_time'])
                                                $scope.isManagementHoliday = true;
                                            else
                                                $scope.isManagementHalfHoliday = true;
                                        }
                                    }
                                }
                            });
                }

                if(field.name === 'selected_project'){
                    $scope.isProjectHoliday = false;
                    $scope.isProjectHalfHoliday = false;
                    var filterRequest = {
                        fields: ['total_count()', 'holidays_id.holidays.day', 'holidays_id.holidays.month', 'holidays_id.holidays.year', 'holidays_id.holidays.half_time'],
                        filters: [{ field: 'projects' + '_id', operator: 'equals', value: record.id, no: 1 }],
                        limit: 1,
                        offset: 0,
                        many_to_many: 'projects'
                    };

                    ModuleService.findRecords('holidays', filterRequest)
                        .then(function (response) {
                            if(response.data.length>0){
                                for(var i = 0; i<response.data.length; i++){
                                    if(response.data[i]['holidays_id.holidays.day'] === day && response.data[i]['holidays_id.holidays.month'] === month && response.data[i]['holidays_id.holidays.year'] === year){
                                        if(!response.data[i]['holidays_id.holidays.half_time'])
                                            $scope.isProjectHoliday = true;
                                        else
                                            $scope.isProjectHalfHoliday = true;
                                    }
                                }
                            }
                        });
                }

            };
        }
    ]);