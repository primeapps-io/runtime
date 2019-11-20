'use strict';

angular.module('primeapps')

    .factory('ModuleService', ['$rootScope', '$http', 'config', '$filter', '$q', 'helper', 'defaultLabels', 'activityTypes', '$cache', 'dataTypes', 'operators', 'systemFields', 'yesNo', 'transactionTypes', 'systemRequiredFields', 'icons2', 'icons', 'systemReadonlyFields',
        function ($rootScope, $http, config, $filter, $q, helper, defaultLabels, activityTypes, $cache, dataTypes, operators, systemFields, yesNo, transactionTypes, systemRequiredFields, icons2, icons, systemReadonlyFields) {

            return {
                count: function () {
                    return $http.get(config.apiUrl + 'module/count');
                },
                find: function (data) {
                    return $http.post(config.apiUrl + 'module/find', data);
                },
                profileSettingsCount: function (id) {
                    return $http.get(config.apiUrl + 'module_profile_settings/count/' + id);
                },
                delete: function (id) {
                    return $http.delete(config.apiUrl + 'module/delete/' + id);
                },
                profileSettingsFind: function (data) {
                    return $http.post(config.apiUrl + 'module_profile_settings/find', data);
                },
                moduleCreate: function (module) {
                    return $http.post(config.apiUrl + 'module/create', module);
                },
                getModuleById: function (id) {
                    return $http.get(config.apiUrl + 'module/get_by_id/' + id);
                },
                getModuleByName: function (moduleName) {
                    return $http.get(config.apiUrl + 'module/get_by_name/' + moduleName);
                },
                getIcons: function (version) {
                    if (version === 2)
                        return icons2.icons;
                    else
                        return icons.icons
                },
                getSystemRequiredFields: function () {
                    return systemRequiredFields;
                },
                getSystemReadonlyFields: function () {
                    return systemReadonlyFields;
                },
                getModules: function () {
                    return $http.get(config.apiUrl + 'module/get_all');
                },
                // delete: function (id) {
                //     return $http.delete(config.apiUrl + 'module/delete/' + id);
                // },
                newField: function (dataType) {

                    var field = {};
                    field.label_en = dataType.label.en;
                    field.data_type = dataType.name;
                    field.label_tr = dataType.label.tr;
                    field.label_en = dataType.label.en;
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
                    field.name = "custom_field" + dataType.name;
                    field.isNew = true;
                    field.permissions = [];
                    field.firstDrag = true;
                    angular.forEach($rootScope.appProfiles, function (profile) {
                        if (profile.is_persistent && profile.has_admin_rights)
                            profile.name = $filter('translate')('Setup.Profiles.Administrator');

                        if (profile.is_persistent && !profile.has_admin_rights)
                            profile.name = $filter('translate')('Setup.Profiles.Standard');

                        field.permissions.push({
                            profile_id: profile.id,
                            profile_name: profile.name,
                            type: 'full',
                            profile_is_admin: profile.has_admin_rights
                        });
                    });

                    return field;
                },
                getModuleFields: function (moduleName) {
                    var deferred = $q.defer();

                    $http.get(config.apiUrl + 'module/get_module_fields?moduleName=' + moduleName).then(function (response) {
                        deferred.resolve(response);
                    }).catch(function (reason) {
                        deferred.reject(reason);
                    });

                    return deferred.promise;
                },
                getTemplateFields: function () {
                    var fields = [];
                    var that = this;
                    angular.forEach(dataTypes, function (dataType) {
                        switch (dataType.name) {
                            case 'text_single':
                                fields.push({
                                    icon: "k-icon k-i-foreground-color",
                                    field: that.newField(dataType)
                                });
                                break;
                            case 'text_multi':
                                fields.push({
                                    icon: "k-i-table-align-top-left",
                                    field: that.newField(dataType)
                                });
                                break;
                            case 'number':
                                fields.push({
                                    icon: "k-i-custom-format",
                                    field: that.newField(dataType)

                                });

                                break;
                            case 'number_auto':
                                fields.push({
                                    icon: "k-i-list-numbered",
                                    field: that.newField(dataType)
                                });
                                break;
                            case 'number_decimal':
                                fields.push({
                                    icon: "k-i-decimal-decrease",
                                    field: that.newField(dataType)
                                });
                                break;
                            case 'currency':
                                fields.push({
                                    icon: "k-i-dollar",
                                    field: that.newField(dataType)
                                });

                                break;
                            case 'location':
                                fields.push({
                                    icon: "k-i-marker-pin-target",
                                    field: that.newField(dataType)
                                });
                                break;
                            case 'date':
                                fields.push({
                                    icon: "k-icon k-i-calendar",
                                    field: that.newField(dataType)
                                });
                                break;
                            case 'date_time':
                                fields.push({
                                    icon: "k-icon k-i-calendar-date",
                                    field: that.newField(dataType)
                                });
                                break;
                            case 'time':
                                fields.push({
                                    icon: "k-icon k-i-clock",
                                    field: that.newField(dataType)
                                });
                                break;
                            case 'email':
                                fields.push({
                                    icon: "k-icon k-i-email",
                                    field: that.newField(dataType)
                                });
                                break;
                            case 'picklist':
                                fields.push({
                                    icon: "k-i-list-ordered",
                                    field: that.newField(dataType)
                                });
                                break;
                            case 'lookup':
                                fields.push({
                                    icon: "k-icon k-i-search",
                                    field: that.newField(dataType)
                                });
                                break;
                            case 'checkbox':
                                fields.push({
                                    icon: "k-icon k-i-checkbox-checked",
                                    field: that.newField(dataType)
                                });
                                break;
                            case 'document':
                                fields.push({
                                    icon: "k-icon k-i-file",
                                    field: that.newField(dataType)
                                });
                                break;
                            case 'url':
                                fields.push({
                                    icon: "k-icon k-i-link-horizontal",
                                    field: that.newField(dataType)
                                });
                                break;
                            case 'image':
                                fields.push({
                                    icon: "k-icon k-i-image",
                                    field: that.newField(dataType)
                                });
                                break;
                            case 'rating':
                                fields.push({
                                    icon: "k-icon k-i-star-outline",
                                    field: that.newField(dataType)
                                });
                                break;
                            case 'combination':
                                fields.push({
                                    icon: "k-icon k-i-hyperlink-open-sm",
                                    field: that.newField(dataType)
                                });
                                break;
                            case 'multiselect':
                                fields.push({
                                    icon: "k-i-select-box",
                                    field: that.newField(dataType)
                                });
                                break;
                            case 'tag':
                                fields.push({
                                    icon: "k-i-textbox-hidden",
                                    field: that.newField(dataType)
                                });
                                break;

                        }

                    });
                    return fields;

                },
                getDataTypes: function () {
                    $rootScope.dataTypesExtended = angular.copy(dataTypes);

                    var combinationDataType = {};
                    combinationDataType.name = 'combination';
                    combinationDataType.label = {};
                    combinationDataType.label.en = defaultLabels.DataTypeCombinationEn;
                    combinationDataType.label.tr = defaultLabels.DataTypeCombinationTr;
                    combinationDataType.order = 14;

                    $rootScope.dataTypesExtended.combination = combinationDataType;
                    var dataTypeList = [];

                    angular.forEach($rootScope.dataTypesExtended, function (dataType) {
                        switch (dataType.name) {
                            case 'text_single':
                            case 'map':
                            case 'location':
                            case 'rating':
                            case 'email':
                                dataType.maxLength = 3;
                                break;
                            case 'text_multi':
                                dataType.maxLength = 4;
                                break;
                            case 'number':
                            case 'number_decimal':
                            case 'currency':
                                dataType.maxLength = 2;
                                break;
                        }

                        switch (dataType.name) {
                            case 'text_single':
                                dataType.max = 400;
                                break;
                            case 'email':
                                dataType.max = 100;
                                break;
                            case 'text_multi':
                                dataType.max = 2000;
                                break;
                            case 'number':
                            case 'number_decimal':
                            case 'currency':
                                dataType.max = 27;
                                break;
                            case 'rating':
                                break;
                            case 'tag':
                                break;
                            case 'json':
                                break;
                        }

                        dataTypeList.push(dataType);
                    });

                    return dataTypeList;
                },
                getPicklist: function (id) {
                    if (id >= 900000) {
                        var deffered = $q.defer();
                        deffered.resolve({ data: { items: [] } });
                        return deffered.promise;
                    }

                    return $http.get(config.apiUrl + 'picklist/get/' + id);
                },
                formatFieldValue: function (field, value, picklists, record, module) {
                    field.valueFormatted = '';

                    if (value === undefined || value === null)
                        return;

                    switch (field.data_type) {
                        case 'number_decimal':
                            field.valueFormatted = $filter('number')(value, field.decimal_places || 2);
                            break;
                        case 'number_auto':
                            var valueFormatted = value.toString();

                            if (field.auto_number_prefix)
                                valueFormatted = field.auto_number_prefix + valueFormatted;

                            if (field.auto_number_suffix)
                                valueFormatted += field.auto_number_suffix;

                            field.valueFormatted = valueFormatted;
                            break;
                        case 'currency':
                            var recordCurrencySymbol;

                            if (record && record['currency']) {
                                if (angular.isObject(record['currency'])) {
                                    recordCurrencySymbol = record['currency'].value;
                                } else {
                                    var currencyField = $filter('filter')(module.fields, { name: 'currency' }, true)[0];
                                    var currencyPicklistItem = $filter('filter')(picklists[currencyField.picklist_id], { labelStr: record['currency'] })[0];

                                    if (currencyPicklistItem && currencyPicklistItem.value)
                                        recordCurrencySymbol = currencyPicklistItem.value;
                                }
                            }

                            field.valueFormatted = $filter('currency')(value, recordCurrencySymbol || field.currency_symbol || $rootScope.currencySymbol, field.decimal_places || 2);
                            break;
                        case 'date':
                            field.valueFormatted = $filter('date')(value, 'shortDate');
                            break;
                        case 'date_time':
                            field.valueFormatted = $filter('date')(value, 'short');
                            break;
                        case 'time':
                            field.valueFormatted = $filter('date')(value, 'shortTime');
                            break;
                        case 'picklist':
                            if (!angular.isObject(value)) {
                                var picklistItem = $filter('filter')(picklists[field.picklist_id], { labelStr: value }, true)[0];
                                field.valueFormatted = picklistItem ? picklistItem.label[$rootScope.language] : value;
                            } else {
                                field.valueFormatted = value.label[$rootScope.language];
                            }
                            break;
                        case 'multiselect':
                            for (var i = 0; i < value.length; i++) {
                                var item = value[i];

                                if (!angular.isObject(item)) {
                                    var picklistItem = $filter('filter')(picklists[field.picklist_id], { labelStr: item }, true)[0];
                                    field.valueFormatted += (picklistItem ? picklistItem.label[$rootScope.language] : item) + '; ';
                                } else {
                                    field.valueFormatted += (item.label ? item.label[$rootScope.language] : '') + '; ';
                                }
                            }

                            field.valueFormatted = field.valueFormatted.slice(0, -2);
                            break;
                        case 'tag':
                            for (var i = 0; i < value.length; i++) {
                                var item = value[i];

                                if (!angular.isObject(item)) {

                                    field.valueFormatted += " " + item + ";";
                                } else {
                                    field.valueFormatted += " " + item;
                                }
                            }

                            // field.valueFormatted = field.valueFormatted.slice(0, -2);
                            break;
                        case 'checkbox':
                            field.valueFormatted = $filter('filter')(picklists['yes_no'], { system_code: value.toString() })[0].label[$rootScope.language];
                            break;
                        default:
                            field.valueFormatted = value;
                            break;
                    }
                },
                moduleUpdate: function (module, id) {
                    return $http.put(config.apiUrl + 'module/update/' + id, module);
                },
                //moduleProfileSettings
                getAllModuleProfileSettings: function () {
                    return $http.get(config.apiUrl + 'module_profile_settings/get_all');
                },

                createModuleProfileSetting: function (moduleProfileSetting) {
                    return $http.post(config.apiUrl + 'module_profile_settings/create', moduleProfileSetting);
                },

                updateModuleProfileSetting: function (id, moduleProfileSetting) {
                    return $http.put(config.apiUrl + 'module_profile_settings/update/' + id, moduleProfileSetting);
                },

                deleteModuleProfileSetting: function (id) {
                    return $http.delete(config.apiUrl + 'module_profile_settings/delete/' + id);
                },

                getPicklists: function () {
                    return $http.get(config.apiUrl + 'picklist/get_all');
                },

                getPickItemsLists: function (module, withRelatedPicklists) {
                    if (!module.fields)
                        return {};

                    var deferred = $q.defer();
                    var fields = angular.copy(module.fields);
                    var picklists = {};
                    var picklistIds = [];

                    /*if(module.name === 'izinler'){
                     var leave_entry_type = [
                     { label:{ en:"Morning", tr:"Sabah" }, order:1, system_code:"true", type:2 },
                     { label:{ en:"Afternoon", tr:"Öğleden Sonra" }, order:2, system_code:"true", type:2 }
                     ]
                     picklists["leave_entry_type"] = leave_entry_type;
                     }*/

                    if (withRelatedPicklists) {
                        for (var i = 0; i < module.fields.length; i++) {
                            var field = module.fields[i];

                            if (field.data_type === 'lookup' && field.lookup_type !== 'users' && field.lookup_type !== 'profiles' && field.lookup_type !== 'roles' && field.lookup_type !== 'relation') {
                                var lookupModule = $filter('filter')($scope.modules, { name: field.lookup_type }, true)[0];

                                if (!lookupModule)
                                    continue;

                                for (var j = 0; j < lookupModule.fields.length; j++) {
                                    var lookupModuleField = lookupModule.fields[j];

                                    if (lookupModuleField.data_type === 'picklist' || lookupModuleField.data_type === 'multiselect')
                                        fields.push(lookupModuleField);
                                }
                            }
                        }
                    }

                    var setDependency = function (picklist, field) {
                        if (module.dependencies && module.dependencies.length > 0) {
                            var dependency = $filter('filter')(module.dependencies, { child_field: field.name }, true)[0];

                            if (dependency && dependency.deleted !== true && dependency.dependency_type === 'list_field') {
                                for (var i = 0; i < picklist.length; i++) {
                                    var picklistItem = picklist[i];
                                    picklistItem.hidden = true;
                                }
                            }
                        }
                    };

                    for (var k = 0; k < fields.length; k++) {
                        var fieldItem = fields[k];

                        if (fieldItem.picklist_id) {
                            var picklistCache = $cache.get('picklist_' + fieldItem.picklist_id);

                            if (fieldItem.picklist_id === 900000) {
                                if (picklistCache) {
                                    picklists[fieldItem.picklist_id] = picklistCache;
                                    continue;
                                }

                                var modulePicklist = [];

                                for (var l = 0; l < $scope.modules.length; l++) {
                                    var moduleItem = $scope.modules[l];

                                    if (!moduleItem.display || moduleItem.name === 'activities')
                                        continue;

                                    if (!helper.hasPermission(moduleItem.name, operations.read))
                                        continue;

                                    var modulePicklistItem = {};
                                    modulePicklistItem.id = parseInt(moduleItem.id) + 900000;
                                    modulePicklistItem.type = 900000;
                                    modulePicklistItem.system_code = moduleItem.name;
                                    modulePicklistItem.order = moduleItem.order;
                                    modulePicklistItem.label = {};
                                    modulePicklistItem.label.en = moduleItem.label_en_singular;
                                    modulePicklistItem.label.tr = moduleItem.label_tr_singular;
                                    modulePicklistItem.labelStr = moduleItem['label_' + $rootScope.Language + '_singular'];
                                    modulePicklistItem.value = moduleItem.name;

                                    modulePicklist.push(modulePicklistItem);
                                }

                                modulePicklist = $filter('orderBy')(modulePicklist, 'order');
                                picklists['900000'] = modulePicklist;
                                $cache.put('picklist_' + 900000, modulePicklist);

                                continue;
                            }

                            if (!picklistCache) {
                                picklistIds.push(fieldItem.picklist_id);
                            } else {
                                picklistCache = $filter('orderByLabel')(picklistCache, $rootScope.language);

                                if (fieldItem.picklist_sortorder && !fieldItem.deleted)
                                    picklistCache = $filter('orderBy')(picklistCache, fieldItem.picklist_sortorder);

                                setDependency(picklistCache, fieldItem);
                                picklists[fieldItem.picklist_id] = picklistCache;
                            }
                        }
                    }

                    //Picklist for all modules (activity_type, yes_no)
                    var activityTypePicklistCache = $cache.get('picklist_activity_type');

                    if (activityTypePicklistCache)
                        picklists['activity_type'] = activityTypePicklistCache;
                    else
                        picklists['activity_type'] = activityTypes;

                    var transactionTypePicklistCache = $cache.get('picklist_transaction_type');

                    if (transactionTypePicklistCache)
                        picklists['transaction_type'] = transactionTypePicklistCache;
                    else {
                        if (module.name === 'accounts') {
                            picklists['transaction_type'] = $filter('filter')(transactionTypes, { type: 1 }, true);
                        } else if (module.name === 'suppliers') {
                            picklists['transaction_type'] = $filter('filter')(transactionTypes, { type: 2 }, true);
                        }
                    }

                    var yesNoPicklistCache = $cache.get('picklist_yes_no');

                    if (yesNoPicklistCache)
                        picklists['yes_no'] = yesNoPicklistCache;
                    else
                        picklists['yes_no'] = yesNo;

                    //All picklists in cache. Return them.
                    if (picklistIds.length <= 0) {
                        deferred.resolve(picklists);
                        return deferred.promise;
                    }

                    picklistIds = picklistIds.getUnique();

                    this.findPicklist(picklistIds)
                        .then(function (response) {
                            if (!response.data) {
                                deferred.resolve(picklists);
                                return deferred.promise;
                            }

                            for (var i = 0; i < fields.length; i++) {
                                var field = fields[i];

                                if (!field.picklist_id)
                                    continue;

                                if (picklistIds.indexOf(field.picklist_id) < 0)
                                    continue;

                                var picklistItems = helper.mergePicklists(response.data);
                                picklists[field.picklist_id] = $filter('filter')(picklistItems, { type: field.picklist_id }, true);
                                picklists[field.picklist_id] = $filter('orderByLabel')(picklists[field.picklist_id], $rootScope.language);

                                if (field.picklist_sortorder && !field.deleted)
                                    picklists[field.picklist_id] = $filter('orderBy')(picklists[field.picklist_id], field.picklist_sortorder);

                                if (module.dependencies && module.dependencies.length > 0) {
                                    var dependency = $filter('filter')(module.dependencies, { child_field: field.name }, true)[0];

                                    if (dependency && dependency.deleted !== true && dependency.dependency_type === 'list_field') {
                                        for (var j = 0; j < picklists[field.picklist_id].length; j++) {
                                            var picklistItem = picklists[field.picklist_id][j];
                                            picklistItem.hidden = true;
                                        }
                                    }
                                }

                                setDependency(picklists[field.picklist_id], field);
                                $cache.put('picklist_' + field.picklist_id, picklists[field.picklist_id]);
                            }

                            deferred.resolve(picklists);
                        })
                        .catch(function (reason) {
                            deferred.reject(reason.data);
                        });

                    return deferred.promise;
                },

                createPicklist: function (picklist) {
                    return $http.post(config.apiUrl + 'picklist/create', picklist);
                },

                updatePicklist: function (picklist) {
                    return $http.put(config.apiUrl + 'picklist/update/' + picklist.id, picklist);
                },

                findPicklist: function (ids) {
                    return $http.post(config.apiUrl + 'picklist/find', ids);
                },

                processPicklist: function (picklist) {
                    picklist.label = picklist['label_' + $rootScope.language];

                    angular.forEach(picklist.items, function (item) {
                        item.label = item['label_' + $rootScope.language];
                        item.track = parseInt(item.id + '00000');
                    });

                    picklist.items = $filter('orderBy')(picklist.items, 'order');

                    return picklist;
                },

                preparePicklist: function (picklist) {
                    picklist.label_en = picklist.label;
                    picklist.label_tr = picklist.label;

                    angular.forEach(picklist.items, function (item) {
                        item.label_en = item.label;
                        item.label_tr = item.label;
                    });

                    return picklist;
                },
                processFields: function (fields) {
                    for (var l = 0; l < fields.length; l++) {
                        var field = fields[l];
                        field.label = field['label_' + $rootScope.language];
                        field.dataType = dataTypes[field.data_type];
                        field.operators = [];

                        if (field.data_type === 'lookup') {
                            if (field.lookup_type != 'users' && field.lookup_type != 'profiles' && field.lookup_type != 'roles' && field.lookup_type != 'relation') {

                            } else {

                                field.operators.push(operators.equals);
                                field.operators.push(operators.not_equal);
                                field.operators.push(operators.empty);
                                field.operators.push(operators.not_empty);


                            }

                        } else {
                            for (var n = 0; n < field.dataType.operators.length; n++) {
                                var operatorId = field.dataType.operators[n];
                                var operator = operators[operatorId];
                                field.operators.push(operator);
                            }
                        }


                    }

                    return fields;
                },
                processModule2: function (module, modules) {
                    var that = this;
                    if (!modules)
                        modules = $rootScope.appModules;

                    for (var i = 0; i < module.sections.length; i++) {
                        var section = module.sections[i];
                        section.columns = [];
                        var sectionPermissions = [];

                        if (section.permissions)
                            sectionPermissions = angular.copy(section.permissions);

                        section.permissions = [];

                        for (var j = 0; j < $rootScope.appProfiles.length; j++) {
                            var profile = $rootScope.appProfiles[j];

                            if (profile.is_persistent && profile.has_admin_rights)
                                profile.name = $filter('translate')('Setup.Profiles.Administrator');

                            if (profile.is_persistent && !profile.has_admin_rights)
                                profile.name = $filter('translate')('Setup.Profiles.Standard');

                            var sectionPermission = $filter('filter')(sectionPermissions, { profile_id: profile.id }, true)[0];

                            if (!sectionPermission) {
                                section.permissions.push({
                                    profile_id: profile.id,
                                    profile_name: profile.name,
                                    profile_is_admin: profile.has_admin_rights,
                                    type: 'full'
                                });
                            } else {
                                section.permissions.push({
                                    id: sectionPermission.id,
                                    profile_id: profile.id,
                                    profile_name: profile.name,
                                    profile_is_admin: profile.has_admin_rights,
                                    type: sectionPermission.type
                                });
                            }
                        }

                        for (var k = 1; k <= section.column_count; k++) {
                            var column = {};
                            column.no = k;

                            section.columns.push(column);
                        }
                    }

                    for (var l = 0; l < module.fields.length; l++) {
                        var field = module.fields[l];
                        field.label = field['label_' + $rootScope.language];
                        field.dataType = dataTypes[field.data_type];
                        field.operators = [];
                        field.sectionObj = $filter('filter')(module.sections, { name: field.section }, true)[0];

                        if (field.data_type === 'lookup') {
                            if (field.lookup_type != 'users' && field.lookup_type != 'profiles' && field.lookup_type != 'roles' && field.lookup_type != 'relation') {
                                var lookupModule = $filter('filter')(modules, { name: field.lookup_type }, true)[0];


                                if (!lookupModule)
                                    continue;

                                that.getModuleFields(lookupModule.name).then(function (lookupModuleFields) {

                                    var lookupModuleFields = lookupModuleFields.data;
                                    field.lookupModulePrimaryField = $filter('filter')(lookupModuleFields, { primary_lookup: true }, true)[0];

                                    if (!field.lookupModulePrimaryField)
                                        field.lookupModulePrimaryField = $filter('filter')(lookupModuleFields, { primary: true }, true)[0];

                                    var lookupModulePrimaryFieldDataType = dataTypes[field.lookupModulePrimaryField.data_type];

                                    for (var m = 0; m < lookupModulePrimaryFieldDataType.operators.length; m++) {
                                        var operatorIdLookup = lookupModulePrimaryFieldDataType.operators[m];
                                        var operatorLookup = operators[operatorIdLookup];
                                        field.operators.push(operatorLookup);
                                    }

                                });
                            } else {
                                field.operators.push(operators.equals);
                                field.operators.push(operators.not_equal);
                                field.operators.push(operators.empty);
                                field.operators.push(operators.not_empty);

                                if (field.lookup_type === 'users') {
                                    var lookupModule = $filter('filter')(modules, { name: 'users' }, true)[0];
                                    if (lookupModule) {
                                        field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];

                                    }
                                } else if (field.lookup_type === 'profiles') {
                                    var lookupModule = $filter('filter')(modules, { name: 'profiles' }, true)[0];
                                    if (lookupModule) {
                                        field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];

                                    }
                                } else if (field.lookup_type === 'roles') {
                                    var lookupModule = $filter('filter')(modules, { name: 'roles' }, true)[0];
                                    if (lookupModule) {
                                        field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];
                                    }
                                }
                            }

                        } else {
                            for (var n = 0; n < field.dataType.operators.length; n++) {
                                var operatorId = field.dataType.operators[n];
                                var operator = operators[operatorId];
                                field.operators.push(operator);
                            }
                        }

                        if (field.name === 'related_module') {
                            field.picklist_original_id = angular.copy(field.picklist_id);
                            field.picklist_id = 900000;
                        }
                        var systemRequiredFields = angular.copy(this.getSystemRequiredFields());

                        var systemReadonlyFields = angular.copy(this.getSystemReadonlyFields());

                        if (systemRequiredFields.all.indexOf(field.name) > -1 || (systemRequiredFields[module.name] && systemRequiredFields[module.name].indexOf(field.name) > -1))
                            field['systemRequired'] = true;

                        if (systemReadonlyFields.all.indexOf(field.name) > -1 || (systemReadonlyFields[module.name] && systemReadonlyFields[module.name].indexOf(field.name) > -1))
                            field['systemRequired'] = true;

                        var fieldPermissions = [];

                        if (field.permissions)
                            fieldPermissions = angular.copy(field.permissions);

                        field.permissions = [];

                        for (var o = 0; o < $rootScope.appProfiles.length; o++) {
                            var profileItem = $rootScope.appProfiles[o];

                            if (profileItem.is_persistent && profileItem.has_admin_rights)
                                profileItem.name = $filter('translate')('Setup.Profiles.Administrator');

                            if (profileItem.is_persistent && !profileItem.has_admin_rights)
                                profileItem.name = $filter('translate')('Setup.Profiles.Standard');

                            var fieldPermission = $filter('filter')(fieldPermissions, { profile_id: profileItem.id }, true)[0];

                            if (!fieldPermission)
                                field.permissions.push({
                                    profile_id: profileItem.id,
                                    profile_name: profileItem.name,
                                    profile_is_admin: profileItem.has_admin_rights,
                                    type: 'full'
                                });
                            else
                                field.permissions.push({
                                    id: fieldPermission.id,
                                    profile_id: profileItem.id,
                                    profile_name: profileItem.name,
                                    profile_is_admin: profileItem.has_admin_rights,
                                    type: fieldPermission.type
                                });
                        }

                    }


                    if (module.name === 'activities')
                        module.display_calendar = true;

                    return module;
                },

                processModule: function (module) {
                    angular.forEach(module.fields, function (field) {
                        if (field.combination) {
                            field.data_type = 'combination';
                            field.dataType = $rootScope.dataTypesExtended.combination;
                            field.combinationField1 = field.combination.field1;
                            field.combinationField2 = field.combination.field2;
                            field.combinationCharacter = field.combination.combination_character;
                        }
                    });

                    return module;
                },

                processRecordField: function (record, field, picklists) {
                    var that = this;
                    var fieldName = field.name;

                    switch (field.data_type) {
                        case 'lookup':
                            var lookupRecord = {};
                            var lookupIsNull = record[fieldName + '.id'] === null;

                            if (record[fieldName + '.id'] === undefined) {
                                field.inline_edit = false;
                                lookupIsNull = true;
                            }

                            for (var key in record) {
                                if (record.hasOwnProperty(key)) {
                                    var value = record[key];

                                    if (key.startsWith(fieldName + ".")) {
                                        if (!lookupIsNull) {
                                            var keyParts = key.split('.');

                                            if (keyParts[0] !== field.name)
                                                continue;

                                            lookupRecord[keyParts[1]] = value;
                                        }

                                        delete record[key];
                                    }
                                }
                            }

                            if (lookupIsNull)
                                return;

                            if (field.lookup_type === 'users') {
                                lookupRecord.primary_value = lookupRecord['full_name'];
                            } else if (field.lookup_type === 'profiles') {
                                lookupRecord.primary_value = lookupRecord['name'];
                            } else if (field.lookup_type === 'roles') {
                                lookupRecord.primary_value = lookupRecord['label_' + $rootScope.user.tenant_language];
                            } else if (field.lookup_type === 'relation') {
                                if (record[field.lookup_relation] && record['related_to'])
                                    lookupRecord = record['related_to'];
                                else
                                    lookupRecord = null;
                            } else {
                                var lookupModule = $filter('filter')($rootScope.appModules, { name: field.lookup_type }, true)[0];
                                var lookupModulePrimaryField = $filter('filter')(lookupModule.fields, {
                                    primary: true,
                                    deleted: false
                                }, true)[0];
                                lookupRecord.primary_value = lookupRecord[lookupModulePrimaryField.name];
                            }

                            record[fieldName] = lookupRecord;
                            break;
                        case 'picklist':
                            var picklistItem = $filter('filter')(picklists[field.picklist_id], {
                                labelStr: record[fieldName],
                                inactive: '!true'
                            }, true)[0];
                            record[fieldName] = picklistItem;
                            break;
                        case 'multiselect':
                            var picklistItems = [];

                            if (record[fieldName] && record[fieldName].length > 0) {
                                for (var i = 0; i < record[fieldName].length; i++) {
                                    var multiselectItem = record[fieldName][i];

                                    picklistItem = $filter('filter')(picklists[field.picklist_id], {
                                        labelStr: multiselectItem,
                                        inactive: '!true'
                                    }, true)[0];
                                    //Check if item name exist in picklist ( Item name can be change )
                                    if (picklistItem)
                                        picklistItems.push(picklistItem);
                                }
                            }

                            record[fieldName] = picklistItems;
                            break;
                        case 'date':
                        case 'date_time':
                        case 'time':
                            if (record[fieldName] === undefined || record[fieldName] === null || !record[fieldName].length)
                                return;

                            record[fieldName] = new Date(record[fieldName]);
                            break;
                    }
                },

                prepareDefaults: function (module) {
                    module.system_type = 'custom';
                    module.label_en_plural = defaultLabels.DefaultModuleNameEn + "s";
                    module.label_en_singular = defaultLabels.DefaultModuleNameEn;
                    module.label_tr_plural = defaultLabels.DefaultModuleNameTr;
                    module.label_tr_singular = defaultLabels.DefaultModuleNameTr;
                    module.display = true;
                    module.sharing = 'private';
                    module.deleted = false;

                    var sortOrders = [];

                    angular.forEach($rootScope.appModules, function (moduleItem) {
                        sortOrders.push(moduleItem.order);
                    });

                    if ($rootScope.appModules.length < 1)
                        module.order = 1;
                    else
                        module.order = Math.max.apply(null, sortOrders) + 1;

                    module.name = 'custom_module_' + 9999;
                    module.sections = [];
                    module.fields = [];

                    var section1 = {};
                    section1.system_type = 'custom';
                    section1.column_count = 2;
                    section1.name = 'custom_section1';
                    section1.label_en = defaultLabels.DefaultSectionNameEn;
                    section1.label_tr = defaultLabels.DefaultSectionNameTr;
                    section1.order = 1;
                    section1.display_form = true;
                    section1.display_detail = true;
                    section1.deleted = false;
                    section1.columns = [];
                    section1.columns.push({ no: 1 });
                    section1.columns.push({ no: 2 });

                    module.sections.push(section1);

                    var section2 = {};
                    section2.system_type = 'custom';
                    section2.column_count = 2;
                    section2.name = 'custom_section2';
                    section2.label_en = defaultLabels.SystemInfoSectionNameEn;
                    section2.label_tr = defaultLabels.SystemInfoSectionNameTr;
                    section2.order = 2;
                    section2.display_form = false;
                    section2.display_detail = true;
                    section2.deleted = false;
                    section2.columns = [];
                    section2.columns.push({ no: 1 });
                    section2.columns.push({ no: 2 });

                    module.sections.push(section2);

                    var field1 = {};
                    field1.system_type = 'system';
                    field1.data_type = 'lookup';
                    field1.dataType = $rootScope.dataTypesExtended.lookup;
                    field1.section = 'custom_section2';
                    field1.section_column = 1;
                    field1.name = 'owner';
                    field1.label_en = 'Owner';
                    field1.label_tr = 'Kayıt Sahibi';
                    field1.order = 1;
                    field1.primary = false;
                    field1.display_form = true;
                    field1.display_detail = true;
                    field1.display_list = true;
                    field1.validation = {};
                    field1.validation.required = true;
                    field1.validation.readonly = false;
                    field1.inline_edit = true;
                    field1.editable = true;
                    field1.show_label = true;
                    field1.lookup_type = 'users';
                    field1.deleted = false;
                    field1.systemReadonly = true;
                    field1.systemRequired = true;

                    module.fields.push(field1);

                    var field2 = {};
                    field2.system_type = 'custom';
                    field2.data_type = 'text_single';
                    field2.dataType = $rootScope.dataTypesExtended.text_single;
                    field2.section = 'custom_section1';
                    field2.section_column = 1;
                    field2.name = 'custom_field2';
                    field2.label_en = defaultLabels.DefaultFieldNameEn;
                    field2.label_tr = defaultLabels.DefaultFieldNameTr;
                    field2.order = 2;
                    field2.primary = true;
                    field2.display_form = true;
                    field2.display_detail = true;
                    field2.display_list = true;
                    field2.validation = {};
                    field2.validation.required = true;
                    field2.validation.readonly = false;
                    field2.inline_edit = true;
                    field2.editable = true;
                    field2.show_label = true;
                    field2.deleted = false;

                    module.fields.push(field2);

                    var field3 = {};
                    field3.system_type = 'system';
                    field3.data_type = 'lookup';
                    field3.dataType = $rootScope.dataTypesExtended.lookup;
                    field3.section = 'custom_section2';
                    field3.section_column = 1;
                    field3.name = 'created_by';
                    field3.label_en = defaultLabels.CreatedByFieldEn;
                    field3.label_tr = defaultLabels.CreatedByFieldTr;
                    field3.order = 3;
                    field3.primary = false;
                    field3.display_form = false;
                    field3.display_detail = true;
                    field3.display_list = true;
                    field3.validation = {};
                    field3.validation.required = true;
                    field3.validation.readonly = true;
                    field3.inline_edit = false;
                    field3.editable = true;
                    field3.show_label = true;
                    field3.lookup_type = 'users';
                    field3.deleted = false;
                    field3.systemReadonly = true;
                    field3.systemRequired = true;

                    module.fields.push(field3);

                    var field4 = {};
                    field4.system_type = 'system';
                    field4.data_type = 'date_time';
                    field4.dataType = $rootScope.dataTypesExtended.date_time;
                    field4.section = 'custom_section2';
                    field4.section_column = 1;
                    field4.name = 'created_at';
                    field4.label_en = defaultLabels.CreatedAtFieldEn;
                    field4.label_tr = defaultLabels.CreatedAtFieldTr;
                    field4.order = 4;
                    field4.primary = false;
                    field4.display_form = false;
                    field4.display_detail = true;
                    field4.display_list = true;
                    field4.validation = {};
                    field4.validation.required = true;
                    field4.validation.readonly = true;
                    field4.inline_edit = false;
                    field4.editable = true;
                    field4.show_label = true;
                    field4.deleted = false;
                    field4.systemReadonly = true;
                    field4.systemRequired = true;

                    module.fields.push(field4);

                    var field5 = {};
                    field5.system_type = 'system';
                    field5.data_type = 'lookup';
                    field5.dataType = $rootScope.dataTypesExtended.lookup;
                    field5.section = 'custom_section2';
                    field5.section_column = 2;
                    field5.name = 'updated_by';
                    field5.label_en = defaultLabels.UpdatedByFieldEn;
                    field5.label_tr = defaultLabels.UpdatedByFieldTr;
                    field5.order = 5;
                    field5.primary = false;
                    field5.display_form = false;
                    field5.display_detail = true;
                    field5.display_list = true;
                    field5.validation = {};
                    field5.validation.required = true;
                    field5.validation.readonly = true;
                    field5.inline_edit = false;
                    field5.editable = true;
                    field5.show_label = true;
                    field5.lookup_type = 'users';
                    field5.deleted = false;
                    field5.systemReadonly = true;
                    field5.systemRequired = true;

                    module.fields.push(field5);

                    var field6 = {};
                    field6.system_type = 'system';
                    field6.data_type = 'date_time';
                    field6.dataType = $rootScope.dataTypesExtended.date_time;
                    field6.section = 'custom_section2';
                    field6.section_column = 2;
                    field6.name = 'updated_at';
                    field6.label_en = defaultLabels.UpdatedAtFieldEn;
                    field6.label_tr = defaultLabels.UpdatedAtFieldTr;
                    field6.order = 6;
                    field6.primary = false;
                    field6.display_form = false;
                    field6.display_detail = true;
                    field6.display_list = true;
                    field6.validation = {};
                    field6.validation.required = true;
                    field6.validation.readonly = true;
                    field6.inline_edit = false;
                    field6.editable = true;
                    field6.show_label = true;
                    field6.deleted = false;
                    field6.systemReadonly = true;
                    field6.systemRequired = true;

                    module.fields.push(field6);

                    return module;
                },

                getModuleLayout: function (module) {
                    var moduleLayout = {};
                    moduleLayout.rows = [];

                    module.sections = $filter('orderBy')(module.sections, 'order');
                    module.fields = $filter('orderBy')(module.fields, 'order');

                    angular.forEach(module.sections, function (section) {
                        if (section.deleted)
                            return;

                        var row = {};
                        row.section = section;
                        row.order = section.order;
                        row.columns = [];

                        section.columns = $filter('orderBy')(section.columns, 'order');

                        angular.forEach(section.columns, function (columItem) {
                            var column = {};
                            column.column = columItem;
                            column.cells = [];

                            angular.forEach(module.fields, function (field) {
                                if (field.section !== section.name || field.section_column !== columItem.no || field.deleted)
                                    return;

                                var cell = {};
                                cell.field = field;
                                cell.order = field.order;

                                column.cells.push(cell);

                                if (field.primary)
                                    row.hasPrimaryField = true;

                                if (field.systemRequired)
                                    row.hasSystemRequiredField = true;
                            });

                            row.columns.push(column);
                        });

                        moduleLayout.rows.push(row);
                    });

                    return moduleLayout;
                },

                refreshModule: function (moduleLayout, module) {
                    var rows = moduleLayout.rows;
                    var sections = [];
                    var fields = [];
                    var lastFieldOrder = 0;

                    var showField = {
                        currentField: null,
                        currentColumn: null,
                        currentRow: null
                    };


                    for (var i = 0; i < rows.length; i++) {
                        var row = rows[i];
                        var section = row.section;
                        section.order = i + 1;

                        sections.push(section);

                        delete row.hasPrimaryField;
                        delete row.hasSystemRequiredField;

                        for (var j = 0; j < row.columns.length; j++) {
                            var column = row.columns[j];

                            for (var k = 0; k < column.cells.length; k++) {
                                var cell = column.cells[k];
                                var field = cell.field;
                                field.section = section.name;
                                if (section.column_count == 2)
                                    field.section_column = column.column.no;
                                else
                                    field.section_column = section.column_count;
                                field.order = lastFieldOrder + 1;
                                if (field.firstDrag === true) {

                                    showField.currentField = field;
                                    showField.currentRow = row;
                                    showField.currentColumn = column;
                                    //  field.firstDrag = false;
                                    field.label_tr = "";
                                    field.label_en = "";
                                }

                                lastFieldOrder = field.order;
                                fields.push(field);

                                if (field.primary)
                                    row.hasPrimaryField = true;

                                if (field.systemRequired)
                                    row.hasSystemRequiredField = true;
                            }
                        }
                    }

                    module.sections = sections;
                    module.fields = fields;

                    return showField;
                },

                prepareModule: function (module, picklistsModule, deletedModules, pureModule) {
                    var otherLanguage = $rootScope.language === 'en' ? 'tr' : 'en';

                    if (module.name.indexOf('custom_module') > -1) {
                        module['label_' + otherLanguage + '_plural'] = module['label_' + $rootScope.language + '_plural'];
                        module['label_' + otherLanguage + '_singular'] = module['label_' + $rootScope.language + '_singular'];
                        module.name = helper.getSlug(module['label_' + $rootScope.language + '_plural']);

                        var allModules = $rootScope.appModules.concat(deletedModules);

                        var i = 2;
                        while (true) {
                            //parseInt(module.name.charAt(0)) first index is starts_with numeric, we added 'n'
                            var findMatch = module.name.match(/(\D+)?\d/);
                            var index = findMatch ? findMatch[0].length - 1 : -1;
                            var newModuleName = index === 0 ? 'n' + module.name : module.name; // if first index value === 0, its starts_with number
                            var existingModule = $filter('filter')(allModules, { name: newModuleName }, true)[0];

                            if (!existingModule)
                                break;

                            if (i < 20) {
                                module.name = helper.getSlug(module['label_' + $rootScope.language + '_plural']) + i;
                                i++;
                            } else {
                                var dt = new Date();
                                module.name = helper.getSlug(module['label_' + $rootScope.language + '_plural']) + dt.getTime();
                            }
                        }
                    }

                    module['label_' + otherLanguage + '_plural'] = module['label_' + $rootScope.language + '_plural'];
                    module['label_' + otherLanguage + '_singular'] = module['label_' + $rootScope.language + '_singular'];

                    angular.forEach(module.sections, function (section) {
                        delete section.columns;

                        if (section.name.indexOf('custom_section') > -1) {
                            var newSectionName = helper.getSlug(section['label_' + $rootScope.language]);

                            var sectionFields = $filter('filter')(module.fields, { section: section.name }, true);

                            angular.forEach(sectionFields, function (sectionField) {
                                sectionField.section = newSectionName;
                            });

                            var cleanSlug = angular.copy(newSectionName);
                            var existingSection = $filter('filter')(module.sections, { name: cleanSlug }, true)[0];

                            if (existingSection) {
                                do {
                                    var sectionNameNumber;

                                    if (existingSection.name.indexOf('_') > -1) {
                                        var slugParts = existingSection.name.split('_');
                                        var lastPart = slugParts[slugParts.length - 1];
                                        sectionNameNumber = lastPart.replace(/\D/g, '');
                                        lastPart = lastPart.replace(/[0-9]/g, '');
                                        slugParts.pop();
                                        cleanSlug = slugParts.join('_') + '_' + lastPart;
                                    } else {
                                        sectionNameNumber = existingSection.name.replace(/\D/g, '');
                                        cleanSlug = existingSection.name.replace(/[0-9]/g, '');
                                    }

                                    var newSlug = '';

                                    if (sectionNameNumber)
                                        newSlug = cleanSlug + (parseInt(sectionNameNumber) + 1);
                                    else
                                        newSlug = cleanSlug + 2;

                                    existingSection = $filter('filter')(module.sections, { name: newSlug }, true)[0];

                                    if (!existingSection)
                                        section.name = newSlug;
                                }
                                while (existingSection)
                            } else
                                section.name = newSectionName;
                        }
                        //permissions
                        var sectionPermissions = angular.copy(section.permissions);
                        var permissions = [];

                        angular.forEach(sectionPermissions, function (permission) {
                            if (permission.id || permission.type !== 'full')
                                permissions.push(permission);
                        });

                        if (permissions.length > 0)
                            section.permissions = permissions;
                        else
                            section.permissions = undefined;
                    });

                    angular.forEach(module.fields, function (field) {
                        if (field.data_type === 'combination') {
                            field.data_type = 'text_single';
                            field.display_form = false;
                            field.combination = {};
                            var field1Name = field.combinationField1;
                            var field2Name = field.combinationField2;

                            if (field1Name.indexOf('custom_field') > -1) {
                                var field1 = $filter('filter')(module.fields, { name: field1Name }, true)[0];
                                field1Name = helper.getSlug(field1['label_' + $rootScope.language]);
                            }

                            if (field2Name.indexOf('custom_field') > -1) {
                                var field2 = $filter('filter')(module.fields, { name: field2Name }, true)[0];
                                field2Name = helper.getSlug(field2['label_' + $rootScope.language]);
                            }

                            field.combination.field1 = field1Name;
                            field.combination.field2 = field2Name;
                            field.combination.combination_character = field.combinationCharacter;
                            delete field.combinationCharacter;

                            if (!field.validation)
                                field.validation = {};

                            field.validation.readonly = true;
                            field.inline_edit = false;

                            delete field.combinationField1;
                            delete field.combinationField2;
                        }

                        if (field.data_type === 'number_auto') {
                            if (!field.validation)
                                field.validation = {};

                            field.validation.readonly = true;
                            field.inline_edit = false;
                        }

                        if (field.name === 'related_module') {
                            field.picklist_id = field.picklist_original_id;
                        }

                        if (field.unique_combine && field.unique_combine.indexOf('custom_field') > -1) {
                            var combinationField = $filter('filter')(module.fields, { name: field.unique_combine }, true)[0];
                            field.unique_combine = helper.getSlug(combinationField['label_' + $rootScope.language]);
                        }

                        if (field.data_type === 'url' && (!field.validation || !field.validation.pattern)) {
                            if (!field.validation)
                                field.validation = {};

                            field.validation.pattern = '^(https?|ftp)://.*$';
                        }
                        if (field.data_type === 'location') {
                            if (!field.validation)
                                field.validation = {};
                            field.validation.readonly = true;
                            field.inline_edit = false;
                        }

                        if (field.encrypted && field.encryption_authorized_users && field.encryption_authorized_users.length > 0) {
                            var encryptionAuthorizedUsers = null;
                            for (var j = 0; j < field.encryption_authorized_users.length; j++) {
                                var user = field.encryption_authorized_users[j];
                                if (encryptionAuthorizedUsers === null)
                                    encryptionAuthorizedUsers = user.id;
                                else
                                    encryptionAuthorizedUsers += ',' + user.id;
                            }
                            field.encryption_authorized_users = encryptionAuthorizedUsers;
                        }

                        if (!field.encrypted || (field.encrypted && field.encryption_authorized_users.length < 1)) {
                            field.encryption_authorized_users = null;
                            field.encryption_authorized_users_list = null;
                        }

                        delete field.show_lock;

                        //permissions
                        var fieldPermissions = angular.copy(field.permissions);
                        var permissions = [];

                        angular.forEach(fieldPermissions, function (permission) {
                            if (permission.id || permission.type !== 'full')
                                permissions.push(permission);
                        });

                        if (permissions.length > 0)
                            field.permissions = permissions;
                        else
                            field.permissions = undefined;
                    });

                    //daha önce silinmiş bir field ile aynı isimde field eklerken hata olmaması için yazıldı
                    if (pureModule) {
                        for (var i = 0; i < pureModule.fields.length; i++) {
                            if (pureModule.fields[i].deleted)
                                module.fields.push(pureModule.fields[i]);
                        }
                    }

                    angular.forEach(module.fields, function (field) {
                        delete field.dataType;
                        delete field.operators;
                        delete field.systemRequired;
                        delete field.systemReadonly;
                        delete field.valueFormatted;

                        if (field.name.indexOf('custom_field') > -1) {
                            var slug = helper.getSlug(field['label_' + $rootScope.language]);

                            if (systemFields.indexOf(slug) > -1)
                                slug = slug + '_c';

                            var cleanSlug = angular.copy(slug);
                            var existingField = $filter('filter')(module.fields, { name: slug }, true)[0];

                            if (existingField) {
                                do {
                                    var fieldNameNumber;

                                    if (existingField.name.indexOf('_') > -1) {
                                        var slugParts = existingField.name.split('_');
                                        var lastPart = slugParts[slugParts.length - 1];
                                        fieldNameNumber = lastPart.replace(/\D/g, '');
                                        lastPart = lastPart.replace(/[0-9]/g, '');
                                        slugParts.pop();
                                        cleanSlug = slugParts.join('_') + '_' + lastPart;
                                    } else {
                                        fieldNameNumber = existingField.name.replace(/\D/g, '');
                                        cleanSlug = existingField.name.replace(/[0-9]/g, '');
                                    }

                                    var newSlug = '';

                                    if (fieldNameNumber)
                                        newSlug = cleanSlug + (parseInt(fieldNameNumber) + 1);
                                    else
                                        newSlug = cleanSlug + 2;

                                    existingField = $filter('filter')(module.fields, { name: newSlug }, true)[0];

                                    if (!existingField)
                                        field.name = newSlug;
                                }
                                while (existingField);
                            } else {
                                field.name = slug;
                            }
                        }
                    });

                    delete module.relations;
                    delete module.dependencies;
                    delete module.calculations;

                    return module;
                },

                getDeletedModules: function () {
                    var deferred = $q.defer();
                    var deletedModulesCache = $cache.get('modulesDeleted');

                    if (deletedModulesCache) {
                        deferred.resolve(deletedModulesCache);
                    } else {
                        $http.get(config.apiUrl + 'module/get_all_deleted')
                            .then(function (response) {
                                $cache.put('modulesDeleted', response.data);
                                deferred.resolve(response.data);
                            });
                    }

                    return deferred.promise;
                },

                hasFieldDisplayPermission: function (field) {
                    if (!field.permissions)
                        return true;

                    var permission = $filter('filter')(field.permissions, { profile_id: $rootScope.user.profile.id }, true)[0];

                    var hasFieldSectionDisplayPermission = function (field) {
                        if (!field.sectionObj || !field.sectionObj.permissions)
                            return true;

                        var permission = $filter('filter')(field.sectionObj.permissions, { profile_id: $rootScope.user.profile.id }, true)[0];

                        if (!permission)
                            return true;

                        return permission.type !== 'none';
                    };

                    if (!permission)
                        return hasFieldSectionDisplayPermission(field);

                    if (permission.type === 'none')
                        return false;

                    return hasFieldSectionDisplayPermission(field);
                },

                getFields: function (module) {
                    var fields = {};
                    fields.selectedFields = [];
                    fields.availableFields = [];
                    fields.allFields = [];
                    if (!module.relatedModule)
                        return fields;
                    var moduleFields = angular.copy(module.relatedModule.fields);
                    moduleFields = $filter('filter')(moduleFields, {
                        display_list: true,
                        lookup_type: '!relation'
                    }, true);

                    var seperatorFieldMain = {};
                    seperatorFieldMain.name = 'seperator-main';
                    seperatorFieldMain.label = $rootScope.language === 'tr' ? module.relatedModule.label_tr_singular : module.relatedModule.label_en_singular;
                    seperatorFieldMain.order = 0;
                    seperatorFieldMain.seperator = true;
                    moduleFields.push(seperatorFieldMain);
                    var seperatorLookupOrder = 0;

                    angular.forEach(moduleFields, function (field) {
                        if (field.data_type === 'lookup' && field.lookup_type !== 'relation') {
                            var lookupModule = angular.copy($filter('filter')($rootScope.appModules, { name: field.lookup_type }, true)[0]);
                            seperatorLookupOrder += 100;
                            if (lookupModule === null || lookupModule === undefined) return;
                            var seperatorFieldLookup = {};
                            seperatorFieldLookup.name = 'seperator-' + lookupModule.name;
                            seperatorFieldLookup.order = seperatorLookupOrder;
                            seperatorFieldLookup.seperator = true;

                            if ($rootScope.language === 'tr')
                                seperatorFieldLookup.label = lookupModule.label_tr_singular + ' (' + field.label_tr + ')';
                            else
                                seperatorFieldLookup.label = lookupModule.label_en_singular + ' (' + field.label_en + ')';

                            moduleFields.push(seperatorFieldLookup);

                            var lookupModuleFields = angular.copy(lookupModule.fields);
                            lookupModuleFields = $filter('filter')(lookupModuleFields, { display_list: true }, true);

                            angular.forEach(lookupModuleFields, function (fieldLookup) {
                                if (fieldLookup.data_type === 'lookup')
                                    return;

                                fieldLookup.label = $rootScope.language === 'tr' ? fieldLookup.label_tr : fieldLookup.label_en;
                                fieldLookup.labelExt = '(' + field.label + ')';
                                fieldLookup.name = field.name + '.' + lookupModule.name + '.' + fieldLookup.name;
                                fieldLookup.order = parseInt(fieldLookup.order) + seperatorLookupOrder;
                                moduleFields.push(fieldLookup);
                            });
                        }
                    });

                    angular.forEach(moduleFields, function (field) {
                        if (field.deleted)
                            return;

                        var selectedField = null;

                        if (module.display_fields) {
                            var selectedFieldName = $filter('filter')(module.display_fields, field.name, true)[0];
                            if (selectedFieldName) {
                                selectedField = $filter('filter')(moduleFields, { "name": selectedFieldName }, true)[0];
                            }
                        }
                        ;


                        var newField = {};
                        newField.name = field.name;
                        newField.label = field.label;
                        newField.labelExt = field.labelExt;
                        newField.order = field.order;
                        newField.lookup_type = field.lookup_type;
                        newField.seperator = field.seperator;
                        newField.multiline_type = field.multiline_type;

                        if (selectedField) {
                            newField.order = selectedField.order;
                            fields.selectedFields.push(newField);
                        } else {
                            var primaryField = $filter('filter')(moduleFields, { primary: true }, true)[0];

                            if (field.name !== primaryField.name)
                                fields.availableFields.push(newField);
                            else
                                fields.selectedFields.push(newField);
                        }

                        fields.allFields.push(newField);
                    });

                    fields.selectedFields = $filter('orderBy')(fields.selectedFields, 'order');
                    fields.availableFields = $filter('orderBy')(fields.availableFields, 'order');

                    return fields;
                },

                deleteFieldsMappings: function (request) {
                    return $http.post(config.apiUrl + 'convert/delete_fields_mappings/', request);
                },

                processRelations: function (relations) {
                    angular.forEach(relations, function (relation) {
                        var relatedModule = $filter('filter')($rootScope.appModules, { name: relation.related_module }, true)[0];

                        if (!relatedModule || relatedModule.order === 0) {
                            relation.deleted = true;
                            return;
                        }

                        relation.relatedModule = relatedModule;

                        var relationField = $filter('filter')(relatedModule.fields, {
                            name: relation.relation_field,
                            deleted: false
                        }, true)[0];

                        if (!relationField && relation.relation_type === 'one_to_many') {
                            relation.deleted = true;
                            return;
                        }

                        if (relation.relation_type === 'one_to_many') {
                            relation.relationField = relationField;
                        }
                    });

                    return relations;
                },

                prepareRelation: function (relation) {
                    relation.related_module = relation.relatedModule.name;

                    if (relation.relationField)
                        relation.relation_field = relation.relationField.name;

                    delete relation.relatedModule;
                    delete relation.relationField;
                    delete relation.hasRelationField;
                    delete relation.type;

                    var otherLanguage = $rootScope.language === 'en' ? 'tr' : 'en';

                    if (!relation['label_' + otherLanguage]) {
                        relation['label_' + otherLanguage + '_plural'] = relation['label_' + $rootScope.language + '_plural'];
                        relation['label_' + otherLanguage + '_singular'] = relation['label_' + $rootScope.language + '_singular'];
                    }
                },

                prepareRecord: function (record, module, currentRecord) {
                    var newRecord = angular.copy(record);
                    var newCurrentRecord = angular.copy(currentRecord);

                    //region BUG 1061
                    if (currentRecord) {
                        for (var i = 0; i < module.dependencies.length; i++) {
                            var dependency = module.dependencies[i];
                            if (dependency.dependency_type === 'display' && !dependency.deleted) {
                                if (!angular.equals(record[dependency.parent_field], currentRecord[dependency.parent_field])) {
                                    if (dependency.values_array && dependency.values_array.length > 0) {
                                        var empty = true;
                                        for (var j = 0; j < dependency.values_array.length; j++) {
                                            var value = dependency.values_array[j];
                                            if (Array.isArray(record[dependency.parent_field])) {
                                                for (var k = 0; k < record[dependency.parent_field].length; k++) {
                                                    var multiValue = record[dependency.parent_field][k];
                                                    if (multiValue.id.toString() === value) {
                                                        empty = false;
                                                    }
                                                }
                                            } else if (record[dependency.parent_field].id.toString() === value) {
                                                empty = false;
                                            }
                                        }
                                        if (empty && dependency.child_field) {
                                            newRecord[dependency.child_field] = null;
                                        }
                                    } else if (!record[dependency.parent_field] && dependency.child_field) {
                                        newRecord[dependency.child_field] = null;
                                    }
                                }
                            }
                        }
                    }
                    //endregion

                    for (var i = 0; i < module.fields.length; i++) {
                        var field = module.fields[i];

                        if ((typeof newRecord[field.name] === 'string' && newRecord[field.name].trim() === ''))
                            newRecord[field.name] = undefined;

                        if (!currentRecord && !newRecord[field.name])
                            continue;

                        if (currentRecord && !currentRecord[field.name] && !newRecord[field.name]) {
                            delete newRecord[field.name];
                            continue;
                        }

                        if (field.data_type === 'checkbox' && newRecord[field.name] === null && currentRecord[field.name])
                            newRecord[field.name] = false;

                        if (field.deleted) {
                            delete newRecord[field.name];
                            continue;
                        }

                        if (newRecord[field.name] !== undefined && newRecord[field.name] !== null) {
                            if (!newCurrentRecord)
                                newCurrentRecord = {};

                            switch (field.data_type) {
                                case 'number':
                                    newRecord[field.name] = parseInt(newRecord[field.name]);
                                    newCurrentRecord[field.name] = newCurrentRecord[field.name] ? parseInt(newCurrentRecord[field.name]) : null;
                                    break;
                                case 'number_decimal':
                                    break;
                                case 'checkbox':
                                    if (newRecord[field.name] === null && newRecord[field.name] === undefined) {
                                        newRecord[field.name] = false;
                                    }
                                    break;
                                case 'currency':
                                    newRecord[field.name] = parseFloat(newRecord[field.name]);
                                    newCurrentRecord[field.name] = newCurrentRecord[field.name] ? parseFloat(newCurrentRecord[field.name]) : null;
                                    break;
                                case 'date':
                                    var dateParts = moment(newRecord[field.name]).format().split('+');
                                    var datePartsCurrent = moment(newCurrentRecord[field.name]).format().split('+');
                                    newRecord[field.name] = dateParts[0];
                                    newCurrentRecord[field.name] = newCurrentRecord[field.name] ? datePartsCurrent[0] : null;
                                    break;
                                case 'picklist':
                                case 'lookup':
                                    newRecord[field.name] = newRecord[field.name].id;
                                    newCurrentRecord[field.name] = newCurrentRecord[field.name] ? newCurrentRecord[field.name].id : null;
                                    break;
                                case 'text_multi':
                                    var htmltext = function (html) {
                                        var tag = document.createElement('div');
                                        tag.innerHTML = html;

                                        return tag.innerHTML.toString();
                                    };

                                    var htmlValue = newRecord[field.name];
                                    if (field.multiline_type_use_html === true) {
                                        var htmlValueConvert = htmltext(htmlValue);
                                        newRecord[field.name] = htmlValueConvert;
                                    }
                                    break;
                                case 'multiselect':
                                    var ids = [];
                                    var currentIds = [];

                                    for (var j = 0; j < newRecord[field.name].length; j++) {
                                        var item = newRecord[field.name][j];
                                        ids.push(item.id);
                                    }

                                    if (newCurrentRecord[field.name]) {
                                        for (var k = 0; k < newCurrentRecord[field.name].length; k++) {
                                            var picklistItem = newCurrentRecord[field.name][k];
                                            currentIds.push(picklistItem.id);
                                        }
                                    }

                                    if (ids && ids.length)
                                        newRecord[field.name] = ids;
                                    else
                                        newRecord[field.name] = null;

                                    if (currentIds && currentIds.length)
                                        newCurrentRecord[field.name] = currentIds;
                                    else
                                        newCurrentRecord[field.name] = null;
                                    break;
                                case "tag":
                                    var tags = [];
                                    angular.forEach(newRecord[field.name], function (item) {
                                        tags.push(item["text"]);
                                    });
                                    newRecord[field.name] = tags.toString();
                                    break;

                            }

                            if (currentRecord && angular.equals(newCurrentRecord[field.name], newRecord[field.name]))
                                delete newRecord[field.name];
                        } else {
                            newRecord[field.name] = null;
                        }
                    }

                    if (newRecord.shared_read && newRecord.shared_read.length) {
                        newRecord.shared_users = [];
                        newRecord.shared_user_groups = [];

                        for (var l = 0; l < newRecord.shared_read.length; l++) {
                            var shared = newRecord.shared_read[l];

                            if (shared.type === 'user')
                                newRecord.shared_users.push(shared.id);

                            if (shared.type === 'group')
                                newRecord.shared_user_groups.push(shared.id);
                        }

                        if (!newRecord.shared_users.length)
                            newRecord.shared_users = null;

                        if (!newRecord.shared_user_groups.length)
                            newRecord.shared_user_groups = null;

                        delete newRecord.shared_read;
                    } else {
                        newRecord.shared_users = null;
                        newRecord.shared_user_groups = null;
                        delete newRecord.shared_read;
                    }

                    if (newRecord.shared_edit && newRecord.shared_edit.length) {
                        newRecord.shared_users_edit = [];
                        newRecord.shared_user_groups_edit = [];

                        for (var m = 0; m < newRecord.shared_edit.length; m++) {
                            var sharedEdit = newRecord.shared_edit[m];

                            if (sharedEdit.type === 'user')
                                newRecord.shared_users_edit.push(sharedEdit.id);

                            if (sharedEdit.type === 'group')
                                newRecord.shared_user_groups_edit.push(sharedEdit.id);
                        }

                        if (!newRecord.shared_users_edit.length)
                            newRecord.shared_users_edit = null;

                        if (!newRecord.shared_user_groups_edit.length)
                            newRecord.shared_user_groups_edit = null;

                        delete newRecord.shared_edit;
                    } else {
                        newRecord.shared_users_edit = null;
                        newRecord.shared_user_groups_edit = null;
                        delete newRecord.shared_edit;
                    }

                    return newRecord;
                },

                getFieldsOperator: function (module, modules, counter) {
                    var that = this;
                    angular.forEach(module.fields, function (field) {
                        field.dataType = dataTypes[field.data_type];
                        field.operators = [];
                        if (field.data_type === 'lookup') {
                            if (field.lookup_type !== 'users' && field.lookup_type !== 'profiles' && field.lookup_type !== 'roles' && field.lookup_type !== 'relation') {
                                var lookupModule = $filter('filter')($rootScope.appModules, { name: field.lookup_type }, true)[0];
                                if (lookupModule)
                                    that.getModuleFields(lookupModule.name).then(function (response) {
                                        lookupModule.fields = response.data;
                                        field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { primary_lookup: true }, true)[0];

                                        if (!field.lookupModulePrimaryField)
                                            field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];

                                        var lookupModulePrimaryFieldDataType = dataTypes[field.lookupModulePrimaryField.data_type];

                                        field.lookupModulePrimaryField.operators = [];

                                        for (var m = 0; m < lookupModulePrimaryFieldDataType.operators.length; m++) {
                                            var operatorIdLookup = lookupModulePrimaryFieldDataType.operators[m];
                                            var operatorLookup = operators[operatorIdLookup];
                                            field.operators.push(operatorLookup);
                                            field.lookupModulePrimaryField.operators.push(operatorLookup);
                                        }
                                    });
                            } else {
                                field.operators.push(operators.equals);
                                field.operators.push(operators.not_equal);
                                field.operators.push(operators.empty);
                                field.operators.push(operators.not_empty);
                                //TODO WHEN ADDED CUSTOM MODULES USER, PROFILES AND ROLES
                                // if (field.lookup_type === 'users') {
                                //     var lookupModule = $filter('filter')($rootScope.appModules, { name: 'users' }, true)[0];
                                //that.getModuleFields(lookupModule.name).then(function (response) {
                                //lookupModule.fields = response.data;
                                //     field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];
                                //});
                                // }
                                // else if (field.lookup_type === 'profiles') {
                                //	lookupModule = $filter('filter')($rootScope.appModules, { name: 'profiles' }, true)[0];
                                //	that.getModuleFields(lookupModule.name).then(function (response) {
                                //		lookupModule.fields = response.data;
                                //		field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];
                                //	});
                                //}
                                //else if (field.lookup_type === 'roles') {
                                //	lookupModule = $filter('filter')($rootScope.appModules, { name: 'roles' }, true)[0];
                                //	that.getModuleFields(lookupModule.name).then(function (response) {
                                //		lookupModule.fields = response.data;
                                //		field.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];
                                //	});
                                //}
                            }

                        } else {
                            for (var n = 0; n < field.dataType.operators.length; n++) {
                                var operatorId = field.dataType.operators[n];
                                var operator = operators[operatorId];
                                field.operators.push(operator);
                            }
                        }
                    });
                    return module;
                },

                getAllProcess: function (id) {
                    return $http.get(config.apiUrl + 'process/get_all');
                },

                getActionButtons: function (moduleId) {
                    return $http.get(config.apiUrl + 'action_button/get/' + moduleId);
                },

                createActionButton: function (actionButton) {
                    return $http.post(config.apiUrl + 'action_button/create', actionButton);
                },

                updateActionButton: function (actionButton) {
                    return $http.put(config.apiUrl + 'action_button/update/' + actionButton.id, actionButton);
                },

                deleteActionButton: function (id) {
                    return $http.delete(config.apiUrl + 'action_button/delete/' + id);
                },

                lookup: function (searchTerm, field, record, additionalFields, exactMatch) {
                    var deferred = $q.defer();
                    var lookupType = field.lookup_type;
                    var that = this;
                    var isDropdownField = field.data_type === 'lookup' && field.show_as_dropdown;
                    if (field.lookupModulePrimaryField.data_type != 'text_single' && field.lookupModulePrimaryField.data_type != 'picklist' && field.lookupModulePrimaryField.data_type != 'email' &&
                        field.lookupModulePrimaryField.data_type != 'number' && field.lookupModulePrimaryField.data_type != 'number_auto') {
                        deferred.resolve([]);
                        return deferred.promise;
                    }

                    if (lookupType === 'relation')
                        lookupType = record[field.lookup_relation] != undefined ? record[field.lookup_relation].value : null;

                    if (!lookupType) {
                        deferred.resolve([]);
                        return deferred.promise;
                    }

                    var hasPermission = lookupType != 'users' && lookupType != 'profiles' && lookupType != 'roles' ? helper.hasPermission(lookupType, operations.read) : true;

                    if (!hasPermission && !($rootScope.branchAvailable && lookupType == 'branches')) {
                        deferred.resolve([]);
                        return deferred.promise;
                    }

                    if (!searchTerm && !isDropdownField) {
                        deferred.resolve([]);
                        return deferred.promise;
                    }

                    var lookupModule = $filter('filter')($rootScope.appModules, { name: lookupType }, true)[0];
                    var selectedFields = [];
                    selectedFields.push(field.lookupModulePrimaryField.name);

                    if (additionalFields) {
                        for (var i = 0; i < additionalFields.length; i++) {
                            var additionalField = additionalFields[i];

                            if (additionalField != field.lookupModulePrimaryField.name && additionalField != 'id')
                                selectedFields.push(additionalField)
                        }
                    }

                    var filters = [];

                    if (field.lookupModulePrimaryField.data_type != 'number' && field.lookupModulePrimaryField.data_type != 'number_auto') {
                        if (!exactMatch)
                            switch (field.lookup_search_type) {
                                case 'contains':
                                    filters.push({ field: field.lookupModulePrimaryField.name, operator: 'contains', value: searchTerm, no: 1 });
                                    break;
                                case 'starts_with':
                                    filters.push({ field: field.lookupModulePrimaryField.name, operator: 'starts_with', value: searchTerm, no: 1 });
                                    break;
                                default:
                                    filters.push({ field: field.lookupModulePrimaryField.name, operator: 'starts_with', value: searchTerm, no: 1 });
                                    break;
                            }
                        else
                            filters.push({ field: field.lookupModulePrimaryField.name, operator: 'is', value: searchTerm, no: 1 });
                    }
                    else {
                        filters.push({ field: field.lookupModulePrimaryField.name, operator: 'equals', value: parseInt(searchTerm), no: 1 });
                    }


                    var findRequest = {
                        fields: selectedFields,
                        filters: filters,
                        sort_field: field.lookupModulePrimaryField.name,
                        sort_direction: 'asc',
                        limit: 1000,
                        offset: 0
                    };

                    //Lookup type show as dropdown
                    if (isDropdownField) {
                        findRequest.filters = [];
                        findRequest.limit = 1000;
                    }
                    //get only active users to list! if need also inactive users, use utils lookupuser with includeInactiveUsers parameter
                    if (lookupModule.name == 'users' || ($rootScope.branchAvailable && lookupType == 'branches')) {
                        var filterOrderNo = findRequest.filters.length + 1;
                        findRequest.filters.push({ field: 'is_active', operator: 'equals', value: true, no: filterOrderNo });
                    }

                    //lookup field filters (from field_filters table)
                    if (field.filters) {
                        var no = findRequest.filters.length;
                        for (var z = 0; z < field.filters.length; z++) {
                            var filter = field.filters[z];
                            no++;
                            var filterMatch = filter.value.match(/^\W+(.+)]/i);
                            if (filterMatch != null && field.lookup_type != 'users' && field.lookup_type != 'profiles' && field.lookup_type != 'roles') {
                                var recordMatch = filterMatch[1].split('.');
                                var findRecordValue;

                                if (recordMatch.length === 1 && record[recordMatch[0]])
                                    findRecordValue = record[recordMatch[0]];

                                if (recordMatch.length === 2 && record[recordMatch[0]])
                                    findRecordValue = record[recordMatch[0]][recordMatch[1]];

                                if (recordMatch.length === 3 && record[recordMatch[0]])
                                    findRecordValue = record[recordMatch[0]][recordMatch[1]][recordMatch[2]];

                                if (findRecordValue != null) {
                                    findRequest.filters.push({ field: filter.filter_field, operator: filter.operator, value: findRecordValue, no: no });
                                    findRequest.fields.push(filter.filter_field);
                                }

                            } else {
                                findRequest.filters.push({ field: filter.filter_field, operator: filter.operator, value: filter.value, no: no });
                                findRequest.fields.push(filter.filter_field);
                            }

                        }
                    }

                    this.findRecords(lookupType, findRequest)
                        .then(function (response) {
                            if (!response.data || !response.data.length) {
                                deferred.resolve([]);
                                return deferred.promise;
                            }

                            var lookupRecords = [];

                            if (!additionalFields) {
                                for (var i = 0; i < response.data.length; i++) {
                                    var recordItem = response.data[i];
                                    if (lookupType == 'profiles' && recordItem['id'] === 1) {
                                        recordItem['name'] = $rootScope.user.tenant_language === 'tr' ? 'Sistem Yöneticisi' : 'Administrator';
                                    }
                                    var lookupRecord = angular.copy(recordItem);

                                    lookupRecord.primary_value = recordItem[field.lookupModulePrimaryField.name];
                                    lookupRecords.push(lookupRecord);
                                    deferred.resolve(lookupRecords);
                                }
                            }
                            else {
                                that.getPicklists(lookupModule)
                                    .then(function (picklists) {
                                        for (var i = 0; i < response.data.length; i++) {
                                            var recordItem = response.data[i];
                                            var lookupRecord = angular.copy(recordItem);
                                            lookupRecord = that.processRecordSingle(lookupRecord, lookupModule, picklists);
                                            lookupRecord.primary_value = recordItem[field.lookupModulePrimaryField.name];
                                            lookupRecords.push(lookupRecord);
                                            deferred.resolve(lookupRecords);
                                        }
                                    });
                            }
                        })
                        .catch(function (reason) {
                            deferred.reject(reason.data);
                        });

                    return deferred.promise;
                },
            };


        }
    ])
    ;

