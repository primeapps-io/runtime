'use strict';

angular.module('primeapps')

    .controller('WordTemplatesController', ['$rootScope', '$scope', '$state', '$filter', 'WordTemplatesService', '$http', 'config', '$modal', '$cookies', 'ModuleService',
        function ($rootScope, $scope, $state, $filter, WordTemplatesService, $http, config, $modal, $cookies, ModuleService) {

            $scope.$parent.menuTopTitle = "Templates";
           // $scope.$parent.activeMenu = 'templates';
            $scope.$parent.activeMenuItem = 'templatesWord';

            $rootScope.breadcrumblist[2].title = 'Word Templates';

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

            //3 templateType Module
            WordTemplatesService.count(3).then(function (response) {
                $scope.pageTotal = response.data;
            });

            //3 templateType Module
            WordTemplatesService.find($scope.requestModel, 3).then(function (response) {
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

                WordTemplatesService.find(requestModel, 3).then(function (response) {

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
                   // $scope.getDownloadUrl(template);
                }
                else {
                    $scope.template = [];
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

            $scope.fileUpload = {
                settings: {
                    multi_selection: false,
                    unique_names: false,
                    url: 'storage/upload_template',
                    headers: {
                        'Authorization': 'Bearer ' + window.localStorage.getItem('access_token'),//$localStorage.get('access_token'),
                        'Accept': 'application/json',
                        'X-Organization-Id': $rootScope.currentOrgId,
                        'X-App-Id': $scope.appId
                    },
                    filters: {
                        mime_types: [
                            { title: "Email Attachments", extensions: "pdf,doc,docx,xls,xlsx,csv" },
                        ],
                        max_file_size: "50mb"
                    }
                },
                events: {
                    fileUploaded: function (uploader, file, response) {
                        var resp = JSON.parse(response.response);
                        var template = {
                            name: $scope.template.templateName,
                            module: $scope.template.templateModule.name,
                            template_type: 'module',
                            content: resp.unique_name,
                            content_type: resp.content_type,
                            chunks: resp.chunks,
                            subject: "Word",
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
            }
            ;

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

            $scope.showTemplateGuideModal = function () {

                $scope.wordTemplateGuideModal = $scope.wordTemplateGuideModal || $modal({
                        scope: $scope,
                        templateUrl: 'view/app/templates/wordtemplates/wordTemplateGuide.html',
                        animation: 'am-fade-and-slide-right',
                        backdrop: 'static',
                        show: false//,
                        //controller: function () {
                        //    ModuleService.getModules().then(function (response) {
                        //        $scope.modules = response.data;
                        //    });
                        //}
                    });

                $scope.wordTemplateGuideModal.$promise.then(function () {
                    $scope.wordTemplateGuideModal.show();
                });
            };

            $scope.getDownloadUrl = function (template) {
                return '/attach/download_template?fileId=' + template.id + "&tempType=" + template.template_type + "&appId=" + $scope.appId + "&organizationId=" + $rootScope.currentOrgId ;
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
                $scope.template.templateModule = template.module;
                $scope.currentContent = angular.copy(template.content);
            };

            var success = function () {
                $scope.saving = false;
                //  $state.go('studio.app.templatesWord');
                swal($filter('translate')('Setup.Templates.SaveSuccess'), "", "success");
                $scope.addNewWordTemplateFormModal.hide();
            };

            //for GuideTemplate
            var getLookupModules = function (module) {

                if (!module) return;

                var lookupModules = [];
                for (var i = 0; i < module.fields.length; i++) {
                    if (module.fields[i].data_type == 'lookup') {
                        if (module.name === 'quote_products' && module.fields[i].lookup_type === 'quotes')
                            continue;

                        if (module.name === 'order_products' && module.fields[i].lookup_type === 'sales_order')
                            continue;

                        for (var j = 0; j < $scope.modules.length; j++) {
                            if (module.fields[i].lookup_type == $scope.modules[j].name) {
                                var lookupModule = angular.copy($scope.modules[j]);
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
                $scope.loading = true;
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
                    var quoteProductsModule = $filter('filter')($scope.modules, { name: 'quote_products' }, true)[0];
                    ModuleService.getModuleByName(quoteProductsModule.name).then(function (response) {
                        quoteProductsModule = response.data;
                        getLookupModules(quoteProductsModule);
                        module.relatedModules.push(quoteProductsModule);
                    }).finally(function () {
                        $scope.loading = false;
                    });
                }

                if (module.name === 'sales_orders') {
                    var orderProductsModule = $filter('filter')($scope.modules, { name: 'order_products' }, true)[0];
                    ModuleService.getModuleByName(orderProductsModule.name).then(function (response) {
                        orderProductsModule = response.data;
                        getLookupModules(orderProductsModule);
                        module.relatedModules.push(orderProductsModule);
                    }).finally(function () {
                        $scope.loading = false;
                    });
                }

                angular.forEach(module.relations, function (relation) {
                    var relatedModule = $filter('filter')($scope.modules, { name: relation.related_module }, true)[0];
                    ModuleService.getModuleByName(relatedModule.name).then(function (response) {
                        relatedModule = response.data;
                        if (relation.deleted || !relatedModule || relatedModule.order === 0)
                            return;

                        relatedModule = angular.copy(relatedModule);

                        if (relation.relation_type === 'many_to_many') {
                            angular.forEach(relatedModule.fields, function (field) {
                                field.name = relation.related_module + '_id.' + field.name;
                            });
                        }
                        else {
                            getLookupModules(relatedModule);
                        }

                        module.relatedModules.push(relatedModule);
                    }).finally(function () {
                        $scope.loading = false;
                    });
                });

                $scope.loading = false;
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

            $scope.delete = function (id) {
                var willDelete =
                    swal({
                        title: "Are you sure?",
                        text: "Are you sure that you want to delete this word template?",
                        icon: "warning",
                        buttons: ['Cancel', 'Yes'],
                        dangerMode: true
                    }).then(function (value) {
                        if (value) {
                            WordTemplatesService.delete(id).then(function () {
                                $scope.changePage(1);
                                swal($filter('translate')('Setup.Templates.DeleteSuccess' | translate), "", "success");
                            }).catch(function () {
                                $scope.templates = $scope.templatesState;
                                if ($scope.addNewWordTemplateFormModal) {
                                    $scope.addNewWordTemplateFormModal.hide();
                                    $scope.saving = false;
                                }
                            });
                        }
                    });
            };
        }
    ]);