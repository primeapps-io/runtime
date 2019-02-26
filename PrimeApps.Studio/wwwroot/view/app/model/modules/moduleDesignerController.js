'use strict';

angular.module('primeapps')

    .controller('moduleDesignerController', ['$rootScope', '$scope', '$filter', '$location', '$state', '$q', '$popover', '$modal', 'helper', '$timeout', 'dragularService', 'defaultLabels', '$interval', '$cache', 'systemRequiredFields', 'systemReadonlyFields', 'ModuleService', 'LayoutService', '$element',
        function ($rootScope, $scope, $filter, $location, $state, $q, $popover, $modal, helper, $timeout, dragularService, defaultLabels, $interval, $cache, systemRequiredFields, systemReadonlyFields, ModuleService, LayoutService, $element) {

            $rootScope.subtoggleClass = 'full-toggled2';
            $rootScope.toggleClass = 'toggled full-toggled';
            $rootScope.breadcrumblist[2].title = 'Module Designer';
            $scope.loading = true;

            $scope.templatesFields = ModuleService.getTemplateFields();

            $scope.id = $location.search().id;
            $scope.clone = $location.search().clone;
            $scope.redirect = $location.search().redirect;

            $scope.record = {};
            $scope.currentDeletedFields = [];

            // $scope.documentSearch = $rootScope.user.profile.document_search;

            $scope.dataTypes = ModuleService.getDataTypes();
            $scope.lookupUser = helper.lookupUser;
            $scope.calendarColors = [
                {dark: '#b8c110', light: '#e9ebb9'},
                {dark: '#01a0e4', light: '#b3e2f5'},
                {dark: '#61b033', light: '#d0e6c3'},
                {dark: '#ee6d1a', light: '#fad2bb'},
                {dark: '#e21550', light: '#f5bacb'},
                {dark: '#b62f7c', light: '#e8c1d7'},
                {dark: '#643a8c', light: '#d1c5db'},
                {dark: '#174c9c', light: '#bacae0'},
                {dark: '#00995e', light: '#b4e0ce'},
                {dark: '#e83a21', light: '#f7c4be'}
            ];
            $scope.addressTypes = [
                {name: 'country', value: $filter('translate')('Common.Country')},
                {name: 'city', value: $filter('translate')('Common.City')},
                {name: 'disrict', value: $filter('translate')('Common.Disrict')},
                {name: 'street', value: $filter('translate')('Common.Street')}
            ];
            $scope.lookupSearchTypes = [
                {name: 'starts_with', value: 'Starts With'},
                {name: 'contains', value: 'Contains'}
            ];
            $scope.viewTypes = [
                {name: 'dropdown', value: $filter('translate')('Common.Dropdown')},
                {name: 'radio', value: $filter('translate')('Common.Radio')},
                {name: 'checkbox', value: $filter('translate')('Common.Checkbox')}
            ];
            $scope.positions = [
                {name: 'left', value: $filter('translate')('Common.Left')},
                {name: 'right', value: $filter('translate')('Common.Right')}
            ];
            var module = {};

            $scope.dragger = function () {
                var drakeRows;
                var drakeCells;
                var templateFieldDrag;

                var setDraggableLayout = function () {
                    if (drakeRows)
                        drakeRows.destroy();

                    if (drakeCells)
                        drakeCells.destroy();
                    if (templateFieldDrag)
                        templateFieldDrag.destroy();

                    var moduleLayout = $scope.moduleLayout;

                    var templatesFields = $scope.templatesFields;

                    var container = angular.element(document.querySelector('.row-container'));
                    var templateContainer = angular.element(document.querySelector('.template-field'));

                    var rowContainers = [];
                    var cellContainers = [];
                    var templateFieldsContainers = [];
                    for (var i = 0; i < templateContainer.children().length; i++) {

                        var templateFieldContainer = templateContainer.children()[i];

                        if (templateFieldContainer.className.indexOf('template-field-box') > -1)
                            templateFieldsContainers.push(templateFieldContainer);
                    }

                    for (var i = 0; i < container.children().length; i++) {
                        var rowContainer = container.children().eq(i);

                        if (rowContainer[0].className.indexOf('subpanel') > -1)
                            rowContainers.push(rowContainer);
                    }

                    for (var j = 0; j < rowContainers.length; j++) {
                        var columnContainer = rowContainers[j].children().children().children();

                        for (var k = 0; k < columnContainer.length; k++) {
                            if (columnContainer[k].className.indexOf('cell-container') > -1)
                                cellContainers.push(columnContainer[k]);
                        }
                    }

                    drakeRows = dragularService(container, {
                        scope: $scope,
                        nameSpace: 'rows',
                        containersModel: moduleLayout.rows,
                        classes: {
                            mirror: 'gu-mirror-module',
                            transit: 'gu-transit-module'
                        },
                        moves: function (el, container, handle) {
                            return handle.classList.contains('row-handle');
                        }
                    });

                    drakeCells = dragularService(cellContainers, {
                        scope: $scope,
                        nameSpace: 'cells',
                        containersModel: (function () {
                            var containersModel = [];

                            angular.forEach(moduleLayout.rows, function (row) {
                                angular.forEach(row.columns, function (column) {
                                    containersModel.push(column.cells);
                                })
                            });

                            return containersModel;
                        })(),
                        classes: {
                            mirror: 'gu-mirror-field',
                            transit: 'gu-transit-field'
                        },
                        moves: function (el, container, handle) {
                            return handle.classList.contains('cell-handle');
                        }
                    });

                    templateFieldDrag = dragularService(templateContainer, {
                        scope: $scope,
                        nameSpace: 'cells',
                        containersModel: templatesFields,
                        copy: true,
                        classes: {
                            mirror: 'gu-mirror-field',
                            transit: 'gu-transit-field'
                        }
                    });

                    templateContainer.on('dragularenter', function (e) {
                        //console.log(e);
                    });
                    templateContainer.on('dragularleave dragularrelease', function (e) {
                        e.isDefaultPrevented();
                    });

                };

                $timeout(function () {
                    setDraggableLayout();
                }, 0);

                $scope.$on('dragulardrop', function (e, el) {
                    e.stopPropagation();
                    $timeout(function () {
                        $scope.refreshModule();
                    }, 0);
                });

                $scope.$watch('moduleChange', function (value) {
                    if (!value)
                        return;

                    $timeout(function () {
                        setDraggableLayout();
                    }, 0);
                });
            };

            $scope.initModuleDesginer = function () {

                $scope.module = ModuleService.processModule2($scope.module, $scope.modules);
                $scope.module = ModuleService.processModule($scope.module);

                $scope.moduleLayout = ModuleService.getModuleLayout($scope.module);

                $scope.dragger();

                ModuleService.getPicklists().then(function onSuccess(picklists) {
                    $scope.picklists = picklists.data;
                });

                var getMultilineTypes = function () {
                    var multilineType1 = {
                        value: 'small',
                        label: $filter('translate')('Setup.Modules.MultilineTypeSmall')
                    };
                    var multilineType2 = {
                        value: 'large',
                        label: $filter('translate')('Setup.Modules.MultilineTypeLarge')
                    };

                    $scope.multilineTypes = [];
                    $scope.multilineTypes.push(multilineType1);
                    $scope.multilineTypes.push(multilineType2);
                };

                var getLookupTypes = function (refresh) {
                    helper.getPicklists([0], refresh, $rootScope.appModules)
                        .then(function (picklists) {
                            $scope.lookupTypes = picklists['900000'];

                            var hasUserLookType = $filter('filter')($scope.lookupTypes, {name: 'users'}, true).length > 0;

                            if (!hasUserLookType) {
                                var userLookType = {};
                                userLookType.id = 900000;
                                userLookType.label = {};
                                userLookType.label.en = defaultLabels.UserLookupFieldEn;
                                userLookType.label.tr = defaultLabels.UserLookupFieldTr;
                                userLookType.order = 0;
                                userLookType.type = 0;
                                userLookType.value = 'users';

                                $scope.lookupTypes.unshift(userLookType);

                                var profileLookType = {};
                                profileLookType.id = 900100;
                                profileLookType.label = {};
                                profileLookType.label.en = defaultLabels.ProfileLookupFieldEn;
                                profileLookType.label.tr = defaultLabels.ProfileLookupFieldTr;
                                profileLookType.order = 0;
                                profileLookType.type = 0;
                                profileLookType.value = 'profiles';

                                $scope.lookupTypes.push(profileLookType);

                                var roleLookType = {};
                                roleLookType.id = 900101;
                                roleLookType.label = {};
                                roleLookType.label.en = defaultLabels.RoleLookupFieldEn;
                                roleLookType.label.tr = defaultLabels.RoleLookupFieldTr;
                                roleLookType.order = 0;
                                roleLookType.type = 0;
                                roleLookType.value = 'roles';

                                $scope.lookupTypes.push(roleLookType);
                            }
                        });
                };

                var getRoundingTypes = function () {
                    var roundingType1 = {value: 'off', label: $filter('translate')('Setup.Modules.RoundingTypeOff')};
                    var roundingType2 = {value: 'down', label: $filter('translate')('Setup.Modules.RoundingTypeDown')};
                    var roundingType3 = {value: 'up', label: $filter('translate')('Setup.Modules.RoundingTypeUp')};

                    $scope.roundingTypes = [];
                    $scope.roundingTypes.push(roundingType1);
                    $scope.roundingTypes.push(roundingType2);
                    $scope.roundingTypes.push(roundingType3);
                };

                var getSortOrderTypes = function () {
                    var sortOrderType1 = {
                        value: 'alphabetical',
                        label: $filter('translate')('Setup.Modules.PicklistSortOrderAlphabetical')
                    };
                    var sortOrderType2 = {
                        value: 'order',
                        label: $filter('translate')('Setup.Modules.PicklistSortOrderOrder')
                    };

                    $scope.sortOrderTypes = [];
                    $scope.sortOrderTypes.push(sortOrderType1);
                    $scope.sortOrderTypes.push(sortOrderType2);
                };

                var getCalendarDateTypes = function () {
                    var calendarDateType1 = {
                        value: 'start_date',
                        label: $filter('translate')('Setup.Modules.CalendarDateTypeStartDate')
                    };
                    var calendarDateType2 = {
                        value: 'end_date',
                        label: $filter('translate')('Setup.Modules.CalendarDateTypeEndDate')
                    };

                    $scope.calendarDateTypes = [];
                    $scope.calendarDateTypes.push(calendarDateType1);
                    $scope.calendarDateTypes.push(calendarDateType2);
                };

                getMultilineTypes();
                getLookupTypes(false);
                getRoundingTypes();
                getSortOrderTypes();
                getCalendarDateTypes();

                ModuleService.getDeletedModules()
                    .then(function (deletedModules) {
                        $scope.deletedModules = deletedModules;
                    });

                // ModuleService.getPicklists($scope.module)
                //     .then(function (picklists) {
                //         $scope.picklistsModule = picklists.data;
                //     });

                $scope.currenyPK = $filter('filter')($scope.module.fields, {primary: true}, true)[0];
                $scope.loading = false;
            };

            if (!$scope.id && !$scope.clone) {
                $scope.module = ModuleService.prepareDefaults(module);
                $scope.moduleLabelNotChanged = true;

                $scope.initModuleDesginer();

            }
            else {
                ModuleService.getModuleById($scope.id).then(function (result) {
                    $scope.module = result.data;
                    if (!$scope.module) {
                        toastr.warning($filter('translate')('Common.NotFound'));
                        $state.go('app.dashboard');
                        return;
                    }

                    if ($scope.clone) {
                        if ($scope.clone === 'opportunity' || $scope.clone === 'activity') {
                            toastr.warning($filter('translate')('Common.NotFound'));
                            $state.go('app.dashboard');
                            return;
                        }

                        $scope.module.label_en_plural = $scope.module.label_en_plural + ' (Copy)';
                        $scope.module.label_en_singular = $scope.module.label_en_singular + ' (Copy)';
                        $scope.module.label_tr_plural = $scope.module.label_tr_plural + ' (Kopya)';
                        $scope.module.label_tr_singular = $scope.module.label_tr_singular + ' (Kopya)';
                        $scope.module.related_modules = [];
                        $scope.moduleLabelNotChanged = true;
                        $scope.module.system_type = 'custom';
                        var sortOrders = [];

                        angular.forEach($rootScope.appModules, function (moduleItem) {
                            sortOrders.push(moduleItem.order);
                        });

                        angular.forEach($scope.module.fields, function (field) {
                            if (!field.deleted) {
                                if (systemRequiredFields.all.indexOf(field.name) < 0) {
                                    field.systemRequired = false;
                                }

                                if (systemReadonlyFields.all.indexOf(field.name) < 0) {
                                    field.systemReadonly = false;
                                }
                            }
                        });

                        var sectionsCopied = angular.copy($filter('filter')($scope.module.sections, {deleted: '!true'}, true));
                        var fieldsCopied = angular.copy($filter('filter')($scope.module.fields, {deleted: '!true'}, true));
                        var defaultFields = ['owner', 'created_by', 'created_at', 'updated_by', 'updated_at'];
                        var sectionNames = [];

                        for (var i = 0; i < sectionsCopied.length; i++) {
                            var section = sectionsCopied[i];
                            delete section.id;
                            var newName = 'custom_section' + i + 1;

                            var sectionName = $filter('filter')(sectionNames, {name: section.name}, true)[0];

                            if (!sectionName)
                                sectionNames.push({name: section.name, newName: newName});

                            section.name = newName;
                        }

                        for (var j = 0; j < fieldsCopied.length; j++) {
                            var field = fieldsCopied[j];

                            if (defaultFields.indexOf(field.name) < 0) {
                                delete field.id;
                                field.name = 'custom_field' + j + 1;
                            }

                            var sectionName = $filter('filter')(sectionNames, {name: field.section}, true)[0];

                            field.section = sectionName.newName;
                        }

                        $scope.module.sections = sectionsCopied;
                        $scope.module.fields = fieldsCopied;

                        $scope.module.order = Math.max.apply(null, sortOrders) + 1;
                        $scope.module.name = 'custom_module' + module.order + '_c';
                    }

                    if (!$scope.module.detail_view_type)
                        $scope.module.detail_view_type = 'tab';

                    $scope.initModuleDesginer();

                });

            }

            $scope.lookup = function (searchTerm) {
                if (!$scope.currentLookupField.lookupType) {
                    var deferred = $q.defer();
                    deferred.resolve([]);
                    return deferred.promise;
                }

                if ($scope.currentLookupField.lookup_type === 'users' && !$scope.currentLookupField.lookupModulePrimaryField) {
                    var userModulePrimaryField = {};
                    userModulePrimaryField.data_type = 'text_single';
                    userModulePrimaryField.name = 'full_name';
                    $scope.currentLookupField.lookupModulePrimaryField = userModulePrimaryField;
                }

                if (($scope.currentLookupField.lookupModulePrimaryField.data_type === 'number' || $scope.currentLookupField.lookupModulePrimaryField.data_type === 'number_auto') && isNaN(parseFloat(searchTerm))) {
                    $scope.$broadcast('angucomplete-alt:clearInput', $scope.currentLookupField.name);
                    return $q.defer().promise;
                }

                $scope.currentLookupField.data_type = $scope.currentLookupField.dataType.name;
                $scope.currentLookupField.lookup_type = $scope.currentLookupField.lookupType.value;

                return ModuleService.lookup(searchTerm, $scope.currentLookupField, $scope.record);
            };

            $scope.setCurrentLookupField = function (field) {
                $scope.currentLookupField = angular.copy(field);
            };

            $scope.refreshModule = function () {
                ModuleService.refreshModule($scope.moduleLayout, $scope.module);
            };

            $scope.showEditModal = function (isModuleSaving) {
                $scope.moduleState = angular.copy($scope.module);
                $scope.isModuleSaving = isModuleSaving;
                $scope.editModal = $scope.editModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/model/modules/editForm.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });
                $scope.icons = ModuleService.getIcons();
                $scope.showAdvancedOptionsEdit = false;
                $scope.editModal.$promise.then($scope.editModal.show);
            };

            $scope.cancelModule = function () {
                angular.forEach($scope.module, function (value, key) {
                    if (angular.isArray($scope.module[key]) || angular.isObject($scope.module[key]))
                        return;

                    $scope.module[key] = $scope.moduleState[key];
                });

                $scope.editModal.hide();
            };

            $scope.hasCalendarDateFields = function () {
                if ($scope.module.name === 'activities')
                    return true;

                var startDateField = $filter('filter')($scope.module.fields, {calendar_date_type: 'start_date'}, true)[0];
                var endDateField = $filter('filter')($scope.module.fields, {calendar_date_type: 'end_date'}, true)[0];

                return startDateField && endDateField;
            };

            $scope.newField = function (name) {
                var field = {};
                field.label_en = name;
                field.label_tr = name;
                field.validation = {};
                field.validation.required = false;
                field.primary = false;
                field.display_form = true;
                field.display_detail = true;
                field.display_list = true;
                field.inline_edit = true;
                field.editable = true;
                field.show_label = true;
                field.deleted = false;
                field.name = name;
                field.isNew = true;
                field.permissions = [];
                return field;
            };

            $scope.showFieldModal = function (row, column, field) {
                $scope.currentRow = row;
                $scope.currentColumn = column;
                $scope.showPermissionWarning = false;
                $scope.fieldActiveSection ="properties";
                if (!field) {
                    field = $scope.newField();
                }
                else {
                    if (field.data_type === 'lookup')
                        field.lookupType = $filter('filter')($scope.lookupTypes, {value: field.lookup_type}, true)[0];

                    $scope.currentFieldState = angular.copy(field);

                    if (field.default_value && field.data_type === 'lookup') {
                        if (field.lookup_type !== 'users') {
                            var lookupId = parseInt(field.default_value);

                            ModuleService.getRecord(field.lookup_type, lookupId, true)
                                .then(function (response) {
                                    var lookupObject = {};
                                    lookupObject.id = response.data.id;
                                    lookupObject.primary_value = response.data[field.lookupModulePrimaryField.name];
                                    field.temporary_default_value = lookupObject;
                                });
                        }
                        else {
                            if (field.default_value === '[me]') {
                                var lookupObject = {};
                                lookupObject.id = 0;
                                lookupObject.email = '[me]';
                                lookupObject.full_name = $filter('translate')('Common.LoggedInUser');
                                field.default_value = [lookupObject];
                            }
                            else {
                                ModuleService.getRecord(field.lookup_type, lookupId, true)
                                    .then(function (response) {
                                        var lookupObject = {};
                                        lookupObject.id = response.data.id;
                                        lookupObject.email = response.data.email;
                                        lookupObject.full_name = response.data[field.lookupModulePrimaryField.name];
                                        field.default_value = [lookupObject];
                                    });
                            }
                        }
                    }

                    if (field.default_value && (field.data_type === 'date_time' || field.data_type === 'date' || field.data_type === 'time') && field.default_value === '[now]')
                        field.default_value_now = true;

                    if ((field.data_type === 'picklist' || field.data_type === 'multiselect') && field.picklist_id) {
                        ModuleService.getPicklist(field.picklist_id)
                            .then(function (response) {
                                $scope.defaulPicklistValues = response.data.items;
                                if (field.default_value) {
                                    if (field.data_type === 'picklist') {
                                        field.default_value = parseInt(field.default_value);
                                        field.default_value = $filter('filter')($scope.defaulPicklistValues, {id: parseInt(field.default_value)}, true)[0];
                                    }
                                    else if (field.data_type === 'multiselect') {
                                        var picklistIds = field.default_value.split(';');
                                        var values = [];

                                        angular.forEach(picklistIds, function (picklistId) {
                                            var picklistRecord = $filter('filter')($scope.defaulPicklistValues, {id: parseInt(picklistId)}, true)[0];
                                            picklistRecord.labelStr = picklistRecord['label_' + $rootScope.user.tenant_language];
                                            values.push(picklistRecord);
                                        });

                                        $scope.currentField.defaultValueMultiselect = values;
                                    }
                                }
                            });
                    }

                    if (field.data_type === 'checkbox') {
                        if (field.default_value === 'true')
                            field.default_value = true;
                        else
                            field.default_value = false;
                    }

                    if (field.encrypted && field.encryption_authorized_users_list.length > 0) {
                        var userList = [];
                        for (var k = 0; k < field.encryption_authorized_users_list.length; k++) {
                            var user = $filter('filter')($rootScope.users, {id: parseInt(field.encryption_authorized_users_list[k])}, true)[0];
                            userList.push(user);
                        }
                        console.log(userList)
                        field.encryption_authorized_users = userList;
                        console.log(field)
                    }
                }

                $scope.currentField = field;
                $scope.currentFieldState = angular.copy(field);
                $scope.showAdvancedOptions = false;
                $scope.currentField.dataType = $filter('filter')($scope.dataTypes, {name: $scope.currentField.data_type}, true)[0];
                $scope.setMultilineDataType();
                var url = angular.copy(window.location.hash);
                $scope.fieldModal = $scope.fieldModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/model/modules/fieldForm.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });
                $scope.fieldModal.$promise.then(function () {
                    $scope.fieldModal.show();
                });


            };

            $scope.multiselectEncryptionUsers = function () {
                return $filter('filter')($rootScope.users, function (value, index, array) {
                    return (value.order !== 0);
                }, true);
            };

            $scope.encryptedFieldChange = function () {
                console.log($scope)
                if ($scope.currentField.encrypted)
                    $scope.currentField.display_list = false;
            }

            $scope.changeShowLabel = function () {
                if (!$scope.currentField.show_label) {
                    $scope.currentField.editable = false;
                }
            };

            $scope.changeDefaultValue = function () {
                if ($scope.currentField.default_value) {
                    $scope.currentField.show_label = true;
                }
            };

            $scope.dataTypeChanged = function () {
                if (!$scope.currentField.dataType)
                    return;

                if ($scope.currentField.isNew) {
                    var label = $scope.currentField['label_' + $rootScope.language];
                    var dataType = $scope.currentField.dataType;
                    var required = $scope.currentField.validation.required;

                    $scope.currentField = newField();
                    $scope.currentField['label_' + $rootScope.language] = label;

                    if (dataType)
                        $scope.currentField.dataType = dataType;

                    if (required)
                        $scope.currentField.validation.required = required;

                    $scope.showAdvancedOptions = false;

                    switch (dataType.name) {
                        case 'text_single':
                            $scope.currentField.validation.max_length = 400;
                        case 'picklist':
                        case 'multiselect':
                            $scope.currentField.picklist_sortorder = 'order';
                            break;
                        case 'tag':
                            $scope.currentField.picklist_sortorder = 'order';
                            break;
                        case 'rating':
                            $scope.currentField.validation.min_length = 5;
                            break;
                        case 'json':
                            $scope.currentField.validation.required = true;
                            $scope.currentField.display_form = false;
                            $scope.currentField.display_list = false;
                            $scope.currentField.display_detail = false;
                            $scope.currentField.editable = false;
                            $scope.currentField.inline_edit = false;
                            break;
                    }
                }
            };

            $scope.requiredChanged = function () {
                if ($scope.currentField.validation.required) {
                    $scope.currentField.display_form = true;

                    angular.forEach($scope.currentField.permissions, function (permission) {
                        permission.type = 'full';
                    });
                }
                else {
                    $scope.currentField.permissions = $scope.currentFieldState.permissions;
                }

                $scope.showPermissionWarning = !$scope.currentFieldState.validation.required && $scope.currentField.validation.required;
            };

            $scope.lookupTypeChanged = function () {
                if (!$scope.currentField.lookupType)
                    return;

                $scope.currentField.default_value = null;
                $scope.currentField.temporary_default_value = null;
                $scope.$broadcast('angucomplete-alt:clearInput', 'lookupDefaultValue');

                var lookupModule = $filter('filter')($rootScope.appModules, {name: $scope.currentField.lookupType.value}, true)[0];

                if (lookupModule) {
                    var filterArray = [
                        {
                            column: "Primary",
                            operator: "is",
                            value: true
                        }
                    ];

                    ModuleService.getModuleFields(lookupModule.name, filterArray).then(function (result) {
                        $scope.currentField.lookupModulePrimaryField = $filter('filter')(result.data, {primary: true}, true)[0];

                    });

                }
            };

            $scope.calendarDateTypeChanged = function () {
                if ($scope.currentField.calendar_date_type) {
                    if (!$scope.currentField.validation)
                        $scope.currentField.validation = {};

                    $scope.currentField.validation.required = true;
                }
                else {
                    $scope.currentField.validation.required = $scope.currentFieldState.validation.required;
                }
            };

            $scope.defaultValueNowChanged = function () {
                if ($scope.currentField.default_value_now)
                    $scope.currentField.default_value = null;
            };

            $scope.setMultilineDataType = function () {
                if (!$scope.currentField.dataType || $scope.currentField.dataType.name != 'text_multi')
                    return;

                if ($scope.currentField.multiline_type && $scope.currentField.multiline_type === 'large') {
                    $scope.currentField.dataType.maxLength = 5;
                    $scope.currentField.dataType.max = 32000;
                    $scope.multiLineShowHtml = true;
                }
                else {
                    $scope.currentField.dataType.maxLength = 4;
                    $scope.currentField.dataType.max = 2000;
                    $scope.multiLineShowHtml = false;
                }
            };

            // $scope.validateInlineEdit = function () {
            //     if ($scope.currentField.multiline_type_use_html === true) {
            //         $scope.currentField.inline_edit = false;
            //     }
            // }

            $scope.bindPicklistDragDrop = function () {
                $timeout(function () {
                    if ($scope.drakePicklist) {
                        $scope.drakePicklist.destroy();
                        $scope.drakePicklist = null;
                    }

                    var picklistContainer = document.querySelector('#picklistContainer');
                    var picklistOptionContainer = document.querySelector('#picklistOptionContainer');
                    var rightTopBar = document.getElementById('rightTopBar');
                    var rightBottomBar = document.getElementById('rightBottomBar');
                    var timer;

                    $scope.drakePicklist = dragularService([picklistContainer], {
                        scope: $scope,
                        containersModel: [$scope.picklistModel.items],
                        classes: {
                            mirror: 'gu-mirror-option',
                            transit: 'gu-transit-option'
                        },
                        lockY: true,
                        moves: function (el, container, handle) {
                            return handle.classList.contains('option-handle');
                        }
                    });

                    angular.element(picklistContainer).on('dragulardrop', function () {
                        var picklistSortOrder = $filter('filter')($scope.sortOrderTypes, {value: 'order'}, true)[0];
                        $scope.currentField.picklist_sortorder = picklistSortOrder;
                    });

                    registerEvents(rightTopBar, picklistOptionContainer, -5);
                    registerEvents(rightBottomBar, picklistOptionContainer, 5);

                    function registerEvents(bar, container, inc, speed) {
                        if (!speed) {
                            speed = 10;
                        }

                        angular.element(bar).on('dragularenter', function () {
                            container.scrollTop += inc;

                            timer = $interval(function moveScroll() {
                                container.scrollTop += inc;
                            }, speed);
                        });

                        angular.element(bar).on('dragularleave dragularrelease', function () {
                            $interval.cancel(timer);
                        });
                    }
                }, 100);
            };

            var combinationDataTypes = [
                'text_single',
                'number',
                'number_decimal',
                'number_auto',
                'currency',
                'date',
                'date_time',
                'time',
                'email',
                'picklist',
                'combination'
            ];

            $scope.filterToCombinations = function (field) {
                return !field.systemRequired && !field.systemReadonly && combinationDataTypes.indexOf(field.data_type) > -1 && !field.deleted;
            };

            $scope.filterToUniqueCombinations = function (field) {
                if (!field.systemReadonly && field.data_type != 'combination' && field.data_type != 'number_auto' && field.data_type != 'text_multi' && field.data_type != 'checkbox')
                    return true;

                return false;
            };

            $scope.showSectionModal = function (section) {
                if (!section) {
                    section = {};
                    section.column_count = 2;
                    section.label_en = '';
                    section.label_tr = '';
                    section.display_form = true;
                    section.display_detail = true;
                    section.deleted = false;
                    $scope.showPermissionWarning = false;
                    section.permissions = [];
                    angular.forEach($rootScope.appProfiles, function (profile) {
                        if (profile.is_persistent && profile.has_admin_rights)
                            profile.name = $filter('translate')('Setup.Profiles.Administrator');

                        if (profile.is_persistent && !profile.has_admin_rights)
                            profile.name = $filter('translate')('Setup.Profiles.Standard');

                        section.permissions.push({
                            profile_id: profile.id,
                            profile_name: profile.name,
                            type: 'full',
                            profile_is_admin: profile.has_admin_rights
                        });
                    });
                    var sortOrders = [];

                    angular.forEach($scope.module.sections, function (item) {
                        sortOrders.push(item.order);
                    });

                    section.order = Math.max.apply(null, sortOrders) + 1;
                    section.name = 'custom_section' + section.order;
                    section.columns = [];

                    for (var i = 1; i <= section.column_count; i++) {
                        var column = {};
                        column.no = i;

                        section.columns.push(column);
                    }

                    section.isNew = true;
                }
                else {
                    $scope.currentSectionState = angular.copy(section);
                }

                $scope.currentSection = section;
                $scope.currentSectionState = angular.copy(section);

                $scope.sectionModal = $scope.sectionModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/model/modules/sectionForm.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.sectionModal.$promise.then($scope.sectionModal.show);
            };

            $scope.saveSettings = function (editForm) {
                if (!editForm.$valid)
                    return;

                $scope.moduleLabelNotChanged = false;

                if ($scope.module.calendar_color_dark)
                    $scope.module.calendar_color_light = $filter('filter')($scope.calendarColors, {dark: $scope.module.calendar_color_dark}, true)[0].light;

                if (!$scope.isModuleSaving)
                    $scope.editModal.hide();
                else
                    $scope.saveModule();
            };

            $scope.saveField = function (fieldForm) {
                if (!fieldForm.$valid) {
                    $timeout(function () {
                        var scroller = document.getElementById('field-form-body');
                        scroller.scrollTop = scroller.scrollHeight;
                    }, 0, false);

                    return;
                }
                if ($scope.currentField.dataType.name === 'lookup' && !$scope.currentField.id) {
                    var lookupcount = $filter('filter')($scope.module.fields, {
                        data_type: 'lookup',
                        deleted: false
                    }, true);
                    if (lookupcount.length > 11) {
                        toastr.warning($filter('translate')('Setup.Modules.MaxLookupCount'));
                        return;
                    }
                }

                $scope.showAdvancedOptions = false;

                if ($scope.currentField.lookupType) {
                    if ($scope.currentField.show_as_dropdown) {
                        // $scope.currentField.inline_edit = false;
                    }
                    $scope.currentField.lookup_type = $scope.currentField.lookupType.value;
                    delete $scope.currentField.lookupType;
                    delete $scope.currentField.temporary_default_value;
                }

                if ($scope.currentField.dataType.name === 'checkbox' && !$scope.currentField.default_value)
                    $scope.currentField.default_value = false;

                if ($scope.currentField.dataType.name === 'picklist' && $scope.currentField.default_value && $scope.currentField.default_value.id)
                    $scope.currentField.default_value = $scope.currentField.default_value.id;

                if ($scope.currentField.default_value_now !== undefined && $scope.currentField.default_value_now != null && ($scope.currentField.dataType.name === 'date_time' || $scope.currentField.dataType.name === 'date' || $scope.currentField.dataType.name === 'time')) {
                    if ($scope.currentField.default_value_now === true)
                        $scope.currentField.default_value = '[now]';
                    else
                        $scope.currentField.default_value = null;
                }

                //model settings for tags-input dataType
                if ($scope.currentField.defaultValueMultiselect) {
                    $scope.currentField.default_value = '';
                    angular.forEach($scope.currentField.defaultValueMultiselect, function (item) {
                        $scope.currentField.default_value += (item.id) + ';';
                    });
                    $scope.currentField.default_value = $scope.currentField.default_value.slice(0, -1);
                }

                if ($scope.currentField.isNew) {
                    delete $scope.currentField.isNew;

                    //var otherLanguage = $rootScope.language === 'en' ? 'tr' : 'en';
                    // var field = angular.copy($scope.currentField);
                    //field.data_type = field.dataType.name;
                    //field.section = $scope.currentRow.section.name;
                    // field.section_column = $scope.currentColumn.column.no;
                    // field['label_' + otherLanguage] = field['label_' + $rootScope.language];
                    // field.validation.readonly = false;
                    //
                    // $scope.module.fields.push(field);
                    // $scope.moduleLayout = ModuleService.getModuleLayout($scope.module);
                    // $scope.fieldModal.hide();
                    // $scope.moduleChange = new Date();
                }


                if ($scope.currentField.dataType.name === 'lookup' && $scope.currentField.lookup_type !== 'users' && $scope.currentField.default_value)
                    $scope.currentField.default_value = $scope.currentField.default_value.id;

                if ($scope.currentField.dataType.name === 'lookup' && $scope.currentField.lookup_type === 'users' && $scope.currentField.default_value) {
                    var defaultValue = $scope.currentField.default_value[0];

                    if (defaultValue.id === 0)
                        $scope.currentField.default_value = '[me]';
                    else
                        $scope.currentField.default_value = defaultValue.id;
                }

                if ($scope.currentField.calendar_date_type) {
                    for (var i = 0; i < module.fields.length; i++) {
                        var moduleField = module.fields[i];

                        if (moduleField.calendar_date_type && moduleField.calendar_date_type === $scope.currentField.calendar_date_type)
                            moduleField.calendar_date_type = null;
                    }
                }
                $scope.fieldModal.hide();
            };

            $scope.saveSection = function (sectionForm) {
                if (!sectionForm.$valid)
                    return;

                if ($scope.currentSection.isNew) {
                    delete $scope.currentSection.isNew;

                    var otherLanguage = $rootScope.language === 'en' ? 'tr' : 'en';
                    var section = angular.copy($scope.currentSection);
                    section['label_' + otherLanguage] = section['label_' + $rootScope.language];

                    $scope.module.sections.push(section);
                    $scope.moduleLayout = ModuleService.getModuleLayout($scope.module);
                    $scope.sectionModal.hide();
                    $scope.moduleChange = new Date();
                }
                else {
                    $scope.sectionModal.hide();
                }
            };

            $scope.deleteField = function (fieldName) {
                var field = $filter('filter')($scope.module.fields, {name: fieldName}, true)[0];

                if (field.name.indexOf('custom_field') > -1) {
                    var fieldIndex = helper.arrayObjectIndexOf($scope.module.fields, field);
                    $scope.module.fields.splice(fieldIndex, 1);
                }
                else {
                    field.deleted = true;
                    field.order = 999;

                    var deletedField = {
                        name: field.name,
                        id: field.id
                    };

                    $scope.currentDeletedFields.push(deletedField);
                }

                $scope.moduleLayout = ModuleService.getModuleLayout($scope.module);
                $scope.moduleChange = new Date();
            };

            $scope.deleteSection = function (sectionName) {
                var section = $filter('filter')($scope.module.sections, {name: sectionName}, true)[0];
                var sectionFields = $filter('filter')($scope.module.fields, {section: section.name}, true);

                if (section.name.indexOf('custom_section') > -1) {
                    var sectionIndex = helper.arrayObjectIndexOf($scope.module.sections, section);
                    $scope.module.sections.splice(sectionIndex, 1);

                    angular.forEach(sectionFields, function (field) {
                        if (field.name.indexOf('custom_field') > -1) {
                            var fieldIndex = helper.arrayObjectIndexOf($scope.module.fields, field);
                            $scope.module.fields.splice(fieldIndex, 1);
                        }
                        else {
                            field.deleted = true;
                        }
                    });
                }
                else {
                    section.deleted = true;

                    angular.forEach(sectionFields, function (field) {
                        field.deleted = true;
                    });
                }

                $scope.moduleLayout = ModuleService.getModuleLayout($scope.module);
                $scope.moduleChange = new Date();
            };

            $scope.changeSectionColumn = function (section) {
                $scope.moduleLayout = ModuleService.getModuleLayout($scope.module);
                $scope.moduleChange = new Date();
            };

            $scope.cancelField = function () {
                if ($scope.currentField.isNew) {
                    if ($scope.picklistsModule)
                        delete $scope.picklistsModule[$scope.currentField.name];

                    $scope.currentField = null;
                    $scope.fieldModal.hide();
                    return;
                }

                angular.forEach($scope.currentField, function (value, key) {
                    $scope.currentField[key] = $scope.currentFieldState[key];
                });

                $scope.fieldModal.hide();
            };

            $scope.cancelSection = function () {
                angular.forEach($scope.currentSection, function (value, key) {
                    $scope.currentSection[key] = $scope.currentSectionState[key];
                });

                $scope.sectionModal.hide();
            };

            $scope.getPicklistById = function (id) {
                ModuleService.getPicklist(id)
                    .then(function (response) {
                        $scope.defaulPicklistValues = response.data.items;
                    });
            };

            $scope.fieldValueChange = function (field) {
                ModuleService.setDependency(field, $scope.module, $scope.record, $scope.picklistsModule);
                ModuleService.setDisplayDependency($scope.module, $scope.record);

                if ($scope.record.currency)
                    $scope.currencySymbol = $scope.record.currency.value || $rootScope.currencySymbol;
            };

            //multiselect default value
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

            $scope.newPicklist = function () {
                $scope.picklistModel = {};
                $scope.showPicklistForm = true;
            };

            $scope.MinMaxValue = function () {
                if ($scope.currentField.validation.min_length && !$scope.currentField.validation.max_length || parseInt($scope.currentField.validation.min_length) > parseInt($scope.currentField.validation.max_length)) {
                    $scope.currentField.validation.max_length = $scope.currentField.validation.min_length;
                }
            };

            $scope.editPicklist = function () {
                ModuleService.getPicklist($scope.currentField.picklist_id)
                    .then(function onSuccess(picklist) {
                        $scope.picklistModel = ModuleService.processPicklist(picklist.data);
                        $scope.showPicklistForm = true;
                        //$scope.bindPicklistDragDrop();
                    });
            };

            $scope.newOption = function (picklistForm) {
                picklistForm.$submitted = false;
                picklistForm.$setValidity('minimum', true);

                if (!$scope.picklistModel.items)
                    $scope.picklistModel.items = [];

                var picklistItem = {};
                picklistItem.label = '';

                var sortOrders = [];

                angular.forEach($scope.picklistModel.items, function (item) {
                    sortOrders.push(item.order);
                });

                if ($scope.picklistModel.items.length > 0)
                    picklistItem.order = Math.max.apply(null, sortOrders) + 1;
                else
                    picklistItem.order = 1;

                picklistItem.track = picklistItem.order;
                $scope.picklistModel.items.push(picklistItem);

                $scope.bindPicklistDragDrop();
            };

            $scope.deleteOption = function (picklistItem) {
                $scope.picklistModel.items.splice($scope.picklistModel.items.indexOf(picklistItem), 1);
            };

            $scope.savePicklist = function (picklistForm) {
                if (!picklistForm.$valid)
                    return;

                var existingPicklist = null;

                if ($rootScope.language === 'tr')
                    existingPicklist = $filter('filter')($scope.picklists, {label_tr: $scope.picklistModel.label}, true)[0];
                else
                    existingPicklist = $filter('filter')($scope.picklists, {label_en: $scope.picklistModel.label}, true)[0];

                if (!$scope.picklistModel.id && existingPicklist) {
                    picklistForm.$setValidity('unique', false);
                    return;
                }

                if ($scope.picklistModel.id && existingPicklist && existingPicklist.id != $scope.picklistModel.id) {
                    picklistForm.$setValidity('unique', false);
                    return;
                }

                if (!$scope.picklistModel.items || $scope.picklistModel.items.length < 2) {
                    picklistForm.$setValidity('minimum', false);
                    return;
                }

                if (!picklistForm.$valid)
                    return;

                for (var i = 0; i < $scope.picklistModel.items.length; i++) {
                    var picklistItem = $scope.picklistModel.items[i];
                    picklistItem.order = i + 1;
                }

                $scope.picklistSaving = true;
                ModuleService.preparePicklist($scope.picklistModel);

                //TODO
                // if (!$scope.picklistModel.id) {
                //     ModuleService.createPicklist($scope.picklistModel)
                //         .then(function onSuccess(response) {
                //             if (!response.data.id) {
                //                 toastr.warning($filter('translate')('Common.NotFound'));
                //                 $scope.picklistSaving = false;
                //                 return;
                //             }
                //
                //             ModuleService.getPicklists()
                //                 .then(function (picklists) {
                //                     if (picklists.data) {
                //                         $scope.picklists = picklists.data;
                //                         $scope.currentField.picklist_id = response.data.id;
                //                     }
                //                     $scope.showPicklistForm = false;
                //                 })
                //                 .catch(function onError() {
                //                     $scope.picklistSaving = true;
                //                 });
                //         })
                //         .catch(function onError() {
                //             $scope.picklistSaving = true;
                //         });
                // }
                // else {
                //     ModuleService.updatePicklist($scope.picklistModel)
                //         .then(function onSuccess() {
                //             ModuleService.getPicklists()
                //                 .then(function onSuccess(picklists) {
                //                     $scope.picklists = picklists.data;
                //                     $scope.showPicklistForm = false;
                //                     $cache.remove('picklist_' + $scope.picklistModel.id);
                //                 })
                //                 .catch(function onError() {
                //                     $scope.picklistSaving = true;
                //                 });
                //         })
                //         .catch(function onError() {
                //             $scope.picklistSaving = true;
                //         });
                // }
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

            $scope.makePrimary = function (field) {
                field.primary = true;
                field.validation.required = true;

                angular.forEach($scope.module.fields, function (fieldItem) {
                    fieldItem.primary = fieldItem.name === field.name;
                });

                $scope.moduleLayout = ModuleService.getModuleLayout($scope.module);
                $scope.moduleChange = new Date();
            };

            $scope.makePrimaryLookup = function (field) {
                field.primary_lookup = true;
                field.validation.required = true;

                angular.forEach($scope.module.fields, function (fieldItem) {
                    fieldItem.primary_lookup = fieldItem.name === field.name;
                });

                $scope.moduleLayout = ModuleService.getModuleLayout($scope.module);
                $scope.moduleChange = new Date();
            };

            var checkRequiredFields = function () {
                $scope.notValidFields = [];
                var allowedFields = ['created_by', 'created_at', 'updated_by', 'updated_at', 'owner'];

                angular.forEach($scope.module.fields, function (field) {
                    var section = $filter('filter')($scope.module.sections, {name: field.section}, true)[0];

                    if (!section.display_form && field.validation.required && allowedFields.indexOf(field.name) < 0)
                        $scope.notValidFields.push(field);
                });

                if ($scope.notValidFields.length) {
                    $scope.fieldErrorModal = $scope.fieldErrorModal || $modal({
                        scope: $scope,
                        templateUrl: 'view/app/model/modules/warningRequiredFieldDisplay.html',
                        animation: '',
                        backdrop: 'static',
                        show: false
                    });

                    $scope.fieldErrorModal.$promise.then($scope.fieldErrorModal.show);

                    return false;
                }

                return true;
            };

            var updateView = function (views, cacheKey) {
                angular.forEach(views, function (view, key) {
                    angular.forEach(view.fields, function (_field, key) {
                        if (_field.field.split(".")[1] == $scope.module.name) {
                            var primaryField = $filter('filter')($scope.module.fields, {primary: true}, true)[0];
                            _field.field = _field.field.replace(_field.field.split(".")[2], primaryField.name);
                            ModuleService.updateView(view, view.id, undefined);
                        }
                    });
                });
                if (cacheKey)
                    $cache.remove(cacheKey + "_" + cacheKey);
            };

            $scope.saveModule = function () {
                if (!checkRequiredFields())
                    return;

                if (!$scope.isModuleSaving)
                    return;

                if ((!$scope.id || $scope.clone) && $scope.moduleLabelNotChanged) {
                    $scope.showEditModal(true);
                    return;
                }
                //When update modelu primary key also change view lookup view.
                var newPK = $filter('filter')($scope.module.fields, {primary: true}, true)[0];
                if ($scope.currenyPK.name !== newPK.name) {
                    for (var moduleKey = $rootScope.appModules.length - 1; moduleKey >= 0; moduleKey--) {
                        for (var fieldKey = $rootScope.appModules[moduleKey].fields.length - 1; fieldKey >= 0; fieldKey--) {
                            if ($rootScope.appModules[moduleKey].fields[fieldKey].lookup_type == $scope.module.name) {
                                var cacheKey = $rootScope.appModules[moduleKey].name;
                                var cache = $cache.get(cacheKey + "_" + cacheKey);
                                if (!cache) {
                                    ModuleService.getViews($rootScope.appModules[moduleKey].id, undefined, undefined)
                                        .then(function (views) {
                                            updateView(views);

                                        });
                                }
                                else {
                                    updateView(cache.views, cacheKey);
                                }
                            }
                        }
                    }
                }

                $scope.saving = true;
                var deletedFields = $filter('filter')($scope.module.fields, {deleted: true}, true);
                ModuleService.refreshModule($scope.moduleLayout, $scope.module);

                if (deletedFields.length)
                    $scope.module.fields = $scope.module.fields.concat(deletedFields);

                var moduleModel = ModuleService.prepareModule(angular.copy($scope.module), $scope.picklistsModule, $scope.deletedModules);
                //moduleModel.order = 6;//TODO: add an order backend

                if (angular.isObject(moduleModel.menu_icon))
                    moduleModel.menu_icon = moduleModel.menu_icon.value;

                if (!$scope.id || $scope.clone) {
                    ModuleService.moduleCreate(moduleModel).then(function () {
                        $scope.saving = false;
                        $state.go('studio.app.modules', {
                            orgId: $rootScope.currentOrgId,
                            appId: $rootScope.currentAppId
                        });
                    });
                    $scope.editModal.hide();
                }
                else {
                    ModuleService.moduleUpdate(moduleModel, moduleModel.id).then(function () {
                        $scope.saving = false;
                        $state.go('studio.app.modules', {
                            orgId: $rootScope.currentOrgId,
                            appId: $rootScope.currentAppId
                        });
                    });
                    if ($scope.editModal)
                        $scope.editModal.hide();
                }

            }

        }

    ]);