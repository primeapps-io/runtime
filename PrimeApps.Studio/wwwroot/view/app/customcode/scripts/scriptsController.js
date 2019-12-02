'use strict';

angular.module('primeapps')

    .controller('ScriptsController', ['$rootScope', '$scope', '$state', '$timeout', 'ScriptsService', '$modal', 'componentPlaces', 'componentPlaceEnums', '$filter', 'helper', '$localStorage',
        function ($rootScope, $scope, $state, $timeout, ScriptsService, $modal, componentPlaces, componentPlaceEnums, $filter, helper, $localStorage) {
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
            $scope.environments = ScriptsService.getEnvironments();

            $scope.environmentChange = function (env, index, otherValue) {
                otherValue = otherValue || false;

                if (index === 2) {
                    $scope.environments[1].selected = true;
                    $scope.environments[1].disabled = !!env.selected;

                    if (otherValue) {
                        $scope.environments[2].selected = otherValue;
                    }
                }
            };
             
            $scope.save = function (scriptForm) {
                $scope.saving = true;

                if (!scriptForm.$valid || !$scope.scriptNameValid) {
                    if (scriptForm.custom_url.$invalid)
                        toastr.error("Please enter a valid url.");
                    else
                        toastr.error($filter('translate')('Setup.Modules.RequiredError'));

                    $scope.saving = false;
                    return;
                }

                $scope.scriptModel.environments = [];
                angular.forEach($scope.environments, function (env) {
                    if (env.selected)
                        $scope.scriptModel.environments.push(env.value);
                });

                delete $scope.scriptModel.environment;
                delete $scope.scriptModel.environment_list;

                if ($scope.id) {
                    ScriptsService.update($scope.scriptModel)
                        .then(function (response) {
                            if (response.data) {
                                toastr.success("Script is updated successfully.");
                                $scope.cancel();
                                $scope.grid.dataSource.read();
                            }
                            $scope.saving = false;
                        })
                        .catch(function (reason) {
                            toastr.error($filter('translate')('Common.Error'));
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
                                $scope.saving = false;
                            }
                            $scope.saving = false;
                            $scope.grid.dataSource.read();
                        }).catch(function (reason) {
                            toastr.error($filter('translate')('Common.Error'));
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
                                $scope.grid.dataSource.read();
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

                    if (script.environment && script.environment.indexOf(',') > -1)
                        $scope.scriptModel.environments = script.environment.split(',');
                    else
                        $scope.scriptModel.environments = script.environment;

                    angular.forEach($scope.scriptModel.environments, function (envValue) {
                        $scope.environmentChange($scope.environments[envValue - 1], envValue - 1, true);
                    });
                }
                else
                    $scope.environments[0].selected = true;

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
                $timeout(function () {
                    $scope.scriptModel = {};
                }, 1000);

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

            //For Kendo UI
            $scope.goUrl = function (script) {
                var selection = window.getSelection();
                if (selection.toString().length === 0) {
                    $state.go('studio.app.scriptDetail', { name: script.name }); //click event.
                }
            };

            var accessToken = $localStorage.read('access_token');

            $scope.mainGridOptions = {
                dataSource: {
                    type: "odata-v4",
                    page: 1,
                    pageSize: 10,
                    serverPaging: true,
                    serverFiltering: true,
                    serverSorting: true,
                    transport: {
                        read: {
                            url: "/api/script/find",
                            type: 'GET',
                            dataType: "json",
                            beforeSend: function (req) {
                                req.setRequestHeader('Authorization', 'Bearer ' + accessToken);
                                req.setRequestHeader('X-App-Id', $rootScope.currentAppId);
                                req.setRequestHeader('X-Organization-Id', $rootScope.currentOrgId);
                            }
                        }
                    },
                    schema: {
                        data: "items",
                        total: "count",
                        model: {
                            id: "id",
                        },
                        parse: function (data) {
                            for (var i = 0; i < data.items.length; i++) { 
                                var module = $filter('filter')($scope.modules, { id: data.items[i].module_id }, true);
                                if (module && module.length > 0)
                                    data.items[i].module = angular.copy(module[0]); 
                            }

                            return data;
                        }
                    }

                },
                scrollable: false,
                persistSelection: true,
                sortable: true,
                filterable: {
                    extra: false
                },
                rowTemplate: function (e) {
                    var trTemp = '<tr ng-click="goUrl(dataItem)">';
                    trTemp += '<td><span>' + e.label + '</span></td>';
                    trTemp += '<td> <span>' + e.name + '</span></td > ';
                    trTemp += '<td><span>' + e.module['label_' + $scope.language + '_plural'] + '</span></td>';
                    trTemp += '<td ng-click="$event.stopPropagation();"> <button ng-click="$event.stopPropagation(); delete(dataItem, $event);" type="button" class="action-button2-delete"><i class="fas fa-trash"></i></button></td></tr>';
                    return trTemp;
                },
                pageable: {
                    refresh: true,
                    pageSize: 10,
                    pageSizes: [10, 25, 50, 100],
                    buttonCount: 5,
                    info: true,
                },
                columns: [
                    {
                        field: 'Label',
                        title: 'Label',
                    },
                    {
                        field: 'Name',
                        title: 'Identifier',
                    },
                    {
                        field: 'Module.Label' + $scope.language + 'Plural',
                        title: $filter('translate')('Setup.Modules.Name')
                    },
                    {
                        field: '',
                        title: '',
                        width: "90px"
                    }]
            };
            //For Kendo UI

        }
    ]);