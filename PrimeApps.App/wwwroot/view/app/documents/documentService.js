'use strict';

angular.module('primeapps')

    .factory('DocumentService', ['$http', 'config', 'guidEmpty', '$filter', 'helper', '$rootScope',
        function ($http, config, guidEmpty, $filter, helper, $rootScope) {
            return {
                getDocuments: function (tenantId, recordId, moduleId) {
                    return $http.post(config.apiUrl + 'document/get_documents', {
                        record_id: recordId,
                        tenant_id: tenantId,
                        module_id: moduleId
                    });
                },

                getEntityDocuments: function (tenantId, recordId, moduleId) {
                    return $http.post(config.apiUrl + 'document/get_entity_documents', {
                        tenant_id: tenantId,
                        module_id: moduleId,
                        record_id: recordId
                    });
                },
                getDocument: function (fileId) {
                    return $http.post(config.apiUrl + 'document/get_document', fileId);
                },
                removeModuleDocument: function (data) {
                    return $http.post(config.apiUrl + 'document/remove_document', data);
                },
                advancedSearch: function (moduleName, filters, take, skip) {
                    //return $http.post(config.apiUrl + 'document/document_search', filters);

                    return $http.post(config.apiUrl + 'document/document_search', {
                        module: moduleName,
                        filters: filters,
                        top: take,
                        skip: skip
                    });
                },

                create: function (tenant_id, uniqueName, fileName, mimeType, fileSize, description, recordId, moduleId, chunkSize) {
                    var languages = {};
                    languages[$rootScope.globalization.Label] = {
                        'name': fileName,
                            'description': description
                    };
                    
                    return $http.post(config.apiUrl + 'document/create', {
                        unique_file_name: uniqueName,
                        file_name: fileName,
                        mime_type: mimeType,
                        file_size: fileSize,
                        chunk_size: chunkSize || 1,
                        description: description,
                        tenant_id: tenant_id,
                        record_id: recordId,
                        module_id: moduleId,
                        languages: JSON.stringify(languages)
                    });
                },

                update: function (id, instanceId, document, recordId, moduleId) {
                    $rootScope.languageStringify(document);
                    return $http.post(config.apiUrl + 'document/modify', {
                        id: id,
                        file_name: document.name,
                        description: document.description,
                        instance_id: instanceId,
                        record_id: recordId,
                        module_id: moduleId,
                        languages: document.languages
                    });
                },

                remove: function (document) {
                    return $http.post(config.apiUrl + 'document/remove', document);
                },

                upload: function (formData) {
                    return $http.post(
                        config.apiUrl + 'document/Upload',
                        formData,
                        {
                            headers: { 'Content-Type': undefined },
                            transformRequest: angular.identity
                        }
                    );
                },

                processDocuments: function (documentsResultSet, users, filter, sort, reverse) {
                    if (filter) {
                        var newFilter = angular.copy(filter);
                        if (newFilter.created_by === guidEmpty) {
                            delete newFilter.created_by;
                        }
                        documentsResultSet.documents = $filter('filter')(documentsResultSet.documents, newFilter, false);
                    }

                    var documentsResults = {
                        documentList: [],
                        totalDocumentCount: documentsResultSet.total_documents_count,
                        filteredDocumentCount: documentsResultSet.filtered_documents_count
                    };

                    angular.forEach(documentsResultSet.documents, function (document) {
                        var compareData = angular.isObject(document.created_by) ? document.created_by.id : document.created_by;
                        var createdBy = $filter('filter')(users, { id: compareData }, true)[0];
                        var name = document.languages[$rootScope.globalization.Label]['name'];
                        document.name = name;
                        document.description = document.languages[$rootScope.globalization.Label]['description'];
                        document.created_by = createdBy;
                        document.timestamp = helper.getTime(document.created_time);
                        document.extension = helper.getFileExtension(document.name);
                        document.name_plain = name.slice(0, name.indexOf(document.extension) - 1);

                        documentsResults.documentList.push(document);
                    }, documentsResults.documentList);

                    if (sort) {
                        documentsResults.documentList = $filter('orderBy')(documentsResults.documentList, sort, reverse);
                    }

                    return documentsResults;
                }
            };
        }]);