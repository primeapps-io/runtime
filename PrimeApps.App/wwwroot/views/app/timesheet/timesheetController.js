'use strict';

angular.module('ofisim')

    .controller('TimesheetController', ['$rootScope', '$scope', '$state', '$location', '$filter', '$templateCache', '$modal', '$timeout', '$cache', '$modal', '$q', '$http', 'config', 'ngToast', 'calendarConfig', 'ModuleService',
        function ($rootScope, $scope, $state, $location, $filter, $templateCache, $modal, $timeout, $cache, $dropdown, $q, $http, config, ngToast, calendarConfig, ModuleService) {
            var language = window.localStorage['NG_TRANSLATE_LANG_KEY'] || 'tr';
            var locale = window.localStorage['locale_key'] || language;

            calendarConfig.dateFormatter = 'angular';
            calendarConfig.allDateFormats.angular.title.month = 'MMMM yyyy';
            calendarConfig.allDateFormats.angular.title.year = 'yyyy';

            if (language === 'tr') {
                calendarConfig.allDateFormats.angular.title.week = '{week}. Hafta {year}';
                calendarConfig.i18nStrings.weekNumber = '{week}. Hafta';
            }
            else {
                calendarConfig.allDateFormats.angular.title.week = 'Week {week} of {year}';
                calendarConfig.i18nStrings.weekNumber = 'Week {week}';
            }

            if (locale === 'tr') {
                calendarConfig.allDateFormats.angular.date.hour = 'HH:mm';
                calendarConfig.allDateFormats.angular.date.datetime = 'dd.MM.yyyy HH:mm';
            }
            else {
                calendarConfig.allDateFormats.angular.date.hour = 'ha';
                calendarConfig.allDateFormats.angular.date.datetime = 'MMM d, h:mm a';
            }

            $scope.timesheetModule = $filter('filter')($rootScope.modules, { name: 'timesheet' }, true)[0];
            $scope.timesheetItemModule = $filter('filter')($rootScope.modules, { name: 'timesheet_item' }, true)[0];
            $scope.approverModule = $filter('filter')($rootScope.modules, { name: 'approval_workflow' }, true)[0];
            $scope.owner = $filter('filter')($rootScope.users, { Id: ($location.search().user ? parseInt($location.search().user) : $rootScope.user.ID) }, true)[0];
            $scope.projectId = $location.search().project;
            $scope.month = $location.search().month;
            $scope.loading = true;

            if ($location.search().ctype) {
                switch ($location.search().ctype) {
                    case '0':
                        $scope.chargeType = 'billable';
                        break;
                    case '1':
                        $scope.chargeType = 'nonbillable';
                        break;
                    case '2':
                        $scope.chargeType = 'business';
                        break;
                }
            }

            if ($scope.owner.Id === $rootScope.user.ID)
                $scope.calendarView = 'month';
            else
                $scope.calendarView = 'day';

            $scope.calendarDay = new Date();
            $scope.loadingCalendar = true;
            $scope.events = [];

            if ($scope.month)
                $scope.calendarDay.setMonth(parseInt($scope.month));

            if (!$scope.owner) {
                ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
                $state.go('app.crm.dashboard');
                return;
            }

            $scope.timesheetTitle = $scope.owner.Id != $rootScope.user.ID ? $scope.owner.FullName + '\'s Timesheet' : '';

            $templateCache.put('views/app/timesheet/templates/calendarMonthCell.html', "<div mwl-droppable on-drop=\"vm.handleEventDrop(dropData.event, day.date, dropData.draggedFromDate)\" mwl-drag-select=\"!!vm.onDateRangeSelect\" on-drag-select-start=\"vm.onDragSelectStart(day)\" on-drag-select-move=\"vm.onDragSelectMove(day)\" on-drag-select-end=\"vm.onDragSelectEnd(day)\" class=\"cal-month-day {{ day.cssClass }}\" ng-class=\"{ 'cal-day-outmonth': !day.inMonth, 'cal-day-inmonth': day.inMonth, 'cal-day-weekend': day.isWeekend, 'cal-day-past': day.isPast, 'cal-day-today': day.isToday, 'cal-day-future': day.isFuture, 'cal-day-selected': vm.dateRangeSelect && vm.dateRangeSelect.startDate <= day.date && day.date <= vm.dateRangeSelect.endDate, 'cal-day-open': dayIndex === vm.openDayIndex }\">  <span class=\"pull-right\" data-cal-date ng-click=\"vm.calendarCtrl.dateClicked(day.date)\" ng-bind=\"day.label\"> </span>  <div class=\"add-button\" style='z-index: 999'  ng-if=\"!day.isWeekend\"> <a href class=\"btn btn-xs btn-default\" ng-click=\"vm.templateScope.openCreateModal($event, day, 'timesheet_item', null)\" ng-if=\"(!$parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.currentTimesheet || $parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.currentTimesheet.statusValue==='draft') && $parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.owner.Id == $parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.user.ID\">+</a> </div>   <div class=\"add-button\" style='z-index: 999'  ng-if=\"day.isWeekend\"> <a href class=\"btn btn-xs btn-default\" confirm-click action=\"vm.templateScope.openCreateModal($event, day, 'timesheet_item', null)\" placement=\"left\" confirm-message=\"Weekend, are you sure?\" confirm-yes=\"Yes\" confirm-no=\"No\" title=\"Sil\" ng-if=\"(!$parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.currentTimesheet || $parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.currentTimesheet.statusValue==='draft') && $parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.owner.Id == $parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.user.ID\">+</a> </div><span ng-repeat=\"(type, group) in day.groups track by type\" data-trigger=\"hover\" data-placement=\"top\" bs-tooltip> <div class='text-center'> <span ng-if=\"group.events[0].daysWorked=='0.5'\" ng-style=\"{'background-color': group.events[0].colorCode}\" class='morning-box'></span><span ng-if=\"group.events[0].daysWorked=='1'\" ng-style=\"{'background-color': group.events[0].colorCode}\" class='full-day-box'></span><span ng-if='group.events[1].type' ng-style=\"{'background-color': group.events[1].colorCode}\" class='afternoon-box'></span>&nbsp; </span> </div>  <div class=\"cal-day-tick\" ng-show=\"dayIndex === vm.openDayIndex && (vm.cellAutoOpenDisabled || vm.view[vm.openDayIndex].events.length > 0) && !vm.slideBoxDisabled\"> <i class=\"glyphicon glyphicon-chevron-up\"></i> <i class=\"fa fa-chevron-up\"></i> </div>  <ng-include src=\"vm.customTemplateUrls.calendarMonthCellEvents || vm.calendarConfig.templates.calendarMonthCellEvents\"></ng-include></div>");
            $templateCache.put('views/app/timesheet/templates/calendarSlideBox.html', "<div class=\"cal-slide-box\" uib-collapse=\"vm.isCollapsed\" mwl-collapse-fallback=\"vm.isCollapsed\"> <div class=\"cal-slide-content cal-event-list\"> <table class=\"table table-striped\"> <thead> <tr> <th>Days Worked</th><th>Entry Type</th> <th>Place of Performance</th> <th>Comments</th> <th>Status</th><th style='width: 60px;'></th></tr></thead> <tbody> <tr ng-style=\"{'background-color': event.colorCode}\" ng-repeat=\"event in vm.events | orderBy:'startsAt' track by event.calendarEventId\" class=\"event-line\" ng-class=\"event.cssClass\" mwl-draggable=\"event.draggable===true\" drop-data=\"{event: event}\"><td>{{event.daysWorked}} <span ng-if=\"event.per_diem\">( Per Diem )</span></td><td>{{event.entry_type}}</td><td ng-if=\"event.place_of_performance\"><span>{{event.place_of_performance}}</span></td><td ng-if=\"!event.place_of_performance\"><span>{{event.please_specify_country}} - {{event.please_specify}}</span></td><td> <b>{{event.selectedAction}}:</b> {{event.comment}}</td> <td>{{event.status}}</td> <td><span class='action-buttons' ng-if=\"(!$parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.currentTimesheet || $parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.currentTimesheet.statusValue==='draft' || $parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.currentTimesheet.statusValue.indexOf('rejected')>-1) && $parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.owner.Id == $parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.user.ID\"> <a ng-click=\"vm.templateScope.openEditModal($event, vm.cell, 'timesheet_item', event)\" class=\"action-icon\" title=\"{{'Common.Edit' | translate}}\"><i class=\"flaticon-pencil124\" ng-if=\"event.statusValue === 'draft' || event.statusValue.indexOf('rejected') > -1 \"></i></a>&nbsp; <i style='cursor: pointer' class=\"action-icon flaticon-bin9\" confirm-click action=\"vm.templateScope.delete(event)\" placement=\"left\" confirm-message=\"{{'Common.AreYouSure' | translate}}\" confirm-yes=\"{{'Common.Yes' | translate}}\" confirm-no=\"{{'Common.No' | translate}}\" title=\"{{'Common.Delete' | translate}}\" ng-if=\"$parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.currentTimesheet.statusValue==='draft' && event.statusValue === 'draft' \"></i> </span></td></tr></tbody> </table></div></div>");
            $templateCache.put('views/app/timesheet/templates/calendarYearView.html', "<div class=\"cal-year-box\"> <div ng-repeat=\"rowOffset in [0, 4, 8] track by rowOffset\"> <div class=\"row cal-before-eventlist\"> <div class=\"span3 col-md-3 col-xs-6 cal-cell {{ day.cssClass }}\" ng-repeat=\"month in vm.view | calendarLimitTo:4:rowOffset track by $index\" ng-init=\"monthIndex = vm.view.indexOf(month)\" ng-click=\"vm.calendarCtrl.dateClicked(month.date)\" ng-class=\"{'cal-day-today': month.isToday}\" mwl-droppable on-drop=\"vm.handleEventDrop(dropData.event, month.date)\">  <span class=\"pull-right\" data-cal-date ng-click=\"vm.calendarCtrl.dateClicked(month.date)\" ng-bind=\"month.label\"> </span>  <div class=\"counts\"> <span ng-repeat=\"(type, group) in month.groups track by type\" data-title=\"{{group.module}}\" data-trigger=\"hover\" data-placement=\"top\" bs-tooltip> <span class=\"label\" ng-style=\"{'background-color': group.color}\"> {{ group.events.length }} </span>&nbsp; </span> </div>  <div class=\"cal-day-tick\" ng-show=\"monthIndex === vm.openMonthIndex && (vm.cellAutoOpenDisabled || vm.view[vm.openMonthIndex].events.length > 0) && !vm.slideBoxDisabled\"> <i class=\"glyphicon glyphicon-chevron-up\"></i> <i class=\"fa fa-chevron-up\"></i> </div>  </div> </div>  </div>  </div>");
            $templateCache.put('views/app/timesheet/templates/calendarDayView.html', "<div class=\"cal-day-box\"><table class=\"table timesheet-table\"><thead class='thead-border'><tr><th style='text-align: center;'>DAY</th><th style='text-align: center'>DAYS WORKED</th><th>ENTRY TYPE</th><th>CHARGE TYPE</th><th>PLACE OF PERFORMANCE</th><th>COMMENTS</th><th>STATUS</th><th style='width: 60px;'></th></tr></thead><tbody class='tbody-border' ng-repeat=\"data in vm.listViewDays\" ng-init=\"listIndex=$index\" ng-if=\"vm.currentMonth==data.date\"><tr><td rowspan='2' class='text-center month-days'>{{data.label}}</td><td ng-style=\"{'vertical-align': data.events[0].paddingTop, 'background-color': data.events[0].colorCode}\" class='text-center'>&nbsp;{{data.events[0].daysWorked}} <span ng-if=\"data.events[0].per_diem\">&nbsp;( Per Diem )</span></td><td ng-style=\"{'vertical-align': data.events[0].paddingTop, 'background-color': data.events[0].colorCode}\">{{data.events[0].entry_type}}</td><td ng-style=\"{'vertical-align': data.events[0].paddingTop, 'background-color': data.events[0].colorCode}\">{{data.events[0].chargeType}}</td><td ng-if=\"data.events[0].place_of_performance\" ng-style=\"{'vertical-align': data.events[0].paddingTop, 'background-color': data.events[0].colorCode}\">{{data.events[0].place_of_performance}}</td><td ng-if=\"!data.events[0].place_of_performance\" ng-style=\"{'vertical-align': data.events[0].paddingTop, 'background-color': data.events[0].colorCode}\">{{data.events[0].please_specify_country}} <span ng-if=\"data.events[0].please_specify\">-</span> {{data.events[0].please_specify}}</td><td ng-style=\"{'vertical-align': data.events[0].paddingTop, 'background-color': data.events[0].colorCode}\"><span class='selected-action'>{{data.events[0].selectedAction}}</span><span ng-if=\"data.events[0].type\">:</span>  {{data.events[0].comment}}</td><td ng-style=\"{'vertical-align': data.events[0].paddingTop, 'background-color': data.events[0].colorCode}\">{{data.events[0].status}}</td><td ng-style=\"{'vertical-align': data.events[0].paddingTop, 'background-color': data.events[0].colorCode}\"><span ng-if=\"data.events[0].code && (!$parent.$parent.$parent.$parent.$parent.$parent.$parent.currentTimesheet || $parent.$parent.$parent.$parent.$parent.$parent.$parent.currentTimesheet.statusValue==='draft' || $parent.$parent.$parent.$parent.$parent.$parent.$parent.currentTimesheet.statusValue.indexOf('rejected')>-1) && $parent.$parent.$parent.$parent.$parent.$parent.$parent.owner.Id == $parent.$parent.$parent.$parent.$parent.$parent.$parent.user.ID\"  class='action-buttons'> <a ng-click=\"vm.templateScope.openEditModal($event, data, 'timesheet_item', data.events[0])\" class=\"action-icon\" title=\"{{'Common.Edit' | translate}}\"><i class=\"flaticon-pencil124\" ng-if=\"data.events[0].statusValue === 'draft' || data.events[0].statusValue.indexOf('rejected') > -1 \"></i></a>&nbsp; <i style='cursor: pointer' class=\"action-icon flaticon-bin9\" confirm-click action=\"vm.templateScope.delete(data.events[0])\" placement=\"left\" confirm-message=\"{{'Common.AreYouSure' | translate}}\" confirm-yes=\"{{'Common.Yes' | translate}}\" confirm-no=\"{{'Common.No' | translate}}\" title=\"{{'Common.Delete' | translate}}\" ng-if=\"$parent.$parent.$parent.$parent.$parent.$parent.$parent.currentTimesheet.statusValue==='draft' && data.events[0].statusValue === 'draft' < 0\"></i> </span><span class=\"action-buttons\" style='z-index: 999; padding-right: 15px'  ng-if=\"!data.isWeekend && data.events[0].code != 'morning_only' && data.events[0].code != 'full_day' && data.events[0].code != 'per_diem_only'\"> <a href ng-if=\"(!$parent.$parent.$parent.$parent.$parent.$parent.$parent.currentTimesheet || $parent.$parent.$parent.$parent.$parent.$parent.$parent.currentTimesheet.statusValue==='draft') && $parent.$parent.$parent.$parent.$parent.$parent.$parent.owner.Id == $parent.$parent.$parent.$parent.$parent.$parent.$parent.user.ID\" ng-click=\"vm.templateScope.openCreateModal($event, data, 'timesheet_item', '1')\"><i class=\"fa fa-plus\"></i></a> </span>   <span class=\"action-buttons\" style='z-index: 999; padding-right: 15px' ng-if=\"data.isWeekend && data.events[0].code != 'morning_only' && data.events[0].code != 'full_day' && data.events[0].code != 'per_diem_only'\"> <a href class=\"action-button\" ng-if=\"(!$parent.$parent.$parent.$parent.$parent.$parent.$parent.currentTimesheet || $parent.$parent.$parent.$parent.$parent.$parent.$parent.currentTimesheet.statusValue==='draft') && $parent.$parent.$parent.$parent.$parent.$parent.$parent.owner.Id == $parent.$parent.$parent.$parent.$parent.$parent.$parent.user.ID\" confirm-click action=\"vm.templateScope.openCreateModal($event, data, 'timesheet_item', '1')\" placement=\"left\" confirm-message=\"Weekend, are you sure?\" confirm-yes=\"Yes\" confirm-no=\"No\" title=\"Ekle\" ><i class=\"fa fa-plus\"></i></a> </span></td></tr><tr ng-if='data.events[0].daysWorked==1' ng-style=\"{'background-color':  data.events[0].colorCode, 'height': '50%'}\"><td></td><td></td><td></td><td></td><td></td><td></td><td></td></tr><tr ng-if='data.events[0].daysWorked!=1' ng-style=\"{'background-color': data.events[1].colorCode, 'height': '50%'}\"><td class='text-center'>&nbsp;{{data.events[1].daysWorked}} <span ng-if=\"data.events[1].per_diem\">( Per Diem )</span></td><td>{{data.events[1].entry_type}}</td><td>{{data.events[1].chargeType}}</td><td ng-if=\"data.events[1].place_of_performance\">{{data.events[1].place_of_performance}}</td><td ng-if=\"!data.events[1].place_of_performance\">{{data.events[1].please_specify_country}} <span ng-if=\"data.events[1].please_specify\">-</span> {{data.events[1].please_specify}}</td><td><span class='selected-action'>{{data.events[1].selectedAction}}</span><span ng-if=\"data.events[1]\">:</span> {{data.events[1].comment}}</td><td>{{data.events[1].status}}</td><td><span ng-if=\"data.events[1] && (!$parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.currentTimesheet || $parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.currentTimesheet.statusValue==='draft' || $parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.currentTimesheet.statusValue.indexOf('rejected')>-1) && $parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.owner.Id == $parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.user.ID\" class='action-buttons'> <a ng-click=\"vm.templateScope.openEditModal($event, data, 'timesheet_item', data.events[1])\" class=\"action-icon\" title=\"{{'Common.Edit' | translate}}\"><i class=\"flaticon-pencil124\" ng-if=\"data.events[1].statusValue === 'draft' || data.events[1].statusValue.indexOf('rejected') > -1 \"></i></a>&nbsp; <i style='cursor: pointer' class=\"action-icon flaticon-bin9\" confirm-click action=\"vm.templateScope.delete(data.events[1])\" placement=\"left\" confirm-message=\"{{'Common.AreYouSure' | translate}}\" confirm-yes=\"{{'Common.Yes' | translate}}\" confirm-no=\"{{'Common.No' | translate}}\" title=\"{{'Common.Delete' | translate}}\" ng-if=\"$parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.currentTimesheet.statusValue==='draft' && data.events[1].statusValue === 'draft' < 0\"></i> </span>  <span class=\"action-buttons\" style='z-index: 999; padding-right: 15px' ng-if=\"!data.isWeekend && data.events[1].code != 'afternoon_only' && data.events[0].code != 'full_day' && data.events[0].code != 'per_diem_only'\"> <a href class=\"action-button\" ng-if=\"(!$parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.currentTimesheet || $parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.currentTimesheet.statusValue==='draft') && $parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.owner.Id == $parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.user.ID\" ng-click=\"vm.templateScope.openCreateModal($event, data, 'timesheet_item', '2')\"><i class=\"fa fa-plus\"></i></a> </span>   <span class=\"action-buttons\" style='z-index: 999; padding-right: 15px' ng-if=\"data.isWeekend && data.events[1].code != 'afternoon_only' && data.events[0].code != 'full_day' && data.events[0].code != 'per_diem_only'\"> <a href class=\"button-action\" ng-if=\"(!$parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.currentTimesheet || $parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.currentTimesheet.statusValue==='draft') && $parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.owner.Id == $parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.$parent.user.ID\" confirm-click action=\"vm.templateScope.openCreateModal($event, data, 'timesheet_item', '2')\" placement=\"left\" confirm-message=\"Weekend, are you sure?\" confirm-yes=\"Yes\" confirm-no=\"No\" title=\"Sil\" ><i class=\"fa fa-plus\"></i></a> </span></td></tr></tbody></table></div>");
            $templateCache.put('views/app/timesheet/templates/calendarWeekView.html', "<div class=\"cal-week-box\" ng-class=\"{'cal-day-box': vm.showTimes}\"> <div class=\"cal-row-fluid cal-row-head\">  <div class=\"cal-cell1\" ng-repeat=\"day in vm.view.days track by $index\" ng-class=\"{ 'cal-day-weekend': day.isWeekend, 'cal-day-past': day.isPast, 'cal-day-today': day.isToday, 'cal-day-future': day.isFuture}\" mwl-element-dimensions=\"vm.dayColumnDimensions\" mwl-droppable on-drop=\"vm.eventDropped(dropData.event, day.date)\">  <span ng-bind=\"day.weekDayLabel\"></span> <br> <small> <span data-cal-date ng-click=\"vm.calendarCtrl.dateClicked(day.date)\" class=\"pointer\" ng-bind=\"day.dayLabel\"> </span> </small>  </div>  </div>  <div class=\"cal-day-panel clearfix\" ng-style=\"{height: vm.showTimes ? (vm.dayViewHeight + 'px') : 'auto'}\">  <mwl-calendar-hour-list day-view-start=\"vm.dayViewStart\" day-view-end=\"vm.dayViewEnd\" day-view-split=\"vm.dayViewSplit\" day-width=\"vm.dayColumnDimensions.width\" view-date=\"vm.viewDate\" on-timespan-click=\"vm.onTimespanClick\" on-date-range-select=\"vm.onDateRangeSelect\" custom-template-urls=\"vm.customTemplateUrls\" cell-modifier=\"vm.cellModifier\" template-scope=\"vm.templateScope\" ng-if=\"vm.showTimes\"> </mwl-calendar-hour-list>  <div class=\"row\" ng-repeat=\"row in vm.view.eventRows track by $index\"> <div class=\"col-xs-12\"> <div class=\"cal-row-fluid\"> <div ng-repeat=\"eventRow in row.row track by eventRow.event.calendarEventId\" ng-class=\"'cal-cell' + (vm.showTimes ? 1 : eventRow.span) + (vm.showTimes ? '' : ' cal-offset' + eventRow.offset)\" ng-style=\"{ top: vm.showTimes ? ((eventRow.top) + 'px') : 'auto', position: vm.showTimes ? 'absolute' : 'inherit', width: vm.showTimes ? (vm.dayColumnDimensions.width + 'px') : '', left: vm.showTimes ? (vm.dayColumnDimensions.width * eventRow.offset) + 15 + 'px' : '' }\"> <div class=\"day-highlight\" ng-class=\"[eventRow.event.cssClass, !vm.showTimes && eventRow.startsBeforeWeek ? '' : 'border-left-rounded', !vm.showTimes && eventRow.endsAfterWeek ? '' : 'border-right-rounded']\" ng-style=\"{backgroundColor: eventRow.event.color.secondary}\" data-event-class mwl-draggable=\"eventRow.event.draggable === true\" axis=\"vm.showTimes ? 'xy' : 'x'\" snap-grid=\"vm.showTimes ? {x: vm.dayColumnDimensions.width, y: vm.dayViewEventChunkSize || 30} : {x: vm.dayColumnDimensions.width}\" on-drag=\"vm.tempTimeChanged(eventRow.event, y / 30)\" on-drag-end=\"vm.weekDragged(eventRow.event, x / vm.dayColumnDimensions.width, y / 30)\" mwl-resizable=\"eventRow.event.resizable === true && eventRow.event.endsAt && !vm.showTimes\" resize-edges=\"{left: true, right: true}\" on-resize-end=\"vm.weekResized(eventRow.event, edge, x / vm.dayColumnDimensions.width)\"> <strong ng-bind=\"(eventRow.event.tempStartsAt || eventRow.event.startsAt) | calendarDate:'time':true\" ng-show=\"vm.showTimes\"></strong> <a href=\"#/app/crm/module/{{eventRow.event.type}}{{'?id=' + eventRow.event.id + '&back=calendar'}}\" ng-click=\"vm.onEventClick({calendarEvent: eventRow.event})\" class=\"event-item\" ng-bind-html=\"vm.calendarEventTitle.weekView(eventRow.event) | calendarTrustAsHtml\" uib-tooltip-html=\"vm.calendarEventTitle.weekViewTooltip(eventRow.event) | calendarTrustAsHtml\" tooltip-placement=\"left\" tooltip-append-to-body=\"true\"> </a> </div> </div> </div> </div>  </div>  </div> </div>");

            $scope.calendarCurrentMonth = parseInt(moment($scope.calendarDay).month()) + 1;
            $scope.calendarChosenMonth = parseInt(moment($scope.calendarDay).month()) + 1;

            $scope.previousMonth = function () {
                $scope.calendarChosenMonth--;
                getCurrentTimesheet();
                getCalendar();
            };

            $scope.nextMonth = function () {
                $scope.calendarChosenMonth++;
                getCurrentTimesheet();
                getCalendar();
            };

            //for holidays control
            var filterRequest = {
                filters: [{ field: 'country', operator: 'equals', value: 'Turkey', no: 1 }],
            };
            filterRequest.limit = 999;
            ModuleService.findRecords('holidays', filterRequest)
                .then(function (response) {
                    if (response.data.length > 0)
                        $scope.turkeyHolidays = response.data;
                });


            var checkStatus = function (timesheetItems) {
                ModuleService.getPicklists($scope.approverModule)
                    .then(function (picklistsApprover) {
                        var approvalTypeField = $filter('filter')($scope.approverModule.fields, { name: 'approval_type' }, true)[0];
                        var requestApprover = {
                            fields: ['approval_id', 'first_approver', 'first_approver_expert', 'second_approver'],
                            filters: [],
                            limit: 1,
                            offset: 0
                        };

                        if ($scope.projectId) {
                            requestApprover.filters.push({ field: 'related_project', operator: 'equals', value: $scope.projectId, no: 1 });
                            var approvalTypeBillablePicklistItem = $filter('filter')(picklistsApprover[approvalTypeField.picklist_id], { system_code: 'billable' }, true)[0];
                            var approvalTypeNonBillablePicklistItem = $filter('filter')(picklistsApprover[approvalTypeField.picklist_id], { system_code: 'nonbillable' }, true)[0];
                            var approvalTypeBusinessPicklistItem = $filter('filter')(picklistsApprover[approvalTypeField.picklist_id], { system_code: 'business' }, true)[0];

                            if ($scope.chargeType === 'billable')
                                requestApprover.filters.push({ field: 'approval_type', operator: 'is', value: approvalTypeBillablePicklistItem['label_' + $rootScope.user.tenantLanguage], no: 2 });
                            else if ($scope.chargeType === 'nonbillable')
                                requestApprover.filters.push({ field: 'approval_type', operator: 'is', value: approvalTypeNonBillablePicklistItem['label_' + $rootScope.user.tenantLanguage], no: 2 });
                            else if ($scope.chargeType === 'business')
                                requestApprover.filters.push({ field: 'approval_type', operator: 'is', value: approvalTypeBusinessPicklistItem['label_' + $rootScope.user.tenantLanguage], no: 2 });
                        }
                        else {
                            var approvalTypeManagementPicklistItem = $filter('filter')(picklistsApprover[approvalTypeField.picklist_id], { system_code: 'management' }, true)[0];
                            requestApprover.filters.push({ field: 'staff', operator: 'equals', value: $scope.currentStaff.id, no: 1 });
                            requestApprover.filters.push({ field: 'approval_type', operator: 'is', value: approvalTypeManagementPicklistItem['label_' + $rootScope.user.tenantLanguage], no: 2 });
                        }

                        ModuleService.findRecords('approval_workflow', requestApprover)
                            .then(function (approvalResponse) {
                                if (!approvalResponse || !approvalResponse.data[0]) {
                                    ngToast.create({ content: $filter('translate')('Common.Error'), className: 'danger' });
                                    return;
                                }

                                $scope.approvers = approvalResponse.data[0];
                                var approval = approvalResponse.data[0];
                                var requestHumanResources = {};
                                requestHumanResources.filters = [{ field: 'e_mail1', operator: 'is', value: $rootScope.user.email, no: 1 }];
                                var requestExpert = {};
                                requestExpert.filters = [{ field: 'e_mail1', operator: 'is', value: $rootScope.user.email, no: 1 }];

                                var promises = [];
                                promises.push($http.post(config.apiUrl + 'record/find/human_resources', requestHumanResources));
                                promises.push($http.post(config.apiUrl + 'record/find/experts', requestExpert));

                                $q.all(promises)
                                    .then(function (response) {
                                        var humanResource = response[0].data[0];
                                        var expert = response[1].data[0];

                                        var timesheetItemsCurrentMonth = $filter('filter')(timesheetItems, { related_timesheet: $scope.currentTimesheet.id }, true);

                                        for (var i = 0; i < timesheetItemsCurrentMonth.length; i++) {
                                            var timesheetItem = timesheetItemsCurrentMonth[i];

                                            if (timesheetItem.statusValue !== 'waiting_first' && timesheetItem.statusValue !== 'waiting_second') {
                                                $scope.alreadyProcessed = true;
                                                break;
                                            }

                                            if ($scope.chargeType === 'billable') {
                                                if (expert && timesheetItem.statusValue === 'waiting_first' && approval['first_approver_expert'] !== expert.id) {
                                                    $scope.alreadyProcessed = true;
                                                    break;
                                                }

                                                if (humanResource && timesheetItem.statusValue === 'waiting_second' && approval['second_approver'] !== humanResource.id) {
                                                    $scope.alreadyProcessed = true;
                                                    break;
                                                }
                                            }
                                            else {
                                                if (humanResource && ((timesheetItem.statusValue === 'waiting_first' && approval['first_approver'] !== humanResource.id) || (timesheetItem.statusValue === 'waiting_second' && approval['second_approver'] !== humanResource.id))) {
                                                    $scope.alreadyProcessed = true;
                                                    break;
                                                }
                                            }

                                            if (!humanResource && timesheetItem.statusValue !== 'waiting_first') {
                                                $scope.alreadyProcessed = true;
                                                break;
                                            }
                                        }
                                    })
                                    .finally(function () {
                                        $scope.loadingCalendar = false;
                                        $scope.loading = false;
                                    });
                            });
                    });
            };

            var getCalendar = function () {
                var entryTypeField = $filter('filter')($scope.timesheetItemModule.fields, { name: 'entry_type' }, true)[0];
                $scope.events = [];

                var requestTimesheet = {};
                requestTimesheet.filters = [
                    { field: 'term', operator: 'is', value: $scope.termPrevious, no: 1 },
                    { field: 'year', operator: 'is', value: $scope.yearPrevious, no: 2 },
                    { field: 'owner', operator: 'equals', value: $scope.owner.Id, no: 3 }
                ];
                requestTimesheet.limit = 1;

                ModuleService.findRecords('timesheet', requestTimesheet)
                    .then(function (respose) {
                        var previousTimesheet = 0;

                        if (respose && respose.data.length && $scope.owner.Id === $rootScope.user.ID)
                            previousTimesheet = respose.data[0];

                        var requestTimesheetItems = {};
                        requestTimesheetItems.fields = [];
                        angular.forEach($scope.timesheetItemModule.fields, function (field) {
                            if (!field.deleted)
                                requestTimesheetItems.fields.push(field.name)
                        });
                        requestTimesheetItems.fields.push('selected_project.projects.project_code');
                        requestTimesheetItems.filters = [{ field: 'owner', operator: 'equals', value: $scope.owner.Id, no: 1 }];
                        requestTimesheetItems.sort_field = 'created_at';
                        requestTimesheetItems.sort_direction = 'desc';
                        requestTimesheetItems.limit = 2000;

                        if (previousTimesheet)
                            requestTimesheetItems.filters.push({ field: 'related_timesheet', operator: 'equals', value: previousTimesheet['id'], no: 2 });

                        if ($scope.currentTimesheet)
                            requestTimesheetItems.filters.push({ field: 'related_timesheet', operator: 'equals', value: $scope.currentTimesheet['id'], no: (previousTimesheet ? 3 : 2) });

                        if ($scope.projectId) {
                            requestTimesheetItems.filters.push({ field: 'selected_project', operator: 'equals', value: $scope.projectId, no: (previousTimesheet ? 4 : 3) });

                            if ($scope.chargeType === 'billable')
                                requestTimesheetItems.filters.push({ field: 'approval_type', operator: 'is', value: 'billable', no: (previousTimesheet ? 5 : 4) });
                            else if ($scope.chargeType === 'nonbillable')
                                requestTimesheetItems.filters.push({ field: 'approval_type', operator: 'is', value: 'nonbillable', no: (previousTimesheet ? 5 : 4) });
                            else if ($scope.chargeType === 'business')
                                requestTimesheetItems.filters.push({ field: 'approval_type', operator: 'is', value: 'business', no: (previousTimesheet ? 5 : 4) });
                        }
                        else if ($scope.owner.Id != $rootScope.user.ID) {
                            requestTimesheetItems.filters.push({ field: 'selected_project', operator: 'empty', value: '-', no: (previousTimesheet ? 4 : 3) });
                        }

                        if (previousTimesheet && $scope.currentTimesheet) {
                            requestTimesheetItems.filter_logic = '(1 and (2 or 3))';

                            if ($scope.projectId)
                                requestTimesheetItems.filter_logic = '(1 and (2 or 3) and 4)';

                            if ($scope.chargeType)
                                requestTimesheetItems.filter_logic = '((1 and (2 or 3) and 4) and 5)';
                        }

                        ModuleService.getPicklists($scope.timesheetItemModule)
                            .then(function (items) {
                                $scope.picklistsTimesheetItems = items;
                                $scope.workedTimeItems = items[entryTypeField.picklist_id];

                                var placeOfPerformancePicklist = $filter('filter')($scope.timesheetItemModule.fields, { name: 'place_of_performance' }, true)[0];
                                var placeOfPerformancePicklistItem = $filter('filter')($scope.picklistsTimesheetItems[placeOfPerformancePicklist.picklist_id], { system_code: 'other' }, true)[0];

                                ModuleService.findRecords('timesheet_item', requestTimesheetItems)
                                    .then(function (response) {
                                        var timesheetItems = response.data;
                                        var statusFieldTimesheetItem = $filter('filter')($scope.timesheetItemModule.fields, { name: 'status' }, true)[0];

                                        angular.forEach(timesheetItems, function (timesheetItem) {

                                            var eventStartDate = moment.utc(timesheetItem.selected_date).toDate();
                                            var eventEndDate = moment.utc(timesheetItem.selected_date).toDate();

                                            var event = {};
                                            event.id = timesheetItem.id;
                                            event.title = timesheetItem.subject ? timesheetItem.subject + ' - <span class="event-user">' + timesheetItem['owner.users.full_name'] + '</span>' : timesheetItem['owner.users.full_name'];
                                            event.color = {
                                                primary: '#fbb903',
                                                secondary: '#fdf1ba'
                                            };
                                            event.module = $rootScope.language === 'tr' ? 'Timesheet' : 'Timesheet';
                                            event.type = 'timesheet_item';
                                            event.startsAt = eventStartDate;
                                            event.endsAt = eventEndDate;
                                            event.comment = timesheetItem.comment2;
                                            event.status = timesheetItem.status;
                                            event.per_diem = timesheetItem.per_diem;
                                            event.chargeType = timesheetItem.charge_type;
                                            event.currentListMonth = parseInt(moment(eventStartDate).month()) + 1;


                                            if (timesheetItem.selected_company)
                                                event.selectedAction = timesheetItem.selected_company;
                                            else
                                                event.selectedAction = timesheetItem['selected_project.projects.project_code'];


                                            if (timesheetItem.place_of_performance == placeOfPerformancePicklistItem.labelStr)
                                                event.place_of_performance = null;
                                            else
                                                event.place_of_performance = timesheetItem.place_of_performance;


                                            event.please_specify = timesheetItem.please_specify;
                                            event.please_specify_country = timesheetItem.please_specify_country;
                                            event.entry_type = timesheetItem.entry_type;

                                            var eventEntryType = $filter('filter')($scope.workedTimeItems, { labelStr: event.entry_type }, true)[0].system_code;
                                            event.eventEntryType = eventEntryType;

                                            if (timesheetItem.approval_type === 'billable') {
                                                event.colorCode = 'rgba(238, 109, 26, 0.14)';
                                                event.paddingTop = 'middle';
                                                event.code = eventEntryType;
                                            }
                                            else if (timesheetItem.approval_type === 'nonbillable') {
                                                event.colorCode = 'rgba(1, 160, 228, 0.14)';
                                                event.paddingTop = 'middle';
                                                event.code = eventEntryType;
                                            }
                                            else if (timesheetItem.approval_type === 'management') {
                                                event.colorCode = 'rgba(184, 193, 16, 0.14)';
                                                event.code = eventEntryType;
                                            }
                                            else if (timesheetItem.approval_type === 'business') {
                                                event.colorCode = 'rgba(157, 0, 255, 0.08)';
                                                event.code = eventEntryType;
                                            }
                                            else {
                                                event.colorCode = 'rgba(157, 0, 255, 0.08)';
                                                event.code = eventEntryType;
                                            }

                                            if (eventEntryType === 'full_day' || eventEntryType === 'per_diem_only') {
                                                event.daysWorked = '1';
                                            }
                                            else {
                                                event.daysWorked = '0.5';
                                            }

                                            $scope.events.push(event);

                                            var statusPicklistItemTimesheetItem = $filter('filter')($scope.picklistsTimesheetItems[statusFieldTimesheetItem.picklist_id], { labelStr: timesheetItem['status'] }, true)[0];
                                            timesheetItem.statusValue = statusPicklistItemTimesheetItem.value;

                                            event.statusValue = timesheetItem.statusValue;
                                        });

                                        $scope.timesheetItems = timesheetItems;

                                        if ($scope.owner.Id !== $rootScope.user.ID)
                                            checkStatus(timesheetItems);
                                        else {
                                            $scope.loadingCalendar = false;
                                            $scope.loading = false;
                                        }

                                    });
                            });

                        $timeout(function () {
                            $cache.put('timesheet_events', $scope.events);
                        }, 5000)
                    });
            };

            var getCurrentTimesheetItems = function () {
                var deferred = $q.defer();
                var requestTimesheetItems = {};
                requestTimesheetItems.fields = [];
                angular.forEach($scope.timesheetItemModule.fields, function (field) {
                    if (!field.deleted)
                        requestTimesheetItems.fields.push(field.name)
                });
                requestTimesheetItems.fields.push('selected_project.projects.project_code');
                requestTimesheetItems.filters = [{ field: 'owner', operator: 'equals', value: $scope.owner.Id, no: 1 }];
                requestTimesheetItems.sort_field = 'created_at';
                requestTimesheetItems.sort_direction = 'desc';
                requestTimesheetItems.limit = 2000;

                if ($scope.currentTimesheet)
                    requestTimesheetItems.filters.push({ field: 'related_timesheet', operator: 'equals', value: $scope.currentTimesheet['id'], no: 2 });

                if ($scope.projectId)
                    requestTimesheetItems.filters.push({ field: 'selected_project', operator: 'equals', value: $scope.projectId, no: ($scope.currentTimesheet ? 3 : 4) });
                else if ($scope.owner.Id != $rootScope.user.ID)
                    requestTimesheetItems.filters.push({ field: 'selected_project', operator: 'empty', value: '-', no: ($scope.currentTimesheet ? 3 : 4) });

                ModuleService.getPicklists($scope.timesheetItemModule)
                    .then(function (items) {
                        $scope.picklistsTimesheetItems = items;

                        ModuleService.findRecords('timesheet_item', requestTimesheetItems)
                            .then(function (response) {
                                deferred.resolve(response.data);
                            });
                    });

                return deferred.promise;
            };

            var getCurrentTimesheet = function () {
                var deferred = $q.defer();
                $scope.currentTimesheet = null;
                $scope.currentTimesheetItems = null;

                ModuleService.getPicklists($scope.timesheetModule)
                    .then(function (picklistsTimesheet) {
                        $scope.picklistsTimesheet = picklistsTimesheet;
                        var termField = $filter('filter')($scope.timesheetModule.fields, { name: 'term' }, true)[0];
                        var yearField = $filter('filter')($scope.timesheetModule.fields, { name: 'year' }, true)[0];
                        var currentMonth = parseInt(moment($scope.calendarDay).month()) + 1;
                        var previousMonth = parseInt(moment($scope.calendarDay).month());
                        var currentDate = new Date();

                        if(previousMonth === 11 && currentDate.getMonth() === 0 && $location.search().month){
                            $scope.calendarDay.setYear(moment($scope.calendarDay).year()-1);
                            previousMonth = 12 ;
                        }

                        var currentYear = moment($scope.calendarDay).year();
                        var previousYear = moment($scope.calendarDay).year();

                        if (previousMonth === 0)
                            previousMonth = 12 ;

                        $scope.termCurrent = $filter('filter')(picklistsTimesheet[termField.picklist_id], { value: currentMonth.toString() }, true)[0].labelStr;
                        $scope.yearCurrent = $filter('filter')(picklistsTimesheet[yearField.picklist_id], { value: currentYear.toString() }, true)[0].labelStr;
                        $scope.termPrevious = $filter('filter')(picklistsTimesheet[termField.picklist_id], { value: previousMonth.toString() }, true)[0].labelStr;
                        $scope.yearPrevious = $filter('filter')(picklistsTimesheet[yearField.picklist_id], { value: previousYear.toString() }, true)[0].labelStr;

                        var requestTimesheet = {};
                        requestTimesheet.filters = [
                            { field: 'term', operator: 'is', value: $scope.termCurrent, no: 1 },
                            { field: 'year', operator: 'is', value: $scope.yearCurrent, no: 2 },
                            { field: 'owner', operator: 'equals', value: $scope.owner.Id, no: 3 }
                        ];
                        requestTimesheet.limit = 1;

                        ModuleService.findRecords('timesheet', requestTimesheet)
                            .then(function (respose) {
                                if (!respose || !respose.data.length) {
                                    deferred.resolve({});
                                    return deferred.promise;
                                }

                                $scope.currentTimesheet = respose.data[0];
                                var statusField = $filter('filter')($scope.timesheetModule.fields, { name: 'status' }, true)[0];
                                var statusPicklistItem = $filter('filter')($scope.picklistsTimesheet[statusField.picklist_id], { labelStr: $scope.currentTimesheet['status'] }, true)[0];
                                $scope.currentTimesheet.statusValue = statusPicklistItem.value;

                                getCurrentTimesheetItems()
                                    .then(function (timesheetItems) {
                                        if (!timesheetItems || !timesheetItems.length) {
                                            deferred.resolve({});
                                            return deferred.promise;
                                        }

                                        $scope.projectIds = [];

                                        angular.forEach(timesheetItems, function (timesheetItem) {
                                            var chargeTypeField = $filter('filter')($scope.timesheetItemModule.fields, { name: 'charge_type' }, true)[0];
                                            var chargeTypePicklistItem = $filter('filter')($scope.picklistsTimesheetItems[chargeTypeField.picklist_id], { labelStr: timesheetItem['charge_type'] }, true)[0];
                                            timesheetItem.approverType = chargeTypePicklistItem.value;
                                            var statusFieldTimesheetItem = $filter('filter')($scope.timesheetItemModule.fields, { name: 'status' }, true)[0];
                                            var statusPicklistItemTimesheetItem = angular.isObject(timesheetItem['status']) ? timesheetItem['status'] : $filter('filter')($scope.picklistsTimesheetItems[statusFieldTimesheetItem.picklist_id], { labelStr: timesheetItem['status'] }, true)[0];
                                            timesheetItem.statusValue = statusPicklistItemTimesheetItem.value;
                                            var projectId = timesheetItem['selected_project'] ? parseInt(timesheetItem['selected_project']) : 0;

                                            if (projectId > 0 && $scope.projectIds.indexOf(projectId) < 0)
                                                $scope.projectIds.push(projectId);
                                        });

                                        $scope.currentTimesheetItems = timesheetItems;

                                        deferred.resolve({});
                                    });
                            });
                    });

                return deferred.promise;
            };

            var getCurrentStaff = function () {
                var requestHumanResources = {};
                requestHumanResources.filters = [{ field: 'e_mail1', operator: 'is', value: $scope.owner.Email, no: 1 }];

                ModuleService.findRecords('human_resources', requestHumanResources)
                    .then(function (respose) {
                        $scope.currentStaff = respose.data[0];

                        if (!$scope.currentStaff) {
                            $scope.isExpert = true;
                            //ngToast.create({ content: 'Human resource record not found. Please contact your system administrator.', className: 'warning' });
                        }
                    });
            };

            getCurrentTimesheet()
                .then(function () {
                    getCalendar();
                    getCurrentStaff();
                });

            $scope.groupEvents = function (cell) {
                if (!cell.events || !cell.events.length)
                    return;

                cell.groups = {};

                cell.events.forEach(function (event) {
                    cell.groups[event.type] = cell.groups[event.type] || {};
                    cell.groups[event.type].module = event.module;
                    cell.groups[event.type].color = event.color.primary;
                    cell.groups[event.type].events = cell.groups[event.type].events || [];
                    cell.groups[event.type].events.push(event);
                });

                angular.forEach(cell.groups, function (item) {
                    var itemEvents = angular.copy(item.events);
                    angular.forEach(itemEvents, function (event) {
                        if (event.code == 'morning_only') {
                            item.events[0] = event;
                        } else if (event.code == 'afternoon_only') {
                            if (itemEvents.length > 1)
                                item.events[1] = event;
                            else
                                item.events[0] = [];
                            item.events[1] = event;
                        }
                    });
                });
            };

            $scope.delete = function (event) {
                ModuleService.deleteRecord(event.type, event.id)
                    .then(function () {
                        getCalendar();
                        $cache.remove(event.type + '_' + event.type);
                    });
            };

            $scope.openCreateModal = function ($event, day, module, dayTime) {
                if (day) {
                    if ($scope.calendarView === 'month') {
                        $scope.calendarDate = day.date;
                    }
                    else {
                        $scope.calendarDate = day.fullDate;
                        $scope.selectedDayTime = dayTime;
                    }
                }

                $scope.editModuleLoading = false;
                $scope.day = day;
                $scope.requestType = 'create';

                $scope.currentLookupField = { lookup_type: module };

                $scope.formModal = $scope.formModal || $modal({
                    scope: $scope,
                    templateUrl: 'views/app/timesheet/timesheetModal.html',
                    animation: '',
                    backdrop: 'static',
                    show: false,
                    tag: 'createModal'
                });

                $scope.formModal.$promise.then($scope.formModal.show);
            };

            $scope.openEditModal = function ($event, day, module, event) {
                if (day) {
                    if (day.fullDate)
                        $scope.calendarDate = day.fullDate;
                    else
                        $scope.calendarDate = day.date;
                }

                $scope.editModuleLoading = true;

                $scope.day = day;
                $scope.requestType = 'edit';
                $scope.editData = event;
                $scope.currentLookupField = { lookup_type: module };

                $scope.formModal = $scope.formModal || $modal({
                    scope: $scope,
                    templateUrl: 'views/app/timesheet/timesheetModal.html',
                    animation: '',
                    backdrop: 'static',
                    show: false,
                    tag: 'createModal'
                });

                $scope.formModal.$promise.then($scope.formModal.show);
            };

            $scope.formModalSuccess = function () {
                getCurrentTimesheet()
                    .then(function () {
                        getCalendar();
                    });
            };

            //Approval processes
            var getApprovers = function () {
                var deferred = $q.defer();

                ModuleService.getPicklists($scope.approverModule)
                    .then(function (picklistsApprover) {
                        var approvers = {};
                        $scope.picklistsApprover = picklistsApprover;
                        var approvalTypeField = $filter('filter')($scope.approverModule.fields, { name: 'approval_type' }, true)[0];
                        var approvalTypeManagementPicklistItem = $filter('filter')(picklistsApprover[approvalTypeField.picklist_id], { system_code: 'management' }, true)[0];
                        var requestApproversFields =
                            [
                                'approval_id',
                                'approval_type',
                                'related_project',
                                'related_project.projects.project_code.primary',
                                'staff',
                                'staff.human_resources.name_surname.primary',
                                'staff.human_resources.e_mail1',
                                'first_approver',
                                'first_approver.human_resources.name_surname.primary',
                                'first_approver.human_resources.e_mail1',
                                'first_approver_expert',
                                'first_approver_expert.experts.name_surname.primary',
                                'first_approver_expert.experts.e_mail1',
                                'second_approver',
                                'second_approver.human_resources.name_surname.primary',
                                'second_approver.human_resources.e_mail1'
                            ];

                        var getApproversProject = function () {
                            var requestApproversProject = {
                                fields: requestApproversFields,
                                filters: [],
                                logic_type: 'or',
                                limit: 2000,
                                offset: 0
                            };

                            if ($scope.projectIds) {
                                for (var i = 0; i < $scope.projectIds.length; i++) {
                                    var filter = {
                                        field: 'related_project.projects.id',
                                        operator: 'equals',
                                        value: $scope.projectIds[i],
                                        no: i + 1
                                    };

                                    requestApproversProject.filters.push(filter);
                                }
                            }

                            ModuleService.findRecords('approval_workflow', requestApproversProject)
                                .then(function (response) {
                                    var approversProject = response.data;

                                    if (approversProject && approversProject.length > 0) {
                                        angular.forEach(approversProject, function (approverProjectResponse) {
                                            var approvalTypeManagementPicklistItem = $filter('filter')(picklistsApprover[approvalTypeField.picklist_id], { labelStr: approverProjectResponse['approval_type'] }, true)[0];
                                            approverProjectResponse.approvalType = approvalTypeManagementPicklistItem.system_code;
                                        });
                                    }

                                    approvers.project = approversProject;
                                    deferred.resolve(approvers);
                                });
                        };

                        var getApproversManagementAndProject = function () {
                            var requestApproversManagement = {
                                fields: requestApproversFields,
                                filters: [
                                    { field: 'staff', operator: 'equals', value: $scope.currentStaff.id, no: 1 },
                                    { field: 'approval_type', operator: 'is', value: approvalTypeManagementPicklistItem.labelStr, no: 2 }
                                ],
                                limit: 2000,
                                offset: 0
                            };

                            ModuleService.findRecords('approval_workflow', requestApproversManagement)
                                .then(function (response) {
                                    var approversManagement = response.data;

                                    if (approversManagement && approversManagement.length > 0) {
                                        angular.forEach(approversManagement, function (approverManagement) {
                                            var approvalTypeManagementPicklistItem = $filter('filter')(picklistsApprover[approvalTypeField.picklist_id], { labelStr: approverManagement['approval_type'] }, true)[0];
                                            approverManagement.approvalType = approvalTypeManagementPicklistItem.system_code;
                                        });
                                    }

                                    approvers.management = approversManagement[0];

                                    getApproversProject();
                                });
                        };

                        if ($scope.isExpert)
                            getApproversProject();
                        else
                            getApproversManagementAndProject();
                    });

                return deferred.promise;
            };

            $scope.submitApproval = function (ownerUser, resubmit, humanResource) {
                $scope.submitting = true;
                var user = null;

                if (ownerUser)
                    user = ownerUser;
                else
                    user = $filter('filter')($rootScope.users, { Id: $rootScope.user.ID }, true)[0];

                getApprovers().then(function (approvers) {
                    if (!approvers) {
                        ngToast.create({ content: 'Approvers not found. Please contact your system administrator.', className: 'warning' });
                        $scope.submitting = false;
                        return;
                    }

                    var approvalRequests = [];
                    var hasNoApprovers = false;
                    var statusFieldTimesheetItem = $filter('filter')($scope.timesheetItemModule.fields, { name: 'status' }, true)[0];
                    var waitingFirstPicklistItem = $filter('filter')($scope.picklistsTimesheetItems[statusFieldTimesheetItem.picklist_id], { value: 'waiting_first' }, true)[0];
                    var waitingSecondPicklistItem = $filter('filter')($scope.picklistsTimesheetItems[statusFieldTimesheetItem.picklist_id], { value: 'waiting_second' }, true)[0];
                    var approvedPicklistItem = $filter('filter')($scope.picklistsTimesheetItems[statusFieldTimesheetItem.picklist_id], { value: 'approved_second' }, true)[0];

                    for (var i = 0; i < $scope.currentTimesheetItems.length; i++) {
                        var timesheetItem = $scope.currentTimesheetItems[i];

                        if (hasNoApprovers)
                            continue;

                        if (timesheetItem.statusValue === 'waiting_second' || timesheetItem.statusValue === 'approved_second')
                            continue;

                        var timesheetItemType;
                        if (timesheetItem.approverType === 'management')
                            timesheetItemType = approvers.management;
                        else
                            timesheetItemType = $filter('filter')(approvers.project, { approvalType: timesheetItem.approverType }, true)[0];

                        if ((timesheetItem.statusValue === 'waiting_first' || timesheetItem.statusValue === 'approved_first') && !timesheetItemType.second_approver)
                            continue;

                        if (resubmit && !(timesheetItem.statusValue === 'rejected_first' || timesheetItem.statusValue === 'rejected_second'))
                            continue;

                        var approvalRequest = {};
                        approvalRequest.timesheetItemId = timesheetItem.id;
                        approvalRequest.chargeType = timesheetItem.approval_type;
                        var approver = null;

                        switch (timesheetItem.approverType) {
                            case 'billable':
                                approvalRequest.projectId = timesheetItem['selected_project'];
                                approver = $filter('filter')(approvers.project, { 'related_project.projects.id': approvalRequest.projectId, approvalType: 'billable' }, true)[0];

                                if (!approver) {
                                    hasNoApprovers = true;
                                    break;
                                }

                                if (timesheetItem.statusValue === 'draft' || timesheetItem.statusValue === 'rejected_first') {
                                    approvalRequest.fullName = approver['first_approver_expert.experts.name_surname.primary'];
                                    approvalRequest.email = approver['first_approver_expert.experts.e_mail1'];
                                    approvalRequest.timesheetItemStatus = waitingFirstPicklistItem.id;
                                    approvalRequest['1_approver'] = $filter('filter')($rootScope.users, { Email: approvalRequest.email }, true)[0].Id;
                                }
                                else if (timesheetItem.statusValue === 'approved_first' || timesheetItem.statusValue === 'rejected_second') {
                                    approvalRequest.fullName = approver['second_approver.human_resources.name_surname.primary'];
                                    approvalRequest.email = approver['second_approver.human_resources.e_mail1'];
                                    approvalRequest.timesheetItemStatus = waitingSecondPicklistItem.id;
                                    approvalRequest['2_approver'] = $filter('filter')($rootScope.users, { Email: approvalRequest.email }, true)[0].Id;
                                }
                                break;
                            case 'nonbillable':
                                approvalRequest.projectId = timesheetItem['selected_project'];
                                approver = $filter('filter')(approvers.project, { 'related_project.projects.id': approvalRequest.projectId, approvalType: 'nonbillable' }, true)[0];

                                if (!approver) {
                                    hasNoApprovers = true;
                                    break;
                                }

                                if ((timesheetItem.statusValue === 'draft' || timesheetItem.statusValue === 'rejected_first') && $scope.owner.Email == approver['first_approver.human_resources.e_mail1']) {
                                    approvalRequest.fullName = approver['second_approver.human_resources.name_surname.primary'];
                                    approvalRequest.email = approver['second_approver.human_resources.e_mail1'];
                                    approvalRequest.timesheetItemStatus = waitingSecondPicklistItem.id;
                                    approvalRequest['2_approver'] = $filter('filter')($rootScope.users, { Email: approvalRequest.email }, true)[0].Id;
                                }
                                else if (timesheetItem.statusValue === 'draft' || timesheetItem.statusValue === 'rejected_first') {
                                    approvalRequest.fullName = approver['first_approver.human_resources.name_surname.primary'];
                                    approvalRequest.email = approver['first_approver.human_resources.e_mail1'];
                                    approvalRequest.timesheetItemStatus = waitingFirstPicklistItem.id;
                                    approvalRequest['1_approver'] = $filter('filter')($rootScope.users, { Email: approvalRequest.email }, true)[0].Id;
                                }
                                else if ((timesheetItem.statusValue === 'approved_first' || timesheetItem.statusValue === 'rejected_second') && $scope.owner.Email !== approver['first_approver.human_resources.e_mail1']) {
                                    approvalRequest.fullName = approver['second_approver.human_resources.name_surname.primary'];
                                    approvalRequest.email = approver['second_approver.human_resources.e_mail1'];
                                    approvalRequest.timesheetItemStatus = approvedPicklistItem.id;
                                    approvalRequest['2_approver'] = $filter('filter')($rootScope.users, { Email: approvalRequest.email }, true)[0].Id;
                                }
                                else if (timesheetItem.statusValue === 'approved_first' || timesheetItem.statusValue === 'rejected_second') {
                                    approvalRequest.fullName = approver['second_approver.human_resources.name_surname.primary'];
                                    approvalRequest.email = approver['second_approver.human_resources.e_mail1'];
                                    approvalRequest.timesheetItemStatus = waitingSecondPicklistItem.id;
                                    approvalRequest['2_approver'] = $filter('filter')($rootScope.users, { Email: approvalRequest.email }, true)[0].Id;
                                }
                                break;
                            case 'business':
                                approvalRequest.projectId = timesheetItem['selected_project'];
                                approver = $filter('filter')(approvers.project, { 'related_project.projects.id': approvalRequest.projectId, approvalType: 'business' }, true)[0];

                                if (!approver) {
                                    hasNoApprovers = true;
                                    break;
                                }

                                if ((timesheetItem.statusValue === 'draft' || timesheetItem.statusValue === 'rejected_first') && $scope.owner.Email == approver['first_approver.human_resources.e_mail1']) {
                                    approvalRequest.fullName = approver['second_approver.human_resources.name_surname.primary'];
                                    approvalRequest.email = approver['second_approver.human_resources.e_mail1'];
                                    approvalRequest.timesheetItemStatus = waitingSecondPicklistItem.id;
                                    approvalRequest['2_approver'] = $filter('filter')($rootScope.users, { Email: approvalRequest.email }, true)[0].Id;
                                }
                                else if (timesheetItem.statusValue === 'draft' || timesheetItem.statusValue === 'rejected_first') {
                                    approvalRequest.fullName = approver['first_approver.human_resources.name_surname.primary'];
                                    approvalRequest.email = approver['first_approver.human_resources.e_mail1'];
                                    approvalRequest.timesheetItemStatus = waitingFirstPicklistItem.id;
                                    approvalRequest['1_approver'] = $filter('filter')($rootScope.users, { Email: approvalRequest.email }, true)[0].Id;
                                }
                                else if ((timesheetItem.statusValue === 'approved_first' || timesheetItem.statusValue === 'rejected_second') && $scope.owner.Email !== approver['first_approver.human_resources.e_mail1']) {
                                    approvalRequest.fullName = approver['second_approver.human_resources.name_surname.primary'];
                                    approvalRequest.email = approver['second_approver.human_resources.e_mail1'];
                                    approvalRequest.timesheetItemStatus = approvedPicklistItem.id;
                                    approvalRequest['2_approver'] = $filter('filter')($rootScope.users, { Email: approvalRequest.email }, true)[0].Id;
                                }
                                else if (timesheetItem.statusValue === 'approved_first' || timesheetItem.statusValue === 'rejected_second') {
                                    approvalRequest.fullName = approver['second_approver.human_resources.name_surname.primary'];
                                    approvalRequest.email = approver['second_approver.human_resources.e_mail1'];
                                    approvalRequest.timesheetItemStatus = waitingSecondPicklistItem.id;
                                    approvalRequest['2_approver'] = $filter('filter')($rootScope.users, { Email: approvalRequest.email }, true)[0].Id;
                                }
                                break;
                            case 'management':
                                approver = approvers.management;

                                if (!approver) {
                                    hasNoApprovers = true;
                                    break;
                                }

                                if (timesheetItem.statusValue === 'draft' || timesheetItem.statusValue === 'rejected_first') {
                                    approvalRequest.fullName = approver['first_approver.human_resources.name_surname.primary'];
                                    approvalRequest.email = approver['first_approver.human_resources.e_mail1'];
                                    approvalRequest.timesheetItemStatus = waitingFirstPicklistItem.id;
                                    approvalRequest['1_approver'] = $filter('filter')($rootScope.users, { Email: approvalRequest.email }, true)[0].Id;
                                }
                                else if (timesheetItem.statusValue === 'approved_first' || timesheetItem.statusValue === 'rejected_second') {
                                    approvalRequest.fullName = approver['second_approver.human_resources.name_surname.primary'];
                                    approvalRequest.email = approver['second_approver.human_resources.e_mail1'];
                                    approvalRequest.timesheetItemStatus = waitingSecondPicklistItem.id;
                                    approvalRequest['2_approver'] = $filter('filter')($rootScope.users, { Email: approvalRequest.email }, true)[0].Id;
                                }
                                break;
                        }
                        approvalRequests.push(approvalRequest);
                    }

                    if (hasNoApprovers) {
                        ngToast.create({ content: 'Approvers not found. Please contact your system administrator.', className: 'warning' });
                        $scope.submitting = false;
                        return;
                    }

                    if (!approvalRequests.length && ownerUser) {
                        ngToast.create({ content: 'Timesheet approved succesfully.', className: 'success' });
                        $scope.submitting = false;
                        $scope.approved = true;
                        return;
                    }

                    var template = '<!DOCTYPE html> <html> <head> <meta name="viewport" content="width=device-width"> <meta http-equiv="Content-Type" content="text/html; charset=UTF-8"> <style type="text/css"> @media only screen and (max-width: 620px) { table[class=body] h1 { font-size: 28px !important; margin-bottom: 10px !important; } table[class=body] p, table[class=body] ul, table[class=body] ol, table[class=body] td, table[class=body] span, table[class=body] a { font-size: 16px !important; } table[class=body] .wrapper, table[class=body] .article { padding: 10px !important; } table[class=body] .content { padding: 0 !important; } table[class=body] .container { padding: 0 !important; width: 100% !important; } table[class=body] .main { border-left-width: 0 !important; border-radius: 0 !important; border-right-width: 0 !important; } table[class=body] .btn table { width: 100% !important; } table[class=body] .btn a { width: 100% !important; } table[class=body] .img-responsive { height: auto !important; max-width: 100% !important; width: auto !important; }} @media all { .ExternalClass { width: 100%; } .ExternalClass, .ExternalClass p, .ExternalClass span, .ExternalClass font, .ExternalClass td, .ExternalClass div { line-height: 100%; } .apple-link a { color: inherit !important; font-family: inherit !important; font-size: inherit !important; font-weight: inherit !important; line-height: inherit !important; text-decoration: none !important; } .btn-primary table td:hover { background-color: #34495e !important; } .btn-primary a:hover { background-color: #34495e !important; border-color: #34495e !important; } } </style> </head> <body class="" style="background-color:#f6f6f6;font-family:sans-serif;-webkit-font-smoothing:antialiased;font-size:14px;line-height:1.4;margin:0;padding:0;-ms-text-size-adjust:100%;-webkit-text-size-adjust:100%;"> <table border="0" cellpadding="0" cellspacing="0" class="body" style="border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;background-color:#f6f6f6;width:100%;"> <tr> <td style="font-family:sans-serif;font-size:14px;vertical-align:top;">&nbsp;</td> <td class="container" style="font-family:sans-serif;font-size:14px;vertical-align:top;display:block;max-width:580px;padding:10px;width:580px;Margin:0 auto !important;"> <div class="content" style="box-sizing:border-box;display:block;Margin:0 auto;max-width:580px;padding:10px;"> <!-- START CENTERED WHITE CONTAINER --> <table class="main" style="border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;background:#fff;border-radius:3px;width:100%;"> <!-- START MAIN CONTENT AREA --> <tr> <td class="wrapper" style="font-family:sans-serif;font-size:14px;vertical-align:top;box-sizing:border-box;padding:20px;"> <table border="0" cellpadding="0" cellspacing="0" style="border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;width:100%;"> <tr> <td style="font-family:sans-serif;font-size:14px;vertical-align:top;"> Dear {approver}, <br><br><b>{owner}</b> has requested to check the relevant Timesheet. For checking, please click on the button below. <br>{firstApprover}<br><br>Regards,<br>PGInternational<br><br><table border="0" cellpadding="0" cellspacing="0" class="btn btn-primary" style="border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;box-sizing:border-box;width:100%;"> <tbody> <tr> <td align="left" style="font-family:sans-serif;font-size:14px;vertical-align:top;padding-bottom:15px;"> <table border="0" cellpadding="0" cellspacing="0" style="border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;width:100%;width:auto;"> <tbody> <tr> <td style="font-family:sans-serif;font-size:14px;vertical-align:top;background-color:#ffffff;border-radius:5px;text-align:center;background-color:#3498db;"> <a href="https://bee.pginternational.com/#/app/crm/timesheet?user={user}&project={project}&month={month}{chargeType}" target="_blank" style="text-decoration:underline;background-color:#ffffff;border:solid 1px #3498db;border-radius:5px;box-sizing:border-box;color:#3498db;cursor:pointer;display:inline-block;font-size:14px;font-weight:bold;margin:0;padding:12px 25px;text-decoration:none;background-color:#3498db;border-color:#3498db;color:#ffffff;">Go to {owner}\'s Timesheet</a> </td> </tr> </tbody> </table> </td> </tr> </tbody> </table></td> </tr> </table> </td> </tr> <!-- END MAIN CONTENT AREA --> </table> <!-- START FOOTER --> <div class="footer" style="clear:both;padding-top:10px;text-align:center;width:100%;"> <table border="0" cellpadding="0" cellspacing="0" style="border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;width:100%;"> <tr> <td class="content-block" style="font-family:sans-serif;font-size:14px;vertical-align:top;color:#999999;font-size:12px;text-align:center;"> <br><span class="apple-link" style="color:#999999;font-size:12px;text-align:center;"></span> </td> </tr> </table> </div> <!-- END FOOTER --> <!-- END CENTERED WHITE CONTAINER --> </div> </td> <td style="font-family:sans-serif;font-size:14px;vertical-align:top;">&nbsp;</td> </tr> </table> </body> </html>';
                    template = template.replaceAll('{owner}', user.FullName);
                    template = template.replaceAll('{month}', moment($scope.calendarDay).month());
                    var promises = [];
                    var logs = [];

                    for (var j = 0; j < approvalRequests.length; j++) {
                        var approvalRequest = approvalRequests[j];
                        var currentLog = $filter('filter')(logs, { approverEmail: approvalRequest.email }, true);

                        if (currentLog.length && approvalRequest.projectId)
                            currentLog = $filter('filter')(currentLog, { projectId: approvalRequest.projectId }, true);

                        if (!currentLog || !currentLog.length) {
                            var templateItem = angular.copy(template);
                            templateItem = templateItem.replaceAll('{approver}', approvalRequest.fullName);
                            templateItem = templateItem.replaceAll('{user}', user.Id);
                            templateItem = templateItem.replaceAll('{project}', approvalRequest.projectId || '');

                            if (approvalRequest.chargeType === 'billable')
                                templateItem = templateItem.replaceAll('{chargeType}', '&ctype=0');
                            else if (approvalRequest.chargeType === 'nonbillable')
                                templateItem = templateItem.replaceAll('{chargeType}', '&ctype=1');
                            else if (approvalRequest.chargeType === 'business')
                                templateItem = templateItem.replaceAll('{chargeType}', '&ctype=2');
                            else
                                templateItem = templateItem.replaceAll('{chargeType}', '');

                            templateItem = templateItem.replaceAll('{timesheet}', $scope.currentTimesheet.year + '-' + $scope.currentTimesheet.term);

                            if (humanResource)
                                templateItem = templateItem.replaceAll('{firstApprover}', '<br> It was previously approved by First Approver' + ' ' + '<b>' + humanResource.name_surname + '</b>');
                            else
                                templateItem = templateItem.replaceAll('{firstApprover}', '<br>');

                            var requestMail = {};
                            requestMail.Subject = user.FullName + ' has requested that you approve the Timesheet (' + $scope.currentTimesheet.year + '-' + $scope.currentTimesheet.term + ')';
                            requestMail.TemplateWithBody = templateItem;
                            requestMail.ToAddresses = [approvalRequest.email];

                            if (approvalRequest.timesheetItemStatus !== approvedPicklistItem.id)
                                promises.push($http.post(config.apiUrl + 'messaging/send_external_email', requestMail));

                            var log = {};
                            log.approverEmail = approvalRequest.email;

                            if (approvalRequest.projectId)
                                log.projectId = approvalRequest.projectId;

                            logs.push(log);
                        }

                        var requestUpdate = {};
                        requestUpdate.id = approvalRequest.timesheetItemId;
                        requestUpdate.status = approvalRequest.timesheetItemStatus;

                        if(approvalRequest['1_approver'])
                            requestUpdate['1_approver'] = approvalRequest['1_approver'];

                        if(approvalRequest['2_approver'])
                            requestUpdate['2_approver'] = approvalRequest['2_approver'];

                        promises.push($http.put(config.apiUrl + 'record/update/' + $scope.timesheetItemModule.name + '?timezone_offset=' + new Date().getTimezoneOffset() * -1, requestUpdate));
                    }

                    if (promises.length) {
                        $q.all(promises)
                            .then(function (response) {
                                var hasError = false;

                                angular.forEach(response, function (resp) {
                                    if (hasError)
                                        return;

                                    if (resp.status != 200)
                                        hasError = true;
                                });

                                if (hasError) {
                                    ngToast.create({ content: $filter('translate')('Common.Error'), className: 'danger' });
                                    return;
                                }

                                var requestUpdateTimesheet = {};
                                requestUpdateTimesheet.id = $scope.currentTimesheet.id;
                                requestUpdateTimesheet.status = waitingFirstPicklistItem.id;

                                $http.put(config.apiUrl + 'record/update/' + $scope.timesheetModule.name + '?timezone_offset=' + new Date().getTimezoneOffset() * -1, requestUpdateTimesheet)
                                    .then(function () {
                                        getCurrentTimesheet();

                                        if (!ownerUser) {
                                            ngToast.create({ content: 'Timesheet submitted for approval succesfully.', className: 'success' });
                                        }
                                        else {
                                            ngToast.create({ content: 'Timesheet approved succesfully.', className: 'success' });
                                            $scope.approved = true;
                                        }
                                    })
                                    .finally(function () {
                                        $scope.submitting = false;
                                        $scope.approving = false;
                                    });
                            });
                    }
                    else {
                        $scope.approving = false;
                    }
                });
            };

            $scope.approveApproval = function () {
                $scope.approving = true;

                ModuleService.getPicklists($scope.approverModule)
                    .then(function (picklistsApprover) {
                        var approvalTypeField = $filter('filter')($scope.approverModule.fields, { name: 'approval_type' }, true)[0];
                        var requestApprover = {
                            fields: ['approval_id', 'first_approver', 'first_approver_expert', 'second_approver'],
                            filters: [],
                            limit: 1,
                            offset: 0
                        };

                        if ($scope.projectId) {
                            requestApprover.filters.push({ field: 'related_project', operator: 'equals', value: $scope.projectId, no: 1 });
                            var approvalTypeBillablePicklistItem = $filter('filter')(picklistsApprover[approvalTypeField.picklist_id], { system_code: 'billable' }, true)[0];
                            var approvalTypeNonBillablePicklistItem = $filter('filter')(picklistsApprover[approvalTypeField.picklist_id], { system_code: 'nonbillable' }, true)[0];
                            var approvalTypeBusinessPicklistItem = $filter('filter')(picklistsApprover[approvalTypeField.picklist_id], { system_code: 'business' }, true)[0];

                            if ($scope.chargeType === 'billable')
                                requestApprover.filters.push({ field: 'approval_type', operator: 'is', value: approvalTypeBillablePicklistItem['label_' + $rootScope.user.tenantLanguage], no: 2 });
                            else if ($scope.chargeType === 'nonbillable')
                                requestApprover.filters.push({ field: 'approval_type', operator: 'is', value: approvalTypeNonBillablePicklistItem['label_' + $rootScope.user.tenantLanguage], no: 2 });
                            else if ($scope.chargeType === 'business')
                                requestApprover.filters.push({ field: 'approval_type', operator: 'is', value: approvalTypeBusinessPicklistItem['label_' + $rootScope.user.tenantLanguage], no: 2 });
                        }
                        else {
                            var approvalTypeManagementPicklistItem = $filter('filter')(picklistsApprover[approvalTypeField.picklist_id], { system_code: 'management' }, true)[0];
                            requestApprover.filters.push({ field: 'staff', operator: 'equals', value: $scope.currentStaff.id, no: 1 });
                            requestApprover.filters.push({ field: 'approval_type', operator: 'is', value: approvalTypeManagementPicklistItem['label_' + $rootScope.user.tenantLanguage], no: 2 });
                        }

                        ModuleService.findRecords('approval_workflow', requestApprover)
                            .then(function (approvalResponse) {
                                if (!approvalResponse || !approvalResponse.data[0]) {
                                    ngToast.create({ content: $filter('translate')('Common.Error'), className: 'danger' });
                                    return;
                                }

                                var requestHumanResources = {};
                                requestHumanResources.filters = [{ field: 'e_mail1', operator: 'is', value: $rootScope.user.email, no: 1 }];

                                var success = function (humanResourceResponse) {
                                    var humanResource = humanResourceResponse.data[0];
                                    var approval = approvalResponse.data[0];
                                    var statusFieldTimesheetItem = $filter('filter')($scope.timesheetItemModule.fields, { name: 'status' }, true)[0];
                                    var approvedFirstPicklistItem = $filter('filter')($scope.picklistsTimesheetItems[statusFieldTimesheetItem.picklist_id], { value: 'approved_first' }, true)[0];
                                    var approvedSecondPicklistItem = $filter('filter')($scope.picklistsTimesheetItems[statusFieldTimesheetItem.picklist_id], { value: 'approved_second' }, true)[0];
                                    var timesheetItemOwnerId = 0;
                                    var promises = [];

                                    for (var i = 0; i < $scope.timesheetItems.length; i++) {
                                        var timesheetItem = $scope.timesheetItems[i];

                                        if (timesheetItem.statusValue === 'waiting_first' && approval.second_approver) {
                                            timesheetItem.statusValue = 'approved_first';
                                            timesheetItem.status = approvedFirstPicklistItem;
                                        }
                                        else if (timesheetItem.statusValue === 'waiting_first' && !approval.second_approver) {
                                            timesheetItem.statusValue = 'approved_second';
                                            timesheetItem.status = approvedSecondPicklistItem;
                                        }
                                        else if (timesheetItem.statusValue === 'waiting_second' && humanResource && approval['second_approver'] === humanResource.id) {
                                            timesheetItem.statusValue = 'approved_second';
                                            timesheetItem.status = approvedSecondPicklistItem;
                                        }
                                        else {
                                            continue;
                                        }

                                        var currentTimesheetItem = $filter('filter')($scope.currentTimesheetItems, { id: timesheetItem.id }, true)[0];
                                        currentTimesheetItem.statusValue = timesheetItem.statusValue;
                                        timesheetItemOwnerId = timesheetItem.owner;

                                        var requestUpdate = {};
                                        requestUpdate.id = timesheetItem.id;
                                        requestUpdate.status = timesheetItem.status.id;

                                        promises.push($http.put(config.apiUrl + 'record/update/' + $scope.timesheetItemModule.name + '?timezone_offset=' + new Date().getTimezoneOffset() * -1, requestUpdate));
                                    }

                                    if (!promises.length) {
                                        ngToast.create({
                                            content: 'You already approved or rejected this timesheet.',
                                            className: 'warning'
                                        });
                                        $scope.approving = false;
                                        $scope.approved = true;
                                        return;
                                    }

                                    $q.all(promises)
                                        .then(function (response) {
                                            var hasError = false;

                                            angular.forEach(response, function (resp) {
                                                if (hasError)
                                                    return;

                                                if (resp.status != 200)
                                                    hasError = true;
                                            });

                                            if (hasError) {
                                                ngToast.create({
                                                    content: $filter('translate')('Common.Error'),
                                                    className: 'danger'
                                                });
                                                return;
                                            }

                                            var ownerUser = $filter('filter')($rootScope.users, { Id: timesheetItemOwnerId }, true)[0];
                                            $scope.submitApproval(ownerUser, false, humanResource);
                                        });
                                };

                                if (approvalResponse.data[0].first_approver_expert && !approvalResponse.data[0].first_approver) {
                                    ModuleService.findRecords('experts', requestHumanResources)
                                        .then(function (humanResourceResponse) {
                                            if (!humanResourceResponse.data.length) {
                                                ModuleService.findRecords('human_resources', requestHumanResources)
                                                    .then(function (humanResourceResponse) {
                                                        success(humanResourceResponse)
                                                    })
                                            }
                                            else
                                                success(humanResourceResponse);
                                        })
                                } else {
                                    ModuleService.findRecords('human_resources', requestHumanResources)
                                        .then(function (humanResourceResponse) {
                                            success(humanResourceResponse)
                                        })
                                }
                            });
                    });
            };

            $scope.openRejectApprovalModal = function () {
                $scope.rejectModal = $scope.rejectModal || $modal({
                    scope: $scope,
                    templateUrl: 'views/app/timesheet/rejectModal.html',
                    animation: '',
                    backdrop: 'static',
                    show: false,
                    tag: 'createModal'
                });

                $scope.rejectModal.$promise.then($scope.rejectModal.show);
            };

            $scope.rejectApproval = function (message) {
                $scope.rejecting = true;

                ModuleService.getPicklists($scope.approverModule)
                    .then(function (picklistsApprover) {
                        var approvalTypeField = $filter('filter')($scope.approverModule.fields, { name: 'approval_type' }, true)[0];
                        var requestApprover = {
                            fields: ['approval_id', 'first_approver', 'first_approver_expert', 'second_approver'],
                            filters: [],
                            limit: 1,
                            offset: 0
                        };

                        if ($scope.projectId) {
                            requestApprover.filters.push({ field: 'related_project', operator: 'equals', value: $scope.projectId, no: 1 });
                            var approvalTypeBillablePicklistItem = $filter('filter')(picklistsApprover[approvalTypeField.picklist_id], { system_code: 'billable' }, true)[0];
                            var approvalTypeNonBillablePicklistItem = $filter('filter')(picklistsApprover[approvalTypeField.picklist_id], { system_code: 'nonbillable' }, true)[0];
                            var approvalTypeBusinessPicklistItem = $filter('filter')(picklistsApprover[approvalTypeField.picklist_id], { system_code: 'business' }, true)[0];

                            if ($scope.chargeType === 'billable')
                                requestApprover.filters.push({ field: 'approval_type', operator: 'is', value: approvalTypeBillablePicklistItem['label_' + $rootScope.user.tenantLanguage], no: 2 });
                            else if ($scope.chargeType === 'nonbillable')
                                requestApprover.filters.push({ field: 'approval_type', operator: 'is', value: approvalTypeNonBillablePicklistItem['label_' + $rootScope.user.tenantLanguage], no: 2 });
                            else if ($scope.chargeType === 'business')
                                requestApprover.filters.push({ field: 'approval_type', operator: 'is', value: approvalTypeBusinessPicklistItem['label_' + $rootScope.user.tenantLanguage], no: 2 });
                        }
                        else {
                            var approvalTypeManagementPicklistItem = $filter('filter')(picklistsApprover[approvalTypeField.picklist_id], { system_code: 'management' }, true)[0];
                            requestApprover.filters.push({ field: 'staff', operator: 'equals', value: $scope.currentStaff.id, no: 1 });
                            requestApprover.filters.push({ field: 'approval_type', operator: 'is', value: approvalTypeManagementPicklistItem['label_' + $rootScope.user.tenantLanguage], no: 2 });
                        }

                        ModuleService.findRecords('approval_workflow', requestApprover)
                            .then(function (approvalResponse) {
                                if (!approvalResponse || !approvalResponse.data[0]) {
                                    ngToast.create({ content: $filter('translate')('Common.Error'), className: 'danger' });
                                    return;
                                }

                                var requestHumanResources = {};
                                requestHumanResources.filters = [{ field: 'e_mail1', operator: 'is', value: $rootScope.user.email, no: 1 }];

                                ModuleService.findRecords('human_resources', requestHumanResources)
                                    .then(function (humanResourceResponse) {
                                        var humanResource = humanResourceResponse.data[0];
                                        var approval = approvalResponse.data[0];
                                        var statusFieldTimesheetItem = $filter('filter')($scope.timesheetItemModule.fields, { name: 'status' }, true)[0];
                                        var rejectedFirstPicklistItem = $filter('filter')($scope.picklistsTimesheetItems[statusFieldTimesheetItem.picklist_id], { value: 'rejected_first' }, true)[0];
                                        var rejectedSecondPicklistItem = $filter('filter')($scope.picklistsTimesheetItems[statusFieldTimesheetItem.picklist_id], { value: 'rejected_second' }, true)[0];
                                        var rejectPicklistItemId = 0;
                                        var timesheetItemOwnerId = 0;
                                        var promises = [];

                                        for (var i = 0; i < $scope.timesheetItems.length; i++) {
                                            var timesheetItem = $scope.timesheetItems[i];

                                            if (timesheetItem.statusValue === 'waiting_first') {
                                                rejectPicklistItemId = rejectedFirstPicklistItem.id;
                                            }
                                            else if (timesheetItem.statusValue === 'waiting_second' && humanResource && approval['second_approver'] === humanResource.id) {
                                                rejectPicklistItemId = rejectedFirstPicklistItem.id;
                                            }
                                            else {
                                                continue;
                                            }

                                            timesheetItemOwnerId = timesheetItem.owner;
                                            var requestUpdate = {};
                                            requestUpdate.id = timesheetItem.id;
                                            requestUpdate.status = rejectPicklistItemId;

                                            promises.push($http.put(config.apiUrl + 'record/update/' + $scope.timesheetItemModule.name + '?timezone_offset=' + new Date().getTimezoneOffset() * -1, requestUpdate));
                                        }

                                        if (!promises.length) {
                                            ngToast.create({ content: 'You already rejected or approved this timesheet.', className: 'warning' });
                                            $scope.approving = false;
                                            $scope.approved = true;
                                            return;
                                        }

                                        var ownerUser = $filter('filter')($rootScope.users, { Id: timesheetItemOwnerId }, true)[0];
                                        var requestUpdateTimesheet = {};
                                        requestUpdateTimesheet.id = $scope.currentTimesheet.id;
                                        requestUpdateTimesheet.status = rejectPicklistItemId;

                                        promises.push($http.put(config.apiUrl + 'record/update/' + $scope.timesheetModule.name + '?timezone_offset=' + new Date().getTimezoneOffset() * -1, requestUpdateTimesheet));

                                        $q.all(promises)
                                            .then(function (response) {
                                                var hasError = false;

                                                angular.forEach(response, function (resp) {
                                                    if (hasError)
                                                        return;

                                                    if (resp.status != 200)
                                                        hasError = true;
                                                });

                                                if (hasError) {
                                                    ngToast.create({ content: $filter('translate')('Common.Error'), className: 'danger' });
                                                    return;
                                                }

                                                var template = '<!DOCTYPE html> <html> <head> <meta name="viewport" content="width=device-width"> <meta http-equiv="Content-Type" content="text/html; charset=UTF-8"> <title></title> <style type="text/css"> @media only screen and (max-width: 620px) { table[class=body] h1 { font-size: 28px !important; margin-bottom: 10px !important; } table[class=body] p, table[class=body] ul, table[class=body] ol, table[class=body] td, table[class=body] span, table[class=body] a { font-size: 16px !important; } table[class=body] .wrapper, table[class=body] .article { padding: 10px !important; } table[class=body] .content { padding: 0 !important; } table[class=body] .container { padding: 0 !important; width: 100% !important; } table[class=body] .main { border-left-width: 0 !important; border-radius: 0 !important; border-right-width: 0 !important; } table[class=body] .btn table { width: 100% !important; } table[class=body] .btn a { width: 100% !important; } table[class=body] .img-responsive { height: auto !important; max-width: 100% !important; width: auto !important; }} @media all { .ExternalClass { width: 100%; } .ExternalClass, .ExternalClass p, .ExternalClass span, .ExternalClass font, .ExternalClass td, .ExternalClass div { line-height: 100%; } .apple-link a { color: inherit !important; font-family: inherit !important; font-size: inherit !important; font-weight: inherit !important; line-height: inherit !important; text-decoration: none !important; } .btn-primary table td:hover { background-color: #34495e !important; } .btn-primary a:hover { background-color: #34495e !important; border-color: #34495e !important; } } </style> </head> <body class="" style="background-color:#f6f6f6;font-family:sans-serif;-webkit-font-smoothing:antialiased;font-size:14px;line-height:1.4;margin:0;padding:0;-ms-text-size-adjust:100%;-webkit-text-size-adjust:100%;"> <table border="0" cellpadding="0" cellspacing="0" class="body" style="border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;background-color:#f6f6f6;width:100%;"> <tr> <td style="font-family:sans-serif;font-size:14px;vertical-align:top;">&nbsp;</td> <td class="container" style="font-family:sans-serif;font-size:14px;vertical-align:top;display:block;max-width:580px;padding:10px;width:580px;Margin:0 auto !important;"> <div class="content" style="box-sizing:border-box;display:block;Margin:0 auto;max-width:580px;padding:10px;"> <!-- START CENTERED WHITE CONTAINER --> <table class="main" style="border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;background:#fff;border-radius:3px;width:100%;"> <!-- START MAIN CONTENT AREA --> <tr> <td class="wrapper" style="font-family:sans-serif;font-size:14px;vertical-align:top;box-sizing:border-box;padding:20px;"> <table border="0" cellpadding="0" cellspacing="0" style="border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;width:100%;"> <tr> <td style="font-family:sans-serif;font-size:14px;vertical-align:top;"> Dear {owner}, <br><br><b>{approver}</b> has rejected {timesheetOwner} ({timesheet}). Please see the message below and revise the Timesheet by clicking on the button below. <br><br> Message: &nbsp;{message}<br><br><br><br><table border="0" cellpadding="0" cellspacing="0" class="btn btn-primary" style="border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;box-sizing:border-box;width:100%;"> <tbody> <tr> <td align="left" style="font-family:sans-serif;font-size:14px;vertical-align:top;padding-bottom:15px;"> <table border="0" cellpadding="0" cellspacing="0" style="border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;width:100%;width:auto;"> <tbody> <tr> <td style="font-family:sans-serif;font-size:14px;vertical-align:top;background-color:#ffffff;border-radius:5px;text-align:center;background-color:#3498db;"> <a href="https://bee.pginternational.com/#/app/crm/timesheet?month={month}" target="_blank" style="text-decoration:underline;background-color:#ffffff;border:solid 1px #3498db;border-radius:5px;box-sizing:border-box;color:#3498db;cursor:pointer;display:inline-block;font-size:14px;font-weight:bold;margin:0;padding:12px 25px;text-decoration:none;background-color:#3498db;border-color:#3498db;color:#ffffff;">Go to {timesheetOwner}</a> </td> </tr> </tbody> </table> </td> </tr> </tbody> </table></td> </tr> </table> </td> </tr> <!-- END MAIN CONTENT AREA --> </table> <!-- START FOOTER --> <div class="footer" style="clear:both;padding-top:10px;text-align:center;width:100%;"> <table border="0" cellpadding="0" cellspacing="0" style="border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;width:100%;"> <tr> <td class="content-block" style="font-family:sans-serif;font-size:14px;vertical-align:top;color:#999999;font-size:12px;text-align:center;"> <br><span class="apple-link" style="color:#999999;font-size:12px;text-align:center;"></span> </td> </tr> </table> </div> <!-- END FOOTER --> <!-- END CENTERED WHITE CONTAINER --> </div> </td> <td style="font-family:sans-serif;font-size:14px;vertical-align:top;">&nbsp;</td> </tr> </table> </body> </html>';
                                                template = template.replaceAll('{owner}', ownerUser.FullName);
                                                template = template.replaceAll('{approver}', $rootScope.user.fullName);
                                                template = template.replaceAll('{message}', message.replace(/(?:\r\n|\r|\n)/g, '<br>'));
                                                template = template.replaceAll('{timesheet}', $scope.currentTimesheet.year + '-' + $scope.currentTimesheet.term);
                                                template = template.replaceAll('{month}', moment($scope.calendarDay).month());
                                                template = template.replaceAll('{timesheetOwner}', 'Your Timesheet');

                                                var requestMail = {};
                                                requestMail.Subject = $rootScope.user.fullName + ' has rejected your Timesheet (' + $scope.currentTimesheet.year + '-' + $scope.currentTimesheet.term + ')';
                                                requestMail.TemplateWithBody = template;
                                                requestMail.ToAddresses = [ownerUser.Email];

                                                $http.post(config.apiUrl + 'messaging/send_external_email', requestMail)
                                                    .then(function () {
                                                        if ($scope.currentTimesheetItems[0].statusValue === 'waiting_second') {
                                                            var requestFirstApproverInfo = {};
                                                            var firstApprover;
                                                            if ($scope.currentTimesheetItems[0].approverType === 'billable')
                                                                firstApprover = $scope.approvers.first_approver_expert;
                                                            else
                                                                firstApprover = $scope.approvers.first_approver;

                                                            requestFirstApproverInfo.filters = [{
                                                                field: 'id',
                                                                operator: 'equals',
                                                                value: firstApprover,
                                                                no: 1
                                                            }];


                                                            var success = function (firstApproverInfo) {
                                                                var templateReject = '<!DOCTYPE html> <html> <head> <meta name="viewport" content="width=device-width"> <meta http-equiv="Content-Type" content="text/html; charset=UTF-8"> <title></title> <style type="text/css"> @media only screen and (max-width: 620px) { table[class=body] h1 { font-size: 28px !important; margin-bottom: 10px !important; } table[class=body] p, table[class=body] ul, table[class=body] ol, table[class=body] td, table[class=body] span, table[class=body] a { font-size: 16px !important; } table[class=body] .wrapper, table[class=body] .article { padding: 10px !important; } table[class=body] .content { padding: 0 !important; } table[class=body] .container { padding: 0 !important; width: 100% !important; } table[class=body] .main { border-left-width: 0 !important; border-radius: 0 !important; border-right-width: 0 !important; } table[class=body] .btn table { width: 100% !important; } table[class=body] .btn a { width: 100% !important; } table[class=body] .img-responsive { height: auto !important; max-width: 100% !important; width: auto !important; }} @media all { .ExternalClass { width: 100%; } .ExternalClass, .ExternalClass p, .ExternalClass span, .ExternalClass font, .ExternalClass td, .ExternalClass div { line-height: 100%; } .apple-link a { color: inherit !important; font-family: inherit !important; font-size: inherit !important; font-weight: inherit !important; line-height: inherit !important; text-decoration: none !important; } .btn-primary table td:hover { background-color: #34495e !important; } .btn-primary a:hover { background-color: #34495e !important; border-color: #34495e !important; } } </style> </head> <body class="" style="background-color:#f6f6f6;font-family:sans-serif;-webkit-font-smoothing:antialiased;font-size:14px;line-height:1.4;margin:0;padding:0;-ms-text-size-adjust:100%;-webkit-text-size-adjust:100%;"> <table border="0" cellpadding="0" cellspacing="0" class="body" style="border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;background-color:#f6f6f6;width:100%;"> <tr> <td style="font-family:sans-serif;font-size:14px;vertical-align:top;">&nbsp;</td> <td class="container" style="font-family:sans-serif;font-size:14px;vertical-align:top;display:block;max-width:580px;padding:10px;width:580px;Margin:0 auto !important;"> <div class="content" style="box-sizing:border-box;display:block;Margin:0 auto;max-width:580px;padding:10px;"> <!-- START CENTERED WHITE CONTAINER --> <table class="main" style="border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;background:#fff;border-radius:3px;width:100%;"> <!-- START MAIN CONTENT AREA --> <tr> <td class="wrapper" style="font-family:sans-serif;font-size:14px;vertical-align:top;box-sizing:border-box;padding:20px;"> <table border="0" cellpadding="0" cellspacing="0" style="border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;width:100%;"> <tr> <td style="font-family:sans-serif;font-size:14px;vertical-align:top;"> Dear {owner}, <br><br><b>{approver}</b> has rejected {timesheetOwner} ({timesheet}). {timesheetOwner} will be requested  that you approve the Timesheet.  <br><br> Message: &nbsp; {message}<br><br><br><br><!-- START FOOTER --> <div class="footer" style="clear:both;padding-top:10px;text-align:center;width:100%;"> <table border="0" cellpadding="0" cellspacing="0" style="border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;width:100%;"> <tr> <td class="content-block" style="font-family:sans-serif;font-size:14px;vertical-align:top;color:#999999;font-size:12px;text-align:center;"> <br><span class="apple-link" style="color:#999999;font-size:12px;text-align:center;"></span> </td> </tr> </table> </div> <!-- END FOOTER --> <!-- END CENTERED WHITE CONTAINER --> </div> </td> <td style="font-family:sans-serif;font-size:14px;vertical-align:top;">&nbsp;</td> </tr> </table> </body> </html>';
                                                                templateReject = templateReject.replaceAll('{owner}', ownerUser.FullName);
                                                                templateReject = templateReject.replaceAll('{approver}', $rootScope.user.fullName);
                                                                templateReject = templateReject.replaceAll('{message}', message.replace(/(?:\r\n|\r|\n)/g, '<br>'));
                                                                templateReject = templateReject.replaceAll('{timesheet}', $scope.currentTimesheet.year + '-' + $scope.currentTimesheet.term);
                                                                templateReject = templateReject.replaceAll('{month}', moment($scope.calendarDay).month());
                                                                templateReject = templateReject.replaceAll('{timesheetOwner}', 'Your Timesheet');
                                                                templateReject = templateReject.replaceAll(ownerUser.FullName, firstApproverInfo.data[0].name_surname);
                                                                templateReject = templateReject.replaceAll('Your Timesheet', '<b>' + $scope.timesheetTitle + '</b>');

                                                                var requestMailToFirstApprover = {};
                                                                requestMailToFirstApprover.Subject = $rootScope.user.fullName + ' has rejected ' + $scope.timesheetTitle + '(' + $scope.currentTimesheet.year + '-' + $scope.currentTimesheet.term + ')';
                                                                requestMailToFirstApprover.TemplateWithBody = templateReject;
                                                                requestMailToFirstApprover.ToAddresses = [firstApproverInfo.data[0].e_mail1];

                                                                $http.post(config.apiUrl + 'messaging/send_external_email', requestMailToFirstApprover)
                                                                    .then(function () {
                                                                        ngToast.create({
                                                                            content: 'Timesheet rejected succesfully.',
                                                                            className: 'success'
                                                                        });
                                                                        $scope.rejecting = false;
                                                                        $scope.rejected = true;
                                                                        $scope.rejectModal.hide();
                                                                    });
                                                            };

                                                            if ($scope.currentTimesheetItems[0].approverType === 'billable') {
                                                                ModuleService.findRecords('experts', requestFirstApproverInfo)
                                                                    .then(function (firstApproverInfo) {
                                                                        success(firstApproverInfo)
                                                                    })
                                                            } else {
                                                                ModuleService.findRecords('human_resources', requestFirstApproverInfo)
                                                                    .then(function (firstApproverInfo) {
                                                                        if (ownerUser.Email != firstApproverInfo.data[0].e_mail1)
                                                                            success(firstApproverInfo)
                                                                        else {
                                                                            ngToast.create({
                                                                                content: 'Timesheet rejected succesfully.',
                                                                                className: 'success'
                                                                            });
                                                                            $scope.rejecting = false;
                                                                            $scope.rejected = true;
                                                                            $scope.rejectModal.hide();
                                                                        }
                                                                    })
                                                            }

                                                        } else {
                                                            ngToast.create({
                                                                content: 'Timesheet rejected succesfully.',
                                                                className: 'success'
                                                            });
                                                            $scope.rejecting = false;
                                                            $scope.rejected = true;
                                                            $scope.rejectModal.hide();
                                                        }
                                                    });
                                            });
                                    });
                            });
                    });
            };


            $scope.showModuleFrameModal = function () {
                $scope.timesheetUrl = 'https://timesheet.projectgroup.com.tr/?id=' + $scope.owner.Id;
                $scope.frameModal = $scope.frameModal || $modal({
                    scope: $scope,
                    controller: 'TimesheetFrameController',
                    templateUrl: 'views/app/timesheet/timesheetFrame.html',
                    backdrop: 'static',
                    show: false
                });

                $scope.frameModal.$promise.then($scope.frameModal.show);
            };
        }]);