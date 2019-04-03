'use strict';

angular.module('primeapps')

    .controller('ViewsController', ['$rootScope', '$scope', '$state', '$stateParams', '$location', '$filter', '$cache', '$q', 'helper', 'dragularService', 'operators', 'ViewsService', '$http', 'config', '$modal', 'ModuleService',
        function ($rootScope, $scope, $state, $stateParams, $location, $filter, $cache, $q, helper, dragularService, operators, ViewsService, $http, config, $modal, ModuleService) {

            $scope.id = $location.search().id ? $location.search().id : 0;
            if ($scope.id > 0)
                $scope.$parent.$parent.$parent.$parent.openSubMenu('visualization');

            $scope.$parent.activeMenuItem = 'views';
            $rootScope.breadcrumblist[2].title = 'Views';

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }
            };

            $scope.generator(10);

            $scope.loading = true;
            $scope.requestModel = {
                limit: '10',
                offset: 0
            };

            $scope.activePage = 1;
            ViewsService.count($scope.id).then(function (response) {
                $scope.pageTotal = response.data;
                $scope.changePage(1);
            });

            $scope.changePage = function (page) {
                $scope.loading = true;

                if (page !== 1) {
                    var difference = Math.ceil($scope.pageTotal / $scope.requestModel.limit);

                    if (page > difference) {
                        if (Math.abs(page - difference) < 1)
                            --page;
                        else
                            page = page - Math.abs(page - Math.ceil($scope.pageTotal / $scope.requestModel.limit))
                    }
                }

                $scope.activePage = page;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;

                ViewsService.find($scope.id, requestModel).then(function (response) {
                    var customViews = angular.copy(response.data);
                    for (var i = customViews.length - 1; i >= 0; i--) {
                        var parentModule = $filter('filter')($rootScope.appModules, {id: customViews[i].module_id}, true)[0];
                        if (parentModule) {
                            customViews[i].parent_module = $filter('filter')($rootScope.appModules, {id: customViews[i].module_id}, true)[0];
                        } else {
                            customViews.splice(i, 1);
                        }
                    }
                    $scope.customViews = customViews;
                    $scope.loading = false;
                });
            };

            $scope.changeOffset = function () {
                $scope.changePage($scope.activePage)
            };

            $scope.deleteView = function (id, event) {
                var willDelete =
                    swal({
                        title: "Are you sure?",
                        text: " ",
                        icon: "warning",
                        buttons: ['Cancel', 'Yes'],
                        dangerMode: true
                    })
                        .then(function (value) {
                            if (value) {
                                var deleteElement = angular.element(event.srcElement);
                                angular.element(deleteElement.closest('tr')).addClass('animated-background');

                                if (id) {
                                    ViewsService.deleteView(id)
                                        .then(function () {
                                            angular.element(document.getElementsByClassName('ng-scope animated-background')).remove();
                                            $scope.pageTotal--;
                                            $scope.changePage($scope.activePage);
                                            toastr.success("View is deleted successfully.", "Deleted!");
                                        })
                                        .catch(function () {
                                            $scope.customViews = $scope.customViewsState;
                                            angular.element(document.getElementsByClassName('ng-scope animated-background')).removeClass('animated-background');
                                            toastr.warning($filter('translate')('Common.Error'));
                                            if ($scope.addNewFiltersModal) {
                                                $scope.addNewFiltersModal.hide();
                                                $scope.saving = false;
                                            }
                                        });
                                } else {
                                    angular.element(deleteElement.closest('tr')).removeClass('animated-background');
                                    toastr.warning($filter('translate')('Setup.Modules.OneView'));
                                    return;
                                }
                            }
                        });
            };

            $scope.showFormModal = function (view) {
                $scope.wizardStep = 0;
                if (view) {
                    $scope.loadingModal = true;
                    $scope.isNew = false;
                    ViewsService.getView(view.id)
                        .then(function (view) {
                            $scope.view = angular.copy(view);
                            $scope.module = $filter('filter')($rootScope.appModules, {id: view.module_id}, true)[0];
                            $scope.view.label = $scope.view['label_' + $scope.language];
                            $scope.view.edit = true;

                            // $scope.isOwner = $scope.view.created_by === $rootScope.user.ID;

                            // if (!$scope.view) {
                            //     TODO
                            //     $state.go('app.crm.moduleList', { type: module.name });
                            //     return;
                            // }

                            if ($scope.view.filter_logic && $scope.language === 'tr')
                                $scope.view.filter_logic = $scope.view.filter_logic.replace('or', 'veya').replace('and', 've');

                            moduleChanged($scope.module, false);
                            $scope.loadingModal = false;
                        });
                } else {
                    $scope.isNew = true;
                    $scope.view = {};
                    $scope.module = undefined;
                    //moduleChanged($scope.module, true);
                    $scope.loadingModal = false;
                }
                $scope.addNewFiltersModal = $scope.addNewFiltersModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/visualization/views/viewsForm.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.addNewFiltersModal.$promise.then(function () {
                    $scope.addNewFiltersModal.show();
                });
            };

            $scope.selectedModuleChanged = function (module) {
                $scope.module = module;
                moduleChanged($scope.module, true);
            };

            $scope.$on('dragulardrop', function (e, el) {
                if ($scope.fields.selectedFields.length < 1) {
                    $scope.background_color = "background-color: #eed3d7";
                    toastr.error($filter('translate')('View.FieldError'));
                } else
                    $scope.background_color = "background-color: #fbfbfb";
            });

            var dragular = function () {

                if ($scope.availableFields_ && $scope.selectedFields_) {
                    $scope.availableFields_.destroy();
                    $scope.selectedFields_.destroy();
                    $scope.availableFields_ = null;
                    $scope.selectedFields_ = null;
                }

                var containerLeft = document.querySelector('#availableFields');
                var containerRight = document.querySelector('#selectedFields');

                //dragularService.cleanEnviroment();

                $scope.availableFields_ = dragularService([containerLeft], {
                    scope: $scope,
                    containersModel: [$scope.fields.availableFields],
                    classes: {
                        mirror: 'gu-mirror-view',
                        transit: 'gu-transit-view'
                    },
                    accepts: accepts,
                    moves: function (el, container, handle) {
                        return handle.classList.contains('dragable');
                    }
                });

                $scope.selectedFields_ = dragularService([containerRight], {
                    scope: $scope,
                    classes: {
                        mirror: 'gu-mirror-view',
                        transit: 'gu-transit-view'
                    },
                    containersModel: [$scope.fields.selectedFields]
                });

                function accepts(el, target, source) {
                    if (source !== target) {
                        return true;
                    }
                }
            };

            var moduleChanged = function (module, setView) {
                $scope.lookupUser = helper.lookupUser;
                $scope.loadingFilter = true;
                if (!$scope.view) {
                    $scope.view = {};
                }

                if (setView) {
                    $scope.view.system_type = 'custom';
                    $scope.view.sharing_type = 'everybody';
                }

                /*var cacheKey = module.name + '_' + module.name;
                 var cache = $cache.get(cacheKey);

                 if (!cache || !cache['views'] || cache['views'].length < 1) {
                 $state.go('app.crm.moduleList', { type: module.name });
                 return;
                 }*/

                ModuleService.getModuleFields(module.name).then(function (response) {

                    $scope.module.fields = response.data;
                    $scope.module = ModuleService.getFieldsOperator(module);
                    $scope.fields = ViewsService.getFields($scope.module, angular.copy($scope.view), $rootScope.appModules);
                   
                    ModuleService.getPickItemsLists($scope.module)
                        .then(function (picklists) {
                            $scope.modulePicklists = picklists;
                            $scope.view.filterList = [];

                            for (var i = 0; i < 5; i++) {
                                var filter = {};
                                filter.id = i;
                                filter.field = null;
                                filter.operator = null;
                                filter.value = null;
                                filter.no = i + 1;

                                $scope.view.filterList.push(filter);
                            }

                            if ($scope.view.filters && $scope.view.filters.length > 0) {
                                $scope.view.filters = $filter('orderBy')($scope.view.filters, 'no');
                                $scope.view.filterList = setFilter($scope.view.filters, $scope.module.fields, $scope.modulePicklists, $scope.view.filterList);
                            }
                            $scope.loadingFilter = false;
                            dragular();
                        })
                        .finally(function () {
                            $scope.loadingModal = false;
                        });
                });
            };

            $scope.multiselect = function (searchTerm, field) {
                var picklistItems = [];

                angular.forEach($scope.modulePicklists[field.picklist_id], function (picklistItem) {
                    if (picklistItem.inactive)
                        return;

                    if (picklistItem.labelStr.toLowerCase().indexOf(searchTerm) > -1)
                        picklistItems.push(picklistItem);
                });

                return picklistItems;
            };

            var dateTimeChanged = function (filterListItem) {
                if (filterListItem.operator) {
                    var newValue = new Date(filterListItem.value);

                    switch (filterListItem.operator.name) {
                        case 'greater':
                            newValue.setHours(23);
                            newValue.setMinutes(59);
                            newValue.setSeconds(59);
                            newValue.setMilliseconds(99);
                            break;
                        case 'less':
                            newValue.setHours(0);
                            newValue.setMinutes(0);
                            newValue.setSeconds(0);
                            newValue.setMilliseconds(0);
                            break;
                    }

                    filterListItem.value = newValue;
                }
            };

            $scope.dateTimeChanged = function (field) {
                dateTimeChanged(field);
            };

            $scope.operatorChanged = function (field, index) {
                var filterListItem = $scope.view.filterList[index];

                if (!filterListItem || !filterListItem.operator)
                    return;

                if (field.data_type === 'date_time' && filterListItem.value)
                    dateTimeChanged(filterListItem);

                if (filterListItem.operator.name === 'empty' || filterListItem.operator.name === 'not_empty') {
                    filterListItem.value = null;
                    filterListItem.disabled = true;
                } else {
                    filterListItem.disabled = false;
                }
            };

            $scope.save = function (viewForm, wizardStep) {

                if (!viewForm.$valid || !$scope.validate(viewForm))
                    return;

                $scope.saving = true;

                var view = {};
                view.module_id = $scope.module.id;
                view.label = $scope.view.label;
                view.sharing_type = $scope.view.sharing_type;
                view.fields = [];
                view.filters = [];

                if ($scope.view.filter_logic) {
                    view.filter_logic = $scope.view.filter_logic.replace('veya', 'or').replace('ve', 'and');

                    if (!(view.filter_logic.charAt(0) === '(' && view.filter_logic.charAt(view.filter_logic.length - 1) === ')'))
                        view.filter_logic = '(' + view.filter_logic + ')';
                }

                for (var i = 0; i < $scope.fields.selectedFields.length; i++) {
                    var selectedField = $scope.fields.selectedFields[i];
                    var field = {};
                    field.field = selectedField.name;
                    field.order = i + 1;

                    view.fields.push(field);

                    if (selectedField.lookup_type && selectedField.lookup_type !== 'relation') {
                        var lookupModule = $filter('filter')($rootScope.appModules, {name: selectedField.lookup_type}, true)[0];
                        //TODO dont forget
                        if (lookupModule) {
                            var primaryField = $filter('filter')(lookupModule.fields, {primary: true}, true)[0];
                            var fieldPrimary = {};
                            fieldPrimary.field = selectedField.name + '.' + lookupModule.name + '.' + primaryField.name + '.primary';
                            fieldPrimary.order = i + 1;
                            view.fields.push(fieldPrimary);
                        }
                    }
                }

                var filterList = angular.copy($scope.view.filterList);

                angular.forEach(filterList, function (filterItem) {

                    if (!filterItem.field || !filterItem.operator)
                        return;

                    if (!(filterItem.operator.name === 'empty' || filterItem.operator.name === 'not_empty') && (filterItem.value === null || filterItem.value === undefined))
                        return;

                    var field = filterItem.field;
                    var fieldName = field.name;

                    if (field.data_type === 'lookup' && field.lookup_type !== 'users') {
                        var lookupModule = $filter('filter')($rootScope.appModules, {name: field.lookup_type}, true)[0];
                        var lookupModulePrimaryField = $filter('filter')(lookupModule.fields, {primary: true}, true)[0];
                        fieldName = field.name + '.' + field.lookup_type + '.' + lookupModulePrimaryField.name;
                    }

                    var filter = {};
                    filter.field = fieldName;
                    filter.operator = filterItem.operator.name;
                    filter.value = filterItem.value;
                    filter.no = filterItem.no;

                    field = !filterItem.field.lookupModulePrimaryField ? filterItem.field : filterItem.field.lookupModulePrimaryField;

                    if (!(filterItem.operator.name === 'empty' || filterItem.operator.name === 'not_empty')) {
                        if (field.data_type === 'picklist')
                            filter.value = filter.value.label[$scope.language];

                        if (field.data_type === 'multiselect') {
                            var value = '';

                            angular.forEach(filter.value, function (picklistItem) {
                                value += picklistItem.label[$scopelanguage] + '|';
                            });

                            filter.value = value.slice(0, -1);
                        }

                        if (field.data_type === 'lookup' && field.lookup_type === 'users') {
                            if (filter.value[0].id === 0)
                                filter.value = '[me]';
                            else
                                filter.value = filter.value[0].id;
                        }

                        if (field.data_type === 'checkbox')
                            filter.value = filter.value.system_code;
                    } else {
                        filter.value = '-';
                    }

                    view.filters.push(filter);
                });

                if ($scope.view.sharing_type === 'custom') {
                    if (!$scope.view.shares) {
                        view.sharing_type = 'me';
                    } else {
                        view.shares = [];

                        angular.forEach($scope.view.shares, function (user) {
                            view.shares.push(user.id);
                        });
                    }
                }

                if (!$scope.view.id) {
                    ViewsService.create(view)
                        .then(function (response) {
                            //var viewState = cache.viewState;
                            var viewState;

                            if (!viewState) {
                                viewState = {};
                                viewState.sort_field = 'created_at';
                                viewState.sort_direction = 'desc';
                                viewState.row_per_page = 10;
                            }

                            viewState.active_view = response.data.id;

                            success();
                            $scope.pageTotal = $scope.pageTotal + 1;
                        })
                        .catch(function (data) {
                            error(data.data, data.status);
                        })
                        .finally(function () {
                            $scope.saving = false;
                        });
                } else {
                    ViewsService.update(view, $scope.view.id, $scope.view._rev)
                        .then(function () {
                            success();
                        })
                        .catch(function (data) {
                            error(data.data, data.status);
                        })
                        .finally(function () {
                            $scope.saving = false;
                        });
                }

                function success() {
                    //swal("Good job!", "You clicked the button!", "success");
                    toastr.success("Filter is saved successfully.");
                    //$state.go('studio.app.filters');
                    $scope.addNewFiltersModal.hide();
                    $scope.changePage($scope.activePage);
                }

                function error(data, status) {
                    if (status === 400) {
                        if (data && data['FilterLogic']) {
                            viewForm.filterLogic.$setValidity('filterLogic', false);
                            toastr.error($filter('translate')('View.InvalidFilterLogic'));
                            $scope.wizardStep = 2;
                        }

                        if (data && data['request._filter_logic']) {
                            viewForm.filterLogic.$setValidity('filterLogicFilters', false);
                            toastr.error($filter('translate')('View.InvalidFilterLogicFilters'));
                            $scope.wizardStep = 2;
                        }
                    }
                }
            };

            $scope.validate = function (viewForm) {

                viewForm.$submitted = true;

                if (!viewForm.label.$valid || !viewForm.module.$valid) {

                    if (viewForm.label.$error.required && viewForm.module.$error.required)
                        toastr.error($filter('translate')('Module.RequiredError'));

                    if (viewForm.label.$error.required && !viewForm.module.$error.required)
                        toastr.error("Label is required");

                    if (viewForm.module.$error.required && !viewForm.label.$error.required)
                        toastr.error("Module is required");

                    return false;
                }

                if ($scope.fields && $scope.fields.selectedFields.length < 1 && $scope.wizardStep !== 0) {

                    viewForm.$setValidity('field', false);

                    if (viewForm.$error.field)
                        toastr.error($filter('translate')('View.FieldError'));

                    $scope.background_color = "background-color: #eed3d7";
                    return false;
                }

                if (viewForm.filterLogic && !viewForm.filterLogic.$valid) {
                    return false;
                }

                //  $scope.wizardStep += $scope.view.id ? $scope.wizardStep : next ? $scope.wizardStep === 3 ? 0 : 1 : $scope.wizardStep > 0 ? -1 : $scope.wizardStep;
                return true;
            };

            $scope.costumeDate = "this_day()";
            $scope.dateFormat = [
                {
                    label: $filter('translate')('View.Second'),
                    value: "s"
                },
                {
                    label: $filter('translate')('View.Minute'),
                    value: "m"
                },
                {
                    label: $filter('translate')('View.Hour'),
                    value: "h"
                },
                {
                    label: $filter('translate')('View.Day'),
                    value: "D"
                },
                {
                    label: $filter('translate')('View.Week'),
                    value: "W"
                },
                {
                    label: $filter('translate')('View.Month'),
                    value: "M"
                },
                {
                    label: $filter('translate')('View.Year'),
                    value: "Y"
                }
            ];

            $scope.costumeDateFilter = [
                {
                    name: "thisNow",
                    label: $filter('translate')('View.Now'),
                    value: "now()"
                },
                {
                    name: "thisToday",
                    label: $filter('translate')('View.StartOfTheDay'),
                    value: "today()"
                },
                {
                    name: "thisWeek",
                    label: $filter('translate')('View.StartOfThisWeek'),
                    value: "this_week()"
                },
                {
                    name: "thisMonth",
                    label: $filter('translate')('View.StartOfThisMonth'),
                    value: "this_month()"
                },
                {
                    name: "thisYear",
                    label: $filter('translate')('View.StartOfThisYear'),
                    value: "this_year()"
                },
                {
                    name: "year",
                    label: $filter('translate')('View.NowYear'),
                    value: "year()"
                },
                {
                    name: "month",
                    label: $filter('translate')('View.NowMonth'),
                    value: "month()"
                },
                {
                    name: "day",
                    label: $filter('translate')('View.NowDay'),
                    value: "day()"
                },
                {
                    name: "costume",
                    label: $filter('translate')('View.CustomDate'),
                    value: "costume"
                },
                {
                    name: "todayNextPrev",
                    label: $filter('translate')('View.FromTheBeginningOfTheDay'),
                    value: "costumeN",
                    nextprevdatetype: "D"
                },
                {
                    name: "weekNextPrev",
                    label: $filter('translate')('View.FromTheBeginningOfTheWeek'),
                    value: "costumeW",
                    nextprevdatetype: "M"
                },
                {
                    name: "monthNextPrev",
                    label: $filter('translate')('View.FromTheBeginningOfTheMonth'),
                    value: "costumeM",
                    nextprevdatetype: "M"
                },
                {
                    name: "yearNextPrev",
                    label: $filter('translate')('View.FromTheBeginningOfTheYear'),
                    value: "costumeY",
                    nextprevdatetype: "Y"
                }
            ];

            $scope.dateChange = function (filter) {
                if (filter.costumeDate !== 'costume' && filter.costumeDate !== 'costumeN' && filter.costumeDate !== 'costumeW' && filter.costumeDate !== 'costumeM' && filter.costumeDate !== 'costumeY') {
                    filter.value = filter.costumeDate;
                }
                if (filter.costumeDate === 'costumeN' || filter.costumeDate === 'costumeW' || filter.costumeDate === 'costumeM' || filter.costumeDate === 'costumeY') {
                    filter.value = "";
                    filter.valueX = "";
                    filter.nextprevdatetype = "";

                }

            };

            $scope.nextPrevDateChange = function (filter) {
                $scope.setCostumDate(filter);
            };
            $scope.nextPrevDateTypeChange = function (filter) {
                $scope.setCostumDate(filter);
            };
            $scope.setCostumDate = function (filter) {
                if (filter.valueX === null || filter.valueX === "" || filter.valueX === undefined) {
                    filter.value = "";
                    return false;
                }
                if (filter.nextprevdatetype === undefined) {
                    filter.nextprevdatetype = $scope.dateFormat[0].value;
                }
                switch (filter.costumeDate) {
                    case "costumeN":
                        filter.value = "today(" + filter.valueX + filter.nextprevdatetype + ")";
                        break;
                    case "costumeM":
                        filter.value = "this_month(" + filter.valueX + filter.nextprevdatetype + ")";
                        break;
                    case "costumeW":
                        filter.value = "this_week(" + filter.valueX + filter.nextprevdatetype + ")";
                        break;
                    case "costumeY":
                        filter.value = "this_year(" + filter.valueX + filter.nextprevdatetype + ")";
                        break;
                }

            };

            var setFilter = function (viewFilters, moduleFields, modulePicklists, filterList) {

                for (var j = 0; j < viewFilters.length; j++) {
                    var name = viewFilters[j].field;
                    var value = viewFilters[j].value;

                    if (name.indexOf('.') > -1) {
                        name = name.split('.')[0];
                        viewFilters[j].field = name;
                    }

                    var field = $filter('filter')(moduleFields, {name: name}, true)[0];
                    var fieldValue = null;

                    if (!field)
                        return filterList;

                    switch (field.data_type) {
                        case 'picklist':
                            fieldValue = $filter('filter')(modulePicklists[field.picklist_id], {labelStr: value}, true)[0];
                            break;
                        case 'multiselect':
                            fieldValue = [];
                            var multiselectValue = value.split('|');

                            angular.forEach(multiselectValue, function (picklistLabel) {
                                var picklist = $filter('filter')(modulePicklists[field.picklist_id], {labelStr: picklistLabel}, true)[0];

                                if (picklist)
                                    fieldValue.push(picklist);
                            });
                            break;
                        case 'tag':
                            fieldValue = [];
                            var tagValue = value.split('|');

                            angular.forEach(tagValue, function (label) {
                                fieldValue.push(label);
                            });
                            break;
                        case 'lookup':
                            if (field.lookup_type === 'users') {
                                var user = {};

                                if (value === '0' || value === '[me]') {
                                    user.id = 0;
                                    user.email = '[me]';
                                    user.full_name = $filter('translate')('Common.LoggedInUser');
                                } else {
                                    if (value !== '-') {
                                        var userItem =
                                            $filter('filter')($rootScope.users, {Id: parseInt(value)}, true)[0
                                                ];
                                        user.id = userItem.Id;
                                        user.email = userItem.Email;
                                        user.full_name = userItem.FullName;
                                    }

                                    //TODO: $rootScope.users kaldirilinca duzeltilecek
                                    // ModuleService.getRecord('users', value)
                                    //     .then(function (lookupRecord) {
                                    //         fieldValue = [lookupRecord.data];
                                    //     });
                                }

                                fieldValue = [user];
                            } else {
                                fieldValue = value;
                            }
                            break;
                        case 'date':
                        case 'date_time':
                        case 'time':
                            if (!$scope.isCostumeDate($scope.view.filters[j])) {
                                fieldValue = new Date(value);
                                $scope.view.filterList[j].costumeDate = "costume";
                                $scope.view.filters[j].costumeDate = "costume";
                            } else {
                                fieldValue = $scope.view.filters[j].value;
                                $scope.view.filterList[j].costumeDate = $scope.view.filters[j].costumeDate;
                                $scope.view.filterList[j].valueX = $scope.view.filters[j].valueX;
                                $scope.view.filterList[j].nextprevdatetype = $scope.view.filters[j].nextprevdatetype;
                            }
                            break;
                        case 'checkbox':
                            fieldValue = $filter('filter')(modulePicklists.yes_no, {system_code: value}, true)[0];
                            break;
                        default:
                            fieldValue = value;
                            break;
                    }

                    filterList[j].field = field;
                    filterList[j].operator = operators[viewFilters[j].operator];
                    filterList[j].value = fieldValue;

                    if (viewFilters[j].operator === 'empty' || viewFilters[j].operator === 'not_empty') {
                        filterList[j].value = null;
                        filterList[j].disabled = true;
                    }
                }
                return filterList;
            };

            $scope.isCostumeDate = function (filter) {
                var getNumberRegex = /[^\d.-]/g;
                if (filter.value.indexOf('now(') > -1) {
                    filter.costumeDate = "now()";
                    return true;
                }
                if (filter.value.indexOf('today(') > -1) {
                    if (/\d+/.test(filter.value)) {
                        filter.costumeDate = "costumeN";
                        filter.valueX = parseFloat(filter.value.replace(/[^\d.-]/g, ''));
                        filter.nextprevdatetype = filter.value.match(/([A-z])\)/g, '')[0].match(/[A-z]/g, '')[0];
                        return true;
                    } else {
                        filter.costumeDate = "today()";
                        return true;
                    }
                }
                if (filter.value === 'year()') {
                    filter.costumeDate = "year()";
                    return true;
                }
                if (filter.value === 'month()') {
                    filter.costumeDate = "month()";
                    return true;
                }
                if (filter.value === 'day()') {
                    filter.costumeDate = "day()";
                    return true;
                }

                if (filter.value.indexOf('this_week(') > -1) {
                    if (/\d+/.test(filter.value)) {
                        filter.costumeDate = "costumeW";
                        filter.valueX = parseFloat(filter.value.replace(/[^\d.-]/g, ''));
                        filter.nextprevdatetype = filter.value.match(/([A-z])\)/g, '')[0].match(/[A-z]/g, '')[0];
                        return true;
                    } else {
                        filter.costumeDate = "this_week()";
                        return true;
                    }
                }

                if (filter.value.indexOf('this_month(') > -1) {
                    if (/\d+/.test(filter.value)) {
                        filter.costumeDate = "costumeM";
                        filter.valueX = parseFloat(filter.value.replace(/[^\d.-]/g, ''));
                        filter.nextprevdatetype = filter.value.match(/([A-z])\)/g, '')[0].match(/[A-z]/g, '')[0];
                        return true;
                    } else {
                        filter.costumeDate = "this_month()";
                        return true;
                    }
                }

                if (filter.value.indexOf('this_year(') > -1) {
                    if (/\d+/.test(filter.value)) {
                        filter.costumeDate = "costumeY";
                        filter.valueX = parseFloat(filter.value.replace(/[^\d.-]/g, ''));
                        filter.nextprevdatetype = filter.value.match(/([A-z])\)/g, '')[0].match(/[A-z]/g, '')[0];
                        return true;
                    } else {
                        filter.costumeDate = "this_year()";
                        return true;
                    }
                }
                return false;
            };

            $scope.filterChange = function (viewForm) {

                if (viewForm.filterLogic.$invalid) {
                    viewForm.filterLogic.$valid = true;
                    viewForm.filterLogic.$invalid = false;
                    viewForm.$valid = true;
                }
            };
        }
    ]);