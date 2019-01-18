'use strict';

angular.module('primeapps')

    .directive('documentList', ['$filter', 'guidEmpty', 'entityTypes', 'helper', 'operations', 'DocumentService', '$localStorage', 'config', '$stateParams', '$window', 'ngToast',
        function ($filter, guidEmpty, entityTypes, helper, operations, DocumentService, $localStorage, config, $stateParams, $window, ngToast) {
            return {
                restrict: 'EA',
                scope: {
                    documents: '=',
                    entityId: '=',
                    entityType: '=',
                    isAll: '='
                },
                templateUrl: cdnUrl + 'view/app/documents/documentList.html',
                controller: ['$scope',
                    function ($scope) {
                        $scope.editedDocument = null;
                        $scope.documentState = {};
                        $scope.documentUpdating = false;
                        $scope.apiUrl = $scope.$root.config.apiUrl;
                        $scope.guidEmpty = guidEmpty;
                        $scope.hasPermission = helper.hasPermission;
                        $scope.hasDocumentsPermission = helper.hasDocumentsPermission;
                        $scope.entityTypes = entityTypes;
                        $scope.operations = operations;
                        $scope.lightBox = false;
                        $scope.type = $stateParams.type;
                        $scope.module = $filter('filter')($scope.$root.modules, { name: $stateParams.type }, true)[0];
                        $scope.moduleId = $scope.module.id;


                        var tenant_id = $scope.$root.workgroup.tenant_id;
                        var entityId = $scope.entityId;
                        var moduleId = $scope.moduleId;

                        $scope.edit = function (document) {
                            $scope.editedDocument = document;
                            $scope.documentState = angular.copy(document);
                            document.editing = true;
                        };

                        $scope.cancelEdit = function () {
                            this.document = $scope.documentState;
                            $scope.editedDocument = null;
                        };

                        $scope.update = function (document) {
                            if (!$scope.editedDocument || !$scope.editedDocument.name_plain || !$scope.editedDocument.name_plain.trim())
                                return;

                            $scope.documentUpdating = true;
                            var documentName = $scope.editedDocument.NamePlain.trim() + '.' + $scope.editedDocument.Extension;

                            DocumentService.update($scope.editedDocument.id, $scope.$root.workgroup.tenant_id, documentName, $scope.editedDocument.description, $scope.editedDocument.RecordId, $scope.editedDocument.ModuleId)
                                .then(function () {
                                    $scope.documentUpdating = false;
                                    $scope.editedDocument = null;
                                    document.Name = documentName;
                                })
                                .catch(function () {
                                    $scope.documentUpdating = false;
                                });
                        };

                        $scope.remove = function (document) {
                            DocumentService.remove(document)
                                .then(function () {
                                    if ($scope.isAll) {
                                        DocumentService.getDocuments(tenant_id, entityId, moduleId)
                                            .then(function (data) {
                                                var processResults = DocumentService.processDocuments(data.data, $scope.$root.users);
                                                $scope.$parent.documents = processResults.documentList;
                                                if ($scope.$parent.documentsResultSet) {
                                                    $scope.$parent.documentsResultSet.totalDocumentCount = data.data.total_documents_count;
                                                }
                                            });
                                    }
                                    else {
                                        DocumentService.getEntityDocuments(tenant_id, entityId, moduleId)
                                            .then(function (data) {
                                                var processResults = DocumentService.processDocuments(data.data, $scope.$root.users);
                                                $scope.$parent.documents = processResults.documentList;
                                                $scope.$parent.documentsResultSet.totalDocumentCount = data.data.total_documents_count;
                                            });
                                    }
                                });
                        };

                        $scope.getParentEntityName = function (entityType, entityId) {
                            var entity = $filter('filter')($scope.$root.clientData, { EntityType: entityType, EntityID: entityId })[0];

                            if (entity)
                                return entity.EntityName;

                            return '';
                        };

                        $scope.icon = function (extension) {
                            var icon = 'fa fa-2x ';

                            switch (extension) {
                                case 'doc':
                                case 'docx':
                                    icon += 'fa-file-word-o';
                                    break;
                                case 'xls':
                                case 'xlsx':
                                    icon += 'fa-file-excel-o';
                                    break;
                                case 'ppt':
                                case 'pptx':
                                    icon += 'fa-file-powerpoint-o';
                                    break;
                                case 'pdf':
                                    icon += 'fa-file-pdf-o';
                                    break;
                                case 'txt':
                                    icon += 'fa-file-text-o';
                                    break;
                                case 'bmp':
                                case 'jpeg':
                                case 'jpg':
                                case 'png':
                                case 'gif':
                                    icon += 'fa-file-image-o';
                                    break;
                                case 'zip':
                                    icon += 'fa-file-archive-o';
                                    break;
                                case 'rar':
                                    icon += 'fa-file-archive-o';
                                    break;
                                case 'eml':
                                case 'msg':
                                    icon += 'fa fa-envelope';
                                    break;
                                default:
                                    icon += 'fa-file-o';
                                    break;
                            }

                            return icon;
                        };

                        $scope.downloadDocument = function (document) {
                            DocumentService.getDocument(document.id)
                                .then(function (doc) {
                                    if (doc.data) {
                                        $window.open("/api/storage/download?fileId=" + document.id, "_blank");
                                        //var downloadUrl = $scope.getDownloadUrl(document);
                                        //if(downloadUrl){
                                        //$window.location = downloadUrl;
                                        //$window.open(downloadUrl, "_blank");

                                        //}
                                    }
                                    else {
                                        ngToast.create({ content: $filter('translate')('Documents.NotFound'), className: 'warning', timeout: 6000 });
                                    }
                                });
                        };

                        $scope.getDownloadUrl = function (document) {
                            return config.apiUrl + 'storage/download?fileID=' + document.id + '&access_token=' + $localStorage.read('access_token');
                        };

                        $scope.showLightBox = function (fileData, Index) {
                            $scope.lightBox = true;
                            $scope.fileData = fileData;
                            $scope.Index = Index;
                        };
                        $window.addEventListener("keydown", function (event) {
                            switch (event.key) {
                                case "ArrowLeft":
                                    angular.element(document.getElementById('previous')).triggerHandler('click');
                                    break;
                                case "ArrowRight":
                                    angular.element(document.getElementById('next')).triggerHandler('click');
                                    break;
                                case "Escape":
                                    angular.element(document.getElementById('close-lightbox')).triggerHandler('click');
                                    break;
                            }

                        });
                        $scope.closeLightBox = function () {
                            $scope.lightBox = false
                        };
                    }]
            };
        }
    ])

    .directive('documentForm', ['config', 'guidEmpty', '$localStorage', '$filter', '$q', 'ngToast', 'helper', 'FileUploader', 'resizeService', 'DocumentService', '$timeout', '$stateParams', '$cookies',
        function (config, guidEmpty, $localStorage, $filter, $q, ngToast, helper, FileUploader, resizeService, DocumentService, $timeout, $stateParams, $cookies) {
            return {
                restrict: 'EA',
                scope: {
                    entityId: '=',
                    entityType: '=',
                    isAll: '=',
                    show: '=',
                    hideCloseButton: '=',
                    hideUploadButton: '=',
                    hideAllPanel: '=',
                    customUploader: '=',
                    entityIdFunc: '&',
                    moduleId: '='
                },
                templateUrl: cdnUrl + 'view/app/documents/documentForm.html',
                controller: ['$scope', '$timeout',
                    function ($scope, $timeout) {
                        $scope.documentCreating = false;
                        var tenant_id = $scope.$root.workgroup.tenant_id;
                        var entityId = $scope.entityId;
                        var stateType = $stateParams.type;
                        $scope.module = $filter('filter')($scope.$root.modules, { name: stateType }, true)[0];
                        var moduleId = $scope.module.id;

                        var uploader = $scope.uploader = $scope.customUploader || new FileUploader({
                            url: config.apiUrl + 'storage/upload_whole',
                            headers: {
                                'Authorization': 'Bearer ' + $localStorage.read('access_token'),
                                "Content-Type": "application/json", "Accept": "application/json",
                                'X-Tenant-Id': $cookies.get('tenant_id')
                            }
                        });

                        uploader.onCompleteItem = function (fileItem, response, status, headers) {
                            if (status === 200) {
                                var uniqueName = response.unique_name;
                                var chunkSize = response.chunks;
                                var fileName = fileItem.file.name
                                var mimeType = response.content_type;
                                var fileSize = fileItem._file.size;
                                var description = '';

                                if (!$scope.customUploader) {
                                    DocumentService.create(tenant_id, uniqueName, fileName, mimeType, fileSize, description, entityId, moduleId, chunkSize);
                                }
                                else {
                                    entityId = $scope.entityIdFunc();

                                    DocumentService.create(tenant_id, uniqueName, fileName, mimeType, fileSize, description, entityId, moduleId, chunkSize);
                                }
                            }
                        };

                        if (!$scope.customUploader) {
                            uploader.onCompleteAll = function () {
                                $timeout(function () {
                                    if ($scope.isAll) {
                                        DocumentService.getDocuments(tenant_id, entityId, moduleId)
                                            .then(function (data) {
                                                var processResults = DocumentService.processDocuments(data.data, $scope.$root.users);
                                                $scope.$parent.documents = processResults.documentList;
                                                $scope.$parent.documentsResultSet.totalDocumentCount = data.data.total_documents_count;
                                            });
                                    }
                                    else {
                                        DocumentService.getEntityDocuments(tenant_id, entityId, moduleId)
                                            .then(function (data) {
                                                var processResults = DocumentService.processDocuments(data.data, $scope.$root.users);
                                                $scope.$parent.documents = processResults.documentList;
                                                $scope.$parent.documentsResultSet.totalDocumentCount = data.data.total_documents_count;
                                            });
                                    }
                                }, 2000);
                            };
                        }

                        uploader.onWhenAddingFileFailed = function (item, filter, options) {
                            switch (filter.name) {
                                case 'docFilter':
                                    ngToast.create({ content: $filter('translate')('Documents.FormatError'), className: 'warning', timeout: 8000 });
                                    break;
                                case 'sizeFilter':
                                    ngToast.create({ content: $filter('translate')('Documents.SizeError'), className: 'warning', timeout: 6000 });
                                    break;
                                case 'limitFilter':
                                    ngToast.create({ content: $filter('translate')('Documents.LimitError'), className: 'warning', timeout: 6000 });
                                    break;
                            }
                        };

                        uploader.onAfterAddingFile = function (item) {
                            var type = '|' + item.file.type.slice(item.file.type.lastIndexOf('/') + 1) + '|';

                            if ('|jpg|png|jpeg|bmp|gif'.indexOf(type) > -1) {
                                readFile(item._file)
                                    .then(function (image) {
                                        item.image = image;
                                        var img = new Image();

                                        img.onload = function () {
                                            if (img.width <= 1024)
                                                return;

                                            resizeService.resizeImage(item.image, { width: 1024 }, function (err, resizedImage) {
                                                if (err)
                                                    return;

                                                item._file = dataURItoBlob(resizedImage);
                                                item.file.size = item._file.size;
                                            });
                                        };

                                        img.src = image;
                                    });
                            }
                        };

                        uploader.filters.push({
                            name: 'docFilter',
                            fn: function (item, options) {
                                var extension = helper.getFileExtension(item.name);

                                if (extension === 'mpp')
                                    return true;

                                if (extension === 'rar') {
                                    var type = '|x-rar-compressed|';
                                }
                                else if (extension === 'zip') {
                                    var type = '|zip|';
                                }
                                else if (extension === 'eml') {
                                    var type = '|eml|';
                                } else if (extension === 'msg') {
                                    var type = '|msg|';
                                }
                                else {
                                    var type = '|' + item.type.slice(item.type.lastIndexOf('/') + 1) + '|';
                                }
                                return '|msword|x-rar-compressed|zip|eml|msg|vnd.ms-excel|vnd.ms-powerpoint|vnd.openxmlformats-officedocument.wordprocessingml.document|vnd.openxmlformats-officedocument.wordprocessingml.template|vnd.openxmlformats-officedocument.spreadsheetml.sheet|vnd.openxmlformats-officedocument.presentationml.presentation|vnd.openxmlformats-officedocument.presentationml.template|vnd.openxmlformats-officedocument.presentationml.slideshow|rtf|pdf|plain|tiff|bmp|jpeg|jpg|png|gif|'.indexOf(type) > -1;
                            }
                        });

                        uploader.filters.push({
                            name: 'sizeFilter',
                            fn: function (item) {
                                return item.size < 10485760;//10 mb
                            }
                        });

                        //uploader.filters.push({
                        //    name: 'limitFilter',
                        //    fn: function (item) {
                        //        return ($scope.$root.licenseStatus.License.StorageSize - $scope.$root.licenseStatus.LicenseUsage.FileStorageSize) >= item.size;
                        //    }
                        //});

                        var dataURItoBlob = function (dataURI) {
                            var binary = atob(dataURI.split(',')[1]);
                            var mimeString = dataURI.split(',')[0].split(':')[1].split(';')[0];
                            var array = [];

                            for (var i = 0; i < binary.length; i++) {
                                array.push(binary.charCodeAt(i));
                            }

                            return new Blob([new Uint8Array(array)], { type: mimeString });
                        };

                        function readFile(file) {
                            var deferred = $q.defer();
                            var reader = new FileReader();

                            reader.onload = function (e) {
                                deferred.resolve(e.target.result);
                            };

                            reader.readAsDataURL(file);

                            return deferred.promise;
                        }

                    }]
            };
        }])

    .directive('lightboxDirective', function () {
        return {
            restrict: 'E', // applied on 'element'
            transclude: true, // re-use the inner HTML of the directive
            template: '<section ng-transclude></section>' // need this so that inner HTML will be used
        }
    });

