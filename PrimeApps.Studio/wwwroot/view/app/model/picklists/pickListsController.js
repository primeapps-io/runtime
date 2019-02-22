'use strict';

angular.module('primeapps')

    .controller('pickListsController', ['$rootScope', '$scope', '$state', '$stateParams', 'PickListsService', '$modal', 'dragularService', '$timeout', '$interval',
        function ($rootScope, $scope, $state, $stateParams, PickListsService, $modal, dragularService, $timeout, $interval) {
            $scope.$parent.activeMenuItem = 'picklists';
            $rootScope.breadcrumblist[2].title = 'Picklists';
            $scope.loading = true;
            $scope.loadingItem = true;
            $scope.pageOfItem;

            $scope.requestModel = { //default page value
                limit: "10",
                offset: 0,
                order_column: "label_en"
            };

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }
            };

            $scope.generator(10);



            PickListsService.getPage($scope.requestModel).then(function (response) {
                if (response.data) {
                    $scope.picklists = response.data;

                    PickListsService.count().then(function (count) {
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
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;

                PickListsService.getPage(requestModel).then(function (response) {

                    $scope.picklists = response.data;
                    $scope.loading = false;
                }).catch(function (reason) {
                    $scope.loading = false;
                });

            };

            $scope.changePageItem = function (page) {
                $scope.loadingItem = true;

                PickListsService.get($scope.id).then(function (response) {
                    $scope.picklist = response.data;
                    PickListsService.countItems($scope.id)
                        .then(function (count) {
                            if (count.data) {
                                $scope.pageTotalItems = count.data;
                                $scope.loadingItem = false;
                            }
                        }).catch(function (reason) {
                            $scope.loadingItem = false;
                            $scope.picklistFormModal.hide();
                        });
                }).catch(function (reason) {
                    $scope.loadingItem = false;
                    $scope.picklistFormModal.hide();
                });

            };

            $scope.changeOffset = function (value) {
                $scope.changePage(value);
            };

            $scope.selectPicklist = function (id) {
                PickListsService.get(id)
                    .then(function (response) {
                        if (response.data) {
                            $scope.picklist = response.data;
                            $scope.modalLoading = false;
                            $scope.bindPicklistDragDrop();
                        }
                    }).catch(function (reason) {
                        $scope.modalLoading = false;
                        $scope.picklistFormModal.hide();
                    });
            };

            //Modal Start
            $scope.showFormModal = function (picklist) {
                $scope.modalLoading = true;

                if (picklist) {
                    $scope.picklist = picklist;
                    $scope.id = picklist.id;
                    $scope.selectPicklist(picklist.id);
                }
                else
                    $scope.modalLoading = false;

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

            $scope.cancel = function () {
                $scope.picklistFormModal.hide();
                $scope.id = null;
                $scope.picklist = {};
            }

            $scope.delete = function (id) {
                if (id) {
                    PickListsService.delete(id)
                        .then(function (response) {
                            if (response.data) {
                                toastr.success($filter('translate')('Picklist.DeleteSuccess'));
                                $scope.changePage(1);
                            }
                        }).catch(function (reason) {
                            $scope.loading = false;
                        });
                }
            }

            $scope.deleteItem = function (id) {
                if ($scope.picklist && id) { 
                    PickListsService.deleteItem(id)
                        .then(function (response) {
                            if (response.data) {
                                toastr.success($filter('translate')('Picklist.DeleteItemSuccess'));
                            }
                        }).catch(function (reason) {
                            $scope.loading = false;
                        });
                }
            }
             
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
                            return handle.classList.contains('option-handle');
                        }
                    });

                    angular.element(picklistContainer).on('dragulardrop', function () {
                        var picklistSortOrder = $filter('filter')($scope.sortOrderTypes, { value: 'order' }, true)[0];
                        $scope.currentField.picklist_sortorder = picklistSortOrder;
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