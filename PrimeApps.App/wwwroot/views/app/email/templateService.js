'use strict';

angular.module('ofisim')

    .factory('TemplateService', ['$rootScope', '$http', 'config', '$filter', 'ModuleService',
        function ($rootScope, $http, config, $filter, ModuleService) {
            return {
                getAll: function (type, module) {
                    return $http.get(config.apiUrl + 'template/get_all?type=' + type + '&moduleName=' + (module || ''));
                },

                create: function (template) {
                    return $http.post(config.apiUrl + 'template/create', template);
                },

                update: function (template) {
                    return $http.put(config.apiUrl + 'template/update/' + template.id, template);
                },

                delete: function (id) {
                    return $http.delete(config.apiUrl + 'template/delete/' + id);
                },
                getFields: function (module) {
                    var moduleFields = angular.copy(module.fields);
                    var fields = [];
                    moduleFields = $filter('filter')(moduleFields, { display_list: true, lookup_type: '!relation' }, true);

                    var seperatorFieldMain = {};
                    seperatorFieldMain.name = 'seperator-main';
                    seperatorFieldMain.label = $rootScope.language === 'tr' ? module.label_tr_singular : module.label_en_singular;
                    seperatorFieldMain.order = 0;
                    seperatorFieldMain.seperator = true;
                    moduleFields.push(seperatorFieldMain);
                    var seperatorLookupOrder = 0;

                    angular.forEach(moduleFields, function (field) {
                        if (field.data_type === 'lookup' && field.lookup_type != 'relation') {
                            var lookupModule = angular.copy($filter('filter')($rootScope.modules, { name: field.lookup_type }, true)[0]);
                            seperatorLookupOrder += 100;
                            if (lookupModule === null || lookupModule === undefined) return;
                            var seperatorFieldLookup = {};
                            seperatorFieldLookup.name = 'seperator-' + lookupModule.name;
                            seperatorFieldLookup.order = seperatorLookupOrder;
                            seperatorFieldLookup.seperator = true;

                            if ($rootScope.language === 'tr')
                                seperatorFieldLookup.label = lookupModule.label_tr_singular + ' (' + field.label_tr + ')';
                            else
                                seperatorFieldLookup.label = lookupModule.label_en_singular + ' (' + field.label_en + ')';

                            moduleFields.push(seperatorFieldLookup);

                            var lookupModuleFields = angular.copy(lookupModule.fields);
                            lookupModuleFields = $filter('filter')(lookupModuleFields, { display_list: true }, true);

                            angular.forEach(lookupModuleFields, function (fieldLookup) {
                                if (fieldLookup.data_type === 'lookup')
                                    return;

                                fieldLookup.label = $rootScope.language === 'tr' ? fieldLookup.label_tr : fieldLookup.label_en;
                                fieldLookup.labelExt = '(' + field.label + ')';
                                fieldLookup.name = field.name + '.' + fieldLookup.name;
                                fieldLookup.order = parseInt(fieldLookup.order) + seperatorLookupOrder;
                                fieldLookup.parent_type = field.lookup_type;
                                moduleFields.push(fieldLookup);
                            });
                        }
                    });

                    angular.forEach(moduleFields, function (field) {
                        if (field.deleted || !ModuleService.hasFieldDisplayPermission(field))
                            return;

                        if (field.name && field.data_type != 'lookup') {
                            var newField = {};
                            newField.name = field.name;
                            newField.label = field.label;
                            newField.labelExt = field.labelExt;
                            newField.order = field.order;
                            newField.lookup_type = field.lookup_type;
                            newField.seperator = field.seperator;
                            newField.multiline_type = field.multiline_type;
                            newField.data_type = field.data_type;
                            newField.parent_type = field.parent_type;
                            fields.push(newField);
                        }

                    });

                    fields = $filter('orderBy')(fields, 'order');

                    return fields;
                }
            };
        }]);

