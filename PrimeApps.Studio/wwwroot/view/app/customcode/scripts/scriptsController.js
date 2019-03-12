'use strict';

angular.module('primeapps')

    .controller('ScriptsController', ['$rootScope', '$scope', '$state', 'ScriptsService', '$modal', 'componentPlaces', 'componentPlaceEnums', '$filter', 'helper',
        function ($rootScope, $scope, $state, ScriptsService, $modal, componentPlaces, componentPlaceEnums, $filter, helper) {
            $scope.$parent.activeMenuItem = 'scripts';
            $rootScope.breadcrumblist[2].title = 'Scripts';
            $scope.scripts = [];
            $scope.scriptModel = {};
            $scope.loading = false;
            $scope.nameBlur = false;
            $scope.nameValid = null;
            $scope.componentPlaces = componentPlaces;
            $scope.componentPlaceEnums = componentPlaceEnums;
            $scope.modules = $rootScope.appModules;
            $scope.activePage = 1;

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

                if (page !== 1) {
                    var difference = Math.ceil($scope.pageTotal / $scope.requestModel.limit);

                    if (page > difference) {
                        if (Math.abs(page - difference) < 1)
                            --page;
                        else
                            page = page - Math.abs(page - Math.ceil($scope.pageTotal / $scope.requestModel.limit))
                    }
                }

                $scope.requestModel.offset = page;

                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;
                ScriptsService.find(requestModel)
                    .then(function (response) {
                        if (response.data) {
                            $scope.scripts = response.data;
                            setModule(response.data);
                        }
                        $scope.loading = false;
                    })
                    .catch(function (reason) {
                        $scope.loading = false;
                    });

            };

            $scope.changePage(1);

            $scope.changeOffset = function () {
                $scope.changePage($scope.activePage)
            };

            var setModule = function (data) {
                for (var i = 0; i < data.length; i++) {
                    var module = $filter('filter')($scope.modules, { id: data[i].module_id }, true);
                    if (module && module.length > 0)
                        data[i].module = angular.copy(module[0]);
                }
            };

            $scope.save = function (scriptForm) {
                $scope.saving = true;

                if (!scriptForm.$valid) {
                    $scope.saving = false;
                    return;
                }

                if ($scope.id) {
                    ScriptsService.update($scope.scriptModel)
                        .then(function (response) {
                            if (response.data) {
                                toastr.success("Script is updated successfully.");
                                $scope.cancel();
                                $scope.changeOffset(1);
                            }
                            $scope.saving = false;
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
                                toastr.success("Script is created successfully.");
                                $state.go('studio.app.scriptDetail', { name: $scope.scriptModel.name });
                                $scope.cancel();
                            }
                            $scope.saving = false;
                            //$scope.changeOffset(1);
                        }).catch(function (reason) {
                            toastr.error($filter('translate')('Error'));
                            $scope.saving = false;
                        });
                }
            }

            $scope.delete = function (script) {

                script.deleting = true;

                if (!script.id) {
                    script.deleting = false;
                    return;
                }

                swal({
                    title: "Are you sure?",
                    text: " ",
                    icon: "warning",
                    buttons: ['Cancel', 'Yes'],
                    dangerMode: true
                }).then(function (value) {
                    if (value) {
                        ScriptsService.delete(script.id)
                            .then(function (response) {
                                if (response.data) {
                                    toastr.success("Script is deleted successfully.");
                                }

                                script.deleting = false;
                                $scope.changeOffset(1);
                            }).catch(function (reason) {
                                toastr.error($filter('translate')('Error'));
                                script.deleting = false;
                            });
                    }
                    else
                        script.deleting = false;
                });
            };

            $scope.createIdentifier = function () {
                if (!$scope.scriptModel || !$scope.scriptModel.label) {
                    $scope.scriptModel.name = null;
                    return;
                }

                $scope.scriptModel.name = helper.getSlug($scope.scriptModel.label, '-');
                $scope.scriptNameBlur($scope.scriptModel);
            };


            $scope.scriptNameBlur = function (name) {
                //if ($scope.isScriptNameBlur && $scope.scriptNameValid)
                //    return;

                $scope.isScriptNameBlur = true;
                $scope.checkScriptName(name ? name : "");
            };

            $scope.checkScriptName = function (script) {
                if (!script || !script.name)
                    return;

                script.name = script.name.replace(/\s/g, '');
                script.name = script.name.replace(/[^a-zA-Z0-9\-]/g, '');

                if (!$scope.isScriptNameBlur)
                    return;

                $scope.scriptNameChecking = true;
                $scope.scriptNameValid = null;

                if (!script.name || script.name === '') {
                    $scope.scriptNameChecking = false;
                    $scope.scriptNameValid = false;
                    return;
                }

                ScriptsService.isUniqueName(script.name)
                    .then(function (response) {
                        $scope.scriptNameChecking = false;
                        if (response.data) {
                            $scope.scriptNameValid = true;
                        }
                        else {
                            $scope.scriptNameValid = false;
                        }
                    })
                    .catch(function () {
                        $scope.scriptNameValid = false;
                        $scope.scriptNameChecking = false;
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
                $scope.nameBlur = false;
                $scope.scriptNameValid = null;
                $scope.scriptModel = {};
            };

            $scope.runDeployment = function () {
                toastr.success("Deployment Started");
                FunctionsService.deploy($scope.script.name)
                    .then(function (response) {
                        //setAceOption($scope.record.runtime);
                        $scope.reload();
                    })
                    .catch(function (response) {
                    });
            };

        }
    ]);