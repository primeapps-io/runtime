'use strict';

angular.module('ofisim')

    .controller('CalendarController', ['$rootScope', '$scope', '$state', '$filter', 'activityTypes', '$templateCache', '$modal', '$timeout', '$cache', '$dropdown', 'ModuleService', 'calendarConfig', '$http', 'config',
        function ($rootScope, $scope, $state, $filter, activityTypes, $templateCache, $modal, $timeout, $cache, $dropdown, ModuleService, calendarConfig, $http, config) {
            var language = window.localStorage['NG_TRANSLATE_LANG_KEY'] || 'tr';
            var locale = window.localStorage['locale_key'] || language;

            if (!$rootScope.user.profile.HasAdminRights) {
                $http.get(config.apiUrl + 'settings/get_by_key/custom/calendar_delete_show').then(function (result) {
                    $scope.calendarDeleteShow = result.data.value;
                });
            }

            calendarConfig.dateFormatter = 'angular';
            calendarConfig.allDateFormats.angular.title.month = 'MMMM yyyy';
            calendarConfig.allDateFormats.angular.title.year = 'yyyy';

            $scope.users = angular.copy($rootScope.users);

            $scope.users.unshift({Id: 0, FullName: $filter('translate')('Tasks.AllUsers'), IsActive: true});
            $scope.filter = { owner: $scope.users[0] };

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

            $scope.moduleActivity = $filter('filter')($rootScope.modules, { name: 'activities' }, true)[0];
            $scope.modules = $filter('filter')($rootScope.modules, { display_calendar: true, name: '!activities' }, true);
            $scope.calendarView = 'month';
            $scope.calendarDay = new Date();
            $scope.calendarTitle = '';
            $scope.events = [];
			$scope.activityPermissions = $filter('filter')($rootScope.user.profile.permissions, { module_id: $scope.moduleActivity.id}, true)[0];

            $templateCache.put('views/app/calendar/templates/calendarMonthCell.html', "<div mwl-droppable on-drop=\"vm.handleEventDrop(dropData.event, day.date, dropData.draggedFromDate)\" mwl-drag-select=\"!!vm.onDateRangeSelect\" on-drag-select-start=\"vm.onDragSelectStart(day)\" on-drag-select-move=\"vm.onDragSelectMove(day)\" on-drag-select-end=\"vm.onDragSelectEnd(day)\" class=\"cal-month-day {{ day.cssClass }}\" ng-class=\"{ 'cal-day-outmonth': !day.inMonth, 'cal-day-inmonth': day.inMonth, 'cal-day-weekend': day.isWeekend, 'cal-day-past': day.isPast, 'cal-day-today': day.isToday, 'cal-day-future': day.isFuture, 'cal-day-selected': vm.dateRangeSelect && vm.dateRangeSelect.startDate <= day.date && day.date <= vm.dateRangeSelect.endDate, 'cal-day-open': dayIndex === vm.openDayIndex }\">  <span class=\"pull-right\" data-cal-date ng-click=\"vm.calendarCtrl.dateClicked(day.date)\" ng-bind=\"day.label\"> </span>  <div class=\"add-button\"> <a href class=\"btn btn-xs btn-default\" ng-click=\"vm.templateScope.openCreateModal($event, day)\">+</a> </div>  <div class=\"counts\"> <span ng-repeat=\"(type, group) in day.groups track by type\" data-title=\"{{group.module}}\" data-trigger=\"hover\" data-placement=\"top\" bs-tooltip> <span class=\"label\" ng-style=\"{'background-color': group.color}\"> {{ group.events.length }} </span>&nbsp; </span> </div>  <div class=\"cal-day-tick\" ng-show=\"dayIndex === vm.openDayIndex && (vm.cellAutoOpenDisabled || vm.view[vm.openDayIndex].events.length > 0) && !vm.slideBoxDisabled\"> <i class=\"glyphicon glyphicon-chevron-up\"></i> <i class=\"fa fa-chevron-up\"></i> </div>  <ng-include src=\"vm.customTemplateUrls.calendarMonthCellEvents || vm.calendarConfig.templates.calendarMonthCellEvents\"></ng-include>  <div id=\"cal-week-box\" ng-if=\"$first && rowHovered\"> <span ng-bind=\"vm.getWeekNumberLabel(day)\"></span> </div>  </div>");
            $templateCache.put('views/app/calendar/templates/calendarSlideBox.html', "<div   class=\"cal-slide-box\" uib-collapse=\"vm.isCollapsed\" mwl-collapse-fallback=\"vm.isCollapsed\"> <div class=\"cal-slide-content cal-event-list\"> <ul class=\"unstyled list-unstyled\"> <li ng-repeat=\"event in vm.events | orderBy:'startsAt' track by event.calendarEventId\"  class=\"event-line\" ng-class=\"event.cssClass\" mwl-draggable=\"event.draggable === true\" drop-data=\"{event: event}\"> <span class=\"event-bullet\" ng-style=\"{backgroundColor: event.color.primary}\" data-title=\"{{event.module}}\" data-trigger=\"hover\" data-placement=\"right\" bs-tooltip></span>  <a href=\"#/app/crm/module/{{event.type}}{{'?id=' + event.id + '&back=calendar'}}\" class=\"event-item\"> <span ng-bind-html=\"isMonthView ? vm.calendarEventTitle.monthView(event) : vm.calendarEventTitle.yearView(event) | calendarTrustAsHtml\"></span> </a>  <span class=\"action-buttons\"> <a  href=\"#/app/crm/moduleForm/{{event.type}}{{'?id=' + event.id + '&back=calendar'}}\"  ng-show=\"event.permissions.Modify\" class=\"action-icon\" title=\"{{'Common.Edit' | translate}}\"><i class=\"flaticon-pencil124\"></i></a> <i ng-show=\"event.permissions.Remove && event.calendardeleteshow !='active'\"  class=\"action-icon flaticon-bin9\" confirm-click action=\"vm.templateScope.delete(event)\" placement=\"left\" confirm-message=\"{{'Common.AreYouSure' | translate}}\" confirm-yes=\"{{'Common.Yes' | translate}}\" confirm-no=\"{{'Common.No' | translate}}\" title=\"{{'Common.Delete' | translate}}\"></i> </span> </li> </ul> </div> </div>");
            $templateCache.put('views/app/calendar/templates/calendarYearView.html', "<div class=\"cal-year-box\"> <div ng-repeat=\"rowOffset in [0, 4, 8] track by rowOffset\"> <div class=\"row cal-before-eventlist\"> <div class=\"span3 col-md-3 col-xs-6 cal-cell {{ day.cssClass }}\" ng-repeat=\"month in vm.view | calendarLimitTo:4:rowOffset track by $index\" ng-init=\"monthIndex = vm.view.indexOf(month)\" ng-click=\"vm.calendarCtrl.dateClicked(month.date)\" ng-class=\"{'cal-day-today': month.isToday}\" mwl-droppable on-drop=\"vm.handleEventDrop(dropData.event, month.date)\">  <span class=\"pull-right\" data-cal-date ng-click=\"vm.calendarCtrl.dateClicked(month.date)\" ng-bind=\"month.label\"> </span>  <div class=\"counts\"> <span ng-repeat=\"(type, group) in month.groups track by type\" data-title=\"{{group.module}}\" data-trigger=\"hover\" data-placement=\"top\" bs-tooltip> <span class=\"label\" ng-style=\"{'background-color': group.color}\"> {{ group.events.length }} </span>&nbsp; </span> </div>  <div class=\"cal-day-tick\" ng-show=\"monthIndex === vm.openMonthIndex && (vm.cellAutoOpenDisabled || vm.view[vm.openMonthIndex].events.length > 0) && !vm.slideBoxDisabled\"> <i class=\"glyphicon glyphicon-chevron-up\"></i> <i class=\"fa fa-chevron-up\"></i> </div>  </div> </div>  </div>  </div>");
            $templateCache.put('views/app/calendar/templates/calendarDayView.html', "<div class=\"cal-week-box cal-all-day-events-box\" ng-if=\"vm.allDayEvents.length > 0\"> <div class=\"cal-day-panel clearfix\"> <div class=\"row\"> <div class=\"col-xs-12\"> <div class=\"cal-row-fluid\"> <div class=\"cal-cell-6 day-highlight\" ng-style=\"{backgroundColor: event.color.secondary}\" data-event-class ng-repeat=\"event in vm.allDayEvents track by event.calendarEventId\"> <strong>{{'Calendar.AllDay' | translate}}</strong> <a href=\"#/app/crm/module/{{event.type}}{{'?id=' + event.id + '&back=calendar'}}\" class=\"event-item\" ng-bind-html=\"vm.calendarEventTitle.dayView(event) | calendarTrustAsHtml\"> </a> </div> </div> </div> </div> </div> </div>  <div class=\"cal-day-box\"> <div class=\"cal-day-panel clearfix\" ng-style=\"{height: vm.dayViewHeight + 'px', minWidth: vm.viewWidth + 'px'}\">  <mwl-calendar-hour-list day-view-start=\"vm.dayViewStart\" day-view-end=\"vm.dayViewEnd\" day-view-split=\"vm.dayViewSplit\" on-timespan-click=\"vm.onTimespanClick\" on-date-range-select=\"vm.onDateRangeSelect\" on-event-times-changed=\"vm.onEventTimesChanged\" view-date=\"vm.viewDate\" custom-template-urls=\"vm.customTemplateUrls\" template-scope=\"vm.templateScope\" cell-modifier=\"vm.cellModifier\"> </mwl-calendar-hour-list>  <div class=\"pull-left day-event day-highlight\" ng-repeat=\"dayEvent in vm.nonAllDayEvents track by dayEvent.event.calendarEventId\" ng-class=\"dayEvent.event.cssClass\" ng-style=\"{ top: dayEvent.top - 1 + 'px', left: dayEvent.left + 60 + 'px', height: dayEvent.height + 'px', width: dayEvent.width + 'px', backgroundColor: dayEvent.event.color.secondary, borderColor: dayEvent.event.color.primary }\" mwl-draggable=\"dayEvent.event.draggable === true\" axis=\"'xy'\" snap-grid=\"{y: vm.dayViewEventChunkSize || 30, x: 50}\" on-drag=\"vm.eventDragged(dayEvent.event, y / 30)\" on-drag-end=\"vm.eventDragComplete(dayEvent.event, y / 30)\" mwl-resizable=\"dayEvent.event.resizable === true && dayEvent.event.endsAt\" resize-edges=\"{top: true, bottom: true}\" on-resize=\"vm.eventResized(dayEvent.event, edge, y / 30)\" on-resize-end=\"vm.eventResizeComplete(dayEvent.event, edge, y / 30)\" uib-tooltip-html=\"vm.calendarEventTitle.dayViewTooltip(dayEvent.event) | calendarTrustAsHtml\" tooltip-append-to-body=\"true\">  <span class=\"cal-hours\"> <span ng-show=\"dayEvent.top == 0\"><span ng-bind=\"(dayEvent.event.tempStartsAt || dayEvent.event.startsAt) | calendarDate:'day':true\"></span>, </span> <span ng-bind=\"(dayEvent.event.tempStartsAt || dayEvent.event.startsAt) | calendarDate:'time':true\"></span> </span> <a href=\"#/app/crm/module/{{dayEvent.event.type}}{{'?id=' + dayEvent.event.id + '&back=calendar'}}\" class=\"event-item\" ng-click=\"vm.onEventClick({calendarEvent: dayEvent.event})\"> <span ng-bind-html=\"vm.calendarEventTitle.dayView(dayEvent.event) | calendarTrustAsHtml\"></span> </a>  <a href=\"javascript:;\" class=\"event-item-action\" ng-repeat=\"action in dayEvent.event.actions track by $index\" ng-class=\"action.cssClass\" ng-bind-html=\"action.label | calendarTrustAsHtml\" ng-click=\"action.onClick({calendarEvent: dayEvent.event})\"> </a>  </div>  </div>  </div>");
            $templateCache.put('views/app/calendar/templates/calendarWeekView.html', "<div class=\"cal-week-box\" ng-class=\"{'cal-day-box': vm.showTimes}\"> <div class=\"cal-row-fluid cal-row-head\">  <div class=\"cal-cell1\" ng-repeat=\"day in vm.view.days track by $index\" ng-class=\"{ 'cal-day-weekend': day.isWeekend, 'cal-day-past': day.isPast, 'cal-day-today': day.isToday, 'cal-day-future': day.isFuture}\" mwl-element-dimensions=\"vm.dayColumnDimensions\" mwl-droppable on-drop=\"vm.eventDropped(dropData.event, day.date)\">  <span ng-bind=\"day.weekDayLabel\"></span> <br> <small> <span data-cal-date ng-click=\"vm.calendarCtrl.dateClicked(day.date)\" class=\"pointer\" ng-bind=\"day.dayLabel\"> </span> </small>  </div>  </div>  <div class=\"cal-day-panel clearfix\" ng-style=\"{height: vm.showTimes ? (vm.dayViewHeight + 'px') : 'auto'}\">  <mwl-calendar-hour-list day-view-start=\"vm.dayViewStart\" day-view-end=\"vm.dayViewEnd\" day-view-split=\"vm.dayViewSplit\" day-width=\"vm.dayColumnDimensions.width\" view-date=\"vm.viewDate\" on-timespan-click=\"vm.onTimespanClick\" on-date-range-select=\"vm.onDateRangeSelect\" custom-template-urls=\"vm.customTemplateUrls\" cell-modifier=\"vm.cellModifier\" template-scope=\"vm.templateScope\" ng-if=\"vm.showTimes\"> </mwl-calendar-hour-list>  <div class=\"row\" ng-repeat=\"row in vm.view.eventRows track by $index\"> <div class=\"col-xs-12\"> <div class=\"cal-row-fluid\"> <div ng-repeat=\"eventRow in row.row track by eventRow.event.calendarEventId\" ng-class=\"'cal-cell' + (vm.showTimes ? 1 : eventRow.span) + (vm.showTimes ? '' : ' cal-offset' + eventRow.offset)\" ng-style=\"{ top: vm.showTimes ? ((eventRow.top) + 'px') : 'auto', position: vm.showTimes ? 'absolute' : 'inherit', width: vm.showTimes ? (vm.dayColumnDimensions.width + 'px') : '', left: vm.showTimes ? (vm.dayColumnDimensions.width * eventRow.offset) + 15 + 'px' : '' }\"> <div class=\"day-highlight\" ng-class=\"[eventRow.event.cssClass, !vm.showTimes && eventRow.startsBeforeWeek ? '' : 'border-left-rounded', !vm.showTimes && eventRow.endsAfterWeek ? '' : 'border-right-rounded']\" ng-style=\"{backgroundColor: eventRow.event.color.secondary}\" data-event-class mwl-draggable=\"eventRow.event.draggable === true\" axis=\"vm.showTimes ? 'xy' : 'x'\" snap-grid=\"vm.showTimes ? {x: vm.dayColumnDimensions.width, y: vm.dayViewEventChunkSize || 30} : {x: vm.dayColumnDimensions.width}\" on-drag=\"vm.tempTimeChanged(eventRow.event, y / 30)\" on-drag-end=\"vm.weekDragged(eventRow.event, x / vm.dayColumnDimensions.width, y / 30)\" mwl-resizable=\"eventRow.event.resizable === true && eventRow.event.endsAt && !vm.showTimes\" resize-edges=\"{left: true, right: true}\" on-resize-end=\"vm.weekResized(eventRow.event, edge, x / vm.dayColumnDimensions.width)\"> <strong ng-bind=\"(eventRow.event.tempStartsAt || eventRow.event.startsAt) | calendarDate:'time':true\" ng-show=\"vm.showTimes\"></strong> <a href=\"#/app/crm/module/{{eventRow.event.type}}{{'?id=' + eventRow.event.id + '&back=calendar'}}\" ng-click=\"vm.onEventClick({calendarEvent: eventRow.event})\" class=\"event-item\" ng-bind-html=\"vm.calendarEventTitle.weekView(eventRow.event) | calendarTrustAsHtml\" uib-tooltip-html=\"vm.calendarEventTitle.weekViewTooltip(eventRow.event) | calendarTrustAsHtml\" tooltip-placement=\"left\" tooltip-append-to-body=\"true\"> </a> </div> </div> </div> </div>  </div>  </div> </div>");

            var getCalendar = function (reset) {
                var eventsCache = $cache.get('calendar_events');

                if (!reset && eventsCache) {
                    $scope.events = eventsCache;
                    return
                }

                if ($rootScope.calendarFields) {
                    $scope.additionalFields = [];
                    var calendarFieldsParts = $rootScope.calendarFields.value.split('|');

                    for (var i = 0; i < calendarFieldsParts.length; i++) {
                        var calendarFieldsPart = calendarFieldsParts[i];
                        var moduleFieldsPart = calendarFieldsPart.split(';');
                        var additionalField = {};
                        additionalField.module = moduleFieldsPart[0];
                        additionalField.fields = moduleFieldsPart[1].split(',');

                        $scope.additionalFields.push(additionalField);
                    }
                }

                $scope.events = [];
                $scope.permissions = [];
                $scope.permissions['activities'] = $scope.activityPermissions;
                if ($scope.modules.length > 0) {
                    angular.forEach($scope.modules, function (module) {
                        var primaryField = $filter('filter')(module.fields, { primary: true }, true)[0];
                        var primaryLookupField = $filter('filter')(module.fields, { primary_lookup: true }, true)[0];
                        var startDateField = $filter('filter')(module.fields, { calendar_date_type: 'start_date' }, true)[0];
                        var endDateField = $filter('filter')(module.fields, { calendar_date_type: 'end_date' }, true)[0];

                        if (!primaryField || !startDateField || !endDateField)
                            return;
                        if (!$scope.permissions[module.name])
							$scope.permissions[module.name] = $filter('filter')($rootScope.user.profile.permissions, { module_id: module.id}, true)[0];


                        if (!$scope.permissions[module.name].Read)
                            return;
                        if (primaryLookupField)
                            primaryField = primaryLookupField;

                        var findRequest = {};
                        findRequest.fields = [primaryField.name, startDateField.name, endDateField.name, 'owner.users.full_name'];
                        findRequest.sort_field = 'created_at';
                        findRequest.sort_direction = 'desc';
                        findRequest.limit = 2000;

                        if ($scope.filter && $scope.filter.owner && $scope.filter.owner.Id > 0) {
                            if (!findRequest.filters)
                                findRequest.filters = [];

                            findRequest.filters.push({ field: 'owner', operator: 'equals', value: $scope.filter.owner.Id, no: 1 });
                        }

                        if ($scope.additionalFields) {
                            var additionalFields = $filter('filter')($scope.additionalFields, { module: module.name }, true)[0];

                            if (additionalFields && additionalFields.fields)
                                findRequest.fields = findRequest.fields.concat(additionalFields.fields);
                        }

                        ModuleService.getPicklists(module)
                            .then(function (picklistsModule) {
                                ModuleService.findRecords(module.name, findRequest)
                                    .then(function (response) {
                                        var records = angular.copy(response.data);

                                        if (records) {
                                            if (additionalFields) {
                                                var viewFields = [];

                                                for (var i = 0; i < findRequest.fields.length; i++) {
                                                    var viewField = {};
                                                    viewField.field = findRequest.fields[i];
                                                    viewField.order = i + 1;

                                                    viewFields.push(viewField);
                                                }

                                                records = ModuleService.processRecordMulti(records, module, picklistsModule, viewFields, module.name);
                                            }

                                            angular.forEach(response.data, function (eventItem) {
                                                var event = {};
                                                event.id = eventItem.id;
                                                event.title = '';
                                                event.color = { primary: module.calendar_color_dark || '#fbb903', secondary: module.calendar_color_light || '#fdf1ba' };
                                                event.module = module['label_' + $rootScope.language + '_singular'];
                                                event.type = module.name;
                                                event.permissions = $scope.permissions[event.type];
                                                event.calendardeleteshow = $scope.calendarDeleteShow;
                                                if (additionalFields) {
                                                    var record = $filter('filter')(records, { id: eventItem['id'] }, true)[0];
                                                    event.title = $filter('filter')(record.fields, { name: primaryField.name }, true)[0].valueFormatted;

                                                    for (var i = 0; i < additionalFields.fields.length; i++) {
                                                        var fieldName = additionalFields.fields[i];

                                                        if (fieldName.indexOf('.') > -1)
                                                            fieldName = fieldName.split('.')[2];

                                                        var currentField = $filter('filter')(record.fields, { name: fieldName }, true)[0];

                                                        if (currentField)
                                                            event.title += ' | ' + currentField.valueFormatted;
                                                        else
                                                            event.title += ' | ';
                                                    }
                                                }
                                                else {
                                                    event.title = eventItem[primaryField.name] ? (primaryField.auto_number_prefix || '') + eventItem[primaryField.name] + (primaryField.auto_number_suffix || '') : eventItem['owner.users.full_name'];
                                                }

                                                event.title += ' | <span class="event-user">' + eventItem['owner.users.full_name'] + '</span>';

                                                if (!eventItem[startDateField.name])
                                                    eventItem[startDateField.name] = eventItem[endDateField.name];

                                                event.startsAt = moment.utc(eventItem[startDateField.name]).toDate();
                                                event.endsAt = moment.utc(eventItem[endDateField.name]).toDate();

                                                $scope.events.push(event);
                                            });
                                        }
                                    });
                            });
                    });
                }

                var primaryField = $filter('filter')($scope.moduleActivity.fields, { primary: true }, true)[0];
                var activityTypeEvent = $filter('filter')(activityTypes, { system_code: 'event' }, true)[0];
                var findRequest = {};
                findRequest.fields = [primaryField.name, 'event_start_date', 'event_end_date', 'all_day_event', 'owner.users.full_name'];
                findRequest.filters = [{ field: 'activity_type_system', operator: 'is', value: 'event', no: 1 }];
                findRequest.sort_field = 'created_at';
                findRequest.sort_direction = 'desc';
                findRequest.limit = 2000;

                if ($scope.filter && $scope.filter.owner && $scope.filter.owner.Id > 0)
                    findRequest.filters.push({ field: 'owner', operator: 'equals', value: $scope.filter.owner.Id, no: 2 });
                if ($scope.permissions['activities'].Read) {
                    ModuleService.findRecords('activities', findRequest)
                        .then(function (response) {

                            angular.forEach(response.data, function (eventItem) {
                                var eventStartDate = moment.utc(eventItem.event_start_date).toDate();
                                var eventEndDate = moment.utc(eventItem.event_end_date).toDate();

                                var event = {};
                                event.id = eventItem.id;
                                event.title = eventItem[primaryField.name] ? eventItem[primaryField.name] + ' | <span class="event-user">' + eventItem['owner.users.full_name'] + '</span>' : eventItem['owner.users.full_name'];
                                event.color = {
                                    primary: $scope.moduleActivity.calendar_color_dark || '#fbb903',
                                    secondary: $scope.moduleActivity.calendar_color_light || '#fdf1ba'
                                };
                                event.module = activityTypeEvent.label[$rootScope.language];
                                event.type = 'activities';
                                event.permissions = $scope.permissions[event.type];
                                event.calendardeleteshow = $scope.calendarDeleteShow;
                                if (eventItem.all_day_event === 'Yes' || eventItem.all_day_event === 'Evet') {
                                    var eventStartDateAllDay = new Date(eventStartDate.getFullYear(), eventStartDate.getMonth(), eventStartDate.getDate(), 0, 0, 0);
                                    var eventEndDateAllDay = new Date(eventEndDate.getFullYear(), eventEndDate.getMonth(), eventEndDate.getDate(), 23, 59, 59);
                                    event.startsAt = eventStartDateAllDay;
                                    event.endsAt = eventEndDateAllDay;
                                    event.allDay = true;
                                }
                                else {
                                    event.startsAt = eventStartDate;
                                    event.endsAt = eventEndDate;
                                }

                                $scope.events.push(event);
                            });
                        });

                }

                $timeout(function () {
                    $cache.put('calendar_events', $scope.events);
                }, 5000)
            };

            getCalendar();

            $scope.filterChanged = function () {
                getCalendar(true);
            };

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
            };

            $scope.delete = function (event) {
                ModuleService.deleteRecord(event.type, event.id)
                    .then(function () {
                        getCalendar(true);
                        $cache.remove(event.type + '_' + event.type);
                    });
            };

            $scope.openCreateModal = function ($event, day, module) {
                if (day)
                    $scope.calendarDate = day.date;

                if (!module && $scope.modules.length > 0) {
                    $scope.dropdownModules = {};
                    $scope.content = [];
                    $scope.content.push({ text: $rootScope.language === 'tr' ? 'Etkinlik' : 'Event', href: '', click: 'openCreateModal(null, null, "activities")' });

                    angular.forEach($scope.modules, function (module) {
                        $scope.content.push({ text: module['label_' + $rootScope.language + '_singular'], href: '', click: 'openCreateModal(null, null, "' + module.name + '")' });
                    });

                    $scope.dropdownModules[day.label] = $scope.dropdownModules[day.label] || $dropdown(angular.element($event.target),
                        {
                            scope: $scope,
                            show: false,
                            animation: ''
                        }
                    );

                    $scope.dropdownModules[day.label].$promise.then($scope.dropdownModules[day.label].show);
                    return;
                }

                if (!module)
                    module = 'activities';

                $scope.currentLookupField = { lookup_type: module };

                $scope.formModal = $scope.formModal || $modal({
                    scope: $scope,
                    templateUrl: 'views/app/module/moduleFormModal.html',
                    animation: '',
                    backdrop: 'static',
                    show: false,
                    tag: 'createModal'
                });

                $scope.formModal.$promise.then($scope.formModal.show);
            };

            $scope.formModalSuccess = function () {
                getCalendar(true);
            };
        }]);