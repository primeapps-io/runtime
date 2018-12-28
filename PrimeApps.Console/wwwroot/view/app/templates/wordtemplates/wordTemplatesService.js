angular.module('primeapps')

    .factory('WordTemplatesService', ['$rootScope', '$http', 'config', '$q', '$filter',
        function ($rootScope, $http, config, $q, $filter) {
            return {

                create: function (view) {
                    return $http.post(config.apiUrl + 'view/create', view);
                },

                update: function (view, id) {
                    return $http.put(config.apiUrl + 'view/update/' + id, view);
                },

                getFields: function (module, view) {
                    var moduleFields = angular.copy(module.fields);
                    var fields = {};
                    fields.selectedFields = [];
                    fields.availableFields = [];
                    fields.allFields = [];
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
                            if (lookupModule === null || lookupModule === undefined)return;
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
                                fieldLookup.name = field.name + '.' + lookupModule.name + '.' + fieldLookup.name;
                                fieldLookup.order = parseInt(fieldLookup.order) + seperatorLookupOrder;
                                moduleFields.push(fieldLookup);
                            });
                        }
                    });

                    angular.forEach(moduleFields, function (field) {
                        if (field.deleted || !ModuleService.hasFieldDisplayPermission(field) && field.multiline_type != 'large')
                            return;

                        var selectedField = null;

                        if (view.fields)
                            selectedField = $filter('filter')(view.fields, { field: field.name }, true)[0];

                        var newField = {};
                        newField.name = field.name;
                        newField.label = field.label;
                        newField.labelExt = field.labelExt;
                        newField.order = field.order;
                        newField.lookup_type = field.lookup_type;
                        newField.seperator = field.seperator;
                        newField.multiline_type = field.multiline_type;

                        if (selectedField) {
                            newField.order = selectedField.order;
                            fields.selectedFields.push(newField);
                        }
                        else {
                            var primaryField = $filter('filter')(module.fields, { primary: true }, true)[0];

                            if (field.name != primaryField.name)
                                fields.availableFields.push(newField);
                            else
                                fields.selectedFields.push(newField);
                        }

                        fields.allFields.push(newField);
                    });

                    fields.selectedFields = $filter('orderBy')(fields.selectedFields, 'order');
                    fields.availableFields = $filter('orderBy')(fields.availableFields, 'order');

                    return fields;
                }
            };
        }]);