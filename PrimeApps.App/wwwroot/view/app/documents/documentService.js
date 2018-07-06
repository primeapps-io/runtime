'use strict';

angular.module('primeapps')

    .factory('DocumentService', ['$http', 'config', 'guidEmpty', '$filter', 'helper',
        function ($http, config, guidEmpty, $filter, helper) {
            return {
                getDocuments: function (tenantId, recordId, moduleId) {
                    return $http.post(config.apiUrl + 'Document/GetDocuments', {
                        record_id: recordId,
                        tenant_id: tenantId,
                        module_id:moduleId
                    });
                },

                getEntityDocuments: function (tenantId, recordId, moduleId) {
                    return $http.post(config.apiUrl + 'Document/GetEntityDocuments', {
                        tenant_id: tenantId,
                        module_id: moduleId,
                        record_id: recordId
                    });
                },
                getDocument: function (fileId) {
                    return $http.post(config.apiUrl + 'Document/GetDocument', fileId);
                },
                removeModuleDocument : function(data){
                    return $http.post(config.apiUrl + 'Document/remove_document', data);
                },
                advancedSearch : function(moduleName,filters,take,skip){
                    //return $http.post(config.apiUrl + 'Document/document_search', filters);

                    return $http.post(config.apiUrl + 'Document/document_search', {
                        module: moduleName,
                        filters: filters,
                        top:take,
                        skip:skip
                    });
                },

                create: function (tenant_id, uniqueName, fileName, mimeType, fileSize, description, recordId, moduleId, chunkSize) {
                    return $http.post(config.apiUrl + 'Document/Create', {
                        unique_file_name: uniqueName,
                        file_name: fileName,
                        mime_type: mimeType,
                        file_size: fileSize,
                        chunk_size: chunkSize || 1,
                        description: description,
                        tenant_id: tenant_id,
                        record_id : recordId,
                        module_id : moduleId
                    });
                },

                update: function (id, instanceId, name, description, recordId, moduleId) {
                    return $http.post(config.apiUrl + 'Document/Modify', {
                        id: id,
                        file_name: name,
                        description: description,
                        instance_id: instanceId,
                        record_id:recordId,
                        module_id:moduleId
                    });
                },

                remove: function (document) {
                    return $http.post(config.apiUrl + 'Document/Remove', document);
                },

                upload: function (formData) {
                    return $http.post(
                        config.apiUrl + 'Document/Upload',
                        formData,
                        {
                            headers: {'Content-Type': undefined},
                            transformRequest: angular.identity
                        }
                    );
                },

                move: function (id, entityId, entityType) {
                    return $http.post(config.apiUrl + 'Document/Move', {
                        ID: id,
                        EntityID: entityId,
                        EntityType: entityType
                    });
                },

                processDocuments: function (documentsResultSet, users, filter, sort, reverse) {
                    if (filter) {
                        var newFilter = angular.copy(filter);
                        if (newFilter.CreatedBy == guidEmpty){
                            delete newFilter.CreatedBy;
                        }
                        documentsResultSet.documents = $filter('filter')(documentsResultSet.documents, newFilter, false);
                    }

                    var documentsResults = {
                        documentList: [],
                        totalDocumentCount: documentsResultSet.total_documents_count,
                        filteredDocumentCount: documentsResultSet.filtered_documents_count
                    };

                    angular.forEach(documentsResultSet.documents, function (document) {
                        var compareData = document.created_by.id;
                        if(!compareData){
                            compareData = document.created_by;
                        }
                        var createdBy = $filter('filter')(users, {id: compareData}, true)[0];
                        document.created_by = createdBy;
                        document.timestamp = helper.getTime(document.created_time);
                        document.extension = helper.getFileExtension(document.name);
                        document.name_plain = document.name.slice(0, document.name.indexOf(document.Extension) - 1);

                        documentsResults.documentList.push(document);
                    }, documentsResults.documentList);

                    if (sort) {
                        documentsResults.documentList = $filter('orderBy')(documentsResults.documentList, sort, reverse);
                    }

                    return documentsResults;
                }
            };
        }]);