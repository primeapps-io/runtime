'use strict';

angular.module('ofisim')

    .controller('ModuleListController', ['$rootScope', '$scope', 'ngToast', '$filter', 'helper', '$location', '$state', '$stateParams', '$q', '$window', '$localStorage', '$cache', 'config', 'ngTableParams', 'blockUI', 'exportFile', '$popover', '$modal', 'operations', 'activityTypes', 'transactionTypes', 'ModuleService',
        function ($rootScope, $scope, ngToast, $filter, helper, $location, $state, $stateParams, $q, $window, $localStorage, $cache, config, ngTableParams, blockUI, exportFile, $popover, $modal, operations, activityTypes, transactionTypes, ModuleService) {
            $scope.type = $stateParams.type;
            $scope.operations = operations;
            $scope.hasPermission = helper.hasPermission;
            $scope.activityTypes = activityTypes;
            $scope.transactionTypes = transactionTypes;
            $scope.loading = true;
            $scope.module = $filter('filter')($rootScope.modules, { name: $scope.type }, true)[0];
            $scope.lookupUser = helper.lookupUser;
            $scope.searchingDocuments = false;
            $scope.isAdmin = $rootScope.user.profile.HasAdminRights;

            if (!$scope.module) {
                ngToast.create({ content: $filter('translate')('Common.NotFound'), className: 'warning' });
                $state.go('app.crm.dashboard');
                return;
            }

            $scope.bulkUpdate = {};
            $scope.filter = {};

            if (!$scope.hasPermission($scope.type, $scope.operations.read)) {
                ngToast.create({ content: $filter('translate')('Common.Forbidden'), className: 'warning' });
                $state.go('app.crm.dashboard');
                return;
            }

            $scope.fields = [];
            $scope.selectedRows = [];
            $scope.selectedRecords = [];
            $scope.isAllSelected = false;
            $scope.currentUser = ModuleService.processUser($rootScope.user);
            $scope.currentDayMin = helper.getCurrentDateMin().toISOString();
            $scope.currentDayMax = helper.getCurrentDateMax().toISOString();
            $scope.currentHour = helper.floorMinutes(new Date());
            var tableBlockUI = blockUI.instances.get('tableBlockUI');
            var counts = [10, 25, 50, 100];
            var cacheKey = $scope.module.name + '_' + $scope.module.name;
            $scope.allFields = [];

            for (var i = 0; i < $scope.module.fields.length; i++) {
                var field = $scope.module.fields[i];

                if (ModuleService.hasFieldDisplayPermission(field))
                    $scope.allFields.push(angular.copy(field));
            }
            if ($stateParams.viewid) {
                $scope.viewid = $stateParams.viewid;
            } else {
                $scope.viewid = null;
            }

            if ($scope.module.name === 'activities') {
                var activityTypesActive = $filter('filter')(activityTypes, { hidden: '!true' });

                if (activityTypesActive && activityTypesActive.length === 1)
                    $scope.activityTypeActive = activityTypesActive[0];
            }

            ModuleService.setTable($scope, tableBlockUI, counts, 10, null, $scope.module.name, $scope.type, true);

            $scope.refresh = function (clearFilter) {
                $cache.remove(cacheKey);

                if (clearFilter) {
                    $scope.tableParams.filterList = null;
                    $scope.tableParams.refreshing = true;
                }

                $scope.tableParams.reloading = true;
                $scope.tableParams.reload();
            };

            //Sets holidays to business days
            var setHolidays = function () {
                if ($scope.module.name === 'leaves' || $scope.module.name === 'izinler') {
                    var holidaysModule = $filter('filter')($rootScope.modules, { name: 'holidays' }, true)[0];

                    if (holidaysModule) {
                        var countryField = $filter('filter')(holidaysModule.fields, { name: 'country' }, true)[0];

                        helper.getPicklists([countryField.picklist_id])
                            .then(function (picklists) {
                                var countryPicklist = picklists[countryField.picklist_id];
                                var countryPicklistItemTr = $filter('filter')(countryPicklist, { value: 'tr' }, true)[0];
                                var countryPicklistItemEn = $filter('filter')(countryPicklist, { value: 'en' }, true)[0];
                                var language = window.localStorage['NG_TRANSLATE_LANG_KEY'] || 'tr';
                                var request = {};
                                request.limit = 1000;

                                if ($rootScope.language === 'tr')
                                    request.filters = [{ field: 'country', operator: 'equals', value: countryPicklistItemTr.labelStr, no: 1 }];
                                else
                                    request.filters = [{ field: 'country', operator: 'is', value: countryPicklistItemEn.labelStr }];

                                ModuleService.findRecords('holidays', request)
                                    .then(function (response) {
                                        var data = response.data;
                                        var holidays = [];
                                        for (var i = 0; i < data.length; i++) {
                                            var date = moment(data[i].date).format('DD-MM-YYYY');
                                            holidays.push(date);
                                        }
                                        var workingWeekdays = [1, 2, 3, 4, 5];

                                        var workSaturdays = $filter('filter')($rootScope.moduleSettings, { key: 'work_saturdays' }, true);
                                        if (workSaturdays.length > 0 && workSaturdays[0].value === 't') {
                                            workingWeekdays.push(6);
                                        }

                                        moment.locale(language, {
                                            week: { dow: 1 }, // Monday is the first day of the week.
                                            workingWeekdays: workingWeekdays, // Set working weekdays.
                                            holidays: holidays,
                                            holidayFormat: 'DD-MM-YYYY'
                                        });
                                    });
                            });
                    }
                }
            };

            setHolidays();

            $scope.collectiveLeave = function () {
                if ($scope.mailModal) {
                    $scope.mailModal.show();
                }

                $scope.mailModal = $scope.mailModal || $modal({
                    scope: $scope,
                    templateUrl: 'web/views/app/leave/collectiveLeave.html',
                    backdrop: 'static',
                    show: true
                });
            };

            $scope.delete = function (id) {
                ModuleService.getRecord($scope.module.name, id)
                    .then(function (recordData) {
                        if (!helper.hasRecordEditPermission(recordData.data)) {
                            ngToast.create({ content: $filter('translate')('Common.Forbidden'), className: 'warning' });
                            return;
                        }

                        ModuleService.deleteRecord($scope.module.name, id)
                            .then(function () {
                                $cache.remove(cacheKey);
                                $scope.tableParams.reload();
                            });
                    });
            };

            $scope.hideCreateNew = function (field) {
                if (field.lookup_type === 'users')
                    return true;

                if (field.lookup_type === 'relation' && !$scope.record.related_module)
                    return true;

                return false;
            };
            $scope.multiselect = function (searchTerm, field) {
                var picklistItems = [];

                for (var i = 0; i < $scope.tableParams.picklists[field.picklist_id].length; i++) {
                    var picklistItem = $scope.tableParams.picklists[field.picklist_id][i];

                    if (picklistItem.inactive)
                        return;

                    if (picklistItem.labelStr.toLowerCase().indexOf(searchTerm) > -1)
                        picklistItems.push(picklistItem);
                }

                return picklistItems;
            };

            $scope.changeView = function () {
                tableBlockUI.start();
                $scope.selectedView = $scope.view;
                var cache = $cache.get(cacheKey);
                var viewStateCache = cache.viewState;
                var viewState = viewStateCache || {};
                viewState.active_view = $scope.selectedView.id;

                ModuleService.setViewState(viewState, $scope.module.id, viewState.id)
                    .then(function () {
                        tableBlockUI.stop();
                        $scope.refresh(true);
                    });
            };

            $scope.deleteView = function (id) {
                ModuleService.deleteView(id)
                    .then(function () {
                        $scope.view = $filter('filter')($scope.views, { active: true })[0];
                        $scope.changeView();
                    });
            };

            $scope.export = function () {
                if ($scope.tableParams.total() < 1)
                    return;

                var isFileSaverSupported = false;

                try {
                    isFileSaverSupported = !!new Blob;
                } catch (e) {
                }

                if (!isFileSaverSupported) {
                    ngToast.create({
                        content: $filter('translate')('Module.ExportUnsupported'),
                        className: 'warning',
                        timeout: 8000
                    });
                    return;
                }

                if ($scope.tableParams.total() > 3000) {
                    ngToast.create({
                        content: $filter('translate')('Module.ExportWarning'),
                        className: 'warning',
                        timeout: 8000
                    });
                    return;
                }

                var fileName = $scope.module['label_' + $rootScope.language + '_plural'] + '-' + $filter('date')(new Date(), 'dd-MM-yyyy') + '.xls';
                $scope.exporting = true;

                ModuleService.getCSVData($scope, $scope.type)
                    .then(function (csvData) {
                        ngToast.create({
                            content: $filter('translate')('Module.ExcelExportSuccess'),
                            className: 'success',
                            timeout: 5000
                        });
                        exportFile.excel(csvData, fileName);
                        $scope.exporting = false;
                    });
            };

            $scope.showActivityButtons = function () {
                $scope.activityButtonsPopover = $scope.activityButtonsPopover || $popover(angular.element(document.getElementById('activityButtons')), {
                    templateUrl: 'web/views/common/newactivity.html',
                    placement: 'bottom',
                    autoClose: true,
                    scope: $scope,
                    show: true
                });
            };

            $scope.showTransactionButtons = function () {
                $scope.transactionButtonsPopover = $scope.transactionButtonsPopover || $popover(angular.element(document.getElementById('transactionButtons')), {
                    templateUrl: 'web/views/common/newtransaction.html',
                    placement: 'bottom',
                    autoClose: true,
                    scope: $scope,
                    show: true
                });
            };

            $scope.showDataTransferButtons = function () {
                $scope.dataTransferButtonsPopover = $scope.dataTransferButtonsPopover || $popover(angular.element(document.getElementById('dataTransferButtons')), {
                    template: 'web/views/common/datatransfer.html',
                    placement: 'bottom',
                    autoClose: true,
                    scope: $scope,
                    show: true
                });
            };

            $scope.selectRow = function ($event, record) {
                /*selects or unselects records*/
                if ($event.target.checked) {
                    record.fields.forEach(function (field) {
                        /*find primary field and get its value*/
                        if (field.primary == true) {
                            /*add selected record*/
                            $scope.selectedRows.push(record.id);

                            $scope.selectedRecords.push({
                                id: record.id,
                                displayName: field.valueFormatted
                            });

                            return;
                        }
                    });
                } else {
                    $scope.selectedRows = $scope.selectedRows.filter(function (selectedItem) {
                        return selectedItem != record.id;
                    });

                    $scope.selectedRecords = $scope.selectedRecords.filter(function (selectedItem) {
                        return selectedItem.id != record.id;
                    });
                }
                $scope.isAllSelected = false;
            };

            $scope.isRowSelected = function (id) {
                return $scope.selectedRows.filter(function (selectedItem) {
                    return selectedItem == id;
                }).length > 0;
            };

            $scope.selectAll = function ($event, data) {
                $scope.selectedRows = [];

                if ($scope.isAllSelected) {
                    $scope.isAllSelected = false;
                    $scope.selectedRecords = [];
                } else {
                    $scope.isAllSelected = true;

                    for (var i = 0; i < data.length; i++) {
                        var record = data[i];

                        for (var j = 0; j < record.fields.length; j++) {
                            var field = record.fields[j];
                            if (field.primary) {
                                $scope.selectedRows.push(record.id)
                            }
                        }
                    }
                }
            };

            $scope.deleteSelecteds = function () {
                if (!$scope.selectedRows || !$scope.selectedRows.length) {
                    ngToast.create({ content: $filter('translate')('Module.NoRecordSelected'), className: 'warning' });
                    return;
                }
                ModuleService.deleteRecordBulk($scope.module.name, $scope.selectedRows)
                    .then(function () {
                        $cache.remove(cacheKey);
                        $scope.tableParams.reloading = true;
                        $scope.tableParams.reload();
                        ngToast.create({ content: $filter('translate')('Silme işleminiz başarıyla gerçekleşti. '), className: 'success' });
                        $scope.selectedRows = [];
                        $scope.isAllSelected = false;
                    });
            };

            $scope.addCustomField = function ($event, customField) {
                /// adds custom fields to the html template.
                tinymce.activeEditor.execCommand('mceInsertContent', false, "{" + customField.name + "}");
            };

            $scope.showEMailModal = function () {
                if (!$rootScope.system.messaging.SystemEMail && !$rootScope.system.messaging.PersonalEMail) {
                    ngToast.create({ content: $filter('translate')('EMail.NoProvider'), className: 'warning' });
                    return;
                }

                if ($scope.selectedRows.length == 0 && !$scope.isAllSelected) {
                    ngToast.create({ content: $filter('translate')('Module.NoRecordSelected'), className: 'warning' });
                    return;
                }
                /*Generates and displays modal form for the mail*/
                $scope.mailModal = $scope.mailModal || $modal({
                    scope: $scope,
                    templateUrl: 'web/views/app/email/bulkEMailModal.html',
                    backdrop: 'static',
                    show: false
                });

                $scope.mailModal.$promise.then($scope.mailModal.show);
            };

            $scope.showSMSModal = function () {
                if (!$rootScope.system.messaging.SMS) {
                    ngToast.create({ content: $filter('translate')('SMS.NoProvider'), className: 'warning' });
                    return;
                }

                if ($scope.selectedRows.length == 0 && !$scope.isAllSelected) {
                    ngToast.create({ content: $filter('translate')('Module.NoRecordSelected'), className: 'warning' });
                    return;
                }

                /*Generates and displays modal form for the mail*/
                $scope.smsModal = $scope.smsModal || $modal({
                    scope: $scope,
                    templateUrl: 'web/views/app/sms/bulkSMSModal.html',
                    backdrop: 'static',
                    show: false
                });

                $scope.smsModal.$promise.then($scope.smsModal.show);
            };

            $scope.dropdownHide = function () {
                var element = angular.element(document.getElementById('dropdownMenu1'));
                if (element[0]) element[0].click();
                if (element[1]) element[1].click();
            };

            $scope.showLightBox = function (recordData, Index) {
                $scope.lightBox = true;
                $scope.recordData = recordData;
                $scope.Index = Index;
            };

            $scope.closeLightBox = function () {
                $scope.lightBox = false
            };


            //bulkUpdate
            var field = $filter('filter')($scope.module.fields, { name: name }, true)[0];

            $scope.setCurrentLookupField = function (field) {
                $scope.currentLookupField = field;
            };

            $scope.inputReset = function () {
                $scope.bulkUpdate.value = null;


            };

            $scope.updateSelected = function (bulkUpdateModalForm) {
                if (!$scope.selectedRows || !$scope.selectedRows.length)
                    return;

                function validate() {
                    var isValid = true;

                    angular.forEach($scope.module.fields, function (field) {
                        if (!$scope.bulkUpdate.value)
                            return;

                        if ($scope.bulkUpdate.field.data_type === 'lookup' && typeof $scope.bulkUpdate.value != 'object') {
                            bulkUpdateModalForm[field.name].$setValidity('object', false);
                            isValid = false;
                        }

                    });

                    return isValid;
                }

                if (!bulkUpdateModalForm.$valid || !validate())
                    return;

                $scope.submittingModal = true;
                var request = {};
                var fieldName = $scope.bulkUpdate.field.name;
                request.ids = $scope.selectedRows;
                request.record = {};
                request.record[fieldName] = $scope.bulkUpdate.value;
                request.record = ModuleService.prepareRecord(request.record, $scope.module);


                ModuleService.updateRecordBulk($scope.module.name, request)
                    .then(function () {
                        $scope.updateModal.hide();
                        $scope.submittingModal = false;
                        ngToast.create({ content: $filter('translate')('Güncelleme işleminiz başarıyla gerçekleşti. '), className: 'success' });
                        $cache.remove(cacheKey);
                        $scope.tableParams.reloading = true;
                        $scope.tableParams.reload();
                        $scope.isAllSelected = false;
                        $scope.bulkUpdate.value = null;
                    });
            };

            $scope.UpdateMultiselect = function (searchTerm, field) {
                var picklistItems = [];

                angular.forEach($scope.modulePicklists[field.picklist_id], function (picklistItem) {
                    if (picklistItem.inactive)
                        return;

                    if (picklistItem.labelStr.toLowerCase().indexOf(searchTerm) > -1)
                        picklistItems.push(picklistItem);
                });

                return picklistItems;
            };

            ModuleService.getPicklists($scope.module, true)
                .then(function (picklists) {
                    $scope.modulePicklists = picklists;


                    for (var i = 0; i < 5; i++) {
                        var filter = {};
                        filter.field = null;
                        filter.operator = null;
                        filter.value = null;
                        filter.no = i + 1;
                    }
                    if (name.indexOf('.') > -1) {
                        name = name.split('.')[0];

                    }

                    var fieldValue = null;

                    if (!field)
                        return;

                    switch (field.data_type) {
                        case 'picklist':
                            fieldValue = $filter('filter')($scope.modulePicklists[field.picklist_id], { labelStr: value }, true)[0];
                            break;
                        case 'multiselect':
                            fieldValue = [];
                            var multiselectValue = value.split('|');

                            angular.forEach(multiselectValue, function (picklistLabel) {
                                var picklist = $filter('filter')($scope.modulePicklists[field.picklist_id], { labelStr: picklistLabel }, true)[0];

                                if (picklist)
                                    fieldValue.push(picklist);
                            });
                            break;
                        case 'lookup':
                            if (field.lookup_type === 'users') {
                                var user = {};

                                if (value === '0' || value === '[me]') {
                                    user.id = 0;
                                    user.email = '[me]';
                                    user.full_name = $filter('translate')('Common.LoggedInUser');
                                }
                                else {
                                    var userItem = $filter('filter')($rootScope.users, { Id: parseInt(value) }, true)[0];
                                    user.id = userItem.Id;
                                    user.email = userItem.Email;
                                    user.full_name = userItem.FullName;

                                    //TODO: $rootScope.users kaldirilinca duzeltilecek
                                    // ModuleService.getRecord('users', value)
                                    //     .then(function (lookupRecord) {
                                    //         fieldValue = [lookupRecord.data];
                                    //     });
                                }

                                fieldValue = [user];
                            }
                            else {
                                fieldValue = value;
                            }
                            break;
                        case 'date':
                        case 'date_time':
                        case 'time':
                            fieldValue = new Date(value);
                            break;
                        case 'checkbox':
                            fieldValue = $filter('filter')($scope.modulePicklists.yes_no, { system_code: value }, true)[0];
                            break;
                        default:
                            fieldValue = value;
                            break;
                    }

                    $scope.view.filterList[j].field = field;
                    $scope.view.filterList[j].value = fieldValue;

                });

            $scope.customModuleFields = function (items) {
                var fields = [];
                angular.forEach(items, function (item) {
                    if (item.data_type != 'image' && item.data_type != 'location' && item.data_type != 'document' && item.data_type != 'number_auto' && item.data_type != 'text_multi' && (item.validation && !item.validation.readonly) && item.data_type != 'url' && !item.custom_label) {
                        fields.push(item);
                    }
                });
                return fields;
            };

            $scope.showUpdateModal = function () {
                if (!$scope.selectedRows || !$scope.selectedRows.length) {
                    ngToast.create({ content: $filter('translate')('Module.NoRecordSelected'), className: 'warning' });
                    return;
                }
                if ($scope.selectedRows.length > 100) {
                    ngToast.create({ content: $filter('translate')('Module.RecordLimit'), className: 'warning' });
                    return;
                }
                $scope.selected = $scope.selectedRows.length;

                $scope.updateModal = $scope.updateModal || $modal({
                    scope: $scope,
                    templateUrl: 'web/views/app/module/bulkUpdateModal.html',
                    animation: '',
                    backdrop: 'static',
                    show: false,
                    tag: 'createModal'
                });

                $scope.updateModal.$promise.then($scope.updateModal.show);
            };

            $scope.showDeleteModal = function () {

                $scope.selected = $scope.selectedRows.length;

                $scope.deleteModal = $scope.deleteModal || $modal({
                    scope: $scope,
                    templateUrl: 'views/app/crm/module/bulkDelete.html',
                    animation: '',
                    backdrop: 'static',
                    show: false,
                    tag: 'createModal'
                });

                $scope.deleteModal.$promise.then($scope.deleteModal.show);
            };

        }
    ]);