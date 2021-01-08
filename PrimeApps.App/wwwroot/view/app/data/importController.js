
'use strict';

angular.module('primeapps')
    .controller('ImportController', ['$rootScope', '$scope', '$stateParams', '$state', 'config', '$q', '$localStorage', '$filter', 'helper', 'FileUploader', '$timeout', '$cache', 'emailRegex', 'ModuleService', 'ImportService', '$cookies', 'components', '$mdDialog', 'mdToast',
        function ($rootScope, $scope, $stateParams, $state, config, $q, $localStorage, $filter, helper, FileUploader, $timeout, $cache, emailRegex, ModuleService, ImportService, $cookies, components, $mdDialog, mdToast) {
            $scope.type = $stateParams.type;
            $scope.wizardStep = 0;
            $scope.fieldMap = {};
            $scope.fixedValue = {};
            $scope.importMapping = {};
            $scope.mappingField = {};
            $scope.selectedMapping = {};
            $scope.mappingArray = [];
            var isSubmitTrigger = false;
            $scope.loading = false;
            $scope.module = $filter('filter')($rootScope.modules, { name: $scope.type }, true)[0];

            if (!$scope.module) {
                mdToast.warning($filter('translate')('Common.NotFound'));
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
            });

            angular.forEach($scope.module.sections, function (section) {
                var fields = $filter('filter')($scope.module.fields, { section: section.name, deleted: '!true' });

                if (!fields.length)
                    section.deleted = true;
                if (section.type === 'component')// for fake sections
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
                            mdToast.warning({ content: $filter('translate')('Module.ExportUnsupported'), timeout: 10000 });
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
                            mdToast.warning($filter('translate')('Data.Import.InvalidExcel'));
                            $scope.loading = false;
                        });
                    }
                };

                fileReader.readAsBinaryString(file);
            };


            var fixed = {
                column: 'fixed',
                name: $filter('translate')('Data.Import.FixedValue') + ' -->'
            };

            var defaultCurrentUser = {
                column: 'currentUser',
                name: $rootScope.user.full_name
            };

            $scope.selectSheet = function (retry, sheet) {

                $scope.selectedSheet = sheet || $scope.selectedSheet;
                // $scope.rowsBase = XLSX.utils.sheet_to_json($scope.workbook.Sheets[$scope.selectedSheet], { header: 'A' });
                $scope.rows = XLSX.utils.sheet_to_json($scope.workbook.Sheets[$scope.selectedSheet], { raw: true, header: 'A' });
                $scope.headerRow = angular.copy($scope.rows[0]);
                $scope.rows.shift();
                $scope.cells = [];

                if ($scope.rows.length > 3000) {
                    mdToast.warning($filter('translate')('Data.Import.CountError'));
                    $scope.loading = false;
                    return;
                }

                if (retry) {
                    $scope.submit(true);
                    return;
                }

                if (!$scope.selectedMapping.name) {
                    $scope.fieldMap = {};
                    $scope.fixedValue = {};
                    $scope.fixedValueFormatted = {};

                    angular.forEach($scope.headerRow, function (value, key) {
                        var cellName = value + $filter('translate')('Data.Import.ColumnIndex', { index: key });
                        var cell = { column: key, name: cellName, used: false };
                        $scope.cells.push(cell);

                        for (var h = 0; h < $scope.module.fields.length; h++) {

                            var field = $scope.module.fields[h];

                            if (field.deleted)
                                return;

                            var language = $rootScope.getLanguageValue(field.languages, 'label');

                            if (field.name === 'owner') {
                                // cell.used = true;
                                $scope.fieldMap[field.name] = defaultCurrentUser;
                            } else if (language && language.toLowerCase() === value.trim().toLowerCase()) {
                                // cell.used = true;
                                $scope.fieldMap[field.name] = filterCells(key);
                            } else {
                                if (field && field.default_value) {
                                    if (field.data_type === 'picklist') {
                                        $scope.fieldMap[field.name] = fixed;
                                        var picklistValue = $filter('filter')($scope.picklistsModule[field.picklist_id], { id: field.default_value })[0];
                                        $scope.fixedValue[field.name] = picklistValue;
                                        $scope.fixedValueFormatted[field.name] = $rootScope.getLanguageValue(picklistValue.languages, 'label');
                                    } else {
                                        $scope.fieldMap[field.name] = fixed;
                                        $scope.fixedValue[field.name] = field.default_value;
                                        $scope.fixedValueFormatted[field.name] = field.default_value;
                                    }
                                }
                            }
                        }
                    });
                }

                $scope.loading = false;
            };

            FileUploader.FileSelect.prototype.isEmptyAfterSelection = function () {
                return true;
            };

            var uploader = $scope.uploader = new FileUploader();

            $scope.fileName = null;
            uploader.onAfterAddingFile = function (fileItem) {
                $scope.selectedSheet = undefined;
                $scope.loading = true;
                if ($scope.fileName === null || $scope.fileName === fileItem._file.name || $scope.wizardStep === 0) {
                    readExcel(fileItem._file);
                    $scope.fileName = fileItem._file.name;
                }
                else if ($scope.fileName !== fileItem._file.name) {
                    uploader.clearQueue();
                }
            };

            uploader.onWhenAddingFileFailed = function (item, filter, options) {
                switch (filter.name) {
                    case 'excelFilter':
                        mdToast.warning($filter('translate')('Data.Import.FormatError'));
                        break;
                    case 'sizeFilter':
                        mdToast.warning($filter('translate')('Data.Import.SizeError'));
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
                    $rootScope.processPicklistLanguages(picklists);
                    $scope.picklistsModule = picklists;
                });


            $scope.multiselect = function (searchTerm, field) {
                var picklistItems = [];

                angular.forEach($scope.picklistsModule[field.picklist_id], function (picklistItem) {
                    if (picklistItem.inactive || picklistItem.hidden)
                        return;

                    //if (picklistItem.labelStr.toLowerCase().indexOf(searchTerm) > -1)
                    var item = $rootScope.getLanguageValue(picklistItem.languages, 'label');
                    if (item && item.toLowerCase().indexOf(searchTerm) > -1)
                        picklistItems.push(picklistItem);
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
                $scope.showAdvancedOptions = false;
            };

            $scope.cellChanged = function (field, ev) {
                if ($scope.fieldMap[field.name] === 'fixed') {
                    $scope.openFixedValueModal(field, ev);
                    return;
                }

                // angular.forEach($scope.cells, function (cell) {
                //     cell.used = false;
                // });
                //
                // angular.forEach($scope.fieldMap, function (value, key) {
                //     var selectedCell = $filter('filter')($scope.cells, { column: value }, true)[0];
                //
                //     if (selectedCell)
                //         selectedCell.used = true;
                // });

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

            $scope.openFixedValueModal = function (field, ev) {
                $scope.fixedValue = $scope.fixedValue || {};
                $scope.fixedValueState = angular.copy($scope.fixedValue);
                $scope.fixedField = field;

                var parentEl = angular.element(document.body);

                $mdDialog.show({
                    parent: parentEl,
                    templateUrl: 'view/app/data/fixedValue.html',
                    clickOutsideToClose: true,
                    targetEvent: ev,
                    scope: $scope,
                    preserveScope: true

                });
            };

            $scope.closeDialog = function () {
                $mdDialog.hide();
            };

            $scope.modalSubmit = function (fixedValueForm) {
                if (!fixedValueForm.validate())
                    return;

                if (!$scope.fixedValue[$scope.fixedField.name])
                    delete $scope.fieldMap[$scope.fixedField.name];

                $scope.fixedValueFormatted = angular.copy($scope.fixedValue);
                ModuleService.formatRecordFieldValues($scope.fixedValueFormatted, $scope.module, $scope.picklistsModule);

                angular.forEach($scope.fixedValueFormatted, function (value, key) {
                    var field = $filter('filter')($scope.module.fields, { name: key }, true)[0];

                    if (field && field.valueFormatted) {
                        // if (field.data_type === 'lookup'){
                        //     $scope.fixedValueFormatted[key] = $scope.fixedValueFormatted[key];
                        // }
                        // else if (field.valueFormatted){
                        $scope.fixedValueFormatted[key] = field.valueFormatted;
                        // }
                    }
                });

                $scope.excelCellOptions($scope.fixedField, true);
                $scope.closeDialog();
            };

            $scope.modalCancel = function () {
                if (!$scope.fixedValueState[$scope.fixedField.name] && $scope.fixedValue[$scope.fixedField.name])
                    delete $scope.fixedValue[$scope.fixedField.name];

                if (!$scope.fixedValue[$scope.fixedField.name])
                    delete $scope.fieldMap[$scope.fixedField.name];
            };

            $scope.loadMappingList = function () {
                ImportService.getAllMapping($scope.module.id)
                    .then(function (value) {
                        if (value.data) {
                            $scope.mappingArray = value.data;
                            //$scope.mappingOptions.setDataSource($scope.mappingArray);
                        }
                    });

                $scope.mappingDropDown2.dataSource.read();
            };

            $scope.mappingSave = function (importMappingForm) {
                $scope.savingTemplate = true;
                $scope.importMapping.module_id = $scope.module.id;

                if ($scope.fixedValue)
                    $scope.fieldMap['fixed'] = angular.copy($scope.fixedValue);

                if ($scope.fixedValueFormatted)
                    $scope.fieldMap['fixedFormat'] = angular.copy($scope.fixedValueFormatted);

                var fieldMap = angular.copy($scope.fieldMap);

                if (fieldMap)
                    $scope.importMapping.mapping = JSON.stringify(fieldMap);
                else {
                    mdToast.warning({
                        content: $filter('translate')('Data.Import.MappingNotFound'),
                        timeout: 6000
                    });
                    $scope.importMappingSaveModal.hide();
                }

                if ($scope.importMapping.skip)
                    $scope.importMapping.skip = $scope.importMapping.skip;
                else
                    $scope.importMapping.skip = false;


                if (!importMappingForm.$valid) {
                    $scope.savingTemplate = false;
                    return;
                }

                if ($scope.importMapping.name || $scope.selectedMapping) {

                    if ($scope.selectedMapping.id) {
                        ImportService.updateMapping($scope.selectedMapping.id, $scope.importMapping)
                            .then(function (response) {
                                var result = response.data;

                                if (result) {
                                    $scope.loadMappingList();
                                    $timeout(function () {
                                        mdToast.success({
                                            content: $filter('translate')('Data.Import.MappingSaveSucces'),
                                            timeout: 6000
                                        });
                                        $scope.savingTemplate = false;
                                        $scope.cancel();

                                    }, 500);
                                }
                                else
                                    $scope.savingTemplate = false;
                            })
                            .catch(function (data) {
                                mdToast.error({
                                    content: $filter('translate')('Common.Error'),
                                    timeout: 6000
                                });
                                $scope.savingTemplate = false;
                                $scope.cancel();
                            });

                    }
                    else {
                        ImportService.getMapping($scope.importMapping)
                            .then(function (response) {
                                var result = response.data;

                                if (result) {
                                    mdToast.warning({
                                        content: $filter('translate')('Data.Import.TryMappingName'),
                                        timeout: 6000
                                    });

                                    $scope.importMapping = {};
                                    $scope.savingTemplate = false;
                                    $scope.cancel();
                                }
                                else {
                                    ImportService.saveMapping($scope.importMapping)
                                        .then(function (response) {
                                            var newData = response.data;

                                            if (newData) {
                                                $timeout(function () {
                                                    $scope.loadMappingList();
                                                    mdToast.success({
                                                        content: $filter('translate')('Data.Import.MappingSaveSucces'),
                                                        timeout: 6000
                                                    });
                                                    $scope.savingTemplate = false;
                                                    $scope.cancel();

                                                    ImportService.getAllMapping($scope.module.id)
                                                        .then(function (value) {
                                                            if (value.data) {

                                                                $scope.mappingArray = value.data;
                                                                $scope.selectedMapping = newData;
                                                                $scope.mappingDropDown2.dataSource.read()
                                                                //$scope.mappingOptions.setDataSource(value.data);
                                                                $scope.mappingSelectedChange(newData);
                                                            }
                                                        });
                                                }, 500);
                                            }
                                            else
                                                $scope.savingTemplate = false;
                                        })
                                        .catch(function (data) {
                                            mdToast.error({
                                                content: $filter('translate')('Common.Error'),
                                                timeout: 6000
                                            });
                                            $scope.savingTemplate = false;
                                            $scope.cancel();
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

            $scope.deleteMapping = function (item) {
                $scope.selectedMapping = item;
                components.run('BeforeDelete', 'Script', $scope, $scope.selectedMapping);

                ImportService.deleteMapping($scope.selectedMapping)
                    .then(function () {
                        $scope.loadMappingList();
                        mdToast.success({
                            content: $filter('translate')('Data.Import.DeletedMapping'),
                            timeout: 6000
                        });

                        //components.run('AfterDelete', 'Script', $scope, $scope.selectedMapping);
                        //$state.go('app.moduleList', { type: $scope.type });
                    })
                    .catch(function (data) {
                        mdToast.error({
                            content: $filter('translate')('Common.Error'),
                            timeout: 6000
                        });
                    });
            };

            $scope.showConfirm = function (item, ev) {
                var confirm = $mdDialog.confirm()
                    .title($filter('translate')('Common.AreYouSure'))
                    .targetEvent(ev)
                    .ok($filter('translate')('Common.Yes'))
                    .cancel($filter('translate')('Common.No'));

                $mdDialog.show(confirm).then(function () {
                    $scope.deleteMapping(item);
                }, function () {

                });

            };


            $scope.mappingModalCancel = function () {
                $scope.importMapping = {};
            };

            $scope.openMappingModal = function () {
                isSubmitTrigger = true;

                if (!$scope.importForm.validate()) {
                    mdToast.warning($filter('translate')('Module.RequiredError'));
                    return;
                }

                var parentEl = angular.element(document.body);
                $mdDialog.show({
                    parent: parentEl,
                    templateUrl: 'view/app/data/importMappingSave.html',
                    clickOutsideToClose: false,
                    scope: $scope,
                    preserveScope: true

                });

            };

            $scope.mappingSelectedChange = function (item) {
                if (item && item.id) {
                    $scope.selectedMapping = item;
                    var result = $filter('filter')($scope.mappingArray, { id: item.id }, true)[0];

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
                else {
                    $scope.selectedMapping = {};
                    $scope.importMapping = {};
                }
            };

            $scope.checkWizard = function () {
                var hasAdmin = $scope.user.profile.has_admin_rights;
                $scope.fieldMapCopy = angular.copy($scope.fieldMap);

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
                    $scope.dateOrder = $rootScope.locale === 'en' ? 'M/D/YYYY' : 'DD/MM/YYYY'; //TODO culture

                if (!$scope.dateDelimiter)
                    $scope.dateDelimiter = $rootScope.locale === 'en' ? '/' : '.';

                if (!$scope.timeFormat)
                    $scope.timeFormat = $rootScope.locale === 'en' ? 'H:mm a' : 'HH:mm'; //TODO culture

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
                            //fixedValue[key] = fixedValue[key] != null ? fixedValue[key].label[$rootScope.user.tenant_language] : null;
                            fixedValue[key] = fixedValue[key] != null ? fixedValue[key].labelStr : null;
                            break;
                        case 'tag':
                            var picklistItems = fixedValue[key].split('|');
                            fixedValue[key] = '{';

                            for (var i = 0; i < picklistItems.length; i++) {
                                fixedValue[key] += '"' + picklistItems[i] + '",';
                            }

                            if (fixedValue[key])
                                fixedValue[key] = fixedValue[key].slice(0, -1) + '}';
                            break;
                        case 'multiselect':
                            var values = '{{';

                            angular.forEach(fixedValue[key], function (picklistItem) {
                                //values += '"' + picklistItem['label_' + $rootScope.user.tenant_language] + '",';
                                values += '"' + $rootScope.getLanguageValue(picklistItem.languages, "label") + '",';
                            });

                            fixedValue[key] = values.slice(0, -1) + '}}';
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
                    $scope.error.fieldLabel = $rootScope.getLanguageValue(field.languages, "label");

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
                            var standardDateFormat = $scope.locale === 'en' ? 'D/M/YYYY' : 'DD/MM/YYYY';

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
                            var tempRecordValue = angular.copy(recordValue);
                            var tempValue = getJsDateFromExcel(recordValue);

                            if (tempValue.toString() === "Invalid Date")
                                recordValue = recordValue.split(' ')[0];
                            else
                                recordValue = tempValue;

                            var dateTimeFormat = $scope.getDateTimeFormat();
                            var standardDateTimeFormat = $scope.locale === 'en' ? 'D/M/YYYY' : 'DD/MM/YYYY';
                            standardDateTimeFormat += ' ' + $scope.timeFormat;
                            var mmtDateTime = moment(recordValue, dateTimeFormat, true);

                            if (!mmtDateTime.isValid()) {
                                mmtDateTime = moment(tempRecordValue, dateTimeFormat, true);

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
                                newValueArray[1] = $scope.locale === 'en' ? newValueArray[1].split(' ')[0] : newValueArray[1];
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
                                recordValue = mmtTime.format();
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
                            recordValue = '{{';

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
                                recordValue = recordValue.slice(0, -1) + '}}';
                            break;
                        case 'lookup':
                            if (field.lookup_type === 'relation')
                                field.lookup_type = $scope.fixedValue.related_module.value;

                            var lookupIds = [];

                            if ($scope.lookupIds[field.lookup_type]) {
                                for (var j = 0; j < $scope.lookupIds[field.lookup_type].length; j++) {
                                    var lookupIdItem = $scope.lookupIds[field.lookup_type][j];

                                    var isInt = recordValue % 1 === 0;

                                    if (isInt && lookupIdItem.id === parseInt(recordValue))
                                        lookupIds.push(lookupIdItem);
                                    else if (lookupIdItem.value == recordValue)
                                        lookupIds.push(lookupIdItem);
                                }
                            }

                            var lookupModule = $filter('filter')($rootScope.modules, { name: field.lookup_type })[0];

                            if (field.lookup_type === 'users') {
                                lookupModule = {
                                    languages: {}
                                };
                                lookupModule.languages[$rootScope.globalization.Label] = {
                                    label: {
                                        singular: 'User'
                                    }
                                };
                                //lookupModule.label_tr_singular = 'Kullanıcı';
                                //lookupModule.label_en_singular = 'User';
                            }

                            if (lookupIds.length > 1) {
                                $scope.error.message = $filter('translate')('Data.Import.Error.LookupMoreThanOne', { module: $rootScope.getLanguageValue(lookupModule.languages, "label", "singular") });
                                recordValue = null;
                                return;
                            }

                            var lookupId = lookupIds[0];

                            if (!lookupId) {
                                $scope.error.message = $filter('translate')('Data.Import.Error.LookupNotFound', { module: $rootScope.getLanguageValue(lookupModule.languages, "label", "singular") });
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

                    if (recordValue !== null && recordValue !== undefined) {
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
                    var newFieldMap = prepareFieldMap($scope.fieldMap);
                    for (var fieldMapKey in newFieldMap) {
                        if (fieldMapKey === 'fixed' || fieldMapKey === 'fixedFormat') //To added excel import mapping
                            continue;

                        if (newFieldMap.hasOwnProperty(fieldMapKey)) {
                            var fieldMapValue = newFieldMap[fieldMapKey];

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
                                    $scope.error.fieldLabel = $rootScope.getLanguageValue(field.languages, "label");
                                    $scope.error.message = $filter('translate')('Data.Import.Error.Required');
                                    break;
                                }

                                if (!cellValue && cellValue !== 0)
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
                var newFieldMap = prepareFieldMap($scope.fieldMap);

                if (!$scope.processing) {
                    for (var i = 0; i < $scope.rows.length; i++) {
                        var row = $scope.rows[i];
                        /**Eğer import edilmek istenen excel'de owner sütunu boş ise
                         *defaultta işlemi  yapan kullanıcıyı setle
                         **/
                        if (!row[newFieldMap["owner"]]) {
                            row[newFieldMap["owner"]] = $rootScope.user.full_name;
                        } else {
                            var isRowOwnerRight = $filter('filter')($scope.users, function (user) {
                                var fullname = user.first_name + " " + user.last_name;
                                return fullname.trim().toLowerCaseTurkish() === row[newFieldMap["owner"]].toString().trim().toLowerCaseTurkish();
                            }, true)[0];

                            if (!isRowOwnerRight)
                                row[$scope.fieldMap["owner"]] = $rootScope.user.full_name;
                        }

                        for (var rowKey in row) {
                            if (row.hasOwnProperty(rowKey)) {
                                var cellValue = row[rowKey].toString().trim();

                                for (var fieldMapKey in newFieldMap) {
                                    if (newFieldMap.hasOwnProperty(fieldMapKey)) {
                                        var fieldMapValue = newFieldMap[fieldMapKey];

                                        if (fieldMapKey !== 'fixed' && rowKey === fieldMapValue) {
                                            var field = angular.copy($filter('filter')($scope.module.fields, { name: fieldMapKey }, true)[0]);

                                            if (field.data_type !== 'lookup')
                                                break;

                                            if (field.lookup_type === 'relation')
                                                field.lookup_type = $scope.fixedValue.related_module.value;

                                            var lookupItem = $filter('filter')(lookupRequest, { type: field.lookup_type }, true)[0];

                                            if (!lookupItem) {
                                                lookupItem = {};
                                                lookupItem.type = field.lookup_type;
                                                lookupItem.values = [];

                                                if (field.lookup_type !== 'users') {
                                                    var lookupModule = $filter('filter')($rootScope.modules, { name: field.lookup_type }, true)[0];
                                                    if (lookupModule) {
                                                        var field = $filter('filter')(lookupModule.fields, { primary_lookup: true }, true)[0];
                                                        if (field) {
                                                            lookupItem.field = field.name;
                                                        } else {
                                                            field = $filter('filter')(lookupModule.fields, { primary: true }, true)[0];
                                                            if (field)
                                                                lookupItem.field = field.name
                                                        }
                                                    }

                                                }
                                                else {
                                                    if (cellValue.indexOf('@') > -1)
                                                        lookupItem.field = 'email';
                                                    else
                                                        lookupItem.field = 'full_name';
                                                }

                                                if (cellValue && lookupItem.values.indexOf(cellValue) < 0) {
                                                    lookupRequest.push(lookupItem);
                                                }

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
                }

                if (!lookupRequest.length) {
                    deferred.resolve([]);
                }
                else {
                    if (!$scope.processing) {
                        $scope.processing = true;
                        ImportService.getLookupIds(lookupRequest)
                            .then(function (lookupIds) {
                                deferred.resolve(lookupIds.data);
                                $scope.processing = false;
                            })
                            .catch(function (reason) {
                                deferred.reject(reason.data);
                                $scope.processing = false;
                            });
                    }
                }

                return deferred.promise;
            };

            $scope.submit = function (retry) {
                $scope.error = null;
                $scope.errorUnique = null;
                $scope.processing = false;

                if (!$scope.importForm.validate()) {
                    mdToast.warning($filter('translate')('Module.RequiredError'));
                    return;
                }

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
                            if (retry)
                                $scope.fieldMap = $scope.fieldMapOld;
                            else
                                $scope.fieldMapOld = $scope.fieldMap;

                            $scope.lookupIds = lookupIds;
                            $scope.records = $scope.prepareRecords();

                            if ($scope.error && $scope.error.message != null) {
                                uploader.clearQueue();
                            }

                            components.run('BeforeImport', 'Script', $scope);
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

            };

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
                        $scope.fileName = null;

                        $timeout(function () {
                            $scope.saving = false;
                            $scope.longProcessing = false;
                            uploader.uploadAll();
                            mdToast.success({ content: $filter('translate')('Data.Import.Success'), timeout: 6000 });
                            $state.go('app.moduleList', { type: $scope.type });
                        }, 500);
                    })
                    .catch(function (data) {
                        if (data.status === 409) {
                            $scope.errorUnique = {};
                            $scope.errorUnique.field = data.data.field;

                            if (data.data.field2)
                                $scope.errorUnique.field = data.data.field2;
                            uploader.clearQueue();
                        }

                        $scope.saving = false;
                        $scope.longProcessing = false;
                    });
            };

            $scope.combinationFilter = function (field) {
                return !field.combination;

            };

            function getJsDateFromExcel(excelDate) {
                return new Date((excelDate - (25567 + 2)) * 86400 * 1000);
            }

            function getExcelBaseValue(field, rowNo, cellName) {
                //var row = $scope.rowsBase[rowNo];
                const row = $scope.rows[rowNo];
                if (row)
                    return row[cellName].toString();
            }

            $scope.mappingControl = function () {

                if (!$scope.importForm.validate())
                    $scope.wizardStep = 2;
            };

            //For Kendo UI
            $scope.cancel = function () {
                $mdDialog.cancel();
            };

            var dataSource = [];

            angular.element(document).ready(function () {
                $scope.excelCellOptions = function (field, change) {
                    dataSource = [];

                    if (field.name !== 'related_module')
                        dataSource = angular.copy($scope.cells);

                    fixed.name = $scope.fixedValue[field.name] ? $scope.fixedValueFormatted[field.name] : $filter('translate')('Data.Import.FixedValue') + ' -->';
                    dataSource.push(fixed);
                    dataSource.push(defaultCurrentUser);

                    if (change) {
                        var dropdown = angular.element("#" + field.name).data("kendoDropDownList");
                        dropdown.setDataSource(dataSource);
                    }
                    else
                        return {
                            dataSource: dataSource,
                            autoBind: true,
                            dataTextField: "name",
                            dataValueField: "column",
                        };
                };

                $scope.fieldMap = angular.copy($scope.fieldMapCopy);
            });

            $scope.mappingOptions = {
                dataSource: {
                    transport: {
                        read: {
                            url: "/api/data/import_get_all_mappings/" + $scope.module.id,
                            type: 'GET',
                            dataType: "json",
                            beforeSend: $rootScope.beforeSend(),
                            cache: false
                        }
                    },
                },
                dataTextField: "name",
                dataValueField: "id",
                autoBind: true
            };

            $scope.dateMinValidationControl = function (fixedField) {
                return fixedField.validation.min ? (fixedField.validation.min === 'today' ? $scope.currentDayMin : fixedField.validation.min) : new Date(2000, 0, 1, 0, 0, 0);
            };

            $scope.dateMaxValidationControl = function (fixedField) {
                return fixedField.validation.max ? (fixedField.validation.max === 'today' ? $scope.currentDayMax : fixedField.validation.max) : new Date(2099, 0, 1, 0, 0, 0);
            };

            $scope.fixedSelectListOption = function (fixedField) {
                var data = $filter('filter')($scope.picklistsModule[fixedField.picklist_id], { inactive: '!true', hidden: '!true' }, true);
                return {
                    dataSource: data,
                    dataTextField: "languages." + $rootScope.globalization.Label + ".label",
                    dataValueField: "id",
                }
            };

            $scope.fixedMultiSelect = function (fixedField) {
                var data = $filter('filter')($scope.picklistsModule[fixedField.picklist_id], { inactive: '!true', hidden: '!true' }, true);
                return {
                    placeholder: $filter('translate')('Common.MultiselectPlaceholder'),
                    dataTextField: "languages." + $rootScope.globalization.Label + ".label",
                    valuePrimitive: true,
                    autoBind: false,
                    dataSource: data
                }
            };

            $scope.fixedLookupOptions = function (fixedField) {
                return {
                    dataSource: new kendo.data.DataSource({
                        serverFiltering: true,
                        transport: {
                            read: function (options) {
                                const findRequest = {
                                    module: fixedField.lookup_type,
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

                                    if (fixedField && fixedField.filters.length > 0) {
                                        for (var h = 0; h < fixedField.filters.length; h++) {
                                            const filter = fixedField.filters[h];
                                            options.data.filter.filters.push({
                                                field: filter.filter_field,
                                                operator: filter.operator,
                                                value: filter.value
                                            });
                                        }
                                    }

                                    var defaultLookupFilter = {
                                        field: fixedField.lookupModulePrimaryField.name,
                                        operator: fixedField.lookup_search_type && fixedField.lookup_search_type !== "" ? fixedField.lookup_search_type.replace('_', '') : "startswith",
                                        value: ''
                                    };

                                    if (fixedField.lookupModulePrimaryField.data_type === 'number' || fixedField.lookupModulePrimaryField.data_type === 'number_auto') {
                                        defaultLookupFilter.operator = 'not_empty'// value !== '' ? 'equals' : 'not_empty';
                                        defaultLookupFilter.value = defaultLookupFilter.value === 'not_empty' ? '-' : defaultLookupFilter.value;
                                    }

                                }
                                if (options.data.filter.filters.length > 0) {
                                    //default lookup operator = starts_with or contains its come from studio
                                    //if lookup primary field's data_type equal number or auto_number we have to change default operator.
                                    processLookupOperator(options.data.filter.filters, fixedField.lookupModulePrimaryField);
                                    options.data.filter.filters = ModuleService.fieldLookupFilters(fixedField, $scope.record, options.data.filter.filters, findRequest);
                                    findRequest.fields.push(fixedField.lookupModulePrimaryField.name);
                                    if (fixedField.lookupModulePrimaryField.data_type === 'number' || fixedField.lookupModulePrimaryField.data_type === 'number_auto') {

                                        if (!options.data.filter.filters[0].operator)
                                            options.data.filter.filters[0].operator = 'equals';

                                    } else if (options.data.filter.filters.length > 0) {
                                        const operator = options.data.filter.filters[0].operator;
                                        if (operator.contains('_'))
                                            options.data.filter.filters[0].operator = operator !== "" ? operator.replace('_', '') : "startswith";
                                    }
                                }

                                $.ajax({
                                    url: '/api/record/find_custom',
                                    contentType: 'application/json',
                                    dataType: 'json',
                                    type: 'POST',
                                    data: JSON.stringify(Object.assign(findRequest, options.data)),
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
                        },
                    }),
                    optionLabel: $filter('translate')('Common.Select'),
                    autoBind: false,
                    dataTextField: fixedField.lookupModulePrimaryField.name,
                    dataValueField: "id"
                };
            };
            //For Kendo UI

            function filterCells(key) {
                return $filter('filter')($scope.cells, { column: key }, true)[0];
            }

            function prepareFieldMap(fieldMap) {
                var newFieldMap = {};
                for (var key in fieldMap) {
                    newFieldMap[key] = fieldMap[key].column;
                }
                return newFieldMap;
            }

            $scope.getColumnName = function (fieldName) {
                var obj = $scope.fieldMap[fieldName];
                if (obj) {
                    return obj.column;
                }
            };

            $scope.getFields = function (sectionName, sectionColumn) {
                var result = $filter('filter')($scope.module.fields, function (field) {
                    return !field.deleted && field.data_type !== 'number_auto' && field.section === sectionName && field.section_column === sectionColumn && !field.combination
                }, true);

                return $filter('orderBy')(result, 'order');
            };

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
