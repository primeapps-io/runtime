'use strict';

angular.module('primeapps')

    .controller('pickListsFormController', ['$rootScope', '$scope', '$state', '$stateParams', 'PickListsService', '$modal',
        function ($rootScope, $scope, $state, $stateParams, PickListsService, $modal) {
            $scope.loadingItem = true;
            $scope.modalLoading = true;
            $scope.id = $scope.$parent.id;
            $scope.picklist = $scope.$parent.picklist;

            $scope.requestModelItem = { //default item page value
                limit: "10",
                offset: 0,
                order_column: "label_en"
            };

            $scope.generatorItem = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }
            };

            $scope.generatorItem(5);

            $scope.changePageItem = function (page) {
                $scope.loadingItem = true;
                if ($scope.requestModelItem.limit === null || $scope.requestModelItem.limit === 1)
                    $scope.requestModelItem.limit = "10";

                $scope.requestModelItem.offset = page - 1;
                var requestModel = angular.copy($scope.requestModelItem);

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

            $scope.changeOffsetItem = function (value) {
                if ($scope.id && $scope.picklist)
                    $scope.changePageItem(value);
                else
                    return;
            };

            $scope.cancel = function () {
                $scope.picklistFormModal.hide();
                $scope.id = null;
                $scope.picklist = {};
            }

            $scope.delete = function (id) {
                if (id);
            }


        }
    ]);