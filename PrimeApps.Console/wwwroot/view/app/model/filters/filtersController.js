'use strict';

angular.module('primeapps')

    .controller('FiltersController', ['$rootScope', '$scope', '$state', '$stateParams', '$location', 'ngToast', '$filter', '$cache', '$q', 'helper', 'dragularService', 'operators', 'FiltersService',  '$http', 'config',
        function ($rootScope, $scope, $state, $stateParams, $location, ngToast, $filter, $cache, $q, helper, dragularService, operators, FiltersService, $http, config) {


            $scope.$parent.menuTopTitle ="Models";
            $scope.$parent.activeMenu= 'model';
            $scope.$parent.activeMenuItem = 'filters';

            // var clone = $location.search().clone;
            // var id = $location.search().id;
            // var module = $filter('filter')($rootScope.modules, { name: $stateParams.type }, true)[0];
            // $scope.costumeDate = "this_day()";
            // $scope.dateFormat = [
            //     {
            //         label: $filter('translate')('View.Second'),
            //         value: "s"
            //     },
            //     {
            //         label: $filter('translate')('View.Minute'),
            //         value: "m"
            //     },
            //     {
            //         label: $filter('translate')('View.Hour'),
            //         value: "h"
            //     },
            //     {
            //         label: $filter('translate')('View.Day'),
            //         value: "D"
            //     },
            //     {
            //         label: $filter('translate')('View.Week'),
            //         value: "W"
            //     },
            //     {
            //         label: $filter('translate')('View.Month'),
            //         value: "M"
            //     },
            //     {
            //         label: $filter('translate')('View.Year'),
            //         value: "Y"
            //     }
            // ];
            //
            // $scope.costumeDateFilter = [
            //     {
            //         name: "thisNow",
            //         label: $filter('translate')('View.Now'),
            //         value: "now()"
            //     },
            //     {
            //         name: "thisToday",
            //         label: $filter('translate')('View.StartOfTheDay'),
            //         value: "today()"
            //     },
            //     {
            //         name: "thisWeek",
            //         label: $filter('translate')('View.StartOfThisWeek'),
            //         value: "this_week()"
            //     },
            //     {
            //         name: "thisMonth",
            //         label: $filter('translate')('View.StartOfThisMonth'),
            //         value: "this_month()"
            //     },
            //     {
            //         name: "thisYear",
            //         label: $filter('translate')('View.StartOfThisYear'),
            //         value: "this_year()"
            //     },
            //     {
            //         name: "year",
            //         label: $filter('translate')('View.NowYear'),
            //         value: "year()"
            //     },
            //     {
            //         name: "month",
            //         label: $filter('translate')('View.NowMonth'),
            //         value: "month()"
            //     },
            //     {
            //         name: "day",
            //         label: $filter('translate')('View.NowDay'),
            //         value: "day()"
            //     },
            //     {
            //         name: "costume",
            //         label: $filter('translate')('View.CustomDate'),
            //         value: "costume"
            //     },
            //     {
            //         name: "todayNextPrev",
            //         label: $filter('translate')('View.FromTheBeginningOfTheDay'),
            //         value: "costumeN",
            //         nextprevdatetype: "D"
            //     },
            //     {
            //         name: "weekNextPrev",
            //         label: $filter('translate')('View.FromTheBeginningOfTheWeek'),
            //         value: "costumeW",
            //         nextprevdatetype: "M"
            //     },
            //     {
            //         name: "monthNextPrev",
            //         label: $filter('translate')('View.FromTheBeginningOfTheMonth'),
            //         value: "costumeM",
            //         nextprevdatetype: "M"
            //     },
            //     {
            //         name: "yearNextPrev",
            //         label: $filter('translate')('View.FromTheBeginningOfTheYear'),
            //         value: "costumeY",
            //         nextprevdatetype: "Y"
            //     }
            // ];
            //
            // $scope.dateChange = function (filter) {
            //     if (filter.costumeDate != 'costume' && filter.costumeDate != 'costumeN' && filter.costumeDate != 'costumeW' && filter.costumeDate != 'costumeM' && filter.costumeDate != 'costumeY') {
            //         filter.value = filter.costumeDate;
            //     }
            //     if (filter.costumeDate === 'costumeN' || filter.costumeDate === 'costumeW' || filter.costumeDate === 'costumeM' || filter.costumeDate === 'costumeY') {
            //         filter.value = "";
            //         filter.valueX = "";
            //         filter.nextprevdatetype = "";
            //
            //     }
            //
            // };
            //
            // $scope.nextPrevDateChange = function (filter) {
            //     $scope.setCostumDate(filter);
            // };
            // $scope.nextPrevDateTypeChange = function (filter) {
            //     $scope.setCostumDate(filter);
            // };
            // $scope.setCostumDate = function (filter) {
            //     if (filter.valueX === null || filter.valueX === "" || filter.valueX === undefined) {
            //         filter.value = "";
            //         return false;
            //     }
            //     if (filter.nextprevdatetype === undefined) {
            //         filter.nextprevdatetype = $scope.dateFormat[0].value;
            //     }
            //     switch (filter.costumeDate) {
            //         case "costumeN":
            //             filter.value = "today(" + filter.valueX + filter.nextprevdatetype + ")";
            //             break;
            //         case "costumeM":
            //             filter.value = "this_month(" + filter.valueX + filter.nextprevdatetype + ")";
            //             break;
            //         case "costumeW":
            //             filter.value = "this_week(" + filter.valueX + filter.nextprevdatetype + ")";
            //             break;
            //         case "costumeY":
            //             filter.value = "this_year(" + filter.valueX + filter.nextprevdatetype + ")";
            //             break;
            //     }
            //
            // };
            // if (!module) {
            //     ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
            //     $state.go('app.dashboard');
            //     return;
            // }
            //
            // $scope.module = module;
            //
            // var cacheKey = module.name + '_' + module.name;
            // var cache = $cache.get(cacheKey);
            //
            // if (!cache || !cache['views'] || cache['views'].length < 1) {
            //     $state.go('app.moduleList', { type: module.name });
            //     return;
            // }
            //
            // if (id) {
            //     var views = cache['views'];
            //     $scope.view = angular.copy($filter('filter')(views, { id: parseInt(id) }, true)[0]);
            //     $scope.view.label = $scope.view['label_' + $rootScope.language];
            //     $scope.isOwner = $scope.view.created_by === $rootScope.user.id;
            //
            //     if (!$scope.view) {
            //         $state.go('app.moduleList', { type: module.name });
            //         return;
            //     }
            //
            //     if ($scope.view.filter_logic && $rootScope.language === 'tr')
            //         $scope.view.filter_logic = $scope.view.filter_logic.replace(/or/g, 'veya').replace(/and/g, 've');
            // }
            // else {
            //     $scope.view = {};
            //     $scope.view.system_type = 'custom';
            //     $scope.view.sharing_type = 'me';
            // }
            //
            // if ($filter('filter')($rootScope.approvalProcesses, { module_id: module.id }, true)[0]) {
            //     $scope.showProcessFilter = true;
            //     $scope.currenProcessField = {
            //         field: "process.process_requests.process_status",
            //         order: $scope.view.fields ? $scope.view.fields.length : 0
            //     };
            //     $scope.currenProcessFilter = $filter('filter')($scope.view.filters, { field: "process.process_requests.process_status" }, true);
            //     if (!$scope.currenProcessFilter || $scope.currenProcessFilter.length < 1) {
            //         $scope.currenProcessFilter = {
            //             field: "process.process_requests.process_status",
            //             operator: "equals",
            //             value: "0"
            //         };
            //
            //     }
            //     else {
            //         $scope.currenProcessFilter = $scope.currenProcessFilter[0];
            //     }
            //     $scope.currenProcessFilter.field = "process.process_requests.process_status";
            //     $scope.processFilter = $scope.currenProcessFilter.value;
            //
            // }
            //
            // $scope.fields = ViewService.getFields($scope.module, angular.copy($scope.view));
            //
            // var containerLeft = document.querySelector('#availableFields');
            // var containerRight = document.querySelector('#selectedFields');
            //
            // dragularService([containerLeft], {
            //     scope: $scope,
            //     containersModel: [$scope.fields.availableFields],
            //     classes: {
            //         mirror: 'gu-mirror-view',
            //         transit: 'gu-transit-view'
            //     },
            //     accepts: accepts,
            //     moves: function (el, container, handle) {
            //         return handle.classList.contains('dragable');
            //     }
            // });
            //
            // dragularService([containerRight], {
            //     scope: $scope,
            //     classes: {
            //         mirror: 'gu-mirror-view',
            //         transit: 'gu-transit-view'
            //     },
            //     containersModel: [$scope.fields.selectedFields]
            // });
            //
            // function accepts(el, target, source) {
            //     if (source != target) {
            //         return true;
            //     }
            // }
            //
            // $scope.$on('dragulardrop', function (e, el) {
            //     $scope.viewForm.$setValidity('field', true);
            // });
            //
            // ModuleService.getPicklists($scope.module, true)
            //     .then(function (picklists) {
            //         $scope.modulePicklists = picklists;
            //         $scope.view.filterList = [];
            //
            //         for (var i = 0; i < 10; i++) {
            //             var filter = {};
            //             filter.field = null;
            //             filter.operator = null;
            //             filter.value = null;
            //             filter.no = i + 1;
            //
            //             $scope.view.filterList.push(filter);
            //         }
            //
            //         if ($scope.view.filters) {
            //             $scope.view.filters = $filter('orderBy')($scope.view.filters, 'no');
            //
            //             for (var j = 0; j < $scope.view.filters.length; j++) {
            //                 if ($scope.view.filters[j].field === 'process.process_requests.process_status') {
            //                     $scope.view.filters.splice(j, 1);
            //                 }
            //
            //                 var name = $scope.view.filters[j].field;
            //                 var value = $scope.view.filters[j].value;
            //
            //                 if (name.indexOf('.') > -1) {
            //                     name = name.split('.')[0];
            //                     $scope.view.filters[j].field = name;
            //                 }
            //
            //                 var field = $filter('filter')($scope.module.fields, { name: name }, true)[0];
            //                 var fieldValue = null;
            //
            //                 if (!field)
            //                     return;
            //
            //                 switch (field.data_type) {
            //                     case 'picklist':
            //                         fieldValue = $filter('filter')($scope.modulePicklists[field.picklist_id], { labelStr: value }, true)[0];
            //                         break;
            //                     case 'multiselect':
            //                         fieldValue = [];
            //                         var multiselectValue = value.split('|');
            //
            //                         angular.forEach(multiselectValue, function (picklistLabel) {
            //                             var picklist = $filter('filter')($scope.modulePicklists[field.picklist_id], { labelStr: picklistLabel }, true)[0];
            //
            //                             if (picklist)
            //                                 fieldValue.push(picklist);
            //                         });
            //                         break;
            //                     case 'tag':
            //                         fieldValue = [];
            //                         var tagValue = value.split('|');
            //
            //                         angular.forEach(tagValue, function (label) {
            //                             fieldValue.push(label);
            //                         });
            //                         break;
            //                     case 'lookup':
            //                         if (field.lookup_type === 'users') {
            //                             var user = {};
            //
            //                             if (value === '0' || value === '[me]') {
            //                                 user.id = 0;
            //                                 user.email = '[me]';
            //                                 user.full_name = $filter('translate')('Common.LoggedInUser');
            //                             }
            //                             else {
            //                                 if (value != '-') {
            //                                     var userItem =
            //                                         $filter('filter')($rootScope.users, { id: parseInt(value) }, true)[0
            //                                         ];
            //                                     user.id = userItem.id;
            //                                     user.email = userItem.Email;
            //                                     user.full_name = userItem.FullName;
            //                                 }
            //
            //                                 //TODO: $rootScope.users kaldirilinca duzeltilecek
            //                                 // ModuleService.getRecord('users', value)
            //                                 //     .then(function (lookupRecord) {
            //                                 //         fieldValue = [lookupRecord.data];
            //                                 //     });
            //                             }
            //
            //                             fieldValue = [user];
            //                         }
            //                         else {
            //                             fieldValue = value;
            //                         }
            //                         break;
            //                     case 'date':
            //                     case 'date_time':
            //                     case 'time':
            //                         if (!$scope.isCostumeDate($scope.view.filters[j])) {
            //                             fieldValue = new Date(value);
            //                             $scope.view.filterList[j].costumeDate = "costume";
            //                             $scope.view.filters[j].costumeDate = "costume";
            //                         } else {
            //                             fieldValue = $scope.view.filters[j].value;
            //                             $scope.view.filterList[j].costumeDate = $scope.view.filters[j].costumeDate;
            //                             $scope.view.filterList[j].valueX = $scope.view.filters[j].valueX;
            //                             $scope.view.filterList[j].nextprevdatetype = $scope.view.filters[j].nextprevdatetype;
            //                         }
            //
            //                         break;
            //                     case 'checkbox':
            //                         fieldValue = $filter('filter')($scope.modulePicklists.yes_no, { system_code: value }, true)[0];
            //                         break;
            //                     default:
            //                         fieldValue = value;
            //                         break;
            //                 }
            //
            //                 $scope.view.filterList[j].field = field;
            //                 $scope.view.filterList[j].operator = operators[$scope.view.filters[j].operator];
            //                 $scope.view.filterList[j].value = fieldValue;
            //
            //                 if ($scope.view.filters[j].operator === 'empty' || $scope.view.filters[j].operator === 'not_empty') {
            //                     $scope.view.filterList[j].value = null;
            //                     $scope.view.filterList[j].disabled = true;
            //                 }
            //             }
            //         }
            //     });
            //
            // $scope.multiselect = function (searchTerm, field) {
            //     var picklistItems = [];
            //
            //     angular.forEach($scope.modulePicklists[field.picklist_id], function (picklistItem) {
            //         if (picklistItem.inactive)
            //             return;
            //
            //         if (picklistItem.labelStr.toLowerCase().indexOf(searchTerm) > -1)
            //             picklistItems.push(picklistItem);
            //     });
            //
            //     return picklistItems;
            // };
            //
            // $scope.tags = function (searchTerm, field) {
            //     return $http.get(config.apiUrl + "tag/get_tag/" + field.id).then(function (response) {
            //         var tags = response.data;
            //         return tags.filter(function (tag) {
            //             return tag.text.toLowerCase().indexOf(searchTerm.toLowerCase()) != -1;
            //         });
            //     });
            // };
            //
            // $scope.lookupUser = helper.lookupUser;
            //
            // var dateTimeChanged = function (filterListItem) {
            //     if (filterListItem.operator) {
            //         var newValue = new Date(filterListItem.value);
            //
            //         switch (filterListItem.operator.name) {
            //             case 'greater':
            //                 newValue.setHours(23);
            //                 newValue.setMinutes(59);
            //                 newValue.setSeconds(59);
            //                 newValue.setMilliseconds(99);
            //                 break;
            //             case 'less':
            //                 newValue.setHours(0);
            //                 newValue.setMinutes(0);
            //                 newValue.setSeconds(0);
            //                 newValue.setMilliseconds(0);
            //                 break;
            //         }
            //
            //         filterListItem.value = newValue;
            //     }
            // };
            //
            // $scope.dateTimeChanged = function (field) {
            //     dateTimeChanged(field);
            // };
            //
            // $scope.isCostumeDate = function (filter) {
            //     var getNumberRegex = /[^\d.-]/g;
            //     if (filter.value.indexOf('now(') > -1) {
            //         filter.costumeDate = "now()";
            //         return true;
            //     }
            //     if (filter.value.indexOf('today(') > -1) {
            //         if (/\d+/.test(filter.value)) {
            //             filter.costumeDate = "costumeN";
            //             filter.valueX = parseFloat(filter.value.replace(/[^\d.-]/g, ''));
            //             filter.nextprevdatetype = filter.value.match(/([A-z])\)/g, '')[0].match(/[A-z]/g, '')[0];
            //             return true;
            //         } else {
            //             filter.costumeDate = "today()";
            //             return true;
            //         }
            //     }
            //     if (filter.value === 'year()') {
            //         filter.costumeDate = "year()";
            //         return true;
            //     }
            //     if (filter.value === 'month()') {
            //         filter.costumeDate = "month()";
            //         return true;
            //     }
            //     if (filter.value === 'day()') {
            //         filter.costumeDate = "day()";
            //         return true;
            //     }
            //
            //     if (filter.value.indexOf('this_week(') > -1) {
            //         if (/\d+/.test(filter.value)) {
            //             filter.costumeDate = "costumeW";
            //             filter.valueX = parseFloat(filter.value.replace(/[^\d.-]/g, ''));
            //             filter.nextprevdatetype = filter.value.match(/([A-z])\)/g, '')[0].match(/[A-z]/g, '')[0];
            //             return true;
            //         } else {
            //             filter.costumeDate = "this_week()";
            //             return true;
            //         }
            //     }
            //
            //     if (filter.value.indexOf('this_month(') > -1) {
            //         if (/\d+/.test(filter.value)) {
            //             filter.costumeDate = "costumeM";
            //             filter.valueX = parseFloat(filter.value.replace(/[^\d.-]/g, ''));
            //             filter.nextprevdatetype = filter.value.match(/([A-z])\)/g, '')[0].match(/[A-z]/g, '')[0];
            //             return true;
            //         } else {
            //             filter.costumeDate = "this_month()";
            //             return true;
            //         }
            //     }
            //
            //     if (filter.value.indexOf('this_year(') > -1) {
            //         if (/\d+/.test(filter.value)) {
            //             filter.costumeDate = "costumeY";
            //             filter.valueX = parseFloat(filter.value.replace(/[^\d.-]/g, ''));
            //             filter.nextprevdatetype = filter.value.match(/([A-z])\)/g, '')[0].match(/[A-z]/g, '')[0];
            //             return true;
            //         } else {
            //             filter.costumeDate = "this_year()";
            //             return true;
            //         }
            //     }
            //     return false;
            //
            // };
            //
            // $scope.operatorChanged = function (field, index) {
            //     var filterListItem = $scope.view.filterList[index];
            //
            //     if (!filterListItem || !filterListItem.operator)
            //         return;
            //
            //     if (field.data_type === 'date_time' && filterListItem.value && filterListItem.costumeDate === 'costume')
            //         dateTimeChanged(filterListItem);
            //
            //     if (filterListItem.operator.name === 'empty' || filterListItem.operator.name === 'not_empty') {
            //         filterListItem.value = null;
            //         filterListItem.disabled = true;
            //     }
            //     else {
            //         filterListItem.disabled = false;
            //     }
            // };
            //
            // $scope.save = function () {
				// function validate() {
            //
            //         var isValid = true;
				// 	/**
            //          * boolean clone
            //          * boolean id
            //          * View'den, kopyalama linki("clone=true&id=") üzerinden gelen clone = true oluyor
            //          * Eğer clone = true ise yeni View create edilecektir
            //          * Yeni View'in create edilmesi için id = false olmalıdır
            //          * */
				// 	if (clone)
				// 		id = false;
            //
            //         if ($scope.fields.selectedFields.length < 1) {
            //             $scope.viewForm.$setValidity('field', false);
            //             isValid = false;
            //         }
            //
            //         return isValid;
            //     }
            //
            //     if (!$scope.viewForm.$valid || !validate())
            //         return;
            //
            //     $scope.submitting = true;
            //
            //     var view = {};
            //     view.module_id = module.id;
            //     view.label = $scope.view.label;
            //     view.sharing_type = $scope.view.sharing_type;
            //     view.fields = [];
            //     view.filters = [];
            //
            //     if ($scope.view.filter_logic) {
            //         view.filter_logic = $scope.view.filter_logic.replace(/veya/g, 'or').replace(/ve/g, 'and');
            //
            //         if (!(view.filter_logic.charAt(0) === '(' && view.filter_logic.charAt(view.filter_logic.length - 1) === ')'))
            //             view.filter_logic = '(' + view.filter_logic + ')';
            //     }
            //
            //     for (var i = 0; i < $scope.fields.selectedFields.length; i++) {
            //         var selectedField = $scope.fields.selectedFields[i];
            //         var field = {};
            //         field.field = selectedField.name;
            //         field.order = i + 1;
            //
            //         view.fields.push(field);
            //
            //         if (selectedField.lookup_type && selectedField.lookup_type != 'relation') {
            //             var lookupModule = $filter('filter')($rootScope.modules, { name: selectedField.lookup_type }, true)[0];
            //             var primaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];
            //             var fieldPrimary = {};
            //             fieldPrimary.field = selectedField.name + '.' + lookupModule.name + '.' + primaryField.name + '.primary';
            //             fieldPrimary.order = i + 1;
            //
            //             view.fields.push(fieldPrimary);
            //         }
            //     }
            //
            //     var filterList = angular.copy($scope.view.filterList);
            //
            //     angular.forEach(filterList, function (filterItem) {
            //
            //         if (!filterItem.field || !filterItem.operator)
            //             return;
            //
            //         if (!(filterItem.operator.name === 'empty' || filterItem.operator.name === 'not_empty') && (filterItem.value == null || filterItem.value == undefined))
            //             return;
            //
            //         var field = filterItem.field;
            //         var fieldName = field.name;
            //
            //         if (field.data_type === 'lookup' && field.lookup_type != 'users') {
            //             var lookupModule = $filter('filter')($rootScope.modules, { name: field.lookup_type }, true)[0];
            //             var lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];
            //             fieldName = field.name + '.' + field.lookup_type + '.' + lookupModulePrimaryField.name;
            //
            //             var filterFieldName = fieldName + '.primary';
            //             if (!$filter('filter')(view.fields, { field: filterFieldName }, true)[0]) {
            //                 var lookupField = {};
            //                 lookupField.field = field.name;
            //                 lookupField.order = view.fields.length + 1;
            //
            //                 view.fields.push(lookupField);
            //
            //                 var lookupFieldPrimary = {};
            //                 lookupFieldPrimary.field = filterFieldName;
            //                 lookupFieldPrimary.order = view.fields.length;
            //
            //                 view.fields.push(lookupFieldPrimary);
            //             }
            //
            //         }
            //
            //         var filter = {};
            //         filter.field = fieldName;
            //         filter.operator = filterItem.operator.name;
            //         filter.value = filterItem.value;
            //         filter.no = filterItem.no;
            //
            //         field = !filterItem.field.lookupModulePrimaryField || filterItem.field.lookup_type === 'users' ? filterItem.field : filterItem.field.lookupModulePrimaryField;
            //
            //         if (!(filterItem.operator.name === 'empty' || filterItem.operator.name === 'not_empty')) {
            //             if (field.data_type === 'picklist')
            //                 filter.value = filter.value.label[$rootScope.user.tenant_language];
            //
            //             if (field.data_type === 'multiselect') {
            //                 var value = '';
            //
            //                 angular.forEach(filter.value, function (picklistItem) {
            //                     value += picklistItem.label[$rootScope.user.tenant_language] + '|';
            //                 });
            //
            //                 filter.value = value.slice(0, -1);
            //             }
            //
            //             if (field.data_type === 'tag') {
            //                 var value = '';
            //
            //                 angular.forEach(filter.value, function (item) {
            //                     value += item.text + '|';
            //                 });
            //
            //                 filter.value = value.slice(0, -1);
            //             }
            //
            //             if (field.data_type === 'lookup' && field.lookup_type === 'users') {
            //                 if (filter.value[0].id === 0)
            //                     filter.value = '[me]';
            //                 else
            //                     filter.value = filter.value[0].id;
            //             }
            //
            //             if (field.data_type === 'checkbox')
            //                 filter.value = filter.value.system_code;
            //         }
            //         else {
            //             filter.value = '-';
            //         }
            //
            //         view.filters.push(filter);
            //     });
            //
            //     if ($scope.view.sharing_type === 'custom') {
            //         if (!$scope.view.shares) {
            //             view.sharing_type = 'me';
            //         }
            //         else {
            //             view.shares = [];
            //
            //             angular.forEach($scope.view.shares, function (user) {
            //                 view.shares.push(user.id);
            //             });
            //         }
            //     }
            //
            //     if ($scope.currenProcessFilter && $scope.currenProcessFilter.value != "0")
            //         view.filters.push($scope.currenProcessFilter);
            //     if ($scope.currenProcessField && $scope.currenProcessFilter.value != "0")
            //         view.fields.push($scope.currenProcessField);
            //     if (!id) {
            //         ViewService.create(view)
            //             .then(function (response) {
            //                 var viewState = cache.viewState;
            //
            //                 if (!viewState) {
            //                     viewState = {};
            //                     viewState.sort_field = 'created_at';
            //                     viewState.sort_direction = 'desc';
            //                     viewState.row_per_page = 10;
            //                 }
            //
            //                 viewState.active_view = response.data.id;
            //
            //                 ModuleService.setViewState(viewState, $scope.module.id, viewState.id)
            //                     .then(function () {
            //                         success();
            //                     })
            //                     .finally(function () {
            //                         $scope.submitting = false;
            //                     });
            //             })
            //             .catch(function (data) {
            //                 error(data.data, data.status);
            //             })
            //             .finally(function () {
            //                 $scope.submitting = false;
            //             });
            //     }
            //     else {
            //         ViewService.update(view, $scope.view.id, $scope.view._rev)
            //             .then(function () {
            //                 success();
            //             })
            //             .catch(function (data) {
            //                 error(data.data, data.status);
            //             })
            //             .finally(function () {
            //                 $scope.submitting = false;
            //             });
            //     }
            //
            //     function success() {
            //         $cache.remove(cacheKey);
            //         $state.go('app.moduleList', { type: module.name });
            //     }
            //
            //     function error(data, status) {
            //         if (status === 400) {
            //             if (data.model_state && data.model_state['view._filter_logic'])
            //                 $scope.viewForm.filterLogic.$setValidity('filterLogic', false);
            //
            //             if (data.model_state && data.model_state['request._filter_logic'])
            //                 $scope.viewForm.filterLogic.$setValidity('filterLogicFilters', false);
            //         }
            //     }
            // }
            //
    }
    ]);