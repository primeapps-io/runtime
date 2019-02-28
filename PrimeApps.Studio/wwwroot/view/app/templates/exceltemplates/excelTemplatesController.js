'use strict';

angular.module('primeapps')

    .controller('ExcelTemplatesController', ['$rootScope', '$scope', '$state', '$stateParams', '$location', '$filter', '$cache', '$q', 'helper', 'dragularService', 'operators', 'ExcelTemplatesService', '$http', 'config', '$modal', '$cookies', '$window', 'FileUploader',
        function ($rootScope, $scope, $state, $stateParams, $location, $filter, $cache, $q, helper, dragularService, operators, ExcelTemplatesService, $http, config, $modal, $cookies, $window, FileUploader) {

            //$scope.$parent.menuTopTitle = "Templates";
            //$scope.$parent.activeMenu = 'templates';
            $scope.$parent.activeMenuItem = 'templatesExcel';

            $rootScope.breadcrumblist[2].title = 'Excel Templates';

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }
            };
            $scope.generator(10);

            $scope.loading = true;

            $scope.requestModel = {
                limit: '10',
                offset: 0
            };

            ExcelTemplatesService.count("excel").then(function (response) {
                $scope.pageTotal = response.data;
            });

            ExcelTemplatesService.find($scope.requestModel, "excel").then(function (response) {
                var templates = response.data;
                angular.forEach(templates, function (template) {
                    template.module = $filter('filter')($rootScope.appModules, { name: template.module }, true)[0];
                });
                $scope.templates = templates;
                $scope.templatesState = templates;

            }).finally(function () {
                $scope.loading = false;
            });

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;

                ExcelTemplatesService.find(requestModel, "excel").then(function (response) {

                    var templates = response.data;
                    angular.forEach(templates, function (template) {
                        template.module = $filter('filter')($rootScope.appModules, { name: template.module }, true)[0];
                    });
                    $scope.templates = templates;
                    $scope.templatesState = templates;

                }).finally(function () {
                    $scope.loading = false;
                });
            };

            $scope.changeOffset = function () {
                $scope.changePage(1);
            };

            $scope.showFormModal = function (template) {
                $scope.requiredColor = "";
                if (template) {
                    setCurrentTemplate(template);
                    // $scope.getDownloadUrlExcel();
                }
                else {
                    $scope.template = [];
                }

                $scope.addNewExcelTemplateFormModal = $scope.addNewExcelTemplateFormModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/templates/exceltemplates/excelTemplatesForm.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.addNewExcelTemplateFormModal.$promise.then(function () {
                    $scope.addNewExcelTemplateFormModal.show();
                });
            };

            $scope.showTemplateGuideModal = function () {
                // $scope.getDownloadUrl();
                $scope.excelTemplateGuideModal = $scope.excelTemplateGuideModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/templates/exceltemplates/excelTemplateGuide.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.excelTemplateGuideModal.$promise.then(function () {
                    $scope.excelTemplateGuideModal.show();
                });
            };

            // $scope.fileUpload = {
            //     settings: {
            //         multi_selection: false,
            //         unique_names: false,
            //         url: 'storage/upload_template',
            //         chunk_size: '256kb',
            //         headers: {
            //             'Authorization': 'Bearer ' + window.localStorage.getItem('access_token'),//$localStorage.get('access_token'),
            //             'Accept': 'application/json',
            //             'X-Organization-Id': $rootScope.currentOrgId,
            //             'X-App-Id': $scope.appId
            //         },
            //         filters: {
            //             mime_types: [
            //                 { title: "Template Files", extensions: "xls,xlsx" },
            //             ],
            //             max_file_size: "10mb"
            //         }
            //     },
            //     events: {
            //         fileUploaded: function (uploader, file, response) {
            //             var resp = JSON.parse(response.response);
            //             var template = {
            //                 name: $scope.template.templateName,
            //                 module: $scope.template.templateModule.name,
            //                 template_type: 'excel',
            //                 content: resp.unique_name,
            //                 content_type: resp.content_type,
            //                 chunks: resp.chunks,
            //                 subject: "Excel",
            //                 active: $scope.template.active,
            //                 permissions: $scope.template.permissions
            //             };
            //
            //             if (!$scope.template.id) {
            //                 ExcelTemplatesService.create(template)
            //                     .then(function () {
            //                         success();
            //                     })
            //                     .catch(function () {
            //                         $scope.saving = false;
            //                     });
            //             }
            //             else {
            //                 template.id = $scope.template.id;
            //
            //                 ExcelTemplatesService.update(template)
            //                     .then(function () {
            //                         success();
            //                     })
            //                     .catch(function () {
            //                         $scope.saving = false;
            //                     });
            //             }
            //         }
            //     }
            // };

            var fileUpload = $scope.fileUpload = new FileUploader({
                url: 'storage/upload_template',
                chunk_size: '256kb',
                headers: {
                    'Authorization': 'Bearer ' + window.localStorage.getItem('access_token'),
                    'Accept': 'application/json',
                    'X-Organization-Id': $rootScope.currentOrgId,
                    'X-App-Id': $scope.appId
                },
                queueLimit: 1
            });

            fileUpload.onAfterAddingFile = function (item) {

                var reader = new FileReader();

                // reader.onload = function (event) {
                //     $scope.$apply(function () {
                //         item.template = event.target.result;
                //     });
                // };
                reader.readAsDataURL(item._file);
            };

            fileUpload.onWhenAddingFileFailed = function (item, filter, options) {
                switch (filter.name) {
                    case 'excelFilter':
                        toastr.warning($filter('translate')('Data.Import.FormatError'));
                        break;
                    case 'sizeFilter':
                        toastr.warning($filter('translate')('Setup.Settings.SizeError'));
                        break;
                }
            };

            fileUpload.filters.push({
                name: 'excelFilter',
                fn: function (item, options) {
                    var extension = helper.getFileExtension(item.name);
                    return true ? (extension == 'xls' || extension == 'xlsx') : false;
                }
            });

            fileUpload.filters.push({
                name: 'sizeFilter',
                fn: function (item) {
                    return item.size < 10485760;//10 mb
                }
            });

            $scope.remove = function () {
                if (fileUpload.queue[0]) {
                    fileUpload.queue[0].remove();
                    $scope.templateFileCleared = true;
                }
                else {
                    $scope.template.content = undefined;
                    $scope.templateFileCleared = true;
                }
            };

            $scope.save = function (uploadForm) {

                if (!uploadForm.$valid || !$scope.logoUpload) {
                    $scope.requiredColor = 'background-color:rgba(206, 4, 4, 0.15) !important;';
                    return;
                }

                $scope.saving = true;
                var header = {
                    'Authorization': 'Bearer ' + window.localStorage.getItem('access_token'),
                    'Accept': 'application/json',
                    'X-Organization-Id': $rootScope.currentOrgId,
                    'X-App-Id': $scope.appId
                };

                if (!$scope.template.id) {
                    //$scope.fileUpload.uploader.start();
                    fileUpload.queue[0].uploader.headers = header;
                    fileUpload.queue[0].headers = header;
                    fileUpload.queue[0].upload();
                    fileUpload.onCompleteItem = function (fileItem, tempInfo, status) {
                        uploadThenComplete(fileItem, tempInfo, status);
                    };
                    $scope.pageTotal = $scope.pageTotal + 1;
                }
                else {
                    if ($scope.templateFileCleared) {
                        //$scope.fileUpload.uploader.start();
                        fileUpload.queue[0].uploader.headers = header;
                        fileUpload.queue[0].headers = header;
                        fileUpload.queue[0].upload();
                        fileUpload.onCompleteItem = function (fileItem, tempInfo, status) {
                            uploadThenComplete(fileItem, tempInfo, status);
                        };
                    }
                    else {
                        var template = angular.copy($scope.template);
                        template.module = $scope.template.templateModule.name;
                        template.name = $scope.template.templateName;

                        ExcelTemplatesService.update(template)
                            .then(function () {
                                success();
                            })
                            .catch(function () {
                                $scope.saving = false;
                            });
                    }
                }
            };

            $scope.clearTemplateFile = function () {

                if ($scope.fileUpload.uploader.files[0])
                    $scope.fileUpload.uploader.removeFile($scope.fileUpload.uploader.files[0]);

                if ($scope.template && $scope.template.content)
                    $scope.template.content = undefined;

                $scope.templateFileCleared = true;
            };

            var setCurrentTemplate = function (template) {
                /**template.name
                 * wordTemplates.html'deki değişken adıyla aynı olduğu için modal açıldığında wordTemplatesForm.html'de ki alan değişikliğinde wordTemplates.html'deki alan etkileniyor*/
                $scope.templateFileCleared = false;
                $scope.template = template;
                $scope.template.templateName = template.name;
                $scope.template.templateModule = template.module;
                $scope.currentContent = angular.copy(template.content);
            };

            var success = function () {
                $scope.saving = false;
                $scope.addNewExcelTemplateFormModal.hide();
                $scope.changePage(1);
                toastr.success($filter('translate')('Setup.Templates.SaveSuccess'));
                $scope.addNewWordTemplateFormModal.hide();
            };

            $scope.getDownloadUrlExcel = function (module) {
                module = module.name;
                $window.open("/attach/export_excel?module=" + module + "&appId=" + $scope.appId + "&organizationId=" + $rootScope.currentOrgId + '&locale=' + $scope.$parent.$parent.language, "_blank");
            };

            $scope.getDownloadUrl = function (template) {
                return '/attach/download_template?fileId=' + template.id + "&tempType=" + template.template_type + "&appId=" + $scope.appId + "&organizationId=" + $rootScope.currentOrgId;
            };

            $scope.delete = function (id) {
                var willDelete =
                    swal({
                        title: "Are you sure?",
                        text: " ",
                        icon: "warning",
                        buttons: ['Cancel', 'Yes'],
                        dangerMode: true
                    }).then(function (value) {
                        if (value) {
                            ExcelTemplatesService.delete(id).then(function () {
                                $scope.changePage(1);
                                $scope.pageTotal = $scope.pageTotal - 1;
                                toastr.success($filter('translate')('Setup.Templates.DeleteSuccess' | translate));
                            }).catch(function () {
                                $scope.templates = $scope.templatesState;

                                if ($scope.addNewExcelTemplateFormModal) {
                                    $scope.addNewExcelTemplateFormModal.hide();
                                    $scope.saving = false;
                                }
                            });
                        }
                    });
            };

            var uploadThenComplete = function (fileItem, tempInfo, status) {

                if (status === 200) {
                    var template = {
                        name: $scope.template.templateName,
                        module: $scope.template.templateModule.name,
                        template_type: 'excel',
                        content: tempInfo.unique_name,
                        content_type: tempInfo.content_type,
                        chunks: tempInfo.chunks,
                        subject: "Excel",
                        active: $scope.template.active,
                        permissions: $scope.template.permissions
                    };

                    if (!$scope.template.id) {
                        ExcelTemplatesService.create(template)
                            .then(function () {
                                success();
                            })
                            .catch(function () {
                                $scope.saving = false;
                            });
                    }
                    else {
                        template.id = $scope.template.id;

                        ExcelTemplatesService.update(template)
                            .then(function () {
                                success();
                            })
                            .catch(function () {
                                $scope.saving = false;
                            });
                    }

                }
            };

        }
    ]);