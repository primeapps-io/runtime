angular.module('primeapps')

    .factory('ModuleService', ['$rootScope', '$http', 'config', '$q', '$filter', '$cache', '$window', 'helper', 'operations', 'ngTableParams', 'icons', 'dataTypes', 'operators', 'activityTypes', 'transactionTypes', 'yesNo', 'components',
        function ($rootScope, $http, config, $q, $filter, $cache, $window, helper, operations, ngTableParams, icons, dataTypes, operators, activityTypes, transactionTypes, yesNo, components) {
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

                createModuleRelation: function (relation, moduleId) {
                    return $http.post(config.apiUrl + 'module/create_relation/' + moduleId, relation);
                },

                updateModuleRelation: function (relation, moduleId) {
                    return $http.put(config.apiUrl + 'module/update_relation/' + moduleId + '/' + relation.id, relation);
                },

                deleteModuleRelation: function (id) {
                    return $http.delete(config.apiUrl + 'module/delete_relation/' + id);
                },

                createModuleDependency: function (dependency, moduleId) {
                    return $http.post(config.apiUrl + 'module/create_dependency/' + moduleId, dependency);
                },

                updateModuleDependency: function (dependency, moduleId) {
                    return $http.put(config.apiUrl + 'module/update_dependency/' + moduleId + '/' + dependency.id, dependency);
                },

                deleteModuleDependency: function (id) {
                    return $http.delete(config.apiUrl + 'module/delete_dependency/' + id);
                },

                getRecord: function (module, id, ignoreNotFound) {
                    return $http.get(config.apiUrl + 'record/get/' + module + '/' + id, { ignoreNotFound: ignoreNotFound });
                },

                findRecords: function (module, request) {
                    $rootScope.activeModuleName = null;
                    return $http.post(config.apiUrl + 'record/find/' + module, request);
                },

                insertRecord: function (module, record) {
                    return $http.post(config.apiUrl + 'record/create/' + module + '?timezone_offset=' + new Date().getTimezoneOffset() * -1, record);
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
                    if ($rootScope.branchAvailable && module === 'calisanlar') {
                        var deferred = $q.defer();

                        var that = this;
                        this.getRecord(module, id)
                            .then(function (response) {
                                if (response.data) {
                                    var user = $filter('filter')($rootScope.workgroup.users, { email: response.data['e_posta'] }, true)[0];
                                    if (user) {
                                        that.deleteUser(user, $rootScope.workgroup.tenant_id);
                                    }
                                    $http.delete(config.apiUrl + 'record/delete/' + module + '/' + id)
                                        .then(function (response) {
                                            deferred.resolve(response);
                                            return deferred.promise;
                                        });
                                }
                            });
                        return deferred.promise;
                    } else {
                        return $http.delete(config.apiUrl + 'record/delete/' + module + '/' + id);
                    }
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
                    for (var i = 0; i < request.records.length; i++) {
                        delete request.records[i].shared_users_edit;
                        delete request.records[i].shared_users;
                        delete request.records[i].shared_user_groups_edit;
                        delete request.records[i].shared_user_groups;
                        delete request.records[i].shared_read;
                    }

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

                setViewState: function (viewState, moduleId, id) {
                    if (id)
                        viewState.id = id;

                    viewState.module_id = moduleId;

                    return $http.put(config.apiUrl + 'view/set_view_state', viewState);
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

                approveProcessRequest: function (operation_type, moduleName, id) {
                    return $http.put(config.apiUrl + 'process_request/approve', {
                        record_id: id,
                        module_name: moduleName,
                        operation_type: operation_type
                    });
                },

                rejectProcessRequest: function (operation_type, moduleName, message, id) {
                    return $http.put(config.apiUrl + 'process_request/reject', {
                        record_id: id,
                        module_name: moduleName,
                        operation_type: operation_type,
                        message: message
                    });
                },

                deleteProcessRequest: function (moduleId, id) {
                    return $http.put(config.apiUrl + 'process_request/delete', { record_id: id, module_id: moduleId });
                },

                send_approval: function (operation_type, moduleName, id) {
                    return $http.put(config.apiUrl + 'process_request/send_approval', {
                        record_id: id,
                        module_name: moduleName,
                        operation_type: operation_type
                    });
                },

                sendApprovalManuel: function (request) {
                    return $http.post(config.apiUrl + 'process_request/send_approval_manuel', request);
                },

                getProcess: function (id) {
                    return $http.get(config.apiUrl + 'process/get/' + id);
                },

                getAllProcess: function (id) {
                    return $http.get(config.apiUrl + 'process/get_all');
                },

                sendSMS: function (moduleId, ids, query, isAllSelected, message, phoneField, templateId) {
                    return $http.post(config.apiUrl + 'messaging/send_sms', {
                        "module_id": moduleId,
                        "Ids": ids,
                        "Query": query,
                        "IsAllSelected": isAllSelected,
                        "Message": message,
                        "phone_field": phoneField,
                        "template_id": templateId
                    });
                },

                sendEMail: function (moduleId, ids, query, isAllSelected, templateId, emailField, Cc, Bcc, senderAlias, senderEMail, providerType, attachmentContainer, subject, attachmentLink, attachmentName) {
                    return $http.post(config.apiUrl + 'messaging/send_email_job', {
                        "module_id": moduleId,
                        "Ids": ids,
                        "Query": query,
                        "is_all_selected": isAllSelected,
                        "template_id": templateId,
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
                        var linkPrefix = '#/app/module/';

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

                        if (type === 'rehber') {
                            linkPrefix = '#/app/';
                            type = 'directory';
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

                    var recordsCopy = angular.copy(records);

                    for (var i = 0; i < recordsCopy.length; i++) {
                        var record = recordsCopy[i];

                        var recordProcessed = {};
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
                                recordProcessed.fields.push(recordProcessedField);
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
                                lookupRecord.primary_value = lookupRecord['name_' + $rootScope.language];
                            } else if (field.lookup_type === 'roles') {
                                lookupRecord.primary_value = lookupRecord['label_' + $rootScope.user.tenant_language];
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

                                    var picklistItem = $filter('filter')(picklists[field.picklist_id], {
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

                getPicklists: function (module, withRelatedPicklists) {
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
                                    modulePicklistItem.labelStr = moduleItem['label_' + $rootScope.user.tenant_language + '_singular'];
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

                    var hasPermission = lookupType !== 'users' && lookupType !== 'profiles' && lookupType !== 'roles' ? helper.hasPermission(lookupType, operations.read) : true;

                    if (!hasPermission && !($rootScope.branchAvailable && lookupType === 'branches')) {
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
                        findRequest.limit = 1000;
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

                    var lookupModuleFields = that.moduleFieldsConvertByKey(lookupModule.fields);

                    //lookup field filters (from field_filters table)
                    if (field.filters) {
                        var no = findRequest.filters.length;
                        for (var z = 0; z < field.filters.length; z++) {
                            var filter = field.filters[z];
                            no++;
                            var findRecordValue;

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

                    this.setCustomCalculations(module, record);
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
                            case 'text_single':
                            case 'text_multi':
                            case 'number':
                            case 'number_decimal':
                            case 'currency':
                            case 'url':
                            case 'email':
                            case 'image':
                                record[fieldName] = field.default_value;
                                break;
                            case 'date':
                            case 'time':
                            case 'date_time':
                                if (field.default_value === '[now]') {
                                    record[fieldName] = new Date().toISOString();
                                }else
                                    record[fieldName] =  new Date(field.default_value);
                                
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
                                    var picklistId = picklistIds[j];
                                    var picklistRecordMultiselect = $filter('filter')(picklists[field.picklist_id], { id: parseInt(picklistId) }, true)[0];

                                    if (!picklistRecordMultiselect) {
                                        picklistRecordMultiselect = {};
                                        picklistRecordMultiselect.id = record[fieldName];
                                        picklistRecordMultiselect.type = field.picklist_id;
                                        picklistRecordMultiselect.label = {};
                                        picklistRecordMultiselect.label.tr = record[fieldName];
                                        picklistRecordMultiselect.label.en = record[fieldName];
                                        picklistRecordMultiselect.labelStr = record[fieldName];
                                        picklistRecordMultiselect.required = false;
                                        picklistRecordMultiselect.order = 9999;

                                        if (!picklistRecordMultiselect.inactive)
                                            helper.getPicklists([field.picklist_id], true);
                                    }

                                    if (field.default_value)
                                        record[fieldName].push(picklistRecordMultiselect);
                                }
                                break;
                            case 'checkbox':
                                if (field.default_value === 'true')
                                    record[field.name] = true;
                                else
                                    record[field.name] = false;
                                break;
                        }

                        this.setDisplayDependency(module, record);
                    }

                    //Set default currency
                    var currencyField = $filter('filter')(module.fields, { name: 'currency', deleted: '!true' })[0];

                    if ((module.name === 'products' || module.name === 'quotes' || module.name === 'sales_orders' || module.name === 'purchase_orders') && !record['currency'] && currencyField) {
                        if ($rootScope.currencySymbol) {
                            var currencySymbol = angular.copy($rootScope.currencySymbol);

                            if (currencySymbol === '\u20ba')
                                currencySymbol = '₺';

                            var picklistCurrencyItem = $filter('filter')(picklists[currencyField.picklist_id], { value: currencySymbol }, true)[0];
                            record['currency'] = picklistCurrencyItem;

                            this.setDisplayDependency(module, record);
                        }
                    }
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

                                        that.lookup(parentValue, lookupField, record, additionalFields, true)
                                            .then(function (data) {
                                                if (data[0]) {
                                                    var lookupRecord = data[0];
                                                    lookupRecord.primary_value = lookupRecord[childField.lookupModulePrimaryField.name];
                                                    record[dependency.child_field] = lookupRecord;
                                                    childField.valueChangeDontRun = true;

                                                    that.customActions(module, record, scope.moduleForm);
                                                    scope.$broadcast('angucomplete-alt:changeInput', dependency.child_field, lookupRecord);
                                                }
                                            });
                                    }

                                    break;
                            }
                        } else {
                            record[dependency.child_field] = undefined;
                            scope.$broadcast('angucomplete-alt:clearInput', dependency.child_field);
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
                                var recordValueItem = recordValue[j];

                                var currentDependentValue = $filter('filter')(dependency.values, recordValueItem.id, true)[0];

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
                            delete record[dependency.dependent_field];
                            continue;
                        }

                        if (dependency.otherwise && dependentValue) {
                            dependent.hidden = true;
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

                customActions: function (module, record, moduleForm, picklists, scope) {
                    //if ($rootScope.user.appId === 4) {//Ofisim IK
                    if (module.name === 'izinler') {
                        var calculate = function (that) {
                            if (record['calisan'] && record['izin_turu'] && record['izin_turu_data']) {
                                var startOf = moment().date(1).month(0).year(moment().year()).format('YYYY-MM-DD');

                                //Yıllık izin seçilmiş ise işe bağladığı tarih dikkate alınarak 1 yıllık kullandığı izinleri çekmek için tarih hesaplanıyor.
                                if (record['izin_turu_data']['yillik_izin'] && record['calisan_data']) {
                                    var jobStart = moment(record['calisan_data']['ise_baslama_tarihi']);
                                    var jobDay = jobStart.get('date');
                                    var jobMonth = jobStart.get('month');
                                    var currentYear = moment().get('year');

                                    var currentDate = moment().date(jobDay).month(jobMonth).year(currentYear).format('YYYY-MM-DD');

                                    if (moment(currentDate).isAfter(moment().format('YYYY-MM-DD'))) {
                                        currentYear -= 1;
                                    }

                                    startOf = moment().date(jobDay).month(jobMonth).year(currentYear).format('YYYY-MM-DD');
                                }

                                if (record['izin_turu_data']['her_ay_yenilenir']) {
                                    startOf = moment().date(1).month(moment().month()).year(moment().year()).format('YYYY-MM-DD');
                                }

                                /*var filterRequest = {
                                 fields: ["hesaplanan_alinacak_toplam_izin", "baslangic_tarihi", "process.process_requests.process_status"],
                                 filters: [
                                 { field: 'calisan', operator: 'equals', value: record['calisan'].id, no: 1 },
                                 { field: 'izin_turu', operator: 'equals', value: record['izin_turu'].id, no: 2 },
                                 { field: 'baslangic_tarihi', operator: 'greater_equal', value: startOf, no: 3 },
                                 { field: 'deleted', operator: 'equals', value: false, no: 4 },
                                 { field: 'process.process_requests.process_status', operator: 'not_equal', value: 3, no: 5 }
                                 ],
                                 limit: 999999
                                 };*/

                                var filterRequest = {
                                    fields: ["hesaplanan_alinacak_toplam_izin", "baslangic_tarihi", "bitis_tarihi", "izin_turu", "process.process_requests.process_status"],
                                    filters: [
                                        { field: 'calisan', operator: 'equals', value: record['calisan'].id, no: 1 },
                                        { field: 'baslangic_tarihi', operator: 'greater_equal', value: startOf, no: 2 },
                                        { field: 'deleted', operator: 'equals', value: false, no: 3 },
                                        {
                                            field: 'process.process_requests.process_status',
                                            operator: 'not_equal',
                                            value: 3,
                                            no: 4
                                        }//,
                                        //{ field: 'process.process_requests.process_status', operator: 'empty', value: '-', no: 5 }
                                    ],
                                    //filter_logic: '(((1 and 2) and 3) and (4 or 5))',
                                    limit: 999999
                                };


                                that.findRecords('izinler', filterRequest)
                                    .then(function (response) {
                                        var totalUsed = 0;
                                        record['alinan_izinler'] = response.data;

                                        if (record['izin_turu_data'] && !record['izin_turu_data']['yillik_izin']) {
                                            if (response.data.length > 0) {
                                                var filteredLeaves = $filter('filter')(response.data, { izin_turu: record['izin_turu'].id }, true);
                                                angular.forEach(filteredLeaves, function (izin) {
                                                    if (izin["hesaplanan_alinacak_toplam_izin"]) {
                                                        totalUsed += izin["hesaplanan_alinacak_toplam_izin"];
                                                    }
                                                });
                                            } else {
                                                delete record['alinan_izinler'];
                                                record['mevcut_kullanilabilir_izin'] = record['izin_turu_data']['yillik_hakedilen_limit_gun'];
                                            }

                                            if (record['izin_turu_data']['saatlik_kullanim_yapilir']) {
                                                var calismaSaati = record['izin_turu_data']['toplam_calisma_saati'];
                                                var hakedilen = record['izin_turu_data']['yillik_hakedilen_limit_gun'];
                                                if (record['izin_turu_data']['ogle_tatilini_dikkate_al']) {
                                                    var ogleTatili = parseFloat(moment.duration(moment(record['izin_turu_data']['ogle_tatili_bitis']).diff(moment(record['izin_turu_data']['ogle_tatili_baslangic']))).asHours().toFixed(2));
                                                    calismaSaati = calismaSaati - ogleTatili;
                                                }
                                                var kalan = hakedilen * calismaSaati - totalUsed;
                                                record['mevcut_kullanilabilir_izin'] = kalan;
                                            } else {
                                                record['mevcut_kullanilabilir_izin'] = record['izin_turu_data']['yillik_hakedilen_limit_gun'] - totalUsed;
                                            }
                                        } else if (record['izin_turu'] && record['izin_turu_data'] && record['calisan_data'] && record['izin_turu_data']['yillik_izin'] && record['calisan']['e_posta'] === record['calisan_data']['e_posta']) {
                                            var checkUsedCount = 0;

                                            //var filteredLeaves = $filter('filter')(record["alinan_izinler"], function (izin) {
                                            //    return izin['izin_turu'] === record['izin_turu'].id && izin['process.process_requests.process_status'] === 1;
                                            //}, true);

                                            //angular.forEach(filteredLeaves, function (izin) {
                                            //    if (izin["hesaplanan_alinacak_toplam_izin"]) {
                                            //        checkUsedCount += izin["hesaplanan_alinacak_toplam_izin"];
                                            //    }
                                            //});

                                            /*if (record["izin_turu_data"]["izin_hakki_onay_sureci_sonunda_dusulsun"]) {*/
                                            record['mevcut_kullanilabilir_izin'] = record['calisan_data']['kalan_izin_hakki'] ? record['calisan_data']['kalan_izin_hakki'] : 0.0;
                                            /*}
                                             else {
                                             angular.forEach(record['alinan_izinler'], function (izinler) {
                                             if (izinler['hesaplanan_alinacak_toplam_izin'] && izinler['izin_turu'] === record['izin_turu'].id && izinler['process.process_requests.process_status'] === 1)
                                             checkUsedCount += izinler['hesaplanan_alinacak_toplam_izin'];
                                             });
                                             record['mevcut_kullanilabilir_izin'] = record['calisan_data']['kalan_izin_hakki'] ? record['calisan_data']['kalan_izin_hakki'] - checkUsedCount : 0.0;
                                             }*/
                                        }
                                    });
                            }
                        };
                        var that = this;
                        if (record['calisan']) {
                            var calisalarModuleName = $filter('filter')($rootScope.modules, { name: "calisanlar" }, true)[0];
                            if (!calisalarModuleName)
                                calisalarModuleName = "human_resources";
                            else
                                calisalarModuleName = "calisanlar";

                            this.getRecord(calisalarModuleName, record['calisan'].id)
                                .then(function (response) {
                                    var account = response.data;

                                    record['calisan_data'] = account;
                                    record['goreve_baslama_tarihi'] = account['ise_baslama_tarihi'];
                                    record['dogum_tarihi'] = account['dogum_tarihi'];
                                    calculate(that);
                                });
                        }

                        if (record['izin_turu'] && (!record['izin_turu_data'] || (record['izin_turu_data'] && record['izin_turu_data'].id != record['izin_turu'].id))) {
                            record['mevcut_kullanilabilir_izin'] = 0.0;

                            this.getRecord('izin_turleri', record['izin_turu'].id)
                                .then(function (response) {
                                    var izin = response.data;
                                    record['izin_turu_data'] = izin;
                                    if (scope) {
                                        scope['izinTuruData'] = izin;
                                        calculate(that);
                                        that.setCustomCalculations(module, record, picklists, scope);
                                    }
                                });
                        }
                    }
                },

                customValidations: function (module, record, checkUsed) {
                    //if ($rootScope.user.appId === 4) {//Ofisim IK
                    if (module.name === 'izinler') {
                        var izin_turu = record['izin_turu_data'];
                        /*
                         * İzin tarihlerinin aynı olup olmadığı kontrol ediliyor. Aynı ise uyarı veriliyor.
                         * */

                        if (moment(record["baslangic_tarihi"]).isSame(record["bitis_tarihi"])) {
                            return $filter('translate')('Leave.Validations.SameDate');
                        }

                        if (record['talep_edilen_izin'] <= 0) {
                            return $filter('translate')('Leave.Validations.Requested0OfDaysNot')
                        }

                        //Aynı Tarihlerde Başka izin varmı diye kontrol ediliyor.
                        if (record['alinan_izinler'] && record['alinan_izinler'].length > 0) {
                            for (var i = 0; i < record['alinan_izinler'].length; i++) {

                                if (record.id && record.id === record['alinan_izinler'][i].id) {
                                    continue;
                                }

                                var startDate = moment(moment(Date.parse(moment(record['baslangic_tarihi']))).format('YYYY-MM-DDTHH:mm:ss'));
                                var endDate = moment(moment(Date.parse(moment(record['bitis_tarihi']))).format('YYYY-MM-DDTHH:mm:ss'));

                                if (startDate.isBetween(moment(record['alinan_izinler'][i].baslangic_tarihi), moment(record['alinan_izinler'][i].bitis_tarihi), null, '[)') ||
                                    endDate.isBetween(moment(record['alinan_izinler'][i].baslangic_tarihi), moment(record['alinan_izinler'][i].bitis_tarihi), null, '(]') ||
                                    (startDate.isSameOrBefore(moment(record['alinan_izinler'][i].baslangic_tarihi)) && endDate.isSameOrAfter(moment(record['alinan_izinler'][i].bitis_tarihi)))
                                ) {
                                    if (record["alinan_izinler"][i].id !== record.id) {
                                        return $filter('translate')('Leave.Validations.AlreadyHave');
                                    }
                                }
                            }
                        }

                        /*
                         * İLK İZİN KULLANIMI HAKEDİŞ ZAMANI KONTROLÜ
                         * Seçilen izin türünün kullanılabilmesi için ilk izin kullanımı hakediş zamanının (seçilen tipe özel) kullanıcının işe giriş tarihinden küçük olması gerekmektedir.
                         * Yıllık izin ve diğer izin türleri için ortak kullanılır.
                         * */
                        if (izin_turu["ilk_izin_kullanimi_hakedis_zamani_ay"] && izin_turu["ilk_izin_kullanimi_hakedis_zamani_ay"] !== null && izin_turu["ilk_izin_kullanimi_hakedis_zamani_ay"] !== 0) {
                            var start_day_month = moment().diff(record["goreve_baslama_tarihi"], "months");
                            var rule_day_month = izin_turu["ilk_izin_kullanimi_hakedis_zamani_ay"];
                            if (start_day_month < rule_day_month) {
                                return $filter('translate')('Leave.Validations.FirstLeaveUse', { month: rule_day_month });
                            }
                        }

                        if (izin_turu["yillik_izin"]) {
                            /*
                             * Mevcut izin hakkı almaya çalıştığı izin için yeterli mi diye kontrol ediliyor.
                             * Değil ise izin borçlanma seçeneği mevcutmu diye kontrol ediliyor.
                             * */

                            if (record['mevcut_kullanilabilir_izin'] === null)
                                return $filter('translate')('Leave.Validations.LeaveEnd');

                            /*var checkUsedCount = 0;

                             if (checkUsed) {
                             if (record["izin_turu_data"]["izin_hakki_onay_sureci_sonunda_dusulsun"]) {
                             record['mevcut_kullanilabilir_izin'] = record['calisan_data']['kalan_izin_hakki'] ? record['calisan_data']['kalan_izin_hakki'] : 0.0;
                             } else {
                             angular.forEach(record['alinan_izinler'], function (izinler) {
                             if (izinler['hesaplanan_alinacak_toplam_izin'] && izinler['izin_turu'] === record['izin_turu'].id && izinler['process.process_requests.process_status'] !== 3)
                             checkUsedCount += izinler['hesaplanan_alinacak_toplam_izin'];
                             });
                             record['mevcut_kullanilabilir_izin'] = record['calisan_data']['kalan_izin_hakki'] ? record['calisan_data']['kalan_izin_hakki'] - checkUsedCount : 0.0;
                             }
                             }*/

                            /*
                             * Record edit yapılırken hesaplanan alanın değeri değişmiş ise kalan izin hakkı tekrar kontrol edilecek.
                             * Yeni bir kayıt için normal kontrol devam edecek.
                             * Record kayıt edilip sonra onaya gönderildiği için record.process_status varmı diye kontrol ediliyor. Yoksa standart süreç işliyor.
                             * */
                            if (record.id && record.process_status) {
                                var filteredLeave = $filter('filter')(record['alinan_izinler'], { id: record.id }, true)[0];

                                if (filteredLeave && record['hesaplanan_alinacak_toplam_izin'] > filteredLeave['hesaplanan_alinacak_toplam_izin'] && record['mevcut_kullanilabilir_izin'] + filteredLeave['hesaplanan_alinacak_toplam_izin'] < record['hesaplanan_alinacak_toplam_izin']) {
                                    if (!izin_turu["izin_borclanma_yapilabilir"]) {
                                        return $filter('translate')('Leave.Validations.LeaveEnd');
                                    }
                                }
                            } else if (record['mevcut_kullanilabilir_izin'] === 0 || (record['mevcut_kullanilabilir_izin'] /*- checkUsedCount*/ < record['hesaplanan_alinacak_toplam_izin'])) {
                                if (!izin_turu["izin_borclanma_yapilabilir"]) {
                                    return $filter('translate')('Leave.Validations.LeaveEnd');
                                }
                            }

                            record['mevcut_kullanilabilir_izin'] = record['calisan_data']['kalan_izin_hakki'] || 0.0;

                            delete record["goreve_baslama_tarihi"];
                            delete record['calisan_data'];
                            delete record["izin_turu_data"];
                            delete record["dogum_tarihi"];
                            delete record['alinan_izinler'];

                            return "";
                        } else {
                            /*
                             * İzin tipinin yıllık hakedilen limiti doldurup doldurmadığı kontrol ediliyor.
                             * Bu yıl aynı türden aldığı(örneğin mazeret izni) izinler yıllık hakedilen izin miktarını (yıllık hakedilen mazeret izni) geçiyor mu diye kontrol edilecek.
                             * */
                            var filteredLeaves = $filter('filter')(record['alinan_izinler'], function (izin) {
                                return izin.izin_turu === record['izin_turu'].id && izin['process.process_requests.process_status'] !== null && (!record.id || (record.id && record.id !== izin.id));
                            });

                            if (izin_turu["yillik_hakedilen_limit_gun"] && izin_turu["yillik_hakedilen_limit_gun"] !== 0) {
                                var totalUsed = 0;
                                if (filteredLeaves && filteredLeaves.length > 0) {
                                    angular.forEach(filteredLeaves, function (izin) {
                                        if (izin["hesaplanan_alinacak_toplam_izin"])
                                            totalUsed += izin["hesaplanan_alinacak_toplam_izin"];
                                    });
                                }

                                if (record['izin_turu_data'] && record['izin_turu_data']['saatlik_kullanim_yapilir']) {
                                    var workHour = record['izin_turu_data']['toplam_calisma_saati'];

                                    if (record['izin_turu_data']['ogle_tatilini_dikkate_al']) {
                                        var ogleTatili = parseFloat(moment.duration(moment(record['izin_turu_data']['ogle_tatili_bitis']).diff(moment(record['izin_turu_data']['ogle_tatili_baslangic']))).asHours().toFixed(2));
                                        workHour = workHour - ogleTatili;
                                    }

                                    var maxLimit = workHour * izin_turu['yillik_hakedilen_limit_gun'];

                                    if (maxLimit < totalUsed + record['hesaplanan_alinacak_toplam_izin']) {
                                        if (record['izin_turu_data']['her_ay_yenilenir']) {
                                            return $filter('translate')('Leave.Validations.NotHaveLimitHourMonth', {
                                                total_hour: maxLimit,
                                                remaining_hour: maxLimit - totalUsed
                                            });
                                        }
                                        return $filter('translate')('Leave.Validations.NotHaveLimitHour', {
                                            total_hour: maxLimit,
                                            remaining_hour: maxLimit - totalUsed
                                        });
                                    }
                                } else if (totalUsed + record['hesaplanan_alinacak_toplam_izin'] > izin_turu["yillik_hakedilen_limit_gun"]) {
                                    if (record['izin_turu_data']['her_ay_yenilenir']) {
                                        return $filter('translate')('Leave.Validations.NotHaveLimitDayMonth', {
                                            total_hour: izin_turu["yillik_hakedilen_limit_gun"],
                                            remaining_hour: izin_turu["yillik_hakedilen_limit_gun"] - totalUsed
                                        });
                                    }
                                    return $filter('translate')('Leave.Validations.NotHaveLimitDay', {
                                        total_hour: izin_turu["yillik_hakedilen_limit_gun"],
                                        remaining_hour: izin_turu["yillik_hakedilen_limit_gun"] - totalUsed
                                    });
                                }
                            }

                            /*
                             * Seçilen izin türü için tek sefer de alabileceği en fazla izin hakkı kontrol ediliyor. Eğer bu alan set li değil veya 0 ise dikkate alınmıyor.
                             * */

                            if (izin_turu["tek_seferde_alinabilecek_en_fazla_izin_gun"] !== null && izin_turu["tek_seferde_alinabilecek_en_fazla_izin_gun"] !== 0) {

                                if (record['izin_turu_data'] && record['izin_turu_data']["saatlik_kullanim_yapilir"]) {
                                    var workHour = record['izin_turu_data']["toplam_calisma_saati"];
                                    if (record['izin_turu_data']['ogle_tatilini_dikkate_al']) {
                                        var ogleTatili = parseFloat(moment.duration(moment(record['izin_turu_data']['ogle_tatili_bitis']).diff(moment(record['izin_turu_data']['ogle_tatili_baslangic']))).asHours().toFixed(2));
                                        workHour = workHour - ogleTatili;
                                    }
                                    var maxLimit = workHour * izin_turu["tek_seferde_alinabilecek_en_fazla_izin_gun"];
                                    if (maxLimit < record["talep_edilen_izin"]) {
                                        return $filter('translate')('Leave.Validations.OneGoHour', { total_hour: izin_turu["tek_seferde_alinabilecek_en_fazla_izin_gun"] * maxLimit });
                                    }
                                } else if (izin_turu["tek_seferde_alinabilecek_en_fazla_izin_gun"] < record["talep_edilen_izin"]) {
                                    return $filter('translate')('Leave.Validations.OneGoDay', { total_hour: izin_turu["tek_seferde_alinabilecek_en_fazla_izin_gun"] });
                                }
                            }

                            /*
                             * Eğer seçilen izin türü resmi tatiller ile birleştirilemiyor ise;
                             * İznin başlangıç günün den 1 çıkarılır, setlenen yeni gün haftasonuna denk gelmiyor ve yine de tatil günü olarak geçiyor ise tatil gününü birleştirdiği için bu izin reddedilir.
                             * Aynı işlem iznin bitiş zamanı için de yapılır ama bu sefer bitiş zamanına 1 eklenerek yapılır.
                             * */
                            if (izin_turu['resmi_tatiller_ile_birlestirilebilir'] === null || izin_turu['resmi_tatiller_ile_birlestirilebilir'] === false) {
                                var start_day = moment(record["baslangic_tarihi"]);
                                var finish_day = moment(record["bitis_tarihi"]);
                                var start_day_added = start_day.add(-1, 'days')
                                //var day = moment(start_day_added).format('dddd');

                                var workSaturdays = $filter('filter')($rootScope.moduleSettings, { key: 'work_saturdays' }, true);
                                if (workSaturdays.length > 0 && workSaturdays[0].value === 't') {
                                    workSaturdays = true;
                                } else {
                                    workSaturdays = false;
                                }

                                var isWeekend = moment(start_day_added).isoWeekday() === 7;

                                if (!isWeekend && !workSaturdays) {
                                    isWeekend = moment(start_day_added).isoWeekday() === 6;
                                }

                                var isBusinessDay = moment(moment(start_day_added).format('YYYY-MM-DD')).isBusinessDay();

                                if (isBusinessDay) {
                                    for (var i = 0; i < $rootScope.holidaysData.length; i++) {
                                        var holiday = $rootScope.holidaysData[i];
                                        if (holiday.half_day && moment(holiday.date).isBusinessDay() && moment(start_day_added.format("YYYY-MM-DD")).isSame(moment(holiday.date).format("YYYY-MM-DD"))) {
                                            isBusinessDay = false;
                                        }
                                    }
                                }

                                if (!isWeekend && !isBusinessDay) {
                                    return $filter('translate')('Leave.Validations.InfusibleWithHolidays');
                                }
                                var finish_day_added = finish_day;

                                if (izin_turu["saatlik_kullanim_yapilir"]) {
                                    finish_day_added = finish_day.add(1, 'days');
                                }

                                //var finish_day_added = finish_day.add(1, 'days');
                                //day = moment(finish_day_added).weekday();
                                isWeekend = moment(finish_day_added).isoWeekday() === 7;
                                if (!isWeekend && !workSaturdays) {
                                    isWeekend = moment(finish_day_added).isoWeekday() === 6;
                                }

                                isBusinessDay = moment(moment(finish_day_added).format('YYYY-MM-DD')).isBusinessDay();

                                if (isBusinessDay) {
                                    for (var i = 0; i < $rootScope.holidaysData.length; i++) {
                                        var holiday = $rootScope.holidaysData[i];
                                        if (holiday.half_day && moment(holiday.date).isBusinessDay() && moment(finish_day_added.format("YYYY-MM-DD")).isSame(moment(holiday.date).format("YYYY-MM-DD"))) {
                                            isBusinessDay = false;
                                        }
                                    }
                                }

                                if (!isWeekend && !isBusinessDay) {
                                    return $filter('translate')('Leave.Validations.InfusibleWithHolidays');
                                }
                            }

                            /*
                             * Doğum günü izni için kurallar kontrol ediliyor.
                             *
                             * */

                            if (izin_turu["dogum_gunu_izni"]) {
                                //Etiya Özel Doğum Günü izni 1 hafta önce veya 3 hafta sonra kullanılması durumu
                                if (izin_turu['1_hafta_once_ve_3_hafta_sonra_arasinda_kullanilir']) {
                                    var current = moment(record["baslangic_tarihi"]);
                                    var first = moment(record["dogum_tarihi"]).set('year', current.get('year')).subtract(1, 'weeks');
                                    var end = moment(record["dogum_tarihi"]).set('year', current.get('year')).add(3, 'weeks');

                                    var dateChecker = moment(current.format('YYYY-MM-DD')).isBetween(first.format('YYYY-MM-DD'), end.format('YYYY-MM-DD'), null, '[]');
                                    if (!dateChecker) {
                                        return 'Mutlu Yıllar! Doğumgünü iznini doğum gününün 1 hafta öncesinden başlayarak 3 hafta sonrasına kadar olan 4 haftalık süre diliminde kullanabilirsin. İzin tarihini değiştirerek tekrar denemelisin.';
                                    }

                                } else {
                                    var dogum_tarihi = moment(record["dogum_tarihi"]);
                                    var start_day = moment(record["baslangic_tarihi"]);
                                    dogum_tarihi = moment(dogum_tarihi.year(start_day.year()).toISOString());

                                    if (izin_turu["dogum_gunu_izni_kullanimi"].includes("15")) {
                                        var calculatedField = start_day.diff(dogum_tarihi, 'days');

                                        if (calculatedField < 0)
                                            calculatedField = calculatedField * -1;

                                        if (calculatedField > 15) {
                                            return $filter('translate')('Leave.Validations.BirthDayLimitDay');
                                        }
                                    } else if (dogum_tarihi.month() !== start_day.month()) {
                                        return $filter('translate')('Leave.Validations.BirthDayLimitMonth');
                                    }
                                }

                            }

                            //Etiya Özel Mazeret İzni Arka Arkaya 3 gün Alınamaz.
                            if (izin_turu['3_gun_arka_arkaya_kullanilamasin'] && filteredLeaves && filteredLeaves.length > 0) {
                                if (filteredLeaves.length > 0) {
                                    function calculateDate(date, operation) {
                                        var isSaturday = moment(date).isoWeekday() === 6;
                                        var isSunday = moment(date).isoWeekday() === 7;

                                        if (isSaturday || isSunday || !moment(date).isBusinessDay()) {
                                            if (operation === 'subtract') {
                                                return calculateDate(moment(date).subtract(1, 'days'), operation);
                                            } else if (operation === 'add') {
                                                return calculateDate(moment(date).add(1, 'days'), operation);
                                            }
                                        } else
                                            return date;
                                    }

                                    var current = moment(record['baslangic_tarihi']).format('YYYY-MM-DD');
                                    var st1 = calculateDate(moment(current).subtract(1, 'days'), 'subtract').format('YYYY-MM-DD');
                                    var st2 = calculateDate(moment(st1).subtract(1, 'days'), 'subtract').format('YYYY-MM-DD');

                                    var st3 = calculateDate(moment(current).add(1, 'days'), 'add').format('YYYY-MM-DD');
                                    var st4 = calculateDate(moment(st3).add(1, 'days'), 'add').format('YYYY-MM-DD');

                                    var check1 = false;
                                    var check2 = false;
                                    var check3 = false;
                                    var check4 = false;

                                    var calDateCheck1 = 0;
                                    var calDateCheck2 = 0;
                                    var calDateCheck3 = 0;
                                    var calDateCheck4 = 0;

                                    for (var i = 0; i < filteredLeaves.length; i++) {
                                        if (moment(filteredLeaves[i].baslangic_tarihi).format('YYYY-MM-DD') === st1) {
                                            calDateCheck1 = filteredLeaves[i].hesaplanan_alinacak_toplam_izin;
                                            check1 = true;
                                        } else if (moment(filteredLeaves[i].baslangic_tarihi).format('YYYY-MM-DD') === st2) {
                                            calDateCheck2 = filteredLeaves[i].hesaplanan_alinacak_toplam_izin;
                                            check2 = true;
                                        } else if (moment(filteredLeaves[i].baslangic_tarihi).format('YYYY-MM-DD') === st3) {
                                            calDateCheck3 = filteredLeaves[i].hesaplanan_alinacak_toplam_izin;
                                            check3 = true;
                                        } else if (moment(filteredLeaves[i].baslangic_tarihi).format('YYYY-MM-DD') === st4) {
                                            calDateCheck4 = filteredLeaves[i].hesaplanan_alinacak_toplam_izin;
                                            check4 = true;
                                        }

                                        /*if (moment(record['baslangic_tarihi']).isBetween(moment(mazeretIzinleri[i].baslangic_tarihi), moment(mazeretIzinleri[i].bitis_tarihi), null, '[)') ||
                                         moment(record['bitis_tarihi']).isBetween(moment(mazeretIzinleri[i].baslangic_tarihi), moment(mazeretIzinleri[i].bitis_tarihi), null, '(}') ||
                                         (moment(record['baslangic_tarihi']).isSameOrBefore(moment(mazeretIzinleri[i].baslangic_tarihi)) && moment(record['bitis_tarihi']).isSameOrAfter(moment(mazeretIzinleri[i].bitis_tarihi)))
                                         ) {
                                         return $filter('translate')('Leave.Validations.AlreadyHave');
                                         }*/


                                    }

                                    if ((check1 && check2 && calDateCheck1 + calDateCheck2 >= 16) || (check3 && check4 && calDateCheck3 + calDateCheck4 >= 16) || (check1 && check3 && calDateCheck1 + calDateCheck3 >= 16))
                                        return 'Üst üste 16 saatlik mazeret izni girişi yaptığın için, 16 saati takiben mazeret izni girişi yapamazsın.';
                                }
                            }

                            delete record["goreve_baslama_tarihi"];
                            delete record['calisan_data'];
                            delete record["dogum_tarihi"];
                            delete record["izin_turu_data"];
                            delete record['alinan_izinler'];
                            return "";
                        }
                    }
                    //}
                },

                setCustomCalculations: function (module, record, picklists, scope) {
                    //if ($rootScope.user.appId === 4) {//Ofisim IK
                    /*if (module.name === 'accounts') {
                     if (!record['id']) {
                     record['kalan_izin_hakki'] = record['toplam_izin_hakki'];
                     }
                     }*/

                    if ((module.name === 'calisanlar' || module.name === 'human_resources') && !record.id) {
                        record['sabit_devreden_izin'] = record['devreden_izin'];
                    }

                    if (module.name === 'izinler') {

                        if (record['baslangic_tarihi'] && !record['bitis_tarihi'] && record['izin_turu_data'] && record['izin_turu_data']['saatlik_kullanim_yapilir']) {
                            var defaultDate = new Date(record['baslangic_tarihi']);
                            defaultDate.setHours(8, 0, 0, 0);
                            record['bitis_tarihi'] = new Date(defaultDate).toISOString();
                            record['baslangic_tarihi'] = new Date(defaultDate).toISOString();
                        }

                        if (record['baslangic_tarihi'] && record['bitis_tarihi']) {
                            var baslangicTarihi = new Date(record['baslangic_tarihi']);
                            var bitisTarihi = new Date(record['bitis_tarihi']);

                            /*
                             * Bitiş tarihi başlama tarihinde önceyse bitiş tarihi başlangıç tarihine eşitleniyor.
                             * Saatlik kullanım yapılıyor ise saat setleniyor.
                             * */
                            if (bitisTarihi < baslangicTarihi) {
                                bitisTarihi = angular.copy(baslangicTarihi);
                                record['bitis_tarihi'] = new Date(bitisTarihi).toISOString();
                                record['baslangic_tarihi'] = new Date(baslangicTarihi).toISOString();
                            }

                            /*if (record['izin_turu_data'] && record['izin_turu_data']["saatlik_kullanim_yapilir"] &&
                             ((baslangicTarihi.getMonth() !== bitisTarihi.getMonth() ||
                             baslangicTarihi.getDate() !== bitisTarihi.getDate() ||
                             baslangicTarihi.getYear() !== bitisTarihi.getYear()) ||
                             (bitisTarihi.getHours() === baslangicTarihi.getHours() && bitisTarihi.getMinutes() === baslangicTarihi.getMinutes()))) {

                             baslangicTarihi = angular.copy(baslangicTarihi);
                             baslangicTarihi.setHours(8, 0, 0, 0);
                             record['baslangic_tarihi'] = new Date(baslangicTarihi).toISOString();

                             bitisTarihi = angular.copy(baslangicTarihi);
                             bitisTarihi.setHours(17, 0, 0, 0);
                             record['bitis_tarihi'] = new Date(bitisTarihi).toISOString();
                             }*/
                        }

                        if (record['izin_turu_data'] && record['baslangic_tarihi'] && record['bitis_tarihi']) {

                            /*
                             * Sadece tam gün olarak kullanılan izinlerde saati 00:00 setlediği için utc formata çevirirken 1 gün öncesine geçip saati 21:00 yapıyordu bu yüzden saati sabah 8 olarak setliyoruz.
                             * */
                            if (record['izin_turu_data']['sadece_tam_gun_olarak_kullanilir']) {
                                var bitisTarihi = new Date(record['bitis_tarihi']);
                                var baslangicTarihi = new Date(record['baslangic_tarihi']);

                                baslangicTarihi.setHours(8, 0, 0, 0);
                                bitisTarihi.setHours(8, 0, 0, 0);

                                record['bitis_tarihi'] = new Date(bitisTarihi).toISOString();
                                record['baslangic_tarihi'] = new Date(baslangicTarihi).toISOString();
                            }

                            if (!record['izin_turu_data']['saatlik_kullanim_yapilir'] && !record['izin_turu_data']['sadece_tam_gun_olarak_kullanilir']) {
                                var bitisTarihi = new Date(record['bitis_tarihi']);
                                var baslangicTarihi = new Date(record['baslangic_tarihi']);
                                bitisTarihi.setHours(0, 0, 0, 0);
                                baslangicTarihi.setHours(0, 0, 0, 0);

                                if (baslangicTarihi.toISOString() === bitisTarihi.toISOString()) {
                                    record['from_entry_type'] = picklists[scope.customLeaveFields['from_entry_type'].picklist_id][0];
                                    //Yıllık izin alınmak istendiğinde Bitiş Tarihi aynı tarih atılıp öğleden sonraya çekiliyordu. Bu yüzden kaldırıldı.
                                    //record['to_entry_type'] = picklists[scope.customLeaveFields['to_entry_type'].picklist_id][0];
                                }

                                if (record['to_entry_type'].system_code === 'entry_type_afternoon')
                                    bitisTarihi.setHours(12, 0, 0, 0);
                                else
                                    bitisTarihi.setHours(8, 0, 0, 0);

                                record['bitis_tarihi'] = new Date(bitisTarihi).toISOString();


                                if (record['from_entry_type'].system_code === 'entry_type_afternoon')
                                    baslangicTarihi.setHours(12, 0, 0, 0);
                                else
                                    baslangicTarihi.setHours(8, 0, 0, 0);

                                record['baslangic_tarihi'] = new Date(baslangicTarihi).toISOString();
                            }

                            //izin alanları seçilirken alınacak izin miktarı hesaplanarak set ediliyor.
                            var calculatedField = 0;

                            /*
                             * Seçilen izin tipi izin hakkından düşülecek mi diyor kontrol ediliyor.
                             * İzin hakkından düşülecek ise takvim günü olarak mı yoksa normal mi düşüleceğine göre istenen izin miktarı hesaplanıyor ve set ediliyor.
                             * */

                            var fromDate = moment(record["baslangic_tarihi"]);
                            var toDate = moment(record["bitis_tarihi"]);

                            if (record['izin_turu_data']['saatlik_kullanim_yapilir']) {
                                var calismaSaati = record['izin_turu_data']['toplam_calisma_saati'];

                                if (record['izin_turu_data']['ogle_tatilini_dikkate_al']) {
                                    var ogleTatili = parseFloat(moment.duration(moment(record['izin_turu_data']['ogle_tatili_bitis']).diff(moment(record['izin_turu_data']['ogle_tatili_baslangic']))).asHours().toFixed(2));
                                    calismaSaati = calismaSaati - ogleTatili;
                                }

                                if (!moment(fromDate.format("YYYY-MM-DD")).isSame(toDate.format("YYYY-MM-DD"))) {
                                    var bitisTarihi = new Date(record['baslangic_tarihi']);
                                    bitisTarihi = angular.copy(bitisTarihi);
                                    record['bitis_tarihi'] = new Date(bitisTarihi).toISOString();
                                    toDate = moment(record["bitis_tarihi"]);
                                }

                                var duration = moment.duration(moment(record['bitis_tarihi']).diff(moment(record["baslangic_tarihi"])))._data;
                                if (duration.hours < 0 || duration.minutes < 0) {
                                    bitisTarihi.setHours(moment(record["baslangic_tarihi"]).hour(), moment(record["baslangic_tarihi"]).minute(), 0, 0);
                                    record['bitis_tarihi'] = new Date(bitisTarihi).toISOString();
                                    toDate = moment(record["bitis_tarihi"]);
                                }

                                var differenceAsHours = moment.duration(toDate.diff(fromDate)).asHours();
                                var workHour = record['izin_turu_data']["toplam_calisma_saati"];

                                /*if(record['izin_turu_data']['ogle_tatilini_dikkate_al']){
                                 var ogleTatili = moment.duration(moment(record['izin_turu_data']['ogle_tatili_bitis']).diff(moment(record['izin_turu_data']['ogle_tatili_baslangic']))).asHours().toFixed(2);
                                 workHour = workHour - ogleTatili;
                                 }*/

                                if (differenceAsHours > workHour) {
                                    var diff = differenceAsHours - workHour;
                                    bitisTarihi = moment(bitisTarihi).add(-diff, 'hours');
                                    record['bitis_tarihi'] = new Date(bitisTarihi).toISOString();
                                    toDate = moment(record["bitis_tarihi"]);
                                }

                                var calculatedField = toDate.diff(fromDate, 'minutes') / 60;

                                if (record['izin_turu_data']["ogle_tatilini_dikkate_al"]) {
                                    var format = "HH:mm:ss";
                                    var oBaslangic = moment(record['izin_turu_data']["ogle_tatili_baslangic"]).format(format),
                                        oBitis = moment(record['izin_turu_data']["ogle_tatili_bitis"]).format(format),
                                        sBaslangic = moment(fromDate).format(format),
                                        sBitis = moment(toDate).format(format);

                                    var di = 0;
                                    if (moment(oBaslangic, format).isSameOrBefore(moment(sBaslangic, format))) {
                                        if (moment(oBitis, format).isSameOrBefore(moment(sBaslangic, format))) {
                                            //console.log("t1 t2 ba bi")
                                            di = 0;
                                        } else if (moment(oBitis, format).isSameOrBefore(moment(sBitis, format))) {
                                            di = moment(oBitis, format).diff(moment(sBaslangic, format), 'minutes') / 60;
                                            //console.log("t1 ba t2 bi");
                                        } else {
                                            di = calculatedField;
                                            //console.log("t1 ba bi t2");
                                        }
                                    } else if (moment(oBaslangic, format).isSameOrBefore(moment(sBitis, format))) {
                                        // t1 bi
                                        if (moment(oBitis, format).isSameOrBefore(moment(sBitis, format))) {
                                            //console.log("ba t1 t2 bi");
                                            di = moment(oBitis, format).diff(moment(oBaslangic, format), 'minutes') / 60;
                                        } else if (moment(oBitis, format).isSameOrAfter(moment(sBitis, format))) {
                                            //console.log("ba t1 bi t2");
                                            di = moment(sBitis, format).diff(moment(oBaslangic, format), 'minutes') / 60;
                                        }
                                    }
                                    /*else if(moment(oBaslangictime, format).isAfter(moment(sBitis, format))){
                                     console.log("ba bi t1 t2");
                                     } else {
                                     console.log("else");
                                     }*/
                                    calculatedField -= di;
                                }

                            } else {
                                if (record['izin_turu_data']["izin_hakkindan_takvim_gunu_olarak_dusulsun"]) {
                                    calculatedField = toDate.diff(fromDate, 'days');
                                } else {
                                    calculatedField = fromDate.businessDiff(toDate);
                                }

                                if (record['izin_turu_data'] && !record['izin_turu_data']["saatlik_kullanim_yapilir"] && !record['izin_turu_data']['sadece_tam_gun_olarak_kullanilir']) {
                                    var checker = calculatedField;
                                    if (fromDate.format("YYYY.MM.DD") !== toDate.format("YYYY.MM.DD")) {
                                        if (record['from_entry_type'].id !== record['to_entry_type'].id) {
                                            var fromIsBusinessDay = moment(moment(record['baslangic_tarihi']).format('YYYY-MM-DD')).isBusinessDay();
                                            var toIsBusinessDay = moment(moment(record['bitis_tarihi']).format('YYYY-MM-DD')).isBusinessDay();

                                            if (fromIsBusinessDay && record['from_entry_type'].system_code === 'entry_type_afternoon') {
                                                checker = calculatedField - 0.5;
                                            }

                                            if (toIsBusinessDay && record['to_entry_type'].system_code == 'entry_type_afternoon') {
                                                checker = calculatedField - 0.5;
                                            }
                                        }
                                    } else {
                                        checker = calculatedField - 0.5;
                                    }

                                    if (checker >= 0)
                                        calculatedField = checker;

                                } else {
                                    var bitisTarihi = new Date(record['bitis_tarihi']);
                                    var baslangicTarihi = new Date(record['baslangic_tarihi']);

                                    if (!record['izin_turu_data']['sadece_tam_gun_olarak_kullanilir']) {
                                        bitisTarihi.setHours(0, 0, 0, 0);
                                        record['bitis_tarihi'] = new Date(bitisTarihi).toISOString();

                                        baslangicTarihi.setHours(0, 0, 0, 0);
                                        record['baslangic_tarihi'] = new Date(baslangicTarihi).toISOString();
                                    }

                                    fromDate = moment(record["baslangic_tarihi"]);
                                    toDate = moment(record["bitis_tarihi"]);
                                    if (record['izin_turu_data']["izin_hakkindan_takvim_gunu_olarak_dusulsun"]) {
                                        calculatedField = toDate.diff(fromDate, 'days');
                                    } else {
                                        calculatedField = fromDate.businessDiff(toDate);
                                    }
                                }
                            }

                            var workSaturdays = $filter('filter')($rootScope.moduleSettings, { key: 'work_saturdays' }, true);
                            if (workSaturdays.length > 0)
                                workSaturdays = workSaturdays[0];
                            else
                                workSaturdays.value = 'f';

                            if (record['izin_turu_data']["cuma_gunu_alinan_izinlere_cumartesiyi_de_ekle"] &&
                                calculatedField >= 1 &&
                                workSaturdays.value !== 't' &&
                                !record['izin_turu_data']["saatlik_kullanim_yapilir"] &&
                                !record['izin_turu_data']["izin_hakkindan_takvim_gunu_olarak_dusulsun"]
                            ) {
                                var fridays = moment(fromDate).weekdaysInBetween(toDate, [5]);

                                //Kütüphane 1 gün varsa yanlış hesaplıyor. Extra hesaplama yapılıyor.

                                //var isDayFriday = moment(fromDate).format('dddd');

                                //İzin başlangıç günü cuma ise weekdaysInBetween metodu başlangıç tarihini işin içine katmadığı için bu tarihi kendimiz ekliyoruz.
                                /*
                                 * Başlangıç Cuma öğleden sonra ise Cumayı her zaman 0.5 gün sayıyoruz.
                                 * Bağlangıç Cuma Sabah ve Bitiş Pazartesi Öğleden Sonra ise ve Cuma alınan izinleri 2 gün say seçiliyse Cumayı 2 gün sayıyoruz.
                                 * Bağlangıç Cuma Sabah ve Bitiş Pazartesi Sabah ise ve Cuma alınan izinleri 2 gün say seçiliyse ama Tek Başına Cumaları 1 gün say seçili ise Cumayı 1 gün sayıyoruz diğer türlü 2 gün sayıyoruz.
                                 * */
                                var isFromDateFriday = moment(fromDate).isoWeekday() === 5;
                                if (isFromDateFriday && moment(fromDate).isBusinessDay()) {
                                    //Alınan izin sadece cuma yı mı kapsıyor diye kontrol ediliyor. Yarım gün kuralları tarih değişikliği yaratmadığı için onlarda 1 gün üzerinden değerlendiriliyor.
                                    if (toDate.diff(fromDate, 'days') <= 3) {
                                        if (record['izin_turu_data']['sadece_tam_gun_olarak_kullanilir'] ||
                                            (!record['izin_turu_data']['sadece_tam_gun_olarak_kullanilir'] && record['from_entry_type'].system_code !== 'entry_type_afternoon' && record['to_entry_type'].system_code === 'entry_type_afternoon' && moment(toDate).isBusinessDay()) ||
                                            (!record['izin_turu_data']['sadece_tam_gun_olarak_kullanilir'] && record['from_entry_type'].system_code !== 'entry_type_afternoon' && !record['izin_turu_data']['sadece_cuma_ise_tek_gun_say'])
                                        ) {
                                            calculatedField += 1;
                                        }
                                    } else if (calculatedField >= 2 && record['from_entry_type'].system_code !== 'entry_type_afternoon') {
                                        //Ağustos 10 Öğleden Sonra - 14 sabah record['from_entry_type'].system_code != 'entry_type_afternoon' kontrolü burda gerekli oluyor.
                                        calculatedField += 1;
                                    } else if (record['izin_turu_data']['sadece_tam_gun_olarak_kullanilir'] ||
                                        (!record['izin_turu_data']['sadece_tam_gun_olarak_kullanilir'] && record['from_entry_type'].system_code !== 'entry_type_afternoon')
                                    ) {
                                        //Cuma gününün tatillerle birleştirilmiş halinde tarihler arasında ki diff 3 den fazla olduğu için extra kontrol ediliyor.
                                        if (calculatedField === 1 && !record['izin_turu_data']['sadece_cuma_ise_tek_gun_say'])
                                            calculatedField += 1;
                                    }
                                }

                                //Sadece cuma günü alınan izinler de 2 gün sayma
                                if (moment(toDate).isoWeekday() === 5 && moment(toDate).isBusinessDay() && toDate.diff(fromDate, 'days') < 2)
                                    calculatedField -= 1;

                                //Bitiş tarihini işe tekrar başlama olarak gördüğümüz için ve aşşağıdaki kontrol bunu bize sagladığı için bu tarihi manuel çıkarıyoruz.
                                /*var isToDateFriday = moment(toDate).isoWeekday() === 5;
                                 if(isToDateFriday && moment(toDate).isBusinessDay())
                                 calculatedField -= 1;*/


                                if (fridays) {
                                    if (fridays.length) {
                                        for (var x = 0; x < fridays.length; x++) {
                                            if (moment(fridays[x]).isBusinessDay()) {
                                                calculatedField += 1;
                                            }
                                        }
                                    } else {
                                        if (fridays.isBusinessDay())
                                            calculatedField += 1;
                                    }
                                }
                            }

                            if (!record['izin_turu_data']['izin_hakkindan_takvim_gunu_olarak_dusulsun']) {
                                //Yarım gün(hafta içine denk gelen) tatiller alınan izinden çıkarılıyor
                                for (var i = 0; i < $rootScope.holidaysData.length; i++) {
                                    var holiday = $rootScope.holidaysData[i];
                                    if (holiday.half_day && moment(holiday.date).isBusinessDay() && moment(moment(holiday.date).format('YYYY-MM-DD')).isBetween(moment(record['baslangic_tarihi']).format('YYYY-MM-DD'), moment(record['bitis_tarihi']).format('YYYY-MM-DD'), null, '[)')) {
                                        calculatedField -= 0.5;
                                    }
                                    //Tam gün(Cumartesi gününe denk gelen) tatiller alınan izinden çıkarılıyor
                                    else if (!holiday.half_day && moment(holiday.date).isBusinessDay() && moment(moment(holiday.date).format('YYYY-MM-DD')).isBetween(moment(record['baslangic_tarihi']).format('YYYY-MM-DD'), moment(record['bitis_tarihi']).format('YYYY-MM-DD'), null, '[)')) {
                                        calculatedField -= 1.0;
                                    }
                                    /*Yarım gün(Cumartesi gününe denk gelen) tatiller alınan izinden çıkarılıyor Hafta sonu olduğu için her türlü toplamda alınan izinden 1 gün çıkarılıyor
									 calculatedField kontrolünü yapma sebebimiz, alınan izin sadece Cuma gününü mü kapsıyor ? 
									 */
                                    else if (calculatedField > 1 && holiday.half_day && moment(moment(holiday.date).format('YYYY-MM-DD')).day() === 6 && moment(moment(holiday.date).format('YYYY-MM-DD')).isBetween(moment(record['baslangic_tarihi']).format('YYYY-MM-DD'), moment(record['bitis_tarihi']).format('YYYY-MM-DD'), null, '[)')) {
                                        calculatedField -= 1.0;
                                    }
                                    //Tam gün(Cumartesi gününe denk gelen) tatiller alınan izinden çıkarılıyor Hafta sonu olduğu için izinden 1 gün çıkarılıyor
                                    else if (calculatedField > 1 && !holiday.half_day && moment(moment(holiday.date).format('YYYY-MM-DD')).day() === 6 && moment(moment(holiday.date).format('YYYY-MM-DD')).isBetween(moment(record['baslangic_tarihi']).format('YYYY-MM-DD'), moment(record['bitis_tarihi']).format('YYYY-MM-DD'), null, '[)')) {
                                        calculatedField -= 1.0;
                                    }
                                }
                            }

                            if (record['izin_turu_data']["saatlik_kullanim_yapilir"] && record['izin_turu_data']['saatlik_kullanimi_yukari_yuvarla']) {
                                calculatedField = Math.ceil(calculatedField);
                            }

                            var requestLeave = calculatedField;
                            if (!record['izin_turu_data']["yillik_izin"] && !record['izin_turu_data']["izin_hakkindan_dusulsun"]) {
                                calculatedField = 0;
                            }

                            record['hesaplanan_alinacak_toplam_izin'] = calculatedField;
                            record['talep_edilen_izin'] = requestLeave;
                        }
                    }
                    //}
                },

                updateView: function (view, id) {
                    return $http.put(config.apiUrl + 'view/update/' + id, view);
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

                            views = $filter('orderBy')(views, 'label_' + $rootScope.language);
                            deferred.resolve(views);
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

                getViewFields: function (module, fields) {
                    var viewFields = [];
                    var moduleFields = angular.copy(module.fields);

                    for (var i = 0; i < fields.length; i++) {
                        var field = fields[i];
                        var currentModuleFields = moduleFields;
                        var currentFieldName = field.field;
                        var parentFieldName = null;

                        if (field.field.indexOf('.') > -1) {
                            var fieldParts = field.field.split('.');

                            if (fieldParts[3] != null && fieldParts[3] === 'primary')
                                continue;

                            currentModuleFields = angular.copy($filter('filter')($rootScope.modules, { name: fieldParts[1] }, true)[0].fields);
                            currentFieldName = fieldParts[2];
                            parentFieldName = fieldParts[0];
                        }

                        var moduleField = $filter('filter')(currentModuleFields, { name: currentFieldName }, true)[0];

                        if (!moduleField || !this.hasFieldDisplayPermission(moduleField))
                            continue;

                        if (parentFieldName != null)
                            moduleField.parentField = $filter('filter')(moduleFields, { name: parentFieldName }, true)[0];

                        var viewField = angular.copy(moduleField);
                        viewField.fieldName = field.field;
                        viewField.order = field.order;

                        viewFields.push(viewField);
                    }

                    return viewFields;
                },

                setFilters: function (filters, field, fieldName, value, operator, no, isView) {
                    if (field.data_type === 'lookup' && field.lookup_type === 'users' && value === '[me]')
                        value = $rootScope.user.id;

                    if (field.data_type === 'email' && value === '[me.email]')
                        value = $rootScope.user.email;

                    if (field.data_type === 'date' && value) {
                        var valueFormatted = moment(value).format('YYYY-MM-DD');

                        if (moment(value).isValid())
                            value = moment(value).format('YYYY-MM-DD');
                    }

                    if (!filters)
                        filters = [];

                    if (operator === 'empty' || operator === 'not_empty')
                        value = '-';

                    var currentFilter = $filter('filter')(filters, {
                        field: fieldName,
                        operator: operator,
                        no: parseInt(no)
                    }, true)[0];

                    if (!currentFilter) {
                        no = !no ? filters.length + 1 : parseInt(no);

                        var filter = {};
                        filter.field = fieldName;
                        filter.operator = operator;
                        filter.value = value;
                        filter.no = no;
                        filter.isView = isView;

                        if (field.data_type === 'document') {
                            filter.document_search = field.document_search;
                        }

                        filters.push(filter);
                    } else {
                        currentFilter.operator = operator;
                        currentFilter.value = value;

                        if (field.data_type === 'document') {
                            currentFilter.document_search = field.document_search;
                        }
                    }

                    return filters;
                },

                setTable: function (scope, tableBlockUI, counts, defaultCount, filters, parent, type, isSelectable, parentId, parentType, displayFields, relatedModule, parentModule, returnTab, previousParentType, previousParentId, previousReturnTab, parentScope) {
                    var that = this;
                    var key = parent + '_' + type;
                    scope.isManyToMany = relatedModule && relatedModule.relation_type === 'many_to_many';

                    if (scope.isManyToMany)
                        key = parent + '_' + relatedModule.relation_field + '_' + scope.module.name;

                    if (!$rootScope.activePages)
                        $rootScope.activePages = {};

                    if (!$rootScope.activeFilters)
                        $rootScope.activeFilters = {};

                    scope.tableParams = new ngTableParams(
                        {
                            page: 1,
                            count: defaultCount,
                            load: false
                        },
                        {
                            counts: counts,
                            getData: function ($defer, params) {
                                if (params.pagination)
                                    $window.scrollTo(0, 0);
                                $rootScope.activePages[key] = params.page();
                                var cache = $cache.get(key);

                                //clears other pages checkboxes and selected items array when page changed
                                //scope.selectedRows = [];
                                scope.isAllSelected = false;

                                if (!params.reloading && cache && !scope.parentModule) {
                                    that.getPicklists(scope.module)
                                        .then(function (picklists) {
                                            scope.tableParams.picklists = picklists;

                                            if (cache['views'] && cache['views'].length && !scope.viewid) {
                                                setViewsFields(cache['views'], cache['viewState'], parent);
                                                scope.fields = that.getViewFields(scope.module, scope.view.fields);

                                                params.sorting(cache['sorting']);
                                                params.$params.count = cache['count'];
                                                params.total(cache['total']);

                                                var rowsCache = cache['rows-' + key + params.page()];

                                                if (rowsCache && rowsCache.length) {
                                                    params.total(cache['total'] || relatedModule.total);
                                                    scope.findRequest = cache['findRequest'];

                                                    if (relatedModule) {
                                                        relatedModule.total = relatedModule.total || cache['total'];
                                                        relatedModule.loading = false;
                                                    }

                                                    $defer.resolve(rowsCache);
                                                    scope.loading = false;
                                                    tableBlockUI.stop();
                                                } else {
                                                    getData();
                                                }
                                            } else {
                                                getData();
                                            }

                                            for (var key in params.filters) {
                                                if (params.filters.hasOwnProperty(key)) {
                                                    var value = params.filters[key];
                                                    that.setDependencyFilter(key, value.value, scope.module, params.picklists);
                                                }
                                            }
                                        });
                                } else {
                                    if (params.reloading) {
                                        params.page(1);
                                        $rootScope.activePages[key] = 1;
                                        $rootScope.activeFilters[key] = null;

                                        if (params.refreshing) {
                                            cache = null;
                                            filters = angular.copy(params.filterList);
                                            params.filters = null;
                                            params.refreshing = false;
                                        }

                                        if (params.renew)
                                            filters = angular.copy(params.filterList);

                                        if (cache) {
                                            var newCache = {};
                                            newCache.views = angular.copy(cache.views);
                                            newCache.viewState = angular.copy(cache.viewState);
                                            cache = newCache;
                                        }

                                        getData();
                                    } else {
                                        getData();
                                    }
                                }


                                function setViewFilters() {
                                    var viewFilters = angular.copy(scope.view.filters);
                                    viewFilters = $filter('orderBy')(viewFilters, 'no');

                                    if (viewFilters && viewFilters.length > 0) {
                                        for (var i = 0; i < viewFilters.length; i++) {
                                            var viewFilter = viewFilters[i];
                                            var moduleFields = [];
                                            var fieldName = viewFilter.field;
                                            var fieldNameOrginal = viewFilter.field;
                                            var field = null;

                                            if (fieldName.indexOf('.') < 0) {
                                                moduleFields = scope.module.fields;
                                            } else {
                                                var fieldParts = fieldName.split('.');

                                                if (fieldParts[1] !== 'process_requests' && fieldParts[1] !== 'process_approvers')
                                                    moduleFields = $filter('filter')($rootScope.modules, { name: fieldParts[1] }, true)[0].fields;

                                                if (fieldParts[1] === 'process_requests' || fieldParts[1] === 'process_approvers') {
                                                    field = {};
                                                    field.data_type = 'number';
                                                    field.name = 'process_status'
                                                }

                                                if (fieldParts[1] === 'process_approvers') {
                                                    field = {};
                                                    field.data_type = 'lookup';
                                                    field.lookup_type = 'users';
                                                    field.name = 'user_id'
                                                }

                                                fieldNameOrginal = fieldParts[2];
                                            }

                                            if (!field)
                                                field = $filter('filter')(moduleFields, { name: fieldNameOrginal }, true)[0];

                                            if (!field)
                                                continue;

                                            if (field.data_type === 'lookup' && field.lookup_type === 'users' && viewFilter.value === '0')
                                                viewFilter.value = $rootScope.user.id;

                                            if (field.data_type === 'multiselect')
                                                viewFilter.value = viewFilter.value.split('|');
                                            if (field.data_type === 'tag')
                                                viewFilter.value = viewFilter.value.split('|');

                                            filters = that.setFilters(filters, field, fieldName, viewFilter.value, viewFilter.operator, viewFilter.no, true)
                                        }
                                    }
                                }

                                function getData() {
                                    if (!scope.loading)
                                        tableBlockUI.start();

                                    that.getPicklists(scope.module, true)
                                        .then(function (picklists) {
                                            scope.tableParams.picklists = picklists;

                                            for (var filterKey in params.filters) {
                                                if (params.filters.hasOwnProperty(filterKey)) {
                                                    var value = params.filters[filterKey];
                                                    that.setDependencyFilter(filterKey, value.value, scope.module, params.picklists);
                                                }
                                            }

                                            that.getViews(scope.module.id, displayFields, cache)
                                                .then(function (views) {
                                                    that.getViewState(scope.module.id, cache)
                                                        .then(function (viewState) {
                                                            var cacheItem = cache || {};
                                                            var limit = params.count();
                                                            var sorting = params.sorting();
                                                            var sortClicked = params.sortClicked;
                                                            var sortField = null;
                                                            var sortDirection = null;
                                                            var viewId = scope.viewid;

                                                            if (sorting && Object.keys(sorting).length > 0) {
                                                                sortField = Object.keys(sorting)[0];
                                                                sortDirection = sorting[sortField];
                                                            }

                                                            var newViewState = function (sortField, sortDirection, viewId) {
                                                                var activeView;
                                                                if (viewId) {
                                                                    activeView = $filter('filter')(views, { id: parseInt(viewId) }, true)[0];
                                                                } else {
                                                                    activeView = $filter('filter')(views, { active: true })[0];
                                                                }

                                                                viewState = {};
                                                                viewState.active_view = activeView.id;
                                                                viewState.sort_field = sortField;
                                                                viewState.sort_direction = sortDirection;
                                                                viewState.row_per_page = 10;
                                                                if (!scope.viewid || scope.viewid === null) {
                                                                    that.setViewState(viewState, scope.module.id)
                                                                        .then(function (response) {
                                                                            viewState.id = response.data.id;
                                                                        });
                                                                }
                                                            };

                                                            if (!sortClicked) {
                                                                if (viewState && viewState.sort_field && !viewId) {
                                                                    sortField = viewState.sort_field;
                                                                    sortDirection = viewState.sort_direction;
                                                                } else {
                                                                    sortField = 'created_at';
                                                                    sortDirection = 'desc';
                                                                    newViewState(sortField, sortDirection, viewId);
                                                                }
                                                            } else {
                                                                if (viewState) {
                                                                    viewState.sort_field = sortField;
                                                                    viewState.sort_direction = sortDirection;
                                                                    that.setViewState(viewState, scope.module.id, viewState.id);
                                                                } else {
                                                                    newViewState(sortField, sortDirection, viewId);
                                                                }
                                                            }

                                                            var sortingView = {};
                                                            sortingView[sortField] = sortDirection;
                                                            params.sorting(sortingView);
                                                            cacheItem['sorting'] = sortingView;

                                                            setViewsFields(views, viewState, parent);

                                                            var filterLogic = scope.view && scope.view.filter_logic;

                                                            if (params.filters && Object.keys(params.filters).length > 0) {
                                                                setViewFilters();
                                                                var listFilters = angular.copy(params.filters);

                                                                if (listFilters) {
                                                                    for (var fieldName in listFilters) {
                                                                        if (listFilters.hasOwnProperty(fieldName)) {
                                                                            var search = listFilters[fieldName];
                                                                            var moduleFields = [];
                                                                            var fieldNameOrginal = fieldName;
                                                                            var hasSameFilter = false;

                                                                            if (fieldName.indexOf('.') < 0) {
                                                                                moduleFields = scope.module.fields;
                                                                            } else {
                                                                                var fieldParts = fieldName.split('.');
                                                                                moduleFields = $filter('filter')($rootScope.modules, { name: fieldParts[1] }, true)[0].fields;
                                                                                fieldNameOrginal = fieldParts[2];
                                                                            }

                                                                            var field = $filter('filter')(moduleFields, { name: fieldNameOrginal }, true)[0];

                                                                            if (!field)
                                                                                continue;

                                                                            if (!search.operator)
                                                                                search.operator = $filter('orderBy')(field.operators, 'order')[0];

                                                                            if ((search.operator.name !== 'empty' && search.operator.name !== 'not_empty') &&
                                                                                (search.value === undefined || search.value === null || search.value === '' || (angular.isArray(search.value) && !search.value.length) || ((field.data_type === 'number' || field.data_type === 'number_decimal' || field.data_type === 'number_auto' || field.data_type === 'currency') && isNaN(search.value)))) {
                                                                                delete listFilters[fieldName];
                                                                                delete params.filters[fieldName];

                                                                                var currentFilter = filters && $filter('filter')(filters, { field: fieldName }, true)[0];

                                                                                /*
                                                                                 * Tek başına !angular.isUndefined(currentFilter.isView) olarak kontrol etmek.
                                                                                 * View larda oluşturulan filtrelerin ezilmesine sebep oluyordu.
                                                                                 * Extra !currentFilter.isView kontrolü eklenerek view filtrelerinin korunması sağlandı.
                                                                                 * */
                                                                                if (currentFilter && !angular.isUndefined(currentFilter.isView) && !currentFilter.isView)
                                                                                    filters.splice(filters.indexOf(currentFilter), 1);

                                                                                continue;
                                                                            }

                                                                            /*
                                                                             * View da filtre eklenen bir alan eğer listeye de kolon olarak eklenmiş ise ve
                                                                             * Bu alana liste de ayrı bir filtre uygulanırsa view de oluşturulan filtre ve liste de oluşturulan filtre birlikte gidiyordu
                                                                             * Liste de eklenen filtre'nin view de kini ezmesi sağlandı.
                                                                             * */
                                                                            var currentFilter = filters && $filter('filter')(filters, { field: fieldName }, true)[0];
                                                                            if (currentFilter && !angular.isUndefined(currentFilter.isView) && currentFilter.isView &&
                                                                                (
                                                                                    (angular.isArray(search.value) && search.value.length) ||
                                                                                    ((field.data_type === 'number' || field.data_type === 'number_decimal' || field.data_type === 'number_auto' || field.data_type === 'currency') && !isNaN(search.value)) ||
                                                                                    search.value !== undefined ||
                                                                                    search.value !== null ||
                                                                                    search.value !== ''
                                                                                )
                                                                            ) {
                                                                                filters.splice(filters.indexOf(currentFilter), 1);
                                                                            }

                                                                            var lookupModule;
                                                                            var lookupModulePrimaryField;

                                                                            if (field.data_type === 'lookup' && field.lookup_type !== 'users' && field.lookup_type !== 'profiles' && field.lookup_type !== 'roles') {
                                                                                lookupModule = $filter('filter')($rootScope.modules, { name: field.lookup_type }, true)[0];
                                                                                lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];
                                                                            }

                                                                            if (search.operator.name !== 'empty' && search.operator.name !== 'not_empty') {
                                                                                if (field.data_type === 'lookup' && (field.lookup_type === 'users' || field.lookup_type === 'profiles' || field.lookup_type === 'roles'))
                                                                                    search.value = search.value[0].id;

                                                                                if (field.data_type === 'lookup' && field.lookup_type !== 'users' && field.lookup_type !== 'profiles' && field.lookup_type !== 'roles') {
                                                                                    fieldName = field.name + '.' + field.lookup_type + '.' + lookupModulePrimaryField.name;
                                                                                    field = lookupModulePrimaryField;
                                                                                }

                                                                                if (field.data_type === 'picklist')
                                                                                    search.value = search.value.label[$rootScope.user.tenant_language];

                                                                                if (field.data_type === 'multiselect') {
                                                                                    var valueArray = [];

                                                                                    for (var m = 0; m < search.value.length; m++) {
                                                                                        var val = search.value[m];
                                                                                        valueArray.push(val.label[$rootScope.user.tenant_language]);
                                                                                    }

                                                                                    search.value = valueArray;
                                                                                }
                                                                                if (field.data_type === 'tag') {
                                                                                    var valueArray = [];

                                                                                    for (var m = 0; m < search.value.length; m++) {
                                                                                        var val = search.value[m];
                                                                                        valueArray.push(val.text);
                                                                                    }
                                                                                    search.value = valueArray;
                                                                                }

                                                                                if (field.data_type === 'checkbox')
                                                                                    search.value = search.value.system_code;
                                                                            } else {
                                                                                if (field.data_type === 'lookup' && field.lookup_type !== 'users' && field.lookup_type !== 'profiles' && field.lookup_type !== 'roles') {
                                                                                    fieldName = field.name + '.' + field.lookup_type + '.' + lookupModulePrimaryField.name;
                                                                                    var currentFilterLookup = filters && $filter('filter')(filters, { field: fieldName }, true)[0];

                                                                                    if (currentFilterLookup)
                                                                                        filters.splice(filters.indexOf(currentFilterLookup), 1);
                                                                                }
                                                                            }

                                                                            if (filters) {
                                                                                for (var j = 0; j < filters.length; j++) {
                                                                                    var filter = filters[j];

                                                                                    if (filter.field === fieldName && filter.operator === search.operator.name && filter.value === search.value)
                                                                                        hasSameFilter = true;
                                                                                }
                                                                            }

                                                                            if (hasSameFilter)
                                                                                continue;

                                                                            filters = that.setFilters(filters, field, fieldName, search.value, search.operator.name);

                                                                            if (filterLogic)
                                                                                filterLogic = '(' + filterLogic + ' and ' + filters.length + ')';
                                                                        }
                                                                    }
                                                                }

                                                                $rootScope.activeFilters[key] = params.filters;
                                                            }

                                                            var selectedFields = [];
                                                            var viewFields = scope.view.fields;
                                                            var viewFieldsManyToMany = [];
                                                            var fieldNamePrimaryManyToMany = '';

                                                            if (scope.isManyToMany) {
                                                                var relationFieldName = relatedModule.related_module === parentModule ? relatedModule.related_module + '2' : relatedModule.related_module;
                                                                var fieldPrimaryManyToMany = $filter('filter')(scope.module.fields, { primary: true }, true)[0];
                                                                fieldNamePrimaryManyToMany = relationFieldName + '_id.' + relatedModule.related_module + '.' + fieldPrimaryManyToMany.name;
                                                            }

                                                            for (var i = 0; i < viewFields.length; i++) {
                                                                var viewField = viewFields[i];

                                                                if (!scope.isManyToMany) {
                                                                    selectedFields.push(viewField.field);
                                                                } else {
                                                                    var fieldView = $filter('filter')(scope.module.fields, { name: viewField.field }, true)[0];

                                                                    if (viewField.field.indexOf('.') < 0 && fieldView.data_type !== 'lookup') {
                                                                        var fieldNameView = relatedModule.related_module === parentModule ? relatedModule.related_module + '2' : relatedModule.related_module;
                                                                        var fieldManyToMany = fieldNameView + '_id.' + relatedModule.related_module + '.' + viewField.field;
                                                                        selectedFields.push(fieldManyToMany);

                                                                        var viewFieldManyToMany = angular.copy(viewField);
                                                                        viewFieldManyToMany.field = fieldManyToMany;
                                                                        viewFieldsManyToMany.push(viewFieldManyToMany);
                                                                    }
                                                                }
                                                            }

                                                            if (scope.isManyToMany) {
                                                                var sortFieldName = relatedModule.related_module === parentModule ? relatedModule.related_module + '2' : relatedModule.related_module;
                                                                sortField = sortFieldName + '_id.' + relatedModule.related_module + '.' + sortField;
                                                                viewFields = viewFieldsManyToMany;
                                                                scope.view.fields = viewFieldsManyToMany;
                                                            }

                                                            if ($filter('filter')(scope.module.fields, {
                                                                name: 'currency',
                                                                deleted: '!true'
                                                            }, true)[0] && selectedFields.indexOf('currency') < 0 && !scope.isManyToMany)
                                                                selectedFields.push('currency');

                                                            if (!params.filters || !Object.keys(params.filters).length) {
                                                                setViewFilters();

                                                                if (scope.view.filter_logic)
                                                                    filterLogic = scope.view.filter_logic;
                                                            }


                                                            //Get records with NOT IN filter
                                                            if (relatedModule) {
                                                                var recordKey = 'ids-' + scope.parentModule + params.page() + '_' + scope.parentModule + '_' + scope.module.name;

                                                                if (scope.relatedModuleInModal && $cache.get(recordKey) && $cache.get(recordKey).length > 0) {
                                                                    var currentNotInFilter = $filter('filter')(filters, { operator: '!not_in' }, true)[0];

                                                                    if (filters.length > 0 && !currentNotInFilter) {
                                                                        var customFilter = {
                                                                            field: 'id',
                                                                            no: filters.length + 1,
                                                                            operator: 'not_in',
                                                                            value: $cache.get(recordKey)
                                                                        };

                                                                        if (filters.length > 0)
                                                                            for (var k = 0; k < filters.length; k++) {
                                                                                var fltr = filters[k];

                                                                                if (fltr.operator !== customFilter.operator && fltr.value !== customFilter.value)
                                                                                    filters.push(customFilter);
                                                                            }
                                                                        else
                                                                            filters.push(customFilter);
                                                                    }
                                                                }
                                                            }


                                                            var findRequest = {
                                                                fields: selectedFields,
                                                                filters: filters,
                                                                sort_field: sortField,
                                                                sort_direction: sortDirection,
                                                                limit: limit,
                                                                offset: (params.page() - 1) * limit
                                                            };

                                                            if (filterLogic)
                                                                findRequest.filter_logic = filterLogic;

                                                            if (scope.isManyToMany)
                                                                findRequest.many_to_many = parentModule;

                                                            cacheItem['views'] = views;
                                                            cacheItem['viewState'] = viewState;
                                                            cacheItem['count'] = limit;

                                                            if (!params.refreshing)
                                                                scope.fields = that.getViewFields(scope.module, viewFields);

                                                            /// save request query to use it in excel export later.
                                                            scope.findRequest = findRequest;
                                                            scope.viewFields = viewFields;
                                                            cacheItem['findRequest'] = findRequest;

                                                            //Not use cache for related data because it is also filtering main module records.
                                                            if (!scope.parentModule && !scope.viewid)
                                                                $cache.put(key, cacheItem);

                                                            components.run('EmptyList', 'Script', scope);

                                                            var findRecords = function (findRequest, cacheItem) {
                                                                scope.listFindRequest = findRequest;

                                                                scope.executeCode = false;
                                                                components.run('BeforeListRequest', 'Script', scope);

                                                                if (scope.executeCode)
                                                                    return;

                                                                that.findRecords(scope.module.name, scope.listFindRequest)
                                                                    .then(function (response) {
                                                                        var hasFilter = false;

                                                                        if (params.filters) {
                                                                            for (var key in params.filters) {
                                                                                if (params.filters.hasOwnProperty(key)) {
                                                                                    var value = params.filters[key];

                                                                                    if (value.hasOwnProperty('value')) {
                                                                                        hasFilter = true;
                                                                                        break;
                                                                                    }
                                                                                }
                                                                            }
                                                                        }

                                                                        scope.isEmpty = response.data.length < 1 && !hasFilter;

                                                                        var rows = that.processRecordMulti(response.data, scope.module, scope.tableParams.picklists, viewFields, type, parentId, parentType, returnTab, previousParentType, previousParentId, previousReturnTab, parentScope);

                                                                        if (relatedModule)
                                                                            relatedModule.loading = false;

                                                                        //Not use cache for related data because it is also filtering main module records.
                                                                        if (!scope.parentModule && !scope.viewid) {
                                                                            cacheItem['rows-' + key + params.page()] = rows;
                                                                            $cache.put(key, cacheItem);
                                                                        }

                                                                        //Get number of record which is displaying in table.
                                                                        if (relatedModule && !scope.relatedModuleInModal) {
                                                                            var recordIds = [];
                                                                            scope.listFindRequest = {
                                                                                fields: relatedModule.relation_type === 'one_to_many' ? ['id'] : [scope.module.name + '_id'],
                                                                                filters: [{
                                                                                    field: relatedModule.relation_type === 'one_to_many' ? relatedModule.relation_field : scope.parentModule + "_id",
                                                                                    no: 1,
                                                                                    operator: "equals",
                                                                                    value: scope.parentId
                                                                                }],
                                                                                limit: 9999999,
                                                                                offset: 0
                                                                            };
                                                                            var relationType = relatedModule.relation_type ? relatedModule.relation_type : 'many_to_many';
                                                                            scope.listFindRequest[relationType] = scope.parentModule;

                                                                            that.findRecords(scope.module.name, scope.listFindRequest)
                                                                                .then(function (response) {
                                                                                    if (response.data.length > 0) {
                                                                                        for (var i = 0; i < response.data.length; i++) {
                                                                                            var id = response.data[i];
                                                                                            recordIds.push(Object.values(id)[0]);
                                                                                        }

                                                                                        $cache.put(recordKey, recordIds);
                                                                                    }
                                                                                });
                                                                        }
                                                                        params.reloading = false;
                                                                        params.sortClicked = false;
                                                                        $defer.resolve(rows);
                                                                    })
                                                                    .finally(function () {
                                                                        scope.loading = false;
                                                                        if (scope.parentId) {
                                                                            components.run('SubListLoaded', 'Script', scope);
                                                                        }
                                                                        tableBlockUI.stop();

                                                                        if (params.pagination) {
                                                                            $window.scrollTo(0, 0);
                                                                            params.pagination = false;
                                                                        }
                                                                    });
                                                            };

                                                            if (params.page() === 1) {
                                                                var findRequestTotalCount = angular.copy(findRequest);
                                                                findRequestTotalCount.fields = ['total_count()'];
                                                                findRequestTotalCount.limit = 1;
                                                                findRequestTotalCount.offset = 0;
                                                                delete findRequestTotalCount.sort_field;
                                                                delete findRequestTotalCount.sort_direction;

                                                                if (findRequest.filters) {
                                                                    for (var l = 0; l < findRequest.filters.length; l++) {
                                                                        var filterItem = findRequest.filters[l];

                                                                        if (filterItem.field.indexOf('.') > -1 && filterItem.field.indexOf('process_approvers') < 0)
                                                                            findRequestTotalCount.fields.push(filterItem.field);
                                                                    }
                                                                }

                                                                if (scope.isManyToMany && findRequestTotalCount.fields.indexOf(fieldNamePrimaryManyToMany) < 0)
                                                                    findRequestTotalCount.fields.push(fieldNamePrimaryManyToMany);

                                                                that.findRecords(scope.module.name, findRequestTotalCount)
                                                                    .then(function (response) {
                                                                        response = response.data;
                                                                        var total = 0;
                                                                        var hasFilter = false;

                                                                        if (params.filters) {
                                                                            for (var key in params.filters) {
                                                                                if (params.filters.hasOwnProperty(key)) {
                                                                                    var value = params.filters[key];

                                                                                    if (value.hasOwnProperty('value')) {
                                                                                        hasFilter = true;
                                                                                        break;
                                                                                    }
                                                                                }
                                                                            }
                                                                        }

                                                                        if (response === null || !response.length || response[0] === null || response[0].total_count === null) {
                                                                            $defer.resolve([]);
                                                                            scope.isEmpty = response.length < 1 && !hasFilter;
                                                                            if (scope.parentId) {
                                                                                components.run('SubListLoaded', 'Script', scope);
                                                                            }
                                                                            scope.loading = false;
                                                                            tableBlockUI.stop();
                                                                        } else {
                                                                            findRecords(findRequest, cacheItem);
                                                                            total = response[0].total_count;
                                                                        }

                                                                        params.total(total);
                                                                        cacheItem['total'] = total;

                                                                        if (parentId)
                                                                            scope.$parent.$parent['currentTotalCount' + type] = total;

                                                                        if (relatedModule)
                                                                            relatedModule.total = total;

                                                                        if (params.pagination) {
                                                                            $window.scrollTo(0, 0);
                                                                            params.pagination = false;
                                                                        }
                                                                    })
                                                                    .catch(function () {
                                                                        scope.loading = false;
                                                                        tableBlockUI.stop();
                                                                    });
                                                            } else {
                                                                if (relatedModule)
                                                                    params.total(relatedModule.total);

                                                                findRecords(findRequest, cacheItem);
                                                            }
                                                        });
                                                });
                                        });
                                }
                            }
                        });

                    scope.tableParams.filterList = filters && filters.length ? filters : undefined;
                    scope.tableParams.isSelectable = isSelectable;

                    //to show filter for manytomany modal
                    if (scope.$parent.loadingModal) {
                        scope.tableParams.showFilter = true;
                    }

                    if ($rootScope.activePages[key]) {
                        scope.tableParams.page($rootScope.activePages[key]);
                    }

                    if ($rootScope.activeFilters[key]) {
                        scope.tableParams.filters = $rootScope.activeFilters[key];
                        scope.tableParams.showFilter = true;
                    }

                    scope.tableParams.load = true;

                    var setViewsFields = function (views, viewState, parent) {
                        scope.views = views;

                        if (!scope.selectedView) {
                            if (!viewState || parent !== scope.module.name)
                                scope.view = $filter('filter')(views, { active: true, default: true })[0];
                            else if (viewState)
                                scope.view = $filter('filter')(views, { id: viewState.active_view }, true)[0];
                        } else {
                            scope.view = scope.selectedView;

                            for (var i = 0; i < views.length; i++) {
                                var view = views[i];
                                view.active = view.id === scope.selectedView.id;
                            }
                        }

                        if (viewState && !scope.view) {
                            scope.view = $filter('filter')(views, { active: true })[0];
                            viewState.active_view = scope.view.id;
                            viewState.sort_field = 'created_at';
                            viewState.sort_direction = 'desc';
                            viewState.row_per_page = 10;

                            that.setViewState(viewState, scope.module.id, viewState.id);
                        }
                    };

                    scope.setDependency = function (field) {
                        that.setDependencyFilter(field.fieldName, scope.tableParams.filters[field.fieldName].value, scope.module, scope.tableParams.picklists);
                    };

                    var dateTimeChanged = function (field) {
                        if (scope.tableParams.filters[field.fieldName].operator) {
                            var newValue = new Date(scope.tableParams.filters[field.fieldName].value);

                            switch (scope.tableParams.filters[field.fieldName].operator.name) {
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

                            scope.tableParams.filters[field.fieldName].value = newValue;
                        }
                    };

                    scope.dateTimeChanged = function (field) {
                        dateTimeChanged(field);
                    };

                    scope.operatorChanged = function (field) {
                        if (scope.tableParams.filters[field.fieldName].operator.name === 'empty' || scope.tableParams.filters[field.fieldName].operator.name === 'not_empty') {
                            scope.tableParams.filters[field.fieldName].value = null;
                            scope.tableParams.filters[field.fieldName].disabled = true;
                        } else {
                            scope.tableParams.filters[field.fieldName].disabled = false;
                        }
                    };
                },

                getCSVData: function (scope, type) {
                    var that = this;
                    var deferred = $q.defer();
                    var findRequest = angular.copy(scope.findRequest);
                    findRequest.limit = 3000;
                    findRequest.offset = 0;

                    that.findRecords(scope.module.name, findRequest)
                        .then(function (response) {
                            var records = that.processRecordMulti(response.data, scope.module, scope.tableParams.picklists, scope.viewFields, type);
                            var csv = [];
                            var header = [];
                            var fields = $filter('orderBy')(scope.fields, 'order');

                            for (var i = 0; i < fields.length; i++) {
                                var field = fields[i];
                                header.push(field['label_' + $rootScope.language] + (field.parentField ? ' (' + field.parentField['label_' + $rootScope.language] + ')' : ''));
                            }

                            csv.push(header);

                            for (var j = 0; j < records.length; j++) {
                                var record = records[j];

                                var row = [];
                                var recordFields = $filter('orderBy')(record.fields, 'order');

                                for (var k = 0; k < recordFields.length; k++) {
                                    var fieldItem = recordFields[k];

                                    if (fieldItem.data_type === 'currency' || fieldItem.data_type === 'number_decimal')
                                        row.push(fieldItem.value || '');
                                    else
                                        row.push(fieldItem.valueFormatted || '');
                                }

                                csv.push(row);
                            }

                            deferred.resolve(csv);
                        });

                    return deferred.promise;
                },

                getYesNo: function (yesNoPickist) {
                    var yesNo = {};
                    yesNo.yes = $filter('filter')(yesNoPickist, { system_code: 'true' })[0].label[$rootScope.language];
                    yesNo.no = $filter('filter')(yesNoPickist, { system_code: 'false' })[0].label[$rootScope.language];

                    return yesNo;
                },

                getIcons: function () {
                    return icons.icons;
                },

                getActionButtons: function (moduleId, refresh) {
                    var deferred = $q.defer();
                    var cacheKey = 'action_button_' + moduleId;
                    var cache = $cache.get(cacheKey);

                    if (cache && !refresh) {
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

                hasSectionFullPermission: function (section) {
                    if (!section.permissions)
                        return true;

                    var permission = $filter('filter')(section.permissions, { profile_id: $rootScope.user.profile.id }, true)[0];

                    if (!permission)
                        return true;

                    return permission.type === 'full';
                },

                hasActionButtonDisplayPermission: function (actionButton) {
                    if (!actionButton.permissions)
                        return true;

                    var permission = $filter('filter')(actionButton.permissions, { profile_id: $rootScope.user.profile.id }, true)[0];

                    if (!permission)
                        return true;

                    return permission.type !== 'none';
                },

                getDailyRates: function () {
                    return $http.get(config.apiUrl + 'exchange_rates/get_daily_rates');
                },

                getAllTenantSettingsByType: function (settingType, userId) {
                    return $http.get(config.apiUrl + 'settings/get_all/' + settingType + (userId ? '?user_id=' + userId : ''));
                },

                tenantSettingUpdate: function (setting) {
                    return $http.put(config.apiUrl + 'settings/update/' + setting.id, setting);
                }
            };
        }]);

angular.module('primeapps')

    .constant('icons', {
        icons: [
            {
                "value": "fa fa-adjust",
                "label": "<i class=\"fa fa-adjust\"> Adjust"
            },
            {
                "value": "fa fa-anchor",
                "label": "<i class=\"fa fa-anchor\"> Anchor"
            },
            {
                "value": "fa fa-archive",
                "label": "<i class=\"fa fa-archive\"> Archive"
            },
            {
                "value": "fa fa-area-chart",
                "label": "<i class=\"fa fa-area-chart\"> Area-chart",
                "chart": true
            },
            {
                "value": "fa fa-arrows",
                "label": "<i class=\"fa fa-arrows\"> Arrows"
            },
            {
                "value": "fa fa-arrows-h",
                "label": "<i class=\"fa fa-arrows-h\"> Arrows-h"
            },
            {
                "value": "fa fa-arrows-v",
                "label": "<i class=\"fa fa-arrows-v\"> Arrows-v"
            },
            {
                "value": "fa fa-asterisk",
                "label": "<i class=\"fa fa-asterisk\"> Asterisk"
            },
            {
                "value": "fa fa-at",
                "label": "<i class=\"fa fa-at\"> At"
            },
            {
                "value": "fa fa-automobile",
                "label": "<i class=\"fa fa-automobile\"> Automobile"
            },
            {
                "value": "fa fa-ban",
                "label": "<i class=\"fa fa-ban\"> Ban"
            },
            {
                "value": "fa fa-bank",
                "label": "<i class=\"fa fa-bank\"> Bank"
            },
            {
                "value": "fa fa-bar-chart",
                "label": "<i class=\"fa fa-bar-chart\"> Bar-chart",
                "chart": true
            },
            {
                "value": "fa fa-bar-chart-o",
                "label": "<i class=\"fa fa-bar-chart-o\"> Bar-chart-o"
            },
            {
                "value": "fa fa-barcode",
                "label": "<i class=\"fa fa-barcode\"> Barcode"
            },
            {
                "value": "fa fa-bars",
                "label": "<i class=\"fa fa-bars\"> Bars"
            },
            {
                "value": "fa fa-bed",
                "label": "<i class=\"fa fa-bed\"> Bed"
            },
            {
                "value": "fa fa-beer",
                "label": "<i class=\"fa fa-beer\"> Beer"
            },
            {
                "value": "fa fa-bell",
                "label": "<i class=\"fa fa-bell\"> Bell"
            },
            {
                "value": "fa fa-bell-o",
                "label": "<i class=\"fa fa-bell-o\"> Bell-o"
            },
            {
                "value": "fa fa-bell-slash",
                "label": "<i class=\"fa fa-bell-slash\"> Bell-slash"
            },
            {
                "value": "fa fa-bell-slash-o",
                "label": "<i class=\"fa fa-bell-slash-o\"> Bell-slash-o"
            },
            {
                "value": "fa fa-bicycle",
                "label": "<i class=\"fa fa-bicycle\"> Bicycle"
            },
            {
                "value": "fa fa-binoculars",
                "label": "<i class=\"fa fa-binoculars\"> Binoculars"
            },
            {
                "value": "fa fa-birthday-cake",
                "label": "<i class=\"fa fa-birthday-cake\"> Birthday-cake"
            },
            {
                "value": "fa fa-bolt",
                "label": "<i class=\"fa fa-bolt\"> Bolt"
            },
            {
                "value": "fa fa-bomb",
                "label": "<i class=\"fa fa-bomb\"> Bomb"
            },
            {
                "value": "fa fa-book",
                "label": "<i class=\"fa fa-book\"> Book"
            },
            {
                "value": "fa fa-bookmark",
                "label": "<i class=\"fa fa-bookmark\"> Bookmark"
            },
            {
                "value": "fa fa-bookmark-o",
                "label": "<i class=\"fa fa-bookmark-o\"> Bookmark-o"
            },
            {
                "value": "fa fa-briefcase",
                "label": "<i class=\"fa fa-briefcase\"> Briefcase"
            },
            {
                "value": "fa fa-bug",
                "label": "<i class=\"fa fa-bug\"> Bug"
            },
            {
                "value": "fa fa-building",
                "label": "<i class=\"fa fa-building\"> Building"
            },
            {
                "value": "fa fa-building-o",
                "label": "<i class=\"fa fa-building-o\"> Building-o"
            },
            {
                "value": "fa fa-bullhorn",
                "label": "<i class=\"fa fa-bullhorn\"> Bullhorn"
            },
            {
                "value": "fa fa-bullseye",
                "label": "<i class=\"fa fa-bullseye\"> Bullseye"
            },
            {
                "value": "fa fa-bus",
                "label": "<i class=\"fa fa-bus\"> Bus"
            },
            {
                "value": "fa fa-cab",
                "label": "<i class=\"fa fa-cab\"> Cab"
            },
            {
                "value": "fa fa-calculator",
                "label": "<i class=\"fa fa-calculator\"> Calculator"
            },
            {
                "value": "fa fa-calendar",
                "label": "<i class=\"fa fa-calendar\"> Calendar"
            },
            {
                "value": "fa fa-calendar-o",
                "label": "<i class=\"fa fa-calendar-o\"> Calendar-o"
            },
            {
                "value": "fa fa-camera",
                "label": "<i class=\"fa fa-camera\"> Camera"
            },
            {
                "value": "fa fa-camera-retro",
                "label": "<i class=\"fa fa-camera-retro\"> Camera-retro"
            },
            {
                "value": "fa fa-car",
                "label": "<i class=\"fa fa-car\"> Car"
            },
            {
                "value": "fa fa-caret-square-o-down",
                "label": "<i class=\"fa fa-caret-square-o-down\"> Caret-square-o-down"
            },
            {
                "value": "fa fa-caret-square-o-left",
                "label": "<i class=\"fa fa-caret-square-o-left\"> Caret-square-o-left"
            },
            {
                "value": "fa fa-caret-square-o-right",
                "label": "<i class=\"fa fa-caret-square-o-right\"> Caret-square-o-right"
            },
            {
                "value": "fa fa-caret-square-o-up",
                "label": "<i class=\"fa fa-caret-square-o-up\"> Caret-square-o-up"
            },
            {
                "value": "fa fa-cart-arrow-down",
                "label": "<i class=\"fa fa-cart-arrow-down\"> Cart-arrow-down"
            },
            {
                "value": "fa fa-cart-plus",
                "label": "<i class=\"fa fa-cart-plus\"> Cart-plus"
            },
            {
                "value": "fa fa-cc",
                "label": "<i class=\"fa fa-cc\"> Cc"
            },
            {
                "value": "fa fa-certificate",
                "label": "<i class=\"fa fa-certificate\"> Certificate"
            },
            {
                "value": "fa fa-check",
                "label": "<i class=\"fa fa-check\"> Check"
            },
            {
                "value": "fa fa-check-circle",
                "label": "<i class=\"fa fa-check-circle\"> Check-circle"
            },
            {
                "value": "fa fa-check-circle-o",
                "label": "<i class=\"fa fa-check-circle-o\"> Check-circle-o"
            },
            {
                "value": "fa fa-check-square",
                "label": "<i class=\"fa fa-check-square\"> Check-square"
            },
            {
                "value": "fa fa-check-square-o",
                "label": "<i class=\"fa fa-check-square-o\"> Check-square-o"
            },
            {
                "value": "fa fa-child",
                "label": "<i class=\"fa fa-child\"> Child"
            },
            {
                "value": "fa fa-circle",
                "label": "<i class=\"fa fa-circle\"> Circle"
            },
            {
                "value": "fa fa-circle-o",
                "label": "<i class=\"fa fa-circle-o\"> Circle-o"
            },
            {
                "value": "fa fa-circle-o-notch",
                "label": "<i class=\"fa fa-circle-o-notch\"> Circle-o-notch"
            },
            {
                "value": "fa fa-circle-thin",
                "label": "<i class=\"fa fa-circle-thin\"> Circle-thin"
            },
            {
                "value": "fa fa-clock-o",
                "label": "<i class=\"fa fa-clock-o\"> Clock-o"
            },
            {
                "value": "fa fa-close",
                "label": "<i class=\"fa fa-close\"> Close"
            },
            {
                "value": "fa fa-cloud",
                "label": "<i class=\"fa fa-cloud\"> Cloud"
            },
            {
                "value": "fa fa-cloud-download",
                "label": "<i class=\"fa fa-cloud-download\"> Cloud-download"
            },
            {
                "value": "fa fa-cloud-upload",
                "label": "<i class=\"fa fa-cloud-upload\"> Cloud-upload"
            },
            {
                "value": "fa fa-code",
                "label": "<i class=\"fa fa-code\"> Code"
            },
            {
                "value": "fa fa-code-fork",
                "label": "<i class=\"fa fa-code-fork\"> Code-fork"
            },
            {
                "value": "fa fa-coffee",
                "label": "<i class=\"fa fa-coffee\"> Coffee"
            },
            {
                "value": "fa fa-cog",
                "label": "<i class=\"fa fa-cog\"> Cog"
            },
            {
                "value": "fa fa-cogs",
                "label": "<i class=\"fa fa-cogs\"> Cogs"
            },
            {
                "value": "fa fa-comment",
                "label": "<i class=\"fa fa-comment\"> Comment"
            },
            {
                "value": "fa fa-comment-o",
                "label": "<i class=\"fa fa-comment-o\"> Comment-o"
            },
            {
                "value": "fa fa-comments",
                "label": "<i class=\"fa fa-comments\"> Comments"
            },
            {
                "value": "fa fa-comments-o",
                "label": "<i class=\"fa fa-comments-o\"> Comments-o"
            },
            {
                "value": "fa fa-compass",
                "label": "<i class=\"fa fa-compass\"> Compass"
            },
            {
                "value": "fa fa-copyright",
                "label": "<i class=\"fa fa-copyright\"> Copyright"
            },
            {
                "value": "fa fa-credit-card",
                "label": "<i class=\"fa fa-credit-card\"> Credit-card"
            },
            {
                "value": "fa fa-crop",
                "label": "<i class=\"fa fa-crop\"> Crop"
            },
            {
                "value": "fa fa-crosshairs",
                "label": "<i class=\"fa fa-crosshairs\"> Crosshairs"
            },
            {
                "value": "fa fa-cube",
                "label": "<i class=\"fa fa-cube\"> Cube"
            },
            {
                "value": "fa fa-cubes",
                "label": "<i class=\"fa fa-cubes\"> Cubes"
            },
            {
                "value": "fa fa-cutlery",
                "label": "<i class=\"fa fa-cutlery\"> Cutlery"
            },
            {
                "value": "fa fa-dashboard",
                "label": "<i class=\"fa fa-dashboard\"> Dashboard"
            },
            {
                "value": "fa fa-database",
                "label": "<i class=\"fa fa-database\"> Database"
            },
            {
                "value": "fa fa-desktop",
                "label": "<i class=\"fa fa-desktop\"> Desktop"
            },
            {
                "value": "fa fa-diamond",
                "label": "<i class=\"fa fa-diamond\"> Diamond"
            },
            {
                "value": "fa fa-dot-circle-o",
                "label": "<i class=\"fa fa-dot-circle-o\"> Dot-circle-o"
            },
            {
                "value": "fa fa-download",
                "label": "<i class=\"fa fa-download\"> Download"
            },
            {
                "value": "fa fa-edit",
                "label": "<i class=\"fa fa-edit\"> Edit"
            },
            {
                "value": "fa fa-ellipsis-h",
                "label": "<i class=\"fa fa-ellipsis-h\"> Ellipsis-h"
            },
            {
                "value": "fa fa-ellipsis-v",
                "label": "<i class=\"fa fa-ellipsis-v\"> Ellipsis-v"
            },
            {
                "value": "fa fa-envelope",
                "label": "<i class=\"fa fa-envelope\"> Envelope"
            },
            {
                "value": "fa fa-envelope-o",
                "label": "<i class=\"fa fa-envelope-o\"> Envelope-o"
            },
            {
                "value": "fa fa-envelope-square",
                "label": "<i class=\"fa fa-envelope-square\"> Envelope-square"
            },
            {
                "value": "fa fa-eraser",
                "label": "<i class=\"fa fa-eraser\"> Eraser"
            },
            {
                "value": "fa fa-exchange",
                "label": "<i class=\"fa fa-exchange\"> Exchange"
            },
            {
                "value": "fa fa-exclamation",
                "label": "<i class=\"fa fa-exclamation\"> Exclamation"
            },
            {
                "value": "fa fa-exclamation-circle",
                "label": "<i class=\"fa fa-exclamation-circle\"> Exclamation-circle"
            },
            {
                "value": "fa fa-exclamation-triangle",
                "label": "<i class=\"fa fa-exclamation-triangle\"> Exclamation-triangle"
            },
            {
                "value": "fa fa-external-link",
                "label": "<i class=\"fa fa-external-link\"> External-link"
            },
            {
                "value": "fa fa-external-link-square",
                "label": "<i class=\"fa fa-external-link-square\"> External-link-square"
            },
            {
                "value": "fa fa-eye",
                "label": "<i class=\"fa fa-eye\"> Eye"
            },
            {
                "value": "fa fa-eye-slash",
                "label": "<i class=\"fa fa-eye-slash\"> Eye-slash"
            },
            {
                "value": "fa fa-eyedropper",
                "label": "<i class=\"fa fa-eyedropper\"> Eyedropper"
            },
            {
                "value": "fa fa-fax",
                "label": "<i class=\"fa fa-fax\"> Fax"
            },
            {
                "value": "fa fa-female",
                "label": "<i class=\"fa fa-female\"> Female"
            },
            {
                "value": "fa fa-fighter-jet",
                "label": "<i class=\"fa fa-fighter-jet\"> Fighter-jet"
            },
            {
                "value": "fa fa-file-archive-o",
                "label": "<i class=\"fa fa-file-archive-o\"> File-archive-o"
            },
            {
                "value": "fa fa-file-audio-o",
                "label": "<i class=\"fa fa-file-audio-o\"> File-audio-o"
            },
            {
                "value": "fa fa-file-code-o",
                "label": "<i class=\"fa fa-file-code-o\"> File-code-o"
            },
            {
                "value": "fa fa-file-excel-o",
                "label": "<i class=\"fa fa-file-excel-o\"> File-excel-o"
            },
            {
                "value": "fa fa-file-image-o",
                "label": "<i class=\"fa fa-file-image-o\"> File-image-o"
            },
            {
                "value": "fa fa-file-movie-o",
                "label": "<i class=\"fa fa-file-movie-o\"> File-movie-o"
            },
            {
                "value": "fa fa-file-pdf-o",
                "label": "<i class=\"fa fa-file-pdf-o\"> File-pdf-o"
            },
            {
                "value": "fa fa-file-photo-o",
                "label": "<i class=\"fa fa-file-photo-o\"> File-photo-o"
            },
            {
                "value": "fa fa-file-picture-o",
                "label": "<i class=\"fa fa-file-picture-o\"> File-picture-o"
            },
            {
                "value": "fa fa-file-powerpoint-o",
                "label": "<i class=\"fa fa-file-powerpoint-o\"> File-powerpoint-o"
            },
            {
                "value": "fa fa-file-sound-o",
                "label": "<i class=\"fa fa-file-sound-o\"> File-sound-o"
            },
            {
                "value": "fa fa-file-video-o",
                "label": "<i class=\"fa fa-file-video-o\"> File-video-o"
            },
            {
                "value": "fa fa-file-word-o",
                "label": "<i class=\"fa fa-file-word-o\"> File-word-o"
            },
            {
                "value": "fa fa-file-zip-o",
                "label": "<i class=\"fa fa-file-zip-o\"> File-zip-o"
            },
            {
                "value": "fa fa-film",
                "label": "<i class=\"fa fa-film\"> Film"
            },
            {
                "value": "fa fa-filter",
                "label": "<i class=\"fa fa-filter\"> Filter"
            },
            {
                "value": "fa fa-fire",
                "label": "<i class=\"fa fa-fire\"> Fire"
            },
            {
                "value": "fa fa-fire-extinguisher",
                "label": "<i class=\"fa fa-fire-extinguisher\"> Fire-extinguisher"
            },
            {
                "value": "fa fa-flag",
                "label": "<i class=\"fa fa-flag\"> Flag"
            },
            {
                "value": "fa fa-flag-checkered",
                "label": "<i class=\"fa fa-flag-checkered\"> Flag-checkered"
            },
            {
                "value": "fa fa-flag-o",
                "label": "<i class=\"fa fa-flag-o\"> Flag-o"
            },
            {
                "value": "fa fa-flash",
                "label": "<i class=\"fa fa-flash\"> Flash"
            },
            {
                "value": "fa fa-flask",
                "label": "<i class=\"fa fa-flask\"> Flask"
            },
            {
                "value": "fa fa-folder",
                "label": "<i class=\"fa fa-folder\"> Folder"
            },
            {
                "value": "fa fa-folder-o",
                "label": "<i class=\"fa fa-folder-o\"> Folder-o"
            },
            {
                "value": "fa fa-folder-open",
                "label": "<i class=\"fa fa-folder-open\"> Folder-open"
            },
            {
                "value": "fa fa-folder-open-o",
                "label": "<i class=\"fa fa-folder-open-o\"> Folder-open-o"
            },
            {
                "value": "fa fa-frown-o",
                "label": "<i class=\"fa fa-frown-o\"> Frown-o"
            },
            {
                "value": "fa fa-futbol-o",
                "label": "<i class=\"fa fa-futbol-o\"> Futbol-o"
            },
            {
                "value": "fa fa-gamepad",
                "label": "<i class=\"fa fa-gamepad\"> Gamepad"
            },
            {
                "value": "fa fa-gavel",
                "label": "<i class=\"fa fa-gavel\"> Gavel"
            },
            {
                "value": "fa fa-gear",
                "label": "<i class=\"fa fa-gear\"> Gear"
            },
            {
                "value": "fa fa-gears",
                "label": "<i class=\"fa fa-gears\"> Gears"
            },
            {
                "value": "fa fa-gift",
                "label": "<i class=\"fa fa-gift\"> Gift"
            },
            {
                "value": "fa fa-glass",
                "label": "<i class=\"fa fa-glass\"> Glass"
            },
            {
                "value": "fa fa-globe",
                "label": "<i class=\"fa fa-globe\"> Globe"
            },
            {
                "value": "fa fa-graduation-cap",
                "label": "<i class=\"fa fa-graduation-cap\"> Graduation-cap"
            },
            {
                "value": "fa fa-group",
                "label": "<i class=\"fa fa-group\"> Group"
            },
            {
                "value": "fa fa-hdd-o",
                "label": "<i class=\"fa fa-hdd-o\"> Hdd-o"
            },
            {
                "value": "fa fa-headphones",
                "label": "<i class=\"fa fa-headphones\"> Headphones"
            },
            {
                "value": "fa fa-heart",
                "label": "<i class=\"fa fa-heart\"> Heart"
            },
            {
                "value": "fa fa-heart-o",
                "label": "<i class=\"fa fa-heart-o\"> Heart-o"
            },
            {
                "value": "fa fa-heartbeat",
                "label": "<i class=\"fa fa-heartbeat\"> Heartbeat"
            },
            {
                "value": "fa fa-history",
                "label": "<i class=\"fa fa-history\"> History"
            },
            {
                "value": "fa fa-home",
                "label": "<i class=\"fa fa-home\"> Home"
            },
            {
                "value": "fa fa-hotel",
                "label": "<i class=\"fa fa-hotel\"> Hotel"
            },
            {
                "value": "fa fa-image",
                "label": "<i class=\"fa fa-image\"> Image"
            },
            {
                "value": "fa fa-inbox",
                "label": "<i class=\"fa fa-inbox\"> Inbox"
            },
            {
                "value": "fa fa-info",
                "label": "<i class=\"fa fa-info\"> Info"
            },
            {
                "value": "fa fa-info-circle",
                "label": "<i class=\"fa fa-info-circle\"> Info-circle"
            },
            {
                "value": "fa fa-institution",
                "label": "<i class=\"fa fa-institution\"> Institution"
            },
            {
                "value": "fa fa-key",
                "label": "<i class=\"fa fa-key\"> Key"
            },
            {
                "value": "fa fa-keyboard-o",
                "label": "<i class=\"fa fa-keyboard-o\"> Keyboard-o"
            },
            {
                "value": "fa fa-language",
                "label": "<i class=\"fa fa-language\"> Language"
            },
            {
                "value": "fa fa-laptop",
                "label": "<i class=\"fa fa-laptop\"> Laptop"
            },
            {
                "value": "fa fa-leaf",
                "label": "<i class=\"fa fa-leaf\"> Leaf"
            },
            {
                "value": "fa fa-legal",
                "label": "<i class=\"fa fa-legal\"> Legal"
            },
            {
                "value": "fa fa-lemon-o",
                "label": "<i class=\"fa fa-lemon-o\"> Lemon-o"
            },
            {
                "value": "fa fa-level-down",
                "label": "<i class=\"fa fa-level-down\"> Level-down"
            },
            {
                "value": "fa fa-level-up",
                "label": "<i class=\"fa fa-level-up\"> Level-up"
            },
            {
                "value": "fa fa-life-bouy",
                "label": "<i class=\"fa fa-life-bouy\"> Life-bouy"
            },
            {
                "value": "fa fa-life-buoy",
                "label": "<i class=\"fa fa-life-buoy\"> Life-buoy"
            },
            {
                "value": "fa fa-life-ring",
                "label": "<i class=\"fa fa-life-ring\"> Life-ring"
            },
            {
                "value": "fa fa-life-saver",
                "label": "<i class=\"fa fa-life-saver\"> Life-saver"
            },
            {
                "value": "fa fa-lightbulb-o",
                "label": "<i class=\"fa fa-lightbulb-o\"> Lightbulb-o"
            },
            {
                "value": "fa fa-line-chart",
                "label": "<i class=\"fa fa-line-chart\"> Line-chart",
                "chart": true
            },
            {
                "value": "fa fa-location-arrow",
                "label": "<i class=\"fa fa-location-arrow\"> Location-arrow"
            },
            {
                "value": "fa fa-lock",
                "label": "<i class=\"fa fa-lock\"> Lock"
            },
            {
                "value": "fa fa-magic",
                "label": "<i class=\"fa fa-magic\"> Magic"
            },
            {
                "value": "fa fa-magnet",
                "label": "<i class=\"fa fa-magnet\"> Magnet"
            },
            {
                "value": "fa fa-mail-forward",
                "label": "<i class=\"fa fa-mail-forward\"> Mail-forward"
            },
            {
                "value": "fa fa-mail-reply",
                "label": "<i class=\"fa fa-mail-reply\"> Mail-reply"
            },
            {
                "value": "fa fa-mail-reply-all",
                "label": "<i class=\"fa fa-mail-reply-all\"> Mail-reply-all"
            },
            {
                "value": "fa fa-male",
                "label": "<i class=\"fa fa-male\"> Male"
            },
            {
                "value": "fa fa-meh-o",
                "label": "<i class=\"fa fa-meh-o\"> Meh-o"
            },
            {
                "value": "fa fa-microphone",
                "label": "<i class=\"fa fa-microphone\"> Microphone"
            },
            {
                "value": "fa fa-microphone-slash",
                "label": "<i class=\"fa fa-microphone-slash\"> Microphone-slash"
            },
            {
                "value": "fa fa-minus",
                "label": "<i class=\"fa fa-minus\"> Minus"
            },
            {
                "value": "fa fa-minus-circle",
                "label": "<i class=\"fa fa-minus-circle\"> Minus-circle"
            },
            {
                "value": "fa fa-minus-square",
                "label": "<i class=\"fa fa-minus-square\"> Minus-square"
            },
            {
                "value": "fa fa-minus-square-o",
                "label": "<i class=\"fa fa-minus-square-o\"> Minus-square-o"
            },
            {
                "value": "fa fa-mobile",
                "label": "<i class=\"fa fa-mobile\"> Mobile"
            },
            {
                "value": "fa fa-mobile-phone",
                "label": "<i class=\"fa fa-mobile-phone\"> Mobile-phone"
            },
            {
                "value": "fa fa-money",
                "label": "<i class=\"fa fa-money\"> Money"
            },
            {
                "value": "fa fa-moon-o",
                "label": "<i class=\"fa fa-moon-o\"> Moon-o"
            },
            {
                "value": "fa fa-mortar-board",
                "label": "<i class=\"fa fa-mortar-board\"> Mortar-board"
            },
            {
                "value": "fa fa-motorcycle",
                "label": "<i class=\"fa fa-motorcycle\"> Motorcycle"
            },
            {
                "value": "fa fa-music",
                "label": "<i class=\"fa fa-music\"> Music"
            },
            {
                "value": "fa fa-navicon",
                "label": "<i class=\"fa fa-navicon\"> Navicon"
            },
            {
                "value": "fa fa-newspaper-o",
                "label": "<i class=\"fa fa-newspaper-o\"> Newspaper-o"
            },
            {
                "value": "fa fa-paint-brush",
                "label": "<i class=\"fa fa-paint-brush\"> Paint-brush"
            },
            {
                "value": "fa fa-paper-plane",
                "label": "<i class=\"fa fa-paper-plane\"> Paper-plane"
            },
            {
                "value": "fa fa-paper-plane-o",
                "label": "<i class=\"fa fa-paper-plane-o\"> Paper-plane-o"
            },
            {
                "value": "fa fa-paw",
                "label": "<i class=\"fa fa-paw\"> Paw"
            },
            {
                "value": "fa fa-pencil",
                "label": "<i class=\"fa fa-pencil\"> Pencil"
            },
            {
                "value": "fa fa-pencil-square",
                "label": "<i class=\"fa fa-pencil-square\"> Pencil-square"
            },
            {
                "value": "fa fa-pencil-square-o",
                "label": "<i class=\"fa fa-pencil-square-o\"> Pencil-square-o"
            },
            {
                "value": "fa fa-phone",
                "label": "<i class=\"fa fa-phone\"> Phone"
            },
            {
                "value": "fa fa-phone-square",
                "label": "<i class=\"fa fa-phone-square\"> Phone-square"
            },
            {
                "value": "fa fa-photo",
                "label": "<i class=\"fa fa-photo\"> Photo"
            },
            {
                "value": "fa fa-picture-o",
                "label": "<i class=\"fa fa-picture-o\"> Picture-o"
            },
            {
                "value": "fa fa-pie-chart",
                "label": "<i class=\"fa fa-pie-chart\"> Pie-chart",
                "chart": true
            },
            {
                "value": "fa fa-plane",
                "label": "<i class=\"fa fa-plane\"> Plane"
            },
            {
                "value": "fa fa-plug",
                "label": "<i class=\"fa fa-plug\"> Plug"
            },
            {
                "value": "fa fa-plus",
                "label": "<i class=\"fa fa-plus\"> Plus"
            },
            {
                "value": "fa fa-plus-circle",
                "label": "<i class=\"fa fa-plus-circle\"> Plus-circle"
            },
            {
                "value": "fa fa-plus-square",
                "label": "<i class=\"fa fa-plus-square\"> Plus-square"
            },
            {
                "value": "fa fa-plus-square-o",
                "label": "<i class=\"fa fa-plus-square-o\"> Plus-square-o"
            },
            {
                "value": "fa fa-power-off",
                "label": "<i class=\"fa fa-power-off\"> Power-off"
            },
            {
                "value": "fa fa-print",
                "label": "<i class=\"fa fa-print\"> Print"
            },
            {
                "value": "fa fa-puzzle-piece",
                "label": "<i class=\"fa fa-puzzle-piece\"> Puzzle-piece"
            },
            {
                "value": "fa fa-qrcode",
                "label": "<i class=\"fa fa-qrcode\"> Qrcode"
            },
            {
                "value": "fa fa-question",
                "label": "<i class=\"fa fa-question\"> Question"
            },
            {
                "value": "fa fa-question-circle",
                "label": "<i class=\"fa fa-question-circle\"> Question-circle"
            },
            {
                "value": "fa fa-quote-left",
                "label": "<i class=\"fa fa-quote-left\"> Quote-left"
            },
            {
                "value": "fa fa-quote-right",
                "label": "<i class=\"fa fa-quote-right\"> Quote-right"
            },
            {
                "value": "fa fa-random",
                "label": "<i class=\"fa fa-random\"> Random"
            },
            {
                "value": "fa fa-recycle",
                "label": "<i class=\"fa fa-recycle\"> Recycle"
            },
            {
                "value": "fa fa-refresh",
                "label": "<i class=\"fa fa-refresh\"> Refresh"
            },
            {
                "value": "fa fa-remove",
                "label": "<i class=\"fa fa-remove\"> Remove"
            },
            {
                "value": "fa fa-reorder",
                "label": "<i class=\"fa fa-reorder\"> Reorder"
            },
            {
                "value": "fa fa-reply",
                "label": "<i class=\"fa fa-reply\"> Reply"
            },
            {
                "value": "fa fa-reply-all",
                "label": "<i class=\"fa fa-reply-all\"> Reply-all"
            },
            {
                "value": "fa fa-retweet",
                "label": "<i class=\"fa fa-retweet\"> Retweet"
            },
            {
                "value": "fa fa-road",
                "label": "<i class=\"fa fa-road\"> Road"
            },
            {
                "value": "fa fa-rocket",
                "label": "<i class=\"fa fa-rocket\"> Rocket"
            },
            {
                "value": "fa fa-rss",
                "label": "<i class=\"fa fa-rss\"> Rss"
            },
            {
                "value": "fa fa-rss-square",
                "label": "<i class=\"fa fa-rss-square\"> Rss-square"
            },
            {
                "value": "fa fa-search",
                "label": "<i class=\"fa fa-search\"> Search"
            },
            {
                "value": "fa fa-search-minus",
                "label": "<i class=\"fa fa-search-minus\"> Search-minus"
            },
            {
                "value": "fa fa-search-plus",
                "label": "<i class=\"fa fa-search-plus\"> Search-plus"
            },
            {
                "value": "fa fa-send",
                "label": "<i class=\"fa fa-send\"> Send"
            },
            {
                "value": "fa fa-send-o",
                "label": "<i class=\"fa fa-send-o\"> Send-o"
            },
            {
                "value": "fa fa-server",
                "label": "<i class=\"fa fa-server\"> Server"
            },
            {
                "value": "fa fa-share",
                "label": "<i class=\"fa fa-share\"> Share"
            },
            {
                "value": "fa fa-share-alt",
                "label": "<i class=\"fa fa-share-alt\"> Share-alt"
            },
            {
                "value": "fa fa-share-alt-square",
                "label": "<i class=\"fa fa-share-alt-square\"> Share-alt-square"
            },
            {
                "value": "fa fa-share-square",
                "label": "<i class=\"fa fa-share-square\"> Share-square"
            },
            {
                "value": "fa fa-share-square-o",
                "label": "<i class=\"fa fa-share-square-o\"> Share-square-o"
            },
            {
                "value": "fa fa-shield",
                "label": "<i class=\"fa fa-shield\"> Shield"
            },
            {
                "value": "fa fa-ship",
                "label": "<i class=\"fa fa-ship\"> Ship"
            },
            {
                "value": "fa fa-shopping-cart",
                "label": "<i class=\"fa fa-shopping-cart\"> Shopping-cart"
            },
            {
                "value": "fa fa-sign-in",
                "label": "<i class=\"fa fa-sign-in\"> Sign-in"
            },
            {
                "value": "fa fa-sign-out",
                "label": "<i class=\"fa fa-sign-out\"> Sign-out"
            },
            {
                "value": "fa fa-signal",
                "label": "<i class=\"fa fa-signal\"> Signal"
            },
            {
                "value": "fa fa-sitemap",
                "label": "<i class=\"fa fa-sitemap\"> Sitemap"
            },
            {
                "value": "fa fa-sliders",
                "label": "<i class=\"fa fa-sliders\"> Sliders"
            },
            {
                "value": "fa fa-smile-o",
                "label": "<i class=\"fa fa-smile-o\"> Smile-o"
            },
            {
                "value": "fa fa-soccer-ball-o",
                "label": "<i class=\"fa fa-soccer-ball-o\"> Soccer-ball-o"
            },
            {
                "value": "fa fa-sort",
                "label": "<i class=\"fa fa-sort\"> Sort"
            },
            {
                "value": "fa fa-sort-alpha-asc",
                "label": "<i class=\"fa fa-sort-alpha-asc\"> Sort-alpha-asc"
            },
            {
                "value": "fa fa-sort-alpha-desc",
                "label": "<i class=\"fa fa-sort-alpha-desc\"> Sort-alpha-desc"
            },
            {
                "value": "fa fa-sort-amount-asc",
                "label": "<i class=\"fa fa-sort-amount-asc\"> Sort-amount-asc"
            },
            {
                "value": "fa fa-sort-amount-desc",
                "label": "<i class=\"fa fa-sort-amount-desc\"> Sort-amount-desc"
            },
            {
                "value": "fa fa-sort-asc",
                "label": "<i class=\"fa fa-sort-asc\"> Sort-asc"
            },
            {
                "value": "fa fa-sort-desc",
                "label": "<i class=\"fa fa-sort-desc\"> Sort-desc"
            },
            {
                "value": "fa fa-sort-down",
                "label": "<i class=\"fa fa-sort-down\"> Sort-down"
            },
            {
                "value": "fa fa-sort-numeric-asc",
                "label": "<i class=\"fa fa-sort-numeric-asc\"> Sort-numeric-asc"
            },
            {
                "value": "fa fa-sort-numeric-desc",
                "label": "<i class=\"fa fa-sort-numeric-desc\"> Sort-numeric-desc"
            },
            {
                "value": "fa fa-sort-up",
                "label": "<i class=\"fa fa-sort-up\"> Sort-up"
            },
            {
                "value": "fa fa-space-shuttle",
                "label": "<i class=\"fa fa-space-shuttle\"> Space-shuttle"
            },
            {
                "value": "fa fa-spinner",
                "label": "<i class=\"fa fa-spinner\"> Spinner"
            },
            {
                "value": "fa fa-spoon",
                "label": "<i class=\"fa fa-spoon\"> Spoon"
            },
            {
                "value": "fa fa-square",
                "label": "<i class=\"fa fa-square\"> Square"
            },
            {
                "value": "fa fa-square-o",
                "label": "<i class=\"fa fa-square-o\"> Square-o"
            },
            {
                "value": "fa fa-star",
                "label": "<i class=\"fa fa-star\"> Star"
            },
            {
                "value": "fa fa-star-half",
                "label": "<i class=\"fa fa-star-half\"> Star-half"
            },
            {
                "value": "fa fa-star-half-empty",
                "label": "<i class=\"fa fa-star-half-empty\"> Star-half-empty"
            },
            {
                "value": "fa fa-star-half-full",
                "label": "<i class=\"fa fa-star-half-full\"> Star-half-full"
            },
            {
                "value": "fa fa-star-half-o",
                "label": "<i class=\"fa fa-star-half-o\"> Star-half-o"
            },
            {
                "value": "fa fa-star-o",
                "label": "<i class=\"fa fa-star-o\"> Star-o"
            },
            {
                "value": "fa fa-street-view",
                "label": "<i class=\"fa fa-street-view\"> Street-view"
            },
            {
                "value": "fa fa-suitcase",
                "label": "<i class=\"fa fa-suitcase\"> Suitcase"
            },
            {
                "value": "fa fa-sun-o",
                "label": "<i class=\"fa fa-sun-o\"> Sun-o"
            },
            {
                "value": "fa fa-support",
                "label": "<i class=\"fa fa-support\"> Support"
            },
            {
                "value": "fa fa-tablet",
                "label": "<i class=\"fa fa-tablet\"> Tablet"
            },
            {
                "value": "fa fa-tachometer",
                "label": "<i class=\"fa fa-tachometer\"> Tachometer"
            },
            {
                "value": "fa fa-tag",
                "label": "<i class=\"fa fa-tag\"> Tag"
            },
            {
                "value": "fa fa-tags",
                "label": "<i class=\"fa fa-tags\"> Tags"
            },
            {
                "value": "fa fa-tasks",
                "label": "<i class=\"fa fa-tasks\"> Tasks"
            },
            {
                "value": "fa fa-taxi",
                "label": "<i class=\"fa fa-taxi\"> Taxi"
            },
            {
                "value": "fa fa-terminal",
                "label": "<i class=\"fa fa-terminal\"> Terminal"
            },
            {
                "value": "fa fa-thumb-tack",
                "label": "<i class=\"fa fa-thumb-tack\"> Thumb-tack"
            },
            {
                "value": "fa fa-thumbs-down",
                "label": "<i class=\"fa fa-thumbs-down\"> Thumbs-down"
            },
            {
                "value": "fa fa-thumbs-o-down",
                "label": "<i class=\"fa fa-thumbs-o-down\"> Thumbs-o-down"
            },
            {
                "value": "fa fa-thumbs-o-up",
                "label": "<i class=\"fa fa-thumbs-o-up\"> Thumbs-o-up"
            },
            {
                "value": "fa fa-thumbs-up",
                "label": "<i class=\"fa fa-thumbs-up\"> Thumbs-up"
            },
            {
                "value": "fa fa-ticket",
                "label": "<i class=\"fa fa-ticket\"> Ticket"
            },
            {
                "value": "fa fa-times",
                "label": "<i class=\"fa fa-times\"> Times"
            },
            {
                "value": "fa fa-times-circle",
                "label": "<i class=\"fa fa-times-circle\"> Times-circle"
            },
            {
                "value": "fa fa-times-circle-o",
                "label": "<i class=\"fa fa-times-circle-o\"> Times-circle-o"
            },
            {
                "value": "fa fa-tint",
                "label": "<i class=\"fa fa-tint\"> Tint"
            },
            {
                "value": "fa fa-toggle-down",
                "label": "<i class=\"fa fa-toggle-down\"> Toggle-down"
            },
            {
                "value": "fa fa-toggle-left",
                "label": "<i class=\"fa fa-toggle-left\"> Toggle-left"
            },
            {
                "value": "fa fa-toggle-off",
                "label": "<i class=\"fa fa-toggle-off\"> Toggle-off"
            },
            {
                "value": "fa fa-toggle-on",
                "label": "<i class=\"fa fa-toggle-on\"> Toggle-on"
            },
            {
                "value": "fa fa-toggle-right",
                "label": "<i class=\"fa fa-toggle-right\"> Toggle-right"
            },
            {
                "value": "fa fa-toggle-up",
                "label": "<i class=\"fa fa-toggle-up\"> Toggle-up"
            },
            {
                "value": "fa fa-trash",
                "label": "<i class=\"fa fa-trash\"> Trash"
            },
            {
                "value": "fa fa-trash-o",
                "label": "<i class=\"fa fa-trash-o\"> Trash-o"
            },
            {
                "value": "fa fa-tree",
                "label": "<i class=\"fa fa-tree\"> Tree"
            },
            {
                "value": "fa fa-trophy",
                "label": "<i class=\"fa fa-trophy\"> Trophy"
            },
            {
                "value": "fa fa-truck",
                "label": "<i class=\"fa fa-truck\"> Truck"
            },
            {
                "value": "fa fa-tty",
                "label": "<i class=\"fa fa-tty\"> Tty"
            },
            {
                "value": "fa fa-umbrella",
                "label": "<i class=\"fa fa-umbrella\"> Umbrella"
            },
            {
                "value": "fa fa-university",
                "label": "<i class=\"fa fa-university\"> University"
            },
            {
                "value": "fa fa-unlock",
                "label": "<i class=\"fa fa-unlock\"> Unlock"
            },
            {
                "value": "fa fa-unlock-alt",
                "label": "<i class=\"fa fa-unlock-alt\"> Unlock-alt"
            },
            {
                "value": "fa fa-unsorted",
                "label": "<i class=\"fa fa-unsorted\"> Unsorted"
            },
            {
                "value": "fa fa-upload",
                "label": "<i class=\"fa fa-upload\"> Upload"
            },
            {
                "value": "fa fa-user",
                "label": "<i class=\"fa fa-user\"> User"
            },
            {
                "value": "fa fa-user-plus",
                "label": "<i class=\"fa fa-user-plus\"> User-plus"
            },
            {
                "value": "fa fa-user-secret",
                "label": "<i class=\"fa fa-user-secret\"> User-secret"
            },
            {
                "value": "fa fa-user-times",
                "label": "<i class=\"fa fa-user-times\"> User-times"
            },
            {
                "value": "fa fa-users",
                "label": "<i class=\"fa fa-users\"> Users"
            },
            {
                "value": "fa fa-video-camera",
                "label": "<i class=\"fa fa-video-camera\"> Video-camera"
            },
            {
                "value": "fa fa-volume-down",
                "label": "<i class=\"fa fa-volume-down\"> Volume-down"
            },
            {
                "value": "fa fa-volume-off",
                "label": "<i class=\"fa fa-volume-off\"> Volume-off"
            },
            {
                "value": "fa fa-volume-up",
                "label": "<i class=\"fa fa-volume-up\"> Volume-up"
            },
            {
                "value": "fa fa-warning",
                "label": "<i class=\"fa fa-warning\"> Warning"
            },
            {
                "value": "fa fa-wheelchair",
                "label": "<i class=\"fa fa-wheelchair\"> Wheelchair"
            },
            {
                "value": "fa fa-wifi",
                "label": "<i class=\"fa fa-wifi\"> Wifi"
            },
            {
                "value": "fa fa-wrench",
                "label": "<i class=\"fa fa-wrench\"> Wrench"
            }
        ]
    });