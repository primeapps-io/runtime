'use strict';

angular.module('primeapps')

    .controller('PicklistsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'PicklistsService', '$modal', 'dragularService', '$timeout', '$interval', 'helper',
        function ($rootScope, $scope, $filter, $state, $stateParams, PicklistsService, $modal, dragularService, $timeout, $interval, helper) {
            $scope.$parent.activeMenuItem = 'picklists';
            $rootScope.breadcrumblist[2].title = 'Picklists';
            $scope.loading = true;
            $scope.loadingItem = true;
            $scope.addItem = false;
            $scope.editItem = false;
            $scope.orderChanged = false;
            $scope.pageOfItem;
            $scope.itemModel = {};
            $scope.activePage = 1;
            $scope.picklistModel = {};

            $scope.requestModel = { //default page value
                limit: "10",
                offset: 0,
                order_column: "label_en"
            };

            $scope.systemTypes = [
                { name: "System", order: 3 },
                { name: "Custom", order: 2 },
                { name: "Component", order: 1 }];

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
                $scope.picklistFormModal.hide();
                $scope.orderChanged = false;

                $timeout(function () {
                    $scope.picklist = {};
                    $scope.picklistModel = {};
                    $scope.itemModel = {};
                    $scope.addItem = false;
                    $scope.id = null;
                }, 300);
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
                            }

                            $scope.saving = false;
                            $scope.cancel();
                            $scope.changeOffset();

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

                if ((!$scope.itemModel.system_code && !$scope.itemModel.label_tr) || !$scope.id) {
                    $scope.itemModel.saving = false;
                    toastr.warning('Name and System Code cannot be empty!');
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

                PicklistsService.updateItem(item.id, item)
                    .then(function (response) {
                        if (response.data)
                            toastr.success($filter('translate')('Picklist.SaveItemSuccess'));

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
        }
    ]);