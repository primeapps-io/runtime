'use strict';

angular.module('primeapps')

    .factory('RelationsService', ['$rootScope', '$http', 'config', '$filter', '$q', 'helper', 'defaultLabels', '$cache', 'dataTypes', 'systemFields',
        function ($rootScope, $http, config, $filter, $q, helper, defaultLabels, $cache, dataTypes, systemFields) {
            return {
                count: function (id) {
                    return $http.get(config.apiUrl + 'relation/count/' + id);
                },
                find: function (id, data) {
                    return $http.post(config.apiUrl + 'relation/find/' + id, data);
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

                createPicklist: function (picklist) {
                    return $http.post(config.apiUrl + 'picklist/create', picklist);
                },

                updatePicklist: function (picklist) {
                    return $http.put(config.apiUrl + 'picklist/update/' + picklist.id, picklist);
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

                prepareDefaults: function (module) {
                    module.system_type = 'custom';
                    module.label_en_plural = defaultLabels.DefaultModuleNameEn;
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

                    module.order = Math.max.apply(null, sortOrders) + 1;
                    module.name = 'custom_module' + module.order;
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
                    field1.section = 'custom_section1';
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
                    field2.section_column = 2;
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
                                if (field.section != section.name || field.section_column != columItem.no || field.deleted)
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
                                field.section_column = column.column.no;
                                field.order = lastFieldOrder + 1;

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
                },

                prepareModule: function (module, picklistsModule, deletedModules) {
                    var otherLanguage = $rootScope.language === 'en' ? 'tr' : 'en';

                    if (module.name.indexOf('custom_module') > -1) {
                        module['label_' + otherLanguage + '_plural'] = module['label_' + $rootScope.language + '_plural'];
                        module['label_' + otherLanguage + '_singular'] = module['label_' + $rootScope.language + '_singular'];
                        module.name = helper.getSlug(module['label_' + $rootScope.language + '_plural']);
                        var allModules = $rootScope.appModules.concat(deletedModules);
                        var i = 2;

                        while (true) {

                            var findMatch = module.name.match(/(\D+)?\d/);
                            var index = findMatch ? findMatch[0].length - 1 : -1;
                            var newModuleName = index === 0 ? 'n' + module.name : module.name; // if first index value === 0, its starts_with number
                            var existingModule = $filter('filter')(allModules, { name: newModuleName }, true)[0];

                            if (!existingModule)
                                break;

                            if (i < 20) {
                                module.name = helper.getSlug(module['label_' + $rootScope.language + '_plural']) + i;
                                i++;
                            }
                            else {
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
                                    }
                                    else {
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
                            }
                            else
                                section.name = newSectionName;
                        }
                        //permissions
                        var sectionPermissions = angular.copy(section.permissions);
                        var permissions = [];

                        angular.forEach(sectionPermissions, function (permission) {
                            if (permission.id || permission.type != 'full')
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
                            if (permission.id || permission.type != 'full')
                                permissions.push(permission);
                        });

                        if (permissions.length > 0)
                            field.permissions = permissions;
                        else
                            field.permissions = undefined;
                    });

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
                                    }
                                    else {
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
                            }
                            else {
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
                    }
                    else {
                        $http.get(config.apiUrl + 'module/get_all_deleted')
                            .then(function (response) {
                                $cache.put('modulesDeleted', response.data);
                                deferred.resolve(response.data);
                            });
                    }

                    return deferred.promise;
                },

                getFields: function (module, allModules) {
                    var fields = {};
                    fields.selectedFields = [];
                    fields.availableFields = [];
                    fields.allFields = [];
                    if (!module.relation_module)
                        return fields;
                    var moduleFields = angular.copy(module.relation_module.fields);
                    moduleFields = $filter('filter')(moduleFields, {
                        display_list: true,
                        lookup_type: '!relation'
                    }, true);

                    var seperatorFieldMain = {};
                    seperatorFieldMain.name = 'seperator-main';
                    seperatorFieldMain.label = $rootScope.language === 'tr' ? module.relation_module.label_tr_singular : module.relation_module.label_en_singular;
                    seperatorFieldMain.order = 0;
                    seperatorFieldMain.seperator = true;
                    moduleFields.push(seperatorFieldMain);
                    var seperatorLookupOrder = 0;

                    angular.forEach(moduleFields, function (field) {
                        if (field.data_type === 'lookup' && field.lookup_type != 'relation') {
                            var lookupModule = angular.copy($filter('filter')(allModules, { name: field.lookup_type }, true)[0]);
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
                        newField.label = field.label ? field.label : field['label_' + $rootScope.language];
                        newField.labelExt = field.labelExt;
                        newField.order = field.order;
                        newField.lookup_type = field.lookup_type;
                        newField.seperator = field.seperator;
                        newField.multiline_type = field.multiline_type;

                        if (selectedField) {
                            newField.order = selectedField.order;
                            fields.selectedFields.push(newField);
                        }
                        else {
                            var primaryField = $filter('filter')(moduleFields, { primary: true }, true)[0];

                            if (field.name != primaryField.name)
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

                processRelations: function (relations, modules) {
                    angular.forEach(relations, function (relation) {
                        var relatedModule = $filter('filter')(modules, { name: relation.related_module }, true)[0];

                        if (!relatedModule || relatedModule.order === 0) {
                            relation.deleted = true;
                            return;
                        }

                        relation.relatedModule = relatedModule;

                        var relationField = $filter('filter')(relatedModule.fields, { name: relation.relation_field, deleted: false }, true)[0];

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
                    relation.related_module = relation.relation_module.name;

                    if (relation.relationField)
                        relation.relation_field = relation.relationField.name;

                    delete relation.relation_module;
                    delete relation.relationField;
                    delete relation.hasRelationField;
                    delete relation.type;
                    delete relation.display_fields_array;
                    delete relation.parent_module;
                    delete relation.module;


                    var otherLanguage = $rootScope.language === 'en' ? 'tr' : 'en';

                    if (!relation['label_' + otherLanguage]) {
                        relation['label_' + otherLanguage + '_plural'] = relation['label_' + $rootScope.language + '_plural'];
                        relation['label_' + otherLanguage + '_singular'] = relation['label_' + $rootScope.language + '_singular'];
                    }
                },

                processDependencies: function (module) {
                    if (!module.dependencies)
                        module.dependencies = [];

                    if (!module.display_dependencies)
                        module.display_dependencies = [];

                    var dependencies = [];

                    angular.forEach(module.dependencies, function (dependency) {
                        dependency.type = dependency.dependency_type;
                        dependency.parentField = $filter('filter')(module.fields, { name: dependency.parent_field, deleted: '!true' })[0];
                        dependency.childField = $filter('filter')(module.fields, { name: dependency.child_field, deleted: '!true' })[0];
                        dependency.sectionField = $filter('filter')(module.sections, {
                            name: dependency.child_section,
                            deleted: '!true'
                        })[0];

                        if (dependency.dependency_type === 'display') {
                            dependency.dependencyType = 'display';
                            dependency.name = $filter('translate')('Setup.Modules.DependencyTypeDisplay');

                            if (dependency.values && !Array.isArray(dependency.values)) {
                                var values = dependency.values.split(',');
                                dependency.values = [];

                                angular.forEach(values, function (value) {
                                    dependency.values.push(parseInt(value));
                                });
                            }
                        }
                        else if (dependency.dependency_type === 'freeze') {
                            dependency.dependencyType = 'freeze';
                            dependency.name = $filter('translate')('Setup.Modules.DependencyTypeFreeze');

                            if (dependency.values && !Array.isArray(dependency.values)) {
                                var values = dependency.values.split(',');
                                dependency.values = [];

                                angular.forEach(values, function (value) {
                                    dependency.values.push(parseInt(value));
                                });
                            }
                        }

                        else {
                            dependency.dependencyType = 'value';
                            dependency.name = $filter('translate')('Setup.Modules.DependencyTypeValueChange');


                            if (dependency.field_map_parent) {
                                dependency.field_map = {};
                                dependency.field_map.parent_map_field = dependency.field_map_parent;
                                dependency.field_map.child_map_field = dependency.field_map_child;
                            }

                            if (dependency.value_map && !dependency.value_maps) {
                                dependency.value_maps = {};

                                var valueMaps = dependency.value_map.split('|');

                                angular.forEach(valueMaps, function (valueMap) {
                                    var map = valueMap.split(';');
                                    dependency.value_maps[map[0]] = map[1].split(',');
                                });
                            }
                        }

                        if (dependency.parentField && dependency.childField)
                            dependencies.push(dependency);
                    });

                    return dependencies;
                },

                prepareDependency: function (dependency) {
                    switch (dependency.dependencyType) {
                        case 'display':
                            var displayDependency = {};
                            displayDependency.id = dependency.id;
                            displayDependency.dependency_type = 'display';
                            displayDependency.parent_field = dependency.parent_field;
                            displayDependency.child_field = dependency.child_field;
                            displayDependency.child_section = dependency.child_section;
                            displayDependency.values = dependency.values;

                            return displayDependency;
                        case 'freeze':
                            var displayDependency = {};
                            displayDependency.id = dependency.id;
                            displayDependency.dependency_type = 'freeze';
                            displayDependency.parent_field = dependency.parent_field;
                            displayDependency.child_field = dependency.child_field;
                            displayDependency.child_section = dependency.child_section;
                            displayDependency.values = dependency.values;

                            return displayDependency;
                        case 'value':
                            var valueDependency = {};
                            valueDependency.id = dependency.id;
                            valueDependency.dependency_type = dependency.type;
                            valueDependency.parent_field = dependency.parent_field;
                            valueDependency.child_field = dependency.child_field;

                            switch (dependency.type) {
                                case 'list_text':
                                    valueDependency.clear = dependency.clear;
                                    break;
                                case 'list_value':
                                    valueDependency.value_map = '';

                                    angular.forEach(dependency.value_maps, function (value, key) {
                                        valueDependency.value_map += key + ';';

                                        angular.forEach(value, function (valueItem) {
                                            valueDependency.value_map += valueItem + ',';
                                        });

                                        valueDependency.value_map = valueDependency.value_map.slice(0, -1) + '|';
                                    });

                                    valueDependency.value_map = valueDependency.value_map.slice(0, -1);
                                    break;
                                case 'list_field':
                                case 'lookup_list':
                                    valueDependency.field_map_parent = dependency.field_map.parent_map_field;
                                    valueDependency.field_map_child = dependency.field_map.child_map_field;
                                    break;
                            }

                            return valueDependency;
                    }
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

                updateField: function (fieldId, field) {
                    return $http.put(config.apiUrl + 'module/update_field/' + fieldId, field);
                },
                createModuleRelation: function (relation, moduleId) {
                    return $http.post(config.apiUrl + 'module/create_relation/' + moduleId, relation);
                },

                updateModuleRelation: function (relation, moduleId) {
                    return $http.put(config.apiUrl + 'module/update_relation/' + moduleId + '/' + relation.id, relation);
                },

                deleteModuleRelation: function (id) {
                    return $http.delete(config.apiUrl + 'module/delete_relation/' + id);
                }
            };
        }]);

angular.module('primeapps')

    .constant('defaultLabels', {
        DefaultModuleNameEn: 'Module',
        DefaultModuleNameTr: 'Modül',
        DefaultSectionNameEn: 'Section',
        DefaultSectionNameTr: 'Bölüm',
        SystemInfoSectionNameEn: 'System Information',
        SystemInfoSectionNameTr: 'Sistem Bilgisi',
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