angular.module('primeapps')

    .constant('defaultLabels', {
        DefaultModuleNameEn: 'Module',
        DefaultModuleNameTr: 'Modül',
        DefaultSectionNameEn: 'General',
        DefaultSectionNameTr: 'Genel',
        SystemInfoSectionNameEn: 'System',
        SystemInfoSectionNameTr: 'Sistem',
        DefaultFieldNameEn: 'Name',
        DefaultFieldNameTr: 'İsim',
        UserLookupFieldEn: 'User',
        UserLookupFieldTr: 'Kullanıcı',
        ProfileLookupFieldEn: 'Profile',
        ProfileLookupFieldTr: 'Profil',
        RoleLookupFieldEn: 'Role',
        RoleLookupFieldTr: 'Rol',
        CreatedByFieldEn: 'Created by',
        CreatedByFieldTr: 'Oluşturan',
        UpdatedByFieldEn: 'Updated by',
        UpdatedByFieldTr: 'Güncelleyen',
        CreatedAtFieldEn: 'Created at',
        CreatedAtFieldTr: 'Oluşturulma Tarihi',
        UpdatedAtFieldEn: 'Updated at',
        UpdatedAtFieldTr: 'Güncellenme Tarihi',
        DefaultPicklistItemEn: 'Option',
        DefaultPicklistItemTr: 'Seçenek',
        DataTypeCombinationEn: 'Combination',
        DataTypeCombinationTr: 'Birleşim',
        DataTypeCalculatedEn: 'Calculated',
        DataTypeCalculatedTr: 'Hesaplama'
    });