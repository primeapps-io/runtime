'use strict';

angular.module('primeapps')

    .factory('DocumentService', ['$http', 'config', 'guidEmpty', '$filter', 'helper',
        function ($http, config, guidEmpty, $filter, helper) {
            return {
                getDocuments: function (instanceId, recordId, moduleId) {
                    return $http.post(config.apiUrl + 'Document/GetDocuments', {
                        RecordId: recordId,
                        InstanceID: instanceId,
                        ModuleId:moduleId
                    });
                },

                getEntityDocuments: function (instanceId, recordId, moduleId) {
                    return $http.post(config.apiUrl + 'Document/GetEntityDocuments', {
                        InstanceID: instanceId,
                        ModuleId: moduleId,
                        RecordId: recordId
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

                create: function (instanceId, uniqueName, fileName, mimeType, fileSize, description, recordId, moduleId, chunkSize) {
                    return $http.post(config.apiUrl + 'Document/Create', {
                        UniqueFileName: uniqueName,
                        FileName: fileName,
                        MimeType: mimeType,
                        FileSize: fileSize,
                        ChunkSize: chunkSize || 1,
                        Description: description,
                        InstanceID: instanceId,
                        RecordId : recordId,
                        ModuleId : moduleId
                    });
                },

                update: function (id, instanceId, name, description, recordId, moduleId) {
                    return $http.post(config.apiUrl + 'Document/Modify', {
                        id: id,
                        FileName: name,
                        Description: description,
                        InstanceID: instanceId,
                        RecordId:recordId,
                        ModuleId:moduleId
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
                        documentsResultSet.Documents = $filter('filter')(documentsResultSet.Documents, newFilter, false);
                    }

                    var documentsResults = {
                        documentList: [],
                        totalDocumentCount:documentsResultSet.TotalDocumentsCount,
                        filteredDocumentCount:documentsResultSet.FilteredDocumentsCount
                    }

                    angular.forEach(documentsResultSet.Documents, function (document) {
                        var compareData = document.CreatedBy.id;
                        if(!compareData){
                            compareData = document.CreatedBy;
                        }
                        var createdBy = $filter('filter')(users, {id: compareData}, true)[0];
                        document.CreatedBy = createdBy;
                        document.Timestamp = helper.getTime(document.CreatedTime);
                        document.Extension = helper.getFileExtension(document.Name);
                        document.NamePlain = document.Name.slice(0, document.Name.indexOf(document.Extension) - 1);

                        documentsResults.documentList.push(document);
                    }, documentsResults.documentList);

                    if (sort) {
                        documentsResults.documentList = $filter('orderBy')(documentsResults.documentList, sort, reverse);
                    }

                    return documentsResults;
                }
            };
        }]);