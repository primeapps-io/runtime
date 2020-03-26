'use strict';

angular.module('primeapps')

    .factory('DependenciesService', ['$rootScope', '$http', 'config', '$filter', '$q', 'helper', 'defaultLabels', '$cache', 'dataTypes', 'systemFields', 'activityTypes', 'yesNo', 'transactionTypes', 'ModuleService',
        function ($rootScope, $http, config, $filter, $q, helper, defaultLabels, $cache, dataTypes, systemFields, activityTypes, yesNo, transactionTypes, ModuleService) {

            return {
                count: function (id) {
                    return $http.get(config.apiUrl + 'dependency/count/' + id);
                },
                find: function (id, data) {
                    return $http.post(config.apiUrl + 'dependency/find/' + id, data);
                },

                getDependency: function (id) {
                    return $http.get(config.apiUrl + 'dependency/get_by_id/' + id);
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

                findPicklist: function (ids) {
                    return $http.post(config.apiUrl + 'picklist/find', ids);
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

                getAllPicklists: function () {
                    return $http.get(config.apiUrl + 'picklist/get_all');
                },

                getPicklists: function (module, withRelatedPicklists) {
                    var deferred = $q.defer();
                    var that = this;
                    var fields = angular.copy(module.fields);
                    var picklists = {};
                    var picklistIds = [];

                    //ModuleService.getModuleFields(module).then(function (response) {
                    //fields = response.data;

                    if (withRelatedPicklists) {
                        for (var i = 0; i < module.fields.length; i++) {
                            var field = module.fields[i];

                            if (field.data_type === 'lookup' && field.lookup_type != 'users' && field.lookup_type != 'relation') {
                                var lookupModule = $filter('filter')($rootScope.modules, { name: field.lookup_type }, true)[0];

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

                            if (dependency && dependency.deleted != true && (dependency.dependency_type === 'list_field' || dependency.dependency_type === 'list_value')) {
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

                                for (var l = 0; l < $rootScope.modules.length; l++) {
                                    var moduleItem = $rootScope.modules[l];

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
                                    modulePicklistItem.labelStr = moduleItem['label_' + $rootScope.user.tenantLanguage + '_singular'];
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
                            }
                            else {
                                picklistCache = $filter('orderByLabel')(picklistCache, $rootScope.language);

                                if (fieldItem.picklist_sortorder)
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

                    that.findPicklist(picklistIds)
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

                                if (field.picklist_sortorder)
                                    picklists[field.picklist_id] = $filter('orderBy')(picklists[field.picklist_id], field.picklist_sortorder);

                                if (module.dependencies && module.dependencies.length > 0) {
                                    var dependency = $filter('filter')(module.dependencies, { child_field: field.name }, true)[0];

                                    if (dependency && dependency.deleted != true && dependency.dependency_type === 'list_field') {
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
                    //  });
                    return deferred.promise;
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

                    angular.forEach($rootScope.modules, function (moduleItem) {
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
                        if (field.data_type === 'lookup' && field.lookup_type != 'relation') {
                            var lookupModule = angular.copy($filter('filter')($rootScope.modules, { name: field.lookup_type }, true)[0]);
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

                processRelations: function (relations) {
                    angular.forEach(relations, function (relation) {
                        var relatedModule = $filter('filter')($rootScope.modules, { name: relation.related_module }, true)[0];

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

                // processDependencies: function (module) {
                //     var copyModule = module;
                //     if (!module.dependencies)
                //         module.dependencies = [];
                //
                //     if (!module.display_dependencies)
                //         module.display_dependencies = [];
                //
                //     var dependencies = [];
                //
                //     angular.forEach(module.dependencies, function (dependency) {
                //         dependency.parent_module = copyModule;
                //         dependency.type = dependency.dependency_type;
                //         dependency.parentField = $filter('filter')(module.fields, {
                //             name: dependency.parent_field,
                //             deleted: '!true'
                //         })[0];
                //         dependency.childField = $filter('filter')(module.fields, {
                //             name: dependency.child_field,
                //             deleted: '!true'
                //         })[0];
                //         dependency.sectionField = $filter('filter')(module.sections, {
                //             name: dependency.child_section,
                //             deleted: '!true'
                //         })[0];
                //
                //         if (dependency.dependency_type === 'display') {
                //             dependency.dependencyType = 'display';
                //             dependency.name = $filter('translate')('Setup.Modules.DependencyTypeDisplay');
                //
                //             if (dependency.values && !Array.isArray(dependency.values)) {
                //                 var values = dependency.values.split(',');
                //                 dependency.values = [];
                //
                //                 angular.forEach(values, function (value) {
                //                     dependency.values.push(parseInt(value));
                //                 });
                //             }
                //         }
                //         else if (dependency.dependency_type === 'freeze') {
                //             dependency.dependencyType = 'freeze';
                //             dependency.name = $filter('translate')('Setup.Modules.DependencyTypeFreeze');
                //
                //             if (dependency.values && !Array.isArray(dependency.values)) {
                //                 var values = dependency.values.split(',');
                //                 dependency.values = [];
                //
                //                 angular.forEach(values, function (value) {
                //                     dependency.values.push(parseInt(value));
                //                 });
                //             }
                //         }
                //
                //         else {
                //             dependency.dependencyType = 'value';
                //             dependency.name = $filter('translate')('Setup.Modules.DependencyTypeValueChange');
                //
                //
                //             if (dependency.field_map_parent) {
                //                 dependency.field_map = {};
                //                 dependency.field_map.parent_map_field = dependency.field_map_parent;
                //                 dependency.field_map.child_map_field = dependency.field_map_child;
                //             }
                //
                //             if (dependency.value_map && !dependency.value_maps) {
                //                 dependency.value_maps = {};
                //
                //                 var valueMaps = dependency.value_map.split('|');
                //
                //                 angular.forEach(valueMaps, function (valueMap) {
                //                     var map = valueMap.split(';');
                //                     dependency.value_maps[map[0]] = map[1].split(',');
                //                 });
                //             }
                //         }
                //
                //         if (dependency.parentField && dependency.childField)
                //             dependencies.push(dependency);
                //     });
                //
                //     return dependencies;
                // },

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

                createModuleDependency: function (dependency, moduleId) {
                    this.removeAppModules();
                    return $http.post(config.apiUrl + 'module/create_dependency/' + moduleId, dependency);
                },

                updateModuleDependency: function (dependency, moduleId) {
                    this.removeAppModules();
                    return $http.put(config.apiUrl + 'module/update_dependency/' + moduleId + '/' + dependency.id, dependency);
                },

                deleteModuleDependency: function (id) {
                    this.removeAppModules();
                    return $http.delete(config.apiUrl + 'module/delete_dependency/' + id);
                },

                removeAppModules: function () {
                    if ($rootScope.activeAppId) {
                        var appModules = $filter('filter')($rootScope.appModules, { appId: $rootScope.activeAppId }, true)[0];

                        if (appModules)
                            $rootScope.appModules.splice($rootScope.appModules.indexOf(appModules), 1);
                    }
                },
                processDependencies: function (dependency) {//(dependencies) {
                    // angular.forEach(dependencies, function (dependency) {
                    // dependency.parent_module = dependency.module;
                    dependency.type = dependency.dependency_type;
                    // dependency.parentField = $filter('filter')(dependency.module.fields, {
                    //     name: dependency.parent_field,
                    //     deleted: '!true'
                    // })[0];
                    // dependency.childField = $filter('filter')(dependency.module.fields, {
                    //     name: dependency.child_field,
                    //     deleted: '!true'
                    // })[0];
                    // dependency.sectionField = $filter('filter')(dependency.module.sections, {
                    //     name: dependency.child_section,
                    //     deleted: '!true'
                    // })[0];

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
                                map[1] = map[1].split(',');
                                for (var i = 0; i < map[1].length; i++) {
                                    map[1][i] = parseInt(map[1][i]);
                                }
                                dependency.value_maps[map[0]] = map[1];
                            });
                        }
                    }
                    //});
                    // return dependencies;
                    return dependency;
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