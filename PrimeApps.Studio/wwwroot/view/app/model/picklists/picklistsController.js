'use strict';

angular.module('primeapps')

    .controller('PicklistsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'PicklistsService', '$modal', 'dragularService', '$timeout', '$interval', 'helper', 'FileUploader',
        function ($rootScope, $scope, $filter, $state, $stateParams, PicklistsService, $modal, dragularService, $timeout, $interval, helper, FileUploader) {
            $scope.$parent.activeMenuItem = 'picklists';
            $rootScope.breadcrumblist[2].title = 'Picklists';
            $scope.loading = true;
            $scope.loadingItem = true;
            $scope.addItem = false;
            $scope.editItem = false;
            $scope.orderChanged = false;
            $scope.excelMode = false;
            $scope.pageOfItem;
            $scope.itemModel = {};
            $scope.fieldMap = {};
            $scope.fixedField = {};
            $scope.activePage = 1;
            $scope.picklistModel = {};
            $scope.wizardStep = 0;
            $scope.requestModel = { //default page value
                limit: "10",
                offset: 0,
                order_column: "label_en"
            };

            $scope.fields = [
                { name: "name", label: "Name", required: true, order: 1 },
                { name: "system_code", label: "System Code", required: true, order: 2 },
                { name: "value", label: "Value-1", required: false, order: 3 },
                { name: "value2", label: "Value-2", required: false, order: 4 },
                { name: "value3", label: "Value-3", required: false, order: 5 }];

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }
            };

            $scope.generator(10);



            PicklistsService.getPage($scope.requestModel).then(function (response) {
                if (response.data) {
                    $scope.picklists = response.data;

                    PicklistsService.count().then(function (count) {
                        $scope.pageTotal = count.data;
                        $scope.loading = false;
                    }).catch(function (reason) {
                        $scope.loading = false;
                    });
                }
            }).catch(function (reason) {
                $scope.loadingItem = false;
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

                var requestModel = angular.copy($scope.requestModel);
                if (page != 0)
                    requestModel.offset = page - 1;
                else
                    requestModel.offset = 0;

                PicklistsService.getPage(requestModel).then(function (response) {

                    $scope.picklists = response.data;
                    $scope.loading = false;
                }).catch(function (reason) {
                    $scope.loading = false;
                });

            };

            $scope.changePageItem = function (page) {
                $scope.loadingItem = true;

                PicklistsService.get($scope.id).then(function (response) {
                    $scope.picklist = response.data;
                    PicklistsService.countItems($scope.id)
                        .then(function (count) {
                            if (count.data) {
                                $scope.pageTotalItems = count.data;
                                $scope.loadingItem = false;
                            }
                        }).catch(function (reason) {
                            $scope.loadingItem = false;
                            $scope.cancel();
                        });
                }).catch(function (reason) {
                    $scope.loadingItem = false;
                    $scope.cancel();
                });

            };

            $scope.changeOffset = function () {
                $scope.changePage($scope.activePage);
            };

            $scope.selectPicklist = function (id) {
                $scope.modalLoading = true;
                PicklistsService.get(id)
                    .then(function (response) {
                        if (response.data) {
                            $scope.picklist = response.data;
                        }
                        $scope.modalLoading = false;
                        $scope.bindPicklistDragDrop();

                    }).catch(function (reason) {
                        $scope.modalLoading = false;
                        $scope.cancel();
                    });
            };

            //Modal Start
            $scope.showFormModal = function (picklist, editMode) {
                $scope.modalLoading = true;

                if (editMode) {
                    $scope.picklistModel = picklist;
                    $scope.modalLoading = false;
                }
                else {
                    if (picklist) {
                        $scope.picklist = picklist;
                        $scope.id = picklist.id;
                        $scope.selectPicklist(picklist.id);
                    }
                    else {
                        $scope.picklist = {};
                        $scope.id = null;
                        $scope.modalLoading = false;
                    }

                }



                $scope.picklistFormModal = $scope.picklistForm || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/model/picklists/picklistsForm.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.picklistFormModal.$promise.then(function () {
                    $scope.picklistFormModal.show();
                });
            };

            //Modal Cancel Function
            $scope.cancel = function () {

                if ($scope.excelMode) {
                    $scope.excelMode = false;
                    $scope.wizardStep = 0;
                    $scope.clear();
                }
                else {
                    $scope.picklistFormModal.hide();
                    $scope.orderChanged = false;

                    $timeout(function () {
                        $scope.picklist = {};
                        $scope.picklistModel = {};
                        $scope.itemModel = {};
                        $scope.addItem = false;
                        $scope.id = null;
                    }, 300);
                }
            };

            $scope.addMode = function (state) {
                $scope.addItem = state;
            };

            //$scope.picklistCodeBlur = function () {
            //    $scope.picklistModel.system_code = helper.getSlug($scope.picklistModel.system_code, '_');
            //    $scope.checkNameUnique($scope.picklistModel);
            //};

            $scope.checkNameUnique = function (picklist) {
                if (!picklist || !picklist.system_code)
                    return;

                picklist.system_code = helper.getSlug(picklist.system_code, '_');

                //picklist.system_code = picklist.system_code.replace(/\s/g, '');
                //picklist.system_code = picklist.system_code.replace(/[^a-zA-Z0-9\-]/g, '');

                $scope.picklistNameChecking = true;
                $scope.picklistNameValid = null;

                if (!picklist.system_code || picklist.system_code === '') {
                    picklist.system_code = null;
                    $scope.picklistNameChecking = true;
                    $scope.picklistNameValid = true;
                    return;
                }

                PicklistsService.isUniqueCheck(picklist.system_code)
                    .then(function (response) {
                        $scope.picklistNameChecking = false;
                        if (response.data) {
                            $scope.picklistNameValid = true;
                        }
                        else {
                            $scope.picklistNameValid = false;
                        }
                    })
                    .catch(function () {
                        $scope.picklistNameValid = false;
                        $scope.picklistNameChecking = false;
                    });
            };

            //Picklist save & update function
            $scope.save = function (picklistForm) {
                $scope.saving = true;

                if (picklistForm && !picklistForm.$valid) {
                    picklistForm.$submitted = true;
                    $scope.saving = false;
                    return false;
                }

                if (!$scope.picklistModel.label_en) {
                    $scope.saving = false;
                    return false;
                }

                if ($scope.picklistModel.system_code) {
                    PicklistsService.isUniqueCheck($scope.picklistModel.system_code)
                        .then(function (response) {
                            if (response.data) {
                                saveAction();
                            }
                            else {
                                toastr.warning('Please enter a unique system code!');
                                $scope.saving = false;
                            }
                        });
                }
                else {
                    saveAction();
                }


            };

            var saveAction = function () {
                $scope.picklistModel.label_tr = $scope.picklistModel.label_en;
                $scope.picklistModel.items = [];

                if ($scope.picklistModel.id) {
                    PicklistsService.update($scope.picklistModel)
                        .then(function (response) {
                            if (response.data) {
                                toastr.success($filter('translate')('Picklist.SaveSuccess'));
                            }
                            $scope.saving = false;
                            $scope.cancel();
                            $scope.changeOffset();
                        });
                }
                else {

                    PicklistsService.create($scope.picklistModel)
                        .then(function (response) {
                            if (response.data) {
                                toastr.success($filter('translate')('Picklist.SaveSuccess'));
                                $scope.createdPicklist = response.data;
                            }
                            $scope.saving = false;
                            $scope.cancel();
                            $scope.changeOffset();

                            if (response.data) {
                                $timeout(function () {
                                    $scope.showFormModal($scope.createdPicklist, false);
                                }, 1500);
                            }

                        }).catch(function (reason) {
                            $scope.saving = false;
                        });
                }
            };



            //Picklist Delete Function
            $scope.delete = function (id) {
                if (!id) {
                    $scope.loading = false;
                    return false;
                }

                swal({
                    title: "Are you sure?",
                    text: " ",
                    icon: "warning",
                    buttons: ['Cancel', 'Yes'],
                    dangerMode: true
                }).then(function (value) {
                    if (value) {
                        $scope.loading = true;
                        PicklistsService.delete(id)
                            .then(function (response) {
                                if (response.data) {
                                    toastr.success($filter('translate')('Picklist.DeleteSuccess'));
                                    $scope.changeOffset();
                                }
                            }).catch(function (reason) {
                                $scope.loading = false;
                            });
                    }
                    else
                        $scope.loading = false;
                });

            };

            //Picklist Item Save Function
            $scope.saveItem = function () {
                $scope.itemModel.saving = true;

                if (!$scope.id) {
                    $scope.itemModel.saving = false;
                    $scope.cancel();
                }

                if (!$scope.itemModel.system_code || !$scope.itemModel.label_tr) {
                    $scope.itemModel.saving = false;
                    if (!$scope.itemModel.system_code)
                        toastr.warning('System Code cannot be empty!');
                    else if (!$scope.itemModel.label_tr)
                        toastr.warning('Name cannot be empty!');
                    return false;
                }

                var length = $scope.picklist.items ? $scope.picklist.items.length : 0;
                $scope.itemModel.order = length + 1;

                PicklistsService.createItem($scope.id, $scope.itemModel)
                    .then(function (response) {
                        if (response.data) {
                            toastr.success($filter('translate')('Picklist.SaveItemSuccess'));
                            //$scope.picklistFormModal.hide();
                            $scope.addMode(false);
                            $scope.itemModel = {};
                            $scope.selectPicklist($scope.id);
                        }

                        $scope.itemModel.saving = false;
                    }).catch(function (reason) {
                        if (reason.status === 409)
                            toastr.warning('System code value must be unique!')
                        $scope.itemModel.saving = false;
                    });
            };

            //Picklist items Update Function
            $scope.updateItem = function (item) {
                item.savingItem = true;
                if (!item || !$scope.id) {
                    item.savingItem = false;
                    toastr.warning($filter('translate')('Common.Error'));
                    return false;
                }

                var tempItem = angular.copy(item);
                PicklistsService.updateItem(tempItem.id, tempItem)
                    .then(function (response) {
                        if (response.data) {
                            toastr.success($filter('translate')('Picklist.SaveItemSuccess'));
                            $scope.selectPicklist($scope.id);
                        }
                        item.edit = false;
                        $timeout(function () {
                            item.savingItem = false;
                        }, 300);
                    }).catch(function (reason) {
                        item.savingItem = false;
                    });
            };

            //Picklist Delete Function
            $scope.deleteItem = function (item) {
                item.deletingItem = true;
                if ($scope.picklist && item.id) {
                    PicklistsService.deleteItem(item.id)
                        .then(function (response) {
                            if (response.data) {
                                toastr.success($filter('translate')('Picklist.DeleteItemSuccess'));
                                item.deletingItem = false;
                                $scope.selectPicklist($scope.picklist.id);
                            }
                        }).catch(function (reason) {
                            item.deletingItem = false;
                        });
                }
            };

            //Save Order Button Action Function when changed of item order state
            $scope.orderSave = function () {
                if ($scope.editItem) {
                    toastr.warning('Please save changes.');
                    return;
                }
                $scope.saving = true;

                if (!$scope.picklist && !$scope.picklist.items) {
                    $scope.saving = true;
                    return;
                }

                for (var i = 0; i < $scope.picklist.items.length; i++) {
                    $scope.picklist.items[i].order = i + 1;
                }

                PicklistsService.update($scope.picklist)
                    .then(function (response) {
                        if (response.data) {
                            toastr.success($filter('translate')('Picklist.SaveOrderSuccess'));
                            $scope.saving = false;
                            $scope.orderChanged = false;
                            $scope.selectPicklist($scope.picklist.id);
                        }

                    }).catch(function (reason) {
                        $scope.saving = false;
                        $scope.orderChanged = false;
                    });
            };

            $scope.editModeOpen = function (item) {
                if (!item)
                    return;

                item.edit = true;
                $scope.editItem = true;
                //editModeClase'ta kullanılacak
                $scope.copyPicklistName = item.label_en;
            };

            $scope.editModeClose = function (item) {
                if (!item)
                    return;

                item.edit = false;
                item.label_en = $scope.copyPicklistName;
                $scope.editItem = false;
            };

            //Picklist item system code auto generator
            $scope.systemCodeGenerate = function () {
                if (!$scope.picklistItem['label_' + $scope.language])
                    $scope.itemModel.system_code = '';
                else {
                    var tempCode = $scope.itemModel['label_' + $scope.language].trim();
                    $scope.itemModel.system_code = tempCode.replace(/ /g, '_');
                }

            };


            // Drag & Drop For Items list
            $scope.bindPicklistDragDrop = function () {
                $timeout(function () {
                    if ($scope.drakePicklist) {
                        $scope.drakePicklist.destroy();
                        $scope.drakePicklist = null;
                    }

                    var picklistContainer = document.querySelector('#picklistContainer');
                    var picklistOptionContainer = document.querySelector('#picklistOptionContainer');
                    var rightTopBar = document.getElementById('rightTopBar');
                    var rightBottomBar = document.getElementById('rightBottomBar');
                    var timer;

                    $scope.drakePicklist = dragularService([picklistContainer], {
                        scope: $scope,
                        containersModel: [$scope.picklist.items],
                        classes: {
                            mirror: 'gu-mirror-option pickitemcopy',
                            transit: 'gu-transit-option'
                        },
                        lockY: true,
                        moves: function (el, container, handle) {
                            var result = handle.classList.value.includes('fa-arrows');
                            if (result)
                                $scope.orderChanged = true;

                            return handle.classList.contains('option-handle');
                        }

                    });

                    registerEvents(rightTopBar, picklistOptionContainer, -5);
                    registerEvents(rightBottomBar, picklistOptionContainer, 5);

                    function registerEvents(bar, container, inc, speed) {
                        if (!speed) {
                            speed = 10;
                        }

                        angular.element(bar).on('dragularenter', function () {
                            container.scrollTop += inc;

                            timer = $interval(function moveScroll() {
                                container.scrollTop += inc;
                            }, speed);
                        });

                        angular.element(bar).on('dragularleave dragularrelease', function () {
                            $interval.cancel(timer);
                        });
                    }
                }, 100);
            };


            //Excel  import Area

            var readExcel = function (file, retry) {
                var fileReader = new FileReader();

                fileReader.onload = function (evt) {
                    if (!evt || evt.target.error) {
                        $timeout(function () {
                            toastr.warning($filter('translate')('Module.ExportUnsupported'));
                        });
                        return;
                    }

                    try {
                        $scope.workbook = XLS.read(evt.target.result, { type: 'binary' });
                        $scope.sheets = $scope.workbook.SheetNames;

                        $timeout(function () {
                            $scope.selectedSheet = $scope.selectedSheet || $scope.sheets[0];
                            $scope.selectSheet(retry);

                        });
                    }
                    catch (ex) {
                        $timeout(function () {
                            toastr.warning($filter('translate')('Data.Import.InvalidExcel'));
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
                    toastr.warning($filter('translate')('Data.Import.CountError'));
                    return;
                }

                angular.forEach($scope.headerRow, function (value, key) {
                    var cellName = value + $filter('translate')('Data.Import.ColumnIndex', { index: key });
                    $scope.cells.push({ column: key, name: cellName, used: false });
                });

                if (retry) {
                    $scope.saveMapping();
                    return;
                }

                //$timeout(function () {
                //    if (!$scope.selectedMapping.name) {
                //        $scope.fieldMap = {};
                //        $scope.fixedValue = {};
                //        $scope.fixedValueFormatted = {};

                //        angular.forEach($scope.headerRow, function (value, key) {
                //            var used = false;

                //            if ($rootScope.language === 'tr') {
                //                angular.forEach($scope.module.fields, function (field) {
                //                    if (field.deleted)
                //                        return;

                //                    if (field.label_tr.toLowerCaseTurkish() === value.trim().toLowerCaseTurkish()) {
                //                        used = true;
                //                        $scope.fieldMap[field.name] = key;
                //                    }
                //                });
                //            }
                //            else {
                //                angular.forEach($scope.module.fields, function (field) {
                //                    if (field.deleted)
                //                        return;

                //                    if (field.label_tr.toLowerCase() === value.trim().toLowerCase()) {
                //                        used = true;
                //                        $scope.fieldMap[field.name] = key;
                //                    }
                //                    else {
                //                        if (field && field.default_value) {
                //                            if (field.data_type === 'picklist') {
                //                                $scope.fieldMap[field.name] = 'fixed';
                //                                var picklistValue = $filter('filter')($scope.picklistsModule[field.picklist_id], { id: field.default_value })[0];
                //                                $scope.fixedValue[field.name] = picklistValue;
                //                                $scope.fixedValueFormatted[field.name] = $rootScope.language === 'tr' ? picklistValue.label_tr : picklistValue.label_en;
                //                            }
                //                            else {
                //                                $scope.fieldMap[field.name] = 'fixed';
                //                                $scope.fixedValue[field.name] = field.default_value;
                //                                $scope.fixedValueFormatted[field.name] = field.default_value;
                //                            }
                //                        }
                //                    }
                //                });
                //            }

                //            var cell = $filter('filter')($scope.cells, { column: key }, true)[0];

                //            if (cell)
                //                cell.used = used;
                //        });
                //    }
                //});
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
                        toastr.warning($filter('translate')('Data.Import.FormatError'));
                        break;
                    case 'sizeFilter':
                        toastr.warning($filter('translate')('Data.Import.SizeError'));
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


            $scope.clear = function () {
                uploader.clearQueue();
                $scope.uploader.clearQueue();
                $scope.rows = null;
                $scope.cells = null;
                $scope.sheets = null;
                $scope.fieldMap = null;
                $scope.fixedField = null;
            };

            $scope.prepareItems = function () {
                var items = [];
                $scope.preparing = true;

                var getRecordFieldValue = function (cellValue, field, rowNo, cellName) {
                    var recordValue = '';

                    if (cellValue)
                        recordValue = cellValue.toString().trim();

                    $scope.error = {};
                    $scope.error.rowNo = rowNo;
                    $scope.error.cellName = cellName;
                    $scope.error.cellValue = cellValue;
                    $scope.error.fieldLabel = field['label'];

                    if (recordValue != null && recordValue != undefined) {

                        if (recordValue.toString().length > 100) {
                            $scope.error.message = $filter('translate')('Data.Import.Validation.MaxLength', { maxLength: field.validation.max_length });
                            recordValue = null;
                        }

                        if (recordValue != null && recordValue != undefined)
                            $scope.error = null;

                        return recordValue;
                    }
                };

                for (var i = 0; i < $scope.rows.length; i++) {
                    var item = {};
                    var row = $scope.rows[i];
                    $scope.error = null;

                    for (var fieldMapKey in $scope.fieldMap) {
                        if ($scope.fieldMap.hasOwnProperty(fieldMapKey)) {
                            var fieldMapValue = $scope.fieldMap[fieldMapKey];

                            if (fieldMapValue === 'fixed') {
                                item[fieldMapKey] = $scope.fixedField[fieldMapKey];
                            }
                            else {
                                var field = angular.copy($filter('filter')($scope.fields, { name: fieldMapKey }, true)[0]);
                                var cellValue = row[fieldMapValue];

                                if (field && field.required && !cellValue) {
                                    $scope.error = {};
                                    $scope.error.rowNo = i + 2;
                                    $scope.error.cellName = fieldMapValue;
                                    $scope.error.cellValue = cellValue;
                                    $scope.error.fieldLabel = field['label'];
                                    $scope.error.message = $filter('translate')('Data.Import.Error.Required');
                                    break;
                                }

                                if (!cellValue)
                                    continue;

                                var recordFieldValue = getRecordFieldValue(cellValue, field, i + 2, fieldMapValue);

                                if (angular.isUndefined(recordFieldValue))
                                    break;

                                item[fieldMapKey] = recordFieldValue;
                            }

                            ////if (fieldMapKey === 'system_code') {
                            ////    PicklistsService.isUniqueCheck(recordFieldValue)
                            ////        .then(function (response) {
                            ////            if (!response.data) {
                            ////                $scope.error = {};
                            ////                $scope.error.rowNo = i + 2;
                            ////                $scope.error.cellName = fieldMapValue;
                            ////                $scope.error.cellValue = cellValue;
                            ////                $scope.error.fieldLabel = field['label'];
                            ////                $scope.error.message = $filter('translate')('Data.Import.Error.Unique1', { field1: fieldMapKey }); 
                            ////                $socpe.errorUnique
                            ////            }
                            ////        });  
                            ////}
                        }
                    }

                    if ($scope.error)
                        break;

                    items.push(item);
                }

                $scope.preparing = false;
                return items;
            };

            $scope.saveMapping = function () {
                $scope.items = $scope.prepareItems();
            };

            $scope.tryAgain = function () {
                $scope.trying = true;
                $scope.error = null;
                $scope.errorUnique = null;
                var file = uploader.queue[0]._file;
                readExcel(file, true);
                $scope.trying = false;
            };

            $scope.importValidStep = function (step, ignore) {
                if (!$scope.importForm)
                    return false;

                $scope.importForm.$submitted = true;

                if (!$scope.selectedSheet || !uploader.queue.length) {
                    toastr.warning($filter('translate')('Data.Import.Error.Required'));
                    return false;
                }

                if (!$scope.importForm.$valid && !ignore) {
                    toastr.warning($filter('translate')('Data.Import.Error.Required'));
                    return false;
                }

                $scope.importForm.$submitted = false;
                $scope.wizardStep = step;
                return true;
            }

            $scope.saveImport = function () {
                $scope.saving = true;
                if ($scope.error || !$scope.items || $scope.items.length <= 0)
                    return false;

                PicklistsService.import($scope.id, $scope.items)
                    .then(function (response) {
                        toastr.success("Picklist items are saved successfully.");
                        $scope.cancel();
                        $scope.selectPicklist(picklist.id);
                        $scope.saving = false;
                    })
                    .catch(function (error) {
                        //console.log(error);
                        if (error.data) {
                            $scope.error = {};
                            $scope.error.rowNo = error.data.row + 2;
                            $scope.error.cellValue = error.data.system_code;
                            $scope.error.fieldLabel = error.data.field;
                            $scope.error.message = 'Duplicate value on System Code field is not allowed.';
                        }
                        $scope.saving = false;

                    });
            };
            //Excel  import Area

        }
    ]);