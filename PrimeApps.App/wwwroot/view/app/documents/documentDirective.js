'use strict';

angular.module('primeapps')

    .directive('documentList', ['$filter', 'guidEmpty', 'entityTypes', 'helper', 'operations', 'DocumentService', '$localStorage', 'config', '$stateParams', '$window', '$mdDialog', 'mdToast', '$rootScope',
        function ($filter, guidEmpty, entityTypes, helper, operations, DocumentService, $localStorage, config, $stateParams, $window, $mdDialog, mdToast, $rootScope) {
            return {
                restrict: 'EA',
                scope: {
                    documents: '=',
                    entityId: '=',
                    entityType: '=',
                    isAll: '='
                },
                templateUrl: 'view/app/documents/documentList.html',
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
                        $scope.blobUrl = blobUrl;

                        var tenant_id = $scope.$root.workgroup.tenant_id;
                        var entityId = $scope.entityId;
                        var moduleId = $scope.moduleId;

                        $scope.edit = function (document) {
                            $rootScope.processLanguage(document);
                            $scope.editedDocument = document;
                            $scope.documentState = angular.copy(document);
                            document.editing = true;
                            $scope.copyDesc = angular.copy(document.languages[$rootScope.globalization.Label].description);
                            $scope.copyNamePlain = angular.copy(document.name_plain);
                        };

                        $scope.cancelEdit = function () {
                            this.document = $scope.documentState;
                            $scope.editedDocument = null;
                        };

                        $scope.update = function (document) {
                            if (!$scope.editedDocument || !$scope.editedDocument.name_plain || !$scope.editedDocument.name_plain.trim() || checkInputsIsDirty()) {
                                $scope.editedDocument = null;
                                return;
                            }

                            $scope.documentUpdating = true;
                            var documentName = $scope.editedDocument.name_plain.trim() + '.' + $scope.editedDocument.extension;
                            $scope.editedDocument.languages[$rootScope.globalization.Label].name = documentName;
                            $scope.editedDocument.languages[$rootScope.globalization.Label].description = $scope.editedDocument.description;

                            DocumentService.update($scope.editedDocument.id, $scope.$root.workgroup.tenant_id, $scope.editedDocument, $scope.editedDocument.record_id, $scope.editedDocument.module_id)
                                .then(function () {
                                    $scope.documentUpdating = false;
                                    $scope.editedDocument = null;
                                    document.name = documentName;
                                    document.languages[$rootScope.globalization.Label].name = documentName;
                                    document.languages[$rootScope.globalization.Label].description = $scope.editedDocument.description;
                                    mdToast.success($filter('translate')('Module.AttachUploadSuccess'));
                                })
                                .catch(function () {
                                    $scope.documentUpdating = false;
                                });
                        };

                        $scope.remove = function (ev, document) {
                            var confirm = $mdDialog.confirm()
                                .title($filter('translate')('Common.AreYouSure'))
                                .targetEvent(ev)
                                .ok($filter('translate')('Common.Yes'))
                                .cancel($filter('translate')('Common.No'));

                            $mdDialog.show(confirm).then(function () {
                                DocumentService.remove(document)
                                    .then(function () {
                                        mdToast.success($filter('translate')('Module.AttachSuccesDeleteRecordMessage'));
                                        if ($scope.isAll) {
                                            DocumentService.getDocuments(tenant_id, entityId, moduleId)
                                                .then(function (response) {
                                                    $rootScope.processLanguages(response.data.documents);
                                                    var processResults = DocumentService.processDocuments(response.data, $scope.$root.users);
                                                    $scope.$parent.documents = processResults.documentList;
                                                    if ($scope.$parent.documentsResultSet) {
                                                        $scope.$parent.documentsResultSet.totalDocumentCount = response.data.total_documents_count;
                                                    }
                                                });
                                        } else {
                                            DocumentService.getEntityDocuments(tenant_id, entityId, moduleId)
                                                .then(function (response) {
                                                    $rootScope.processLanguages(response.data.documents);
                                                    var processResults = DocumentService.processDocuments(response.data, $scope.$root.users);
                                                    $scope.$parent.documents = processResults.documentList;
                                                    $scope.$parent.documentsResultSet.totalDocumentCount = response.data.total_documents_count;
                                                });
                                        }
                                    });
                            }, function () {
                                //cancel
                            });

                        };

                        $scope.getParentEntityName = function (entityType, entityId) {
                            var entity = $filter('filter')($scope.$root.clientData, {
                                EntityType: entityType,
                                EntityID: entityId
                            })[0];

                            if (entity)
                                return entity.EntityName;

                            return '';
                        };

                        $scope.icon = function (extension) {
                            var icon = 'fas ';

                            switch (extension) {
                                case 'doc':
                                case 'docx':
                                    icon += 'fa-file-word';
                                    break;
                                case 'xls':
                                case 'xlsx':
                                    icon += 'fa-file-excel';
                                    break;
                                case 'ppt':
                                case 'pptx':
                                    icon += 'fa-file-powerpoint';
                                    break;
                                case 'pdf':
                                    icon += 'fa-file-pdf';
                                    break;
                                case 'txt':
                                    icon += 'fa-file-alt';
                                    break;
                                case 'bmp':
                                case 'jpeg':
                                case 'jpg':
                                case 'png':
                                case 'gif':
                                    icon += 'fa-file-image';
                                    break;
                                case 'zip':
                                    icon += 'fa-file-archive';
                                    break;
                                case 'rar':
                                    icon += 'fa-file-archive';
                                    break;
                                case 'eml':
                                case 'msg':
                                    icon += 'fa fa-envelope';
                                    break;
                                default:
                                    icon += 'fa-file';
                                    break;
                            }

                            return icon;
                        };

                        $scope.downloadDocument = function (document) {
                            DocumentService.getDocument(document.id)
                                .then(function (doc) {
                                    if (doc.data) {
                                        $window.open("/storage/download?fileId=" + document.id, "_blank");
                                    } else {
                                        mdToast.warning($filter('translate')('Documents.NotFound'));
                                    }
                                });
                        };

                        $scope.getDownloadUrl = function (document) {
                            return config.apiUrl + 'storage/download?fileID=' + document.id + '&access_token=' + $localStorage.read('access_token');
                        };

                        $scope.showLightBox = function (ev, fileData, Index) {
                            $scope.lightBox = true;
                            $scope.fileData = fileData;
                            $scope.Index = Index;
                            $mdDialog.show({
                                contentElement: '#mdLightbox-doc',
                                parent: angular.element(document.body),
                                targetEvent: ev,
                                clickOutsideToClose: true,
                                fullscreen: false // Only for -xs, -sm breakpoints.
                            });
                        };
                        // $window.addEventListener("keydown", function (event) {
                        //     switch (event.key) {
                        //         case "ArrowLeft":
                        //             angular.element(document.getElementById('previous')).triggerHandler('click');
                        //             break;
                        //         case "ArrowRight":
                        //             angular.element(document.getElementById('next')).triggerHandler('click');
                        //             break;
                        //         case "Escape":
                        //             angular.element(document.getElementById('close-lightbox')).triggerHandler('click');
                        //             break;
                        //     }
                        // });

                        $scope.closeLightBox = function () {
                            $scope.lightBox = false;
                            $mdDialog.hide();
                        };
                        //check fr update
                        var checkInputsIsDirty = function () {
                            return $scope.copyNamePlain === $scope.editedDocument.name_plain && $scope.copyDesc === $scope.editedDocument.description;
                        };
                    }]
            };
        }
    ])

    .directive('documentForm', ['config', 'guidEmpty', '$localStorage', '$filter', '$q', 'helper', 'FileUploader', 'resizeService', 'DocumentService', '$timeout', '$stateParams', '$cookies', 'mdToast', '$rootScope',
        function (config, guidEmpty, $localStorage, $filter, $q, helper, FileUploader, resizeService, DocumentService, $timeout, $stateParams, $cookies, mdToast, $rootScope) {
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
                templateUrl: 'view/app/documents/documentForm.html',
                controller: ['$scope', '$timeout',
                    function ($scope, $timeout) {
                        $scope.documentCreating = false;
                        var tenant_id = $scope.$root.workgroup.tenant_id;
                        var entityId = $scope.entityId;
                        var stateType = $stateParams.type;
                        $scope.module = $filter('filter')($scope.$root.modules, { name: stateType }, true)[0];
                        var moduleId = $scope.module.id;

                        var uploader = $scope.uploader = $scope.customUploader || new FileUploader({
                            url: 'storage/upload_whole'
                        });

                        /*Record id yoksa, eklentilerdeki uploader'ı module uploader'a setliyoruz*/
                        if (!$scope.entityId) {
                            $scope.$parent.$parent.uploader = $scope.uploader;
                        }

                        uploader.onCompleteItem = function (fileItem, response, status, headers) {
                            if (status === 200) {
                                var uniqueName = response.unique_name;
                                var chunkSize = response.chunks;
                                var fileName = fileItem.file.name;
                                var mimeType = response.content_type;
                                var fileSize = fileItem._file.size;
                                var description = fileName || '';
                                entityId = entityId ? entityId : $scope.$parent.entityIdFunc();

                                if (!$scope.customUploader) {
                                    DocumentService.create(tenant_id, uniqueName, fileName, mimeType, fileSize, description, entityId, moduleId, chunkSize).then(function onSuccess() {
                                        mdToast.success($filter('translate')('Module.AttachUploadSuccess'));
                                    });
                                } else {
                                    DocumentService.create(tenant_id, uniqueName, fileName, mimeType, fileSize, description, entityId, moduleId, chunkSize).then(function onSuccess() {
                                        mdToast.success($filter('translate')('Module.AttachUploadSuccess'));
                                    });
                                }
                            }
                        };

                        if (!$scope.customUploader) {
                            uploader.onCompleteAll = function () {
                                $timeout(function () {
                                    if ($scope.isAll) {
                                        DocumentService.getDocuments(tenant_id, entityId, moduleId)
                                            .then(function (response) {
                                                $rootScope.processLanguages(response.data.documents);
                                                var processResults = DocumentService.processDocuments(response.data, $scope.$root.users);
                                                $scope.$parent.documents = processResults.documentList;
                                                $scope.$parent.documentsResultSet.totalDocumentCount = response.data.total_documents_count;
                                            });
                                    } else {
                                        DocumentService.getEntityDocuments(tenant_id, entityId, moduleId)
                                            .then(function (response) {
                                                $rootScope.processLanguages(response.data.documents);
                                                var processResults = DocumentService.processDocuments(response.data, $scope.$root.users);
                                                $scope.$parent.documents = processResults.documentList;
                                                $scope.$parent.documentsResultSet.totalDocumentCount = response.data.total_documents_count;
                                            });
                                    }
                                }, 2000);
                            };
                        }

                        uploader.onWhenAddingFileFailed = function (item, filter, options) {
                            switch (filter.name) {
                                case 'docFilter':
                                    mdToast.warning($filter('translate')('Documents.FormatError'));
                                    break;
                                case 'sizeFilter':
                                    mdToast.warning($filter('translate')('Documents.SizeError'));
                                    break;
                                case 'limitFilter':
                                    mdToast.warning($filter('translate')('Documents.LimitError'));
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
                                } else if (extension === 'zip') {
                                    var type = '|zip|';
                                } else if (extension === 'eml') {
                                    var type = '|eml|';
                                } else if (extension === 'msg') {
                                    var type = '|msg|';
                                } else {
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

                        $scope.close = function () {
                            $scope.$parent.showDocumentForm = !$scope.$parent.showDocumentForm;
                        };

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

