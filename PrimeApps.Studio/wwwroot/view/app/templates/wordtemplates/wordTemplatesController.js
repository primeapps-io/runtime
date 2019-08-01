'use strict';

angular.module('primeapps')

    .controller('WordTemplatesController', ['$rootScope', '$scope', '$state', '$filter', 'WordTemplatesService', '$http', 'config', '$modal', '$cookies', 'ModuleService', 'FileUploader', 'helper',
        function ($rootScope, $scope, $state, $filter, WordTemplatesService, $http, config, $modal, $cookies, ModuleService, FileUploader, helper) {

            //$scope.$parent.menuTopTitle = "Templates";
            // $scope.$parent.activeMenu = 'templates';
            $scope.$parent.activeMenuItem = 'templatesWord';
            WordTemplatesService.getAllModule()
                .then(function (response) {
                    $scope.customModules = response.data;
                });

            $rootScope.breadcrumblist[2].title = 'Document';

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

            $scope.activePage = 1;
            WordTemplatesService.count("module").then(function (response) {
                $scope.pageTotal = response.data;
                $scope.changePage(1);
            });

            $scope.changePage = function (page) {
                $scope.loading = true;

                if (page !== 1) {
                    var difference = Math.ceil($scope.pageTotal / $scope.requestModel.limit);

                    if (page > difference) {
                        if (Math.abs(page - difference) < 1)
                            --page;
                        else
                            page = page - Math.abs(page - Math.ceil($scope.pageTotal / $scope.requestModel.limit))
                    }
                }

                $scope.activePage = page;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;

                WordTemplatesService.find(requestModel, "module").then(function (response) {

                    var templates = response.data;
                    angular.forEach(templates, function (template) {
                        template.module = $filter('filter')($rootScope.customModules, { name: template.module }, true)[0];
                    });
                    $scope.templates = templates;
                    $scope.templatesState = templates;

                }).finally(function () {
                    $scope.loading = false;
                });
            };

            $scope.changeOffset = function () {
                $scope.changePage($scope.activePage);
            };

            $scope.showFormModal = function (template) {
                $scope.requiredColor = "";
                $scope.template = [];

                if ($scope.fileUpload)
                    $scope.fileUpload.queue = [];

                if (template) {
                    // fileUpload.queue[0] = []; //{ _file: { name: '' } };
                    setCurrentTemplate(template);
                    // $scope.getDownloadUrl(template);
                } else {// if template, isNew we set the first value active
                    $scope.template.active = true;
                }

                $scope.addNewWordTemplateFormModal = $scope.addNewWordTemplateFormModal || $modal({
                        scope: $scope,
                        templateUrl: 'view/app/templates/wordtemplates/wordTemplatesForm.html',
                        animation: 'am-fade-and-slide-right',
                        backdrop: 'static',
                        show: false
                    });

                $scope.addNewWordTemplateFormModal.$promise.then(function () {
                    $scope.addNewWordTemplateFormModal.show();
                });
            };

            // $scope.fileUpload = {
            //     settings: {
            //         multi_selection: false,
            //         unique_names: false,
            //         url: 'storage/upload_template',
            //         chunk_size: '256kb',
            //         queueLimit: 1,
            //         headers: {
            //             'Authorization': 'Bearer ' + window.localStorage.getItem('access_token'),//$localStorage.get('access_token'),
            //             'Accept': 'application/json',
            //             'X-Organization-Id': $rootScope.currentOrgId,
            //             'X-App-Id': $scope.appId
            //         },
            //         filters: {
            //             mime_types: [
            //                 { title: "Template Files", extensions: "doc,docx" },
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
            //                 template_type: 'module',
            //                 content: resp.unique_name,
            //                 content_type: resp.content_type,
            //                 chunks: resp.chunks,
            //                 subject: "Word",
            //                 active: $scope.template.active
            //             };
            //
            //             if (!$scope.template.id) {
            //                 WordTemplatesService.create(template)
            //                     .then(function () {
            //                         success();
            //                         $scope.changePage(1);
            //                         $scope.pageTotal = $scope.pageTotal + 1;
            //                     })
            //                     .catch(function () {
            //                         $scope.saving = false;
            //                     });
            //             }
            //             else {
            //                 template.id = $scope.template.id;
            //
            //                 WordTemplatesService.update(template)
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
                    case 'docFilter':
                        toastr.warning($filter('translate')('Setup.Settings.DocumentTypeError'));
                        break;
                    case 'sizeFilter':
                        toastr.warning($filter('translate')('Setup.Settings.SizeError'));
                        break;
                }
            };

            fileUpload.filters.push({
                name: 'docFilter',
                fn: function (item, options) {
                    var extension = helper.getFileExtension(item.name);
                    return true ? (extension === 'docx' || extension === 'doc') : false;
                }
            });

            fileUpload.filters.push({
                name: 'sizeFilter',
                fn: function (item) {
                    return item.size < 10485760;//10 mb
                }
            });

            fileUpload.onAfterAddingFile = function (fileItem) {
                $scope.template.content = fileItem._file.name;
                $scope.requiredColor = undefined;
            };

            $scope.remove = function () {
                if (fileUpload.queue[0]) {
                    fileUpload.queue[0].remove();

                }
                $scope.template.content = undefined;
                $scope.templateFileCleared = true;

            };

            $scope.save = function (uploadForm) {

                if (uploadForm.$invalid || !$scope.template.content) {
                    if (!$scope.template.content)
                        $scope.requiredColor = 'background-color:rgba(206, 4, 4, 0.15) !important;';
                    // else
                    toastr.error($filter('translate')('Module.RequiredError'));

                    return;
                }

                if ($scope.fileUpload.queue.length > 0 && $scope.fileUpload.queue[0].file.size <= 0) {
                    toastr.warning("File cannot be empty!");
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
                    fileUpload.queue[0].uploader.headers = header;
                    fileUpload.queue[0].headers = header;
                    fileUpload.queue[0].upload();
                    fileUpload.onCompleteItem = function (fileItem, tempInfo, status) {
                        uploadThenComplete(fileItem, tempInfo, status);
                    };
                    //$scope.fileUpload.uploader.start();
                } else {
                    if ($scope.templateFileCleared) {
                        fileUpload.queue[0].uploader.headers = header;
                        fileUpload.queue[0].headers = header;
                        fileUpload.queue[0].upload();
                        fileUpload.onCompleteItem = function (fileItem, tempInfo, status) {
                            uploadThenComplete(fileItem, tempInfo, status);
                        };
                        //$scope.fileUpload.uploader.start();
                    } else {
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

            $scope.showTemplateGuideModal = function () {
                $scope.tempalteFieldName = "/" + $filter('translate')('Setup.Templates.TemplateFieldName');
                $scope.selectedModule = null;
                $scope.wordTemplateGuideModal = $scope.wordTemplateGuideModal || $modal({
                        scope: $scope,
                        templateUrl: 'view/app/templates/wordtemplates/wordTemplateGuide.html',
                        animation: 'am-fade-and-slide-right',
                        backdrop: 'static',
                        show: false
                    });

                $scope.wordTemplateGuideModal.$promise.then(function () {
                    $scope.wordTemplateGuideModal.show();
                });
            };

            $scope.getDownloadUrl = function (template) {
                return '/attach/download_template?fileId=' + template.id + "&tempType=" + template.template_type + "&appId=" + $scope.appId + "&organizationId=" + $rootScope.currentOrgId;
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
                $scope.template = angular.copy(template);
                $scope.template.templateName = template.name;
                $scope.template.active = template.active;
                $scope.template.templateModule = template.module;
                $scope.currentContent = angular.copy(template.content);
            };

            var success = function (create) {
                $scope.saving = false;
                toastr.success($filter('translate')('Setup.Templates.SaveSuccess'));
                $scope.addNewWordTemplateFormModal.hide();
                $scope.changePage($scope.activePage);
                $scope.pageTotal = create ? $scope.pageTotal++ : $scope.pageTotal;
            };

            //for GuideTemplate
            var getLookupModules = function (module) {

                if (!module) return;

                var lookupModules = [];
                for (var i = 0; i < module.fields.length; i++) {
                    if (module.fields[i].data_type === 'lookup') {
                        if (module.name === 'quote_products' && module.fields[i].lookup_type === 'quotes')
                            continue;

                        if (module.name === 'order_products' && module.fields[i].lookup_type === 'sales_order')
                            continue;

                        for (var j = 0; j < $scope.customModules.length; j++) {
                            if (module.fields[i].lookup_type === $scope.customModules[j].name) {
                                var lookupModule = angular.copy($scope.customModules[j]);
                                lookupModule.parent_field = module.fields[i];
                                lookupModules.push(lookupModule);
                                break;
                            }
                        }
                    }
                }

                if (lookupModules.length)
                    module.lookupModules = lookupModules;
            };

            var addNoteModuleRelation = function (module) {
                var noteModule = {};
                noteModule.type = 'custom';
                noteModule.name = 'notes';
                noteModule.label_tr_singular = 'Not';
                noteModule.label_tr_plural = 'Notlar';
                noteModule.label_en_singular = 'Note';
                noteModule.label_en_plural = 'Notes';
                noteModule.order = 9999;
                noteModule.fields = [];
                noteModule.fields.push({ id: 1, name: 'text', label_tr: 'Not', label_en: 'Note' });
                noteModule.fields.push({
                    id: 2,
                    name: 'first_name',
                    label_tr: 'Oluşturan - Adı',
                    label_en: 'First Name'
                });
                noteModule.fields.push({
                    id: 3,
                    name: 'last_name',
                    label_tr: 'Oluşturan - Soyadı',
                    label_en: 'Last Name'
                });
                noteModule.fields.push({
                    id: 4,
                    name: 'full_name',
                    label_tr: 'Oluşturan - Adı Soyadı',
                    label_en: 'Full Name'
                });
                noteModule.fields.push({ id: 5, name: 'email', label_tr: 'Oluşturan - Eposta', label_en: 'Email' });
                noteModule.fields.push({
                    id: 6,
                    name: 'created_at',
                    label_tr: 'Oluşturulma Tarihi',
                    label_en: 'Created at'
                });

                module.relatedModules.push(noteModule);
            };

            $scope.moduleChanged = function (selectedModule) {
                $scope.guideLoading = true;
                ModuleService.getModuleByName(selectedModule.name).then(function (response) {
                    $scope.selectedModule = response.data;
                    $scope.lookupModules = getLookupModules($scope.selectedModule);
                    $scope.getModuleRelations($scope.selectedModule);
                    $scope.selectedSubModule = null;
                });
            };

            $scope.getModuleRelations = function (module) {
                if (!module)
                    return;

                module.relatedModules = [];

                if (module.name === 'quotes') {
                    var quoteProductsModule = $filter('filter')($rootScope.customModules, { name: 'quote_products' }, true)[0];
                    ModuleService.getModuleByName(quoteProductsModule.name).then(function (response) {
                        quoteProductsModule = response.data;
                        getLookupModules(quoteProductsModule);
                        module.relatedModules.push(quoteProductsModule);
                    }).finally(function () {
                        $scope.guideLoading = false;
                    });
                }

                if (module.name === 'sales_orders') {
                    var orderProductsModule = $filter('filter')($rootScope.customModules, { name: 'order_products' }, true)[0];
                    ModuleService.getModuleByName(orderProductsModule.name).then(function (response) {
                        orderProductsModule = response.data;
                        getLookupModules(orderProductsModule);
                        module.relatedModules.push(orderProductsModule);
                    }).finally(function () {
                        $scope.guideLoading = false;
                    });
                }

                angular.forEach(module.relations, function (relation) {
                    var relatedModule = $filter('filter')($rootScope.customModules, { name: relation.related_module }, true)[0];
                    ModuleService.getModuleByName(relatedModule.name).then(function (response) {
                        relatedModule = response.data;
                        if (relation.deleted || !relatedModule || relatedModule.order === 0)
                            return;

                        relatedModule = angular.copy(relatedModule);

                        if (relation.relation_type === 'many_to_many') {
                            angular.forEach(relatedModule.fields, function (field) {
                                field.name = relation.related_module + '_id.' + field.name;
                            });
                        } else {
                            getLookupModules(relatedModule);
                        }

                        module.relatedModules.push(relatedModule);
                    }).finally(function () {
                        $scope.guideLoading = false;
                    });
                });

                $scope.guideLoading = false;
                addNoteModuleRelation(module);
            };

            $scope.filterReferences = function (field) {
                if ($scope.selectedSubModule && ($scope.selectedSubModule.name === 'quote_products' || $scope.selectedModule.name === 'order_products') && field.name === 'usage_unit')
                    return;

                return field.data_type != 'relation' && field.data_type != 'lookup';
            };

            $scope.filterUsers = function (field) {
                return field.data_type != 'users';
            };

            $scope.getRelatedFieldName = function (field, module) {
                return module.parent_field.name + '.' + (field.multiline_type_use_html ? 'html__' : '') + field.name;
            };

            $scope.delete = function (id, event) {
                var willDelete =
                    swal({
                        title: "Are you sure?",
                        text: " ",
                        icon: "warning",
                        buttons: ['Cancel', 'Yes'],
                        dangerMode: true
                    }).then(function (value) {
                        if (value) {

                            var elem = angular.element(event.srcElement);
                            angular.element(elem.closest('tr')).addClass('animated-background');

                            WordTemplatesService.delete(id).then(function () {
                                angular.element(document.getElementsByClassName('ng-scope animated-background')).remove();
                                $scope.changePage($scope.activePage);
                                $scope.pageTotal--;
                                toastr.success($filter('translate')('Setup.Templates.DeleteSuccess' | translate));
                            }).catch(function () {
                                angular.element(document.getElementsByClassName('ng-scope animated-background')).removeClass('animated-background');
                                $scope.templates = $scope.templatesState;
                                if ($scope.addNewWordTemplateFormModal) {
                                    $scope.addNewWordTemplateFormModal.hide();
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
                        template_type: 'module',
                        content: tempInfo.unique_name,
                        content_type: tempInfo.content_type,
                        chunks: tempInfo.chunks,
                        subject: "Word",
                        active: $scope.template.active
                    };
                    if (!$scope.template.id) {
                        WordTemplatesService.create(template)
                            .then(function () {
                                success(true);
                            })
                            .catch(function () {
                                $scope.saving = false;
                            });
                    } else {
                        template.id = $scope.template.id;

                        WordTemplatesService.update(template)
                            .then(function () {
                                success();
                            })
                            .catch(function () {
                                $scope.saving = false;
                            });
                    }
                } else {
                    toastr.error($filter('translate')('Common.Error'));
                    $scope.saving = false;
                }
            };
        }
    ]);