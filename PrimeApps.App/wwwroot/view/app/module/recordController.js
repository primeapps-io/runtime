'use strict';

angular.module('primeapps')

    .controller('RecordController', ['$rootScope', '$scope', '$filter', 'helper', '$location', '$state', '$stateParams', '$q', '$window', '$localStorage', '$cache', 'config', '$timeout', 'operations', 'FileUploader', 'ModuleService', 'DocumentService', '$http', 'resizeService', 'components', 'mdToast', '$mdDialog', 'exportFile', 'AppService', 'customScripting',
        function ($rootScope, $scope, $filter, helper, $location, $state, $stateParams, $q, $window, $localStorage, $cache, config, $timeout, operations, FileUploader, ModuleService, DocumentService, $http, resizeService, components, mdToast, $mdDialog, exportFile, AppService, customScripting) {

            $scope.formUniqKey = "".generateRandomKey(10);
            if (!$scope.formType) {
                $scope.type = $stateParams.type;
                $scope.subtype = $stateParams.stype;
                $scope.id = $location.search().id;
                $scope.parentType = $location.search().ptype;
                $scope.parentId = $location.search().pid;
                $scope.returnTab = $location.search().rtab;
                $scope.previousParentType = $location.search().pptype;
                $scope.previousParentId = $location.search().ppid;
                $scope.previousReturnTab = $location.search().prtab;
                $scope.backUrlAtt = $location.search().back;
                $scope.back = $location.search().back;
                $scope.many = $location.search().many;
                $scope.clone = $location.search().clone;
                $scope.revise = $location.search().revise;
                $scope.paramField = $location.search().field;
                $scope.paramValue = $location.search().value;
                $scope.fastRecordModal = false
            }
            $rootScope.sideinclude = false;
            $scope.isDisabled = false;
            $scope.actionButtonDisabled = false;
            $scope.operations = operations;
            $scope.hasPermission = helper.hasPermission;
            $scope.hasDocumentsPermission = helper.hasDocumentsPermission;
            $scope.hasAdminRights = helper.hasAdminRights;
            $scope.hasFieldFullPermission = ModuleService.hasFieldFullPermission;
            $scope.hasFieldReadOnlyPermission = ModuleService.hasFieldReadOnlyPermission;
            $scope.hasSectionFullPermission = ModuleService.hasSectionFullPermission;
            $scope.hasSectionDisplayPermission = ModuleService.hasSectionDisplayPermission;
            $scope.hasSectionReadOnlyPermission = ModuleService.hasSectionReadOnlyPermission;
            $scope.lookupUserAndGroup = helper.lookupUserAndGroup;
            $scope.lookupUser = helper.lookupUser;
            $scope.loading = true;
            $scope.image = {};
            $scope.document = {};
            $scope.hasProcessEditPermission = false;
            $scope.userAdded = false;
            $scope.isActive = {};
            $scope.tab = 'general';
            $scope.customLeaveFields = {};
            $scope.isAdmin = $rootScope.user.profile.has_admin_rights;
            $scope.googleMapsApiKey = googleMapsApiKey;
            $scope.relationsGridOptions = [];
            $scope.showHelp = false;
            $scope.buttonsParametersData = {};
            $scope.tenantLanguage = tenantLanguage;
            $rootScope.expressionLookupModuleId = [];
            $rootScope.hideFieldValue = {};
            $scope.relatedModuleDatas = {};
            $scope.profileInfo = {};

            $scope.goUrl2 = function (id, moduleName) {
                const selection = window.getSelection();
                if (selection.toString().length === 0) {
                    window.location = '#/app/record/' + moduleName + '?id=' + id;
                }
            };

            $scope.imageDownload = function (url, title) {
                $window.download(url, title);
            };

            if ($scope.parentId)
                $window.scrollTo(0, 0);

            if ($scope.backUrlAtt)
                $scope.tab = 'document';

            $scope.module = angular.copy($rootScope.modulus[$scope.type]);

            $scope.manyToManyRelation = undefined;
            if ($scope.module.relations && $scope.parentType) {
                $scope.manyToManyRelation = $filter('filter')($scope.module.relations, {
                    related_module: $scope.parentType,
                    relation_type: 'many_to_many',
                    deleted: false
                })[0];
            }

            const help = $filter('filter')($scope.module.helps, function (help) {
                return help.modal_type === 'side_modal' && help.module_type === 'module_detail'
            }, true)[0];

            if (help)
                $scope.showHelp = true;

            // I have changed frontend filter to banckend filter
            $scope.module.sections = $filter('filter')($scope.module.sections, function (section) {

                // if ($scope.preview) {
                //     if (section.name === 'system' || section.name === 'system_information')
                //         section.display_detail = true;
                // }

                //$rootScope.processLanguage(section);

                return section.display_detail && !section.deleted;
            });

            $rootScope.breadcrumblist = [
                {
                    title: $filter('translate')('Layout.Menu.Dashboard'),
                    link: "#/app/dashboard"
                },
                {
                    title: $rootScope.getLanguageValue($scope.module.languages, 'label', 'plural'),
                    link: "#/app/modules/" + $scope.module.name
                },
                {
                    title: $scope.title
                }
            ];

            if ($scope.module.detail_view_type) {
                $scope.tabconfig = $scope.module.detail_view_type !== 'flat';
            } else {
                $scope.tabconfig = $rootScope.detailViewType !== 'flat';
            }

            if ($scope.module.default_tab)
                $scope.tab = $scope.module.default_tab;

            components.run('BeforeFormLoaded', 'script', $scope);
            $scope.currentSectionComponentsTemplate = currentSectionComponentsTemplate;

            //allow encrypted fields
            if (!$scope.id) {
                for (var f = 0; f < $scope.module.fields.length; f++) {
                    var field = $scope.module.fields[f];
                    if (field.show_lock) {
                        field.show_lock = false;
                    }
                }
            }

            if (!$scope.module) {
                mdToast.warning($filter('translate')('Common.NotFound'));
                $state.go('app.dashboard');
                return;
            }

            $scope.dropdownFields = $filter('filter')($scope.module.fields, {
                data_type: 'lookup',
                show_as_dropdown: true
            }, true);

            $scope.dropdownFieldDatas = {};
            for (var i = 0; i < $scope.dropdownFields.length; i++) {
                $scope.dropdownFieldDatas[$scope.dropdownFields[i].name] = [];
            }

            if (!$scope.id && !$scope.hasPermission($scope.type, $scope.operations.write)) {
                mdToast.error($filter('translate')('Common.Forbidden'));
                $state.go('app.dashboard');
                return;
            }

            $scope.primaryField = $filter('filter')($scope.module.fields, { primary: true })[0];
            $scope.currentUser = ModuleService.processUser($rootScope.user);
            $scope.currentDayMin = helper.getCurrentDateMin().toISOString();
            $scope.currentDayMax = helper.getCurrentDateMax().toISOString();
            $scope.currentHour = helper.floorMinutes(new Date());
            $scope.relatedToField = $filter('filter')($scope.module.fields, { name: 'related_to' }, true)[0];
            $scope.record = {};

            if ($scope.formType && $scope.recordModal)
                $scope.record = $scope.recordModal;

            if (!$scope.id) {
                $scope.title = $filter('translate')('Module.New', { title: $rootScope.getLanguageValue($scope.module.languages, 'label', 'singular') });
            }


            if ($scope.currentModuleProcess) {
                const profileIds = $scope.currentModuleProcess.profiles.split(',');
                for (var i = 0; i < profileIds.length; i++) {
                    if (profileIds[i] === $rootScope.user.profile.id.toString())
                        $scope.hasProcessEditPermission = true;
                }
            }
            if ($scope.parentType) {
                if ($scope.many) {
                    $scope.parentModule = $scope.parentType;
                } else {
                    const parentTypeField = $filter('filter')($scope.module.fields, { name: $scope.parentType }, true)[0];

                    if (!parentTypeField) {
                        $scope.parentType = null;
                        $scope.parentId = null;
                    } else {
                        $scope.parentModule = parentTypeField.lookup_type;
                    }
                }
            }

            $scope.picklistFilter = function (param) {
                return function (item) {
                    $scope.componentFilter = {};
                    $scope.componentFilter.item = item;
                    $scope.componentFilter.result = true;
                    components.run('PicklistFilter', 'Script', $scope);
                    return !item.hidden && !item.inactive && $scope.componentFilter.result;
                };
            };

            const isFreeze = function (record) {
                var type = false;

                if (record.process_status === 1)
                    return true;

                if ($scope.module.dependencies.length > 0) {
                    const freezeDependencies = $filter('filter')($scope.module.dependencies, {
                        dependency_type: 'freeze',
                        deleted: false
                    }, true);

                    for (var j = 0; j < freezeDependencies.length; j++) {
                        const dependency = freezeDependencies[j];
                        const freezeFields = $filter('filter')($scope.module.fields, { name: dependency.parent_field }, true);
                        for (var k = 0; k < freezeFields.length; k++) {
                            const field = freezeFields[k];
                            if (dependency.values_array && dependency.values_array.length > 0) {
                                for (var l = 0; l < dependency.values_array.length; l++) {
                                    const value = parseInt(dependency.values_array[l]);
                                    if (record[field.name] && (value === record[field.name] || value === record[field.name].id))
                                        type = true;
                                }
                            }
                        }
                    }
                }

                record.freeze = type ? type : undefined;

                return type;
            };

            $scope.addNewTagItem = function () {

                const tagInstance = angular.element(document.getElementById($scope.field.name)).data("kendoMultiSelect");

                if (!tagInstance || !tagInstance._prev) {
                    return;
                }

                const dataSource = tagInstance.dataSource;
                const value = tagInstance._prev;
                var newId = 0;
                if (dataSource._data.length > 0) {
                    const dataArray = $filter('orderBy')(dataSource._data, 'id');
                    // Id'lerin çakışmaması için, array'in son data Id'sine bir ekliyoruz.
                    newId = dataArray[dataArray.length - 1].id + 1;
                }

                const newItem = {
                    text: value,
                    id: newId
                };

                dataSource.add(newItem);

                const existingProduct = dataSource.data().find(function (element) {
                    return element.text === value;
                });

                if (existingProduct) {
                    if (!$scope.record[$scope.field.name])
                        $scope.record[$scope.field.name] = [];

                    // Add to selected values
                    $scope.record[$scope.field.name].push(newItem.id);
                }
            };

            $scope.getRecord = function (isRefresh) {

                if (isRefresh)
                    $scope.loading = isRefresh;

                ModuleService.getPicklists($scope.module)
                    .then(function (picklists) {

                        const promises = [];
                        $rootScope.processPicklistLanguages(picklists);
                        $scope.picklistsModule = picklists;
                        const ownerField = $filter('filter')($scope.module.fields, { name: 'owner' }, true)[0];

                        const setFieldDependencies = function () {
                            for (var k = 0; k < $scope.module.fields.length; k++) {

                                var field = $scope.module.fields[k];
                                ModuleService.setDependency(field, $scope.module, $scope.record, $scope.picklistsModule, $scope);

                                if ((!$scope.record.id && field.default_value && field.data_type === 'picklist') || (field.default_value && field.data_type === 'picklist' && $scope.record[field.name] && parseInt(field.default_value) === $scope.record[field.name].id)) {
                                    $scope.record[field.name] = $scope.record[field.name] || $filter('filter')($scope.picklistsModule[field.picklist_id], { id: field.default_value })[0];
                                    $scope.fieldValueChange(field);
                                }

                                //Set picklist dataSource and options
                                if (field.data_type === 'picklist' && !field.deleted) {

                                    $scope["customOptions" + field.picklist_id] = {
                                        dataSource: new kendo.data.DataSource({
                                            transport: {
                                                read: function (o) {
                                                    o.success($filter('filter')($scope.picklistsModule[$scope.optionId], function (item) {
                                                        $scope.componentFilter = {};
                                                        $scope.componentFilter.item = item;
                                                        $scope.componentFilter.result = true;
                                                        components.run('PicklistFilter', 'Script', $scope);
                                                        return !item.hidden && !item.inactive && $scope.componentFilter.result;
                                                    }, true))
                                                }
                                            }
                                        }),
                                        dataTextField: 'label',
                                        dataValueField: "id",
                                        ingoreCase: true,
                                        filter: $scope.picklistsModule[field.picklist_id].length > 10 ? 'startswith' : null,
                                        filtering: function (e) {
                                            //get filter descriptor
                                            if (e.filter && e.filter.value && e.filter.value.indexOf('i') > -1) {
                                                var filterValue = e.filter.value;
                                                var field = e.filter.field;
                                                var operator = e.filter.operator;
                                                e.preventDefault();

                                                var newValueUp = filterValue.replace(/i/gim, 'İ').toLocaleUpperCase();
                                                var newValueTR = filterValue.toLocaleUpperCase().replace(/İ/gim, 'I');

                                                this.dataSource.filter({
                                                    logic: "or",
                                                    filters: [{
                                                        field: field,
                                                        operator: operator,
                                                        value: filterValue,
                                                        ignoreCase: true
                                                    }, {
                                                        field: field,
                                                        operator: operator,
                                                        value: newValueUp,
                                                        ignoreCase: true
                                                    }, {
                                                        field: field,
                                                        operator: operator,
                                                        value: newValueTR,
                                                        ignoreCase: true
                                                    }, {
                                                        field: field,
                                                        operator: operator,
                                                        value: newValueTR.toLocaleLowerCase(),
                                                        ignoreCase: true
                                                    }]
                                                });
                                            }
                                            // handle the event
                                        },
                                        autoBind: false,
                                        optionLabel: $filter('translate')('Common.Select')
                                    };
                                }

                                if (field.data_type === 'multiselect' && !field.deleted) {
                                    $scope["customOptions" + field.picklist_id] = {

                                        dataSource: new kendo.data.DataSource({
                                            data: $filter('filter')($scope.picklistsModule[field.picklist_id], function (item) {
                                                $scope.componentFilter = {};
                                                $scope.componentFilter.item = item;
                                                $scope.componentFilter.result = true;
                                                components.run('PicklistFilter', 'Script', $scope);

                                                return !item.hidden && !item.inactive && $scope.componentFilter.result;
                                            }, true),
                                        }),
                                        tagMode: $scope.id ? $scope.record[field.name].length <= 5 ? "multiple" : "single" : "multiple",
                                        dataTextField: 'label',
                                        dataValueField: "id",
                                        filter: 'contains',
                                        filtering: function (e) {
                                            //get filter descriptor
                                            if (e.filter && e.filter.value.indexOf('i') > -1) {
                                                var filterValue = e.filter.value;
                                                var field = e.filter.field;
                                                var operator = e.filter.operator;
                                                e.preventDefault();

                                                var newValueUp = filterValue.replace(/i/gim, 'İ').toLocaleUpperCase();
                                                var newValueTR = filterValue.toLocaleUpperCase().replace(/İ/gim, 'I');

                                                this.dataSource.filter({
                                                    logic: "or",
                                                    filters: [{
                                                        field: field,
                                                        operator: operator,
                                                        value: filterValue,
                                                        ignoreCase: true
                                                    }, {
                                                        field: field,
                                                        operator: operator,
                                                        value: newValueUp,
                                                        ignoreCase: true
                                                    }, {
                                                        field: field,
                                                        operator: operator,
                                                        value: newValueTR,
                                                        ignoreCase: true
                                                    }, {
                                                        field: field,
                                                        operator: operator,
                                                        value: newValueTR.toLocaleLowerCase(),
                                                        ignoreCase: true
                                                    }]
                                                });
                                            }
                                            // handle the event
                                        },
                                        valuePrimitive: true,
                                        autoClose: false,
                                        optionLabel: $filter('translate')('Common.Select'),
                                        change: function (e) {

                                            const fieldName = e.sender.element[0].name;
                                            if (fieldName) {
                                                const field = $filter('filter')($scope.module.fields, {
                                                    name: fieldName,
                                                    deleted: false
                                                }, true)[0];
                                                if (field) {
                                                    $scope.fieldValueChange(field);
                                                }
                                            }
                                            const selectedValues = this.value();
                                            const currentTagMode = this.options.tagMode;
                                            var newTagMode = currentTagMode;
                                            if (selectedValues.length <= 5) {
                                                newTagMode = "multiple";
                                            } else {
                                                newTagMode = "single"
                                            }
                                            if (newTagMode !== currentTagMode) {
                                                this.value([]);
                                                this.setOptions({
                                                    tagMode: newTagMode
                                                });
                                                this.value(selectedValues);
                                            }
                                        }
                                    };
                                }

                                if (field.data_type === 'tag' && !field.deleted) {

                                    const dataSource = new kendo.data.DataSource({
                                        transport: {
                                            read: {
                                                url: "/api/tag/get_tag/" + field.id,
                                                type: 'GET',
                                                dataType: "json",
                                                beforeSend: $rootScope.beforeSend(),
                                            }
                                        },
                                        requestEnd: function (e) {
                                            const response = e.response;
                                            if (response && response.length > 0) {
                                                const field = $filter('filter')($scope.module.fields, {
                                                    id: response[0].field_id,
                                                    deleted: false
                                                }, true)[0];
                                                if (field && $scope.record[field.name]) {
                                                    for (var i = 0; i < $scope.record[field.name].length; i++) {
                                                        const recordTag = $filter('filter')(response, { text: $scope.record[field.name][i] }, true)[0];
                                                        if (recordTag)
                                                            $scope.record[field.name][i] = recordTag.id;
                                                    }
                                                }
                                            }
                                        }
                                    });

                                    if (isRefresh)
                                        dataSource.read();

                                    $scope['tagOptions' + field.id] = {
                                        dataSource: dataSource,
                                        dataTextField: "text",
                                        dataValueField: "id",
                                        valuePrimitive: true,
                                        filter: 'contains',
                                        tagMode: $scope.id ? $scope.record[field.name].length <= 5 ? "multiple" : "single" : "multiple",
                                        noDataTemplate: '#var value = instance.input.val();# <div>No data found. </div><div ng-show="showAddNewTagButton(\'' + field.name + '\')" style="text-transform: inherit">Do you want to add new item ?</div><md-button ng-show="showAddNewTagButton(\'' + field.name + '\')" class="btn btn-secondary" ng-click="addNewTagItem()" aria-label="New Item"> <i class="fas fa-plus"></i> <span>#:value#</span></md-button></div>',
                                        change: function (e) {
                                            const selectedValues = this.value();
                                            const currentTagMode = this.options.tagMode;
                                            var newTagMode = currentTagMode;
                                            if (selectedValues.length <= 5) {
                                                newTagMode = "multiple";
                                            } else {
                                                newTagMode = "single"
                                            }
                                            if (newTagMode !== currentTagMode) {
                                                this.value([]);
                                                this.setOptions({
                                                    tagMode: newTagMode
                                                });
                                                this.value(selectedValues);
                                            }

                                            var fieldName = e.sender.element[0].name;
                                            if (fieldName) {
                                                const field = $filter('filter')($scope.module.fields, {
                                                    name: fieldName,
                                                    deleted: false
                                                }, true)[0];
                                                if (field) {
                                                    $scope.fieldValueChange(field);
                                                }
                                            }
                                        }
                                    };

                                }

                                if (field.data_type === 'lookup' && !field.deleted) {
                                    //global yapılacak
                                    $scope['lookupOptions' + field.id] = {
                                        dataSource: new kendo.data.DataSource({
                                            serverFiltering: true,
                                            transport: {
                                                read: function (options) {
                                                    const findRequest = {
                                                        module: $scope.currentLookupField.lookup_type,
                                                        convert: false
                                                    };

                                                    if (!options.data.filter) {
                                                        options.data.filter = {
                                                            "filters": []
                                                        }
                                                    }

                                                    var filter = Object.assign({}, options.data.filter.filters[0]);
                                                    if (!options.data.filter || (options.data.filter && options.data.filter.filters.length === 0)) {
                                                        options.data.filter = {};
                                                        options.data.filter.filters = [];
                                                        options.data.filter.logic = 'and';
                                                        /*Mevcut kayıt üzerinden ilgili relation'a gidildiğinde lookup setli olduğu için aşağıdaki değer undefined dönüyor : O yüzden data.filter.filters'ta ki value setlenirken tekrardan kontrol yapılıyor*/
                                                        const value = $scope.record[$scope.currentLookupField.name] ? $scope.record[$scope.currentLookupField.name][$scope.currentLookupField.lookupModulePrimaryField.name] : '';
                                                        prepareLookupSearchField();

                                                        if ($scope.currentLookupField && $scope.currentLookupField.filters.length > 0) {
                                                            for (var h = 0; h < $scope.currentLookupField.filters.length; h++) {
                                                                const filter = $scope.currentLookupField.filters[h];
                                                                options.data.filter.filters.push({
                                                                    field: filter.filter_field,
                                                                    operator: filter.operator,
                                                                    value: filter.value
                                                                });
                                                            }
                                                        }

                                                        var defaultLookupFilter = {
                                                            field: $scope.currentLookupField.lookupModulePrimaryField.name,
                                                            operator: $scope.currentLookupField.lookup_search_type && $scope.currentLookupField.lookup_search_type !== "" ? $scope.currentLookupField.lookup_search_type.replace('_', '') : "startswith",
                                                            value: $scope.record[$scope.currentLookupField.name] ? value ? value : '' : ''
                                                        };

                                                        if ($scope.currentLookupField.lookupModulePrimaryField.data_type === 'number' || $scope.currentLookupField.lookupModulePrimaryField.data_type === 'number_auto') {
                                                            defaultLookupFilter.operator = value !== '' ? 'equals' : 'not_empty';
                                                            defaultLookupFilter.value = defaultLookupFilter.value === 'not_empty' ? '-' : defaultLookupFilter.value;
                                                        }

                                                    }
                                                    if (options.data.filter.filters.length > 0) {
                                                        //default lookup operator = starts_with or contains its come from studio
                                                        //if lookup primary field's data_type equal number or auto_number we have to change default operator.
                                                        processLookupOperator(options.data.filter.filters, $scope.currentLookupField.lookupModulePrimaryField);
                                                        options.data.filter.filters = ModuleService.fieldLookupFilters($scope.currentLookupField, $scope.record, options.data.filter.filters, findRequest);
                                                        findRequest.fields.push($scope.currentLookupField.lookupModulePrimaryField.name);
                                                        if ($scope.currentLookupField.lookupModulePrimaryField.data_type === 'number' || $scope.currentLookupField.lookupModulePrimaryField.data_type === 'number_auto') {

                                                            if (!options.data.filter.filters[0].operator)
                                                                options.data.filter.filters[0].operator = 'equals';

                                                        } else if (options.data.filter.filters.length > 0) {
                                                            const operator = options.data.filter.filters[0].operator;
                                                            if (operator.contains('_'))
                                                                options.data.filter.filters[0].operator = operator !== "" ? operator.replace('_', '') : "startswith";
                                                        }
                                                        $scope.lookupSearchValue = $scope.lookupInput ? $scope.lookupInput._prev : '';
                                                    }

                                                    $.ajax({
                                                        url: '/api/record/find_custom',
                                                        contentType: 'application/json',
                                                        dataType: 'json',
                                                        type: 'POST',
                                                        data: JSON.stringify(Object.assign(findRequest, options.data)),
                                                        success: function (result) {
                                                            $rootScope.processLanguages(result.data);
                                                            options.success(result);
                                                        },
                                                        beforeSend: $rootScope.beforeSend()
                                                    })
                                                }
                                            },
                                            schema: {
                                                data: "data",
                                                total: "total",
                                                model: { id: "id" }
                                            },
                                        }),
                                        optionLabel: $filter('translate')('Common.Select'),
                                        autoBind: false,
                                        dataTextField: field.lookupModulePrimaryField.name,
                                        dataValueField: "id",
                                        noDataTemplate: '<div>' + $filter('translate')('Common.NoDataFound') + ' </div><div ng-if="(field.name !== \'owner\' || formType === \'null\') && field.lookup_type !== \'users\'"><div style="text-transform: inherit" ng-if="lookupSearchValue!==\'' + '\'">' + $filter('translate')('Module.AddNewItem') + '</div><md-button ng-if="lookupSearchValue!==\'' + '\'" class="btn btn-secondary" ng-click="fastNewRecordModal(field.lookup_type,true,lookupSearchValue,field.name)"> <i class="fas fa-plus"></i> <span>{{lookupSearchValue}}</span></md-button></div>',
                                    };
                                    if ($scope.parentType === field.name) {
                                        $scope.parentTypeField = field;
                                        $scope.setCurrentLookupField($scope.parentTypeField, 'lookup' + $scope.parentTypeField.id)
                                        $scope["lookupOptions" + $scope.parentTypeField.id].dataSource.read();
                                    }
                                }

                                if (field.data_type === 'rating' && !field.deleted) {
                                    $scope['ratingOption' + field.id] = {
                                        //min: field.min || 0,
                                        max: field.max || 5,
                                        precision: "half",
                                        value: $scope.record[field.name],
                                        change: function (e) {
                                            var fieldName = e.sender.element[0].name;
                                            if (fieldName) {
                                                const field = $filter('filter')($scope.module.fields, {
                                                    name: fieldName,
                                                    deleted: false
                                                }, true)[0];
                                                if (field) {
                                                    $scope.fieldValueChange(field);
                                                }
                                            }
                                        }
                                    };
                                }

                                if (field.data_type === 'number' && !field.deleted) {
                                    field = setMinMaxValueForField(field);
                                    $scope['numberOptions' + field.id] = {
                                        format: "#",
                                        decimals: 0,
                                        max: field.validation.max,
                                        min: field.validation.min
                                    };
                                }

                                if (field.data_type === 'number_decimal' && !field.deleted) {
                                    field = setMinMaxValueForField(field);
                                    var numberDecimalOptions = $scope['numberDecimalOptions' + field.id] = {
                                        max: field.validation.max,
                                        min: field.validation.min,
                                        rounding: field.rounding
                                    };
                                    numberDecimalOptions.format = ModuleService.renderNumberDecimalFormat(field.decimal_places);
                                }

                                if (field.data_type === 'currency' && !field.deleted) {

                                    field = setMinMaxValueForField(field);
                                    var currencyOptions = $scope['currencyOptions' + field.id] = {
                                        culture: kendo.cultures.current.name,
                                        format: "c",
                                        decimals: field.decimal_places || kendo.cultures.current.numberFormat.currency.decimals,
                                        max: field.validation.max,
                                        min: field.validation.min,
                                        rounding: field.rounding
                                    };

                                    currencyOptions.format = ModuleService.renderCurrencyFormat(field.currency_symbol, field.decimal_places);

                                }
                            }
                        };

                        components.run('BeforeFormPicklistLoaded', 'Script', $scope);

                        if (!$scope.id) {

                            $scope.loading = false;
                            $scope.record.owner = $scope.currentUser;
                            $scope.record.owner.full_name = $scope.currentUser.primary_value;

                            if ($scope.parentId) {
                                const moduleParent = $filter('filter')($rootScope.modules, { name: $scope.parentModule }, true)[0];

                                ModuleService.getRecord($scope.parentModule, $scope.parentId)
                                    .then(function onSuccess(parent) {

                                        const moduleParentPrimaryField = $filter('filter')(moduleParent.fields, {
                                            primary: true,
                                            deleted: false
                                        }, true)[0];
                                        const lookupRecord = {};
                                        lookupRecord.id = parent.data.id;
                                        lookupRecord[moduleParentPrimaryField.name] = parent.data[moduleParentPrimaryField.name];
                                        lookupRecord.primary_value = parent.data[moduleParentPrimaryField.name];


                                        if ($scope.relatedToField) {
                                            $scope.record['related_to'] = lookupRecord;
                                            $scope.record['related_module'] = $filter('filter')(picklists['900000'], { value: $scope.parentType }, true)[0];
                                        } else {
                                            $scope.record[$scope.parentType] = lookupRecord;

                                            const relatedDependency = $filter('filter')($scope.module.dependencies, { dependent_field: $scope.parentType }, true)[0];

                                            if (relatedDependency && relatedDependency.deleted !== true) {
                                                const dependencyField = $filter('filter')($scope.module.fields, { name: relatedDependency.field }, true)[0];
                                                $scope.record[relatedDependency.field] = $filter('filter')($scope.picklistsModule[dependencyField.picklist_id], { id: relatedDependency.values[0] }, true)[0];

                                                const dependentField = $filter('filter')($scope.module.fields, { name: relatedDependency.dependent_field }, true)[0];
                                                dependentField.hidden = false;
                                            }

                                            setFieldDependencies();
                                        }
                                    });
                            } else {
                                setFieldDependencies();
                            }

                            ModuleService.setDefaultValues($scope.module, $scope.record, picklists);
                            ModuleService.setDisplayDependency($scope.module, $scope.record);

                            components.run('FieldChange', 'Script', $scope, $scope.record, ownerField);
                            setSharedLookups();
                            $scope.setProfileSharingConfigs();
                            return;
                        }

                        ModuleService.getRecord($scope.module.name, $scope.id)
                            .then(function onSuccess(recordData) {

                                if (Object.keys(recordData.data).length === 0) {
                                    mdToast.error($filter('translate')('Common.Forbidden'));
                                    $state.go('app.dashboard');
                                    return;
                                }
                                $scope.getRecordData = Object.assign({}, recordData.data);

                                const record = ModuleService.processRecordSingle(recordData.data, $scope.module, $scope.picklistsModule);

                                if ($scope.module.name === 'p_profiles') {
                                    $scope.profileInfo = {
                                        name: record['name'],
                                        profile_id: parseInt(record['profile_id'])
                                    };
                                }

                                setSharedLookups();
                                $scope.ActionButtonsLoad($scope.module.id);

                                if (!$scope.hasPermission($scope.type, $scope.operations.modify) && !$scope.hasPermission($scope.type, $scope.operations.modify, recordData.data) && $scope.hasPermission($scope.type, $scope.operations.read) && !$scope.hasPermission($scope.type, $scope.operations.read, recordData.data)) {
                                    mdToast.error($filter('translate')('Common.Forbidden'));
                                    $state.go('app.dashboard');
                                    return;
                                }

                                if (!$scope.hasPermission($scope.type, $scope.operations.modify)) {
                                    /*if (!$scope.clone && !$scope.hasPermission($scope.type, $scope.operations.modify, recordData.data) && ($scope.hasPermission($scope.type, $scope.operations.read, recordData.data) || $scope.hasPermission($scope.type, $scope.operations.read)))
                                        record.freeze = true;
                                    else {*/
                                    isFreeze(record);
                                    //}
                                } else if (!$scope.hasPermission($scope.type, $scope.operations.modify, recordData.data)) {
                                    record.freeze = true;
                                }

                                components.run('BeforeFormRecordLoaded', 'Script', $scope, record);
                                ModuleService.formatRecordFieldValues(Object.assign({}, recordData.data), $scope.module, $scope.picklistsModule);
                                $scope.title = $scope.primaryField.valueFormatted;
                                $rootScope.breadcrumblist[2].title = $scope.title;
                                $scope.recordState = Object.assign({}, record);

                                //encrypted fields
                                for (var f = 0; f < $scope.module.fields.length; f++) {
                                    const field = $scope.module.fields[f];
                                    var showEncryptedInput = false;
                                    if (field.encrypted && field.encryption_authorized_users_list.length > 0 && record[field.name]) {
                                        for (var p = 0; p < field.encryption_authorized_users_list.length; p++) {
                                            const encryrptionPermission = field.encryption_authorized_users_list[p];
                                            if ($rootScope.user.id === parseInt(encryrptionPermission))
                                                showEncryptedInput = true;
                                        }
                                    }

                                    field.show_lock = field.encrypted && !showEncryptedInput;
                                }

                                if ($scope.relatedToField && record['related_to']) {
                                    const lookupModule = $filter('filter')($rootScope.modules, { id: record[$scope.relatedToField.lookup_relation].id - 900000 }, true)[0];
                                    const lookupModulePrimaryField = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];

                                    ModuleService.getRecord(lookupModule.name, record['related_to'], true)
                                        .then(function onSuccess(relatedRecord) {

                                            relatedRecord = relatedRecord.data;
                                            relatedRecord.primary_value = relatedRecord[lookupModulePrimaryField.name];
                                            record['related_to'] = relatedRecord;

                                            $scope.record = record;
                                            $scope.recordState = Object.assign({}, $scope.record);

                                            setFieldDependencies();

                                            /*
                                             * Record edit denildiğinde sayfa ilk yüklendiğinde fieldValueChange in tetiklenmediği durumlar var.
                                             * (ex: Admin olmayan bir kullanıcı izinler modülünde ki bir record a edit dediğinde fieldValueChange metodu sayfa ilk açıldığında tetiklenmediği için görsel değişiklikler gerçekleşmiyor)
                                             * Eğer field ların içinde form da gözüken bir lookup alan var ise (show as dropdown olmayan) fieldValueChange otomatik olarak tetikleniyor. Ama böyle bir alan mevcut değil ise tetiklenmiyor.
                                             * Çözüm olarak fieldValueChange in içerisin de ki function ların çalışması için fake bir şekilde ilk field ı kullanarak fieldValueChange metodunu tetikliyoruz.
                                             * */
                                            if ($scope.module && $scope.module.fields && $scope.module.fields.length > 0) {
                                                $scope.fieldValueChange($scope.module.fields[0]);
                                            }

                                            $scope.loading = false;
                                        })
                                        .catch(function onError(response) {
                                            if (response.status === 404) {
                                                record['related_to'] = null;
                                                $scope.record = record;
                                                $scope.recordState = Object.assign({}, $scope.record);
                                            }

                                            $scope.loading = false;
                                        });
                                } else {

                                    $scope.record = record;
                                    setFieldDependencies();

                                    if ($scope.clone) {
                                        $scope.currentUser.full_name = $scope.currentUser.primary_value;
                                        $scope.record.owner = $scope.currentUser;
                                        hideAutoNumberFieldsWhenRecordClone();
                                    }

                                    $scope.loading = false;
                                }

                                if ($scope.record.currency)
                                    $scope.currencySymbol = $scope.record.currency.value || $rootScope.currencySymbol;

                                ModuleService.customActions($scope.module, $scope.record);

                                components.run('FieldChange', 'Script', $scope, $scope.record, ownerField);

                                components.run('AfterFormRecordLoaded', 'Script', $scope);

                                ModuleService.setDisplayDependency($scope.module, record);
                                $scope.setProfileSharingConfigs();
                            })
                            .catch(function onError() {
                                $scope.loading = false;
                            });

                        $scope.export = function (relatedModule) {

                            if (!relatedModule.total || relatedModule.total < 1)
                                return;

                            var isFileSaverSupported = false;

                            try {
                                isFileSaverSupported = !!new Blob;
                            } catch (e) {
                            }

                            if (!isFileSaverSupported) {
                                mdToast.messages($filter('translate')('Module.ExportUnsupported'));
                                return;
                            }

                            if (relatedModule.total > 3000) {
                                mdToast.messages($filter('translate')('Module.ExportWarning'));
                                return;
                            }

                            const fileName = $rootScope.getLanguageValue(relatedModule.languages, 'label', 'plural') + '-' + $filter('date')(new Date(), 'dd-MM-yyyy') + '.xls';
                            $scope.exporting = true;

                            ModuleService.getCSVData($scope, relatedModule, $scope.module)
                                .then(function (csvData) {
                                    mdToast.success($filter('translate')('Module.ExcelExportSuccess'));
                                    exportFile.excel(csvData, fileName);
                                    $scope.exporting = false;
                                });


                        };

                        //Get All Record Count in Related Modules
                        $scope.selectedRows = {};
                        $scope.selectedRecords = {};
                        $scope.isAllSelected = {};
                        $scope.tabRelations = {};
                        if ($scope.module.relations) {

                            for (var i = 0; i < $scope.module.relations.length; i++) {

                                const relation = $scope.module.relations[i];
                                $scope.selectedRows[relation.related_module] = [];
                                $scope.selectedRecords[relation.related_module] = [];
                                $scope.isAllSelected[relation.related_module] = false;

                                if (relation.deleted)
                                    continue;

                                $scope.toolbarRelationOptions['relation' + relation.id] = {
                                    resizable: true,
                                    items: []
                                };
                                if (relation.detail_view_type === 'flat') {
                                    $scope.getRelatedActionButtons(relation);
                                }


                                const relatedModule = $filter('filter')($rootScope.modules, { name: relation.related_module }, true)[0];

                                var filterRequest = {};

                                if (relation.relation_type === 'many_to_many') {
                                    if (!relatedModule) continue;
                                    filterRequest = {
                                        fields: ['total_count()'],
                                        filters: [{
                                            field: $scope.module.name + '_id',
                                            operator: 'equals',
                                            value: $scope.id,
                                            no: 1
                                        }],
                                        limit: 1,
                                        offset: 0,
                                        many_to_many: $scope.module.name,
                                        many_to_many_table_name: relation.many_to_many_table_name,
                                        relatedModule: 'relation' + relation.id
                                    };
                                } else
                                    filterRequest = {
                                        fields: ['total_count()'],
                                        filters: [{
                                            field: relation.relation_field,
                                            operator: 'equals',
                                            value: $scope.id,
                                            no: 1
                                        }],
                                        limit: 1,
                                        offset: 0,
                                        relatedModule: 'relation' + relation.id
                                    };

                                if (relation.detail_view_type === 'flat' || $scope.module.detail_view_type === 'flat') {
                                    setTimeout(function () {
                                        $scope.generateRelationsTable(relation);
                                    }, 100);
                                } else {
                                    $scope.tabRelations['relation' + relation.id] = relation;
                                    ModuleService.findRecords(relation.related_module, filterRequest).then(function (res) {
                                        prepareRelations(res, $scope.tabRelations[res.config.data.relatedModule]);
                                    });
                                }
                            }
                        }
                        components.run('AfterFormPicklistLoaded', 'Script', $scope);
                    });
            };

            $scope.getRecord();

            $scope.grid = [];
            $scope.isOpen = false;

            $scope.demo = {
                isOpen: false,
                count: 1000,
                selectedDirection: 'left'
            };

            $scope.selectRow = function ($event, record, moduleName) {
                record.isChecked = $event.target.checked;
                $scope.selectedRecords[moduleName] = [];
                const moduleData = $scope.grid[moduleName].dataSource.data();
                var selectAll = true;
                for (var i = 0; i < moduleData.length; i++) {
                    if (!moduleData[i].isChecked) {
                        $scope.isAllSelected[moduleName] = false;
                        selectAll = false;
                    } else {
                        $scope.selectedRecords[moduleName].push(moduleData[i].id || moduleData[i][$scope.forManyToManyFielName]);
                    }
                }
                if (selectAll) {
                    $scope.isAllSelected[moduleName] = true;
                }

            };
            $scope.isOpenTest = false;
            $scope.selectAll = function ($event, moduleName) {
                const moduleData = $scope.grid[moduleName].dataSource.data();
                $scope.selectedRecords[moduleName] = [];
                for (var i = 0; i < moduleData.length; i++) {
                    moduleData[i].isChecked = $event.target.checked;
                    if ($event.target.checked) {
                        $scope.selectedRecords[moduleName].push(moduleData[i].id || moduleData[i][$scope.forManyToManyFielName]);
                    }
                }
                if ($event.target.checked) {
                    $scope.isOpenTest = true;
                }
            };

            $scope.refreshGrid = function (moduleName) {
                $scope.grid[moduleName].dataSource.read();
            };

            $scope.setCurrentLookupField = function (field, lookupInput) {

                if (field.filters)
                    field.filters = $filter('filter')(field.filters, { deleted: false }, true);

                $scope.currentLookupField = field;

                if (lookupInput) {
                    if (field.filters && field.filters.length > 0) {
                        if ($scope["lookupOptions" + field.id])
                            $scope["lookupOptions" + field.id].dataSource.read();
                    }

                    $scope.lookupInput = lookupInput;
                    $scope.lookupSearchValue = lookupInput._prev;
                }
            };

            $scope.uploader = new FileUploader({
                url: 'storage/record_file_upload'
            });

            $scope.entityIdFunc = function () {
                return $scope.recordId;
            };

            $scope.recordDelete = function (ev, id, moduleName) {

                // Appending dialog to document.body to cover sidenav in docs app
                const confirm = $mdDialog.confirm()
                    .title($filter('translate')('Common.AreYouSure'))
                    .targetEvent(ev)
                    .ok($filter('translate')('Common.Yes'))
                    .cancel($filter('translate')('Common.No'));

                $mdDialog.show(confirm).then(function () {
                    ModuleService.getRecord(moduleName, id)
                        .then(function (recordData) {
                            if (!helper.hasPermission($scope.type, operations.modify, recordData.data)) {
                                mdToast.error($filter('translate')('Common.Forbidden'));
                                return;
                            }

                            const record = recordData;

                            $scope.executeCode = false;
                            components.run('BeforeDelete', 'Script', $scope, record);

                            if ($scope.executeCode)
                                return;

                            ModuleService.deleteRecord(moduleName, id)
                                .then(function () {
                                    $scope.refreshGrid(moduleName);
                                    mdToast.success($filter('translate')('Module.DeleteMessage'));
                                    $scope.isAllSelected[moduleName] = false;
                                });
                        });
                }, function () {
                    $scope.status = 'You decided to keep your debt.';
                });
            };

            $scope.deleteSelectedsSubTable = function (ev, moduleName) {
                const confirm = $mdDialog.confirm()
                    .title($filter('translate')('Common.AreYouSure'))
                    .targetEvent(ev)
                    .ok($filter('translate')('Common.Yes'))
                    .cancel($filter('translate')('Common.No'));

                $mdDialog.show(confirm).then(function () {
                    ModuleService.deleteRecordBulk(moduleName, $scope.selectedRecords[moduleName])
                        .then(function (response) {
                            $scope.refreshGrid(moduleName);
                            $scope.selectedRecords[moduleName] = [];
                            $scope.selectedRows[moduleName] = [];
                            $scope.isAllSelected[moduleName] = false;
                        });
                }, function () {
                    $scope.status = 'You decided to keep your debt.';
                });

            };

            $scope.getAddHideFieldValueForExpression = function (record) {
                var hideKeys = Object.keys($rootScope.hideFieldValue);
                var recordKeys = Object.keys(record);
                var hideDetailFields = $filter('filter')($scope.module.fields, { display_detail: false });
                var displayDependenciesFields = $scope.module.display_dependencies;

                if (hideDetailFields)
                    for (var i = 0; i < hideDetailFields.length; i++) {
                        const hideProperty = hideDetailFields[i].name;
                        if (!record[hideProperty] && record[hideProperty] !== false && record[hideProperty] !== 0 && $scope.getRecordData)
                            record[hideProperty] = $scope.getRecordData[hideProperty];
                    }
                if (displayDependenciesFields)
                    for (var k = 0; k < displayDependenciesFields.length; k++) {
                        const property = displayDependenciesFields[k].dependent_field;
                        if (!record[property] && record[property] !== false && record[property] !== 0 && $scope.getRecordData)
                            record[property] = $scope.getRecordData[property];
                    }

                for (var l = 0; l < hideKeys.length; l++) {
                    if (recordKeys.indexOf(hideKeys[l]) === -1) {
                        var key = hideKeys[l];
                        var obj = {};
                        var data = $rootScope.hideFieldValue[key];
                        obj[key] = data.substring(1, data.length - 1);
                        record = Object.assign(record, obj)
                    }
                }

                return record;
            };

            $scope.submit = function (orjRecord, e) {

                var checker;
                var record = Object.assign({}, orjRecord);
                record = $scope.getAddHideFieldValueForExpression(record);

                function validate() {

                    var isValid = true;

                    for (var i = 0; i < $scope.module.fields.length; i++) {
                        const field = $scope.module.fields[i];
                        if (!record[field.name] && (field.data_type !== 'date' && field.data_type !== 'date_time' && field.data_type !== 'time') && !(field.data_type === 'picklist' && field.view_type === 'checkbox' && !field.hidden))
                            continue;

                        if (field.data_type === 'document' && $scope.document[field.name] && $scope.document[field.name].UniqueName && record[field.name] != null) {
                            record[field.name] = $scope.document[field.name].UniqueName;
                        } else if (field.data_type === 'document' && !record[field.name] && !field.hidden && field.validation.required) {
                            isValid = false;
                        } else if (field.data_type === 'lookup' && typeof record[field.name] !== 'object') {
                            // $scope.moduleForm[field.name].$setValidity('object', false);
                            isValid = false;
                        } else if (field.data_type === 'image' && $scope.image[field.name] && $scope.image[field.name].UniqueName && record[field.name] != null) {
                            record[field.name] = $scope.image[field.name].UniqueName;
                        } else if (field.data_type === 'date' || field.data_type === 'date_time' || field.data_type === 'time') {
                            delete record[field.name + 'str'];
                        } else if (field.data_type === 'tag') {
                            for (var j = 0; j < record[field.name].length; j++) {
                                const tag = $filter('filter')($scope['tagOptions' + field.id].dataSource._view, { id: parseInt(record[field.name][j]) }, true)[0];
                                if (tag)
                                    record[field.name][j] = tag.text;
                            }
                        } else if (!record[field.name] && field.data_type === 'picklist' && field.view_type === 'checkbox' && !field.hidden && field.validation.required)
                            isValid = false;

                        // else if (field.validation && field.validation.min_length && (field.validation.min_length !== '' || field.validation.min_length !== 0) && record[field.name]
                        //     && field.validation.min_length > record[field.name].toString().length && field.validation.required &&
                        //     (field.data_type === 'text_single' || field.data_type === 'text_multi' || field.data_type === 'number' || field.data_type === 'number_decimal' ||
                        //         field.data_type === 'currency' || field.data_type === 'email' || field.data_type === 'url')) {
                        //     isValid = false;
                        // }
                    }
                    return isValid;
                }

                function expressionValidate() {
                    var expValid = ModuleService.setExpression($scope.module, $scope.record, $scope.picklistsModule, null, true, true);
                    return expValid;
                }

                $scope.submitting = true;
                if (!$scope.moduleForm.validate() || !validate()) {

                    e.preventDefault();

                    $scope.submitting = false;
                    mdToast.error($filter('translate')('Module.RequiredError'));
                    return;
                }

                var expValidateResult = expressionValidate();
                if (expValidateResult !== undefined) {
                    $scope.submitting = false;
                    e.preventDefault();
                    mdToast.error(expValidateResult);
                    return;
                }

                if ($scope.module.name === 'p_profiles' && $scope.profileConfig) {

                    angular.forEach($scope.profileConfig['config'], function (moduleConfig) {
                        if (!moduleConfig.actions.read || !moduleConfig.actions.read.enable) {

                            moduleConfig.fields.dont_show = [];
                            moduleConfig.fields.readonly = [];

                            moduleConfig.sections.dont_show = [];
                            moduleConfig.sections.readonly = [];

                            moduleConfig.records.type = "all";
                            moduleConfig.records.filters = [];

                            if (!moduleConfig.actions.read) {
                                moduleConfig.actions.read = {};
                                moduleConfig.actions.read.enable = false;
                            }

                            if (!moduleConfig.actions.create) {
                                moduleConfig.actions.create = {};
                                moduleConfig.actions.create.enable = false;
                            } else {
                                moduleConfig.actions.create.enable = false;
                            }

                            if (!moduleConfig.actions.delete) {
                                moduleConfig.actions.delete = {};
                                moduleConfig.actions.delete.enable = false;
                            } else {
                                moduleConfig.actions.delete.enable = false;
                            }

                            if (!moduleConfig.actions.update) {
                                moduleConfig.actions.update = {};
                                moduleConfig.actions.update.enable = false;
                            } else {
                                moduleConfig.actions.update.enable = false;
                            }
                        }
                    });

                    record["configs"] = JSON.stringify($scope.profileConfig);
                }

                components.run('BeforeFormSubmit', 'Script', $scope, record);

                if (!$scope.id || $scope.clone) {
                    $scope.executeCode = false;
                    components.run('BeforeCreate', 'Script', $scope, record);

                    if ($scope.executeCode) {
                        $scope.submitting = false;
                        return;
                    }
                } else {
                    $scope.executeCode = false;
                    components.run('BeforeUpdate', 'Script', $scope, record);
                    if ($scope.executeCode) {
                        $scope.submitting = false;
                        return;
                    }
                }

                if ($rootScope.branchAvailable && $scope.branchManager) {
                    for (var i = 0; i < $scope.record.Authorities.length; i++) {
                        var authority = $scope.record.Authorities[i];
                        checker = $filter('filter')($scope.authorities, { user_id: authority.id }, true)[0];

                        if (!checker) {
                            $http.post(config.apiUrl + 'user_custom_shares/create/', {
                                shared_user_id: $scope.branchManager.id,
                                user_id: authority.id
                            });
                        }
                    }

                    for (var i = 0; i < $scope.authorities.length; i++) {
                        const authority = $scope.authorities[i];
                        checker = $filter('filter')($scope.record.Authorities, { id: authority.user_id }, true)[0];

                        if (!checker) {
                            $http.delete(config.apiUrl + 'user_custom_shares/delete/' + authority.id)
                        }
                    }
                }

                delete record['Authorities'];

                if ($scope.clone) {

                    delete $scope.record.created_at;
                    delete $scope.record.created_by;
                    delete $scope.record.updated_at;
                    delete $scope.record.updated_by;

                    hideAutoNumberFieldsWhenRecordClone(true, record);

                    $scope.recordState = null;
                }

                record = ModuleService.prepareRecord(record, $scope.module, $scope.recordState);
                //const recordCopy = Object.assign({}, $scope.record);

                if ($scope.module.name === 'p_profiles')
                    record["configs"] = JSON.stringify($scope.profileConfig);

                if (!$scope.id) {

                    ModuleService.insertRecord($scope.module.name, record)
                        .then(function onSuccess(response) {

                            if ($scope.manyToManyRelation) {
                                const data = addManyToManyRelationRecord(response.data.id);

                                const indexType = $scope.manyToManyRelation.many_to_many_table_name.indexOf($scope.type);
                                const indexRelatedModule = $scope.manyToManyRelation.many_to_many_table_name.indexOf($scope.manyToManyRelation.related_module);
                                var firstModule = '';
                                var secondModule = '';
                                if (indexType < indexRelatedModule) {
                                    firstModule = $scope.type;
                                    secondModule = $scope.manyToManyRelation.related_module;
                                } else {
                                    firstModule = $scope.manyToManyRelation.related_module;
                                    secondModule = $scope.type;
                                }
                                ModuleService.addRelations(firstModule, secondModule, data);
                            }

                            result(response.data);

                            record.id = response.data.id;
                            components.run('AfterCreate', 'Script', $scope, record);
                        })
                        .catch(function onError(data) {
                            error(data.data, data.status);
                        })
                        .finally(function () {
                            $scope.submitting = false;
                        });
                } else {
                    //encrypted field
                    for (var f = 0; f < $scope.module.fields.length; f++) {
                        var field = $scope.module.fields[f];
                        var showEncryptedInput = false;
                        if (field.encrypted && field.encryption_authorized_users_list.length > 0 && record[field.name]) {
                            for (var p = 0; p < field.encryption_authorized_users_list.length; p++) {
                                const encryptionPermission = field.encryption_authorized_users_list[p];
                                if ($rootScope.user.id === parseInt(encryptionPermission))
                                    showEncryptedInput = true;
                            }
                        }

                        if (field.encrypted && !showEncryptedInput)
                            delete record[field.name];
                    }

                    if ($scope.clone) {
                        if ($scope.revise) {
                            record.master_id = record.id;
                        }

                        delete record.id;

                        //removes process approval fields
                        delete record.process_id;
                        delete record.process_status;
                        delete record.process_status_order;
                        delete record['process_request_updated_at'];
                        delete record['process_request_updated_by'];
                        delete record.operation_type;
                        delete record.freeze;


                        if (record.auto_id) record.auto_id = "";

                        ModuleService.insertRecord($scope.module.name, record)
                            .then(function onSuccess(response) {
                                if (!record.master_id) {
                                    $scope.submitting = false;
                                    result(response.data);
                                }

                                record.id = response.data.id;
                                if ($scope.clone) {
                                    $scope.clone = undefined;
                                    $window.location.href = '#/app/record/' + $scope.type + '?id=' + record.id;
                                }

                                components.run('AfterCreate', 'Script', $scope, record);
                            })
                            .catch(function onError(data) {
                                error(data.data, data.status);
                                $scope.submitting = false;
                            });
                    } else {

                        ModuleService.updateRecord($scope.module.name, record)
                            .then(function onSuccess(response) {
                                $scope.recordState = Object.assign({}, $scope.record);
                                checkProfiles(response.data);
                                result(response.data);
                                components.run('AfterUpdate', 'Script', $scope, record);
                            })
                            .catch(function onError(data) {
                                error(data.data, data.status);
                            })
                            .finally(function () {
                                $scope.submitting = false;
                            });
                    }
                }

                function result(response) {
                    $scope.addedUser = false;
                    $scope.recordId = response.id;

                    if ($scope.uploader.queue.length > 0) {
                        $scope.uploader.onCompleteAll();
                        $scope.uploader.uploadAll();
                    }

                    components.run('BeforeFormSubmitResult', 'Script', $scope, response);

                    $scope.success = function () {
                        const recordName = response[$scope.module.primaryField.name] || $scope.title;

                        if (!$scope.id || $scope.clone)
                            mdToast.success(recordName + ' ' + $filter('translate')('Module.SuccessfullyAdded'));
                        else
                            mdToast.success(recordName + ' ' + $filter('translate')('Module.SuccessfullyUpdated'));


                        const params = { type: $scope.type, id: response.id };

                        if ($scope.parentId) {
                            params.type = $scope.parentModule;
                            params.id = $scope.parentId;
                            params.rptype = $scope.returnTab;

                            if ($scope.previousParentType) {
                                params.rpptype = $scope.previousParentType;
                                params.rppid = $scope.previousParentId;
                                params.rprtab = $scope.previousReturnTab;
                            }
                        }

                        if ($scope.back)
                            params.back = $scope.back;

                        var cacheKey = $scope.module.name + '_' + $scope.module.name;

                        if (!$scope.parentId) {
                            $cache.remove(cacheKey);
                        } else {
                            cacheKey = (!$scope.relatedToField ? $scope.parentType : 'related_to') + $scope.parentId + '_' + (!$scope.many ? $scope.module.name : $scope.many);
                            const parentCacheKey = $scope.parentType + '_' + $scope.parentType;
                            $cache.remove(cacheKey);
                            $cache.remove(parentCacheKey);
                        }

                        if ($rootScope.activePages && $rootScope.activePages[$scope.module.name])
                            $rootScope.activePages[$scope.module.name] = null;

                        if (!$scope.fastRecordModal) {
                            if ($scope.parentModule && $scope.parentId) {
                                $state.go('app.record', {
                                    type: $scope.parentModule,
                                    id: $scope.parentId,
                                    rtab: $scope.returnTab
                                });
                            } else {
                                $state.go('app.record', {
                                    type: $scope.module.name,
                                    id: $scope.recordId
                                });
                            }
                        } else {
                            $mdDialog.hide();
                            ModuleService.getRecord($scope.type, $scope.recordId)
                                .then(function (lookupRecord) {
                                    var data = lookupRecord.data;
                                    var fieldName = $scope.lookupName;
                                    $scope.modalCustomScopeRecord[fieldName] = data;

                                    //After create fastRecordModal, we have to set value for input
                                    if ($scope.fastRecordModal) {
                                        var lookupInstance = angular.element(document.getElementById(fieldName)).data("kendoDropDownList");
                                        lookupInstance.value(data.id);
                                    }
                                });
                        }
                    };
                    $scope.success();
                }

                function error(data, status) {
                    $scope.addedUser = false;

                    if (status === 409) {
                        const field = data.field2 ? $filter('filter')($scope.module.fields, {
                            name: data.field2,
                            deleted: false
                        })[0] : $filter('filter')($scope.module.fields, { name: data.field, deleted: false })[0];
                        var currentField = field.name;
                        var value = '';

                        if (field.data_type === 'picklist') {
                            value = $scope.record[currentField]['label'];
                        } else if (field.data_type === 'multiselect') {
                            var array = [];

                            for (var l = 0; l < $scope.record[currentField].length; l++) {
                                var id = $scope.record[data.field][l];
                                const result = $filter('filter')($scope['customOptions' + field.picklist_id].dataSource._pristineData, { id: id }, true)[0];
                                if (result)
                                    array.push(result.label);
                            }

                            value = array.join(',');
                        } else if (field.data_type === 'tag') {
                            value = $scope.record[currentField].join(',');
                        } else if (field.data_type === 'lookup') {
                            var objectValue = $scope.record[currentField];
                            value = objectValue[field.lookupModulePrimaryField.name];
                        } else
                            value = $scope.record[currentField];

                        mdToast.warning($filter('translate')('Common.Conflict', {
                            fieldName: $rootScope.getLanguageValue(field.languages, 'label'),
                            value: value
                        }));
                    }
                }
            };

            $scope.calculate = function (field) {
                ModuleService.calculate(field, $scope.module, $scope.record);
            };

            $scope.setReadonly = function (field) {
                if (profileConfigs.config[$scope.module.name]) {
                    if (profileConfigs.config[$scope.module.name].fields && profileConfigs.config[$scope.module.name].fields.readonly && profileConfigs.config[$scope.module.name].fields.readonly.includes(field.name)) {
                        return true;
                    } else if (profileConfigs.config[$scope.module.name].sections && profileConfigs.config[$scope.module.name].sections.readonly && profileConfigs.config[$scope.module.name].sections.readonly.includes(field.section)) {
                        return true;
                    } else {
                        return false;
                    }
                }
            }

            $scope.fieldValueChange = function (field) {

                if (field.valueChangeDontRun) {
                    delete field.valueChangeDontRun;
                    return;
                }

                ModuleService.setDependency(field, $scope.module, $scope.record, $scope.picklistsModule, $scope);
                if (!$scope.loading)
                    ModuleService.setDisplayDependency($scope.module, $scope.record);

                var validation = ModuleService.setExpression($scope.module, $scope.record, $scope.picklistsModule, field, false, false)
                if (validation !== undefined) {
                    mdToast.error(validation);
                }

                ModuleService.setCustomCalculations($scope.module, $scope.record, $scope.picklistsModule, $scope);
                ModuleService.customActions($scope.module, $scope.record, $scope.moduleForm, $scope.picklistsModule, $scope);
                components.run('FieldChange', 'Script', $scope, $scope.record, field);

                if ($scope.record.currency)
                    $scope.currencySymbol = $scope.record.currency.value || $rootScope.currencySymbol;

                if (field.data_type === 'picklist') {
                    const dependencies = $filter('filter')($scope.module.dependencies, function (dependency) {
                        return dependency.parent_field === field.name && (dependency.dependency_type === 'list_field' || dependency.dependency_type === 'list_value');
                    }, true);
                    if (!dependencies || !dependencies.length)
                        return;

                    for (var i = 0; i < dependencies.length; i++) {
                        var dependency = dependencies[i];

                        if (dependency.deleted)
                            continue;

                        const childField = $filter('filter')($scope.module.fields, { name: dependency.child_field }, true)[0];
                        if ($scope.optionId) {
                            $scope['customOptions' + $scope.optionId].dataSource.read();
                        }
                        $scope.optionId = childField.picklist_id;
                        $scope['customOptions' + $scope.optionId].dataSource.read();
                    }
                }
            };

            $scope.hideCreateNew = function (field) {
                if (field.lookup_type === 'users')
                    return true;

                return field.lookup_type === 'relation' && !$scope.record.related_module;

            };

            $scope.openFormModal = function (str) {
                $scope.primaryValueModal = str;

                const parentEl = angular.element(document.body);
                $mdDialog.show({
                    parent: parentEl,
                    templateUrl: 'view/app/module/moduleFormModal.html',
                    clickOutsideToClose: false,
                    scope: $scope,
                    preserveScope: true

                });
            };

            $scope.ActionButtonsLoad = function (moduleId) {
                ModuleService.getActionButtons($scope.module.id)
                    .then(function (actionButtons) {

                        $scope.actionButtons = actionButtons;
                        $scope.toolbarOptions = {
                            resizable: true,
                            items: []
                        };
                        $rootScope.processLanguages(actionButtons);

                        for (var i = 0; actionButtons.length > i; i++) {
                            actionButtonIsShown(actionButtons[i]);

                            if (actionButtons[i].trigger !== 'List' && actionButtons[i].trigger !== 'Relation') {
                                const name = $rootScope.getLanguageValue(actionButtons[i].languages, 'label');
                                if (actionButtons[i].type === 'Scripting' && ModuleService.hasActionButtonDisplayPermission(actionButtons[i], false, $scope.record, $scope.module)) {
                                    var item = {
                                        template: '<md-button ng-show="actionButtons[' + i + '].isShown" class="btn ' + actionButtons[i].color + '"  ng-click="runScript(' + i + ')" aria-label="' + name + '" > <i class="' + actionButtons[i].icon + '"></i> <span>' + name + '</span></md-button>',
                                        overflowTemplate: '<md-button ng-show="actionButtons[' + i + '].isShown" ng-click="runScript(' + i + ')"  class="action-dropdown-item"><i class="' + actionButtons[i].icon + '"></i><span> ' + name + '</span></md-button>',
                                        overflow: "auto"
                                    };

                                    $scope.toolbarOptions.items.push(item);
                                }

                                if (actionButtons[i].type === 'Webhook' && ModuleService.hasActionButtonDisplayPermission(actionButtons[i], false, $scope.record, $scope.module)) {
                                    var item = {
                                        template: '<md-button ng-show="actionButtons[' + i + '].isShown" class="btn ' + actionButtons[i].color + '"  ng-click="webhookRequest(' + i + ')"   aria-label="' + name + '" > <i class="' + actionButtons[i].icon + '"></i> <span>' + name + '</span></md-button>',
                                        overflowTemplate: '<md-button ng-show="actionButtons[' + i + '].isShown" ng-click="webhookRequest(' + i + ')"  class="action-dropdown-item "><i class="' + actionButtons[i].icon + '"></i><span> ' + name + ' </span></md-button>',
                                        overflow: "auto"
                                    };

                                    $scope.toolbarOptions.items.push(item);
                                }

                                if (actionButtons[i].type === 'ModalFrame' && ModuleService.hasActionButtonDisplayPermission(actionButtons[i], false, $scope.record, $scope.module)) {
                                    var item = {
                                        template: '<md-button ng-show="actionButtons[' + i + '].isShown" class="btn ' + actionButtons[i].color + '" ng-click="showModuleFrameModal(\'' + actionButtons[i].url + '\')"   aria-label="' + name + '" > <i class="' + actionButtons[i].icon + '"></i> <span>' + name + '</span></md-button>',
                                        overflowTemplate: '<md-button ng-show="actionButtons[' + i + '].isShown"  ng-click="showModuleFrameModal(\'' + actionButtons[i].url + '\')"   class="action-dropdown-item"><i class="' + actionButtons[i].icon + '"></i><span> ' + name + ' </span></md-button>',
                                        overflow: "auto"
                                    };

                                    $scope.toolbarOptions.items.push(item);
                                }

                                if (actionButtons[i].type === 'CallMicroflow' && ModuleService.hasActionButtonDisplayPermission(actionButtons[i], false, $scope.record, $scope.module)) {
                                    var item = {
                                        template: '<md-button ng-show="actionButtons[' + i + '].isShown" class="btn ' + actionButtons[i].color + '"  ng-click="runMicroflow(' + actionButtons[i].microflow_id + ',' + i + ',' + false + ')" aria-label="' + name + '" > <i class="' + actionButtons[i].icon + '"></i> <span>' + name + '</span></md-button>',
                                        overflowTemplate: '<md-button ng-show="actionButtons[' + i + '].isShown"   ng-click="runMicroflow(' + actionButtons[i].microflow_id + ',' + i + ',' + false + ')" class="action-dropdown-item"><i class="' + actionButtons[i].icon + '"></i><span> ' + name + '</span></md-button>',
                                        overflow: "auto"
                                    };

                                    $scope.toolbarOptions.items.push(item);
                                }
                            }
                        }

                        var toolbar = $("#action-buttons-record-area").data("kendoToolBar");
                        if (toolbar) {//for reload buttons
                            $scope.actionButtonsBackup = Object.assign([], $scope.toolbarOptions.items);
                            toolbar.destroy();
                            $scope.toolbarOptions.items = [];
                            $timeout(function () {
                                $scope.toolbarOptions.items = Object.assign([], $scope.actionButtonsBackup)
                            }, 50)
                        }
                    });
            };

            $scope.showModuleFrameModal = function (url) {
                if (new RegExp("https:").test(url)) {
                    var title, w, h;
                    title = 'myPop1';
                    w = document.body.offsetWidth - 200;
                    h = document.body.offsetHeight - 200;
                    var left = (screen.width / 2) - (w / 2);
                    var top = (screen.height / 2) - (h / 2);
                    window.open(url, title, 'toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=no, resizable=no, copyhistory=no, width=' + w + ', height=' + h + ', top=' + top + ', left=' + left);

                } else {
                    $scope.frameUrl = url;
                }

            };

            $scope.openLocationModal = function (filedName) {
                $scope.filedName = filedName;
                const parentEl = angular.element(document.body);
                $mdDialog.show({
                    parent: parentEl,
                    controller: 'locationFormModalController',
                    templateUrl: 'view/app/location/locationFormModal.html',
                    clickOutsideToClose: false,
                    scope: $scope,
                    preserveScope: true

                });
            };

            $scope.removeDocument = function (field) {

                var data = {};
                data["module"] = $scope.module.name;
                data["recordId"] = $scope.record.id;
                data["fieldName"] = field.name;
                data["fileNameExt"] = helper.getFileExtension($scope.record[field.name]);
                data["instanceId"] = $rootScope.workgroup.tenant_id;

                $scope.record[field.name] = null;
                if (field.data_type === 'document') {
                    $scope.uploaderBasic[field.name].queue = [];
                }
                if (field.data_type === 'image') {
                    $scope.croppedImage = $scope.croppedImage || {};
                    $scope['croppedImage'][field.id] = undefined;
                }
                angular.element("input[type='file']").val(null);
            };

            $scope.checkUploadFile = function (event) {
                const files = event.target.files;
                const clickedInputId = event.target.id;
                const inputLabel = angular.element(document.getElementById('lbl_' + clickedInputId))[0];
                if (isAcceptedExtension(files[0])) {
                    inputLabel.innerText = files[0].name;
                }
            };

            $scope.fileLoadingCounter = 0;

            $scope.uploaderBasic = function (field) {

                $scope.isFinishUpload = false;

                $scope.document[field.name] = {};
                const headers = {};

                if ($rootScope.preview) {
                    headers['X-App-Id'] = $rootScope.user.app_id;
                } else {
                    headers['X-Tenant-Id'] = $rootScope.user.tenant_id;
                }

                const uploader_basic = $scope.uploaderBasic[field.name] = new FileUploader({
                    url: 'storage/record_file_upload',
                    headers: headers,
                    queueLimit: 1
                });

                uploader_basic.onAfterAddingFile = function (item) {

                    $scope.fileLoadingCounter++;
                    $scope.record[field.name] = item.uploader.queue[0].file.name;
                    $scope.document[field.name]['Name'] = item.uploader.queue[0].file.name;
                    $scope.document[field.name]['Size'] = item.uploader.queue[0].file.size;
                    $scope.document[field.name]['Type'] = item.uploader.queue[0].file.type;
                    item.upload();
                };

                uploader_basic.onWhenAddingFileFailed = function (item, filter, options) {
                    switch (filter.name) {
                        case 'docFilter':
                            mdToast.warning($filter('translate')('Setup.Settings.DocumentTypeError'));
                            break;
                        case 'sizeFilter':
                            mdToast.warning($filter('translate')('Setup.Settings.SizeError'));
                            break;
                    }
                };

                uploader_basic.filters.push({
                    name: 'docFilter',
                    fn: function (item) {
                        const extension = helper.getFileExtension(item.name);
                        return (extension === 'txt' || extension === 'docx' || extension === 'pdf' || extension === 'doc' || extension === 'xlsx' || extension === 'xls');
                    }
                });

                uploader_basic.filters.push({
                    name: 'sizeFilter',
                    fn: function (item) {
                        return item.size < 5242880;//5 mb
                    }
                });

                uploader_basic.onSuccessItem = function (item, response) {
                    $scope.document[field.name]['UniqueName'] = response.unique_name;
                    $scope.fileLoadingCounter--;
                };

                uploader_basic.onErrorItem = function (item, response) {
                    $scope.document[field.name]['UniqueName'] = response.unique_name;
                    $scope.fileLoadingCounter--;
                };

                $scope.isFinishUpload = true;
                return uploader_basic;

            };

            $scope.getFileDownload = function (fileName, uniqueName) {
                if (uniqueName === undefined) {
                    $window.location.href = 'storage/record_file_download?fileName=' + fileName
                } else {
                    $window.location.href = 'storage/record_file_download?fileName=' + uniqueName
                }
            };

            $scope.uploaderImage = function (field) {
                $scope.image[field.name] = {};
                const headers = {
                    'Authorization': 'Bearer ' + $localStorage.read('access_token'),
                    'Accept': 'application/json' /// we have to set accept header to provide consistency between browsers.
                };

                if ($rootScope.preview) {
                    headers['X-App-Id'] = $rootScope.user.app_id;
                } else {
                    headers['X-Tenant-Id'] = $rootScope.user.tenant_id;
                }

                const uploader_image = $scope.uploaderImage[field.name] = new FileUploader({
                    url: 'storage/record_file_upload',
                    headers: headers,
                    queueLimit: 1
                });

                uploader_image.onAfterAddingFile = function (item) {
                    readFile(item._file)
                        .then(function (image) {
                            $scope.copyImg = image;
                            item.image = image;
                            resizeService.resizeImage(item.image, { width: 1024 }, function (err, resizedImage) {
                                if (err)
                                    return;

                                item._file = dataURItoBlob(resizedImage);
                                item.file.size = item._file.size;
                                $scope.fileLoadingCounter++;
                                $scope.record[field.name] = item.uploader.queue[0].file.name;
                                $scope.image[field.name]['Name'] = item.uploader.queue[0].file.name;
                                $scope.image[field.name]['Size'] = item.uploader.queue[0].file.size;
                                $scope.image[field.name]['Type'] = item.uploader.queue[0].file.type;

                                const itemName = item.uploader.queue[0].file.name.split('.');
                                item.uploader.queue[0].file.name = helper.getSlug(itemName[0]) + "." + itemName[1];

                                item.upload();
                            });
                        });
                };
                uploader_image.onWhenAddingFileFailed = function (item, filter, options) {
                    switch (filter.name) {
                        case 'imgFilter':
                            mdToast.warning($filter('translate')('Setup.Settings.ImageError'));
                            break;
                        case 'sizeFilter':
                            mdToast.warning($filter('translate')('Setup.Settings.SizeError'));
                            break;
                    }
                };
                uploader_image.filters.push({
                    name: 'imgFilter',
                    fn: function (item) {
                        const extension = helper.getFileExtension(item.name);
                        return (extension === 'jpg' || extension === 'jpeg' || extension === 'png' || extension === 'doc' || extension === 'gif');
                    }
                });
                uploader_image.filters.push({
                    name: 'sizeFilter',
                    fn: function (item) {
                        return item.size < 5242880;// 5mb
                    }
                });
                uploader_image.onSuccessItem = function (item, response) {

                    $scope['croppedImage'] = $scope['croppedImage'] || {};
                    $scope['croppedImage'][field.id] = $scope.copyImg;
                    $scope.image[field.name]['UniqueName'] = response.public_url;
                    $scope.fileLoadingCounter--;
                };

                const dataURItoBlob = function (dataURI) {
                    const binary = atob(dataURI.split(',')[1]);
                    const mimeString = dataURI.split(',')[0].split(':')[1].split(';')[0];
                    const array = [];

                    for (var i = 0; i < binary.length; i++) {
                        array.push(binary.charCodeAt(i));
                    }

                    return new Blob([new Uint8Array(array)], { type: mimeString });
                };

                function readFile(file) {
                    const deferred = $q.defer();
                    const reader = new FileReader();

                    reader.onload = function (e) {
                        deferred.resolve(e.target.result);
                    };

                    reader.readAsDataURL(file);

                    return deferred.promise;
                }

                return uploader_image;

            };

            //webhook request func for action button
            $scope.webhookRequest = function (index, isRelated) {
                const action = isRelated ? $scope.relationActionButtons[index] : $scope.actionButtons[index];

                if (!action) {
                    mdToast.warning($filter('translate')('Module.ActionButtonWebhookFail'));
                    return;
                }

                const jsonData = {};
                const headersData = [];
                const params = action.parameters.split(',');
                const headers = action.headers.split(',');
                $scope.webhookRequesting = {};

                $scope.webhookRequesting[action.id] = true;

                for (var i = 0; i < params.length; i++) {
                    const data = params[i];
                    const dataObject = data.split('|');
                    const parameterName = dataObject[0];
                    const moduleName = dataObject[1];
                    const fieldName = dataObject[2];

                    if (moduleName !== $scope.module.name) {
                        if ($scope.record[moduleName])
                            jsonData[parameterName] = $scope.record[moduleName][fieldName];
                        else
                            jsonData[parameterName] = null;

                    } else {
                        if ($scope.record[fieldName])
                            jsonData[parameterName] = $scope.record[fieldName];
                        else
                            jsonData[parameterName] = null;
                    }
                }

                for (var i = 0; i < headers.length; i++) {
                    const data = headers[i];
                    const tempHeader = data.split('|');
                    const type = tempHeader[0];
                    const moduleName = tempHeader[1];
                    const key = tempHeader[2];
                    const value = tempHeader[3];

                    switch (type) {
                        case 'module':
                            const fieldName = value;
                            if (moduleName !== $scope.module.name) {
                                if ($scope.record[moduleName])
                                    headersData[key] = $scope.record[moduleName][fieldName];
                                else
                                    headersData[key] = null;
                            } else {
                                if ($scope.record[fieldName])
                                    headersData[key] = $scope.record[fieldName];
                                else
                                    headersData[key] = null;
                            }
                            break;
                        case 'static':
                            switch (value) {
                                case '{:app:}':
                                    headersData[key] = $rootScope.user.app_id;
                                    break;
                                case '{:tenant:}':
                                    headersData[key] = $rootScope.user.tenant_id;
                                    break;
                                case '{:user:}':
                                    headersData[key] = $rootScope.user.id;
                                    break;
                                default:
                                    headersData[key] = null;
                                    break;
                            }
                            break;
                        case 'custom':
                            headersData[key] = value;
                            break;
                        default:
                            headersData[key] = null;
                            break;
                    }
                }

                if (action.method_type === 'post') {

                    $http.post(action.url, jsonData, { headers: headersData })
                        .then(function () {
                            mdToast.success($filter('translate')('Module.ActionButtonWebhookSuccess'));
                            $scope.webhookRequesting[action.id] = false;
                        })
                        .catch(function () {
                            mdToast.warning($filter('translate')('Module.ActionButtonWebhookSuccess'));
                            $scope.webhookRequesting[action.id] = false;
                        });

                } else if (action.method_type === 'get') {

                    var query = "";

                    for (var key in jsonData) {
                        query += key + "=" + jsonData[key] + "&";
                    }
                    if (query.length > 0) {
                        query = query.substring(0, query.length - 1);
                    }

                    $http.get(action.url + "?" + query)
                        .then(function () {
                            mdToast.success($filter('translate')('Module.ActionButtonWebhookSuccess'));
                            $scope.webhookRequesting[action.id] = false;
                        })
                        .catch(function () {
                            mdToast.warning($filter('translate')('Module.ActionButtonWebhookFail'));
                            $scope.webhookRequesting[action.id] = false;
                        });

                }
            };

            $scope.setDropdownData = function (field) {
                if (field.filters && field.filters.length > 0)
                    $scope.dropdownFieldDatas[field.name] = null;
                else if ($scope.dropdownFieldDatas[field.name] && $scope.dropdownFieldDatas[field.name].length > 0)
                    return;

                $scope.currentLookupField = field;
                $scope.lookup()
                    .then(function (response) {
                        $scope.dropdownFieldDatas[field.name] = response;
                    });

            };

            $scope.getAttachments = function () {
                if (!helper.hasDocumentsPermission($scope.operations.read))
                    return;

                DocumentService.getEntityDocuments($rootScope.workgroup.tenant_id, $scope.id, $scope.module.id)
                    .then(function (response) {
                        $rootScope.processLanguages(response.data.documents);
                        $scope.documentsResultSet = DocumentService.processDocuments(response.data, $rootScope.users);
                        $scope.documents = $scope.documentsResultSet.documentList;
                        $scope.loadingDocuments = false;
                    });
            };

            //Şimdilik kullanılmadığı için kapatıldı ileri dönük ihtiyaç olursa açılabilir.
            //$scope.getAttachments();

            components.run('AfterFormLoaded', 'script', $scope);

            $scope.changeTab = function (relatedModule) {
                $scope.isActive = [];
                $scope.isActive[relatedModule.id] = true;
                if (!$scope.relationsGridOptions[relatedModule.id]) {
                    $scope.getRelatedActionButtons(relatedModule);
                    $scope.generateRelationsTable(relatedModule);
                }


            };

            $scope.findRequest = [];

            $scope.generateRelationsTable = function (relatedModule) {

                const relatedModuleViewFields = [];
                var filterRequest = {};
                var dataItemId = 'dataItem.id';

                if (relatedModule.relation_type === 'many_to_many') {
                    $scope.forManyToManyFielName = $scope.module.name !== relatedModule.related_module ? relatedModule.related_module + '_id' : relatedModule.related_module + '1_id';
                    dataItemId = 'dataItem[\'' + $scope.forManyToManyFielName + '\']';
                    filterRequest = {
                        module: relatedModule.related_module,
                        "convert": true,
                        filters: [{
                            field: $scope.module.name !== relatedModule.related_module ? $scope.module.name + '_id' : $scope.module.name + '1_id',
                            operator: 'equals',
                            value: $scope.id,
                            no: 1
                        }],
                        many_to_many: $scope.module.name,
                        many_to_many_table_name: relatedModule.many_to_many_table_name
                    };
                } else if (relatedModule.relation_type === 'related_to') {
                    filterRequest = {
                        module: relatedModule.related_module,
                        "convert": true,
                        filters: [{
                            field: 'related_to',
                            operator: 'equals',
                            value: $rootScope.getLanguageValue($scope.module.languages, 'label', 'singular'),
                            no: 1
                        }],
                        many_to_many: $scope.module.name,
                        many_to_many_table_name: relatedModule.many_to_many_table_name
                    };
                } else
                    filterRequest = {
                        module: relatedModule.related_module,
                        "convert": true,
                        filters: [{
                            field: relatedModule.relation_field,
                            operator: 'equals',
                            value: $scope.id,
                            no: 1
                        }]
                    };

                for (var i = 0; i < relatedModule.display_fields.length; i++) {
                    const item = relatedModule.display_fields[i];
                    var field = undefined;
                    if (item.indexOf('.') > -1) {
                        const fieldArray = item.split('.');
                        var lookupModule = {};
                        switch (fieldArray[1]) {
                            case 'users':
                                lookupModule = AppService.getUserModule();
                                break;
                            case 'profiles':
                                lookupModule = AppService.getProfileModule();
                                break;
                            case 'roles':
                                lookupModule = AppService.getRoleModule();
                                break;
                            default:
                                lookupModule = $scope.modulus[fieldArray[1]];
                        }
                        field = Object.assign({}, $filter('filter')(lookupModule.fields, { name: fieldArray[2] }, true)[0]);
                        var ext = $filter('filter')($scope.modulus[relatedModule.related_module].fields, { name: fieldArray[0] }, true)[0];
                        field.labelExt = '(' + $rootScope.getLanguageValue(ext.languages, 'label') + ')';
                        field.label = field.label || $rootScope.getLanguageValue(field.languages, 'label');
                        field.name = item;
                    } else {
                        field = Object.assign({}, $filter('filter')($scope.modulus[relatedModule.related_module].fields, { name: item }, true)[0]);
                    }

                    if (!field || !ModuleService.hasFieldDisplayPermission(field) || !field.display_detail)
                        continue;

                    relatedModuleViewFields.push(field);
                }

                const tableConfig = ModuleService.generatRowtmpl(relatedModuleViewFields, true, {
                    module: $scope.module,
                    relatedModule: relatedModule
                });

                const locale = $scope.locale || $scope.language;

                filterRequest['fields'] = tableConfig.requestFields;
                $scope.findRequest[relatedModule.id] = filterRequest;
                $scope.relatedModule = relatedModule;

                $scope.relationsGridOptions[relatedModule.id] = {
                    dataSource: {
                        serverPaging: true,
                        serverFiltering: true,
                        serverSorting: true,
                        transport: {
                            read: function (options) {
                                $.ajax({
                                    url: '/api/record/find_custom?locale=' + locale,
                                    contentType: 'application/json',
                                    dataType: 'json',
                                    type: 'POST',
                                    data: JSON.stringify(Object.assign($scope.findRequest[relatedModule.id], options.data)),
                                    success: function (result) {
                                        options.success(result);
                                        //////$scope.relatedModuleDatas[relatedModule.id] = result.data;
                                        relatedModule.total = result.total;
                                        components.run('SubListLoaded', 'Script', $scope, $scope.record, undefined, relatedModule.related_module);
                                    },
                                    beforeSend: $rootScope.beforeSend()
                                })
                            }
                        },
                        schema: {
                            data: "data",
                            total: "total",
                            model: { id: "id" }
                        }
                    },
                    rowTemplate: '<tr ng-click="goUrl2(' + dataItemId + ',\'' + relatedModule.related_module + '\')">' + tableConfig.rowtempl + '</tr>',
                    altRowTemplate: '<tr class="k-alt" ng-click="goUrl2(' + dataItemId + ',\'' + relatedModule.related_module + '\')">' + tableConfig.rowtempl + '</tr>',
                    sortable: true,
                    noRecords: true,
                    pageable: {
                        refresh: true,
                        pageSize: 10,
                        pageSizes: [10, 25, 50, 100, 500],
                        buttonCount: 5,
                        info: true,
                    },
                    columns: tableConfig.columns,
                }

            };

            $scope.copyHref = function () {
                $window.location.href = '#/app/record/' + $scope.type + '?clone=true&id=' + $scope.record.id + ($scope.parentId ? ('&ptype=' + $scope.parentType + '&pid=' + $scope.parentId) : '');
            };

            $scope.delete = function () {
                $scope.executeCode = false;
                components.run('BeforeDelete', 'Script', $scope, $scope.record);

                if ($scope.executeCode)
                    return;

                ModuleService.deleteRecord($scope.module.name, $scope.record.id)
                    .then(function () {
                        mdToast.success($filter('translate')('Module.SuccessDeleteRecordMessage'));
                        components.run('AfterDelete', 'Script', $scope, $scope.record);
                        clearCache();

                        if ($scope.parentId) {
                            $state.go('app.moduleDetail', { type: $scope.parentModule, id: $scope.parentId });
                            return;
                        }

                        $state.go('app.moduleList', { type: $scope.type });
                    });
            };

            $scope.showConfirm = function (ev) {
                const confirm = $mdDialog.confirm()
                    .title($filter('translate')('Common.AreYouSure'))
                    //.textContent('')
                    //.ariaLabel('')
                    .targetEvent(ev)
                    .ok($filter('translate')('Common.Yes'))
                    .cancel($filter('translate')('Common.No'));

                $mdDialog.show(confirm).then(function () {
                    $scope.delete();
                }, function () {
                    //cancel
                });
            };

            const clearCache = function () {
                var cacheKey = $scope.module.name + '_' + $scope.module.name;

                if (!$scope.parentId) {
                    $cache.remove(cacheKey);

                    if ($scope.module.name === 'opportunities')
                        $cache.remove('opportunity' + $scope.id + '_stage_history');
                } else {
                    cacheKey = (!$scope.relatedToField ? $scope.parentType : 'related_to') + $scope.parentId + '_' + $scope.module.name;
                    const parentCacheKey = $scope.parentType + '_' + $scope.parentType;
                    $cache.remove(cacheKey);
                    $cache.remove(parentCacheKey);
                }

                if ($rootScope.activePages && $rootScope.activePages[$scope.module.name])
                    $rootScope.activePages[$scope.module.name] = null;


            };

            $scope.openExportDialog = function () {
                $scope.pdfCreating = true;
                $scope.loadingModal = true;

                var openPdfModal = function () {
                    const parentEl = angular.element(document.body);
                    $mdDialog.show({
                        parent: parentEl,
                        templateUrl: 'view/app/module/modulePdfModal.html',
                        clickOutsideToClose: false,
                        scope: $scope,
                        preserveScope: true
                    });
                };

                if ($scope.quoteTemplates) {
                    $scope.quoteTemplate = $scope.quoteTemplates[0];
                    $scope.loadingModal = false;
                    openPdfModal();
                } else
                    ModuleService.getTemplates($scope.module.name, 'module')
                        .then(function (templateResponse) {
                            if (templateResponse.data.length === 0) {

                                mdToast.warning($filter('translate')('Setup.Templates.TemplateNotFound'));
                                $scope.pdfCreating = false;
                            } else {
                                const templateWord = templateResponse.data;
                                $scope.quoteTemplates = $filter('filter')(templateWord, { active: true }, true);

                                $scope.quoteTemplatesOptions = {
                                    dataSource: $scope.quoteTemplates,
                                    dataTextField: "name",
                                    dataValueField: "id"
                                };
                                $scope.quoteTemplate = $scope.quoteTemplates[0];
                                $scope.loadingModal = false;
                                openPdfModal();
                            }
                        })
                        .catch(function () {
                            $scope.loadingModal = false;
                            $scope.pdfCreating = false;
                        });
            };

            $scope.showSingleSMSModal = function () {
                if (!$rootScope.system.messaging.SMS) {
                    mdToast.warning($filter('translate')('SMS.NoProvider'));
                    return;
                }

                const parentEl = angular.element(document.body);
                $mdDialog.show({
                    parent: parentEl,
                    templateUrl: 'view/app/sms/singleSMSModal.html',
                    clickOutsideToClose: true,
                    scope: $scope,
                    preserveScope: true

                });
            };

            $scope.showQuoteEMailModal = function () {

                if (!$rootScope.system.messaging.SystemEMail && !$rootScope.system.messaging.PersonalEMail) {
                    mdToast.warning($filter('translate')('EMail.NoProvider'));
                    return;
                }

                const parentEl = angular.element(document.body);
                $mdDialog.show({
                    parent: parentEl,
                    templateUrl: 'view/app/email/singleEmailModal.html',
                    clickOutsideToClose: true,
                    scope: $scope,
                    preserveScope: true

                });
            };

            $scope.getDownloadUrl = function (format) {
                $scope.isDownLoad = true;
                $window.open("/attach/export?module=" + $scope.type + "&id=" + $scope.id + "&templateId=" + $scope.quoteTemplate.id + "&access_token=" + $localStorage.read('access_token') + '&format=' + format + '&locale=' + $rootScope.locale + '&timezoneOffset=' + new Date().getTimezoneOffset() * -1, "_blank");
                mdToast.success($filter('translate')('Module.DownloadPdfWordMessage', { value: format === 'pdf' ? 'Pdf' : 'Word' }));
                $scope.isDownLoad = false;
            };

            $scope.close = function () {
                $mdDialog.hide();
            };

            $scope.setId = function (field, isPicklistId, isShareField) {
                $scope.field = field;
                if (!isShareField) {
                    $scope.optionId = isPicklistId ? field.picklist_id : field.id;
                }

            };

            $scope.openCalendar = function (field) {
                var data = undefined;
                switch (field.data_type) {
                    case 'date':
                        data = 'kendoDatePicker';
                        break;
                    case 'date_time':
                        data = 'kendoDateTimePicker';
                        break;
                    case 'time':
                        data = 'kendoTimePicker';
                        break;
                }

                if (data) {
                    const dateInstance = angular.element(document.getElementById(field.name)).data(data);
                    if (dateInstance) {
                        dateInstance.open();
                    }
                }
            };

            $scope.documentDownload = function (fileName, fieldName) {
                $window.location = "storage/record_file_download?fileName=" + fileName;
            };

            //ortak katmana yapılacak
            const prepareLookupSearchField = function () {
                if ($scope.currentLookupField.lookup_type === 'users' && !$scope.currentLookupField.lookupModulePrimaryField) {
                    const userModulePrimaryField = {};
                    userModulePrimaryField.data_type = 'text_single';
                    userModulePrimaryField.name = 'full_name';
                    $scope.currentLookupField.lookupModulePrimaryField = userModulePrimaryField;
                }

                if ($scope.currentLookupField.lookup_type === 'profiles' && !$scope.currentLookupField.lookupModulePrimaryField) {
                    const userModulePrimaryField = {};
                    userModulePrimaryField.data_type = 'text_single';
                    userModulePrimaryField.name = 'name';
                    $scope.currentLookupField.lookupModulePrimaryField = userModulePrimaryField;
                }

                if ($scope.currentLookupField.lookup_type === 'roles' && !$scope.currentLookupField.lookupModulePrimaryField) {
                    const userModulePrimaryField = {};
                    userModulePrimaryField.data_type = 'text_single';
                    userModulePrimaryField.name = 'label_' + $rootScope.user.tenantLanguage;
                    $scope.currentLookupField.lookupModulePrimaryField = userModulePrimaryField;
                }

                if ($scope.currentLookupField.lookup_type === 'relation') {
                    if (!$scope.record.related_module) {
                        $scope.$broadcast('angucomplete-alt:clearInput', $scope.currentLookupField.name);
                        return $q.defer().promise;
                    }

                    const relationModule = $filter('filter')($rootScope.modules, { name: $scope.record.related_module.value }, true)[0];

                    if (!relationModule) {
                        $scope.$broadcast('angucomplete-alt:clearInput', $scope.currentLookupField.name);
                        return $q.defer().promise;
                    }

                    $scope.currentLookupField.lookupModulePrimaryField = $filter('filter')(relationModule.fields, { primary: true }, true)[0];
                }
                components.run('BeforeLookup', 'Script', $scope);
            };

            $scope.closeLightBox = function () {
                $mdDialog.hide();
            };

            $scope.showLightBox = function (ev, record, isImage, fieldName, relation) {

                if (!record)
                    return;

                $scope.showImageData = {};
                $scope.multiSelectAndTagDatas = {};
                const module = relation ? $rootScope.modulus[relation.related_module] : $scope.module;
                var field = fieldName ? $filter('filter')(module.fields, {
                    name: fieldName,
                    deleted: false
                })[0] : $scope.primaryField.name;

                //location & image srcs
                if (isImage) {
                    $scope.showImageData = {
                        url: ev.target.src,
                        title: record[field] || $rootScope.getLanguageValue(field.languages, 'label'),
                        type: fieldName ? 'location' : 'image',
                        map_url: "http://www.google.com/maps/place/" + record[field.name]
                    };
                } else if (record[fieldName]) {
                    field = $filter('filter')(module.fields, { name: fieldName, deleted: false })[0];
                    $scope.multiSelectAndTagDatas = {
                        array: record[fieldName].split(';'),
                        title: record[field] || $rootScope.getLanguageValue(field.languages, 'label')
                    };
                }

                $mdDialog.show({
                    contentElement: '#mdLightbox',
                    parent: angular.element(document.body),
                    targetEvent: ev,
                    clickOutsideToClose: true,
                    fullscreen: false
                });

            };

            $scope.formType = null;

            $scope.fastNewRecordModal = function (moduleName, fastAddRecord, lookupValue, lookupName, id) {
                $scope.lookupInput.close();
                $rootScope.fastRecordAddModal(moduleName, fastAddRecord, lookupValue, lookupName, id, $scope);
            };

            const setSharedLookups = function () {
                /**When form was loading, we have to create set shared_read && shared_edit lookups*/
                for (var i = 0; i < 2; i++) {
                    var sharedFields = ['shared_read', 'shared_edit'];
                    $scope['lookupOptions_' + sharedFields[i]] = {
                        dataSource: new kendo.data.DataSource({
                            transport: {
                                read: function (o) {
                                    o.success($scope.record[$scope.field] ? $scope.record[$scope.field] : [])
                                }
                            }
                        }),
                        dataTextField: "name",
                        dataValueField: "id",
                        autoBind: false,
                        filtering: function (e) {
                            if (e.filter) {
                                $scope.lookupUserAndGroup($scope.module.id, false, e.filter.value).then(function onSuccess(res) {

                                    if (res && res.length > 0) {
                                        const dataSource = $scope['lookupOptions_' + $scope.field].dataSource;
                                        dataSource._data = [];
                                        for (var i = 0; i < res.length; i++) {
                                            const isExist = dataSource._data.length > 0 ? $filter('filter')(dataSource._data, { id: res[i].id }, true)[0] : false;
                                            if (!isExist)
                                                dataSource.add(res[i]);
                                        }
                                    }
                                });
                            }
                        }
                    };
                }
            };

            function prepareRelations(response, relation) {
                const data = response.data;

                if (data[0] && data[0].total_count)
                    relation.total = data[0].total_count;
                else {
                    delete relation.total;
                }

                if (relation.detail_view_type === 'flat' || $scope.module.detail_view_type === 'flat') {
                    if ($scope.returnParentType === relation.id) {
                        $scope.activeType = 'tab';
                        $scope.tab = 'general';
                    }
                }

                if ($scope.returnTab == relation.id) {
                    $scope.activeType = 'tab';
                    $scope.tab = relation.id.toString();
                    $scope.changeTab(relation);
                }
            }

            $scope.goToRecord = function (item, lookupType, showAnchor, dataItem) {
                ModuleService.goToRecord(item, lookupType, showAnchor, dataItem);
            };


            $scope.getTagAndMultiDatas = function (dataItem, stringList) {
                if (stringList) {
                    return ModuleService.getTagAndMultiDatas(dataItem, stringList);
                }
            };

            $scope.getImageStyle = function (fieldName, relation) {
                if (fieldName && relation) {
                    return ModuleService.getImageStyle(fieldName, relation.related_module);
                }
            };

            $scope.getRatingCount = function (fieldName, relation) {
                if (fieldName && relation) {
                    return ModuleService.getRatingCount(fieldName, relation.related_module);
                }
            };

            $scope.getLocationUrl = function (coordinates) {
                if (coordinates)
                    return ModuleService.getLocationUrl(coordinates);
            };

            function addManyToManyRelationRecord(id) {

                const relations = [];
                const relation = {};

                if ($scope.type !== $scope.manyToManyRelation.related_module) {
                    relation[$scope.type + '_id'] = id;
                    relation[$scope.manyToManyRelation.related_module + '_id'] = $location.search().pid;
                } else {
                    relation[$scope.type + '1_id'] = id;
                    relation[$scope.manyToManyRelation.related_module + '2_id'] = $location.search().pid;
                }

                relations.push(relation);
                const data = {
                    records: relations
                };

                data.tableName = $scope.manyToManyRelation.many_to_many_table_name;

                return data;
            }

            $scope.deleteRelation = function (ev, relatedModule, isSingle, id) {

                const confirm = $mdDialog.confirm()
                    .title($filter('translate')('Common.AreYouSure'))
                    .targetEvent(ev)
                    .ok($filter('translate')('Common.Yes'))
                    .cancel($filter('translate')('Common.No'));

                $mdDialog.show(confirm).then(function () {
                    const relations = prepareManyToManyDeleteDatas(relatedModule.related_module, isSingle, id);
                    var data = undefined;
                    if (relations && relations.length > 0) {
                        if (relatedModule.many_to_many_table_name)
                            data = { records: relations, tableName: relatedModule.many_to_many_table_name };
                        else
                            data = relations;

                        ModuleService.deleteRelation($scope.module.name, relatedModule.related_module, data)
                            .then(function () {
                                mdToast.success($filter('translate')('Module.DeleteMessage'));
                                $scope.grid[relatedModule.related_module].dataSource.read();
                                relatedModule.total = relatedModule.total - relations.length;
                            });
                    }
                }, function () {
                    //Cancel
                });
            };

            function prepareManyToManyDeleteDatas(relatedModule, isSingle, id) {
                const relations = [];
                const relation = {};

                if (isSingle) {
                    relation[$scope.module.name + '_id'] = parseInt($scope.id);
                    relation[relatedModule + '_id'] = id;
                    relations.push(relation);
                } else {
                    for (var m = 0; m < $scope.selectedRecords[relatedModule].length; m++) {
                        relation[$scope.module.name + '_id'] = $scope.id;
                        relation[relatedModule + '_id'] = $scope.selectedRecords[relatedModule][m];
                        relations.push(Object.assign({}, relation));
                    }
                    $scope.selectedRecords[relatedModule] = [];
                    $scope.isAllSelected[relatedModule] = false;
                }

                return relations;
            }

            $scope.toolbarRelationOptions = [];

            $scope.getRelatedActionButtons = function (relation) {
                $scope.relationActionButtons = [];
                if ($rootScope.modulus[relation.related_module]) {
                    ModuleService.getActionButtons($rootScope.modulus[relation.related_module].id).then(function (actionButtonsResponse) {
                        if (actionButtonsResponse.length > 0) {
                            var relationActionButtons = $filter('filter')(actionButtonsResponse, function (actionButton) {
                                return actionButton.trigger !== 'Detail' && actionButton.trigger !== 'Form' && actionButton.trigger !== 'List';
                            }, true);

                            $rootScope.processLanguages(relationActionButtons);

                            $scope.relationActionButtons['relation' + relation.id] = relationActionButtons;
                            if (relationActionButtons) {
                                for (var j = 0; j < relationActionButtons.length; j++) {
                                    var relationActionButton = relationActionButtons[j];
                                    relationActionButton.module_name = relation.related_module;
                                    actionButtonIsShown(relationActionButton);

                                    const name = $rootScope.getLanguageValue(relationActionButton.languages, 'label');

                                    if (relationActionButtons[j].type === 'Scripting' && ModuleService.hasActionButtonDisplayPermission(relationActionButtons[j], false, $scope.record, $scope.module)) {
                                        var relationKey = 'relation' + relation.id;
                                        var item = {
                                            template: '<md-button ng-show="relationActionButtons[\'' + relationKey + '\'][' + j + '].isShown" class="btn ' + relationActionButtons[j].color + '"  ng-click="runScript(' + j + ',\'' + relationKey + '\')" aria-label="' + name + '" > <i class="' + relationActionButtons[j].icon + '"></i> <span>' + name + '</span></md-button>',
                                            overflowTemplate: '<md-button ng-show="relationActionButtons[\'' + relationKey + '\'][' + j + '].isShown"  ng-click="runScript(' + j + ',\'' + relationKey + '\')"  class="action-dropdown-item"><i class="' + relationActionButtons[j].icon + '"></i><span> ' + name + '</span></md-button>',
                                            overflow: "auto"
                                        };

                                        $scope.toolbarRelationOptions[relationKey].items.push(item);
                                    }

                                    if (relationActionButtons[j].type === 'Webhook' && ModuleService.hasActionButtonDisplayPermission(relationActionButtons[j], false, $scope.record, $scope.module)) {
                                        var item = {
                                            template: '<md-button ng-show="relationActionButtons[\'' + relationKey + '\'][' + j + '].isShown" class="btn ' + relationActionButtons[j].color + '"  ng-click="webhookRequest(' + j + ', true)"   aria-label="' + name + '" > <i class="' + relationActionButtons[j].icon + '"></i> <span>' + name + '</span></md-button>',
                                            overflowTemplate: '<md-button ng-show="relationActionButtons[\'' + relationKey + '\'][' + j + '].isShown" ng-click="webhookRequest(' + j + ')"  class="action-dropdown-item "><i class="' + relationActionButtons[j].icon + '"></i><span> ' + name + ' </span></md-button>',
                                            overflow: "auto"
                                        };

                                        $scope.toolbarRelationOptions['relation' + relation.id].items.push(item);
                                    }

                                    if (relationActionButtons[j].type === 'ModalFrame' && ModuleService.hasActionButtonDisplayPermission(relationActionButtons[j], false, $scope.record, $scope.module)) {
                                        var item = {
                                            template: '<md-button ng-show="relationActionButtons[\'' + relationKey + '\'][' + j + '].isShown" class="btn ' + relationActionButtons[j].color + '"  ng-click="showModuleFrameModal(\'' + relationActionButtons[j].url + '\')"   aria-label="' + name + '" > <i class="' + relationActionButtons[j].icon + '"></i> <span>' + name + '</span></md-button>',
                                            overflowTemplate: '<md-button ng-show="relationActionButtons[\'' + relationKey + '\'][' + j + '].isShown"  ng-click="showModuleFrameModal(\'' + relationActionButtons[j].url + '\')"   class="action-dropdown-item"><i class="' + relationActionButtons[j].icon + '"></i><span> ' + name + ' </span></md-button>',
                                            overflow: "auto"
                                        };

                                        $scope.toolbarRelationOptions['relation' + relation.id].items.push(item);
                                    }

                                    if (relationActionButtons[j].type === 'CallMicroflow' && ModuleService.hasActionButtonDisplayPermission(relationActionButtons[j], false, $scope.record, $scope.module)) {
                                        var item = {
                                            template: '<md-button ng-show="relationActionButtons[\'' + relationKey + '\'][' + j + '].isShown" class="btn ' + relationActionButtons[j].color + '"  ng-click="runMicroflow(' + relationActionButtons[j].microflow_id + ',' + j + ',' + true + ')"   aria-label="' + name + '" > <i class="' + relationActionButtons[j].icon + '"></i> <span>' + name + '</span></md-button>',
                                            overflowTemplate: '<md-button ng-show="relationActionButtons[\'' + relationKey + '\'][' + j + '].isShown"  ng-click="runMicroflow(' + relationActionButtons[j].microflow_id + ',' + j + ',' + true + ')"  class="action-dropdown-item"><i class="' + relationActionButtons[j].icon + '"></i><span> ' + name + '</span></md-button>',
                                            overflow: "auto"
                                        };

                                        $scope.toolbarRelationOptions['relation' + relation.id].items.push(item);
                                    }

                                }
                            }
                        }

                    })
                }
            };

            $scope.runMicroflow = function (workflowId, index, isRelation) {
                var button = null;

                if (isRelation)
                    button = $scope.relationActionButtons[index];
                else
                    button = $scope.actionButtons[index];

                $scope.actionButtonsData = {
                    data: {
                        "module_id": $scope.module.id
                    },
                    "workflow_id": workflowId,
                    "button": button
                };

                //action button uzerinde eger bir record name setliyse bilgileri setliyoruz.
                //record'suz bir sekilde manual flowu calistirabilmesi icin bu kontrolu yapiyoruz.
                if (button.record_name) {
                    $scope.actionButtonsData.data.record_name = button.record_name;
                    $scope.actionButtonsData.data.record_ids = [parseInt($scope.id)];
                }

                $scope.buttonsParametersData = {};

                if (button.parameters) {
                    $scope.showMicroflowParameters = true;
                    $scope.showScriptParameters = false;
                    $scope.buttonParameterNameTitle = $rootScope.getLanguageValue(button.languages, "label");

                    $scope.buttonsParameters = JSON.parse(button.parameters);

                    $scope.picklistItems = {};

                    angular.forEach($scope.buttonsParameters, function (parameter) {
                        if (parameter.type === 3 || parameter.type === 4) {
                            $http.get(config.apiUrl + 'picklist/get/' + parameter.lowerType)
                                .then(function (response) {
                                    $scope.picklistItems[parameter.key] = response.data;
                                    $rootScope.processLanguage($scope.picklistItems)
                                });
                        }
                    });

                    var parentEl = angular.element(document.body);
                    $mdDialog.show({
                        parent: parentEl,
                        templateUrl: 'view/app/module/actionButtonsParameterModal.html',
                        clickOutsideToClose: true,
                        //targetEvent: ev,
                        scope: $scope,
                        preserveScope: true
                    });

                } else {
                    ModuleService.runMicroflow(workflowId, $scope.actionButtonsData.data)
                        .then(function (res) {
                            var buttonMsg = $rootScope.getLanguageValue(button.languages, 'message');
                            if (res.status === 200 && button.message_type === 'popup' && buttonMsg) {
                                mdToast.success(buttonMsg);
                            }
                        })
                }
            };

            $scope.runMicroflowParameters = function (form) {
                if (!form.validate())
                    return;

                $scope.actionButtonsData.data.parameters = $scope.buttonsParametersData;

                ModuleService.runMicroflow($scope.actionButtonsData.workflow_id, $scope.actionButtonsData.data)
                    .then(function (res) {
                        var buttonMsg = $rootScope.getLanguageValue($scope.actionButtonsData.button.languages, 'message');
                        if (res.status === 200 && $scope.actionButtonsData.button.message_type === 'popup' && buttonMsg) {
                            mdToast.success(buttonMsg);
                        }
                    })

                $scope.closeLightBox();
            };

            $scope['numberOptionsButtons'] = {
                format: "{0:n0}",
                decimals: 0,
            };

            $scope.getModuleFields = function () {
                return $filter('filter')($scope.module.fields, function (field) {
                    return field.display_detail && ModuleService.hasFieldDisplayPermission(field);
                }, true);
            };

            $scope.openAddModal = function (ev, relateModule) {

                $scope.currentRelateModule = relateModule;
                $scope.manyToManyModule = $scope.modulus[relateModule.related_module];
                $scope.manyToManyModule.gridName = $scope.manyToManyModule.name + "-manytomany";
                $scope.manyToManyModule.related_module = $scope.manyToManyModule.name;
                $scope.manyToManyModule.tableName = relateModule.many_to_many_table_name;
                $scope.selectedRecords[$scope.manyToManyModule.gridName] = [];

                const ViewFields = [];
                ViewFields.push($scope.manyToManyModule.primaryField);

                const tableConfig = ModuleService.generatRowtmpl(ViewFields, true, {
                    relatedModule: $scope.manyToManyModule,
                    moduleName: $scope.manyToManyModule.name,
                    tableOptinosMenuHide: true
                });

                tableConfig.rowtempl = tableConfig.rowtempl.replaceAll($scope.manyToManyModule.name, $scope.manyToManyModule.gridName);
                tableConfig.rowtempl = tableConfig.rowtempl.replaceAll('id="{{dataItem.id}}"', 'id="' + $scope.manyToManyModule.gridName + '{{dataItem.id}}"');
                tableConfig.rowtempl = tableConfig.rowtempl.replaceAll('for="{{dataItem.id}}"', 'for="' + $scope.manyToManyModule.gridName + '{{dataItem.id}}"');
                $scope.findRequest[$scope.relatedModule.id].grid_fields = tableConfig.requestFields;

                const locale = $scope.locale || $scope.language;
                $scope.manyToManyGridOptions = {
                    dataSource: {
                        serverPaging: true,
                        serverFiltering: true,
                        serverSorting: true,
                        transport: {
                            read: function (options) {
                                $.ajax({
                                    url: '/api/record/get_relation_data',
                                    contentType: 'application/json',
                                    dataType: 'json',
                                    type: 'POST',
                                    data: JSON.stringify(Object.assign($scope.findRequest[$scope.relatedModule.id], options.data)),
                                    success: function (result) {
                                        options.success(result);
                                    },
                                    beforeSend: $rootScope.beforeSend()
                                })
                            }
                        },
                        schema: {
                            data: "data",
                            total: "total",
                            model: { id: "id" }
                        }
                    },
                    rowTemplate: '<tr>' + tableConfig.rowtempl + '</tr>',
                    altRowTemplate: '<tr class="k-alt">' + tableConfig.rowtempl + '</tr>',
                    sortable: true,
                    noRecords: true,
                    pageable: {
                        refresh: true,
                        pageSize: 10,
                        pageSizes: [10, 25, 50, 100, 500],
                        buttonCount: 5,
                        info: true,
                    },
                    columns: [
                        {
                            width: "60px",
                            headerTemplate: '<input type="checkbox" id="header-chb-' + $scope.manyToManyModule.gridName + '\'" ng-model="isAllSelected[\'' + $scope.manyToManyModule.gridName + '\']" ng-click="selectAll($event,\'' + $scope.manyToManyModule.gridName + '\')" id="header-chb-manytomany" class="k-checkbox header-checkbox"><label class="k-checkbox-label" for="header-chb-' + $scope.manyToManyModule.gridName + '\'"></label>'
                        },
                        {
                            field: $scope.manyToManyModule.primaryField.name,
                            media: "(min-width: 575px)",
                            title: $rootScope.getLanguageValue($scope.manyToManyModule.primaryField.languages, 'label')
                        },
                        {
                            title: "Items",
                            media: "(max-width: 575px)"
                        }

                    ]
                };

                const parentEl = angular.element(document.body);
                $mdDialog.show({
                    parent: parentEl,
                    templateUrl: 'view/app/module/moduleAddModal.html',
                    clickOutsideToClose: true,
                    targetEvent: ev,
                    scope: $scope,
                    preserveScope: true
                });
                $scope.isAllSelected[$scope.manyToManyModule.gridName] = false;
            };

            $scope.addRecords = function () {
                const selectedRowsManytoMany = $scope.selectedRecords[$scope.manyToManyModule.gridName];

                if (!selectedRowsManytoMany || !selectedRowsManytoMany.length) {
                    mdToast.warning($filter('translate')('Module.NoRecordSelected'));
                    return;
                }

                $scope.recordsAdding = true;
                const relationRecords = [];
                for (var i = 0; i < selectedRowsManytoMany.length; i++) {
                    var relationRecord = {};
                    relationRecord[$scope.module.name + "_id"] = $scope.id;
                    relationRecord[$scope.manyToManyModule.name + "_id"] = selectedRowsManytoMany[i];
                    relationRecords.push(relationRecord);
                }

                const data = {
                    tableName: $scope.manyToManyModule.tableName,
                    records: relationRecords
                };

                ModuleService.addRelations($scope.module.name, $scope.manyToManyModule.name, data)
                    .then(function (count) {
                        $scope.recordsAdding = false;
                        $scope.refreshGrid($scope.manyToManyModule.name);
                        $scope.closeLightBox()
                    });
            };

            $scope.controlRemove = function (dataItem, relation) {
                return dataItem.showRemove = $scope.hasPermission(relation ? relation.related_module : $scope.module.name, operations.remove);
            };

            $scope.controlCopy = function (dataItem, relation) {
                return dataItem.showCopy = $scope.hasPermission(relation ? relation.related_module : $scope.module.name, operations.write);
            };

            $scope.controlEdit = function (dataItem, relation) {
                return dataItem.showEdit = $scope.hasPermission(relation ? relation.related_module : $scope.module.name, operations.modify);
            };

            angular.element(document).ready(function () {
                $scope.fileNotSelected = false;
                $scope.moduleForm.options.rules['range'] = ModuleService.rangeRuleForForms();
                $timeout(function () {

                }, 500);
            });

            function setMinMaxValueForField(field) {
                field.validation.min = field.validation.min || Number.MIN_SAFE_INTEGER;
                field.validation.max = field.validation.max || Number.MAX_SAFE_INTEGER;
                return field;
            }

            $scope.showAddNewTagButton = function (fieldName) {
                if (fieldName) {
                    const tagInstance = angular.element(document.getElementById(fieldName)).data("kendoMultiSelect");
                    if (tagInstance)
                        return tagInstance._prev !== "";
                }
                return false;
            };

            $scope.runScript = function (index, relationName) {
                var action = null;
                if (relationName) {
                    action = $scope.relationActionButtons[relationName][index];
                } else {
                    action = $scope.actionButtons[index];
                }

                $scope.buttonsParametersData = {}
                $scope.actionButtonsData = {
                    data: {
                        "record_ids": [parseInt($scope.id)],
                        "module_id": $scope.module.id,
                        "record_name": action.record_name
                    },
                    "button": action
                }


                if (action.parameters && action.type === 'CallMicroflow') {
                    $scope.showMicroflowParameters = false;
                    $scope.showScriptParameters = true;
                    $scope.buttonParameterNameTitle = $rootScope.getLanguageValue(action.languages, "label");

                    $scope.buttonsParameters = JSON.parse(action.parameters);

                    $scope.picklistItems = {};
                    angular.forEach($scope.buttonsParameters, function (parameter) {
                        if (parameter.type === 3 || parameter.type === 4) {
                            $http.get(config.apiUrl + 'picklist/get/' + parameter.lowerType)
                                .then(function (response) {
                                    $scope.picklistItems[parameter.key] = response.data;
                                    $rootScope.processLanguage($scope.picklistItems)
                                });
                        }
                    });

                    var parentEl = angular.element(document.body);
                    $mdDialog.show({
                        parent: parentEl,
                        templateUrl: 'view/app/module/actionButtonsParameterModal.html',
                        clickOutsideToClose: true,
                        //targetEvent: ev,
                        scope: $scope,
                        preserveScope: true
                    });

                } else {
                    customScripting.run($scope, action.template);

                    var btnMsg = $rootScope.getLanguageValue(action.languages, 'message')
                    if (action.message_type === 'popup' && btnMsg) {
                        mdToast.success(btnMsg);
                    }
                }
            };

            $scope.runScriptParameters = function (form) {
                if (!form.validate())
                    return;
                $scope.actionButtonsData.data.parameters = $scope.buttonsParametersData;

                customScripting.run($scope, $scope.actionButtonsData.button.template);

                var btnMsg = $rootScope.getLanguageValue($scope.actionButtonsData.button.languages, 'message')
                if ($scope.actionButtonsData.button.message_type === 'popup' && btnMsg) {
                    mdToast.success(btnMsg);
                }
                $scope.closeLightBox();
            };

            var actionButtonIsShown = function (actionButton) {
                actionButton.isShown = false;

                if (actionButton.dependent_field) {
                    if ($scope.record[actionButton.dependent_field] && $scope.record[actionButton.dependent_field].labelStr == actionButton.dependent)
                        actionButton.isShown = true;
                } else {
                    actionButton.isShown = true;
                }
            };

            $scope.downloadImg = function (url) {
                if (url) {
                    const splitArray = url.split('/');
                    const fileName = splitArray[splitArray.length - 1];
                    $http({
                        method: 'GET',
                        url: url,
                        responseType: 'arraybuffer'
                    }).then(function (response) {

                        if (response.data) {
                            const array = new Uint8Array(response.data);
                            const blob = new Blob([array], {
                                type: 'application/octet-stream'
                            });

                            saveAs(blob, fileName);
                        }
                    }, function (response) {
                        void 0;
                    });
                }
            };

            $scope.languageOptions = ModuleService.getLanguageOptions();
            $scope.backToText = $filter('translate')('Common.BackTo', { moduleName: $rootScope.getLanguageValue($scope.module.languages, 'label', 'plural') });

            if (!$scope.module.fakeSection) {
                var fakeSection = Object.assign({}, $scope.module.sections[0])
                fakeSection.id = -1;
                fakeSection.type = 'component';
                fakeSection.order = 1;
                $scope.module.sections.push(fakeSection);
                $scope.module.fakeSection = true;
            }

            $scope.setProfileSharingConfigs = function () {
                if ($scope.module.name === 'p_profiles') {

                    $scope.filterJson = {
                        "group": {
                            "logic": "and",
                            "filters": [],
                            "level": 1
                        }
                    };

                    $scope.menuFieldSelectedModulOptions = {
                        placeholder: $filter('translate')('Setup.Profiles.ChooseField'),
                        dataTextField: "languages." + $rootScope.globalization.Label + ".label",
                        dataValueField: "name",
                        valuePrimitive: true,
                        autoBind: false
                    };

                    $scope.menuSectionSelectedModulOptions = {
                        placeholder: $filter('translate')('Setup.Profiles.ChooseArea'),
                        dataTextField: "languages." + $rootScope.globalization.Label + ".label",
                        dataValueField: "name",
                        valuePrimitive: true,
                        autoBind: false
                    };

                    $scope.menuPicklistOptions = {
                        dataTextField: "languages." + $rootScope.globalization.Label + ".name",
                        dataValueField: "id",
                        change: function () {
                            $scope.setMenuTree();
                        }
                    };

                    ModuleService.getMenuList()
                        .then(function (resp) {
                            $rootScope.processLanguages(resp.data);
                            $scope.menuDataSource = resp.data;
                            if ($scope.id && ($scope.record["configs"] && $scope.record["configs"] !== null)) {
                                $scope.profileConfig = JSON.parse($scope.record["configs"]);
                            } else {
                                $scope.profileConfig = {
                                    //"permissions": $rootScope.user.profile.permissions,
                                    "start_page": "dashboard",
                                    "other_permissions": {
                                        "is_persistent ": false,
                                        "send_email": false,
                                        "send_sms": false,
                                        "export_data": false,
                                        "import_data": false,
                                        "word_pdf_download": false,
                                        "smtp_settings": false,
                                        "dashboard": true,
                                        "change_email": false,
                                    }
                                }
                            }
                            var defaultMenu = $filter('filter')($scope.menuDataSource, { default: true })[0];
                            $scope.profileConfig.menu = !$scope.profileConfig.menu ? defaultMenu.id.toString() : $scope.profileConfig.menu;
                            $scope.setMenuTree();
                        });

                    $scope.setMenuTree = function () {
                        $scope.menuTreeOptions = undefined;
                        if (!$scope.profileConfig.menu || $scope.profileConfig.menu == "") {
                            return false;
                        }

                        ModuleService.getMenuItemsByMenuId($scope.profileConfig.menu).then(function (resp) {
                            $scope.menuItems = ModuleService.proccesMenuItems(resp.data);
                            setStartPage(0);
                            $scope.menuTreeOptions = {
                                dataSource: new kendo.data.HierarchicalDataSource({ data: $scope.menuItems }),
                                template: function (data) {
                                    data.item.languages = JSON.parse(data.item.languages);
                                    return '<strong style="cursor:pointer;" ng-click="selectModul(' + data.item.module_id + ')" flex md-truncate>' + $rootScope.getLanguageValue(data.item.languages, 'label') + '</strong > '
                                },
                                dragAndDrop: false,
                                autoBind: true,
                                dataTextField: "languages." + $rootScope.globalization.Label + ".label",
                                dataValueField: "id"
                            };

                            $scope.setProfileConfig();
                        });
                    }

                    $scope.selectModul = function (moduleId) {

                        if (!moduleId)
                            return;

                        $scope.menuSelectedModul = ModuleService.moduleFilterById(moduleId)[0];
                        $scope.fieldskey = {};
                        for (var i = 0; i < $scope.menuSelectedModul.fields.length; i++) {
                            var field = $scope.menuSelectedModul.fields[i];
                            $scope.fieldskey[field.name] = field;
                        }

                        if (!$scope.profileConfig["config"]) {
                            $scope.profileConfig["config"] = {};

                            angular.forEach($scope.menuItems, function (item) {
                                angular.forEach($rootScope.modules, function (module) {

                                    if (module.name === 'users' || module.name === 'profiles' || module.name === 'roles')
                                        return;

                                    $scope.profileConfig["config"][module.name] = {
                                        records: {
                                            type: "all",
                                            filters: [],
                                        },
                                        fields: {
                                            readonly: [],
                                            dont_show: []
                                        },
                                        sections: {
                                            readonly: [],
                                            dont_show: []
                                        },
                                        actions:
                                            {
                                                create:
                                                    {
                                                        enable: module.name === 'p_profiles',
                                                        filters: []
                                                    },
                                                update:
                                                    {
                                                        enable: module.name === 'p_profiles',
                                                        filters: []
                                                    },
                                                delete:
                                                    {
                                                        enable: module.name === 'p_profiles',
                                                        filters: []
                                                    },
                                                read:
                                                    {
                                                        enable: module.name === 'p_profiles'
                                                    }
                                            }
                                    };
                                });

                            });
                        }

                        if (!$scope.profileConfig["config"][$scope.menuSelectedModul.name]) {
                            $scope.profileConfig["config"][$scope.menuSelectedModul.name] = {
                                records: {
                                    type: "all",
                                    filters: [],
                                },
                                fields: {
                                    readonly: [],
                                    dont_show: []
                                },
                                sections: {
                                    readonly: [],
                                    dont_show: []
                                },
                                actions: {
                                    create:
                                        {
                                            enable: module.name === 'p_profiles',
                                            filters: []
                                        },
                                    update:
                                        {
                                            enable: module.name === 'p_profiles',
                                            filters: []
                                        },
                                    delete:
                                        {
                                            enable: module.name === 'p_profiles',
                                            filters: []
                                        },
                                    read:
                                        {
                                            enable: module.name === 'p_profiles'
                                        }
                                }
                            };
                        }
                        $scope.setProfileConfig();
                    }

                    $scope.setProfileConfig = function () {
                        $scope.record["configs"] = JSON.stringify($scope.profileConfig);
                    }

                    function setStartPage(index) {
                        var data = $scope.menuItems[index];
                        if (data.route && data.route != "" && data.route != null) {
                            $scope.profileConfig.start_page = data.route;
                        } else {
                            if (data.menu_items && data.menu_items[0] && data.menu_items[0].route) {
                                $scope.profileConfig.start_page = data.menu_items[0].route;
                            } else {
                                setStartPage(index + 1)
                            }
                        }
                    }
                }
            }

            function hideAutoNumberFieldsWhenRecordClone(isSave, record) {
                var autoNumberFields = $filter('filter')($scope.module.fields, {
                    data_type: 'number_auto',
                    deleted: false
                });

                if (autoNumberFields) {
                    for (var p = 0; p < autoNumberFields.length; p++) {
                        var field = autoNumberFields[p];
                        if (isSave)
                            delete record[field.name];
                        else
                            field.display_detail = false;
                    }
                }
            }

            $scope.tinymceOptions = {
                toolbar: "styleselect | bold italic underline | forecolor backcolor | alignleft aligncenter alignright alignjustify | link image imagetools | table bullist numlist  blockquote code fullscreen",
                menubar: 'false',
                plugins: [
                    "advlist autolink lists link image charmap print preview anchor table",
                    "searchreplace visualblocks code fullscreen",
                    "insertdatetime table contextmenu paste imagetools wordcount textcolor colorpicker"
                ],
            };

            function checkProfiles(response) {
                if ($scope.module.name === 'p_profiles' && response && response['name'] !== $scope.profileInfo['name']) {
                    const label = $rootScope.globalization.Label;

                    var profile = $filter('filter')($rootScope.profiles, { id: $scope.profileInfo['profile_id'] }, true)[0];
                    if (profile) {
                        if (angular.isObject(profile.languages)) {
                            profile.languages[label]['name'] = response['name'];
                        } else {
                            $rootScope.user.profile.languages = JSON.parse(profile.languages);
                            profile.languages[label]['name'] = response['name'];
                        }

                        if ($rootScope.user.profile) {
                            if (angular.isObject($rootScope.user.profile.languages)) {
                                $rootScope.user.profile.languages[label]['name'] = response['name'];
                            } else {
                                $rootScope.user.profile.languages = JSON.parse($rootScope.user.profile.languages);
                                $rootScope.user.profile.languages[label]['name'] = response['name'];
                            }
                        }
                    }
                }
            }

            function processLookupOperator(filters, field) {
                if (filters && filters.length > 0) {
                    var result = $filter('filter')(filters, { field: field.name }, true)[0];
                    if (result && (field.data_type === 'number' || field.data_type === 'number_auto')) {
                        result.operator = result.operator.startsWith('starts') || result.operator === 'contains' ? 'equals' : result.operator
                    }
                }
            }
        }
    ]);
