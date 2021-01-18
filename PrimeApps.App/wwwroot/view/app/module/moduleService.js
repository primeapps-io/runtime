angular.module('primeapps')

    .factory('ModuleService', ['$rootScope', '$http', 'config', '$q', '$filter', '$cache', '$window', 'helper', 'operations', 'icons2', 'dataTypes', 'operators', 'yesNo', 'components',
        function ($rootScope, $http, config, $q, $filter, $cache, $window, helper, operations, icons2, dataTypes, operators, yesNo, components) {
            return {
                myAccount: function () {
                    return $http.post(config.apiUrl + 'User/MyAccount', {});
                },
                getAllUser: function () {
                    return $http.get(config.apiUrl + 'User/get_all');
                },
                getUserLicenseStatus: function () {
                    return $http.post(config.apiUrl + 'License/GetUserLicenseStatus', {});
                },
                addUser: function (user) {
                    return $http.post(config.apiUrl + 'User/add_user', user);
                },
                deleteUser: function (user, instanceId) {
                    return $http.post(config.apiUrl + 'Instance/Dismiss', {
                        UserID: user.ID,
                        EMail: user.email,
                        HasAccount: user.hasAccount,
                        InstanceID: instanceId
                    });
                },
                getUserEmailControl: function (email) {
                    return $http.get(config.apiUrl + 'User/get_user_email_control?Email=' + email);
                },
                get: function (id) {
                    return $http.get(config.apiUrl + 'template/get/' + id);
                },

                create: function (module) {
                    return $http.post(config.apiUrl + 'module/create', module);
                },

                update: function (module, id) {
                    return $http.put(config.apiUrl + 'module/update/' + id, module);
                },

                delete: function (id) {
                    return $http.delete(config.apiUrl + 'module/delete/' + id);
                },
                getRecord: function (module, id, ignoreNotFound) {
                    return $http.get(config.apiUrl + 'record/get/' + module + '/' + id, { ignoreNotFound: ignoreNotFound });
                },
                getRecords: function (module, ids) {
                    return $http.post(config.apiUrl + 'record/get_all_by_id/' + module, ids);
                },
                findCustom: function (local, request) {
                    return $http.post(config.apiUrl + 'record/find_custom?locale=' + local, request);
                },
                findRecords: function (module, request) {
                    $rootScope.activeModuleName = null;
                    return $http.post(config.apiUrl + 'record/find/' + module, request);
                },

                insertRecord: function (module, record) {
                    return $http.post(config.apiUrl + 'record/create/' + module + '?timezone_offset=' + new Date().getTimezoneOffset() * -1, record);
                },
                runMicroflow: function (workflowId, data) {
                    return $http.post(config.apiUrl + 'bpm/start_workflow/' + workflowId, data);
                },

                updateRecord: function (module, record) {

                    //removes process approvel fields
                    delete record.process_id;
                    delete record.process_status;
                    delete record.process_status_order;
                    delete record.operation_type;
                    delete record['process_request_updated_by'];
                    delete record['process_request_updated_at'];
                    delete record.freeze;

                    return $http.put(config.apiUrl + 'record/update/' + module + '?timezone_offset=' + new Date().getTimezoneOffset() * -1, record);
                },

                deleteRecord: function (module, id) {
                    return $http.delete(config.apiUrl + 'record/delete/' + module + '/' + id);
                },

                addRelations: function (module, relatedModule, relations) {
                    return $http.post(config.apiUrl + 'record/add_relations/' + module + '/' + relatedModule, relations);
                },

                deleteRelation: function (module, relatedModule, relation) {
                    return $http({
                        method: 'DELETE',
                        url: config.apiUrl + 'record/delete_relation/' + module + '/' + relatedModule,
                        data: relation,
                        headers: { 'Content-type': 'application/json;charset=utf-8' }
                    });
                },

                insertRecordBulk: function (module, records) {
                    for (var i = 0; i < records.length; i++) {
                        var record = records[i];
                    }
                    return $http.post(config.apiUrl + 'record/create_bulk/' + module, records);
                },

                deleteRecordBulk: function (module, ids) {
                    return $http({
                        method: 'DELETE',
                        url: config.apiUrl + 'record/delete_bulk/' + module,
                        data: ids,
                        headers: { 'Content-type': 'application/json;charset=utf-8' }
                    });
                },

                updateRecordBulk: function (module, request) {
                    delete request.records.shared_users_edit;
                    delete request.records.shared_users;
                    delete request.records.shared_user_groups_edit;
                    delete request.records.shared_user_groups;
                    delete request.records.shared_read;

                    return $http({
                        method: 'Put',
                        url: config.apiUrl + 'record/update_bulk/' + module,
                        data: request,
                        headers: { 'Content-type': 'application/json;charset=utf-8' }
                    });
                },

                findPicklist: function (ids) {
                    return $http.post(config.apiUrl + 'picklist/find', ids);
                },
                deleteView: function (id) {
                    return $http.delete(config.apiUrl + 'view/delete/' + id);
                },
                approveMultipleProcessRequest: function (record_ids, moduleName) {
                    return $http.put(config.apiUrl + 'process_request/approve_multiple_request', {
                        record_ids: record_ids,
                        module_name: moduleName
                    });
                },
                sendSMS: function (moduleId, ids, query, isAllSelected, message, phoneField, template) {
                    return $http.post(config.apiUrl + 'messaging/send_sms', {
                        "module_id": moduleId,
                        "ids": ids,
                        "query": query,
                        "is_all_selected": isAllSelected,
                        "message": message,
                        "phone_field": phoneField,
                        "template_id": template.id
                    });
                },

                sendEMail: function (moduleId, ids, query, isAllSelected, template, emailField, Cc, Bcc, senderAlias, senderEMail, providerType, attachmentContainer, subject, attachmentLink, attachmentName) {
                    return $http.post(config.apiUrl + 'messaging/send_email_job', {
                        "module_id": moduleId,
                        "Ids": ids,
                        "Query": query,
                        "is_all_selected": isAllSelected,
                        "template_id": template.id,
                        "e_mail_field": emailField,
                        "Cc": Cc,
                        "Bcc": Bcc,
                        "sender_alias": senderAlias,
                        "provider_type": providerType,
                        "sender_e_mail": senderEMail,
                        "attachment_container": attachmentContainer,
                        "subject": subject,
                        "attachment_link": attachmentLink,
                        "attachment_name": attachmentName
                    });
                },

                getTemplates: function (moduleName, type) {
                    return $http.get(config.apiUrl + 'template/get_all?moduleName=' + moduleName + '&type=' + type);
                },

                processRecordSingle: function (record, module, picklists) {
                    for (var i = 0; i < module.fields.length; i++) {
                        var field = module.fields[i];
                        this.processRecordField(record, field, picklists);
                    }

                    return record;
                },

                formatRecordFieldValues: function (record, module, picklists) {
                    for (var i = 0; i < module.fields.length; i++) {
                        var field = module.fields[i];
                        this.formatFieldValue(field, record[field.name], picklists, record, module);
                    }
                },

                processRecordMulti: function (records, module, picklists, viewFields, type, parentId, parentType, returnTab, previousParentType, previousParentId, previousReturnTab, parentScope) {
                    var recordsProcessed = [];

                    var setLink = function (field, record, type, parentType, parentId, returnTab, previousParentType, previousParentId, previousReturnTab, parentScope, orjField, isLookup) {
                        var linkPrefix = '#/app/record/';

                        /*
                         * Lookup bir alan external link içeriyorsa record id yerine lookup olan record un id sini basıyoruz.
                         * */
                        if (orjField.external_link) {
                            if (isLookup) {
                                field.link = orjField.external_link + '?id=' + field.value_id + '&back=' + type;
                            } else {
                                field.link = orjField.external_link + '?id=' + record.id + '&back=' + type;
                            }
                            return;
                        }

                        var parentRecord;

                        if (parentScope)
                            parentRecord = parentScope.$parent.record;

                        if (parentScope && !parentRecord)
                            parentRecord = parentScope.$parent.$parent.record;

                        if (parentScope && !parentRecord)
                            parentRecord = parentScope.$parent.$parent.$parent.record;

                        if (!field.primary && field.data_type !== 'email' && field.data_type !== 'url' && field.data_type !== 'lookup' && field.data_type !== 'location')
                            return;

                        if (field.primary && !field.isJoin) {
                            field.link = linkPrefix + type + '?id=' + record.id + (parentId ? ('&ptype=' + parentType + '&pid=' + parentId + '&rtab=' + returnTab + (parentRecord && parentRecord.freeze ? '&freeze=true' : '')) + (previousParentId ? ('&pptype=' + previousParentType + '&ppid=' + previousParentId + '&prtab=' + previousReturnTab) : '') : '');
                            return;
                        } else if (field.primary && field.isJoin && field.value_type !== 'users' && field.value_type !== 'profiles' && field.value_type !== 'roles') {
                            field.link = linkPrefix + field.value_type + '?id=' + field.value_id + (parentId ? ('&ptype=' + parentType + '&pid=' + parentId + (parentRecord && parentRecord.freeze ? '&freeze=true' : '')) : '&back=' + type);
                            return;
                        }

                        if (field.data_type === 'lookup' && field.lookup_type !== 'users' && field.lookup_type !== 'profiles' && field.lookup_type !== 'roles') {
                            var lookupType = field.lookup_type;

                            if (typeof field.lookup_type === 'object')
                                lookupType = field.lookup_type.id;

                            var lookupRecordId = record[field.name + '.' + lookupType + '.id'];
                            field.link = linkPrefix + lookupType + '?id=' + lookupRecordId + (parentId ? ('&ptype=' + parentType + '&pid=' + parentId + '&rtab=' + returnTab + (parentRecord && parentRecord.freeze ? '&freeze=true' : '')) + (previousParentId ? ('&pptype=' + previousParentType + '&ppid=' + previousParentId + '&prtab=' + previousReturnTab) : '') : '&back=' + type);
                            return;
                        }

                        if (field.data_type === 'email') {
                            field.link = 'mailto:' + field.value;
                        }

                        if (field.data_type === 'url')
                            field.link = field.value;

                        if (field.data_type === 'location')
                            field.link = 'http://www.google.com/maps/place/' + field.value;
                    };

                    var setValue = function (field, recordProcessedField, record, recordKey, recordValue) {
                        switch (field.data_type) {
                            case 'text_single':
                                if (field.mask && recordValue) {
                                    recordProcessedField.valueFormatted = $filter('mask')(recordValue, field.mask);
                                } else {
                                    recordProcessedField.value = recordValue;
                                    recordProcessedField.valueFormatted = field.valueFormatted;
                                }
                                break;
                            case 'lookup':
                                if (!record[recordKey])
                                    return;

                                recordProcessedField.value = record[recordKey];

                                if (field.lookup_type !== 'relation')
                                    recordProcessedField.lookup_type = field.lookup_type;
                                else
                                    recordProcessedField.lookup_type = record[field.lookup_relation];

                                recordProcessedField.valueFormatted = recordProcessedField.value;
                                break;
                            case 'picklist':
                            case 'multiselect':
                            case 'tag':
                            case 'checkbox':
                                recordProcessedField.value = field.valueFormatted;
                                recordProcessedField.valueFormatted = field.valueFormatted;
                                break;
                            default:
                                recordProcessedField.value = recordValue;
                                recordProcessedField.valueFormatted = field.valueFormatted;
                                break;
                        }
                    };


                    for (var i = 0; i < records.length; i++) {
                        var record = records[i];

                        var recordProcessed = record;
                        recordProcessed.id = record.id || record[module.name + '_id'];

                        //Approval Process
                        if (record["process.process_requests.process_id"]) {
                            recordProcessed.isProcessItem = true;
                            recordProcessed["process.process_requests.process_status"] = record["process.process_requests.process_status"];
                        } else
                            recordProcessed.isProcessItem = false;

                        //Module list records add advanced sharing info.
                        recordProcessed.shared_users_edit = record['shared_users_edit'];
                        recordProcessed.shared_users = record['shared_users'];
                        recordProcessed.shared_user_groups_edit = record['shared_user_groups_edit'];
                        recordProcessed.shared_user_groups = record['shared_user_groups'];
                        recordProcessed.shared_read = record['shared_read'];

                        recordProcessed.fields = [];

                        for (var j = 0; j < viewFields.length; j++) {
                            var viewField = viewFields[j];

                            var currentModule = angular.copy(module);
                            var currentViewFieldName = viewField.field;
                            var isJoin = false;

                            if (viewField.field.indexOf('.') > -1) {
                                var viewFieldParts = viewField.field.split('.');

                                if (viewFieldParts[3] != null && viewFieldParts[3] === 'primary')
                                    continue;

                                currentModule = $filter('filter')($rootScope.modules, { name: viewFieldParts[1] }, true)[0];
                                currentViewFieldName = viewFieldParts[2];
                                isJoin = true;
                            }

                            for (var k = 0; k < currentModule.fields.length; k++) {
                                var field = currentModule.fields[k];

                                if (currentViewFieldName !== field.name || !this.hasFieldDisplayPermission(field))
                                    continue;

                                if (picklists == null) {
                                    var picklistsValue = this.getPicklists(module);
                                    picklists = picklistsValue.$$state.value;
                                }
                                this.processRecordsField(record, field, true);
                                this.formatFieldValue(field, record[viewField.field], picklists, record, module);

                                var recordProcessedField = {
                                    data_type: field.data_type,
                                    name: field.name,
                                    primary: field.primary,
                                    value: '',
                                    isJoin: isJoin,
                                    isHtml: field.multiline_type_use_html,
                                    image_size_list: field.image_size_list
                                };

                                if (field.data_type === 'rating')
                                    recordProcessedField['max'] = field.validation.min_length;

                                for (var recordKey in record) {
                                    if (record.hasOwnProperty(recordKey)) {
                                        var recordValue = record[recordKey];
                                        var isLookup = false;
                                        if (recordKey.indexOf('.') > -1) {
                                            var recordKeyParts = recordKey.split('.');

                                            if (isJoin && field.primary) {
                                                recordProcessedField.value_id = record[recordKeyParts[0] + '.' + recordKeyParts[1] + '.id'];
                                                recordProcessedField.value_type = recordKeyParts[1];
                                                isLookup = true;
                                            }

                                            recordKey = recordKeyParts[2];
                                        }

                                        if (recordKey !== field.name)
                                            continue;

                                        setValue(field, recordProcessedField, record, recordKey, recordValue);
                                        setLink(recordProcessedField, record, type, parentType, parentId, returnTab, previousParentType, previousParentId, previousReturnTab, parentScope, field, isLookup);
                                    }
                                }

                                recordProcessedField.order = viewField.order || field.order;
                                recordProcessed[field.name + "_data"] = recordProcessedField;
                            }
                        }

                        recordsProcessed.push(recordProcessed);
                    }

                    return recordsProcessed;
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

                                            lookupRecord[keyParts[1]] = keyParts[1] === 'languages' ? JSON.parse(value) : value;
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
                                lookupRecord.primary_value = $rootScope.getLanguageValue(lookupRecord.languages, 'name');
                            } else if (field.lookup_type === 'roles') {
                                lookupRecord.primary_value = $rootScope.getLanguageValue(lookupRecord.languages, 'label');
                            } else if (field.lookup_type === 'relation') {
                                if (record[field.lookup_relation] && record['related_to'])
                                    lookupRecord = record['related_to'];
                                else
                                    lookupRecord = null;
                            } else {
                                var lookupModule = $filter('filter')($rootScope.modules, { name: field.lookup_type }, true)[0];
                                var lookupModulePrimaryField = $filter('filter')(lookupModule.fields, {
                                    primary: true,
                                    deleted: false
                                }, true)[0];
                                lookupRecord.primary_value = lookupRecord[lookupModulePrimaryField.name];
                            }

                            record[fieldName] = lookupRecord;
                            break;
                        case 'picklist':
                            record[fieldName] = $filter('filter')(picklists[field.picklist_id], function (item) {
                                return !item.inactive && that.getPicklistValue(item, record[fieldName])
                            }, true)[0];
                            break;
                        case 'multiselect':
                            var picklistItems = [];

                            if (record[fieldName]) {
                                if (!angular.isArray(record[fieldName])) {
                                    record[fieldName] = record[fieldName].split(';');
                                }

                                for (var i = 0; i < record[fieldName].length; i++) {

                                    const multiselectItem = record[fieldName][i];
                                    const picklistItem = $filter('filter')(picklists[field.picklist_id], function (pickList) {
                                        return !pickList.inactive && that.getPicklistValue(pickList, multiselectItem);
                                    }, true)[0];
                                    //Check if item name exist in picklist ( Item name can be change )
                                    if (picklistItem)
                                        picklistItems.push(picklistItem.id);
                                }
                            }

                            record[fieldName] = picklistItems;
                            break;
                        case 'date':
                        case 'date_time':
                        case 'time':
                            if (record[fieldName] === undefined || record[fieldName] === null || !record[fieldName].length)
                                return;

                            record[fieldName + 'str'] = field.data_type === 'time' ? kendo.toString(new Date(record[fieldName]), 't') : kendo.toString(new Date(record[fieldName]), 'g');
                            break;
                        case 'tag':
                            if (record[fieldName] === undefined || record[fieldName] === null || !record[fieldName].length)
                                record[fieldName] = [];
                            break;
                    }
                },

                processRecordsField: function (record, field) {
                    var fieldName = field.name;

                    switch (field.data_type) {
                        case 'lookup':
                            var lookupModule = $filter('filter')($rootScope.modules, { name: field.lookup_type }, true)[0];

                            if (lookupModule === null || lookupModule === undefined)
                                return;

                            var lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];
                            record[fieldName] = record[field.name + '.' + field.lookup_type + '.' + lookupModulePrimaryField.name + '.primary'];
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

                processUser: function (user) {
                    var lookupUser = {};
                    lookupUser.id = user.id;
                    lookupUser.primary_value = user.first_name + ' ' + user.last_name;
                    lookupUser.email = user.email;

                    return lookupUser;
                },

                processRecords: function (records, module, picklists) {
                    var recordsProcessed = [];

                    var recordsCopy = angular.copy(records);

                    for (var i = 0; i < recordsCopy.length; i++) {
                        var record = recordsCopy[i];

                        for (var j = 0; j < module.fields.length; j++) {
                            var field = module.fields[j];

                            this.processRecordsField(record, field, picklists);
                            this.formatFieldValue(field, record[field.name], picklists, record, module);
                        }

                        recordsProcessed.push(record);
                    }

                    return recordsProcessed;
                },

                formatFieldValue: function (field, value, picklists, record, module) {
                    field.valueFormatted = '';

                    if (value === undefined || value === null)
                        return;

                    switch (field.data_type) {
                        case 'number_decimal':
                            field.valueFormatted = $filter('number')(value, field.decimal_places);
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

                            field.valueFormatted = $filter('currency')(value, recordCurrencySymbol || field.currency_symbol || $rootScope.currencySymbol, field.decimal_places);
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
                                field.valueFormatted = picklistItem ? $rootScope.getLanguageValue(picklistItem.languages, 'label') : value;
                            } else {
                                field.valueFormatted = $rootScope.getLanguageValue(value.languages, 'label');
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
                            const res = $filter('filter')(picklists['yes_no'], { system_code: value.toString() })[0];
                            if (res)
                                field.valueFormatted = $rootScope.getLanguageValue(res.languages, 'label');
                            break;
                        case 'lookup':
                            field.valueFormatted = angular.isObject(value) ? value[field.lookupModulePrimaryField.name] : value;
                            break;
                        default:
                            field.valueFormatted = value;
                            break;
                    }
                },

                getPicklists: function (module, withRelatedPicklists) {
                    var deferred = $q.defer();
                    var fields = angular.copy(module.fields);
                    var picklists = {};
                    var picklistIds = [];

                    if (withRelatedPicklists) {
                        for (var i = 0; i < module.fields.length; i++) {
                            var field = module.fields[i];

                            if (field.data_type === 'lookup' && field.lookup_type !== 'users' && field.lookup_type !== 'profiles' && field.lookup_type !== 'roles' && field.lookup_type !== 'relation') {
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

                                for (var l = 0; l < $rootScope.modules.length; l++) {
                                    var moduleItem = $rootScope.modules[l];

                                    if (!moduleItem.display)
                                        continue;

                                    if (!helper.hasPermission(moduleItem.name, operations.read))
                                        continue;

                                    var modulePicklistItem = {};
                                    modulePicklistItem.id = parseInt(moduleItem.id) + 900000;
                                    modulePicklistItem.type = 900000;
                                    modulePicklistItem.system_code = moduleItem.name;
                                    modulePicklistItem.order = moduleItem.order;
                                    modulePicklistItem.label = $rootScope.getLanguageValue(moduleItem.languages, "label", "singular");
                                    //modulePicklistItem.label = {};
                                    // modulePicklistItem.label.en = moduleItem.label_en_singular;
                                    // modulePicklistItem.label.tr = moduleItem.label_tr_singular;
                                    modulePicklistItem.labelStr = $rootScope.getLanguageValue(moduleItem.languages, "label", "singular");
                                    modulePicklistItem.value = moduleItem.name;
                                    modulePicklistItem.languages = {};
                                    modulePicklistItem.languages[$rootScope.globalization.Label] = {
                                        'label': $rootScope.getLanguageValue(moduleItem.languages, 'label')
                                    };
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
                                picklistCache = $filter('orderBy')(picklistCache, 'label');

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

                    var yesNoPicklistCache = $cache.get('picklist_yes_no');

                    if (yesNoPicklistCache)
                        picklists['yes_no'] = yesNoPicklistCache;
                    else {
                        picklists['yes_no'] = $rootScope.yesNo;
                    }

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

                                $rootScope.processLanguages(response.data);
                                var picklistItems = helper.mergePicklists(response.data);
                                picklists[field.picklist_id] = $filter('filter')(picklistItems, { type: field.picklist_id }, true);
                                picklists[field.picklist_id] = $filter('orderBy')(picklists[field.picklist_id], 'label');

                                if (field.picklist_sortorder && !field.deleted)
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
                },

                moduleFieldsConvertByKey: function (fields) {
                    var newFields = [];
                    for (var i = 0; i < fields.length; i++) {
                        newFields[fields[i].name] = fields[i]
                    }
                    return newFields;
                },

                lookup: function (searchTerm, field, record, additionalFields, exactMatch, customFilters) {
                    var deferred = $q.defer();
                    var lookupType = field.lookup_type;
                    var that = this;
                    var isDropdownField = field.data_type === 'lookup' && field.show_as_dropdown;
                    if (field.lookupModulePrimaryField.data_type !== 'text_single' && field.lookupModulePrimaryField.data_type !== 'picklist' && field.lookupModulePrimaryField.data_type != 'email' &&
                        field.lookupModulePrimaryField.data_type !== 'number' && field.lookupModulePrimaryField.data_type !== 'number_auto') {
                        deferred.resolve([]);
                        return deferred.promise;
                    }

                    if (lookupType === 'relation')
                        lookupType = record[field.lookup_relation] !== undefined ? record[field.lookup_relation].value : null;

                    if (!lookupType) {
                        deferred.resolve([]);
                        return deferred.promise;
                    }

                    if (!searchTerm && !isDropdownField) {
                        deferred.resolve([]);
                        return deferred.promise;
                    }

                    var lookupModule = $filter('filter')($rootScope.modules, { name: lookupType }, true)[0];
                    var selectedFields = [];
                    selectedFields.push(field.lookupModulePrimaryField.name);

                    if (additionalFields) {
                        for (var i = 0; i < additionalFields.length; i++) {
                            var additionalField = additionalFields[i];

                            if (additionalField !== field.lookupModulePrimaryField.name && additionalField !== 'id')
                                selectedFields.push(additionalField)
                        }
                    }

                    var filters = [];

                    if (field.lookupModulePrimaryField.data_type !== 'number' && field.lookupModulePrimaryField.data_type !== 'number_auto') {
                        if (!exactMatch)
                            switch (field.lookup_search_type) {
                                case 'contains':
                                    filters.push({
                                        field: field.lookupModulePrimaryField.name,
                                        operator: 'contains',
                                        value: searchTerm,
                                        no: 1
                                    });
                                    break;
                                case 'starts_with':
                                    filters.push({
                                        field: field.lookupModulePrimaryField.name,
                                        operator: 'starts_with',
                                        value: searchTerm,
                                        no: 1
                                    });
                                    break;
                                default:
                                    filters.push({
                                        field: field.lookupModulePrimaryField.name,
                                        operator: 'starts_with',
                                        value: searchTerm,
                                        no: 1
                                    });
                                    break;
                            }
                        else
                            filters.push({
                                field: field.lookupModulePrimaryField.name,
                                operator: 'is',
                                value: searchTerm,
                                no: 1
                            });
                    } else {
                        filters.push({
                            field: field.lookupModulePrimaryField.name,
                            operator: 'equals',
                            value: parseInt(searchTerm),
                            no: 1
                        });
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
                        findRequest.limit = 100000;
                    }

                    //get only active users to list! if need also inactive users, use utils lookupuser with includeInactiveUsers parameter
                    var filterOrderNo = findRequest.filters.length;
                    if (lookupModule.name === 'users' || ($rootScope.branchAvailable && lookupType === 'branches')) {
                        findRequest.filters.push({
                            field: 'is_active',
                            operator: 'equals',
                            value: true,
                            no: filterOrderNo + 1
                        });
                    }

                    if (lookupModule.name === 'users') {
                        findRequest.filters.push({
                            field: 'email',
                            operator: 'not_contain',
                            value: "integration_",
                            no: filterOrderNo + 1
                        });
                    }

                    var lookupModuleFields = that.moduleFieldsConvertByKey(lookupModule.fields);//lookup field filters (from field_filters table)
                    if (field.filters) {
                        var no = findRequest.filters.length;
                        for (var z = 0; z < field.filters.length; z++) {
                            var filter = field.filters[z];
                            no++;
                            var findRecordValue = null;
                            var filterMatch = filter.value.match(/^\W+(.+)]/i);
                            if (filterMatch !== null && field.lookup_type !== 'users' && field.lookup_type !== 'profiles' && field.lookup_type !== 'roles') {
                                var recordMatch = filterMatch[1].split('.');


                                if (recordMatch.length === 1 && record[recordMatch[0]])
                                    findRecordValue = record[recordMatch[0]];

                                if (recordMatch.length === 2 && record[recordMatch[0]])
                                    findRecordValue = record[recordMatch[0]][recordMatch[1]];

                                if (recordMatch.length === 3 && record[recordMatch[0]])
                                    findRecordValue = record[recordMatch[0]][recordMatch[1]][recordMatch[2]];

                                if (findRecordValue != null) {
                                    findRequest.filters.push({
                                        field: filter.filter_field,
                                        operator: filter.operator,
                                        value: findRecordValue,
                                        no: no
                                    });
                                    findRequest.fields.push(filter.filter_field);
                                }

                            } else {
                                var filterField = lookupModuleFields[filter.filter_field];

                                switch (filterField.data_type) {
                                    case "multiselect":
                                    case "tag":
                                        findRecordValue = filter.value.split("|");
                                        break;
                                    default:
                                        findRecordValue = filter.value;
                                        break;

                                }
                                findRequest.filters.push({
                                    field: filter.filter_field,
                                    operator: filter.operator,
                                    value: findRecordValue,
                                    no: no
                                });
                                findRequest.fields.push(filter.filter_field);
                            }

                        }
                    }

                    if (customFilters) {
                        for (var j = 0; j < customFilters.length; j++) {
                            var customFilter = customFilters[j];
                            customFilter.no = findRequest.filters.length + 1;

                            findRequest.filters.push(customFilter);
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
                                    if (lookupType === 'profiles' && recordItem['id'] === 1) {
                                        recordItem['name'] = $rootScope.user.tenant_language === 'tr' ? 'Sistem Yöneticisi' : 'Administrator';
                                    }
                                    var lookupRecord = angular.copy(recordItem);

                                    lookupRecord.primary_value = recordItem[field.lookupModulePrimaryField.name];
                                    lookupRecords.push(lookupRecord);
                                    deferred.resolve(lookupRecords);
                                }
                            } else {
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

                fieldLookupFilters: function (field, record, filters, findRequest) {

                    var kendoOperators = {};
                    kendoOperators = {
                        is: 'is',
                        is_not: 'is_not',
                        equals: 'eq',
                        not_equal: 'neq',
                        contains: 'contains',
                        not_contain: 'doesnotcontain',
                        starts_with: 'startswith',
                        ends_with: 'endswith',
                        empty: 'isempty',
                        not_empty: 'isnotempty',
                        greater: 'gt',
                        greater_equal: 'gte',
                        less: 'lt',
                        less_equal: 'lte',
                        not_in: 'not_in'
                    };

                    var processFilters = [];
                    var lookupType = field.lookup_type;
                    var lookupModule = $rootScope.modulus[lookupType];
                    var lookupModuleFields = this.moduleFieldsConvertByKey(lookupModule.fields);

                    if (!findRequest.fields)
                        findRequest.fields = [];

                    if (filters && field.filters && field.filters.length > 0) {
                        var no = 0;

                        for (var z = 0; z < field.filters.length; z++) {
                            var filter = field.filters[z];
                            var fltr = $filter('filter')(filters, { field: filter['filter_field'], operator: filter['operator'], value: filter['value'] }, true)[0];

                            if (fltr) {
                                var index = filters.indexOf(fltr);
                                filters.splice(index, 1);
                            }
                            no++;
                            var findRecordValue = filter.value;
                            var filterMatch = filter.value.match(/^\W+(.+)]/i);

                            if (filterMatch !== null && field.lookup_type !== 'users' && field.lookup_type !== 'profiles' && field.lookup_type !== 'roles') {
                                var recordMatch = filterMatch[1].split('.');

                                if (recordMatch.length === 1 && record[recordMatch[0]])
                                    findRecordValue = record[recordMatch[0]];

                                else if (recordMatch.length === 2 && record[recordMatch[0]])
                                    findRecordValue = record[recordMatch[0]][recordMatch[1]];

                                else if (recordMatch.length === 3 && record[recordMatch[0]])
                                    findRecordValue = record[recordMatch[0]][recordMatch[1]][recordMatch[2]];

                                else if (findRecordValue.startsWith("[") && findRecordValue.endsWith("]"))
                                    findRecordValue = null;

                                if (findRecordValue != null) {
                                    processFilters.push({
                                        field: filter.filter_field,//recordMatch[0],
                                        operator: kendoOperators[filter.operator] || filter.operator,
                                        value: findRecordValue,
                                        no: no
                                    });
                                    findRequest.fields.push(filter.filter_field);
                                }

                            } else {

                                var filterField = lookupModuleFields[filter.filter_field];
                                if (filterField) {
                                    switch (filterField.data_type) {
                                        case "multiselect":
                                        case "tag":
                                            findRecordValue = filter.value.split("|");
                                            break;
                                        default:
                                            findRecordValue = filter.value;
                                            break;

                                    }
                                }
                                processFilters.push({
                                    field: filter.filter_field,
                                    operator: kendoOperators[filter.operator] || filter.operator,
                                    value: findRecordValue,
                                    no: no
                                });
                                findRequest.fields.push(filter.filter_field);
                            }
                        }

                        if (filters.length > 0) {
                            angular.forEach(filters, function (f) {
                                f.operator = kendoOperators[f.operator];
                            });

                            processFilters = processFilters.concat(filters);
                        }

                        return processFilters;
                    } else {
                        return filters;
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
                                const field = $filter('filter')(module.fields, { name: dependency.parent_field, deleted: false }, true)[0];
                                if (!angular.equals(record[dependency.parent_field], currentRecord[dependency.parent_field])) {
                                    if (dependency.values_array && dependency.values_array.length > 0) {
                                        var empty = true;
                                        for (var j = 0; j < dependency.values_array.length; j++) {
                                            var value = dependency.values_array[j];
                                            if (Array.isArray(record[dependency.parent_field])) {
                                                for (var k = 0; k < record[dependency.parent_field].length; k++) {
                                                    var multiValue = record[dependency.parent_field][k];
                                                    if (multiValue.toString() === value) {
                                                        empty = false;
                                                    }
                                                }
                                            } else if (record[dependency.parent_field] && ((angular.isObject(record[dependency.parent_field]) && record[dependency.parent_field].id.toString() === value) || (field && field.data_type === "checkbox"))) {
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

                        if (!currentRecord && !newRecord[field.name] && newRecord[field.name] !== false && newRecord[field.name] !== 0)
                            continue;

                        if (currentRecord && !currentRecord[field.name] && currentRecord[field.name] !== false && currentRecord[field.name] !== 0 && !newRecord[field.name]) {
                            delete newRecord[field.name];
                            continue;
                        }

                        if (field.data_type === 'checkbox' && newRecord[field.name] === null && currentRecord[field.name])
                            newRecord[field.name] = false;

                        if (field.deleted) {
                            delete newRecord[field.name];
                            continue;
                        }
                        if (field.data_type === 'tag' && ((angular.isArray(newRecord[field.name]) && newRecord[field.name].length < 1) || newRecord[field.name] === null) && (currentRecord ? ((angular.isArray(currentRecord[field.name]) && currentRecord[field.name].length < 1) || currentRecord[field.name] === null) : false)) {
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
                                    if (newRecord[field.name] === null || newRecord[field.name] === undefined) {
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

                                    function htmltext(html) {
                                        var tag = document.createElement('div');
                                        tag.innerHTML = html;

                                        return tag.innerHTML.toString();
                                    }

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
                                        //ids.push(item.id);
                                        ids.push(item);
                                    }

                                    if (newCurrentRecord[field.name]) {
                                        for (var k = 0; k < newCurrentRecord[field.name].length; k++) {
                                            var picklistItem = newCurrentRecord[field.name][k];
                                            //currentIds.push(picklistItem.id);
                                            currentIds.push(picklistItem);
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
                                case 'tag':
                                    newRecord[field.name] = newRecord[field.name].toString();//tags.toString();
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

                prepareRecordBulk: function (records, module) {
                    var that = this;
                    var recordsBulk = [];

                    for (var i = 0; i < records.length; i++) {
                        var record = records[i];
                        var newRecord = that.prepareRecord(record, module);
                        recordsBulk.push(newRecord);
                    }

                    return recordsBulk;
                },

                calculate: function (field, module, record) {
                    if (!module.calculations)
                        return;

                    var calculation = $filter('filter')(module.calculations, { field_1: field.name }, true)[0];

                    if (!calculation)
                        calculation = $filter('filter')(module.calculations, { field_2: field.name }, true)[0];

                    if (!calculation)
                        return;

                    var calculations = $filter('filter')(module.calculations, { result_field: calculation.result_field }, true);
                    calculations = $filter('orderBy')(calculations, 'order');
                    var result = 0;
                    var resultField = '';

                    for (var i = 0; i < calculations.length; i++) {
                        var calculation = calculations[i];
                        result = eval((result || record[calculation.field_1] || 0) + calculation.operator + (record[calculation.field_2] || calculation.custom_value || 0));
                        resultField = calculation.result_field;
                    }

                    record[resultField] = result || 0;

                },

                setExpressionValueElementType: function (result, element, record) {
                    // if (result === null || result === undefined || typeof result === 'string' || typeof result === 'number')
                    //     return result;
                    switch (element.data_type) {
                        case 'picklist':
                            if (!result)
                                break
                            result = result.labelStr;
                            break;
                        case 'multiselect':
                            if (!result)
                                break
                            var field = $filter('filter')($rootScope.moduleRecordData.module.fields, { name: element.name }, true)[0];
                            var currentPicklist = $rootScope.moduleRecordData.picklist[field.picklist_id];
                            var resData = "";
                            var resutlLength = result.length;
                            for (var i = 0; i < resutlLength; i++) {
                                var value = $filter('filter')(currentPicklist, { id: result[i] }, true)[0];
                                resData += $rootScope.getLanguageValue(value.languages, 'label')
                                if (i + 1 !== resutlLength)
                                    resData += ", ";
                            }
                            result = resData;
                            break;
                        case 'date':
                        case 'date_time':
                        case 'time':
                            result = record[element.name + "str"]
                            break;
                    }
                    return result;
                },

                expressionGetValue: function (record, resultArr, element, isValidate) {
                    var expressionResult = '';
                    //Valua ve Validasyon json dataları üzerinden her obje tibine göre data ilgili data dolumları yapılır.
                    switch (element.type) {
                        case 'field':
                            if (element.is_main === true && element.isLookup === true) {//main lookup field data
                                var arrayOfLookup = element.name.split('.');
                                if (!arrayOfLookup) return;
                                var elementName = arrayOfLookup[0];
                                var elementMod = arrayOfLookup[1];
                                var primaryLookupField = arrayOfLookup[arrayOfLookup.length - 1];

                                if (elementName === 'created_by') {
                                    var result = record.created_by ? record.created_by.full_name : record.owner.full_name;
                                    expressionResult += "\"" + result + "\"";
                                } else if (elementName === 'updated_by') {
                                    var result = record.updated_by ? record.updated_by.full_name : record.owner.full_name;
                                    expressionResult += "\"" + result + "\"";
                                } else if (elementName === 'owner') {
                                    expressionResult += "\"" + record.owner.full_name + "\"";
                                } else {
                                    if (record[elementName] && record[elementName].id) {
                                        var data = '';
                                        if (elementMod !== 'users' && elementMod !== 'roles' && elementMod !== 'profiles') {
                                            data = record[elementName][primaryLookupField];
                                        } else if (elementMod === 'users') {
                                            data = record[elementName].full_name;
                                        } else {
                                            //profiles & role
                                            if (record[elementName]["languages"]) {
                                                data = $rootScope.getLanguageValue(record[element.name].languages, 'label') || $rootScope.getLanguageValue(record[element.name].languages, 'name');
                                            }
                                        }

                                        if (typeof data === 'number')
                                            expressionResult += data;
                                        else
                                            expressionResult += "\"" + data + "\"";
                                    } else if (record[element.name] && record[element.name].id) {
                                        if (elementMod !== 'users' && elementMod !== 'roles' && elementMod !== 'profiles') {
                                            //Bura kontrol edilecek gelen element.name'in değerine göre parse edilecek
                                            data = record[element.name].name;
                                        } else if (elementMod === 'users')
                                            data = record[element.name].full_name;
                                        else {
                                            //profiles & role
                                            if (record[element.name]["languages"]) {
                                                data = $rootScope.getLanguageValue(record[element.name].languages, 'label') || $rootScope.getLanguageValue(record[element.name].languages, 'name');
                                            }
                                        }

                                        if (typeof data === 'number')
                                            expressionResult += data;
                                        else
                                            expressionResult += "\"" + data + "\"";
                                    } else
                                        expressionResult += "\" \"";
                                }
                            } else if (element.is_main === true && element.isLookup === false) {//main field data
                                var result = record[element.name];

                                switch (resultArr.data_type) {
                                    case 'text_single':
                                    case 'text_multi':
                                        result = this.setExpressionValueElementType(result, element, record);
                                        if (result === undefined || result === null || result.toString().trim() === '')
                                            result = '';
                                        expressionResult += "\"" + result + "\"";
                                        break;
                                    case 'number':
                                        result = this.setExpressionValueElementType(result, element, record);
                                        switch (isValidate) {
                                            case true:
                                                if (result === null || isNaN(result))
                                                    result = null;
                                                else {
                                                    result = Math.round(result);
                                                    if (result < 0)
                                                        result = "(" + result + ")";
                                                }
                                                expressionResult += result;
                                                break;

                                            default:
                                                if (result === '' || result === null || result === undefined || isNaN(result))
                                                    result = null
                                                else {
                                                    result = Math.round(result);
                                                    if (result < 0)
                                                        result = "(" + result + ")";
                                                }
                                                expressionResult += result;
                                                break;
                                        }
                                        break;
                                    case 'number_decimal':
                                    case 'currency':
                                        result = this.setExpressionValueElementType(result, element, record)
                                        switch (isValidate) {
                                            case true:
                                                if (result === null || isNaN(result))
                                                    result = null;
                                                else {
                                                    if (result < 0)
                                                        result = "(" + result + ")";
                                                }
                                                expressionResult += result;
                                                break;

                                            default:
                                                if (result === '' || result === null || result === undefined || isNaN(result))
                                                    result = null;
                                                else {
                                                    if (result < 0)
                                                        result = "(" + result + ")";
                                                }
                                                expressionResult += result;
                                                break;
                                        }
                                        break;
                                }
                            } else if (element.is_main === false && element.isLookup === true) {//lookup field data
                                var lookupName = element.name.split('.');

                                var result = '';
                                if (record[lookupName[0]]) {
                                    if (record[lookupName[0]][lookupName[1]])
                                        result = record[lookupName[0]][lookupName[1]];
                                    else if (record[lookupName[0]][lookupName[2]])
                                        result = record[lookupName[0]][lookupName[2]];
                                } else if ($rootScope.user[lookupName[2]])
                                    result = $rootScope.user[lookupName[2]];

                                switch (resultArr.data_type) {
                                    case 'text_single':
                                    case 'text_multi':
                                        if (result === undefined || result === null || result.trim() === '')
                                            result = '';
                                        expressionResult += "\"" + result + "\""
                                        break;
                                    case 'number':
                                        switch (isValidate) {
                                            case true:
                                                if (result === null || isNaN(result))
                                                    result = null;
                                                else {
                                                    result = Math.round(result);
                                                    if (result < 0)
                                                        result = "(" + result + ")";
                                                }
                                                expressionResult += result;
                                                break;

                                            default:
                                                if (result === '' || result === null || result === undefined || isNaN(result))
                                                    result = null;
                                                else {
                                                    result = Math.round(result);
                                                    if (result < 0)
                                                        result = "(" + result + ")";
                                                }
                                                expressionResult += result;
                                                break;
                                        }
                                        break;
                                    case 'number_decimal':
                                    case 'currency':
                                        switch (isValidate) {
                                            case true:
                                                if (result === null || isNaN(result))
                                                    result = null;
                                                else {
                                                    if (result < 0)
                                                        result = "(" + result + ")";
                                                }
                                                expressionResult += result;
                                                break;

                                            default:
                                                if (result === '' || result === null || result === undefined || isNaN(result))
                                                    result = null;
                                                else {
                                                    if (result < 0)
                                                        result = "(" + result + ")";
                                                }
                                                expressionResult += result
                                                break;
                                        }
                                        break;
                                }
                            }
                            break;
                        case 'operator':
                            if (element.forTypeName === 'length')
                                expressionResult += ").length";
                            else
                                expressionResult += element.value
                            break;
                        case 'function':
                            if (element.name === 'and' || element.name === 'or')
                                expressionResult += element.value
                            else
                                expressionResult += '('
                            break;
                        case 'input':
                            if(resultArr.data_type === "number" || resultArr.data_type === "number_decimal" || resultArr.data_type === "currency" || resultArr.data_type === "number_auto"){
                                var parseIntData = parseInt(element.dataValue);
                                if (!isNaN(parseIntData)) {
                                    expressionResult += element.dataValue;
                                } else {
                                    expressionResult += null;
                                }
                            }else{
                                var parseIntData = parseInt(element.dataValue);
                                if (!isNaN(parseIntData)) {
                                    expressionResult += element.dataValue;
                                } else {
                                    expressionResult += "\"" + element.dataValue + "\"";
                                }
                            }
                            break;
                    }
                    return expressionResult;
                },

                expressionRunOrder: function (fields, record, isValidate) {//
                    var fieldLastList = [];
                    for (var g = 0; g < fields.length; g++) {
                        var formulaJson = null;
                        if (isValidate)
                            formulaJson = fields[g].validation && fields[g].validation.custom ? JSON.parse(fields[g].validation.custom) : null;
                        else {
                            try {
                                if (isNaN(fields[g].default_value) && fields[g].default_value)
                                    formulaJson = fields[g].default_value ? JSON.parse(fields[g].default_value) : null;
                                else {
                                    record[fields[g].name] = record[fields[g].name] ? record[fields[g].name] : fields[g].default_value;
                                    continue;
                                }
                            } catch (e) {
                                record[fields[g].name] = record[fields[g].name] ? record[fields[g].name] : fields[g].default_value;
                                continue;
                            }
                        }

                        var data = {
                            label: fields[g].name,
                            fields: [],
                            formulaJson: formulaJson,
                            data_type: fields[g].data_type,
                            isRun: false,
                            isConstant: false,
                            treeIndex: []
                        }
                        if (formulaJson === null || formulaJson.length === 0)
                            continue;

                        //Value , Validasyon tipine göre runtime için objeleri oluştururlur.
                        if (isValidate) {
                            for (var n = 0; n < formulaJson.length; n++) {
                                for (var i = 0; i < formulaJson[n].elements.length; i++) {
                                    data.validationMsg = $rootScope.getLanguageValue(formulaJson[n].languages, 'label');
                                    if (formulaJson[n].elements[i].type === 'field') {
                                        var obj = {
                                            label: formulaJson[n].elements[i].label,
                                            name: formulaJson[n].elements[i].name,
                                            isLookup: formulaJson[n].elements[i].isLookup
                                        }
                                        data.fields.push(obj);
                                    }
                                }
                            }
                        } else {
                            for (var e = 0; e < formulaJson.elements.length; e++) {
                                if (formulaJson.elements[e].type === 'field') {
                                    var obj2 = {
                                        label: formulaJson.elements[e].label,
                                        name: formulaJson.elements[e].name,
                                        isLookup: formulaJson.elements[e].isLookup
                                    }
                                    data.fields.push(obj2);
                                }
                            }
                        }
                        if (data.fields.length === 0)
                            data.isConstant = true;

                        fieldLastList.push(data);

                    }

                    var firstData = [];
                    var lastData = [];

                    //Sadece Constant değerine sahip olanları ayrılır.
                    fieldLastList.forEach(function (value) {
                        if (value.fields.length === 0) {
                            firstData.push(value);
                        } else
                            lastData.push(value);
                    })

                    var firstDataLenght = firstData.length;
                    if (firstDataLenght > 0)
                        firstDataLenght -= 1;
                    for (var v = 0; v < lastData.length; v++) {
                        var firstElement = lastData[v];

                        for (var c = 0; c < lastData.length; c++) {
                            if (v === c)
                                continue;

                            var secondElement = lastData[c];
                            var fieldsIncludes = $filter('filter')(lastData[v].fields, { name: lastData[c].label }, true)

                            //Fieldlerde validasyon ve value için hangi işlemin daha önce yapılacağına göre sıralama yapar.
                            if (fieldsIncludes.length > 0) {
                                var firstIndex = lastData.indexOf(firstElement);
                                var secondIndex = lastData.indexOf(secondElement);
                                if (firstIndex < secondIndex) {
                                    if (!secondElement.treeIndex.includes(firstElement.label))
                                        secondElement.treeIndex.push(firstElement.label);
                                    lastData[secondIndex] = firstElement;
                                    lastData[firstIndex] = secondElement;
                                } else {
                                    if (!secondElement.treeIndex.includes(firstElement.label))
                                        secondElement.treeIndex.push(firstElement.label);
                                }

                            }
                        }
                    }

                    var resultArr = firstData.concat(lastData);

                    return resultArr;
                },

                exprressionDependencyControl: function (module, resultArr) {
                    var activeField = $filter('filter')(module.fields, {
                        name: resultArr.label,
                        deleted: false
                    }, true)[0];
                    if (!activeField || activeField.hidden !== undefined && activeField.hidden === true ||
                        activeField.sectionObj.hidden !== undefined && activeField.sectionObj.hidden === true)
                        return true //hide
                    return false
                },

                expressionRunValue: function (resultArr, record, currentField, module) {
                    //oluşturulan runOrder sırasına göre çalıştırır.

                    var dependenciesFields = [];
                    if (currentField) {
                        dependenciesFields = $filter('filter')(module.dependencies, {
                            parent_field: currentField.name,
                            deleted: false
                        }, true);
                    }

                    if (!currentField) {
                        for (var j = 0; j < resultArr.length; j++) {
                            var expressionResult = '';
                            resultArr[j].isRun = true

                            for (var r = 0; r < resultArr[j].formulaJson.elements.length; r++) {
                                expressionResult += this.expressionGetValue(record, resultArr[j], resultArr[j].formulaJson.elements[r], false)
                            }

                            try {
                                if (!record[resultArr[j].label])
                                    this.setHideFieldValue(resultArr[j].label, expressionResult)

                                record[resultArr[j].label] = eval(expressionResult);
                            } catch (e) {
                                record[resultArr[j].label] = '';
                            }
                        }
                    } else if (currentField && currentField.data_type === 'lookup') {
                        var runLookup = $filter('filter')(resultArr, {
                            fields: {
                                name: currentField.lookup_type,
                                isLookup: true
                            }
                        });
                        var addRunLookup = [];
                        if (runLookup && runLookup.length > 0) {
                            runLookup.forEach(function (item) {
                                if (item.treeIndex.length > 0) {
                                    var addLookup = $filter('filter')(resultArr, { label: item.treeIndex[0] }, true)[0];
                                    addRunLookup.push(addLookup);
                                }
                            })
                        }

                        var lastRunLookup = addRunLookup.length > 0 ? runLookup.concat(addRunLookup) : runLookup;
                        for (var j = 0; j < lastRunLookup.length; j++) {
                            var expressionResultLookup = '';
                            for (var r = 0; r < lastRunLookup[j].formulaJson.elements.length; r++) {
                                expressionResultLookup += this.expressionGetValue(record, lastRunLookup[j], lastRunLookup[j].formulaJson.elements[r], false)
                            }
                            try {
                                if (!record[lastRunLookup[j].label])
                                    this.setHideFieldValue(lastRunLookup[j].label, expressionResultLookup)

                                record[lastRunLookup[j].label] = eval(expressionResultLookup);
                            } catch (e) {
                                record[lastRunLookup[j].label] = '';
                            }
                        }

                    } else if (currentField && dependenciesFields.length > 0) {//fieldlardaki dependency(display için) durumuna göre yeniden hesaplama.
                        var then = this;
                        dependenciesFields.forEach(function (item) {
                            var runDep1 = $filter('filter')(resultArr, { label: item.child_field }, true);
                            var runDep2 = $filter('filter')(resultArr, { fields: { name: item.child_field } }, true);
                            var allRunDep = runDep1.concat(runDep2);
                            var that = this;
                            for (var j = 0; j < allRunDep.length; j++) {
                                var expressionResult = '';
                                for (var r = 0; r < allRunDep[j].formulaJson.elements.length; r++) {
                                    expressionResult += then.expressionGetValue(record, allRunDep[j], allRunDep[j].formulaJson.elements[r], false)
                                }
                                try {
                                    if (!record[allRunDep[j].label])
                                        that.setHideFieldValue(allRunDep[j].label, expressionResult)

                                    record[allRunDep[j].label] = eval(expressionResult);
                                } catch (e) {
                                    record[allRunDep[j].label] = '';
                                }
                            }
                        })
                    } else {
                        //field üstünde değişiklik yaptığımızda sadece o field objesindeki treeIndex alanındaki expressionlar tekrar çalıştırılır Recursive olarak.
                        var resultLastArr = $filter('filter')(resultArr, { fields: { name: currentField.name } }, true);
                        var addRunAnyExp = [];
                        if (resultLastArr && resultLastArr.length > 0) {
                            resultLastArr.forEach(function (item) {
                                if (item.treeIndex.length > 0) {
                                    var addAnyExp = $filter('filter')(resultArr, { label: item.treeIndex[0] }, true)[0];
                                    addRunAnyExp.push(addAnyExp);
                                }
                            })
                        }

                        var resultLastArrRun = addRunAnyExp.length > 0 ? resultLastArr.concat(addRunAnyExp) : resultLastArr;
                        this.expressionRecursiveRunValue(resultLastArrRun, resultArr, record, currentField, module.fields, false);
                    }
                },

                expressionRecursiveRunValue: function (resultLastArr, resultArr, record, currentField, moduleFields, validate) {
                    for (var j = 0; j < resultLastArr.length; j++) {
                        var expressionResult = '';
                        resultLastArr[j].isRun = true

                        for (var r = 0; r < resultLastArr[j].formulaJson.elements.length; r++) {
                            expressionResult += this.expressionGetValue(record, resultLastArr[j], resultLastArr[j].formulaJson.elements[r])
                        }

                        try {
                            if (!record[resultLastArr[j].label])
                                this.setHideFieldValue(resultLastArr[j].label, expressionResult)

                            record[resultLastArr[j].label] = eval(expressionResult);
                        } catch (e) {
                            record[resultLastArr[j].label] = '';
                        }
                        //tree fields
                        var resultTreeExp = $filter('filter')(resultArr, { label: resultLastArr[j].treeIndex[0] }, true)[0];
                        if (resultLastArr[j].treeIndex.length > 0) {
                            this.expressionRecursiveRunValue(resultTreeExp, resultArr, record, currentField, moduleFields, validate)
                        }
                    }
                },

                setHideFieldValue: function (labelName, expresionResult) {
                    $rootScope.hideFieldValue[labelName] = expresionResult;
                },

                expressionRunValidation: function (resultArr, record, currentField, isSave, module, isValidate) {
                    if (!currentField && isSave) {//save işlemi yapıldıgında tetiklenen expression validasyon.
                        for (var j = 0; j < resultArr.length; j++) {

                            if (this.exprressionDependencyControl(module, resultArr[j]))
                                continue

                            var expressionResult = '';
                            resultArr[j].isRun = true
                            for (var h = 0; h < resultArr[j].formulaJson.length; h++) {
                                for (var k = 0; k < resultArr[j].formulaJson[h].elements.length; k++) {
                                    expressionResult += this.expressionGetValue(record, resultArr[j], resultArr[j].formulaJson[h].elements[k], isValidate)
                                }
                            }

                            try {
                                var resultExp = eval(expressionResult);
                                if (resultExp === false)
                                    return resultArr[j].validationMsg;
                            } catch (e) {
                                return resultArr[j].validationMsg;
                            }
                        }
                    } else if (currentField && !isSave) {// field change tetiklendiğinde calışan expression validasyon.
                        var resultLastArr = $filter('filter')(resultArr, { label: currentField.name }, true);
                        for (var j = 0; j < resultLastArr.length; j++) {

                            if (this.exprressionDependencyControl(module, resultLastArr[j]))
                                continue

                            var expressionResult2 = '';
                            resultLastArr[j].isRun = true
                            for (var h = 0; h < resultLastArr[j].formulaJson.length; h++) {
                                for (var k = 0; k < resultLastArr[j].formulaJson[h].elements.length; k++) {
                                    expressionResult2 += this.expressionGetValue(record, resultLastArr[j], resultLastArr[j].formulaJson[h].elements[k], isValidate)
                                }
                            }

                            try {
                                var resultExp2 = eval(expressionResult2);
                                if (resultExp2 === false) {
                                    return resultLastArr[j].validationMsg;
                                }
                            } catch (e) {
                                return resultLastArr[j].validationMsg;
                            }
                        }
                    }

                },

                setExpression: function (module, record, picklist, currentField, isValidate, isSave) {
                    var resultArrValue = [];
                    var resultArrValidation = [];
                    $rootScope.moduleRecordData = {
                        "picklist": picklist,
                        "module": module
                    };

                    var valueFilter = $filter('filter')($rootScope.expressionRunOrderData.Value, { moduleName: module.name }, true)[0];
                    var validationFilter = $filter('filter')($rootScope.expressionRunOrderData.Validation, { moduleName: module.name }, true)[0];

                    if (!valueFilter || !validationFilter) {
                        var fields = $filter('filter')(module.fields, function (field) {
                            return (field.data_type === 'text_single' || field.data_type === 'text_multi' || field.data_type === 'number' ||
                                field.data_type === 'number_decimal' || field.data_type === 'currency') && field.deleted === false;
                        }, true);

                        //value ve validasyon için runOrder oluşturma.
                        resultArrValue = this.expressionRunOrder(fields, record, false);
                        resultArrValidation = this.expressionRunOrder(fields, record, true);

                        var valueData = { moduleName: module.name, value: resultArrValue }
                        var validationData = { moduleName: module.name, validation: resultArrValidation }

                        $rootScope.expressionRunOrderData.Value.push(valueData);
                        $rootScope.expressionRunOrderData.Validation.push(validationData);
                    } else {
                        resultArrValue = valueFilter.value
                        resultArrValidation = validationFilter.validation
                    }

                    if (!isSave)
                        this.expressionRunValue(resultArrValue, record, currentField, module)
                    return this.expressionRunValidation(resultArrValidation, record, currentField, isSave, module, true);
                },

                setDefaultValues: function (module, record, picklists) {
                    for (var i = 0; i < module.fields.length; i++) {
                        var field = module.fields[i];

                        if (field.deleted)
                            continue;

                        var fieldName = field.name;

                        if (field.default_value === undefined || field.default_value === null)
                            continue;

                        if (record[fieldName])
                            continue;

                        switch (field.data_type) {
                            // case 'text_single':
                            // case 'text_multi':
                            // case 'number':
                            // case 'number_decimal':
                            // case 'currency':
                            //     break;
                            case 'url':
                            case 'email':
                            case 'image':
                                record[fieldName] = field.default_value;
                                break;
                            case 'date':
                            case 'time':
                            case 'date_time':
                                if (field.default_value === '[now]') {
                                    record[fieldName] = new Date();
                                    record[fieldName + 'str'] = field.data_type === 'time' ? kendo.toString(new Date(), 't') : kendo.toString(new Date(), 'g');
                                } else {
                                    record[fieldName] = new Date(field.default_value);
                                    record[fieldName + 'str'] = field.data_type === 'time' ? kendo.toString(new Date(field.default_value), 't') : kendo.toString(new Date(field.default_value), 'g');
                                }
                                break;
                            case 'picklist':
                                var picklistRecord = $filter('filter')(picklists[field.picklist_id], { id: parseInt(field.default_value) }, true)[0];

                                if (!picklistRecord) {
                                    picklistRecord = {};
                                    picklistRecord.id = record[fieldName];
                                    picklistRecord.type = field.picklist_id;
                                    picklistRecord.label = {};
                                    picklistRecord.label.tr = record[fieldName];
                                    picklistRecord.label.en = record[fieldName];
                                    picklistRecord.labelStr = record[fieldName];
                                    picklistRecord.required = false;
                                    picklistRecord.order = 9999;

                                    helper.getPicklists([field.picklist_id], true);
                                }

                                record[field.name] = picklistRecord;
                                break;
                            case 'lookup':
                                var lookupId = field.default_value !== '[me]' ? parseInt(field.default_value) : $rootScope.user.id;
                                var fieldCurrent = angular.copy(field);

                                this.getRecord(field.lookup_type, lookupId, true)
                                    .then(function (response) {
                                        var lookupObject = {};
                                        lookupObject.id = response.data.id;
                                        lookupObject.primary_value = response.data[fieldCurrent.lookupModulePrimaryField.name];
                                        record[fieldCurrent.name] = lookupObject;
                                    });
                                break;
                            case 'multiselect':
                                var picklistIds = field.default_value.split(';');
                                record[fieldName] = [];

                                for (var j = 0; j < picklistIds.length; j++) {
                                    var picklistId = parseInt(picklistIds[j]);
                                    if (isNaN(picklistId))
                                        continue;

                                    record[fieldName].push(picklistId);
                                }
                                break;
                            case 'checkbox':
                                record[field.name] = field.default_value === 'true';
                                break;
                            case 'tag':
                                record[fieldName] = [];
                                var tags = field.default_value.split(',');
                                for (var j = 0; j < tags.length; j++) {
                                    if (field.tag && field.tag.dataSource && field.tag.dataSource._data.length > 0)
                                        var tag = $filter('filter')(field.tag.dataSource._data, { text: tags[j] }, true)[0];
                                    if (tag)
                                        record[fieldName].push(tag.id);
                                }
                                break;
                        }
                        this.setDisplayDependency(module, record);
                    }
                    this.setExpression(module, record, picklists, null, false, false);
                },

                setDependency: function (field, module, record, picklistsModule, scope) {
                    var that = this;
                    //Event start/end date dependency
                    if (!record.id && record.event_start_date && field.name === 'event_start_date') {
                        var startDate = new Date(record.event_start_date);

                        if (startDate.getHours() === 0 && startDate.getSeconds() === 0 && startDate.getMilliseconds() === 0) {
                            startDate.setHours(8);
                            record.event_start_date = startDate;
                        }

                        record.event_end_date = new Date(startDate.getTime() + 3600000);//1 hour
                    }

                    if (record.event_start_date && record.event_end_date && field.name === 'event_end_date') {
                        var eventStartDate = new Date(record.event_start_date);
                        var eventEndDate = new Date(record.event_end_date);

                        if (eventStartDate > eventEndDate)
                            record.event_start_date = new Date(eventEndDate.getTime() - 3600000);//1 hour
                    }

                    //Calendar date start/end date dependency
                    if (!record.id && field.calendar_date_type === 'start_date' && record[field.name]) {
                        var calendarStartDate = new Date(record[field.name]);

                        if (calendarStartDate.getHours() === 0 && calendarStartDate.getSeconds() === 0 && calendarStartDate.getMilliseconds() === 0) {
                            calendarStartDate.setHours(8);
                            record[field.name] = calendarStartDate;
                        }

                        var endDateField = $filter('filter')(module.fields, { calendar_date_type: 'end_date' }, true)[0];

                        if (endDateField)
                            record[endDateField.name] = new Date(calendarStartDate.getTime() + 3600000);//1 hour
                    }

                    if (field.calendar_date_type === 'end_date' && record[field.name]) {
                        var startDateField = $filter('filter')(module.fields, { calendar_date_type: 'start_date' }, true)[0];

                        if (startDateField && record[startDateField.name]) {
                            var calendarStartDate = new Date(record[startDateField.name]);
                            var calendarEndDate = new Date(record[field.name]);

                            if (calendarStartDate > calendarEndDate)
                                record[startDateField.name] = new Date(calendarEndDate.getTime() - 3600000);//1 hour
                        }
                    }

                    if (field.name === 'task_due_date' && $rootScope.taskReminderAuto) {
                        var reminderDate = new Date(record['task_due_date']);
                        reminderDate.setTime(reminderDate.getTime() + (8 * 60 * 60 * 1000));//Added 8 hours
                        record['task_reminder'] = reminderDate;
                    }

                    if (!module.dependencies)
                        return;

                    var dependencies = $filter('filter')(module.dependencies, { parent_field: field.name }, true);

                    if (!dependencies || !dependencies.length)
                        return;

                    for (var i = 0; i < dependencies.length; i++) {
                        var dependency = dependencies[i];

                        if (dependency.deleted)
                            continue;

                        var childField = $filter('filter')(module.fields, { name: dependency.child_field }, true)[0];

                        if (!dependency.clear) {
                            if (!childField)
                                continue;

                            switch (dependency.dependency_type) {
                                case 'list_value':
                                case 'list_field':
                                    if (!record.id)
                                        record[dependency.child_field] = null;

                                    var childFieldPicklist = picklistsModule[childField.picklist_id];

                                    for (var j = 0; j < childFieldPicklist.length; j++) {
                                        var item = childFieldPicklist[j];
                                        delete item.hidden;
                                    }

                                    if (dependency.dependency_type === 'list_value') {
                                        if (!record[dependency.parent_field])
                                            continue;

                                        var childValues = [];

                                        for (var key in dependency.value_maps) {
                                            if (dependency.value_maps.hasOwnProperty(key)) {
                                                var values = dependency.value_maps[key];

                                                if (key === record[dependency.parent_field].id.toString())
                                                    childValues = values;
                                            }
                                        }

                                        if (!childValues.length)
                                            continue;

                                        for (var k = 0; k < childFieldPicklist.length; k++) {
                                            var itemChild = childFieldPicklist[k];

                                            if (childValues.indexOf(itemChild.id) < 0)
                                                itemChild.hidden = true;
                                        }
                                    } else if (dependency.dependency_type === 'list_field') {
                                        if (!record[dependency.parent_field]) {
                                            for (var l = 0; l < childFieldPicklist.length; l++) {
                                                var itemClidField = childFieldPicklist[l];
                                                itemClidField.hidden = true;
                                            }

                                            continue;
                                        }

                                        var parentFieldValue = record[dependency.parent_field][dependency.field_map_parent];

                                        for (var m = 0; m < childFieldPicklist.length; m++) {
                                            var childFieldPicklistItem = childFieldPicklist[m];

                                            if (childFieldPicklistItem[dependency.field_map_child] !== parentFieldValue)
                                                childFieldPicklistItem.hidden = true;
                                        }
                                    }

                                    break;
                                case 'list_text':
                                    if (record[dependency.parent_field])
                                        record[dependency.child_field] = record[dependency.parent_field].value || record[dependency.parent_field].label[$rootScope.language];
                                    else
                                        record[dependency.child_field] = null;

                                    break;
                                case 'lookup_text':
                                    if (record[dependency.parent_field])
                                        record[dependency.child_field] = record[dependency.parent_field][dependency.field_map_parent];
                                    else
                                        record[dependency.child_field] = null;

                                    break;
                                case 'lookup_list':
                                case 'lookup_field':
                                    if (!record[dependency.parent_field])
                                        continue;

                                    var parentValue = record[dependency.parent_field][dependency.field_map_parent];
                                    var childPicklist = picklistsModule[childField.picklist_id];
                                    var childValue = null;

                                    if (dependency.dependency_type === 'lookup_list') {
                                        for (var n = 0; n < childPicklist.length; n++) {
                                            var childPicklistItem = childPicklist[n];

                                            if (childPicklistItem[dependency.field_map_child] === parentValue)
                                                childValue = childPicklistItem;
                                        }

                                        if (childValue && (!record.id || !record[dependency.child_field]))
                                            record[dependency.child_field] = childValue;
                                    } else if (dependency.dependency_type === 'lookup_field' && parentValue) {
                                        var lookupModule = $filter('filter')($rootScope.modules, { name: childField.lookup_type }, true)[0];
                                        var lookupField = angular.copy(childField);
                                        lookupField.lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { name: dependency.field_map_child }, true)[0];
                                        var additionalFields = [childField.lookupModulePrimaryField.name];

                                        //TODO REMOVE
                                        //Eski lookup yapısını kalma functionu  tamam olarak kullandığından emin değilim
                                        that.lookup(parentValue, lookupField, record, additionalFields, true)
                                            .then(function (data) {
                                                if (data[0]) {
                                                    var lookupRecord = data[0];
                                                    lookupRecord.primary_value = lookupRecord[childField.lookupModulePrimaryField.name];
                                                    record[dependency.child_field] = lookupRecord;
                                                    childField.valueChangeDontRun = true;
                                                }
                                            });
                                    }

                                    break;
                                case "display":
                                    if (!record.id) {
                                        switch (childField.data_type) {
                                            case 'checkbox':
                                                var valueDefault = childField.default_value === "true";
                                                record[dependency.child_field] = childField.default_value ? valueDefault : null;
                                                break;
                                            default:
                                                record[dependency.child_field] = childField.default_value ? childField.default_value : null;
                                                break;
                                        }
                                    }

                                    break;
                            }
                        } else {
                            record[dependency.child_field] = undefined;
                        }

                        this.calculate(childField, module, record);
                    }
                },

                setDependencyFilter: function (fieldName, value, module, picklistsModule) {
                    var currentModule = angular.copy(module);
                    var fieldNameOrginal = angular.copy(fieldName);

                    if (fieldName.indexOf('.') > -1) {
                        var fieldParts = fieldName.split('.');
                        currentModule = $filter('filter')($rootScope.modules, { name: fieldParts[1] }, true)[0];
                        fieldNameOrginal = fieldParts[2];
                    }

                    if (!currentModule.dependencies || !currentModule.dependencies.length)
                        return;

                    var dependencies = $filter('filter')(currentModule.dependencies, { parent_field: fieldNameOrginal }, true);

                    if (!dependencies || !dependencies.length)
                        return;

                    var moduleFields = angular.copy(currentModule.fields);

                    for (var i = 0; i < dependencies.length; i++) {
                        var dependency = dependencies[i];

                        if (dependency.deleted)
                            continue;

                        var childField = $filter('filter')(moduleFields, { name: dependency.child_field }, true)[0];

                        if (!childField || !value)
                            continue;

                        if (dependency.dependency_type === 'list_field') {
                            var childFieldPicklist = picklistsModule[childField.picklist_id];

                            for (var j = 0; j < childFieldPicklist.length; j++) {
                                var item = childFieldPicklist[j];
                                delete item.hidden;
                            }

                            var parentFieldValue = value[dependency.field_map_parent];

                            for (var k = 0; k < childFieldPicklist.length; k++) {
                                var childFieldPicklistItem = childFieldPicklist[k];

                                if (childFieldPicklistItem[dependency.field_map_child] !== parentFieldValue)
                                    childFieldPicklistItem.hidden = true;
                            }
                        }
                    }
                },

                setDisplayDependency: function (module, record) {
                    if (!module.display_dependencies)
                        return;

                    for (var i = 0; i < module.display_dependencies.length; i++) {
                        var dependency = module.display_dependencies[i];

                        if (dependency.deleted)
                            continue;

                        var dependent = null;

                        if (dependency.dependent_section) {
                            dependent = $filter('filter')(module.sections, { name: dependency.dependent_section }, true)[0];
                        } else {
                            dependent = $filter('filter')(module.fields, { name: dependency.dependent_field }, true)[0];
                        }

                        if (!dependent)
                            continue;

                        if (record[dependency.field] === undefined) {
                            dependent.hidden = true;
                            delete record[dependency.dependent_field];
                            continue;
                        }

                        var dependentValue = null;
                        var recordValue = angular.copy(record[dependency.field]);

                        if (angular.isArray(recordValue)) {
                            if (!record[dependency.field].length) {
                                dependent.hidden = true;
                                delete record[dependency.dependent_field];
                                continue;
                            }

                            for (var j = 0; j < recordValue.length; j++) {
                                var recordValueItemId = recordValue[j];

                                var currentDependentValue = $filter('filter')(dependency.values, recordValueItemId, true)[0];

                                if (!dependentValue && currentDependentValue)
                                    dependentValue = currentDependentValue;
                            }
                        } else if (!dependency.values) {
                            dependentValue = recordValue;
                        } else if (recordValue) {
                            if (typeof (recordValue) === 'boolean')
                                dependentValue = recordValue;
                            else if (recordValue.id)
                                dependentValue = $filter('filter')(dependency.values, recordValue.id, true)[0];
                            else
                                dependentValue = $filter('filter')(dependency.values, recordValue, true)[0];
                        }

                        if (!dependency.otherwise && !dependentValue) {
                            dependent.hidden = true;

                            if (module.name === 'p_employees' && (dependency.dependent_field === "profile" || dependency.dependent_field === "email"))
                                continue;

                            delete record[dependency.dependent_field];
                            continue;
                        }

                        if (dependency.otherwise && dependentValue) {
                            dependent.hidden = true;

                            if (module.name === 'p_employees' && (dependency.dependent_field === "profile" || dependency.dependent_field === "email"))
                                continue;

                            delete record[dependency.dependent_field];
                            continue;
                        }

                        if (!dependency.dependent_section) {
                            var dependencyField = $filter('filter')(module.fields, { name: dependency.field }, true)[0];
                            if (dependencyField.hidden) {
                                dependent.hidden = true;
                                delete record[dependency.dependent_field];
                                continue;
                            }
                        }

                        dependent.hidden = false;
                    }
                },

                generatTd: function (item) {
                    var tmpl = {
                        mobileContent: "",
                        normalContent: ""
                    };

                    switch (item.data_type) {
                        case "rating":
                            tmpl.mobileContent = '<div class="mobile-grid-block"><span>' + item.label + ': </span><input class="rating-stars" kendo-rating k-precision="\'half\'" k-label="false" k-max="getRatingCount(\'' + item.name + '\')"  ng-init="rating = dataItem[\'' + item.name + '\']"  ng-model="rating" ng-disabled="true" /></div>';
                            tmpl.normalContent = '<td class="hide-on-m2"><input class="rating-stars" kendo-rating k-precision="\'half\'" k-label="false" k-max="getRatingCount(\'' + item.name + '\', relatedModule)"  ng-init="rating = dataItem[\'' + item.name + '\']"  ng-model="rating" ng-disabled="true"/></td>';
                            break;
                        case "image":
                            tmpl.mobileContent =
                                '<div class="mobile-grid-block"><span>' + item.label + ': </span>' +
                                '<div class="image-preview image-preview-60"><div class="image-holder">' +
                                '<img ng-click="$event.stopPropagation(); showLightBox($event, (dataItem[\'' + item.name + '\'] ? dataItem : \'\'), true)" ng-src="{{dataItem[\'' + item.name + '\'] ? (config.imageUrl+dataItem[\'' + item.name + '\']): \'images/no-image.png\'}}"/>' +
                                '</div></div>' +
                                '</div>';
                            tmpl.normalContent =
                                '<td class="hide-on-m2">' +
                                '<div ng-style="getImageStyle(\'' + item.name + '\')" class="image-preview image-preview-60"><div class="image-holder">' +
                                '<img ng-click="$event.stopPropagation(); showLightBox($event, (dataItem[\'' + item.name + '\'] ? dataItem : \'\'), true, undefined, relatedModule)" ng-src="{{dataItem[\'' + item.name + '\'] ? (config.imageUrl+dataItem[\'' + item.name + '\']): \'images/no-image.png\'}}"/>\n' +
                                '</div></div>' +
                                '</td>';
                            break;
                        case "text_multi":
                            tmpl.mobileContent = '<div class="mobile-grid-block"><span>' + item.label + ': </span><div class="grid-list-button d-block"><span ng-bind-html="dataItem[\'' + item.name + '\'] "></span></div></div>';
                            tmpl.normalContent = '<td class="hide-on-m2"  ng-bind-html="dataItem[\'' + item.name + '\']"></td>';
                            break;
                        case "lookup":
                            var showAnchor = true;

                            if (item.lookup_type === 'users' || item.lookup_type === 'roles' || item.lookup_type === 'profiles')
                                showAnchor = false;

                            tmpl.mobileContent =
                                '<div class="mobile-grid-block"><span>' + item.label + ': </span>' +
                                '<div ng-show="dataItem[\'' + item.name + '\']" class="grid-list-button">' +
                                '<span>{{dataItem[\'' + item.name + '\']}}</span>' +
                                '<a ng-if="' + showAnchor + '"  ng-click="$event.stopPropagation(); goToRecord(\'' + item.name + '\',\'' + item.lookup_type + '\',' + showAnchor + ', dataItem, \'' + item.external_link + '\')"><i class= "fas fa-external-link-alt"></i></a>' +
                                '</div>' +
                                '</div>';
                            tmpl.normalContent =
                                '<td class="hide-on-m2">' +
                                '<div ng-if="' + showAnchor + '" ng-show="dataItem[\'' + item.name + '\']" class="grid-list-button">' +
                                '<span>{{dataItem[\'' + item.name + '\']}}</span>' +
                                '<a ng-click="$event.stopPropagation(); goToRecord(\'' + item.name + '\',\'' + item.lookup_type + '\',' + showAnchor + ', dataItem,\'' + item.external_link + '\')"><i class= "fas fa-external-link-alt"></i></a>' +
                                '</div>' +
                                '<span class="text-nowrap" ng-if="!' + showAnchor + '">{{dataItem[\'' + item.name + '\']}}</span></span>' +
                                '</td>';
                            break;
                        case "url":
                            tmpl.mobileContent =
                                '<div class="mobile-grid-block">' +
                                '<span>' + item.label + ': </span>' +
                                '<div class="grid-list-button" ng-show="dataItem[\'' + item.name + '\']"> ' +
                                '<span>{{dataItem[\'' + item.name + '\']}}</span>' +
                                '<a ng-click="$event.stopPropagation();" href="{{dataItem[\'' + item.name + '\']}}" target="_blank"><i class= "fas fa-external-link-alt"></i></a>' +
                                '</div>' +
                                '</div>';
                            tmpl.normalContent =
                                '<td class="hide-on-m2">' +
                                '<div class="grid-list-button" ng-show="dataItem[\'' + item.name + '\']"> ' +
                                '<span>{{dataItem[\'' + item.name + '\']}}</span>' +
                                '<a ng-click="$event.stopPropagation();" href="{{dataItem[\'' + item.name + '\']}}" target="_blank"><i class= "fas fa-external-link-alt"></i></a>' +
                                '</div>' +
                                '</td>';
                            break;
                        case "email":
                            tmpl.mobileContent =
                                '<div class="mobile-grid-block"><span>' + item.label + ': </span>' +
                                '<div class="grid-list-button" ng-show="dataItem[\'' + item.name + '\']" >' +
                                '<span>{{dataItem[\'' + item.name + '\']}}</span>' +
                                '<a ng-click="$event.stopPropagation();" href="mailto:{{dataItem[\'' + item.name + '\']}}" ><i class= "fas fa-paper-plane"></i></a>' +
                                '</div>' +
                                '</div>';
                            tmpl.normalContent =
                                '<td class="hide-on-m2">' +
                                '<div class="grid-list-button" ng-show="dataItem[\'' + item.name + '\']" >' +
                                '<span>{{dataItem[\'' + item.name + '\']}}</span>' +
                                '<a ng-click="$event.stopPropagation();" href="mailto:{{dataItem[\'' + item.name + '\']}}"><i class= "fas fa-paper-plane"></i></a>' +
                                '</div>' +
                                '</td>';
                            break;
                        case "document":
                            tmpl.mobileContent =
                                '<div class="mobile-grid-block"><span>' + item.label + ': </span>' +
                                '<div class="grid-list-button" ng-show="dataItem[\'' + item.name + '\']">' +
                                '<span>{{dataItem[\'' + item.name + '\']}}</span>' +
                                '<a ng-click="$event.stopPropagation();" href="storage/record_file_download?fileName={{dataItem[\'' + item.name + '\']}}"><i class= "fas fa-download"></i></a>' +
                                '</div>' +
                                '</div>';
                            tmpl.normalContent =
                                '<td class="hide-on-m2">' +
                                '<div class="grid-list-button" ng-show="dataItem[\'' + item.name + '\']">' +
                                '<span>{{dataItem[\'' + item.name + '\']}}</span>' +
                                '<a ng-click="$event.stopPropagation();" href="storage/record_file_download?fileName={{dataItem[\'' + item.name + '\']}}"><i class= "fas fa-download"></i></a>' +
                                '</div>' +
                                '</td>';
                            break;
                        case "tag":
                        case "multiselect":
                            tmpl.mobileContent =
                                '<div class="mobile-grid-block"><span>' + item.label + ': </span>' +
                                '<div class="grid-list-button" ng-show="dataItem[\'' + item.name + '\']">' +
                                '<span class="grid-multiselect-text" ng-repeat="data in getTagAndMultiDatas(dataItem, dataItem[\'' + item.name + '\'])">' +
                                '{{data}}' +
                                '</span > ' +
                                '<button ng-show="dataItem.show" aria-label="{{\'Common.ShowMore\' | translate}}" ng-click="$event.stopPropagation(); showLightBox($event, dataItem, false, \'' + item.name + '\')"> <i class="fas fa-ellipsis-v"></i><md-tooltip md-autohide="true" md-direction="bottom">{{\'Common.ShowMore\' | translate}}</md-tooltip></button>' +
                                '</div>' +
                                '</div>';
                            tmpl.normalContent =
                                '<td class="hide-on-m2">' +
                                '<div class="grid-list-button" ng-show="dataItem[\'' + item.name + '\']">' +
                                '<span class="grid-multiselect-text" ng-repeat="data in getTagAndMultiDatas(dataItem, \'' + item.name + '\')">' +
                                '{{data}}' +
                                '</span > ' +
                                '<button ng-show="dataItem.show" aria-label="{{\'Common.ShowMore\' | translate}}" ng-click="$event.stopPropagation(); showLightBox($event, dataItem, false, \'' + item.name + '\', relatedModule)"> <i class="fas fa-ellipsis-v"></i><md-tooltip md-autohide="true" md-direction="bottom">{{\'Common.ShowMore\' | translate}}</md-tooltip></button>' +
                                '</div>' +
                                '</td>';
                            break;
                        case "location":
                            tmpl.mobileContent = '<div class="mobile-grid-block"><span>' + item.label + ': </span>' +
                                '<div class="image-preview image-preview-60"><div class="image-holder">' +
                                '<img ng-click="$event.stopPropagation(); showLightBox($event, (dataItem[\'' + item.name + '\'] ? dataItem : \'\'), true, \'' + item.name + '\')" ng-src="{{dataItem[\'' + item.name + '\'] ? getLocationUrl(dataItem[\'' + item.name + '\']) : \'images/no-location.png\'}}"/>' +
                                '</div></div>' +
                                '</div>';
                            tmpl.normalContent = '<td class="hide-on-m2">' +
                                ' <div class="image-preview image-preview-60">' +
                                '<div class="image-holder">' +
                                '<img ng-click="$event.stopPropagation(); showLightBox($event, (dataItem[\'' + item.name + '\'] ? dataItem : \'\'), true, \'' + item.name + '\', relatedModule)" ng-src="{{dataItem[\'' + item.name + '\'] ? getLocationUrl(dataItem[\'' + item.name + '\']) : \'images/no-location.png\'}}"/>' +
                                '</div>' +
                                '</td>';
                            break;
                        default:
                            tmpl.mobileContent = '<div class="mobile-grid-block"><span>' + item.label + ': </span><div class="grid-list-button"><span>{{ dataItem[\'' + item.name + '\'] }}</span></div></div>';
                            tmpl.normalContent = '<td class="hide-on-m2"><span class="text-nowrap">{{ dataItem[\'' + item.name + '\'] }}</span></td>';
                            break;
                    }
                    return tmpl;
                },

                generatoptionsItem: function (moduleName, itemType, dataItemId) {
                    var modulename = "'" + moduleName + "'";
                    switch (itemType) {
                        case "remove":
                            return ' <md-menu-item ng-if="controlRemove(dataItem)">\n' +
                                ' <md-button id="deleteButton-{{dataItem.id}}" ng-click="recordDelete($event,' + dataItemId + ',' + modulename + ')">\n' +
                                ' <i class="fas fa-trash"></i> <span> ' + $filter('translate')('Common.Delete') + '</span>\n' +
                                ' </md-button>\n' +
                                ' </md-menu-item>\n';
                        case "copy":
                            return ' <md-menu-item ng-if="controlCopy(dataItem, relatedModule)">\n' +
                                ' <md-button ui-sref="app.record({type: relatedModule ? relatedModule.related_module : module.name,clone:true,id:' + dataItemId + '})">\n' +
                                ' <i class="fas fa-copy"></i> ' + $filter('translate')('Common.Copy') + ' <span></span>\n' +
                                ' </md-button>\n' +
                                ' </md-menu-item>\n';
                        case "edit":
                            return ' <md-menu-item ng-if="controlEdit(dataItem, relatedModule)">\n' +
                                ' <md-button ui-sref="app.record({type: relatedModule ? relatedModule.related_module : module.name,id:' + dataItemId + '})">\n' +
                                ' <i class="fas fa-edit"></i> ' + $filter('translate')('Common.Edit') + ' <span></span>\n' +
                                ' </md-button>\n' +
                                ' </md-menu-item>\n';
                        case "send-sms":
                            return ' <md-menu-item ng-if="user.profile.send_sms">\n' +
                                ' <md-button  ng-click="selectRowSingle($event,dataItem); showSMSModal()" >\n' +
                                ' <i class="fas fa-sms"></i> ' + $filter('translate')('Module.SendSMS') + ' <span></span>\n' +
                                ' </md-button>\n' +
                                ' </md-menu-item>\n';
                        case "send-email":
                            return ' <md-menu-item>\n' +
                                ' <md-button ui-sref="app.record({type: relatedModule ? relatedModule.related_module : module.name,clone:true,id:dataItem.id})">\n' +
                                ' <i class="fas fa-envelope"></i> ' + $filter('translate')('Module.SendEMail') + ' <span></span>\n' +
                                ' </md-button>\n' +
                                ' </md-menu-item>\n';
                        case "download":
                            return ' <md-menu-item>\n' +
                                ' <md-button ng-click="selectRowSingle($event,dataItem); showSMSModal()">\n' +
                                ' <i class="fas fa-file-download"></i> ' + $filter('translate')('Common.Download') + ' <span></span>\n' +
                                ' </md-button>\n' +
                                ' </md-menu-item>\n';
                        case "remove_relation":
                            return ' <md-menu-item ng-if="controlRemove(dataItem, relatedModule)">\n' +
                                ' <md-button id="deleteButton-{{dataItemId}}" ng-click="deleteRelation($event, relatedModule, true, ' + dataItemId + ')">\n' +
                                ' <i class="fas fa-minus-circle"></i> <span> ' + $filter('translate')('Module.RemoveRelation') + '</span>\n' +
                                ' </md-button>\n' +
                                ' </md-menu-item>\n';
                    }
                },

                generatRowtmpl: function (selectedFields, isSubTable, config) {
                    var that = this;
                    var table = {
                        columns: [],
                        rowtempl: "",
                        requestFields: [],
                    };

                    //Select tabele column
                    var mobileContent = "";
                    var isManyToMany = false;
                    var dataItemId = 'dataItem.id';
                    var optionsItems = "";
                    if (config.relatedModule && config.relatedModule.relation_type === 'many_to_many') {
                        isManyToMany = true;
                        var manyToManyField = config.module.name !== config.relatedModule.related_module ? config.relatedModule.related_module + '_id' : config.relatedModule.related_module + '1_id';
                        var generateFieldForManytoMay = manyToManyField + '.' + config.relatedModule.related_module + '.';
                        dataItemId = 'dataItem[\'' + manyToManyField + '\']';
                    }

                    var tableOptinosMenu = ["edit", "copy", "remove"];
                    var selectableColumnTh = {};
                    var selecttableColumnTemplate = '';
                    if (!isSubTable) {

                        for (var i = 0; i < tableOptinosMenu.length; i++) {
                            optionsItems = optionsItems + that.generatoptionsItem(config.moduleName, tableOptinosMenu[i], dataItemId);
                        }

                        selectableColumnTh = {
                            width: "40px",
                            headerTemplate: "<input type='checkbox' ng-if=' grid.dataSource.data().length>0' ng-checked='isAllSelected'  ng-click='selectAll($event, grid.dataSource.data())' id='header-chb' class='k-checkbox header-checkbox'><label class='k-checkbox-label' for='header-chb'></label>",
                        };

                        selecttableColumnTemplate = '<td ng-click="$event.stopPropagation();" class="position-relative"><input ng-click="selectRow($event,dataItem);$event.stopPropagation();" ng-disabled="dataItem.freeze && !user.profile.has_admin_rights" ng-checked="isRowSelected(dataItem.id) || dataItem.selected"  type="checkbox" id="{{dataItem.id}}" class="k-checkbox row-checkbox"><label class="k-checkbox-label" for="{{dataItem.id}}"></label></td>';
                    } else {

                        if (isManyToMany && tableOptinosMenu.indexOf("remove_relation") < 0) {
                            tableOptinosMenu.push("remove_relation");
                        }

                        for (var i = 0; i < tableOptinosMenu.length; i++) {
                            optionsItems = optionsItems + that.generatoptionsItem(config.relatedModule.related_module, tableOptinosMenu[i], dataItemId);
                        }

                        selectableColumnTh = {
                            width: "40px",
                            headerTemplate: '<input type="checkbox"  ng-model="isAllSelected[\'' + config.relatedModule.related_module + '\']"  ng-click="selectAll($event,\'' + config.relatedModule.related_module + '\')" id="header-chb-' + config.relatedModule.related_module + '" class="k-checkbox header-checkbox"><label class="k-checkbox-label" for="header-chb-' + config.relatedModule.related_module + '"></label>',
                        };

                        selecttableColumnTemplate = '<td ng-click="$event.stopPropagation();" class="position-relative">  <input ng-model="dataItem.isChecked"  type="checkbox" id="{{' + dataItemId + '}}" ng-click="selectRow($event,dataItem,\'' + config.relatedModule.related_module + '\')" class="k-checkbox row-checkbox"><label class="k-checkbox-label" for="{{' + dataItemId + '}}"></label></td>';
                    }

                    table.rowtempl = table.rowtempl + selecttableColumnTemplate;
                    table.columns.push(selectableColumnTh);

                    //Select tabele column end

                    selectedFields.map(function (item, index) {
                        if (item && item.name) {
                            if (generateFieldForManytoMay) {
                                item.name = generateFieldForManytoMay + item.name;
                            }

                            if (item.name.contains('.languages.')) {
                                item.name = item.name.substring(0, item.name.indexOf('.label'));
                            }

                            table.requestFields.push(item.name);

                            table.columns.push({
                                title: item.labelExt ? item.label + ' ' + item.labelExt : item.label,
                                field: item.name,
                                media: "(min-width: 575px)",
                                groupable: false,
                                groupHeaderTemplate: "#= aggregates.groupSelect #" + (item.labelExt ? item.label + ' ' + item.labelExt : item.label) + ": #= value #  #= aggregates.template #"
                            });


                            var template = that.generatTd(item, mobileContent);

                            table.rowtempl = table.rowtempl + template.normalContent;
                            mobileContent = mobileContent + template.mobileContent;
                        }

                    });

                    var optionsColumn = {
                        field: "",
                        width: "50px",
                    };
                    var optionsColumnTemplate = "";
                    if (!isManyToMany) {
                        optionsColumnTemplate = '<td ng-click="$event.stopPropagation();" class="padding0"><md-menu md-position-mode="target-right target">\n' +
                            '<md-button ng-show="dataItem.showRemove || dataItem.showCopy || dataItem.showEdit" ng-disabled="dataItem.freeze && !user.profile.has_admin_rights" class="md-icon-button" aria-label=" " ng-click="$mdMenu.open()"> <i class="fas fa-ellipsis-v"></i></md-button>\n' +
                            ' <md-menu-content width="2" class="md-dense">\n' +
                            optionsItems + '\n' +
                            ' </md-menu-content>\n' +
                            ' </md-menu></td>';
                    } else {
                        optionsColumnTemplate = '<td ng-click="$event.stopPropagation();" class="padding0"> <button class="md-icon-button md-button md-blue-theme md-ink-ripple" id="deleteButton-{{dataItemId}}" ng-click="deleteRelation($event, relatedModule, true, dataItem[\'' + config.relatedModule.related_module + '_id\'])">\n' +
                            ' <i class="fas fa-minus-circle"></i>\n' +
                            ' </button></td>';
                    }


                    table.rowtempl = table.rowtempl + '<td class="show-on-m2">' + mobileContent + '</td>';

                    var ItemsColumn = {
                        title: "Items",
                        media: "(max-width: 575px)"
                    };

                    if (config.activeView && config.activeView.view_type === 'report' && config.activeView.aggregations) {
                        ItemsColumn.footerTemplate = ""
                        for (var j = 0; j < config.activeView.aggregations.length; j++) {
                            var aggertionFiled = config.activeView.aggregations[j].aggregation_type + '(' + config.activeView.aggregations[j].field + ')';
                            ItemsColumn.footerTemplate = ItemsColumn.footerTemplate + "  <div> {{ fieldskey['" + config.activeView.aggregations[j].field + "']['label_'+language] }} {{ 'Report." + config.activeView.aggregations[j].aggregation_type + "' | translate  }} : {{ " + aggertionFiled.replace("(", "_").replace(")", "") + "}} </div>";
                        }
                    }

                    table.columns.push(ItemsColumn);

                    if (optionsItems != "" && !config.tableOptinosMenuHide) {
                        table.rowtempl = table.rowtempl + optionsColumnTemplate;
                        table.columns.push(optionsColumn);
                    }

                    if (config.activeView && config.activeView.view_type === 'grid') {
                        var temp = '<td class=\'k-group-cell\'>&nbsp;</td>';
                        for (var i = 0; i < config.gridGroupBy.length; i++) {
                            table.rowtempl = temp + table.rowtempl;
                        }
                    }

                    return table;
                },

                deleteView: function (id) {
                    return $http.delete(config.apiUrl + 'view/delete/' + id);
                },

                getViews: function (module, displayFields, cache) {
                    var that = this;
                    var deferred = $q.defer();
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
                            } else {
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

                    $http.get(config.apiUrl + 'view/get_all/' + module.id)
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
                                    } else {
                                        field = $filter('filter')(module.fields, { name: viewField.field }, true)[0]
                                    }

                                    if (field && that.hasFieldDisplayPermission(field))
                                        viewFields.push(viewField);
                                }

                                view.fields = viewFields;

                                view.label = view['label_' + $rootScope.language];

                                views.push(view);
                            }

                            // views = $filter('orderBy')(views, 'label_' + $rootScope.language);
                            deferred.resolve(response.data);
                        })
                        .catch(function (reason) {
                            deferred.reject(reason.data);
                        });

                    return deferred.promise;
                },

                getViewState: function (moduleId, cache) {
                    var deferred = $q.defer();

                    if (cache && cache['viewState']) {
                        deferred.resolve(cache['viewState']);
                        return deferred.promise;
                    }

                    $http.get(config.apiUrl + 'view/get_view_state/' + moduleId)
                        .then(function (response) {
                            deferred.resolve(response.data);
                        })
                        .catch(function (reason) {
                            deferred.reject(reason.data);
                        });

                    return deferred.promise;
                },

                getViewFields: function (module, view) {
                    var that = this;
                    var moduleClone = Object.assign({}, module);
                    var moduleFields = moduleClone.fields;
                    var fields = {};
                    fields.selectedFields = [];
                    fields.availableFields = [];
                    fields.primaryField = '';

                    moduleFields = $filter('filter')(moduleFields, {
                        display_list: true,
                        lookup_type: '!relation'
                    }, true);


                    var seperatorFieldMain = {
                        name: 'seperator-main',
                        label: $rootScope.getLanguageValue(module.languages, 'label', 'singular'),
                        order: 0,
                        seperator: true
                    };

                    moduleFields.push(seperatorFieldMain);
                    var seperatorLookupOrder = 0;

                    angular.forEach(moduleFields, function (field) {
                        if (field.data_type === 'lookup' && field.lookup_type !== 'relation') {
                            var lookupModule = $filter('filter')($rootScope.modules, { name: field.lookup_type }, true)[0];
                            seperatorLookupOrder += 1000;
                            if (lookupModule === null || lookupModule === undefined) return;
                            var seperatorFieldLookup = {
                                name: 'seperator-' + lookupModule.name,
                                order: seperatorLookupOrder,
                                seperator: true
                            };

                            seperatorFieldLookup.label = $rootScope.getLanguageValue(lookupModule.languages, 'label', 'singular') + ' (' + $rootScope.getLanguageValue(field.languages, 'label') + ')';

                            moduleFields.push(seperatorFieldLookup);

                            var lookupModuleFields = lookupModule.fields;
                            lookupModuleFields = $filter('filter')(lookupModuleFields, { display_list: true }, true);

                            angular.forEach(lookupModuleFields, function (fieldLookup) {

                                if (fieldLookup.data_type === 'lookup') {
                                    fieldLookup.field = Object.assign({}, fieldLookup);
                                    return;
                                }
                                fieldLookup = Object.assign({}, fieldLookup);

                                fieldLookup.label = $rootScope.getLanguageValue(fieldLookup.languages, 'label');
                                fieldLookup.labelExt = '(' + field.label + ')';
                                //fieldLookup.label = fieldLookup.label;
                                fieldLookup.name = field.name + '.' + lookupModule.name + '.' + fieldLookup.name;
                                fieldLookup.order = parseInt(fieldLookup.order) + seperatorLookupOrder;
                                fieldLookup.id = fieldLookup.order;
                                fieldLookup.parent_id = seperatorLookupOrder;
                                moduleFields.push(fieldLookup);

                            });
                        }
                    });

                    angular.forEach(moduleFields, function (field) {
                        if (field.deleted || !that.hasFieldDisplayPermission(field) && field.multiline_type !== 'large')
                            return;

                        var selectedField = null;

                        if (view.fields)
                            selectedField = $filter('filter')(view.fields, { field: field.name }, true)[0];

                        var newField = {};
                        newField.name = field.name;
                        newField.label = field.label;
                        newField.labelExt = field.labelExt;
                        newField.order = field.order;
                        newField.id = field.id;

                        if (field.name.indexOf('seperator-') < 0)
                            newField.parent_id = field.parent_id ? field.parent_id : 0;

                        newField.lookup_type = field.lookup_type;
                        newField.seperator = field.seperator;
                        newField.multiline_type = field.multiline_type;
                        newField.data_type = field.data_type;

                        if (field.data_type === 'lookup') {
                            newField.external_link = field.external_link ? escape(field.external_link) : "";
                            newField.name = field.name + '.' + field.lookup_type + '.' + field.lookupModulePrimaryField.name;
                        }

                        if (selectedField) {
                            newField.order = selectedField.order;
                            newField.id = selectedField.id ? selectedField.id : selectedField.order;
                            newField.parent_id = selectedField.parent_id;
                            fields.selectedFields.push(newField);
                        } else {
                           // var primaryField = $filter('filter')(module.fields, { primary: true }, true)[0];
                            //if (field.name !== primaryField.name) {
                                fields.availableFields.push(newField);
                            //}
                           /* else {
                                newField.primary = true;
                                fields.selectedFields.push(newField);
                            }*/
                        }
                    });

                    fields.selectedFields = $filter('orderBy')(fields.selectedFields, 'order');
                    fields.availableFields = $filter('orderBy')(fields.availableFields, 'order');

                    return fields;
                },

                getCSVData: function (scope, relatedModule, module) {
                    var that = this;
                    var deferred = $q.defer();
                    var findRequest = angular.copy(scope.findRequest[relatedModule.id]);
                    findRequest.take = 3000;
                    that.findCustom(scope.locale || scope.language, findRequest)
                        .then(function (response) {
                            var records = response.data.data;
                            var csv = [];
                            var header = [];
                            var fields = $rootScope.modulus[relatedModule.related_module].fields;
                            var lookupFields = {};
                            var manyToManyFields = {};

                            var lookups = $filter('filter')(relatedModule.display_fields, function (field) {
                                return field.contains('.');
                            }, true);

                            for (var j = 0; j < relatedModule.display_fields.length; j++) {
                                var field = $filter('filter')(fields, { name: relatedModule.display_fields[j] }, true)[0];
                                if (field) {
                                    header.push($rootScope.getLanguageValue(field.languages, 'label') + (field.parentField ? ' (' + $rootScope.getLanguageValue(field.parentField.languages, 'label') + ')' : ''));
                                } else {
                                    if (lookups) {
                                        for (var o = 0; o < lookups.length; o++) {
                                            var res = lookups[o].split('.');
                                            if (res) {
                                                field = $filter('filter')(fields, { name: res[0] }, true)[0];
                                                header.push($rootScope.getLanguageValue(field.languages, 'label') + (field.lookupModulePrimaryField ? ' (' + field.lookupModulePrimaryField.label + ')' : ''));
                                                lookups.splice(o, 1);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            csv.push(header);

                            for (var m = 0; m < records.length; m++) {
                                var record = records[m];
                                var row = [];
                                var recordFields = relatedModule.display_fields;

                                for (var k = 0; k < recordFields.length; k++) {
                                    var pushData = record[recordFields[k]]; //|| lookupFields[recordFields[k]] || record[manyToManyFields[recordFields[k]]];
                                    row.push(pushData);
                                }

                                csv.push(row);
                            }

                            deferred.resolve(csv);
                        });

                    return deferred.promise;
                },

                getIcons: function () {
                    return icons2.icons;
                },

                getActionButtons: function (moduleId, refresh) {
                    var deferred = $q.defer();
                    var cacheKey = 'action_button_' + moduleId;
                    var cache = $cache.get(cacheKey);

                    if (cache && !refresh && !preview) {
                        deferred.resolve(cache);
                        return deferred.promise;
                    }

                    $http.get(config.apiUrl + 'action_button/get/' + moduleId)
                        .then(function (actionButtons) {
                            $cache.put(cacheKey, actionButtons.data);
                            deferred.resolve(actionButtons.data);
                        })
                        .catch(function (reason) {
                            deferred.reject(reason.data);
                        });

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

                hasFieldReadOnlyPermission: function (field) {
                    if (!field.permissions)
                        return false;

                    var permission = $filter('filter')(field.permissions, { profile_id: $rootScope.user.profile.id }, true)[0];

                    if (!permission)
                        return false;

                    return permission.type === 'read_only';
                },

                hasFieldFullPermission: function (field) {
                    if (!field.permissions)
                        return true;

                    var permission = $filter('filter')(field.permissions, { profile_id: $rootScope.user.profile.id }, true)[0];

                    var hasFieldSectionFullPermission = function (field) {
                        if (!field.sectionObj || !field.sectionObj.permissions)
                            return true;

                        var permission = $filter('filter')(field.sectionObj.permissions, { profile_id: $rootScope.user.profile.id }, true)[0];

                        if (!permission)
                            return true;

                        return permission.type === 'full';
                    };

                    if (!permission)
                        return hasFieldSectionFullPermission(field);

                    if (permission.type !== 'full')
                        return false;

                    return hasFieldSectionFullPermission(field);
                },

                hasSectionDisplayPermission: function (section) {
                    if (!section.permissions)
                        return true;

                    var permission = $filter('filter')(section.permissions, { profile_id: $rootScope.user.profile.id }, true)[0];

                    if (!permission)
                        return true;

                    return permission.type !== 'none';
                },

                hasSectionReadOnlyPermission: function (section) {
                    if (!section.permissions)
                        return false;

                    var permission = $filter('filter')(section.permissions, { profile_id: $rootScope.user.profile.id }, true)[0];

                    if (!permission)
                        return false;

                    return permission.type === 'read_only';
                },

                hasSectionFullPermission: function (section) {
                    if (!section.permissions)
                        return true;

                    var permission = $filter('filter')(section.permissions, { profile_id: $rootScope.user.profile.id }, true)[0];

                    if (!permission)
                        return true;

                    return permission.type === 'full';
                },

                hasActionButtonDisplayPermission: function (actionButton, isList, record, module) {
                    if (actionButton.visible) {
                        var allUsersAndProfiles = false;
                        var buttonFilters = false;
                        if (actionButton.visible === 'all_users')
                            allUsersAndProfiles = true;
                        else {
                            if (actionButton.visible === 'profiles') {
                                if (actionButton.permissions !== undefined) {
                                    var isPermission = $filter('filter')(actionButton.permissions, { profile_id: $rootScope.user.profile.id }, true)[0];
                                    allUsersAndProfiles = !!isPermission;
                                } else
                                    allUsersAndProfiles = true;
                            }
                        }

                        if (isList)//buttons filters is not use list type
                            buttonFilters = true;
                        else {
                            buttonFilters = this.hasActionButtonDisplayCriteria(module, actionButton, record)
                        }

                        return allUsersAndProfiles === true && buttonFilters === true;
                    } else {
                        if (!actionButton.permissions)
                            return true;
                        var permission = $filter('filter')(actionButton.permissions, { profile_id: $rootScope.user.profile.id }, true)[0];

                        if (!permission)
                            return true;

                        return permission.type !== 'none';
                    }
                },

                convertRecordValue: function (filter) {
                    var value = null;
                    switch (filter.field.data_type) {
                        case "date_time":
                        case "date":
                            if (!filter.value) {
                                switch (filter.costumeDate) {
                                    case "costumeN":
                                        value = "today(" + filter.valueX + filter.nextprevdatetype + ")";
                                        break;
                                    case "costumeM":
                                        value = "this_month(" + filter.valueX + filter.nextprevdatetype + ")";
                                        break;
                                    case "costumeW":
                                        value = "this_week(" + filter.valueX + filter.nextprevdatetype + ")";
                                        break;
                                    case "costumeY":
                                        value = "this_year(" + filter.valueX + filter.nextprevdatetype + ")";
                                        break;
                                    case "now()":
                                        value = Date.now().toLocaleString();
                                        break;
                                    default:
                                        if (filter.value.name === 'costume') {
                                            value = filter.costumeDate;
                                        } else {
                                            value = filter.value.value;
                                        }
                                        break;
                                }
                            }
                            break;
                        case "multiselect":
                            var value2 = '';
                            angular.forEach(filter.value, function (picklistItem) {
                                value2 += picklistItem.label[$rootScope.language] + '|';
                            });
                            value = value2.slice(0, -1);
                            break;
                        case "checkbox":
                            value = filter.value.system_code.toString();
                            break;
                        case "tag":
                            var value2 = '';
                            angular.forEach(filter.value, function (item) {
                                value2 += item.text + '|';
                            });
                            value = value2.slice(0, -1);
                            break;
                        case "lookup":
                            if (filter.lookup_type === 'users') {
                                if (filter.value[0].id === 0)
                                    value = '[me]';
                                else
                                    value = filter.value.id;
                            } else {
                                value = filter.value;
                            }
                            break;
                        case "picklist":
                            value = filter.value.labelStr
                            break;
                        default:
                            value = filter.value;
                            break
                    }
                    return value;
                },

                buttonsFiltersConvertForRun: function (filterJson, record, that, index) {
                    filterJson.filters.forEach(function (data) {
                        if (data['group'] != null) {
                            that.buttonsFiltersConvertForRun(data['group'], record, that, index)
                        } else {
                            index = index + 1;
                            var result = false;
                            var filterField = data["field"];
                            if (!filterField) {
                                $rootScope.filterLogicButtons = "false";
                                return;
                            }
                            var filterFieldName = filterField.name;
                            var filterValue = data["value"];
                            var recordValue = record[filterFieldName];
                            filterValue = that.convertRecordValue(data);

                            if (typeof filterValue === 'object' && typeof recordValue === 'object' && data["field"].data_type === 'lookup' && data["field"].lookup_type === 'users') {
                                recordValue = recordValue.email;
                                for (var f = 0; f < filterValue.length; f++) {
                                    var filterValueData = filterValue[f].email;
                                    if (filterValueData === '[me]') {
                                        filterValue = $rootScope.user.email;
                                        break;
                                    } else if (filterValueData === recordValue) {
                                        filterValue = filterValueData;
                                        break;
                                    }
                                }
                            }

                            if (typeof recordValue === "boolean")
                                recordValue = recordValue.toString();

                            if (angular.isObject(recordValue) && recordValue.languages) {
                                recordValue = recordValue.languages[$rootScope.globalization.Label]["label"]
                            }

                            if (recordValue === null || recordValue === undefined || filterValue === null || filterValue === undefined) {
                                result = false;
                            }

                            if (filterValue && recordValue) {
                                switch (data.operator.name) {
                                    case "contains":
                                        result = recordValue.toString().contains(filterValue.toString());
                                        break;
                                    case "is":
                                    case "equals":
                                        result = recordValue.toString() === filterValue.toString();
                                        break;
                                    case "is_not":
                                    case "not_equal":
                                        result = recordValue.toString() !== filterValue.toString();
                                        break;
                                    case "greater":
                                        result = parseFloat(recordValue.toString()) > parseFloat(filterValue.toString());
                                        break;
                                    case "greater_equal":
                                        result = parseFloat(recordValue.toString()) >= parseFloat(filterValue.toString());
                                        break;
                                    case "less":
                                        result = parseFloat(recordValue.toString()) < parseFloat(filterValue.toString());
                                        break;
                                    case "less_equal":
                                        result = parseFloat(recordValue.toString()) <= parseFloat(filterValue.toString());
                                        break;
                                    case "empty":
                                        result = recordValue === "" || recordValue === null || recordValue === undefined;
                                        break;
                                    case "not_empty":
                                        //for picklist angular.isObject(recordValue)
                                        result = angular.isObject(recordValue) || (angular.isDefined(recordValue) && recordValue != null && recordValue !== "" && recordValue.length > 0);
                                        break;
                                    case "not_contain":
                                        result = !recordValue.toString().contains(filterValue.toString());
                                        break;
                                    case "starts_with":
                                        result = recordValue.toString().startsWith(filterValue.toString());
                                        break;
                                    case "ends_with":
                                        result = recordValue.toString().endsWith(filterValue.toString());
                                        break;
                                }
                            } else {
                                switch (data.operator.name) {
                                    case "empty":
                                        result = recordValue === "" || recordValue === null || recordValue === undefined;
                                        break;
                                }
                            }

                            $rootScope.filterLogicButtons = $rootScope.filterLogicButtons.replace(index, result.toString().toLocaleLowerCase());
                        }
                    })
                },

                hasActionButtonDisplayCriteria: function (module, actionButton, record) {
                    $rootScope.filterLogicButtons = null;
                    if (actionButton.filter_logic_json) {
                        $rootScope.filterLogicButtons = actionButton.filter_logic;
                        var filterJson = JSON.parse(actionButton.filter_logic_json);
                        if (filterJson.filters.length === 0)
                            return true;
                        var fullModule = $rootScope.modulus[module.name]
                        this.convertFilterFields(filterJson, fullModule, false);//filter içindeki field alanlarını doldurur.
                        var index = 0;
                        this.buttonsFiltersConvertForRun(filterJson, record, this, index);

                        $rootScope.filterLogicButtons = $rootScope.filterLogicButtons.replaceAll("or", "||");
                        $rootScope.filterLogicButtons = $rootScope.filterLogicButtons.replaceAll("and", "&&");
                        return eval($rootScope.filterLogicButtons)
                    } else
                        return true;
                },

                convertFilterFields: function (obj, currentModule, clear, operators) {
                    if (obj['filters'] != null && obj['filters'].length > 0) {
                        const that = this;
                        angular.forEach(obj['filters'], function (filter, index) {
                            if (filter['group'] != null) {
                                that.convertFilterFields(filter['group'], currentModule, clear);
                            } else if (filter.operator != null && filter.operator !== "") {
                                if (clear)
                                    filter.field = angular.isObject(filter.field) ? filter.field.value || filter.field.id : filter.field;
                                else {
                                    if (currentModule.fields)
                                        filter.field = angular.isObject(filter.field) ? filter.field : $filter('filter')(currentModule.fields, {
                                            id: filter.field,
                                            deleted: false
                                        }, true)[0];
                                    else {
                                        var value = angular.isObject(filter.field) ? filter.field.value : filter.field;
                                        if (value.contains('[') || value.contains(']')) {
                                            filter.field = value.replaceAll('[', '').replaceAll(']', '');
                                        }

                                        const splitData = angular.isObject(filter.field) ? filter.field.value : filter.field;
                                        const splitArray = splitData.contains('.') ? splitData.split('.') : [splitData];
                                        const res = $filter('filter')(currentModule, { label: splitArray[0] }, true)[0];
                                        if (res) {
                                            filter.field = $filter('filter')(res.properties, { name: splitArray[1] }, true)[0];
                                            filter.field.value = value;
                                        } else if (!angular.isObject(filter.field)) {
                                            filter.field = {
                                                value: splitData,
                                                data_type: 'text_single',
                                                operators: operators['text_single']
                                            };
                                        }
                                    }
                                }
                            }
                        });
                    }
                },

                getChartsTypes: function () {
                    var types = [
                        {
                            label: $filter('translate')('Report.Chart.ColumnChart2d'),
                            name: "column2d",
                        },
                        {
                            label: $filter('translate')('Report.Chart.ColumnChart3d'),
                            name: "column3d",
                        },
                        {
                            label: $filter('translate')('Report.Chart.LineChart'),
                            name: "line",
                        },
                        {
                            label: $filter('translate')('Report.Chart.AreaChart'),
                            name: "area2d",
                        },
                        {
                            label: $filter('translate')('Report.Chart.BarChart2d'),
                            name: "bar2d",
                        },
                        {
                            label: $filter('translate')('Report.Chart.BarChart3d'),
                            name: "bar3d",
                        },
                        {
                            label: $filter('translate')('Report.Chart.PieChart'),
                            name: "pie3d",
                        },
                        {
                            label: $filter('translate')('Report.Chart.DoughnutChart2d'),
                            name: "doughnut2d",
                        },
                        {
                            label: $filter('translate')('Report.Chart.DoughnutChart3d'),
                            name: "doughnut3d",
                        },
                        {
                            label: $filter('translate')('Report.Chart.ScrollColumnChart'),
                            name: "scrollcolumn2d",
                        },
                        {
                            label: $filter('translate')('Report.Chart.ScrollLineChart'),
                            name: "scrollline2d",
                        },
                        {
                            label: $filter('translate')('Report.Chart.ScrollAreaChart'),
                            name: "scrollarea2d",
                        },
                        {
                            label: $filter('translate')('Report.Chart.FunnelChart'),
                            name: "funnel",
                        },
                        {
                            label: $filter('translate')('Report.Chart.PyramidChart'),
                            name: "pyramid",
                        }

                    ];
                    return types;

                },

                chartFilter: function (filter) {
                    return $http.post(config.apiUrl + 'view/chart_filter', filter);
                },
                getChart: function (viewId) {
                    return $http.get(config.apiUrl + 'view/get_chart/' + viewId);
                },
                getWidget: function (viewId) {
                    return $http.get(config.apiUrl + 'view/get_widget/' + viewId);
                },
                saveView: function (type, data) {
                    if (type === 'edit')
                        return $http.put(config.apiUrl + 'view/update/' + data.id, data);

                    return $http.post(config.apiUrl + "view/create", data);
                },

                goToRecord: function (item, lookupType, showAnchor, dataItem, externalLink) {
                    if (item) {
                        var generateLookupId = item.split('.');
                        if (generateLookupId && generateLookupId.length > 2)
                            generateLookupId[generateLookupId.length - 1] = 'id';

                        generateLookupId = generateLookupId.join('.');
                        if (showAnchor) {
                            if (externalLink && externalLink != "") {
                                $window.open(unescape(externalLink) + "?id=" + dataItem[generateLookupId], '_self');
                            } else {
                                $window.open('#/app/record/' + lookupType + "?id=" + dataItem[generateLookupId], '_self');
                            }

                        }
                    }
                },

                getTagAndMultiDatas: function (dataItem, arrayList) {

                    if (arrayList && arrayList.length > 3) {
                        dataItem.show = true;
                        arrayList.splice(3, arrayList.length - 3);
                    } else
                        dataItem.show = false;

                    return arrayList;
                },

                getImageStyle: function (fieldName, moduleName) {
                    var module = $rootScope.modulus[moduleName];
                    if (module) {
                        var field = $filter('filter')(module.fields, { name: fieldName, deleted: false })[0];

                        if (field && field.image_size_list > 0) {
                            var style = {
                                'width': field.image_size_list + 'px',
                                'height': field.image_size_list + 'px'
                            };
                            return style;
                        }
                    }
                },

                getRatingCount: function (fieldName, moduleName) {
                    var module = $rootScope.modulus[moduleName];
                    if (module) {
                        var field = $filter('filter')(module.fields, { name: fieldName, deleted: false })[0];
                        if (field) {
                            //default 5
                            return field.max || 5;
                        }
                    }
                },
                getLocationUrl: function (coordinates) {
                    return "https://maps.googleapis.com/maps/api/staticmap?zoom=10&size=300x150&maptype=roadmap&markers=color:red|" + coordinates + "&key=" + googleMapsApiKey;
                },

                openCalendar: function (field) {
                    var data = undefined;
                    switch (field.data_type) {
                        case 'date':
                            data = 'kendoDatePicker';
                            break;
                        case 'date_time':
                            data = 'kendoDateTimePicker';
                            break;
                        case 'time':
                            data = 'kendoTimePicker';
                            break;
                    }

                    if (data) {
                        const dateInstance = angular.element(document.getElementById(field.name)).data(data);
                        if (dateInstance) {
                            dateInstance.open();
                        }
                    }
                },

                rangeRuleForForms: function () {
                    return function (input) {
                        const min = parseInt(input[0]['ariaValueMin'], 10);
                        const max = parseInt(input[0]["ariaValueMax"], 10);
                        var minLength = parseInt(input[0]['minLength'], 10);
                        var maxLength = parseInt(input[0]['maxLength'], 10);
                        const valueAsString = input.val();
                        const value = parseInt(valueAsString, 10);
                        const inputId = input[0]['id'] || input[0]['nextSibling']['id'];

                        if ((minLength === -1 || maxLength === -1) && inputId) {
                            var array = inputId.split('_');
                            if (array && array.length > 1)
                                var module = $filter('filter')($rootScope.modules, { id: parseInt(array[1]) }, true)[0];
                            if (module) {
                                var field = $filter('filter')(module.fields, { name: array[0] }, true)[0];
                                if (field) {
                                    minLength = field.validation.min_length || 0;
                                    maxLength = field.validation.max_length || (field.data_type === 'text_single' ? 50 : 16);
                                }
                            }
                        }

                        //text_single rule
                        if (!isNaN(minLength) && !isNaN(maxLength) && valueAsString && minLength > -1 && maxLength > -1) {
                            return minLength <= valueAsString.length && valueAsString.length <= maxLength;
                        }

                        if (isNaN(min) || isNaN(max) || isNaN(minLength) || isNaN(maxLength) || minLength === -1 || maxLength === -1) {
                            return true;
                        }

                        if (isNaN(value)) {
                            return true;
                        }
                        return min <= value && value <= max && minLength <= valueAsString.length && valueAsString.length <= maxLength;
                    };
                },

                setMinMaxValueForField: function (field) {
                    field.validation.min = field.validation.min || Number.MIN_SAFE_INTEGER;
                    field.validation.max = field.validation.max || Number.MAX_SAFE_INTEGER;
                    return field;
                },
                renderCurrencyFormat: function (symbol, decimalPlaces) {
                    symbol = symbol || "$";
                    var decimal = "";
                    for (var i = 0; i < decimalPlaces; i++) {
                        decimal += "0"
                    }

                    if (symbol === "₺") {
                        return "#." + decimal + symbol;
                    } else {
                        return symbol + "#." + decimal;
                    }
                },

                renderNumberDecimalFormat: function (fieldDecimals) {
                    var decimal = "";
                    for (var i = 0; i < fieldDecimals; i++) {
                        decimal += "0"
                    }
                    return "#." + decimal;
                },
                kendoCaleanderDataProcces: function (data, view, primayField, enddateisAvailable) {
                    var arr = []
                    for (var i = 0; i < data.length; i++) {
                        if (data[i][view.startdatefield]) {
                            var item = {};
                            item[view.startdatefield] = data[i][view.startdatefield];

                            if (enddateisAvailable)
                                item[view.enddatefield] = data[i][view.enddatefield];
                            else
                                item["end"] = data[i][view.startdatefield];

                            item["id"] = data[i]["id"];
                            item[primayField] = data[i][primayField];
                            arr.push(item)
                        }
                    }
                    return arr;
                },
                getLanguageOptions: function () {
                    return {
                        dataSource: $rootScope.globalizations,
                        dataTextField: "Language",
                        dataValueField: "Label",
                        filter: $rootScope.globalizations.length > 10 ? 'startswith' : null,
                        optionLabel: $filter('translate')('Common.Select')
                    };
                },
                getPicklistValue: function (item, value) {
                    if (item && value) {
                        // if (preview) {
                        //     const languages = item.languages;
                        //     for (var key in languages) {
                        //         if (languages.hasOwnProperty(key)) {
                        //             if (languages[key]['label'] === value && languages[$rootScope.globalization.Label])
                        //                 return languages[$rootScope.globalization.Label]['label'];
                        //         }
                        //     }
                        // }

                        //tenantLanguage = picklist_language
                        const language = $filter('filter')($rootScope.globalizations, function (globalization) {
                            return globalization.Culture.substr(0, 2) === tenantLanguage
                        }, true)[0];

                        var result = item.languages[language ? language.Label : $rootScope.globalization.Label];

                        if (result && result['label'] === value)
                            return item.languages[$rootScope.globalization.Label]['label'];
                    }
                },

                getMenuList: function () {
                    return $http.get(config.apiUrl + 'menu/get_all');
                },

                getMenuItemsByMenuId: function (menuId) {
                    return $http.get(config.apiUrl + 'menu/get_menu_items/' + menuId);
                },

                proccesMenuItems: function (menuItems) {
                    var items = [];
                    for (var i = 0; i < menuItems.length; i++) {
                        var item = menuItems[i];
                        item.expanded = true;
                        if (item.parent)
                            continue;

                        if (item.menu_items) {
                            item.items = item.menu_items;
                        }

                        items.push(item)
                    }
                    items = $filter('orderBy')(items, 'order', false);
                    return items;
                },

                moduleFilterById: function (moduleId) {
                    return $filter('filter')($rootScope.modules, { id: moduleId }, true);
                },
                getAllTenantSettingsByType: function (settingType, userId) {
                    return $http.get(config.apiUrl + 'settings/get_all/' + settingType + (userId ? '?user_id=' + userId : ''));
                }
            };
        }]);

