'use strict';

angular.module('primeapps')

    .controller('pickListsController', ['$rootScope', '$scope', '$state', '$stateParams', 'PickListsService', '$modal',
        function ($rootScope, $scope, $state, $stateParams, PickListsService, $modal) {
            $scope.$parent.activeMenuItem = 'picklists';
            $rootScope.breadcrumblist[2].title = 'Picklists';
            $scope.loading = true;
            $scope.wizardStep = 0;

            $scope.requestModel = { //default page value
                limit: "10",
                offset: 0,
                order_column: "name"
            };


            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }
            };

            $scope.generator(10);



            PickListsService.find($scope.requestModel).then(function (response) {
                if (response.data) {
                    $scope.picklists = response.data;

                    PickListsService.count().then(function (count) {
                        $scope.pageTotal = count.data;
                    });

                    $scope.loading = false;
                }
            });

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;

                PickListsService.find(requestModel).then(function (response) {

                    $scope.picklists = response.data;
                    $scope.loading = false;
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
                        }
                    });
            };

            //Modal Start
            $scope.showFormModal = function (id) {
                $scope.modalLoading = true;
                if (id) {
                    $scope.id = id;
                    $scope.selectPicklist(id);
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