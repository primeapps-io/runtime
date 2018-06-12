'use strict';

angular.module('ofisim')

    .controller('timetrackerModalController', ['$rootScope', '$scope', 'ngToast', '$filter', 'helper', '$location', '$state', '$stateParams', '$q', '$window', '$localStorage', '$cache', 'operations', 'activityTypes', 'ModuleService',
        function ($rootScope, $scope, ngToast, $filter, helper, $location, $state, $stateParams, $q, $window, $localStorage, $cache, operations, activityTypes, ModuleService) {
            $scope.operations = operations;
            $scope.hasPermission = helper.hasPermission;
            $scope.hasFieldFullPermission = ModuleService.hasFieldFullPermission;
            $scope.loadingModal = true;
            $scope.timetrackerModule = $filter('filter')($rootScope.modules, { name: 'timetrackers' }, true)[0];
            $scope.relationModule = $filter('filter')($scope.timetrackerModule.relations, { related_module: 'timetracker_items' }, true)[0];

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

            $scope.timetrackerArr = [];
            var timesheetFields = $scope.relationModule.display_fields_array;
            angular.forEach(timesheetFields, function (field) {
                $scope.timetrackerArr.push($filter('filter')($scope.moduleModal.fields, { name: field }, true)[0]);
            });


            if (!$scope.moduleModal) {
                $scope.formModal.hide();
                ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
                $state.go('app.crm.dashboard');
                return;
            }

            $scope.primaryFieldModal = $filter('filter')($scope.moduleModal.fields, { primary_lookup: true })[0];

            if (!$scope.primaryFieldModal)
                $scope.primaryFieldModal = $filter('filter')($scope.moduleModal.fields, { primary: true })[0];

            $scope.currentUser = ModuleService.processUser($rootScope.user);
            $scope.currentDayMin = helper.getCurrentDateMin().toISOString();
            $scope.currentDayMax = helper.getCurrentDateMax().toISOString();
            $scope.currentHour = helper.floorMinutes(new Date());
            $scope.recordModal = {};
            $scope.recordModal.owner = $scope.currentUser;
            $scope.headerTitleCase = $scope.$parent.requestType;

            //if openEditModal func works, requestType becomes 'edit' to specify 'update' request.
            if ($scope.$parent.$parent.$parent.requestType == 'edit') {
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
                $scope.recordModal['tarih'] = $scope.$parent.$parent.$parent.calendarDate;
            }

            ModuleService.getPicklists($scope.moduleModal)
                .then(function (picklists) {
                    var picklistsModuleModal = angular.copy(picklists);

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

            $scope.setCurrentLookupFieldModal = function (field) {
                $scope.currentLookupFieldModal = field;
            };

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

                //izin günleri için izindir=true yollama
                var timetrackerItemModule = $filter('filter')($rootScope.modules, { name: 'timetracker_items' }, true)[0];
                var faaliyetPicklist = $filter('filter')(timetrackerItemModule.fields, { name: 'gorev' }, true)[0].picklist_id;
                var faaliyetItem = $filter('filter')($scope.picklistsModuleModal[faaliyetPicklist], { id: recordModal.gorev }, true)[0];
                if(faaliyetItem.value === 'izindir')
                    recordModal['izindir'] = true;

                if ($scope.$parent.$parent.$parent.requestType === 'create') {
                    //create
                    var findTimetracker = {};
                    findTimetracker.fields = ['week', 'year', 'month'];
                    findTimetracker.filters = [{ field: 'owner', operator: 'equals', value: $scope.recordModal.owner.id, no: 1 }];
                    findTimetracker.sort_field = 'created_at';
                    findTimetracker.sort_direction = 'desc';
                    findTimetracker.limit = 2000;

                    ModuleService.findRecords('timetrackers', findTimetracker)
                        .then(function (response) {
                            response = response.data;
                            var request = {};
                            var week = false;
                            var year = false;
                            var currentWeek = $scope.$parent.dayObj.week;
                            var currentMonth = $scope.$parent.dayObj.month;
                            var currentYear = moment($scope.$parent.calendarDay).year();

                            angular.forEach(response, function (eventItem) {
                                if (eventItem.week === currentWeek)
                                    week = true;

                                if (eventItem.year === currentYear)
                                    year = true;
                            });

                            var isDayFull = false;
                            var warnMessage = '';

                            if (!week || !year) {

                                request.week = currentWeek;
                                request.month = currentMonth;
                                request.year = currentYear;
                                request.owner = $rootScope.user.ID;

                                //SAAT KONTROLÜ
                                if ($scope.$parent.$parent.$parent.dayObj.totalHour + recordModal.saat <= $scope.$parent.$parent.$parent.settings.dayMaxHour) {
                                    isDayFull = false;
                                }
                                else {
                                    isDayFull = true;
                                    warnMessage = 'Günlük maksimum saat limitini aşıyor.'
                                }

                                if(recordModal.saat === 0){
                                    isDayFull = true;
                                    warnMessage = 'Lütfen geçerli bir saat giriniz.'
                                }

                                if (!isDayFull) {
                                    ModuleService.insertRecord('timetrackers', request)
                                        .then(function (response) {
                                            recordModal.related_timetracker = response.data.id;

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

                                                    $scope.$parent.$parent.$parent.getTimetrackerItems(currentWeek);

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
                                    if (eventItem.week === currentWeek && eventItem.year === currentYear && eventItem.month === currentMonth)
                                        recordModal.related_timetracker = eventItem.id;
                                });

                                if ($scope.$parent.$parent.$parent.dayObj.totalHour + recordModal.saat <= $scope.$parent.$parent.$parent.settings.dayMaxHour) {
                                    isDayFull = false;
                                }
                                else {
                                    isDayFull = true;
                                    warnMessage = 'Günlük maksimum saat limitini aşıyor.'
                                }

                                if(recordModal.saat === 0){
                                    isDayFull = true;
                                    warnMessage = 'Lütfen geçerli bir saat giriniz.'
                                }

                                if (!isDayFull) {
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

                                            $scope.$parent.$parent.$parent.getTimetrackerItems(currentWeek, null, null, currentMonth);

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
                    var isDayFull = false;
                    var warnMessage = '';

                    //SAAT KONTROLÜ
                    if(!recordModal.saat)
                        recordModal.saat = $scope.$parent.$parent.$parent.editData.saat;

                    if ($scope.$parent.$parent.$parent.dayObj.totalHour + recordModal.saat - $scope.$parent.$parent.$parent.editData.saat <= $scope.$parent.$parent.$parent.settings.dayMaxHour) {
                        isDayFull = false;
                    }
                    else {
                        isDayFull = true;
                        warnMessage = 'Günlük maksimum saat limitini aşıyor.'
                    }

                    if (!isDayFull) {
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

                                $scope.$parent.$parent.$parent.getTimetrackerItems($scope.$parent.dayObj.week);
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

            $scope.fieldValueChange = function (field) {
                ModuleService.setDependency(field, $scope.moduleModal, $scope.recordModal, $scope.picklistsModuleModal);
                ModuleService.setDisplayDependency($scope.moduleModal, $scope.recordModal);

                if ($scope.moduleModalForm && $scope.moduleModalForm[field.name] && $scope.moduleModalForm[field.name].$error.unique)
                    $scope.moduleModalForm[field.name].$setValidity('unique', true);
            };
        }
    ]);