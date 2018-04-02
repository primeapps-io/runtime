'use strict';

var app = angular.module('ofisim', []);

    app.controller('TimetrackerController', ['$rootScope', '$scope', 'moment', '$modal', '$filter', '$location', 'ModuleService', 'config', '$http', '$state', 'helper', 'ngToast',
        function ($rootScope, $scope, moment, $modal, $filter, $location, ModuleService, config, $http, $state, helper, ngToast) {
            $scope.loggedInUser = $rootScope.user.ID;
            $scope.owner = $filter('filter')($rootScope.users, { Id: ($location.search().user ? parseInt($location.search().user) : $rootScope.user.ID) }, true)[0];
            $scope.userWeek = parseInt($location.search().week);
            $scope.userYear = parseInt($location.search().year);
            $scope.userMonth = parseInt($location.search().month);
            $scope.weekLength = moment().isoWeek();
            $scope.settings = {};
            $scope.loading = true;
            $scope.loadingForm = false;
            $scope.hasManuelProcess = false;
            $scope.waitingForApproval = false;
            $scope.manuelApproveRequest = false;
            $scope.isApproved = false;
            $scope.refreshSubModules = {};
            $scope.currentUser = ModuleService.processUser($rootScope.user);
            $scope.module = $filter('filter')($rootScope.modules, { name: 'timetrackers' }, true)[0];
            $scope.timetrackerItemModule = $filter('filter')($rootScope.modules, { name: 'timetracker_items' }, true)[0];
            $scope.relationModule = $filter('filter')($scope.module.relations, { related_module: 'timetracker_items' }, true)[0];
            $scope.lookupTypes = $filter('filter')($scope.timetrackerItemModule.fields, { data_type: 'lookup' }, true);
            $scope.hasAdminRights = angular.copy($rootScope.user.profile.HasAdminRights);
            $scope.firstConditionOfMonth = false;
            $scope.secondConditionOfMonth = false;
            $scope.showFullName = $location.search().user ? true : false;

            $scope.labels = [];
            for( var i = 0; i<$scope.relationModule.display_fields_array.length ; i++){
                var relationItem = $scope.relationModule.display_fields_array[i];
                var field = $filter('filter')($scope.timetrackerItemModule.fields, { name: relationItem }, true)[0];
                if(field)
                    $scope.labels.push(field);
            }
            $scope.lookupTypeForLabel = $filter('filter')($scope.labels, { data_type: 'lookup' }, true);

            $http.get(config.apiUrl + 'settings/get_by_key/5/timetracker_settings').then(function (response) {
                $scope.timetrackerSettingId = response.data.id;
                $scope.settings= angular.fromJson(response.data.value);
            });

            $scope.getTimeTrackerCalendar = function (w, paramDays, action, m) {
                var week = moment().isoWeek();
                if($scope.userWeek)
                    week = $scope.userWeek;

                if(w)
                    week = w;

                var year = moment().year();
                if($scope.userYear)
                    year = $scope.userYear;

                var weekObj = moment(year, "YYYY").week(week).weekday(0);

                var month = weekObj.month() + 1;
                if($scope.userMonth)
                    month = $scope.userMonth;

                if(m)
                    month = m;

                $scope.currentMonth = moment(weekObj).subtract(0, "month").startOf("month").format('MMMM');
                var firstDay = weekObj.day();
                var days = [];

                var firstMonthMoreDays = false;
                var secondMonthMoreDays = false;
                for( var i = 0; i < 7 ; i++){
                    var date = moment(moment(weekObj).isoWeekday(firstDay + i).format('YYYY-MM-DD'));
                    var fullDate = moment(year, "YYYY").week(week).weekday(i);
                    var dayOfWeek = date.day();
                    var day = {
                        'dayOrder' : dayOfWeek,
                        'date' : moment(weekObj).isoWeekday(firstDay + i).format('DD MMMM dddd'),
                        'date_formatted': moment(date).format('YYYY-MM-DD'),
                        'full_date' : fullDate,
                        'week' : week,
                        'timetracker_items' : [],
                        'description' : 'Bu güne ait kayıt bulunmamaktadır.',
                        'totalHour' : null,
                        'holiday' : false,
                        'notDayOfCurrentMonth' : false,
                        'month' : date.month()+1
                    };

                    if(!paramDays && !action){
                        if(date.month()+1 !== month){
                            day.notDayOfCurrentMonth = true;
                        }
                    }else{
                        if(action === 'previous'){
                            if($filter('filter')(paramDays, { notDayOfCurrentMonth: true }, true).length < 1){
                                var lastDayOfMonth = moment([year, day.month + 1]).endOf('month').date();
                                if(date.month()+1 === month && ( weekObj.date() > lastDayOfMonth-7 && weekObj.date() <= lastDayOfMonth)){
                                    $scope.currentMonth = moment(month+1, 'MM').format('MMMM');
                                    secondMonthMoreDays = true;
                                    day.notDayOfCurrentMonth = true;
                                }
                            }else if($filter('filter')(paramDays, { notDayOfCurrentMonth: true }, true).length > 0 && paramDays[0].notDayOfCurrentMonth === true){
                                if(date.month() !== month && !$scope.firstConditionOfMonth){
                                    firstMonthMoreDays = true;
                                }
                            }
                        }else{
                            if($filter('filter')(paramDays, { notDayOfCurrentMonth: true }, true).length < 1){
                                if(date.month()+1 !== month){
                                    day.notDayOfCurrentMonth = true;
                                    firstMonthMoreDays = true;
                                }
                            }else if($filter('filter')(paramDays, { notDayOfCurrentMonth: true }, true).length > 0  && paramDays[0].notDayOfCurrentMonth === false ){
                                if(date.month() !== month && !$scope.secondConditionOfMonth){
                                    secondMonthMoreDays = true;
                                }
                            }
                        }

                    }

                    days.push(day)
                }

                if(paramDays && action === 'previous'){
                    if(firstMonthMoreDays){
                        $scope.firstConditionOfMonth = true;
                        week = paramDays[0].week;
                        for(var t = 0; t<paramDays.length; t++){
                            var dayItem = paramDays[t];
                            dayItem.timetracker_items = [];
                            if(!dayItem.notDayOfCurrentMonth)
                                dayItem.notDayOfCurrentMonth = true;
                            else
                                dayItem.notDayOfCurrentMonth = false;
                        }
                        days = paramDays;
                    }else{
                        $scope.firstConditionOfMonth = false;
                    }

                    if(secondMonthMoreDays){
                        month = month + 1 ;
                    }
                }

                if(paramDays && action === 'next'){
                    if(secondMonthMoreDays){
                        $scope.secondConditionOfMonth = true;
                        week = paramDays[0].week;
                        for(var t = 0; t<paramDays.length; t++){
                            var dayItem = paramDays[t];
                            dayItem.timetracker_items = [];
                            if(!dayItem.notDayOfCurrentMonth)
                                dayItem.notDayOfCurrentMonth = true;
                            else
                                dayItem.notDayOfCurrentMonth = false;
                        }
                        days = paramDays;
                    }else{
                        $scope.secondConditionOfMonth = false;
                    }

                    if(firstMonthMoreDays){

                    }
                }

                var request = {};
                request.fields = [];
                angular.forEach($scope.module.fields, function (field) {
                    if (!field.deleted)
                        request.fields.push(field.name)
                });
                request.filters = [
                    { field: 'week', operator: 'equals', value: week, no: 1 },
                    { field: 'year', operator: 'equals', value: year, no: 2 },
                    { field: 'month', operator: 'equals', value: month, no: 3 },
                    { field: 'owner', operator: 'equals', value: $scope.owner.Id, no: 4 }
                ];

                request.limit = 1;
                ModuleService.findRecords('timetrackers', request)
                    .then(function (response) {
                        if(response.data.length <1){
                            if($scope.userWeek && $scope.userYear){
                                ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
                                $state.go('app.crm.dashboard');
                                return;
                            }

                            var calisanEmail = $scope.owner.Email;
                            var calisanRequest = {};
                            calisanRequest.fields = ['id'];
                            calisanRequest.filters = [
                                { field: 'e_posta', operator:'is', value: calisanEmail, no: 1 }
                            ];
                            calisanRequest.limit = 1;
                            ModuleService.findRecords('calisanlar', calisanRequest)
                                .then(function (responseCalisan) {
                                    if(responseCalisan.data.length < 1){
                                        ngToast.create({ content: $filter('translate')('Common.TimetrackerNotFound'), className: 'warning' });
                                        $state.go('app.crm.dashboard');
                                    }else{
                                        var timetrackerRequest = {};
                                        timetrackerRequest.week = week;
                                        timetrackerRequest.year = year;
                                        timetrackerRequest.month = month;
                                        timetrackerRequest.calisan = responseCalisan.data[0].id;
                                        timetrackerRequest.owner = $scope.owner.Id;
                                        timetrackerRequest.date_range = days[0].date+'-'+days[6].date;
                                        ModuleService.insertRecord('timetrackers', timetrackerRequest)
                                            .then(function (responseTimetracker) {
                                                $scope.currentTimetracker = responseTimetracker.data;
                                                if($scope.owner.Id !== $scope.currentUser.id && ($scope.currentTimetracker.custom_approver === null || $scope.currentTimetracker.custom_approver !== $scope.currentUser.email)){
                                                    ModuleService.deleteRecord('timetrackers', responseTimetracker.data.id)
                                                        .then(function () {
                                                            ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
                                                            $state.go('app.crm.dashboard');
                                                        });
                                                }
                                                $scope.getItems(week, year, month, days);
                                            });
                                    }
                            });
                        }else{
                            $scope.currentTimetracker = response.data[0];
                            if($scope.owner.Id !== $scope.currentUser.id && ($scope.currentTimetracker.custom_approver === null || $scope.currentTimetracker.custom_approver !== $scope.currentUser.email)){
                                ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
                                $state.go('app.crm.dashboard');
                                return;
                            }
                            $scope.getItems(week, year, month, days);
                        }
                    });
            };

            $scope.getItems = function (week, year, month, days) {
                $scope.currentTimetracker.process_id = $scope.currentTimetracker['process.process_requests.process_id'];
                $scope.currentTimetracker.process_status = $scope.currentTimetracker['process.process_requests.process_status'];
                $scope.currentTimetracker.process_status_order = $scope.currentTimetracker['process.process_requests.process_status_order'];
                $scope.currentTimetracker.operation_type = $scope.currentTimetracker['process.process_requests.operation_type'];
                for(var i=0; i < $rootScope.approvalProcesses.length; i++){
                    var currentProcess = $rootScope.approvalProcesses[i];
                    if(currentProcess.module_id === $scope.module.id && currentProcess.trigger_time === 'manuel')
                        $scope.hasManuelProcess = true;
                }
                //approval process
                if ($scope.currentTimetracker.process_status) {
                    if ($scope.currentTimetracker.process_status === 2)
                        $scope.isApproved = true;

                    if ($scope.currentTimetracker.process_status === 1 || $scope.currentTimetracker.process_status === 2 || ($scope.currentTimetracker.process_status === 3 && $scope.currentTimetracker.updated_by !== $scope.currentUser.id))
                        $scope.freeze = true;

                    ModuleService.getProcess($scope.currentTimetracker.process_id)
                        .then(function (response) {
                            var customApprover = $scope.currentTimetracker.custom_approver;
                            if(customApprover === $rootScope.user.email){
                                $scope.isApprovalRecord = true;
                                $scope.waitingForApproval = false;
                            }
                            else if(customApprover !== $rootScope.user.email && $scope.currentTimetracker.process_status === 1){
                                $scope.waitingForApproval = true;
                                $scope.isApproved = false;
                                $scope.isApprovalRecord = false;
                            }

                            if ($scope.currentTimetracker.operation_type === 0 && $scope.currentTimetracker.process_status === 2) {
                                for (var i = 0; i < response.data.operations_array.length; i++) {
                                    var process = response.data.operations_array[i];

                                    if (process === "update")
                                        $scope.freeze = false;
                                }
                            }

                            if ($scope.currentTimetracker.operation_type === 1 && $scope.currentTimetracker.process_status === 2) {
                                $scope.freeze = false;
                            }


                        });
                }
                else{
                    $scope.isApproved = false;
                    $scope.freeze = false;
                }
                var requestTimetrackerItems = {};
                requestTimetrackerItems.fields = [];
                angular.forEach($scope.timetrackerItemModule.fields, function (field) {
                    if (!field.deleted)
                        requestTimetrackerItems.fields.push(field.name)
                });

                angular.forEach($scope.lookupTypes, function (field) {
                    angular.forEach($scope.lookupTypeForLabel, function (fieldLabel) {
                        if (field.name === fieldLabel.name)
                            requestTimetrackerItems.fields.push(fieldLabel.name+'.'+fieldLabel.lookup_type+'.'+fieldLabel.lookupModulePrimaryField.name)
                    });
                });
                requestTimetrackerItems.filters = [{ field: 'owner', operator: 'equals', value: $scope.owner.Id, no: 1 }, { field: 'related_timetracker', operator: 'equals', value: $scope.currentTimetracker.id, no: 2 }];
                requestTimetrackerItems.sort_field = 'created_at';
                requestTimetrackerItems.sort_direction = 'desc';
                requestTimetrackerItems.limit = 2000;
                ModuleService.findRecords('timetracker_items', requestTimetrackerItems)
                    .then(function (response) {
                        $scope.timetrackers = response.data;
                        var totalWeekHour = null;
                        for( var i = 0; i<response.data.length ; i++){
                            var timetrackerItem = response.data[i];
                            angular.forEach($scope.lookupTypeForLabel, function (fieldLabel) {
                                if (timetrackerItem[fieldLabel.name] && timetrackerItem[fieldLabel.name] !==null)
                                    timetrackerItem[fieldLabel.name] = timetrackerItem[fieldLabel.name+'.'+fieldLabel.lookup_type+'.'+fieldLabel.lookupModulePrimaryField.name];
                            });
                            for( var j = 0; j<days.length ; j++){
                                var dayItem = days[j];
                                for(var t = 0; t<$rootScope.holidays.length ; t++){
                                    var holiday = $rootScope.holidays[0];
                                    if(holiday === moment(dayItem.date_formatted).format('DD-MM-YYYY')){
                                        dayItem.holiday = true;
                                        dayItem.description = 'Resmi tatil günü.'
                                    }
                                }
                                if (dayItem.date_formatted === moment.utc(timetrackerItem.tarih).format('YYYY-MM-DD')) {
                                    days[j].timetracker_items.push(timetrackerItem);
                                    dayItem.totalHour += timetrackerItem.saat;
                                    totalWeekHour += timetrackerItem.saat;
                                }
                            }
                        }
                        $scope.days = days;
                        $scope.week = week;
                        $scope.year = year;
                        $scope.month = month;
                        $scope.totalWeekHour = totalWeekHour;
                        $scope.loading = false;
                        $scope.loadingForm = false;

                        var timetrackerRecord = angular.copy($scope.currentTimetracker);
                        delete timetrackerRecord['process.process_requests.operation_type'];
                        delete timetrackerRecord['process.process_requests.process_id'];
                        delete timetrackerRecord['process.process_requests.process_status'];
                        delete timetrackerRecord['process.process_requests.process_status_order'];
                        timetrackerRecord['toplam_saat'] = totalWeekHour;
                        ModuleService.updateRecord('timetrackers', timetrackerRecord)
                            .then(function (response){})
                    });
            };

            $scope.getTimeTrackerCalendar();

            $scope.openCreateModal = function (dayObj) {
                $scope.currentLookupField = { lookup_type: 'timetracker_items' };
                $scope.dayObj = dayObj;
                $scope.calendarDate = dayObj.date_formatted;
                $scope.editModuleLoading = false;
                $scope.requestType = 'create';
                $scope.getTimetrackerItems = $scope.getTimeTrackerCalendar;
                $scope.timetrackerSettings = $scope.settings;

                $scope.formModal = $scope.formModal || $modal({
                        scope: $scope,
                        templateUrl: 'web/views/app/timesheet/timetrackerModal.html',
                        animation: '',
                        backdrop: 'static',
                        show: false,
                        tag: 'createModal'
                    });

                $scope.formModal.$promise.then($scope.formModal.show);
            };

            $scope.openEditModal = function (dayObj, item) {
                $scope.currentLookupField = { lookup_type: 'timetracker_items' };
                $scope.dayObj = dayObj;
                $scope.calendarDate = dayObj.date_formatted;
                $scope.editModuleLoading = true;
                $scope.requestType = 'edit';
                $scope.getTimetrackerItems = $scope.getTimeTrackerCalendar;
                $scope.editData = item;
                $scope.timetrackerSettings = $scope.settings;

                $scope.formModal = $scope.formModal || $modal({
                        scope: $scope,
                        templateUrl: 'web/views/app/timesheet/timetrackerModal.html',
                        animation: '',
                        backdrop: 'static',
                        show: false,
                        tag: 'createModal'
                    });

                $scope.formModal.$promise.then($scope.formModal.show);
            };

            $scope.delete = function (dayObj, id) {
                ModuleService.deleteRecord('timetracker_items', id)
                    .then(function () {
                        $scope.getTimeTrackerCalendar(dayObj.week, null, null, dayObj.month);
                    });
            };

            $scope.settingsModal = function () {

                $scope.settingsFormModal = $scope.settingsFormModal || $modal({
                        scope: $scope,
                        templateUrl: 'web/views/app/timesheet/timetrackerSettingsModal.html',
                        animation: '',
                        backdrop: 'static',
                        show: false,
                        tag: 'createModal'
                    });

                $scope.settingsFormModal.$promise.then($scope.settingsFormModal.show);
            };

            $scope.submitSettings = function () {
                if ($scope.settings.dayMaxHour && $scope.settings.dayMinHour && $scope.settings.weekHour && $scope.settings.dayStandartHour){
                    $scope.settingsApproving = true;

                    if(!$scope.settings.includeWeekend)
                        $scope.settings.includeWeekend = false;

                    // if(!$scope.settings.includeLeaves)
                    //     $scope.settings.includeLeaves = false;

                    if(!$scope.settings.includeHoliday)
                        $scope.settings.includeHoliday = false;

                    var settingObj = {
                        key : 'timetracker_settings',
                        value : angular.toJson($scope.settings),
                        type : 5
                    };

                    $http.put(config.apiUrl + 'settings/update/' + $scope.timetrackerSettingId, settingObj).then(function (response) {
                        $scope.settingsApproving = false;
                        $scope.settings= angular.fromJson(response.data.value);
                        $scope.getTimeTrackerCalendar();
                        $scope.settingsFormModal.hide();
                    });
                }
            };

            $scope.sendToProcessApproval = function () {
                $scope.manuelApproveRequest = true;
                var request = {
                    "record_id" : $scope.currentTimetracker.id,
                    "module_id" : $scope.module.id
                };

                ModuleService.sendApprovalManuel(request)
                    .then(function () {
                        $scope.hasManuelProcess = false;
                        $scope.waitingForApproval = true;
                        $scope.freeze = true;
                        $scope.manuelApproveRequest = false;
                        $scope.currentTimetracker.process_status = 1;
                        $scope.currentTimetracker.process_status_order++;
                    }).catch(function onError() {
                    $scope.manuelApproveRequest = false;
                });
            };

            $scope.approveProcess = function () {
                $scope.approving = true;

                ModuleService.approveProcessRequest($scope.currentTimetracker.operation_type, $scope.currentTimetracker.id)
                    .then(function () {
                        $scope.isApproved = true;
                        $scope.freeze = true;
                        $scope.approving = false;
                        $scope.currentTimetracker.process_status = 2;
                        $scope.waitingForApproval = true;
                    }).catch(function onError() {
                    $scope.approving = false;
                });
            };

            $scope.rejectProcess = function (message) {
                $scope.rejecting = true;

                ModuleService.rejectProcessRequest($scope.currentTimetracker.operation_type, message, $scope.currentTimetracker.id)
                    .then(function () {
                        $scope.isRejectedRequest = true;
                        $scope.rejecting = false;
                        $scope.currentTimetracker.process_status = 3;
                        $scope.rejectModal.hide();
                    }).catch(function onError() {
                    $scope.rejecting = false;
                });
            };

            $scope.reApproveProcess = function () {
                $scope.reapproving = true;

                ModuleService.send_approval($scope.currentTimetracker.operation_type, $scope.currentTimetracker.id)
                    .then(function () {
                        $scope.waitingForApproval = true;
                        $scope.freeze = true;
                        $scope.reapproving = false;
                        $scope.currentTimetracker.process_status = 1;
                        $scope.currentTimetracker.process_status_order++;
                    }).catch(function onError() {
                    $scope.reapproving = false;
                });
            };

            $scope.openRejectApprovalModal = function () {
                $scope.rejectModal = $scope.rejectModal || $modal({
                        scope: $scope,
                        templateUrl: 'web/views/app/module/rejectProcessModal.html',
                        animation: '',
                        backdrop: 'static',
                        show: false,
                        tag: 'createModal'
                    });

                $scope.rejectModal.$promise.then($scope.rejectModal.show);
            };

        }
    ]);

    app.filter('makePositive', function() {
        return function(num) { return Math.abs(num); }
    });