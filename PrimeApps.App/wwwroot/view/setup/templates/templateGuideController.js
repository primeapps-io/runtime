'use strict';

angular.module('primeapps')

    .controller('TemplateGuideController', ['$rootScope', '$scope', '$filter', 'ngToast', 'ModuleService', '$window',
        function ($rootScope, $scope, $filter, ngToast, ModuleService, $window) {
            $scope.templateModules = [];
            $scope.$parent.collapsed = true;

            angular.forEach($rootScope.modules, function (module) {
                if (module.order != 0)
                    $scope.templateModules.push(angular.copy(module));
            });

            var getLookupModules = function (module) {
                if (!module) return;

                var lookupModules = [];

                for (var i = 0; i < module.fields.length; i++) {
                    if (module.fields[i].data_type == 'lookup') {
                        for (var j = 0; j < $rootScope.modules.length; j++) {
                            if (module.fields[i].lookup_type == $rootScope.modules[j].name) {
                                var lookupModule = angular.copy($rootScope.modules[j]);
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


            $scope.getDownloadUrlExcel = function () {
                var moduleName = $scope.selectedModuleExcel.name;
                $window.open("/attach/export_excel?module=" + moduleName + '&locale=' + $rootScope.locale, "_blank");
                ngToast.create({ content: $filter('translate')('Module.ExcelDesktop'), className: 'success' });

            };

            $scope.moduleChanged = function () {
                $scope.lookupModules = getLookupModules($scope.selectedModule);
                $scope.getModuleRelations($scope.selectedModule);
                $scope.selectedSubModule = null;
            };

            $scope.getModuleRelations = function (module) {
                if (!module)
                    return;

                module.relatedModules = [];

                angular.forEach($scope.selectedModule.relations, function (relation) {
                    var relatedModule = $filter('filter')($rootScope.modules, { name: relation.related_module }, true)[0];

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

            $scope.filterUsers = function (field) {
                return field.data_type != 'users';
            };

            $scope.getRelatedFieldName = function (field, module) {
                return module.parent_field.name + '.' + (field.multiline_type_use_html ? 'html__' : field.data_type == 'image' ? 'img__' : '') + field.name;
            };
        }
    ]);