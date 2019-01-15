'use strict';

angular.module('primeapps')

    .controller('WordTemplatesController', ['$rootScope', '$scope', '$state', '$filter', 'WordTemplatesService', '$http', 'config', '$modal', '$cookies', 'ModuleService',
        function ($rootScope, $scope, $state, $filter, WordTemplatesService, $http, config, $modal, $cookies, ModuleService) {

            $scope.$parent.menuTopTitle = "Templates";
            $scope.$parent.activeMenu = 'templates';
            $scope.$parent.activeMenuItem = 'templatesWord';
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
                    template.module = $filter('filter')($scope.$parent.modules, { name: template.module }, true)[0];
                });
                $scope.templates = templates;

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
                        template.module = $filter('filter')($scope.$parent.modules, { name: template.module }, true)[0];
                    });
                    $scope.templates = templates;

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
                    $scope.getDownloadUrl();
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
                    runtimes: 'html5',
                    url: config.apiUrl + 'Document/Upload',
                    chunk_size: '256kb',
                    multipart: true,
                    unique_names: true,
                    headers: {
                        'Authorization': 'Bearer ' + window.localStorage.getItem('access_token'),//$localStorage.get('access_token'),
                        'Accept': 'application/json',
                        'X-Organization-Id': $cookies.get('organization_id'),
                        'X-App-Id': $cookies.get('app_id')
                    },
                    filters: {
                        mime_types: [
                            { title: 'Template Files', extensions: 'doc,docx' }
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
                            template_type: 'module',
                            content: resp.UniqueName,
                            content_type: resp.ContentType,
                            chunks: resp.Chunks,
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
            } ;

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
                    show: false,
                    controller: function () {
                        ModuleService.getModules().then(function (response) {
                            $scope.modules = response.data;
                        });
                    }
                });

                $scope.wordTemplateGuideModal.$promise.then(function () {
                    $scope.wordTemplateGuideModal.show();
                });
            };

            $scope.getDownloadUrl = function (template) {

                if (template) {
                    $scope.templateDownloadUrl = config.apiUrl + 'Document/download_template?templateId=' + template.id + '&access_token=' + window.localStorage.getItem('access_token'); //$localStorage.get('access_token');
                    return $scope.templateDownloadUrl;
                }
                else {
                    $scope.templateDownloadUrl = config.apiUrl + 'Document/download_template?templateId=' + $scope.template.id + '&access_token=' + window.localStorage.getItem('access_token'); //$localStorage.get('access_token');
                    return $scope.templateDownloadUrl;
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
                noteModule.fields.push({ id: 2, name: 'first_name', label_tr: 'Oluşturan - Adı', label_en: 'First Name' });
                noteModule.fields.push({ id: 3, name: 'last_name', label_tr: 'Oluşturan - Soyadı', label_en: 'Last Name' });
                noteModule.fields.push({ id: 4, name: 'full_name', label_tr: 'Oluşturan - Adı Soyadı', label_en: 'Full Name' });
                noteModule.fields.push({ id: 5, name: 'email', label_tr: 'Oluşturan - Eposta', label_en: 'Email' });
                noteModule.fields.push({ id: 6, name: 'created_at', label_tr: 'Oluşturulma Tarihi', label_en: 'Created at' });

                module.relatedModules.push(noteModule);
            };

            $scope.moduleChanged = function (selectedModule) {
                $scope.lookupModules = getLookupModules(selectedModule);
                $scope.getModuleRelations(selectedModule);
                $scope.selectedSubModule = null;
            };

            $scope.getModuleRelations = function (module) {
                if (!module)
                    return;

                module.relatedModules = [];

                if (module.name === 'quotes') {
                    var quoteProductsModule = $filter('filter')($scope.modules, { name: 'quote_products' }, true)[0];
                    getLookupModules(quoteProductsModule);
                    module.relatedModules.push(quoteProductsModule);
                }

                if (module.name === 'sales_orders') {
                    var orderProductsModule = $filter('filter')($scope.modules, { name: 'order_products' }, true)[0];
                    getLookupModules(orderProductsModule);
                    module.relatedModules.push(orderProductsModule);
                }

                angular.forEach(module.relations, function (relation) {
                    var relatedModule = $filter('filter')($scope.modules, { name: relation.related_module }, true)[0];

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
                });

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

        }
    ]);