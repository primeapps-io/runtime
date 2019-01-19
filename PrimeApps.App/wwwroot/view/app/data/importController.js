﻿'use strict';

angular.module('primeapps')
    .controller('ImportController', ['$rootScope', '$scope', '$stateParams', '$state', 'config', '$q', '$localStorage', '$filter', '$popover', 'helper', 'FileUploader', 'ngToast', '$modal', '$timeout', '$cache', 'emailRegex', 'ModuleService', 'ImportService', '$cookies', 'components',
        function ($rootScope, $scope, $stateParams, $state, config, $q, $localStorage, $filter, $popover, helper, FileUploader, ngToast, $modal, $timeout, $cache, emailRegex, ModuleService, ImportService, $cookies, components) {
            $scope.type = $stateParams.type;
            $scope.wizardStep = 0;
            $scope.fieldMap = {};
            $scope.fixedValue = {};
            $scope.importMapping = {};
            $scope.mappingField = {};
            $scope.selectedMapping = {};
            $scope.mappingArray = [];
            var isSubmitTrigger = false;

            $scope.module = $filter('filter')($rootScope.modules, { name: $scope.type }, true)[0];

            if (!$scope.module) {
                ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
                $state.go('app.dashboard');
                return;
            }

            $scope.module = angular.copy($scope.module);

            angular.forEach($scope.module.fields, function (field) {
                if (field.name === 'created_by' || field.name === 'updated_by' || field.name === 'created_at' || field.name === 'updated_at') {
                    field.validation.required = false;

                    if (!helper.hasAdminRights())
                        field.deleted = true;
                }

                if ($scope.module.name === 'activities') {
                    if (field.name != 'owner' && field.name != 'activity_type' && field.name != 'subject')
                        field.validation.required = false;

                    if (field.name === 'task_reminder' || field.name === 'reminder_recurrence' || field.name === 'task_notification' || field.name === 'event_reminder')
                        field.deleted = true;
                }
            });

            angular.forEach($scope.module.sections, function (section) {
                var fields = $filter('filter')($scope.module.fields, { section: section.name, deleted: '!true' });

                if (!fields.length)
                    section.deleted = true;
            });

            ImportService.getAllMapping($scope.module.id)
                .then(function (response) {
                    if (response) {
                        var result = response.data;
                        $scope.mappingArray = result;
                    }
                });

            var readExcel = function (file, retry) {
                var fileReader = new FileReader();

                fileReader.onload = function (evt) {
                    if (!evt || evt.target.error) {
                        $timeout(function () {
                            ngToast.create({ content: $filter('translate')('Module.ExportUnsupported'), className: 'warning', timeout: 10000 });
                        });
                        return;
                    }

                    try {
                        $scope.workbook = XLS.read(evt.target.result, { type: 'binary' });
                        $scope.sheets = $scope.workbook.SheetNames;

                        $timeout(function () {
                            $scope.selectedSheet = $scope.selectedSheet || $scope.sheets[0];
                            $scope.selectSheet(retry);
                            $scope.getSampleDate();
                        });
                    }
                    catch (ex) {
                        $timeout(function () {
                            ngToast.create({ content: $filter('translate')('Data.Import.InvalidExcel'), className: 'warning' });
                        });
                    }
                };

                fileReader.readAsBinaryString(file);
            };

            $scope.selectSheet = function (retry) {
                $scope.rowsBase = XLSX.utils.sheet_to_json($scope.workbook.Sheets[$scope.selectedSheet], { header: 'A' });
                $scope.rows = XLSX.utils.sheet_to_json($scope.workbook.Sheets[$scope.selectedSheet], { raw: true, header: 'A' });
                $scope.headerRow = angular.copy($scope.rows[0]);
                $scope.rows.shift();
                $scope.cells = [];

                if ($scope.rows.length > 3000) {
                    ngToast.create({ content: $filter('translate')('Data.Import.CountError'), className: 'warning' });
                    return;
                }

                angular.forEach($scope.headerRow, function (value, key) {
                    var cellName = value + $filter('translate')('Data.Import.ColumnIndex', { index: key });
                    $scope.cells.push({ column: key, name: cellName, used: false });
                });

                if (retry) {
                    $scope.submit(true);
                    return;
                }

                $timeout(function () {
                    if (!$scope.selectedMapping.name) {
                        $scope.fieldMap = {};

                        angular.forEach($scope.headerRow, function (value, key) {
                            var used = false;

                            if ($rootScope.language === 'tr') {
                                angular.forEach($scope.module.fields, function (field) {
                                    if (field.deleted)
                                        return;

                                    if (field.label_tr.toLowerCaseTurkish() === value.trim().toLowerCaseTurkish()) {
                                        used = true;
                                        $scope.fieldMap[field.name] = key;
                                    }
                                });
                            }
                            else {
                                angular.forEach($scope.module.fields, function (field) {
                                    if (field.deleted)
                                        return;

                                    if (field.label_tr.toLowerCase() === value.trim().toLowerCase()) {
                                        used = true;
                                        $scope.fieldMap[field.name] = key;
                                    }
                                });
                            }

                            var cell = $filter('filter')($scope.cells, { column: key }, true)[0];

                            if (cell)
                                cell.used = used;
                        });
                    }
                });
            };

            var uploader = $scope.uploader = new FileUploader({
                queueLimit: 1
            });

            uploader.onAfterAddingFile = function (fileItem) {
                readExcel(fileItem._file);

            };

            uploader.onWhenAddingFileFailed = function (item, filter, options) {
                switch (filter.name) {
                    case 'excelFilter':
                        ngToast.create({ content: $filter('translate')('Data.Import.FormatError'), className: 'warning' });
                        break;
                    case 'sizeFilter':
                        ngToast.create({ content: $filter('translate')('Data.Import.SizeError'), className: 'warning' });
                        break;
                }
            };

            uploader.onBeforeUploadItem = function (item) {
                item.url = 'storage/upload_import_excel?import_id=' + $scope.importResponse.id;
            };

            uploader.filters.push({
                name: 'excelFilter',
                fn: function (item, options) {
                    var extension = helper.getFileExtension(item.name);
                    return (extension === 'xls' || extension === 'xlsx');
                }
            });

            uploader.filters.push({
                name: 'sizeFilter',
                fn: function (item) {
                    return item.size < 2097152;//2 mb
                }
            });

            ModuleService.getPicklists($scope.module)
                .then(function (picklists) {
                    $scope.picklistsModule = picklists;
                });

            $scope.lookup = function (searchTerm) {
                if ($scope.fixedField.lookup_type === 'users' && !$scope.fixedField.lookupModulePrimaryField) {
                    var userModulePrimaryField = {};
                    userModulePrimaryField.data_type = 'text_single';
                    userModulePrimaryField.name = 'full_name';
                    $scope.fixedField.lookupModulePrimaryField = userModulePrimaryField;
                }

                if ($scope.fixedField.lookup_type === 'relation') {
                    if (!$scope.fixedValue.related_module) {
                        $scope.$broadcast('angucomplete-alt:clearInput', $scope.fixedField.name);
                        return $q.defer().promise;
                    }

                    var relationModule = $filter('filter')($rootScope.modules, { name: $scope.fixedValue.related_module.value }, true)[0];

                    if (!relationModule) {
                        $scope.$broadcast('angucomplete-alt:clearInput', $scope.fixedField.name);
                        return $q.defer().promise;
                    }

                    $scope.fixedField.lookupModulePrimaryField = $filter('filter')(relationModule.fields, { primary: true }, true)[0];
                }

                if (($scope.fixedField.lookupModulePrimaryField.data_type === 'number' || $scope.fixedField.lookupModulePrimaryField.data_type === 'number_auto') && isNaN(parseFloat(searchTerm))) {
                    $scope.$broadcast('angucomplete-alt:clearInput', $scope.fixedField.name);
                    return $q.defer().promise;
                }

                return ModuleService.lookup(searchTerm, $scope.fixedField, $scope.fixedValue);
            };

            $scope.multiselect = function (searchTerm, field) {
                var picklistItems = [];

                angular.forEach($scope.picklistsModule[field.picklist_id], function (picklistItem) {
                    if (picklistItem.inactive || picklistItem.hidden)
                        return;

                    if (picklistItem.labelStr.toLowerCase().indexOf(searchTerm) > -1)
                        picklistItems.push(picklistItem);
                });

                return picklistItems;
            };

            $scope.clear = function () {
                uploader.clearQueue();
                $scope.rows = null;
                $scope.cells = null;
                $scope.sheets = null;
                $scope.fieldMap = null;
                $scope.fixedValue = null;
                $scope.fixedValueFormatted = null;
                $scope.importForm.$setPristine();
                $scope.showAdvancedOptions = false;
            };

            $scope.cellChanged = function (field) {
                if ($scope.fieldMap[field.name] === 'fixed') {
                    $scope.openFixedValueModal(field);
                    return;
                }

                angular.forEach($scope.cells, function (cell) {
                    cell.used = false;
                });

                angular.forEach($scope.fieldMap, function (value, key) {
                    var selectedCell = $filter('filter')($scope.cells, { column: value }, true)[0];

                    if (selectedCell)
                        selectedCell.used = true;
                });

                if (!$scope.fieldMap[field.name] && $scope.fixedValue && $scope.fixedValue[field.name])
                    delete $scope.fixedValue[field.name];

                if (field.name === 'related_module') {
                    delete $scope.fixedValue['related_to'];
                }
            };

            $scope.fixedValueChanged = function (field) {
                if (field.name === 'related_module') {
                    delete $scope.fixedValue['related_to'];
                }
            };

            $scope.openFixedValueModal = function (field) {
                $scope.fixedValue = $scope.fixedValue || {};
                $scope.fixedValueState = angular.copy($scope.fixedValue);
                $scope.fixedField = field;

                $scope.fixedValueModal = $scope.fixedValueModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/data/fixedValue.html',
                    animation: '',
                    backdrop: 'static',
                    show: false
                });

                $scope.fixedValueModal.$promise.then(function () {
                    $scope.fixedValueModal.show();
                });
            };

            $scope.modalSubmit = function (fixedValueForm) {
                if (!fixedValueForm.$valid)
                    return;

                if (!$scope.fixedValue[$scope.fixedField.name])
                    delete $scope.fieldMap[$scope.fixedField.name];

                $scope.fixedValueFormatted = angular.copy($scope.fixedValue);
                ModuleService.formatRecordFieldValues($scope.fixedValueFormatted, $scope.module, $scope.picklistsModule);

                angular.forEach($scope.fixedValueFormatted, function (value, key) {
                    var field = $filter('filter')($scope.module.fields, { name: key }, true)[0];

                    if (field && field.valueFormatted)
                        $scope.fixedValueFormatted[key] = field.valueFormatted;

                    if (field.data_type === 'lookup')
                        $scope.fixedValueFormatted[key] = $scope.fixedValueFormatted[key].primary_value;
                });

                $scope.fixedValueModal.hide();
            };

            $scope.modalCancel = function () {
                if (!$scope.fixedValueState[$scope.fixedField.name] && $scope.fixedValue[$scope.fixedField.name])
                    delete $scope.fixedValue[$scope.fixedField.name];

                if (!$scope.fixedValue[$scope.fixedField.name])
                    delete $scope.fieldMap[$scope.fixedField.name];
            };

            $scope.mappingSave = function (importMappingForm) {
                $scope.savingTemplate = true;
                $scope.importMapping.module_id = $scope.module.id;

                if ($scope.$parent.$parent.fixedValue)
                    $scope.$parent.$parent.fieldMap['fixed'] = angular.copy($scope.$parent.$parent.fixedValue);

                if ($scope.fixedValueFormatted)
                    $scope.$parent.$parent.fieldMap['fixedFormat'] = angular.copy($scope.$parent.$parent.fixedValueFormatted);

                var fieldMap = angular.copy($scope.$parent.$parent.fieldMap);

                if (fieldMap)
                    $scope.importMapping.mapping = JSON.stringify(fieldMap);
                else {
                    ngToast.create({
                        content: $filter('translate')('Data.Import.MappingNotFound'),
                        className: 'warning',
                        timeout: 6000
                    });
                    $scope.importMappingSaveModal.hide();
                }

                if ($scope.$parent.importMapping.skip)
                    $scope.importMapping.skip = $scope.$parent.importMapping.skip;
                else
                    $scope.importMapping.skip = false;


                if (!importMappingForm.$valid) {
                    $scope.savingTemplate = false;
                    return;
                }

                if ($scope.$parent.importMapping.name || $scope.$parent.selectedMapping) {
                    $scope.importMapping.name = $scope.$parent.importMapping.name;

                    if ($scope.$parent.selectedMapping.id) {
                        ImportService.updateMapping($scope.$parent.selectedMapping.id, $scope.importMapping)
                            .then(function (response) {
                                var result = response.data;

                                if (result) {
                                    $timeout(function () {
                                        ngToast.create({
                                            content: $filter('translate')('Data.Import.MappingSaveSucces'),
                                            className: 'success',
                                            timeout: 6000
                                        });
                                        $scope.savingTemplate = false;
                                        $scope.importMappingSaveModal.hide();

                                    }, 500);
                                }
                                else
                                    $scope.savingTemplate = false;
                            })
                            .catch(function (data) {
                                ngToast.create({
                                    content: $filter('translate')('Common.Error'),
                                    className: 'danger',
                                    timeout: 6000
                                });
                                $scope.savingTemplate = false;
                                $scope.importMappingSaveModal.hide();
                            });

                    }
                    else {
                        ImportService.getMapping($scope.importMapping)
                            .then(function (response) {
                                var result = response.data;

                                if (result) {
                                    ngToast.create({
                                        content: $filter('translate')('Data.Import.TryMappingName'),
                                        className: 'warning',
                                        timeout: 6000
                                    });

                                    $scope.$parent.importMapping = {};
                                    $scope.savingTemplate = false;
                                    $scope.importMappingSaveModal.hide();
                                }
                                else {
                                    ImportService.saveMapping($scope.importMapping)
                                        .then(function (response) {
                                            var newData = response.data;

                                            if (newData) {
                                                $timeout(function () {
                                                    ngToast.create({
                                                        content: $filter('translate')('Data.Import.MappingSaveSucces'),
                                                        className: 'success',
                                                        timeout: 6000
                                                    });
                                                    $scope.savingTemplate = false;
                                                    $scope.importMappingSaveModal.hide();

                                                    ImportService.getAllMapping($scope.module.id)
                                                        .then(function (value) {
                                                            if (value.data) {

                                                                $scope.$parent.$parent.$parent.mappingArray = value.data;
                                                                $scope.$parent.$parent.$parent.selectedMapping = newData;
                                                                $scope.mappingSelectedChange();
                                                            }
                                                        });
                                                }, 500);
                                            }
                                            else
                                                $scope.savingTemplate = false;
                                        })
                                        .catch(function (data) {
                                            ngToast.create({
                                                content: $filter('translate')('Common.Error'),
                                                className: 'danger',
                                                timeout: 6000
                                            });
                                            $scope.savingTemplate = false;
                                            $scope.importMappingSaveModal.hide();
                                        });
                                }

                            });
                    }
                }
                else {
                    $scope.importMapping = {};
                    $scope.savingTemplate = false;
                    return;
                }
            };

            $scope.deleteMapping = function () {
                components.run('BeforeDelete', 'Script', $scope, $scope.selectedMapping);

                ImportService.deleteMapping($scope.selectedMapping)
                    .then(function () {
                        ngToast.create({
                            content: $filter('translate')('Data.Import.DeletedMapping'),
                            className: 'success',
                            timeout: 6000
                        });

                        //components.run('AfterDelete', 'Script', $scope, $scope.selectedMapping);
                        $state.go('app.moduleList', { type: $scope.type });
                    })
                    .catch(function (data) {
                        ngToast.create({
                            content: $filter('translate')('Common.Error'),
                            className: 'danger',
                            timeout: 6000
                        });
                    });
            };


            $scope.mappingModalCancel = function () {
                $scope.importMapping = {};
            };

            $scope.openMappingModal = function () {
                isSubmitTrigger = true;

                if (!$scope.importForm.$valid)
                    return;

                $scope.importMappingSaveModal = $scope.importMappingSaveModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/data/importMappingSave.html',
                    animation: '',
                    backdrop: 'static',
                    show: false
                });

                $scope.importMappingSaveModal.$promise.then(function () {
                    $scope.importMappingSaveModal.show();
                });
            };

            $scope.mappingSelectedChange = function () {
                if ($scope.selectedMapping.id) {
                    var result = $filter('filter')($scope.mappingArray, { id: $scope.selectedMapping.id }, true)[0];

                    if (result) {
                        $scope.importMapping.name = result.name;
                        $scope.importMapping.skip = result.skip;

                        var mappingData = angular.fromJson(result.mapping);

                        if (mappingData) {
                            if (mappingData.fixed) {
                                $scope.fixedValue = mappingData.fixed;
                                delete mappingData.fixed;
                            }

                            if (mappingData.fixedFormat) {
                                $scope.fixedValueFormatted = mappingData.fixedFormat;
                                delete mappingData.fixedFormat;
                            }
                            $scope.fieldMap = mappingData;
                        }
                    }
                }
            };

            $scope.checkWizard = function () {
                var hasAdmin = $scope.user.profile.has_admin_rights;

                if ($scope.selectedMapping.skip && !hasAdmin) {
                    $scope.wizardStep = 2;
                    isSubmitTrigger = true;
                    $scope.submit(true);
                }
                else {
                    isSubmitTrigger = false;
                    $scope.wizardStep = 1;
                }
            };

            $scope.getDateFormat = function () {
                var dateFormat = angular.copy($scope.dateOrder);
                dateFormat = dateFormat.replace(/\//g, $scope.dateDelimiter);
                return dateFormat;
            };

            $scope.getDateTimeFormat = function () {
                var dateFormat = $scope.getDateFormat();
                dateFormat += ' ' + $scope.timeFormat;
                return dateFormat;
            };

            $scope.getSampleDate = function () {
                if (!$scope.dateOrder)
                    $scope.dateOrder = $rootScope.locale === 'en' ? 'MM/DD/YYYY' : 'DD/MM/YYYY';

                if (!$scope.dateDelimiter)
                    $scope.dateDelimiter = $rootScope.locale === 'en' ? '/' : '.';

                if (!$scope.timeFormat)
                    $scope.timeFormat = 'HH:mm';

                var date = new Date('2010-01-31 16:20:00');
                var dateFormat = $scope.getDateTimeFormat();
                $scope.sampleDate = moment(date).format(dateFormat);
            };

            $scope.prepareFixedValue = function () {
                var fixedValue = angular.copy($scope.fixedValue);

                angular.forEach(fixedValue, function (value, key) {
                    var field = $filter('filter')($scope.module.fields, { name: key }, true)[0];

                    switch (field.data_type) {
                        case 'number':
                            fixedValue[key] = parseInt(fixedValue[key]);
                            break;
                        case 'number_decimal':
                        case 'currency':
                            fixedValue[key] = parseFloat(fixedValue[key]);
                            break;
                        case 'picklist':
                            fixedValue[key] = fixedValue[key] != null ? fixedValue[key].label[$rootScope.user.tenant_language] : null;
                            break;
                        case 'tag':
                            var picklistItems = recordValue.split('|');
                            recordValue = '{';

                            for (var i = 0; i < picklistItems.length; i++) {
                                recordValue += '"' + picklistItems[i] + '",';
                            }

                            if (recordValue)
                                recordValue = recordValue.slice(0, -1) + '}';
                            break;
                        case 'multiselect':
                            var values = '{';

                            angular.forEach(fixedValue[key], function (picklistItem) {
                                values += '"' + picklistItem['label_' + $rootScope.user.tenant_language] + '",';
                            });

                            fixedValue[key] = values.slice(0, -1) + '}';
                            break;
                        case 'lookup':
                            fixedValue[key] = fixedValue[key].id;
                            break;
                    }
                });

                return fixedValue;
            };

            $scope.prepareRecords = function () {
                var records = [];

                var getRecordFieldValueAndValidate = function (cellValue, field, rowNo, cellName) {
                    var recordValue = '';

                    if (cellValue)
                        recordValue = cellValue.toString().trim();

                    $scope.error = {};
                    $scope.error.rowNo = rowNo;
                    $scope.error.cellName = cellName;
                    $scope.error.cellValue = cellValue;
                    $scope.error.fieldLabel = field['label_' + $rootScope.language];

                    switch (field.data_type) {
                        case 'number':
                            recordValue = parseInt(recordValue);

                            if (isNaN(recordValue)) {
                                $scope.error.message = $filter('translate')('Data.Import.Error.InvalidNumber');
                                recordValue = 0;
                            }
                            break;
                        case 'number_decimal':
                            recordValue = parseFloat(recordValue);

                            if (isNaN(recordValue)) {
                                $scope.error.message = $filter('translate')('Data.Import.Error.InvalidNumber');
                                recordValue = 0;
                            }
                            break;
                        case 'currency':
                            recordValue = recordValue.replace(',', '.');
                            recordValue = parseFloat(recordValue);

                            if (isNaN(recordValue)) {
                                $scope.error.message = $filter('translate')('Data.Import.Error.InvalidDecimal');
                                recordValue = null;
                            }
                            break;
                        case 'date':

                            var tempValue = getJsDateFromExcel(recordValue);

                            if (tempValue.toString() === "Invalid Date")
                                recordValue = recordValue.split(' ')[0];
                            else
                                recordValue = tempValue;

                            var dateFormat = $scope.getDateFormat();
                            var standardDateFormat = 'DD/MM/YYYY';

                            var mmtDate = moment(recordValue, dateFormat, true);

                            if (!mmtDate.isValid()) {
                                mmtDate = moment(recordValue, standardDateFormat, true);

                                if (!mmtDate.isValid()) {
                                    var dateDelimeter = $scope.dateDelimiter;
                                    var dateArray = recordValue.split(dateDelimiter);
                                    var dateFormatArray = dateFormat.split(dateDelimiter);

                                    if (dateArray.length > 1) {
                                        var YY = dateArray[dateFormatArray.indexOf('YYYY')];
                                        var MM = dateArray[dateFormatArray.indexOf('MM')];
                                        var DD = dateArray[dateFormatArray.indexOf('DD')];

                                        var parseValue = Date.parse(MM + dateDelimeter + DD + dateDelimiter + YY); //dont change sequence

                                        mmtDate = moment(new Date(parseValue), dateFormat, true);

                                        if (!mmtDate.isValid()) {
                                            $scope.error.message = $filter('translate')('Data.Import.Error.InvalidDate');
                                            recordValue = null;
                                        }
                                        else {
                                            recordValue = mmtDate.format();
                                        }
                                    }
                                }
                            }
                            else {
                                recordValue = mmtDate.format();
                            }
                            break;
                        case 'date_time':
                            var tempValue = getJsDateFromExcel(recordValue);

                            if (tempValue.toString() === "Invalid Date")
                                recordValue = recordValue.split(' ')[0];
                            else
                                recordValue = tempValue;

                            var dateTimeFormat = $scope.getDateTimeFormat();
                            var standardDateTimeFormat = 'DD/MM/YYYY' + ' ' + $scope.timeFormat;
                            var mmtDateTime = moment(recordValue, dateTimeFormat, true);

                            if (!mmtDateTime.isValid()) {
                                mmtDateTime = moment(recordValue, standardDateTimeFormat, true);

                                if (!mmtDateTime.isValid()) {
                                    $scope.error.message = $filter('translate')('Data.Import.Error.InvalidDateTime');
                                    recordValue = null;
                                }
                                else {
                                    recordValue = mmtDateTime.toDate();
                                }
                            }
                            else {
                                recordValue = mmtDateTime.toDate();
                            }
                            break;
                        case 'time':
                            var baseValue = getExcelBaseValue(field, rowNo - 1, cellName);
                            var newValueArray = [];
                            var mmtTime;

                            if (baseValue) {
                                newValueArray = baseValue.split(":");
                                mmtTime = moment(new Date(null, null, null, newValueArray[0], newValueArray[1]), $scope.timeFormat, true);
                            }
                            else {
                                recordValue = getJsDateFromExcel(recordValue);
                                mmtTime = moment(recordValue.toUTCString(), $scope.timeFormat, true);
                            }

                            if (!mmtTime.isValid()) {
                                $scope.error.message = $filter('translate')('Data.Import.Error.InvalidTime');
                                recordValue = null;
                            }
                            else {
                                recordValue = mmtTime.toDate();
                            }
                            break;
                        case 'email':
                            if (!emailRegex.test(recordValue)) {
                                $scope.error.message = $filter('translate')('Data.Import.Error.InvalidEmail');
                                recordValue = null;
                            }
                            break;
                        case 'picklist':
                            var picklistItem = $filter('filter')($scope.picklistsModule[field.picklist_id], { labelStr: recordValue }, true)[0];

                            if (!picklistItem) {
                                $scope.error.message = $filter('translate')('Data.Import.Error.PicklistItemNotFound');
                                recordValue = null;
                            }
                            break;
                        case 'multiselect':
                            var picklistItems = recordValue.split('|');
                            recordValue = '{';

                            for (var i = 0; i < picklistItems.length; i++) {
                                var picklistItemLabel = picklistItems[i];
                                var multiselectPicklistItem = $filter('filter')($scope.picklistsModule[field.picklist_id], { labelStr: picklistItemLabel })[0];

                                if (!multiselectPicklistItem) {
                                    $scope.error.message = $filter('translate')('Data.Import.Error.MultiselectItemNotFound', { item: picklistItemLabel });
                                    recordValue = null;
                                    break;
                                }

                                recordValue += '"' + picklistItemLabel + '",';
                            }

                            if (recordValue)
                                recordValue = recordValue.slice(0, -1) + '}';
                            break;
                        case 'lookup':
                            if (field.lookup_type === 'relation')
                                field.lookup_type = $scope.fixedValue.related_module.value;

                            var lookupIds = [];

                            for (var j = 0; j < $scope.lookupIds[field.lookup_type].length; j++) {
                                var lookupIdItem = $scope.lookupIds[field.lookup_type][j];

                                var isInt = recordValue % 1 === 0;

                                if (isInt && lookupIdItem.id === parseInt(recordValue))
                                    lookupIds.push(lookupIdItem);
                                else if (lookupIdItem.value == recordValue)
                                    lookupIds.push(lookupIdItem);
                            }

                            var lookupModule = $filter('filter')($rootScope.modules, { name: field.lookup_type })[0];

                            if (field.lookup_type === 'users') {
                                lookupModule = {};
                                lookupModule.label_tr_singular = 'Kullanıcı';
                                lookupModule.label_en_singular = 'User';
                            }

                            if (lookupIds.length > 1) {
                                $scope.error.message = $filter('translate')('Data.Import.Error.LookupMoreThanOne', { module: lookupModule['label_' + $rootScope.language + '_singular'] });
                                recordValue = null;
                                return;
                            }

                            var lookupId = lookupIds[0];

                            if (!lookupId) {
                                $scope.error.message = $filter('translate')('Data.Import.Error.LookupNotFound', { module: lookupModule['label_' + $rootScope.language + '_singular'] });
                                recordValue = null;
                                return;
                            }

                            recordValue = lookupId.id;
                            break;
                        case 'checkbox':
                            if (recordValue.toLowerCase() === 'yes' || recordValue.toLowerCase() === 'evet' || recordValue.toLowerCase() === 'true') {
                                recordValue = 'true';
                            }
                            else if (recordValue.toLowerCase() === 'no' || recordValue.toLowerCaseTurkish() === 'hayır' || recordValue.toLowerCase() === 'false') {
                                recordValue = 'false';
                            }
                            else {
                                $scope.error.message = $filter('translate')('Data.Import.Error.InvalidCheckbox');
                                recordValue = null;
                            }
                            break;
                    }

                    if (recordValue != null && recordValue != undefined) {
                        if (!field.validation)
                            field.validation = {};

                        if (!field.validation.max_length) {
                            switch (field.data_type) {
                                case 'text_single':
                                    field.validation.max_length = 50;
                                    break;
                                case 'text_multi':
                                    field.validation.max_length = 500;
                                    break;
                                case 'number':
                                    field.validation.max_length = 15;
                                    break;
                                case 'number_decimal':
                                    field.validation.max_length = 19;
                                    break;
                                case 'currency':
                                    field.validation.max_length = 21;
                                    break;
                                case 'email':
                                    field.validation.max_length = 100;
                                    break;
                            }
                        }

                        if (!field.validation.max && (field.data_type === 'number' || field.data_type === 'number_decimal' || field.data_type === 'currency'))
                            field.validation.max = Number.MAX_VALUE;

                        if (field.validation.max_length && recordValue.toString().length > field.validation.max_length) {
                            $scope.error.message = $filter('translate')('Data.Import.Validation.MaxLength', { maxLength: field.validation.max_length });
                            recordValue = null;
                        }

                        if (field.validation.min_length && recordValue.toString().length < field.validation.min_length) {
                            $scope.error.message = $filter('translate')('Data.Import.Validation.MinLength', { minLength: field.validation.min_length });
                            recordValue = null;
                        }

                        if (field.validation.max && recordValue > field.validation.max) {
                            $scope.error.message = $filter('translate')('Data.Import.Validation.Max', { max: field.validation.max });
                            recordValue = null;
                        }

                        if (field.validation.min && recordValue < field.validation.min) {
                            $scope.error.message = $filter('translate')('Data.Import.Validation.Min', { min: field.validation.min });
                            recordValue = null;
                        }

                        if (field.validation.pattern) {
                            var rgx = new RegExp(field.validation.pattern);

                            if (!rgx.test(recordValue)) {
                                $scope.error.message = $filter('translate')('Data.Import.Validation.Pattern');
                                recordValue = null;
                            }
                        }
                    }

                    if (recordValue != null && recordValue != undefined)
                        $scope.error = null;

                    return recordValue;
                };

                var fixedValue = $scope.prepareFixedValue();

                for (var i = 0; i < $scope.rows.length; i++) {
                    var record = {};
                    var row = $scope.rows[i];
                    $scope.error = null;

                    for (var fieldMapKey in $scope.fieldMap) {
                        if (fieldMapKey === 'fixed' || fieldMapKey === 'fixedFormat') //To added excel import mapping 
                            continue;

                        if ($scope.fieldMap.hasOwnProperty(fieldMapKey)) {
                            var fieldMapValue = $scope.fieldMap[fieldMapKey];

                            if (fieldMapValue === 'fixed') {
                                record[fieldMapKey] = fixedValue[fieldMapKey];
                            }
                            else {
                                var field = angular.copy($filter('filter')($scope.module.fields, { name: fieldMapKey }, true)[0]);
                                var cellValue = row[fieldMapValue];

                                if (field.validation && field.validation.required && !cellValue) {
                                    $scope.error = {};
                                    $scope.error.rowNo = i + 2;
                                    $scope.error.cellName = fieldMapValue;
                                    $scope.error.cellValue = cellValue;
                                    $scope.error.fieldLabel = field['label_' + $rootScope.language];
                                    $scope.error.message = $filter('translate')('Data.Import.Error.Required');
                                    break;
                                }

                                if (!cellValue)
                                    continue;

                                var recordFieldValue = getRecordFieldValueAndValidate(cellValue, field, i + 2, fieldMapValue);

                                if (angular.isUndefined(recordFieldValue))
                                    break;

                                record[fieldMapKey] = recordFieldValue;
                            }
                        }
                    }

                    if ($scope.error)
                        break;

                    records.push(record);
                }

                $scope.preparing = false;

                return records;
            };

            $scope.getLookupIds = function () {
                var deferred = $q.defer();
                var lookupRequest = [];

                for (var i = 0; i < $scope.rows.length; i++) {
                    var row = $scope.rows[i];

                    for (var rowKey in row) {
                        if (row.hasOwnProperty(rowKey)) {
                            var cellValue = row[rowKey].toString().trim();

                            for (var fieldMapKey in $scope.fieldMap) {
                                if ($scope.fieldMap.hasOwnProperty(fieldMapKey)) {
                                    var fieldMapValue = $scope.fieldMap[fieldMapKey];

                                    if (fieldMapKey != 'fixed' && rowKey === fieldMapValue) {
                                        var field = angular.copy($filter('filter')($scope.module.fields, { name: fieldMapKey }, true)[0]);

                                        if (field.data_type != 'lookup')
                                            break;

                                        if (field.lookup_type === 'relation')
                                            field.lookup_type = $scope.fixedValue.related_module.value;

                                        var lookupItem = $filter('filter')(lookupRequest, { type: field.lookup_type }, true)[0];

                                        if (!lookupItem) {
                                            lookupItem = {};
                                            lookupItem.type = field.lookup_type;
                                            lookupItem.values = [];

                                            if (field.lookup_type != 'users') {
                                                var lookupModule = $filter('filter')($rootScope.modules, { name: field.lookup_type }, true)[0];
                                                lookupItem.field = $filter('filter')(lookupModule.fields, { primary: true }, true)[0].name;
                                            }
                                            else {
                                                if (cellValue.indexOf('@') > -1)
                                                    lookupItem.field = 'email';
                                                else
                                                    lookupItem.field = 'full_name';
                                            }

                                            lookupRequest.push(lookupItem);
                                        }

                                        if (cellValue && lookupItem.values.indexOf(cellValue) < 0) {
                                            lookupItem.values.push(cellValue);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (!lookupRequest.length) {
                    deferred.resolve([]);
                }
                else {
                    ImportService.getLookupIds(lookupRequest)
                        .then(function (lookupIds) {
                            deferred.resolve(lookupIds.data);
                        })
                        .catch(function (reason) {
                            deferred.reject(reason.data);
                        });
                }

                return deferred.promise;
            };

            $scope.submit = function (retry) {
                $scope.error = null;
                $scope.errorUnique = null;

                if ($scope.module.name === 'activities' && $scope.fieldMap['related_to'] && (!$scope.fixedValue || !$scope.fixedValue['related_module'])) {
                    $scope.importForm['related_module'].$setValidity('required', false);
                }

                if (!$scope.importForm.$valid)
                    return;

                if (isSubmitTrigger) {
                    isSubmitTrigger = false;
                    return;
                }

                if (!retry)
                    $scope.wizardStep = 2;

                $scope.preparing = true;
                $scope.trying = false;

                $timeout(function () {
                    $scope.getLookupIds()
                        .then(function (lookupIds) {
                            $scope.lookupIds = lookupIds;
                            $scope.records = $scope.prepareRecords();
                        });
                }, 200);
            };

            $scope.tryAgain = function () {
                $scope.trying = true;
                $scope.error = null;
                $scope.errorUnique = null;
                var file = uploader.queue[0]._file;
                readExcel(file, true);
            };

            $scope.saveLoaded = function () {
                $scope.getLookupIds()
                    .then(function (lookupIds) {
                        $scope.lookupIds = lookupIds;
                        $scope.records = $scope.prepareRecords();
                        $scope.save();
                    });

            }

            $scope.save = function () {
                if (!$scope.records && $scope.selectedMapping.id) {
                    $scope.saveLoaded();
                    return;
                }

                $scope.saving = true;

                $timeout(function () {
                    if ($scope.saving)
                        $scope.longProcessing = true;
                }, 4000);

                ImportService.import($scope.records, $scope.module.name)
                    .then(function (response) {
                        var cacheKey = $scope.module.name + '_' + $scope.module.name;
                        $cache.remove(cacheKey);
                        $scope.importResponse = response.data;
                        uploader.uploadAll();

                        $timeout(function () {
                            $scope.saving = false;
                            $scope.longProcessing = false;
                            ngToast.create({ content: $filter('translate')('Data.Import.Success'), className: 'success', timeout: 6000 });
                            $state.go('app.moduleList', { type: $scope.type });
                        }, 500);
                    })
                    .catch(function (data) {
                        if (data.status === 409) {
                            $scope.errorUnique = {};
                            $scope.errorUnique.field = data.data.field;

                            if (data.data.field2)
                                $scope.errorUnique.field = data.data.field2;
                        }

                        $scope.saving = false;
                        $scope.longProcessing = false;
                    });
            };

            $scope.combinationFilter = function (field) {
                if (field.combination) {
                    return false;
                }
                return true
            }

            function getJsDateFromExcel(excelDate) {
                return new Date((excelDate - (25567 + 2)) * 86400 * 1000);
            }

            function getExcelBaseValue(field, rowNo, cellName) {
                var row = $scope.rowsBase[rowNo];
                var cellBaseValue = row[cellName];

                return cellBaseValue;
            }
        }
    ]);