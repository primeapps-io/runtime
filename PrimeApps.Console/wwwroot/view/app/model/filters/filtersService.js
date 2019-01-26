angular.module('primeapps')

    .factory('FiltersService', ['$rootScope', '$http', 'config', '$q', '$filter', 'ModuleService', '$cache', 'activityTypes', 'transactionTypes', 'yesNo', 'helper', 'dataTypes', 'operators',
        function ($rootScope, $http, config, $q, $filter, ModuleService, $cache, activityTypes, transactionTypes, yesNo, helper, dataTypes, operators) {
            return {

                create: function (view) {
                    return $http.post(config.apiUrl + 'view/create', view);
                },

                update: function (view, id) {
                    return $http.put(config.apiUrl + 'view/update/' + id, view);
                },

                deleteView: function (id) {
                    return $http.delete(config.apiUrl + 'view/delete/' + id);
                },

                getFields: function (module, view, modules) {
                    var fields = {};
                    var moduleFields = angular.copy(module.fields);
                    fields.selectedFields = [];
                    fields.availableFields = [];
                    fields.allFields = [];
                    moduleFields = $filter('filter')(moduleFields, { display_list: true, lookup_type: '!relation' }, true);

                    var seperatorFieldMain = {};
                    seperatorFieldMain.name = 'seperator-main';
                    seperatorFieldMain.label = $rootScope.language === 'tr' ? module.label_tr_singular : module.label_en_singular;
                    seperatorFieldMain.order = 0;
                    seperatorFieldMain.seperator = true;
                    moduleFields.push(seperatorFieldMain);
                    var seperatorLookupOrder = 0;

                    angular.forEach(moduleFields, function (field) {
                        if (field.data_type === 'lookup' && field.lookup_type != 'relation') {
                            var lookupModule = angular.copy($filter('filter')(modules, { name: field.lookup_type }, true)[0]);
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
                        if (field.deleted)//|| !ModuleService.hasFieldDisplayPermission(field))
                            return;

                        var selectedField = null;

                        if (view.fields)
                            selectedField = $filter('filter')(view.fields, { field: field.name }, true)[0];

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
                            var primaryField = $filter('filter')(module.fields, { primary: true }, true)[0];
                            if (primaryField) {
                                if (field.name != primaryField.name)
                                    fields.availableFields.push(newField);
                                else
                                    fields.selectedFields.push(newField);
                            }
                        }

                        fields.allFields.push(newField);
                    });

                    fields.selectedFields = $filter('orderBy')(fields.selectedFields, 'order');
                    fields.availableFields = $filter('orderBy')(fields.availableFields, 'order');

                    return fields;
                },

                getPicklists: function (module, withRelatedPicklists, modules) {
                    var deferred = $q.defer();
                    var that = this;
                    ModuleService.getModuleByName(module.name).then(function (response) {


                        var picklists = {};
                        var picklistIds = [];
                        module = response.data
                        var fields = angular.copy(module.fields);
                        module = response.data
                        var fields = angular.copy(module.fields);
                        if (withRelatedPicklists) {
                            for (var i = 0; i < module.fields.length; i++) {
                                var field = module.fields[i];

                                if (field.data_type === 'lookup' && field.lookup_type != 'users' && field.lookup_type != 'relation') {
                                    var lookupModule = $filter('filter')(modules, { name: field.lookup_type }, true)[0];

                                    if (!lookupModule)
                                        continue;
                                    ModuleService.getModuleFields(module.name).then(function (response) {
                                        lookupModule.fields = response.data;
                                        for (var j = 0; j < lookupModule.fields.length; j++) {
                                            var lookupModuleField = lookupModule.fields[j];

                                            if (lookupModuleField.data_type === 'picklist' || lookupModuleField.data_type === 'multiselect')
                                                fields.push(lookupModuleField);
                                        }
                                    });
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

                                    for (var l = 0; l < modules.length; l++) {
                                        var moduleItem = modules[l];

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
                                        modulePicklistItem.labelStr = moduleItem['label_' + $rootScope.language + '_singular'];
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

                        return deferred.promise;

                    });
                    return deferred.promise;
                },

                findPicklist: function (ids) {
                    return $http.post(config.apiUrl + 'picklist/find', ids);
                },

                getViews: function (moduleId, displayFields, cache) {
                    var that = this;
                    var deferred = $q.defer();
                    var module = $filter('filter')($rootScope.modules, { id: moduleId }, true)[0];

                    if (displayFields) {
                        var views = [];
                        var view = {};
                        view.active = true;
                        var viewFields = [];
                        var filterFieldIndex = 1;

                        for (var i = 0; i < displayFields.length; i++) {
                            var displayField = displayFields[i];
                            var field;

                            if (displayField.indexOf('.') > -1) {
                                var fieldParts = displayField.split('.');
                                var lookupModule = $filter('filter')($rootScope.modules, { name: fieldParts[1] }, true)[0];
                                field = $filter('filter')(lookupModule.fields, { name: fieldParts[2] }, true)[0];
                            }
                            else {
                                field = $filter('filter')(module.fields, { name: displayField }, true)[0]
                            }

                            if (!field || !that.hasFieldDisplayPermission(field))
                                continue;

                            var filterField = {};
                            filterField.field = displayField;
                            filterField.order = filterFieldIndex;

                            viewFields.push(filterField);
                            filterFieldIndex++;
                        }

                        view.fields = viewFields;
                        views.push(view);
                        deferred.resolve(views);
                        return deferred.promise;
                    }

                    if (cache && cache['views']) {
                        deferred.resolve(cache['views']);
                        return deferred.promise;
                    }

                    $http.get(config.apiUrl + 'view/get_all/' + moduleId)
                        .then(function (response) {
                            if (!response.data) {
                                deferred.resolve([]);
                                return deferred.promise;
                            }

                            var views = [];

                            for (var i = 0; i < response.data.length; i++) {
                                var view = response.data[i];
                                var viewFields = [];

                                for (var j = 0; j < view.fields.length; j++) {
                                    var viewField = view.fields[j];
                                    var field;

                                    if (viewField.field.indexOf('.') > -1) {
                                        var fieldParts = viewField.field.split('.');
                                        var lookupModule = $filter('filter')($rootScope.modules, { name: fieldParts[1] }, true)[0];

                                        if (!lookupModule)
                                            continue;

                                        field = $filter('filter')(lookupModule.fields, { name: fieldParts[2] }, true)[0];
                                    }
                                    else {
                                        field = $filter('filter')(module.fields, { name: viewField.field }, true)[0]
                                    }

                                    if (field && that.hasFieldDisplayPermission(field))
                                        viewFields.push(viewField);
                                }

                                view.fields = viewFields;

                                if (view.system_type === 'system') {
                                    if (view.filters.length === 1) {
                                        view.label = $filter('translate')('Module.My', { title: module['label_' + $rootScope.language + '_plural'] });
                                    } else if (view.filters.length === 0) {
                                        view.label = $filter('translate')('Module.All', { title: module['label_' + $rootScope.language + '_plural'] });
                                    } else {
                                        view.label = view['label_' + $rootScope.language];
                                    }
                                } else {
                                    view.label = view['label_' + $rootScope.language];
                                }

                                views.push(view);
                            }

                            views = $filter('orderBy')(views, 'label_' + $rootScope.language);
                            deferred.resolve(views);
                        })
                        .catch(function (reason) {
                            deferred.reject(reason.data);
                        });

                    return deferred.promise;
                },

                getPicklist: function (id) {
                    if (id >= 900000) {
                        var deffered = $q.defer();
                        deffered.resolve({ data: { items: [] } });
                        return deffered.promise;
                    }

                    return $http.get(config.apiUrl + 'picklist/get/' + id);
                },

                getView: function (id) {
                    var deferred = $q.defer();
                    $http.get(config.apiUrl + 'view/get/' + id).then(function (view) {
                        deferred.resolve(view.data);
                    }).catch(function (reason) {
                        deferred.reject(reason.data);
                    });

                    return deferred.promise;
                },

                count: function (id) {
                    return $http.get(config.apiUrl + 'view/count/' + id);
                },

                find: function (id, data) {
                    return $http.post(config.apiUrl + 'view/find/' + id, data);
                },

                setViewState: function (viewState, moduleId, id) {
                    if (id)
                        viewState.id = id;

                    viewState.module_id = moduleId;

                    return $http.put(config.apiUrl + 'view/set_view_state', viewState);
                },
                setFilter: function (viewFilters, moduleFields, modulePicklists, filterList) {

                    for (var j = 0; j < viewFilters.length; j++) {
                        var name = viewFilters[j].field;
                        var value = viewFilters[j].value;

                        if (name.indexOf('.') > -1) {
                            name = name.split('.')[0];
                            viewFilters[j].field = name;
                        }

                        var field = $filter('filter')(moduleFields, { name: name }, true)[0];
                        var fieldValue = null;

                        if (!field)
							return filterList;

                        switch (field.data_type) {
                            case 'picklist':
                                fieldValue = $filter('filter')(modulePicklists[field.picklist_id], { labelStr: value }, true)[0];
                                break;
                            case 'multiselect':
                                fieldValue = [];
                                var multiselectValue = value.split('|');

                                angular.forEach(multiselectValue, function (picklistLabel) {
                                    var picklist = $filter('filter')(modulePicklists[field.picklist_id], { labelStr: picklistLabel }, true)[0];

                                    if (picklist)
                                        fieldValue.push(picklist);
                                });
                                break;
                            case 'lookup':
                                if (field.lookup_type === 'users') {
                                    var user = {};

                                    if (value === '0' || value === '[me]') {
                                        user.id = 0;
                                        user.email = '[me]';
                                        user.full_name = $filter('translate')('Common.LoggedInUser');
                                    }
                                    else {
                                        var userItem = $filter('filter')($rootScope.users, { Id: parseInt(value) }, true)[0];
                                        user.id = userItem.Id;
                                        user.email = userItem.Email;
                                        user.full_name = userItem.FullName;

                                        //TODO: $rootScope.users kaldirilinca duzeltilecek
                                        // ModuleService.getRecord('users', value)
                                        //     .then(function (lookupRecord) {
                                        //         fieldValue = [lookupRecord.data];
                                        //     });
                                    }

                                    fieldValue = [user];
                                }
                                else {
                                    fieldValue = value;
                                }
                                break;
                            case 'date':
                            case 'date_time':
                            case 'time':
                                fieldValue = new Date(value);
                                break;
                            case 'checkbox':
                                fieldValue = $filter('filter')(modulePicklists.yes_no, { system_code: value }, true)[0];
                                break;
                            default :
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
                }
            };
        }]);