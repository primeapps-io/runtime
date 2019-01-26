'use strict';

angular.module('primeapps')

    .controller('ExcelTemplatesController', ['$rootScope', '$scope', '$state', '$stateParams', '$location', 'ngToast', '$filter', '$cache', '$q', 'helper', 'dragularService', 'operators', 'ExcelTemplatesService', '$http', 'config', '$modal', '$cookies', '$window',
        function ($rootScope, $scope, $state, $stateParams, $location, ngToast, $filter, $cache, $q, helper, dragularService, operators, ExcelTemplatesService, $http, config, $modal, $cookies, $window) {

            $scope.$parent.menuTopTitle = "Templates";
            $scope.$parent.activeMenu = 'templates';
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

            //4 templateType Module
            ExcelTemplatesService.count(4).then(function (response) {
                $scope.pageTotal = response.data;
            });

            //4 templateType Module
            ExcelTemplatesService.find($scope.requestModel, 4).then(function (response) {
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

                ExcelTemplatesService.find(requestModel, 3).then(function (response) {

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
                $scope.changePage(1)
            };

            $scope.showFormModal = function (template) {

                if (template) {
                    setCurrentTemplate(template);
					$scope.getDownloadUrlExcel();
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

            $scope.fileUpload = {
                settings: {
                    runtimes: 'html5',
                    url: config.apiUrl + 'Document/Upload_Excel',
                    chunk_size: '256kb',
                    multipart: true,
                    unique_names: true,
                    headers: {
                        'Authorization': 'Bearer ' + window.localStorage.getItem('access_token'),//$localStorage.get('access_token'),
                        'Accept': 'application/json',
                        'X-Organization-Id': $rootScope.currentOrgId,
                        'X-App-Id': $rootScope.currentAppId
                    },
                    filters: {
                        mime_types: [
                            { title: 'Template Files', extensions: 'xls,xlsx' }
                        ],
                        max_file_size: '10mb'
                    }
                },
                events: {
                    fileUploaded: function (uploader, file, response) {
                        var resp = JSON.parse(response.response);
                        var template = {
                            name: $scope.template.name,
                            module: $scope.template.module.name,
                            template_type: 'excel',
                            content: resp.UniqueName,
                            content_type: resp.ContentType,
                            chunks: resp.Chunks,
                            subject: "Excel",
                            active: $scope.template.active
                        };

                        if (!$scope.template.id) {
                            WordTemplatesService.create(template)
                                .then(function () {
                                    success();
                                })
                                .catch(function () {
                                    $scope.saving = false;
                                });
                        }
                        else {
                            template.id = $scope.template.id;

                            WordTemplatesService.update(template)
                                .then(function () {
                                    success();
                                })
                                .catch(function () {
                                    $scope.saving = false;
                                });
                        }
                    }
                }
            };

            $scope.save = function (uploadForm) {

                if (!uploadForm.$valid)
                    return;

                $scope.saving = true;

                if (!$scope.template.id) {
                    $scope.fileUpload.uploader.start();
                }
                else {
                    if ($scope.templateFileCleared) {
                        $scope.fileUpload.uploader.start();
                    }
                    else {
                        var template = angular.copy($scope.template);
                        template.module = $scope.template.templateModule.name;
                        template.name = $scope.template.templateName;

                        WordTemplatesService.update(template)
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
                $state.go('studio.app.templatesWord');
                swal($filter('translate')('Setup.Templates.SaveSuccess'), "", "success");
                $scope.addNewWordTemplateFormModal.hide();
            };

            $scope.getDownloadUrlExcel = function (selectedModuleExcel) {
                if (selectedModuleExcel) {
                    var moduleName = selectedModuleExcel.name;
                    $window.open("/attach/export_excel?module=" + moduleName + '&locale=' + $scope.language, "_blank");
                    swal($filter('translate')('Module.ExcelDesktop'), "", "success");
                    $scope.excelTemplateGuideModal.hide();
                }
            };

            $scope.delete = function (id) {
                const willDelete =
                    swal({
                        title: "Are you sure?",
                        text: "Are you sure that you want to delete this excel template ?",
                        icon: "warning",
                        buttons: ['Cancel', 'Okey'],
                        dangerMode: true
                    }).then(function (value) {
                        if (value) {
                            ExcelTemplatesService.delete(id).then(function () {
                                $scope.changePage(1);
                                swal($filter('translate')('Setup.Templates.DeleteSuccess' | translate), "", "success");
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

        }
    ]);