'use strict';

angular.module('primeapps')

    .controller('ScriptsController', ['$rootScope', '$scope', '$state', 'ScriptsService', '$modal', 'componentPlaces', 'componentPlaceEnums', '$filter',
        function ($rootScope, $scope, $state, ScriptsService, $modal, componentPlaces, componentPlaceEnums, $filter) {
            $scope.$parent.activeMenuItem = 'scripts';
            $rootScope.breadcrumblist[2].title = 'Scripts';
            $scope.scripts = [];
            $scope.scriptModel = {};
            $scope.loading = false;
            $scope.componentPlaces = componentPlaces;
            $scope.componentPlaceEnums = componentPlaceEnums;
            $scope.modules = $rootScope.appModules;

            $scope.requestModel = {
                limit: "10",
                offset: 0
            };

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }
            };

            $scope.generator(10);

            var count = function () {
                ScriptsService.count().then(function (response) {
                    $scope.pageTotal = response.data;
                });
            };
            count();

            $scope.changePage = function (page) {
                $scope.loading = true;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;
                ScriptsService.find(requestModel)
                    .then(function (response) {
                        if (response.data) {
                            $scope.scripts = response.data;
                        }
                        $scope.loading = false;
                    })
                    .catch(function (reason) {
                        $scope.loading = false;
                    });

            };

            $scope.changePage(1);

            $scope.changeOffset = function () {
                $scope.changePage(1)
            };

            $scope.save = function (scriptForm) {
                $scope.saving = true;

                if (!scriptForm.$valid)
                    return;

                if ($scope.id) {
                    ScriptsService.update($scope.scriptModel)
                        .then(function (response) {
                            if (response.data) {
                                toastr.success("Script is updated successfully.")
                            }

                            $scope.saving = false;
                            $scope.cancel();
                            $scope.changeOffset(1);
                        })
                        .catch(function (reason) {
                            toastr.error($filter('translate')('Error'));
                            $scope.saving = false;
                        });

                }
                else {
                    ScriptsService.create($scope.scriptModel)
                        .then(function (response) {
                            if (response.data) {
                                toastr.success("Script is created successfully.")
                            }

                            $scope.loading = false;
                            $scope.cancel();
                            $scope.changeOffset(1);
                        }).catch(function (reason) {
                            toastr.error($filter('translate')('Error'));
                            $scope.loading = false;
                        });
                }
            }

            $scope.delete = function (script) {
                script.deleting = true;

                if (!script.id) {
                    script.deleting = false;
                    return;
                }

                ScriptsService.delete(script.id)
                    .then(function (response) {
                        if (response.data) {
                            toastr.success("Script is deleted successfully.")
                        }

                        script.deleting = false;
                        $scope.changeOffset(1);
                    }).catch(function (reason) {
                        toastr.error($filter('translate')('Error'));
                        script.deleting = false;
                    });

            };

            $scope.showFormModal = function (script) {
                if (script) {
                    $scope.scriptModel = $filter('filter')($scope.scripts, { id: script.id }, true)[0];

                    if (!$scope.scriptModel.place_value)
                        $scope.scriptModel.place_value = $scope.scriptModel.place;

                    $scope.scriptModel.place = $scope.componentPlaceEnums[$scope.scriptModel.place_value];
                    $scope.id = script.id;
                }

                $scope.scriptFormModal = $scope.scriptFormModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/customcode/scripts/scriptFormModal.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });
                $scope.scriptFormModal.$promise.then(function () {
                    $scope.scriptFormModal.show();
                });
            };

            $scope.cancel = function () {
                $scope.scriptFormModal.hide();
                $scope.editing = false;
                $scope.id = null;
                $scope.scriptModel = {};
            };

        }
    ]);