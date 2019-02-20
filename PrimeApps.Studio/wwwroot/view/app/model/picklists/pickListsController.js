'use strict';

angular.module('primeapps')

    .controller('pickListsController', ['$rootScope', '$scope', '$state', '$stateParams', 'PickListsService', '$modal',
        function ($rootScope, $scope, $state, $stateParams, PickListsService, $modal) {
            $scope.$parent.activeMenuItem = 'picklists';
            $rootScope.breadcrumblist[2].title = 'Picklists';
            $scope.loading = true;
            $scope.loadingItem = true;

            $scope.requestModel = { //default page value
                limit: "10",
                offset: 0,
                order_column: "label_en"
            };
            $scope.requestModelItem = { //default item page value
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

            $scope.generatorItem = function (limit) {
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
                if ($scope.requestModelItem.limit === null || $scope.requestModelItem.limit === 1)
                    $scope.requestModelItem.limit = "10";

                var requestModel = angular.copy($scope.requestModelItem);
                requestModel.offset = page - 1;

                PickListsService.getItemPage($scope.id, requestModel).then(function (response) {
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

            $scope.changeOffsetItem = function (value) {
                if ($scope.id && $scope.picklist)
                    $scope.changePageItem(value);
                else
                    return;
            };

            $scope.selectPicklist = function (id) {
                PickListsService.get(id)
                    .then(function (response) {
                        if (response.data) {
                            $scope.picklist = response.data;
                            $scope.modalLoading = false;
                        }
                    }).catch(function (reason) {
                        $scope.modalLoading = false;
                        $scope.picklistFormModal.hide();
                    });
            };

            //Modal Start
            $scope.showFormModal = function (picklist) {
                $scope.modalLoading = true;
                $scope.generatorItem(5);

                if (picklist) {
                    $scope.picklist = picklist;
                    $scope.id = picklist.id;
                    // $scope.selectPicklist(picklist.id);
                    $scope.changeOffsetItem(1);
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

        }
    ]);